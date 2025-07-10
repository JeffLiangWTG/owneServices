using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class ValuationDeclarationQuestion9Layout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var controlBag = Question5To7ControlBag.InstanceFor934;
			var layout = new PanelLayout();
			layout.RegisterControlBag(controlBag);

			var captionRuler = layout.CreateRuler(50);
			var answerRuler = layout.CreateRuler(1000);

			layout.Include(captionRuler, controlBag.Question7ALabel, answerRuler, controlBag.Question7ADropEdit);
			layout.Include(captionRuler, controlBag.Question7BLabel, answerRuler, controlBag.Question7BDropEdit);

			return layout;
		}
	}
}
