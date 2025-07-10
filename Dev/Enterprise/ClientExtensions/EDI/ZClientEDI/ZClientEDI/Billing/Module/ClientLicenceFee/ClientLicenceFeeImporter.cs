using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.DataMapping;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.Billing.Module
{
	public class ClientLicenceFeeImporter
	{
		public ClientLicenceFeeImporter()
		{
		}

		public void Import()
		{
			var importInfo = new ClientLicenceFeeImportInfo(new ClientLicenceFeeFlattenedCollection(new BusinessObjectFactory()));
			var processor = new ClientLicenceFeeFlattenedDataTransferProcessor(new ClientLicenceFeeCollectionNonDependent(new BusinessObjectFactory()), importInfo);
			const string contextKey = "{163B1FD5-9142-4F44-945C-A05C88CB2530}";
			var isCancelled = false;
			var form = new MultistepDataImportWizardForm(importInfo, contextKey, new DataTransferProcessor[] { processor }, "Licence Fee");
			form.Cancelled += (s, e) => { processor.Rollback(); isCancelled = true; };
			form.Imported += (s, e) => { DisplayResult(processor, isCancelled); };
			form.Show();
		}

		void DisplayResult(ClientLicenceFeeFlattenedDataTransferProcessor processor, bool isCancelled)
		{
			string message;
			string caption;
			if (isCancelled)
			{
				message = Res.GetString("1641bb28-a4c8-4a3d-a08c-1548a4a55e43", "No record was imported.");
				caption = Res.GetString("58dd950e-a724-4e4e-834d-c1971f02ec3e", "Import canceled");
			}
			else
			{
				message = Res.GetString("4ce9772a-d5a3-4890-9105-876a2229f8ac", "Records to import = {0}", processor.HeadersToCreate) + "\r\n";
				message += processor.Log;
				message += "\r\n" + Res.GetString("b83dcea9-9be9-43a4-9d7d-aaef0e9863ca", "TOTAL: Records imported = {0}, Records excluded = {1}",
					processor.HeadersCreated, processor.HeadersExcluded) + "\r\n";
				caption = Res.GetString("01fb08fc-dabb-4e3a-aab4-edf01e9258b3", "Import completed");
			}
			using (ZMessageBox notification = new ZMessageBox(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information))
			{
				notification.ShowDialog();
			}
		}
	}
}
