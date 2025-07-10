using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class Question10Layout : IPanelLayoutProvider
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

			layout.Include(controlBag.Question10Label);
			layout.Include(captionRuler, controlBag.Question10ALabel, answerRuler, controlBag.Question10ADropEdit);
			layout.Include(captionRuler, controlBag.Question10BLabel, answerRuler, controlBag.Question10BDropEdit);
			layout.Include(captionRuler, controlBag.Question10CLabel, answerRuler, controlBag.Question10CDropEdit);
			layout.Include(captionRuler, controlBag.Question10DLabel, answerRuler, controlBag.Question10DDropEdit);

			return layout;
		}
	}
}
