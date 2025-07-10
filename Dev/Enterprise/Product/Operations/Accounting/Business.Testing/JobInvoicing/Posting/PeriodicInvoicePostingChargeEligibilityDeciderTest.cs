using System;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting.Testing
{
	public class PeriodicInvoicePostingChargeEligibilityDeciderTest : PostingChargeEligibilityDeciderTest
	{
		protected override PostingChargeEligibilityDecider GetEligibilityDecider(Charge[] charges)
		{
			var predicate = new Predicate<Charge>(x => x.InvoicingJob != null && x.InvoicingJob.ConsumerTypeShouldCreateWIP(x.JR_InvoiceType));
			return new PeriodicInvoicePostingChargeEligibilityDecider(charges, predicate);
		}

		protected override void SetUp()
		{
			base.SetUp();

			OrgInvoiceType orgInvoiceType = LocalClient.CompanyData.InvoiceTypes.AddNew();
			orgInvoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			orgInvoiceType.DeferredCharges.AddNew().PO_AC = MarginChargeCode.PK;
			orgInvoiceType.DeferredCharges.AddNew().PO_AC = DisbursementChargeCode.PK;
			orgInvoiceType.DeferredCharges.AddNew().PO_AC = RevenueChargeCode.PK;

			orgInvoiceType = LocalClient.CompanyData.InvoiceTypes.AddNew();
			orgInvoiceType.PI_Module = JobInvoicingConsumerTypes.Brokerage.Code;
			orgInvoiceType.DeferredCharges.AddNew().PO_AC = MarginChargeCode.PK;
			orgInvoiceType.DeferredCharges.AddNew().PO_AC = DisbursementChargeCode.PK;
			orgInvoiceType.DeferredCharges.AddNew().PO_AC = RevenueChargeCode.PK;

			orgInvoiceType = Agent.CompanyData.InvoiceTypes.AddNew();
			orgInvoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			orgInvoiceType.DeferredCharges.AddNew().PO_AC = MarginChargeCode.PK;
			orgInvoiceType.DeferredCharges.AddNew().PO_AC = DisbursementChargeCode.PK;
			orgInvoiceType.DeferredCharges.AddNew().PO_AC = RevenueChargeCode.PK;

			orgInvoiceType = Agent.CompanyData.InvoiceTypes.AddNew();
			orgInvoiceType.PI_Module = JobInvoicingConsumerTypes.Brokerage.Code;
			orgInvoiceType.DeferredCharges.AddNew().PO_AC = MarginChargeCode.PK;
			orgInvoiceType.DeferredCharges.AddNew().PO_AC = DisbursementChargeCode.PK;
			orgInvoiceType.DeferredCharges.AddNew().PO_AC = RevenueChargeCode.PK;
		}

		public void TestGetEligibleChargesForDeferredCharge()
		{
			SetupEligibleCharges();

			AssertEquals("Precondition: 9 charges on job", 9, TestJob.Charges.Count);

			PostingChargeEligibilityDecider decider = GetEligibilityDecider((Charge[])TestJob.Charges.ToArray(typeof(Charge)));
			IReceivablesPostingChargeCollection results = decider.GetEligibleCharges(JobInvoicingPostingOption.All);
			AssertEquals("All Charges are deferred.", 8, results.Count);

			results = decider.GetEligibleCharges(JobInvoicingPostingOption.Revenue);
			AssertEquals("All Charges are deferred.", 8, results.Count);

			Agent.CompanyData.InvoiceTypes.RemoveAll();
			Charge1.JR_InvoiceType = InvoiceTypeCalculationProvider.ConvertDeferredInvoiceTypeToNonDeferredOne(Charge1.JR_InvoiceType);
			Charge2.JR_InvoiceType = InvoiceTypeCalculationProvider.ConvertDeferredInvoiceTypeToNonDeferredOne(Charge2.JR_InvoiceType);
			Charge3.JR_InvoiceType = InvoiceTypeCalculationProvider.ConvertDeferredInvoiceTypeToNonDeferredOne(Charge3.JR_InvoiceType);
			Charge4.JR_InvoiceType = InvoiceTypeCalculationProvider.ConvertDeferredInvoiceTypeToNonDeferredOne(Charge4.JR_InvoiceType);
			Charge.JR_InvoiceType = InvoiceTypesList.Codes.DoNotPost;

			results = decider.GetEligibleCharges(JobInvoicingPostingOption.All);
			AssertEquals("LocalClient Charges are deferred.", 4, results.Count);

			results = decider.GetEligibleCharges(JobInvoicingPostingOption.Revenue);
			AssertEquals("LocalClient Charges are deferred.", 4, results.Count);
		}

		protected override void AssertEligibilityForChargesDisallowedToView()
		{
			ForwardingShipment shipment = TestHelper.CreateShipment("S00001234");
			TestJob = TestHelper.CreateJob(shipment);
			Charge1 = TestHelper.CreateCharge(TestJob, TestHelper.CC1, "charge 1", TestHelper.AUD, 100m, TestHelper.Creditor1, TestHelper.AUD, 100m, TestHelper.Debtor);
			Charge1.JR_InvoiceType = InvoiceTypeCalculationProvider.ConvertNonDeferredInvoiceTypeToDeferredOne(Charge1.JR_InvoiceType);
			Charge1.JR_GB = TestHelper.NonCurrentBranch.PK;
			Charge1.JR_GE = TestHelper.NonCurrentDepartment.PK;

			Charge2 = TestHelper.CreateCharge(TestJob, TestHelper.CC1, "charge 2", TestHelper.AUD, 200m, TestHelper.Creditor1, TestHelper.AUD, 200m, TestHelper.Debtor);
			Charge2.JR_InvoiceType = InvoiceTypeCalculationProvider.ConvertNonDeferredInvoiceTypeToDeferredOne(Charge2.JR_InvoiceType);
			Charge2.JR_GB = Env.CurrentBranch.PK;
			Charge2.JR_GE = Env.CurrentDepartment.PK;

			Factory.Save();

			AssertEquals("User is disallowed to view charge 1", false, Charge1.IsAllowedToModifyThisCharge);
			AssertEquals("User is allowed to view charge 2", true, Charge2.IsAllowedToModifyThisCharge);

			var decider = GetEligibilityDecider((Charge[])TestJob.Charges.ToArray(typeof(Charge)));
			var results = decider.GetEligibleCharges(JobInvoicingPostingOption.All);

			AssertEquals("Charge 1 is eligible to post", true, results.Contains(Charge1));
			AssertEquals("Charge 2 is eligible to post", true, results.Contains(Charge2));
		}

		protected override bool ShouldChargeBeEligible(Charge charge)
		{
			return InvoiceTypeCalculationProvider.IsDisbursementInvoiceType(charge.JR_InvoiceType) && charge.IsDeferredCharge;
		}

		protected override void SetupEligibleCharges()
		{
			base.SetupEligibleCharges();
			Charge1.JR_InvoiceType = InvoiceTypeCalculationProvider.ConvertNonDeferredInvoiceTypeToDeferredOne(Charge1.JR_InvoiceType);
			Charge2.JR_InvoiceType = InvoiceTypeCalculationProvider.ConvertNonDeferredInvoiceTypeToDeferredOne(Charge2.JR_InvoiceType);
			Charge3.JR_InvoiceType = InvoiceTypeCalculationProvider.ConvertNonDeferredInvoiceTypeToDeferredOne(Charge3.JR_InvoiceType);
			Charge4.JR_InvoiceType = InvoiceTypeCalculationProvider.ConvertNonDeferredInvoiceTypeToDeferredOne(Charge4.JR_InvoiceType);
			Charge5.JR_InvoiceType = InvoiceTypeCalculationProvider.ConvertNonDeferredInvoiceTypeToDeferredOne(Charge5.JR_InvoiceType);
			Charge6.JR_InvoiceType = InvoiceTypeCalculationProvider.ConvertNonDeferredInvoiceTypeToDeferredOne(Charge6.JR_InvoiceType);
			Charge7.JR_InvoiceType = InvoiceTypeCalculationProvider.ConvertNonDeferredInvoiceTypeToDeferredOne(Charge7.JR_InvoiceType);
			Charge8.JR_InvoiceType = InvoiceTypeCalculationProvider.ConvertNonDeferredInvoiceTypeToDeferredOne(Charge8.JR_InvoiceType);
			Charge.JR_InvoiceType = InvoiceTypeCalculationProvider.ConvertNonDeferredInvoiceTypeToDeferredOne(Charge.JR_InvoiceType);
		}
	}
}
