
using Enterprise.MasterFiles.Business;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.CashBook
{
	public abstract class DirectTransactionHeaderBaseLookups : AccTransactionHeaderLookups
	{
		public DirectTransactionHeaderBaseLookups(DirectTransactionHeaderBase parent)
			: base(parent)
		{
		}

		public abstract CodeDescriptionPairList ReceiptPaymentMethodsList { get; }
	}
}