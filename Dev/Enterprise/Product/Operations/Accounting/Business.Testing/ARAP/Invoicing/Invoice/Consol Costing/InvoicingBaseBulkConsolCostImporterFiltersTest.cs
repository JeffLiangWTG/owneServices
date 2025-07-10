using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.GenericConsol;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(InvoicingBaseBulkConsolCostImporterFilters))]
	public class InvoicingBaseBulkConsolCostImporterFiltersTest : InvoiceBulkOperationFiltersTest
	{
		public void TestExcludeReverseSignedCostsFilter_Invoice()
		{
			AssertReverseSignedCost<APInvoice>(2);
		}

		public void TestExcludeReverseSignedCostsFilter_CreditNote()
		{
			AssertReverseSignedCost<APCreditNote>(1);
		}

		public override void TestRelatedMilestoneFilter()
		{
			AssertNull((ModuleDateFilter)Filters["Milestone Date (Related)"]);
			AssertNull((ModuleTextFilter)Filters["Milestone Completed (Related)"]);
			AssertNull((ModuleDateFilter)Filters["Next Milestone (Related)"]);
			AssertNull((ModuleDateFilter)Filters["Last Completed Milestone (Related)"]);
		}

		public void TestMileStoneFiltersWithSubGroup()
		{
			AssertNotNull((ModuleDateFilter)Filters["Milestone Date"]);
			AssertNotNull((ModuleTextFilter)Filters["Milestone Completed"]);
			AssertNotNull((ModuleDateFilter)Filters["Next Milestone"]);
			AssertNotNull((ModuleDateFilter)Filters["Last Completed Milestone"]);

			var invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;

			var consol1 = CreateConsolWithShipment("C1");
			var consol2 = CreateConsolWithShipment("C2");
			var consol3 = CreateConsolWithShipment("C3");
			var consol4 = CreateConsolWithShipment("C4");

			var cost1 = CreateCostsAndMileStones(consol1, "LST", new ZDateTime(2014, 3, 11), ZDateTimeOffset.Empty);
			var cost2 = CreateCostsAndMileStones(consol2, "LST", ZDateTime.Empty, ZDateTimeOffset.Empty);
			var cost3 = CreateCostsAndMileStones(consol3, "NXT", ZDateTime.Empty, new ZDateTimeOffset(new ZDateTime(2014, 5, 18)));
			var cost4 = CreateCostsAndMileStones(consol4, "LST", new ZDateTime(2014, 8, 6), ZDateTimeOffset.Empty);

			var filters = new InvoicingBaseBulkConsolCostImporterFilters(invoice);
			var milestoneDatefilter = (ModuleDateFilter)filters["Milestone Date"];
			milestoneDatefilter.IsActive = true;
			milestoneDatefilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			milestoneDatefilter.Property1 = new ZDateTime(2014, 3, 10);
			milestoneDatefilter.Property2 = new ZDateTime(2014, 3, 13);

			var consolCollection = GetCollection(filters.GetQuery());
			AssertContainsExactElementsInAnyOrder(new[] { consol1.PK }, consolCollection.Select(x => x.PK));
			var costCollection = GetCostCollection(filters.GetChildQuery(), consolCollection);
			AssertContainsExactElementsInAnyOrder(new[] { cost1 }, costCollection);

			filters = new InvoicingBaseBulkConsolCostImporterFilters(invoice);
			var milestoneCompletedFilter = (ModuleTextFilter)filters["Milestone Completed"];
			milestoneCompletedFilter.IsActive = true;
			milestoneCompletedFilter.Property = "Not Completed";

			var nextMilestoneFilter = (ModuleDateFilter)filters["Next Milestone"];
			nextMilestoneFilter.IsActive = true;
			nextMilestoneFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			nextMilestoneFilter.Property1 = new ZDateTime(2014, 5, 17);
			nextMilestoneFilter.Property2 = new ZDateTime(2014, 5, 19);

			consolCollection = GetCollection(filters.GetQuery());
			AssertContainsExactElementsInAnyOrder(new[] { consol3.PK }, consolCollection.Select(x => x.PK));
			costCollection = GetCostCollection(filters.GetChildQuery(), consolCollection);
			AssertContainsExactElementsInAnyOrder(new[] { cost3 }, costCollection);

			filters = new InvoicingBaseBulkConsolCostImporterFilters(invoice);
			var lastCompletedMilestoneFilter = (ModuleDateFilter)filters["Last Completed Milestone"];
			lastCompletedMilestoneFilter.IsActive = true;
			lastCompletedMilestoneFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			lastCompletedMilestoneFilter.Property1 = new ZDateTime(2014, 8, 5);
			lastCompletedMilestoneFilter.Property2 = new ZDateTime(2014, 8, 7);

			consolCollection = GetCollection(filters.GetQuery());
			AssertContainsExactElementsInAnyOrder(new[] { consol4.PK }, consolCollection.Select(x => x.PK));
			costCollection = GetCostCollection(filters.GetChildQuery(), consolCollection);
			AssertContainsExactElementsInAnyOrder(new[] { cost4 }, costCollection);
		}

		public void TestTaskAssignedToFiltersWithSubGroup()
		{
			AssertNotNull((ModuleNkFilter)Filters["Any Open Task Assigned To"]);
			AssertNotNull((ModuleNkFilter)Filters["Next Task Assigned To"]);

			var invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;

			var bizO = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var consol1 = CreateConsolWithShipment("C1");
			var consol2 = CreateConsolWithShipment("C2");
			var consol3 = CreateConsolWithShipment("C3");
			var consol4 = CreateConsolWithShipment("C4");

			var cost1 = CreateCostsAndTasks(bizO, consol1, "WRK", "SM1", 1);
			var cost2 = CreateCostsAndTasks(bizO, consol2, "WRK", "SM2", 1);
			var cost3 = CreateCostsAndTasks(bizO, consol3, "WRK", "SM2", 2);
			var cost4 = CreateCostsAndTasks(bizO, consol4, "WRK", "SM3", 3);

			var filters = new InvoicingBaseBulkConsolCostImporterFilters(invoice);
			var anyTaskAssignedTofilter = (ModuleNkFilter)filters["Any Open Task Assigned To"];
			anyTaskAssignedTofilter.Property = "SM1";
			anyTaskAssignedTofilter.IsActive = true;

			var consolCollection = GetCollection(filters.GetQuery());
			AssertContainsExactElementsInAnyOrder(new[] { consol1.PK }, consolCollection.Select(x => x.PK));
			var costCollection = GetCostCollection(filters.GetChildQuery(), consolCollection);
			AssertContainsExactElementsInAnyOrder(new[] { cost1 }, costCollection);

			filters = new InvoicingBaseBulkConsolCostImporterFilters(invoice);
			var nextTaskAssignedTofilter = (ModuleNkFilter)filters["Next Task Assigned To"];
			nextTaskAssignedTofilter.IsActive = true;
			nextTaskAssignedTofilter.Property = "SM2";

			consolCollection = GetCollection(filters.GetQuery());
			AssertContainsExactElementsInAnyOrder(new[] { consol2.PK, consol3.PK }, consolCollection.Select(x => x.PK));
			costCollection = GetCostCollection(filters.GetChildQuery(), consolCollection);
			AssertContainsExactElementsInAnyOrder(new[] { cost2, cost3 }, costCollection);
		}

		public void TestTasksFiltersWithSubGroup()
		{
			AssertNotNull((ModuleGuidForeignCollectionFilter)Filters["Tasks"]);

			var invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;

			var bizO = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var consol1 = CreateConsolWithShipment("C1");
			var consol2 = CreateConsolWithShipment("C2");

			var cost1 = TestObjectCreator.CreateConsolCost(consol1, TestObjectCreator.CC1, 10m, TestObjectCreator.AALSHI);
			var cost2 = TestObjectCreator.CreateConsolCost(consol2, TestObjectCreator.CC1, 10m, TestObjectCreator.AALSHI);

			var openTask = consol1.WorkflowItems.Tasks.AddNew();
			openTask.P9_Description = "Open task";
			openTask.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			openTask.P9_Status = "ASN";

			var closedTask = consol2.WorkflowItems.Tasks.AddNew();
			closedTask.P9_Description = "Closed task";
			closedTask.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			closedTask.P9_Status = "CLS";

			Factory.Save();

			var filters = new InvoicingBaseBulkConsolCostImporterFilters(invoice);
			var filter = filters.AddFilterStrip<ModuleGuidForeignCollectionFilter>("Tasks");
			filter.SelectedFilters.AddTextFilterStrip("Status", "CLS");

			var subFilterResult = Factory.Load<ProcessTask>(filter.SelectedFilters.Filter);
			AssertContainsExactElementsInAnyOrder("Should have found the closed task", new[] { closedTask }, subFilterResult);

			var consolCollection = GetCollection(filters.GetQuery());
			AssertContainsExactElementsInAnyOrder(new[] { consol2.PK }, consolCollection.Select(x => x.PK));
			var costCollection = GetCostCollection(filters.GetChildQuery(), consolCollection);
			AssertContainsExactElementsInAnyOrder(new[] { cost2.PK }, costCollection.Select(x => x.PK));
		}

		public override void TestDefaultCreditorFilterValueOnApplyDefaults_WhenTaxBranchReportingIsON()
		{
			var defaulttaxBranch = TestObjectCreator.CreateNewBranch(GlbCompany.CurrentCompany, "BR9");
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var expectedCreditorValue = TestObjectCreator.AALSHI.PK;
				AssertNotNull(Filters);
				Filters.DefaultCreditorFilterValue_ForTestOnly = expectedCreditorValue;
				Filters.DefaultTaxBranchFilterValue_ForTestOnly = defaulttaxBranch.PK;
				Filters.GetQuery();

				((ModuleGuidFilter)Filters[InvoiceBulkOperationFilterHelper.CreditorFilterName]).Property = TestObjectCreator.ABIGAS.PK;
				Assert("Should be Tax Branch Reporting enabled", AccountingMasterFilesUtils.IsTaxBranchApplicable);
				((ModuleGuidFilter)Filters[InvoiceBulkOperationFilterHelper.TaxBranchFilterName]).Property = GlbBranch.CurrentBranch.PK;

				Filters.ApplyDefaults_ForTestOnly();

				AssertEquals("Creditor filter must have default value.", expectedCreditorValue, ((ModuleGuidFilter)Filters[InvoiceBulkOperationFilterHelper.CreditorFilterName]).Property);
			}
		}

		public void TestExceptionsFiltersWithSubGroup()
		{
			AssertNotNull((ModuleGuidForeignCollectionFilter)Filters["Exceptions"]);

			var invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;

			var bizO = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var consol1 = CreateConsolWithShipment("C1");
			var consol2 = CreateConsolWithShipment("C2");

			var cost1 = TestObjectCreator.CreateConsolCost(consol1, TestObjectCreator.CC1, 10m, TestObjectCreator.AALSHI);
			var cost2 = TestObjectCreator.CreateConsolCost(consol2, TestObjectCreator.CC1, 10m, TestObjectCreator.AALSHI);

			var openException = consol1.WorkflowItems.Exceptions.AddNew();
			openException.IsExceptionActioned = false;
			openException.P9_Description = "Open exception";

			var closedException = consol2.WorkflowItems.Exceptions.AddNew();
			closedException.IsExceptionActioned = true;
			closedException.P9_Description = "Closed exception";

			Factory.Save();

			var filters = new InvoicingBaseBulkConsolCostImporterFilters(invoice);
			var filter = (ModuleGuidForeignCollectionFilter)filters["Exceptions"];
			((ModuleTextFilter)filter.SelectedFilters.ActiveModuleFilters[0]).Property = ExceptionStatusCodeList.Codes.Actioned;

			var subFilterResult = Factory.Load<ProcessTask>(filter.SelectedFilters.Filter);
			AssertContainsExactElementsInAnyOrder("Should have found the closed exception", new[] { closedException }, subFilterResult);

			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;

			var consolCollection = GetCollection(filters.GetQuery());
			AssertContainsExactElementsInAnyOrder(new[] { consol2.PK }, consolCollection.Select(x => x.PK));
			var costCollection = GetCostCollection(filters.GetChildQuery(), consolCollection);
			AssertContainsExactElementsInAnyOrder(new[] { cost2.PK }, costCollection.Select(x => x.PK));
		}

		void AssertReverseSignedCost<T>(int expectedCostNumber) where T : InvoicingBase
		{
			InvoicingBase invoice = Factory.NewWithValidTestData<T>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;

			JobConsolCost cost1 = Factory.NewWithValidTestData<JobConsolCost>();
			JobConsolCost cost2 = Factory.NewWithValidTestData<JobConsolCost>();
			cost1.E6_OH_Creditor = cost2.E6_OH_Creditor = TestObjectCreator.AALSHI.PK;
			cost1.E6_GC = cost2.E6_GC = GlbCompany.CurrentCompany.PK;
			cost1.E6_OSCostAmount = -1.1m;
			cost1.E6_LocalCostAmount = -1.1m;
			cost2.E6_OSCostAmount = 1.1m;
			cost2.E6_LocalCostAmount = 1.1m;
			JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			charge2.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_E6 = cost1.PK;
			charge2.JR_E6 = cost2.PK;
			charge1.JR_OSCostAmt = -1.1m;
			charge1.JR_LocalCostAmt = -1.1m;
			charge2.JR_OSCostAmt = 1.1m;
			charge2.JR_LocalCostAmt = 1.1m;

			Factory.Save();

			InvoicingBaseBulkConsolCostImporterFilters filters = new InvoicingBaseBulkConsolCostImporterFilters(invoice);

			JobConsolCost[] costs = Factory.Load<JobConsolCost>(filters.GetChildQuery());
			AssertEquals("Both costs should be loaded", 2, costs.Length);

			((ModuleFlagsFilter)filters["ExcludeReverseSignedConsolCostsFilter"]).Property0 = true;
			filters["ExcludeReverseSignedConsolCostsFilter"].IsActive = true;
			costs = Factory.Load<JobConsolCost>(filters.GetChildQuery());
			AssertEquals("Only one cost should be loaded", 1, costs.Length);
			AssertEquals("Only this cost should be loaded", (expectedCostNumber == 1 ? cost1 : cost2).PK, costs[0].PK);
		}

		public void TestCreditorFilter()
		{
			AssertEquals("This is mandatory filter.", FilterVisibility.AlwaysVisible, Filters["Creditor"].Visibility);
			AssertEquals("This is mandatory filter.", true, Filters["Creditor"].ReadOnly);

			ForwardingConsol consol1 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C1");
			ForwardingShipment shipment = consol1.Shipments.AddNew();
			JobConsolCost cost = TestObjectCreator.CreateConsolCost(consol1, TestObjectCreator.CC1, TestObjectCreator.AALSHI);
			cost.E6_OSCostAmount = 10M;
			ForwardingConsol consol2 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C2");
			shipment = consol2.Shipments.AddNew();
			cost = TestObjectCreator.CreateConsolCost(consol2, TestObjectCreator.CC1, null);
			cost.E6_OSCostAmount = 11M;
			Factory.Save();

			ActivateAlwaysVisibleFilters();

			AssertFilterFinds(consol1, Filters.GetQuery());
			AssertFilterDoesNotFind(consol2, Filters.GetQuery());
		}

		public void TestConsolCostOwnerFilter()
		{
			ForwardingConsol consol1 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C1");
			ForwardingShipment shipment1 = consol1.Shipments.AddNew();
			JobConsolCost cost1 = TestObjectCreator.CreateConsolCost(consol1, TestObjectCreator.CC1, TestObjectCreator.AALSHI);
			cost1.E6_OSCostAmount = 10m;
			cost1.E6_GS_NKConsolCostOwner = GlbStaff.CurrentUser.GS_Code;
			ForwardingConsol consol2 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C2");
			ForwardingShipment shipment2 = consol2.Shipments.AddNew();
			JobConsolCost cost2 = TestObjectCreator.CreateConsolCost(consol2, TestObjectCreator.CC1, TestObjectCreator.AALSHI);
			cost2.E6_OSCostAmount = 20m;
			cost2.E6_GS_NKConsolCostOwner = "CCC";
			Factory.Save();

			ActivateAlwaysVisibleFilters();

			((ModuleNkFilter)Filters[InvoiceBulkOperationFilterHelper.ConsolCostOwnerFilterName]).Property = GlbStaff.CurrentUser.GS_Code;
			((ModuleNkFilter)Filters[InvoiceBulkOperationFilterHelper.ConsolCostOwnerFilterName]).IsActive = true;

			AssertFilterFinds(consol1, Filters.GetQuery());
			AssertFilterDoesNotFind(consol2, Filters.GetQuery());
		}

		public void TestContainerNumberFilter()
		{
			ForwardingConsol consol1 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C1");
			ForwardingShipment shipment = consol1.Shipments.AddNew();
			JobConsolCost cost = TestObjectCreator.CreateConsolCost(consol1, TestObjectCreator.CC1, TestObjectCreator.AALSHI);
			cost.E6_OSCostAmount = 10M;
			ForwardingConsol consol2 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C2");
			ForwardingContainer fc1 = Factory.NewWithValidTestData<ForwardingContainer>();
			fc1.JC_ContainerNum = "123";
			fc1.JC_JK = consol1.PK;
			shipment = consol2.Shipments.AddNew();
			cost = TestObjectCreator.CreateConsolCost(consol2, TestObjectCreator.CC1, null);
			cost.E6_OSCostAmount = 11M;
			Factory.Save();

			ActivateAlwaysVisibleFilters();

			((ModuleTextFilter)Filters["Container Number"]).Property = "123";
			((ModuleTextFilter)Filters["Container Number"]).IsActive = true;

			AssertFilterFinds(consol1, Filters.GetQuery());
			AssertFilterDoesNotFind(consol2, Filters.GetQuery());
		}

		public void TestIncludeConsolCostsWithNoCreditorFilter()
		{
			ForwardingConsol consol1 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C1");
			ForwardingShipment shipment = consol1.Shipments.AddNew();
			JobConsolCost cost = TestObjectCreator.CreateConsolCost(consol1, TestObjectCreator.CC1, TestObjectCreator.AALSHI);
			cost.E6_OSCostAmount = 10M;
			ForwardingConsol consol2 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C2");
			shipment = consol2.Shipments.AddNew();
			cost = TestObjectCreator.CreateConsolCost(consol2, TestObjectCreator.CC1, null);
			cost.E6_OSCostAmount = 11M;
			Factory.Save();

			ActivateAlwaysVisibleFilters();

			ModuleFlagsFilter filter = (ModuleFlagsFilter)Filters["IncludeConsolCostsWithNoCreditor"];
			filter.IsActive = true;
			filter.Property0 = false;

			AssertFilterFinds(consol1, Filters.GetQuery());
			AssertFilterDoesNotFind(consol2, Filters.GetQuery());

			filter.Property0 = true;
			AssertFilterFinds(consol1, Filters.GetQuery());
			AssertFilterFinds(consol2, Filters.GetQuery());
		}

		public void TestAccrualPostFilters()
		{
			ForwardingConsol consol01JAN2005 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C1");
			ForwardingShipment shipment01JAN2005 = consol01JAN2005.Shipments.AddNew();
			JobConsolCost cost01JAN2005 = TestObjectCreator.CreateConsolCost(consol01JAN2005, TestObjectCreator.CC1, TestObjectCreator.AALSHI);
			cost01JAN2005.E6_LocalCostAmount = 100m;
			cost01JAN2005.E6_ApportionmentMethod = "SHP";
			AssertEquals(1, cost01JAN2005.ApportionmentCharges.Count);
			AssertEquals(100m, cost01JAN2005.ApportionmentCharges[0].JR_LocalCostAmt);
			AssertNotNull(cost01JAN2005.ApportionmentCharges[0].JR_AL_APLine);

			ForwardingConsol consol01FEB2005 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C2");
			ForwardingShipment shipment01FEB2005 = consol01FEB2005.Shipments.AddNew();
			JobConsolCost cost01FEB2005 = TestObjectCreator.CreateConsolCost(consol01FEB2005, TestObjectCreator.CC1, TestObjectCreator.AALSHI);
			cost01FEB2005.E6_LocalCostAmount = 100m;
			cost01FEB2005.E6_ApportionmentMethod = "SHP";
			AssertEquals(1, cost01FEB2005.ApportionmentCharges.Count);
			AssertEquals(100m, cost01FEB2005.ApportionmentCharges[0].JR_LocalCostAmt);
			AssertNotNull(cost01FEB2005.ApportionmentCharges[0].JR_AL_APLine);
			Factory.Save();

			Accrual aCR01JAN2005 = Factory.Load<Accrual>(cost01JAN2005.ApportionmentCharges[0].JR_AL_APLine);
			AssertNotNull(aCR01JAN2005);
			aCR01JAN2005.AL_PostDate = Date01JAN2005;

			Accrual aCR01FEB2005 = Factory.Load<Accrual>(cost01FEB2005.ApportionmentCharges[0].JR_AL_APLine);
			AssertNotNull(aCR01FEB2005);
			aCR01FEB2005.AL_PostDate = Date01FEB2005;
			Factory.Save();

			AssertDateFiltersWork(consol01JAN2005, consol01FEB2005, (ModuleDateFilter)Filters["PostDate"]);
		}

		public void TestShipmentETAFilters()
		{
			ForwardingConsol consol01JAN2005 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C1");
			Transport transport01JAN2005 = consol01JAN2005.Transports[0];
			transport01JAN2005.JW_IsLinked = false;
			transport01JAN2005.JW_ETA = Date01JAN2005;
			transport01JAN2005.JW_ETD = ZDateTime.Empty;
			AssertEquals(Date01JAN2005, consol01JAN2005.JK_JX_JB_E_ARV);

			ForwardingConsol consol01FEB2005 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C2");
			Transport transport01FEB2005 = consol01FEB2005.Transports[0];
			transport01FEB2005.JW_IsLinked = false;
			transport01FEB2005.JW_ETA = Date01FEB2005;
			transport01FEB2005.JW_ETD = ZDateTime.Empty;
			AssertEquals(Date01FEB2005, consol01FEB2005.JK_JX_JB_E_ARV);

			JobConsolCost cost1 = TestObjectCreator.CreateConsolCost(consol01JAN2005, TestObjectCreator.CC1, TestObjectCreator.AALSHI);
			JobConsolCost cost2 = TestObjectCreator.CreateConsolCost(consol01FEB2005, TestObjectCreator.CC1, TestObjectCreator.AALSHI);
			cost1.E6_OSCostAmount = cost2.E6_OSCostAmount = 10m;
			JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			charge2.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_E6 = cost1.PK;
			charge2.JR_E6 = cost2.PK;
			charge1.JR_AT_CostGSTRate = cost1.E6_AT_TaxRate;
			charge2.JR_AT_CostGSTRate = cost2.E6_AT_TaxRate;
			charge1.JR_OSCostAmt = charge2.JR_OSCostAmt = charge1.JR_LocalCostAmt = charge2.JR_LocalCostAmt = 10m;
			charge1.JR_OSCostGSTAmt_Calc = charge2.JR_OSCostGSTAmt_Calc = 1m;

			Factory.Save();

			AssertDateFiltersWork(consol01JAN2005, consol01FEB2005, (ModuleDateFilter)Filters["ShipmentETA_ETD"]);

			transport01JAN2005.JW_ETA = ZDateTime.Empty;
			transport01JAN2005.JW_IsLinked = true;
			transport01JAN2005.JW_JX = Factory.NewWithValidTestData<JobSailing>().PK;
			transport01JAN2005.Sailing.Destination.JB_E_ARV = Date01JAN2005;
			transport01FEB2005.JW_ETA = ZDateTime.Empty;
			transport01FEB2005.JW_IsLinked = true;
			transport01FEB2005.JW_JX = Factory.NewWithValidTestData<JobSailing>().PK;
			transport01FEB2005.Sailing.Destination.JB_E_ARV = Date01FEB2005;

			Factory.Save();

			AssertDateFiltersWork(consol01JAN2005, consol01FEB2005, (ModuleDateFilter)Filters["ShipmentETA_ETD"]);
		}

		public void TestShipmentETDFilters()
		{
			ForwardingConsol consol01JAN2005 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C1");
			Transport transport01JAN2005 = consol01JAN2005.Transports[0];
			transport01JAN2005.JW_IsLinked = false;
			transport01JAN2005.JW_ETA = ZDateTime.Empty;
			transport01JAN2005.JW_ETD = Date01JAN2005;
			AssertEquals(Date01JAN2005, consol01JAN2005.JK_JX_JA_E_DEP);

			ForwardingConsol consol01FEB2005 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C2");
			Transport transport01FEB2005 = consol01FEB2005.Transports[0];
			transport01FEB2005.JW_IsLinked = false;
			transport01FEB2005.JW_ETA = ZDateTime.Empty;
			transport01FEB2005.JW_ETD = Date01FEB2005;
			AssertEquals(Date01FEB2005, consol01FEB2005.JK_JX_JA_E_DEP);

			JobConsolCost cost1 = TestObjectCreator.CreateConsolCost(consol01JAN2005, TestObjectCreator.CC1, TestObjectCreator.AALSHI);
			JobConsolCost cost2 = TestObjectCreator.CreateConsolCost(consol01FEB2005, TestObjectCreator.CC1, TestObjectCreator.AALSHI);
			cost1.E6_OSCostAmount = cost2.E6_OSCostAmount = 10m;
			JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			charge2.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_E6 = cost1.PK;
			charge2.JR_E6 = cost2.PK;
			charge1.JR_AT_CostGSTRate = cost1.E6_AT_TaxRate;
			charge2.JR_AT_CostGSTRate = cost2.E6_AT_TaxRate;
			charge1.JR_OSCostAmt = charge2.JR_OSCostAmt = charge1.JR_LocalCostAmt = charge2.JR_LocalCostAmt = 10m;
			charge1.JR_OSCostGSTAmt_Calc = charge2.JR_OSCostGSTAmt_Calc = 1m;

			Factory.Save();

			AssertDateFiltersWork(consol01JAN2005, consol01FEB2005, (ModuleDateFilter)Filters["ShipmentETA_ETD"]);

			transport01JAN2005.JW_ETD = ZDateTime.Empty;
			transport01JAN2005.JW_IsLinked = true;
			transport01JAN2005.JW_JX = Factory.NewWithValidTestData<JobSailing>().PK;
			transport01JAN2005.Sailing.Origin.JA_E_DEP = Date01JAN2005;
			transport01FEB2005.JW_ETD = ZDateTime.Empty;
			transport01FEB2005.JW_IsLinked = true;
			transport01FEB2005.JW_JX = Factory.NewWithValidTestData<JobSailing>().PK;
			transport01FEB2005.Sailing.Origin.JA_E_DEP = Date01FEB2005;

			Factory.Save();

			AssertDateFiltersWork(consol01JAN2005, consol01FEB2005, (ModuleDateFilter)Filters["ShipmentETA_ETD"]);
		}

		public void TestConsolCostsAreForCurrentCompany()
		{
			ActivateAlwaysVisibleFilters();
			ZQuery query = Filters.GetQuery();
			string expectedCompanyFilter = JobConsolCostSchema.E6_GC.Name + " = '{0}'";
			expectedCompanyFilter = string.Format(expectedCompanyFilter, GlbCompany.CurrentCompany.PK.ToString());
			Assert(query.LiteralTextSqlFormatted.Contains(expectedCompanyFilter));

			AccChargeCode chargeToUse = TestObjectCreator.CC1;

			ForwardingConsol consolWithCostInCurrentCompany = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C1");
			ForwardingConsol consolWithCostInNonCurrentCompany = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C2");
			Factory.Save();

			JobConsolCost costInOtherCompany = TestObjectCreator.CreateConsolCostInNonCurrentCompany(consolWithCostInNonCurrentCompany, chargeToUse, TestObjectCreator.AALSHI);
			JobConsolCost costInCurrentCompany = TestObjectCreator.CreateConsolCost(consolWithCostInCurrentCompany, chargeToUse, TestObjectCreator.AALSHI);
			costInOtherCompany.E6_OSCostAmount = costInCurrentCompany.E6_OSCostAmount = 10m;
			JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			charge2.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_E6 = costInOtherCompany.PK;
			charge2.JR_E6 = costInCurrentCompany.PK;
			charge1.JR_AT_CostGSTRate = costInOtherCompany.E6_AT_TaxRate;
			charge2.JR_AT_CostGSTRate = costInCurrentCompany.E6_AT_TaxRate;
			costInOtherCompany.E6_OSCostAmount = costInCurrentCompany.E6_OSCostAmount = charge1.JR_OSCostAmt = charge2.JR_OSCostAmt = charge1.JR_LocalCostAmt = charge2.JR_LocalCostAmt = 10m;

			Factory.Save();

			ActivateAlwaysVisibleFilters();
			AssertFilterFinds(consolWithCostInCurrentCompany, query);
			AssertFilterDoesNotFind(consolWithCostInNonCurrentCompany, query);
		}

		public void TestConsolCostsFilters()
		{
			AccChargeCode chargeToUse = TestObjectCreator.CC1;
			OrgHeader creditorToUse = TestObjectCreator.AALSHI;

			ModuleGuidFilter chargeCodefilter = (ModuleGuidFilter)Filters["ChargeCode"];
			chargeCodefilter.Property = chargeToUse.PK;
			chargeCodefilter.IsActive = true;
			ModuleGuidFilter creditorFilter = (ModuleGuidFilter)Filters["Creditor"];
			creditorFilter.Property = creditorToUse.PK;
			creditorFilter.IsActive = true;
			ZQuery query = Filters.GetQuery();

			ForwardingConsol consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C1");
			Factory.Save();

			JobConsolCost costWithCreditor = TestObjectCreator.CreateConsolCostInNonCurrentCompany(consol, TestObjectCreator.CC2, creditorToUse);
			JobConsolCost costWithChargeCode = TestObjectCreator.CreateConsolCost(consol, chargeToUse, TestObjectCreator.Creditor1);
			costWithCreditor.E6_OSCostAmount = costWithChargeCode.E6_OSCostAmount = 10m;
			JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_OH_CostAccount = creditorToUse.PK;
			charge2.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			charge1.JR_E6 = costWithCreditor.PK;
			charge2.JR_E6 = costWithChargeCode.PK;
			charge1.JR_AT_CostGSTRate = costWithCreditor.E6_AT_TaxRate;
			charge2.JR_AT_CostGSTRate = costWithChargeCode.E6_AT_TaxRate;
			costWithCreditor.E6_OSCostAmount = costWithChargeCode.E6_OSCostAmount = charge1.JR_OSCostAmt = charge2.JR_OSCostAmt = charge1.JR_LocalCostAmt = charge2.JR_LocalCostAmt = 10m;

			Factory.Save();

			AssertFilterDoesNotFind(consol, query);
		}

		public void TestInvoiceDetailsFilter()
		{
			Invoice.AH_TransactionNum = "QQQQQ";
			ForwardingConsol consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C1");
			JobConsolCost cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.AALSHI);
			cost.E6_OSCostAmount = 100M;
			cost.E6_InvoiceNum = "QQQQQ";
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			charge.JR_APInvoiceNum = "QQQQQ";
			charge.JR_E6 = cost.PK;
			charge.JR_RX_NKCostCurrency = "AUD";
			charge.JR_OSCostAmt = cost.E6_OSCostAmount;
			charge.JR_LocalCostAmt = cost.E6_OSCostAmount;
			charge.JR_AT_CostGSTRate = cost.E6_AT_TaxRate;
			Factory.Save();
			ActivateAlwaysVisibleFilters();
			AssertFilterFinds(consol, Filters.GetQuery());

			cost.E6_InvoiceNum = "AAA";
			charge.JR_APInvoiceNum = "AAA";
			Factory.Save();
			AssertFilterDoesNotFind(consol, Filters.GetQuery());
		}

		public void TestChildCollectionForFiltersWithCategories_ChargeCode()
		{
			ForwardingConsol consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C1");
			Factory.Save();
			JobConsolCost cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.AALSHI);
			cost.E6_OSCostAmount = 100M;
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			charge.JR_E6 = cost.PK;
			charge.JR_OSCostAmt = cost.E6_OSCostAmount;
			charge.JR_LocalCostAmt = cost.E6_OSCostAmount;
			charge.JR_AT_CostGSTRate = cost.E6_AT_TaxRate;
			ForwardingConsol consol2 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C2");
			JobConsolCost cost2 = TestObjectCreator.CreateConsolCost(consol2, TestObjectCreator.CC2, TestObjectCreator.AALSHI);
			cost2.E6_OSCostAmount = 200M;
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge2.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			charge2.JR_E6 = cost2.PK;
			charge2.JR_OSCostAmt = cost2.E6_OSCostAmount;
			charge2.JR_LocalCostAmt = cost2.E6_OSCostAmount;
			charge2.JR_AT_CostGSTRate = cost2.E6_AT_TaxRate;
			Factory.Save();

			cost = Factory.Load<InvoicingBaseConsolCostForImporting>(cost.PK);
			cost2 = Factory.Load<InvoicingBaseConsolCostForImporting>(cost2.PK);

			FilterStripCollection filterStrips = new FilterStripCollection(Filters.ModuleFilters);
			FilterStrip filterStrip1 = filterStrips.AddNew("ChargeCode");
			filterStrip1.OrCategory = FilterOrCategory.Red;
			((ModuleGuidFilter)filterStrip1.CurrentModuleFilter).Property = TestObjectCreator.CC1.PK;
			FilterStrip filterStrip2 = filterStrips.AddNew("ChargeCode");
			filterStrip2.OrCategory = FilterOrCategory.Red;
			((ModuleGuidFilter)filterStrip2.CurrentModuleFilter).Property = TestObjectCreator.CC2.PK;

			AssertFilterFinds(consol, Filters.GetQuery());
			AssertFilterFinds(consol2, Filters.GetQuery());

			InvoicingBaseConsolCostCollectionForImporting collection = new InvoicingBaseConsolCostCollectionForImporting(Factory);
			collection.Load(Filters.GetChildQuery());
			AssertCollectionContains(cost, collection);
			AssertCollectionContains(cost2, collection);
		}

		public void TestChildCollectionForFiltersWithCategories_Currency()
		{
			ForwardingConsol consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C1");
			Factory.Save();
			JobConsolCost cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.AALSHI);
			cost.E6_RX_NKCurrency = "AUD";
			cost.E6_OSCostAmount = 100M;
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			charge.JR_E6 = cost.PK;
			charge.JR_RX_NKCostCurrency = "AUD";
			charge.JR_OSCostAmt = cost.E6_OSCostAmount;
			charge.JR_LocalCostAmt = cost.E6_OSCostAmount;
			charge.JR_AT_CostGSTRate = cost.E6_AT_TaxRate;
			ForwardingConsol consol2 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C2");
			JobConsolCost cost2 = TestObjectCreator.CreateConsolCost(consol2, TestObjectCreator.CC2, TestObjectCreator.AALSHI);
			cost2.E6_RX_NKCurrency = "USD";
			cost2.E6_ExchangeRate = 1M;
			cost2.E6_OSCostAmount = 200M;
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge2.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			charge2.JR_E6 = cost2.PK;
			charge2.JR_RX_NKCostCurrency = "USD";
			charge2.JR_OSCostAmt = cost2.E6_OSCostAmount;
			charge2.JR_LocalCostAmt = cost2.E6_OSCostAmount;
			charge2.JR_AT_CostGSTRate = cost2.E6_AT_TaxRate;
			Factory.Save();

			cost = Factory.Load<InvoicingBaseConsolCostForImporting>(cost.PK);
			cost2 = Factory.Load<InvoicingBaseConsolCostForImporting>(cost2.PK);

			FilterStripCollection filterStrips = new FilterStripCollection(Filters.ModuleFilters);
			FilterStrip filterStrip1 = filterStrips.AddNew("Currency");
			filterStrip1.OrCategory = FilterOrCategory.Red;
			((ModuleNkFilter)filterStrip1.CurrentModuleFilter).Property = "AUD";
			FilterStrip filterStrip2 = filterStrips.AddNew("Currency");
			filterStrip2.OrCategory = FilterOrCategory.Red;
			((ModuleNkFilter)filterStrip2.CurrentModuleFilter).Property = "USD";

			AssertFilterFinds(consol, Filters.GetQuery());
			AssertFilterFinds(consol2, Filters.GetQuery());

			InvoicingBaseConsolCostCollectionForImporting collection = new InvoicingBaseConsolCostCollectionForImporting(Factory);
			collection.Load(Filters.GetChildQuery());
			AssertCollectionContains(cost, collection);
			AssertCollectionContains(cost2, collection);
		}

		public void TestChildCollectionForFiltersWithCategories_ExcludeReverseSignedConsolCostsFilter()
		{
			ForwardingConsol consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C1");
			JobConsolCost cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.AALSHI);
			cost.E6_OSCostAmount = 100M;
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			charge.JR_E6 = cost.PK;
			charge.JR_RX_NKCostCurrency = "AUD";
			charge.JR_OSCostAmt = cost.E6_OSCostAmount;
			charge.JR_LocalCostAmt = cost.E6_OSCostAmount;
			charge.JR_AT_CostGSTRate = cost.E6_AT_TaxRate;
			ForwardingConsol consol2 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C2");
			JobConsolCost cost2 = TestObjectCreator.CreateConsolCost(consol2, TestObjectCreator.CC2, TestObjectCreator.AALSHI);
			cost2.E6_OSCostAmount = -200M;
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge2.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			charge2.JR_E6 = cost2.PK;
			charge2.JR_OSCostAmt = cost2.E6_OSCostAmount;
			charge2.JR_LocalCostAmt = cost2.E6_OSCostAmount;
			charge2.JR_AT_CostGSTRate = cost2.E6_AT_TaxRate;
			Factory.Save();

			cost = Factory.Load<InvoicingBaseConsolCostForImporting>(cost.PK);
			cost2 = Factory.Load<InvoicingBaseConsolCostForImporting>(cost2.PK);

			FilterStripCollection filterStrips = new FilterStripCollection(Filters.ModuleFilters);
			FilterStrip filterStrip1 = filterStrips.AddNew("ExcludeReverseSignedConsolCostsFilter");
			filterStrip1.OrCategory = FilterOrCategory.Red;
			((ModuleFlagsFilter)filterStrip1.CurrentModuleFilter).Property0 = true;
			FilterStrip filterStrip2 = filterStrips.AddNew("ExcludeReverseSignedConsolCostsFilter");
			filterStrip2.OrCategory = FilterOrCategory.Red;
			((ModuleFlagsFilter)filterStrip2.CurrentModuleFilter).Property0 = false;

			AssertFilterFinds(consol, Filters.GetQuery());
			AssertFilterFinds(consol2, Filters.GetQuery());

			InvoicingBaseConsolCostCollectionForImporting collection = new InvoicingBaseConsolCostCollectionForImporting(Factory);
			collection.Load(Filters.GetChildQuery());
			AssertCollectionContains(cost, collection);
			AssertCollectionContains(cost2, collection);
		}

		public void TestChildCollectionForFiltersWithCategories_FixedPlaceOfSupply()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var placeOfSupplyCode1 = PlaceOfSupplyListProvider.GetPlaceOfSupplyList(GlbCompany.CurrentCompany)[0].Code;
				var placeOfSupplyCode2 = PlaceOfSupplyListProvider.GetPlaceOfSupplyList(GlbCompany.CurrentCompany)[2].Code;

				var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C1");
				consol.Shipments.AddNew();
				var cost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 500M, TestObjectCreator.AALSHI);
				cost1.E6_OSCostAmount = 100M;
				cost1.E6_PlaceOfSupply = placeOfSupplyCode1;

				var consol2 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C2");
				consol2.Shipments.AddNew();
				var cost2 = TestObjectCreator.CreateConsolCost(consol2, TestObjectCreator.CC2, TestObjectCreator.AALSHI);
				cost2.E6_OSCostAmount = 200M;
				cost2.E6_PlaceOfSupply = placeOfSupplyCode2;

				var consol3 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C3");
				consol3.Shipments.AddNew();
				var cost3 = TestObjectCreator.CreateConsolCost(consol3, TestObjectCreator.CC2, TestObjectCreator.AALSHI);
				cost3.E6_OSCostAmount = 200M;

				Factory.Save();

				cost1 = Factory.Load<InvoicingBaseConsolCostForImporting>(cost1.PK);
				cost2 = Factory.Load<InvoicingBaseConsolCostForImporting>(cost2.PK);
				cost3 = Factory.Load<InvoicingBaseConsolCostForImporting>(cost3.PK);

				var filterStrips = new FilterStripCollection(Filters.ModuleFilters);
				var filterStrip1 = filterStrips.AddNew(InvoiceBulkOperationFilterHelper.FixedPlaceOfSupplyFilterName);
				((ModuleTextFilter)filterStrip1.CurrentModuleFilter).Property = placeOfSupplyCode1;
				AssertFilterFinds(consol, Filters.GetQuery());

				var collection = new InvoicingBaseConsolCostCollectionForImporting(Factory);
				collection.Load(Filters.GetChildQuery());
				AssertCollectionContains(cost1, collection);

				((ModuleTextFilter)filterStrip1.CurrentModuleFilter).Property = placeOfSupplyCode2;
				AssertFilterFinds(consol2, Filters.GetQuery());

				collection = new InvoicingBaseConsolCostCollectionForImporting(Factory);
				collection.Load(Filters.GetChildQuery());
				AssertCollectionContains(cost2, collection);
			}
		}

		public void TestChildCollectionForFiltersWithCategories_PostDate()
		{
			ForwardingConsol consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C1");
			JobConsolCost cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.AALSHI);
			cost.E6_OSCostAmount = 100M;
			JobCharge charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			charge.JR_E6 = cost.PK;
			charge.JR_RX_NKCostCurrency = "AUD";
			charge.JR_OSCostAmt = cost.E6_OSCostAmount;
			charge.JR_LocalCostAmt = cost.E6_OSCostAmount;
			charge.JR_AT_CostGSTRate = cost.E6_AT_TaxRate;
			ForwardingConsol consol2 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C2");
			JobConsolCost cost2 = TestObjectCreator.CreateConsolCost(consol2, TestObjectCreator.CC2, null);
			cost2.E6_OSCostAmount = 200M;
			JobCharge charge2 = Factory.NewWithValidTestData<Charge>();
			charge2.JR_OH_CostAccount = ZGuid.Empty;
			charge2.JR_E6 = cost2.PK;
			charge2.JR_OSCostAmt = cost2.E6_OSCostAmount;
			charge2.JR_LocalCostAmt = cost2.E6_OSCostAmount;
			charge2.JR_AT_CostGSTRate = cost2.E6_AT_TaxRate;
			Factory.Save();

			Accrual accrual1 = Factory.Load<Accrual>(charge.JR_AL_APLine);
			AssertNotNull(accrual1);
			accrual1.AL_PostDate = Date01JAN2005;

			Accrual accrual2 = Factory.Load<Accrual>(charge2.JR_AL_APLine);
			AssertNotNull(accrual2);
			accrual2.AL_PostDate = Date01FEB2005;
			Factory.Save();

			cost = Factory.Load<InvoicingBaseConsolCostForImporting>(cost.PK);
			cost2 = Factory.Load<InvoicingBaseConsolCostForImporting>(cost2.PK);

			FilterStripCollection filterStrips = new FilterStripCollection(Filters.ModuleFilters);
			FilterStrip filterStrip0 = filterStrips.AddNew("IncludeConsolCostsWithNoCreditor");
			((ModuleFlagsFilter)filterStrip0.CurrentModuleFilter).Property0 = true;
			FilterStrip filterStrip1 = filterStrips.AddNew("PostDate");
			filterStrip1.OrCategory = FilterOrCategory.Red;
			((ModuleDateFilter)filterStrip1.CurrentModuleFilter).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)filterStrip1.CurrentModuleFilter).Property1 = Date01JAN2005;
			((ModuleDateFilter)filterStrip1.CurrentModuleFilter).Property2 = Date01JAN2005;
			FilterStrip filterStrip2 = filterStrips.AddNew("PostDate");
			filterStrip2.OrCategory = FilterOrCategory.Red;
			((ModuleDateFilter)filterStrip2.CurrentModuleFilter).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)filterStrip2.CurrentModuleFilter).Property1 = Date01FEB2005;
			((ModuleDateFilter)filterStrip2.CurrentModuleFilter).Property2 = Date01FEB2005;

			AssertFilterFinds(consol, Filters.GetQuery());
			AssertFilterFinds(consol2, Filters.GetQuery());

			InvoicingBaseConsolCostCollectionForImporting collection = new InvoicingBaseConsolCostCollectionForImporting(Factory);
			collection.Load(Filters.GetChildQuery());
			AssertCollectionContains(cost, collection);
			AssertCollectionContains(cost2, collection);
		}

		public void TestChildCollectionForFiltersWithCategories_Trigger()
		{
			AssertNotNull((ModuleGuidForeignCollectionFilter)Filters["Triggers"]);

			var invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;

			var bizO = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var consol1 = CreateConsolWithShipment("C1");

			var cost1 = TestObjectCreator.CreateConsolCost(consol1, TestObjectCreator.CC1, 10m, TestObjectCreator.AALSHI);

			var trigger = consol1.WorkflowItems.Triggers.AddNew();
			trigger.P9_Status = "OPN";
			trigger.P9_Description = "Open Trigger";
			trigger.P9_ParentID = consol1.PK;

			Factory.Save();

			var filters = new InvoicingBaseBulkConsolCostImporterFilters(invoice);
			var filter = filters.AddFilterStrip<ModuleGuidForeignCollectionFilter>("Triggers");
			filter.SelectedFilters.AddTextFilterStrip("Description", "Open Trigger");

			var consolCollection = GetCollection(filters.GetQuery());
			AssertContainsExactElementsInAnyOrder(new[] { consol1.PK }, consolCollection.Select(x => x.PK));
			var costCollection = GetCostCollection(filters.GetChildQuery(), consolCollection);
			AssertContainsExactElementsInAnyOrder(new[] { cost1.PK }, costCollection.Select(x => x.PK));
		}

		public void TestSupplierCostReferenceFilter()
		{
			ForwardingConsol consol1 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C1");
			consol1.Shipments.AddNew();
			JobConsolCost cost = TestObjectCreator.CreateConsolCost(consol1, TestObjectCreator.CC1, 10m, TestObjectCreator.AALSHI);
			cost.E6_OSCostAmount = 100M;
			cost.E6_CostReference = "ABC";
			ForwardingConsol consol2 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C2");
			consol2.Shipments.AddNew();
			cost = TestObjectCreator.CreateConsolCost(consol2, TestObjectCreator.CC1, 10m, TestObjectCreator.AALSHI);
			cost.E6_OSCostAmount = 200M;
			cost.E6_CostReference = "XYZ";
			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)Filters["SupplierCostReference"];
			filter.Property = "ABC";
			filter.IsActive = true;

			AssertFilterFinds(consol1, Filters.GetQuery());
			AssertFilterDoesNotFind(consol2, Filters.GetQuery());
		}

		public void TestATA_ATDFilter()
		{
			var consol1 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C1");
			var consol2 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C2");
			consol1.JK_TransportMode = "SEA";
			consol2.JK_TransportMode = "AIR";
			consol1.Shipments.AddNew();
			consol2.Shipments.AddNew();
			TestObjectCreator.CreateConsolCost(consol1, TestObjectCreator.CC1, 10m, TestObjectCreator.AALSHI);
			TestObjectCreator.CreateConsolCost(consol2, TestObjectCreator.CC1, 10m, TestObjectCreator.AALSHI);

			var transport1 = consol1.Transports[0];
			transport1.JW_ATA = new ZDateTime(2014, 10, 10);
			transport1.JW_ATD = new ZDateTime(2014, 10, 11);

			var transport2 = consol2.Transports[0];
			transport2.JW_ATA = new ZDateTime(2014, 10, 20);
			transport2.JW_ATD = new ZDateTime(2014, 10, 25);

			Factory.Save();

			((ModuleDateFilter)Filters["ConsolATA_ATD"]).Property1 = new ZDateTime(2014, 10, 09);
			((ModuleDateFilter)Filters["ConsolATA_ATD"]).Property2 = new ZDateTime(2014, 10, 15);
			((ModuleDateFilter)Filters["ConsolATA_ATD"]).IsActive = true;
			((ModuleDateFilter)Filters["ConsolATA_ATD"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			var consolCollection = GetCollection(Filters.GetQuery());
			AssertCollection(consolCollection, new ForwardingConsol[] { consol1 });

			((ModuleDateFilter)Filters["ConsolATA_ATD"]).Property1 = new ZDateTime(2014, 10, 19);
			((ModuleDateFilter)Filters["ConsolATA_ATD"]).Property2 = new ZDateTime(2014, 10, 22);
			((ModuleDateFilter)Filters["ConsolATA_ATD"]).IsActive = true;
			((ModuleDateFilter)Filters["ConsolATA_ATD"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			consolCollection = GetCollection(Filters.GetQuery());
			AssertCollection(consolCollection, new ForwardingConsol[] { consol2 });

			((ModuleDateFilter)Filters["ConsolATA_ATD"]).IsActive = true;
			((ModuleDateFilter)Filters["ConsolATA_ATD"]).PropertySearch = ModuleDateFilter.HasDateEntered;
			consolCollection = GetCollection(Filters.GetQuery());
			AssertCollection(consolCollection, new ForwardingConsol[] { consol1, consol2 });

			((ModuleDateFilter)Filters["ConsolATA_ATD"]).IsActive = true;
			((ModuleDateFilter)Filters["ConsolATA_ATD"]).PropertySearch = ModuleDateFilter.HasNoDateEntered;
			consolCollection = GetCollection(Filters.GetQuery());
			AssertCollection(consolCollection, Array.Empty<ForwardingConsol>());
		}

		public void TestMasterBillIssueDateFilter()
		{
			var consol1 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C1");
			var consol2 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C2");
			var consol3 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C3");
			var consol4 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C4");
			consol1.JK_MasterBillIssueDate = new ZDateTime(2014, 10, 10);
			consol2.JK_MasterBillIssueDate = new ZDateTime(2014, 10, 20);
			consol3.JK_MasterBillIssueDate = new ZDateTime(2014, 10, 28);
			consol1.Shipments.AddNew();
			consol2.Shipments.AddNew();
			consol3.Shipments.AddNew();
			consol4.Shipments.AddNew();
			TestObjectCreator.CreateConsolCost(consol1, TestObjectCreator.CC1, 10m, TestObjectCreator.AALSHI);
			TestObjectCreator.CreateConsolCost(consol2, TestObjectCreator.CC1, 10m, TestObjectCreator.AALSHI);
			TestObjectCreator.CreateConsolCost(consol3, TestObjectCreator.CC1, 10m, TestObjectCreator.AALSHI);
			TestObjectCreator.CreateConsolCost(consol4, TestObjectCreator.CC1, 10m, TestObjectCreator.AALSHI);

			Factory.Save();

			((ModuleDateFilter)Filters["MasterBillIssueDate"]).Property1 = new ZDateTime(2014, 10, 09);
			((ModuleDateFilter)Filters["MasterBillIssueDate"]).Property2 = new ZDateTime(2014, 10, 15);
			((ModuleDateFilter)Filters["MasterBillIssueDate"]).IsActive = true;
			((ModuleDateFilter)Filters["MasterBillIssueDate"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			var consolCollection = GetCollection(Filters.GetQuery());
			AssertCollection(consolCollection, new ForwardingConsol[] { consol1 });

			((ModuleDateFilter)Filters["MasterBillIssueDate"]).Property1 = new ZDateTime(2014, 10, 19);
			((ModuleDateFilter)Filters["MasterBillIssueDate"]).Property2 = new ZDateTime(2014, 10, 22);
			((ModuleDateFilter)Filters["MasterBillIssueDate"]).IsActive = true;
			((ModuleDateFilter)Filters["MasterBillIssueDate"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			consolCollection = GetCollection(Filters.GetQuery());
			AssertCollection(consolCollection, new ForwardingConsol[] { consol2 });

			((ModuleDateFilter)Filters["MasterBillIssueDate"]).IsActive = true;
			((ModuleDateFilter)Filters["MasterBillIssueDate"]).PropertySearch = ModuleDateFilter.HasDateEntered;
			consolCollection = GetCollection(Filters.GetQuery());
			AssertCollection(consolCollection, new ForwardingConsol[] { consol1, consol2, consol3 });

			((ModuleDateFilter)Filters["MasterBillIssueDate"]).IsActive = true;
			((ModuleDateFilter)Filters["MasterBillIssueDate"]).PropertySearch = ModuleDateFilter.HasNoDateEntered;
			consolCollection = GetCollection(Filters.GetQuery());
			AssertCollection(consolCollection, new ForwardingConsol[] { consol4 });
		}

		public void TestNoAuditInformationInModuleFilters()
		{
			JobCollection collection = new JobCollection(Factory);
			Filters.QueryObjectType = collection.TypeOfElements;
			Filters.LoadModuleFilters();

			AssertNull($"Audit Information: {FilterDescriptions.CreatedTime}", Filters.ModuleFilters[FilterDescriptions.CreatedTime]);
			AssertNull($"Audit Information: {FilterDescriptions.CreatingUser}", Filters.ModuleFilters[FilterDescriptions.CreatingUser]);
			AssertNull($"Audit Information: {FilterDescriptions.LastEditTime}", Filters.ModuleFilters[FilterDescriptions.LastEditTime]);
			AssertNull($"Audit Information: {FilterDescriptions.LastEditUser}", Filters.ModuleFilters[FilterDescriptions.LastEditUser]);
		}

		public void TestFlightFilter()
		{
			var consol1 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C1");
			var consol2 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C2");

			consol1.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			consol2.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			consol1.Shipments.AddNew();
			consol2.Shipments.AddNew();
			TestObjectCreator.CreateConsolCost(consol1, TestObjectCreator.CC1, 10m, TestObjectCreator.AALSHI);
			TestObjectCreator.CreateConsolCost(consol2, TestObjectCreator.CC1, 10m, TestObjectCreator.AALSHI);

			Transport transport1 = consol1.Transports[0];
			transport1.JW_VoyageFlight = "snth";
			Transport transport2 = consol2.Transports[0];
			transport2.JW_VoyageFlight = "aoeu";

			TestObjectCreator.Factory.Save();

			FilterStripCollection filterStrips = new FilterStripCollection(Filters.ModuleFilters);
			FilterStrip filterStrip1 = filterStrips.AddNew("Flight");
			((ModuleTextFilter)filterStrip1.CurrentModuleFilter).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			((ModuleTextFilter)filterStrip1.CurrentModuleFilter).Property = "aoeu";
			AssertCollection(GetCollection(Filters.GetQuery()), consol2);

			FilterStrip filterStrip2 = filterStrips.AddNew("Flight");
			((ModuleTextFilter)filterStrip2.CurrentModuleFilter).ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			((ModuleTextFilter)filterStrip2.CurrentModuleFilter).Property = "th";
			var actualCollection = GetCollection(Filters.GetQuery());
			AssertCollectionNotContains(consol1, actualCollection);
			AssertCollectionNotContains(consol2, actualCollection);

			filterStrip1.OrCategory = FilterOrCategory.Red;
			filterStrip2.OrCategory = FilterOrCategory.Red;
			AssertCollection(GetCollection(Filters.GetQuery()), consol1, consol2);
		}

		[ExpectNoExceptions]
		public void TestMilestoneFilters_ShouldGenerateValidQuery()
		{
			var apInvoice = Factory.New<APInvoice>();
			apInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			var costing = new APInvoiceConsolCosting(Factory, apInvoice);
			var importer = new InvoicingBaseBulkConsolCostImporter(costing);

			var creditorFilter = (ModuleGuidFilter)importer.Filters["Creditor"];
			creditorFilter.IsActive = true;
			creditorFilter.Property = TestObjectCreator.AALSHI.PK;

			var milestoneDateFilter = (ModuleDateFilter)importer.Filters["Milestone Date"];
			milestoneDateFilter.IsActive = true;
			milestoneDateFilter.Property1 = new ZDateTime(2014, 3, 11);

			var milestoneCompleteFilter = (ModuleTextFilter)importer.Filters["Milestone Completed"];
			milestoneCompleteFilter.IsActive = true;
			milestoneCompleteFilter.Property = "Completed";

			importer.LoadConsolsCollection();
		}

		public void TestGetSendingAgentFilter()
		{
			var sendingAgentPK = new Guid();
			var filters = new InvoicingBaseBulkConsolCostImporterFiltersStub(Invoice);
			var zquery = filters.GetSendingAgentFilter(sendingAgentPK);
			AssertContains("VX_ParentTableCode", zquery.LiteralTextADO);
		}
		public void TestGetReceivingAgentFilter()
		{
			var receivingAgentPK = new Guid();
			var filters = new InvoicingBaseBulkConsolCostImporterFiltersStub(Invoice);
			var zquery = filters.GetReceivingAgentFilter(receivingAgentPK);
			AssertContains("VX_ParentTableCode", zquery.LiteralTextADO);
		}

		public void TestGetContainerModeFilter()
		{
			var filters = new InvoicingBaseBulkConsolCostImporterFiltersStub(Invoice);
			var zquery = filters.GetContainerModeFilter(string.Empty);
			AssertContains("VX_ParentTableCode", zquery.LiteralTextADO);
		}

		public void TestGetMasterBillFilter()
		{
			var filters = new InvoicingBaseBulkConsolCostImporterFiltersStub(Invoice);
			var zquery = filters.GetMasterBillFilter(SQLComparisonOperator.Contains, string.Empty);
			AssertContains("VX_ParentTableCode", zquery.LiteralTextADO);
		}

		public override void TestCoLoadMasterBillFilter()
		{
			var consol1 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C1");
			var consol2 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C2");
			var consol3 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C3");
			consol1.JK_AgentType = Enterprise.Core.Constants.AgentType.CoLoad;
			consol1.JK_CoLoadMasterBill = "123456";
			consol2.JK_AgentType = Enterprise.Core.Constants.AgentType.CoLoad;
			consol2.JK_CoLoadMasterBill = "67890";
			consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol3.JK_AgentType = Enterprise.Core.Constants.AgentType.Direct;
			consol3.JK_CoLoadMasterBill = "78910";
			consol1.Shipments.AddNew();
			consol2.Shipments.AddNew();
			consol3.Shipments.AddNew();
			TestObjectCreator.CreateConsolCost(consol1, TestObjectCreator.CC1, 10m, TestObjectCreator.AALSHI);
			TestObjectCreator.CreateConsolCost(consol2, TestObjectCreator.CC1, 10m, TestObjectCreator.AALSHI);
			TestObjectCreator.CreateConsolCost(consol3, TestObjectCreator.CC1, 10m, TestObjectCreator.AALSHI);
			TestObjectCreator.Factory.Save();

			((ModuleTextFilter)Filters["CoLoadMasterBill"]).IsActive = true;

			((ModuleTextFilter)Filters["CoLoadMasterBill"]).Property = "789";
			((ModuleTextFilter)Filters["CoLoadMasterBill"]).ComparisonOperator =
				ModuleTextFilter.ComparisonConstants.Contains;

			var consolCollection = GetCollection(Filters.GetQuery());
			AssertEquals("Should only be one item in collection", 1, consolCollection.Count);
			AssertEquals("Should show correct item", consol2.PK, consolCollection[0].PK);

			((ModuleTextFilter)Filters["CoLoadMasterBill"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			consolCollection = GetCollection(Filters.GetQuery());
			AssertEquals("Should only be one item in collection", 1, consolCollection.Count);
			AssertEquals("Should show correct item", consol3.PK, consolCollection[0].PK);

			((ModuleTextFilter)Filters["CoLoadMasterBill"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			((ModuleTextFilter)Filters["CoLoadMasterBill"]).Property = "6";
			consolCollection = GetCollection(Filters.GetQuery());
			AssertEquals("Should only be one item in collection", 1, consolCollection.Count);
			AssertEquals("Should show correct item", consol3.PK, consolCollection[0].PK);
		}

		public void TestWorkflowFilter_SubGroup()
		{
			var filters = new InvoicingBaseBulkConsolCostImporterFiltersStub(Invoice);
			var workflowFilterHelper = filters.GetWorkflowFilterStripsHelper();

			var workflowfilterCollection = new ModuleFilterCollection();
			if (workflowFilterHelper.IsApplicableToBizOTypeIsAssignableFrom())
			{
				workflowFilterHelper.AddFilterStrips(workflowfilterCollection);
			}

			foreach (var workflowFilter in workflowfilterCollection)
			{
				var filter = filters.ModuleFilters.FirstOrDefault(x => x.Description == workflowFilter.Description);
				if (filter != null)
				{
					//This test can be removed after ModuleFilterSubGroup is refactored
					AssertNotNull($"workflow filter [{filter.Description}] should have sub group. Please include it in ModuleFilterSubGroup of InvoiceBulkOperationWithParentChildRelationshipFilters.cs", filter.SubGroup);
				}
				else
				{
					Assert("filter is not workflow filter", true);
				}
			}
		}

		public void TestTaxBranchFilter()
		{
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertEquals("This is mandatory filter.", FilterVisibility.AlwaysVisible, Filters["TaxBranch"].Visibility);
			AssertEquals("This is mandatory filter.", false, Filters["TaxBranch"].ReadOnly);
			AssertEquals("Default of TaxBranch filter", Invoice.AH_GB_TaxBranch, ((ModuleGuidFilter)Filters["TaxBranch"]).Property);

			var branch = TestObjectCreator.CreateBranch("GB1", GlbCompany.CurrentCompany);
			var consol1 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C1");
			var shipment = consol1.Shipments.AddNew();
			var cost = TestObjectCreator.CreateConsolCost(consol1, TestObjectCreator.CC1, TestObjectCreator.AALSHI);
			cost.E6_OSCostAmount = 100M;

			Factory.Save();

			((ModuleGuidFilter)Filters["TaxBranch"]).Property = branch.PK;
			((ModuleGuidFilter)Filters["TaxBranch"]).IsActive = true;
			AssertFilterDoesNotFind(consol1, Filters.GetQuery());

			cost.E6_GB_CostTaxBranch = branch.PK;
			Factory.Save();

			AssertFilterFinds(consol1, Filters.GetQuery());
		}

		internal class InvoicingBaseBulkConsolCostImporterFiltersStub : InvoicingBaseBulkConsolCostImporterFilters
		{
			public InvoicingBaseBulkConsolCostImporterFiltersStub(InvoicingBase parentInvoice) : base(parentInvoice)
			{
				SetDefaultValueInFilters();
			}

			public new ZQuery GetSendingAgentFilter(ZGuid sendingAgentPK)
			{
				return base.GetSendingAgentFilter(sendingAgentPK);
			}

			public new ZQuery GetReceivingAgentFilter(ZGuid receivingAgentPK)
			{
				return base.GetReceivingAgentFilter(receivingAgentPK);
			}

			public new ZQuery GetContainerModeFilter(ZString value)
			{
				return base.GetContainerModeFilter(value);
			}

			public new ZQuery GetMasterBillFilter(SQLComparisonOperator comparisonOperator, ZString value)
			{
				return base.GetMasterBillFilter(comparisonOperator, value);
			}

			public new IFilterStripsHelper GetWorkflowFilterStripsHelper()
			{
				return base.GetWorkflowFilterStripsHelper();
			}
		}

		#region Implementation

		void AssertDateFiltersWork(ForwardingConsol consol01JAN2005, ForwardingConsol consol01FEB2005, ModuleDateFilter filter)
		{
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = Date31JAN2005;
			AssertFilterFinds(consol01FEB2005, Filters.GetQuery());
			AssertFilterDoesNotFind(consol01JAN2005, Filters.GetQuery());

			filter.Property1 = Date05FEB2005;
			AssertFilterDoesNotFind(consol01JAN2005, Filters.GetQuery());
			AssertFilterDoesNotFind(consol01FEB2005, Filters.GetQuery());

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = Date31JAN2005;
			AssertFilterFinds(consol01JAN2005, Filters.GetQuery());
			AssertFilterDoesNotFind(consol01FEB2005, Filters.GetQuery());

			filter.Property2 = Date31DEC2004;
			AssertFilterDoesNotFind(consol01JAN2005, Filters.GetQuery());
			AssertFilterDoesNotFind(consol01FEB2005, Filters.GetQuery());

			filter.Property1 = Date31JAN2005;
			filter.Property2 = Date05FEB2005;
			AssertFilterDoesNotFind(consol01JAN2005, Filters.GetQuery());
			AssertFilterFinds(consol01FEB2005, Filters.GetQuery());

			filter.Property1 = Date31DEC2004;
			filter.Property2 = Date31JAN2005;
			AssertFilterFinds(consol01JAN2005, Filters.GetQuery());
			AssertFilterDoesNotFind(consol01FEB2005, Filters.GetQuery());

			filter.Property1 = Date05FEB2005;
			filter.Property2 = Date05FEB2005.AddDays(10);
			AssertFilterDoesNotFind(consol01JAN2005, Filters.GetQuery());
			AssertFilterDoesNotFind(consol01FEB2005, Filters.GetQuery());

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			AssertFilterFinds(consol01JAN2005, Filters.GetQuery());
			AssertFilterFinds(consol01FEB2005, Filters.GetQuery());
		}

		void AssertFilterFinds(ForwardingConsol consolToExpect, ZQuery query)
		{
			GenericConsolCollection collection = new GenericConsolCollection(Factory, query);
			AssertNotNull(collection.FindByPK(consolToExpect.PK));
		}

		void AssertFilterDoesNotFind(ForwardingConsol consolNotToExpect, ZQuery query)
		{
			GenericConsolCollection collection = new GenericConsolCollection(Factory, query);
			AssertNull(collection.FindByPK(consolNotToExpect.PK));
		}

		void AssertCollection(ActiveBusinessObjectCollection<GenericConsol.GenericConsol> collection, params ForwardingConsol[] items)
		{
			AssertEquals("Count of items in collection", items.Length, collection.Count);
			foreach (ForwardingConsol consolToExpect in items)
			{
				AssertNotNull("Should have contained this item", collection.FindByPK(consolToExpect.PK));
			}
		}

		GenericConsolCollection GetCollection(ZQuery query)
		{
			GenericConsolCollection result = new GenericConsolCollection(Factory, query);
			return result;
		}

		InvoicingBaseConsolCostCollectionForImporting GetCostCollection(ZQuery query, GenericConsolCollection consols)
		{
			var costsFilter = new ZQuery(JobConsolCostSchema.E6_ParentID, consols.Select(x => x.PK).ToArray());
			costsFilter.AddToFilter(query);

			var costCollection = new InvoicingBaseConsolCostCollectionForImporting(Factory);
			costCollection.Load(costsFilter);

			return costCollection;
		}

		readonly ZDateTime Date31DEC2004 = new ZDateTime(2004, 12, 31);
		readonly ZDateTime Date01JAN2005 = new ZDateTime(2005, 01, 01);
		readonly ZDateTime Date31JAN2005 = new ZDateTime(2005, 1, 31);
		readonly ZDateTime Date01FEB2005 = new ZDateTime(2005, 02, 01);
		readonly ZDateTime Date05FEB2005 = new ZDateTime(2005, 02, 05);

		protected new InvoicingBaseBulkConsolCostImporterFilters Filters
		{
			get { return (InvoicingBaseBulkConsolCostImporterFilters)base.Filters; }
		}

		protected override InvoiceBulkOperationFilters GetNewFilters()
		{
			return new InvoicingBaseBulkConsolCostImporterFilters(Invoice);
		}

		APInvoice Invoice
		{
			get
			{
				if (invoice == null)
				{
					invoice = Factory.NewWithValidTestData<APInvoice>();
					invoice.AH_OH = TestObjectCreator.AALSHI.PK;
				}
				return invoice;
			}
		}

		APInvoice invoice;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new InvoicingBaseBulkConsolCostImporterFilters(Factory.New<APInvoice>());
		}

		InvoicingBaseConsolCostForImporting CreateCostsAndMileStones(ForwardingConsol consol, ZString status, ZDateTime actualDate, ZDateTimeOffset scheduledDate)
		{
			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 10m, TestObjectCreator.AALSHI);

			var mileStone = consol.WorkflowItems.Milestones.AddNew();
			mileStone.P9_Type = "MIL";
			mileStone.P9_Status = status;
			mileStone.SetMilestoneActualDateForTest(actualDate);
			mileStone.SetMilestoneScheduledDateForTest(scheduledDate);
			Factory.Save();

			return Factory.Load<InvoicingBaseConsolCostForImporting>(cost.PK);
		}

		InvoicingBaseConsolCostForImporting CreateCostsAndTasks(DummyWithWorkflow bizO, ForwardingConsol consol, ZString status, ZString staffCode, ZInt sequence)
		{
			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 10m, TestObjectCreator.AALSHI);

			var task = bizO.WorkflowItems.Tasks.AddNew();
			task.P9_Status = status;
			task.P9_ParentID = consol.PK;
			task.P9_GS_NKAssignedStaffMember = staffCode;
			task.P9_Sequence = sequence;
			Factory.Save();

			return Factory.Load<InvoicingBaseConsolCostForImporting>(cost.PK);
		}

		ForwardingConsol CreateConsolWithShipment(ZString consolNum)
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", consolNum);
			var shipment1 = consol.Shipments.AddNew();
			Factory.Save();

			return consol;
		}

		#endregion
	}
}
