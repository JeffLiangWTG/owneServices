using System;
using CargoWise.EntityFramework;
using Enterprise.Client.ZClientCCP.Business;
using Enterprise.Client.ZClientCCP.GUI;
using Enterprise.Client.ZClientCCP.Kawasaki;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.ZClientCCP.Module
{
	/// <summary>
	/// Summary description for KawasakiImportController.
	/// </summary>
	public class KawasakiImportController
	{
		public KawasakiImportController()
		{
			ImporterEventsAttached = false;
			fKawasakiDataTransfer = new KawasakiDataTransfer();
			Factory = new BusinessObjectFactory();
			JobDec = (BaseJobDeclaration)Factory.New(typeof(BaseJobDeclaration));
			JobDec.IsImportingData = true;
			KawasakiSupplierDataTransfer = new KawasakiDataTransferSupplySupplier(JobDec, DialogFilter, FormHeading);
		}

		public void ShowDialog()
		{
			TransferForm = new KawasakiDataTransferForm(KawasakiSupplierDataTransfer);
			TransferForm.StartProcess += new ProcessFileEventHandler(ImporterForm_StartProcess);
			TransferForm.ProcessCancelled += new EventHandler(IscarImporterForm_Cancelled);
			ZFormModaliser.ShowDialogAndDispose(TransferForm);
		}

		public void SaveInvoices()
		{
			Factory.Save();
		}

		bool ImporterEventsAttached;
		protected KawasakiDataTransferForm TransferForm;
		protected KawasakiDataTransfer fKawasakiDataTransfer;
		protected BaseJobDeclaration JobDec;
		protected BusinessObjectFactory Factory;
		protected KawasakiDataTransferSupplySupplier KawasakiSupplierDataTransfer;
		const string DialogFilter = "Comma delimited files (*.csv)|*.csv|Text files (*.txt)|*.txt|All files (*.*)|*.*";
		const string FormHeading = "Kawasaki Invoice Importation";

		protected void ImporterForm_StartProcess(object sender, ProcessFileEventArgs e)
		{
			if (!ImporterEventsAttached)
			{
				fKawasakiDataTransfer.Processed += new ProcessedEventHandler(DataImporter_FileRowProcessed);
				fKawasakiDataTransfer.ProcessCompleted += new EventHandler(DataImporter_ProcessCompleted);
				ImporterEventsAttached = true;
			}

			string fileName = e.UnmappedFileName;
			using (ZOpenFileDialog.ForceLocalFile(ref fileName))
			{
				if (!fKawasakiDataTransfer.ImportInvoices(JobDec, fileName))
				{
					Globals.Message.ShowError(fKawasakiDataTransfer.ErrorMessage, "Error");
					TransferForm.Close();
				}
			}
		}

		protected void IscarImporterForm_Cancelled(object sender, EventArgs e)
		{
			fKawasakiDataTransfer.CancelImport();
		}

		protected void DataImporter_FileRowProcessed(object sender, ProcessedEventArgs e)
		{
			TransferForm.SetProcessProgress(e.PercentageComplete, e.ProcessedCount, e.FailureCount, e.LogEntry);
		}

		protected void DataImporter_ProcessCompleted(object sender, EventArgs e)
		{
			TransferForm.FinishProcess();
		}
	}
}
