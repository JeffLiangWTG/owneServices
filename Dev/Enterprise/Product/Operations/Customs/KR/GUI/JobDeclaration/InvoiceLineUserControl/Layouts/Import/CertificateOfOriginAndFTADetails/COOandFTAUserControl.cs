using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class COOandFTAUserControl : ZUserControl
	{
		public COOandFTAUserControl()
		{
			InitializeComponent();

			CertificateOfOriginPanel.UpdateLayout(new CertificateOfOriginLayout());
			CertificateOfOriginIssuePanel.UpdateLayout(new CertificateOfOriginIssueLayout());
			FTADetailsPanel.UpdateLayout(new InvLineFTADetailsLayout());
			DetailedFTAPanel.UpdateLayout(new DetailedFTALayout());
		}
	}
}
