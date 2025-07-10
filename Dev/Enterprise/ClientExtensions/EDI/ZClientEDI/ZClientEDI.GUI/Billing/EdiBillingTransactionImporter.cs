using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.DataMapping;

namespace Enterprise.Client.EDI.Billing.GUI
{
	public class EdiBillingTransactionImporter
	{
		public EdiBillingTransactionImporter()
		{
		}

		public void Import()
		{
			var importInfo = new EdiBillingTransactionImportInfo(new EdiBillingTransactionFlattenedCollection(new BusinessObjectFactory()));
			var processor = new EdiBillingTransactionFlattenedDataTransferProcessor(importInfo);
			const string contextKey = "{e6977c27-410b-4bb2-9e98-3702939ceaa2}";
			var isCancelled = false;
			var form = new MultistepDataImportWizardForm(importInfo, contextKey, new DataTransferProcessor[] { processor }, "Non-CW1 Usages");
			form.Cancelled += (s, e) => { processor.Rollback(); isCancelled = true; };
			form.Imported += (s, e) => { DisplayResult(processor, isCancelled); };
			form.Show();
		}

		void DisplayResult(EdiBillingTransactionFlattenedDataTransferProcessor processor, bool isCancelled)
		{
			if (processor.FlattenedCollection.HasErrors())
			{
				var msg = new ZErrorMessageBox(processor.FlattenedCollection, "There are errors that need to be corrected before the Transactions can be imported.", "Import failed.");
				ZFormModaliser.ShowDialogAndDispose(msg);
			}
			else
			{
				Globals.Message.ShowInformation("Import completed");
			}
		}
	}
}
