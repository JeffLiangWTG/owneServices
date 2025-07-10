using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class DeclarationAuthorLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var controlBag = AuthorAndAuditorControlBag.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(controlBag);

			var captionRuler = layout.CreateRuler(100);

			layout.Include(captionRuler, controlBag.AuthorGuidDropEdit, controlBag.AuthorNameTextBox);
			layout.Include(captionRuler, controlBag.AuthorPhoneTextBox);
			layout.Include(captionRuler, controlBag.AuthorJobTitleTextBox);

			return layout;
		}
	}
}
