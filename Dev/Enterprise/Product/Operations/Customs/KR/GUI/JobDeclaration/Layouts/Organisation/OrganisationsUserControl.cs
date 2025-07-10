using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class OrganisationsUserControl : ZUserControl
	{
		public OrganisationsUserControl()
		{
			InitializeComponent();

			AuthorPanel.UpdateLayout(new DeclarationAuthorLayout());
			ResponsiblePersonPanel.UpdateLayout(new AuditorLayout());
		}
	}
}
