using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace AnalyzersRunner
{
	[RequiresSoftware(RequiredSoftware.VisualStudio | RequiredSoftware.DotNetSdk)]
	[FrequentlyFailing]
	[DatCapabilityRequirement("SOURCE_CODE")]
	[DatCapabilityRequirement("SYMLINK")]
	[ALPOnly]
	public class RunAnalyzers : TestCase
	{
		// Test methods dynamically created by AnalyzersUnitTestGenerator.exe in the post-build event of this project will be inserted here.

		public void TestIntentionalFailures()
		{
			const string projectFileRelativePath = "Common\\Tools\\Analyzers\\AnalyzersRunner.FunctionalTestingTarget\\AnalyzersRunner.FunctionalTestingTarget.csproj";

			var projectFileFullPath = Path.Combine(BaseSourcePath, projectFileRelativePath);

			Assert($"AnalyzersRunner.FunctionalTestingTarget project does not exist. expected path = {projectFileFullPath}", File.Exists(projectFileFullPath));

			//functional testing of the analyzers, to make sure that
			var analysisOutput = ProcessRunner.RunAnalysisOnProject(projectFileRelativePath);

			if (analysisOutput.Length == 0)
			{
				Fail("No analyzer issues reported. We expect to have several reported");
			}
			else
			{
				/*
				 example analysis output:
				  
					<a href='vsnet:C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CA1028.cs#4'>C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CA1028.cs(4,14)</a>: [Error] CA1028: If possible, make the underlying type of CA1028 System.Int32 instead of sbyte<br/>
					<a href='vsnet:C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CA1028.cs#4'>C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CA1028.cs(4,14)</a>: [Error] CA1028: If possible, make the underlying type of CA1028 System.Int32 instead of sbyte<br/>
					<a href='vsnet:C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1024.cs#13'>C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1024.cs(13,8)</a>: [Error] CW1024: dictionary.TryGetValue("test", out _) - You have made multiple calls to the same concurrent collection. This is not valid, because there is nothing stopping the state of the dictionary from changing in between method calls. You should use the thread safe methods available on the concurrent type to ensure the state is reliable, or create a snapshot of the collection which will not change and work with that.<br/>
					<a href='vsnet:C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1030A.cs#10'>C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1030A.cs(10,18)</a>: [Error] CW1030A: mode: FileMode.OpenOrCreate - File.OpenWrite will open an existing file for writing without truncating it, you probably mean File.Create() which will overwrite an existing file.<br/>
					<a href='vsnet:C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1031.cs#11'>C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1031.cs(11,8)</a>: [Error] CW1031: Environment.OSVersion.Version - Use CargoWise.Common.VersionHelper to check feature availability.<br/>
					<a href='vsnet:C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1031.cs#16'>C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1031.cs(16,22)</a>: [Error] CW1031: GetVersionEx - Use CargoWise.Common.VersionHelper to check feature availability.<br/>
					<a href='vsnet:C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1050.cs#6'>C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1050.cs(6,14)</a>: [Error] CW1050: ValueInSeconds - Use System.TimeSpan instead of int, decimal or byte types for a duration.<br/>
					<a href='vsnet:C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1050.cs#9'>C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1050.cs(9,14)</a>: [Error] CW1050: ValueInMinutes - Use System.TimeSpan instead of int, decimal or byte types for a duration.<br/>
					<a href='vsnet:C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1050.cs#12'>C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1050.cs(12,18)</a>: [Error] CW1050: ValueInHours - Use System.TimeSpan instead of int, decimal or byte types for a duration.<br/>
					<a href='vsnet:C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1054.cs#8'>C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1054.cs(8,8)</a>: [Error] CW1054: @"c:\windows" - Do not use hard coded paths<br/>
					<a href='vsnet:C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1055.cs#10'>C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1055.cs(10,8)</a>: [Error] CW1055: Process.GetProcesses() - Process.GetProcesses/GetProcessesByName throws exceptions when there are insuffient privileges to see all processes on the system. Use ProcessLocator instead, which will only return processes visible under the current user's security privileges.<br/>
					<a href='vsnet:C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1055.cs#13'>C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1055.cs(13,8)</a>: [Error] CW1055: Process.GetProcessesByName("") - Process.GetProcesses/GetProcessesByName throws exceptions when there are insuffient privileges to see all processes on the system. Use ProcessLocator instead, which will only return processes visible under the current user's security privileges.<br/>
					<a href='vsnet:C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1056.cs#14'>C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1056.cs(14,4)</a>: [Error] CW1056: GC.Collect() - Directly calling GC.Collect can cause excessive % of time in GC, and it's usually a sign that you are doing something else wrong. Use GCWrapper to call GC.Collect indirectly to free memory from large object you are finished using, or to free memory before starting a memory intensive operation.<br/>
					<a href='vsnet:C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1058.cs#10'>C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1058.cs(10,8)</a>: [Error] CW1058: Debugger.IsAttached - Logic that the customer sees but the developer never sees may have bugs. Remove it.<br/>
					<a href='vsnet:C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1060.cs#10'>C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1060.cs(10,8)</a>: [Error] CW1060: DateTime.Now - Delete the usage. If this runs on a WiseCloud server or local workstation you will end up with that machine's date-time based on the user locale settings. You will not know where the server is based or the locale settings.<br/>
					<a href='vsnet:C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1061.cs#10'>C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1061.cs(10,8)</a>: [Error] CW1061: DateTime.UtcNow - Delete the usage. If this runs on a WiseCloud server or local workstation the result will be calculated from the machine time. This time may not be correct or configured for the context.<br/>
					<a href='vsnet:C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1062.cs#10'>C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1062.cs(10,8)</a>: [Error] CW1062: DateTime.Today - Delete the usage. If this runs on a WiseCloud server or local workstation the result will be calculated from the machine time. This date may not be correct or configured for the context.<br/>
					<a href='vsnet:C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1066.cs#14'>C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1066.cs(14,6)</a>: [Error] CW1066: goto case 0; - Almost always indicates poorly factored code. Consider the bigger picture, not just this line of code. Is this method doing too much? Can it be split into several methods?<br/>
					<a href='vsnet:C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1068.cs#10'>C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1068.cs(10,4)</a>: [Error] CW1068: Math.Round(5.2) - Math.Round has some problems. Please use Enterprise.ZArchitecture.Core.Utilities.Round instead.<br/>
					<a href='vsnet:C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1069.cs#10'>C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1069.cs(10,4)</a>: [Error] CW1069: EventLog.WriteEntry("", "") - Use Enterprise.ZArchitecture.Core.SafeEventLogExtensions.SafeWriteEntry() extension method or SafeEventLogExtensions.SafeWriteEntryToApplicationLog() static method instead.<br/>
					<a href='vsnet:C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1071.cs#10'>C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1071.cs(10,4)</a>: [Error] CW1071: GC.WaitForPendingFinalizers() - Remove the call to WaitForPendingFinalizers or GetTotalMemory(true). GetTotalMemory(false) is OK. Blocking an STA thread can lead to deadlocks if the finalizer thread needs to marshall a call to a COM object in the STA.<br/>
					<a href='vsnet:C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1071.cs#13'>C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1071.cs(13,4)</a>: [Error] CW1071: GC.GetTotalMemory(true) - Remove the call to WaitForPendingFinalizers or GetTotalMemory(true). GetTotalMemory(false) is OK. Blocking an STA thread can lead to deadlocks if the finalizer thread needs to marshall a call to a COM object in the STA.<br/>
					<a href='vsnet:C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1078.cs#10'>C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1078.cs(10,4)</a>: [Error] CW1078: Process.Start - Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.<br/>
					<a href='vsnet:C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1079.cs#10'>C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1079.cs(10,8)</a>: [Error] CW1079: ex.Message == "do not compare" - Do not compare on Exception.Message. When changing cultures the 'Message' property may change.<br/>
					<a href='vsnet:C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1083.cs#6'>C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1083.cs(6,17)</a>: [Error] CW1083: SL_SE_NKEvent = "bad code" - Using string literals for event codes is error prone and will fail when Richard renames them. Use Events.* instead.<br/>
					<a href='vsnet:C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1103.cs#10'>C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1103.cs(10,19)</a>: [Error] CW1103: "dd-MMM-yy" - Using string constants is not the best way of date/time output. Use ZDateTime.ToShortDateString() instead.<br/>
					<a href='vsnet:C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1108.cs#10'>C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1108.cs(10,12)</a>: [Error] CW1108: DataSet - Remove. Use the BusinessObjectFactory instead, unless this BusinessObject is not in the database.<br/>
					<a href='vsnet:C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1122.cs#10'>C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\CW1122.cs(10,4)</a>: [Error] CW1122: DateTime.Parse("test") - DateTime.Parse is affected by regional settings, so use the constructor instead.<br/>
					<a href='vsnet:C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\EDI011.cs#10'>C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\EDI011.cs(10,8)</a>: [Error] EDI011: Path.GetTempPath() - Use Enterprise.ZArchitecture.Core.TempFile, CargoWise.IO.Temp or NUnit.Framework.TempForTest to access temporary files<br/>
					<a href='vsnet:C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\EDI011.cs#13'>C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\CargoWise.Analyzers\EDI011.cs(13,8)</a>: [Error] EDI011: Path.GetTempFileName() - Use Enterprise.ZArchitecture.Core.TempFile, CargoWise.IO.Temp or NUnit.Framework.TempForTest to access temporary files<br/>
					<a href='vsnet:C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\WTG.Analyzers\WTG1001.cs#6'>C:\git\wtg\CargoWise\Dev\Common\Tools\Analyzers\AnalyzersRunner.FunctionalTestingTarget\WTG.Analyzers\WTG1001.cs(6,3)</a>: [Error] WTG1001: Our convention is to omit the 'private' modifier where it is already the default.

				*/

				var analyzerIssuesRemainingToBeAsserted = analysisOutput.Trim().Split(new[] { "<br/>\r\n", "<br/>\r", "<br/>\n", }, StringSplitOptions.None).ToList();

				void AssertAnalyzerIssue(string diagnosticId, int lineNumber, int columnNumber, string fileName, ReportDiagnostic severity)
				{
					var expectedMessage = $"AnalyzersRunner.FunctionalTestingTarget\\{fileName}({lineNumber},{columnNumber})</a>: [{severity}] {diagnosticId}:";

					var analyzerIssueIndex = analyzerIssuesRemainingToBeAsserted.FindIndex(analyzerIssue => analyzerIssue.IndexOf(expectedMessage) != -1);

					var hasAnalysisError = analyzerIssueIndex != -1;

					Assert($"missing analyzer issue. diagnostic id = {diagnosticId}. expected line = {lineNumber}. expected column = {columnNumber}\nstring to look for in analysis output = '{expectedMessage}'\n\nremaining issues:\n{string.Join("\n\t", analyzerIssuesRemainingToBeAsserted)}\n\noriginal full analysis output from AnalyzersRunner:\n{analysisOutput}", hasAnalysisError);

					if (hasAnalysisError)
					{
						analyzerIssuesRemainingToBeAsserted.RemoveAt(analyzerIssueIndex);
					}
				}

				var expectedIssues = new[]
				{
					////////////////////////////////////////////
					//
					// CargoWise.Analyzers

					//CUS001:Some RefZZ Fields Are Case Insensitive Rule
					new { id = "CUS001", line = 10, character = 8, file = "CargoWise.Analyzers\\CUS001.cs", severity = ReportDiagnostic.Error },
					new { id = "CUS001", line = 13, character = 8, file = "CargoWise.Analyzers\\CUS001.cs", severity = ReportDiagnostic.Error },

					//CW1013:CargoWise ProgressBar Rule
					new { id = "CW1013", line = 10, character = 8, file = "CargoWise.Analyzers\\CW1013.cs", severity = ReportDiagnostic.Error },

					//CW1014:Embedded Icon Rule
					new { id = "CW1014", line = 14, character = 4, file = "CargoWise.Analyzers\\CW1014.cs", severity = ReportDiagnostic.Error },

					//CW1015:No Application.OpenForms Rule
					new { id = "CW1015", line = 8, character = 25, file = "CargoWise.Analyzers\\CW1015.cs", severity = ReportDiagnostic.Error },

					//CW1016:Don't use FlexCelPdfExport
					new { id = "CW1016", line = 10, character = 14, file = "CargoWise.Analyzers\\CW1016.cs", severity = ReportDiagnostic.Error },

					//CW1017:Non DPI-aware code has been detected
					new { id = "CW1017", line = 10, character = 4, file = "CargoWise.Analyzers\\CW1017.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1017", line = 11, character = 4, file = "CargoWise.Analyzers\\CW1017.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1017", line = 12, character = 4, file = "CargoWise.Analyzers\\CW1017.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1017", line = 13, character = 4, file = "CargoWise.Analyzers\\CW1017.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1017", line = 14, character = 4, file = "CargoWise.Analyzers\\CW1017.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1017", line = 15, character = 4, file = "CargoWise.Analyzers\\CW1017.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1017", line = 16, character = 4, file = "CargoWise.Analyzers\\CW1017.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1017", line = 17, character = 4, file = "CargoWise.Analyzers\\CW1017.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1017", line = 18, character = 4, file = "CargoWise.Analyzers\\CW1017.cs", severity = ReportDiagnostic.Error },

					//CW1018:Http Application Rule
					new { id = "CW1018", line = 6, character = 17, file = "CargoWise.Analyzers\\CW1018.cs", severity = ReportDiagnostic.Error },

					//CW1018A:Http Application Db App Settings Rule
					new { id = "CW1018A", line = 10, character = 8, file = "CargoWise.Analyzers\\CW1018A.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1018A", line = 11, character = 8, file = "CargoWise.Analyzers\\CW1018A.cs", severity = ReportDiagnostic.Error },

					//CW1019:No SqlTransaction.Rollback Rule
					new { id = "CW1019", line = 11, character = 4, file = "CargoWise.Analyzers\\CW1019.cs", severity = ReportDiagnostic.Error },

					//CW1020:Don't Use Currency Manager Current Rule
					new { id = "CW1020", line = 10, character = 8, file = "CargoWise.Analyzers\\CW1020.cs", severity = ReportDiagnostic.Error },

					//CW1021:Static Fields Are Thread Static Rule
					new { id = "CW1021", line = 6, character = 24, file = "CargoWise.Analyzers\\CW1021.cs", severity = ReportDiagnostic.Error },

					//CW1022:Thread Static Set In Static Initializer Rule
					new { id = "CW1022", line = 8, character = 3, file = "CargoWise.Analyzers\\CW1022.cs", severity = ReportDiagnostic.Error },

					//CW1023:[Immutable] types should be immutable
					new { id = "CW1023", line = 9, character = 14, file = "CargoWise.Analyzers\\CW1023.cs", severity = ReportDiagnostic.Error },

					//CW1024:Bad Concurrent Collection Access
					new { id = "CW1024", line = 13, character = 8, file = "CargoWise.Analyzers\\CW1024.cs", severity = ReportDiagnostic.Error },

					//CW1030A:File.OpenWrite Rule
					new { id = "CW1030A", line = 10, character = 18, file = "CargoWise.Analyzers\\CW1030A.cs", severity = ReportDiagnostic.Error },

					//CW1031:Do not use GetVersionEx or Environment.OSVersion to check feature availability
					new { id = "CW1031", line = 11, character = 8, file = "CargoWise.Analyzers\\CW1031.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1031", line = 16, character = 22, file = "CargoWise.Analyzers\\CW1031.cs", severity = ReportDiagnostic.Error },

					//CW1032:Use Moq for Mocking
					new { id = "CW1032", line = 8, character = 8, file = "CargoWise.Analyzers\\CW1032.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1032", line = 9, character = 8, file = "CargoWise.Analyzers\\CW1032.cs", severity = ReportDiagnostic.Error },

					//CW1040:Check for mixed arithmatic between scaled and unscaled components
					new { id = "CW1040", line = 10, character = 4, file = "CargoWise.Analyzers\\CW1040.cs", severity = ReportDiagnostic.Error },

					//CW1041:Check for improper use of the scaling tools
					new { id = "CW1041", line = 11, character = 8, file = "CargoWise.Analyzers\\CW1041.cs", severity = ReportDiagnostic.Error },

					//CW1042:Check for invalid assignments between scaled and unscaled fields/properties
					new { id = "CW1042", line = 14, character = 4, file = "CargoWise.Analyzers\\CW1042.cs", severity = ReportDiagnostic.Error },

					//CW1043:eAdaptor naming rule
					new { id = "CW1043", line = 8, character = 8, file = "CargoWise.Analyzers\\CW1043.cs", severity = ReportDiagnostic.Error },

					//CW1044:Factory.GetDatabaseCount() Collection Count Rule
					new { id = "CW1044", line = 12, character = 8, file = "CargoWise.Analyzers\\CW1044.cs", severity = ReportDiagnostic.Error },

					//CW1046:Do Not Specify Tooltips Manually Rule
					new { id = "CW1046", line = 11, character = 4, file = "CargoWise.Analyzers\\CW1046.cs", severity = ReportDiagnostic.Error },

					//CW1049:Don't Use Application Do Events Rule
					new { id = "CW1049", line = 10, character = 4, file = "CargoWise.Analyzers\\CW1049.cs", severity = ReportDiagnostic.Error },

					//CW1050:Use System.TimeSpan Type For A Duration
					new { id = "CW1050", line = 6, character = 14, file = "CargoWise.Analyzers\\CW1050.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1050", line = 9, character = 14, file = "CargoWise.Analyzers\\CW1050.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1050", line = 12, character = 18, file = "CargoWise.Analyzers\\CW1050.cs", severity = ReportDiagnostic.Error },

					//CW1051:Do not use BaseSourcePath for resources
					new { id = "CW1051", line = 11, character = 21, file = "CargoWise.Analyzers\\CW1051.cs", severity = ReportDiagnostic.Error },

					//CW1052:Do Not Cast Factory Method
					new { id = "CW1052", line = 11, character = 8, file = "CargoWise.Analyzers\\CW1052.cs", severity = ReportDiagnostic.Error },

					//CW1053:Do not use default SingleRefDatabaseName constant
					new { id = "CW1053", line = 8, character = 8, file = "CargoWise.Analyzers\\CW1053.cs", severity = ReportDiagnostic.Error },

					//CW1054:Do Not Use Hard Coded Paths
					new { id = "CW1054", line = 8, character = 8, file = "CargoWise.Analyzers\\CW1054.cs", severity = ReportDiagnostic.Error },

					//CW1055:Do Not Use Processes.GetProcess or Process.GetProcessByName
					new { id = "CW1055", line = 10, character = 8, file = "CargoWise.Analyzers\\CW1055.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1055", line = 13, character = 8, file = "CargoWise.Analyzers\\CW1055.cs", severity = ReportDiagnostic.Error },

					//CW1056:Do Not Use GC.Collect()
					new { id = "CW1056", line = 10, character = 4, file = "CargoWise.Analyzers\\CW1056.cs", severity = ReportDiagnostic.Error },

					//CW1057:Do Not Hardcode Mail Server Name
					new { id = "CW1057", line = 8, character = 8, file = "CargoWise.Analyzers\\CW1057.cs", severity = ReportDiagnostic.Error },

					//CW1058:Do Not Use Debugger.IsAttached
					new { id = "CW1058", line = 10, character = 8, file = "CargoWise.Analyzers\\CW1058.cs", severity = ReportDiagnostic.Error },

					//CW1060:Do not use System.DateTime.Now Rule
					new { id = "CW1060", line = 10, character = 8, file = "CargoWise.Analyzers\\CW1060.cs", severity = ReportDiagnostic.Error },

					//CW1061:Do not use System.DateTime.UtcNow Rule
					new { id = "CW1061", line = 10, character = 8, file = "CargoWise.Analyzers\\CW1061.cs", severity = ReportDiagnostic.Error },

					//CW1062:Do not use System.DateTime.Today Rule
					new { id = "CW1062", line = 10, character = 8, file = "CargoWise.Analyzers\\CW1062.cs", severity = ReportDiagnostic.Error },

					//CW1063:Do Not Use System.Windows.Forms.Screen Class
					new { id = "CW1063", line = 10, character = 8, file = "CargoWise.Analyzers\\CW1063.cs", severity = ReportDiagnostic.Error },

					//CW1065:Encrypt SQL Connection Rule
					new { id = "CW1065", line = 8, character = 16, file = "CargoWise.Analyzers\\CW1065.cs", severity = ReportDiagnostic.Error },

					//CW1066:Goto Default Or Case Rule
					new { id = "CW1066", line = 14, character = 6, file = "CargoWise.Analyzers\\CW1066.cs", severity = ReportDiagnostic.Error },

					//CW1067:Application ThreadException Rule
					new { id = "CW1067", line = 10, character = 4, file = "CargoWise.Analyzers\\CW1067.cs", severity = ReportDiagnostic.Error },

					//CW1068:Do Not Use Math.Round
					new { id = "CW1068", line = 10, character = 4, file = "CargoWise.Analyzers\\CW1068.cs", severity = ReportDiagnostic.Error },

					//CW1069:Do Not Use EventLog.WriteEntry
					new { id = "CW1069", line = 10, character = 4, file = "CargoWise.Analyzers\\CW1069.cs", severity = ReportDiagnostic.Error },

					//CW1071:Do Not Use GC.WaitForPendingFinalizers or .GetTotalMemory(true)
					new { id = "CW1071", line = 10, character = 4, file = "CargoWise.Analyzers\\CW1071.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1071", line = 13, character = 4, file = "CargoWise.Analyzers\\CW1071.cs", severity = ReportDiagnostic.Error },

					//CW1072:Do Not Use DB Name In SQL Commands
					new { id = "CW1072", line = 8, character = 8, file = "CargoWise.Analyzers\\CW1072.cs", severity = ReportDiagnostic.Error },

					//CW1073:Do Not Hardcode SQL Password
					new { id = "CW1073", line = 10, character = 8, file = "CargoWise.Analyzers\\CW1073.cs", severity = ReportDiagnostic.Error },

					//CW1074:Do Not Hardcode Developer DB Username
					new { id = "CW1074", line = 8, character = 8, file = "CargoWise.Analyzers\\CW1074.cs", severity = ReportDiagnostic.Error },

					//CW1075:Do Not Use Loop To Add Or Conditions To Filter
					new { id = "CW1075", line = 14, character = 23, file = "CargoWise.Analyzers\\CW1075.cs", severity = ReportDiagnostic.Error },

					//CW1076:Do Not Use MessageBox.Show
					new { id = "CW1076", line = 10, character = 4, file = "CargoWise.Analyzers\\CW1076.cs", severity = ReportDiagnostic.Error },

					//CW1077:Do Not Use Microsoft.Office.Interop
					new { id = "CW1077", line = 2, character = 7, file = "CargoWise.Analyzers\\CW1077.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1077", line = 3, character = 23, file = "CargoWise.Analyzers\\CW1077.cs", severity = ReportDiagnostic.Error },

					//CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.
					new { id = "CW1078", line = 10, character = 4, file = "CargoWise.Analyzers\\CW1078.cs", severity = ReportDiagnostic.Error },

					//CW1079:Do not compare on Exception.Message
					new { id = "CW1079", line = 10, character = 8, file = "CargoWise.Analyzers\\CW1079.cs", severity = ReportDiagnostic.Error },

					//CW1080:Do Not Use BusinessObjectCollection IsAssignableFrom
					new { id = "CW1080", line = 10, character = 8, file = "CargoWise.Analyzers\\CW1080.cs", severity = ReportDiagnostic.Error },

					//CW1081:IsSubClassOf Typeof BusinessObjectCollection Rule
					new { id = "CW1081", line = 10, character = 8, file = "CargoWise.Analyzers\\CW1081.cs", severity = ReportDiagnostic.Error },

					//CW1082W:DoNotUseTooManyArguments
					new { id = "CW1082W", line = 9, character = 32, file = "CargoWise.Analyzers\\CW1082.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1082W", line = 13, character = 33, file = "CargoWise.Analyzers\\CW1082.cs", severity = ReportDiagnostic.Error },

					//CW1082:DoNotUseTooManyArguments
					new { id = "CW1082", line = 17, character = 33, file = "CargoWise.Analyzers\\CW1082.cs", severity = ReportDiagnostic.Error },

					//CW1083:Do Not Use String Literals For Event Codes
					new { id = "CW1083", line = 6, character = 17, file = "CargoWise.Analyzers\\CW1083.cs", severity = ReportDiagnostic.Error },

					//CW1084:Virtual New AddNew
					new { id = "CW1084", line = 6, character = 27, file = "CargoWise.Analyzers\\CW1084.cs", severity = ReportDiagnostic.Error },

					//CW1086:Do Not Use System.Web.Mail
					new { id = "CW1086", line = 2, character = 7, file = "CargoWise.Analyzers\\CW1086.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1086", line = 11, character = 8, file = "CargoWise.Analyzers\\CW1086.cs", severity = ReportDiagnostic.Error },

					//CW1087:Do Not Use PrinterSettings.InstalledPrinters
					new { id = "CW1087", line = 10, character = 8, file = "CargoWise.Analyzers\\CW1087.cs", severity = ReportDiagnostic.Error },

					//CW1088:Do Not Use System.Windows.Forms.Clipboard
					new { id = "CW1088", line = 10, character = 4, file = "CargoWise.Analyzers\\CW1088.cs", severity = ReportDiagnostic.Error },

					//CW1089:Do Not Use Assembly.GetEntryAssembly()
					new { id = "CW1089", line = 10, character = 4, file = "CargoWise.Analyzers\\CW1089.cs", severity = ReportDiagnostic.Error },

					//CW1090:Don't use System.Windows.Forms dialogs
					new { id = "CW1090", line = 10, character = 8, file = "CargoWise.Analyzers\\CW1090.cs", severity = ReportDiagnostic.Error },

					//CW1091:Do Not Set CausesValidation to false
					new { id = "CW1091", line = 10, character = 37, file = "CargoWise.Analyzers\\CW1091.cs", severity = ReportDiagnostic.Error },

					//CW1093:Do Not Use System.Windows.Forms.ToolStrip Controls
					new { id = "CW1093", line = 10, character = 8, file = "CargoWise.Analyzers\\CW1093.cs", severity = ReportDiagnostic.Error },

					//CW1094:ToolTipText Should Be Set With Res.GetString
					new { id = "CW1094", line = 12, character = 18, file = "CargoWise.Analyzers\\CW1094.cs", severity = ReportDiagnostic.Error },

					//CW1095:HTTP Parameter Names Should Not Be Translated
					new { id = "CW1095", line = 10, character = 4, file = "CargoWise.Analyzers\\CW1095.cs", severity = ReportDiagnostic.Error },

					//CW1097:Do Not Hardcode Tmp Or Temp Path
					new { id = "CW1097", line = 8, character = 9, file = "CargoWise.Analyzers\\CW1097.cs", severity = ReportDiagnostic.Error },

					//CW1098:Do Not Initialize String Fields With Res.GetString
					new { id = "CW1098", line = 6, character = 36, file = "CargoWise.Analyzers\\CW1098.cs", severity = ReportDiagnostic.Error },

					//CW1099:Res.GetString Default Text Must Be String Literal
					new { id = "CW1099", line = 8, character = 38, file = "CargoWise.Analyzers\\CW1099.cs", severity = ReportDiagnostic.Error },

					//CW1102:Do Not Use System Windows Forms Tab Page Analyzer
					new { id = "CW1102", line = 10, character = 35, file = "CargoWise.Analyzers\\CW1102.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1102", line = 14, character = 23, file = "CargoWise.Analyzers\\CW1102.cs", severity = ReportDiagnostic.Error },

					//CW1103:Do Not Use String Literals For Date Formats
					new { id = "CW1103", line = 10, character = 19, file = "CargoWise.Analyzers\\CW1103.cs", severity = ReportDiagnostic.Error },

					// CW1104:Do Not Use System Windows Forms Tab Control Analyzer
					new { id = "CW1104", line = 10, character = 41, file = "CargoWise.Analyzers\\CW1104.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1104", line = 14, character = 26, file = "CargoWise.Analyzers\\CW1104.cs", severity = ReportDiagnostic.Error },

					//CW1105:Do Not Use System Windows Forms User Control Or KUser Control Analyzer
					new { id = "CW1105", line = 10, character = 43, file = "CargoWise.Analyzers\\CW1105.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1105", line = 14, character = 27, file = "CargoWise.Analyzers\\CW1105.cs", severity = ReportDiagnostic.Error },

					// CW1106:Do Not Leave In Debug Messages Analyzer
					new { id = "CW1106", line = 12, character = 4, file = "CargoWise.Analyzers\\CW1106.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1106", line = 13, character = 4, file = "CargoWise.Analyzers\\CW1106.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1106", line = 14, character = 4, file = "CargoWise.Analyzers\\CW1106.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1106", line = 15, character = 4, file = "CargoWise.Analyzers\\CW1106.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1106", line = 16, character = 4, file = "CargoWise.Analyzers\\CW1106.cs", severity = ReportDiagnostic.Error },

					//CW1107::Do Not Use DbConnection Methods Analyzer
					new { id = "CW1107", line = 11, character = 4, file = "CargoWise.Analyzers\\CW1107.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1107", line = 12, character = 4, file = "CargoWise.Analyzers\\CW1107.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1107", line = 13, character = 4, file = "CargoWise.Analyzers\\CW1107.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1107", line = 14, character = 14, file = "CargoWise.Analyzers\\CW1107.cs", severity = ReportDiagnostic.Error },

					//CW1108:Do Not Use DataSet
					new { id = "CW1108", line = 10, character = 12, file = "CargoWise.Analyzers\\CW1108.cs", severity = ReportDiagnostic.Error },

					//CW1109:Do Not Use System Windows Forms Form Or KForm Analyzer
					new { id = "CW1109", line = 10, character = 29, file = "CargoWise.Analyzers\\CW1109.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1109", line = 14, character = 20, file = "CargoWise.Analyzers\\CW1109.cs", severity = ReportDiagnostic.Error },

					// CW1111:Do Not Use System Windows Forms Tab Draw Mode Owner Draw Fixed Analyzer
					new { id = "CW1111", line = 13, character = 26, file = "CargoWise.Analyzers\\CW1111.cs", severity = ReportDiagnostic.Error },

					// CW1112:Do Not Bind To Enabled Analyzer
					new { id = "CW1112", line = 13, character = 21, file = "CargoWise.Analyzers\\CW1112.cs", severity = ReportDiagnostic.Error },

					//CW1113:Do Not Show Message Box From Business Layer Analyzer
					new { id = "CW1113", line = 13, character = 5, file = "CargoWise.Analyzers\\CW1113.cs", severity = ReportDiagnostic.Error },

					// CW1115:Use Set Temporary User Context Instead Of Set User Context Analyzer
					new { id = "CW1115", line = 12, character = 4, file = "CargoWise.Analyzers\\CW1115.cs", severity = ReportDiagnostic.Error },

					//CW1116:Use Enterprise Core Data Wrapper Classes Analyzer
					new { id = "CW1116", line = 11, character = 8, file = "CargoWise.Analyzers\\CW1116.cs", severity = ReportDiagnostic.Error },

					//CW1117:Duplicate For Event Scripts Analyzer
					new { id = "CW1117", line = 9, character = 8, file = "CargoWise.Analyzers\\CW1117.cs", severity = ReportDiagnostic.Error },

					//CW1118:Use Keys KeyCode And Keys Modifiers Bitmask Analyzer
					new { id = "CW1118", line = 11, character = 9, file = "CargoWise.Analyzers\\CW1118.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1118", line = 12, character = 9, file = "CargoWise.Analyzers\\CW1118.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1118", line = 13, character = 9, file = "CargoWise.Analyzers\\CW1118.cs", severity = ReportDiagnostic.Error },

					//CW1119:Do Not Use SL_EventTime Table Column Analyzer
					new { id = "CW1119", line = 9, character = 8, file = "CargoWise.Analyzers\\CW1119.cs", severity = ReportDiagnostic.Error },

					//CW1120:Do Not Get Icons Images From Rex Or Resources File Analyzer
					new { id = "CW1120", line = 11, character = 8, file = "CargoWise.Analyzers\\CW1120.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1120", line = 12, character = 8, file = "CargoWise.Analyzers\\CW1120.cs", severity = ReportDiagnostic.Error },

					//CW1121:Do Not Include Column Values Or Names In Error Reporter Key Analyzer
					new { id = "CW1121", line = 13, character = 29, file = "CargoWise.Analyzers\\CW1121.cs", severity = ReportDiagnostic.Error },

					//CW1122:Do Not Use DateTime Parse Method
					new { id = "CW1122", line = 10, character = 4, file = "CargoWise.Analyzers\\CW1122.cs", severity = ReportDiagnostic.Error },

					//CW1123:Do Not Use Cached Value Analyzer
					new { id = "CW1123", line = 15, character = 28, file = "CargoWise.Analyzers\\CW1123.cs", severity = ReportDiagnostic.Error },

					//CW1124:Do Not Use Cached Property Analyzer
					new { id = "CW1124", line = 17, character = 27, file = "CargoWise.Analyzers\\CW1124.cs", severity = ReportDiagnostic.Error },

					//CW1125:Use System Views Instead Of System Tables Analyzer
					new { id = "CW1125", line = 9, character = 26, file = "CargoWise.Analyzers\\CW1125.cs", severity = ReportDiagnostic.Error },

					//CW1127:Db Set Offsets Unavailable Analyzer
					new { id = "CW1127", line = 9, character = 14, file = "CargoWise.Analyzers\\CW1127.cs", severity = ReportDiagnostic.Error },

					//CW1128:Db DbReindex Deprecated Analyzer
					new { id = "CW1128", line = 9, character = 15, file = "CargoWise.Analyzers\\CW1128.cs", severity = ReportDiagnostic.Error },

					//CW1129:Db Index Defrag Deprecated Analyzer
					new { id = "CW1129", line = 9, character = 15, file = "CargoWise.Analyzers\\CW1129.cs", severity = ReportDiagnostic.Error },

					//CW1130:Db Index Key Property Deprecated Analyzer
					new { id = "CW1130", line = 9, character = 16, file = "CargoWise.Analyzers\\CW1130.cs", severity = ReportDiagnostic.Error },

					//CW1131:Db Sp_DbCmptLevel Deprecated Analyzer
					new { id = "CW1131", line = 9, character = 14, file = "CargoWise.Analyzers\\CW1131.cs", severity = ReportDiagnostic.Error },

					//CW1132:Db SpLock Deprecated Analyzer
					new { id = "CW1132", line = 9, character = 14, file = "CargoWise.Analyzers\\CW1132.cs", severity = ReportDiagnostic.Error },

					//CW1134:Non Abstract Test Classes Should Be Internal Sealed Analyzer
					new { id = "CW1134", line = 9, character = 3, file = "CargoWise.Analyzers\\CW1134.cs", severity = ReportDiagnostic.Error },

					//CW1135:Do Not Use Country Specific Business Rule Analyzer
					new { id = "CW1135", line = 12, character = 8, file = "CargoWise.Analyzers\\CW1135.cs", severity = ReportDiagnostic.Error },

					//CW1136:Do Not Use System Runtime Serialization Formatters Binary
					new { id = "CW1136", line = 5, character = 7, file = "CargoWise.Analyzers\\CW1136.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1136", line = 13, character = 20, file = "CargoWise.Analyzers\\CW1136.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1136", line = 14, character = 4, file = "CargoWise.Analyzers\\CW1136.cs", severity = ReportDiagnostic.Error },

					//CW1138:Do Not Use System Runtime Remoting
					new { id = "CW1138", line = 4, character = 7, file = "CargoWise.Analyzers\\CW1138.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1138", line = 12, character = 8, file = "CargoWise.Analyzers\\CW1138.cs", severity = ReportDiagnostic.Error },

					//CW1139:Do Not Use Null Literal As Return In Coalesce Expression Analyzer
					new { id = "CW1139", line = 9, character = 23, file = "CargoWise.Analyzers\\CW1139.cs", severity = ReportDiagnostic.Error },

					//CW1140:Column Name Case Analyzer
					new { id = "CW1140", line = 7, character = 34, file = "CargoWise.Analyzers\\CW1140.cs", severity = ReportDiagnostic.Error },

					// CW1141:Do Not Use Unity
					new { id = "CW1141", line = 2, character = 7, file = "CargoWise.Analyzers\\CW1141.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1141", line = 3, character = 7, file = "CargoWise.Analyzers\\CW1141.cs", severity = ReportDiagnostic.Error },

					//CW1142:Await Expressions In Assert That Argument Analyzer
					new { id = "CW1142", line = 19, character = 16, file = "CargoWise.Analyzers\\CW1142.cs", severity = ReportDiagnostic.Error },

					//CW1143:DoNotUseInvalidCsprojRemoveAttributeValueAnalyzer
					new { id = "CW1143", line = 12, character = 6, file = "AnalyzersRunner.FunctionalTestingTarget.csproj", severity = ReportDiagnostic.Error },

					// CW1145 - SingletonAnalyzer
					new { id = "CW1145", line = 10, character = 24, file = "CargoWise.Analyzers\\CW1145.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1145", line = 13, character = 31, file = "CargoWise.Analyzers\\CW1145.cs", severity = ReportDiagnostic.Error },

					// CW1147 - DoNotUseWebControlsAnalyzer
					new { id = "CW1147", line = 10, character = 8, file = "CargoWise.Analyzers\\CW1147.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1147", line = 11, character = 8, file = "CargoWise.Analyzers\\CW1147.cs", severity = ReportDiagnostic.Error },

					// CW1148 - SuppressCodeSmellAnalyzer
					new { id = "CW1148", line = 2, character = 1, file = "CargoWise.Analyzers\\CW1148.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1148", line = 7, character = 3, file = "CargoWise.Analyzers\\CW1148.cs", severity = ReportDiagnostic.Error },

					// CW1149 - SingletonClassesShouldNotHavePublicCtorsAnalyzer
					new { id = "CW1149", line = 14, character = 10, file = "CargoWise.Analyzers\\CW1149.cs", severity = ReportDiagnostic.Error },

					// CW1150 - PublicFieldsAnalyzer
					new { id = "CW1150", line = 6, character = 17, file = "CargoWise.Analyzers\\CW1150.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1150", line = 7, character = 17, file = "CargoWise.Analyzers\\CW1150.cs", severity = ReportDiagnostic.Error },

					// CW1152 - PublicVirtualAnalyzer
					new { id = "CW1152", line = 6, character = 25, file = "CargoWise.Analyzers\\CW1152.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1152", line = 11, character = 25, file = "CargoWise.Analyzers\\CW1152.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1152", line = 15, character = 25, file = "CargoWise.Analyzers\\CW1152.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1152", line = 17, character = 25, file = "CargoWise.Analyzers\\CW1152.cs", severity = ReportDiagnostic.Error },

					// CW1153 - BaseNamingAnalyzer
					new { id = "CW1153", line = 4, character = 8, file = "CargoWise.Analyzers\\CW1153.cs", severity = ReportDiagnostic.Error },

					// CW1154 - PublicInternalClassesInSeperateFilesAnalyzer
					new { id = "CW1154", line = 4, character = 15, file = "CargoWise.Analyzers\\CW1154.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1154", line = 8, character = 17, file = "CargoWise.Analyzers\\CW1154.cs", severity = ReportDiagnostic.Error },

					// CW1155 - NoMultiLineCommentsAnalyzer
					new { id = "CW1155", line = 6, character = 3, file = "CargoWise.Analyzers\\CW1155.cs", severity = ReportDiagnostic.Error },

					// CW1157 - DoNotUseSystemAppDomainAnalyzer
					new { id = "CW1157", line = 10, character = 20, file = "CargoWise.Analyzers\\CW1157.cs", severity = ReportDiagnostic.Error },

					// CW1159 - ResGetStringShouldUseSourceGenMethodAnalyzer
					new { id = "CW1159", line = 11, character = 8, file = "CargoWise.Analyzers\\CW1159.cs", severity = ReportDiagnostic.Error },

					// CW1160 - FullyQualifySqlObjectsAnalyzer
					new { id = "CW1160", line = 8, character = 23, file = "CargoWise.Analyzers\\CW1160.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1160", line = 9, character = 23, file = "CargoWise.Analyzers\\CW1160.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1160", line = 9, character = 39, file = "CargoWise.Analyzers\\CW1160.cs", severity = ReportDiagnostic.Error },

					// CW1161 - ResGetStringAnalyzer
					new { id = "CW1161", line = 8, character = 8, file = "CargoWise.Analyzers\\CW1161.cs", severity = ReportDiagnostic.Error },

					// CW1163 - DoNotInitializeLazyWithMethodInvocationAnalyzer
					new { id = "CW1163", line = 17, character = 35, file = "CargoWise.Analyzers\\CW1163.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1163", line = 20, character = 38, file = "CargoWise.Analyzers\\CW1163.cs", severity = ReportDiagnostic.Error },

					// CW1164 - DoNotUseStringLiteralsForListAttributeAnalyzer
					new { id = "CW1164", line = 8, character = 9, file = "CargoWise.Analyzers\\CW1164.cs", severity = ReportDiagnostic.Error },

					// CW1166 - DoNotExtendNUnit
					new { id = "CW1166", line = 8, character = 22, file = "CargoWise.Analyzers\\CW1166.cs", severity = ReportDiagnostic.Error },

					// CW1167 - DbDisposableActionForDbConnectionAnalyzer
					new { id = "CW1167", line = 14, character = 12, file = "CargoWise.Analyzers\\CW1167.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1167", line = 33, character = 11, file = "CargoWise.Analyzers\\CW1167.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1167", line = 41, character = 11, file = "CargoWise.Analyzers\\CW1167.cs", severity = ReportDiagnostic.Error },

					// CW1168 - NonAbstractTestClassesShouldBeInternalAnalyzer
					new { id = "CW1168", line = 6, character = 2, file = "CargoWise.Analyzers\\CW1168.cs", severity = ReportDiagnostic.Error },

					// CW1169 - DoNotUseOxyPlotAnalyzer
					new { id = "CW1169", line = 2, character = 7, file = "CargoWise.Analyzers\\CW1169.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1169", line = 10, character = 8, file = "CargoWise.Analyzers\\CW1169.cs", severity = ReportDiagnostic.Error },

					// CW1170 - DoNotUseDbGeography
					new { id = "CW1170", line = 10, character = 8, file = "CargoWise.Analyzers\\CW1170.cs", severity = ReportDiagnostic.Error },

					// CW1171 - DoNotUseMimeMessageToStringAnalyzer
					new { id = "CW1171", line = 11, character = 4, file = "CargoWise.Analyzers\\CW1171.cs", severity = ReportDiagnostic.Error },

					// CW1172 - ResGetStringKeyValidation
					new { id = "CW1172", line = 10, character = 22, file = "CargoWise.Analyzers\\CW1172.cs", severity = ReportDiagnostic.Error },

					// CW1173 - DoNotInvokeResUnderscoreGetStringAnalyzer
					new { id = "CW1173", line = 8, character = 8, file = "CargoWise.Analyzers\\CW1173.cs", severity = ReportDiagnostic.Error },

					// CW1174 - ResGetStringMustBeInvokedWithinANamespace
					new { id = "CW1174", line = 9, character = 7, file = "CargoWise.Analyzers\\CW1174.cs", severity = ReportDiagnostic.Error },

					// CW1175 - SystemColumnSqlAnalyzer
					new { id = "CW1175", line = 8, character = 8, file = "CargoWise.Analyzers\\CW1175.cs", severity = ReportDiagnostic.Error },

					// CW1175A - SystemColumnSqlAnalyzer
					new { id = "CW1175A", line = 8, character = 8, file = "CargoWise.Analyzers\\CW1175A.cs", severity = ReportDiagnostic.Error },

					// CW1192 - CustomizableDataCaptionAsmidRequiredAnalyzer
					new { id = "CW1192", line = 8, character = 4, file = "CargoWise.Analyzers\\CW1192.cs", severity = ReportDiagnostic.Error },

					// CW1193 - DoNotUseCargoWiseOneNameInCodeAnalyzer
					new { id = "CW1193", line = 8, character = 8, file = "CargoWise.Analyzers\\CW1193.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1193", line = 11, character = 17, file = "CargoWise.Analyzers\\CW1193.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1193", line = 13, character = 14, file = "CargoWise.Analyzers\\CW1193.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1193", line = 16, character = 6, file = "CargoWise.Analyzers\\CW1193.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1193", line = 19, character = 15, file = "CargoWise.Analyzers\\CW1193.cs", severity = ReportDiagnostic.Error },

					// CW1194 - UnsafeBusinessObjectCollectionCreationAnalyzer
					new { id = "CW1194", line = 13, character = 4, file = "CargoWise.Analyzers\\CW1194.cs", severity = ReportDiagnostic.Error },

					// CW1198 - DoNotAddAuditEventsToStmALogAnalyzer
					new { id = "CW1198", line = 15, character = 35, file = "CargoWise.Analyzers\\CW1198.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1198", line = 17, character = 35, file = "CargoWise.Analyzers\\CW1198.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1198", line = 19, character = 35, file = "CargoWise.Analyzers\\CW1198.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1198", line = 24, character = 24, file = "CargoWise.Analyzers\\CW1198.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1198", line = 26, character = 24, file = "CargoWise.Analyzers\\CW1198.cs", severity = ReportDiagnostic.Error },
					new { id = "CW1198", line = 28, character = 24, file = "CargoWise.Analyzers\\CW1198.cs", severity = ReportDiagnostic.Error },

					//EDI001:Resource String Static Reference Rule
					new { id = "EDI001", line = 7, character = 24, file = "CargoWise.Analyzers\\EDI001.cs", severity = ReportDiagnostic.Error },

					//EDI003:Do Not Exceed BizO Max Length
					new { id = "EDI003", line = 14, character = 17, file = "CargoWise.Analyzers\\EDI003.cs", severity = ReportDiagnostic.Error },
					new { id = "EDI003", line = 17, character = 17, file = "CargoWise.Analyzers\\EDI003.cs", severity = ReportDiagnostic.Error },

					//EDI005:Weak Reference Target Race Condition Rule
					new { id = "EDI005", line = 14, character = 9, file = "CargoWise.Analyzers\\EDI005.cs", severity = ReportDiagnostic.Error },

					//EDI006:RightToLeft Rule
					new { id = "EDI006", line = 10, character = 4, file = "CargoWise.Analyzers\\EDI006.cs", severity = ReportDiagnostic.Error },

					//EDI007:Customizable Data Translation Rule
					new { id = "EDI007", line = 18, character = 15, file = "CargoWise.Analyzers\\EDI007.cs", severity = ReportDiagnostic.Error },

					//EDI008:Log Reference Values In English Only Rule
					new { id = "EDI008", line = 11, character = 18, file = "CargoWise.Analyzers\\EDI008.cs", severity = ReportDiagnostic.Error },

					//EDI009:Service Task Logs should be in English Only
					new { id = "EDI009", line = 10, character = 36, file = "CargoWise.Analyzers\\EDI009.cs", severity = ReportDiagnostic.Error },

					//EDI010:PredefinedNoteType Description Rule
					new { id = "EDI010", line = 12, character = 4, file = "CargoWise.Analyzers\\EDI010.cs", severity = ReportDiagnostic.Error },

					//EDI011:Temp Path Rule
					new { id = "EDI011", line = 10, character = 8, file = "CargoWise.Analyzers\\EDI011.cs", severity = ReportDiagnostic.Error },
					new { id = "EDI011", line = 13, character = 8, file = "CargoWise.Analyzers\\EDI011.cs", severity = ReportDiagnostic.Error },

					//EDI012:Unmaintainable Product Name CSharp
					new { id = "EDI012", line = 6, character = 30, file = "CargoWise.Analyzers\\EDI012.cs", severity = ReportDiagnostic.Error },

					//EDI013:UnmaintainableProductNameXMLAnalyzer
					new { id = "EDI013", line = 3, character = 23, file = "AnalyzersRunner.FunctionalTestingTarget.csproj", severity = ReportDiagnostic.Error },

					////////////////////////////////////////////
					//
					// Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers

					//RS0030:Do not used banned APIs
					new { id = "RS0030", line = 12, character = 8, file = "Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers\\RS0030.cs", severity = ReportDiagnostic.Error },

					//RS0030:Do not used banned APIs
					new { id = "RS0030", line = 15, character = 8, file = "Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers\\RS0030.cs", severity = ReportDiagnostic.Error },

					//RS0030:Do not used banned APIs
					new { id = "RS0030", line = 18, character = 8, file = "Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers\\RS0030.cs", severity = ReportDiagnostic.Error },

					//RS0030:Do not used banned APIs
					new { id = "RS0030", line = 21, character = 4, file = "Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers\\RS0030.cs", severity = ReportDiagnostic.Error },

					////RS0030:Do not used COM related APIs and Attributes
					new { id = "RS0030", line = 18, character = 4, file = "Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers\\BannedComUsage.cs", severity = ReportDiagnostic.Error },

					////RS0030:Do not used COM related APIs and Attributes
					new { id = "RS0030", line = 19, character = 4, file = "Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers\\BannedComUsage.cs", severity = ReportDiagnostic.Error },

					////RS0030:Do not used COM related APIs and Attributes
					new { id = "RS0030", line = 20, character = 4, file = "Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers\\BannedComUsage.cs", severity = ReportDiagnostic.Error },

					////RS0030:Do not used COM related APIs and Attributes
					new { id = "RS0030", line = 21, character = 4, file = "Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers\\BannedComUsage.cs", severity = ReportDiagnostic.Error },

					////RS0030:Do not used COM related APIs and Attributes
					new { id = "RS0030", line = 30, character = 4, file = "Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers\\BannedComUsage.cs", severity = ReportDiagnostic.Error },

					////RS0030:Do not used COM related APIs and Attributes
					new { id = "RS0030", line = 31, character = 4, file = "Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers\\BannedComUsage.cs", severity = ReportDiagnostic.Error },

					////RS0030:Do not used COM related APIs and Attributes
					new { id = "RS0030", line = 32, character = 4, file = "Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers\\BannedComUsage.cs", severity = ReportDiagnostic.Error },

					////RS0030:Do not used COM related APIs and Attributes
					new { id = "RS0030", line = 33, character = 4, file = "Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers\\BannedComUsage.cs", severity = ReportDiagnostic.Error },

					////RS0030:Do not used COM related APIs and Attributes
					new { id = "RS0030", line = 38, character = 4, file = "Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers\\BannedComUsage.cs", severity = ReportDiagnostic.Error },

					////RS0030:Do not used COM related APIs and Attributes
					new { id = "RS0030", line = 47, character = 4, file = "Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers\\BannedComUsage.cs", severity = ReportDiagnostic.Error },

					////RS0030:Do not used COM related APIs and Attributes
					new { id = "RS0030", line = 54, character = 4, file = "Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers\\BannedComUsage.cs", severity = ReportDiagnostic.Error },

					////RS0030:Do not used COM related APIs and Attributes
					new { id = "RS0030", line = 55, character = 4, file = "Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers\\BannedComUsage.cs", severity = ReportDiagnostic.Error },

					////////////////////////////////////////////
					//
					// Microsoft.CodeAnalysis.CSharp.CodeStyle

					//IDE0005: Using directive is unnecessary
					new { id = "IDE0005", line = 2, character = 1, file = "Microsoft.CodeAnalysis.CSharp.CodeStyle\\IDE0005.cs", severity = ReportDiagnostic.Error },

					//IDE0044:Add readonly modifier
					new { id = "IDE0044", line = 9, character = 10, file = "Microsoft.CodeAnalysis.CSharp.CodeStyle\\IDE0044.cs", severity = ReportDiagnostic.Error },

					//IDE0051:Remove unused private members
					new { id = "IDE0051", line = 6, character = 8, file = "Microsoft.CodeAnalysis.CSharp.CodeStyle\\IDE0051.cs", severity = ReportDiagnostic.Error },
					new { id = "IDE0051", line = 10, character = 16, file = "Microsoft.CodeAnalysis.CSharp.CodeStyle\\IDE0051.cs", severity = ReportDiagnostic.Error },

					//IDE0052:Remove unread private members
					new { id = "IDE0052", line = 6, character = 16, file = "Microsoft.CodeAnalysis.CSharp.CodeStyle\\IDE0052.cs", severity = ReportDiagnostic.Error },

					//IDE0054: Use compound assignment
					new { id = "IDE0054", line = 9, character = 6, file = "Microsoft.CodeAnalysis.CSharp.CodeStyle\\IDE0054.cs", severity = ReportDiagnostic.Error },

					//commented out. this is not being caught by AnalyzersRunner yet. The analyzer is only supported in .net core. If we do decide to activate it later, then support will need to be added to AnalyzersRunner
					//new { id = "IDE0070", line = 8, character = 23, file = "Microsoft.CodeAnalysis.CSharp.CodeStyle\\IDE0070.cs", severity = ReportDiagnostic.Error },

					//IDE0100:Remove unnecessary equality operator
					new { id = "IDE0100", line = 11, character = 13, file = "Microsoft.CodeAnalysis.CSharp.CodeStyle\\IDE0100.cs", severity = ReportDiagnostic.Error },

					////////////////////////////////////////////
					//
					// Microsoft.CodeAnalysis.CSharp.NetAnalyzers

					//CA1805:Do not initialize unnecessarily
					new { id = "CA1805", line = 13, character = 15, file = "Microsoft.CodeAnalysis.CSharp.NetAnalyzers\\CA1805.cs", severity = ReportDiagnostic.Error },
					new { id = "CA1805", line = 13, character = 15, file = "Microsoft.CodeAnalysis.CSharp.NetAnalyzers\\CA1805.cs", severity = ReportDiagnostic.Error },
					new { id = "CA1805", line = 16, character = 31, file = "Microsoft.CodeAnalysis.CSharp.NetAnalyzers\\CA1805.cs", severity = ReportDiagnostic.Error },
					new { id = "CA1805", line = 16, character = 31, file = "Microsoft.CodeAnalysis.CSharp.NetAnalyzers\\CA1805.cs", severity = ReportDiagnostic.Error },

					////////////////////////////////////////////
					//
					// Microsoft.CodeAnalysis.NetAnalyzers

					// CA1001:Types that own disposable fields should be disposable
					//this is in twice, the same diagnostic is reported by two analyzer libraries: Microsoft.CodeQuality.Analyzers.dll and Microsoft.CodeAnalysis.NetAnalyzers.dll
					//we should be standardising on Microsoft.CodeAnalysis.NetAnalyzers according to this doc: https://github.com/dotnet/roslyn-analyzers#main-analyzers
					new { id = "CA1001", line = 8, character = 17, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1001.cs", severity = ReportDiagnostic.Error },
					new { id = "CA1001", line = 8, character = 17, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1001.cs", severity = ReportDiagnostic.Error },

					// CA1018: Mark attributes with AttributeUsageAttribute
					//this is in twice, the same diagnostic is reported by two analyzer libraries: Microsoft.CodeQuality.Analyzers.dll and Microsoft.CodeAnalysis.NetAnalyzers.dll
					//we should be standardising on Microsoft.CodeAnalysis.NetAnalyzers according to this doc: https://github.com/dotnet/roslyn-analyzers#main-analyzers
					new { id = "CA1018", line = 6, character = 22, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1018.cs", severity = ReportDiagnostic.Error },
					new { id = "CA1018", line = 6, character = 22, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1018.cs", severity = ReportDiagnostic.Error },
					new { id = "CA1018", line = 10, character = 16, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1018.cs", severity = ReportDiagnostic.Error },
					new { id = "CA1018", line = 10, character = 16, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1018.cs", severity = ReportDiagnostic.Error },

					//CA1028:Enum Storage should be Int32
					//this is in twice, the same diagnostic is reported by two analyzer libraries: Microsoft.CodeQuality.Analyzers.dll and Microsoft.CodeAnalysis.NetAnalyzers.dll
					//we should be standardising on Microsoft.CodeAnalysis.NetAnalyzers according to this doc: https://github.com/dotnet/roslyn-analyzers#main-analyzers
					new { id = "CA1028", line = 4, character = 14, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1028.cs", severity = ReportDiagnostic.Error },
					new { id = "CA1028", line = 4, character = 14, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1028.cs", severity = ReportDiagnostic.Error },

					//CA1052:Static holder types should be Static or NotInheritable
					//this is in twice, the same diagnostic is reported by two analyzer libraries: Microsoft.CodeQuality.Analyzers.dll and Microsoft.CodeAnalysis.NetAnalyzers.dll
					//we should be standardising on Microsoft.CodeAnalysis.NetAnalyzers according to this doc: https://github.com/dotnet/roslyn-analyzers#main-analyzers
					new { id = "CA1052", line = 6, character = 15, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1052.cs", severity = ReportDiagnostic.Error },
					new { id = "CA1052", line = 6, character = 15, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1052.cs", severity = ReportDiagnostic.Error },
					new { id = "CA1052", line = 13, character = 8, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1052.cs", severity = ReportDiagnostic.Error },
					new { id = "CA1052", line = 13, character = 8, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1052.cs", severity = ReportDiagnostic.Error },
					new { id = "CA1052", line = 15, character = 9, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1052.cs", severity = ReportDiagnostic.Error },
					new { id = "CA1052", line = 15, character = 9, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1052.cs", severity = ReportDiagnostic.Error },

					//CA1725:Parameter names should match base declaration
					//this is in twice, the same diagnostic is reported by two analyzer libraries: Microsoft.CodeQuality.Analyzers.dll and Microsoft.CodeAnalysis.NetAnalyzers.dll
					//we should be standardising on Microsoft.CodeAnalysis.NetAnalyzers according to this doc: https://github.com/dotnet/roslyn-analyzers#main-analyzers
					new { id = "CA1725", line = 14, character = 26, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1725.cs", severity = ReportDiagnostic.Error },
					new { id = "CA1725", line = 14, character = 26, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1725.cs", severity = ReportDiagnostic.Error },
					new { id = "CA1725", line = 23, character = 40, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1725.cs", severity = ReportDiagnostic.Error },
					new { id = "CA1725", line = 23, character = 40, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1725.cs", severity = ReportDiagnostic.Error },
					new { id = "CA1725", line = 36, character = 26, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1725.cs", severity = ReportDiagnostic.Error },
					new { id = "CA1725", line = 36, character = 26, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1725.cs", severity = ReportDiagnostic.Error },

					//CA1810:Initialize reference type static fields inline
					new { id = "CA1810", line = 14, character = 10, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1810.cs", severity = ReportDiagnostic.Error },

					//CA1813: Avoid unsealed attributes
					new { id = "CA1813", line = 7, character = 15, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1813.cs", severity = ReportDiagnostic.Error },
					new { id = "CA1813", line = 12, character = 9, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1813.cs", severity = ReportDiagnostic.Error },
					new { id = "CA1813", line = 18, character = 19, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1813.cs", severity = ReportDiagnostic.Error },

					//CA1819:Properties should not return arrays
					//this is in twice, the same diagnostic is reported by two analyzer libraries: Microsoft.CodeQuality.Analyzers.dll and Microsoft.CodeAnalysis.NetAnalyzers.dll
					//we should be standardising on Microsoft.CodeAnalysis.NetAnalyzers according to this doc: https://github.com/dotnet/roslyn-analyzers#main-analyzers
					new { id = "CA1819", line = 11, character = 19, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1819.cs", severity = ReportDiagnostic.Error },
					new { id = "CA1819", line = 11, character = 19, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1819.cs", severity = ReportDiagnostic.Error },
					new { id = "CA1819", line = 20, character = 19, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1819.cs", severity = ReportDiagnostic.Error },
					new { id = "CA1819", line = 20, character = 19, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1819.cs", severity = ReportDiagnostic.Error },
					new { id = "CA1819", line = 25, character = 20, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1819.cs", severity = ReportDiagnostic.Error },
					new { id = "CA1819", line = 25, character = 20, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1819.cs", severity = ReportDiagnostic.Error },

					//CA1820:Test for empty strings using string length
					new { id = "CA1820", line = 13, character = 8, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1820.cs", severity = ReportDiagnostic.Error },

					//commented out, as this analyzer is not currently enabled
					//CA1700:Do not name enum values 'Reserved'
					//new { id = "CA1700", line = 10, character = 3, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1700.cs", severity = ReportDiagnostic.Info },

					// CA1827: Do not use Count()/LongCount() when Any() can be used
					new { id = "CA1827", line = 12, character = 7, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1827.cs", severity = ReportDiagnostic.Error },
					new { id = "CA1827", line = 15, character = 7, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1827.cs", severity = ReportDiagnostic.Error },

					// CA1829: Use Length/Count property instead of Enumerable.Count method
					new { id = "CA1829", line = 12, character = 7, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1829.cs", severity = ReportDiagnostic.Error },
					new { id = "CA1829", line = 15, character = 7, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA1829.cs", severity = ReportDiagnostic.Error },

					//CA2013:Do not use ReferenceEquals with value types
					new { id = "CA2013", line = 8, character = 24, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA2013.cs", severity = ReportDiagnostic.Error },
					new { id = "CA2013", line = 8, character = 27, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA2013.cs", severity = ReportDiagnostic.Error },

					//CA2200:Rethrow to preserve stack details
					new { id = "CA2200", line = 19, character = 5, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA2200.cs", severity = ReportDiagnostic.Error },
					new { id = "CA2200", line = 19, character = 5, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA2200.cs", severity = ReportDiagnostic.Error },

					//CA2208: Instantiate argument exceptions correctly
					new { id = "CA2208", line = 12, character = 10, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA2208.cs", severity = ReportDiagnostic.Error },
					new { id = "CA2208", line = 17, character = 10, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA2208.cs", severity = ReportDiagnostic.Error },
					new { id = "CA2208", line = 22, character = 10, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA2208.cs", severity = ReportDiagnostic.Error },
					new { id = "CA2208", line = 27, character = 10, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA2208.cs", severity = ReportDiagnostic.Error },

					//CA2259:'ThreadStatic' only affects static fields
					new { id = "CA2259", line = 9, character = 17, file = "Microsoft.CodeAnalysis.NetAnalyzers\\CA2259.cs", severity = ReportDiagnostic.Error },

					////////////////////////////////////////////
					//
					// StyleCop.Analyzers

					//SA1210:Using directives should be ordered alphabetically by the namespaces
					new { id = "SA1210", line = 2, character = 1, file = "StyleCop.Analyzers\\SA1210.cs", severity = ReportDiagnostic.Error },
					new { id = "SA1210", line = 2, character = 1, file = "StyleCop.Analyzers\\SuppressedByCategoryButEnabledByDiagnostic\\SA1210.cs", severity = ReportDiagnostic.Error },

					//SA1505:Opening braces should not be followed by blank line
					new { id = "SA1505", line = 3, character = 1, file = "StyleCop.Analyzers\\SA1505.cs", severity = ReportDiagnostic.Error },

					//SA1507:Code should not contain multiple blank lines in a row
					new { id = "SA1507", line = 6, character = 1, file = "StyleCop.Analyzers\\SA1507.cs", severity = ReportDiagnostic.Error },

					//SA1508:Closing braces should not be preceded by blank line
					new { id = "SA1508", line = 5, character = 1, file = "StyleCop.Analyzers\\SA1508.cs", severity = ReportDiagnostic.Error },

					////////////////////////////////////////////
					//
					// WTG.Analyzers

					//WTG1001:Do not use the 'private' keyword
					new { id = "WTG1001", line = 8, character = 3, file = "WTG.Analyzers\\WTG1001.cs", severity = ReportDiagnostic.Error },

					//commented out, as set to INFO only at the moment
					//WTG1003:Do not leave whitespace on the end of the line
					//new { id = "WTG1003", line = 35, character = 1, file = "WTG.Analyzers\\WTG1004.cs", severity = ReportDiagnostic.Info },

					//WTG1004:Indent with tabs rather than spaces
					new { id = "WTG1004", line = 7, character = 1, file = "WTG.Analyzers\\WTG1004.cs", severity = ReportDiagnostic.Error },

					//WTG1007:Do not compare bool to a constant value
					new { id = "WTG1007", line = 9, character = 13, file = "WTG.Analyzers\\WTG1007.cs", severity = ReportDiagnostic.Error },

					//WTG1018: Boolean literals as method arguments should be passed as named arguments
					new { id = "WTG1018", line = 16, character = 11, file = "WTG.Analyzers\\WTG1018.cs", severity = ReportDiagnostic.Error },
					new { id = "WTG1018", line = 16, character = 17, file = "WTG.Analyzers\\WTG1018.cs", severity = ReportDiagnostic.Error },
					new { id = "WTG1018", line = 16, character = 32, file = "WTG.Analyzers\\WTG1018.cs", severity = ReportDiagnostic.Error },

					//WTG3004:Prefer Array.Empty<T>() over creating a new empty array
					new { id = "WTG3004", line = 8, character = 12, file = "WTG.Analyzers\\WTG3004.cs", severity = ReportDiagnostic.Error },
					new { id = "WTG3004", line = 11, character = 12, file = "WTG.Analyzers\\WTG3004.cs", severity = ReportDiagnostic.Error },
					new { id = "WTG3004", line = 14, character = 12, file = "WTG.Analyzers\\WTG3004.cs", severity = ReportDiagnostic.Error },
					new { id = "WTG3004", line = 17, character = 13, file = "WTG.Analyzers\\WTG3004.cs", severity = ReportDiagnostic.Error },
					new { id = "WTG3004", line = 23, character = 12, file = "WTG.Analyzers\\WTG3004.cs", severity = ReportDiagnostic.Error },

					//WTG3005:Don't call ToString() on a string
					new { id = "WTG3005", line = 8, character = 10, file = "WTG.Analyzers\\WTG3005.cs", severity = ReportDiagnostic.Error },

					//WTG3006:Prefer nameof over calling ToString on an enum literal
					new { id = "WTG3006", line = 11, character = 37, file = "WTG.Analyzers\\WTG3006.cs", severity = ReportDiagnostic.Error },
					new { id = "WTG3006", line = 14, character = 41, file = "WTG.Analyzers\\WTG3006.cs", severity = ReportDiagnostic.Error },
				};

				foreach (var expectedIssue in expectedIssues)
				{
					AssertAnalyzerIssue(expectedIssue.id, expectedIssue.line, expectedIssue.character, expectedIssue.file, expectedIssue.severity);
				}

				//assert that we have no issues left
				AssertArrayEqualsByElements("There are unexpected analyzer issues. These need to be manually verified by opening the AnalyzersRunner.FunctionalTestingTarget project in Visual Studio. Maybe new analyzers have been enabled that now need to be asserted? Or maybe these analyzers are actually disabled, but the AnalyzersRunner is running them by mistake?", Array.Empty<string>(), analyzerIssuesRemainingToBeAsserted.ToArray());
			}
		}
	}
}
