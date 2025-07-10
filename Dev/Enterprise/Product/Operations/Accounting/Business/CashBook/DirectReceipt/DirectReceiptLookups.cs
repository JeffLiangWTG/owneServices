

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.CashBook
{
	public class DirectReceiptLookups : DirectTransactionHeaderBaseLookups
	{
		public DirectReceiptLookups(DirectReceipt.DirectReceipt parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList ReceiptPaymentMethodsList
		{
			get { return ((DirectReceipt.DirectReceipt)Parent).ReceiptMethods; }
		}
	}
}