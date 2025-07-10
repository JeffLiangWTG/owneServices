using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Client.EDI.IssueManager.Business.Test;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.ServiceTask.Testing
{
	[TestedType(typeof(HelpErrorStackLineCountImportServiceTask))]
	class HelpErrorStackLineCountImportServiceTaskTest : ServiceTaskTestCase<HelpErrorStackLineCountImportServiceTask>
	{
		public void TestServiceTaskCanRunInAnyBranch()
		{
			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));
		}

		[TestDate(2015, 8, 1)]
		public void TestImportStackLine_WithSpaceBetweenMethodAndParameterList_AndWithoutAssemblyAttribute()
		{
			EDIDataRegistry.Instance.StackLineCountLastImport = ZDateTime.UtcToday.AddMonths(-1).ToDateTime();

			var assemblyPK = Guid.NewGuid();
			var classPK = Guid.NewGuid();
			InsertAssembly(assemblyPK, "TheBestAssembly.dll", "C:\\nowhere");
			InsertClass(classPK, assemblyPK, "Warehouse.RF.Core.Business.Processors.UnloadProcessor");
			InsertMethod(Guid.NewGuid(), classPK, "DisplayLineProductData");

			var log = CreateLog(firstProcessed: new ZDateTime(2015, 7, 14));
			var calls = GetFileContent("SampleCallsWithTrailingSpaces.xml");
			const string stackLine = "Warehouse.RF.Core.Business.Processors.UnloadProcessor.DisplayLineProductData (System.Boolean convertPacksToQty)";
			var extendedStackLine = $"<Call>  at {stackLine}";

			AssertContains(extendedStackLine, calls);
			CreateLogOccurrence(log, calls);
			Factory.Save();

			var task = new HelpErrorStackLineCountImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			var count = Factory.LoadTop1<HelpErrorStackLineCount>(new ZQuery(HelpErrorStackLineCountSchema.HSL_StackLine, stackLine));

			AssertNotNull(count);
			CombineAssertions(() =>
			{
				AssertEquals(nameof(HelpErrorStackLineCount.HSL_Method), "DisplayLineProductData", count.HSL_Method);
				AssertEquals(nameof(HelpErrorStackLineCount.HSL_Type), "Warehouse.RF.Core.Business.Processors.UnloadProcessor", count.HSL_Type);
				AssertEquals(nameof(HelpErrorStackLineCount.HSL_Assembly), "TheBestAssembly.dll", count.HSL_Assembly);
			});
		}

		public void TestImportStackLineWithAssemblyInfoSucceeds()
		{
			var log01 = CreateLog(firstProcessed: new ZDateTime(2015, 2, 12));
			var log02 = CreateLog(firstProcessed: new ZDateTime(2015, 7, 28));
			var log03 = CreateLog(firstProcessed: new ZDateTime(2017, 2, 25));
			var sampleCallsWithAssembly01 = GetFileContent("SampleCallsWithAssembly01.xml");
			var sampleCallsWithAssemblyStackLines01 = GetFileContent("SampleCallsWithAssembly01_StackLines.xml");
			var sampleCallsWithAssembly02 = GetFileContent("SampleCallsWithAssembly02.xml");
			var sampleCallsWithAssemblyStackLines02 = GetFileContent("SampleCallsWithAssembly02_StackLines.xml");
			CreateLogOccurrence(log01, sampleCallsWithAssembly01);
			CreateLogOccurrence(log02, sampleCallsWithAssembly02);
			CreateLogOccurrence(log01, sampleCallsWithAssembly01); // already exists. Should not be counted
			CreateLogOccurrence(log03, sampleCallsWithAssembly01);
			Factory.Save();
			var task = new HelpErrorStackLineCountImportServiceTask()
			{ ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			var actualCounts = Factory.Load<HelpErrorStackLineCount>(new ZQuery());
			var expectedCalls = GetStackCalls(sampleCallsWithAssemblyStackLines01).Concat(GetStackCalls(sampleCallsWithAssemblyStackLines02)).Concat(GetStackCalls(sampleCallsWithAssemblyStackLines01)).GetStackLineCount();
			AssertCallCounts(actualCounts, expectedCalls);
			AssertSavedNumberOfImportedLogs(3);
		}

		public void TestImportStackLineInnerExceptions()
		{
			var testFiles = new[] { "SampleCallsWithInnerExceptions01.xml" };
			var validExceptionFiles = new string[] { "SampleCallsWithInnerExceptions01_StackLines.xml" };
			TestFilesCore(testFiles, validExceptionFiles);
		}

		public void TestImportStackLineWithoutAssemblyInfoSucceeds()
		{
			var log01 = CreateLog(firstProcessed: new ZDateTime(2015, 2, 12));
			var log02 = CreateLog(firstProcessed: new ZDateTime(2015, 7, 28));
			var log03 = CreateLog(firstProcessed: new ZDateTime(2017, 2, 25));
			SetUpAssemblies();
			var sampleCallsWithoutAssembly01 = GetFileContent("StackLineWithoutAssemblyInfo01.xml");
			var sampleCallsWithoutAssemblyStackLines01 = GetFileContent("StackLineWithoutAssemblyInfo01_StackLines.xml");
			var sampleCallsWithoutAssembly02 = GetFileContent("StackLineWithoutAssemblyInfo02.xml");
			var sampleCallsWithoutAssemblyStackLines02 = GetFileContent("StackLineWithoutAssemblyInfo02_StackLines.xml");
			CreateLogOccurrence(log01, sampleCallsWithoutAssembly01);
			CreateLogOccurrence(log02, sampleCallsWithoutAssembly02);
			CreateLogOccurrence(log01, sampleCallsWithoutAssembly01); // already exists. Should not be counted
			CreateLogOccurrence(log03, sampleCallsWithoutAssembly01);
			Factory.Save();
			var task = new HelpErrorStackLineCountImportServiceTask()
			{ ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			var actualCounts = Factory.Load<HelpErrorStackLineCount>(new ZQuery());
			var expectedCalls = GetStackCalls(sampleCallsWithoutAssemblyStackLines01).Concat(GetStackCalls(sampleCallsWithoutAssemblyStackLines02)).Concat(GetStackCalls(sampleCallsWithoutAssemblyStackLines01)).GetStackLineCount();
			AssertCallCounts(actualCounts, expectedCalls);
			AssertSavedNumberOfImportedLogs(3);
		}

		public void TestImportStackLineWithoutParametersSucceeds()
		{
			var testFiles = new[] { "SampleCallsWithoutParameters.xml" };
			var validExceptionFiles = new string[] { "SampleCallsWithoutParameters_StackLines.xml" };
			TestFilesCore(testFiles, validExceptionFiles);
		}

		public void TestLogsWithInvalidExceptionsAreIgnored()
		{
			var testFiles = new[] { "SampleFoxProLog.xml", "SampleBrokenXML.xml", "SampleAppleCrashXML.xml", "SampleAppleCrashHTML.htm" };
			var validExceptionFiles = Array.Empty<string>();
			TestFilesCore(testFiles, validExceptionFiles);
		}

		[TestDate(2015, 1, 1)]
		public void TestOnlyNewLogsAreImported()
		{
			var sampleCallsWithAssembly01 = GetFileContent("SampleCallsWithAssembly01.xml");
			var sampleCallsWithAssemblyStackLines01 = GetFileContent("SampleCallsWithAssembly01_StackLines.xml");
			CreateLogOccurrence(CreateLog(), sampleCallsWithAssembly01);
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			var task = new HelpErrorStackLineCountImportServiceTask()
			{ ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertSavedNumberOfImportedLogs(1);
			var sampleCallsWithAssembly02 = GetFileContent("SampleCallsWithAssembly02.xml");
			var sampleCallsWithAssemblyStackLines02 = GetFileContent("SampleCallsWithAssembly02_StackLines.xml");
			CreateLogOccurrence(CreateLog(), sampleCallsWithAssembly02);
			Factory.Save();
			TestDateAttribute.Date = new DateTime(2015, 1, 2).AddSeconds(1);
			task = new HelpErrorStackLineCountImportServiceTask()
			{ ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			var expectedCalls = GetStackCalls(sampleCallsWithAssemblyStackLines01).Concat(GetStackCalls(sampleCallsWithAssemblyStackLines02)).GetStackLineCount();
			var actualCounts = Factory.Load<HelpErrorStackLineCount>(new ZQuery());
			AssertCallCounts(actualCounts, expectedCalls);
			AssertSavedNumberOfImportedLogs(2);
		}

		[TestDate(2015, 1, 1)]
		public void TestOnlyLogsUptoStartTimeAreImported()
		{
			var log = CreateLog();
			var sampleCallsWithAssembly01 = GetFileContent("SampleCallsWithAssembly01.xml");
			var sampleCallsWithAssemblyStackLines01 = GetFileContent("SampleCallsWithAssembly01_StackLines.xml");
			CreateLogOccurrence(log, sampleCallsWithAssembly01);
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			var task = new HelpErrorStackLineCountImportServiceTask()
			{ ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertSavedNumberOfImportedLogs(1);
			var sampleCallsWithAssembly02 = GetFileContent("SampleCallsWithAssembly02.xml");
			CreateLogOccurrence(log, sampleCallsWithAssembly02);
			Factory.Save();
			TestDateAttribute.Date = new DateTime(2015, 1, 2);
			task = new HelpErrorStackLineCountImportServiceTask()
			{ ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			var expectedCalls = GetStackCalls(sampleCallsWithAssemblyStackLines01).GetStackLineCount();
			var actualCounts = Factory.Load<HelpErrorStackLineCount>(new ZQuery());
			AssertCallCounts(actualCounts, expectedCalls);
			AssertSavedNumberOfImportedLogs(1);
		}

		[TestDate(2015, 1, 1)]
		public void TestStackLineCountsAccumulateAccrossTaskRuns()
		{
			var sampleCallsWithAssembly01 = GetFileContent("SampleCallsWithAssembly01.xml");
			var sampleCallsWithAssemblyStackLines01 = GetFileContent("SampleCallsWithAssembly01_StackLines.xml");
			var sampleCallsWithAssembly02 = GetFileContent("SampleCallsWithAssembly02.xml");
			var sampleCallsWithAssemblyStackLines02 = GetFileContent("SampleCallsWithAssembly02_StackLines.xml");
			CreateLogOccurrence(CreateLog(new ZDateTime(2014, 1, 1)), sampleCallsWithAssembly01);
			CreateLogOccurrence(CreateLog(new ZDateTime(2014, 1, 1).AddDays(5)), sampleCallsWithAssembly02);
			Factory.Save();
			var task = new HelpErrorStackLineCountImportServiceTask()
			{ ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertSavedNumberOfImportedLogs(2);
			TestDateAttribute.Date = TestDateAttribute.Date.AddMonths(1);
			CreateLogOccurrence(CreateLog(), sampleCallsWithAssembly01);
			CreateLogOccurrence(CreateLog(ZDateTime.Now.AddDays(2)), sampleCallsWithAssembly02);
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(10);
			task = new HelpErrorStackLineCountImportServiceTask()
			{ ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			var expectedCalls = GetStackCalls(sampleCallsWithAssemblyStackLines01).Concat(GetStackCalls(sampleCallsWithAssemblyStackLines02)).Concat(GetStackCalls(sampleCallsWithAssemblyStackLines01)).Concat(GetStackCalls(sampleCallsWithAssemblyStackLines02)).GetStackLineCount();
			var actualCounts = Factory.Load<HelpErrorStackLineCount>(new ZQuery());
			AssertCallCounts(actualCounts, expectedCalls);
			AssertSavedNumberOfImportedLogs(4);
		}

		[TestDate(2015, 1, 1)]
		public void TestExistingStackLineDetailsAreUpdatedIfFound()
		{
			var sampleCallsWithoutAssembly = GetFileContent("StackLineWithoutAssemblyInfo02.xml");
			CreateLogOccurrence(CreateLog(), sampleCallsWithoutAssembly);
			Factory.Save();
			var task = new HelpErrorStackLineCountImportServiceTask()
			{ ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMonths(2);
			var sampleCallsWithAssembly = GetFileContent("SampleCallsWithAssembly02.xml");
			CreateLogOccurrence(CreateLog(), sampleCallsWithAssembly);
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);
			task = new HelpErrorStackLineCountImportServiceTask()
			{ ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			var stackLine = Factory.Load<HelpErrorStackLineCount>(new ZQuery(HelpErrorStackLineCountSchema.HSL_StackLine, "Enterprise.ZArchitecture.Core.ExceptionReportBuilder.GenerateReport()")).First();
			AssertEquals("StackLine Assembly must be updated", "Enterprise.ZArchitecture.Core.dll", stackLine.HSL_Assembly);
			AssertEquals("StackLine Type must be updated", "Enterprise.ZArchitecture.Core.ExceptionReportBuilder", stackLine.HSL_Type);
			AssertEquals("StackLine Method must be updated", "GenerateReport", stackLine.HSL_Method);
			AssertEquals("StackLine Parameters must be updated", string.Empty, stackLine.HSL_Parameters);
			AssertEquals(2, stackLine.HSL_Count);
			stackLine = Factory.Load<HelpErrorStackLineCount>(new ZQuery(HelpErrorStackLineCountSchema.HSL_StackLine, "Enterprise.ZArchitecture.Core.ExceptionDetails.WriteFullReport(XmlTextWriter xtw)")).First();
			AssertEquals("StackLine Assembly must be updated", "Enterprise.ZArchitecture.Core.dll", stackLine.HSL_Assembly);
			AssertEquals("StackLine Type must be updated", "Enterprise.ZArchitecture.Core.ExceptionDetails", stackLine.HSL_Type);
			AssertEquals("StackLine Method must be updated", "WriteFullReport", stackLine.HSL_Method);
			AssertEquals("StackLine Parameters must be updated", "System.Xml.XmlTextWriter", stackLine.HSL_Parameters);
			AssertEquals(2, stackLine.HSL_Count);
		}

		[TestDate(2015, 1, 1)]
		public void TestImportingStackLineWithoutDetailsDoesNotOverrideExisting()
		{
			var sampleCallsWithAssembly = GetFileContent("SampleCallsWithAssembly02.xml");
			CreateLogOccurrence(CreateLog(), sampleCallsWithAssembly);
			Factory.Save();
			var task = new HelpErrorStackLineCountImportServiceTask()
			{ ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMonths(2);
			var sampleCallsWithoutAssembly = GetFileContent("StackLineWithoutAssemblyInfo02.xml");
			CreateLogOccurrence(CreateLog(), sampleCallsWithoutAssembly);
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);
			task = new HelpErrorStackLineCountImportServiceTask()
			{ ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			var stackLine = Factory.Load<HelpErrorStackLineCount>(new ZQuery(HelpErrorStackLineCountSchema.HSL_StackLine, "Enterprise.ZArchitecture.Core.ExceptionReportBuilder.GenerateReport()")).First();
			AssertEquals("StackLine Assembly must not be updated", "Enterprise.ZArchitecture.Core.dll", stackLine.HSL_Assembly);
			AssertEquals("StackLine Type must be updated", "Enterprise.ZArchitecture.Core.ExceptionReportBuilder", stackLine.HSL_Type);
			AssertEquals("StackLine Method must be updated", "GenerateReport", stackLine.HSL_Method);
			AssertEquals("StackLine Parameters must be updated", string.Empty, stackLine.HSL_Parameters);
			AssertEquals(2, stackLine.HSL_Count);
			stackLine = Factory.Load<HelpErrorStackLineCount>(new ZQuery(HelpErrorStackLineCountSchema.HSL_StackLine, "Enterprise.ZArchitecture.Core.ExceptionDetails.WriteFullReport(XmlTextWriter xtw)")).First();
			AssertEquals("StackLine Assembly must not be updated", "Enterprise.ZArchitecture.Core.dll", stackLine.HSL_Assembly);
			AssertEquals("StackLine Type must not be updated", "Enterprise.ZArchitecture.Core.ExceptionDetails", stackLine.HSL_Type);
			AssertEquals("StackLine Method must not be updated", "WriteFullReport", stackLine.HSL_Method);
			AssertEquals("StackLine Parameters must not be updated", "System.Xml.XmlTextWriter", stackLine.HSL_Parameters);
			AssertEquals(2, stackLine.HSL_Count);
		}

		public void TestTaskRunsHourly()
		{
			AssertEquals("Service task should run every 1 hour", "1hour", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		public void TestStackFromMostRecentExeVersionImported()
		{
			var log1 = CreateLog(firstProcessed: new ZDateTime(2017, 11, 15));
			CreateLogOccurrence(log1, GetExceptionXml(@"<Call Assembly=""MyAssembly.dll"">   at MyCode.MyClass.One()</Call>"), exeDate: new ZDateTime(2017, 10, 01));
			CreateLogOccurrence(log1, GetExceptionXml(@"<Call Assembly=""MyAssembly.dll"">   at MyCode.MyClass.Two()</Call>"), exeDate: new ZDateTime(2017, 11, 01));
			Factory.Save();
			var task = new HelpErrorStackLineCountImportServiceTask()
			{ ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			var actualCounts = Factory.Load<HelpErrorStackLineCount>(new ZQuery());
			AssertEquals("MyCode.MyClass.Two()", actualCounts.Single().HSL_StackLine);
		}

		public void TestStackLinesCaseInsensitive()
		{
			var log1 = CreateLog(firstProcessed: new ZDateTime(2017, 11, 15));
			CreateLogOccurrence(log1, GetExceptionXml(@"<Call Assembly=""MyAssembly.dll"">   at MyCode.MyClass.MyFunction()</Call>"));
			var log2 = CreateLog(firstProcessed: new ZDateTime(2017, 11, 15));
			CreateLogOccurrence(log2, GetExceptionXml(@"<Call Assembly=""MyAssembly.dll"">   at MyCode.MyClass.Myfunction()</Call>"));
			Factory.Save();
			var task = new HelpErrorStackLineCountImportServiceTask()
			{ ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			var actualCounts = Factory.Load<HelpErrorStackLineCount>(new ZQuery());
			AssertEquals(2, actualCounts.Single().HSL_Count);
		}

		[TestDate(2017, 11, 15)]
		public void TestStackLinesCaseInsensitiveAccrossMultipleRuns()
		{
			var log1 = CreateLog(firstProcessed: ZDateTime.Now.AddMinutes(-1));
			CreateLogOccurrence(log1, GetExceptionXml(@"<Call Assembly=""MyAssembly.dll"">   at MyCode.MyClass.MyFunction()</Call>"));
			Factory.Save();
			var task = new HelpErrorStackLineCountImportServiceTask()
			{ ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			var log2 = CreateLog(firstProcessed: ZDateTime.Now.AddMinutes(-1));
			CreateLogOccurrence(log2, GetExceptionXml(@"<Call Assembly=""MyAssembly.dll"">   at MyCode.MyClass.Myfunction()</Call>"));
			Factory.Save();
			task = new HelpErrorStackLineCountImportServiceTask()
			{ ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			var actualCounts = Factory.Load<HelpErrorStackLineCount>(new ZQuery());
			AssertEquals(2, actualCounts.Single().HSL_Count);
		}

		[TestDate(2017, 11, 15)]
		public void TestAssemblyUpdated()
		{
			var log1 = CreateLog(firstProcessed: ZDateTime.Now.AddMinutes(-1));
			CreateLogOccurrence(log1, GetExceptionXml(@"<Call>   at MyCode.MyClass.MyFunction()</Call>"));
			Factory.Save();
			var task = new HelpErrorStackLineCountImportServiceTask()
			{ ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			var log2 = CreateLog(firstProcessed: ZDateTime.Now.AddMinutes(-1));
			CreateLogOccurrence(log2, GetExceptionXml(@"<Call Assembly=""MyAssembly.dll"">   at MyCode.MyClass.MyFunction()</Call>"));
			Factory.Save();
			task = new HelpErrorStackLineCountImportServiceTask()
			{ ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			var actualCounts = Factory.Load<HelpErrorStackLineCount>(new ZQuery());
			AssertEquals("MyAssembly.dll", actualCounts.Single().HSL_Assembly);
		}

		public void TestLongStackLineTruncated()
		{
			var log = CreateLog();
			CreateLogOccurrence(log, GetExceptionXml($@"<Call Assembly=""MyAssembly.dll"">   at {new string('x', 896)}.x(String parameter)</Call>"));
			Factory.Save();
			var task = new HelpErrorStackLineCountImportServiceTask()
			{ ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			AssertEquals($"{new string('x', 896)}.x(S", Factory.Load<HelpErrorStackLineCount>(new ZQuery()).Single().HSL_StackLine);
		}

		public void TestServiceTaskUsesUtcTime()
		{
			EDIDataRegistry.Instance.StackLineCountLastImport = ZDateTime.Now.AddMonths(-1).ToDateTime();

			var assemblyPK = Guid.NewGuid();
			var classPK = Guid.NewGuid();
			InsertAssembly(assemblyPK, "TheBestAssembly.dll", "C:\\nowhere");
			InsertClass(classPK, assemblyPK, "Warehouse.RF.Core.Business.Processors.UnloadProcessor");
			InsertMethod(Guid.NewGuid(), classPK, "DisplayLineProductData");

			var log = CreateLog(firstProcessed: ZDateTime.UtcNow);
			var calls = GetFileContent("SampleCallsWithTrailingSpaces.xml");
			const string stackLine = "Warehouse.RF.Core.Business.Processors.UnloadProcessor.DisplayLineProductData (System.Boolean convertPacksToQty)";
			var extendedStackLine = $"<Call>  at {stackLine}";

			AssertContains(extendedStackLine, calls);
			CreateLogOccurrence(log, calls);
			Factory.Save();

			var task = new HelpErrorStackLineCountImportServiceTask { ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			var count = Factory.LoadTop1<HelpErrorStackLineCount>(new ZQuery(HelpErrorStackLineCountSchema.HSL_StackLine, stackLine));

			AssertNotNull(count);
			CombineAssertions(() =>
			{
				AssertEquals(nameof(HelpErrorStackLineCount.HSL_Method), "DisplayLineProductData", count.HSL_Method);
				AssertEquals(nameof(HelpErrorStackLineCount.HSL_Type), "Warehouse.RF.Core.Business.Processors.UnloadProcessor", count.HSL_Type);
				AssertEquals(nameof(HelpErrorStackLineCount.HSL_Assembly), "TheBestAssembly.dll", count.HSL_Assembly);
			});
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
		void TestFilesCore(string[] testFiles, string[] validExceptionFiles)
		{
			foreach (var file in testFiles)
			{
				var log = CreateLog();
				CreateLogOccurrence(log, GetFileContent(file));
			}

			Factory.Save();
			var expectedCalls = validExceptionFiles.SelectMany(f => GetStackCalls(GetFileContent(f))).GetStackLineCount();
			var task = new HelpErrorStackLineCountImportServiceTask()
			{ ServiceLogger = new TestServiceLogger() };
			task.RunTask();
			var actualCounts = Factory.Load<HelpErrorStackLineCount>(new ZQuery());
			AssertCallCounts(actualCounts, expectedCalls);
			AssertSavedNumberOfImportedLogs(testFiles.Length);
		}

		static void AssertCallCounts(HelpErrorStackLineCount[] actualCounts, IEnumerable<StackLineCount> expectedCalls)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Number of stack lines is not correct", expectedCalls.Count(), actualCounts.Length);
				foreach (var callCount in expectedCalls)
				{
					AssertEquals($"Error Count for {callCount.StackLine} is incorrect", callCount.Count, actualCounts.FirstOrDefault(helpErrorStackLineCount => CompareStackLineCountToHelperrorStackLineCount(helpErrorStackLineCount, callCount))?.HSL_Count ?? 0);
				}
			});
		}

		static void AssertSavedNumberOfImportedLogs(int expected)
		{
			AssertEquals("Saved number of imported logs", expected, EDIDataRegistry.Instance.StackLineCountNumberOfImportedLogs);
		}

		string GetExceptionXml(string callElements)
		{
			return @"<EDI_Exception_Report>
				<ExceptionDetails>
					<StackTrace>" + callElements + @"
					</StackTrace>
				</ExceptionDetails>
			</EDI_Exception_Report>";
		}

		static bool CompareStackLineCountToHelperrorStackLineCount(HelpErrorStackLineCount helpErrorStackLineCount, StackLineCount callCount)
		{
			return (helpErrorStackLineCount.HSL_Assembly == callCount.Assembly || string.IsNullOrEmpty(callCount.Assembly)) && (helpErrorStackLineCount.HSL_Type == callCount.Type || string.IsNullOrEmpty(callCount.Type)) && (helpErrorStackLineCount.HSL_Method == callCount.Method || string.IsNullOrEmpty(callCount.Method)) && (helpErrorStackLineCount.HSL_Parameters == callCount.Parameters || string.IsNullOrEmpty(callCount.Parameters)) && (helpErrorStackLineCount.HSL_StackLine == callCount.StackLine);
		}

		static string GetFileContent(string fileName)
		{
			var resourceRetriever = new EmbeddedResourceRetriever(typeof(HelpErrorLogStackLineExtractorTest).Assembly);
			var sampleFilesPrefix = "ZClientEDI.Business.Test.IssueManager.SampleFiles.";
			return resourceRetriever.GetString(sampleFilesPrefix + fileName);
		}

		static string[] ReadFileLines(string fileName)
		{
			var content = GetFileContent(fileName);
			return content.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
		}

		static IEnumerable<StackLineCount> GetStackCalls(string xml)
		{
			return XElement.Parse(xml).XPathSelectElements("/Call").Select(element =>
			{
				var count = new StackLineCount();
				count.Assembly = element.Attribute("Assembly")?.Value;
				count.Type = element.Attribute("Type")?.Value;
				count.Method = element.Attribute("Method")?.Value;
				count.Parameters = element.Attribute("Parameters")?.Value;
				count.StackLine = element.Value;
				return count;
			});
		}

		EdiHelpErrorLog CreateLog(ZDateTime? firstProcessed = null)
		{
			var log = Factory.New<EdiHelpErrorLog>();
			log.HE_LogType = "EXC";
			log.HE_ExceptionType = typeof(OutOfMemoryException).ToString();
			log.HE_ExceptionSource = "CargoWiseOne.exe";
			log.HE_FirstReported = ZDateTime.UtcNow;
			log.HE_FirstProcessed = firstProcessed ?? ZDateTime.UtcNow;
			log.HE_LastReported = ZDateTime.UtcNow;
			return log;
		}

		static void CreateLogOccurrence(EdiHelpErrorLog log, string exceptionContent, ZDateTime? exeDate = null)
		{
			var logOccurance = log.Factory.New<HelpErrorLogOccurrence>();
			logOccurance.HO_HE = log.PK;
			logOccurance.HO_EXEDateTime = exeDate ?? ZDateTime.UtcToday;
			logOccurance.HO_XMLData = exceptionContent;
		}

		static void SetUpAssemblies()
		{
			PopulateAssemblies();
			PopulateClasses();
			PopulateMethods();
		}

		static void PopulateAssemblies()
		{
			var fileContent = ReadFileLines("PublishedAssemblies.csv");
			foreach (var assemblyLine in fileContent)
			{
				var parts = assemblyLine.Split(new[] { ',' });
				var pk = Guid.Parse(parts[0]);
				var assembly = parts[1];
				var sourcePath = parts[2];
				InsertAssembly(pk, assembly, sourcePath);
			}
		}

		static void PopulateClasses()
		{
			var fileContent = ReadFileLines("PublishedClasses.csv");
			foreach (var classLine in fileContent)
			{
				var parts = classLine.Split(new[] { ',' });
				var pk = Guid.Parse(parts[0]);
				var assemblyPk = Guid.Parse(parts[1]);
				var className = parts[2];
				InsertClass(pk, assemblyPk, className);
			}
		}

		static void PopulateMethods()
		{
			var fileContent = ReadFileLines("PublishedMethods.csv");
			foreach (var methodLine in fileContent)
			{
				var parts = methodLine.Split(new[] { ',' });
				var pk = Guid.Parse(parts[0]);
				var classPk = Guid.Parse(parts[1]);
				var methodName = parts[2];
				InsertMethod(pk, classPk, methodName);
			}
		}

		static void InsertAssembly(Guid pk, string assembly, string sourcePath)
		{
			const string insert = @"
				INSERT INTO PublishedAssemblies
					(PA_PK
					,PA_AssemblyName
					,PA_SourcePath)
				 VALUES
					(@PK
					,@AssemblyName
					,@SourcePath)";
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				using (var command = connection.Command(insert))
				{
					command.AddParameter("@PK", System.Data.SqlDbType.UniqueIdentifier, pk);
					command.AddParameter("@AssemblyName", System.Data.SqlDbType.NVarChar, assembly);
					command.AddParameter("@SourcePath", System.Data.SqlDbType.NVarChar, sourcePath);
					command.ExecuteNonQuery();
				}
			}
		}

		static void InsertClass(Guid pk, Guid assemblyPK, string className)
		{
			const string insert = @"
				INSERT INTO PublishedClasses
					(PC_PK
					,PC_Assembly
					,PC_ClassName)
				VALUES
					(@PK
					,@AssemblyPK
					,@ClassName)";
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				using (var command = connection.Command(insert))
				{
					command.AddParameter("@PK", System.Data.SqlDbType.UniqueIdentifier, pk);
					command.AddParameter("@AssemblyPK", System.Data.SqlDbType.UniqueIdentifier, assemblyPK);
					command.AddParameter("@ClassName", System.Data.SqlDbType.NVarChar, className);
					command.ExecuteNonQuery();
				}
			}
		}

		static void InsertMethod(Guid pk, Guid classPK, string methodName)
		{
			const string insert = @"
				INSERT INTO PublishedMethods
				   (PM_PK
				   ,PM_Class
				   ,PM_MethodName
				   ,PM_LastSeen)
				VALUES
					(@PK
					,@ClassPK
					,@MethodName
					,sysdatetimeoffset())";
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				using (var command = connection.Command(insert))
				{
					command.AddParameter("@PK", System.Data.SqlDbType.UniqueIdentifier, pk);
					command.AddParameter("@ClassPK", System.Data.SqlDbType.UniqueIdentifier, classPK);
					command.AddParameter("@MethodName", System.Data.SqlDbType.NVarChar, methodName);
					command.ExecuteNonQuery();
				}
			}
		}
	}
}

namespace Enterprise.Client.EDI.ServiceTask.Testing
{
	struct StackLineCount
	{
		public string Assembly { get; set; }
		public string Type { get; set; }
		public string Method { get; set; }
		public string Parameters { get; set; }
		public string StackLine { get; set; }
		public int Count { get; set; }
	}
}
