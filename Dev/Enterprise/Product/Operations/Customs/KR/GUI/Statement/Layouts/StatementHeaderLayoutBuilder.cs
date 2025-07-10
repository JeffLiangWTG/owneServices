using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class StatementHeaderLayoutBuilder<T> : ColumnLayoutBuilder<T, StatementHeaderControlBag> where T : CusStatementHeader
	{
		public override StatementHeaderControlBag CommonBag => StatementHeaderControlBag.Instance;

		protected override int MaxColumns => 2;
		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
