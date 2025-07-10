using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(GLJournalFilterBusinessObject))]
	public class GLJournalFilterBusinessObjectTest : AccountingFilterStripBusinessObjectTestCase
	{
		public void TestJournalNumberFilter()
		{
			Journal1.IsManuallySetTransactionNumber_ForTestOnly = true;
			Journal2.IsManuallySetTransactionNumber_ForTestOnly = true;

			Journal1.AH_TransactionNum = "11100011";
			Journal2.AH_TransactionNum = "22220002";

			Factory.Save();

			FilterCollection = new GLJournalCollection(Factory);
			FilterBO = (GLJournalFilterBusinessObject)GetNewFilterStripBusinessObject();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO["Journal Number"];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Journal1", FilterCollection.Contains(Journal1));
			Assert("Expecting collection not to contain Journal2", !FilterCollection.Contains(Journal2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22220002";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Journal1", !FilterCollection.Contains(Journal1));
			Assert("Expecting collection to contain Journal2", FilterCollection.Contains(Journal2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Journal1", FilterCollection.Contains(Journal1));
			Assert("Expecting collection to contain Journal2", FilterCollection.Contains(Journal2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Journal1", !FilterCollection.Contains(Journal1));
			Assert("Expecting collection not to contain Journal2", !FilterCollection.Contains(Journal2));
		}

		public void TestJournalTypeFilter()
		{
			Journal1.AH_TransactionType = TransactionTypes.GLAutoJournal;
			Journal1.AH_DueDate = ZDateTime.Today;
			Journal2.AH_TransactionType = TransactionTypes.GLStandardJournal;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterBO["Journal Type"];

			filter.Property = TransactionTypes.GLAutoJournal;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Journal1", FilterCollection.Contains(Journal1));
			Assert("Expecting collection not to contain Journal2", !FilterCollection.Contains(Journal2));

			filter.Property = TransactionTypes.GLStandardJournal;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Journal1", !FilterCollection.Contains(Journal1));
			Assert("Expecting collection to contain Journal2", FilterCollection.Contains(Journal2));

			filter.Property = TransactionTypes.WIPAccrualJournal;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Journal1", !FilterCollection.Contains(Journal1));
			Assert("Expecting collection not to contain Journal2", !FilterCollection.Contains(Journal2));
		}

		public void TestPostPeriodFilter()
		{
			TestCaseHelper.ClearTable(AccPeriodManagement.Schema.TableName);
			var startDateOfFinancialYear = new ZDateTime(ZDateTime.Now.Year, 1, 1);
			var periodManager = TestObjectCreator.CreateTestPeriods(startDateOfFinancialYear);

			Factory.Save();

			Assert("Expecting collection to contain items", periodManager.Periods.Count > 0);
			var period = periodManager.Periods[0];

			Journal1.AH_PostDate = period.AM_StartDate.AddMinutes(-1);
			Journal2.AH_PostDate = period.AM_StartDate;
			Journal3.AH_PostDate = period.AM_EndDate;
			Journal4.AH_PostDate = period.AM_EndDate.AddMinutes(1);

			Factory.Save();

			ModulePeriodFilter filter = (ModulePeriodFilter)FilterBO["Post Period"];

			filter.Property = period.AM_Period.ToString();
			filter.IsActive = true;

			AssertEquals("Should only be one comparison operator on this filter", 1, filter.ComparisonOperator_List.Count);
			AssertEquals("The only comparison operator should be Exact", ModuleNumberFilter.ComparisonConstants.Exact, filter.ComparisonOperator_List[0].Code);

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Journal1", !FilterCollection.Contains(Journal1));
			Assert("Expecting collection to contain Journal2", FilterCollection.Contains(Journal2));
			Assert("Expecting collection to contain Journal3", FilterCollection.Contains(Journal3));
			Assert("Expecting collection not to contain Journal4", !FilterCollection.Contains(Journal4));
		}

		public void TestPostDateFilter()
		{
			TestCaseHelper.ClearTable(AccPeriodManagement.Schema.TableName);
			var startDateOfFinancialYear = new ZDateTime(ZDateTime.Now.Year, 1, 1);
			var periodManager = TestObjectCreator.CreateTestPeriods(startDateOfFinancialYear);

			Factory.Save();

			Assert("Expecting collection to contain items", periodManager.Periods.Count > 0);
			var period = periodManager.Periods[0];

			Journal1.AH_PostDate = period.AM_StartDate.AddMinutes(-1);
			Journal2.AH_PostDate = period.AM_StartDate;
			Journal3.AH_PostDate = period.AM_EndDate;
			Journal4.AH_PostDate = period.AM_EndDate.AddMinutes(1);

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO["Post Date"];

			filter.Property1 = period.AM_StartDate;
			filter.Property2 = period.AM_EndDate;
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Journal1", !FilterCollection.Contains(Journal1));
			Assert("Expecting collection to contain Journal2", FilterCollection.Contains(Journal2));
			Assert("Expecting collection to contain Journal3", FilterCollection.Contains(Journal3));
			Assert("Expecting collection not to contain Journal4", !FilterCollection.Contains(Journal4));
		}

		public void TestReversePeriodFilter()
		{
			TestCaseHelper.ClearTable(AccPeriodManagement.Schema.TableName);
			ZDateTime startDateOfFinancialYear = new ZDateTime(ZDateTime.Now.Year, 1, 1);
			PeriodManager periodManager = TestObjectCreator.CreateTestPeriods(startDateOfFinancialYear);

			Factory.Save();

			Assert("Expecting collection to contain items", periodManager.Periods.Count > 0);

			Journal1.AH_DueDate = periodManager.Periods[0].AM_StartDate.AddMinutes(-1);
			Journal2.AH_DueDate = periodManager.Periods[0].AM_StartDate;
			Journal3.AH_DueDate = periodManager.Periods[0].AM_EndDate;
			Journal4.AH_DueDate = periodManager.Periods[0].AM_EndDate.AddMinutes(1);

			Factory.Save();

			ModulePeriodFilter filter = (ModulePeriodFilter)FilterBO["Reverse/End Period"];

			filter.Property = periodManager.Periods[0].AM_Period.ToString();
			filter.IsActive = true;

			AssertEquals("Should only be one comparison operator on this filter", 1, filter.ComparisonOperator_List.Count);
			AssertEquals("The only comparison operator should be Exact", ModuleNumberFilter.ComparisonConstants.Exact, filter.ComparisonOperator_List[0].Code);

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Journal1", !FilterCollection.Contains(Journal1));
			Assert("Expecting collection to contain Journal2", FilterCollection.Contains(Journal2));
			Assert("Expecting collection to contain Journal3", FilterCollection.Contains(Journal3));
			Assert("Expecting collection not to contain Journal4", !FilterCollection.Contains(Journal4));
		}

		public void TestJournalDescriptionFilter()
		{
			Journal1.AH_Desc = "XXXAAAXX";
			Journal2.AH_Desc = "YYYYAAAY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterBO["Journal Description"];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "XXX";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Journal1", FilterCollection.Contains(Journal1));
			Assert("Expecting collection not to contain Journal2", !FilterCollection.Contains(Journal2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "YYYYAAAY";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Journal1", !FilterCollection.Contains(Journal1));
			Assert("Expecting collection to contain Journal2", FilterCollection.Contains(Journal2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "AAA";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Journal1", FilterCollection.Contains(Journal1));
			Assert("Expecting collection to contain Journal2", FilterCollection.Contains(Journal2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "ZZZ";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Journal1", !FilterCollection.Contains(Journal1));
			Assert("Expecting collection not to contain Journal2", !FilterCollection.Contains(Journal2));
		}

		public void TestBranchManagementCodeFilter()
		{
			var codeCollection = new BranchManagementCodeDescriptionBoolCollection();
			codeCollection.Add("BRA", null, true);
			codeCollection.Add("BRB", null, true);
			AccountingMasterFilesRegistry.Instance.BranchManagementCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, codeCollection);

			Branch1.GB_AccountingGroupCode = "BRA";
			Branch2.GB_AccountingGroupCode = "BRB";

			Journal1.AH_GB = Branch1.PK;
			Journal2.AH_GB = Branch2.PK;

			Factory.Save();

			var branchManagementCodeFilter = (ModuleTextFilter)FilterBO["Branch Management Code"];
			branchManagementCodeFilter.Property = "BRA";
			branchManagementCodeFilter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder("Collection should Contain Journal1", new[] { Journal1 }, FilterCollection);

			branchManagementCodeFilter.Property = "BRB";
			FilterCollection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder("Collection should Contain Journal2", new[] { Journal2 }, FilterCollection);
		}

		public void TestBranchFilter()
		{
			Journal1.AH_GB = Branch1.PK;
			Journal2.AH_GB = Branch2.PK;
			Journal3.AH_GB = Branch3.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Branch"];

			filter.Property = Branch1.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Journal1", FilterCollection.Contains(Journal1));
			Assert("Expecting collection not to contain Journal2", !FilterCollection.Contains(Journal2));
			Assert("Expecting collection not to contain Journal3", !FilterCollection.Contains(Journal3));

			filter.Property = Branch3.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Journal1", !FilterCollection.Contains(Journal1));
			Assert("Expecting collection not to contain Journal2", !FilterCollection.Contains(Journal2));
			Assert("Expecting collection not to contain Journal3", !FilterCollection.Contains(Journal3));
		}

		public void TestViewingNonLoginBranchTransactions()
		{
			Env.Security.GeneralLedgerViewingNonLoginBranchTransactions.IsAllowed = false;
			var branchFilter = (ModuleGuidFilter)FilterBO.GetModuleFiltersCore_ForTestOnly()["Branch"];

			AssertEquals("Should be read only", true, branchFilter.ReadOnly);
			AssertEquals("Should be always visible", FilterVisibility.AlwaysVisible, branchFilter.Visibility);
			AssertEquals("Should have the default value as current login branch", GlbBranch.CurrentBranch.PK, branchFilter.Property);

			Env.Security.GeneralLedgerViewingNonLoginBranchTransactions.IsAllowed = true;
			branchFilter = (ModuleGuidFilter)FilterBO.GetModuleFiltersCore_ForTestOnly()["Branch"];

			AssertEquals("Should not be read only", false, branchFilter.ReadOnly);
			AssertNotEquals("Should not be always visible", FilterVisibility.AlwaysVisible, branchFilter.Visibility);
			AssertNotEquals("Should not have the default value as current login branch", GlbBranch.CurrentBranch.PK, branchFilter.Property);
		}

		public void TestDepartmentFilter()
		{
			Journal1.AH_GE = Department1.PK;
			Journal2.AH_GE = Department2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Department"];

			filter.Property = Department1.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Journal1", FilterCollection.Contains(Journal1));
			Assert("Expecting collection not to contain Journal2", !FilterCollection.Contains(Journal2));

			filter.Property = Department3.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Journal1", !FilterCollection.Contains(Journal1));
			Assert("Expecting collection not to contain Journal2", !FilterCollection.Contains(Journal2));
		}

		public void TestLastRequesterFilter()
		{
			Factory.Save();

			var expectedCreatedTime1 = ZDateTime.Now;
			var expectedCreatedTime2 = ZDateTime.Now.AddDays(1);
			var expectedCreatedTime3 = ZDateTime.Now.AddDays(2);

			var firstUser = "A";
			var secondUser = "B";
			var thirdUser = "C";
			var someoneElse = "IML";

			AddApprovalToJournal(Journal1, secondUser, thirdUser, expectedCreatedTime1, Constants.GenApprovalRequestApprovalStatus.Requested);
			AddApprovalToJournal(Journal2, firstUser, thirdUser, expectedCreatedTime1, Constants.GenApprovalRequestApprovalStatus.Requested);
			AddApprovalToJournal(Journal3, secondUser, thirdUser, expectedCreatedTime1, Constants.GenApprovalRequestApprovalStatus.Requested);

			AddApprovalToJournal(Journal1, firstUser, thirdUser, expectedCreatedTime2, Constants.GenApprovalRequestApprovalStatus.Posted);
			AddApprovalToJournal(Journal2, secondUser, thirdUser, expectedCreatedTime2, Constants.GenApprovalRequestApprovalStatus.Posted);
			AddApprovalToJournal(Journal3, firstUser, thirdUser, expectedCreatedTime2, Constants.GenApprovalRequestApprovalStatus.Requested);

			AddApprovalToJournal(Journal1, firstUser, thirdUser, expectedCreatedTime3, Constants.GenApprovalRequestApprovalStatus.Cancelled);
			AddApprovalToJournal(Journal2, firstUser, thirdUser, expectedCreatedTime3, Constants.GenApprovalRequestApprovalStatus.Cancelled);
			AddApprovalToJournal(Journal3, firstUser, thirdUser, expectedCreatedTime3, Constants.GenApprovalRequestApprovalStatus.Cancelled);

			Factory.Save();

			var filter = (ModuleNkFilter)FilterBO["Last Requester"];

			filter.Property = firstUser;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Journal1", FilterCollection.Contains(Journal1));
			Assert("Expecting collection not to contain Journal2", !FilterCollection.Contains(Journal2));
			Assert("Expecting collection to contain Journal3", FilterCollection.Contains(Journal3));

			filter.Property = someoneElse;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Journal1", !FilterCollection.Contains(Journal1));
			Assert("Expecting collection not to contain Journal2", !FilterCollection.Contains(Journal2));
			Assert("Expecting collection not to contain Journal3", !FilterCollection.Contains(Journal3));
		}

		public void TestOriginalRequesterFilter()
		{
			Factory.Save();

			var expectedCreatedTime1 = ZDateTime.Now;
			var expectedCreatedTime2 = ZDateTime.Now.AddDays(1);
			var expectedCreatedTime3 = ZDateTime.Now.AddDays(2);

			var firstUser = "A";
			var secondUser = "B";
			var thirdUser = "C";
			var someoneElse = "IML";

			AddApprovalToJournal(Journal1, thirdUser, thirdUser, expectedCreatedTime1, Constants.GenApprovalRequestApprovalStatus.Cancelled);
			AddApprovalToJournal(Journal2, thirdUser, thirdUser, expectedCreatedTime1, Constants.GenApprovalRequestApprovalStatus.Cancelled);
			AddApprovalToJournal(Journal3, thirdUser, thirdUser, expectedCreatedTime1, Constants.GenApprovalRequestApprovalStatus.Cancelled);

			AddApprovalToJournal(Journal1, firstUser, thirdUser, expectedCreatedTime2, Constants.GenApprovalRequestApprovalStatus.Posted);
			AddApprovalToJournal(Journal2, secondUser, thirdUser, expectedCreatedTime2, Constants.GenApprovalRequestApprovalStatus.Posted);
			AddApprovalToJournal(Journal3, firstUser, thirdUser, expectedCreatedTime2, Constants.GenApprovalRequestApprovalStatus.Requested);

			AddApprovalToJournal(Journal1, secondUser, thirdUser, expectedCreatedTime3, Constants.GenApprovalRequestApprovalStatus.Requested);
			AddApprovalToJournal(Journal2, firstUser, thirdUser, expectedCreatedTime3, Constants.GenApprovalRequestApprovalStatus.Requested);
			AddApprovalToJournal(Journal3, secondUser, thirdUser, expectedCreatedTime3, Constants.GenApprovalRequestApprovalStatus.Requested);

			Factory.Save();

			var filter = (ModuleNkFilter)FilterBO["Original Requester"];

			filter.Property = firstUser;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Journal1", FilterCollection.Contains(Journal1));
			Assert("Expecting collection not to contain Journal2", !FilterCollection.Contains(Journal2));
			Assert("Expecting collection to contain Journal3", FilterCollection.Contains(Journal3));

			filter.Property = someoneElse;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Journal1", !FilterCollection.Contains(Journal1));
			Assert("Expecting collection not to contain Journal2", !FilterCollection.Contains(Journal2));
			Assert("Expecting collection not to contain Journal3", !FilterCollection.Contains(Journal3));
		}

		public void TestOriginalApproverFilter()
		{
			Factory.Save();

			var expectedCreatedTime1 = ZDateTime.Now;
			var expectedCreatedTime2 = ZDateTime.Now.AddDays(1);
			var expectedCreatedTime3 = ZDateTime.Now.AddDays(2);

			var firstUser = "A";
			var secondUser = "B";
			var thirdUser = "C";
			var someoneElse = "IML";

			AddApprovalToJournal(Journal1, thirdUser, thirdUser, expectedCreatedTime1, Constants.GenApprovalRequestApprovalStatus.Cancelled);
			AddApprovalToJournal(Journal2, thirdUser, thirdUser, expectedCreatedTime1, Constants.GenApprovalRequestApprovalStatus.Cancelled);
			AddApprovalToJournal(Journal3, thirdUser, thirdUser, expectedCreatedTime1, Constants.GenApprovalRequestApprovalStatus.Cancelled);

			AddApprovalToJournal(Journal1, thirdUser, firstUser, expectedCreatedTime2, Constants.GenApprovalRequestApprovalStatus.Posted);
			AddApprovalToJournal(Journal2, thirdUser, secondUser, expectedCreatedTime2, Constants.GenApprovalRequestApprovalStatus.Posted);
			AddApprovalToJournal(Journal3, thirdUser, firstUser, expectedCreatedTime2, Constants.GenApprovalRequestApprovalStatus.Requested);

			AddApprovalToJournal(Journal1, thirdUser, thirdUser, expectedCreatedTime3, Constants.GenApprovalRequestApprovalStatus.Requested);
			AddApprovalToJournal(Journal2, thirdUser, firstUser, expectedCreatedTime3, Constants.GenApprovalRequestApprovalStatus.Requested);
			AddApprovalToJournal(Journal3, thirdUser, thirdUser, expectedCreatedTime3, Constants.GenApprovalRequestApprovalStatus.Requested);

			Factory.Save();

			var filter = (ModuleNkFilter)FilterBO["Original Approver"];

			filter.Property = firstUser;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Journal1", FilterCollection.Contains(Journal1));
			Assert("Expecting collection not to contain Journal2", !FilterCollection.Contains(Journal2));
			Assert("Expecting collection not to contain Journal2", !FilterCollection.Contains(Journal2));

			filter.Property = someoneElse;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Journal1", !FilterCollection.Contains(Journal1));
			Assert("Expecting collection not to contain Journal2", !FilterCollection.Contains(Journal2));
			Assert("Expecting collection not to contain Journal2", !FilterCollection.Contains(Journal2));
		}

		public void TestDateUploadedFilter()
		{
			TestCaseHelper.ClearTable(AccPeriodManagement.Schema.TableName);
			var startDateOfFinancialYear = new ZDateTime(ZDateTime.Now.Year, 1, 1);
			var periodManager = TestObjectCreator.CreateTestPeriods(startDateOfFinancialYear);

			Factory.Save();

			Assert("Expecting collection to contain items", periodManager.Periods.Count > 0);
			var period = periodManager.Periods[0];
			var user = "A";

			Journal1.Logs.AddNew(Events.DataImport, period.AM_StartDate.ToOffset());
			Journal2.Logs.AddNew(Events.DataImport, period.AM_StartDate.AddMinutes(-1).ToOffset());
			Journal3.Logs.AddNew(Events.DataImport, period.AM_EndDate.ToOffset());
			Journal4.Logs.AddNew(Events.DataImport, period.AM_EndDate.AddMinutes(1).ToOffset());

			var journal5 = Factory.NewWithValidTestData<GLJournal>();
			var journal6 = Factory.NewWithValidTestData<GLJournal>();
			var journal7 = Factory.NewWithValidTestData<GLJournal>();
			var journal8 = Factory.NewWithValidTestData<GLJournal>();

			Factory.Save();

			var approval5 = AddApprovalToJournal(journal5, user, user, ZDateTime.Now, Constants.GenApprovalRequestApprovalStatus.Posted);
			var approval6 = AddApprovalToJournal(journal6, user, user, ZDateTime.Now, Constants.GenApprovalRequestApprovalStatus.Posted);
			var approval7 = AddApprovalToJournal(journal7, user, user, ZDateTime.Now, Constants.GenApprovalRequestApprovalStatus.Posted);
			var approval8 = AddApprovalToJournal(journal8, user, user, ZDateTime.Now, Constants.GenApprovalRequestApprovalStatus.Posted);

			approval5.Logs.AddNew(Events.DataImport, period.AM_StartDate.ToOffset());
			approval6.Logs.AddNew(Events.DataImport, period.AM_StartDate.AddMinutes(-1).ToOffset());
			approval7.Logs.AddNew(Events.DataImport, period.AM_EndDate.ToOffset());
			approval8.Logs.AddNew(Events.DataImport, period.AM_EndDate.AddMinutes(1).ToOffset());

			var journal9 = Factory.NewWithValidTestData<GLJournal>();

			Factory.Save();

			var filter = (ModuleDateFilter)FilterBO["Date Uploaded"];

			filter.Property1 = period.AM_StartDate;
			filter.Property2 = period.AM_EndDate;
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Journal1", FilterCollection.Contains(Journal1));
			Assert("Expecting collection not to contain Journal2 because DIM log in journal is out out date range.", !FilterCollection.Contains(Journal2));
			Assert("Expecting collection to contain Journal3", FilterCollection.Contains(Journal3));
			Assert("Expecting collection not to contain Journal4 because DIM log in journal is out out date range.", !FilterCollection.Contains(Journal4));
			Assert("Expecting collection to contain journal5", FilterCollection.Contains(journal5));
			Assert("Expecting collection not to contain journal6 because DIM log in journal approval request is out out date range.", !FilterCollection.Contains(journal6));
			Assert("Expecting collection to contain journal7", FilterCollection.Contains(journal7));
			Assert("Expecting collection not to contain journal8because DIM log in journal approval request is out out date range.", !FilterCollection.Contains(journal8));
			Assert("Expecting collection not to contain journal9 because journal don't have DIM log.", !FilterCollection.Contains(journal9));

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Journal1 because Journal1 has DIM log.", !FilterCollection.Contains(Journal1));
			Assert("Expecting collection not to contain Journal2 because Journal2 has DIM log.", !FilterCollection.Contains(Journal2));
			Assert("Expecting collection not to contain Journal3 because Journal3 has DIM log", !FilterCollection.Contains(Journal3));
			Assert("Expecting collection not to contain Journal4 because Journal4 has DIM log.", !FilterCollection.Contains(Journal4));
			Assert("Expecting collection not to contain journal5 because Journal5 has DIM log.", !FilterCollection.Contains(journal5));
			Assert("Expecting collection not to contain journal6 because Journal6 approval request has DIM log.", !FilterCollection.Contains(journal6));
			Assert("Expecting collection not to contain journal7 because Journal7 has DIM log", !FilterCollection.Contains(journal7));
			Assert("Expecting collection not to contain journal8 because Journal8 approval request has DIM log.", !FilterCollection.Contains(journal8));
			Assert("Expecting collection to contain journal9 because journal9 don't have DIM log.", FilterCollection.Contains(journal9));
		}

		GLJournalApprovalRequest AddApprovalToJournal(GLJournal journal, ZString creater, ZString approver, ZDateTime createdTime, ZString status)
		{
			var approval = journal.Approvals.AddNew();
			approval.XP_SystemCreateUser = creater;
			approval.XP_GS_NKApprovingUser1 = approver;
			approval.XP_SystemCreateTimeUtc = createdTime;
			approval.XP_ApprovalStatus = status;
			approval.Initialize(journal);

			return approval;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GLJournalFilterBusinessObject();
		}

		GLJournal Journal1;
		GLJournal Journal2;
		GLJournal Journal3;
		GLJournal Journal4;

		GlbBranch Branch1;
		GlbBranch Branch2;
		GlbBranch Branch3;

		GlbDepartment Department1;
		GlbDepartment Department2;
		GlbDepartment Department3;

		GLJournalCollection FilterCollection;
		GLJournalFilterBusinessObject FilterBO;

		protected override void SetUp()
		{
			base.SetUp();

			GlbCompany nonCurrentCompany = Factory.NewWithValidTestData<GlbCompany>();

			Branch1 = Factory.NewWithValidTestData<GlbBranch>();
			Branch2 = Factory.NewWithValidTestData<GlbBranch>();
			Branch3 = Factory.NewWithValidTestData<GlbBranch>();

			Branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			Branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			Branch3.GB_GC = nonCurrentCompany.PK;

			Department1 = Factory.NewWithValidTestData<GlbDepartment>();
			Department2 = Factory.NewWithValidTestData<GlbDepartment>();
			Department3 = Factory.NewWithValidTestData<GlbDepartment>();

			Factory.Save();

			Journal1 = Factory.NewWithValidTestData<GLJournal>();
			Journal2 = Factory.NewWithValidTestData<GLJournal>();
			Journal3 = Factory.NewWithValidTestData<GLJournal>();
			Journal4 = Factory.NewWithValidTestData<GLJournal>();

			FilterCollection = new GLJournalCollection(Factory);
			FilterBO = (GLJournalFilterBusinessObject)GetNewFilterStripBusinessObject();
		}
	}
}
