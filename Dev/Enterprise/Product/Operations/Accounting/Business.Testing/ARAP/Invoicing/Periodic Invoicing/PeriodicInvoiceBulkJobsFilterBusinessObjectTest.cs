using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(PeriodicInvoiceBulkJobsFilterBusinessObject))]
	public class PeriodicInvoiceBulkJobsFilterBusinessObjectTest : PeriodicInvoiceBaseJobsFilterBusinessObjectTest
	{
		public override void TestFiltersSubGroups()
		{
			FilterStripBusinessObject filterBO = GetNewFilterStripBusinessObject();
			ZString[] filterNotRequiredOrAlreadyAppliedForSinglePeriodicInvoiceSubGroupFilters = { "Currency", "Debtor", "Invoice Type", "AR Settlement Group", "Organization Branch" };
			ZString[] jobChargeSubGroupFilters = { "Charge Line Branch", "Charge Line Department", "Charge Code" };
			ZString[] jobSubGroupFilters = { "Job Status", "Job Open Date", "Job Local Client", "Job Header Branch", "Job Header Department", "Job Header Tax Branch" };
			ZString[] miscSubGroupFilters = { "ETA", "ETD", "ATA", "ATD", "Delivery Date", "Pickup Date", "Customs Clerance Date", "AWB Issue Date", "Completion Date", "Transport Mode" };

			foreach (ModuleFilter filter in filterBO.ModuleFilters)
			{
				if (filterNotRequiredOrAlreadyAppliedForSinglePeriodicInvoiceSubGroupFilters.Contains(filter.Description))
				{
					AssertEquals(string.Format("SpecialChargeFilterSubGroup for filter {0}", filter.Description), "SpecialChargeFilterSubGroup", filter.SubGroup.GetType().Name);
				}
				if (jobChargeSubGroupFilters.Contains(filter.Description))
				{
					AssertEquals(string.Format("JobChargeFilterSubGroup for filter {0}", filter.Description), "JobChargeFilterSubGroup", filter.SubGroup.GetType().Name);
				}
				if (jobSubGroupFilters.Contains(filter.Description))
				{
					AssertEquals(string.Format("JobFilterSubGroup for filter {0}", filter.Description), "JobFilterSubGroup", filter.SubGroup.GetType().Name);
				}
				if (miscSubGroupFilters.Contains(filter.Description))
				{
					AssertEquals(string.Format("MiscFilterSubGroup for filter {0}", filter.Description), "MiscFilterSubGroup", filter.SubGroup.GetType().Name);
				}
			}
		}

		protected override void AssertFilterCount()
		{
			FilterStripBusinessObject filterBO = GetNewFilterStripBusinessObject();

			int count = 0;

			foreach (ModuleFilter filter in filterBO.ModuleFilters)
			{
				if (filter.Description == "Currency" || filter.Description == "Current Company" || filter.Description == "Tax Branch" || filter.Description == "Transactions Eligible for Posting" || filter.Description == "Deferred Transactions")
				{
					AssertEquals(string.Format("For Periodic Invoice, {0} filter is always hidden", filter.Description), FilterVisibility.AlwaysAppliedAndHidden, filter.Visibility);
				}
				else
				{
					AssertEquals(string.Format("For Periodic Invoice, {0} filter is always visible", filter.Description), FilterVisibility.Visible, filter.Visibility);
				}

				count++;
			}

			AssertEquals("There should be 42 including both visible and invisible filters)", 42, count);
		}

		public void TestChargeCodeFilter_FiltersChargeAmount()
		{
			var shipment = TestObjectCreator.CreateShipment("S000101");
			var job = TestObjectCreator.CreateJob(shipment, false);

			var jobCharge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "CC1", TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 120M, TestObjectCreator.ABIGAS);
			jobCharge1.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			var jobCharge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "CC1", TestObjectCreator.AUD, 200M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 240M, TestObjectCreator.ABIGAS);
			jobCharge2.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			var jobCharge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "CC2", TestObjectCreator.AUD, 300M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 360M, TestObjectCreator.ABIGAS);
			jobCharge3.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge3.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			Factory.Save();

			PeriodicInvoiceBulk periodicInvoiceBulk = new PeriodicInvoiceBulk(Factory);

			periodicInvoiceBulk.CurrencyNK = "AUD";

			ModuleGuidFilter filter = (ModuleGuidFilter)periodicInvoiceBulk.JobsFilter["Charge Code"];
			filter.Property = TestObjectCreator.CC1.PK;
			filter.IsActive = true;

			AssertEquals("Pre-state:", 0M, periodicInvoiceBulk.OSExTaxAmount);
			AssertEquals("Pre-state:", 0M, periodicInvoiceBulk.OSTotalAmount);

			periodicInvoiceBulk.LoadJobs();

			AssertEquals("Expecting collection to contain Job", true, periodicInvoiceBulk.Jobs.Contains(job));
			AssertEquals("The amount in the filtered list should only include CC1 charges", 360M, periodicInvoiceBulk.OSExTaxAmount);
			AssertEquals("The total amount should respect CC1 charge from the filter", 360M, periodicInvoiceBulk.OSTotalAmount);
		}

		protected override PeriodicInvoiceBase CreatePeriodicInvoice()
		{
			return new PeriodicInvoiceBulk(Factory);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new PeriodicInvoiceBulkJobsFilterBusinessObject();
		}
	}
}
