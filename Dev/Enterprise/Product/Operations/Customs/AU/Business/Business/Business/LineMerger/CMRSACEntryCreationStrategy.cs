namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRSACEntryCreationStrategy : CMREntryCreationStrategy
	{
		public CMRSACEntryCreationStrategy(LineMerger merger)
			: base(merger)
		{
		}

		protected override Customs.Business.MergeKey GetKeyForHeaderCore(Customs.Business.BaseJobComInvoiceLine baseInvoiceLine)
		{
			JobComInvoiceHeader invoiceHeader = (JobComInvoiceHeader)baseInvoiceLine.InvoiceHeader;
			Customs.Business.MergeKey result = base.GetKeyForHeaderCore(baseInvoiceLine);
			result.Add(invoiceHeader.AddInfo.ZA_ORG);
			return result;
		}
	}
}
