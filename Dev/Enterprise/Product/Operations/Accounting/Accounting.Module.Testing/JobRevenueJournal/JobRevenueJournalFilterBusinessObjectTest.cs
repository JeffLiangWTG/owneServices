using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(JobRevenueJournalFilterBusinessObject))]
	class JobRevenueJournalFilterBusinessObjectTest : AccountingFilterStripBusinessObjectTestCase
	{
		public void TestLocalJobReference()
		{
			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO["Job Local Reference"];

			filter.Property = job1.JH_JobLocalReference;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals("journals.Count", 1, FilterCollection.Count);
			AssertCollectionContains(journal1, FilterCollection);

			filter.Property = job2.JH_JobLocalReference;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals("journals.Count", 1, FilterCollection.Count);
			AssertCollectionContains(journal2, FilterCollection);

			filter.Property = "Denys";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("journals.Count", 0, FilterCollection.Count);
		}

		public void TestJobNumberFilter()
		{
			Factory.Save();

			ModuleTextRangeFilter filter = (ModuleTextRangeFilter)FilterBO[AccountingUtils.NumberFilterTypes.JobNumber];

			filter.Property1 = "";
			filter.Property2 = "S00001000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals("journals.Count", 1, FilterCollection.Count);
			AssertCollectionContains(journal1, FilterCollection);

			filter.Property1 = "S00001001";
			filter.Property2 = "S00001001";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals("journals.Count", 1, FilterCollection.Count);
			AssertCollectionContains(journal2, FilterCollection);

			filter.Property1 = "S00001000";
			filter.Property2 = "S00001002";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals("journals.Count", 3, FilterCollection.Count);
			AssertCollectionContains(journal1, FilterCollection);
			AssertCollectionContains(journal2, FilterCollection);
			AssertCollectionContains(journal3, FilterCollection);

			filter.Property1 = "S00001003";
			filter.Property2 = "S00001004";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals("journals.Count", 0, FilterCollection.Count);
		}

		public void TestFilterbyCurrentCompany()
		{
			journal2.AH_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;

			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("There should be 2 matching journals", 2, FilterCollection.Count);
			AssertCollectionContains(journal1, FilterCollection);
			AssertCollectionContains(journal3, FilterCollection);
		}

		public void TestTransactionNumberFilter()
		{
			journal1.AH_TransactionNum = "00035262";
			journal2.AH_TransactionNum = "00001626";
			journal3.AH_TransactionNum = "00001583";

			ModuleTextFilter transactionNumberFilter = (ModuleTextFilter)FilterBO[AccountingUtils.NumberFilterTypes.TransactionNumber];
			transactionNumberFilter.Property = "00001626";
			transactionNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			transactionNumberFilter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("There should be 1 journal in the collection", 1, FilterCollection.Count);
			AssertCollectionContains(journal2, FilterCollection);

			transactionNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			transactionNumberFilter.Property = "26";

			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("There should be 2 transactions in the collection", 2, FilterCollection.Count);
			AssertCollectionContains(journal1, FilterCollection);
			AssertCollectionContains(journal2, FilterCollection);
		}

		[TestDate(2015, 6, 1)]
		public void TestPostDateFilter()
		{
			journal2.AH_PostDate = ZDateTime.Now.AddDays(-4);
			journal3.AH_PostDate = ZDateTime.Now.AddDays(-1);

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO[AccountingUtils.DateFilterTypes.PostDate];
			filter.Property1 = ZDateTime.Now.AddDays(-4);
			filter.Property2 = ZDateTime.Now.AddDays(-1);
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Collection should contain 2 journals", 2, FilterCollection.Count);
			AssertCollectionContains(journal2, FilterCollection);
			AssertCollectionContains(journal3, FilterCollection);

			filter.Property1 = ZDateTime.Now;
			filter.Property2 = ZDateTime.Now;
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Collection should contain 1 journal", 1, FilterCollection.Count);
			AssertCollectionContains(journal1, FilterCollection);
		}

		[TestDate(2015, 6, 1)]
		public void TestTransactionDateFilter()
		{
			journal2.AH_InvoiceDate = ZDateTime.Now.AddDays(-4);
			journal3.AH_InvoiceDate = ZDateTime.Now.AddDays(-1);

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO[AccountingUtils.DateFilterTypes.TransactionDate];
			filter.Property1 = ZDateTime.Now.AddDays(-4);
			filter.Property2 = ZDateTime.Now.AddDays(-1);
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Collection should contain 2 journals", 2, FilterCollection.Count);
			AssertCollectionContains(journal2, FilterCollection);
			AssertCollectionContains(journal3, FilterCollection);

			filter.Property1 = ZDateTime.Now;
			filter.Property2 = ZDateTime.Now;
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Collection should contain 1 journal", 1, FilterCollection.Count);
			AssertCollectionContains(journal1, FilterCollection);
		}

		public void TestViewingNonLoginBranchTransactions()
		{
			Env.Security.JobRevenueJournalViewingNonLoginBranchTransactions.IsAllowed = true;
			ModuleGuidFilter branchFilter = (ModuleGuidFilter)FilterBO.GetModuleFiltersCore_ForTestOnly()["Branch"];

			AssertEquals("Should not be read only", false, branchFilter.ReadOnly);
			AssertNotEquals("Should not be always visible", FilterVisibility.AlwaysVisible, branchFilter.Visibility);
			AssertNotEquals("Should not have the default value as current login branch", GlbBranch.CurrentBranch.PK, branchFilter.Property);

			Env.Security.JobRevenueJournalViewingNonLoginBranchTransactions.IsAllowed = false;
			branchFilter = (ModuleGuidFilter)FilterBO.GetModuleFiltersCore_ForTestOnly()["Branch"];

			AssertEquals("Should be read only", true, branchFilter.ReadOnly);
			AssertEquals("Should be always visible", FilterVisibility.AlwaysVisible, branchFilter.Visibility);
			AssertEquals("Should have the default value as current login branch", GlbBranch.CurrentBranch.PK, branchFilter.Property);
		}

		public void TestBranchFilter()
		{
			job1.JH_GB = GlbBranch.CurrentBranch.PK;
			job2.JH_GB = TestObjectCreator.NonCurrentBranch.PK;
			job3.JH_GB = GlbBranch.CurrentBranch.PK;
			BusinessObjectFactory.SaveTogether(job1.Factory, job2.Factory, job3.Factory);
			Factory.Save();
			AssertEquals("Precondition: journal1 should be posted to CurrentBranch.", GlbBranch.CurrentBranch.PK, journal1.Branch.PK);
			AssertEquals("Precondition: journal2 should be posted to NonCurrentBranch.", TestObjectCreator.NonCurrentBranch.PK, journal2.Branch.PK);
			AssertEquals("Precondition: journal3 should be posted to CurrentBranch.", GlbBranch.CurrentBranch.PK, journal3.Branch.PK);

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Branch"];
			filter.Property = TestObjectCreator.NonCurrentBranch.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Collection should contain 1 journal", 1, FilterCollection.Count);
			AssertCollectionContains(journal2, FilterCollection);

			filter.Property = GlbBranch.CurrentBranch.PK;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Collection should contain 2 journals", 2, FilterCollection.Count);
			AssertCollectionContains(journal1, FilterCollection);
			AssertCollectionContains(journal3, FilterCollection);
		}

		public void TestDepartmentFilter()
		{
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job2.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			job3.JH_GE = GlbDepartment.CurrentDepartment.PK;
			BusinessObjectFactory.SaveTogether(job1.Factory, job2.Factory, job3.Factory);
			Factory.Save();
			AssertEquals("Precondition: journal1 should be posted to CurrentDepartment.", GlbDepartment.CurrentDepartment.PK, journal1.Department.PK);
			AssertEquals("Precondition: journal2 should be posted to NonCurrentDepartment.", TestObjectCreator.NonCurrentDepartment.PK, journal2.Department.PK);
			AssertEquals("Precondition: journal3 should be posted to CurrentDepartment.", GlbDepartment.CurrentDepartment.PK, journal3.Department.PK);

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Department"];
			filter.Property = TestObjectCreator.NonCurrentDepartment.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Collection should contain 1 journal", 1, FilterCollection.Count);
			AssertCollectionContains(journal2, FilterCollection);

			filter.Property = GlbDepartment.CurrentDepartment.PK;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Collection should contain 2 journals", 2, FilterCollection.Count);
			AssertCollectionContains(journal1, FilterCollection);
			AssertCollectionContains(journal3, FilterCollection);
		}

		public void TestJobNumberFilterDescription()
		{
			ModuleTextRangeFilter filter = (ModuleTextRangeFilter)FilterBO[AccountingUtils.NumberFilterTypes.JobNumber];
			AssertEquals("Localized Filter Name", filter.LocalizedDescription, "Job Number Range");
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new JobRevenueJournalFilterBusinessObject();
		}

		JobRevenueJournalCollection FilterCollection;
		JobRevenueJournalFilterBusinessObject FilterBO;

		JobRevenueJournal journal1;
		JobRevenueJournal journal2;
		JobRevenueJournal journal3;

		Job job1;
		Job job2;
		Job job3;

		protected override void SetUp()
		{
			base.SetUp();

			FilterCollection = new JobRevenueJournalCollection(Factory);
			FilterBO = (JobRevenueJournalFilterBusinessObject)GetNewFilterStripBusinessObject();

			job1 = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.JobReadyForCostPosting.Code);
			job2 = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.JobReadyForCostPosting.Code);
			job3 = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.JobReadyForCostPosting.Code);

			journal1 = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, job1, 100M);
			journal2 = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, job2, 200M);
			journal3 = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, job3, 300M);
		}

		#endregion
	}
}
