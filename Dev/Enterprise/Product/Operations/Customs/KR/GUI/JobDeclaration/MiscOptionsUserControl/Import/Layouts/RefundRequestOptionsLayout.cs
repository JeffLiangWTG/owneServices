using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class RefundRequestOptionsLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var controlBag = MiscOptionsControlBag.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(controlBag);

			var captionWidth = (int)ColumnLayoutBuilderCaptionWidthSize.Medium + 50;
			var captionRuler = layout.CreateRuler(captionWidth);

			layout.Include(0, captionRuler, controlBag.TaxOfficeCodeFindBox);

			return layout;
		}
	}
}
