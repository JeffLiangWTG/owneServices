using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class Question6Layout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var controlBag = Question5To7ControlBag.InstanceFor5SM;
			var layout = new PanelLayout();
			layout.RegisterControlBag(controlBag);

			var captionRuler = layout.CreateRuler(20);
			var answerRuler = layout.CreateRuler(730);

			layout.Include(captionRuler, controlBag.Question6ALabel, answerRuler, controlBag.Question6ADropEdit);
			layout.Include(captionRuler, controlBag.Question6BLabel, answerRuler, controlBag.Question6BDropEdit);

			return layout;
		}
	}
}
