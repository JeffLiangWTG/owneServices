using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ComplianceReport.PTRS
{
	public class PtrsReportCommentColumn : PtrsReportColumnWrapperBase, IHaveMaxLength
	{
		public PtrsReportCommentColumn(AccTaxReturnColumn column) : base(column)
		{
			MaxLength = AccTaxReturnColumn.Schema.ATC_CommentMaxLength;
		}

		int IHaveMaxLength.MaxLength { get => MaxLength; set => MaxLength = value; }

		protected int MaxLength { get; set; }

		public ZString Value { get => column.ATC_Comment; set => column.ATC_Comment = value; }

		protected override ZPropertyInfo GetValueInfo() => GetWrappedZPropertyInfo(nameof(Value), _ => column.ATC_CommentInfo);

		protected int Value_MaxLength => MaxLength > AccTaxReturnColumn.Schema.ATC_CommentMaxLength
			? AccTaxReturnColumn.Schema.ATC_CommentMaxLength
			: MaxLength;

		protected override void ValueInfo_AdditionalValidation()
		{
			base.ValueInfo_AdditionalValidation();

			var infoMaxLength = ValueInfo.MaxLength;
			if (infoMaxLength > -1 && Value.Length > infoMaxLength)
			{
				var error = Res.GetString("604c704f-06b7-4de3-88e6-3e9e55fa0fe7", "The maximum length of '{0}' has been exceeded. The maximum length of this property is {1} characters, but {2} were entered.", ValueInfo.Name, infoMaxLength, Value.Length);
				ValueInfo.AddError(error);
			}
		}
	}
}
