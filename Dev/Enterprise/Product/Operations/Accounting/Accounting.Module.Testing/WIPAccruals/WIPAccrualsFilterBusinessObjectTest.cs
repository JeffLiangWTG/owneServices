using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(WIPAccrualsFilterBusinessObject))]
	class WIPAccrualsFilterBusinessObjectTest : AccountingFilterStripBusinessObjectTestCase
	{
		#region Number Filter Tests

		public void TestLocalJobReferenceFilter()
		{
			Factory.Save();

			ModuleNumberFilter filter = ((ModuleNumberFilter)FilterBO["Job Local Reference"]);

			filter.Property = Job1.JH_JobLocalReference;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain WIP1", FilterCollection.Contains(WIP1));
			Assert("Expecting collection not to contain Accrual2", !FilterCollection.Contains(Accrual2));
			Assert("Expecting collection not to contain WIP3", !FilterCollection.Contains(WIP3));

			filter.Property = Job2.JH_JobLocalReference;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain WIP1", !FilterCollection.Contains(WIP1));
			Assert("Expecting collection to contain Accrual2", FilterCollection.Contains(Accrual2));
			Assert("Expecting collection not to contain WIP3", !FilterCollection.Contains(WIP3));

			filter.Property = "Denys";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain WIP1", !FilterCollection.Contains(WIP1));
			Assert("Expecting collection not to contain Accrual2", !FilterCollection.Contains(Accrual2));
			Assert("Expecting collection not to contain WIP3", !FilterCollection.Contains(WIP3));
		}

		public void TestJobNumberFilter()
		{
			Job1.JH_JobNum = "11111111";
			Job2.JH_JobNum = "22222222";
			Job3.JH_JobNum = "33333333";

			Factory.Save();

			var filter = (ModuleNumberFilter)FilterBO["Job #"];

			filter.Property = "11111111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain WIP1", FilterCollection.Contains(WIP1));
			Assert("Expecting collection not to contain Accrual2", !FilterCollection.Contains(Accrual2));
			Assert("Expecting collection not to contain WIP3", !FilterCollection.Contains(WIP3));

			filter.Property = "22222222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain WIP1", !FilterCollection.Contains(WIP1));
			Assert("Expecting collection to contain Accrual2", FilterCollection.Contains(Accrual2));
			Assert("Expecting collection not to contain WIP3", !FilterCollection.Contains(WIP3));

			filter.Property = "";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain WIP1", FilterCollection.Contains(WIP1));
			Assert("Expecting collection to contain Accrual2", FilterCollection.Contains(Accrual2));
			Assert("Expecting collection to contain WIP3", FilterCollection.Contains(WIP3));

			filter.Property = "444";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain WIP1", !FilterCollection.Contains(WIP1));
			Assert("Expecting collection not to contain Accrual2", !FilterCollection.Contains(Accrual2));
			Assert("Expecting collection not to contain WIP3", !FilterCollection.Contains(WIP3));
		}

		public void TestJobNumberRangeFilter()
		{
			Job1.JH_JobNum = "11111111";
			Job2.JH_JobNum = "22222222";
			Job3.JH_JobNum = "33333333";

			Factory.Save();

			var filter = (ModuleTextRangeFilter)FilterBO["Job Number Range"];

			filter.Property1 = "";
			filter.Property2 = "11111111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain WIP1", FilterCollection.Contains(WIP1));
			Assert("Expecting collection not to contain Accrual2", !FilterCollection.Contains(Accrual2));
			Assert("Expecting collection not to contain WIP3", !FilterCollection.Contains(WIP3));

			filter.Property1 = "22222222";
			filter.Property2 = "22222222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain WIP1", !FilterCollection.Contains(WIP1));
			Assert("Expecting collection to contain Accrual2", FilterCollection.Contains(Accrual2));
			Assert("Expecting collection not to contain WIP3", !FilterCollection.Contains(WIP3));

			filter.Property1 = "11111111";
			filter.Property2 = "33333333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain WIP1", FilterCollection.Contains(WIP1));
			Assert("Expecting collection to contain Accrual2", FilterCollection.Contains(Accrual2));
			Assert("Expecting collection to contain WIP3", FilterCollection.Contains(WIP3));

			filter.Property1 = "444";
			filter.Property2 = "555";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain WIP1", !FilterCollection.Contains(WIP1));
			Assert("Expecting collection not to contain Accrual2", !FilterCollection.Contains(Accrual2));
			Assert("Expecting collection not to contain WIP3", !FilterCollection.Contains(WIP3));
		}

		public void TestJobNumberFilterIsExclusive()
		{
			var exclusiveFilter = FilterBO.GetModuleFilterThatOverridesAllOtherFilters();

			AssertNotNull("Module should contain an exclusive filter", exclusiveFilter);
			AssertEquals("Module exclusive filter should be a 'Job #' filter", AccountingUtils.NumberFilterTypes.JobNumber, exclusiveFilter.Description);
		}

		public void TestConsolNumberFilter()
		{
			ForwardingConsol jobConsol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			ForwardingConsol jobConsol2 = Factory.NewWithValidTestData<ForwardingConsol>();

			jobConsol1.JK_UniqueConsignRef = "11100011";
			jobConsol2.JK_UniqueConsignRef = "22000222";

			ForwardingShipment jobShipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment jobShipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

			JobConShipLink jobConShipLink1 = Factory.NewWithValidTestData<JobConShipLink>();
			JobConShipLink jobConShipLink2 = Factory.NewWithValidTestData<JobConShipLink>();

			jobConShipLink1.JN_JK = jobConsol1.PK;
			jobConShipLink2.JN_JK = jobConsol2.PK;

			jobConShipLink1.JN_JS = jobShipment1.PK;
			jobConShipLink2.JN_JS = jobShipment2.PK;

			Job1.JH_ParentID = jobShipment1.PK;
			Job2.JH_ParentID = jobShipment2.PK;

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO["Consol #"];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain WIP1", FilterCollection.Contains(WIP1));
			Assert("Expecting collection not to contain Accrual2", !FilterCollection.Contains(Accrual2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain WIP1", !FilterCollection.Contains(WIP1));
			Assert("Expecting collection to contain Accrual2", FilterCollection.Contains(Accrual2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain WIP1", FilterCollection.Contains(WIP1));
			Assert("Expecting collection to contain Accrual2", FilterCollection.Contains(Accrual2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain WIP1", !FilterCollection.Contains(WIP1));
			Assert("Expecting collection not to contain Accrual2", !FilterCollection.Contains(Accrual2));
		}

		public void TestJobNumberFilterDescription()
		{
			var filter = (ModuleNumberFilter)FilterBO[AccountingUtils.NumberFilterTypes.JobNumber];
			AssertEquals("Localized Filter Name", filter.LocalizedDescription, AccountingUtils.NumberFilterTypes.JobNumber);
		}

		#endregion

		#region Date Filter Tests

		public void TestPostDateFilter()
		{
			WIP1.AL_PostDate = new ZDateTime(ZDateTime.Now.Year, 1, 1);
			WIP2.AL_PostDate = new ZDateTime(ZDateTime.Now.Year, 2, 15);
			Accrual1.AL_PostDate = new ZDateTime(ZDateTime.Now.Year, 2, 16);

			Factory.Save();

			ModuleDateFilter postDateFilter = ((ModuleDateFilter)FilterBO[Business.AccountingUtils.DateFilterTypes.PostDate]);
			postDateFilter.Property1 = new ZDateTime(ZDateTime.Now.Year, 2, 1);
			postDateFilter.Property2 = new ZDateTime(ZDateTime.Now.Year, 2, 20);
			postDateFilter.IsActive = true;
			postDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain WIP1", !FilterCollection.Contains(WIP1));
			Assert("Expecting collection to contain Accrual1", FilterCollection.Contains(Accrual1));
			Assert("Expecting collection to contain WIP2", FilterCollection.Contains(WIP2));
		}

		public void TestReverseDateFilter()
		{
			WIP1.RelatedJobCharge.ReverseWIP(new ZDateTime(ZDateTime.Now.Year, 1, 1));

			WIP2.RelatedJobCharge.ReverseWIP(new ZDateTime(ZDateTime.Now.Year, 2, 15));

			Accrual1.RelatedJobCharge.ReverseAccrual(new ZDateTime(ZDateTime.Now.Year, 2, 16));

			Factory.Save();

			ModuleDateFilter reverseDateFilter = ((ModuleDateFilter)FilterBO[Business.AccountingUtils.DateFilterTypes.ReverseDate]);
			reverseDateFilter.Property1 = new ZDateTime(ZDateTime.Now.Year, 2, 1);
			reverseDateFilter.Property2 = new ZDateTime(ZDateTime.Now.Year, 2, 20);
			reverseDateFilter.IsActive = true;
			reverseDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			ModuleTextFilter showReversed = ((ModuleTextFilter)FilterBO["Show Reversed Transactions"]);
			showReversed.Property = "All";

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain WIP1", !FilterCollection.Contains(WIP1));
			Assert("Expecting collection to contain Accrual1", FilterCollection.Contains(Accrual1));
			Assert("Expecting collection to contain WIP2", FilterCollection.Contains(WIP2));
		}

		#endregion

		#region Reference Filter Tests

		#region TestBranchManagementCodeFilter

		public void TestBranchManagementCodeFilter()
		{
			var codeCollection = new BranchManagementCodeDescriptionBoolCollection();
			codeCollection.Add("BRA", null, true);
			codeCollection.Add("BRB", null, true);
			AccountingMasterFilesRegistry.Instance.BranchManagementCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, codeCollection);

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_AccountingGroupCode = "BRA";
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_AccountingGroupCode = "BRB";

			WIP1.RelatedJobCharge.JR_GB = WIP1.AL_GB = branch1.PK;
			Accrual1.RelatedJobCharge.JR_GB = Accrual1.AL_GB = branch2.PK;

			Factory.Save();

			var branchManagementCodeFilter = (ModuleTextFilter)FilterBO["Branch Management Code"];
			branchManagementCodeFilter.Property = "BRA";
			branchManagementCodeFilter.IsActive = true;
			var collection = new WIPAccrualCollection(Factory);
			collection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder("Collection should Contain WIP1", new[] { WIP1 }, collection);

			branchManagementCodeFilter.Property = "BRB";
			collection = new WIPAccrualCollection(Factory);
			collection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder("Collection should Contain Accrual1", new[] { Accrual1 }, collection);
		}

		#endregion

		public void TestBranchFilter()
		{
			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			GlbBranch branch2 = Factory.NewWithValidTestData<GlbBranch>();
			GlbBranch branch3 = Factory.NewWithValidTestData<GlbBranch>();

			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;

			WIP1.RelatedJobCharge.JR_GB = WIP1.AL_GB = branch1.PK;
			Accrual1.RelatedJobCharge.JR_GB = Accrual1.AL_GB = branch2.PK;
			Factory.Save();

			using (TestObjectCreator.SwitchEnvToBranch(branch3))
			{
				WIP3.RelatedJobCharge.JR_GB = WIP3.AL_GB = branch3.PK;
				Factory.Save();
			}

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Branch"];

			filter.Property = branch1.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain WIP1", FilterCollection.Contains(WIP1));
			Assert("Expecting collection not to contain Accrual1", !FilterCollection.Contains(Accrual1));
			Assert("Expecting collection not to contain WIP3", !FilterCollection.Contains(WIP3));

			filter.Property = branch3.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain WIP1", !FilterCollection.Contains(WIP1));
			Assert("Expecting collection not to contain Accrual2", !FilterCollection.Contains(Accrual2));
			Assert("Expecting collection not to contain WIP3", !FilterCollection.Contains(WIP3));
		}

		public void TestDepartmentFilter()
		{
			GlbDepartment department1 = Factory.NewWithValidTestData<GlbDepartment>();
			GlbDepartment department2 = Factory.NewWithValidTestData<GlbDepartment>();
			GlbDepartment department3 = Factory.NewWithValidTestData<GlbDepartment>();

			WIP1.RelatedJobCharge.JR_GE = WIP1.AL_GE = department1.PK;
			Accrual1.RelatedJobCharge.JR_GE = Accrual1.AL_GE = department2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Department"];

			filter.Property = department1.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain WIP1", FilterCollection.Contains(WIP1));
			Assert("Expecting collection not to contain Accrual1", !FilterCollection.Contains(Accrual1));

			filter.Property = department3.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain WIP1", !FilterCollection.Contains(WIP1));
			Assert("Expecting collection not to contain Accrual1", !FilterCollection.Contains(Accrual1));
		}

		public void TestAccountFilter()
		{
			OrgHeader organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			organisation1.OH_IsDebtor = true;
			organisation1.OH_IsCreditor = true;

			OrgHeader organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			organisation2.OH_IsDebtor = true;
			organisation2.OH_IsCreditor = true;

			OrgHeader organisation3 = Factory.NewWithValidTestData<OrgHeader>();
			organisation3.OH_IsDebtor = true;
			organisation3.OH_IsCreditor = true;

			WIP1.AL_OH = organisation1.PK;
			charge1.JR_OH_SellAccount = organisation1.PK;

			Accrual1.AL_OH = organisation2.PK;
			charge1.JR_OH_CostAccount = organisation2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Account"];

			filter.Property = organisation1.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain WIP1", FilterCollection.Contains(WIP1));
			Assert("Expecting collection not to contain Accrual1", !FilterCollection.Contains(Accrual1));

			filter.Property = organisation3.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain WIP1", !FilterCollection.Contains(WIP1));
			Assert("Expecting collection not to contain Accrual1", !FilterCollection.Contains(Accrual1));
		}

		public void TestChargeCodeFilter()
		{
			AccChargeCode code1 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode code2 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode code3 = Factory.NewWithValidTestData<AccChargeCode>();

			WIP1.RelatedJobCharge.JR_AC = WIP1.AL_AC = code1.PK;
			Accrual1.RelatedJobCharge.JR_AC = Accrual1.AL_AC = code2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Charge Code"];

			filter.Property = code1.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain WIP1", FilterCollection.Contains(WIP1));
			Assert("Expecting collection not to contain Accrual1", !FilterCollection.Contains(Accrual1));

			filter.Property = code3.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain WIP1", !FilterCollection.Contains(WIP1));
			Assert("Expecting collection not to contain Accrual1", !FilterCollection.Contains(Accrual1));
		}

		#endregion

		#region Other Filters Tests

		public void TestShowReversedTransactionsFilter()
		{
			WIP1.RelatedJobCharge.ReverseWIP(new ZDateTime(ZDateTime.Now.Year, 1, 1));

			using (SkipReportingWhenReversedWIPACRLinkedToJobChargeAttribute.ActivateTemporary())
			{
				WIP2.AL_ReverseDate = ZDateTime.Empty;
			}

			Accrual1.RelatedJobCharge.ReverseAccrual(new ZDateTime(ZDateTime.Now.Year, 2, 16));

			Factory.Save();

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain WIP1", !FilterCollection.Contains(WIP1));
			Assert("Expecting collection to contain WIP2", FilterCollection.Contains(WIP2));
			Assert("Expecting collection not to contain Accrual1", !FilterCollection.Contains(Accrual1));

			ModuleTextFilter showReversed = ((ModuleTextFilter)FilterBO["Show Reversed Transactions"]);
			showReversed.Property = "Reversed";

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain WIP1", FilterCollection.Contains(WIP1));
			Assert("Expecting collection not to contain WIP2", !FilterCollection.Contains(WIP2));
			Assert("Expecting collection to contain Accrual1", FilterCollection.Contains(Accrual1));

			showReversed.Property = "All";

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain WIP1", FilterCollection.Contains(WIP1));
			Assert("Expecting collection to contain WIP2", FilterCollection.Contains(WIP2));
			Assert("Expecting collection to contain Accrual1", FilterCollection.Contains(Accrual1));
		}

		public void TestTransactionTypeFilter()
		{
			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterBO["Transaction Type"];

			filter.Property = TransactionLineTypes.WIP;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain WIP1", FilterCollection.Contains(WIP1));
			Assert("Expecting collection not to contain Accrual1", !FilterCollection.Contains(Accrual1));

			filter.Property = TransactionLineTypes.Accrual;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain WIP1", !FilterCollection.Contains(WIP1));
			Assert("Expecting collection to contain Accrual1", FilterCollection.Contains(Accrual1));

			filter.Property = TransactionLineTypes.Revenue;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain WIP1", !FilterCollection.Contains(WIP1));
			Assert("Expecting collection not to contain Accrual1", !FilterCollection.Contains(Accrual1));
		}

		public void TestAmountFilter()
		{
			WIP1.AL_OSAmount = WIP1.AL_LineAmount = 10.0;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(WIP1);
			Accrual1.AL_OSAmount = Accrual1.AL_LineAmount = 100.0;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(Accrual1);

			Factory.Save();

			ModuleNumberRangeFilter filter = (ModuleNumberRangeFilter)FilterBO["Amount"];

			filter.Property1 = -10.0;
			filter.Property2 = 0.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expected collection to contain WIP1", FilterCollection.Contains(WIP1));
			Assert("Expected collection not to contain Accrual1", !FilterCollection.Contains(Accrual1));

			filter.Property1 = -100.0;
			filter.Property2 = -10.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expected collection to contain WIP1", FilterCollection.Contains(WIP1));
			Assert("Expected collection to contain Accrual1", FilterCollection.Contains(Accrual1));

			filter.Property1 = -90.0;
			filter.Property2 = -20.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expected collection not to contain WIP1", !FilterCollection.Contains(WIP1));
			Assert("Expected collection not to contain Accrual1", !FilterCollection.Contains(Accrual1));

			filter.Property1 = -300.0;
			filter.Property2 = -200.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expected collection not to contain WIP1", !FilterCollection.Contains(WIP1));
			Assert("Expected collection not to contain Accrual1", !FilterCollection.Contains(Accrual1));
		}

		public void TestShowOrphanWipOrAccrualFilterPresence()
		{
			AccountingMasterFilesRegistry.Instance.OrphanWIPOrACRDetection.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertNull((FilterBO["Show Only Orphan Transactions"]));

			AccountingMasterFilesRegistry.Instance.OrphanWIPOrACRDetection.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			FilterBO = (WIPAccrualsFilterBusinessObject)GetNewFilterStripBusinessObject();
			AssertNotNull((FilterBO["Show Only Orphan Transactions"]));
		}

		[SuspendCriticalValidation]
		//Critical validation needs to be suspended to detach charges from WIPs or Accruals
		public void TestShowOrphanWipOrAccrualFilter()
		{
			AccountingMasterFilesRegistry.Instance.OrphanWIPOrACRDetection.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			ModuleFlagsFilter flagsFilter = (ModuleFlagsFilter)FilterBO["Show Only Orphan Transactions"];
			flagsFilter["Yes"] = false;
			flagsFilter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals("All 6 WIPs and ACRs should come up in the filter", 6, FilterCollection.Count);

			WIP1.RelatedJobCharge.ReverseWIP(ZDateTime.Now);

			WIP1.AL_ReverseDate = ZDateTime.Empty;

			Accrual1.RelatedJobCharge.ReverseAccrual(ZDateTime.Now);

			Accrual1.AL_ReverseDate = ZDateTime.Empty;
			Factory.Save();

			flagsFilter = (ModuleFlagsFilter)FilterBO["Show Only Orphan Transactions"];
			flagsFilter["Yes"] = true;
			flagsFilter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(2, FilterCollection.Count);
			Assert(FilterCollection.Contains(WIP1));
			Assert(FilterCollection.Contains(Accrual1));

			ModuleTextFilter textFilter = (ModuleTextFilter)FilterBO["Transaction Type"];
			textFilter.Property = TransactionLineTypes.Accrual;
			textFilter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(Accrual1));
			Assert(!FilterCollection.Contains(WIP1));

			textFilter = (ModuleTextFilter)FilterBO["Transaction Type"];
			textFilter.Property = TransactionLineTypes.WIP;
			textFilter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(1, FilterCollection.Count);
			Assert(FilterCollection.Contains(WIP1));
			Assert(!FilterCollection.Contains(Accrual1));
		}

		public void TestBillingJobParentModuleFilter()
		{
			var newFactory = Factory.CreateNewFactory();
			var testObjectCreator = new TestObjectCreator(newFactory);

			var declaration = testObjectCreator.CreateDeclaration("001");
			var jobFordeclaration = testObjectCreator.CreateJobHeader();
			jobFordeclaration.JH_ParentID = declaration.PK;
			var accrualForDeclaration = testObjectCreator.CreateAccrual(jobFordeclaration);
			var wipForDeclaration = testObjectCreator.CreateWIP(jobFordeclaration);

			var shipment1 = testObjectCreator.CreateShipment("S001001", false);
			var job3 = testObjectCreator.CreateJob(shipment1, false, false);
			var accrualForShipment = testObjectCreator.CreateAccrual(job3);
			var wipForShipment = testObjectCreator.CreateWIP(job3);

			var shipment2 = testObjectCreator.CreateShipment("S001002", false);
			var jobForShipment2 = testObjectCreator.CreateJob(shipment2, false, false);
			var accrualForShipment2 = testObjectCreator.CreateAccrual(jobForShipment2);
			var wipForShipment2 = testObjectCreator.CreateWIP(jobForShipment2);

			newFactory.Save();

			var filter = (BillingJobParentModuleFilter)FilterBO["Billing Job Parent"];

			filter.IsActive = true;
			filter.SelectedModule = JobInvoicingConsumerTypes.Shipment.Code;

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = shipment1.PK;
			var result = newFactory.Load<AccTransactionLines>(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder("The filter result when comparison operator is 'exact' and value is not empty.",
				new List<ZGuid>(new ZGuid[] { accrualForShipment.PK, wipForShipment.PK }), result.Select(x => x.PK));

			filter.Property = shipment1.PK;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			result = newFactory.Load<AccTransactionLines>(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder("The filter result when comparison operator is 'not equal' and value is not empty.",
				new List<ZGuid>(new ZGuid[] { accrualForShipment2.PK, wipForShipment2.PK, accrualForDeclaration.PK, wipForDeclaration.PK }), result.Select(x => x.PK));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			filter.SelectedModule = JobInvoicingConsumerTypes.Shipment.Code;
			result = newFactory.Load<AccTransactionLines>(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder("The filter result when comparison operator is 'filters match' and no filter selected.",
				new List<ZGuid>(new ZGuid[] { accrualForShipment.PK, wipForShipment.PK, accrualForShipment2.PK, wipForShipment2.PK }), result.Select(x => x.PK));

			var shipmentModuleFilter = filter.SelectedFilters.AddFilterStrip<ModuleTextFilter>("Shipment #");
			shipmentModuleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			shipmentModuleFilter.Property = "S001002";
			result = newFactory.Load<AccTransactionLines>(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder("The filter result when comparison operator is 'filters match' and select shipment filter.",
				new List<ZGuid>(new ZGuid[] { accrualForShipment2.PK, wipForShipment2.PK }), result.Select(x => x.PK));
		}

		#endregion

		#region Implementation

		public void TestCollectionProperties()
		{
			AssertNotNull(FilterBO.BranchCollection);
			AssertNotNull(FilterBO.DepartmentCollection);
			AssertNotNull(FilterBO.OrganisationCollection);
			AssertNotNull(FilterBO.ChargeCodeCollection);
			AssertNotNull(FilterBO.TransactionTypeList);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new WIPAccrualsFilterBusinessObject();
		}

		WIP WIP1;
		WIP WIP2;
		WIP WIP3;

		Accrual Accrual1;
		Accrual Accrual2;
		Accrual Accrual3;

		JobCharge charge1;
		JobCharge charge2;
		JobCharge charge3;
		JobCharge charge4;
		JobCharge charge5;
		JobCharge charge6;

		JobHeader Job1;
		JobHeader Job2;
		JobHeader Job3;

		WIPAccrualCollection FilterCollection;
		WIPAccrualsFilterBusinessObject FilterBO;

		protected override void SetUp()
		{
			base.SetUp();

			Job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			Job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			Job3 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();

			Job1.JH_GB = GlbBranch.CurrentBranch.PK;
			Job2.JH_GB = GlbBranch.CurrentBranch.PK;
			Job3.JH_GB = GlbBranch.CurrentBranch.PK;

			Job1.JH_GC = GlbCompany.CurrentCompany.PK;
			Job2.JH_GC = GlbCompany.CurrentCompany.PK;
			Job3.JH_GC = GlbCompany.CurrentCompany.PK;

			charge1 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_JH = Job1.PK;
			charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge2.JR_JH = Job2.PK;
			charge3 = Factory.NewWithValidTestData<JobCharge>();
			charge3.JR_JH = Job3.PK;

			charge4 = Factory.NewWithValidTestData<JobCharge>();
			charge4.JR_JH = Job1.PK;
			charge5 = Factory.NewWithValidTestData<JobCharge>();
			charge5.JR_JH = Job2.PK;
			charge6 = Factory.NewWithValidTestData<JobCharge>();
			charge6.JR_JH = Job3.PK;

			WIP1 = Factory.NewWithValidTestData<WIP>();
			charge1.JR_AL_ARLine = WIP1.PK;
			WIP1.AL_JH = Job1.PK;
			WIP1.AL_AC = charge1.JR_AC;
			charge1.SetChargeValuesFromLinkedARLineForTests();
			WIP2 = Factory.NewWithValidTestData<WIP>();
			charge2.JR_AL_ARLine = WIP2.PK;
			WIP2.AL_JH = Job2.PK;
			WIP2.AL_AC = charge2.JR_AC;
			charge2.SetChargeValuesFromLinkedARLineForTests();
			WIP3 = Factory.NewWithValidTestData<WIP>();
			charge3.JR_AL_ARLine = WIP3.PK;
			WIP3.AL_JH = Job3.PK;
			WIP3.AL_AC = charge3.JR_AC;
			charge3.SetChargeValuesFromLinkedARLineForTests();

			Accrual1 = Factory.NewWithValidTestData<Accrual>();
			charge4.JR_AL_APLine = Accrual1.PK;
			Accrual1.AL_JH = Job1.PK;
			Accrual1.AL_AC = charge4.JR_AC;
			charge4.SetChargeValuesFromLinkedAPLineForTests();
			Accrual2 = Factory.NewWithValidTestData<Accrual>();
			charge5.JR_AL_APLine = Accrual2.PK;
			Accrual2.AL_JH = Job2.PK;
			Accrual2.AL_AC = charge5.JR_AC;
			charge5.SetChargeValuesFromLinkedAPLineForTests();
			Accrual3 = Factory.NewWithValidTestData<Accrual>();
			charge6.JR_AL_APLine = Accrual3.PK;
			Accrual3.AL_JH = Job3.PK;
			Accrual3.AL_AC = charge6.JR_AC;
			charge6.SetChargeValuesFromLinkedAPLineForTests();

			FilterCollection = new WIPAccrualCollection(Factory);
			FilterBO = (WIPAccrualsFilterBusinessObject)GetNewFilterStripBusinessObject();
		}

		#endregion
	}
}
