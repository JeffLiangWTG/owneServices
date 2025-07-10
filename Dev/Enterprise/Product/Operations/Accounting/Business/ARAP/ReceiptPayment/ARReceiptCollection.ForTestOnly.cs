#if DEBUG

using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public partial class ARReceiptCollection
	{
		public ZBool DefaultIncludeInDepositBatch_ForTestOnly
		{
			get { return DefaultIncludeInDepositBatch; }
			set { DefaultIncludeInDepositBatch = value; }
		}
	}
}

#endif
