using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.IssueManager.Business.Test
{
	public class LogMergerTest : TestCaseWithFactory
	{
		public void TestMerge()
		{
			EdiHelpErrorLog log1 = Factory.New<EdiHelpErrorLog>();
			log1.HE_FirstProcessed = new ZDateTime(2004, 10, 09, 5, 0, 0);
			log1.Occurrences.AddNew();
			log1.Occurrences.AddNew();
			HelpErrorLogKey key1 = log1.Keys.AddNew();
			key1.HK_Key = "Key1";

			EdiHelpErrorLog log2 = Factory.New<EdiHelpErrorLog>();
			log2.HE_FirstProcessed = new ZDateTime(2004, 10, 09, 4, 0, 0);
			log2.Occurrences.AddNew();
			log2.Occurrences.AddNew();
			log2.Occurrences.AddNew();
			HelpErrorLogKey key2 = log2.Keys.AddNew();
			key2.HK_Key = "Key2";

			HelpErrorLogCollection logCollection = new HelpErrorLogCollection(Factory);
			logCollection.IsManagedForDataRefresh = true;
			logCollection.Add(log1);
			logCollection.Add(log2); // simulate being in module grid managed by data refresh bus

			Factory.Save();
			EdiHelpErrorLog[] logsToMerge = new EdiHelpErrorLog[] { log1, log2 };

			Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();

			LogMerger merger = new LogMerger();
			ZGuid result = merger.Merge(logsToMerge);

			AssertEquals("No Errors", 0, Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Count);
			Assert("Successful merge should return valid ZGuid", result.IsValid);
			AssertEquals("Merged into log with earliest FirstReported", log2.PK, result);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			EdiHelpErrorLog reloadedLog1 = newFactory.Load<EdiHelpErrorLog>(log1.PK);
			EdiHelpErrorLog reloadedLog2 = newFactory.Load<EdiHelpErrorLog>(log2.PK);

			AssertEquals("DataRefresh Updates FailCount", new ZInt(5), log2.HE_FailCount);
			AssertEquals("Occurrences merged into log with earliest FirstReported", 5, reloadedLog2.Occurrences.Count);

			AssertEquals("Keys Merged", new ZInt(2), reloadedLog2.Keys.Count);

			Assert("Log1 Deleted", log1.IsDeleted);
			AssertNull("Reloaded Log1 is null", reloadedLog1);
		}

		public void TestMerge_OneIssue()
		{
			EdiHelpErrorLog log1 = Factory.New<EdiHelpErrorLog>();
			Factory.Save();
			EdiHelpErrorLog[] logsToMerge = new EdiHelpErrorLog[] { log1 };

			LogMerger merger = new LogMerger();
			ZGuid result = merger.Merge(logsToMerge);

			Assert("Unsuccessful merge should return empty ZGuid", result.IsEmpty);
			AssertEquals(LogMerger.CannotMergeNeedToSelectMoreThanOneIssueErrorMessage, merger.ErrorMessage);
		}

		public void TestMerge_ClientVisibility()
		{
			EdiHelpErrorLog log1 = Factory.New<EdiHelpErrorLog>();
			log1.HE_FirstProcessed = new ZDateTime(2004, 10, 09, 5, 0, 0);
			log1.HE_IsClientVisible = ZBool.False;

			EdiHelpErrorLog log2 = Factory.New<EdiHelpErrorLog>();
			log2.HE_FirstProcessed = new ZDateTime(2006, 10, 09, 5, 0, 0);
			log2.HE_IsClientVisible = ZBool.True;

			Factory.Save();
			EdiHelpErrorLog[] logsToMerge = new EdiHelpErrorLog[] { log1, log2 };

			LogMerger merger = new LogMerger();
			ZGuid result = merger.Merge(logsToMerge);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			EdiHelpErrorLog reloadedLog1 = newFactory.Load<EdiHelpErrorLog>(log1.PK);
			EdiHelpErrorLog reloadedLog2 = newFactory.Load<EdiHelpErrorLog>(log2.PK);

			Assert("Log2 Deleted", log2.IsDeleted);
			AssertNull("Reloaded Log2 is null", reloadedLog2);
			AssertNotNull("Reloaded Log1 is not null", reloadedLog1);
			AssertEquals(ZBool.True, reloadedLog1.HE_IsClientVisible);

			log2 = Factory.New<EdiHelpErrorLog>();
			log2.HE_FirstProcessed = new ZDateTime(2006, 10, 09, 5, 0, 0);
			log1.HE_IsClientVisible = ZBool.True;

			Factory.Save();
			logsToMerge = new EdiHelpErrorLog[] { log1, log2 };
			result = merger.Merge(logsToMerge);

			reloadedLog1 = newFactory.Load<EdiHelpErrorLog>(log1.PK);
			reloadedLog2 = newFactory.Load<EdiHelpErrorLog>(log2.PK);

			Assert("Log2 Deleted", log2.IsDeleted);
			AssertNull("Reloaded Log2 is null", reloadedLog2);
			AssertNotNull("Reloaded Log1 is not null", reloadedLog1);
			AssertEquals(ZBool.True, reloadedLog1.HE_IsClientVisible);

			log1.HE_IsClientVisible = ZBool.False;

			log2 = Factory.New<EdiHelpErrorLog>();
			log2.HE_FirstProcessed = new ZDateTime(2006, 10, 09, 5, 0, 0);
			log1.HE_IsClientVisible = ZBool.False;

			Factory.Save();
			logsToMerge = new EdiHelpErrorLog[] { log1, log2 };
			result = merger.Merge(logsToMerge);

			reloadedLog1 = newFactory.Load<EdiHelpErrorLog>(log1.PK);
			reloadedLog2 = newFactory.Load<EdiHelpErrorLog>(log2.PK);

			Assert("Log2 Deleted", log2.IsDeleted);
			AssertNull("Reloaded Log2 is null", reloadedLog2);
			AssertNotNull("Reloaded Log1 is not null", reloadedLog1);
			AssertEquals(ZBool.False, reloadedLog1.HE_IsClientVisible);
		}

		public void TestMerge_RelatedWorkItems()
		{
			EdiHelpErrorLog log1 = Factory.New<EdiHelpErrorLog>();
			log1.HE_FirstProcessed = new ZDateTime(2005, 09, 10, 4, 50, 0);
			HelpErrorLogOccurrence occurrence1 = log1.Occurrences.AddNew();
			occurrence1.HO_EXEDateTime = new ZDateTime(2005, 09, 10, 4, 50, 0);
			occurrence1.HO_ExceptionDateTime = new ZDateTime(2005, 09, 10, 4, 50, 0);

			EdiHelpErrorLog log2 = Factory.New<EdiHelpErrorLog>();
			log2.HE_FirstProcessed = new ZDateTime(2005, 09, 10, 3, 50, 0);
			HelpErrorLogOccurrence occurrence2 = log2.Occurrences.AddNew();
			occurrence2.HO_EXEDateTime = new ZDateTime(2005, 09, 10, 3, 50, 0);
			occurrence2.HO_ExceptionDateTime = new ZDateTime(2005, 09, 10, 3, 50, 0);

			NewWorkItem workItem1 = Factory.NewWithValidTestData<NewWorkItem>();
			workItem1.RelatedItems.Add(log1);

			NewWorkItem workItem2 = Factory.NewWithValidTestData<NewWorkItem>();
			workItem2.RelatedItems.Add(log1);

			NewWorkItem workItem3 = Factory.NewWithValidTestData<NewWorkItem>();
			workItem3.RelatedItems.Add(log2);

			Factory.Save();
			EdiHelpErrorLog[] logsToMerge = new EdiHelpErrorLog[] { log1, log2 };

			LogMerger merger = new LogMerger();
			ZGuid result = merger.Merge(logsToMerge);

			Assert(result.IsValid);
			AssertEquals(result, log2.PK);

			workItem1.RelatedItems.Load();
			workItem2.RelatedItems.Load();
			workItem3.RelatedItems.Load();

			AssertEquals(log2.PK, workItem1.RelatedItems[0].PK);
			AssertEquals(log2.PK, workItem2.RelatedItems[0].PK);
			AssertEquals(log2.PK, workItem3.RelatedItems[0].PK);
		}

		public void TestMergeFixedDate()
		{
			EdiHelpErrorLog log1 = Factory.New<EdiHelpErrorLog>();
			log1.HE_FirstProcessed = new ZDateTime(2004, 10, 09, 4, 0, 0);
			log1.HE_FixedDate = new ZDateTime(2009, 09, 10, 3, 50, 0);

			EdiHelpErrorLog log2 = Factory.New<EdiHelpErrorLog>();
			log2.HE_FirstProcessed = new ZDateTime(2005, 10, 09, 5, 0, 0);
			log2.Occurrences.AddNew();
			log2.FirstOccurrence.HO_EXEDateTime = new ZDateTime(2010, 09, 10, 3, 50, 0);

			Factory.Save();
			EdiHelpErrorLog[] logsToMerge = new EdiHelpErrorLog[] { log1, log2 };
			LogMerger merger = new LogMerger();
			merger.Merge(logsToMerge);

			AssertEquals("Fixed date has been cleared", ZDateTime.Empty, log1.HE_FixedDate);

			log2 = Factory.New<EdiHelpErrorLog>();
			log2.HE_FirstProcessed = new ZDateTime(2005, 10, 09, 5, 0, 0);
			ZDateTime fixedDate = new ZDateTime(2010, 10, 10, 3, 50, 0);
			log2.HE_FixedDate = fixedDate;

			log1 = Factory.New<EdiHelpErrorLog>();
			log1.HE_FirstProcessed = new ZDateTime(2004, 10, 09, 5, 0, 0);
			log1.HE_FixedDate = new ZDateTime(2009, 09, 10, 3, 50, 0);

			Factory.Save();
			logsToMerge = new EdiHelpErrorLog[] { log1, log2 };
			merger.Merge(logsToMerge);

			AssertEquals("Fixed date has been changed", fixedDate, log1.HE_FixedDate);

			log1 = Factory.New<EdiHelpErrorLog>();
			log1.HE_FirstProcessed = new ZDateTime(2011, 1, 1);
			log1.HE_FixedDate = new ZDateTime(2011, 2, 2);

			log2 = Factory.New<EdiHelpErrorLog>();
			log2.Occurrences.AddNew().HO_EXEDateTime = new ZDateTime(2011, 3, 3);
			log2.HE_FirstProcessed = new ZDateTime(2012, 1, 1);

			EdiHelpErrorLog log3 = Factory.New<EdiHelpErrorLog>();
			log3.Occurrences.AddNew().HO_EXEDateTime = new ZDateTime(2011, 3, 3);
			log3.HE_FirstProcessed = new ZDateTime(2012, 2, 2);
			fixedDate = log3.HE_FixedDate = new ZDateTime(2013, 1, 1);

			Factory.Save();
			logsToMerge = new EdiHelpErrorLog[] { log1, log2, log3 };
			merger.Merge(logsToMerge);

			AssertEquals("Fixed date has been changed", fixedDate, log1.HE_FixedDate);
		}

		public void TestMergeExeDate()
		{
			EdiHelpErrorLog log1 = Factory.New<EdiHelpErrorLog>();
			log1.HE_FirstProcessed = new ZDateTime(2004, 10, 09, 4, 0, 0);
			log1.HE_LastEXEVersionDate = new ZDateTime(2009, 09, 10, 3, 50, 0);

			EdiHelpErrorLog log2 = Factory.New<EdiHelpErrorLog>();
			log2.HE_FirstProcessed = new ZDateTime(2005, 10, 09, 5, 0, 0);
			ZDateTime latestExeDate = new ZDateTime(2010, 09, 10, 3, 50, 0);
			log2.HE_LastEXEVersionDate = latestExeDate;

			Factory.Save();
			EdiHelpErrorLog[] logsToMerge = new EdiHelpErrorLog[] { log1, log2 };
			LogMerger merger = new LogMerger();
			merger.Merge(logsToMerge);

			AssertEquals(latestExeDate, log1.HE_LastEXEVersionDate);

			log2 = Factory.New<EdiHelpErrorLog>();
			log2.HE_FirstProcessed = new ZDateTime(2005, 10, 09, 5, 0, 0);
			log2.HE_LastEXEVersionDate = new ZDateTime(2010, 10, 10, 3, 50, 0);

			log1 = Factory.New<EdiHelpErrorLog>();
			log1.HE_FirstProcessed = new ZDateTime(2004, 10, 09, 5, 0, 0);
			latestExeDate = new ZDateTime(2011, 09, 10, 3, 50, 0);
			log1.HE_LastEXEVersionDate = latestExeDate;

			Factory.Save();
			logsToMerge = new EdiHelpErrorLog[] { log1, log2 };
			merger.Merge(logsToMerge);

			AssertEquals(latestExeDate, log1.HE_LastEXEVersionDate);
		}

		[StressTest]
		[SnailTest]
		public void TestMergeInsaneAmountOfIssues()
		{
			for (int i = 0; i < 400; i++)
			{
				EdiHelpErrorLog errorLog = Factory.New<EdiHelpErrorLog>();
				errorLog.HE_FirstProcessed = ZDateTime.Today;

				for (int j = 0; j < 50; j++)
				{
					errorLog.Occurrences.AddNew();
				}
			}
			Factory.Save();

			HelpErrorLogCollection collection = new HelpErrorLogCollection(Factory);
			collection.Load();
			Assert("At least 400 issues should be loaded.", collection.Count >= 400);

			LogMerger merger = new LogMerger();
			merger.Merge(collection.ToArray<EdiHelpErrorLog>());

			collection.Load();
			AssertEquals("Only one issue should be left after merging.", 1, collection.Count);
		}

		public void TestWIGetsCreatedAfterMerge()
		{
			InitializeSettingForClientVisible();

			var log1 = CreateHelpErrorLog("Key1", 2, ZDateTime.Today, ZDateTime.Today.AddDays(-25));
			var log2 = CreateHelpErrorLog("Key2", 4, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(-25));

			HelpErrorLogCollection logCollection = new HelpErrorLogCollection(Factory);
			logCollection.IsManagedForDataRefresh = true;
			logCollection.Add(log1);
			logCollection.Add(log2); // simulate being in module grid managed by data refresh bus
			Factory.Save();

			EdiHelpErrorLog[] logsToMerge = new EdiHelpErrorLog[] { log1, log2 };

			Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();

			Assert("There should not be any work item attached to the issue.", !log2.HasWorkItems);

			LogMerger merger = new LogMerger();
			ZGuid result = merger.Merge(logsToMerge);

			AssertEquals("No Errors", 0, Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Count);
			Assert("Successful merge should return valid ZGuid", result.IsValid);
			AssertEquals("Merged into log with earliest FirstReported", log2.PK, result);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			EdiHelpErrorLog masterLog = newFactory.Load<EdiHelpErrorLog>(result);

			AssertEquals("Occurrences merged into log with earliest FirstReported", 6, masterLog.Occurrences.Count);
			AssertEquals("Blank fixed date", ZDateTime.Empty, masterLog.HE_FixedDate);
			Assert("There should be a new work item attached to the master issue.", masterLog.HasWorkItems);
			AssertEquals("One WI exists", 1, masterLog.RelatedWorkItems.Count);
		}

		public void TestWINotGetCreatedAfterMergeInCaseOfFixedDataIsNotBlank()
		{
			InitializeSettingForClientVisible();

			var log1 = CreateHelpErrorLog("Key1", 4, ZDateTime.Today, ZDateTime.Today.AddDays(-25));
			var log2 = CreateHelpErrorLog("Key2", 2, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(-25));
			log2.HE_FixedDate = ZDateTime.Today;

			HelpErrorLogCollection logCollection = new HelpErrorLogCollection(Factory);
			logCollection.IsManagedForDataRefresh = true;
			logCollection.Add(log1);
			logCollection.Add(log2); // simulate being in module grid managed by data refresh bus
			Factory.Save();

			EdiHelpErrorLog[] logsToMerge = new EdiHelpErrorLog[] { log1, log2 };

			Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();

			Assert("There should not be any work item attached to the issue.", !log2.HasWorkItems);

			LogMerger merger = new LogMerger();
			ZGuid result = merger.Merge(logsToMerge);

			AssertEquals("No Errors", 0, Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Count);
			Assert("Successful merge should return valid ZGuid", result.IsValid);
			AssertEquals("Merged into log with earliest FirstReported", log2.PK, result);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			EdiHelpErrorLog masterLog = newFactory.Load<EdiHelpErrorLog>(result);

			AssertEquals("Occurrences merged into log with earliest FirstReported", 6, masterLog.Occurrences.Count);
			AssertNotEquals("Non-Blank fixed date", ZDateTime.Empty, masterLog.HE_FixedDate);
			Assert("There should not be a new work item attached to the master issue as master issue has fixed date.", !masterLog.HasWorkItems);
		}

		public void TestWINotGetCreatedAfterMergeInCaseOpenWIAlreadyExist()
		{
			InitializeSettingForClientVisible();

			var log1 = CreateHelpErrorLog("Key1", 4, ZDateTime.Today, ZDateTime.Today.AddDays(-25));
			var log2 = CreateHelpErrorLog("Key2", 2, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(-25));

			var newWorkItem = Factory.New<NewWorkItem>();
			newWorkItem.RelatedItems.Add(log2);
			var assignment = new IssueAssignment(string.Empty, string.Empty, string.Empty);
			newWorkItem.WKI_WorkItemType = assignment.Product;
			newWorkItem.WKI_WorkItemArea = assignment.ProductArea;
			newWorkItem.WKI_ActivityType = assignment.Module;
			newWorkItem.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.IssueFix;
			newWorkItem.WKI_Priority = ReleaseRings.Codes.GPR;
			var task1 = newWorkItem.WorkflowItems.AddNew();
			task1.P9_Description = "Failing Unit Test";
			task1.P9_GS_NKAssignedStaffMember = "";
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			HelpErrorLogCollection logCollection = new HelpErrorLogCollection(Factory);
			logCollection.IsManagedForDataRefresh = true;
			logCollection.Add(log1);
			logCollection.Add(log2); // simulate being in module grid managed by data refresh bus
			Factory.Save();

			EdiHelpErrorLog[] logsToMerge = new EdiHelpErrorLog[] { log1, log2 };

			Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();

			LogMerger merger = new LogMerger();
			ZGuid result = merger.Merge(logsToMerge);

			AssertEquals("No Errors", 0, Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Count);
			Assert("Successful merge should return valid ZGuid", result.IsValid);
			AssertEquals("Merged into log with earliest FirstReported", log2.PK, result);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			EdiHelpErrorLog masterLog = newFactory.Load<EdiHelpErrorLog>(result);

			AssertEquals("Occurrences merged into log with earliest FirstReported", 6, masterLog.Occurrences.Count);
			Assert("There is work item attached to the master issue from original issue.", masterLog.HasWorkItems);
			AssertEquals("There is only one WI and not new WI gets attached.", 1, masterLog.RelatedWorkItems.Count);
		}

		public void TestWINotGetCreatedAfterMergeInCaseNotEnoughOccurances()
		{
			InitializeSettingForClientVisible();

			var log1 = CreateHelpErrorLog("Key1", 3, ZDateTime.Today, ZDateTime.Today.AddDays(-25));
			var log2 = CreateHelpErrorLog("Key2", 1, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(-25));

			HelpErrorLogCollection logCollection = new HelpErrorLogCollection(Factory);
			logCollection.IsManagedForDataRefresh = true;
			logCollection.Add(log1);
			logCollection.Add(log2); // simulate being in module grid managed by data refresh bus
			Factory.Save();

			EdiHelpErrorLog[] logsToMerge = new EdiHelpErrorLog[] { log1, log2 };

			Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();

			LogMerger merger = new LogMerger();
			ZGuid result = merger.Merge(logsToMerge);

			AssertEquals("No Errors", 0, Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Count);
			Assert("Successful merge should return valid ZGuid", result.IsValid);
			AssertEquals("Merged into log with earliest FirstReported", log2.PK, result);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			EdiHelpErrorLog masterLog = newFactory.Load<EdiHelpErrorLog>(result);

			AssertEquals("Occurrences merged into log with earliest FirstReported", 4, masterLog.Occurrences.Count);

			Assert("There should not be a new work item attached to the master issue as master issue does not has enough occurances.", !masterLog.HasWorkItems);
		}

		void InitializeSettingForClientVisible()
		{
			var thresholdCollection = new IssueWorkItemCreationThresholdCollection();
			var threshold = thresholdCollection.AddNew();
			threshold.IssueOccurrenceThreshold = 6;
			threshold.ThresholdTimespan = 7;
			EDIDataRegistry.Instance.IssueWorkItemCreationThresholdClientVisible.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, thresholdCollection);
		}

		void InitializeSettingForClientNonVisible()
		{
			var thresholdCollection = new IssueWorkItemCreationThresholdCollection();
			var threshold = thresholdCollection.AddNew();
			threshold.IssueOccurrenceThreshold = 7;
			threshold.ThresholdTimespan = 7;
			EDIDataRegistry.Instance.IssueWorkItemCreationThresholdNonClientVisible.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, thresholdCollection);
		}

		public void TestWINotGetCreatedAfterMergeInCaseNotEnoughOccurances_ClientVisible()
		{
			InitializeSettingForClientVisible();

			var log1 = CreateHelpErrorLog("Key1", 3, ZDateTime.Today, ZDateTime.Today.AddDays(-25));
			var log2 = CreateHelpErrorLog("Key2", 2, ZDateTime.Today.AddDays(-8), ZDateTime.Today.AddDays(-25));
			HelpErrorLogOccurrence occurance = (HelpErrorLogOccurrence)log1.Occurrences.First();
			occurance.HO_ExceptionDateTime = new ZDateTime(2020, 09, 22, 5, 0, 0);

			HelpErrorLogCollection logCollection = new HelpErrorLogCollection(Factory);
			logCollection.IsManagedForDataRefresh = true;
			logCollection.Add(log1);
			logCollection.Add(log2); // simulate being in module grid managed by data refresh bus
			Factory.Save();

			EdiHelpErrorLog[] logsToMerge = new EdiHelpErrorLog[] { log1, log2 };

			Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();

			LogMerger merger = new LogMerger();
			ZGuid result = merger.Merge(logsToMerge);

			AssertEquals("No Errors", 0, Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Count);
			Assert("Successful merge should return valid ZGuid", result.IsValid);
			AssertEquals("Merged into log with earliest FirstReported", log2.PK, result);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			EdiHelpErrorLog masterLog = newFactory.Load<EdiHelpErrorLog>(result);

			AssertEquals("Occurrences merged into log with earliest FirstReported", 5, masterLog.Occurrences.Count);

			Assert("There should not be a new work item attached to the master issue as master issue does not has enough occurances within threshold timespan.", !masterLog.HasWorkItems);
		}

		public void TestWINotGetCreatedAfterMergeInCaseNotEnoughOccurancesWithinThresholdTimespan_ClientVisible()
		{
			InitializeSettingForClientVisible();

			var log1 = CreateHelpErrorLog("Key1", 4, ZDateTime.Today, ZDateTime.Today.AddDays(-25));
			var log2 = CreateHelpErrorLog("Key2", 2, ZDateTime.Today.AddDays(-8), ZDateTime.Today.AddDays(-25));
			HelpErrorLogOccurrence occurance = (HelpErrorLogOccurrence)log1.Occurrences.First();
			occurance.HO_ExceptionDateTime = new ZDateTime(2020, 09, 22, 5, 0, 0);

			HelpErrorLogCollection logCollection = new HelpErrorLogCollection(Factory);
			logCollection.IsManagedForDataRefresh = true;
			logCollection.Add(log1);
			logCollection.Add(log2); // simulate being in module grid managed by data refresh bus
			Factory.Save();

			EdiHelpErrorLog[] logsToMerge = new EdiHelpErrorLog[] { log1, log2 };

			Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();

			LogMerger merger = new LogMerger();
			ZGuid result = merger.Merge(logsToMerge);

			AssertEquals("No Errors", 0, Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Count);
			Assert("Successful merge should return valid ZGuid", result.IsValid);
			AssertEquals("Merged into log with earliest FirstReported", log2.PK, result);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			EdiHelpErrorLog masterLog = newFactory.Load<EdiHelpErrorLog>(result);

			AssertEquals("Occurrences merged into log with earliest FirstReported", 6, masterLog.Occurrences.Count);

			Assert("There should not be a new work item attached to the master issue as master issue does not has enough occurances within threshold timespan.", !masterLog.HasWorkItems);
		}

		public void TestWINotGetCreatedAfterMergeInCaseNotEnoughOccurancesWithinThresholdTimespan_ClientNonVisible()
		{
			InitializeSettingForClientVisible();
			InitializeSettingForClientNonVisible();

			var log1 = CreateHelpErrorLog("Key1", 4, ZDateTime.Today, ZDateTime.Today.AddDays(-25), false);
			var log2 = CreateHelpErrorLog("Key2", 2, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(-25), false);
			HelpErrorLogOccurrence occurance = (HelpErrorLogOccurrence)log1.Occurrences.First();
			occurance.HO_ExceptionDateTime = new ZDateTime(2020, 09, 22, 5, 0, 0);

			HelpErrorLogCollection logCollection = new HelpErrorLogCollection(Factory);
			logCollection.IsManagedForDataRefresh = true;
			logCollection.Add(log1);
			logCollection.Add(log2); // simulate being in module grid managed by data refresh bus
			Factory.Save();

			EdiHelpErrorLog[] logsToMerge = new EdiHelpErrorLog[] { log1, log2 };

			Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();

			LogMerger merger = new LogMerger();
			ZGuid result = merger.Merge(logsToMerge);

			AssertEquals("No Errors", 0, Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Count);
			Assert("Successful merge should return valid ZGuid", result.IsValid);
			AssertEquals("Merged into log with earliest FirstReported", log2.PK, result);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			EdiHelpErrorLog masterLog = newFactory.Load<EdiHelpErrorLog>(result);

			AssertEquals("Occurrences merged into log with earliest FirstReported", 6, masterLog.Occurrences.Count);

			Assert("There should not be a new work item attached to the master issue as master issue does not has enough occurances within threshold timespan.", !masterLog.HasWorkItems);
		}

		public void TestWINotGetCreatedAfterMergeInCaseEXEDateIsNotWithinLast30DaysFromRecentOccurances()
		{
			InitializeSettingForClientVisible();

			var log1 = CreateHelpErrorLog("Key1", 4, ZDateTime.Today, ZDateTime.Today.AddDays(-35));
			var log2 = CreateHelpErrorLog("Key2", 2, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(-25));
			HelpErrorLogOccurrence occurance = (HelpErrorLogOccurrence)log1.Occurrences.First();
			occurance.HO_EXEDateTime = new ZDateTime(2020, 09, 22, 5, 0, 0);

			HelpErrorLogCollection logCollection = new HelpErrorLogCollection(Factory);
			logCollection.IsManagedForDataRefresh = true;
			logCollection.Add(log1);
			logCollection.Add(log2); // simulate being in module grid managed by data refresh bus
			Factory.Save();

			EdiHelpErrorLog[] logsToMerge = new EdiHelpErrorLog[] { log1, log2 };

			Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();

			LogMerger merger = new LogMerger();
			ZGuid result = merger.Merge(logsToMerge);

			AssertEquals("No Errors", 0, Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Count);
			Assert("Successful merge should return valid ZGuid", result.IsValid);
			AssertEquals("Merged into log with earliest FirstReported", log2.PK, result);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			EdiHelpErrorLog masterLog = newFactory.Load<EdiHelpErrorLog>(result);

			AssertEquals("Occurrences merged into log with earliest FirstReported", 6, masterLog.Occurrences.Count);

			Assert("There should not be a new work item attached to the master issue as master issue does not has enough occurances within threshold timespan.", !masterLog.HasWorkItems);
		}

		EdiHelpErrorLog CreateHelpErrorLog(string skey, int numberOfOccurances, ZDateTime exceptionDateTime, ZDateTime exeDateTime, bool isClientVisible = true)
		{
			EdiHelpErrorLog log = Factory.New<EdiHelpErrorLog>();
			log.HE_IsClientVisible = isClientVisible;
			log.HE_FirstProcessed = exceptionDateTime;
			for (int i = 0; i < numberOfOccurances; i++)
			{
				HelpErrorLogOccurrence occurrence = log.Occurrences.AddNew();
				occurrence.HO_EXEDateTime = exeDateTime;
				occurrence.HO_ExceptionDateTime = exceptionDateTime;
			}
			HelpErrorLogKey key = log.Keys.AddNew();
			key.HK_Key = skey;
			return log;
		}
	}
}
