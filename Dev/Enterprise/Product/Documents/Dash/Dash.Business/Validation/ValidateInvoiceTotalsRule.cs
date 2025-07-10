using Enterprise.Dash.Integration.Validation;

namespace Enterprise.Dash.Business.Validation
{
	public sealed class ValidateInvoiceTotalsRule : IValidationRule<DashCommercialInvoice>
	{
		public string Validate(DashCommercialInvoice dashCommercialInvoice)
		{
			var calculatedInvoiceTotals = 0m;

			foreach(var item in dashCommercialInvoice.CommercialInvoiceLineItems)
			{
				var dashCommercialInvoiceLineItem = (DashCommercialInvoiceLineItem)item;
				var calculatedLineTotal = dashCommercialInvoiceLineItem.DLI_Quantity * dashCommercialInvoiceLineItem.DLI_PricePerUnit;

				if (dashCommercialInvoiceLineItem.DLI_LineTotal != calculatedLineTotal)
				{
					return $@"{nameof(DashCommercialInvoiceLineItem)} with PK {dashCommercialInvoiceLineItem.PK}: {nameof(dashCommercialInvoiceLineItem.DLI_LineTotal)} is not equal to the product of {nameof(dashCommercialInvoiceLineItem.DLI_Quantity)} and {nameof(dashCommercialInvoiceLineItem.DLI_PricePerUnit)}. {nameof(dashCommercialInvoiceLineItem.DLI_LineTotal)}: {dashCommercialInvoiceLineItem.DLI_LineTotal}. Calculated line total: {calculatedLineTotal}";
				}

				calculatedInvoiceTotals += calculatedLineTotal;
			}

			if (dashCommercialInvoice.DCI_GrossTotal != calculatedInvoiceTotals)
			{
				return $"{nameof(DashCommercialInvoice)} with PK {dashCommercialInvoice.PK}: {nameof(dashCommercialInvoice.DCI_GrossTotal)} is not equal to the sum of line totals. {nameof(dashCommercialInvoice.DCI_GrossTotal)}: {dashCommercialInvoice.DCI_GrossTotal}. Sum of line totals: {calculatedInvoiceTotals}";
			}

			return string.Empty;
		}
	}
}
