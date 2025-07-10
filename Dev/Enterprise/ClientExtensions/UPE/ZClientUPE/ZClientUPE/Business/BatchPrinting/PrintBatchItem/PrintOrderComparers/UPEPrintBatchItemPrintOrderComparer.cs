using System.Collections;

namespace Enterprise.Client.UPE.Business
{
	public abstract class UPEPrintBatchItemPrintOrderComparer : IComparer
	{
		protected UPEPrintBatchItemPrintOrderComparer()
		{
		}

		public static UPEPrintBatchItemPrintOrderComparer New(string batchType)
		{
			switch (batchType)
			{
				case UPEPrintBatchTypes.Codes.TaxInvoice: return new UPETaxInvoicePrintBatchItemComparer();
				default: return null;
			}
		}

		public abstract int Compare(UPEPrintBatchItem lhsItem, UPEPrintBatchItem rhsItem);

		int IComparer.Compare(object lhs, object rhs)
		{
			return Compare((UPEPrintBatchItem)lhs, (UPEPrintBatchItem)rhs);
		}
	}
}
