using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Customs.JP.MessageDefinitions.Outbound;

namespace Enterprise.Customs.JP.Business
{
	sealed class ImportItemProvider : IImportItem
	{
		public ImportItemProvider(CusEntryLine entryLine)
		{
			Argument.NotNull(entryLine, nameof(entryLine));
			this.invoiceLine = entryLine.InvoiceLines[0] as JobComInvoiceLine;
		}

		readonly JobComInvoiceLine invoiceLine;

		public IGoodsItem Item => TryGetGoodsItemProvider(invoiceLine);

		public string CertificateOfOrigin => invoiceLine.JI_PrimaryPreference;

		public string ImportTradeControlOrdinanceAppendixCode => invoiceLine.JI_TradeControlOrderAppendix;

		public string StorageType => invoiceLine.JI_StorageType;

		public decimal? CustomsValueApportionmentCoefficient => null;

		public string FreightApportionmentType => "";

		public IMoney CustomsValue => TryGetMoneyProvider(invoiceLine.CusEntryLine as CusEntryLine);

		public string AdvanceRulingOnClassification => invoiceLine.JI_AdvanceRulingOnClassification;

		public string AdvanceRulingOnOrigin => invoiceLine.JI_AdvanceRulingOnOrigin;

		public string DutyReductionExemptionCode => invoiceLine.JI_DutyReductionExemptionRefundCode;

		public decimal? DutyReductionAmount => invoiceLine.InvoiceHeader.InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.JI_DutyReductionAmount);

		public IEnumerable<IDomesticConsumptionTax> DomesticConsumptionTaxCodes
		{
			get
			{
				foreach (var tariff in invoiceLine.DomesticConsumptionTaxes.Take(6))
				{
					yield return TryGetDomesticConsumptionTaxProvider(tariff);
				}
			}
		}

		GoodsItemProvider TryGetGoodsItemProvider(JobComInvoiceLine invoiceLine) => invoiceLine != null && invoiceLine.IsInDatabase ? new GoodsItemProvider(invoiceLine) : null;

		MoneyProvider TryGetMoneyProvider(CusEntryLine entryLine) => entryLine != null && entryLine.IsInDatabase ? new MoneyProvider(entryLine, nameof(entryLine.CL_CustomsValue)) : null;

		DomesticConsumptionTaxProvider TryGetDomesticConsumptionTaxProvider(CusLineTariffDetail tariff) => tariff != null && tariff.IsInDatabase ? new DomesticConsumptionTaxProvider(tariff) : null;
	}
}
