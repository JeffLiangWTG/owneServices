using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.EPayment
{
	public class EPaymentQuoteSummaryValidation : AutoEPaymentQuoteSummaryValidation
	{
		public EPaymentQuoteSummaryValidation(EPaymentQuoteSummary parent)
			: base(parent) { }

		public override void CheckTotalFeeAmount()
		{
			base.CheckTotalFeeAmount();
			if (Parent.QuotesToSummarise.Any(x => x.QU_ProviderCode == EPaymentProviderCodes.Codes.OFX) && Parent.QuotesToSummarise.Any(x => x.QU_Status == EPaymentStatusCodes.Quote.Received) && Parent.QuotesToSummarise.Any(x => x.QU_ProviderReference.IsEmpty))
			{
				var message = Res.GetString("4db9a5be-4712-48cd-81a7-480db5f42876", "This is an indicative rate only. Processing Fee may be applied on the formal quotes. To request formal quotes, please select an OFX E-Payment Account as the Bank Account for this payment, and ensure you have authorized your OFX User Account.");
				Parent.TotalFeeAmountInfo.AddWarning(message);
			}
		}

		#region Implementation

		public new EPaymentQuoteSummary Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.Parent; }
		}

		#endregion
	}
}
