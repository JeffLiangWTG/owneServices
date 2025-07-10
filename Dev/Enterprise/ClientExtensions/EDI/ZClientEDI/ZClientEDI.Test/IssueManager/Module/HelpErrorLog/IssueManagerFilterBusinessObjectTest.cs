using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Client.EDI.IssueManager.Module.IssueManagerFilterBusinessObject;

namespace Enterprise.Client.EDI.IssueManager.Module
{
	[TestedType(typeof(IssueManagerFilterBusinessObject))]
	class IssueManagerFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestIssueNumberExcludesOtherFilters()
		{
			Factory.Save();
			((ModuleTextFilter)BizObj["Issue Number"]).Property = ZString.Empty;
			((ModuleTextFilter)BizObj["Issue Number"]).IsActive = true;
			((ModuleTextFilter)BizObj["Status"]).Property = ErrorLogStatusFilter.CodeConstants.Fixed;
			((ModuleTextFilter)BizObj["Status"]).IsActive = true;
			FilterCollection.Load(BizObj.Filter);
			AssertCollectionNotContains("PreCondition: Should not find UnassignedLog when Status is set to assigned", UnassignedLog, FilterCollection);
			((ModuleTextFilter)BizObj["Issue Number"]).Property = UnassignedLog.HE_IssueNumber;
			((ModuleTextFilter)BizObj["Issue Number"]).IsActive = true;
			((ModuleTextFilter)BizObj["Status"]).IsActive = false;
			FilterCollection.Load(BizObj.Filter);
			AssertCollectionContains("Should find UnassignedLog by IssueNumber", UnassignedLog, FilterCollection);
			AssertEquals("Should only find the log with matching IssueNumber", 1, FilterCollection.Count);
		}

		public void TestClientExceptionId()
		{
			((ModuleTextFilter)BizObj["Status"]).Property = "";
			((ModuleTextFilter)BizObj["Status"]).IsActive = true;
			((ModuleTextFilter)BizObj["Client Name"]).Property = "";
			((ModuleTextFilter)BizObj["Client Name"]).IsActive = true;
			HelpErrorLogOccurrence occurrence1 = UnassignedLog.Occurrences.AddNew();
			occurrence1.HO_ExceptionID = "E00000001";
			HelpErrorLogOccurrence occurrence4 = AssignedAndFixedLog.Occurrences.AddNew();
			occurrence4.HO_ExceptionID = "E00000004";
			Factory.Save();
			FilterCollection.Load(BizObj.Filter);
			AssertEquals("Unfiltered Count", 3, FilterCollection.Count);
			((ModuleTextFilter)BizObj["Exception ID"]).Property = "E00000001";
			((ModuleTextFilter)BizObj["Exception ID"]).IsActive = true;
			FilterCollection.Load(BizObj.Filter);
			AssertFilter(UnassignedLog);
			((ModuleTextFilter)BizObj["Exception ID"]).Property = "E00000004";
			((ModuleTextFilter)BizObj["Exception ID"]).IsActive = true;
			FilterCollection.Load(BizObj.Filter);
			AssertFilter(AssignedAndFixedLog);
		}

		public void TestOnlyShowClientVisibleIssues()
		{
			AssignedAndFixedLog.HE_IsClientVisible = ZBool.True;
			AssignedNotFixedLog.HE_IsClientVisible = ZBool.False;
			UnassignedLog.HE_IsClientVisible = ZBool.False;
			((ModuleTextFilter)BizObj["Status"]).Property = "";
			((ModuleTextFilter)BizObj["Status"]).IsActive = true;
			((ModuleFlagsFilter)BizObj["Client Visible"]).Property0 = true;
			((ModuleFlagsFilter)BizObj["Client Visible"]).IsActive = true;
			FilterCollection.Load(BizObj.Filter);
			AssertCollectionContains("AssignedAndFixedLog should be in collection", AssignedAndFixedLog, FilterCollection);
			AssertEquals("Should be AssignedAndFixedLog only", 1, FilterCollection.Count);
			((ModuleFlagsFilter)BizObj["Client Visible"]).Property0 = false;
			((ModuleFlagsFilter)BizObj["Client Visible"]).IsActive = true;
			FilterCollection.Load(BizObj.Filter);
			AssertCollectionNotContains("AssignedAndFixedLog shouldn't be in the collection", AssignedAndFixedLog, FilterCollection);
			AssertEquals("Shoul be only two log in the collection", 2, FilterCollection.Count);
		}

		public void TestNoCriteria()
		{
			((ModuleTextFilter)BizObj["Status"]).Property = "";
			((ModuleTextFilter)BizObj["Status"]).IsActive = true;
			FilterCollection.Load(BizObj.Filter);
			AssertEquals("Unfiltered Count", 3, FilterCollection.Count);
		}

		public void TestRelatedWorkItems()
		{
			((ModuleTextFilter)BizObj["Status"]).Property = "";
			((ModuleTextFilter)BizObj["Status"]).IsActive = true;
			((ModuleTextFilter)BizObj["Work Items"]).Property = WithWorkItemsCodes.WithoutWorkItemsCode;
			((ModuleTextFilter)BizObj["Work Items"]).IsActive = true;
			Factory.Save();
			FilterCollection.Load(BizObj.Filter);
			AssertEquals("Issues without work items count", 2, FilterCollection.Count);
		}

		[TestDate(2015, 7, 14)]
		public void TestRelatedWorkItems_WhenAttachedWorkItemContainsNonTaskWorkflowElements()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var task = MasterFilesTestHelper.CreateTask(workItem);
			MasterFilesTestHelper.CreateTrigger(workItem, Events.CustomisableEvent69);
			var milestone = MasterFilesTestHelper.CreateMilestone(workItem, Events.CustomisableEvent69);
			MasterFilesTestHelper.CreateException(workItem, milestone);
			var issue = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			issue.HE_LastVersionNumber = "141.181";
			Factory.Save();
			AssertEquals("Issue should be marked as non-fixed", ZDateTime.Empty, issue.HE_FixedDate);
			var filter = (ModuleTextFilter)BizObj["Work Items"];
			filter.Property = WithWorkItemsCodes.NeedsWorkItem;
			filter.IsActive = true;
			AssertWorkItemsFilterResults(true, "Issue has no attached WI, so it should need a WI.");
			workItem.RelatedItems.Add(issue);
			Factory.Save();
			AssertWorkItemsFilterResults(false, "Attached work item is incomplete, so issue should not need a WI.");
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals("Issue should be marked as fixed", ZDateTime.Now, issue.HE_FixedDate);
			AssertWorkItemsFilterResults(false, "Attached WI is complete, but the issue is marked as fixed, so it should not need a WI.");
			issue.HE_FixedDate = ZDateTime.Empty;
			Factory.Save();
			AssertWorkItemsFilterResults(true, "Attached WI is complete, and the issue is marked as non-fixed, so it should need a WI. Only incomplete tasks should be considered; other workflow elements such as triggers, milestones, and exceptions should not cause attached WI to be considered incomplete.");
			void AssertWorkItemsFilterResults(bool shouldContainWorkItem, string message)
			{
				FilterCollection.Load(BizObj.Filter);
				if (shouldContainWorkItem)
				{
					AssertCollectionContains(message, issue, FilterCollection);
				}
				else
				{
					AssertCollectionNotContains(message, issue, FilterCollection);
				}
			}
		}

		public void TestRelatedToWorkItem()
		{
			var filter = ((ModuleGuidFilter)BizObj["Related To Work Item"]);
			filter.Property = WorkItemForAssignedAndFixedLog.PK;
			filter.IsActive = true;
			Factory.Save();
			FilterCollection.Load(BizObj.Filter);
			AssertEquals("Issues related to work item", 1, FilterCollection.Count);
			AssertEquals(AssignedAndFixedLog.PK, FilterCollection[0].PK);
		}

		[ExpectNoExceptions()]
		public void TestStatus()
		{
			((ModuleTextFilter)BizObj["Status"]).IsActive = true;
			foreach (CodeDescriptionPair cdp in BizObj.errorLogStatusFilter.StatusList)
			{
				((ModuleTextFilter)BizObj["Status"]).Property = cdp.Code;
				((ModuleTextFilter)BizObj["Status"]).IsActive = true;
				FilterCollection.Load(BizObj.Filter);
			}
		}

		[ExpectNoExceptions()]
		public void TestStatusFilterUnknownStatus()
		{
			((ModuleTextFilter)BizObj["Status"]).Property = "AAA";
			((ModuleTextFilter)BizObj["Status"]).IsActive = true;
			FilterCollection.Load(BizObj.Filter);
		}

		public void TestIgnored()
		{
			((ModuleTextFilter)BizObj["Status"]).Property = ErrorLogStatusFilter.CodeConstants.Ignored;
			((ModuleTextFilter)BizObj["Status"]).IsActive = true;
			AssertFilter(AssignedAndFixedLog);
		}

		public void TestFixed()
		{
			((ModuleTextFilter)BizObj["Status"]).Property = ErrorLogStatusFilter.CodeConstants.Fixed;
			((ModuleTextFilter)BizObj["Status"]).IsActive = true;
			AssertFilter(AssignedAndFixedLog);
		}

		public void TestExceptionKey()
		{
			Factory.Save();
			AssertFilter(null, new EdiHelpErrorLog[3] { UnassignedLog, LogWithKey1, LogWithKey2 });
			((ModuleTextFilter)BizObj["Exception Key"]).Property = "key1";
			((ModuleTextFilter)BizObj["Exception Key"]).IsActive = true;
			AssertFilter(new EdiHelpErrorLog[1] { LogWithKey2 }, new EdiHelpErrorLog[1] { LogWithKey1 });
			((ModuleTextFilter)BizObj["Exception Key"]).Property = "key2";
			AssertFilter(new EdiHelpErrorLog[1] { LogWithKey1 }, new EdiHelpErrorLog[1] { LogWithKey2 });
			((ModuleTextFilter)BizObj["Exception Key"]).Property = "key3";
			AssertFilter(new EdiHelpErrorLog[2] { LogWithKey1, LogWithKey2 }, null);
		}

		public void TestFirstVersionNumber()
		{
			LogWithKey1.HE_FirstVersionNumber = "v1";
			LogWithKey2.HE_FirstVersionNumber = "v2";
			Factory.Save();
			AssertFilter(null, new EdiHelpErrorLog[3] { UnassignedLog, LogWithKey1, LogWithKey2 });
			((ModuleTextFilter)BizObj["First Version Number"]).Property = "v1";
			((ModuleTextFilter)BizObj["First Version Number"]).IsActive = true;
			AssertFilter(new EdiHelpErrorLog[1] { LogWithKey2 }, new EdiHelpErrorLog[1] { LogWithKey1 });
			((ModuleTextFilter)BizObj["First Version Number"]).Property = "v2";
			AssertFilter(new EdiHelpErrorLog[1] { LogWithKey1 }, new EdiHelpErrorLog[1] { LogWithKey2 });
			((ModuleTextFilter)BizObj["First Version Number"]).Property = "v3";
			AssertFilter(new EdiHelpErrorLog[2] { LogWithKey1, LogWithKey2 }, null);
		}

		public void TestLastVersionNumber()
		{
			LogWithKey1.HE_LastVersionNumber = "v1";
			LogWithKey2.HE_LastVersionNumber = "v2";
			Factory.Save();
			AssertFilter(null, new EdiHelpErrorLog[3] { UnassignedLog, LogWithKey1, LogWithKey2 });
			((ModuleTextFilter)BizObj["Last Version Number"]).Property = "v1";
			((ModuleTextFilter)BizObj["Last Version Number"]).IsActive = true;
			AssertFilter(new EdiHelpErrorLog[1] { LogWithKey2 }, new EdiHelpErrorLog[1] { LogWithKey1 });
			((ModuleTextFilter)BizObj["Last Version Number"]).Property = "v2";
			AssertFilter(new EdiHelpErrorLog[1] { LogWithKey1 }, new EdiHelpErrorLog[1] { LogWithKey2 });
			((ModuleTextFilter)BizObj["Last Version Number"]).Property = "v3";
			AssertFilter(new EdiHelpErrorLog[2] { LogWithKey1, LogWithKey2 }, null);
		}

		public void TestCompanyName()
		{
			((ModuleTextFilter)BizObj["Status"]).IsActive = false;
			((ModuleTextFilter)BizObj["Client Name"]).Property = "";
			((ModuleTextFilter)BizObj["Client Name"]).IsActive = true;
			FilterCollection.Load(BizObj.Filter);
			AssertEquals("Unfiltered Count", 3, FilterCollection.Count);
			HelpErrorLogOccurrence occurrence1 = UnassignedLog.Occurrences.AddNew();
			occurrence1.HO_Company = "Test1Company";
			HelpErrorLogOccurrence occurrence2 = UnassignedLog.Occurrences.AddNew();
			occurrence2.HO_Company = "Test2Company";
			HelpErrorLogOccurrence occurrence3 = AssignedNotFixedLog.Occurrences.AddNew();
			occurrence3.HO_Company = "Test1Company";
			Factory.Save(); // subquery to search occurrences is a db-only query
			((ModuleTextFilter)BizObj["Client Name"]).Property = "Test1Company";
			((ModuleTextFilter)BizObj["Client Name"]).IsActive = true;
			FilterCollection.Load(BizObj.Filter);
			AssertEquals("Filtered Count", 2, FilterCollection.Count);
			Assert("Filtered Collection should contain correct BizObj", FilterCollection.Contains(occurrence1.HO_HE));
			Assert("Filtered Collection should contain correct BizObj", FilterCollection.Contains(occurrence3.HO_HE));
			((ModuleTextFilter)BizObj["Client Name"]).Property = "Test2Company";
			((ModuleTextFilter)BizObj["Client Name"]).IsActive = true;
			FilterCollection.Load(BizObj.Filter);
			AssertEquals("Filtered Count", 1, FilterCollection.Count);
			Assert("Filtered Collection should contain correct BizObj", FilterCollection.Contains(occurrence2.HO_HE));
			((ModuleTextFilter)BizObj["Client Name"]).Property = "Test3Company";
			((ModuleTextFilter)BizObj["Client Name"]).IsActive = true;
			FilterCollection.Load(BizObj.Filter);
			AssertEquals("Filtered Count", 0, FilterCollection.Count);
		}

		public void TestOccurrencesWithoutDateSet()
		{
			ModuleDateFilter occurrencesFilter = (ModuleDateFilter)BizObj["Occurrences Date"];
			occurrencesFilter.IsActive = true;
			occurrencesFilter.PropertySearch = "Date range";
			AssertNotNull("Should not cause exception.", BizObj.Filter);
			occurrencesFilter.Property1 = new ZDateTime(2005, 09, 8);
			AssertNotNull("Should not cause exception.", BizObj.Filter);
			occurrencesFilter.Property1 = ZDateTime.Empty;
			occurrencesFilter.Property2 = new ZDateTime(2005, 09, 11);
			AssertNotNull("Should not cause exception.", BizObj.Filter);
		}

		public void TestOccurrenceDate()
		{
			HelpErrorLogOccurrence occurrence1 = UnassignedLog.Occurrences.AddNew();
			occurrence1.HO_ExceptionDateTime = new ZDateTime(2005, 09, 10);
			HelpErrorLogOccurrence occurrence2 = AssignedAndFixedLog.Occurrences.AddNew();
			occurrence2.HO_ExceptionDateTime = new ZDateTime(2005, 09, 9);
			HelpErrorLogOccurrence occurrence3 = AssignedNotFixedLog.Occurrences.AddNew();
			occurrence3.HO_ExceptionDateTime = new ZDateTime(2005, 09, 1);
			HelpErrorLogOccurrence occurrence4 = AssignedNotFixedLog.Occurrences.AddNew();
			occurrence4.HO_ExceptionDateTime = new ZDateTime(2005, 09, 12);
			Factory.Save();
			ModuleDateFilter occurrencesFilter = (ModuleDateFilter)BizObj["Occurrences Date"];
			occurrencesFilter.IsActive = true;
			occurrencesFilter.PropertySearch = "Date range";
			occurrencesFilter.Property1 = new ZDateTime(2005, 09, 8);
			occurrencesFilter.Property2 = new ZDateTime(2005, 09, 11);
			ZQuery filter = BizObj.Filter;
			AssertFilter("This log has an occurrence with HO_ExceptionDateTime greater than DateFrom and an occurrence with HO_ExceptionDateTime less than DateTo, but nothing that falls in the range.", new EdiHelpErrorLog[] { AssignedNotFixedLog }, new EdiHelpErrorLog[] { UnassignedLog, AssignedAndFixedLog });
			occurrencesFilter.Property1 = new ZDateTime(2005, 09, 9);
			occurrencesFilter.Property2 = new ZDateTime(2005, 09, 10);
			AssertFilter(new EdiHelpErrorLog[] { AssignedNotFixedLog }, new EdiHelpErrorLog[] { UnassignedLog, AssignedAndFixedLog });
			occurrencesFilter.Property1 = new ZDateTime(2005, 09, 10);
			occurrencesFilter.Property2 = new ZDateTime(2005, 09, 10);
			AssertFilter(new EdiHelpErrorLog[] { AssignedNotFixedLog, AssignedAndFixedLog }, new EdiHelpErrorLog[] { UnassignedLog });
			occurrence4.HO_ExceptionDateTime = new ZDateTime(2005, 09, 10);
			Factory.Save();
			occurrencesFilter.Property1 = new ZDateTime(2005, 09, 10);
			occurrencesFilter.Property2 = new ZDateTime(2005, 09, 10);
			AssertFilter(new EdiHelpErrorLog[] { AssignedAndFixedLog }, new EdiHelpErrorLog[] { UnassignedLog, AssignedNotFixedLog });
		}

		public void TestSessionId()
		{
			var sessionId1 = Guid.NewGuid();
			var sessionId2 = Guid.NewGuid();
			HelpErrorLogOccurrence occurrence1 = UnassignedLog.Occurrences.AddNew();
			occurrence1.HO_ExceptionDateTime = new ZDateTime(2005, 09, 10);
			occurrence1.HO_SessionID = sessionId1;
			HelpErrorLogOccurrence occurrence2 = AssignedAndFixedLog.Occurrences.AddNew();
			occurrence2.HO_ExceptionDateTime = new ZDateTime(2005, 09, 9);
			occurrence2.HO_SessionID = sessionId1;
			HelpErrorLogOccurrence occurrence3 = AssignedNotFixedLog.Occurrences.AddNew();
			occurrence3.HO_ExceptionDateTime = new ZDateTime(2005, 09, 1);
			occurrence3.HO_SessionID = sessionId2;
			HelpErrorLogOccurrence occurrence4 = AssignedNotFixedLog.Occurrences.AddNew();
			occurrence4.HO_ExceptionDateTime = new ZDateTime(2005, 09, 12);
			Factory.Save();
			var occurrencesFilter = (GuidTextFilter)BizObj["Session ID"];
			occurrencesFilter.IsActive = true;
			occurrencesFilter.Property = sessionId1.ToString();
			ZQuery filter = BizObj.Filter;
			AssertFilter(new EdiHelpErrorLog[] { AssignedNotFixedLog }, new EdiHelpErrorLog[] { UnassignedLog, AssignedAndFixedLog });
			occurrencesFilter.Property = sessionId2.ToString();
			AssertFilter(new EdiHelpErrorLog[] { UnassignedLog, AssignedAndFixedLog }, new EdiHelpErrorLog[] { AssignedNotFixedLog });
			occurrencesFilter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			AssertFilter(new EdiHelpErrorLog[] { UnassignedLog, AssignedAndFixedLog }, new EdiHelpErrorLog[] { AssignedNotFixedLog });
			occurrencesFilter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			AssertFilter(Array.Empty<EdiHelpErrorLog>(), new EdiHelpErrorLog[] { AssignedNotFixedLog, UnassignedLog, AssignedAndFixedLog });
		}

		public void TestOccurrencesCombinedFilter()
		{
			HelpErrorLogOccurrence occurrence1 = UnassignedLog.Occurrences.AddNew();
			occurrence1.HO_ExceptionDateTime = new ZDateTime(2005, 09, 10);
			occurrence1.HO_Company = "Test1Company";
			occurrence1.HO_ExceptionID = "E00000001";
			HelpErrorLogOccurrence occurrence2 = AssignedAndFixedLog.Occurrences.AddNew();
			occurrence2.HO_ExceptionDateTime = new ZDateTime(2005, 09, 9);
			occurrence2.HO_Company = "Test2Company";
			occurrence2.HO_ExceptionID = "E00000001";
			HelpErrorLogOccurrence occurrence3 = AssignedNotFixedLog.Occurrences.AddNew();
			occurrence3.HO_ExceptionDateTime = new ZDateTime(2005, 09, 1);
			occurrence3.HO_Company = "Test1Company";
			occurrence3.HO_ExceptionID = "E00000200";
			Factory.Save();
			ModuleTextFilter clientNameFilter = (ModuleTextFilter)BizObj["Client Name"];
			clientNameFilter.IsActive = true;
			clientNameFilter.Property = "Test1Company";
			ModuleDateFilter occurrencesFilter = (ModuleDateFilter)BizObj["Occurrences Date"];
			occurrencesFilter.IsActive = true;
			occurrencesFilter.PropertySearch = "Date range";
			occurrencesFilter.Property1 = new ZDateTime(2005, 09, 8);
			occurrencesFilter.Property2 = new ZDateTime(2005, 09, 11);
			AssertFilter(new EdiHelpErrorLog[] { AssignedNotFixedLog, AssignedAndFixedLog }, new EdiHelpErrorLog[] { UnassignedLog });
			ModuleTextFilter exceptionIDFilter = (ModuleTextFilter)BizObj["Exception ID"];
			exceptionIDFilter.IsActive = true;
			exceptionIDFilter.Property = "E00000001";
			AssertFilter(new EdiHelpErrorLog[] { AssignedNotFixedLog, AssignedAndFixedLog }, new EdiHelpErrorLog[] { UnassignedLog });
			exceptionIDFilter.Property = "E00000200";
			AssertFilter();
		}

		public void TestOccurrencesClientOccurrenceDateCombinedFilter()
		{
			HelpErrorLogOccurrence occurrence1 = UnassignedLog.Occurrences.AddNew();
			occurrence1.HO_ExceptionDateTime = new ZDateTime(2005, 09, 10);
			occurrence1.HO_Company = "Test1Company";
			occurrence1.HO_ExceptionID = "E00000001";
			HelpErrorLogOccurrence occurrence2 = UnassignedLog.Occurrences.AddNew();
			occurrence2.HO_ExceptionDateTime = new ZDateTime(2005, 09, 1);
			occurrence2.HO_Company = "Test2Company";
			occurrence2.HO_ExceptionID = "E00000001";
			Factory.Save();
			ModuleTextFilter clientNameFilter = (ModuleTextFilter)BizObj["Client Name"];
			clientNameFilter.IsActive = true;
			clientNameFilter.Property = "Test1Company";
			ModuleDateFilter occurrencesFilter = (ModuleDateFilter)BizObj["Occurrences Date"];
			occurrencesFilter.IsActive = true;
			occurrencesFilter.PropertySearch = "Date range";
			occurrencesFilter.Property1 = new ZDateTime(2005, 09, 9);
			occurrencesFilter.Property2 = new ZDateTime(2005, 09, 11);
			AssertFilter(UnassignedLog);
			occurrencesFilter.Property1 = new ZDateTime(2005, 08, 10);
			occurrencesFilter.Property2 = new ZDateTime(2005, 09, 3);
			AssertFilter();
		}

		public void TestOccurrenceClientNameAndExceptionIDCombinedFilters()
		{
			EdiHelpErrorLog errorLog1 = Factory.New<EdiHelpErrorLog>();
			HelpErrorLogOccurrence occurrence1 = errorLog1.Occurrences.AddNew();
			occurrence1.HO_ExceptionDateTime = new ZDateTime(2005, 09, 10);
			occurrence1.HO_Company = "Test1Company";
			occurrence1.HO_ExceptionID = "E00000001";
			HelpErrorLogOccurrence occurrence2 = errorLog1.Occurrences.AddNew();
			occurrence2.HO_ExceptionDateTime = new ZDateTime(2005, 09, 10);
			occurrence2.HO_Company = "Test2Company";
			occurrence2.HO_ExceptionID = "E00000002";
			EdiHelpErrorLog errorLog2 = Factory.New<EdiHelpErrorLog>();
			HelpErrorLogOccurrence occurrence3 = errorLog2.Occurrences.AddNew();
			occurrence3.HO_ExceptionDateTime = new ZDateTime(2005, 09, 1);
			occurrence3.HO_Company = "Test2Company";
			occurrence3.HO_ExceptionID = "E00000001";
			HelpErrorLogOccurrence occurrence4 = errorLog1.Occurrences.AddNew();
			occurrence4.HO_ExceptionDateTime = new ZDateTime(2005, 09, 10);
			occurrence4.HO_Company = "Test1Company";
			occurrence4.HO_ExceptionID = "E00000002";
			Factory.Save();
			IssueManagerFilterBusinessObject filterBizO = new IssueManagerFilterBusinessObject();
			ModuleTextFilter exceptionIDFilter = (ModuleTextFilter)filterBizO["Exception ID"];
			exceptionIDFilter.IsActive = true;
			exceptionIDFilter.Property = "E00000001";
			ModuleTextFilter clientNameFilter = (ModuleTextFilter)filterBizO["Client Name"];
			clientNameFilter.IsActive = true;
			clientNameFilter.Property = "Test1Company";
			EdiHelpErrorLog[] errorLogs = Factory.Load<EdiHelpErrorLog>(filterBizO.Filter);
			AssertEquals(1, errorLogs.Length);
			AssertEquals(errorLog1, errorLogs[0]);
			clientNameFilter.Property = "Test2Company";
			errorLogs = Factory.Load<EdiHelpErrorLog>(filterBizO.Filter);
			AssertEquals(1, errorLogs.Length);
			AssertEquals(errorLog2, errorLogs[0]);
		}

		public void TestExeDateFilters()
		{
			var error1 = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			error1.HE_FirstEXEVersionDate = new ZDateTime(2009, 12, 31);
			error1.HE_LastEXEVersionDate = new ZDateTime(2010, 01, 01);
			var error2 = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			error2.HE_FirstEXEVersionDate = new ZDateTime(2010, 12, 31);
			error2.HE_LastEXEVersionDate = new ZDateTime(2011, 01, 01);
			var error3 = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			error3.HE_FirstEXEVersionDate = new ZDateTime(2011, 12, 31);
			error3.HE_LastEXEVersionDate = new ZDateTime(2012, 01, 01);
			var error4 = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			error4.HE_FirstEXEVersionDate = new ZDateTime(2012, 12, 31);
			error4.HE_LastEXEVersionDate = new ZDateTime(2013, 01, 01);
			Factory.Save();
			var latestFilter = (ModuleDateFilter)BizObj["Latest EXE"];
			latestFilter.IsActive = true;
			latestFilter.PropertySearch = "Date range";
			latestFilter.Property1 = new ZDateTime(2012, 01, 02);
			latestFilter.Property2 = new ZDateTime(2013, 02, 02);
			var filterCollection = new HelpErrorLogCollection(Factory);
			filterCollection.Load(BizObj.Filter);
			AssertFilter(new EdiHelpErrorLog[] { error1, error2, error3 }, new EdiHelpErrorLog[] { error4 });
			latestFilter.IsActive = true;
			var firstFilter = (ModuleDateFilter)BizObj["First EXE"];
			firstFilter.IsActive = true;
			firstFilter.PropertySearch = "Date range";
			firstFilter.Property1 = new ZDateTime(2012, 01, 01);
			firstFilter.Property2 = new ZDateTime(2013, 01, 01);
			filterCollection = new HelpErrorLogCollection(Factory);
			filterCollection.Load(BizObj.Filter);
			AssertFilter(new EdiHelpErrorLog[] { error1, error2, error3 }, new EdiHelpErrorLog[] { error4 });
		}

		[TestUtcOffset(08, 00, 00)]
		public void TestExeDateFilters_SearchByLocalTimeRange()
		{
			var log1 = CreateErrorLogWithAnSimpleOccurrence(new ZDateTime(2023, 12, 31, 12, 00, 00, DateTimeKind.Utc));
			var log2 = CreateErrorLogWithAnSimpleOccurrence(new ZDateTime(2023, 12, 31, 20, 00, 00, DateTimeKind.Utc));
			var log3 = CreateErrorLogWithAnSimpleOccurrence(new ZDateTime(2024, 01, 01, 04, 00, 00, DateTimeKind.Utc));
			Factory.Save();

			var fromDate = new ZDateTime(2024, 01, 01, 00, 00, 00, DateTimeKind.Local);
			var toDate = new ZDateTime(2024, 01, 01, 10, 00, 00, DateTimeKind.Local);

			var latestEXEFilter = (ModuleDateFilter)BizObj["Latest EXE"];
			latestEXEFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			latestEXEFilter.Property1 = fromDate;
			latestEXEFilter.Property2 = toDate;
			latestEXEFilter.IsActive = true;
			AssertFilter(new[] { log1, log3 }, new[] { log2 });

			latestEXEFilter.IsActive = false;
			var firstEXEFilter = (ModuleDateFilter)BizObj["First EXE"];
			firstEXEFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			firstEXEFilter.Property1 = fromDate;
			firstEXEFilter.Property2 = toDate;
			firstEXEFilter.IsActive = true;
			AssertFilter(new[] { log1, log3 }, new[] { log2 });
		}

		EdiHelpErrorLog CreateErrorLogWithAnSimpleOccurrence(ZDateTime exeDateTime)
		{
			var log = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			var occurrence = log.Occurrences.AddNew();
			occurrence.HO_EXEDateTime = exeDateTime;
			return log;
		}

		public void TestServerName()
		{
			EdiHelpErrorLog error1 = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			HelpErrorLogOccurrence occurrence1 = error1.Occurrences.AddNew();
			occurrence1.HO_ServerName = "apotsh.db.wisegrid.net";
			EdiHelpErrorLog error2 = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			HelpErrorLogOccurrence occurrence2 = error2.Occurrences.AddNew();
			occurrence2.HO_ServerName = "syddat.db.wtg.zone";
			EdiHelpErrorLog error3 = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			HelpErrorLogOccurrence occurrence3 = error3.Occurrences.AddNew();
			occurrence3.HO_ServerName = ZString.Empty;
			Factory.Save();
			ModuleTextFilter serverNameFilter = (ModuleTextFilter)BizObj["Server Name"];
			serverNameFilter.IsActive = true;
			serverNameFilter.Property = "apotsh";
			serverNameFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			HelpErrorLogCollection collection = new HelpErrorLogCollection(Factory);
			collection.Load(BizObj.Filter);
			AssertFilter(new EdiHelpErrorLog[] { error2, error3 }, new EdiHelpErrorLog[] { error1 });
			serverNameFilter.Property = "db";
			serverNameFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			collection.Load(BizObj.Filter);
			AssertFilter(new EdiHelpErrorLog[] { error3 }, new EdiHelpErrorLog[] { error1, error2 });
			serverNameFilter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			collection.Load(BizObj.Filter);
			AssertFilter(new EdiHelpErrorLog[] { error1, error2 }, new EdiHelpErrorLog[] { error3 });
		}

		public void TestFixedCount()
		{
			EdiHelpErrorLog error1 = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			error1.HE_FixedCount = 13;
			EdiHelpErrorLog error2 = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			error2.HE_FixedCount = 20;
			EdiHelpErrorLog error3 = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			error3.HE_FixedCount = 20;
			EdiHelpErrorLog error4 = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			error4.HE_FixedCount = 16;
			Factory.Save();
			ModuleNumberRangeFilter fixedCountFilter = (ModuleNumberRangeFilter)BizObj["Fixed Count"];
			fixedCountFilter.IsActive = true;
			((ModuleNumberRangeFilter)BizObj["Fixed Count"]).Property1 = 12;
			((ModuleNumberRangeFilter)BizObj["Fixed Count"]).Property2 = 19;
			HelpErrorLogCollection filterCollection = new HelpErrorLogCollection(Factory);
			filterCollection.Load(BizObj.Filter);
			AssertFilter(new EdiHelpErrorLog[] { error3, error2 }, new EdiHelpErrorLog[] { error1, error4 });
		}

		public void TestFailCount()
		{
			EdiHelpErrorLog error1 = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			error1.HE_FailCount = 13;
			EdiHelpErrorLog error2 = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			error2.HE_FailCount = 2000;
			EdiHelpErrorLog error3 = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			error3.HE_FailCount = 300000;
			EdiHelpErrorLog error4 = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			error4.HE_FailCount = 300;
			Factory.Save();
			ModuleNumberRangeFilter failCountFilter = (ModuleNumberRangeFilter)BizObj["Fail Count"];
			failCountFilter.IsActive = true;
			((ModuleNumberRangeFilter)BizObj["Fail Count"]).Property1 = 1500;
			((ModuleNumberRangeFilter)BizObj["Fail Count"]).Property2 = 400000;
			HelpErrorLogCollection filterCollection = new HelpErrorLogCollection(Factory);
			filterCollection.Load(BizObj.Filter);
			AssertFilter(new EdiHelpErrorLog[] { error1, error4 }, new EdiHelpErrorLog[] { error2, error3 });
		}

		LicenceHeader GetLicHeader(string orgName, string databaseCode, string enterpriseCode)
		{
			EDIOrgHeader org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_FullName = orgName;
			org.CreateAndLoadLicenceForOrg();
			LicenceCompany company = org.LicCompany;
			return AddDatabases(company, databaseCode, enterpriseCode);
		}

		LicenceHeader AddDatabases(LicenceCompany company, string databaseCode, string enterpriseCode)
		{
			LicenceDatabase database = company.LicDatabases.AddNew();
			database.LD_ServerCode = databaseCode;
			company.LicEnterprise.LE_EnterpriseCode = enterpriseCode;
			return company.GetHeader(database);
		}

		public void TestDatabaseServerCode()
		{
			FilterStripBusinessObject filterStripBizO = GetNewFilterStripBusinessObject();
			EdiHelpErrorLog log1 = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			HelpErrorLogOccurrence occurrence1 = log1.Occurrences.AddNew();
			HelpErrorLogOccurrence occurrence2 = log1.Occurrences.AddNew();
			LicenceHeader lic1 = GetLicHeader("Org 1", "111", "L01");
			LicenceHeader lic2 = GetLicHeader("Org 2", "222", "L02");
			occurrence1.HO_LD = lic1.LA_LD;
			occurrence2.HO_LD = lic2.LA_LD;
			Factory.Save();
			EdiHelpErrorLog log2 = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			HelpErrorLogOccurrence occurrenceA = log2.Occurrences.AddNew();
			HelpErrorLogOccurrence occurrenceB = log2.Occurrences.AddNew();
			LicenceHeader lic5 = GetLicHeader("Org 5", "LD5", "LI5");
			LicenceHeader lic6 = GetLicHeader("Org 6", "LD6", "LI6");
			occurrenceA.HO_LD = lic5.LA_LD;
			occurrenceB.HO_LD = lic6.LA_LD;
			Factory.Save();
			EdiHelpErrorLog logZ = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			HelpErrorLogOccurrence occurrenceZ = logZ.Occurrences.AddNew();
			LicenceHeader licZ = GetLicHeader("Org Z", "ZZZ", "ZZZ");
			occurrenceZ.HO_LD = licZ.LA_LD;
			Factory.Save();
			BusinessObjectFactory clearFactory = new BusinessObjectFactory();
			ModuleTextFilter filter = (ModuleTextFilter)BizObj["Database Server Code"];
			filter.Property = "111";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			HelpErrorLogCollection logCollection = new HelpErrorLogCollection(clearFactory);
			logCollection.Load(BizObj.Filter);
			AssertFilter(new EdiHelpErrorLog[] { log2 }, new EdiHelpErrorLog[] { log1 });
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "LD";
			logCollection.Load(BizObj.Filter);
			AssertFilter(new EdiHelpErrorLog[] { log1 }, new EdiHelpErrorLog[] { log2 });
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "2";
			logCollection.Load(BizObj.Filter);
			AssertFilter(new EdiHelpErrorLog[] { log2 }, new EdiHelpErrorLog[] { log1 });
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "Y";
			logCollection.Load(BizObj.Filter);
			AssertFilter(new EdiHelpErrorLog[] { log1, log2 }, Array.Empty<EdiHelpErrorLog>());
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = "111";
			logCollection.Load(BizObj.Filter);
			AssertFilter(Array.Empty<EdiHelpErrorLog>(), new EdiHelpErrorLog[] { log1, log2, logZ });
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = "ZZZ";
			logCollection.Load(BizObj.Filter);
			AssertFilter(new EdiHelpErrorLog[] { logZ }, new EdiHelpErrorLog[] { log1, log2 });
			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			filter.Property = "L";
			logCollection.Load(BizObj.Filter);
			AssertFilter(new EdiHelpErrorLog[] { log2 }, new EdiHelpErrorLog[] { log1, logZ });
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.Property = "L";
			logCollection.Load(BizObj.Filter);
			AssertFilter(new EdiHelpErrorLog[] { log2 }, new EdiHelpErrorLog[] { log1, logZ });
		}

		[StressTest]
		public void TestEnterpriseCodeFilter()
		{
			EdiHelpErrorLog log1 = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			HelpErrorLogOccurrence occurrence1 = log1.Occurrences.AddNew();
			HelpErrorLogOccurrence occurrence2 = log1.Occurrences.AddNew();
			LicenceHeader lic1 = GetLicHeader("Org 1", "111", "L01");
			LicenceHeader lic2 = GetLicHeader("Org 2", "222", "L02");
			occurrence1.HO_LD = lic1.LA_LD;
			occurrence2.HO_LD = lic2.LA_LD;
			Factory.Save();
			EdiHelpErrorLog log2 = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			HelpErrorLogOccurrence occurrenceA = log2.Occurrences.AddNew();
			HelpErrorLogOccurrence occurrenceB = log2.Occurrences.AddNew();
			LicenceHeader lic5 = GetLicHeader("Org 5", "LD5", "LI5");
			LicenceHeader lic6 = GetLicHeader("Org 6", "LD6", "LI6");
			occurrenceA.HO_LD = lic5.LA_LD;
			occurrenceB.HO_LD = lic6.LA_LD;
			Factory.Save();
			EdiHelpErrorLog log3 = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			HelpErrorLogOccurrence occurrenceZ = log3.Occurrences.AddNew();
			LicenceHeader licZ = GetLicHeader("Org Z", "ZZZ", "ZZZ");
			occurrenceZ.HO_LD = licZ.LA_LD;
			Factory.Save();
			ModuleGuidFilter filter = (ModuleGuidFilter)BizObj["Enterprise Code"];
			filter.Property = lic1.Company.LicEnterprise.PK;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			BusinessObjectFactory clearFactory = new BusinessObjectFactory();
			HelpErrorLogCollection logCollection = new HelpErrorLogCollection(clearFactory);
			logCollection.Load(BizObj.Filter);
			AssertFilter(new EdiHelpErrorLog[] { log2, log3 }, new EdiHelpErrorLog[] { log1 });
			filter.Property = lic6.Company.LicEnterprise.PK;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			logCollection.Load(BizObj.Filter);
			AssertFilter(Array.Empty<EdiHelpErrorLog>(), new EdiHelpErrorLog[] { log1, log2, log3 });
			filter.Property = licZ.Company.LicEnterprise.PK;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			logCollection.Load(BizObj.Filter);
			AssertFilter(new EdiHelpErrorLog[] { log3 }, new EdiHelpErrorLog[] { log1, log2 });
			//Test Combined with Database Server Code
			filter.Property = lic1.Company.LicEnterprise.PK;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			ModuleTextFilter serverCodeFilter = (ModuleTextFilter)BizObj["Database Server Code"];
			serverCodeFilter.Property = "111";
			serverCodeFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			serverCodeFilter.IsActive = true;
			logCollection.Load(BizObj.Filter);
			Assert(logCollection.Contains(log1));
			Assert(!logCollection.Contains(log2));
			filter.Property = lic1.Company.LicEnterprise.PK;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			serverCodeFilter.Property = "LD5";
			serverCodeFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			serverCodeFilter.IsActive = true;
			logCollection.Load(BizObj.Filter);
			Assert(!logCollection.Contains(log1));
			Assert(!logCollection.Contains(log2));
		}

		[StressTest]
		public void TestEnterpriseIDFilter()
		{
			EdiHelpErrorLog log1 = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			HelpErrorLogOccurrence occurrence1 = log1.Occurrences.AddNew();
			HelpErrorLogOccurrence occurrence2 = log1.Occurrences.AddNew();
			LicenceHeader lic1 = GetLicHeader("Org 1", "111", "L01");
			LicenceHeader lic2 = GetLicHeader("Org 2", "222", "L02");
			occurrence1.HO_LD = lic1.LA_LD;
			occurrence2.HO_LD = lic2.LA_LD;
			Factory.Save();
			EdiHelpErrorLog log2 = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			HelpErrorLogOccurrence occurrenceA = log2.Occurrences.AddNew();
			HelpErrorLogOccurrence occurrenceB = log2.Occurrences.AddNew();
			LicenceHeader lic5 = GetLicHeader("Org 5", "LD5", "LI5");
			LicenceHeader lic6 = GetLicHeader("Org 6", "LD6", "LI6");
			occurrenceA.HO_LD = lic5.LA_LD;
			occurrenceB.HO_LD = lic6.LA_LD;
			Factory.Save();
			EdiHelpErrorLog log3 = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			HelpErrorLogOccurrence occurrenceZ = log3.Occurrences.AddNew();
			LicenceHeader licZ = GetLicHeader("Org Z", "ZZZ", "ZZZ");
			occurrenceZ.HO_LD = licZ.LA_LD;
			Factory.Save();
			ModuleGuidFilter filter = (ModuleGuidFilter)BizObj["Enterprise ID"];
			filter.Property = lic1.Company.LicEnterprise.PK;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			BusinessObjectFactory clearFactory = new BusinessObjectFactory();
			HelpErrorLogCollection logCollection = new HelpErrorLogCollection(clearFactory);
			logCollection.Load(BizObj.Filter);
			AssertFilter(new EdiHelpErrorLog[] { log2, log3 }, new EdiHelpErrorLog[] { log1 });
			filter.Property = lic6.Company.LicEnterprise.PK;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			logCollection.Load(BizObj.Filter);
			AssertFilter(Array.Empty<EdiHelpErrorLog>(), new EdiHelpErrorLog[] { log1, log2, log3 });
			filter.Property = licZ.Company.LicEnterprise.PK;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			logCollection.Load(BizObj.Filter);
			AssertFilter(new EdiHelpErrorLog[] { log3 }, new EdiHelpErrorLog[] { log1, log2 });
			//Test Combined with Database Server Code
			filter.Property = lic1.Company.LicEnterprise.PK;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			ModuleTextFilter serverCodeFilter = (ModuleTextFilter)BizObj["Database Server Code"];
			serverCodeFilter.Property = "111";
			serverCodeFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			serverCodeFilter.IsActive = true;
			logCollection.Load(BizObj.Filter);
			Assert(logCollection.Contains(log1));
			Assert(!logCollection.Contains(log2));
			filter.Property = lic1.Company.LicEnterprise.PK;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			serverCodeFilter.Property = "LD5";
			serverCodeFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			serverCodeFilter.IsActive = true;
			logCollection.Load(BizObj.Filter);
			Assert(!logCollection.Contains(log1));
			Assert(!logCollection.Contains(log2));
		}

		#region Implementation
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new IssueManagerFilterBusinessObject();
		}

		void AssertFilter(string notExpectedErrorMessage, EdiHelpErrorLog[] notExpectedLogs, EdiHelpErrorLog[] expectedLogs)
		{
			FilterCollection.Load(BizObj.Filter);
			if (notExpectedLogs != null)
			{
				foreach (EdiHelpErrorLog log in notExpectedLogs)
				{
					AssertCollectionNotContains(notExpectedErrorMessage, log, FilterCollection);
				}
			}

			if (expectedLogs != null)
			{
				foreach (EdiHelpErrorLog log in expectedLogs)
				{
					AssertCollectionContains(log, FilterCollection);
				}

				AssertEquals(expectedLogs.Length, FilterCollection.Count);
			}
			else
			{
				AssertEquals(0, FilterCollection.Count);
			}
		}

		void AssertFilter(EdiHelpErrorLog[] notExpectedLogs, EdiHelpErrorLog[] expectedLogs)
		{
			AssertFilter(null, notExpectedLogs, expectedLogs);
		}

		void AssertFilter(params EdiHelpErrorLog[] expectedLogs)
		{
			AssertFilter(null, Array.Empty<EdiHelpErrorLog>(), expectedLogs);
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbBranch.CurrentBranch.HomePort.TimeZoneSet.StandardZone.R2_OffsetMinutesFromUTC = 480;
			TestCaseHelper.ClearTable(IncidentMainSchema.Constants.TableName);
			TestCaseHelper.ClearTable(HelpErrorLogOccurrence.Schema.TableName);
			TestCaseHelper.ClearTable(HelpErrorLogKey.Schema.TableName);
			TestCaseHelper.ClearTable(EdiHelpErrorLog.Schema.TableName);
			UnassignedLog = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			UnassignedLog.HE_FirstReported = ZDateTime.UtcNow.AddHours(-1);
			AssignedNotFixedLog = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			AssignedNotFixedLog.Keys.AddNew().HK_Key = "key1";
			LogWithKey1 = AssignedNotFixedLog;
			AssignedAndFixedLog = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			AssignedAndFixedLog.Keys.AddNew().HK_Key = "key2";
			LogWithKey2 = AssignedAndFixedLog;
			WorkItemForAssignedAndFixedLog = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemForAssignedAndFixedLog.RelatedItems.Add(AssignedAndFixedLog);
			AssignedAndFixedLog.HE_FixedDate = ZDateTime.UtcNow.AddDays(-2); // normally, all related work items should be closed for Fixed Date to be populated
			BizObj = new IssueManagerFilterBusinessObject();
			FilterCollection = new HelpErrorLogCollection(Factory);
		}

		EdiHelpErrorLog UnassignedLog;
		EdiHelpErrorLog AssignedNotFixedLog;
		EdiHelpErrorLog AssignedAndFixedLog;
		EdiHelpErrorLog LogWithKey1;
		EdiHelpErrorLog LogWithKey2;
		NewWorkItem WorkItemForAssignedAndFixedLog;
		IssueManagerFilterBusinessObject BizObj;
		HelpErrorLogCollection FilterCollection;
		#endregion
	}
}
