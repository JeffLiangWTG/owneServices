using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class AuditorLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var controlBag = AuthorAndAuditorControlBag.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(controlBag);

			var captionRuler = layout.CreateRuler(100);

			layout.Include(captionRuler, controlBag.AuditorGuidDropEdit, controlBag.AuditorNameTextBox);
			layout.Include(captionRuler, controlBag.AuditorPhoneTextBox);
			layout.Include(captionRuler, controlBag.AuditorJobTitleTextBox);

			return layout;
		}
	}
}
