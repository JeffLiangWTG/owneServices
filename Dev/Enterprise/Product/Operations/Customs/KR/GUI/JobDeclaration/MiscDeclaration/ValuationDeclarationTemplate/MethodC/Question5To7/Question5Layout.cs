using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class Question5Layout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var controlBag = Question5To7ControlBag.InstanceFor5SM;
			var layout = new PanelLayout();
			layout.RegisterControlBag(controlBag);

			var captionRuler = layout.CreateRuler(20);
			var textBoxCaptionRuler = layout.CreateRuler(245);
			var answerRuler = layout.CreateRuler(730);

			layout.Include(captionRuler, controlBag.Question5ALabel, answerRuler, controlBag.Question5ADropEdit);
			layout.Include(captionRuler, controlBag.Question5BLabel, answerRuler, controlBag.Question5BDropEdit);
			layout.Include(captionRuler, controlBag.Question5CLabel, answerRuler, controlBag.Question5CDropEdit);
			layout.Include(captionRuler, controlBag.Question5DLabel, answerRuler, controlBag.Question5DDropEdit);
			layout.Include(captionRuler, controlBag.Question5ELabel, answerRuler, controlBag.Question5EDropEdit);
			layout.Include(captionRuler, controlBag.Question5ETextLabel, textBoxCaptionRuler, controlBag.Question5ETextBox);

			return layout;
		}
	}
}
