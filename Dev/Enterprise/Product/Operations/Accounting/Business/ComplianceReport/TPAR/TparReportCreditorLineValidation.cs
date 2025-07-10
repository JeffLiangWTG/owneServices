using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ComplianceReport.TPAR
{
	public class TparReportCreditorLineValidation : AccTaxReturnLineValidation
	{
		public TparReportCreditorLineValidation(TparReportCreditorLine parent) : base(parent)
		{
		}

		new TparReportCreditorLine Parent => (TparReportCreditorLine)base.Parent;

		protected override void CheckARL_OverriddenTotalAmountIncludingTax()
		{
			base.CheckARL_OverriddenTotalAmountIncludingTax();
			MandatoryValidation.CheckNotNegative(Parent.ARL_OverriddenTotalAmountIncludingTaxInfo);
			TypeValidation.CheckValidDecimal(Parent.ARL_OverriddenTotalAmountIncludingTaxInfo, 11, 0);
		}

		protected override void CheckARL_OverriddenGSTAmount()
		{
			base.CheckARL_OverriddenGSTAmount();
			MandatoryValidation.CheckNotNegative(Parent.ARL_OverriddenGSTAmountInfo);
			TypeValidation.CheckValidDecimal(Parent.ARL_OverriddenGSTAmountInfo, 11, 0);
			CompareValidation.CheckNumberLessThanOrEqualToOtherNumber(Parent.ARL_OverriddenGSTAmountInfo, Parent.ARL_OverriddenTotalAmountIncludingTaxInfo);
		}

		protected override void CheckARL_OverriddenPaymentsBasisWithholdingTaxAmount()
		{
			base.CheckARL_OverriddenPaymentsBasisWithholdingTaxAmount();
			MandatoryValidation.CheckNotNegative(Parent.ARL_OverriddenPaymentsBasisWithholdingTaxAmountInfo);
			TypeValidation.CheckValidDecimal(Parent.ARL_OverriddenPaymentsBasisWithholdingTaxAmountInfo, 11, 0);
			CompareValidation.CheckNumberLessThanOrEqualToOtherNumber(Parent.ARL_OverriddenPaymentsBasisWithholdingTaxAmountInfo, Parent.ARL_OverriddenTotalAmountIncludingTaxInfo);
		}
	}
}
