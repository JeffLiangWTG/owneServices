#if DEBUG

using CargoWise.Types;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch
{
	public partial class DirectDebitBatchLineCollection
	{
		public void SetReceiptType_ForTestOnly(ZString receiptType)
		{
			SetReceiptType(receiptType);
		}

		public ZDecimal FSelectedTotal_ForTestOnly
		{
			get { return fSelectedTotal; }
			set { fSelectedTotal = value; }
		}

		public ZDecimal FLocalSelectedTotal_ForTestOnly
		{
			get { return fLocalSelectedTotal; }
			set { fLocalSelectedTotal = value; }
		}
	}
}

#endif
