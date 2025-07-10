using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APBulkInvoicePosterFilters))]
	public class APBulkInvoicePosterFiltersTest : InvoiceBulkOperationFiltersTest
	{
		AccTransactionLines CreateLinesAndMileStones(DummyWithWorkflow bizO, JobHeader job, ZString status, ZDateTime actualDate, ZDateTimeOffset scheduledDate)
		{
			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_JH = job.PK;
			line.AL_LineType = TransactionLineTypes.Accrual;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;

			var mileStone = bizO.WorkflowItems.Milestones.AddNew();
			mileStone.P9_Type = "MIL";
			mileStone.P9_Status = status;
			mileStone.SetMilestoneActualDateForTest(actualDate);
			mileStone.SetMilestoneScheduledDateForTest(scheduledDate);
			Factory.Save();

			return line;
		}

		Tuple<DummyWithWorkflow, Job> CreateJob()
		{
			var job = TestObjectCreator.CreateJobHeader();
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			job.JH_ParentID = dummy.PK;

			return Tuple.Create(dummy, job);
		}

		public void TestMileStoneFiltering()
		{
			var job1 = CreateJob();
			var job2 = CreateJob();
			var job3 = CreateJob();
			var job4 = CreateJob();

			var line1 = CreateLinesAndMileStones(job1.Item1, job1.Item2, "LST", new ZDateTime(2014, 3, 11), ZDateTimeOffset.Empty);
			var line2 = CreateLinesAndMileStones(job2.Item1, job2.Item2, "LST", ZDateTime.Empty, ZDateTimeOffset.Empty);
			var line3 = CreateLinesAndMileStones(job3.Item1, job3.Item2, "NXT", ZDateTime.Empty, new ZDateTimeOffset(new ZDateTime(2014, 5, 18)));
			var line4 = CreateLinesAndMileStones(job4.Item1, job4.Item2, "LST", new ZDateTime(2014, 8, 6), ZDateTimeOffset.Empty);

			APBulkInvoicePoster poster = new APBulkInvoicePoster(Factory);
			poster.LoadAccrualCollection();
			AssertEquals("Should only be four items in collection", 4, poster.Accruals.Count);
			Assert(poster.Accruals.Any(x => x.PK == line1.PK));
			Assert(poster.Accruals.Any(x => x.PK == line2.PK));
			Assert(poster.Accruals.Any(x => x.PK == line3.PK));
			Assert(poster.Accruals.Any(x => x.PK == line4.PK));

			poster = new APBulkInvoicePoster(Factory);
			var milestoneDatefilter = (ModuleDateFilter)poster.Filters["Milestone Date (Related)"];
			milestoneDatefilter.IsActive = true;
			milestoneDatefilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			milestoneDatefilter.Property1 = new ZDateTime(2014, 3, 10);
			milestoneDatefilter.Property2 = new ZDateTime(2014, 3, 13);
			poster.LoadAccrualCollection();
			AssertEquals("Should only be one item in collection", 1, poster.Accruals.Count);
			AssertEquals(line1.PK, poster.Accruals[0].PK);

			poster = new APBulkInvoicePoster(Factory);
			var milestoneCompletedFilter = (ModuleTextFilter)poster.Filters["Milestone Completed (Related)"];
			milestoneCompletedFilter.IsActive = true;
			milestoneCompletedFilter.Property = "Not Completed";
			poster.LoadAccrualCollection();
			AssertEquals("Should only be two item in collection", 2, poster.Accruals.Count);
			Assert(poster.Accruals.Any(x => x.PK == line2.PK));
			Assert(poster.Accruals.Any(x => x.PK == line3.PK));

			poster = new APBulkInvoicePoster(Factory);
			var nextMilestoneFilter = (ModuleDateFilter)poster.Filters["Next Milestone (Related)"];
			nextMilestoneFilter.IsActive = true;
			nextMilestoneFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			nextMilestoneFilter.Property1 = new ZDateTime(2014, 5, 17);
			nextMilestoneFilter.Property2 = new ZDateTime(2014, 5, 19);
			poster.LoadAccrualCollection();
			AssertEquals("Should only be one item in collection", 1, poster.Accruals.Count);
			AssertEquals("Should show correct item", line3.PK, poster.Accruals[0].PK);

			poster = new APBulkInvoicePoster(Factory);
			var lastCompletedMilestoneFilter = (ModuleDateFilter)poster.Filters["Last Completed Milestone (Related)"];
			lastCompletedMilestoneFilter.IsActive = true;
			lastCompletedMilestoneFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			lastCompletedMilestoneFilter.Property1 = new ZDateTime(2014, 8, 5);
			lastCompletedMilestoneFilter.Property2 = new ZDateTime(2014, 8, 7);
			poster.LoadAccrualCollection();
			AssertEquals("Should only be one item in collection", 1, poster.Accruals.Count);
			AssertEquals("Should show correct item", line4.PK, poster.Accruals[0].PK);
		}

		public void TestDefaultFiltering()
		{
			AccTransactionLines line1 = Factory.New<AccTransactionLines>();
			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			line1.AL_GB = Env.CurrentBranch.PK;
			line1.AL_GE = Env.CurrentDepartment.PK;
			line1.AL_ReverseDate = ZDateTime.Now;
			line1.AL_LineType = TransactionLineTypes.Accrual;

			AccTransactionLines line2 = Factory.New<AccTransactionLines>();
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			line2.AL_GB = Env.CurrentBranch.PK;
			line2.AL_GE = Env.CurrentDepartment.PK;
			line2.AL_ReverseDate = ZDateTime.Empty;
			line2.AL_LineType = TransactionLineTypes.Accrual;

			AccTransactionLines line3 = Factory.New<AccTransactionLines>();
			line3.AL_AG = TestObjectCreator.GLHeader1.PK;
			line3.AL_GB = Env.CurrentBranch.PK;
			line3.AL_GE = Env.CurrentDepartment.PK;
			line3.AL_ReverseDate = ZDateTime.Empty;
			line3.AL_LineType = TransactionLineTypes.WIP;

			Factory.Save();

			APBulkInvoicePoster poster = new APBulkInvoicePoster(Factory);
			poster.LoadAccrualCollection();

			AssertEquals("Should only be one item in collection", 1, poster.Accruals.Count);
			AssertEquals("Should show correct item", line2.PK, poster.Accruals[0].PK);
		}

		public void TestCreditorFilter()
		{
			TestObjectCreator.AALSHI.OH_IsCreditor = true;
			TestObjectCreator.ABIGAS.OH_IsCreditor = true;

			var job = TestObjectCreator.CreateJobHeader();
			Accrual accrual1 = TestObjectCreator.CreateAccrual(job, x => x.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK);
			Accrual accrual2 = TestObjectCreator.CreateAccrual(job, x => x.JR_OH_CostAccount = TestObjectCreator.ABIGAS.PK);
			TestObjectCreator.Factory.Save();

			APBulkInvoicePoster poster = new APBulkInvoicePoster(Factory);
			((ModuleGuidFilter)poster.Filters["Creditor"]).Property = TestObjectCreator.ABIGAS.PK;
			((ModuleGuidFilter)poster.Filters["Creditor"]).IsActive = true;
			poster.LoadAccrualCollection();
			AssertEquals("Should only be one item in collection", 1, poster.Accruals.Count);
			AssertEquals("Should show correct item", accrual2.PK, poster.Accruals[0].PK);
		}

		public void TestChargeCodeFilter()
		{
			var job = TestObjectCreator.CreateJobHeader();
			Accrual accrual1 = TestObjectCreator.CreateAccrual(job, x => x.JR_AC = TestObjectCreator.CC1.PK);
			Accrual accrual2 = TestObjectCreator.CreateAccrual(job, x => x.JR_AC = TestObjectCreator.CC2.PK);
			TestObjectCreator.Factory.Save();

			APBulkInvoicePoster poster = new APBulkInvoicePoster(Factory);
			((ModuleGuidFilter)poster.Filters["ChargeCode"]).Property = TestObjectCreator.CC2.PK;
			((ModuleGuidFilter)poster.Filters["ChargeCode"]).IsActive = true;
			poster.LoadAccrualCollection();
			AssertEquals("Should only be one item in collection", 1, poster.Accruals.Count);
			AssertEquals("Should show correct item", accrual2.PK, poster.Accruals[0].PK);
		}

		public void TestCurrencyFilter()
		{
			var job = TestObjectCreator.CreateJobHeader();
			Accrual accrual1 = TestObjectCreator.CreateAccrual(job, x => x.JR_RX_NKCostCurrency = TestObjectCreator.AUD.RX_Code);
			Accrual accrual2 = TestObjectCreator.CreateAccrual(job, x => x.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code);

			TestObjectCreator.Factory.Save();

			APBulkInvoicePoster poster = new APBulkInvoicePoster(Factory);
			((ModuleNkFilter)poster.Filters["Currency"]).Property = TestObjectCreator.USD.RX_Code;
			((ModuleNkFilter)poster.Filters["Currency"]).IsActive = true;
			poster.LoadAccrualCollection();
			AssertEquals("Should only be one item in collection", 1, poster.Accruals.Count);
			AssertEquals("Should show correct item", accrual2.PK, poster.Accruals[0].PK);
		}

		public void TestDepartmentFilter()
		{
			var job = TestObjectCreator.CreateJobHeader();

			var department = TestObjectCreator.FISDepartment;
			var department2 = TestObjectCreator.FESDepartment;

			Accrual accrual1 = TestObjectCreator.CreateAccrual(job, x => x.JR_GE = department.PK);
			Accrual accrual2 = TestObjectCreator.CreateAccrual(job, x => x.JR_GE = department2.PK);

			TestObjectCreator.Factory.Save();

			APBulkInvoicePoster poster = new APBulkInvoicePoster(Factory);
			((ModuleGuidFilter)poster.Filters["Department"]).Property = department.PK;
			((ModuleGuidFilter)poster.Filters["Department"]).IsActive = true;
			poster.LoadAccrualCollection();
			AssertEquals("Should only be one item in collection", 1, poster.Accruals.Count);
			AssertEquals("Should show correct item", accrual1.PK, poster.Accruals[0].PK);
		}

		public void TestDateFilter()
		{
			ForwardingShipment shipment1, shipment2, shipment3;
			Accrual accrual1 = CreateAccrualLinkedToShipment(out shipment1);
			Accrual accrual2 = CreateAccrualLinkedToShipment(out shipment2);
			Accrual accrual3 = CreateAccrualLinkedToShipment(out shipment3);
			accrual1.AL_PostDate = ZDateTime.Now;
			accrual2.AL_PostDate = ZDateTime.BrettsBirthday;
			accrual3.AL_PostDate = ZDateTime.BrettsBirthday.AddYears(1);
			TestObjectCreator.Factory.Save();

			APBulkInvoicePoster poster = new APBulkInvoicePoster(Factory);

			((ModuleDateFilter)poster.Filters["PostDate"]).Property1 = ZDateTime.BrettsBirthday.AddDays(-200);
			((ModuleDateFilter)poster.Filters["PostDate"]).Property2 = ZDateTime.BrettsBirthday.AddDays(400);
			((ModuleDateFilter)poster.Filters["PostDate"]).IsActive = true;
			((ModuleDateFilter)poster.Filters["PostDate"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			poster.LoadAccrualCollection();
			AssertEquals("Should only be two items in collection (filtered by PSD)", 2, poster.Accruals.Count);
			Assert("Should contain correct item", poster.Accruals.Contains(accrual2.PK));
			Assert("Should contain correct item", poster.Accruals.Contains(accrual3.PK));

			((ModuleDateFilter)poster.Filters["PostDate"]).IsActive = false;

			shipment1.JS_E_DEP = ZDateTime.Now;
			shipment2.JS_E_DEP = ZDateTime.BrettsBirthday;
			shipment3.JS_E_ARV = ZDateTime.BrettsBirthday;
			TestObjectCreator.Factory.Save();

			((ModuleDateFilter)poster.Filters["ShipmentETA_ETD"]).Property1 = ZDateTime.BrettsBirthday.AddDays(-1);
			((ModuleDateFilter)poster.Filters["ShipmentETA_ETD"]).Property2 = ZDateTime.BrettsBirthday.AddDays(1);
			((ModuleDateFilter)poster.Filters["ShipmentETA_ETD"]).IsActive = true;
			((ModuleDateFilter)poster.Filters["ShipmentETA_ETD"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			poster.LoadAccrualCollection();
			AssertEquals("Should only be two items in collection (filtered by SHD)", 2, poster.Accruals.Count);
			Assert("Should contain correct item", poster.Accruals.Contains(accrual2.PK));
			Assert("Should contain correct item", poster.Accruals.Contains(accrual3.PK));
		}

		public void TestAccountingDateFilter()
		{
			AssertEquals(0, Filters.ModuleFilters.Count(x => x.Description == "Accounting Date"));

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Canada))
			{
				var posterCA = new APBulkInvoicePoster(Factory);
				var accDatefilter = (ModuleDateFilter)posterCA.Filters["Accounting Date"];
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

				var accrual1 = TestObjectCreator.CreateAccrual(job1, x => x.JR_AC = TestObjectCreator.CC1.PK);
				var accrual2 = TestObjectCreator.CreateAccrual(job2, x => x.JR_AC = TestObjectCreator.CC2.PK);

				Factory.Save();

				posterCA.LoadAccrualCollection();
				AssertCollection(posterCA.Accruals, accrual1, accrual2);

				accDatefilter.Property1 = new ZDateTime(2011, 1, 20);
				accDatefilter.Property2 = new ZDateTime(2011, 2, 2);
				accDatefilter.IsActive = true;
				accDatefilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				posterCA.LoadAccrualCollection();
				AssertCollection(posterCA.Accruals, accrual1);
			}
		}

		public void TestShipmentETA_ETDFilter()
		{
			ForwardingShipment shipment;
			Accrual accrual = CreateAccrualLinkedToShipment(out shipment);
			shipment.JS_E_ARV = new ZDateTime(2012, 11, 19);
			shipment.JS_E_DEP = new ZDateTime(2012, 08, 24);
			TestObjectCreator.Factory.Save();

			APBulkInvoicePoster poster = new APBulkInvoicePoster(Factory);

			((ModuleDateFilter)poster.Filters["ShipmentETA_ETD"]).Property1 = new ZDateTime(2012, 11, 01);
			((ModuleDateFilter)poster.Filters["ShipmentETA_ETD"]).Property2 = new ZDateTime(2012, 11, 30);
			((ModuleDateFilter)poster.Filters["ShipmentETA_ETD"]).IsActive = true;
			((ModuleDateFilter)poster.Filters["ShipmentETA_ETD"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			poster.LoadAccrualCollection();
			AssertEquals(1, poster.Accruals.Count);
			Assert("Should contain correct item", poster.Accruals.Contains(accrual.PK));

			((ModuleDateFilter)poster.Filters["ShipmentETA_ETD"]).Property1 = new ZDateTime(2012, 08, 01);
			((ModuleDateFilter)poster.Filters["ShipmentETA_ETD"]).Property2 = new ZDateTime(2012, 08, 31);
			((ModuleDateFilter)poster.Filters["ShipmentETA_ETD"]).IsActive = true;
			((ModuleDateFilter)poster.Filters["ShipmentETA_ETD"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			poster.LoadAccrualCollection();
			AssertEquals(1, poster.Accruals.Count);
			Assert("Should contain correct item", poster.Accruals.Contains(accrual.PK));

			((ModuleDateFilter)poster.Filters["ShipmentETA_ETD"]).Property1 = new ZDateTime(2012, 08, 01);
			((ModuleDateFilter)poster.Filters["ShipmentETA_ETD"]).Property2 = new ZDateTime(2012, 11, 30);
			((ModuleDateFilter)poster.Filters["ShipmentETA_ETD"]).IsActive = true;
			((ModuleDateFilter)poster.Filters["ShipmentETA_ETD"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			poster.LoadAccrualCollection();
			AssertEquals(1, poster.Accruals.Count);
			Assert("Should contain correct item", poster.Accruals.Contains(accrual.PK));

			((ModuleDateFilter)poster.Filters["ShipmentETA_ETD"]).Property1 = new ZDateTime(2012, 10, 29);
			((ModuleDateFilter)poster.Filters["ShipmentETA_ETD"]).Property2 = new ZDateTime(2012, 10, 31);
			((ModuleDateFilter)poster.Filters["ShipmentETA_ETD"]).IsActive = true;
			((ModuleDateFilter)poster.Filters["ShipmentETA_ETD"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			poster.LoadAccrualCollection();
			AssertEquals(0, poster.Accruals.Count);
		}

		public void TestTransportModeFilter()
		{
			ForwardingConsol consol1, consol2;
			Accrual accrual1 = CreateAccrualLinkedToConsol(out consol1);
			Accrual accrual2 = CreateAccrualLinkedToConsol(out consol2);
			consol1.JK_TransportMode = "SEA";
			consol2.JK_TransportMode = "AIR";
			TestObjectCreator.Factory.Save();

			APBulkInvoicePoster poster = new APBulkInvoicePoster(Factory);
			((ModuleTextFilter)poster.Filters["TransportMode"]).Property = "AIR";
			((ModuleTextFilter)poster.Filters["TransportMode"]).IsActive = true;
			poster.LoadAccrualCollection();
			AssertEquals("Should only be one item in collection", 1, poster.Accruals.Count);
			AssertEquals("Should show correct item", accrual2.PK, poster.Accruals[0].PK);
		}

		public void TestContainerModeFilter()
		{
			ForwardingConsol consol1, consol2;
			Accrual accrual1 = CreateAccrualLinkedToConsol(out consol1);
			Accrual accrual2 = CreateAccrualLinkedToConsol(out consol2);
			consol1.JK_ConsolMode = "LCL";
			consol2.JK_ConsolMode = "FCL";
			TestObjectCreator.Factory.Save();

			APBulkInvoicePoster poster = new APBulkInvoicePoster(Factory);
			((ModuleTextFilter)poster.Filters["ContainerMode"]).Property = "FCL";
			((ModuleTextFilter)poster.Filters["ContainerMode"]).IsActive = true;
			poster.LoadAccrualCollection();

			AssertEquals("Should only be one item in collection", 1, poster.Accruals.Count);
			AssertEquals("Should show correct item", accrual2.PK, poster.Accruals[0].PK);
		}
		public void TestContainerNumberFilter()
		{
			ForwardingConsol consol1, consol2;
			Accrual accrual1 = CreateAccrualLinkedToConsol(out consol1);
			Accrual accrual2 = CreateAccrualLinkedToConsol(out consol2);
			consol1.JK_ConsolMode = "LCL";
			consol2.JK_ConsolMode = "FCL";
			TestObjectCreator.Factory.Save();
			APBulkInvoicePoster poster = new APBulkInvoicePoster(Factory);
			((ModuleTextFilter)poster.Filters["ContainerMode"]).Property = "FCL";
			((ModuleTextFilter)poster.Filters["ContainerMode"]).IsActive = true;
			poster.LoadAccrualCollection();

			ForwardingContainer fc1 = Factory.NewWithValidTestData<ForwardingContainer>();
			ForwardingContainer fc2 = Factory.NewWithValidTestData<ForwardingContainer>();
			fc1.JC_ContainerNum = "123";
			fc2.JC_ContainerNum = "456";
			fc1.JC_JK = consol1.PK;
			fc2.JC_JK = consol2.PK;

			TestObjectCreator.Factory.Save();
			Factory.Save();

			AssertEquals("container1 is in database", true, fc1.IsInDatabase);
			AssertEquals("container2 is in database", true, fc2.IsInDatabase);

			((ModuleTextFilter)poster.Filters["Container Number"]).Property = "123";
			((ModuleTextFilter)poster.Filters["Container Number"]).IsActive = true;
			AssertEquals("Should only be one item in collection", 1, poster.Accruals.Count);
			AssertEquals("Should show correct item", accrual2.PK, poster.Accruals[0].PK);
		}

		public void TestSendingAgentFilter()
		{
			ForwardingConsol consol1, consol2;
			Accrual accrual1 = CreateAccrualLinkedToConsol(out consol1);
			Accrual accrual2 = CreateAccrualLinkedToConsol(out consol2);
			consol2.SetDefaultSendingForwarderAddress(TestObjectCreator.AALSHI);
			consol2.SetDefaultSendingForwarderAddress(TestObjectCreator.ABIGAS);
			TestObjectCreator.Factory.Save();

			APBulkInvoicePoster poster = new APBulkInvoicePoster(Factory);
			((ModuleGuidFilter)poster.Filters["SendingAgent"]).Property = TestObjectCreator.ABIGAS.PK;
			((ModuleGuidFilter)poster.Filters["SendingAgent"]).IsActive = true;
			poster.LoadAccrualCollection();
			AssertEquals("Should only be one item in collection", 1, poster.Accruals.Count);
			AssertEquals("Should show correct item", accrual2.PK, poster.Accruals[0].PK);
		}

		public void TestReceivingAgentFilter()
		{
			ForwardingConsol consol1, consol2;
			Accrual accrual1 = CreateAccrualLinkedToConsol(out consol1);
			Accrual accrual2 = CreateAccrualLinkedToConsol(out consol2);
			consol2.SetDefaultReceivingForwarderAddress(TestObjectCreator.AALSHI);
			consol2.SetDefaultReceivingForwarderAddress(TestObjectCreator.ABIGAS);
			TestObjectCreator.Factory.Save();

			APBulkInvoicePoster poster = new APBulkInvoicePoster(Factory);
			((ModuleGuidFilter)poster.Filters["ReceivingAgent"]).Property = TestObjectCreator.ABIGAS.PK;
			((ModuleGuidFilter)poster.Filters["ReceivingAgent"]).IsActive = true;
			poster.LoadAccrualCollection();
			AssertEquals("Should only be one item in collection", 1, poster.Accruals.Count);
			AssertEquals("Should show correct item", accrual2.PK, poster.Accruals[0].PK);
		}

		public void TestFlightFilter()
		{
			ForwardingConsol consol1 = null, consol2 = null;
			Accrual accrual1 = CreateAccrualLinkedToConsol(out consol1);
			Accrual accrual2 = CreateAccrualLinkedToConsol(out consol2);

			consol1.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol2.JK_TransportMode = Core.Constants.TransportModes.Air;

			Transport transport1 = consol1.Transports[0];
			transport1.JW_VoyageFlight = "snth";
			Transport transport2 = consol2.Transports[0];
			transport2.JW_VoyageFlight = "aoeu";

			TestObjectCreator.Factory.Save();

			APBulkInvoicePoster poster = new APBulkInvoicePoster(Factory);
			FilterStripCollection filterStrips = new FilterStripCollection(poster.Filters.ModuleFilters);
			FilterStrip filterStrip1 = filterStrips.AddNew("Flight");
			((ModuleTextFilter)filterStrip1.CurrentModuleFilter).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			((ModuleTextFilter)filterStrip1.CurrentModuleFilter).Property = "aoeu";
			poster.LoadAccrualCollection();
			AssertEquals(1, poster.Accruals.Count);
			AssertCollectionContains(accrual2, poster.Accruals);
			AssertCollectionNotContains(accrual1, poster.Accruals);

			FilterStrip filterStrip2 = filterStrips.AddNew("Flight");
			((ModuleTextFilter)filterStrip2.CurrentModuleFilter).ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			((ModuleTextFilter)filterStrip2.CurrentModuleFilter).Property = "th";
			poster.LoadAccrualCollection();
			AssertEquals(0, poster.Accruals.Count);

			filterStrip1.OrCategory = FilterOrCategory.Red;
			filterStrip2.OrCategory = FilterOrCategory.Red;
			poster.LoadAccrualCollection();
			AssertEquals(2, poster.Accruals.Count);
			AssertCollectionContains(accrual1, poster.Accruals);
			AssertCollectionContains(accrual2, poster.Accruals);
		}

		public void TestMasterBillFilter()
		{
			ForwardingConsol consol1, consol2;
			ForwardingShipment shipping;
			Accrual accrual1 = CreateAccrualLinkedToConsol(out consol1);
			Accrual accrual2 = CreateAccrualLinkedToConsol(out consol2);
			Accrual accrual3 = CreateAccrualLinkedToShipment(out shipping);
			consol1.JK_MasterBillNum = "12345";
			consol2.JK_MasterBillNum = "67890";
			shipping.JS_HouseBill = "54321";
			shipping.JS_IsShipping = true;
			TestObjectCreator.Factory.Save();

			APBulkInvoicePoster poster = new APBulkInvoicePoster(Factory);
			((ModuleTextFilter)poster.Filters["MasterBill"]).Property = "67890";
			((ModuleTextFilter)poster.Filters["MasterBill"]).IsActive = true;
			poster.LoadAccrualCollection();
			AssertEquals("Should only be one item in collection", 1, poster.Accruals.Count);
			AssertEquals("Should show correct item", accrual2.PK, poster.Accruals[0].PK);

			((ModuleTextFilter)poster.Filters["MasterBill"]).Property = "54321";
			poster.LoadAccrualCollection();
			AssertEquals("Should only be one item in collection", 1, poster.Accruals.Count);
			AssertEquals("Should show correct item", accrual3.PK, poster.Accruals[0].PK);

			((ModuleTextFilter)poster.Filters["MasterBill"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			((ModuleTextFilter)poster.Filters["MasterBill"]).Property = "4";
			poster.LoadAccrualCollection();
			AssertEquals("Should only be one item in collection", 1, poster.Accruals.Count);
			AssertEquals("Should show correct item", accrual2.PK, poster.Accruals[0].PK);
		}

		public override void TestCoLoadMasterBillFilter()
		{
			ForwardingConsol consol1, consol2, consol3;
			ForwardingShipment shipping;
			var accrual1 = CreateAccrualLinkedToConsol(out consol1);
			var accrual2 = CreateAccrualLinkedToConsol(out consol2);
			var accrual3 = CreateAccrualLinkedToConsol(out consol3);
			var accrual4 = CreateAccrualLinkedToShipment(out shipping);
			consol1.JK_AgentType = Constants.AgentType.CoLoad;
			consol1.JK_CoLoadMasterBill = "12345";
			consol1.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol2.JK_AgentType = Constants.AgentType.CoLoad;
			consol2.JK_CoLoadMasterBill = "67890";
			consol3.JK_AgentType = Constants.AgentType.Direct;
			consol3.JK_CoLoadMasterBill = "67890";
			shipping.JS_HouseBill = "67890";
			shipping.JS_IsShipping = true;

			var declaration = TestObjectCreator.CreateDeclaration("B001");
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_ParentID = declaration.PK;
			var accural5 = TestObjectCreator.CreateAccrual(job);

			TestObjectCreator.Factory.Save();

			var poster = new APBulkInvoicePoster(Factory);

			poster.LoadAccrualCollection();
			AssertEquals("Should be all items in collection", 5, poster.Accruals.Count);

			((ModuleTextFilter)poster.Filters["CoLoadMasterBill"]).IsActive = true;

			((ModuleTextFilter)poster.Filters["CoLoadMasterBill"]).Property = "789";
			((ModuleTextFilter)poster.Filters["CoLoadMasterBill"]).ComparisonOperator =
				ModuleTextFilter.ComparisonConstants.Contains;
			poster.LoadAccrualCollection();
			AssertEquals("Should only be one item in collection", 1, poster.Accruals.Count);
			AssertEquals("Should show correct item", accrual2.PK, poster.Accruals[0].PK);

			((ModuleTextFilter)poster.Filters["CoLoadMasterBill"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			((ModuleTextFilter)poster.Filters["CoLoadMasterBill"]).Property = "4";
			poster.LoadAccrualCollection();
			AssertEquals("Should only be one item in collection", 1, poster.Accruals.Count);
			AssertEquals("Should show correct item", accrual2.PK, poster.Accruals[0].PK);
		}

		public void TestAPSettlementGroupFilter()
		{
			TestObjectCreator.AALSHI.OH_IsCreditor = true;
			TestObjectCreator.ABIGAS.OH_IsCreditor = true;
			var job = TestObjectCreator.CreateJobHeader();

			Accrual accrual1 = TestObjectCreator.CreateAccrual(job, x => x.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK);
			Accrual accrual2 = TestObjectCreator.CreateAccrual(job, x => x.JR_OH_CostAccount = TestObjectCreator.ABIGAS.PK);
			TestObjectCreator.ABIGAS.APSettlementGroupPK = TestObjectCreator.Agent.PK;
			TestObjectCreator.Factory.Save();

			APBulkInvoicePoster poster = new APBulkInvoicePoster(Factory);
			((ModuleGuidFilter)poster.Filters["APSettlementGroup"]).Property = TestObjectCreator.Agent.PK;
			((ModuleGuidFilter)poster.Filters["APSettlementGroup"]).IsActive = true;
			poster.LoadAccrualCollection();
			AssertEquals("Should only be one item in collection", 1, poster.Accruals.Count);
			AssertEquals("Should show correct item", accrual2.PK, poster.Accruals[0].PK);
		}

		public void TestSupplierCostReferenceFilter()
		{
			TestObjectCreator.AALSHI.OH_IsCreditor = true;
			TestObjectCreator.ABIGAS.OH_IsCreditor = true;
			var job = TestObjectCreator.CreateJobHeader();

			Accrual accrual1 = TestObjectCreator.CreateAccrual(job, x => x.JR_CostReference = "ABC1");
			Accrual accrual2 = TestObjectCreator.CreateAccrual(job, x => x.JR_CostReference = "ABC2");
			TestObjectCreator.Factory.Save();

			APBulkInvoicePoster poster = new APBulkInvoicePoster(Factory);
			((ModuleNumberFilter)poster.Filters["SupplierCostReference"]).Property = accrual2.RelatedJobCharge.JR_CostReference;
			((ModuleNumberFilter)poster.Filters["SupplierCostReference"]).IsActive = true;
			poster.LoadAccrualCollection();
			AssertEquals("Should only be one item in collection", 1, poster.Accruals.Count);
			AssertEquals("Should show correct item", accrual2.PK, poster.Accruals[0].PK);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new APBulkInvoicePosterFilters(Factory);
		}

		protected override InvoiceBulkOperationFilters GetNewFilters()
		{
			return new APBulkInvoicePosterFilters(Factory);
		}

		Accrual CreateAccrualLinkedToConsol(out ForwardingConsol consol)
		{
			ForwardingShipment shipment;
			Accrual accrual = CreateAccrualLinkedToShipment(out shipment);
			consol = Factory.New<ForwardingConsol>();
			JobConShipLink jobConShipLink = Factory.New<JobConShipLink>();
			jobConShipLink.JN_JS = shipment.PK;
			jobConShipLink.JN_JK = consol.PK;

			return accrual;
		}

		Accrual CreateAccrualLinkedToShipment(out ForwardingShipment shipment)
		{
			shipment = Factory.New<ForwardingShipment>();
			var jobHeader = TestObjectCreator.CreateJobHeader();
			var accrual = TestObjectCreator.CreateAccrual(jobHeader);
			accrual.Job.JH_ParentID = shipment.PK;

			return accrual;
		}

		void AssertCollection(BusinessObjectCollection collection, params BusinessObject[] items)
		{
			var array = collection.ToArray();
			AssertEquals("Count of items in collection", items.Length, array.Length);
			foreach (BusinessObject bizo in items)
			{
				AssertCollectionContains("Should have contained this item", bizo, array);
			}
		}

		#endregion
	}
}
