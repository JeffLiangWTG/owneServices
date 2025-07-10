using System.Linq;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ImportEntryCreationStrategy : EU.Business.Declaration.EntryCreationStrategy
	{
		public ImportEntryCreationStrategy(EU.Business.Declaration.JobDeclaration declaration) : base(declaration)
		{
		}

		protected override MergeKey GetKeyForHeaderCore(BaseJobComInvoiceLine baseInvoiceLine)
		{
			var invoiceLine = (JobComInvoiceLine)baseInvoiceLine;
			var invoice = invoiceLine.InvoiceHeader;
			var mergeKey = base.GetKeyForHeaderCore(invoiceLine);

			mergeKey.Add(invoice.JZ_UCR);

			return mergeKey;
		}

		protected override bool ShouldProcessCusSupplyChainActorReferencesForLineMergeKey => true;

		protected override bool ShouldProcessFiscalReferencesForLineMergeKey => true;

		public override MergeKey GetKeyForLine(BaseJobComInvoiceLine baseInvoiceLine)
		{
			var mergeKey = base.GetKeyForLine(baseInvoiceLine);

			var invoiceLine = (JobComInvoiceLine)baseInvoiceLine;
			var invoice = invoiceLine.InvoiceHeader;

			if (invoiceLine.CusLineTariffDetails.Any())
			{
				mergeKey.Add(invoiceLine.PK);
			}

			mergeKey.Add(invoice.JZ_ValuationCode);

			return mergeKey;
		}
	}
}
