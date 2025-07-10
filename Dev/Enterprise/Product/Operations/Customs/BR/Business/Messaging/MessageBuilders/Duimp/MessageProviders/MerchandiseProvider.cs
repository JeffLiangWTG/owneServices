using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.BR.MessageContracts.Duimp.Outgoing;
using CargoWise.Customs.Shared.MessageContracts;

namespace Enterprise.Customs.BR.Business.Duimp
{
	public class MerchandiseProvider : IMerchandise
	{
		MerchandiseProvider(IEnumerable<JobComInvoiceLine> invoiceLines)
		{
			this.invoiceLines = CargoWise.Common.Argument.NotNull(invoiceLines, nameof(invoiceLines));
		}
		readonly IEnumerable<JobComInvoiceLine> invoiceLines;
		JobComInvoiceLine invoiceLine => invoiceLines.FirstOrDefault();

		public static MerchandiseProvider New(IEnumerable<JobComInvoiceLine> invoiceLines) => invoiceLines == null || !invoiceLines.Any() ? null : new MerchandiseProvider(invoiceLines);

		public string ApplicationTypeCode => ImportGoodsApplicationTypeList.MapToCustomsCode(invoiceLine.JI_GoodsApplication);

		public string Condition => ImportGoodsConditionTypeList.MapToCustomsCode(invoiceLine.JI_GoodsCondition);

		public string Description => invoiceLine.ComplementaryDescription;

		public double CommercialQuantity => commercialQuantity ??= (double)invoiceLines.Sum(s => s.JI_InvoiceQuantity).Round(5);
		double? commercialQuantity;

		public string CommercialUnit => invoiceLine.InvoiceUQDescInPortugueseBrazil;

		public double QuantityStatisticalMeasure => quantityStatisticalMeasure ??= (double)invoiceLines.Sum(s => s.JI_CustomsQuantity).Round(5);
		double? quantityStatisticalMeasure;

		public double UnitPrice => unitPrice ??= CommercialQuantity > 0d ? (double)(invoiceLines.Sum(s => s.JI_Calc_InvAmount) / (decimal)CommercialQuantity).Round(7) : 0d;
		double? unitPrice;

		public string Currency => invoiceLine.JI_RX_NKLinePriceCurr;

		public double NetWeight => (double)invoiceLines.Sum(s => s.NetWeightInKG).Round(5);
	}
}
