using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions;

namespace Enterprise.Customs.JP.Business
{
	sealed class GoodsItemProvider : IGoodsItem
	{
		public GoodsItemProvider(JobComInvoiceLine invoiceLine)
		{
			Argument.NotNull(invoiceLine, nameof(invoiceLine));
			this.invoiceLine = invoiceLine;
		}

		public GoodsItemProvider(CusEntryLine entryLine)
		{
			Argument.NotNull(entryLine, nameof(entryLine));
			this.entryLine = entryLine;
			this.invoiceLine = entryLine.InvoiceLines[0] as JobComInvoiceLine;
		}

		readonly JobComInvoiceLine invoiceLine;

		readonly CusEntryLine entryLine;

		public string TariffCode => invoiceLine.JI_Tariff.SubstringSafe(0, 9);

		public string NACCSCode => invoiceLine.JI_NACCSCode;

		public string GoodDescription => invoiceLine.IsImport ? invoiceLine.JI_Description : entryLine.EffectiveDescription;

		public IMeasurement Quantity1 => invoiceLine.IsImport ? TryGetMeasurementProvider(invoiceLine, nameof(GoodsItemProvider) + "." + nameof(Quantity1)) : TryGetMeasurementProvider(entryLine, nameof(GoodsItemProvider) + "." + nameof(Quantity1));

		public IMeasurement Quantity2 => invoiceLine.IsImport ? TryGetMeasurementProvider(invoiceLine, nameof(GoodsItemProvider) + "." + nameof(Quantity2)) : TryGetMeasurementProvider(entryLine, nameof(GoodsItemProvider) + "." + nameof(Quantity2));

		public string CountryOfOrigin => invoiceLine.IsImport ? invoiceLine.JI_CountryOfOrigin : string.Empty;

		MeasurementProvider TryGetMeasurementProvider(JobComInvoiceLine invoiceLine, string type) => invoiceLine != null && invoiceLine.IsInDatabase ? new MeasurementProvider(invoiceLine, type) : null;

		MeasurementProvider TryGetMeasurementProvider(CusEntryLine entryLine, string type) => entryLine != null && entryLine.IsInDatabase ? new MeasurementProvider(entryLine, type) : null;
	}
}
