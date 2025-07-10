using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ComplianceReport.PTRS
{
	public class PtrsReportDateColumn : PtrsReportColumnWrapperBase
	{
		public PtrsReportDateColumn(AccTaxReturnColumn column) : base(column)
		{
		}

		public ZDateTime Value
		{
			get => ZDateTime.TryParseExact(column.ATC_Comment, out var result, "dd/MM/yyyy") ? result : ZDateTime.Empty;
			set => column.ATC_Comment = value.ToString("dd/MM/yyyy");
		}

		protected override ZPropertyInfo GetValueInfo() => GetWrappedZPropertyInfo(nameof(Value), _ => column.ATC_CommentInfo);
	}
}
