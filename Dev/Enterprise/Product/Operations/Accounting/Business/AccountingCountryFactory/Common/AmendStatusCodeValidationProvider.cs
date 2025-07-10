using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class AmendStatusCodeValidationProvider : IAmendStatusCodeValidationProvider
	{
		public void ValidateAmendStatusCode(InvoicingBase invoicingBase)
		{
			if (invoicingBase.IsAllowModifyAmendStatusCode && invoicingBase.AH_Calc_AmendStatusCode.IsEmpty && ShouldValidateAmendStatusCode(invoicingBase))
			{
				invoicingBase.AH_Calc_AmendStatusCodeInfo.AddError(Res.GetString("9CD6A90A-39D0-4E42-AEDA-E6AD6B2572C0", "An amendment status code is required for the amending transaction. Please select an amendment status code."));
			}
			ListValidation.ErrorIfInvalidCode(invoicingBase.AH_Calc_AmendStatusCodeInfo, invoicingBase.Lookups.AmendStatusCodeList);
		}

		public virtual void ValidateAmendStatusCodeForInvoice(InvoicingBase invoicingBase)
		{
		}

		public virtual void ValidateAmendStatusCodeForInvoiceReversal(InvoicingBase invoicingBase)
		{
			ValidateAmendStatusCode(invoicingBase);
		}

		protected virtual bool ShouldValidateAmendStatusCode(InvoicingBase invoicingBase) => true;
	}
}
