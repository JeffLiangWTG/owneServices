using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class PenaltyDeclarationOptionsLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var controlBag = MiscOptionsControlBag.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(controlBag);

			var captionWidth = (int)ColumnLayoutBuilderCaptionWidthSize.Long + 30;
			var captionRuler = layout.CreateRuler(captionWidth);
			var mediumWidthRuler = layout.CreateRightRuler(captionWidth + 200);
			var shortWidthRuler = layout.CreateRightRuler(captionWidth + 26);

			layout.Include(0, captionRuler, controlBag.LateDecPenaltyDateCodeDropEdit, mediumWidthRuler);
			layout.Include(0, captionRuler, controlBag.MissedDecPenaltyRateCalcEdit, shortWidthRuler, controlBag.PercentageLabel);

			return layout;
		}
	}
}
