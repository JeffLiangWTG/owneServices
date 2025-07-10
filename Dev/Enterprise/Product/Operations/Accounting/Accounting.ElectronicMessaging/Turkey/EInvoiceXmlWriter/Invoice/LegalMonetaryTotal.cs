using System.Linq;
using Enterprise.Accounting.ElectronicMessaging.efatura.uyumsoft.com.tr;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey
{
	internal class LegalMonetaryTotal
	{
		internal MonetaryTotalType BuildLegalMonetaryTotal(EInvoiceHelper helper)
		{
			var withholdingTaxTotal = helper.FixDecimalPlacesAndSign(helper.NonCommentInvoiceLines.Where(x => helper.HasWithholdingTax(x)).ToList().Sum(x => x.OSExtraVATAmount).Value);
			return new MonetaryTotalType()
			{
				LineExtensionAmount = new LineExtensionAmountType()
				{
					Value = helper.FixDecimalPlacesAndSign(helper.UInvoice.OSExGSTVATAmount.Value),
					currencyID = helper.UInvoice.OSCurrency.Code
				},
				TaxExclusiveAmount = new TaxExclusiveAmountType()
				{
					Value = helper.FixDecimalPlacesAndSign(helper.UInvoice.OSExGSTVATAmount.Value),
					currencyID = helper.UInvoice.OSCurrency.Code
				},
				TaxInclusiveAmount = new TaxInclusiveAmountType()
				{
					Value = helper.FixDecimalPlacesAndSign(helper.UInvoice.OSTotal.Value) + withholdingTaxTotal,
					currencyID = helper.UInvoice.OSCurrency.Code
				},
				PayableAmount = new PayableAmountType()
				{
					Value = helper.FixDecimalPlacesAndSign(helper.UInvoice.OSTotal.Value),
					currencyID = helper.UInvoice.OSCurrency.Code
				}
			};
		}
	}
}
