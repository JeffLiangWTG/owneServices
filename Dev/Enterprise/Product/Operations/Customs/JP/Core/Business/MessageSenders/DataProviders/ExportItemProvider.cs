using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Customs.JP.MessageDefinitions.Outbound;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Business
{
	sealed class ExportItemProvider : IExportItem
	{
		public ExportItemProvider(CusEntryLine entryLine)
		{
			Argument.NotNull(entryLine, nameof(entryLine));
			this.entryLine = entryLine;
			invoiceLine = entryLine.InvoiceLines[0] as JobComInvoiceLine;
		}

		JobComInvoiceLine invoiceLine { get; }

		CusEntryLine entryLine { get; }

		public IGoodsItem Item => TryGetGoodsItemProvider(entryLine);

		public IBasicPrice BasicPrice => TryGetBasicPriceProvider(entryLine);

		public IEnumerable<string> OtherLawCodes => invoiceLine.OtherLaws.Take(CusOtherLawReferenceCollection<CusOtherLawReferenceForInvoiceLines>.MaxRowCount).Cast<CusOtherLawReferenceForInvoiceLines>().Select(x => x.CFR_Reference.ToString());

		public string ExportControlOrdinanceAppendixCode => invoiceLine.JI_TradeControlOrderAppendix;

		public string ForeignExchangeLawArticle48Code => invoiceLine.JI_FEFTAArticle48;

		public string DutyExemptionReductionRefundCode => invoiceLine.JI_DutyReductionExemptionRefundCode;

		public string DomesticConsumptionTaxExemptionCode => invoiceLine.JI_DomesticConsumptionTaxExemptionCode;

		public string DomesticConsumptionTaxExemptionType
		{
			get
			{
				if (string.IsNullOrWhiteSpace(invoiceLine.JI_DomesticConsumptionTaxExemptionCode))
				{
					return string.Empty;
				}
				return invoiceLine.JI_DomesticConsumptionTaxExemptionIsPartial ? "P" : "A";
			}
		}

		BasicPriceProvider TryGetBasicPriceProvider(CusEntryLine entryLine) => entryLine != null && entryLine.IsInDatabase ? new BasicPriceProvider(entryLine) : null;

		GoodsItemProvider TryGetGoodsItemProvider(CusEntryLine entryLine) => entryLine != null && entryLine.IsInDatabase ? new GoodsItemProvider(entryLine) : null;
	}
}
