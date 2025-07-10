using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IssueManager.Business.Test
{
	[TestedType(typeof(EdiHelpErrorLog))]
	class HelpErrorLogTest : EnterpriseBusinessObjectTestCase
	{
		public new void TestFetchForLoad()
		{
			var log = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			log.HE_ExceptionMessage = "Blew up lots";

			HelpErrorLogOccurrence occur1 = Log.Occurrences.AddNew();
			occur1.HO_Company = "Some Company";

			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertNotNull(newFactory.Load<EdiHelpErrorLog>(log.PK));
			AssertEquals("Fetch hints should be used", 0, newFactory.ActiveTableFetchHints);
		}

		public void TestExeDate()
		{
			GlbBranch.CurrentBranch.HomePort.TimeZoneSet.StandardZone.R2_OffsetMinutesFromUTC = 480;

			HelpErrorLogOccurrence occurrence1 = Log.Occurrences.AddNew();
			HelpErrorLogOccurrence occurrence2 = Log.Occurrences.AddNew();
			HelpErrorLogOccurrence occurrence3 = Log.Occurrences.AddNew();

			ZDateTime date2 = new ZDateTime(2004, 10, 10, 10, 10, 10, DateTimeKind.Utc);
			ZDateTime date3 = new ZDateTime(2004, 10, 10, 10, 10, 11, DateTimeKind.Utc);
			ZDateTime date1 = new ZDateTime(2004, 10, 10, 10, 10, 9, DateTimeKind.Utc);

			ZDateTime date1Local = new ZDateTime(2004, 10, 10, 18, 10, 9, DateTimeKind.Local);
			ZDateTime date3Local = new ZDateTime(2004, 10, 10, 18, 10, 11, DateTimeKind.Local);

			occurrence1.HO_EXEDateTime = date2;
			occurrence2.HO_EXEDateTime = date3;
			occurrence3.HO_EXEDateTime = date1;

			Assert(Log.HE_LastEXEVersionDate.IsEmpty);
			Assert(Log.HE_LastEXEVersionDateLocal.IsEmpty);
			Assert(Log.HE_FirstEXEVersionDate.IsEmpty);
			Assert(Log.HE_FirstEXEVersionDateLocal.IsEmpty);

			Log.Factory.Save();

			AssertEquals(date3, Log.HE_LastEXEVersionDate);
			AssertEquals(date3Local, Log.HE_LastEXEVersionDateLocal);
			AssertEquals(date1, Log.HE_FirstEXEVersionDate);
			AssertEquals(date1Local, Log.HE_FirstEXEVersionDateLocal);
		}

		public void TestVersionNumber()
		{
			var occurrence1 = Log.Occurrences.AddNew();
			var occurrence2 = Log.Occurrences.AddNew();
			var occurrence3 = Log.Occurrences.AddNew();

			var version1 = "1.1.1.1";
			var version2 = "1.1.2.1";
			var version3 = "1.1.1.2";

			occurrence1.HO_VersionNumber = version1;
			occurrence2.HO_VersionNumber = version2;
			occurrence3.HO_VersionNumber = version3;

			Assert(Log.HE_LastVersionNumber.IsEmpty);
			Assert(Log.HE_FirstVersionNumber.IsEmpty);

			Log.Factory.Save();

			AssertEquals(version2, Log.HE_LastVersionNumber);
			AssertEquals(version1, Log.HE_FirstVersionNumber);
		}

		[TestDate(2014, 10, 28, 14, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestAttachWorkItemSetsFixedDate()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			ObjectFactory.Get<IBMTestHelper>().CreateSystem(Factory, "WKI");

			var workItemOpen = Factory.NewWithValidTestData<NewWorkItem>();
			var task0 = workItemOpen.WorkflowItems.Tasks.AddNew();

			var workItemClosed = Factory.NewWithValidTestData<NewWorkItem>();
			var task1 = workItemClosed.WorkflowItems.AddNew();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			var expectedFixedDate1 = TestDateAttribute.Date;
			var expectedFixedDate2 = TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);

			var workItemCancelled = Factory.NewWithValidTestData<NewWorkItem>();
			var task2 = workItemCancelled.WorkflowItems.AddNew();
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			var log = Factory.New<EdiHelpErrorLog>();
			log.RelatedWorkItems.Add(workItemClosed);
			AssertEquals("Fixed date is added", expectedFixedDate1, log.HE_FixedDate);
			log.RelatedWorkItems.Add(workItemOpen);
			AssertEquals("Fixed date is removed", ZDateTime.Empty, log.HE_FixedDate);

			var log2 = Factory.New<EdiHelpErrorLog>();
			log2.RelatedWorkItems.Add(workItemOpen);
			AssertEquals("Fixed date is blank", ZDateTime.Empty, log2.HE_FixedDate);
			log2.RelatedWorkItems.Add(workItemClosed);
			AssertEquals("Fixed date is still blank", ZDateTime.Empty, log2.HE_FixedDate);

			var log3 = Factory.New<EdiHelpErrorLog>();
			log3.RelatedWorkItems.Add(workItemClosed);
			AssertEquals("Fixed date is added", expectedFixedDate1, log3.HE_FixedDate);
			log3.RelatedWorkItems.Add(workItemCancelled);
			AssertEquals("Fixed date updated", expectedFixedDate2, log3.HE_FixedDate);

			var log4 = Factory.New<EdiHelpErrorLog>();
			log4.RelatedWorkItems.Add(workItemCancelled);
			AssertEquals("Fixed date is added", expectedFixedDate2, log4.HE_FixedDate);
			log4.RelatedWorkItems.Add(workItemClosed);
			AssertEquals("Fixed date unchanged", expectedFixedDate2, log4.HE_FixedDate);
		}

		[TestDate(2014, 10, 28, 14, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestDetachWorkItemSetsFixedDate()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			ObjectFactory.Get<IBMTestHelper>().CreateSystem(Factory, "WKI");

			var workItemClosed = Factory.NewWithValidTestData<NewWorkItem>();
			var workItemCancelled1 = Factory.NewWithValidTestData<NewWorkItem>();
			var task1 = workItemClosed.WorkflowItems.AddNew();
			var task2 = workItemCancelled1.WorkflowItems.AddNew();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			var expectedFixedDate1 = TestDateAttribute.Date;
			var expectedFixedDate2 = TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);

			var workItemCancelled2 = Factory.NewWithValidTestData<NewWorkItem>();
			var task3 = workItemCancelled2.WorkflowItems.AddNew();
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			var log = Factory.New<EdiHelpErrorLog>();
			log.RelatedWorkItems.Add(workItemClosed);
			log.RelatedWorkItems.Add(workItemCancelled1);
			AssertEquals("Precondition", expectedFixedDate1, log.HE_FixedDate);
			log.RelatedWorkItems.Remove(workItemClosed);
			AssertEquals("Fixed date is not removed", expectedFixedDate1, log.HE_FixedDate);
			log.RelatedWorkItems.Remove(workItemCancelled1);
			AssertEquals("Fixed date is removed", ZDateTime.Empty, log.HE_FixedDate);

			var log2 = Factory.New<EdiHelpErrorLog>();
			log2.RelatedWorkItems.Add(workItemClosed);
			log2.RelatedWorkItems.Add(workItemCancelled2);
			AssertEquals("Precondition", expectedFixedDate2, log2.HE_FixedDate);
			log2.RelatedWorkItems.Remove(workItemCancelled2);
			AssertEquals("Fixed date is updated", expectedFixedDate1, log2.HE_FixedDate);
		}

		[TestDate(2014, 10, 28, 14, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestAttachedWorkItemBeingClosedSetsFixedDate()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			ObjectFactory.Get<IBMTestHelper>().CreateSystem(Factory, "WKI");

			var workItemClosed = Factory.NewWithValidTestData<NewWorkItem>();
			var workItemCancelled = Factory.NewWithValidTestData<NewWorkItem>();
			var task1 = workItemClosed.WorkflowItems.AddNew();
			var task2 = workItemCancelled.WorkflowItems.AddNew();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			var log1 = Factory.New<EdiHelpErrorLog>();
			log1.RelatedWorkItems.Add(workItemClosed);
			log1.RelatedWorkItems.Add(workItemCancelled);
			AssertEquals("Precondition: Fixed date is set because all related work items are closed/cancelled", TestDateAttribute.Date, log1.HE_FixedDate);

			var workItemToBeClosed1 = Factory.NewWithValidTestData<NewWorkItem>();
			log1.RelatedWorkItems.Add(workItemToBeClosed1);
			var expectedFixedDate1 = TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			var task3 = workItemToBeClosed1.WorkflowItems.AddNew();
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Fixed date is updated because attached WI, which got closed, will have a later closed date", expectedFixedDate1, log1.HE_FixedDate);

			var workItemToBeClosed2 = Factory.NewWithValidTestData<NewWorkItem>();
			var log2 = Factory.New<EdiHelpErrorLog>();
			var expectedFixedDate2 = TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			log2.RelatedWorkItems.Add(workItemToBeClosed2);
			var task4 = workItemToBeClosed2.WorkflowItems.AddNew();
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Fixed date is set", expectedFixedDate2, log2.HE_FixedDate);

			workItemToBeClosed2.WorkflowItems.AddNew();
			AssertEquals("Fixed date is removed", ZDateTime.Empty, log2.HE_FixedDate);
		}

		[TestDate(2015, 7, 14)]
		public void TestAttachedWorkItemBeingClosedWhileAnotherWorkItemRemainsOpenDoesNotSetFixedDate()
		{
			var issue = Factory.New<EdiHelpErrorLog>();
			var workItem1 = Factory.New<NewWorkItem>();
			var workItem2 = Factory.New<NewWorkItem>();
			var workItem3 = Factory.New<NewWorkItem>();
			var task1 = workItem1.WorkflowItems.Tasks.AddNew();
			var task2 = workItem2.WorkflowItems.Tasks.AddNew();
			var task3 = workItem3.WorkflowItems.Tasks.AddNew();

			issue.RelatedWorkItems.Add(workItem1);
			issue.RelatedWorkItems.Add(workItem2);
			issue.RelatedWorkItems.Add(workItem3);

			AssertEquals(ZDateTime.Empty, issue.HE_FixedDate);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(ZDateTime.Empty, issue.HE_FixedDate);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals(ZDateTime.Empty, issue.HE_FixedDate);

			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(ZDateTime.UtcNow, issue.HE_FixedDate);
		}

		[TestDate(2021, 3, 29, 14, 19, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestAttachedWorkItemBeingClosedDoesNotUsePreviousJobCloseEventTime()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			ObjectFactory.Get<IBMTestHelper>().CreateSystem(Factory, "WKI");

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var task = workItem.WorkflowItems.AddNew();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals(ZDateTime.UtcNow, workItem.JobCloseDateUtc);
			task = workItem.WorkflowItems.AddNew();
			Log.RelatedWorkItems.Add(workItem);
			Factory.Save();

			TestDateAttribute.AddMonths(1);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals(ZDateTime.UtcNow, Log.HE_FixedDate);
		}

		[TestDate(2021, 4, 29, 14, 19, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestAttachedWorkItemBeingClosedLogsJobClosedEvent()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			ObjectFactory.Get<IBMTestHelper>().CreateSystem(Factory, "WKI");

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			Log.RelatedWorkItems.Add(workItem);
			var task = workItem.WorkflowItems.AddNew();
			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals(ZDateTime.UtcNow, Log.HE_FixedDate);
			var incidentClosedEvent = Log.Logs.MostRecentLogByEventTime(Events.IncidentClosed);
			AssertNotNull(incidentClosedEvent);
			AssertEquals(workItem.WKI_WorkItemNumber + " closed " + ZDateTime.UtcNow.ToShortDateString(), incidentClosedEvent.SL_Reference);
		}

		[TestDate(2021, 4, 29, 14, 19, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestAttachingClosedWorkItemLogsJobClosedEvent()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			ObjectFactory.Get<IBMTestHelper>().CreateSystem(Factory, "WKI");

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var task = workItem.WorkflowItems.AddNew();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			Log.RelatedWorkItems.Add(workItem);
			Factory.Save();

			AssertEquals(ZDateTime.UtcNow, Log.HE_FixedDate);
			var incidentClosedEvent = Log.Logs.MostRecentLogByEventTime(Events.IncidentClosed);
			AssertNotNull(incidentClosedEvent);
			AssertEquals(workItem.WKI_WorkItemNumber + " closed " + ZDateTime.UtcNow.ToShortDateString(), incidentClosedEvent.SL_Reference);
		}

		[TestDate(2021, 4, 29, 14, 19, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestReopeningWorkItemLogsJobReOpenedEvent()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			ObjectFactory.Get<IBMTestHelper>().CreateSystem(Factory, "WKI");

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var task = workItem.WorkflowItems.AddNew();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Log.RelatedWorkItems.Add(workItem);
			Factory.Save();

			task = workItem.WorkflowItems.AddNew();
			Factory.Save();

			AssertEquals(ZDateTime.Empty, Log.HE_FixedDate);
			var incidentReopenedEvent = Log.Logs.MostRecentLogByEventTime(Events.IncidentReopened);
			AssertNotNull(incidentReopenedEvent);
			AssertEquals(workItem.WKI_WorkItemNumber, incidentReopenedEvent.SL_Reference);
		}

		public void TestIsClosedOrCancelled()
		{
			IWorkTaskRelatedItem item = Log;
			Log.HE_FixedDate = ZDateTime.Empty;
			Assert(!item.IsClosedOrCancelled);

			Log.HE_FixedDate = ZDateTime.Invalid;
			Assert(!item.IsClosedOrCancelled);

			Log.HE_FixedDate = ZDateTime.UtcNow;
			Assert(item.IsClosedOrCancelled);
		}

		public void TestRelatedWorkItems()
		{
			NewWorkItem wI1 = Log.RelatedWorkItems.AddNew();
			NewWorkItem wI2 = Factory.New<NewWorkItem>();
			NewWorkItem wI3 = Log.RelatedWorkItems.AddNew();
			Log.RelatedWorkItems.Add(wI2);

			AssertEquals("Log.RelatedWorkItems.Count", 3, Log.RelatedWorkItems.Count);
			AssertEquals(true, Log.HasWorkItems);
			Assert("Log.RelatedWorkItems should contain WI1.", Log.RelatedWorkItems.Contains(wI1));
			Assert("Log.RelatedWorkItems should contain WI2.", Log.RelatedWorkItems.Contains(wI2));
			Assert("Log.RelatedWorkItems should contain WI3.", Log.RelatedWorkItems.Contains(wI3));

			Log.RelatedWorkItems.Remove(wI1);
			AssertEquals("Log.RelatedWorkItems.Count", 2, Log.RelatedWorkItems.Count);
			Assert("Log.RelatedWorkItems shouldn't contain WI1.", !Log.RelatedWorkItems.Contains(wI1));

			Log.RelatedWorkItems.Remove(wI2);
			AssertEquals("Log.RelatedWorkItems.Count", 1, Log.RelatedWorkItems.Count);
			Assert("Log.RelatedWorkItems shouldn't contain WI2.", !Log.RelatedWorkItems.Contains(wI2));

			Log.RelatedWorkItems.RemoveAll();
			AssertEquals(false, Log.HasWorkItems);
		}

		public void TestAutoClose()
		{
			ZDateTime now = ZDateTime.UtcNow;

			Log.AutoClose(now);
			AssertEquals(now, Log.HE_FixedDate);
			AssertNotNull(Log.Logs.MostRecentLogByEventTime(Events.IncidentClosed));

			Factory.Save();
			AssertEquals(new ZByte(0), Log.HE_FixedCount);
		}

		public void TestAutoReOpen()
		{
			Log.HE_FixedDate = ZDateTime.UtcNow;
			Log.AutoReOpen();
			Assert(Log.HE_FixedDate.IsEmpty);
			AssertNotNull(Log.Logs.MostRecentLogByEventTime(Events.IncidentReopened));
		}

		public void TestFixedCount()
		{
			AssertEquals(new ZByte(0), Log.HE_FixedCount);
			Log.HE_FixedDate = ZDateTime.UtcNow;

			Factory.Save();
			AssertEquals(new ZByte(1), Log.HE_FixedCount);

			Log.HE_FixedDate = ZDateTime.Empty;
			Factory.Save();
			AssertEquals(new ZByte(1), Log.HE_FixedCount);

			Log.HE_FixedDate = ZDateTime.UtcNow;
			Factory.Save();
			AssertEquals(new ZByte(2), Log.HE_FixedCount);

			Log.HE_FixedDate = ZDateTime.UtcNow.AddMinutes(1);
			Factory.Save();
			AssertEquals(new ZByte(3), Log.HE_FixedCount);
		}

		public void TestUserEnteredBodyTextForAssignToEmail()
		{
			Log.UserEnteredBodyTextForAssignToEmail = "TEST text";
			AssertEquals("TEST text", Log.UserEnteredBodyTextForAssignToEmail);
			Log.UserEnteredBodyTextForAssignToEmail = "";
			AssertEquals("", Log.UserEnteredBodyTextForAssignToEmail);
		}

		[TestDate(2010, 1, 1)]
		public void TestLogDates()
		{
			GlbBranch.CurrentBranch.HomePort.TimeZoneSet.StandardZone.R2_OffsetMinutesFromUTC = 480;

			HelpErrorLogOccurrence occurrence1 = Log.Occurrences.AddNew();
			HelpErrorLogOccurrence occurrence2 = Log.Occurrences.AddNew();
			HelpErrorLogOccurrence occurrence3 = Log.Occurrences.AddNew();

			ZDateTime date2 = new ZDateTime(2004, 10, 10, 10, 10, 10, DateTimeKind.Utc);
			ZDateTime date3 = new ZDateTime(2004, 10, 10, 10, 10, 11, DateTimeKind.Utc);
			ZDateTime date1 = new ZDateTime(2004, 10, 10, 10, 10, 9, DateTimeKind.Utc);

			ZDateTime date1Local = new ZDateTime(2004, 10, 10, 18, 10, 9, DateTimeKind.Local);
			ZDateTime date3Local = new ZDateTime(2004, 10, 10, 18, 10, 11, DateTimeKind.Local);

			occurrence1.HO_ExceptionDateTime = date2;
			occurrence2.HO_ExceptionDateTime = date3;
			occurrence3.HO_ExceptionDateTime = date1;

			Assert(Log.HE_FirstProcessed.IsEmpty);
			Assert(Log.HE_FirstReported.IsEmpty);
			Assert(Log.HE_FirstProcessedLocal.IsEmpty);
			Assert(Log.HE_LastReported.IsEmpty);
			Assert(Log.HE_LastReportedLocal.IsEmpty);

			Log.Factory.Save();

			AssertEquals(ZDateTime.Now, Log.HE_FirstProcessed);
			AssertEquals(date1, Log.HE_FirstReported);
			AssertEquals(date1Local, Log.HE_FirstReportedLocal);
			AssertEquals(date3, Log.HE_LastReported);
			AssertEquals(date3Local, Log.HE_LastReportedLocal);
		}

		public void TestIssueNumber()
		{
			Assert("IssueNumber is empty when not saved.", Log.HE_IssueNumber.IsEmpty);
			Factory.Save();
			Assert("IssueNumber is not empty when saved.", !Log.HE_IssueNumber.IsEmpty);
		}

		public void TestOccurrences()
		{
			AssertEquals("Initially", 0, Log.Occurrences.Count);

			Log.Occurrences.AddNew();
			AssertEquals("After setting up", 1, Log.Occurrences.Count);
		}

		public void TestOccurrencesText()
		{
			Log.Keys.AddNew();
			AssertEquals(Log.Occurrences.ToString() + " / 1 Key", Log.OccurrencesText);
			Log.Keys.AddNew();
			AssertEquals(Log.Occurrences.ToString() + " / 2 Keys", Log.OccurrencesText);
		}

		public void TestKeyCount()
		{
			AssertEquals(0, Log.KeyCount);
			Log.Keys.AddNew();
			AssertEquals(1, Log.KeyCount);
			Log.Keys.AddNew();
			AssertEquals(2, Log.KeyCount);
		}

		public void TestOccurrencesTextInfo()
		{
			AssertNotNull(Log.OccurrencesText, Log.OccurrencesTextInfo.Value);
		}

		public void TestFirstKey_FirstKeyString_FirstKeyHashCode()
		{
			AssertNull(Log.FirstKey);
			AssertEquals("", Log.FirstKeyString);
			AssertEquals(0, Log.FirstKeyHashCode);

			HelpErrorLogKey key1 = Log.Keys.AddNew();
			key1.HK_Key = "KEY123";
			key1.HK_HashCode = HelpErrorLogKey.GetKeyHashCode("KEY123");

			AssertEquals(key1, Log.FirstKey);
			AssertEquals(key1.HK_Key, Log.FirstKeyString);
			AssertEquals(key1.HK_HashCode, Log.FirstKeyHashCode);

			HelpErrorLogKey key2 = Log.Keys.AddNew();
			key2.HK_Key = "123KEY";
			key2.HK_HashCode = HelpErrorLogKey.GetKeyHashCode("123KEY");

			AssertEquals(key1, Log.FirstKey);
			AssertEquals(key1.HK_Key, Log.FirstKeyString);
			AssertEquals(key1.HK_HashCode, Log.FirstKeyHashCode);
		}

		public void TestHumanReadableShortcutName()
		{
			Log.HE_IssueNumber = "12345678";
			AssertEquals("Issue 12345678", Log.HumanReadableShortcutName);
			Log.HE_ExceptionMessage = "The cat got into the codebase :(";
			AssertEquals("Issue 12345678 - The cat got into the codebase :(", Log.HumanReadableShortcutName);
		}

		public void TestToString()
		{
			AssertEquals("Intially", "0 Occurrences / 0 Keys", Log.ToString());

			Log.HE_ExceptionMessage = "Hi";
			Log.Occurrences.AddNew();
			Log.Keys.AddNew();
			AssertEquals("After setting up", "Hi, 1 Occurrence / 1 Key", Log.ToString());
		}

		#region Create Incidents

		public void TestCreateIncident()
		{
			HelpErrorLogOccurrence occurrence = CreateOccurrenceWithLicence(Log);
			Factory.Save();

			NewWorkItem workItem = Log.RelatedWorkItems.AddNew();
			Log.CreateIncident(occurrence);

			AssertEquals("HasAutomaticallyCreatedIncidents", false, log.HasAutomaticallyCreatedIncidents);
			AssertEquals("RelatedIncidents.Count", 1, Log.RelatedIncidents.Count);
			AssertEquals("RelatedIncidents[0].IsInDatabase", false, Log.RelatedIncidents[0].IsInDatabase);
			AssertEquals("RelatedIncidents[0].IM_LCC", occurrence.HO_LCC, Log.RelatedIncidents[0].ClientCompany.PK);

			Log.CreateIncident(occurrence);
			AssertEquals("RelatedIncidents.Count", 1, Log.RelatedIncidents.Count);
		}

		public void TestCreateIncidents()
		{
			AssertEquals("HasAutomaticallyCreatedIncidents", false, Log.HasAutomaticallyCreatedIncidents);
			PrepareDataForTestCreateIncidents();

			AssertEquals("RelatedIncidents.Count", 1, Log.RelatedIncidents.Count);
			SupportIncident[] createdIncidents = Log.CreateIncidents(true);
			Log.RelatedIncidents.Sort(IncidentMainSchema.Constants.IM_IncidentNumber);

			AssertEquals("HasAutomaticallyCreatedIncidents", true, log.HasAutomaticallyCreatedIncidents);
			AssertEquals("CreateIncidents().Length", 2, createdIncidents.Length);
			AssertEquals("RelatedIncidents.Count", 3, Log.RelatedIncidents.Count);

			AssertEquals("RelatedIncidents should be sorted by IM_IncidentNumber.", 1, Log.RelatedIncidents[1].IM_IncidentNumber.CompareTo(Log.RelatedIncidents[0].IM_IncidentNumber));
			AssertEquals("RelatedIncidents should be sorted by IM_IncidentNumber.", 1, Log.RelatedIncidents[2].IM_IncidentNumber.CompareTo(Log.RelatedIncidents[1].IM_IncidentNumber));

			AssertEquals("RelatedIncidents.Contains(CreatedIncidents()[0])", true, Log.RelatedIncidents.Contains(createdIncidents[0]));
			AssertEquals("RelatedIncidents.Contains(CreatedIncidents()[1])", true, Log.RelatedIncidents.Contains(createdIncidents[1]));

			AssertEquals("RelatedIncidents[0].RelatedWorkItems.Count", 1, Log.RelatedIncidents[0].RelatedWorkItems.Count);
			AssertEquals("RelatedIncidents[1].RelatedWorkItems.Count", 2, Log.RelatedIncidents[1].RelatedWorkItems.Count);
			AssertEquals("RelatedIncidents[2].RelatedWorkItems.Count", 2, Log.RelatedIncidents[2].RelatedWorkItems.Count);

			AssertEquals("RelatedIncidents[0].IM_LA", GetLicenceHeader("CO1C01DB1").ClientCompany.PK, Log.RelatedIncidents[0].IM_LCC);
			AssertNotNull("Incident for Licence2 should be created.", Log.RelatedIncidents.Find(new ZQuery(IncidentMainSchema.IM_LA, GetLicenceHeader("CO2C02DB2").PK)));
			AssertNotNull("Incident for Licence3 should be created.", Log.RelatedIncidents.Find(new ZQuery(IncidentMainSchema.IM_LA, GetLicenceHeader("CO2C02DB3").PK)));
			AssertNotNull("Incident for Licence4 should be created.", Log.RelatedIncidents.Find(new ZQuery(IncidentMainSchema.IM_LA, GetLicenceHeader("CO3C03DB4").PK)));

			ZQuery logFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AutoMatchDone.Code);
			StmALog[] logs = Log.Logs.Find(logFilter);
			AssertEquals("Logs.Find().Length", 1, logs.Length);
			AssertEquals("Logs.Find()[0].SL_Reference", "Automatically created incidents", logs[0].SL_Reference);

			Log.CreateIncidents(true);
			AssertEquals("Logs.Find().Length", 1, Log.Logs.Find(logFilter).Length);
		}

		public void TestHasAutomaticallyCreatedIncidents()
		{
			AssertEquals("HasAutomaticallyCreatedIncidents should be false at first", false, Log.HasAutomaticallyCreatedIncidents);
			PrepareDataForTestCreateIncidents();

			AssertEquals("RelatedIncidents.Count", 1, Log.RelatedIncidents.Count);
			SupportIncident[] createdIncidents = Log.CreateIncidents(true);
			AssertEquals("HasAutomaticallyCreatedIncidents should be true after CreateIncidents", true, Log.HasAutomaticallyCreatedIncidents);

			var logFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AutoMatchDone.Code);
			var log = Log.Logs.Find(logFilter).FirstOrDefault();
			log.IsCancelled = true;
			Log.Factory.Save();

			AssertEquals("HasAutomaticallyCreatedIncidents should be false if log is canceled", false, Log.HasAutomaticallyCreatedIncidents);
		}

		public void TestCreateIncidentsCore()
		{
			HelpErrorLogOccurrence[] occurrenceList = PrepareDataForTestCreateIncidentsCore();
			AssertEquals("RelatedIncidents.Count", 2, Log.RelatedIncidents.Count);
			AssertEquals("RelatedIncidents[0].RelatedItems.Count", 0, Log.RelatedIncidents[0].RelatedItems.Count);
			AssertEquals("RelatedIncidents[1].RelatedItems.Count", 0, Log.RelatedIncidents[1].RelatedItems.Count);
			AssertEquals("RelatedWorkItems.Count", 2, Log.RelatedWorkItems.Count);
			AssertEquals("RelatedWorkItems[0].RelatedItems.Count", 1, Log.RelatedWorkItems[0].RelatedItems.Count);
			AssertEquals("RelatedWorkItems[1].RelatedItems.Count", 1, Log.RelatedWorkItems[1].RelatedItems.Count);

			var wkiAndImQuery = new ZQuery()
				.AddToFilter(GenPivotSchema.XX_RelationType, "WRK")
				.AddToFilter(GenPivotSchema.XX_Relation1TableCode, "WKI")
				.AddToFilter(GenPivotSchema.XX_Relation2TableCode, "IM");
			AssertEquals("0 data about \"XX_RelationType = 'WRK' AND XX_Relation1TableCode = 'WKI' AND XX_Relation2TableCode = 'IM'\"", 0, Factory.Load<GenPivot>(wkiAndImQuery).Length);
			var imAndWkiQuery = new ZQuery()
				.AddToFilter(GenPivotSchema.XX_RelationType, "WRK")
				.AddToFilter(GenPivotSchema.XX_Relation1TableCode, "IM")
				.AddToFilter(GenPivotSchema.XX_Relation2TableCode, "WKI");
			AssertEquals("0 data about \"XX_RelationType = 'WRK' AND XX_Relation1TableCode = 'IM' AND XX_Relation2TableCode = 'WKI'\"", 0, Factory.Load<GenPivot>(imAndWkiQuery).Length);

			MethodInfo methodCreateIncidentsCore = FindMethodCreateIncidentsCore();
			AssertNotNull("methodCreateIncidentsCore", methodCreateIncidentsCore);
			SupportIncident[] supportIncidentList = (SupportIncident[])methodCreateIncidentsCore.Invoke(Log, new object[] { occurrenceList });
			AssertEquals("RelatedIncidents.Count", 4, Log.RelatedIncidents.Count);
			AssertEquals("RelatedIncidents[0].RelatedItems.Count", 0, Log.RelatedIncidents[0].RelatedItems.Count);
			AssertEquals("RelatedIncidents[1].RelatedItems.Count", 0, Log.RelatedIncidents[1].RelatedItems.Count);
			AssertEquals("RelatedIncidents[1].RelatedItems.Count", 2, Log.RelatedIncidents[2].RelatedItems.Count);
			AssertEquals("RelatedIncidents[1].RelatedItems.Count", 2, Log.RelatedIncidents[3].RelatedItems.Count);
			AssertEquals("RelatedWorkItems.Count", 2, Log.RelatedWorkItems.Count);
			AssertEquals("RelatedWorkItems[0].RelatedItems.Count", 3, Log.RelatedWorkItems[0].RelatedItems.Count);
			AssertEquals("RelatedWorkItems[1].RelatedItems.Count", 3, Log.RelatedWorkItems[1].RelatedItems.Count);

			AssertEquals("supportIncidentList.Count", 2, supportIncidentList.Length);
			AssertEquals("supportIncidentList[1].RelatedItems.Count", 2, supportIncidentList[0].RelatedItems.Count);
			AssertEquals("supportIncidentList[1].RelatedItems.Count", 2, supportIncidentList[1].RelatedItems.Count);

			AssertEquals("still 0 data about \"XX_RelationType = 'WRK' AND XX_Relation1TableCode = 'WKI' AND XX_Relation2TableCode = 'IM'\"", 0, Factory.Load<GenPivot>(wkiAndImQuery).Length);
			AssertEquals("4 data about \"XX_RelationType = 'WRK' AND XX_Relation1TableCode = 'IM' AND XX_Relation2TableCode = 'WKI'\"", 4, Factory.Load<GenPivot>(imAndWkiQuery).Length);
		}

		MethodInfo FindMethodCreateIncidentsCore()
		{
			Type type = typeof(EdiHelpErrorLog);
			MethodInfo[] methodInfos = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Instance);
			foreach (MethodInfo methodInfo in methodInfos)
			{
				if ("CreateIncidentsCore".Equals(methodInfo.Name))
				{
					return methodInfo;
				}
			}
			return null;
		}

		HelpErrorLogOccurrence[] PrepareDataForTestCreateIncidentsCore()
		{
			EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.CreateAndLoadLicenceForOrg();
			org2.CreateAndLoadLicenceForOrg();
			org1.LicenceEnterpriseCode = "CO1";
			org1.LicCompany.LC_CompanyCode = "C01";
			org2.LicenceEnterpriseCode = "CO2";
			org2.LicCompany.LC_CompanyCode = "C02";
			LicenceDatabase db1 = org1.LicCompany.LicDatabases.AddNew();
			db1.LD_ServerCode = "DB1";
			LicenceDatabase db2 = org2.LicCompany.LicDatabases.AddNew();
			db2.LD_ServerCode = "DB2";
			var licence1 = org1.LicCompany.GetHeader(db1);
			var licence2 = org2.LicCompany.GetHeader(db2);
			ClientCompany clientCompany1 = Factory.New<ClientCompany>();
			clientCompany1.LCC_Code = "AAA";
			clientCompany1.LCC_LD = db1.PK;
			clientCompany1.LCC_OH = org1.PK;
			ClientCompany clientCompany2 = Factory.New<ClientCompany>();
			clientCompany2.LCC_Code = "BBB";
			clientCompany2.LCC_LD = db2.PK;
			clientCompany2.LCC_OH = org2.PK;

			Log.RelatedWorkItems.Add(Factory.New<NewWorkItem>());
			Log.RelatedWorkItems.Add(Factory.New<NewWorkItem>());

			SupportIncident incident1 = Factory.New<SupportIncident>();
			incident1.IM_LCC = licence1.ClientCompany.PK;
			Log.RelatedIncidents.Add(incident1);
			SupportIncident incident2 = Factory.New<SupportIncident>();
			Log.RelatedIncidents.Add(incident2);
			incident2.IM_LCC = licence2.ClientCompany.PK;

			HelpErrorLogOccurrence relatedIncidentClientCompanyPKsContains = Log.Occurrences.AddNew();
			relatedIncidentClientCompanyPKsContains.HO_LCC = licence1.ClientCompany.PK;

			EDIOrgHeader org3 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org3.CreateAndLoadLicenceForOrg();
			org3.LicenceEnterpriseCode = "CO3";
			org3.LicCompany.LC_CompanyCode = "C03";
			LicenceDatabase db3 = org3.LicCompany.LicDatabases.AddNew();
			db3.LD_ServerCode = "DB3";
			var licence3 = org3.LicCompany.GetHeader(db3);
			EDIOrgHeader org4 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org4.CreateAndLoadLicenceForOrg();
			org4.LicenceEnterpriseCode = "CO4";
			org4.LicCompany.LC_CompanyCode = "C04";
			LicenceDatabase db4 = org4.LicCompany.LicDatabases.AddNew();
			db3.LD_ServerCode = "DB4";
			var licence4 = org4.LicCompany.GetHeader(db4);
			ClientCompany clientCompany3 = Factory.New<ClientCompany>();
			clientCompany3.LCC_Code = "CCC";
			clientCompany3.LCC_LD = db3.PK;
			clientCompany3.LCC_OH = org3.PK;
			ClientCompany clientCompany4 = Factory.New<ClientCompany>();
			clientCompany4.LCC_Code = "DDD";
			clientCompany4.LCC_LD = db4.PK;
			clientCompany4.LCC_OH = org4.PK;

			HelpErrorLogOccurrence relatedIncidentClientCompanyPKsNotContains1 = Log.Occurrences.AddNew();
			relatedIncidentClientCompanyPKsNotContains1.HO_LCC = licence3.ClientCompany.PK;
			relatedIncidentClientCompanyPKsNotContains1.HO_LD = licence3.LA_LD;
			HelpErrorLogOccurrence relatedIncidentClientCompanyPKsNotContains2 = Log.Occurrences.AddNew();
			relatedIncidentClientCompanyPKsNotContains2.HO_LCC = licence4.ClientCompany.PK;
			relatedIncidentClientCompanyPKsNotContains2.HO_LD = licence4.LA_LD;

			return new HelpErrorLogOccurrence[] { relatedIncidentClientCompanyPKsContains, relatedIncidentClientCompanyPKsNotContains1, relatedIncidentClientCompanyPKsNotContains2 };
		}

		LicenceHeader GetLicenceHeader(string code)
		{
			return LicenceHeader.LoadFromLicenceCode(Factory, code);
		}

		void PrepareDataForTestCreateIncidents()
		{
			EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader inactiveOrg = Factory.NewWithValidTestData<EDIOrgHeader>();
			inactiveOrg.OH_IsActive = false;

			org1.CreateAndLoadLicenceForOrg();
			org2.CreateAndLoadLicenceForOrg();
			inactiveOrg.CreateAndLoadLicenceForOrg();

			org1.LicenceEnterpriseCode = "CO1";
			org1.LicCompany.LC_CompanyCode = "C01";
			org2.LicenceEnterpriseCode = "CO2";
			org2.LicCompany.LC_CompanyCode = "C02";
			inactiveOrg.LicenceEnterpriseCode = "CO3";
			inactiveOrg.LicCompany.LC_CompanyCode = "C03";

			LicenceDatabase db1 = org1.LicCompany.LicDatabases.AddNew();
			db1.LD_ServerCode = "DB1";
			LicenceDatabase db2 = org2.LicCompany.LicDatabases.AddNew();
			db2.LD_ServerCode = "DB2";
			LicenceDatabase db3 = org2.LicCompany.LicDatabases.AddNew();
			db3.LD_ServerCode = "DB3";
			LicenceDatabase db4 = inactiveOrg.LicCompany.LicDatabases.AddNew();
			db4.LD_ServerCode = "DB4";

			var licence1 = org1.LicCompany.GetHeader(db1);
			var licence2 = org2.LicCompany.GetHeader(db2);
			var licence3 = org2.LicCompany.GetHeader(db3);
			var licence4 = inactiveOrg.LicCompany.GetHeader(db4);

			ClientCompany clientCompany1 = Factory.New<ClientCompany>();
			clientCompany1.LCC_Code = "AAA";
			clientCompany1.LCC_LD = db1.PK;
			clientCompany1.LCC_OH = org1.PK;

			ClientCompany clientCompany2 = Factory.New<ClientCompany>();
			clientCompany2.LCC_Code = "BBB";
			clientCompany2.LCC_LD = db2.PK;
			clientCompany2.LCC_OH = org2.PK;

			ClientCompany clientCompany3 = Factory.New<ClientCompany>();
			clientCompany3.LCC_Code = "CCC";
			clientCompany3.LCC_LD = db3.PK;

			ClientCompany clientCompany4 = Factory.New<ClientCompany>();
			clientCompany4.LCC_Code = "DDD";
			clientCompany4.LCC_LD = db4.PK;
			clientCompany4.LCC_OH = inactiveOrg.PK;

			HelpErrorLogOccurrence occurrence1 = Log.Occurrences.AddNew();
			HelpErrorLogOccurrence occurrence2 = Log.Occurrences.AddNew();
			HelpErrorLogOccurrence occurrence3 = Log.Occurrences.AddNew();
			HelpErrorLogOccurrence occurrence4 = Log.Occurrences.AddNew();
			HelpErrorLogOccurrence occurrence5 = Log.Occurrences.AddNew();

			occurrence1.HO_LD = licence1.LA_LD;
			occurrence2.HO_LD = licence2.LA_LD;
			occurrence3.HO_LD = licence3.LA_LD;
			occurrence3.HO_LCC = clientCompany3.PK;
			occurrence4.HO_LD = licence3.LA_LD;
			occurrence4.HO_LCC = clientCompany3.PK;
			occurrence5.HO_LD = licence4.LA_LD;
			occurrence5.HO_LCC = clientCompany4.PK;

			NewWorkItem workItem1 = Log.RelatedWorkItems.AddNew();
			NewWorkItem workItem2 = Log.RelatedWorkItems.AddNew();

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident.IM_LCC = licence1.ClientCompany.PK;
			incident.RelatedItems.Add(workItem1);

			Factory.Save();
		}

		#endregion

		public void TestExceptionMessageFirstLine()
		{
			Log.HE_ExceptionMessage = "Foo \r\n Bar";
			AssertEquals("ExceptionMessageFirstLine", "Foo", Log.ExceptionMessageFirstLine);
		}

		public void TestIncidentsAreCreatedOnSaveIfFixed()
		{
			HelpErrorLogOccurrence occurrence = CreateOccurrenceWithLicence(Log);
			NewWorkItem workItem = Log.RelatedWorkItems.AddNew();
			Log.HE_FixedDate = ZDateTime.UtcNow;
			Factory.Save();
			AssertEquals("RelatedIncidents.Count", 0, Log.RelatedIncidents.Count);

			Log.Logs.AddNew(Events.AutoMatchDone);
			Log.HE_FixedDate = ZDateTime.Empty;
			Factory.Save();
			AssertEquals("RelatedIncidents.Count", 0, Log.RelatedIncidents.Count);

			Log.HE_FixedDate = ZDateTime.UtcNow;
			Factory.Save();
			AssertEquals("RelatedIncidents.Count", 1, Log.RelatedIncidents.Count);
			AssertEquals("RelatedIncidents[0].IsInDatabase", true, Log.RelatedIncidents[0].IsInDatabase);
			AssertEquals("RelatedIncidents[0].IM_LCC", occurrence.ClientCompany.PK, Log.RelatedIncidents[0].IM_LCC);
		}

		public void TestRelatedIncidents()
		{
			PrepareDataForTestCreateIncidents();
			Log.CreateIncidents(true);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			EdiHelpErrorLog loadedLog = newFactory.Load<EdiHelpErrorLog>(Log.PK);
			AssertEquals("RelatedIncidents.Count", 3, loadedLog.RelatedIncidents.Count);
			AssertEquals("RelatedIncidents.ReadOnly", true, loadedLog.RelatedIncidents.ReadOnly);

			SupportIncident incident = Factory.New<SupportIncident>();
			NewWorkItem workItem = incident.RelatedWorkItems.AddNew();
			workItem.RelatedItems.Add(Log);

			Factory.Save();

			loadedLog.ReloadRelatedIncidents();
			AssertEquals("RelatedIncidents.Count", 4, loadedLog.RelatedIncidents.Count);
			AssertEquals("RelatedIncidents.Contains(incident.PK)", true, loadedLog.RelatedIncidents.Contains(incident.PK));
		}

		public void TestRelatedIncidentsFilter()
		{
			EdiHelpErrorLog log = Factory.New<EdiHelpErrorLog>();
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.RelatedItems.Add(log);
			Factory.Save();
			AssertEquals(1, log.RelatedIncidents.Count);
			Assert(log.RelatedIncidents.Contains(incident));
		}

		public void TestReload()
		{
			AssertEquals("RelatedWorkItems.Count", 0, Log.RelatedWorkItems.Count);
			AssertEquals("RelatedIncidents.Count", 0, Log.RelatedIncidents.Count);
			AssertEquals("HasAutomaticallyCreatedIncidents", false, Log.HasAutomaticallyCreatedIncidents);

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			EdiHelpErrorLog loadedLog = newFactory.Load<EdiHelpErrorLog>(Log.PK);
			CreateOccurrenceWithLicence(loadedLog);
			loadedLog.RelatedWorkItems.AddNew();
			loadedLog.CreateIncidents(true);

			Log.Reload();
			AssertEquals("RelatedWorkItems.Count", 1, Log.RelatedWorkItems.Count);
			AssertEquals("RelatedIncidents.Count", 1, Log.RelatedIncidents.Count);
			AssertEquals("HasAutomaticallyCreatedIncidents", true, Log.HasAutomaticallyCreatedIncidents);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestPopulateWorkItem_NullArgument()
		{
			Log.PopulateWorkItem(null);
		}

		public void TestPopulateWorkItem()
		{
			Log.HE_IssueNumber = "12349876";
			Log.HE_ExceptionMessage = "This is a really long long long exception message that will get trimmed";

			NewWorkItem item = Factory.New<NewWorkItem>();
			Log.PopulateWorkItem(item);
			AssertEquals("IM_Product", ProductTypes.Codes.Enterprise, item.WKI_WorkItemType);
			AssertEquals("IM_Description", "This is a really long long long exception message that will get trimmed", item.WKI_Summary);
			AssertEquals("IM_WorkItemType", NewWorkItemLookups.WorkItemTypeConstants.IssueFix, item.WKI_ActivitySubtype);
			AssertEquals("IM_Details", ZBlob.FromAscii("This is a really long long long exception message that will get trimmed"), item.WKI_Details);
			AssertEquals("AutoMatchDone Log Reference", "Created from issue 12349876", item.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AutoMatchDone.Code))[0].SL_Reference);
			AssertEquals("RelatedWorkItems.Count", 1, Log.RelatedWorkItems.Count);
			AssertEquals("RelatedItems.Count", 1, item.RelatedItems.Count);
			AssertEquals("RelatedWorkItems[0].PK", item.PK, Log.RelatedWorkItems[0].PK);
			AssertEquals("RelatedItems[0].PK", Log.PK, item.RelatedItems[0].PK);
		}

		public void TestFindWorkItemInfo()
		{
			ZString product = new ZString();
			ZString productArea = new ZString();
			ZString productModule = new ZString();
			AssertEquals("Cannot find correct assembly info", true, EdiHelpErrorLog.FindWorkItemInfo("CargoWise.Data", out product, out productArea, out productModule));
		}

		public void TestLoadFinalKeys()
		{
			AssertEquals(false, Log.HasChanges);

			Log.LoadFinalKeys = true;
			AssertEquals(true, Log.LoadFinalKeys);
			AssertEquals(false, Log.HasChanges);

			Log.LoadFinalKeys = false;
			AssertEquals(true, Log.LoadFinalKeys);
			AssertEquals(false, Log.HasChanges);

			for (int i = 0; i < 51; i++)
			{
				Log.Occurrences.AddNew();
			}

			AssertEquals(false, Log.LoadFinalKeys);
		}

		public void TestLoadFinalKeysInfo()
		{
			AssertEquals("LoadFinalKeys", Log.LoadFinalKeysInfo.Name);
		}

		public void TestGetIncidentCreatedMessage()
		{
			List<SupportIncident> incidents = new List<SupportIncident>();
			incidents.Add(Factory.New<SupportIncident>());
			incidents.Add(Factory.New<SupportIncident>());
			incidents.Add(Factory.New<SupportIncident>());

			incidents[0].IM_IncidentNumber = "00003";
			incidents[1].IM_IncidentNumber = "00002";
			incidents[2].IM_IncidentNumber = "00001";

			EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();

			incidents[0].IM_OH_Client = org1.PK;
			incidents[1].IM_OH_Client = org1.PK;
			incidents[2].IM_OH_Client = org2.PK;

			string message = EdiHelpErrorLog.GetIncidentCreatedMessage(incidents);

			StringBuilder expected = new StringBuilder();
			expected.AppendLine("The following incidents have been created:");
			expected.AppendLine();
			expected.AppendFormat("{0} for {1} - {2}", "00001", org2.OH_Code, org2.OH_FullName);
			expected.AppendLine();
			expected.AppendFormat("{0} for {1} - {2}", "00002", org1.OH_Code, org1.OH_FullName);
			expected.AppendLine();
			expected.AppendFormat("{0} for {1} - {2}", "00003", org1.OH_Code, org1.OH_FullName);

			AssertEquals(expected.ToString(), message);
		}

		public void TestHE_ExceptionMessageWithoutLineBreaks()
		{
			Log.HE_IssueNumber = "12349876";
			Log.HE_ExceptionMessage = "An exception message that contains a\r carriage return";

			NewWorkItem item = Factory.New<NewWorkItem>();
			Log.PopulateWorkItem(item);

			AssertEquals("HE_ExceptionMessage", "An exception message that contains a carriage return", Log.HE_ExceptionMessageWithoutLineBreaks);

			Log.HE_ExceptionMessage = "An exception message that contains a\n new line";
			AssertEquals("HE_ExceptionMessage", "An exception message that contains a new line", Log.HE_ExceptionMessageWithoutLineBreaks);

			Log.HE_ExceptionMessage = "An exception message that contains both a\r\n carriage return and a new line";
			AssertEquals("HE_ExceptionMessage", "An exception message that contains both a carriage return and a new line", Log.HE_ExceptionMessageWithoutLineBreaks);
		}

		public void TestStatusDescription()
		{
			IWorkTaskRelatedItem log = Log;
			AssertEquals("Not Fixed : 0", log.StatusDescription);
			Log.HE_FailCount = 2;
			AssertEquals("Not Fixed : 2", log.StatusDescription);
		}

		public void TestAfterSettingIssueNumber()
		{
			string issueNumber = null;
			var methodCalled = false;
			Log.AfterIssueNumberSet(() => issueNumber = Log.HE_IssueNumber);
			Log.AfterIssueNumberSet(() => methodCalled = true);
			AssertEquals("Method should not be called yet.", false, methodCalled);
			AssertNull("issueNumber should not be set yet", issueNumber);

			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				Log.SetIssueNumber();
			}
			AssertEquals("issueNumber should be set to be the same as the Log.", issueNumber, Log.HE_IssueNumber);
			AssertEquals("Method should have been called.", true, methodCalled);
		}

		public void TestIWorkTaskTreeNodeMembers()
		{
			AssertEquals(ZDateTime.Empty, Log.AgreedDeliveryDate);
			AssertEquals(ZString.Empty, Log.CurrentTaskStatus);
			AssertEquals(ZString.Empty, Log.CurrentTaskDescription);
			AssertEquals(ZString.Empty, Log.CurrentTaskCapabilityCodeDescription);
			AssertEquals(ZString.Empty, Log.CurrentTaskAssigned);
			AssertNull(Log.ChildrenOnlyRelatedItems);
			AssertNull(Log.ParentsOnlyRelatedItems);
		}

		#region Implementation

		EdiHelpErrorLog log;

		EdiHelpErrorLog Log
		{
			get { return log ?? (log = Factory.New<EdiHelpErrorLog>()); }
		}

		HelpErrorLogOccurrence CreateOccurrenceWithLicence(EdiHelpErrorLog log)
		{
			var factory = log.Factory;

			EDIOrgHeader org = factory.NewWithValidTestData<EDIOrgHeader>();
			org.CreateAndLoadLicenceForOrg();
			org.LicenceEnterpriseCode = "DDD";
			org.LicCompany.LC_CompanyCode = "ABC";
			LicenceDatabase db = org.LicCompany.LicDatabases.AddNew();
			db.LD_ServerCode = "HST";
			ClientCompany clientCompany = factory.New<ClientCompany>();
			clientCompany.LCC_LD = db.PK;
			clientCompany.LCC_Code = org.LicCompany.LC_CompanyCode;
			clientCompany.LCC_OH = org.PK;
			HelpErrorLogOccurrence result = log.Occurrences.AddNew();
			result.HO_LCC = clientCompany.PK;
			result.HO_LD = clientCompany.LCC_LD;

			factory.Save();
			return result;
		}

		#endregion
	}

	[TestedType(typeof(EdiHelpErrorLog))]
	sealed class HelpErrorLogRelatedItemTest : WorkTaskRelatedItemTestCase
	{
		protected override string ExpectedSelectionCriterion1 => string.Empty;
		protected override string ExpectedSelectionCriterion2 => string.Empty;
		protected override string ExpectedSelectionCriterion3 => string.Empty;
		protected override string ExpectedSelectionCriterion4 => string.Empty;
		protected override string ExpectedSelectionCriterion5 => string.Empty;

		protected override IWorkTaskRelatedItem GetItemForSelectionCriteriaTest()
		{
			return Factory.NewWithValidTestData<EdiHelpErrorLog>();
		}

		protected override Type ExpectedPivotCollectionType => typeof(GenPivotCollection);
	}

	[TestedType(typeof(EdiHelpErrorLog))]
	sealed class HelpErrorLogRelatedItemSourceTest : WorkTaskRelatedItemSourceTestCase
	{
	}
}
