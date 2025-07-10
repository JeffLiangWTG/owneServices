namespace Enterprise.Accounting.Business.ComplianceReport.PTRS
{
	public class PtrsReportRegNumberColumn : PtrsReportCommentColumn
	{
		public PtrsReportRegNumberColumn(AccTaxReturnColumn column) : base(column)
		{
			MaxLength = ABNLength;
		}

		public const int ABNLength = 11;
		public const int ACNLength = 9;
		public const int BICLength = 5;

		protected override void ValueInfo_AdditionalValidation()
		{
			base.ValueInfo_AdditionalValidation();

			if (!Value.IsEmpty && !Value.IsNumbersOnlyOrEmpty)
			{
				ValueInfo.AddError(Res.GetString("1e921133-9a4c-40f7-bf56-2c019afa0678", "Should contain numbers only."));
			}
		}
	}
}
