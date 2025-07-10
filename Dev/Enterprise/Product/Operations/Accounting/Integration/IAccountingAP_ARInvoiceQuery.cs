using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Integration
{
	public struct AP_ARInvoiceQueryResult
	{
		public ZDecimal APPostedAmount { get; set; }
		public ZDecimal APUnPostedAmount { get; set; }

		public ZBool APFullyPaid { get; set; }

		public ZDecimal ARPostedAmount { get; set; }
		public ZDecimal ARUnPostedAmount { get; set; }

		public ZInt NumberOfUnpostedARCharges { get; set; }
		public ZInt NumberOfUnpostedAPCharges { get; set; }
		public ZInt NumberOfUnpostedARChargesReadyForPosting { get; set; }
		public ZInt NumberOfUnpostedAPChargesReadyForPosting { get; set; }
		public ZInt NumberOfARInvoicesToBeIssued { get; set; }

		public void SetAmounts(ZDecimal aPPostedAmount, ZDecimal aPUnPostedAmount, ZBool aPFullyPaid, ZDecimal aRPostedAmount, ZDecimal aRUnPostedAmount)
		{
			this.APPostedAmount = aPPostedAmount;
			this.APUnPostedAmount = aPUnPostedAmount;

			this.APFullyPaid = aPFullyPaid;

			this.ARPostedAmount = aRPostedAmount;
			this.ARUnPostedAmount = aRUnPostedAmount;
		}

		public void Merge(AP_ARInvoiceQueryResult passed)
		{
			APPostedAmount += passed.APPostedAmount;
			APUnPostedAmount += passed.APUnPostedAmount;

			ARPostedAmount += passed.ARPostedAmount;
			ARUnPostedAmount += passed.ARUnPostedAmount;

			APFullyPaid &= passed.APFullyPaid;

			NumberOfUnpostedARCharges += passed.NumberOfUnpostedARCharges;
			NumberOfUnpostedAPCharges += passed.NumberOfUnpostedAPCharges;
			NumberOfUnpostedARChargesReadyForPosting += passed.NumberOfUnpostedARChargesReadyForPosting;
			NumberOfUnpostedAPChargesReadyForPosting += passed.NumberOfUnpostedAPChargesReadyForPosting;
		}

		public ZDecimal TotalBilledAmount { get; set; }
		public ZDecimal TotalOutstandingAmount { get; set; }
		public ZDecimal TotalInvoicedAmount { get; set; }
	}

	public interface IAccountingAP_ARInvoiceQuery
	{
		AP_ARInvoiceQueryResult GetInvoiceAmount(ICustomsJobInfo declaration, List<ZGuid> chargeCodesToMatch);

		AP_ARInvoiceQueryResult GetPostingDetails(ICustomsJobInfo declaration, ZGuid[] customsDSBCharges, bool postAPCustomsDSB, bool postARCustomsDSB);

		AP_ARInvoiceQueryResult GetTotalInvoicedDetails(ICustomsJobInfo declaration, List<ZGuid> chargeCodesToMatch, string descToMatch = "");

		void ClearServiceCache(BusinessObjectFactory factory);
	}
}
