using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public static class SecurityHelperClass
	{
		public static bool RequestEditAuthorisation(IJobInvoicingPlugIn plugin)
		{
			do
			{
				using (LoginForm logForm = new LoginForm())
				{
					logForm.Message = plugin.InvoicingSupporter.EditSecurityMessage;

					DialogResult dlgResult = ZFormModaliser.ShowDialogWithoutDispose(logForm);

					if (dlgResult != DialogResult.OK)
					{
						return false;
					}

					if (logForm.Credentials != null &&
						logForm.Credentials.UserSecurity != null &&
						logForm.Credentials.UserSecurity.FindCheckPoint(plugin.InvoicingSupporter.EditSecurityCheckpoint.LookupKey).IsAllowed)
					{
						return true;
					}

					Globals.Message.Show(Res.GetString("bd96a18b-23dc-4cff-abc9-59d5f68f0833", "User does not exist or password is invalid"), Res.GetString("c17aea12-9e07-45de-acbb-a5945c1f481f", "Invoice Amendments Days"), MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			} while (true);
		}
	}
}
