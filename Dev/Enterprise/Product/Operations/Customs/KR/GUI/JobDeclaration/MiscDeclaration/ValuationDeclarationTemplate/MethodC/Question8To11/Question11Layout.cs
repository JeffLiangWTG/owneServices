using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class Question11Layout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var controlBag = Question8To11ControlBag.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(controlBag);

			var captionRuler = layout.CreateRuler(20);
			var answerRuler = layout.CreateRuler(730);

			layout.Include(controlBag.Question11Label);
			layout.Include(captionRuler, controlBag.Question11ALabel, answerRuler, controlBag.Question11ADropEdit);
			layout.Include(captionRuler, controlBag.Question11BLabel, answerRuler, controlBag.Question11BDropEdit);
			layout.Include(captionRuler, controlBag.Question11CLabel, answerRuler, controlBag.Question11CDropEdit);
			layout.Include(captionRuler, controlBag.Question11DLabel, answerRuler, controlBag.Question11DDropEdit);

			return layout;
		}
	}
}
