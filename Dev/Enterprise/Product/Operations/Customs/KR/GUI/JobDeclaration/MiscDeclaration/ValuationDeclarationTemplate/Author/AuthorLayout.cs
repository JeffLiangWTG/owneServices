using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class AuthorLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var controlBag = AuthorAndAuditorControlBag.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(controlBag);

			var captionWidth = 80;
			var captionWidth2 = 0;
			var authorDropEditWidth = 104;
			var nameTextBoxWidth = 100;
			var phoneTextBoxWidth3 = 170;
			var jobTitleTextBoxWidth4 = 200;

			var captionRuler1 = layout.CreateRuler(captionWidth);
			var columnWidthRuler1 = layout.CreateRuler(captionWidth + authorDropEditWidth);
			var captionRuler2 = layout.CreateRuler(captionWidth2);
			var columnWidthRuler2 = layout.CreateRightRuler(nameTextBoxWidth);
			var captionRuler3 = layout.CreateRuler(captionWidth);
			var columnWidthRuler3 = layout.CreateRightRuler(captionWidth + phoneTextBoxWidth3);
			var captionRuler4 = layout.CreateRuler(captionWidth);
			var columnWidthRuler4 = layout.CreateRightRuler(captionWidth + jobTitleTextBoxWidth4);

			layout.Include(0, captionRuler1, controlBag.AuthorGuidDropEdit, columnWidthRuler1);
			layout.AddColumn();
			layout.Include(1, captionRuler2, controlBag.AuthorNameTextBox, columnWidthRuler2);
			layout.AddColumn();
			layout.Include(2, captionRuler3, controlBag.AuthorPhoneTextBox, columnWidthRuler3);
			layout.AddColumn();
			layout.Include(3, captionRuler4, controlBag.AuthorJobTitleTextBox, columnWidthRuler4);

			return layout;
		}
	}
}
