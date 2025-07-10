using CargoWise.Types;

namespace Enterprise.Accounting.ElectronicMessaging.Hungary
{
	public class InvoiceLineInfo
	{
		public int LineNumber { get; set; }

		public int? LineModificationReference { get; set; }

		public string LineModificationOperation { get; set; }

		public string LineExpressionIndicator { get; set; }

		public string LineNatureIndicator { get; set; }

		public string LineDescription { get; set; }

		public string LineNetAmountFormatted { get; set; }

		public string lineNetAmountHUFFormatted { get; set; }

		public string LineVatRate { get; set; }

		public string LineExchangeRateFormatted { get; set; }

		public string LineDeliveryDateFormatted { get; set; }

		public VATRateTagType VATType { get; set; }

		public VATExemptionInfo VATExemption { get; set; }
	}

	public class InvoiceLineTaxInfo
	{
		public ZString TaxTypeCode { get; set; }

		public ZDecimal TaxRateAsRatio { get; set; }

		public ZString TaxGroupCode { get; set; }

		public ZString TaxGroupDescription { get; set; }

		public ZString GovtTaxGroupCode { get; set; }

		public VATRateTagType VATRateType { get; set; }
	}
}
