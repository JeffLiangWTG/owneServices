using Enterprise.Customs.GUI;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class MiscOptionsLayoutBuilder : CommonMiscOptionsLayoutBuilder<JobDeclaration>
	{
		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();
			SetVisibility(CommonBag.MergeByDropEdit, h => !h.IsLocalExport, h => h.JE_MessageTypeInfo);
		}
	}
}
