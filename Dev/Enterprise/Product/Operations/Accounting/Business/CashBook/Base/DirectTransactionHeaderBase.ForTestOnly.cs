#if DEBUG

using CargoWise.Types;

namespace Enterprise.Accounting.Business.CashBook
{
	public partial class DirectTransactionHeaderBase
	{
		public void SetReceiptTypeOnly_ForTestOnly(ZString receiptType)
		{
			SetReceiptTypeOnly(receiptType);
		}

		public void SetDefaultValues_ForTestOnly()
		{
			SetDefaultValues();
		}

		public ZBool IsChequeNumberAutoAllocated_ForTestOnly => IsChequeNumberAutoAllocated;
	}
}

#endif
