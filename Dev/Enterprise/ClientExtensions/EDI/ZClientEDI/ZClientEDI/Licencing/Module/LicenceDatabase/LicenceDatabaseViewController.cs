using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.GUI;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Module
{
	public class LicenceDatabaseViewController : ILicenceDatabaseViewController
	{
		public void OnSendNewSystemShutdownDate(LicenceDatabase db)
		{
			var action = new LicenceDatabaseSendSystemExpiryAction(db);
			action.SendNewSystemShutdownDate();
		}
	}
}
