using System.Linq;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ImportUCC5EntryCreationStrategy : EU.Business.Declaration.EntryCreationStrategy
	{
		public ImportUCC5EntryCreationStrategy(EU.Business.Declaration.JobDeclaration declaration) : base(declaration)
		{
		}

		protected override MergeKey GetKeyForHeaderCore(BaseJobComInvoiceLine baseInvoiceLine)
		{
			var invoiceLine = (JobComInvoiceLine)baseInvoiceLine;
			var invoice = invoiceLine.InvoiceHeader;
			var mergeKey = base.GetKeyForHeaderCore(invoiceLine);

			mergeKey.Add(invoice.JZ_IncoTerm);
			mergeKey.Add(invoice.JZ_IncoTermPlace);
			mergeKey.Add(invoice.ZG_AgreedPlaceCode);
			mergeKey.Add(invoice.JZ_AdditionalTerms);
			mergeKey.Add(invoice.JZ_RX_NKInvoice_Currency);
			mergeKey.Add(invoice.JZ_ValuationCode);
			mergeKey.Add(invoice.JZ_UCR);

			return mergeKey;
		}

		public override MergeKey GetKeyForLine(BaseJobComInvoiceLine baseInvoiceLine)
		{
			var mergeKey = base.GetKeyForLine(baseInvoiceLine);

			var invoiceLine = (JobComInvoiceLine)baseInvoiceLine;

			if (invoiceLine.CusLineTariffDetails.Any())
			{
				mergeKey.Add(invoiceLine.PK);
			}

			return mergeKey;
		}
	}
}
