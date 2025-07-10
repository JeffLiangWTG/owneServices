using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(InvoicingBaseBulkChargeImporterFilters))]
	public class InvoicingBaseBulkChargeImporterFiltersTest : InvoiceBulkOperationFiltersTest
	{
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

			var bizO1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var job1 = TestObjectCreator.CreateJobHeader();
			job1.JH_ParentID = bizO1.PK;

			var bizO2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var job2 = TestObjectCreator.CreateJobHeader();
			job2.JH_ParentID = bizO2.PK;
			var bizO3 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var job3 = TestObjectCreator.CreateJobHeader();
			job3.JH_ParentID = bizO3.PK;
			var bizO4 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var job4 = TestObjectCreator.CreateJobHeader();
			job4.JH_ParentID = bizO4.PK;

			var charge1 = CreateChargesAndMileStones(bizO1, job1, "LST", new ZDateTime(2014, 3, 11), ZDateTimeOffset.Empty);
			var charge2 = CreateChargesAndMileStones(bizO2, job2, "LST", ZDateTime.Empty, ZDateTimeOffset.Empty);
			var charge3 = CreateChargesAndMileStones(bizO3, job3, "NXT", ZDateTime.Empty, new ZDateTimeOffset(new ZDateTime(2014, 5, 18)));
			var charge4 = CreateChargesAndMileStones(bizO4, job4, "LST", new ZDateTime(2014, 8, 6), ZDateTimeOffset.Empty);

			var filters = new InvoicingBaseBulkChargeImporterFilters(invoice);
			var milestoneDatefilter = (ModuleDateFilter)filters["Milestone Date"];
			milestoneDatefilter.IsActive = true;
			milestoneDatefilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			milestoneDatefilter.Property1 = new ZDateTime(2014, 3, 10);
			milestoneDatefilter.Property2 = new ZDateTime(2014, 3, 13);

			var jobCollection = GetCollection(filters);
			AssertContainsExactElementsInAnyOrder(new[] { job1 }, jobCollection);
			var chargeCollection = GetChargeCollection(filters, jobCollection);
			AssertContainsExactElementsInAnyOrder(new[] { charge1 }, chargeCollection);

			filters = new InvoicingBaseBulkChargeImporterFilters(invoice);
			var milestoneCompletedFilter = (ModuleTextFilter)filters["Milestone Completed"];
			milestoneCompletedFilter.IsActive = true;
			milestoneCompletedFilter.Property = "Not Completed";

			jobCollection = GetCollection(filters);
			AssertContainsExactElementsInAnyOrder(new[] { job2, job3 }, jobCollection);
			chargeCollection = GetChargeCollection(filters, jobCollection);
			AssertContainsExactElementsInAnyOrder(new[] { charge2, charge3 }, chargeCollection);

			filters = new InvoicingBaseBulkChargeImporterFilters(invoice);
			var nextMilestoneFilter = (ModuleDateFilter)filters["Next Milestone"];
			nextMilestoneFilter.IsActive = true;
			nextMilestoneFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			nextMilestoneFilter.Property1 = new ZDateTime(2014, 5, 17);
			nextMilestoneFilter.Property2 = new ZDateTime(2014, 5, 19);

			jobCollection = GetCollection(filters);
			AssertContainsExactElementsInAnyOrder(new[] { job3 }, jobCollection);
			chargeCollection = GetChargeCollection(filters, jobCollection);
			AssertContainsExactElementsInAnyOrder(new[] { charge3 }, chargeCollection);

			filters = new InvoicingBaseBulkChargeImporterFilters(invoice);
			var lastCompletedMilestoneFilter = (ModuleDateFilter)filters["Last Completed Milestone"];
			lastCompletedMilestoneFilter.IsActive = true;
			lastCompletedMilestoneFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			lastCompletedMilestoneFilter.Property1 = new ZDateTime(2014, 8, 5);
			lastCompletedMilestoneFilter.Property2 = new ZDateTime(2014, 8, 7);

			jobCollection = GetCollection(filters);
			AssertContainsExactElementsInAnyOrder(new[] { job4 }, jobCollection);
			chargeCollection = GetChargeCollection(filters, jobCollection);
			AssertContainsExactElementsInAnyOrder(new[] { charge4 }, chargeCollection);
		}

		public void TestTaskAssignedToFiltersWithSubGroup()
		{
			AssertNotNull((ModuleNkFilter)Filters["Any Open Task Assigned To"]);
			AssertNotNull((ModuleNkFilter)Filters["Next Task Assigned To"]);

			var invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;

			var bizO = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var job1 = TestObjectCreator.CreateJobHeader();
			var job2 = TestObjectCreator.CreateJobHeader();
			var job3 = TestObjectCreator.CreateJobHeader();
			var job4 = TestObjectCreator.CreateJobHeader();

			var charge1 = CreateChargesAndTasks(bizO, job1, "WRK", "SM1", 1);
			var charge2 = CreateChargesAndTasks(bizO, job2, "WRK", "SM2", 1);
			var charge3 = CreateChargesAndTasks(bizO, job3, "WRK", "SM2", 2);
			var charge4 = CreateChargesAndTasks(bizO, job4, "WRK", "SM3", 3);

			var filters = new InvoicingBaseBulkChargeImporterFilters(invoice);
			var anyTaskAssignedTofilter = (ModuleNkFilter)filters["Any Open Task Assigned To"];
			anyTaskAssignedTofilter.Property = "SM1";
			anyTaskAssignedTofilter.IsActive = true;

			var jobCollection = GetCollection(filters);
			AssertContainsExactElementsInAnyOrder(new[] { job1 }, jobCollection);
			var chargeCollection = GetChargeCollection(filters, jobCollection);
			AssertContainsExactElementsInAnyOrder(new[] { charge1 }, chargeCollection);

			filters = new InvoicingBaseBulkChargeImporterFilters(invoice);
			var nextTaskAssignedTofilter = (ModuleNkFilter)filters["Next Task Assigned To"];
			nextTaskAssignedTofilter.IsActive = true;
			nextTaskAssignedTofilter.Property = "SM2";

			jobCollection = GetCollection(filters);
			AssertContainsExactElementsInAnyOrder(new[] { job2, job3 }, jobCollection);
			chargeCollection = GetChargeCollection(filters, jobCollection);
			AssertContainsExactElementsInAnyOrder(new[] { charge2, charge3 }, chargeCollection);
		}

		public void TestTasksFiltersWithSubGroup()
		{
			AssertNotNull((ModuleGuidForeignCollectionFilter)Filters["Tasks"]);

			var invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;

			var bizO = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var job1 = TestObjectCreator.CreateJobHeader();
			var charge1 = CreateNewCharge();
			charge1.JR_JH = job1.PK;
			var job2 = TestObjectCreator.CreateJobHeader();
			var charge2 = CreateNewCharge();
			charge2.JR_JH = job2.PK;

			var taskTemplate = bizO.WorkflowItems.Tasks.AddNew();
			var openTask = (ProcessTask)Factory.NewWithValidTestData(taskTemplate.GetType());
			openTask.P9_Status = "ASN";
			openTask.P9_Type = taskTemplate.P9_Type;
			openTask.P9_Description = "Open task";
			openTask.P9_ParentID = job1.PK;
			openTask.P9_ParentTableCode = job1.TablePrefix;

			var closedTask = (ProcessTask)Factory.NewWithValidTestData(taskTemplate.GetType());
			closedTask.P9_Status = "CLS";
			closedTask.P9_Type = taskTemplate.P9_Type;
			closedTask.P9_Description = "Closed task";
			closedTask.P9_ParentID = job2.PK;
			closedTask.P9_ParentTableCode = job2.TablePrefix;

			taskTemplate.Delete();
			Factory.Save();

			var filters = new InvoicingBaseBulkChargeImporterFilters(invoice);
			var filter = (ModuleGuidForeignCollectionFilter)filters["Tasks"];
			filter.SelectedFilters.AddTextFilterStrip("Status", "CLS");

			var subFilterResult = Factory.Load<ProcessTask>(filter.SelectedFilters.Filter);
			AssertContainsExactElementsInAnyOrder("Should have found the closed task", new[] { closedTask }, subFilterResult);

			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;

			var jobCollection = GetCollection(filters);
			AssertContainsExactElementsInAnyOrder(new[] { job2 }, jobCollection);
			var chargeCollection = GetChargeCollection(filters, jobCollection);
			AssertContainsExactElementsInAnyOrder(new[] { charge2 }, chargeCollection);
		}

		public void TestExceptionsFiltersWithSubGroup()
		{
			AssertNotNull((ModuleGuidForeignCollectionFilter)Filters["Exceptions"]);

			var invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;

			var bizO = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var job1 = TestObjectCreator.CreateJobHeader();
			var charge1 = CreateNewCharge();
			charge1.JR_JH = job1.PK;
			var job2 = TestObjectCreator.CreateJobHeader();
			var charge2 = CreateNewCharge();
			charge2.JR_JH = job2.PK;

			var openException = ((IWorkflowProvider)bizO).WorkflowItems.Exceptions.AddNew();
			openException.IsExceptionActioned = false;
			openException.P9_Description = "Open exception";
			openException.P9_ParentTableCode = job1.TablePrefix;
			openException.P9_ParentID = job1.PK;

			var closedException = ((IWorkflowProvider)bizO).WorkflowItems.Exceptions.AddNew();
			closedException.IsExceptionActioned = true;
			closedException.P9_Description = "Closed exception";
			closedException.P9_ParentTableCode = job2.TablePrefix;
			closedException.P9_ParentID = job2.PK;

			Factory.Save();

			var filters = new InvoicingBaseBulkChargeImporterFilters(invoice);
			var filter = (ModuleGuidForeignCollectionFilter)filters["Exceptions"];
			((ModuleTextFilter)filter.SelectedFilters.ActiveModuleFilters[0]).Property = ExceptionStatusCodeList.Codes.Actioned;

			var subFilterResult = Factory.Load<ProcessTask>(filter.SelectedFilters.Filter);
			AssertContainsExactElementsInAnyOrder("Should have found the closed exception", new[] { closedException }, subFilterResult);

			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;

			var jobCollection = GetCollection(filters);
			AssertContainsExactElementsInAnyOrder(new[] { job2 }, jobCollection);
			var chargeCollection = GetChargeCollection(filters, jobCollection);
			AssertContainsExactElementsInAnyOrder(new[] { charge2 }, chargeCollection);
		}

		public void TestExcludeReverseSignedChargesFilter_Invoice()
		{
			AssertReverseSignedCharge<APInvoice>(2);
		}

		public void TestExcludeReverseSignedChargesFilter_CreditNote()
		{
			AssertReverseSignedCharge<APCreditNote>(1);
		}

		void AssertReverseSignedCharge<T>(int expectedChargeNumber) where T : InvoicingBase
		{
			InvoicingBase invoice = Factory.NewWithValidTestData<T>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;

			Charge charge1 = CreateNewCharge();
			Charge charge2 = CreateNewCharge();
			charge1.JR_OSCostAmt = -charge1.JR_OSCostAmt;
			Factory.Save();

			InvoicingBaseBulkChargeImporterFilters testFilters = new InvoicingBaseBulkChargeImporterFilters(invoice);

			Charge[] charges = Factory.Load<Charge>(testFilters.GetChildQuery());
			AssertEquals("Both charges should be loaded", 2, charges.Length);

			((ModuleFlagsFilter)testFilters[InvoiceBulkOperationFilterHelper.ExcludeReverseSignedChargesFilterName]).Property0 = true;
			testFilters[InvoiceBulkOperationFilterHelper.ExcludeReverseSignedChargesFilterName].IsActive = true;
			charges = Factory.Load<Charge>(testFilters.GetChildQuery());
			AssertEquals("Only one charge should be loaded", 1, charges.Length);
			AssertEquals("Only this charge should be loaded", (expectedChargeNumber == 1 ? charge1 : charge2).PK, charges[0].PK);
		}

		public void TestDefaulting()
		{
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			InvoicingBaseBulkChargeImporterFilters filters = new InvoicingBaseBulkChargeImporterFilters(Invoice);
			AssertEquals(Invoice.AH_OH, ((ModuleGuidFilter)filters["Creditor"]).Property);
			AssertEquals(Invoice.AH_GB_TaxBranch, ((ModuleGuidFilter)filters["TaxBranch"]).Property);
		}

		[SuspendGLAccountAndChargeCodeCriticalValidation]
		public void TestMandatoryFilters()
		{
			Charge charge1 = CreateNewCharge();
			charge1.JR_AL_APLine = Factory.NewWithValidTestData<APInvoice>().Lines.AddNew().PK;
			charge1.APLine.AL_OSAmount = charge1.APLine.AL_LineAmount = -charge1.JR_OSCostAmt;
			Charge charge2 = CreateNewCharge();
			Job job1 = CreateNewJobForCharge(charge1);
			Job job2 = CreateNewJobForCharge(charge2);

			Factory.Save();

			ActivateAlwaysVisibleFilters();
			AssertCollection(GetCollection(Filters), job2);
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

		public void TestCreditorFilterWithAccruals()
		{
			CreditorFilterTestHelper(true);
		}

		public void TestCreditorFilterWithoutAccruals()
		{
			CreditorFilterTestHelper(false);
		}

		void CreditorFilterTestHelper(bool createAccruals)
		{
			Charge charge1 = CreateNewCharge();
			Charge charge2 = CreateNewCharge();
			charge2.JR_OH_CostAccount = TestObjectCreator.ABIGAS.PK;
			Charge charge3 = CreateNewCharge();
			charge3.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			Job job1 = CreateNewJobForCharge(charge1);
			Job job2 = CreateNewJobForCharge(charge2);
			charge3.JR_JH = job1.PK;

			if (!createAccruals)
			{
				// Should not create Accruals on Saving. They are created by default.
				AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			}

			Factory.Save();
			if (createAccruals)
			{
				AssertNotNull(charge1.APLine);
				AssertNotNull(charge2.APLine);
				AssertNotNull(charge3.APLine);
			}
			else
			{
				AssertNull(charge1.APLine);
				AssertNull(charge2.APLine);
				AssertNull(charge3.APLine);
			}

			ModuleGuidFilter filter = (ModuleGuidFilter)Filters["Creditor"];
			filter.Property = ZGuid.Empty;
			filter.RunPreSaveValidation();
			AssertEquals("Should have errors", true, filter.HasErrors);
			filter.Property = Invoice.AH_OH;
			filter.RunPreSaveValidation();
			AssertEquals("Should not have errors. Errors: " + filter.NotificationsIncludingChildren.ToUniqueMessageListString(), false, ((ModuleGuidFilter)Filters["Creditor"]).HasErrors);
			filter.IsActive = true;
			AssertCollection(GetCollection(Filters), job1);

			var collection = Factory.Load<Charge>(Filters.GetChildQuery());
			AssertCollection(collection, charge1);

			AssertEquals("This is mandatory filter", FilterVisibility.AlwaysVisible, Filters["Creditor"].Visibility);
			AssertEquals("This is mandatory filter which can be overriden", false, Filters["Creditor"].ReadOnly);
			AssertNotNull("This is a mandatory filter", filter.PropertyValidation);
		}

		public void TestChargeCodeFilterWithAccruals()
		{
			ChargeCodeFilterTestHelper(true);
		}

		public void TestChargeCodeFilterWithoutAccruals()
		{
			ChargeCodeFilterTestHelper(false);
		}

		void ChargeCodeFilterTestHelper(bool createAccruals)
		{
			Charge charge1 = CreateNewCharge();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_OSCostAmt = 1.1m;
			Charge charge2 = CreateNewCharge();
			charge2.JR_AC = TestObjectCreator.CC2.PK;
			charge2.JR_OSCostAmt = 1.1m;
			Charge charge3 = CreateNewCharge();
			charge3.JR_AC = TestObjectCreator.CC3.PK;
			charge3.JR_OSCostAmt = 1.1m;
			Job job1 = CreateNewJobForCharge(charge1);
			Job job2 = CreateNewJobForCharge(charge2);
			charge3.JR_JH = job2.PK;

			if (!createAccruals)
			{
				// Should not create Accruals on Saving. They are created by default.
				AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			}

			Factory.Save();
			if (createAccruals)
			{
				AssertNotNull(charge1.APLine);
				AssertNotNull(charge2.APLine);
				AssertNotNull(charge3.APLine);
			}
			else
			{
				AssertNull(charge1.APLine);
				AssertNull(charge2.APLine);
				AssertNull(charge3.APLine);
			}

			((ModuleGuidFilter)Filters["ChargeCode"]).Property = TestObjectCreator.CC2.PK;
			((ModuleGuidFilter)Filters["ChargeCode"]).IsActive = true;
			AssertCollection(GetCollection(Filters), job2);

			var collection = Factory.Load<Charge>(Filters.GetChildQuery());
			AssertCollection(collection, charge2);
		}

		public void TestCurrencyFilter()
		{
			Charge charge1 = CreateNewCharge();
			charge1.JR_RX_NKCostCurrency = TestObjectCreator.AUD.RX_Code;
			charge1.JR_OSCostAmt = 1.1m;
			Charge charge2 = CreateNewCharge();
			charge2.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			charge2.JR_OSCostAmt = 1.1m;
			Job job1 = CreateNewJobForCharge(charge1);
			Job job2 = CreateNewJobForCharge(charge2);

			Factory.Save();

			((ModuleNkFilter)Filters["Currency"]).Property = TestObjectCreator.USD.RX_Code;
			((ModuleNkFilter)Filters["Currency"]).IsActive = true;
			AssertCollection(GetCollection(Filters), job2);
		}

		public void TestCustomSqlFilter()
		{
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;

			var charge1 = CreateNewCharge();
			charge1.JR_OSCostAmt = 50m;
			var charge2 = CreateNewCharge();
			charge2.JR_OSCostAmt = 75m;
			var charge3 = CreateNewCharge();
			charge3.JR_OSCostAmt = 100m;

			var job1 = CreateNewJobForCharge(charge1);
			var job2 = CreateNewJobForCharge(charge2);
			charge3.JR_JH = job2.PK;

			job1.JH_Description = "desc1";
			job2.JH_Description = "desc2";

			Factory.Save();

			Assert("PreCond", EnvProxy.Instance.Security.UseSqlFilterStrip.IsAllowed);

			var filters = new InvoicingBaseBulkChargeImporterFilters(invoice);

			var jobCustomSqlFilter = (ModuleSQLFilter)filters[InvoiceBulkOperationFilterHelper.JobHeaderCustomSqlFilterName];
			jobCustomSqlFilter.Property1 = JobHeaderSchema.JH_Description.Name + " = 'desc2'";
			jobCustomSqlFilter.IsActive = true;
			AssertCollection(GetCollection(filters), job2);

			jobCustomSqlFilter.Property1 = JobHeaderSchema.JH_Description.Name + " like 'desc%'";

			var chargeCustomSqlFilter = (ModuleSQLFilter)filters[InvoiceBulkOperationFilterHelper.JobChargeCustomSqlFilterName];
			chargeCustomSqlFilter.Property1 = JobChargeSchema.JR_OSCostAmt.Name + " < 80";
			chargeCustomSqlFilter.IsActive = true;

			var jobCollection = GetCollection(filters);
			AssertContainsExactElementsInAnyOrder(new[] { job1, job2 }, jobCollection);
			var chargeCollection = GetChargeCollection(filters, jobCollection);
			AssertContainsExactElementsInAnyOrder(new[] { charge1, charge2 }, chargeCollection);

			chargeCustomSqlFilter.Property1 = JobChargeSchema.JR_OSCostAmt.Name + " < 60";

			jobCollection = GetCollection(filters);
			AssertContainsExactElementsInAnyOrder(new[] { job1 }, jobCollection);
			chargeCollection = GetChargeCollection(filters, jobCollection);
			AssertContainsExactElementsInAnyOrder(new[] { charge1 }, chargeCollection);

			Assert(!jobCustomSqlFilter.ReadOnly);
			Assert(!chargeCustomSqlFilter.ReadOnly);
			AssertEquals(FilterVisibility.Visible, jobCustomSqlFilter.Visibility);
			AssertEquals(FilterVisibility.Visible, chargeCustomSqlFilter.Visibility);

			EnvProxy.Instance.Security.UseSqlFilterStrip.IsAllowed = false;
			filters = new InvoicingBaseBulkChargeImporterFilters(invoice);
			jobCustomSqlFilter = (ModuleSQLFilter)filters[InvoiceBulkOperationFilterHelper.JobHeaderCustomSqlFilterName];
			chargeCustomSqlFilter = (ModuleSQLFilter)filters[InvoiceBulkOperationFilterHelper.JobChargeCustomSqlFilterName];
			Assert(jobCustomSqlFilter.ReadOnly);
			Assert(chargeCustomSqlFilter.ReadOnly);
			AssertEquals(FilterVisibility.Visible, jobCustomSqlFilter.Visibility);
			AssertEquals(FilterVisibility.Visible, chargeCustomSqlFilter.Visibility);
		}

		public void TestBranchFilter()
		{
			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			GlbBranch branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			Charge charge1 = CreateNewCharge();
			charge1.JR_GB = branch1.PK;
			Charge charge2 = CreateNewCharge();
			charge2.JR_GB = branch2.PK;
			Job job1 = CreateNewJobForCharge(charge1);
			Job job2 = CreateNewJobForCharge(charge2);

			Factory.Save();

			((ModuleGuidFilter)Filters["Branch"]).Property = branch2.PK;
			((ModuleGuidFilter)Filters["Branch"]).IsActive = true;
			AssertCollection(GetCollection(Filters), job2);
		}

		public void TestDepartmentFilter()
		{
			GlbDepartment department1 = Factory.NewWithValidTestData<GlbDepartment>();
			GlbDepartment department2 = Factory.NewWithValidTestData<GlbDepartment>();
			Charge charge1 = CreateNewCharge();
			charge1.JR_GE = department1.PK;
			Charge charge2 = CreateNewCharge();
			charge2.JR_GE = department2.PK;
			Job job1 = CreateNewJobForCharge(charge1);
			Job job2 = CreateNewJobForCharge(charge2);

			Factory.Save();

			((ModuleGuidFilter)Filters["Department"]).Property = department2.PK;
			((ModuleGuidFilter)Filters["Department"]).IsActive = true;
			AssertCollection(GetCollection(Filters), job2);
		}

		public void TestTransportModeFilter()
		{
			ForwardingConsol consol1 = null, consol2 = null;
			Job job1 = CreateJobLinkedToConsol(ref consol1);
			Job job2 = CreateJobLinkedToConsol(ref consol2);
			consol1.JK_TransportMode = "SEA";
			consol2.JK_TransportMode = "AIR";
			CreateNewChargeForJob(job1);
			CreateNewChargeForJob(job2);
			Factory.Save();

			((ModuleTextFilter)Filters["TransportMode"]).Property = "AIR";
			((ModuleTextFilter)Filters["TransportMode"]).IsActive = true;
			AssertCollection(GetCollection(Filters), job2);
		}

		public void TestContainerNumberFilter()
		{
			ForwardingShipment forwardingShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			forwardingShipment.JS_IsForwardRegistered = true;
			ForwardingShipment cFSShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			cFSShipment.JS_IsCFSRegistered = true;
			ForwardingShipment cFSGatePass = Factory.NewWithValidTestData<ForwardingShipment>();
			cFSGatePass.JS_IsCFSRegistered = true;
			cFSGatePass.JS_TranshipToOtherCFS = true;
			ForwardingShipment shippingBooking = Factory.NewWithValidTestData<ForwardingShipment>();

			ForwardingConsol consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.JK_IsCFS = true;

			Job job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_ParentID = forwardingShipment.PK;
			Job job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_ParentID = cFSShipment.PK;
			Job job3 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job3.JH_ParentID = cFSGatePass.PK;
			Job job4 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job4.JH_ParentID = consol1.PK;
			Job job5 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job5.JH_ParentID = shippingBooking.PK;

			CreateNewChargeForJob(job1);
			CreateNewChargeForJob(job2);
			CreateNewChargeForJob(job3);
			CreateNewChargeForJob(job4);
			CreateNewChargeForJob(job5);

			ForwardingContainer fc1 = Factory.NewWithValidTestData<ForwardingContainer>();
			fc1.JC_ContainerNum = "A123";
			fc1.JC_JK = consol1.PK;
			fc1.JC_JS_FCLBookingOnlyLink = shippingBooking.PK;

			consol1.Shipments.AddRange(forwardingShipment, cFSShipment, cFSGatePass);

			forwardingShipment.OuterPackLines.AddNew().Containers.Add(fc1);
			cFSGatePass.OuterPackLines.AddNew().Containers.Add(fc1);

			Factory.Save();

			((ModuleTextFilter)Filters["Container Number"]).Property = "A123";
			((ModuleTextFilter)Filters["Container Number"]).IsActive = true;
			JobCollection collection = GetCollection(Filters);
			AssertEquals("Should be 5 Jobs", 5, collection.Count);
			Assert("Expecting collection to contain Job1", collection.Contains(job1));
			Assert("Expecting collection to contain Job2", collection.Contains(job2));
			Assert("Expecting collection to contain Job3", collection.Contains(job3));
			Assert("Expecting collection to contain Job4", collection.Contains(job4));
			Assert("Expecting collection to contain Job5", collection.Contains(job5));

			((ModuleTextFilter)Filters["Container Number"]).Property = "Y777";
			((ModuleTextFilter)Filters["Container Number"]).IsActive = true;
			collection = GetCollection(Filters);
			AssertEquals("Should be 0 Jobs", 0, collection.Count);
		}

		public void TestContainerModeFilter()
		{
			ForwardingConsol consol1 = null, consol2 = null;
			Job job1 = CreateJobLinkedToConsol(ref consol1);
			Job job2 = CreateJobLinkedToConsol(ref consol2);
			consol1.JK_ConsolMode = "LCL";
			consol2.JK_ConsolMode = "FCL";
			CreateNewChargeForJob(job1);
			CreateNewChargeForJob(job2);
			TestObjectCreator.Factory.Save();

			((ModuleTextFilter)Filters["ContainerMode"]).Property = "FCL";
			((ModuleTextFilter)Filters["ContainerMode"]).IsActive = true;
			AssertCollection(GetCollection(Filters), job2);
		}

		public void TestSendingAgentFilter()
		{
			ForwardingConsol consol1 = null, consol2 = null;
			Job job1 = CreateJobLinkedToConsol(ref consol1);
			Job job2 = CreateJobLinkedToConsol(ref consol2);
			consol2.SetDefaultSendingForwarderAddress(TestObjectCreator.AALSHI);
			consol2.SetDefaultSendingForwarderAddress(TestObjectCreator.ABIGAS);
			CreateNewChargeForJob(job1);
			CreateNewChargeForJob(job2);
			TestObjectCreator.Factory.Save();

			((ModuleGuidFilter)Filters["SendingAgent"]).Property = TestObjectCreator.ABIGAS.PK;
			((ModuleGuidFilter)Filters["SendingAgent"]).IsActive = true;
			AssertCollection(GetCollection(Filters), job2);
		}

		public void TestReceivingAgentFilter()
		{
			ForwardingConsol consol1 = null, consol2 = null;
			Job job1 = CreateJobLinkedToConsol(ref consol1);
			Job job2 = CreateJobLinkedToConsol(ref consol2);
			consol2.SetDefaultReceivingForwarderAddress(TestObjectCreator.AALSHI);
			consol2.SetDefaultReceivingForwarderAddress(TestObjectCreator.ABIGAS);
			CreateNewChargeForJob(job1);
			CreateNewChargeForJob(job2);
			TestObjectCreator.Factory.Save();

			((ModuleGuidFilter)Filters["ReceivingAgent"]).Property = TestObjectCreator.ABIGAS.PK;
			((ModuleGuidFilter)Filters["ReceivingAgent"]).IsActive = true;
			AssertCollection(GetCollection(Filters), job2);
		}

		public void TestFlightFilter()
		{
			ForwardingConsol consol1 = null, consol2 = null;
			Job job1 = CreateJobLinkedToConsol(ref consol1);
			Job job2 = CreateJobLinkedToConsol(ref consol2);

			CreateNewChargeForJob(job1);
			CreateNewChargeForJob(job2);

			consol1.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol2.JK_TransportMode = Core.Constants.TransportModes.Air;

			Transport transport1 = consol1.Transports[0];
			transport1.JW_VoyageFlight = "snth";
			Transport transport2 = consol2.Transports[0];
			transport2.JW_VoyageFlight = "aoeu";

			TestObjectCreator.Factory.Save();

			FilterStripCollection filterStrips = new FilterStripCollection(Filters.ModuleFilters);
			FilterStrip filterStrip1 = filterStrips.AddNew("Flight");
			((ModuleTextFilter)filterStrip1.CurrentModuleFilter).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			((ModuleTextFilter)filterStrip1.CurrentModuleFilter).Property = "aoeu";
			AssertCollection(GetCollection(Filters), job2);

			FilterStrip filterStrip2 = filterStrips.AddNew("Flight");
			((ModuleTextFilter)filterStrip2.CurrentModuleFilter).ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			((ModuleTextFilter)filterStrip2.CurrentModuleFilter).Property = "th";
			var actualCollection = GetCollection(Filters);
			AssertCollectionNotContains(job1, actualCollection);
			AssertCollectionNotContains(job2, actualCollection);

			filterStrip1.OrCategory = FilterOrCategory.Red;
			filterStrip2.OrCategory = FilterOrCategory.Red;
			AssertCollection(GetCollection(Filters), job1, job2);
		}

		public void TestHouseBillFilter()
		{
			ForwardingShipment shipment1 = null, shipment2 = null;
			Job job1 = CreateJobLinkedToShipment(ref shipment1);
			Job job2 = CreateJobLinkedToShipment(ref shipment2);
			shipment1.JS_HouseBill = new string('1', JobShipmentSchema.JS_HouseBill.MaxLength);
			shipment2.JS_HouseBill = "12345";
			CreateNewChargeForJob(job1);
			CreateNewChargeForJob(job2);
			TestObjectCreator.Factory.Save();

			((ModuleTextFilter)Filters["HouseBill"]).Property = new string('1', ((ModuleTextFilter)Filters["HouseBill"]).MaxLength);
			((ModuleTextFilter)Filters["HouseBill"]).IsActive = true;
			AssertCollection(GetCollection(Filters), job1);

			((ModuleTextFilter)Filters["HouseBill"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			((ModuleTextFilter)Filters["HouseBill"]).Property = "4";
			AssertCollection(GetCollection(Filters), job2);

			BaseJobDeclaration declaration = null;
			Job job3 = CreateJobLinkedToDeclaration(ref declaration);
			declaration.JE_HouseBill = new string('1', JobDeclarationSchema.JE_HouseBill.MaxLength);
			CreateNewChargeForJob(job3);
			TestObjectCreator.Factory.Save();
			((ModuleTextFilter)Filters["HouseBill"]).Property = new string('1', ((ModuleTextFilter)Filters["HouseBill"]).MaxLength);
			((ModuleTextFilter)Filters["HouseBill"]).IsActive = true;
			var collection = GetCollection(Filters);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(job3, collection, true);
		}

		public void TestMasterBillFilter()
		{
			ForwardingConsol consol1 = null, consol2 = null;
			var billOfLading = (CommonShipment)Factory.New<Freight.Integration.Agency.IBillOfLading>();
			var shipment = TestObjectCreator.CreateShipment("S123");
			Job job1 = CreateJobLinkedToConsol(ref consol1);
			Job job2 = CreateJobLinkedToConsol(ref consol2);
			Job job3 = TestObjectCreator.CreateJob(billOfLading, false);
			Job job4 = TestObjectCreator.CreateJob(shipment, false);
			consol1.JK_MasterBillNum = "12345";
			consol2.JK_MasterBillNum = "67890";
			billOfLading.JS_HouseBill = "44444";
			shipment.JS_HouseBill = "55555";
			CreateNewChargeForJob(job1);
			CreateNewChargeForJob(job2);
			CreateNewChargeForJob(job3);
			CreateNewChargeForJob(job4);
			TestObjectCreator.Factory.Save();

			((ModuleTextFilter)Filters["MasterBill"]).Property = "67890";
			((ModuleTextFilter)Filters["MasterBill"]).IsActive = true;
			AssertCollection(GetCollection(Filters), job2);

			((ModuleTextFilter)Filters["MasterBill"]).Property = "44444";
			((ModuleTextFilter)Filters["MasterBill"]).IsActive = true;
			AssertCollection(GetCollection(Filters), job3);

			((ModuleTextFilter)Filters["MasterBill"]).Property = "55555";
			((ModuleTextFilter)Filters["MasterBill"]).IsActive = true;
			AssertEquals(0, GetCollection(Filters).Count);

			((ModuleTextFilter)Filters["MasterBill"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			((ModuleTextFilter)Filters["MasterBill"]).Property = "67";
			AssertCollection(GetCollection(Filters), job1, job3);

			((ModuleTextFilter)Filters["MasterBill"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			((ModuleTextFilter)Filters["MasterBill"]).Property = "44";
			AssertCollection(GetCollection(Filters), job1, job2);

			((ModuleTextFilter)Filters["MasterBill"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			((ModuleTextFilter)Filters["MasterBill"]).Property = "55";
			AssertCollection(GetCollection(Filters), job1, job2, job3);
		}

		public override void TestCoLoadMasterBillFilter()
		{
			var consol1 = TestObjectCreator.CreateConsol("AUSYD", "AUBNE", "C01");
			var consol2 = TestObjectCreator.CreateConsol("AUBNE", "NZAKL", "C02");
			var consol3 = TestObjectCreator.CreateConsol("NZAKL", "USLAX", "C03");
			var consol4 = TestObjectCreator.CreateConsol("AUSYD", "AUBNE", "C04");
			var consol5 = TestObjectCreator.CreateConsol("AUSYD", "AUBNE", "C05");

			var shipment1 = TestObjectCreator.CreateShipment("S001", "AUSYD", "USLAX");
			consol1.Shipments.Add(shipment1);
			consol1.JK_AgentType = Enterprise.Core.Constants.AgentType.CoLoad;
			consol1.JK_CoLoadMasterBill = "CLD01";

			var shipment2 = TestObjectCreator.CreateShipment("S002", "AUSYD", "USLAX");
			consol2.Shipments.Add(shipment2);
			consol2.JK_AgentType = Enterprise.Core.Constants.AgentType.CoLoad;
			consol2.JK_CoLoadMasterBill = "CLD02";
			consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var shipment3 = TestObjectCreator.CreateShipment("S003", "AUSYD", "USLAX");
			consol3.Shipments.Add(shipment3);
			consol3.JK_AgentType = Enterprise.Core.Constants.AgentType.CoLoad;
			consol3.JK_CoLoadMasterBill = "CLD03";
			consol4.Shipments.Add(shipment3);
			consol4.JK_AgentType = Enterprise.Core.Constants.AgentType.CoLoad;
			consol4.JK_CoLoadMasterBill = "CLD04";
			consol4.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var shipment4 = TestObjectCreator.CreateShipment("S004", "AUSYD", "USLAX");
			consol5.Shipments.Add(shipment4);
			consol5.JK_CoLoadMasterBill = "";
			consol5.JK_AgentType = Enterprise.Core.Constants.AgentType.Direct;

			var shipment6 = TestObjectCreator.CreateShipment("S006", "AUSYD", "USLAX");
			var consol6 = TestObjectCreator.CreateConsol("AUSYD", "AUBNE", "C06");
			consol6.Shipments.Add(shipment6);
			consol6.JK_CoLoadMasterBill = "";
			consol6.JK_AgentType = Enterprise.Core.Constants.AgentType.CoLoad;

			Job job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_ParentID = shipment1.PK;
			CreateNewChargeForJob(job1);

			Job job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_ParentID = shipment2.PK;
			CreateNewChargeForJob(job2);

			Job job3 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job3.JH_ParentID = shipment3.PK;
			CreateNewChargeForJob(job3);

			Job job5 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job5.JH_ParentID = shipment4.PK;
			CreateNewChargeForJob(job5);

			Job job6 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job6.JH_ParentID = shipment6.PK;
			CreateNewChargeForJob(job6);

			BaseJobDeclaration declaration = null;
			Job job4 = CreateJobLinkedToDeclaration(ref declaration);
			declaration.JE_HouseBill = new string('1', JobDeclarationSchema.JE_HouseBill.MaxLength);
			CreateNewChargeForJob(job4);

			TestObjectCreator.Factory.Save();

			JobCollection collection = GetCollection(Filters);
			AssertCollectionContains("Should have contained this declaration job", job4, collection);

			((ModuleTextFilter)Filters["CoLoadMasterBill"]).Property = "XXXXXXXXX";
			((ModuleTextFilter)Filters["CoLoadMasterBill"]).IsActive = true;
			((ModuleTextFilter)Filters["CoLoadMasterBill"]).ComparisonOperator =
				ModuleTextFilter.ComparisonConstants.NotContain;
			AssertCollection(GetCollection(Filters), job1, job2, job3, job6);//When this filter is enabled the result will exclude jobs with no-shipment type.

			((ModuleTextFilter)Filters["CoLoadMasterBill"]).IsActive = true;
			((ModuleTextFilter)Filters["CoLoadMasterBill"]).ComparisonOperator =
				ModuleTextFilter.ComparisonConstants.IsBlank;
			AssertCollectionContains("Should be no declaration items in the collection because only jobs with shipment type included when this filter is enabled.", job6, GetCollection(Filters));

			((ModuleTextFilter)Filters["CoLoadMasterBill"]).Property = "CLD";
			((ModuleTextFilter)Filters["CoLoadMasterBill"]).IsActive = true;
			((ModuleTextFilter)Filters["CoLoadMasterBill"]).ComparisonOperator =
				ModuleTextFilter.ComparisonConstants.Contains;
			AssertCollection(GetCollection(Filters), job1, job2, job3);

			((ModuleTextFilter)Filters["CoLoadMasterBill"]).Property = "CLD01";
			((ModuleTextFilter)Filters["CoLoadMasterBill"]).IsActive = true;
			((ModuleTextFilter)Filters["CoLoadMasterBill"]).ComparisonOperator =
				ModuleTextFilter.ComparisonConstants.Contains;
			AssertCollection(GetCollection(Filters), job1);

			((ModuleTextFilter)Filters["CoLoadMasterBill"]).Property = "02";
			((ModuleTextFilter)Filters["CoLoadMasterBill"]).ComparisonOperator =
				ModuleTextFilter.ComparisonConstants.Contains;
			((ModuleTextFilter)Filters["CoLoadMasterBill"]).IsActive = true;
			AssertCollection(GetCollection(Filters), job2);

			((ModuleTextFilter)Filters["CoLoadMasterBill"]).Property = "04";
			((ModuleTextFilter)Filters["CoLoadMasterBill"]).ComparisonOperator =
				ModuleTextFilter.ComparisonConstants.Contains;
			((ModuleTextFilter)Filters["CoLoadMasterBill"]).IsActive = true;
			AssertCollection(GetCollection(Filters), job3);
		}

		public void TestMasterBillFilterWithExactComparison()
		{
			ForwardingConsol consol1 = null, consol2 = null;
			Job job1 = CreateJobLinkedToConsol(ref consol1);
			Job job2 = CreateJobLinkedToConsol(ref consol2);
			consol2.JK_MasterBillNum = "12345";
			consol2.JK_MasterBillNum = "67890";
			CreateNewChargeForJob(job1);
			CreateNewChargeForJob(job2);
			TestObjectCreator.Factory.Save();

			var masterBillFilter = (ModuleTextFilter)Filters["MasterBill"];
			masterBillFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			masterBillFilter.Property = "67890";
			masterBillFilter.IsActive = true;
			AssertCollection(GetCollection(Filters), job2);
		}

		public void TestDateFilter()
		{
			ForwardingShipment shipment1 = null, shipment2 = null, shipment3 = null;
			Job job1 = CreateJobLinkedToShipment(ref shipment1);
			Job job2 = CreateJobLinkedToShipment(ref shipment2);
			Job job3 = CreateJobLinkedToShipment(ref shipment3);
			shipment1.JS_E_ARV = ZDateTime.BrettsBirthday.AddYears(-1).AddDays(-100);
			shipment2.JS_E_ARV = ZDateTime.Now.AddDays(-10);
			shipment3.JS_E_DEP = ZDateTime.Now.AddDays(10);

			Charge charge1 = CreateNewChargeForJob(job1);
			charge1.JR_OSCostAmt = 10M;
			Charge charge2 = CreateNewChargeForJob(job2);
			charge2.JR_OSCostAmt = 11M;
			Charge charge3 = CreateNewChargeForJob(job3);
			charge3.JR_OSCostAmt = 12M;

			Factory.Save();

			Accrual accrual1 = Factory.Load<Accrual>(charge1.JR_AL_APLine);
			AssertNotNull(accrual1);
			accrual1.AL_PostDate = ZDateTime.BrettsBirthday.AddYears(-1);
			Accrual accrual2 = Factory.Load<Accrual>(charge2.JR_AL_APLine);
			AssertNotNull(accrual2);
			accrual2.AL_PostDate = ZDateTime.BrettsBirthday;
			Accrual accrual3 = Factory.Load<Accrual>(charge3.JR_AL_APLine);
			AssertNotNull(accrual3);
			accrual3.AL_PostDate = ZDateTime.Now;

			Factory.Save();

			ActivateAlwaysVisibleFilters();
			AssertCollection(GetCollection(Filters), job1, job2, job3);

			((ModuleDateFilter)Filters["PostDate"]).Property1 = ZDateTime.BrettsBirthday.AddDays(-100);
			((ModuleDateFilter)Filters["PostDate"]).Property2 = ZDateTime.Now.AddYears(-1);
			((ModuleDateFilter)Filters["PostDate"]).IsActive = true;
			((ModuleDateFilter)Filters["PostDate"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertCollection(GetCollection(Filters), job2);

			((ModuleDateFilter)Filters["PostDate"]).IsActive = false;

			((ModuleDateFilter)Filters["ShipmentETA_ETD"]).Property1 = ZDateTime.BrettsBirthday.AddDays(-10);
			((ModuleDateFilter)Filters["ShipmentETA_ETD"]).Property2 = ZDateTime.Now;
			((ModuleDateFilter)Filters["ShipmentETA_ETD"]).IsActive = true;
			((ModuleDateFilter)Filters["ShipmentETA_ETD"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			JobCollection collection = GetCollection(Filters);
			AssertCollection(collection, job2);
		}

		public void TestAccountingDateFilter()
		{
			AssertEquals(0, Filters.ModuleFilters.Count(x => x.Description == "Accounting Date"));

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Canada))
			{
				// Reset Invoice to create a new one with CA company.
				invoice = null;
				var filtersCA = (InvoicingBaseBulkChargeImporterFilters)GetNewFilters();
				var accDatefilter = (ModuleDateFilter)filtersCA["Accounting Date"];
				AssertEquals(FilterCategories.Dates, accDatefilter.Category);

				var declaration1 = TestObjectCreator.CreateDeclaration("B001");
				declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("Enterprise.Customs.CA.Business.JobDeclaration", declaration1.GetType().FullName);
				var propertyInfo = declaration1.GetType().GetProperty("CA_K84AccountingDate", BindingFlags.Instance | BindingFlags.Public);
				propertyInfo.SetValue(declaration1, new ZDateTime(2011, 2, 1));
				var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
				job1.JH_ParentID = declaration1.PK;

				var declaration2 = TestObjectCreator.CreateDeclaration("B002");
				declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("Enterprise.Customs.CA.Business.JobDeclaration", declaration2.GetType().FullName);
				propertyInfo.SetValue(declaration2, new ZDateTime(2011, 2, 3));
				var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
				job2.JH_ParentID = declaration2.PK;

				CreateNewChargeForJob(job1);
				CreateNewChargeForJob(job2);

				Factory.Save();

				ActivateAlwaysVisibleFilters(filtersCA);
				var result = GetCollection(filtersCA);
				AssertCollection(result, job1, job2);

				accDatefilter.Property1 = new ZDateTime(2011, 1, 20);
				accDatefilter.Property2 = new ZDateTime(2011, 2, 2);
				accDatefilter.IsActive = true;
				accDatefilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				result = GetCollection(filtersCA);
				AssertCollection(result, job1);
			}
		}

		public void TestShipmentETA_ETDFilter()
		{
			var shipment = TestObjectCreator.CreateShipment("S1");
			shipment.JS_E_ARV = new ZDateTime(2012, 11, 19);
			shipment.JS_E_DEP = new ZDateTime(2012, 08, 24);
			var job = TestObjectCreator.CreateJob(shipment);
			CreateNewChargeForJob(job);
			Factory.Save();

			((ModuleDateFilter)Filters["ShipmentETA_ETD"]).Property1 = new ZDateTime(2012, 11, 01);
			((ModuleDateFilter)Filters["ShipmentETA_ETD"]).Property2 = new ZDateTime(2012, 11, 30);
			((ModuleDateFilter)Filters["ShipmentETA_ETD"]).IsActive = true;
			((ModuleDateFilter)Filters["ShipmentETA_ETD"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			JobCollection jobCollection = GetCollection(Filters);
			AssertCollection(jobCollection, job);

			((ModuleDateFilter)Filters["ShipmentETA_ETD"]).Property1 = new ZDateTime(2012, 08, 01);
			((ModuleDateFilter)Filters["ShipmentETA_ETD"]).Property2 = new ZDateTime(2012, 08, 31);
			((ModuleDateFilter)Filters["ShipmentETA_ETD"]).IsActive = true;
			((ModuleDateFilter)Filters["ShipmentETA_ETD"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			jobCollection = GetCollection(Filters);
			AssertCollection(jobCollection, job);

			((ModuleDateFilter)Filters["ShipmentETA_ETD"]).Property1 = new ZDateTime(2012, 08, 01);
			((ModuleDateFilter)Filters["ShipmentETA_ETD"]).Property2 = new ZDateTime(2012, 11, 30);
			((ModuleDateFilter)Filters["ShipmentETA_ETD"]).IsActive = true;
			((ModuleDateFilter)Filters["ShipmentETA_ETD"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			jobCollection = GetCollection(Filters);
			AssertCollection(jobCollection, job);

			((ModuleDateFilter)Filters["ShipmentETA_ETD"]).Property1 = new ZDateTime(2012, 10, 29);
			((ModuleDateFilter)Filters["ShipmentETA_ETD"]).Property2 = new ZDateTime(2012, 10, 31);
			((ModuleDateFilter)Filters["ShipmentETA_ETD"]).IsActive = true;
			((ModuleDateFilter)Filters["ShipmentETA_ETD"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			jobCollection = GetCollection(Filters);
			AssertCollectionNotContains(job, jobCollection);
		}

		public void TestIncludeAccrualsWithNoCreditor()
		{
			ModuleFlagsFilter flagFilter = (ModuleFlagsFilter)Filters["IncludeAccrualsWithNoCreditor"];
			AssertEquals("IncludeAccrualsWithNoCreditor Filter should be Always Visible", FilterVisibility.AlwaysVisible, flagFilter.Visibility);
			AssertEquals("IncludeAccrualsWithNoCreditor Filter should default to true", true, flagFilter.Property0);

			Charge charge1 = CreateNewCharge();
			Charge charge2 = CreateNewCharge();
			Charge charge3 = CreateNewCharge();
			charge2.JR_OH_CostAccount = TestObjectCreator.ABIGAS.PK;
			charge3.JR_OH_CostAccount = ZGuid.Empty;
			Job job1 = CreateNewJobForCharge(charge1);
			Job job2 = CreateNewJobForCharge(charge2);
			Job job3 = CreateNewJobForCharge(charge3);
			Factory.Save();

			ActivateAlwaysVisibleFilters();
			AssertCollection(GetCollection(Filters), job1, job3);

			flagFilter.Property0 = false;
			AssertEquals("IncludeAccrualsWithNoCreditor Filter should be false", false, flagFilter.Property0);
			AssertCollection(GetCollection(Filters), job1);
		}

		public void TestAllOtherCreditors()
		{
			bool origIncludeChargesForAllOtherCreditors = AccountingConfigurationRegistry.Instance.IncludeChargesForAllOtherCreditors.Value;
			bool origIncludeChargesForCreditorsWithTheSameAPSettlementGroup = AccountingConfigurationRegistry.Instance.IncludeChargesForCreditorsWithTheSameAPSettlementGroup.Value;
			try
			{
				AccountingConfigurationRegistry.Instance.IncludeChargesForAllOtherCreditors.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				AccountingConfigurationRegistry.Instance.IncludeChargesForCreditorsWithTheSameAPSettlementGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				var fFilters = new InvoicingBaseBulkChargeImporterFilters(Invoice);
				ModuleTextFilter textFilter = (ModuleTextFilter)fFilters["OtherCreditors"];
				AssertEquals("OtherCreditors filter should be visible", FilterVisibility.Visible, textFilter.Visibility);
				AssertEquals("OtherCreditors filter property should be empty", ZString.Empty, textFilter.Property);

				AccountingConfigurationRegistry.Instance.IncludeChargesForAllOtherCreditors.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.IncludeChargesForCreditorsWithTheSameAPSettlementGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				fFilters = new InvoicingBaseBulkChargeImporterFilters(Invoice);
				textFilter = (ModuleTextFilter)fFilters["OtherCreditors"];
				AssertEquals("OtherCreditors filter should be always visible", FilterVisibility.AlwaysVisible, textFilter.Visibility);
				AssertEquals("OtherCreditors filter property should be set", InvoicingBaseBulkChargeImporterFilters.OtherCreditors.AllOtherCreditors, textFilter.Property);

				Charge charge1 = CreateNewCharge();
				Charge charge2 = CreateNewCharge();
				Charge charge3 = CreateNewCharge();
				Charge charge4 = CreateNewCharge();
				charge2.JR_OH_CostAccount = TestObjectCreator.ABIGAS.PK;
				charge3.JR_OH_CostAccount = TestObjectCreator.ZECTRA.PK;
				charge4.JR_OH_CostAccount = ZGuid.Empty;
				Job job1 = CreateNewJobForCharge(charge1);
				Job job2 = CreateNewJobForCharge(charge2);
				Job job3 = CreateNewJobForCharge(charge3);
				Job job4 = CreateNewJobForCharge(charge4);
				Factory.Save();

				ActivateAlwaysVisibleFilters();
				AssertCollection(GetCollection(Filters), job1, job2, job3, job4);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.IncludeChargesForAllOtherCreditors.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, origIncludeChargesForAllOtherCreditors);
				AccountingConfigurationRegistry.Instance.IncludeChargesForCreditorsWithTheSameAPSettlementGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, origIncludeChargesForCreditorsWithTheSameAPSettlementGroup);
			}
		}

		[SuspendGLAccountAndChargeCodeCriticalValidation]
		public void TestCreditorsWithTheSameAPSettlementGroup()
		{
			bool origIncludeChargesForAllOtherCreditors = AccountingConfigurationRegistry.Instance.IncludeChargesForAllOtherCreditors.Value;
			bool origIncludeChargesForCreditorsWithTheSameAPSettlementGroup = AccountingConfigurationRegistry.Instance.IncludeChargesForCreditorsWithTheSameAPSettlementGroup.Value;
			try
			{
				/*
				 * invoice creditor = AALSHI -> creditor filter -> AALSHI
				 * create charge with JR_OH_CostAccount = AALSHI		-> no related party group
				 * create charge with JR_OH_CostAccount = ABIGAS		-> with related party group AALSHI
				 * create charge with JR_OH_CostAccount = ZECTRA		-> no related party group
				 * create charge with JR_OH_CostAccount = ZGuid.Empty	-> no related party group
				 */

				Charge charge1 = CreateNewCharge();
				Charge charge2 = CreateNewCharge();
				Charge charge3 = CreateNewCharge();
				Charge charge4 = CreateNewCharge();
				Charge charge5 = CreateNewCharge();
				charge5.JR_AL_APLine = Factory.NewWithValidTestData<APInvoice>().Lines.AddNew().PK;
				charge5.APLine.AL_OSAmount = charge5.APLine.AL_LineAmount = -charge5.JR_OSCostAmt;

				charge2.JR_OH_CostAccount = TestObjectCreator.ABIGAS.PK;
				charge3.JR_OH_CostAccount = TestObjectCreator.ZECTRA.PK;
				charge4.JR_OH_CostAccount = ZGuid.Empty;
				Job job1 = CreateNewJobForCharge(charge1);
				Job job2 = CreateNewJobForCharge(charge2);
				Job job3 = CreateNewJobForCharge(charge3);
				Job job4 = CreateNewJobForCharge(charge4);
				Job job5 = CreateNewJobForCharge(charge5);

				OrgRelatedParty relatedParty2 = Factory.New<OrgRelatedParty>();
				relatedParty2.PR_GC = GlbCompany.CurrentCompany.PK;
				relatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
				relatedParty2.PR_FreightDirection = RelatedPartyDirectionList.Codes.AP;
				relatedParty2.PR_OH_RelatedParty = TestObjectCreator.AALSHI.PK;
				relatedParty2.PR_OH_Parent = TestObjectCreator.ABIGAS.PK;
				Factory.Save();

				AccountingConfigurationRegistry.Instance.IncludeChargesForAllOtherCreditors.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.IncludeChargesForCreditorsWithTheSameAPSettlementGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				var fFilters = new InvoicingBaseBulkChargeImporterFilters(Invoice);
				ModuleTextFilter textFilter = (ModuleTextFilter)fFilters["OtherCreditors"];
				AssertEquals("OtherCreditors filter should be always visible", FilterVisibility.AlwaysVisible, textFilter.Visibility);
				AssertEquals("OtherCreditors filter property should be set", InvoicingBaseBulkChargeImporterFilters.OtherCreditors.CreditorsSharingSameAPSettlementGroup, textFilter.Property);

				ActivateAlwaysVisibleFilters();
				AssertCollection(GetCollection(Filters), job1, job2, job4);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.IncludeChargesForAllOtherCreditors.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, origIncludeChargesForAllOtherCreditors);
				AccountingConfigurationRegistry.Instance.IncludeChargesForCreditorsWithTheSameAPSettlementGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, origIncludeChargesForCreditorsWithTheSameAPSettlementGroup);
			}
		}

		public void TestInvoiceDetailsFilter()
		{
			Invoice.AH_TransactionNum = "QQQQQ";
			Charge charge1 = CreateNewCharge();
			Charge charge2 = CreateNewCharge();
			Job job1 = CreateNewJobForCharge(charge1);
			Job job2 = CreateNewJobForCharge(charge2);
			Factory.Save();

			ActivateAlwaysVisibleFilters();
			AssertCollection(GetCollection(Filters), job1, job2);

			charge1.JR_APInvoiceNum = "QQQQQ";
			Factory.Save();

			AssertCollection(GetCollection(Filters), job1, job2);

			charge1.JR_APInvoiceNum = "AAA";
			Factory.Save();

			AssertCollection(GetCollection(Filters), job2);
		}

		public void TestChildCollectionForFiltersWithCategories_Creditor()
		{
			Charge charge1 = CreateNewCharge();
			Charge charge2 = CreateNewCharge();
			charge2.JR_OH_CostAccount = TestObjectCreator.ABIGAS.PK;
			Job job1 = CreateNewJobForCharge(charge1);
			Job job2 = CreateNewJobForCharge(charge2);
			Factory.Save();

			var filters = (InvoicingBaseBulkChargeImporterFilters)GetNewFilters();
			FilterStripCollection filterStrips = new FilterStripCollection(filters.ModuleFilters);
			FilterStrip filterStrip1 = filterStrips.AddNew("Creditor");
			filterStrip1.OrCategory = FilterOrCategory.Red;
			((ModuleGuidFilter)filterStrip1.CurrentModuleFilter).Property = TestObjectCreator.AALSHI.PK;
			FilterStrip filterStrip2 = filterStrips.AddNew("Creditor");
			filterStrip2.OrCategory = FilterOrCategory.Red;
			((ModuleGuidFilter)filterStrip2.CurrentModuleFilter).Property = TestObjectCreator.ABIGAS.PK;

			AssertCollection(GetCollection(filters), job1, job2);

			var collection = Factory.Load<Charge>(filters.GetChildQuery());
			AssertCollection(collection, charge1, charge2);
		}

		public void TestChildCollectionForFiltersWithCategories_ChargeCode()
		{
			Charge charge1 = CreateNewCharge();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_OSCostAmt = 100m;
			Charge charge2 = CreateNewCharge();
			charge2.JR_AC = TestObjectCreator.CC2.PK;
			charge2.JR_OSCostAmt = 200m;
			Job job1 = CreateNewJobForCharge(charge1);
			Job job2 = CreateNewJobForCharge(charge2);
			Factory.Save();

			FilterStripCollection filterStrips = new FilterStripCollection(Filters.ModuleFilters);
			FilterStrip filterStrip1 = filterStrips.AddNew("ChargeCode");
			filterStrip1.OrCategory = FilterOrCategory.Red;
			((ModuleGuidFilter)filterStrip1.CurrentModuleFilter).Property = TestObjectCreator.CC1.PK;
			FilterStrip filterStrip2 = filterStrips.AddNew("ChargeCode");
			filterStrip2.OrCategory = FilterOrCategory.Red;
			((ModuleGuidFilter)filterStrip2.CurrentModuleFilter).Property = TestObjectCreator.CC2.PK;

			AssertCollection(GetCollection(Filters), job1, job2);

			var collection = Factory.Load<Charge>(Filters.GetChildQuery());
			AssertCollection(collection, charge1, charge2);
		}

		public void TestChildCollectionForFiltersWithCategories_Currency()
		{
			Charge charge1 = CreateNewCharge();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_RX_NKCostCurrency = "AUD";
			charge1.JR_OSCostAmt = 100m;
			Charge charge2 = CreateNewCharge();
			charge2.JR_AC = TestObjectCreator.CC2.PK;
			charge2.JR_RX_NKCostCurrency = "USD";
			charge2.JR_OSCostExRate = 1M;
			charge2.JR_OSCostAmt = 200m;
			Job job1 = CreateNewJobForCharge(charge1);
			Job job2 = CreateNewJobForCharge(charge2);
			Factory.Save();

			FilterStripCollection filterStrips = new FilterStripCollection(Filters.ModuleFilters);
			FilterStrip filterStrip1 = filterStrips.AddNew("Currency");
			filterStrip1.OrCategory = FilterOrCategory.Red;
			((ModuleNkFilter)filterStrip1.CurrentModuleFilter).Property = "AUD";
			FilterStrip filterStrip2 = filterStrips.AddNew("Currency");
			filterStrip2.OrCategory = FilterOrCategory.Red;
			((ModuleNkFilter)filterStrip2.CurrentModuleFilter).Property = "USD";

			AssertCollection(GetCollection(Filters), job1, job2);

			var collection = Factory.Load<Charge>(Filters.GetChildQuery());
			AssertCollection(collection, charge1, charge2);
		}

		public void TestChildCollectionForFiltersWithCategories_ExcludeReverseSignedConsolCostsFilter()
		{
			Charge charge1 = CreateNewCharge();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_OSCostAmt = 100m;
			Charge charge2 = CreateNewCharge();
			charge2.JR_AC = TestObjectCreator.CC2.PK;
			charge2.JR_OSCostAmt = -200m;
			Job job1 = CreateNewJobForCharge(charge1);
			Job job2 = CreateNewJobForCharge(charge2);
			Factory.Save();

			FilterStripCollection filterStrips = new FilterStripCollection(Filters.ModuleFilters);
			FilterStrip filterStrip1 = filterStrips.AddNew(InvoiceBulkOperationFilterHelper.ExcludeReverseSignedChargesFilterName);
			filterStrip1.OrCategory = FilterOrCategory.Red;
			((ModuleFlagsFilter)filterStrip1.CurrentModuleFilter).Property0 = true;
			FilterStrip filterStrip2 = filterStrips.AddNew(InvoiceBulkOperationFilterHelper.ExcludeReverseSignedChargesFilterName);
			filterStrip2.OrCategory = FilterOrCategory.Red;
			((ModuleFlagsFilter)filterStrip2.CurrentModuleFilter).Property0 = false;

			AssertCollection(GetCollection(Filters), job1, job2);

			var collection = Factory.Load<Charge>(Filters.GetChildQuery());
			AssertCollection(collection, charge1, charge2);
		}

		#region PostDate Filter

		public void TestChildCollectionForFiltersWithCategories_PostDate()
		{
			var charge1 = CreateNewCharge();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_OSCostAmt = 100m;
			var charge2 = CreateNewCharge();
			charge2.JR_AC = TestObjectCreator.CC2.PK;
			charge2.JR_OSCostAmt = 200m;
			var job1 = CreateNewJobForCharge(charge1);
			var job2 = CreateNewJobForCharge(charge2);
			Factory.Save();

			var accrual1 = Factory.Load<Accrual>(charge1.JR_AL_APLine);
			AssertNotNull(accrual1);
			accrual1.AL_PostDate = ZDateTime.BrettsBirthday.AddYears(-1);
			var accrual2 = Factory.Load<Accrual>(charge2.JR_AL_APLine);
			AssertNotNull(accrual2);
			accrual2.AL_PostDate = ZDateTime.BrettsBirthday;
			Factory.Save();

			SetupPostDateFilters();

			AssertCollection(GetCollection(Filters), job1, job2);

			var collection = Factory.Load<Charge>(Filters.GetChildQuery());
			AssertCollection(collection, charge1, charge2);
		}

		public void TestChildCollectionForFiltersWithCategories_PostDateIngoresCSTlines()
		{
			var charge1 = CreateNewCharge();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_OSCostAmt = 100m;
			charge1.JR_GB_CostTaxBranch = GlbBranch.CurrentBranch.PK;
			var charge2 = CreateNewCharge();
			charge2.JR_AC = TestObjectCreator.CC2.PK;
			charge2.JR_OSCostAmt = 200m;
			charge2.JR_GB_CostTaxBranch = GlbBranch.CurrentBranch.PK;
			var job1 = CreateNewJobForCharge(charge1);
			var job2 = CreateNewJobForCharge(charge2);
			var charge3 = CreateNewCharge();
			charge3.JR_AC = TestObjectCreator.CC1.PK;
			charge3.JR_OSCostAmt = 100m;
			charge3.JR_JH = job1.PK;
			var charge4 = CreateNewCharge();
			charge4.JR_AC = TestObjectCreator.CC2.PK;
			charge4.JR_OSCostAmt = 200m;
			charge4.JR_JH = job2.PK;

			var line1 = TestObjectCreator.CreateCostLine(charge1, Invoice.PK);
			var line2 = TestObjectCreator.CreateCostLine(charge2, Invoice.PK);
			line1.AL_PostDate = ZDateTime.BrettsBirthday;
			line1.AL_GB_TaxBranch = GlbBranch.CurrentBranch.PK;
			line2.AL_PostDate = ZDateTime.BrettsBirthday;
			line2.AL_GB_TaxBranch = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			AssertEquals("Precondition: line1.AL_PostDate", ZDateTime.BrettsBirthday, line1.AL_PostDate);
			AssertEquals("Precondition: line2.AL_PostDate", ZDateTime.BrettsBirthday, line2.AL_PostDate);
			SetupPostDateFilters();

			AssertCollection(GetCollection(Filters));

			var collection = Factory.Load<Charge>(Filters.GetChildQuery());
			AssertCollection(collection);

			var accrual1 = Factory.Load<Accrual>(charge3.JR_AL_APLine);
			AssertNotNull(accrual1);
			accrual1.AL_PostDate = ZDateTime.BrettsBirthday;
			var accrual2 = Factory.Load<Accrual>(charge4.JR_AL_APLine);
			AssertNotNull(accrual2);
			accrual2.AL_PostDate = ZDateTime.BrettsBirthday;
			Factory.Save();

			AssertCollection(GetCollection(Filters), job1, job2);

			collection = Factory.Load<Charge>(Filters.GetChildQuery());
			AssertCollection(collection, charge3, charge4);
		}

		public void TestChildCollectionForFiltersWithCategories_PostDate_QueryAnalysis()
		{
			SetupPostDateFilters();

			var jobQuery = Filters.GetQuery().LiteralTextSqlFormatted;
			var chargeQuery = Filters.GetChildQuery().LiteralTextSqlFormatted;

			CombineAssertions(() =>
			{
				AssertEquals("How many times JobCharge table in job query", 1, Regex.Matches(jobQuery, "JobCharge").Count);
				AssertEquals("How many times AccTransactionLines table in job query", 1, Regex.Matches(jobQuery, "AccTransactionLines").Count);
				AssertEquals("How many times AccTransactionLines table in charge query", 1, Regex.Matches(chargeQuery, "AccTransactionLines").Count);
			});
		}

		void SetupPostDateFilters()
		{
			var filterStrips = new FilterStripCollection(Filters.ModuleFilters);
			var filterStrip1 = filterStrips.AddNew("PostDate");
			filterStrip1.OrCategory = FilterOrCategory.Red;
			((ModuleDateFilter)filterStrip1.CurrentModuleFilter).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)filterStrip1.CurrentModuleFilter).Property1 = ZDateTime.BrettsBirthday.AddYears(-1);
			((ModuleDateFilter)filterStrip1.CurrentModuleFilter).Property2 = ZDateTime.BrettsBirthday.AddYears(-1).AddDays(1);
			var filterStrip2 = filterStrips.AddNew("PostDate");
			filterStrip2.OrCategory = FilterOrCategory.Red;
			((ModuleDateFilter)filterStrip2.CurrentModuleFilter).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)filterStrip2.CurrentModuleFilter).Property1 = ZDateTime.BrettsBirthday;
			((ModuleDateFilter)filterStrip2.CurrentModuleFilter).Property2 = ZDateTime.BrettsBirthday.AddDays(1);
		}

		#endregion

		public void TestChildCollectionForFiltersWithCategories_Branch()
		{
			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			GlbBranch branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			Charge charge1 = CreateNewCharge();
			charge1.JR_GB = branch1.PK;
			Charge charge2 = CreateNewCharge();
			charge2.JR_GB = branch2.PK;
			Job job1 = CreateNewJobForCharge(charge1);
			Job job2 = CreateNewJobForCharge(charge2);
			Factory.Save();

			FilterStripCollection filterStrips = new FilterStripCollection(Filters.ModuleFilters);
			FilterStrip filterStrip1 = filterStrips.AddNew(InvoiceBulkOperationFilterHelper.BranchFilterName);
			filterStrip1.OrCategory = FilterOrCategory.Red;
			((ModuleGuidFilter)filterStrip1.CurrentModuleFilter).Property = branch1.PK;
			FilterStrip filterStrip2 = filterStrips.AddNew(InvoiceBulkOperationFilterHelper.BranchFilterName);
			filterStrip2.OrCategory = FilterOrCategory.Red;
			((ModuleGuidFilter)filterStrip2.CurrentModuleFilter).Property = branch2.PK;

			AssertCollection(GetCollection(Filters), job1, job2);

			var collection = Factory.Load<Charge>(Filters.GetChildQuery());
			AssertCollection(collection, charge1, charge2);
		}

		public void TestChildCollectionForFiltersWithCategories_Department()
		{
			GlbDepartment department1 = Factory.NewWithValidTestData<GlbDepartment>();
			GlbDepartment department2 = Factory.NewWithValidTestData<GlbDepartment>();
			Charge charge1 = CreateNewCharge();
			charge1.JR_GE = department1.PK;
			Charge charge2 = CreateNewCharge();
			charge2.JR_GE = department2.PK;
			Job job1 = CreateNewJobForCharge(charge1);
			Job job2 = CreateNewJobForCharge(charge2);
			Factory.Save();

			FilterStripCollection filterStrips = new FilterStripCollection(Filters.ModuleFilters);
			FilterStrip filterStrip1 = filterStrips.AddNew(InvoiceBulkOperationFilterHelper.DepartmentFilterName);
			filterStrip1.OrCategory = FilterOrCategory.Red;
			((ModuleGuidFilter)filterStrip1.CurrentModuleFilter).Property = department1.PK;
			FilterStrip filterStrip2 = filterStrips.AddNew(InvoiceBulkOperationFilterHelper.DepartmentFilterName);
			filterStrip2.OrCategory = FilterOrCategory.Red;
			((ModuleGuidFilter)filterStrip2.CurrentModuleFilter).Property = department2.PK;

			AssertCollection(GetCollection(Filters), job1, job2);

			var collection = Factory.Load<Charge>(Filters.GetChildQuery());
			AssertCollection(collection, charge1, charge2);
		}

		public void TestChildCollectionForFiltersWithCategories_PlaceOfSupply()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var placeOfSupplyCode1 = PlaceOfSupplyListProvider.GetPlaceOfSupplyList(GlbCompany.CurrentCompany)[0].Code;
				var placeOfSupplyCode2 = PlaceOfSupplyListProvider.GetPlaceOfSupplyList(GlbCompany.CurrentCompany)[2].Code;
				var charge1 = CreateNewCharge();
				charge1.JR_CostPlaceOfSupply = placeOfSupplyCode1;
				var charge2 = CreateNewCharge();
				charge2.JR_CostPlaceOfSupply = placeOfSupplyCode2;
				var charge3 = CreateNewCharge();
				var job1 = CreateNewJobForCharge(charge1);
				var job2 = CreateNewJobForCharge(charge2);
				var job3 = CreateNewJobForCharge(charge3);
				Factory.Save();

				var filterStrips = new FilterStripCollection(Filters.ModuleFilters);
				var filterStrip1 = filterStrips.AddNew(InvoiceBulkOperationFilterHelper.FixedPlaceOfSupplyFilterName);
				((ModuleTextFilter)filterStrip1.CurrentModuleFilter).Property = placeOfSupplyCode1;

				AssertCollection(GetCollection(Filters), job1);

				var collection = Factory.Load<Charge>(Filters.GetChildQuery());
				AssertCollection(collection, charge1);

				((ModuleTextFilter)filterStrip1.CurrentModuleFilter).Property = placeOfSupplyCode2;
				AssertCollection(GetCollection(Filters), job2);

				collection = Factory.Load<Charge>(Filters.GetChildQuery());
				AssertCollection(collection, charge2);

				((ModuleTextFilter)filterStrip1.CurrentModuleFilter).Property = "**";
				AssertCollection(GetCollection(Filters));

				collection = Factory.Load<Charge>(Filters.GetChildQuery());
				AssertCollection(collection);
			}
		}

		public void TestSupplierCostReferenceFilter()
		{
			Charge charge1 = CreateNewCharge();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_OSCostAmt = 100m;
			charge1.JR_CostReference = "ABC1";
			Charge charge2 = CreateNewCharge();
			charge2.JR_AC = TestObjectCreator.CC2.PK;
			charge2.JR_OSCostAmt = 200m;
			charge2.JR_CostReference = "ABC2";
			Job job1 = CreateNewJobForCharge(charge1);
			Job job2 = CreateNewJobForCharge(charge2);

			Factory.Save();

			((ModuleNumberFilter)Filters["SupplierCostReference"]).Property = charge2.JR_CostReference;
			((ModuleNumberFilter)Filters["SupplierCostReference"]).IsActive = true;
			AssertCollection(GetCollection(Filters), job2);
		}

		public void TestATA_ATDFilter()
		{
			ForwardingConsol consol1 = null, consol2 = null;
			Job job1 = CreateJobLinkedToConsol(ref consol1);
			Job job2 = CreateJobLinkedToConsol(ref consol2);
			consol1.JK_TransportMode = "SEA";
			consol2.JK_TransportMode = "AIR";
			CreateNewChargeForJob(job1);
			CreateNewChargeForJob(job2);

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
			JobCollection jobCollection = GetCollection(Filters);
			AssertCollection(jobCollection, new Job[] { job1 });

			((ModuleDateFilter)Filters["ConsolATA_ATD"]).Property1 = new ZDateTime(2014, 10, 19);
			((ModuleDateFilter)Filters["ConsolATA_ATD"]).Property2 = new ZDateTime(2014, 10, 22);
			((ModuleDateFilter)Filters["ConsolATA_ATD"]).IsActive = true;
			((ModuleDateFilter)Filters["ConsolATA_ATD"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			jobCollection = GetCollection(Filters);
			AssertCollection(jobCollection, new Job[] { job2 });

			((ModuleDateFilter)Filters["ConsolATA_ATD"]).IsActive = true;
			((ModuleDateFilter)Filters["ConsolATA_ATD"]).PropertySearch = ModuleDateFilter.HasDateEntered;
			jobCollection = GetCollection(Filters);
			AssertCollection(jobCollection, new Job[] { job1, job2 });

			((ModuleDateFilter)Filters["ConsolATA_ATD"]).IsActive = true;
			((ModuleDateFilter)Filters["ConsolATA_ATD"]).PropertySearch = ModuleDateFilter.HasNoDateEntered;
			jobCollection = GetCollection(Filters);
			AssertCollection(jobCollection, Array.Empty<Job>());
		}

		public void TestDeliveryDateFilter()
		{
			ForwardingShipment shipment1 = null, shipment2 = null;
			Job job1 = CreateJobLinkedToShipment(ref shipment1);
			Job job2 = CreateJobLinkedToShipment(ref shipment2);
			CreateNewChargeForJob(job1);
			CreateNewChargeForJob(job2);

			shipment1.DocsAndCartage.JP_DeliveryCartageCompleted = new ZDateTime(2014, 10, 10);
			shipment2.DocsAndCartage.JP_DeliveryCartageCompleted = new ZDateTime(2014, 10, 21);

			Factory.Save();

			((ModuleDateFilter)Filters["ShipmentADD"]).Property1 = new ZDateTime(2014, 10, 09);
			((ModuleDateFilter)Filters["ShipmentADD"]).Property2 = new ZDateTime(2014, 10, 15);
			((ModuleDateFilter)Filters["ShipmentADD"]).IsActive = true;
			((ModuleDateFilter)Filters["ShipmentADD"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			JobCollection jobCollection = GetCollection(Filters);
			AssertCollection(jobCollection, new Job[] { job1 });

			((ModuleDateFilter)Filters["ShipmentADD"]).Property1 = new ZDateTime(2014, 10, 19);
			((ModuleDateFilter)Filters["ShipmentADD"]).Property2 = new ZDateTime(2014, 10, 22);
			((ModuleDateFilter)Filters["ShipmentADD"]).IsActive = true;
			((ModuleDateFilter)Filters["ShipmentADD"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			jobCollection = GetCollection(Filters);
			AssertCollection(jobCollection, new Job[] { job2 });

			((ModuleDateFilter)Filters["ShipmentADD"]).IsActive = true;
			((ModuleDateFilter)Filters["ShipmentADD"]).PropertySearch = ModuleDateFilter.HasDateEntered;
			jobCollection = GetCollection(Filters);
			AssertCollection(jobCollection, new Job[] { job1, job2 });

			((ModuleDateFilter)Filters["ShipmentADD"]).IsActive = true;
			((ModuleDateFilter)Filters["ShipmentADD"]).PropertySearch = ModuleDateFilter.HasNoDateEntered;
			jobCollection = GetCollection(Filters);
			AssertCollection(jobCollection, Array.Empty<Job>());
		}

		public void TestPickupDateFilter()
		{
			ForwardingShipment shipment1 = null, shipment2 = null;
			Job job1 = CreateJobLinkedToShipment(ref shipment1);
			Job job2 = CreateJobLinkedToShipment(ref shipment2);
			CreateNewChargeForJob(job1);
			CreateNewChargeForJob(job2);

			shipment1.DocsAndCartage.JP_PickupCartageCompleted = new ZDateTime(2014, 10, 10);
			shipment2.DocsAndCartage.JP_PickupCartageCompleted = new ZDateTime(2014, 10, 21);

			Factory.Save();

			((ModuleDateFilter)Filters["ShipmentAPD"]).Property1 = new ZDateTime(2014, 10, 09);
			((ModuleDateFilter)Filters["ShipmentAPD"]).Property2 = new ZDateTime(2014, 10, 15);
			((ModuleDateFilter)Filters["ShipmentAPD"]).IsActive = true;
			((ModuleDateFilter)Filters["ShipmentAPD"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			JobCollection jobCollection = GetCollection(Filters);
			AssertCollection(jobCollection, new Job[] { job1 });

			((ModuleDateFilter)Filters["ShipmentAPD"]).Property1 = new ZDateTime(2014, 10, 19);
			((ModuleDateFilter)Filters["ShipmentAPD"]).Property2 = new ZDateTime(2014, 10, 22);
			((ModuleDateFilter)Filters["ShipmentAPD"]).IsActive = true;
			((ModuleDateFilter)Filters["ShipmentAPD"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			jobCollection = GetCollection(Filters);
			AssertCollection(jobCollection, new Job[] { job2 });

			((ModuleDateFilter)Filters["ShipmentAPD"]).IsActive = true;
			((ModuleDateFilter)Filters["ShipmentAPD"]).PropertySearch = ModuleDateFilter.HasDateEntered;
			jobCollection = GetCollection(Filters);
			AssertCollection(jobCollection, new Job[] { job1, job2 });

			((ModuleDateFilter)Filters["ShipmentAPD"]).IsActive = true;
			((ModuleDateFilter)Filters["ShipmentAPD"]).PropertySearch = ModuleDateFilter.HasNoDateEntered;
			jobCollection = GetCollection(Filters);
			AssertCollection(jobCollection, Array.Empty<Job>());
		}

		public void TestCustomsClearanceDateFilter()
		{
			ForwardingShipment shipment1 = null, shipment2 = null;
			Job job1 = CreateJobLinkedToShipment(ref shipment1);
			Job job2 = CreateJobLinkedToShipment(ref shipment2);
			CreateNewChargeForJob(job1);
			CreateNewChargeForJob(job2);

			StmALog log1 = shipment1.Logs.AddNew();
			using (log1.LockForUpdatingKeyFieldsForTesting())
			{
				log1.SL_SE_NKEvent = Events.CustomsCleared.Code;
				log1.SL_EventTime = new ZDateTime(2014, 10, 10);
			}

			StmALog log2 = shipment2.Logs.AddNew();
			using (log2.LockForUpdatingKeyFieldsForTesting())
			{
				log2.SL_SE_NKEvent = Events.CustomsCleared.Code;
				log2.SL_EventTime = new ZDateTime(2014, 10, 20);
			}

			Factory.Save();

			((ModuleDateFilter)Filters["CustomsClearanceDate"]).Property1 = new ZDateTime(2014, 10, 09);
			((ModuleDateFilter)Filters["CustomsClearanceDate"]).Property2 = new ZDateTime(2014, 10, 15);
			((ModuleDateFilter)Filters["CustomsClearanceDate"]).IsActive = true;
			((ModuleDateFilter)Filters["CustomsClearanceDate"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			JobCollection jobCollection = GetCollection(Filters);
			AssertCollection(jobCollection, new Job[] { job1 });

			((ModuleDateFilter)Filters["CustomsClearanceDate"]).Property1 = new ZDateTime(2014, 10, 19);
			((ModuleDateFilter)Filters["CustomsClearanceDate"]).Property2 = new ZDateTime(2014, 10, 22);
			((ModuleDateFilter)Filters["CustomsClearanceDate"]).IsActive = true;
			((ModuleDateFilter)Filters["CustomsClearanceDate"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			jobCollection = GetCollection(Filters);
			AssertCollection(jobCollection, new Job[] { job2 });

			((ModuleDateFilter)Filters["CustomsClearanceDate"]).IsActive = true;
			((ModuleDateFilter)Filters["CustomsClearanceDate"]).PropertySearch = ModuleDateFilter.HasDateEntered;
			jobCollection = GetCollection(Filters);
			AssertCollection(jobCollection, new Job[] { job1, job2 });
		}

		public void TestAWBIssueDateFilter()
		{
			ForwardingConsol consol1 = null, consol2 = null;
			Job job1 = CreateJobLinkedToConsol(ref consol1);
			Job job2 = CreateJobLinkedToConsol(ref consol2);
			CreateNewChargeForJob(job1);
			CreateNewChargeForJob(job2);

			consol1.JK_MasterBillIssueDate = new ZDateTime(2014, 10, 10);
			consol2.JK_MasterBillIssueDate = new ZDateTime(2014, 10, 20);

			Factory.Save();

			((ModuleDateFilter)Filters["AWBIssueDate"]).Property1 = new ZDateTime(2014, 10, 09);
			((ModuleDateFilter)Filters["AWBIssueDate"]).Property2 = new ZDateTime(2014, 10, 15);
			((ModuleDateFilter)Filters["AWBIssueDate"]).IsActive = true;
			((ModuleDateFilter)Filters["AWBIssueDate"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			JobCollection jobCollection = GetCollection(Filters);
			AssertCollection(jobCollection, new Job[] { job1 });

			((ModuleDateFilter)Filters["AWBIssueDate"]).Property1 = new ZDateTime(2014, 10, 19);
			((ModuleDateFilter)Filters["AWBIssueDate"]).Property2 = new ZDateTime(2014, 10, 22);
			((ModuleDateFilter)Filters["AWBIssueDate"]).IsActive = true;
			((ModuleDateFilter)Filters["AWBIssueDate"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			jobCollection = GetCollection(Filters);
			AssertCollection(jobCollection, new Job[] { job2 });

			((ModuleDateFilter)Filters["AWBIssueDate"]).IsActive = true;
			((ModuleDateFilter)Filters["AWBIssueDate"]).PropertySearch = ModuleDateFilter.HasDateEntered;
			jobCollection = GetCollection(Filters);
			AssertCollection(jobCollection, new Job[] { job1, job2 });

			((ModuleDateFilter)Filters["AWBIssueDate"]).IsActive = true;
			((ModuleDateFilter)Filters["AWBIssueDate"]).PropertySearch = ModuleDateFilter.HasNoDateEntered;
			jobCollection = GetCollection(Filters);
			AssertCollection(jobCollection, Array.Empty<Job>());
		}

		public void TestHAWBIssueDateFilterWhenHAWBExists()
		{
			ForwardingShipment shipment1 = null, shipment2 = null, shipment3 = null, shipment4 = null;
			Job job1 = CreateJobLinkedToShipment(ref shipment1);
			Job job2 = CreateJobLinkedToShipment(ref shipment2);
			Job job3 = CreateJobLinkedToShipment(ref shipment3);
			Job job4 = CreateJobLinkedToShipment(ref shipment4);
			CreateNewChargeForJob(job1);
			CreateNewChargeForJob(job2);
			CreateNewChargeForJob(job3);
			CreateNewChargeForJob(job4);

			var awbHeaderType = ObjectFactory.GetType<Freight.Integration.AWB.IExportAWBHeader>();
			var exportHeader = Factory.NewWithValidTestData(awbHeaderType);
			exportHeader[ExportAWBHeaderSchema.EH_Table.Name] = "JobShipment";
			exportHeader[ExportAWBHeaderSchema.EH_ParentID.Name] = shipment1.PK;
			exportHeader[ExportAWBHeaderSchema.EH_AWBIssueDate.Name] = new ZDateTime(2014, 10, 10);

			exportHeader = Factory.NewWithValidTestData(awbHeaderType);
			exportHeader[ExportAWBHeaderSchema.EH_Table.Name] = "JobShipment";
			exportHeader[ExportAWBHeaderSchema.EH_ParentID.Name] = shipment2.PK;
			exportHeader[ExportAWBHeaderSchema.EH_AWBIssueDate.Name] = new ZDateTime(2014, 10, 20);

			shipment4.JS_HouseBillIssueDate = new ZDateTime(2014, 10, 25);
			exportHeader = Factory.NewWithValidTestData(awbHeaderType);
			exportHeader[ExportAWBHeaderSchema.EH_Table.Name] = "JobShipment";
			exportHeader[ExportAWBHeaderSchema.EH_ParentID.Name] = shipment4.PK;
			exportHeader[ExportAWBHeaderSchema.EH_AWBIssueDate.Name] = new ZDateTime(2014, 10, 26);

			Factory.Save();

			((ModuleDateFilter)Filters["HAWBIssueDate"]).Property1 = new ZDateTime(2014, 10, 09);
			((ModuleDateFilter)Filters["HAWBIssueDate"]).Property2 = new ZDateTime(2014, 10, 15);
			((ModuleDateFilter)Filters["HAWBIssueDate"]).IsActive = true;
			((ModuleDateFilter)Filters["HAWBIssueDate"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			JobCollection jobCollection = GetCollection(Filters);
			AssertCollection(jobCollection, new Job[] { job1 });

			((ModuleDateFilter)Filters["HAWBIssueDate"]).Property1 = new ZDateTime(2014, 10, 19);
			((ModuleDateFilter)Filters["HAWBIssueDate"]).Property2 = new ZDateTime(2014, 10, 22);
			((ModuleDateFilter)Filters["HAWBIssueDate"]).IsActive = true;
			((ModuleDateFilter)Filters["HAWBIssueDate"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			jobCollection = GetCollection(Filters);
			AssertCollection(jobCollection, new Job[] { job2 });

			((ModuleDateFilter)Filters["HAWBIssueDate"]).IsActive = true;
			((ModuleDateFilter)Filters["HAWBIssueDate"]).PropertySearch = ModuleDateFilter.HasDateEntered;
			jobCollection = GetCollection(Filters);
			AssertCollection(jobCollection, new Job[] { job1, job2, job4 });

			((ModuleDateFilter)Filters["HAWBIssueDate"]).IsActive = true;
			((ModuleDateFilter)Filters["HAWBIssueDate"]).PropertySearch = ModuleDateFilter.HasNoDateEntered;
			jobCollection = GetCollection(Filters);
			AssertCollection(jobCollection, new Job[] { job3 });
		}

		public void TestHAWBIssueDateFilterWhenNoHAWBExists()
		{
			ForwardingShipment shipment1 = null, shipment2 = null, shipment3 = null, shipment4 = null;
			Job job1 = CreateJobLinkedToShipment(ref shipment1);
			Job job2 = CreateJobLinkedToShipment(ref shipment2);
			Job job3 = CreateJobLinkedToShipment(ref shipment3);
			Job job4 = CreateJobLinkedToShipment(ref shipment4);
			CreateNewChargeForJob(job1);
			CreateNewChargeForJob(job2);
			CreateNewChargeForJob(job3);
			CreateNewChargeForJob(job4);

			shipment1.JS_HouseBillIssueDate = new ZDateTime(2014, 10, 10);
			shipment2.JS_HouseBillIssueDate = new ZDateTime(2014, 10, 20);
			shipment3.JS_HouseBillIssueDate = ZDateTime.Empty;
			shipment4.JS_HouseBillIssueDate = new ZDateTime(2014, 10, 25);

			Factory.Save();

			((ModuleDateFilter)Filters["HAWBIssueDate"]).Property1 = new ZDateTime(2014, 10, 09);
			((ModuleDateFilter)Filters["HAWBIssueDate"]).Property2 = new ZDateTime(2014, 10, 15);
			((ModuleDateFilter)Filters["HAWBIssueDate"]).IsActive = true;
			((ModuleDateFilter)Filters["HAWBIssueDate"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			JobCollection jobCollection = GetCollection(Filters);
			AssertCollection(jobCollection, new Job[] { job1 });

			((ModuleDateFilter)Filters["HAWBIssueDate"]).Property1 = new ZDateTime(2014, 10, 19);
			((ModuleDateFilter)Filters["HAWBIssueDate"]).Property2 = new ZDateTime(2014, 10, 22);
			((ModuleDateFilter)Filters["HAWBIssueDate"]).IsActive = true;
			((ModuleDateFilter)Filters["HAWBIssueDate"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			jobCollection = GetCollection(Filters);
			AssertCollection(jobCollection, new Job[] { job2 });

			((ModuleDateFilter)Filters["HAWBIssueDate"]).IsActive = true;
			((ModuleDateFilter)Filters["HAWBIssueDate"]).PropertySearch = ModuleDateFilter.HasDateEntered;
			jobCollection = GetCollection(Filters);
			AssertCollection(jobCollection, new Job[] { job1, job2, job4 });

			((ModuleDateFilter)Filters["HAWBIssueDate"]).IsActive = true;
			((ModuleDateFilter)Filters["HAWBIssueDate"]).PropertySearch = ModuleDateFilter.HasNoDateEntered;
			jobCollection = GetCollection(Filters);
			AssertCollection(jobCollection, new Job[] { job3 });
		}

		public void TestCostTaxBranchFilter()
		{
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertEquals("This is mandatory filter", FilterVisibility.AlwaysVisible, Filters["TaxBranch"].Visibility);
			AssertEquals("This is mandatory filter which can not be overriden", false, Filters["TaxBranch"].ReadOnly);
			AssertEquals("Default of TaxBranch filter", Invoice.AH_GB_TaxBranch, ((ModuleGuidFilter)Filters["TaxBranch"]).Property);

			var branch = TestObjectCreator.CreateBranch("GB1", GlbCompany.CurrentCompany);
			var charge = CreateNewCharge();
			var job = CreateNewJobForCharge(charge);

			Factory.Save();

			((ModuleGuidFilter)Filters["TaxBranch"]).Property = branch.PK;
			AssertCollection(GetCollection(Filters));

			charge.JR_GB_CostTaxBranch = branch.PK;
			Factory.Save();

			AssertCollection(GetCollection(Filters), job);
		}

		public void TestJobTaxBranchFilter()
		{
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var branch = TestObjectCreator.CreateBranch("GB1", GlbCompany.CurrentCompany);
			var charge = CreateNewCharge();
			var job = CreateNewJobForCharge(charge);
			charge.JR_GB_CostTaxBranch = GlbBranch.CurrentBranch.PK;

			Factory.Save();

			((ModuleGuidFilter)Filters["JobTaxBranch"]).Property = branch.PK;
			((ModuleGuidFilter)Filters["JobTaxBranch"]).IsActive = true;
			AssertCollection(GetCollection(Filters));

			job.JH_GB_TaxBranch = branch.PK;
			Factory.Save();

			AssertCollection(GetCollection(Filters), job);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new InvoicingBaseBulkChargeImporterFilters(Factory.New<APInvoice>());
		}

		protected override InvoiceBulkOperationFilters GetNewFilters()
		{
			return new InvoicingBaseBulkChargeImporterFilters(Invoice);
		}

		void AssertCollection(BusinessObjectCollection collection, params BusinessObject[] items)
		{
			AssertCollection(collection.ToArray(), items);
		}

		void AssertCollection(BusinessObject[] collection, params BusinessObject[] items)
		{
			AssertEquals("Count of items in collection", items.Length, collection.Length);
			foreach (BusinessObject bizo in items)
			{
				AssertCollectionContains("Should have contained this item", bizo, collection);
			}
		}

		JobCollection GetCollection(InvoicingBaseBulkChargeImporterFilters filters)
		{
			JobCollection result = new JobCollection(Factory);
			result.Load(filters.GetQuery());
			return result;
		}

		Charge[] GetChargeCollection(InvoicingBaseBulkChargeImporterFilters filters, JobCollection jobs)
		{
			var chargesFilter = new ZQuery(JobChargeSchema.JR_JH, jobs.Select(x => x.PK).ToArray());
			chargesFilter.AddToFilter(filters.GetChildQuery());
			return Factory.Load<Charge>(chargesFilter);
		}

		protected new InvoicingBaseBulkChargeImporterFilters Filters
		{
			get { return (InvoicingBaseBulkChargeImporterFilters)base.Filters; }
		}

		APInvoice Invoice
		{
			get
			{
				if (invoice == null)
				{
					invoice = Factory.NewWithValidTestData<APInvoice>();
					invoice.AH_OH = TestObjectCreator.AALSHI.PK;
					invoice.AH_GB_TaxBranch = GlbBranch.CurrentBranch.PK;
				}
				return invoice;
			}
		}
		APInvoice invoice;

		Job CreateNewJobForCharge(Charge charge)
		{
			Job result = Factory.NewJobWithValidTestDataForTesting<Job>();
			charge.JR_JH = result.PK;
			return result;
		}

		Charge CreateNewCharge()
		{
			Charge result = Factory.NewWithValidTestData<Charge>();
			result.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			result.JR_OSCostAmt = 1.1m;
			return result;
		}

		Charge CreateNewChargeForJob(Job job)
		{
			Charge result = CreateNewCharge();
			result.JR_JH = job.PK;
			return result;
		}

		Job CreateJobLinkedToConsol(ref ForwardingConsol consol)
		{
			ForwardingShipment shipment = null;
			Job job = CreateJobLinkedToShipment(ref shipment);
			consol = Factory.New<ForwardingConsol>();
			JobConShipLink jobConShipLink = Factory.New<JobConShipLink>();
			jobConShipLink.JN_JS = shipment.PK;
			jobConShipLink.JN_JK = consol.PK;

			return job;
		}

		Job CreateJobLinkedToShipment(ref ForwardingShipment shipment)
		{
			shipment = Factory.New<ForwardingShipment>();
			Job jobHeader = Factory.NewJobWithValidTestDataForTesting<Job>();
			jobHeader.JH_ParentID = shipment.PK;

			return jobHeader;
		}

		Job CreateJobLinkedToDeclaration(ref BaseJobDeclaration jobDeclaration)
		{
			jobDeclaration = Factory.New<BaseJobDeclaration>();
			Job jobHeader = Factory.NewJobWithValidTestDataForTesting<Job>();
			jobHeader.JH_ParentID = jobDeclaration.PK;

			return jobHeader;
		}

		Charge CreateChargesAndMileStones(DummyWithWorkflow bizO, JobHeader job, ZString status, ZDateTime actualDate, ZDateTimeOffset scheduledDate)
		{
			var charge = CreateNewCharge();
			charge.JR_JH = job.PK;

			var mileStone = bizO.WorkflowItems.Milestones.AddNew();
			mileStone.P9_Type = "MIL";
			mileStone.P9_Status = status;
			mileStone.P9_ParentID = job.JH_ParentID;
			mileStone.P9_ParentTableCode = job.JH_ParentTableCode;
			mileStone.SetMilestoneActualDateForTest(actualDate);
			mileStone.SetMilestoneScheduledDateForTest(scheduledDate);
			Factory.Save();

			return charge;
		}

		Charge CreateChargesAndTasks(DummyWithWorkflow bizO, JobHeader job, ZString status, ZString staffCode, ZInt sequence)
		{
			var charge = CreateNewCharge();
			charge.JR_JH = job.PK;

			var task = bizO.WorkflowItems.Tasks.AddNew();
			task.P9_Status = status;
			task.P9_ParentID = job.PK;
			task.P9_GS_NKAssignedStaffMember = staffCode;
			task.P9_Sequence = sequence;
			Factory.Save();

			return charge;
		}

		protected override ModuleFilter SetupFilterForTest(ModuleFilter moduleFilter)
		{
			return moduleFilter.Description != InvoiceBulkOperationFilterHelper.HouseBillFilterName ? base.SetupFilterForTest(moduleFilter) : null;
		}

		#endregion
	}
}
