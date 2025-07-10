using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class Question8Layout : IPanelLayoutProvider
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

			layout.Include(controlBag.Question8Label);
			layout.Include(captionRuler, controlBag.Question8ALabel, answerRuler, controlBag.Question8ADropEdit);
			layout.Include(captionRuler, controlBag.Question8BLabel, answerRuler, controlBag.Question8BDropEdit);
			layout.Include(captionRuler, controlBag.Question8CLabel, answerRuler, controlBag.Question8CDropEdit);
			layout.Include(captionRuler, controlBag.Question8DLabel, answerRuler, controlBag.Question8DDropEdit);

			return layout;
		}
	}
}
