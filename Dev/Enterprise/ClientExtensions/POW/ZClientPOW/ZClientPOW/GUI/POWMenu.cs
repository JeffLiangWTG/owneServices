using System;
using System.Windows.Forms;
using Enterprise.Client.ZClientPOW.Suzuki;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.ZClientPOW.GUI
{
	public class POWMenu : Enterprise.Customs.AU.Declaration.GUI.EDIMenu
	{
		static Enterprise.Customs.AU.Declaration.GUI.EDIMenu NewDelegate()
		{
			return new POWMenu();
		}

		public static void Initialise()
		{
			OverridableNewDelegate.Value = new ConstructorDelegate(NewDelegate);
		}

		protected override void SetupTopLevelMenu()
		{
			base.SetupTopLevelMenu();
			MenuItem importCSVMenuItem = new ZMenuItem("Import Suzuki", new EventHandler(onDataClick));
			dataMenuItem.MenuItems.Add(importCSVMenuItem);
		}

		protected void onDataClick(object sender, EventArgs e)
		{
			DataTransferForm = new DataTransferForm();
			DataTransferForm.DialogFilter = "Text files (*.txt)|*.txt|Comma delimited files (*.csv)|*.csv|All files (*.*)|*.*";
			DataTransferForm.StartProcess += new ProcessFileEventHandler(SuzukiImporterForm_StartProcess);
			DataTransferForm.ProcessCancelled += new EventHandler(SuzukiImporterForm_Cancelled);
			ZFormModaliser.ShowDialogAndDispose(DataTransferForm);
		}

		protected void DataImporter_FileRowProcessed(object sender, ProcessedEventArgs e)
		{
			DataTransferForm.SetProcessProgress(e.PercentageComplete, e.ProcessedCount, e.FailureCount, e.LogEntry);
		}

		protected void DataImporter_ProcessCompleted(object sender, EventArgs e)
		{
			DataTransferForm.FinishProcess();
		}

		protected void SuzukiImporterForm_Cancelled(object sender, EventArgs e)
		{
			fSuzukiDataTransfer.CancelImport();
		}

		protected void SuzukiImporterForm_StartProcess(object sender, ProcessFileEventArgs e)
		{
			if (!SuzukiImporterEventsAttached)
			{
				SuzukiDataTransfer.Processed += new ProcessedEventHandler(DataImporter_FileRowProcessed);
				SuzukiDataTransfer.ProcessCompleted += new EventHandler(DataImporter_ProcessCompleted);
				SuzukiImporterEventsAttached = true;
			}

			string fileName = e.UnmappedFileName;
			using (ZOpenFileDialog.ForceLocalFile(ref fileName))
			{
				if (!SuzukiDataTransfer.ImportInvoices(Declaration, fileName))
				{
					Globals.Message.ShowError(SuzukiDataTransfer.ErrorMessage, "Error");
					DataTransferForm.Close();
				}
			}
		}

		protected SuzukiDataTransfer SuzukiDataTransfer
		{
			get
			{
				if (fSuzukiDataTransfer == null)
				{
					fSuzukiDataTransfer = new SuzukiDataTransfer();
				}

				return fSuzukiDataTransfer;
			}
		}

		bool SuzukiImporterEventsAttached;
		SuzukiDataTransfer fSuzukiDataTransfer;
		protected DataTransferForm DataTransferForm;
	}
}
