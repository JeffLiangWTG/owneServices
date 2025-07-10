using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class CompanyCredentialsUserControl : ZUserControl
	{
		public CompanyCredentialsUserControl()
		{
			InitializeComponent();
			UnipassCertificateDynamicLayoutPanel.UpdateLayout(new UnipassCertificateLayout());
		}
	}
}
