namespace Enterprise.Client.UPE.Business
{
	class UPETaxInvoicePrintBatchItemComparer : UPEPrintBatchItemPrintOrderComparer
	{
		public override int Compare(UPEPrintBatchItem lhsItem, UPEPrintBatchItem rhsItem)
		{
			Callout lhs = (Callout)lhsItem.Parent;
			Callout rhs = (Callout)rhsItem.Parent;

			int result = lhs.BillToAccountNumber.ToLower().CompareTo(rhs.BillToAccountNumber.ToLower());
			if (result == 0)
			{
				result = lhs.InvoiceNumber.ToLower().CompareTo(rhs.InvoiceNumber.ToLower());
			}
			return result;
		}
	}
}
