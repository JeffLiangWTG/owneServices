using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ComplianceReport.PTRS
{
	public class PtrsReportEmailColumn : PtrsReportCommentColumn
	{
		public PtrsReportEmailColumn(AccTaxReturnColumn column) : base(column)
		{
			MaxLength = 50;
		}

		protected override void ValueInfo_AdditionalValidation()
		{
			base.ValueInfo_AdditionalValidation();

			EmailAddressValidation.ValidateEmailAddress(ValueInfo);
		}
	}
}
