using System.Windows.Forms;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.GUI;
using Enterprise.Client.EDI.LicenceKeyBuilder.Module;
using Enterprise.Client.EDI.MasterFiles.Business;

namespace Enterprise.Client.EDI.Licencing.Module
{
	public class LicenceViewController : ILicenceViewController
	{
		public void OnGenerateLicenceKey(Form form, LicenceDatabase db, EDIOrgHeader org)
		{
			LicenceAction licenceAction = new LicenceAction(form, null, db, org, false);
			licenceAction.SendEmailLicence(null, null);
		}

		public void OnUpdateLicenceRemotely(Form form, LicenceDatabase db, EDIOrgHeader org)
		{
			LicenceAction licenceAction = new LicenceAction(form, null, db, org, false);
			licenceAction.UpdateLicenceRemotely(null, null);
		}

		public void OnGenerateCompanyNativeXmlEmail(EDIOrgHeader org)
		{
			var action = new GenerateCompanyNativeXmlEmailAction();
			action.ExportAndEmail(org);
		}

		public ILicenceDatabaseViewController GetLicenceDatabaseViewController()
		{
			return new LicenceDatabaseViewController();
		}
	}
}
