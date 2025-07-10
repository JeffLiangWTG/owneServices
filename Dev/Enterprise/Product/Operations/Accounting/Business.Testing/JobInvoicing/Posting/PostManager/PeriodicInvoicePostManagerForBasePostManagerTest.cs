using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(PeriodicInvoicePostManager))]
	public class PeriodicInvoicePostManagerForBasePostManagerTest : BasePostManagerTest
	{
		protected override void AssertAPPaymentApprovalAmountUpdater(bool hasCriticalErrors) => Assert("AP ledger test is not applicable here.", true);

		#region TestCalculateOtherTaxes

		protected override int GetProcessTaxesOnPosting_CallCount(bool isTaxSystemActivated) => isTaxSystemActivated ? 1 : 0;

		#endregion

		protected override Charge CreateChargeForRounding(Job job)
		{
			var charge = base.CreateChargeForRounding(job);
			charge.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice;

			return charge;
		}

		public new void TestPreventPostingAPCRD()
		{
			Assert("Not applicable", true);
		}

		public new void TestPostingChargesWithDuplicateInvoiceNumbers_Standard()
		{
			Assert("Not applicable", true);
		}

		public new void TestPostingChargesWithDuplicateInvoiceNumbers_Standard_NoPermission()
		{
			Assert("Not applicable", true);
		}

		public new void TestPostingChargesWithDuplicateInvoiceNumbers_Calendar()
		{
			Assert("Not applicable", true);
		}

		public new void TestPostingChargesWithDuplicateInvoiceNumbers_Calendar_NoPermission()
		{
			Assert("Not applicable", true);
		}

		#region Implementation

		protected override string[] ReDistributeChargesList
		{
			get
			{
				var allInvoiceTypes = new InvoiceTypesList();
				allInvoiceTypes.AddRange(new AgencyInvoiceTypesList());

				return allInvoiceTypes.Cast<CodeDescriptionPair>().Select(x => x.Code).
					Where(x => InvoiceTypeCalculationProvider.DeferredInvoiceTypes.Contains(x) && AccTransactionHeader.DisbursementInvoiceTypes.Contains(x)).ToArray();
			}
		}

		protected override string[] NotReDistributeChargesList
		{
			get
			{
				var allInvoiceTypes = new InvoiceTypesList();
				allInvoiceTypes.AddRange(new AgencyInvoiceTypesList());

				return allInvoiceTypes.Cast<CodeDescriptionPair>().Select(x => x.Code).
					Where(x => InvoiceTypeCalculationProvider.DeferredInvoiceTypes.Contains(x) && !AccTransactionHeader.DisbursementInvoiceTypes.Contains(x)).ToArray();
			}
		}

		protected override BasePostManager GetPostManager(IEnumerable<Job> jobs, GlbBranch taxBranch)
		{
			var periodicInvoice = new PeriodicInvoice(Factory);
			if (taxBranch != null)
			{
				periodicInvoice.TaxBranch = taxBranch.PK;
			}

			var periodicInvoiceLight = new PeriodicInvoiceLightForCheckSecurityRights(Factory);
			periodicInvoiceLight.InitalizeFromInvoiceInfo(new PeriodicInvoiceBulkPoster.InvoiceInfo(periodicInvoice));

			periodicInvoiceLight.Jobs.AddRange(jobs);
			var allJobCharges = (from Job job in jobs
								 from Charge charge in job.Charges
								 select charge).
								 ToList();
			allJobCharges.ForEach(charge =>
				{
					charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
					OrgInvoiceType invoiceType = charge.SellAccount.CompanyData.InvoiceTypes.AddNew();
					invoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
					invoiceType.PI_RS_NKServiceLevel = "STD";
					invoiceType.PI_Calc_IsInclude = InvoiceTypeChargeInclusionTypeList.Codes.INC;
					invoiceType.DeferredCharges.AddNew().PO_AC = charge.JR_AC;
				});
			periodicInvoiceLight.Charges.AddRange(allJobCharges.ToArray());

			return new PeriodicInvoicePostManager(periodicInvoiceLight);
		}

		protected override void SetupDebtor(OrgHeader debtor)
		{
			var type = debtor.CompanyData.InvoiceTypes.AddNew();
			type.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			type.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			type.PI_Type = InvoiceTypeLayoutList.Codes.CHG;
		}

		#endregion
	}
}
