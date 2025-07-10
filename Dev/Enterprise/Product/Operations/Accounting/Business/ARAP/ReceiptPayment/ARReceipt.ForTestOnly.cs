#if DEBUG

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public partial class ARReceipt
	{
		public ARReceiptCollection GetParentCollection_ForTestOnly()
		{
			return ParentCollection;
		}
	}
}

#endif
