using NUnit.Framework;

namespace Enterprise.Accounting.Web.Business.Testing
{
	[TestedType(typeof(TransactionPaymentStatusRequest))]
	public class TransactionPaymentStatusRequestTestCase : BaseITransactionRequestTestCase
	{
		#region Implementation

		protected override string ExpectedTransactionTypeHint
		{
			get
			{
				return @"A valid TransactionType should be provided. Please, use 
	INV for Invoice,
	CRD for Credit Note,
	ADJ for Ajustment Note,
	PAY for Payment,
	REC for Receipt,
	TRF for Transfer,
	CTR for Contra,
	JNL for Journal,
	EXX for Exchange Difference,
	OVP for Overpayment and
	DSC for Discount.";
			}
		}

		protected override ITransactionNaturalKeys GetNewRequest()
		{
			return new TransactionPaymentStatusRequest();
		}

		#endregion
	}
}
