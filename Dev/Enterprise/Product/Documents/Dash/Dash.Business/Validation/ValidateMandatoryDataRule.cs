using Enterprise.Dash.Integration.Validation;

namespace Enterprise.Dash.Business.Validation
{
	public sealed class ValidateMandatoryDataRule : IValidationRule<DashCommercialInvoice>
	{
		public string Validate(DashCommercialInvoice dashCommercialInvoice)
		{
			var errorMessage = ValidateDashCommercialInvoice(dashCommercialInvoice);

			if (!string.IsNullOrWhiteSpace(errorMessage))
			{
				return errorMessage;
			}

			if (dashCommercialInvoice.CommercialInvoiceLineItems.Count == 0)
			{
				return $"{nameof(DashCommercialInvoice)} with PK {dashCommercialInvoice.PK} does not have commercial invoice lines";
			}

			foreach(var item in dashCommercialInvoice.CommercialInvoiceLineItems)
			{
				var dashCommercialInvoiceLineItem = (DashCommercialInvoiceLineItem)item;
				errorMessage = ValidateDashCommercialInvoiceLineItem(dashCommercialInvoiceLineItem);

				if (!string.IsNullOrWhiteSpace(errorMessage))
				{
					return errorMessage;
				}
			}

			return string.Empty;
		}

		string ValidateDashCommercialInvoice(DashCommercialInvoice dashCommercialInvoice)
		{
			var dashCommercialInvoiceMessagePrefix = $"{nameof(DashCommercialInvoice)} with PK {dashCommercialInvoice.PK}";

			if (dashCommercialInvoice.DCI_InvoiceNumber.IsEmpty)
			{
				return $"{dashCommercialInvoiceMessagePrefix}: {nameof(dashCommercialInvoice.DCI_InvoiceNumber)} must not be empty";
			}

			if (dashCommercialInvoice.DCI_InvoiceDate.IsEmpty)
			{
				return $"{dashCommercialInvoiceMessagePrefix}: {nameof(dashCommercialInvoice.DCI_InvoiceDate)} must not be empty";
			}

			if (dashCommercialInvoice.DCI_GrossTotal.IsEmpty)
			{
				return $"{dashCommercialInvoiceMessagePrefix}: {nameof(dashCommercialInvoice.DCI_GrossTotal)} must not be empty";
			}

			if (dashCommercialInvoice.DCI_OH_MatchedImporterID.IsEmpty)
			{
				return $"{dashCommercialInvoiceMessagePrefix}: {nameof(dashCommercialInvoice.DCI_OH_MatchedImporterID)} must not be empty";
			}

			if (dashCommercialInvoice.DCI_OH_MatchedSupplierID.IsEmpty)
			{
				return $"{dashCommercialInvoiceMessagePrefix}: {nameof(dashCommercialInvoice.DCI_OH_MatchedSupplierID)} must not be empty";
			}

			if (dashCommercialInvoice.DCI_RX_NKInvoiceCurrency.IsEmpty)
			{
				return $"{dashCommercialInvoiceMessagePrefix}: {nameof(dashCommercialInvoice.DCI_RX_NKInvoiceCurrency)} must not be empty";
			}

			return string.Empty;
		}

		string ValidateDashCommercialInvoiceLineItem(DashCommercialInvoiceLineItem dashCommercialInvoiceLineItem)
		{
			var lineItemMessagePrefix = $"{nameof(DashCommercialInvoiceLineItem)} with PK {dashCommercialInvoiceLineItem.PK}";

			if (dashCommercialInvoiceLineItem.DLI_OP_MatchedProductCodeID.IsEmpty)
			{
				return $"{lineItemMessagePrefix}: {nameof(dashCommercialInvoiceLineItem.DLI_OP_MatchedProductCodeID)} must not be empty";
			}

			if (dashCommercialInvoiceLineItem.DLI_PricePerUnit.IsEmpty)
			{
				return $"{lineItemMessagePrefix}: {nameof(dashCommercialInvoiceLineItem.DLI_PricePerUnit)} must not be empty";
			}

			if (dashCommercialInvoiceLineItem.DLI_Quantity.IsEmpty)
			{
				return $"{lineItemMessagePrefix}: {nameof(dashCommercialInvoiceLineItem.DLI_Quantity)} must not be empty";
			}

			if (dashCommercialInvoiceLineItem.DLI_LineTotal.IsEmpty)
			{
				return $"{lineItemMessagePrefix}: {nameof(dashCommercialInvoiceLineItem.DLI_LineTotal)} must not be empty";
			}

			return string.Empty;
		}
	}
}
