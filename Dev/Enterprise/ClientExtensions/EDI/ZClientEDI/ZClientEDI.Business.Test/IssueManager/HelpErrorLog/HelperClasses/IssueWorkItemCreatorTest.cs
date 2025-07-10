using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IssueManager.Business.Tests;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IssueManager.Business.Test;

public class IssueWorkItemCreatorTest : AssignmentTestHelper
{
	DisposableAction deleteTestRecordsAction;
	DisposableAction disposeRegistryValuesAction;

	protected override void SetUp()
	{
		base.SetUp();

		var extractionRegexes = new ExceptionKeyRegexCollection();
		extractionRegexes.Add(new ExceptionKeyRegex { Regex = @"^(at)\s*(?<line>.*\(.*\)).*$", Description = "" });
		var regexRegistryItemDisposable = EDIDataRegistry.Instance.ErrorLogStackLineExtractorRegexes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, extractionRegexes);

		var threshold = new IssueWorkItemCreationThreshold { IssueOccurrenceThreshold = 1, ThresholdTimespan = 28 };
		var creationThresholdRegistryItemDisposable = EDIDataRegistry.Instance.IssueWorkItemCreationThresholdClientVisible
			.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new IssueWorkItemCreationThresholdCollection { threshold });

		var machineLearningEnabledRegistryItemDisposable = EDIDataRegistry.Instance.EnableMachineLearningTeamAssignment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

		InsertTestData();

		disposeRegistryValuesAction = new DisposableAction(DisposeTempRegistryValues);

		void DisposeTempRegistryValues()
		{
			regexRegistryItemDisposable.Dispose();
			creationThresholdRegistryItemDisposable.Dispose();
			machineLearningEnabledRegistryItemDisposable.Dispose();
		}
	}

	protected override void TearDown()
	{
		deleteTestRecordsAction?.Dispose();
		disposeRegistryValuesAction?.Dispose();

		base.TearDown();
	}

	IDisposable SetCreationThreshold(int occurences, int daysTimespan = 28)
	{
		var threshold = new IssueWorkItemCreationThreshold
		{
			IssueOccurrenceThreshold = occurences,
			ThresholdTimespan = daysTimespan
		};
		return EDIDataRegistry.Instance.IssueWorkItemCreationThresholdClientVisible
			.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new IssueWorkItemCreationThresholdCollection { threshold });
	}

	IDisposable SetTestRigIssueRegistryItem(bool value)
	{
		return EDIDataRegistry.Instance.UseTestRigOriginInIssueWorkItemCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
	}

	string GetUniquePath(string path)
	{
		return string.Format(path, Guid.NewGuid(), CultureInfo.InvariantCulture);
	}

	void InsertTestData()
	{
		var errorReportingAssembly = "WTG.ErrorReporting.dll";
		var errorReportingNLogAssembly = "WTG.ErrorReporting.NLog.dll";
		var crikeyMonitorCommonAssembly = "CrikeyMonitor.Common.dll";
		var crikeyMonitorThreadingAssembly = "CrikeyMonitor.Threading.dll";

		// If we provide the assembly here, StackLineWeightsAutoAssigner will add assembly information to matching stacklines
		// And we won't get type and method information because it is only filled for stacklines without assemblies

		AddStackLineCount("",
						  "CrikeyMonitor.Common.GitRepositoryWatcher.OnWatcherRenamedEvent(String oldFullPath, String newFullPath)",
						  16);
		AddStackLineCount("",
						  "WTG.ErrorReporting.EnterpriseErrorReportBuilder.SetRootException(Exception ex)",
						  12);
		AddStackLineCount("",
						  "WTG.ErrorReporting.NLog.ErrorReportingNLogTarget.Write(LogEventInfo logEvent)",
						  13);
		AddStackLineCount("",
						  "CrikeyMonitor.Threading.WorkQueue.WorkerThreadFunc()",
						  2);

		var errorReportingAssemblyPath = GetUniquePath("{0}.com/WTG.ErrorReporting?path=/src/WTG.ErrorReporting");
		var errorReportingNLogAssemblyPath = GetUniquePath("{0}.com/WTG.ErrorReporting?path=/src/WTG.ErrorReporting.NLog");
		var crikeyMonitorCommonAssemblyPath = GetUniquePath("{0}.com/DevTools?path=Main/CrikeyMonitor/CrikeyMonitor.Common");
		var crikeyMonitorThreadingAssemblyPath = GetUniquePath("{0}.com/DevTools?path=Main/CrikeyMonitor/CrikeyMonitor.Threading");

		var errorReportingAssemblyPK = AddPublishedAssembly(errorReportingAssembly, errorReportingAssemblyPath);
		var errorReportingNLogAssemblyPK = AddPublishedAssembly(errorReportingNLogAssembly, errorReportingNLogAssemblyPath);
		var crikeyMonitorCommonAssemblyPK = AddPublishedAssembly(crikeyMonitorCommonAssembly, crikeyMonitorCommonAssemblyPath);
		var crikeyMonitorThreadingAssemblyPK = AddPublishedAssembly(crikeyMonitorThreadingAssembly, crikeyMonitorThreadingAssemblyPath);

		var errorReportBuilderClassPK = AddPublishedClass(errorReportingAssemblyPK, "WTG.ErrorReporting.EnterpriseErrorReportBuilder");
		var errorReportingNLogTargetClassPK = AddPublishedClass(errorReportingNLogAssemblyPK, "WTG.ErrorReporting.NLog.ErrorReportingNLogTarget");
		var crikeyMonitorWorkQueueClassPK = AddPublishedClass(crikeyMonitorThreadingAssemblyPK, "CrikeyMonitor.Threading.WorkQueue");
		var crikeyMonitorGitWatcherClassPK = AddPublishedClass(crikeyMonitorCommonAssemblyPK, "CrikeyMonitor.Common.GitRepositoryWatcher");

		var setRootExceptionPK = AddPublishedMethod(errorReportBuilderClassPK, "SetRootException");
		var writePK = AddPublishedMethod(errorReportingNLogTargetClassPK, "Write");
		var workerThreadFuncPK = AddPublishedMethod(crikeyMonitorWorkQueueClassPK, "WorkerThreadFunc");
		var watcherRenamedEventFuncPK = AddPublishedMethod(crikeyMonitorGitWatcherClassPK, "OnWatcherRenamedEvent");

		AddSourceTreeResponsibility(errorReportingAssemblyPath, "GLW", "NLG", "BLD");
		AddSourceTreeResponsibility(errorReportingNLogAssemblyPath, "IDT", "QAL", "ISM");
		AddSourceTreeResponsibility(crikeyMonitorCommonAssemblyPath, "IDT", "BLD", "CKY");
		AddSourceTreeResponsibility(crikeyMonitorThreadingAssemblyPath, "ABC", "DEF", "GHI");

		deleteTestRecordsAction = new DisposableAction(DeleteTestData);

		void DeleteTestData()
		{
			DeleteSourceTreeResponsibility(errorReportingAssemblyPath);
			DeleteSourceTreeResponsibility(errorReportingNLogAssemblyPath);
			DeleteSourceTreeResponsibility(crikeyMonitorCommonAssemblyPath);
			DeleteSourceTreeResponsibility(crikeyMonitorThreadingAssemblyPath);

			DeletePublishedMethod(setRootExceptionPK);
			DeletePublishedMethod(writePK);
			DeletePublishedMethod(workerThreadFuncPK);
			DeletePublishedMethod(watcherRenamedEventFuncPK);

			DeletePublishedClass(errorReportBuilderClassPK);
			DeletePublishedClass(errorReportingNLogTargetClassPK);
			DeletePublishedClass(crikeyMonitorWorkQueueClassPK);
			DeletePublishedClass(crikeyMonitorGitWatcherClassPK);

			DeletePublishedAssembly(errorReportingAssemblyPK);
			DeletePublishedAssembly(errorReportingNLogAssemblyPK);
			DeletePublishedAssembly(crikeyMonitorCommonAssemblyPK);
			DeletePublishedAssembly(crikeyMonitorThreadingAssemblyPK);
		}
	}

	public void AssertAutoMatchDoneEventReference(bool enableMachineLearningTeamAssignment, EdiHelpErrorLog log, string expectedReference)
	{
		var apiResponse = "{\"status\":\"success\",\"assignment\":{\"product\":\"IDT\",\"productArea\":\"BLD\",\"module\":\"CKY\"}}";
		var fakeUrl = "https://localhost/api/v1/assignment";

		using (EDIDataRegistry.Instance.EnableMachineLearningTeamAssignment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableMachineLearningTeamAssignment))
		using (EDIDataRegistry.Instance.MachineLearningTeamAssignmentApiUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, fakeUrl))
		using (EDIDataRegistry.Instance.MachineLearningTeamAssignmentApiRequestTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
		{
			var issueAssignmentCalculator = new StackLinesWeightsLogAutoAssigner
			{
				MachineLearningTeamAssignmentRetriever = new MachineLearningTeamAssignmentRetriever
				{
					HttpMessageHandler =
						MachineLearningTeamAssignmentTestHelper.GetHttpMessageHandler(HttpStatusCode.OK, apiResponse, null, null)
				}
			};

			IssueWorkItemCreator.LinkOrCreateIssueForWorkItemIfRequired(log, new DataFormatter(), issueAssignmentCalculator, ZDateTime.BrettsBirthday, true);

			Factory.Save();

			var createdWorkItem = Factory.LoadTop1<NewWorkItem>(new ZQuery());
			Factory.EnqueueDelete(typeof(NewWorkItem), createdWorkItem.PK);
			var autoCreatedFromIssueEvent = createdWorkItem.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(e => e.SL_SE_NKEvent == Events.AutoMatchDone.Code);

			AssertNotNull(autoCreatedFromIssueEvent);
			AssertEquals(expectedReference, autoCreatedFromIssueEvent.SL_Reference);
		}
	}

	public void TestIssueWorkItemAutoMatchDoneEventReference()
	{
		var log = CreateLog();
		log.HE_IsClientVisible = true;
		var rawXML = SampleFileRetriever.GetFileContent("SampleCallsForIssueCreation.xml");
		CreateLogOccurrence(log, rawXML, ZDateTime.Today, ZDateTime.Today);
		log.Factory.Save();

		AssertAutoMatchDoneEventReference(false, log, $"Created from issue {log.HE_IssueNumber}, assigned with ABC/DEF/GHI");

		AssertAutoMatchDoneEventReference(true, log, $"Created from issue {log.HE_IssueNumber}, assigned with IDT/BLD/CKY by MLTA service");
	}

	public void TestIssueWorkItemIsCreatedWithDiagnosticDataAboutAssignment()
	{
		// Arrange

		var expectedFrequencyRelevance = 1 - (Math.Log(2 + 1) / Math.Log(16 + 2));
		var expectedPositionRelevance = 6 / 24.0;

		var expectedMostWeightedStackline = "CrikeyMonitor.Threading.WorkQueue.WorkerThreadFunc()";

		var log = CreateLog();
		log.HE_IsClientVisible = true;
		log.HE_ExceptionMessage = "The given key was not present in the dictionary.";

		var rawXML = SampleFileRetriever.GetFileContent("SampleCallsForIssueCreation.xml");
		CreateLogOccurrence(log, rawXML, ZDateTime.Today, ZDateTime.Today);

		var occurrence = Factory.LoadTop1<HelpErrorLogOccurrence>(new ZQuery(HelpErrorLogOccurrenceSchema.HO_HE, log.PK) { OrderBy = HelpErrorLogOccurrenceSchema.Constants.HO_EXEDateTime + " DESC" });

		occurrence.HO_SessionID = new ZGuid("1F082139-8220-4584-9DD0-F9D7A1518C9E");
		occurrence.HO_ExceptionID = "R851766346257880546";
		occurrence.HO_ExceptionDateTime = new DateTime(2024, 6, 5, 6, 57, 4, DateTimeKind.Utc);

		Factory.Save();

		// Act

		IssueWorkItemCreator.LinkOrCreateIssueForWorkItemIfRequired(log, new DataFormatter(), new StackLinesWeightsLogAutoAssigner(), ZDateTime.BrettsBirthday, true);

		Factory.Save();

		// Assert

		var createdWorkItem = Factory.LoadTop1<NewWorkItem>(new ZQuery());
		var eDocs = createdWorkItem.DocManagerInfo.GetRelatedEDocs();

		var eDoc = (StorageDocsBase)eDocs.First(eDoc => eDoc is StorageDocsBase doc &&
									   doc.SC_FileName.Equals("Issue Assignment Log") &&
									   doc.SC_DataType.Equals("TXT"));

		var content = Encoding.ASCII.GetString(eDoc.SC_ImageData);

		var lines = content.SplitByLine().ToArray();

		var maxStackLine =  Array.Find(lines, line => line.Contains(expectedMostWeightedStackline));
		var assignmentLine = Array.Find(lines, line => line.ToLowerInvariant().Contains("assignment:"));

		var assemblyPathsTableIndex = Array.FindIndex(lines, "Table - Assembly Paths".Equals);
		var crikeyMonitorAssemblyPathLine = new ArraySegment<string>(lines, assemblyPathsTableIndex, 16).Single(line => line.Contains("CrikeyMonitor.Common.dll"));

		var teamAssignmentLineIndex = Array.FindIndex(lines, line => line.StartsWith("| Team assignment:"));
		var machineLearningAssignmentLineIndex = Array.FindIndex(lines, line => line.StartsWith("| Machine learning team assignment"));

		CombineAssertions(() =>
		{
			AssertContains(log.PK.ToString(), content);
			AssertContains("The given key was not present in the dictionary.", content);

			AssertContains(occurrence.PK.ToString(), content);
			AssertContains("05-Jun-24 06:57", content);
			AssertContains("R851766346257880546", content);
			AssertContains("1f082139-8220-4584-9dd0-f9d7a1518c9e", content);

			AssertNotSame($"Stackline {expectedMostWeightedStackline} was not found on any line.", maxStackLine, null);
			AssertContains(expectedMostWeightedStackline, maxStackLine);
			AssertContains("CrikeyMonitor.Threading.dll", maxStackLine);
			AssertContains("CrikeyMonitor.Threading.WorkQueue", maxStackLine);
			AssertContains("WorkerThreadFunc", maxStackLine);
			AssertContains(" 2 ", maxStackLine);
			AssertContains(" 6 ", maxStackLine);
			AssertContains($" {expectedFrequencyRelevance:0.###} ", maxStackLine);
			AssertContains($" {expectedPositionRelevance:0.###} ", maxStackLine);

			AssertEquals(2, Regex.Matches(crikeyMonitorAssemblyPathLine, @"[\dA-Za-z-]+\.com/DevTools\?path=Main/CrikeyMonitor/CrikeyMonitor\.Common").Count);

			AssertGreaterThan(machineLearningAssignmentLineIndex, teamAssignmentLineIndex);

			AssertContains("ABC/DEF/GHI", lines[teamAssignmentLineIndex]);

			AssertNotSame("Assignment information should be included.", assignmentLine, null);
			AssertContains("ABC/DEF/GHI", assignmentLine);

			AssertEquals("ABC", createdWorkItem.WKI_WorkItemType);
			AssertEquals("DEF", createdWorkItem.WKI_WorkItemArea);
			AssertEquals("GHI", createdWorkItem.WKI_ActivityType);
		}
		);
	}

	public void TestIssueFromTestRigAttachesToTestRigOriginWI()
	{
		// Arrange
		using var threshold = SetCreationThreshold(10);
		using var value = SetTestRigIssueRegistryItem(true);
		var workItemNumber = "WI00761183";
		var testRigOriginWorkItem = Factory.New<NewWorkItem>();
		testRigOriginWorkItem.WKI_WorkItemNumber = workItemNumber;
		var xml = GetExceptionXmlWithTestRigOrigin(workItemNumber);

		var issue = CreateLog();
		CreateLogOccurrence(issue, xml, ZDateTime.Now, ZDateTime.Now);
		var occurrence = Factory.LoadTop1<HelpErrorLogOccurrence>(new ZQuery(HelpErrorLogOccurrenceSchema.HO_HE, issue.PK)
		{
			OrderBy = HelpErrorLogOccurrenceSchema.Constants.HO_EXEDateTime + " DESC"
		});

		var assigner = new MockAssignmentCalculator(new IssueAssignment("A", "A", "A"));

		// Act
		IssueWorkItemCreator.LinkOrCreateIssueForWorkItemIfRequired(issue, new DataFormatter(), assigner, ZDateTime.Now, occurrence, true);

		// Assert
		var workItems = issue.RelatedWorkItems.Cast<NewWorkItem>().ToList();

		CombineAssertions(() =>
		{
			AssertEquals("Issue should have one work item attached.", 1, workItems.Count);
			AssertEquals("Attached WI should be test rig origin.", workItemNumber, (string)workItems.Single().WKI_WorkItemNumber);
		}
		);
	}

	public void TestIssueFromTestRigCreatesWIWhenRegistryItemIsDisabled()
	{
		// Arrange
		using var threshold = SetCreationThreshold(1);
		using var value = SetTestRigIssueRegistryItem(false);
		var workItemNumber = "WI00761183";
		var testRigOriginWorkItem = Factory.New<NewWorkItem>();
		testRigOriginWorkItem.WKI_WorkItemNumber = workItemNumber;
		var xml = GetExceptionXmlWithTestRigOrigin(workItemNumber);

		var issue = CreateLog();
		CreateLogOccurrence(issue, xml, ZDateTime.Now, ZDateTime.Now);
		var occurrence = Factory.LoadTop1<HelpErrorLogOccurrence>(new ZQuery(HelpErrorLogOccurrenceSchema.HO_HE, issue.PK)
		{
			OrderBy = HelpErrorLogOccurrenceSchema.Constants.HO_EXEDateTime + " DESC"
		});

		var assigner = new MockAssignmentCalculator(new IssueAssignment("HE", "HE", "HA"));

		// Act
		IssueWorkItemCreator.LinkOrCreateIssueForWorkItemIfRequired(issue, new DataFormatter(), assigner, ZDateTime.Now, occurrence, true);

		// Assert
		var workItems = issue.RelatedWorkItems.Cast<NewWorkItem>().ToList();

		AssertEquals("WI should have been created for issue", 1, workItems.Count);

		var createdWorkItem = workItems.Single();

		CombineAssertions(() =>
		{
			AssertNotEquals("Attached WI should not be test rig origin.", workItemNumber, (string)workItems.Single().WKI_WorkItemNumber);
			AssertEquals("HE", createdWorkItem.WKI_WorkItemType);
			AssertEquals("HE", createdWorkItem.WKI_WorkItemArea);
			AssertEquals("HA", createdWorkItem.WKI_ActivityType);
		}
		);
	}

	public void TestNewWorkItemCreatedWhenProcessingProductionOccurrenceFromTestRigIssue()
	{
		// Arrange
		using var threshold = SetCreationThreshold(10);
		using var value = SetTestRigIssueRegistryItem(true);
		var workItemNumber = "WI00761183";
		var testRigOriginWorkItem = Factory.New<NewWorkItem>();
		testRigOriginWorkItem.WKI_WorkItemNumber = workItemNumber;
		var testRigXml = GetExceptionXmlWithTestRigOrigin(workItemNumber);

		var issue = CreateLog();
		CreateLogOccurrence(issue, testRigXml, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(-1));
		issue.HE_LogType = "EXT";

		testRigOriginWorkItem.RelatedItems.Add(issue);

		var productionXml = GetExceptionXml("<Call Assembly=\"Assembly.dll\"> Assembly.Class.Method()</Call>");
		CreateLogOccurrence(issue, productionXml, ZDateTime.Now, ZDateTime.Now);

		var occurrence = Factory.LoadTop1<HelpErrorLogOccurrence>(new ZQuery(HelpErrorLogOccurrenceSchema.HO_HE, issue.PK)
		{
			OrderBy = HelpErrorLogOccurrenceSchema.Constants.HO_EXEDateTime + " DESC"
		});

		var assigner = new MockAssignmentCalculator(new IssueAssignment("A", "B", "C"));

		// Act
		IssueWorkItemCreator.LinkOrCreateIssueForWorkItemIfRequired(issue, new DataFormatter(), assigner, ZDateTime.Now, occurrence, false);

		// Assert
		var workItems = issue.RelatedWorkItems.Cast<NewWorkItem>().ToList();

		var otherWorkItems = workItems.Where(workItem => workItem.WKI_WorkItemNumber != workItemNumber).ToList();

		CombineAssertions(() =>
		{
			AssertEquals("Issue should have two work items", 2, workItems.Count);
			Assert("TestRigOrigin WI should be attached to issue", workItems.Any(workItem => workItem.WKI_WorkItemNumber == workItemNumber));
			AssertEquals("There should be one WI attached to issue apart from TestRigOrigin WI",
				1, otherWorkItems.Count);

			var newWorkItem = otherWorkItems.Single();
			AssertEquals("A", newWorkItem.WKI_WorkItemType);
			AssertEquals("B", newWorkItem.WKI_WorkItemArea);
			AssertEquals("C", newWorkItem.WKI_ActivityType);
		});
	}

	public string GetExceptionXmlWithTestRigOrigin(string testRigOrigin)
	{
		return @$"<EDI_Exception_Report>
	<TestRigOrigin>{testRigOrigin}</TestRigOrigin>
</EDI_Exception_Report>";
	}

	class MockAssignmentCalculator : IIssueAssignmentCalculator
	{
		readonly IssueAssignment _assignment;

		public MockAssignmentCalculator(IssueAssignment assignment)
		{
			_assignment = assignment;
		}

		public IssueAssignment GetAssignment(EdiHelpErrorLog log, DataFormatter formatter)
		{
			return _assignment;
		}
	}
}
