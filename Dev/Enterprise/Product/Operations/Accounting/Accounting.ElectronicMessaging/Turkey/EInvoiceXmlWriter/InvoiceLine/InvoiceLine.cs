using System.Collections.Generic;
using Enterprise.Accounting.ElectronicMessaging.efatura.uyumsoft.com.tr;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey
{
	internal class InvoiceLine
	{
		const string lineUnitTypeCode = "C62";

		internal InvoiceLineType[] BuildInvoiceLine(EInvoiceHelper helper)
		{
			var invoiceLineList = new List<InvoiceLineType>();

			foreach (var line in helper.NonCommentInvoiceLines)
			{
				var lineTaxTotal = new InvoiceLineTaxTotal(line, helper);
				var invoiceLine = new InvoiceLineType()
				{
					ID = new IDType()
					{
						Value = line.Sequence.ToString()
					},
					Note = new InvoiceLineNote().BuildLineNote(line),
					InvoicedQuantity = new InvoicedQuantityType()
					{
						Value = 1,
						unitCode = lineUnitTypeCode
					},
					LineExtensionAmount = new LineExtensionAmountType()
					{
						Value = helper.FixDecimalPlacesAndSign(line.OSAmount.Value),
						currencyID = line.OSCurrency.Code.Value
					},
					TaxTotal = lineTaxTotal.BuildInvoiceLineTaxTotal(),
					Item = new InvoiceLineItem().BuildInvoiceLineItem(line),
					Price = new PriceType()
					{
						PriceAmount = new PriceAmountType()
						{
							Value = helper.ConvertToPositiveValue(line.OSAmount.Value),
							currencyID = line.OSCurrency.Code.Value
						}
					}
				};

				if (helper.HasWithholdingTax(line))
				{
					invoiceLine.WithholdingTaxTotal = lineTaxTotal.BuildInvoiceLineWithholdingTaxTotal();
				}

				invoiceLineList.Add(invoiceLine);
			}

			return invoiceLineList.Count != 0 ? invoiceLineList.ToArray() : null;
		}
	}
}
