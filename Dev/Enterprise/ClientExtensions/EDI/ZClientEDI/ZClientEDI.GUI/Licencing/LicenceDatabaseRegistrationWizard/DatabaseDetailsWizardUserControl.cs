using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Licencing.GUI
{
	public partial class DatabaseDetailsWizardUserControl : ZUserControl
	{
		public DatabaseDetailsWizardUserControl(LicenceCompanyLicenceDatabaseCollection licenceDatabaseCollection)
			: base()
		{
			InitializeComponent();
			SetDataBinding(licenceDatabaseCollection, null);
		}
	}
}
