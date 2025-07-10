

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.CashBook
{
	public class DirectPaymentLookups : DirectTransactionHeaderBaseLookups
	{
		public DirectPaymentLookups(DirectPayment.DirectPayment parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList ReceiptPaymentMethodsList
		{
			get { return ((DirectPayment.DirectPayment)Parent).PaymentMethods; }
		}
	}
}