using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class COAndFTADetailsUserControl : ZUserControl
	{
		public COAndFTADetailsUserControl()
		{
			InitializeComponent();

			CertificateOfOriginPanel.UpdateLayout(new COOLayout());
			CertificateOfOriginIssuePanel.UpdateLayout(new COOIssueLayout());
		}
	}
}
