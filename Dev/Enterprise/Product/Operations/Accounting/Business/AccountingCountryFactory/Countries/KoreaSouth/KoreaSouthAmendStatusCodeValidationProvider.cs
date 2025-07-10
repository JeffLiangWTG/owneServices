using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class KoreaSouthAmendStatusCodeValidationProvider : AmendStatusCodeValidationProvider
	{
		public override void ValidateAmendStatusCodeForInvoice(InvoicingBase invoicingBase)
		{
			base.ValidateAmendStatusCodeForInvoice(invoicingBase);

			if (!invoicingBase.AH_Calc_AmendStatusCodeInfo.HasErrors() && invoicingBase.IsAllowModifyAmendStatusCode && ShouldValidateAmendStatusCode(invoicingBase))
			{
				if (!EInvoicingKoreaSouthConstants.AllowedAmendmentStatusCodesWhenAmendWithInvoice.Contains(invoicingBase.AH_Calc_AmendStatusCode.ToString()))
				{
					invoicingBase.AH_Calc_AmendStatusCodeInfo.AddError(Res.GetString("324C7A50-C8D7-464F-905E-1D05A98B53EB", "For amendment invoice, only amend status code 01, 02 or 05 can be used."));
				}
			}
		}

		public override void ValidateAmendStatusCodeForInvoiceReversal(InvoicingBase invoicingBase)
		{
			base.ValidateAmendStatusCodeForInvoiceReversal(invoicingBase);

			if (!invoicingBase.AH_Calc_AmendStatusCodeInfo.HasErrors() && invoicingBase.IsAllowModifyAmendStatusCode && ShouldValidateAmendStatusCode(invoicingBase))
			{
				if (!EInvoicingKoreaSouthConstants.SuggestedAmendmentStatusCodesWhenReverseInvoice.Contains(invoicingBase.AH_Calc_AmendStatusCode.ToString()))
				{
					invoicingBase.AH_Calc_AmendStatusCodeInfo.AddWarning(Res.GetString("CED29414-D7D4-416C-B15C-21D87802D2B6", "For invoice reversal, amend status code should be 03, 04 or 06."));
				}
			}
		}

		protected override bool ShouldValidateAmendStatusCode(InvoicingBase invoicingBase)
		{
			var lines = invoicingBase.Lines.OfType<InvoicingLineBase>();
			if (lines.Any(x => x.TaxRate != null && x.TaxRate.AT_Type != AccTaxRate.Types.NotReportable && x.TaxRate.AT_Type != AccTaxRate.Types.ExcludedFromTheTaxBase))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
