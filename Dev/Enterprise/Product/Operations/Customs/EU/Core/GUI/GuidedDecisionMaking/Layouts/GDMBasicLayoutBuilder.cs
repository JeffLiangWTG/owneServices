using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class GDMBasicLayoutBuilder<T> : ColumnLayoutBuilder<T, GDMBasicControlBag>
		where T : GuidedDecisionMakingBasic
	{
		public override GDMBasicControlBag CommonBag { get; } = GDMBasicControlBag.Instance;

		protected override int MaxColumns => 1;

		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();

			SetVisibility(CommonBag.PreferenceDropEdit, x => x.IsImport, x => x.IsImportInfo);
			SetVisibility(CommonBag.QuotaOrderNumberDropEdit, x => x.IsImport, x => x.IsImportInfo);
			SetVisibility(CommonBag.CountryOfOriginDropEdit, x => x.IsImport, x => x.IsImportInfo);
			SetVisibility(CommonBag.CountryOfDestinationDropEdit, x => x.IsExport, x => x.IsExportInfo);
		}
	}
}
