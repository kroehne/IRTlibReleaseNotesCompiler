using iText.IO.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using SharpSvn;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ReleaseNotesCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("IRTlib: ReleaseNotesCompiler ({0})\n", typeof(Program).Assembly.GetName().Version.ToString());
            Console.ResetColor();

            string targetDirectory = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string outputFile = System.IO.Path.Combine(targetDirectory, "ReleaseNotes.pdf");
            string yamlModulDesription = System.IO.Path.Combine(targetDirectory, "RepositoryDocumentation.yaml");

            if (args.Length > 0)
                targetDirectory = args[0];

            if (args.Length > 1)
                outputFile = args[1];

            if (args.Length > 2)
                yamlModulDesription = args[2];

            new Program().GenerateReleaseNotes(outputFile, targetDirectory, yamlModulDesription);

        }

        public void GenerateReleaseNotes(string outputFile, string targetDirectory, string yamlModulDesription)
        {
            try
            {
                var _svnWorkingCopy = new SvnWorkingCopyClient();
                _svnWorkingCopy.GetVersion(targetDirectory, out var ver);

                var _svnClient = new SvnClient();
                var sourceUrl = _svnClient.GetRepositoryRoot(targetDirectory);

                ModulRepository mr = new ModulRepository()
                {
                    Repository = sourceUrl.ToString(),
                    RevisionStart = ver.Start.ToString(),
                    RevisionEnd = ver.End.ToString(),
                    RevisionModified = ver.Modified.ToString(),
                    RevisionSwitched = ver.Switched.ToString()
                };

                Collection<SvnLogEventArgs> list;
                _svnClient.GetLog(targetDirectory, out list);

                mr.History = new List<RepositoryHistoryEntry>();
                foreach (SvnLogEventArgs a in list)
                {
                    string _message = "(no message)";
                    if (a.LogMessage != null)
                        _message = a.LogMessage.Replace("\n\n", "\n");

                    mr.History.Add(new RepositoryHistoryEntry() { Author = a.Author, Revision = a.Revision.ToString(), Message = _message });

                }

                // LOAD MODUL DOCUMENTATION 

                bool error = false;
                if (File.Exists(yamlModulDesription))
                {
                    try
                    {
                        var deserializer = new DeserializerBuilder()
                            .WithNamingConvention(UnderscoredNamingConvention.Instance)
                            .Build();

                        var yaml = File.ReadAllText(yamlModulDesription);
                        mr.GeneralConfiguration = deserializer.Deserialize<GeneralConfiguration>(yaml);

                    }
                    catch (Exception _ex)
                    {
                        Console.WriteLine(_ex.ToString());
                        error = true;
                    }
                }
                else
                {
                    Console.WriteLine("File not found: '" + yamlModulDesription + "'");
                    error = true;
                }

                if (error)
                {
                    // EXAMPLE 

                    mr.GeneralConfiguration = new GeneralConfiguration();
                    mr.GeneralConfiguration.Project = "NEPS/TBT";
                    mr.GeneralConfiguration.DefaultStartupParameters.Add(new ParameterDocumentation() { Name = "/AutoLoginUserName", Description = "Person identifier / ID used for a particular run of the module. A string value without white spaces or special characters is expected that can be used as file name. " });
                    mr.GeneralConfiguration.DefaultStartupParameters.Add(new ParameterDocumentation() { Name = "/RawDataFolder", Description = "Absolute or relative path to the raw data file that is created after finishing the assessment implemented in a particular module. " });
                    mr.GeneralConfiguration.DefaultStartupParameters.Add(new ParameterDocumentation() { Name = "/MonitoringFile", Description = "(Optional) Absolute or relative path and file name for the monitoring file. " });

                    mr.GeneralConfiguration.Modules = new List<ModulConfiguration>();

                    ModulConfiguration m1 = new ModulConfiguration();
                    m1.ModuleName = "DGCF_BZT";
                    m1.Study = "B155";
                    m1.HotKey = "Strg+Shift + X";
                    m1.InterviewerMenuDocumentation.Add(new InterviewerMenuDocumentation() { Password = "dipf", Description = "Default interviewer menu for operational use." });
                    m1.Domains.Add("Domain General Cognitive Functioning (BZT)");
                    m1.MonitoringVariableDocumentation.Add(new MonitoringVariableDocumentation() { Name = "h_abbruchDGCFBZT", Type = "integer", Description = "Variable indicates with value '1' if the module was terminated by the interviewer menu or with value '0' if the module terminated normally without an interviewer intervention." });
                    m1.MonitoringVariableDocumentation.Add(new MonitoringVariableDocumentation() { Name = "h_dauerDGCFBZT", Type = "integer", Description = "Time measure for the instrument 'Zeichenrätsel'." });
                    mr.GeneralConfiguration.Modules.Add(m1);

                    ModulConfiguration m2 = new ModulConfiguration();
                    m2.ModuleName = "DGCF_MAT";
                    m2.Study = "B155";
                    m2.HotKey = "Strg+Shift + X";
                    m2.InterviewerMenuDocumentation.Add(new InterviewerMenuDocumentation() { Password = "dipf", Description = "Default interviewer menu for operational use." });
                    m2.MonitoringVariableDocumentation.Add(new MonitoringVariableDocumentation() { Name = "h_abbruchDGCFMAT", Type = "integer", Description = "Variable indicates with value '1' if the module was terminated by the interviewer menu or with value '0' if the module terminated normally without an interviewer intervention." });
                    m2.MonitoringVariableDocumentation.Add(new MonitoringVariableDocumentation() { Name = "h_dauerDGCFMAT", Type = "integer", Description = "Time measure for the instrument 'Musterrätsel'." });
                    m2.Domains.Add("Domain General Cognitive Functioning (MAT)");
                    mr.GeneralConfiguration.Modules.Add(m2);

                    ModulConfiguration m3 = new ModulConfiguration();
                    m3.ModuleName = "ELFE_MP";
                    m3.Study = "B155";
                    m3.HotKey = "Strg+Shift + X";
                    m3.InterviewerMenuDocumentation.Add(new InterviewerMenuDocumentation() { Password = "dipf", Description = "Default interviewer menu for operational use." });
                    m3.MonitoringVariableDocumentation.Add(new MonitoringVariableDocumentation() { Name = "h_abbruchLE", Type = "integer", Description = "Variable indicates with value '1' if the module was terminated by the interviewer menu or with value '0' if the module terminated normally without an interviewer intervention." });
                    m3.MonitoringVariableDocumentation.Add(new MonitoringVariableDocumentation() { Name = "h_dauerLE", Type = "integer", Description = "Time measure for the instrument 'ELFE'." });
                    m3.MonitoringVariableDocumentation.Add(new MonitoringVariableDocumentation() { Name = "h_dauerLEMetaP", Type = "integer", Description = "Time measure for the instrument 'Meta-P (Elfe)'." });
                    m3.Domains.Add("Reading (ELFE)");
                    m3.Domains.Add("Meta-P (ELFE)");
                    mr.GeneralConfiguration.Modules.Add(m3);

                    ModulConfiguration m4 = new ModulConfiguration();
                    m4.ModuleName = "MATHE_MP";
                    m4.Study = "B155";
                    m4.HotKey = "Strg+Shift + X";
                    m4.InterviewerMenuDocumentation.Add(new InterviewerMenuDocumentation() { Password = "dipf", Description = "Default interviewer menu for operational use." });
                    m4.MonitoringVariableDocumentation.Add(new MonitoringVariableDocumentation() { Name = "h_abbruchZaR", Type = "integer", Description = "Variable indicates with value '1' if the module was terminated by the interviewer menu or with value '0' if the module terminated normally without an interviewer intervention." });
                    m4.MonitoringVariableDocumentation.Add(new MonitoringVariableDocumentation() { Name = "h_dauerZaR", Type = "integer", Description = "Time measure for the instrument 'Zahlenrätsel'." });
                    m4.MonitoringVariableDocumentation.Add(new MonitoringVariableDocumentation() { Name = "h_dauerZaRMetaP", Type = "integer", Description = "Time measure for the instrument 'Meta-P (Zahlenrätsel)'." });
                    m4.Domains.Add("Mathematics");
                    m4.Domains.Add("Meta-P (Mathematics)");
                    mr.GeneralConfiguration.Modules.Add(m4);

                    ModulConfiguration m5 = new ModulConfiguration();
                    m5.ModuleName = "PPVT_MP";
                    m5.Study = "B155";
                    m5.HotKey = "Strg+Shift + X";
                    m5.InterviewerMenuDocumentation.Add(new InterviewerMenuDocumentation() { Password = "dipf", Description = "Default interviewer menu for operational use." });
                    m5.MonitoringVariableDocumentation.Add(new MonitoringVariableDocumentation() { Name = "h_abbruchPPVT", Type = "integer", Description = "Variable indicates with value '1' if the module was terminated by the interviewer menu or with value '0' if the module terminated normally without an interviewer intervention." });
                    m4.MonitoringVariableDocumentation.Add(new MonitoringVariableDocumentation() { Name = "h_dauerPPVT", Type = "integer", Description = "Time measure for the instrument 'PPVT'." });
                    m4.MonitoringVariableDocumentation.Add(new MonitoringVariableDocumentation() { Name = "h_dauerPPVTMetaP", Type = "integer", Description = "Time measure for the instrument 'Meta-P (PPVT)'." });
                    m5.Domains.Add("PPVT");
                    m5.Domains.Add("Meta-P / PPVT");
                    m5.StartupParameters.Add(new ParameterDocumentation() { Name = "/Alter", Description = "Age of the student in month, used to select the appropriate start set. An integer number is expected and 120 is the cutoff value." });
                    mr.GeneralConfiguration.Modules.Add(m5);

                    var serializer = new SerializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
                    var yaml = serializer.Serialize(mr.GeneralConfiguration);
                    File.WriteAllText(yamlModulDesription, yaml);
                }

                PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                PdfWriter writer = new PdfWriter(outputFile);


                // INITIALIZE PDF AND DOCUMENT 

                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf, PageSize.A4, false);
                document.SetMargins(50, 50, 50, 75);

                // HEADER 

                document.Add(new Paragraph().SetFontSize(18).SetBold().Add("Release Notes"));
                document.Add(new Paragraph(String.Format("Repository: {0}", mr.Repository)));
                document.Add(new Paragraph(String.Format("Revision: Start = {0} / End = {1} (Modified: {2}, Switched: {3})", mr.RevisionStart, mr.RevisionEnd, mr.RevisionModified, mr.RevisionSwitched)));
                document.Add(new Paragraph(String.Format("Creation Date / Time: {0}", DateTime.Now.ToString())));

                Style notestyle_bold = new Style();
                PdfFont boldfont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                notestyle_bold.SetFont(boldfont);
                Style notestyle_courier = new Style();
                PdfFont courierfont = PdfFontFactory.CreateFont(StandardFonts.COURIER);
                notestyle_courier.SetFont(courierfont);


                Paragraph _summary0 = new Paragraph();
                _summary0.Add(new Text("Modules in this Release: ").AddStyle(notestyle_bold));
                document.Add(_summary0);

                List domainSummary = new List();
                domainSummary.SetSymbolIndent(15);
                domainSummary.SetListSymbol("\u2022");
                domainSummary.SetFont(font);
                foreach (var mc in mr.GeneralConfiguration.Modules)
                {
                    ListItem li = new ListItem();
                    Paragraph p = new Paragraph();
                    p.SetFixedLeading(15);
                    p.SetMargin(0);
                    p.Add(new Text(mc.ModuleName));
                    li.Add(p);
                    domainSummary.Add(li);
                }
                document.Add(domainSummary);


                Paragraph _note0 = new Paragraph();
                _note0.Add(new Text("General Notes: ").AddStyle(notestyle_bold));
                document.Add(_note0);

                Paragraph _note1a = new Paragraph();
                _note1a.Add(new Text("1. The module can be started by calling "));
                _note1a.Add(new Text("TBT\\TestApp.Player.Chromely.exe ").AddStyle(notestyle_courier));
                _note1a.Add(new Text("with named command line parameters. Within values of command line parameters \\ must be replaced with \\\\. This is especially true for file names containing paths and directories. Relative path specification is possible. Path parameter must not end with \\ or \\\\. "));
                _note1a.Add(new Text("\nExample: "));
                Paragraph _note1b = new Paragraph();
                _note1b.Add(new Text("TBT\\TestApp.Player.Chromely.exe /AutoLoginUserName=\"personidentifier\" /RawDataFolder=\"..\\\\resultfolder\" /MonitoringFile=\"..\\\\monitoring-for-personidentifier.json\" /AutoLoginCreateWithTest=\"PPVT_MP\" /Alter=\"121\"").AddStyle(notestyle_courier));
                document.Add(_note1a);
                document.Add(_note1b);

                Paragraph _note2 = new Paragraph();
                _note2.Add(new Text("2. The modules has runtime requirements. Run the 'Readiness-Tool' to check prerequisites for using the modules by calling "));
                _note2.Add(new Text("TBT\\ReadinessTool.exe").AddStyle(notestyle_courier));
                _note2.Add(new Text(". The 'Readiness-Tool' can also be started using the named command line arguments of the module."));
                document.Add(_note2);

                Paragraph _note3a = new Paragraph();
                _note3a.Add(new Text("3. Monitoring data are stored, if requested as monitoring file, in "));
                _note3a.Add(new Text("JSON").AddStyle(notestyle_courier));
                _note3a.Add(new Text(" format as key-value list. Date types 'DateTime', 'Integer', 'String' and 'Decimal' are differentiated. Example: "));
                Paragraph _note3b = new Paragraph();
                _note3b.Add(new Text("{\n\t\"ExampleDateTime\": \"2021-08-02T10:25:58.6209884+02:00\",\n").AddStyle(notestyle_courier));
                _note3b.Add(new Text("\t\"ExampleInteger\": 42,\n").AddStyle(notestyle_courier));
                _note3b.Add(new Text("\t\"ExampleString\": \"Zeichenkette\",\n").AddStyle(notestyle_courier));
                _note3b.Add(new Text("\t\"ExampleDecimal\": 3.141592653589793\n}").AddStyle(notestyle_courier));
                document.Add(_note3a);
                document.Add(_note3b);

                Paragraph _note4 = new Paragraph();
                _note4.Add(new Text("4. The modules keep track of used person identifiers and each person identifier can only be used once."));
                document.Add(_note4);


                // DETAIL MODULE CONFIGURATION DETAILS
                document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

                foreach (var mc in mr.GeneralConfiguration.Modules)
                {
                    document.Add(new Paragraph().SetFontSize(16).Add(String.Format("Module \"{0}\"", mc.ModuleName)));
                    document.Add(new Paragraph(String.Format("Study: {0}", mc.Study)));

                    // DOMAINS

                    document.Add(new Paragraph(String.Format("Module Content (Domains)")));
                    List domainContent = new List();
                    domainContent.SetSymbolIndent(15);
                    domainContent.SetListSymbol("\u2022");
                    domainContent.SetFont(font);
                    foreach (var par in mc.Domains)
                    {
                        ListItem li = new ListItem();
                        Paragraph p = new Paragraph();
                        p.SetFixedLeading(15);
                        p.SetMargin(0);
                        p.Add(new Text(par));
                        li.Add(p);
                        domainContent.Add(li);
                    }
                    document.Add(domainContent);

                    // INTERVIER-MENU

                    document.Add(new Paragraph(String.Format("Interviewer-Menu - Hotkey: {0}", mc.HotKey)));
                    List interviewerMenus = new List();
                    interviewerMenus.SetSymbolIndent(15);
                    interviewerMenus.SetListSymbol("\u2022");
                    interviewerMenus.SetFont(font);
                    Style password = new Style();
                    PdfFont monospace = PdfFontFactory.CreateFont(StandardFonts.COURIER);
                    password.SetFont(monospace).SetFontColor(ColorConstants.BLUE).SetBackgroundColor(ColorConstants.LIGHT_GRAY);
                    foreach (var par in mc.InterviewerMenuDocumentation)
                    {
                        ListItem li = new ListItem();
                        Paragraph p = new Paragraph();
                        p.SetFixedLeading(15);
                        p.SetMargin(0);
                        p.Add(new Text("Password "));
                        p.Add(new Text(par.Password).AddStyle(password));
                        p.Add(new Text(": " + par.Description));
                        li.Add(p);
                        interviewerMenus.Add(li);
                    }
                    document.Add(interviewerMenus);

                    // MONITORING VARIABLES

                    document.Add(new Paragraph(String.Format("Monitoring Variables")));
                    List monitoringVariables = new List();
                    monitoringVariables.SetSymbolIndent(15);
                    monitoringVariables.SetListSymbol("\u2022");
                    monitoringVariables.SetFont(font);
                    Style parametertype = new Style();
                    parametertype.SetFont(PdfFontFactory.CreateFont(StandardFonts.COURIER)).SetFontColor(ColorConstants.BLACK).SetBackgroundColor(ColorConstants.LIGHT_GRAY);

                    Style parmetername = new Style();
                    parmetername.SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE)).SetFontColor(ColorConstants.BLACK);

                    foreach (var par in mc.MonitoringVariableDocumentation)
                    {
                        ListItem li = new ListItem();
                        Paragraph p = new Paragraph();
                        p.SetFixedLeading(15);
                        p.SetMargin(0);
                        p.Add(new Text(par.Name).AddStyle(parmetername));
                        p.Add(new Text(" ["));
                        p.Add(new Text(par.Type).AddStyle(parametertype));
                        p.Add(new Text("]: " + par.Description));
                        li.Add(p);
                        monitoringVariables.Add(li);

                    }
                    document.Add(monitoringVariables);

                    // DEFAULT STARTUP PARAMETER

                    document.Add(new Paragraph(String.Format("Startup Parameter (Default)")));
                    List defaultParameter = new List();
                    defaultParameter.SetSymbolIndent(15);
                    defaultParameter.SetListSymbol("\u2022");
                    defaultParameter.SetFont(font);
                    Style code = new Style();
                    code.SetFont(monospace).SetFontColor(ColorConstants.RED).SetBackgroundColor(ColorConstants.LIGHT_GRAY);
                    foreach (var par in mr.GeneralConfiguration.DefaultStartupParameters)
                    {

                        ListItem li = new ListItem();
                        Paragraph p = new Paragraph();
                        p.SetFixedLeading(15);
                        p.SetMargin(0);
                        p.Add(new Text(par.Name).AddStyle(code));
                        p.Add(new Text(": " + par.Description));
                        li.Add(p);
                        defaultParameter.Add(li);

                    }
                    document.Add(defaultParameter);

                    // SPECIFIC STARTUP PARAMETER 

                    document.Add(new Paragraph(String.Format("Module-Specific Startup Parameter")));
                    List specificParameter = new List();
                    specificParameter.SetSymbolIndent(15);
                    specificParameter.SetListSymbol("\u2022");
                    specificParameter.SetFont(font);
                    if (mc.StartupParameters.Count > 0)
                    {
                        foreach (var par in mc.StartupParameters)
                        {
                            ListItem li = new ListItem();
                            Paragraph p = new Paragraph();
                            p.SetFixedLeading(15);
                            p.SetMargin(0);
                            p.Add(new Text(par.Name).AddStyle(code));
                            p.Add(new Text(": " + par.Description));
                            li.Add(p);
                            specificParameter.Add(li);
                        }
                    }
                    else
                    {
                        ListItem li = new ListItem();
                        Paragraph p = new Paragraph();
                        p.SetFixedLeading(15);
                        p.SetMargin(0);
                        p.Add(new Text("(none)"));
                        li.Add(p);
                        specificParameter.Add(li);
                    }
                    document.Add(specificParameter);

                }


                // ADD REVISION HISTORY

                document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                document.Add(new Paragraph().SetFontSize(18).Add("Revision History"));

                List revisionHistoryList = new List();
                revisionHistoryList.SetSymbolIndent(15);
                revisionHistoryList.SetListSymbol("\u2022");
                revisionHistoryList.SetFont(font);

                foreach (var h in mr.History)
                {
                    Paragraph p = new Paragraph(String.Format("{0} / {1}: {2}", h.Revision, h.Author, h.Message));
                    p.SetFixedLeading(15);
                    p.SetMargin(0);

                    ListItem li = new ListItem();
                    li.Add(p);
                    revisionHistoryList.Add(li);
                }
                document.Add(revisionHistoryList);

                // ADD HEADLINE AND FOOTER

                Rectangle pageSize = pdf.GetPage(1).GetPageSize();

                for (int i = 1; i <= pdf.GetNumberOfPages(); i++)
                {

                    Paragraph header = new Paragraph(String.Format("Release Notes -- {0} -- Revision {1}/{2} -- Page {3} / {4}", mr.Repository, mr.RevisionStart, mr.RevisionEnd, i, pdf.GetNumberOfPages())).SetFont(font).SetFontSize(10);
                    Paragraph footer = new Paragraph(String.Format("© 2009-{0} by DIPF | Leibniz Institute for Research and Information in Education \nTBA (Center for Technology-Based Assessment) -- {1}", DateTime.Today.Year, mr.GeneralConfiguration.Project)).SetFont(font).SetFontSize(10);

                    document.ShowTextAligned(header, 25, pageSize.GetTop() - 25, i, TextAlignment.LEFT, VerticalAlignment.BOTTOM, 0);
                    document.ShowTextAligned(footer, 25, pageSize.GetBottom() + 20, i, TextAlignment.LEFT, VerticalAlignment.BOTTOM, 0);
                }
                 
                // CLOSE DOCUMENT

                document.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Run this inside of an SVN repository. Svn.exe must be availabe in the current PATH. ");
                return;

            }
        }

    }

    public class ModulRepository
    {
    
        public string Repository { get; set; }
        public string RevisionStart { get; set; }
        public string RevisionEnd { get; set; }
        public string RevisionModified { get; set; }
        public string RevisionSwitched { get; set; }
             
        public List<RepositoryHistoryEntry> History { get; set; }
         
        public GeneralConfiguration GeneralConfiguration { get; set; }

        public ModulRepository()
        {
         
            History = new List<RepositoryHistoryEntry>();

            GeneralConfiguration = new GeneralConfiguration();
        }
    }

    public class GeneralConfiguration
    {
        public string Project { get; set; }

        public List<ParameterDocumentation> DefaultStartupParameters { get; set; }

        public List<ModulConfiguration> Modules { get; set; }
      
        public GeneralConfiguration()
        {
            Project = "TBA";
            DefaultStartupParameters = new List<ParameterDocumentation>();
            Modules = new List<ModulConfiguration>();
        }
    }


    public class ModulConfiguration
    {
        public string ModuleName { get; set; }
        public string Study { get; set; }
        public string HotKey { get; set; }

        public List<string> Domains { get; set; }
        public List<ParameterDocumentation> StartupParameters { get; set; }
        public List<InterviewerMenuDocumentation> InterviewerMenuDocumentation { get; set; }
        public List<MonitoringVariableDocumentation> MonitoringVariableDocumentation { get; set; }
        public ModulConfiguration()
        {
            Domains = new List<string>();
            StartupParameters = new List<ParameterDocumentation>();
            InterviewerMenuDocumentation = new List<InterviewerMenuDocumentation>();
            MonitoringVariableDocumentation = new List<MonitoringVariableDocumentation>();
        }
    }

    public class RepositoryHistoryEntry
    {
        public string Revision { get; set; }
        public string Author { get; set; }
        public string Message { get; set; }
    }

    public class ParameterDocumentation
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class InterviewerMenuDocumentation
    {
        public string Password { get; set; }
        public string Description { get; set; }
    }

    public class MonitoringVariableDocumentation
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
    }
}
