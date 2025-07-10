using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.Business
{
	public class JobComInvoiceLineTaxValidation : EU.Business.Declaration.JobComInvoiceLineTaxValidation
	{
		public JobComInvoiceLineTaxValidation(JobComInvoiceLineTax parent) : base(parent)
		{
		}

		TaxValidationHelper CommonValidation => TaxValidationHelper.GetTaxValidationHelper(Parent, Parent.InvoiceLine.Taxes.OfType<EU.Business.Declaration.IEuTax>()); // Don't cache - need to see latest version of .Taxes each time

		protected override void CheckJLT_Calc_RateDuty()
		{
			base.CheckJLT_Calc_RateDuty();
			ListValidation.MessageErrorIfInvalidCode(Parent.JLT_Calc_RateDutyInfo, Parent.Lookups.RateDutyList);
		}

		protected override void CheckJLT_BaseQuantityUQ()
		{
			base.CheckJLT_BaseQuantityUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.JLT_BaseQuantityUQInfo, Parent.Lookups.BaseQuantityUQList);
		}

		protected override void CheckJLT_MethodOfPayment()
		{
			base.CheckJLT_MethodOfPayment();
			CommonValidation.CheckTaxTypeAndMop(Parent.JLT_MethodOfPaymentInfo);
		}

		protected override void CheckJLT_Calc_RateSuspension()
		{
			base.CheckJLT_Calc_RateSuspension();
			ListValidation.MessageErrorIfInvalidCode(Parent.JLT_Calc_RateSuspensionInfo, Parent.Lookups.RateSuspensionList);
		}

		protected override void CheckJLT_RateOverrideReasonCode()
		{
			base.CheckJLT_RateOverrideReasonCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.JLT_RateOverrideReasonCodeInfo, Parent.Lookups.RateOverrideList);
		}
	}
}
