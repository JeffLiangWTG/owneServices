using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class Question9Layout : IPanelLayoutProvider
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
			var questionRuler = layout.CreateRightRuler(750);

			layout.Include(controlBag.Question9Label);
			layout.Include(captionRuler, controlBag.Question9ALabel, answerRuler, controlBag.Question9ADropEdit);
			layout.Include(captionRuler, controlBag.Question9BLabel, answerRuler, controlBag.Question9BDropEdit);

			return layout;
		}
	}
}
