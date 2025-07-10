using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(PeriodicInvoiceBaseJobFilterBusinessObject))]
	public abstract class PeriodicInvoiceBaseJobsFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestCustomSQLFilterIsDeactivated()
		{
			Assert(!GetNewFilterStripBusinessObject().HasCustomSqlFilter);
		}

		public virtual void TestFiltersSubGroups()
		{
			FilterStripBusinessObject filterBO = GetNewFilterStripBusinessObject();
			ZString[] jobChargeSubGroupFilters = { "Currency", "Debtor", "Invoice Type", "AR Settlement Group", "Organization Branch", "Charge Line Branch", "Charge Line Department", "Charge Code", "Charge Line Sell Reference" };
			ZString[] jobSubGroupFilters = { "Job Status", "Job Open Date", "Job Local Client", "Job Header Branch", "Job Header Department", "Job Header Tax Branch" };
			ZString[] miscSubGroupFilters = { "ETA", "ETD", "ATA", "ATD", "Delivery Date", "Pickup Date", "Customs Clerance Date", "AWB Issue Date", "Completion Date", "Transport Mode" };
			ZString[] operationsSubGroupFilters = { "Carrier", "Principal", "Voyage/Vessel", "Sending Agent", "Receiving Agent" };

			foreach (ModuleFilter filter in filterBO.ModuleFilters)
			{
				if (jobChargeSubGroupFilters.Contains(filter.Description))
				{
					AssertEquals(string.Format("JobChargeFilterSubGroup for filter {0}", filter.Description), "JobChargeFilterSubGroup", filter.SubGroup.GetType().Name);
				}
				if (jobSubGroupFilters.Contains(filter.Description))
				{
					AssertEquals(string.Format("JobFilterSubGroup for filter {0}", filter.Description), "JobFilterSubGroup", filter.SubGroup.GetType().Name);
				}
				if (operationsSubGroupFilters.Contains(filter.Description))
				{
					AssertEquals(string.Format("OperationsFilterSubGroup for filter {0}", filter.Description), "OperationsFilterSubGroup", filter.SubGroup.GetType().Name);
				}
				if (miscSubGroupFilters.Contains(filter.Description))
				{
					AssertEquals(string.Format("MiscFilterSubGroup for filter {0}", filter.Description), "MiscFilterSubGroup", filter.SubGroup.GetType().Name);
				}
			}
		}

		public void TestFiltersSubGroupsForCA()
		{
			var additionalSubGroupFilterPairs = new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>("Accounting Date", "MiscFilterSubGroup")
			};

			var filterBizO = GetNewFilterStripBusinessObject();

			foreach (var pair in additionalSubGroupFilterPairs)
			{
				AssertEquals(0, filterBizO.Count(x => x.Description == pair.Key));
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Enterprise.Core.Constants.CountryCodes.Canada))
			{
				var filterBizOCA = GetNewFilterStripBusinessObject();
				foreach (var pair in additionalSubGroupFilterPairs)
				{
					var target = filterBizOCA.ModuleFilters.Single(x => x.Description == pair.Key);
					AssertNotNull(target);
					AssertEquals(pair.Value, target.SubGroup.GetType().Name);
				}
			}
		}

		public void TestFiltersVisibility()
		{
			AssertFilterCount();
		}

		protected abstract void AssertFilterCount();

		public void TestChargeLineDescriptionFilterVisibility()
		{
			// Filter should not be there by default
			PeriodicInvoiceBase periodicInvoiceBase = CreatePeriodicInvoice();
			var filter = periodicInvoiceBase.JobsFilter["Charge Line Description"];
			AssertNull("Charge Line Description Filter shouldn't be present unless it's turned on", filter);

			// Enable Registry and test it appears
			AccountingConfigurationRegistry.Instance.AllowDescriptionChargeLineFilterInPeriodicInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			periodicInvoiceBase = CreatePeriodicInvoice();
			filter = periodicInvoiceBase.JobsFilter["Charge Line Description"];
			AssertNotNull("Charge Line Description Filter should be present because registry is on", filter);
		}

		public void TestChargeLineDescriptionFilter()
		{
			AccountingConfigurationRegistry.Instance.AllowDescriptionChargeLineFilterInPeriodicInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_JH = Job1.PK;
			charge2.JR_JH = Job2.PK;

			charge1.JR_OSSellAmt = charge2.JR_OSSellAmt = 10m;
			charge1.JR_LocalSellAmt = charge2.JR_LocalSellAmt = 10m;
			charge1.JR_RX_NKSellCurrency = charge2.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge1.JR_OH_SellAccount = charge2.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_InvoiceType = charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge1.JR_Desc = "ABC123";
			charge2.JR_Desc = "XYz987";

			PeriodicInvoiceBase periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = "AUD";
			PeriodicInvoice periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			}

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)periodicInvoiceBase.JobsFilter["Charge Line Description"];

			filter.Property = "ABC123";
			filter.IsActive = true;

			periodicInvoiceBase.JobTypeList.ToList().ForEach(x => x.Value = true);
			periodicInvoiceBase.LoadJobs();

			Assert("Expecting collection to contain Job1", periodicInvoiceBase.Jobs.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !periodicInvoiceBase.Jobs.Contains(Job2));

			filter.Property = "XYZ987";
			filter.IsActive = true;

			periodicInvoiceBase.LoadJobs();

			Assert("Expecting collection not to contain Job1", !periodicInvoiceBase.Jobs.Contains(Job1));
			Assert("Expecting collection to contain Job2", periodicInvoiceBase.Jobs.Contains(Job2));

			filter.Property = "zzzzzz";
			filter.IsActive = true;

			periodicInvoiceBase.LoadJobs();

			Assert("Expecting collection not to contain Job1", !periodicInvoiceBase.Jobs.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !periodicInvoiceBase.Jobs.Contains(Job2));
		}

		#region Order Reference Filter Test

		public void TestOrderReferenceFilter()
		{
			var periodicInvoiceBase = CreatePeriodicInvoice();
			var filter = periodicInvoiceBase.JobsFilter["Order Reference"];
			AssertNotNull("Order Reference Filter shouldn be present", filter);
		}

		#endregion

		#region Additional Reference Filter Test

		public void TestAdditionalReferenceFilter()
		{
			var periodicInvoiceBase = CreatePeriodicInvoice();
			var filter = periodicInvoiceBase.JobsFilter["Additional Reference #"];
			AssertNotNull("Additional Reference # Filter shouldn be present", filter);
		}

		#endregion

		#region Charge Filters Tests

		public void TestDebtorFilter()
		{
			if (GetExpectedBusinessObjectType() == typeof(PeriodicInvoiceJobsFilterBusinessObject))
			{
				OrgHeader organisation1 = Factory.NewWithValidTestData<OrgHeader>();
				OrgHeader organisation2 = Factory.NewWithValidTestData<OrgHeader>();

				JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
				JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();

				charge1.JR_JH = Job1.PK;
				charge2.JR_JH = Job2.PK;

				charge1.JR_OH_SellAccount = organisation1.PK;
				charge2.JR_OH_SellAccount = organisation2.PK;
				charge1.JR_OSSellAmt = charge2.JR_OSSellAmt = 10m;
				charge1.JR_LocalSellAmt = charge2.JR_LocalSellAmt = 10m;
				charge1.JR_RX_NKSellCurrency = charge2.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
				charge1.JR_OH_SellAccount = organisation1.PK;
				charge2.JR_OH_SellAccount = organisation2.PK;
				charge1.JR_InvoiceType = charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

				Factory.Save();

				PeriodicInvoiceBase periodicInvoiceBase = CreatePeriodicInvoice();
				periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				PeriodicInvoice periodicInvoice = periodicInvoiceBase as PeriodicInvoice;

				ModuleGuidFilter filter = (ModuleGuidFilter)periodicInvoiceBase.JobsFilter["Debtor"];
				if (periodicInvoice != null)
				{
					periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
					periodicInvoice.DebtorPK = organisation1.PK;
				}
				else
				{
					filter.Property = organisation1.PK;
					filter.IsActive = true;
				}

				periodicInvoiceBase.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.CFSShipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.CFSLoadList.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Brokerage.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.QuotedBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.OneOffQuotation.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.TransportBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.TransportBookingConsignment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.LocalCartage.Code))).ToList().ForEach(x => x.Value = true);
				periodicInvoiceBase.LoadJobs();

				Assert("Expecting collection to contain Job1", periodicInvoiceBase.Jobs.Contains(Job1));
				Assert("Expecting collection not to contain Job2", !periodicInvoiceBase.Jobs.Contains(Job2));

				if (periodicInvoice != null)
				{
					periodicInvoice.DebtorPK = organisation2.PK;
				}
				else
				{
					filter.Property = organisation2.PK;
					filter.IsActive = true;
				}

				periodicInvoiceBase.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.CFSShipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.CFSLoadList.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Brokerage.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.QuotedBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.OneOffQuotation.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.TransportBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.TransportBookingConsignment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.LocalCartage.Code))).ToList().ForEach(x => x.Value = true);
				periodicInvoiceBase.LoadJobs();

				Assert("Expecting collection to contain Job1", !periodicInvoiceBase.Jobs.Contains(Job1));
				Assert("Expecting collection not to contain Job2", periodicInvoiceBase.Jobs.Contains(Job2));
			}
			else
			{
				Assert(true);
			}
		}

		public void TestCurrencyFilter()
		{
			var orgHeader = TestObjectCreator.ABIGAS;
			var chargeCode = TestObjectCreator.CC1;
			var aud = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			var usd = TestObjectCreator.USD.Code;

			var charge1 = Factory.NewWithValidTestData<JobCharge>();
			var charge2 = Factory.NewWithValidTestData<JobCharge>();
			var charge3 = Factory.NewWithValidTestData<JobCharge>();
			var charge4 = Factory.NewWithValidTestData<JobCharge>();
			var charge5 = Factory.NewWithValidTestData<JobCharge>();

			charge1.JR_JH = Job1.PK;
			charge1.JR_AC = chargeCode.PK;
			charge1.JR_GB = GlbBranch.CurrentBranch.PK;
			charge1.JR_OH_SellAccount = orgHeader.PK;
			charge1.JR_RX_NKSellCurrency = usd;
			charge1.JR_OSSellAmt = 100m;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge1.JR_OSSellExRate = 2m;

			charge2.JR_JH = Job2.PK;
			charge2.JR_AC = chargeCode.PK;
			charge2.JR_OSSellAmt = 200m;
			charge2.JR_GB = GlbBranch.CurrentBranch.PK;
			charge2.JR_OH_SellAccount = orgHeader.PK;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge2.JR_RX_NKSellInvoiceCurrency = usd;

			charge3.JR_JH = Job3.PK;
			charge3.JR_AC = chargeCode.PK;
			charge3.JR_GB = GlbBranch.CurrentBranch.PK;
			charge3.JR_OH_SellAccount = orgHeader.PK;
			charge3.JR_RX_NKSellCurrency = usd;
			charge3.JR_OSSellAmt = 150m;
			charge3.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge3.JR_OSSellExRate = 2m;

			charge4.JR_JH = Job4.PK;
			charge4.JR_AC = chargeCode.PK;
			charge4.JR_OSSellAmt = 400m;
			charge4.JR_GB = GlbBranch.CurrentBranch.PK;
			charge4.JR_OH_SellAccount = orgHeader.PK;
			charge4.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge4.JR_RX_NKSellInvoiceCurrency = usd;

			charge5.JR_JH = Job4.PK;
			charge5.JR_AC = chargeCode.PK;
			charge5.JR_GB = GlbBranch.CurrentBranch.PK;
			charge5.JR_OH_SellAccount = orgHeader.PK;
			charge5.JR_RX_NKSellCurrency = usd;
			charge5.JR_OSSellAmt = 250m;
			charge5.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge5.JR_OSSellExRate = 2m;

			Factory.Save();

			var periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = aud;
			var periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.DebtorPK = orgHeader.PK;
			}

			periodicInvoiceBase.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.CFSShipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.CFSLoadList.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Brokerage.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.QuotedBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.OneOffQuotation.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.TransportBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.TransportBookingConsignment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.LocalCartage.Code))).ToList().ForEach(x => x.Value = true);
			periodicInvoiceBase.LoadJobs();

			var filter = (ModuleNkFilter)periodicInvoiceBase.JobsFilter["Currency"];
			AssertEquals("Filter Property", aud, filter.Property);

			AssertEquals("Expecting collection to contain Job1", true, periodicInvoiceBase.Jobs.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2", false, periodicInvoiceBase.Jobs.Contains(Job2));
			AssertEquals("Expecting collection not to contain Job3", false, periodicInvoiceBase.Jobs.Contains(Job3));
			AssertEquals("Expecting collection not to contain Job4", false, periodicInvoiceBase.Jobs.Contains(Job4));

			periodicInvoiceBase.CurrencyNK = usd;
			periodicInvoiceBase.LoadJobs();

			AssertEquals("Filter Property", usd, filter.Property);

			AssertEquals("Expecting collection not to contain Job1", false, periodicInvoiceBase.Jobs.Contains(Job1));
			AssertEquals("Expecting collection to contain Job2", true, periodicInvoiceBase.Jobs.Contains(Job2));
			AssertEquals("Expecting collection not to contain Job3", false, periodicInvoiceBase.Jobs.Contains(Job3));
			AssertEquals("Expecting collection not to contain Job4", false, periodicInvoiceBase.Jobs.Contains(Job4));
		}

		public void TestTaxBranchFilter()
		{
			var testBranch1 = TestObjectCreator.CreateBranch("001", GlbCompany.CurrentCompany);
			var testBranch2 = TestObjectCreator.CreateBranch("002", GlbCompany.CurrentCompany);

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var orgHeader = TestObjectCreator.ABIGAS;

			var charge1 = Factory.NewWithValidTestData<JobCharge>();
			var charge2 = Factory.NewWithValidTestData<JobCharge>();

			charge1.JR_JH = Job1.PK;
			charge2.JR_JH = Job2.PK;
			charge1.JR_AC = chargeCode.PK;
			charge2.JR_AC = chargeCode.PK;
			charge1.JR_OSSellAmt = 100m;
			charge1.JR_LocalSellAmt = 100m;
			charge2.JR_OSSellAmt = 200m;
			charge2.JR_LocalSellAmt = 200m;
			charge1.JR_GB = GlbBranch.CurrentBranch.PK;
			charge2.JR_GB = GlbBranch.CurrentBranch.PK;
			charge1.JR_OH_SellAccount = orgHeader.PK;
			charge2.JR_OH_SellAccount = orgHeader.PK;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			charge1.JR_GB_SellTaxBranch = testBranch1.PK;
			charge2.JR_GB_SellTaxBranch = testBranch2.PK;

			Factory.Save();

			var periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			periodicInvoiceBase.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.CFSShipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.CFSLoadList.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Brokerage.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.QuotedBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.OneOffQuotation.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.TransportBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.TransportBookingConsignment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.LocalCartage.Code))).ToList().ForEach(x => x.Value = true);

			var periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.DebtorPK = orgHeader.PK;
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.TaxBranch = testBranch1.PK;
				periodicInvoice.LoadJobs();

				var filter = (ModuleGuidFilter)periodicInvoice.JobsFilter["Tax Branch"];
				AssertEquals("Filter Property", testBranch1.PK, filter.Property);

				AssertEquals("Expecting collection to contain Job1", true, periodicInvoice.Jobs.Contains(Job1));
				AssertEquals("Expecting collection not to contain Job2", false, periodicInvoice.Jobs.Contains(Job2));

				periodicInvoice.TaxBranch = testBranch2.PK;
				periodicInvoice.LoadJobs();

				AssertEquals("Filter Property", testBranch2.PK, filter.Property);

				Assert("Expecting collection not to contain Job1", !periodicInvoice.Jobs.Contains(Job1));
				Assert("Expecting collection to contain Job2", periodicInvoice.Jobs.Contains(Job2));
			}
			else
			{
				periodicInvoiceBase.LoadJobs();

				Assert("Expecting collection to contain Job1", periodicInvoiceBase.Jobs.Contains(Job1));
				Assert("Expecting collection to contain Job2", periodicInvoiceBase.Jobs.Contains(Job2));
			}
		}

		public void TestChargeCodeFilter()
		{
			AccChargeCode chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();

			OrgHeader orgHeader = TestObjectCreator.ABIGAS;

			JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();

			charge1.JR_JH = Job1.PK;
			charge2.JR_JH = Job2.PK;
			charge1.JR_AC = chargeCode1.PK;
			charge2.JR_AC = chargeCode2.PK;
			charge1.JR_OSSellAmt = 100m;
			charge1.JR_LocalSellAmt = 100m;
			charge2.JR_OSSellAmt = 200m;
			charge2.JR_LocalSellAmt = 200m;
			charge1.JR_GB = GlbBranch.CurrentBranch.PK;
			charge2.JR_GB = GlbBranch.CurrentBranch.PK;
			charge1.JR_OH_SellAccount = orgHeader.PK;
			charge2.JR_OH_SellAccount = orgHeader.PK;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			Factory.Save();

			PeriodicInvoiceBase periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			PeriodicInvoice periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.DebtorPK = orgHeader.PK;
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			}

			ModuleGuidFilter filter = (ModuleGuidFilter)periodicInvoiceBase.JobsFilter["Charge Code"];
			filter.Property = chargeCode1.PK;
			filter.IsActive = true;

			periodicInvoiceBase.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.CFSShipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.CFSLoadList.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Brokerage.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.QuotedBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.OneOffQuotation.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.TransportBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.TransportBookingConsignment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.LocalCartage.Code))).ToList().ForEach(x => x.Value = true);
			periodicInvoiceBase.LoadJobs();

			AssertEquals("Expecting collection to contain Job1", true, periodicInvoiceBase.Jobs.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2", false, periodicInvoiceBase.Jobs.Contains(Job2));

			filter.Property = chargeCode2.PK;
			filter.IsActive = true;

			periodicInvoiceBase.LoadJobs();

			AssertEquals("Expecting collection not to contain Job1", false, periodicInvoiceBase.Jobs.Contains(Job1));
			AssertEquals("Expecting collection to contain Job2", true, periodicInvoiceBase.Jobs.Contains(Job2));
		}

		public void TestARSettlementGroupFilter()
		{
			if (GetExpectedBusinessObjectType() == typeof(PeriodicInvoiceBulkJobsFilterBusinessObject))
			{
				var organisation1 = Factory.NewWithValidTestData<OrgHeader>();
				var organisation2 = Factory.NewWithValidTestData<OrgHeader>();
				var settGroup1 = Factory.NewWithValidTestData<OrgHeader>();
				var settGroup2 = Factory.NewWithValidTestData<OrgHeader>();

				var orgParty1 = Factory.NewWithValidTestData<OrgRelatedParty>();
				var orgParty2 = Factory.NewWithValidTestData<OrgRelatedParty>();

				orgParty1.PR_OH_Parent = organisation1.PK;
				orgParty2.PR_OH_Parent = organisation2.PK;
				orgParty1.PR_PartyType = RelatedPartyTypeList.Codes.ARSettlementGroup;
				orgParty2.PR_PartyType = RelatedPartyTypeList.Codes.ARSettlementGroup;
				orgParty1.PR_OH_RelatedParty = settGroup1.PK;
				orgParty2.PR_OH_RelatedParty = settGroup2.PK;

				var charge1 = Factory.NewWithValidTestData<JobCharge>();
				var charge2 = Factory.NewWithValidTestData<JobCharge>();

				charge1.JR_JH = Job1.PK;
				charge2.JR_JH = Job2.PK;

				charge1.JR_OH_SellAccount = organisation1.PK;
				charge2.JR_OH_SellAccount = organisation2.PK;

				charge1.JR_OSSellAmt = charge2.JR_OSSellAmt = 10m;
				charge1.JR_LocalSellAmt = charge2.JR_LocalSellAmt = 10m;
				charge1.JR_InvoiceType = charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

				Factory.Save();

				var periodicInvoiceBase = CreatePeriodicInvoice();
				periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				var periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
				if (periodicInvoice != null)
				{
					periodicInvoice.DebtorPK = organisation1.PK;
					periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				}

				var filter = (ModuleGuidFilter)periodicInvoiceBase.JobsFilter["AR Settlement Group"];
				AssertNotNull("AR Settlement Group", filter);
				filter.Property = settGroup1.PK;
				filter.IsActive = true;

				periodicInvoiceBase.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.CFSShipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.CFSLoadList.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Brokerage.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.QuotedBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.OneOffQuotation.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.TransportBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.TransportBookingConsignment.Code))).ToList().ForEach(x => x.Value = true);
				periodicInvoiceBase.LoadJobs();

				Assert("Expecting collection to contain Job1", periodicInvoiceBase.Jobs.Contains(Job1));
				Assert("Expecting collection not to contain Job2", !periodicInvoiceBase.Jobs.Contains(Job2));
			}
			else
			{
				Assert(true);
			}
		}

		public void TestOrganizationBranchFilter()
		{
			if (GetExpectedBusinessObjectType() == typeof(PeriodicInvoiceBulkJobsFilterBusinessObject))
			{
				var branch1 = Factory.NewWithValidTestData<GlbBranch>();
				var branch2 = Factory.NewWithValidTestData<GlbBranch>();

				var organisation1 = Factory.NewWithValidTestData<OrgHeader>();
				var organisation2 = Factory.NewWithValidTestData<OrgHeader>();

				organisation1.CompanyData.OB_GB_ControllingBranch = branch1.PK;
				organisation2.CompanyData.OB_GB_ControllingBranch = branch2.PK;

				var charge1 = Factory.NewWithValidTestData<JobCharge>();
				var charge2 = Factory.NewWithValidTestData<JobCharge>();

				charge1.JR_JH = Job1.PK;
				charge2.JR_JH = Job2.PK;

				charge1.JR_OH_SellAccount = organisation1.PK;
				charge2.JR_OH_SellAccount = organisation2.PK;

				charge1.JR_OSSellAmt = charge2.JR_OSSellAmt = 10m;
				charge1.JR_LocalSellAmt = charge2.JR_LocalSellAmt = 10m;
				charge1.JR_InvoiceType = charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

				var periodicInvoiceBase = CreatePeriodicInvoice();
				periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				var periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
				if (periodicInvoice != null)
				{
					periodicInvoice.DebtorPK = organisation1.PK;
					periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				}

				Factory.Save();

				var filter = (ModuleGuidFilter)periodicInvoiceBase.JobsFilter["Organization Branch"];
				AssertNotNull("Organization Branch", filter);
				filter.Property = branch1.PK;
				filter.IsActive = true;

				periodicInvoiceBase.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.CFSShipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.CFSLoadList.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Brokerage.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.QuotedBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.OneOffQuotation.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.TransportBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.TransportBookingConsignment.Code))).ToList().ForEach(x => x.Value = true);
				periodicInvoiceBase.LoadJobs();

				Assert("Expecting collection to contain Job1", periodicInvoiceBase.Jobs.Contains(Job1));
				Assert("Expecting collection not to contain Job2", !periodicInvoiceBase.Jobs.Contains(Job2));

				filter.Property = branch2.PK;
				filter.IsActive = true;

				periodicInvoiceBase.LoadJobs();

				Assert("Expecting collection not to contain Job1", !periodicInvoiceBase.Jobs.Contains(Job1));
				Assert("Expecting collection to contain Job2", periodicInvoiceBase.Jobs.Contains(Job2));
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region Charge Filters Tests

		public void TestInvoiceTypeFilter()
		{
			JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_JH = Job1.PK;
			charge2.JR_JH = Job2.PK;

			charge1.JR_OSSellAmt = charge2.JR_OSSellAmt = 10m;
			charge1.JR_LocalSellAmt = charge2.JR_LocalSellAmt = 10m;

			charge1.JR_OH_SellAccount = charge2.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			PeriodicInvoiceBase periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			PeriodicInvoice periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			}

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)periodicInvoiceBase.JobsFilter["Invoice Type"];

			filter.Property = InvoiceTypesList.Codes.FinalInvoice_Batching;
			filter.IsActive = true;

			periodicInvoiceBase.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.CFSShipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.CFSLoadList.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Brokerage.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.QuotedBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.OneOffQuotation.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.TransportBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.TransportBookingConsignment.Code))).ToList().ForEach(x => x.Value = true);
			periodicInvoiceBase.LoadJobs();

			Assert("Expecting collection not to contain Job1", !periodicInvoiceBase.Jobs.Contains(Job1));
			Assert("Expecting collection to contain Job2", periodicInvoiceBase.Jobs.Contains(Job2));
			Assert("Expecting collection not to contain Job3", !periodicInvoiceBase.Jobs.Contains(Job3));
		}

		public void TestChargeLineBranchFilter()
		{
			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			GlbBranch branch2 = Factory.NewWithValidTestData<GlbBranch>();
			GlbBranch branch3 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch3.GB_GC = GlbCompany.CurrentCompany.PK;

			JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_JH = Job1.PK;
			charge2.JR_JH = Job2.PK;

			charge1.JR_GB = branch1.PK;
			charge2.JR_GB = branch2.PK;

			charge1.JR_OSSellAmt = charge2.JR_OSSellAmt = 10m;
			charge1.JR_LocalSellAmt = charge2.JR_LocalSellAmt = 10m;
			charge1.JR_RX_NKSellCurrency = charge2.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge1.JR_OH_SellAccount = charge2.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_InvoiceType = charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			PeriodicInvoiceBase periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			PeriodicInvoice periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			}

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)periodicInvoiceBase.JobsFilter["Charge Line Branch"];

			filter.Property = branch1.PK;
			filter.IsActive = true;

			periodicInvoiceBase.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.CFSShipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.CFSLoadList.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Brokerage.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.QuotedBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.OneOffQuotation.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.TransportBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.TransportBookingConsignment.Code))).ToList().ForEach(x => x.Value = true);
			periodicInvoiceBase.LoadJobs();

			Assert("Expecting collection to contain Job1", periodicInvoiceBase.Jobs.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !periodicInvoiceBase.Jobs.Contains(Job2));
			Assert("Expecting collection not to contain Job3", !periodicInvoiceBase.Jobs.Contains(Job3));

			filter.Property = branch3.PK;
			filter.IsActive = true;

			periodicInvoiceBase.LoadJobs();

			Assert("Expecting collection not to contain Job1", !periodicInvoiceBase.Jobs.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !periodicInvoiceBase.Jobs.Contains(Job2));
			Assert("Expecting collection not to contain Job3", !periodicInvoiceBase.Jobs.Contains(Job3));
		}

		public void TestChargeLineFPOSFilterVisibility()
		{
			var periodicInvoiceBase = CreatePeriodicInvoice();
			var filter = periodicInvoiceBase.JobsFilter["Charge Line Fixed Place of Supply"];
			AssertNull(filter);

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				periodicInvoiceBase = CreatePeriodicInvoice();
				filter = periodicInvoiceBase.JobsFilter["Charge Line Fixed Place of Supply"];
				AssertNotNull(filter);
			}
		}

		public void TestChargeLineFPOSFilter()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				var charge1 = Factory.NewWithValidTestData<JobCharge>();
				var charge2 = Factory.NewWithValidTestData<JobCharge>();
				var charge3 = Factory.NewWithValidTestData<JobCharge>();
				var charge4 = Factory.NewWithValidTestData<JobCharge>();
				var charge5 = Factory.NewWithValidTestData<JobCharge>();

				charge1.JR_JH = Job1.PK;
				charge2.JR_JH = Job2.PK;
				charge3.JR_JH = Job3.PK;
				charge4.JR_JH = Job4.PK;
				charge5.JR_JH = Job4.PK;

				foreach (var charge in new[] { charge1, charge2, charge3, charge4, charge5 })
				{
					charge.JR_OSSellAmt = 10m;
					charge.JR_LocalSellAmt = 10m;
					charge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
					charge.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
					charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				}

				charge1.JR_CostPlaceOfSupply = "DL";
				charge2.JR_SellPlaceOfSupply = "JH";
				charge3.JR_SellPlaceOfSupply = "DL";
				charge4.JR_SellPlaceOfSupply = "BR";
				charge5.JR_SellPlaceOfSupply = string.Empty;

				var periodicInvoiceBase = CreatePeriodicInvoice();
				periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				var periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
				if (periodicInvoice != null)
				{
					periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
					periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
				}

				Factory.Save();

				var filter = (ModuleTextFilter)periodicInvoiceBase.JobsFilter["Charge Line Fixed Place of Supply"];
				periodicInvoiceBase.JobTypeList.ToList().ForEach(x => x.Value = true);
				AssertFilter("DL", false, false, true, false);
				AssertFilter("AR", false, false, false, false);
				AssertFilter("", true, true, true, true);

				void AssertFilter(string filterValue, bool job1, bool job2, bool job3, bool job4)
				{
					filter.Property = filterValue;
					filter.IsActive = true;
					periodicInvoiceBase.LoadJobs();
					AssertEquals(job1, periodicInvoiceBase.Jobs.Contains(Job1));
					AssertEquals(job2, periodicInvoiceBase.Jobs.Contains(Job2));
					AssertEquals(job3, periodicInvoiceBase.Jobs.Contains(Job3));
					AssertEquals(job4, periodicInvoiceBase.Jobs.Contains(Job4));
				}
			}
		}

		public void TestChargeLineDepartmentFilter()
		{
			GlbDepartment department1 = Factory.NewWithValidTestData<GlbDepartment>();
			GlbDepartment department2 = Factory.NewWithValidTestData<GlbDepartment>();
			GlbDepartment department3 = Factory.NewWithValidTestData<GlbDepartment>();

			JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_JH = Job1.PK;
			charge2.JR_JH = Job2.PK;

			charge1.JR_GE = department1.PK;
			charge2.JR_GE = department2.PK;

			charge1.JR_OSSellAmt = charge2.JR_OSSellAmt = 10m;
			charge1.JR_LocalSellAmt = charge2.JR_LocalSellAmt = 10m;
			charge1.JR_RX_NKSellCurrency = charge2.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge1.JR_OH_SellAccount = charge2.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_InvoiceType = charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			PeriodicInvoiceBase periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			PeriodicInvoice periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			}

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)periodicInvoiceBase.JobsFilter["Charge Line Department"];

			filter.Property = department1.PK;
			filter.IsActive = true;

			periodicInvoiceBase.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.CFSShipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.CFSLoadList.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Brokerage.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.QuotedBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.OneOffQuotation.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.TransportBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.TransportBookingConsignment.Code))).ToList().ForEach(x => x.Value = true);
			periodicInvoiceBase.LoadJobs();

			Assert("Expecting collection to contain Job1", periodicInvoiceBase.Jobs.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !periodicInvoiceBase.Jobs.Contains(Job2));

			filter.Property = department3.PK;
			filter.IsActive = true;

			periodicInvoiceBase.LoadJobs();

			Assert("Expecting collection not to contain Job1", !periodicInvoiceBase.Jobs.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !periodicInvoiceBase.Jobs.Contains(Job2));
		}

		public void TestChargeLineSellReference()
		{
			JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_JH = Job1.PK;
			charge2.JR_JH = Job2.PK;

			charge1.JR_OSSellAmt = charge2.JR_OSSellAmt = 10m;
			charge1.JR_LocalSellAmt = charge2.JR_LocalSellAmt = 10m;
			charge1.JR_RX_NKSellCurrency = charge2.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge1.JR_OH_SellAccount = charge2.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_InvoiceType = charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge1.JR_SellReference = "ABC123";
			charge2.JR_SellReference = "XYz987";

			PeriodicInvoiceBase periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = "AUD";
			PeriodicInvoice periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			}

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)periodicInvoiceBase.JobsFilter["Charge Line Sell Reference"];

			filter.Property = "ABC123";
			filter.IsActive = true;

			periodicInvoiceBase.JobTypeList.ToList().ForEach(x => x.Value = true);
			periodicInvoiceBase.LoadJobs();

			Assert("Expecting collection to contain Job1", periodicInvoiceBase.Jobs.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !periodicInvoiceBase.Jobs.Contains(Job2));

			filter.Property = "XYZ987";
			filter.IsActive = true;

			periodicInvoiceBase.LoadJobs();

			Assert("Expecting collection not to contain Job1", !periodicInvoiceBase.Jobs.Contains(Job1));
			Assert("Expecting collection to contain Job2", periodicInvoiceBase.Jobs.Contains(Job2));

			filter.Property = "zzzzzz";
			filter.IsActive = true;

			periodicInvoiceBase.LoadJobs();

			Assert("Expecting collection not to contain Job1", !periodicInvoiceBase.Jobs.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !periodicInvoiceBase.Jobs.Contains(Job2));
		}

		#endregion

		#region Job Header Filters Tests

		public void TestJobHeaderBranchFilter()
		{
			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			GlbBranch branch2 = Factory.NewWithValidTestData<GlbBranch>();
			GlbBranch branch3 = Factory.NewWithValidTestData<GlbBranch>();

			Job1.JH_GB = branch1.PK;
			Job2.JH_GB = branch2.PK;

			JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_JH = Job1.PK;
			charge2.JR_JH = Job2.PK;
			charge1.JR_OSSellAmt = charge2.JR_OSSellAmt = 10m;
			charge1.JR_LocalSellAmt = charge2.JR_LocalSellAmt = 10m;
			charge1.JR_RX_NKSellCurrency = charge2.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge1.JR_OH_SellAccount = charge2.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_InvoiceType = charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			PeriodicInvoiceBase periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			PeriodicInvoice periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			}

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)periodicInvoiceBase.JobsFilter["Job Header Branch"];

			filter.Property = branch1.PK;
			filter.IsActive = true;

			periodicInvoiceBase.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.CFSShipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.CFSLoadList.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Brokerage.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.QuotedBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.OneOffQuotation.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.TransportBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.TransportBookingConsignment.Code))).ToList().ForEach(x => x.Value = true);
			periodicInvoiceBase.LoadJobs();

			Assert("Expecting collection to contain Job1", periodicInvoiceBase.Jobs.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !periodicInvoiceBase.Jobs.Contains(Job2));
			Assert("Expecting collection not to contain Job3", !periodicInvoiceBase.Jobs.Contains(Job3));

			filter.Property = branch3.PK;
			filter.IsActive = true;

			periodicInvoiceBase.LoadJobs();

			Assert("Expecting collection not to contain Job1", !periodicInvoiceBase.Jobs.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !periodicInvoiceBase.Jobs.Contains(Job2));
			Assert("Expecting collection not to contain Job3", !periodicInvoiceBase.Jobs.Contains(Job3));
		}

		public void TestJobHeaderDepartmentFilter()
		{
			GlbDepartment department1 = Factory.NewWithValidTestData<GlbDepartment>();
			GlbDepartment department2 = Factory.NewWithValidTestData<GlbDepartment>();
			GlbDepartment department3 = Factory.NewWithValidTestData<GlbDepartment>();

			Job1.JH_GE = department1.PK;
			Job2.JH_GE = department2.PK;

			JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_JH = Job1.PK;
			charge2.JR_JH = Job2.PK;
			charge1.JR_OSSellAmt = charge2.JR_OSSellAmt = 10m;
			charge1.JR_LocalSellAmt = charge2.JR_LocalSellAmt = 10m;
			charge1.JR_RX_NKSellCurrency = charge2.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge1.JR_OH_SellAccount = charge2.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_InvoiceType = charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			PeriodicInvoiceBase periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			PeriodicInvoice periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			}

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)periodicInvoiceBase.JobsFilter["Job Header Department"];

			filter.Property = department1.PK;
			filter.IsActive = true;

			periodicInvoiceBase.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.CFSShipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.CFSLoadList.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Brokerage.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.QuotedBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.OneOffQuotation.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.TransportBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.TransportBookingConsignment.Code))).ToList().ForEach(x => x.Value = true);
			periodicInvoiceBase.LoadJobs();

			Assert("Expecting collection to contain Job1", periodicInvoiceBase.Jobs.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !periodicInvoiceBase.Jobs.Contains(Job2));

			filter.Property = department3.PK;
			filter.IsActive = true;

			periodicInvoiceBase.LoadJobs();

			Assert("Expecting collection not to contain Job1", !periodicInvoiceBase.Jobs.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !periodicInvoiceBase.Jobs.Contains(Job2));
		}

		public void TestJobLocalClientFilter()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			Job1.JH_ParentTableCode = "JJ";
			Job2.JH_ParentTableCode = "JJ";
			Job1.LocalChargesPK = org1.PK;
			Job2.LocalChargesPK = org2.PK;

			JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_JH = Job1.PK;
			charge2.JR_JH = Job2.PK;
			charge1.JR_OSSellAmt = charge2.JR_OSSellAmt = 10m;
			charge1.JR_LocalSellAmt = charge2.JR_LocalSellAmt = 10m;
			charge1.JR_RX_NKSellCurrency = charge2.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge1.JR_OH_SellAccount = charge2.JR_OH_SellAccount = org1.PK;
			charge1.JR_InvoiceType = charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			PeriodicInvoiceBase periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			PeriodicInvoice periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.DebtorPK = org1.PK;
			}

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)periodicInvoiceBase.JobsFilter["Job Local Client"];
			filter.Property = org1.PK;
			filter.IsActive = true;

			periodicInvoiceBase.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.CFSShipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.CFSLoadList.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Brokerage.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.QuotedBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.OneOffQuotation.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.TransportBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.TransportBookingConsignment.Code))).ToList().ForEach(x => x.Value = true);

			periodicInvoiceBase.LoadJobs();
			Assert("Expecting collection to contain Job1", periodicInvoiceBase.Jobs.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !periodicInvoiceBase.Jobs.Contains(Job2));

			filter.Property = org3.PK;
			filter.IsActive = true;
			periodicInvoiceBase.LoadJobs();
			Assert("Expecting collection not to contain Job1", !periodicInvoiceBase.Jobs.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !periodicInvoiceBase.Jobs.Contains(Job2));
		}

		[TestDate(2013, 07, 22)]
		public void TestJobOpenDateQuery()
		{
			Job1.JH_A_JOP = new ZDateTime(2009, 02, 01);
			Job2.JH_A_JOP = new ZDateTime(2009, 05, 01);

			JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_JH = Job1.PK;
			charge2.JR_JH = Job2.PK;
			charge1.JR_OSSellAmt = charge2.JR_OSSellAmt = 10m;
			charge1.JR_LocalSellAmt = charge2.JR_LocalSellAmt = 10m;
			charge1.JR_RX_NKSellCurrency = charge2.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge1.JR_OH_SellAccount = charge2.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_InvoiceType = charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			PeriodicInvoiceBase periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			PeriodicInvoice periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			}

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)periodicInvoiceBase.JobsFilter["Job Open Date"];

			filter.Property1 = new ZDateTime(2009, 01, 01);
			filter.Property2 = new ZDateTime(2009, 06, 01);
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			periodicInvoiceBase.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.CFSShipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.CFSLoadList.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Brokerage.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.QuotedBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.OneOffQuotation.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.TransportBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.TransportBookingConsignment.Code))).ToList().ForEach(x => x.Value = true);
			periodicInvoiceBase.LoadJobs();

			Assert("Expecting collection to contain Job1", periodicInvoiceBase.Jobs.Contains(Job1));
			Assert("Expecting collection to contain Job2", periodicInvoiceBase.Jobs.Contains(Job2));

			filter.Property1 = new ZDateTime(2009, 04, 01);
			filter.Property2 = new ZDateTime(2009, 06, 01);
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			periodicInvoiceBase.LoadJobs();

			Assert("Expecting collection not to contain Job1", !periodicInvoiceBase.Jobs.Contains(Job1));
			Assert("Expecting collection to contain Job2", periodicInvoiceBase.Jobs.Contains(Job2));

			filter.Property1 = new ZDateTime(2009, 06, 01);
			filter.Property2 = new ZDateTime(2009, 06, 02);
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			periodicInvoiceBase.LoadJobs();

			Assert("Expecting collection not to contain Job1", !periodicInvoiceBase.Jobs.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !periodicInvoiceBase.Jobs.Contains(Job2));
		}

		public void TestJobStatusQuery()
		{
			JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			JobCharge charge3 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_JH = Job1.PK;
			charge2.JR_JH = Job2.PK;
			charge3.JR_JH = Job3.PK;
			charge1.JR_OSSellAmt = charge2.JR_OSSellAmt = charge3.JR_OSSellAmt = 10m;
			charge1.JR_LocalSellAmt = charge2.JR_LocalSellAmt = charge3.JR_LocalSellAmt = 10m;
			charge1.JR_RX_NKSellCurrency = charge2.JR_RX_NKSellCurrency = charge3.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge1.JR_OH_SellAccount = charge2.JR_OH_SellAccount = charge3.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_InvoiceType = charge2.JR_InvoiceType = charge3.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			PeriodicInvoiceBase periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			PeriodicInvoice periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			}

			Factory.Save();

			Job1.JH_Status = JobHeaderStatus.Closed.Code;
			Job2.JH_Status = JobHeaderStatus.Working.Code;
			Job3.JH_Status = JobHeaderStatus.JobInvoiced.Code;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)periodicInvoiceBase.JobsFilter["Job Status"];

			filter.Property = "OPN";
			filter.IsActive = true;

			periodicInvoiceBase.LoadJobs();

			Assert("Expecting collection not to contain Job1", !periodicInvoiceBase.Jobs.Contains(Job1));
			Assert("Expecting collection to contain Job2", periodicInvoiceBase.Jobs.Contains(Job2));
			Assert("Expecting collection to contain Job3", periodicInvoiceBase.Jobs.Contains(Job3));

			filter.Property = JobHeaderStatus.Working.Code;
			filter.IsActive = true;

			periodicInvoiceBase.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.CFSShipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.CFSLoadList.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Brokerage.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.QuotedBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.OneOffQuotation.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.TransportBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.TransportBookingConsignment.Code))).ToList().ForEach(x => x.Value = true);
			periodicInvoiceBase.LoadJobs();

			Assert("Expecting collection not to contain Job1", !periodicInvoiceBase.Jobs.Contains(Job1));
			Assert("Expecting collection to contain Job2", periodicInvoiceBase.Jobs.Contains(Job2));
			Assert("Expecting collection not to contain Job3", !periodicInvoiceBase.Jobs.Contains(Job3));

			filter.Property = JobHeaderStatus.Complete.Code;
			filter.IsActive = true;

			periodicInvoiceBase.LoadJobs();

			Assert("Expecting collection not to contain Job1", !periodicInvoiceBase.Jobs.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !periodicInvoiceBase.Jobs.Contains(Job2));
			Assert("Expecting collection not to contain Job3", !periodicInvoiceBase.Jobs.Contains(Job3));
		}

		public void TestJobTaxBranchQuery()
		{
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var branch3 = Factory.NewWithValidTestData<GlbBranch>();

			Job1.JH_GB_TaxBranch = branch1.PK;
			Job2.JH_GB_TaxBranch = branch2.PK;

			var charge1 = Factory.NewWithValidTestData<JobCharge>();
			var charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_JH = Job1.PK;
			charge2.JR_JH = Job2.PK;
			charge1.JR_OSSellAmt = charge2.JR_OSSellAmt = 10m;
			charge1.JR_LocalSellAmt = charge2.JR_LocalSellAmt = 10m;
			charge1.JR_RX_NKSellCurrency = charge2.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge1.JR_OH_SellAccount = charge2.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_InvoiceType = charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			var periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			var periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			}

			Factory.Save();

			var filter = (ModuleGuidFilter)periodicInvoiceBase.JobsFilter["Job Header Tax Branch"];

			filter.Property = branch1.PK;
			filter.IsActive = true;

			periodicInvoiceBase.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.CFSShipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.CFSLoadList.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Brokerage.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.QuotedBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.OneOffQuotation.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.TransportBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.TransportBookingConsignment.Code))).ToList().ForEach(x => x.Value = true);
			periodicInvoiceBase.LoadJobs();

			Assert("Expecting collection to contain Job1", periodicInvoiceBase.Jobs.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !periodicInvoiceBase.Jobs.Contains(Job2));
			Assert("Expecting collection not to contain Job3", !periodicInvoiceBase.Jobs.Contains(Job3));

			filter.Property = branch3.PK;
			filter.IsActive = true;

			periodicInvoiceBase.LoadJobs();

			Assert("Expecting collection not to contain Job1", !periodicInvoiceBase.Jobs.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !periodicInvoiceBase.Jobs.Contains(Job2));
			Assert("Expecting collection not to contain Job3", !periodicInvoiceBase.Jobs.Contains(Job3));
		}

		#endregion

		#region Job Type

		public void TestFilterByJobType_ShipmentType()
		{
			Factory.Save();

			SetJobsToTest(JobInvoicingConsumerTypes.Shipment, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK);

			ForwardingShipment shipment = Factory.NewWithValidTestData(typeof(ForwardingShipment)) as ForwardingShipment;
			shipment.JS_IsForwardRegistered = ZBool.True;

			ForwardingShipment cFSShipment = Factory.NewWithValidTestData(typeof(ForwardingShipment)) as ForwardingShipment;
			cFSShipment.JS_IsForwardRegistered = ZBool.False;
			cFSShipment.JS_IsCFSRegistered = ZBool.True;

			this.JobToBeIncluded.JH_ParentID = shipment.PK;
			this.JobToBeExcluded.JH_ParentID = cFSShipment.PK;
			var charge1 = TestObjectCreator.CreateCharge(this.JobToBeIncluded, TestObjectCreator.CC1, 100m, 100m);
			var charge2 = TestObjectCreator.CreateCharge(this.JobToBeExcluded, TestObjectCreator.CC1, 100m, 100m);
			charge1.JR_OH_SellAccount = charge2.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_InvoiceType = charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			Factory.Save();

			AssertJobTypeFilterResult(JobInvoicingConsumerTypes.Shipment.Code);
		}

		protected void AssertJobTypeFilterResult(ZString jobType)
		{
			var periodicInvoiceBase = CreatePeriodicInvoice();
			var periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			if (periodicInvoice != null)
			{
				periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			}

			periodicInvoiceBase.JobTypeList.Where(x => (x.Description.Contains(jobType))).ToList().ForEach(x => x.Value = true);
			periodicInvoiceBase.LoadJobs();

			AssertEquals("There should be 1 matching transaction", 1, periodicInvoiceBase.Jobs.Count);
			Assert(periodicInvoiceBase.Jobs.Contains(JobToBeIncluded));
			Assert(!periodicInvoiceBase.Jobs.Contains(JobToBeExcluded));
		}

		void SetJobsToTest(JobInvoicingConsumerType jobType, ZGuid companyPK, ZGuid branchPK)
		{
			JobToBeIncluded = CreateJob(jobType, companyPK, branchPK);
			JobToBeExcluded = CreateJob(JobInvoicingConsumerTypes.WorkItem, companyPK, branchPK);
		}

		protected Job CreateJob(JobInvoicingConsumerType jobType, ZGuid companyPK, ZGuid branchPK)
		{
			IJobInvoicingPlugIn plugin = null;
			switch (jobType.Code)
			{
				case "QSH":
					plugin = QuotedBooking.CreateNewBooking(Factory);
					break;
				default:
					plugin = TestObjectCreator.CreateJobPlugIn(jobType);
					break;
			}

			var job = TestObjectCreator.CreateJob(plugin, false);
			job.JH_GC = companyPK;
			job.JH_GB = branchPK;
			job.JH_A_JOP = ZDateTime.Today;

			return job;
		}

		public void TestFilterByJobType_CFSShipmentType()
		{
			Factory.Save();

			SetJobsToTest(JobInvoicingConsumerTypes.CFSShipment, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK);

			ForwardingShipment shipment = Factory.NewWithValidTestData(typeof(ForwardingShipment)) as ForwardingShipment;
			shipment.JS_IsForwardRegistered = ZBool.True;

			ForwardingShipment cFSShipment = Factory.NewWithValidTestData(typeof(ForwardingShipment)) as ForwardingShipment;
			cFSShipment.JS_IsForwardRegistered = ZBool.False;
			cFSShipment.JS_IsCFSRegistered = ZBool.True;

			JobToBeExcluded.JH_ParentID = shipment.PK;
			JobToBeIncluded.JH_ParentID = cFSShipment.PK;
			var charge1 = TestObjectCreator.CreateCharge(this.JobToBeIncluded, TestObjectCreator.CC1, 100m, 100m);
			var charge2 = TestObjectCreator.CreateCharge(this.JobToBeExcluded, TestObjectCreator.CC1, 100m, 100m);
			charge1.JR_OH_SellAccount = charge2.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_InvoiceType = charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			Factory.Save();

			AssertJobTypeFilterResult(JobInvoicingConsumerTypes.CFSShipment.Code);
		}

		public void TestPeriodicInvoiceForQuickBookingFilterByJobType()
		{
			IJobInvoicingPlugIn plugin = null;
			var booking = QuotedBooking.CreateNewBooking(Factory);
			plugin = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			var job = TestObjectCreator.CreateJob(plugin, false);
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_A_JOP = ZDateTime.Today;

			booking.JS_IsForwardRegistered = ZBool.False;
			booking.JS_IsCFSRegistered = ZBool.True;

			job.JH_ParentID = booking.PK;
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100m, 100m);
			charge.JR_OH_SellAccount =  TestObjectCreator.AALSHI.PK;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			Factory.Save();

			var periodicInvoiceBase = CreatePeriodicInvoice();
			var periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			if (periodicInvoice != null)
			{
				periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			}

			periodicInvoiceBase.JobTypeList.Where(x =>  x.Description.Contains(JobInvoicingConsumerTypes.QuotedBooking.Code)).ToList().ForEach(x => x.Value = true);
			periodicInvoiceBase.LoadJobs();

			AssertEquals("There should be 1 matching transaction", 1, periodicInvoiceBase.Jobs.Count);
			Assert(periodicInvoiceBase.Jobs.Contains(job));

			periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			if (periodicInvoice != null)
			{
				periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			}
			periodicInvoiceBase.JobTypeList.Where(x => x.Description.Contains(JobInvoicingConsumerTypes.CFSShipment.Code)).ToList().ForEach(x => x.Value = true);
			periodicInvoiceBase.LoadJobs();

			AssertEquals("There should be 0 matching transaction", 0, periodicInvoiceBase.Jobs.Count);
			Assert(!periodicInvoiceBase.Jobs.Contains(job));
		}

		public void TestFilterJobTypeByBrokerageType()
		{
			Factory.Save();

			SetJobsToTest(JobInvoicingConsumerTypes.Brokerage, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK);

			JobToBeExcluded.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			JobToBeIncluded.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			var charge1 = TestObjectCreator.CreateCharge(this.JobToBeIncluded, TestObjectCreator.CC1, 100m, 100m);
			var charge2 = TestObjectCreator.CreateCharge(this.JobToBeExcluded, TestObjectCreator.CC1, 100m, 100m);
			charge1.JR_OH_SellAccount = charge2.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_InvoiceType = charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			Factory.Save();

			AssertJobTypeFilterResult(JobInvoicingConsumerTypes.Brokerage.Code);
		}

		public void TestFilterJobTypeByLocalTransport()
		{
			Factory.Save();

			SetJobsToTest(JobInvoicingConsumerTypes.LocalCartage, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK);

			JobToBeExcluded.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			JobToBeIncluded.JH_ParentTableCode = JobCartageSchema.Constants.Prefix;
			var charge1 = TestObjectCreator.CreateCharge(this.JobToBeIncluded, TestObjectCreator.CC1, 100m, 100m);
			var charge2 = TestObjectCreator.CreateCharge(this.JobToBeExcluded, TestObjectCreator.CC1, 100m, 100m);
			charge1.JR_OH_SellAccount = charge2.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_InvoiceType = charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			Factory.Save();

			AssertJobTypeFilterResult(JobInvoicingConsumerTypes.LocalCartage.Code);
		}

		public void TestFilterJobTypeByISFModule()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedStates;

			SetJobsToTest(JobInvoicingConsumerTypes.ImporterSecurityFiling, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK);

			JobToBeExcluded.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			var charge1 = TestObjectCreator.CreateCharge(this.JobToBeIncluded, TestObjectCreator.CC1, 100m, 100m);
			var charge2 = TestObjectCreator.CreateCharge(this.JobToBeExcluded, TestObjectCreator.CC1, 100m, 100m);
			charge1.JR_OH_SellAccount = charge2.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_InvoiceType = charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			Factory.Save();

			AssertJobTypeFilterResult(JobInvoicingConsumerTypes.ImporterSecurityFiling.Code);
		}

		#endregion

		#region Dates Filters Tests

		public void TestETAFilter()
		{
			ForwardingConsol consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.MostInterestingTransportForBinding[0].JW_ETA = new ZDateTime(2011, 03, 15);
			ForwardingShipment shipment1 = consol1.Shipments.AddNew();
			Job1.JH_ParentID = shipment1.PK;

			ForwardingConsol consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.MostInterestingTransportForBinding[0].JW_ETA = new ZDateTime(2011, 04, 20);
			ForwardingShipment shipment2 = consol2.Shipments.AddNew();
			Job2.JH_ParentID = shipment2.PK;

			JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_JH = Job1.PK;
			charge2.JR_JH = Job2.PK;
			charge1.JR_OSSellAmt = charge2.JR_OSSellAmt = 10m;
			charge1.JR_LocalSellAmt = charge2.JR_LocalSellAmt = 10m;
			charge1.JR_RX_NKSellCurrency = charge2.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge1.JR_OH_SellAccount = charge2.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_InvoiceType = charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			PeriodicInvoiceBase periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			PeriodicInvoice periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			}

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)periodicInvoiceBase.JobsFilter["ETA"];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2011, 01, 01);
			filter.Property2 = new ZDateTime(2011, 04, 01);
			filter.IsActive = true;

			periodicInvoiceBase.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.CFSShipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.CFSLoadList.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Brokerage.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.QuotedBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.OneOffQuotation.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.LocalCartage.Code))).ToList().ForEach(x => x.Value = true);
			periodicInvoiceBase.LoadJobs();

			AssertEquals("Expecting collection to contain Job1", true, periodicInvoiceBase.Jobs.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2", false, periodicInvoiceBase.Jobs.Contains(Job2));
		}

		public void TestETDFilter()
		{
			ForwardingConsol consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.MostInterestingTransportForBinding[0].JW_ETD = new ZDateTime(2011, 03, 15);
			ForwardingShipment shipment1 = consol1.Shipments.AddNew();
			Job1.JH_ParentID = shipment1.PK;

			ForwardingConsol consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.MostInterestingTransportForBinding[0].JW_ETD = new ZDateTime(2011, 04, 20);
			ForwardingShipment shipment2 = consol2.Shipments.AddNew();
			Job2.JH_ParentID = shipment2.PK;

			JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_JH = Job1.PK;
			charge2.JR_JH = Job2.PK;
			charge1.JR_OSSellAmt = charge2.JR_OSSellAmt = 10m;
			charge1.JR_LocalSellAmt = charge2.JR_LocalSellAmt = 10m;
			charge1.JR_RX_NKSellCurrency = charge2.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge1.JR_OH_SellAccount = charge2.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_InvoiceType = charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			PeriodicInvoiceBase periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			PeriodicInvoice periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			}

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)periodicInvoiceBase.JobsFilter["ETD"];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2011, 01, 01);
			filter.Property2 = new ZDateTime(2011, 04, 01);
			filter.IsActive = true;

			periodicInvoiceBase.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.CFSShipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.CFSLoadList.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Brokerage.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.QuotedBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.OneOffQuotation.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.LocalCartage.Code))).ToList().ForEach(x => x.Value = true);
			periodicInvoiceBase.LoadJobs();

			AssertEquals("Expecting collection to contain Job1", true, periodicInvoiceBase.Jobs.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2", false, periodicInvoiceBase.Jobs.Contains(Job2));
		}

		public void TestATAFilter()
		{
			ForwardingConsol consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.MostInterestingTransportForBinding[0].JW_ATA = new ZDateTime(2011, 03, 15);
			ForwardingShipment shipment1 = consol1.Shipments.AddNew();
			Job1.JH_ParentID = shipment1.PK;

			ForwardingConsol consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.MostInterestingTransportForBinding[0].JW_ATA = new ZDateTime(2011, 04, 20);
			ForwardingShipment shipment2 = consol2.Shipments.AddNew();
			Job2.JH_ParentID = shipment2.PK;

			JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_JH = Job1.PK;
			charge2.JR_JH = Job2.PK;
			charge1.JR_OSSellAmt = charge2.JR_OSSellAmt = 10m;
			charge1.JR_LocalSellAmt = charge2.JR_LocalSellAmt = 10m;
			charge1.JR_RX_NKSellCurrency = charge2.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge1.JR_OH_SellAccount = charge2.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_InvoiceType = charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			PeriodicInvoiceBase periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			PeriodicInvoice periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			}

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)periodicInvoiceBase.JobsFilter["ATA"];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2011, 01, 01);
			filter.Property2 = new ZDateTime(2011, 04, 01);
			filter.IsActive = true;

			periodicInvoiceBase.JobTypeList.ToList().ForEach(x => x.Value = true);

			periodicInvoiceBase.LoadJobs();

			AssertEquals("Expecting collection to contain Job1", true, periodicInvoiceBase.Jobs.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2", false, periodicInvoiceBase.Jobs.Contains(Job2));
		}

		public void TestATDFilter()
		{
			ForwardingConsol consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.MostInterestingTransportForBinding[0].JW_ATD = new ZDateTime(2011, 03, 15);
			ForwardingShipment shipment1 = consol1.Shipments.AddNew();
			Job1.JH_ParentID = shipment1.PK;

			ForwardingConsol consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.MostInterestingTransportForBinding[0].JW_ATD = new ZDateTime(2011, 04, 20);
			ForwardingShipment shipment2 = consol2.Shipments.AddNew();
			Job2.JH_ParentID = shipment2.PK;

			JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_JH = Job1.PK;
			charge2.JR_JH = Job2.PK;
			charge1.JR_OSSellAmt = charge2.JR_OSSellAmt = 10m;
			charge1.JR_LocalSellAmt = charge2.JR_LocalSellAmt = 10m;
			charge1.JR_RX_NKSellCurrency = charge2.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge1.JR_OH_SellAccount = charge2.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_InvoiceType = charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			PeriodicInvoiceBase periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			PeriodicInvoice periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			}

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)periodicInvoiceBase.JobsFilter["ATD"];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2011, 01, 01);
			filter.Property2 = new ZDateTime(2011, 04, 01);
			filter.IsActive = true;

			periodicInvoiceBase.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.CFSShipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.CFSLoadList.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Brokerage.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.QuotedBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.OneOffQuotation.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.LocalCartage.Code))).ToList().ForEach(x => x.Value = true);
			periodicInvoiceBase.LoadJobs();

			AssertEquals("Expecting collection to contain Job1", true, periodicInvoiceBase.Jobs.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2", false, periodicInvoiceBase.Jobs.Contains(Job2));
		}

		public void TestDelivevryDateFilter()
		{
			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(2011, 01, 05);
			Job1.JH_ParentID = shipment1.PK;

			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(2011, 02, 18);
			Job2.JH_ParentID = shipment2.PK;

			JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_JH = Job1.PK;
			charge2.JR_JH = Job2.PK;
			charge1.JR_OSSellAmt = charge2.JR_OSSellAmt = 10m;
			charge1.JR_LocalSellAmt = charge2.JR_LocalSellAmt = 10m;
			charge1.JR_RX_NKSellCurrency = charge2.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge1.JR_OH_SellAccount = charge2.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_InvoiceType = charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			PeriodicInvoiceBase periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			PeriodicInvoice periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			}

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)periodicInvoiceBase.JobsFilter["Delivery Date"];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2011, 01, 01);
			filter.Property2 = new ZDateTime(2011, 01, 31);
			filter.IsActive = true;

			periodicInvoiceBase.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.Brokerage.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.QuotedBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.OneOffQuotation.Code))).ToList().ForEach(x => x.Value = true);

			periodicInvoiceBase.LoadJobs();

			AssertEquals("Expecting collection to contain Job1", true, periodicInvoiceBase.Jobs.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2", false, periodicInvoiceBase.Jobs.Contains(Job2));
		}

		public void TestPickupDate()
		{
			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.DocsAndCartage.JP_EstimatedPickup = new ZDateTime(2011, 01, 05);
			Job1.JH_ParentID = shipment1.PK;

			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.DocsAndCartage.JP_EstimatedPickup = new ZDateTime(2011, 02, 18);
			Job2.JH_ParentID = shipment2.PK;

			JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_JH = Job1.PK;
			charge2.JR_JH = Job2.PK;
			charge1.JR_OSSellAmt = charge2.JR_OSSellAmt = 10m;
			charge1.JR_LocalSellAmt = charge2.JR_LocalSellAmt = 10m;
			charge1.JR_RX_NKSellCurrency = charge2.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge1.JR_OH_SellAccount = charge2.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_InvoiceType = charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			PeriodicInvoiceBase periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			PeriodicInvoice periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			}

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)periodicInvoiceBase.JobsFilter["Pickup Date"];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2011, 01, 01);
			filter.Property2 = new ZDateTime(2011, 01, 31);
			filter.IsActive = true;

			periodicInvoiceBase.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.Brokerage.Code)
										|| x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)
										|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
										|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBooking.Code)
										|| x.Description.Contains(JobInvoicingConsumerTypes.QuotedBooking.Code)
										|| x.Description.Contains(JobInvoicingConsumerTypes.OneOffQuotation.Code))).ToList().ForEach(x => x.Value = true);
			periodicInvoiceBase.LoadJobs();

			AssertEquals("Expecting collection to contain Job1", true, periodicInvoiceBase.Jobs.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2", false, periodicInvoiceBase.Jobs.Contains(Job2));
		}

		public void TestAWBIssueDate()
		{
			ForwardingConsol consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.JK_MasterBillIssueDate = new ZDateTime(2011, 02, 02);
			ForwardingShipment shipment1 = consol1.Shipments.AddNew();
			Job1.JH_ParentID = shipment1.PK;

			ForwardingConsol consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.JK_MasterBillIssueDate = new ZDateTime(2011, 04, 20);
			ForwardingShipment shipment2 = consol2.Shipments.AddNew();
			Job2.JH_ParentID = shipment2.PK;

			JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_JH = Job1.PK;
			charge2.JR_JH = Job2.PK;
			charge1.JR_OSSellAmt = charge2.JR_OSSellAmt = 10m;
			charge1.JR_LocalSellAmt = charge2.JR_LocalSellAmt = 10m;
			charge1.JR_RX_NKSellCurrency = charge2.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge1.JR_OH_SellAccount = charge2.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_InvoiceType = charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			PeriodicInvoiceBase periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			PeriodicInvoice periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			}

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)periodicInvoiceBase.JobsFilter["AWB Issue Date"];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2011, 01, 01);
			filter.Property2 = new ZDateTime(2011, 04, 01);
			filter.IsActive = true;

			periodicInvoiceBase.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.CFSShipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.CFSLoadList.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.QuotedBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.OneOffQuotation.Code))).ToList().ForEach(x => x.Value = true);

			periodicInvoiceBase.LoadJobs();

			AssertEquals("Expecting collection to contain Job1", true, periodicInvoiceBase.Jobs.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2", false, periodicInvoiceBase.Jobs.Contains(Job2));
		}

		[TestDate(2017, 01, 01)]
		public void TestCustomsClearanceDate()
		{
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00002001");
			ForwardingShipment shipment2 = TestObjectCreator.CreateShipment("S00002002");
			Job1 = TestObjectCreator.CreateJob(shipment);
			Job2 = TestObjectCreator.CreateJob(shipment2);

			shipment.Logs.AddNew(Events.CustomsCleared, new ZDateTimeOffset(2011, 01, 05));
			shipment2.Logs.AddNew(Events.CustomsCleared, new ZDateTimeOffset(2008, 01, 01));

			JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_JH = Job1.PK;
			charge2.JR_JH = Job2.PK;
			charge1.JR_OSSellAmt = charge2.JR_OSSellAmt = 10m;
			charge1.JR_LocalSellAmt = charge2.JR_LocalSellAmt = 10m;
			charge1.JR_RX_NKSellCurrency = charge2.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge1.JR_OH_SellAccount = charge2.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_InvoiceType = charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			PeriodicInvoiceBase periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			PeriodicInvoice periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			}

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)periodicInvoiceBase.JobsFilter["Customs Clearance Date"];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2011, 01, 01);
			filter.Property2 = new ZDateTime(2011, 04, 01);
			filter.IsActive = true;

			periodicInvoiceBase.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.CFSShipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.CFSLoadList.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Brokerage.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.QuotedBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.OneOffQuotation.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.LocalCartage.Code))).ToList().ForEach(x => x.Value = true);
			periodicInvoiceBase.LoadJobs();

			AssertEquals("Expecting collection to contain Job1", true, periodicInvoiceBase.Jobs.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2", false, periodicInvoiceBase.Jobs.Contains(Job2));
		}

		[TestDate(2011, 05, 19)]
		public void TestCompletionDate()
		{
			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			CommonCartage cartage1 = Factory.NewWithValidTestData<CommonCartage>();
			cartage1.JJ_A_JCL = new DateTime(2011, 02, 01);
			cartage1.JJ_ParentID = shipment1.PK;
			cartage1.JJ_ParentTableCode = shipment1.TablePrefix;
			cartage1.JJ_ConsignmentID = shipment1.JS_UniqueConsignRef + "/I";
			Job1.JH_ParentID = cartage1.PK;
			Job1.JH_ParentTableCode = JobCartageSchema.Constants.Prefix;

			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			CommonCartage cartage2 = Factory.NewWithValidTestData<CommonCartage>();
			cartage2.JJ_A_JCL = new DateTime(2009, 01, 01);
			cartage2.JJ_ParentID = shipment2.PK;
			cartage2.JJ_ParentTableCode = shipment2.TablePrefix;
			cartage2.JJ_ConsignmentID = shipment2.JS_UniqueConsignRef + "/I";
			Job2.JH_ParentID = cartage2.PK;
			Job2.JH_ParentTableCode = JobCartageSchema.Constants.Prefix;

			JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_JH = Job1.PK;
			charge2.JR_JH = Job2.PK;
			charge1.JR_OSSellAmt = charge2.JR_OSSellAmt = 10m;
			charge1.JR_LocalSellAmt = charge2.JR_LocalSellAmt = 10m;
			charge1.JR_RX_NKSellCurrency = charge2.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge1.JR_OH_SellAccount = charge2.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_InvoiceType = charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			PeriodicInvoiceBase periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			PeriodicInvoice periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			}

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)periodicInvoiceBase.JobsFilter["Completion Date"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2011, 01, 01);
			filter.Property2 = new ZDateTime(2011, 04, 01);
			filter.IsActive = true;

			periodicInvoiceBase.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.CFSShipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.CFSLoadList.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Brokerage.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.QuotedBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.OneOffQuotation.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.LocalCartage.Code))).ToList().ForEach(x => x.Value = true);
			periodicInvoiceBase.LoadJobs();

			AssertEquals("Expecting collection to contain Job1", true, periodicInvoiceBase.Jobs.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2", false, periodicInvoiceBase.Jobs.Contains(Job2));
		}

		#endregion

		#region Transport Mode Filters Tests

		public void TestTransportModeFilter()
		{
			ForwardingConsol consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			ForwardingShipment shipment1 = consol1.Shipments.AddNew();
			Job1.JH_ParentID = shipment1.PK;

			ForwardingConsol consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			ForwardingShipment shipment2 = consol2.Shipments.AddNew();
			Job2.JH_ParentID = shipment2.PK;

			JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_JH = Job1.PK;
			charge2.JR_JH = Job2.PK;
			charge1.JR_RX_NKSellCurrency = charge2.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge1.JR_OSSellAmt = charge2.JR_OSSellAmt = 10m;
			charge1.JR_LocalSellAmt = charge2.JR_LocalSellAmt = 10m;
			charge1.JR_OH_SellAccount = charge2.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_InvoiceType = charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			Factory.Save();

			PeriodicInvoiceBase periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			PeriodicInvoice periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			}

			ModuleTextFilter filter = (ModuleTextFilter)periodicInvoiceBase.JobsFilter["Transport Mode"];
			filter.Property = Enterprise.Core.Constants.TransportModes.Air;
			filter.IsActive = true;

			periodicInvoiceBase.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.CFSShipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.CFSLoadList.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Brokerage.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.QuotedBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.OneOffQuotation.Code))).ToList().ForEach(x => x.Value = true);

			periodicInvoiceBase.LoadJobs();

			AssertEquals("Expecting collection to contain Job1", true, periodicInvoiceBase.Jobs.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2", false, periodicInvoiceBase.Jobs.Contains(Job2));
		}

		#endregion

		#region Service Direction Filter Test

		public void TestServiceDirectionFilter_Forwarding()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C001");
			var shipment = TestObjectCreator.CreateShipment("S001", "AUSYD", "USLAX");
			var job = TestObjectCreator.CreateJob(shipment);
			CreateJobCharge(job);

			var shipment1 = TestObjectCreator.CreateShipment("S002", "USLAX", "AUSYD");
			var job1 = TestObjectCreator.CreateJob(shipment1);
			CreateJobCharge(job1);

			var shipment2 = TestObjectCreator.CreateShipment("S003", "USLAX", "NZAKL");
			var job2 = TestObjectCreator.CreateJob(shipment2);
			CreateJobCharge(job2);

			var shipment3 = TestObjectCreator.CreateShipment("S004", "AUSYD", "AUMEL");
			var job3 = TestObjectCreator.CreateJob(shipment3);
			CreateJobCharge(job3);

			Factory.Save();

			PeriodicInvoiceBase periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			PeriodicInvoice periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			}

			AssertDirectionFilter(periodicInvoiceBase, "EXP", JobInvoicingConsumerTypes.Shipment, job);
			AssertDirectionFilter(periodicInvoiceBase, "IMP", JobInvoicingConsumerTypes.Shipment, job1);
			AssertDirectionFilter(periodicInvoiceBase, "CRO", JobInvoicingConsumerTypes.Shipment, job2);
			AssertDirectionFilter(periodicInvoiceBase, "DOM", JobInvoicingConsumerTypes.Shipment, job3);
		}

		public void TestServiceDirectionFilter_Customs()
		{
			var brkJob1 = CreateBRKJob(Customs.Business.JobMessageTypeList.Codes.Import);
			CreateJobCharge(brkJob1);

			var brkJob2 = CreateBRKJob(Customs.Business.JobMessageTypeList.Codes.Export);
			CreateJobCharge(brkJob2);

			var brkJob3 = CreateBRKJob(Customs.Business.JobMessageTypeList.Codes.WarehousedByExternalAgent);
			CreateJobCharge(brkJob3);

			var brkJob4 = CreateBRKJob(Customs.Business.JobMessageTypeList.Codes.ImportDeclarationByExternalBroker);
			CreateJobCharge(brkJob4);

			var brkJob5 = CreateBRKJob(Customs.Business.JobMessageTypeList.Codes.ExportDeclarationByExternalBroker);
			CreateJobCharge(brkJob5);

			Factory.Save();

			PeriodicInvoiceBase periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			PeriodicInvoice periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			}
		}

		public void TestMultipleInstanceOfServiceDirectionFilter()
		{
			var shipment = TestObjectCreator.CreateShipment("S001", "AUSYD", "USLAX");
			var job = TestObjectCreator.CreateJob(shipment);
			CreateJobCharge(job);

			var shipment1 = TestObjectCreator.CreateShipment("S002", "USLAX", "AUSYD");
			var job1 = TestObjectCreator.CreateJob(shipment1);
			CreateJobCharge(job1);

			var shipment2 = TestObjectCreator.CreateShipment("S003", "USLAX", "NZAKL");
			var job2 = TestObjectCreator.CreateJob(shipment2);
			CreateJobCharge(job2);

			Factory.Save();

			PeriodicInvoiceBase periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			PeriodicInvoice periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			}

			ModuleTextFilter filter = (ModuleTextFilter)periodicInvoiceBase.JobsFilter["Service Direction"];
			filter.Property = "EXP";
			filter.IsActive = true;
			filter.OrCategory = FilterOrCategory.Red;

			var filterStrips = new ZArchitecture.Business.Internal.FilterStripCollection(periodicInvoiceBase.JobsFilter.ModuleFilters);
			var newFilterStrip = filterStrips.AddNew(filter.Description);
			var addedFilter = newFilterStrip.CurrentModuleFilter as ModuleTextFilter;
			addedFilter.OrCategory = FilterOrCategory.Red;
			addedFilter.IsActive = true;
			addedFilter.Property = "IMP";

			periodicInvoiceBase.JobTypeList.Where(x => x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)).ToList().ForEach(x => x.Value = true);
			periodicInvoiceBase.LoadJobs();
			foreach (Job jb in new Job[] { job, job1 })
			{
				AssertEquals("Expecting collection to contain " + job.JH_JobNum, true, periodicInvoiceBase.Jobs.Contains(jb));
			}

			filter.Property = "CRO";
			periodicInvoiceBase.LoadJobs();
			foreach (Job jb in new Job[] { job2, job1 })
			{
				AssertEquals("Expecting collection to contain " + job.JH_JobNum, true, periodicInvoiceBase.Jobs.Contains(jb));
			}
		}

		Job CreateBRKJob(ZString messageType)
		{
			var dec = Factory.New<Customs.Business.BaseJobDeclaration>();
			dec.JE_MessageType = messageType;

			var testJob = TestObjectCreator.CreateJob(dec, TestObjectCreator.LocalClient, 5M, TestObjectCreator.Agent, 10M);
			testJob.Parent = dec;
			testJob.JH_GC = Env.CurrentCompanyPK;
			testJob.JH_GB = Env.CurrentBranchPK;

			return testJob;
		}

		void CreateJobCharge(Job job, OrgHeader debtor = null)
		{
			JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_JH = job.PK;
			charge1.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge1.JR_OSSellAmt = 10m;
			charge1.JR_LocalSellAmt = 10m;
			charge1.JR_OH_SellAccount = debtor?.PK ?? TestObjectCreator.AALSHI.PK;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
		}

		void AssertDirectionFilter(PeriodicInvoiceBase periodicInvoiceBase, ZString expectedDirection, JobInvoicingConsumerType jobType, params Job[] expectedJobs)
		{
			ModuleTextFilter filter = (ModuleTextFilter)periodicInvoiceBase.JobsFilter["Service Direction"];
			filter.Property = expectedDirection;
			filter.IsActive = true;
			periodicInvoiceBase.JobTypeList.Where(x => x.Description.Contains(jobType.Code)).ToList().ForEach(x => x.Value = true);
			periodicInvoiceBase.LoadJobs();

			foreach (Job job in expectedJobs)
			{
				AssertEquals("Expecting collection to contain " + job.JH_JobNum, true, periodicInvoiceBase.Jobs.Contains(job));
			}
			AssertEquals("Service Direction : " + expectedDirection, true, periodicInvoiceBase.Jobs.Cast<Periodic_Invoicing.PeriodicInvoiceSelectableJob>().All(x => x.ServiceDirection == expectedDirection));
		}

		#endregion

		public void TestCarrierFilter()
		{
			TestObjectCreator.AALSHI.OH_IsShippingProvider = true;

			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_OA_BookedShippingLineAddress = TestObjectCreator.AALSHI.MainAddress.PK;
			Job1.JH_ParentID = shipment1.PK;
			CreateJobCharge(Job1, TestObjectCreator.AALSHI);

			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment2 = consol2.Shipments.AddNew();
			shipment2.JS_OA_BookedShippingLineAddress = TestObjectCreator.AALSHI.MainAddress.PK;
			shipment2.JS_IsShipping = true;
			Job2.JH_ParentID = shipment2.PK;
			CreateJobCharge(Job2, TestObjectCreator.AALSHI);

			var agencyShipment1 = Factory.NewWithValidTestData<AgencyShipment>();
			agencyShipment1.JS_OA_BookedShippingLineAddress = TestObjectCreator.AALSHI.MainAddress.PK;
			Job3.JH_ParentID = agencyShipment1.PK;
			CreateJobCharge(Job3, TestObjectCreator.AALSHI);

			var agencyShipment2 = Factory.NewWithValidTestData<AgencyShipment>();
			Job4.JH_ParentID = agencyShipment2.PK;
			CreateJobCharge(Job4, TestObjectCreator.AALSHI);

			Factory.Save();

			var periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			PeriodicInvoice periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			}

			periodicInvoiceBase.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.CFSShipment.Code)
												|| x.Description.Contains(JobInvoicingConsumerTypes.CFSLoadList.Code)
												|| x.Description.Contains(JobInvoicingConsumerTypes.Brokerage.Code)
												|| x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)
												|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
												|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBooking.Code)
												|| x.Description.Contains(JobInvoicingConsumerTypes.QuotedBooking.Code)
												|| x.Description.Contains(JobInvoicingConsumerTypes.OneOffQuotation.Code))).ToList().ForEach(x => x.Value = true);

			var filter = (ModuleGuidFilter)periodicInvoiceBase.JobsFilter["Carrier"];
			filter.Property = TestObjectCreator.AALSHI.PK;
			filter.IsActive = true;

			periodicInvoiceBase.LoadJobs();

			AssertEquals("Expecting collection not to contain Job1. Carrier filter has not been applied because it is not an Agency Job", false, periodicInvoiceBase.Jobs.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2. Carrier filter has not been applied because it is not an Agency Job", false, periodicInvoiceBase.Jobs.Contains(Job2));
			AssertEquals("Expecting collection to contain Job3. Carrier filter has been applied", true, periodicInvoiceBase.Jobs.Contains(Job3));
			AssertEquals("Expecting collection not to contain Job4. Carrier filter has been applied ", false, periodicInvoiceBase.Jobs.Contains(Job4));
		}

		public void TestPrincipalFilter()
		{
			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_OH_DeliveryAgent = TestObjectCreator.ABIGAS.PK;
			Job1.JH_ParentID = shipment1.PK;
			CreateJobCharge(Job1, TestObjectCreator.ABIGAS);

			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment2 = consol2.Shipments.AddNew();
			shipment2.JS_OH_DeliveryAgent = TestObjectCreator.ABIGAS.PK;
			shipment2.JS_IsShipping = true;
			Job2.JH_ParentID = shipment2.PK;
			CreateJobCharge(Job2, TestObjectCreator.ABIGAS);

			var agencyShipment1 = Factory.NewWithValidTestData<AgencyShipment>();
			agencyShipment1.JS_OH_DeliveryAgent = TestObjectCreator.ABIGAS.PK;
			Job3.JH_ParentID = agencyShipment1.PK;
			CreateJobCharge(Job3, TestObjectCreator.ABIGAS);

			var agencyShipment2 = Factory.NewWithValidTestData<AgencyShipment>();
			Job4.JH_ParentID = agencyShipment2.PK;
			CreateJobCharge(Job4, TestObjectCreator.ABIGAS);

			Factory.Save();

			var periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			PeriodicInvoice periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.DebtorPK = TestObjectCreator.ABIGAS.PK;
			}

			periodicInvoiceBase.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.CFSShipment.Code)
												|| x.Description.Contains(JobInvoicingConsumerTypes.CFSLoadList.Code)
												|| x.Description.Contains(JobInvoicingConsumerTypes.Brokerage.Code)
												|| x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)
												|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
												|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBooking.Code)
												|| x.Description.Contains(JobInvoicingConsumerTypes.QuotedBooking.Code)
												|| x.Description.Contains(JobInvoicingConsumerTypes.OneOffQuotation.Code))).ToList().ForEach(x => x.Value = true);

			var filter = (ModuleGuidFilter)periodicInvoiceBase.JobsFilter["Principal"];
			filter.Property = TestObjectCreator.ABIGAS.PK;
			filter.IsActive = true;

			periodicInvoiceBase.LoadJobs();

			AssertEquals("Expecting collection not to contain Job1. Principal filter has not been applied because it is not an Agency Job", false, periodicInvoiceBase.Jobs.Contains(Job1));
			AssertEquals("Expecting collection not to contain Job2. Principal filter has not been applied because it is not an Agency Job", false, periodicInvoiceBase.Jobs.Contains(Job2));
			AssertEquals("Expecting collection to contain Job3. Principal filter has been applied", true, periodicInvoiceBase.Jobs.Contains(Job3));
			AssertEquals("Expecting collection not to contain Job4. Principal filter has been applied ", false, periodicInvoiceBase.Jobs.Contains(Job4));
		}

		#region Operations Filters Tests

		public void TestSendingAgentFilters()
		{
			var jobs = PrepareTestDateForSendingAndReceivingAgentFilters();

			periodicInvoiceBase = CreatePeriodicInvoice();
			var periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching;
				periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			}
			periodicInvoiceBase.CurrencyNK = "";

			var filter = (ModuleGuidFilter)periodicInvoiceBase.JobsFilter[PeriodicInvoiceBaseJobFilterBusinessObject.SENDING_AGENT];
			filter.Property = org1.PK;
			filter.IsActive = true;

			periodicInvoiceBase.JobTypeList.ToList().ForEach(x => x.Value = true);
			periodicInvoiceBase.LoadJobs();

			AssertEquals("Expecting collection to contain Job3", true, periodicInvoiceBase.Jobs.Contains(jobs[0]));
			AssertEquals("Expecting collection not to contain Job4", false, periodicInvoiceBase.Jobs.Contains(jobs[1]));
			AssertEquals("Expecting collection to contain Job5", true, periodicInvoiceBase.Jobs.Contains(jobs[2]));
			AssertEquals("Expecting collection not to contain Job6", false, periodicInvoiceBase.Jobs.Contains(jobs[3]));

			filter.Property = org2.PK;
			periodicInvoiceBase.LoadJobs();

			AssertEquals("Expecting collection not to contain Job3", false, periodicInvoiceBase.Jobs.Contains(jobs[0]));
			AssertEquals("Expecting collection to contain Job4", true, periodicInvoiceBase.Jobs.Contains(jobs[1]));
			AssertEquals("Expecting collection not to contain Job5", false, periodicInvoiceBase.Jobs.Contains(jobs[2]));
			AssertEquals("Expecting collection nto contain Job6", true, periodicInvoiceBase.Jobs.Contains(jobs[3]));

			filter.Property = ZGuid.NewZGuid();
			periodicInvoiceBase.LoadJobs();

			AssertEquals("Empty Collection", 0, periodicInvoiceBase.Jobs.Count);
		}

		public void TestReceivingAgentFilters()
		{
			var jobs = PrepareTestDateForSendingAndReceivingAgentFilters();

			periodicInvoiceBase = CreatePeriodicInvoice();
			var periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching;
				periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			}
			periodicInvoiceBase.CurrencyNK = "";

			var filter = (ModuleGuidFilter)periodicInvoiceBase.JobsFilter[PeriodicInvoiceBaseJobFilterBusinessObject.RECEIVING_AGENT];
			filter.Property = org1.PK;
			filter.IsActive = true;

			periodicInvoiceBase.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code) || x.Description.Contains(JobInvoicingConsumerTypes.ForwardingConsol.Code))).ToList().ForEach(x => x.Value = true);
			periodicInvoiceBase.LoadJobs();

			AssertEquals("Expecting collection not to contain Job3", false, periodicInvoiceBase.Jobs.Contains(jobs[0]));
			AssertEquals("Expecting collection to contain Job4", true, periodicInvoiceBase.Jobs.Contains(jobs[1]));
			AssertEquals("Expecting collection not to contain Job5", false, periodicInvoiceBase.Jobs.Contains(jobs[2]));
			AssertEquals("Expecting collection nto contain Job6", true, periodicInvoiceBase.Jobs.Contains(jobs[3]));

			filter.Property = org2.PK;
			periodicInvoiceBase.LoadJobs();

			AssertEquals("Expecting collection to contain Job3", true, periodicInvoiceBase.Jobs.Contains(jobs[0]));
			AssertEquals("Expecting collection not to contain Job4", false, periodicInvoiceBase.Jobs.Contains(jobs[1]));
			AssertEquals("Expecting collection to contain Job5", true, periodicInvoiceBase.Jobs.Contains(jobs[2]));
			AssertEquals("Expecting collection not to contain Job6", false, periodicInvoiceBase.Jobs.Contains(jobs[3]));

			filter.Property = ZGuid.NewZGuid();
			periodicInvoiceBase.LoadJobs();

			AssertEquals("Empty Collection", 0, periodicInvoiceBase.Jobs.Count);
		}

		Job[] PrepareTestDateForSendingAndReceivingAgentFilters()
		{
			org1 = TestObjectCreator.CreateOrgHeader("ORG1", true, true);
			org2 = TestObjectCreator.CreateOrgHeader("ORG2", true, true);

			var chargeCreator = new Action<Job>((job) =>
							{
								JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
								charge1.JR_JH = job.PK;
								charge1.JR_RX_NKSellCurrency = ZString.Empty;
								charge1.JR_OSSellAmt = 10m;
								charge1.JR_LocalSellAmt = 10m;
								charge1.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
								charge1.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching;
							});

			//Consol Job
			var consol1 = TestObjectCreator.CreateConsol("AUSYD", "AUMEL", "C0003");
			consol1.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;
			consol1.JK_OA_ReceivingForwarderAddress = org2.MainAddress.PK;

			var consol2 = TestObjectCreator.CreateConsol("AUMEL", "AUSYD", "C0004");
			consol2.JK_OA_SendingForwarderAddress = org2.MainAddress.PK;
			consol2.JK_OA_ReceivingForwarderAddress = org1.MainAddress.PK;

			var shipment1 = TestObjectCreator.CreateShipment("S0001", consol1);
			var shipment2 = TestObjectCreator.CreateShipment("S0002", consol2);
			var shipment3 = TestObjectCreator.CreateShipment("S0003", consol1);
			var shipment4 = TestObjectCreator.CreateShipment("S0004", consol2);

			var job1 = TestObjectCreator.CreateJob(shipment1);
			chargeCreator(job1);

			var job2 = TestObjectCreator.CreateJob(shipment2);
			chargeCreator(job2);

			var job3 = TestObjectCreator.CreateJob(shipment3);
			chargeCreator(job3);

			var job4 = TestObjectCreator.CreateJob(shipment4);
			chargeCreator(job4);

			Factory.Save();

			return new Job[] { job1, job2, job3, job4 };
		}

		OrgHeader org1, org2;

		#endregion

		#region Implementation

		protected Job Job1;
		protected Job Job2;
		protected Job Job3;
		protected Job Job4;
		Job JobToBeIncluded;
		Job JobToBeExcluded;

		protected JobHeaderCollection FilterCollection;

		protected void BaseLineSetup(Job jobToSave, ZBool saveRevenue, ZBool saveCost, ZBool saveWIP, ZBool saveAccrual)
		{
			if (saveRevenue)
			{
				AccTransactionLines rev1 = Factory.New<AccTransactionLines>();
				SetLine(jobToSave, rev1, TransactionLineTypes.Revenue, 120m);

				AccTransactionLines rev2 = Factory.New<AccTransactionLines>();
				SetLine(jobToSave, rev2, TransactionLineTypes.Revenue, 35m);
			}

			if (saveCost)
			{
				AccTransactionLines cost1 = Factory.New<AccTransactionLines>();
				SetLine(jobToSave, cost1, TransactionLineTypes.Cost, -140m);

				AccTransactionLines cost2 = Factory.New<AccTransactionLines>();
				SetLine(jobToSave, cost2, TransactionLineTypes.Cost, -65m);
			}

			if (saveWIP)
			{
				WIP wIP1 = Factory.New<WIP>();
				wIP1.AL_LineAmount = 135m;
				wIP1.AL_JH = jobToSave.PK;
				wIP1.AL_GE = GlbDepartment.CurrentDepartment.PK;
				wIP1.AL_GB = GlbBranch.CurrentBranch.PK;

				WIP wIP2 = Factory.New<WIP>();
				wIP2.AL_LineAmount = 50m;
				wIP2.AL_JH = jobToSave.PK;
				wIP2.AL_GE = GlbDepartment.CurrentDepartment.PK;
				wIP2.AL_GB = GlbBranch.CurrentBranch.PK;
			}

			if (saveAccrual)
			{
				Accrual accrual1 = Factory.New<Accrual>();
				accrual1.AL_LineAmount = 35m;
				accrual1.AL_JH = jobToSave.PK;
				accrual1.AL_GE = GlbDepartment.CurrentDepartment.PK;
				accrual1.AL_GB = GlbBranch.CurrentBranch.PK;

				Accrual accrual2 = Factory.New<Accrual>();
				accrual2.AL_LineAmount = 55m;
				accrual2.AL_JH = jobToSave.PK;
				accrual2.AL_GE = GlbDepartment.CurrentDepartment.PK;
				accrual2.AL_GB = GlbBranch.CurrentBranch.PK;
			}
		}

		protected void SetLine(Job jobToSave, AccTransactionLines line, ZString lineType, ZDecimal amount)
		{
			line.AL_JH = jobToSave.PK;
			line.AL_LineType = lineType;
			line.AL_LineAmount = amount;
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
		}

		protected override void SetUp()
		{
			base.SetUp();

			ForwardingShipment shipment1 = TestObjectCreator.CreateShipment("S1");
			ForwardingShipment shipment2 = TestObjectCreator.CreateShipment("S2");
			ForwardingShipment shipment3 = TestObjectCreator.CreateShipment("S3");
			ForwardingShipment shipment4 = TestObjectCreator.CreateShipment("S4");

			Job1 = TestObjectCreator.CreateJob(shipment1, false);
			Job2 = TestObjectCreator.CreateJob(shipment2, false);
			Job3 = TestObjectCreator.CreateJob(shipment3, false);
			Job4 = TestObjectCreator.CreateJob(shipment4, false);

			Job1.JH_GB = GlbBranch.CurrentBranch.PK;
			Job2.JH_GB = GlbBranch.CurrentBranch.PK;
			Job3.JH_GB = GlbBranch.CurrentBranch.PK;
			Job4.JH_GB = GlbBranch.CurrentBranch.PK;

			Job1.JH_GC = GlbDepartment.CurrentDepartment.PK;
			Job2.JH_GC = GlbDepartment.CurrentDepartment.PK;
			Job3.JH_GC = GlbDepartment.CurrentDepartment.PK;
			Job4.JH_GC = GlbDepartment.CurrentDepartment.PK;

			Job1.JH_GC = GlbCompany.CurrentCompany.PK;
			Job2.JH_GC = GlbCompany.CurrentCompany.PK;
			Job3.JH_GC = GlbCompany.CurrentCompany.PK;
			Job4.JH_GC = GlbCompany.CurrentCompany.PK;

			FilterCollection = new JobHeaderCollection(Factory);
			GetNewFilterStripBusinessObject();
		}

		PeriodicInvoiceBase periodicInvoiceBase;

		protected abstract PeriodicInvoiceBase CreatePeriodicInvoice();

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = TestObjectCreator.GBP.RX_Code;
			return periodicInvoiceBase.JobsFilter;
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}

		#endregion

		public void TestFilterWorksCorrectlyForSecondTime()
		{
			JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();

			charge1.JR_JH = Job1.PK;
			charge2.JR_JH = Job2.PK;

			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge2.JR_AC = TestObjectCreator.CC2.PK;
			charge1.JR_OSSellAmt = charge2.JR_OSSellAmt = 10m;
			charge1.JR_LocalSellAmt = charge2.JR_LocalSellAmt = 10m;
			charge1.JR_RX_NKSellCurrency = charge2.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge1.JR_OH_SellAccount = charge2.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_InvoiceType = charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			PeriodicInvoiceBase periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = "AUD";
			PeriodicInvoice periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			}

			Factory.Save();

			periodicInvoiceBase.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.CFSShipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.CFSLoadList.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Brokerage.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.QuotedBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.OneOffQuotation.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.LocalCartage.Code))).ToList().ForEach(x => x.Value = true);
			periodicInvoiceBase.LoadJobs();

			AssertEquals("Without any filters, expecting collection to contain Job1", true, periodicInvoiceBase.Jobs.Contains(Job1));
			AssertEquals("Without any filters, expecting collection to contain Job2", true, periodicInvoiceBase.Jobs.Contains(Job2));

			ModuleGuidFilter filter = (ModuleGuidFilter)periodicInvoiceBase.JobsFilter["Charge Code"];

			filter.Property = TestObjectCreator.CC1.PK;
			filter.IsActive = true;

			periodicInvoiceBase.LoadJobs();

			AssertEquals("With Charge Code filter, expecting collection to contain Job1", true, periodicInvoiceBase.Jobs.Contains(Job1));
			AssertEquals("With Charge Code filter, expecting collection not to contain job2", false, periodicInvoiceBase.Jobs.Contains(Job2));

			filter.Property = ZGuid.Empty;
			filter.IsActive = true;

			periodicInvoiceBase.LoadJobs();

			AssertEquals("With Charge Code filter but empty, expecting collection to contain Job1 and Job2", true, periodicInvoiceBase.Jobs.Contains(Job1));
			AssertEquals("With Charge Code filter but empty, expecting collection to contain Job1 and Job2", true, periodicInvoiceBase.Jobs.Contains(Job2));

			filter.Property = TestObjectCreator.CC2.PK;
			filter.IsActive = true;

			periodicInvoiceBase.LoadJobs();

			AssertEquals("With Charge Code filter, expecting collection not to contain Job1", false, periodicInvoiceBase.Jobs.Contains(Job1));
			AssertEquals("With Charge Code filter, expecting collection to contain Job2", true, periodicInvoiceBase.Jobs.Contains(Job2));
		}

		public void TestChargeCollectionWithNonChargeRelatedFilters()
		{
			PeriodicInvoiceBase periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = "AUD";
			PeriodicInvoice periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			}

			var shipment1 = TestObjectCreator.CreateShipment("S0001");
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			var shipment2 = TestObjectCreator.CreateShipment("S0002");
			shipment2.JS_TransportMode = Constants.TransportModes.Sea;
			var job1 = TestObjectCreator.CreateJob(shipment1);
			var job2 = TestObjectCreator.CreateJob(shipment2);
			var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, "Charge1 on Job1", TestObjectCreator.AUD, 100m, TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 100m, TestObjectCreator.AALSHI);
			var charge2 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC3, "Charge2 on Job2", TestObjectCreator.AUD, 100m, TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 200m, TestObjectCreator.AALSHI);
			var charge3 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC5, "Charge3 on Job1", TestObjectCreator.AUD, 100m, TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 200m, TestObjectCreator.AALSHI);
			charge1.JR_OH_SellAccount = charge2.JR_OH_SellAccount = charge3.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			charge1.JR_InvoiceType = charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge3.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)periodicInvoiceBase.JobsFilter["Transport Mode"];

			filter.Property = Constants.TransportModes.Air;
			filter.IsActive = true;
			periodicInvoiceBase.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.AgencyBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.QuotedBooking.Code)
													|| x.Description.Contains(JobInvoicingConsumerTypes.OneOffQuotation.Code))).ToList().ForEach(x => x.Value = true);

			periodicInvoiceBase.LoadJobs();

			AssertEquals("Expecting collection to contain Job1", true, periodicInvoiceBase.Jobs.Contains(job1));
			AssertEquals("Expecting collection not to contain job2", false, periodicInvoiceBase.Jobs.Contains(job2));

			AssertEquals("Expecting Collection to contain charge1 on job1", true, periodicInvoiceBase.Charges.Contains(charge1));
			AssertEquals("Expecting Collection not to contain charge2 on job2", false, periodicInvoiceBase.Charges.Contains(charge2));
			AssertEquals("Expecting Collection not to contain charge3 on job1", false, periodicInvoiceBase.Charges.Contains(charge3));
		}

		public void TestAllModuleFiltersHaveSubGroups()
		{
			var filterStrip = GetNewFilterStripBusinessObject();
			CombineAssertions("All filters should have SubGroup. But following filters don't -->", () => filterStrip.ModuleFilters.ToList().ForEach(f => Assert(f.Description, f.SubGroup != null)));
		}

		public void TestAllAuditFiltersAreAddedAndHaveCorrectSubgroup()
		{
			var descriptions = new[]
				{
					FilterDescriptions.CreatedOnWeb,
					FilterDescriptions.CreatedTime,
					FilterDescriptions.CreatingUser,
					FilterDescriptions.LastEditTime,
					FilterDescriptions.LastEditUser
				};

			var filterStrip = GetNewFilterStripBusinessObject();
			CombineAssertions("All Audit filters should be added with JobFilterSubGroup. But following filters don't -->", () => AssertAllAuditFilter());

			void AssertAllAuditFilter()
			{
				foreach (var desc in descriptions)
				{
					var filter = filterStrip.ModuleFilters[desc];
					AssertNotNull(desc, filter);
					AssertEquals(desc, "JobFilterSubGroup", filter.SubGroup.GetType().Name);
				}
			}
		}

		public void TestNoExceptionIsThrownWhenLoadingJobChargesWithActiveAuditFilter()
		{
			var brkJob1 = CreateBRKJob(Customs.Business.JobMessageTypeList.Codes.Import);
			CreateJobCharge(brkJob1);

			var brkJob2 = CreateBRKJob(Customs.Business.JobMessageTypeList.Codes.Export);
			CreateJobCharge(brkJob2);

			Factory.Save();

			var periodicInvoiceBase = CreatePeriodicInvoice();
			periodicInvoiceBase.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			periodicInvoiceBase.JobTypeList.First(x => x.Description.Contains(JobInvoicingConsumerTypes.Brokerage.Code)).Value = true;

			var periodicInvoice = periodicInvoiceBase as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			}

			var filter = (ModuleDateFilter)periodicInvoiceBase.JobsFilter[FilterDescriptions.CreatedTime];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = DateTime.Today.AddDays(-1);
			filter.Property2 = DateTime.Today.AddDays(1);
			filter.IsActive = true;

			AssertNoExceptionThrown("No error should be thrown when one of the audit Filters is used for loading JobCharges", () => periodicInvoiceBase.LoadJobs());

			foreach (Job job in new[] { brkJob1, brkJob2 })
			{
				AssertEquals("Expecting collection to contain " + job.JH_JobNum, true, periodicInvoiceBase.Jobs.Contains(job));
			}
		}
	}
}
