namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	internal class DummyEntryCreationStrategy : EntryCreationStrategyBase
	{
		public DummyEntryCreationStrategy(LineMerger lineMerger) : base(lineMerger)
		{
		}

		public string GetNonAmendableLineDetailsExposed(JobComInvoiceLine invoiceLine)
		{
			return GetNonAmendableLineDetails(invoiceLine);
		}
	}
}
