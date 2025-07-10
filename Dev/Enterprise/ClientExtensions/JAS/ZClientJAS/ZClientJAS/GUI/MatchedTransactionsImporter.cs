using System;
using Enterprise.Client.JAS.Business.Matching;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.JAS.GUI
{
	public class MatchedTransactionsImporter
	{
		public MatchedTransactionsImporter()
		{
		}

		DataTransferForm fImporterForm;
		public DataTransferForm ImporterForm
		{
			get
			{
				if (fImporterForm == null)
				{
					string fileFilter = "Matched transaction files (*.out)|*.out|Text files (*.txt)|*.txt|All files (*.*)|*.*";
					fImporterForm = new DataTransferForm(fileFilter, "Import Matched Transactions");
					fImporterForm.StartProcess += new ProcessFileEventHandler(fImporterForm_StartProcess);
					fImporterForm.ProcessCancelled += new EventHandler(fImporterForm_ProcessCancelled);
				}
				return fImporterForm;
			}
		}

		MatchedDataImporter fImporter;
		protected MatchedDataImporter Importer
		{
			get
			{
				if (fImporter == null)
				{
					fImporter = new MatchedDataImporter();
					fImporter.ProcessCompleted += new EventHandler(fImporter_ProcessCompleted);
					fImporter.Processed += new ProcessedEventHandler(fImporter_Processed);
					fImporter.MatchedDataImported += new MatchedDataImportedEventHandler(fImporter_MatchedDataImported);
				}
				return fImporter;
			}
		}

		#region Implementation

		void fImporterForm_StartProcess(object sender, ProcessFileEventArgs e)
		{
			Importer.CancelImport = false;
			string fileName = e.UnmappedFileName;
			using (ZOpenFileDialog.ForceLocalFile(ref fileName))
			{
				Importer.Import(fileName);
			}
		}

		void fImporter_ProcessCompleted(object sender, EventArgs e)
		{
			ImporterForm.FinishProcess();
		}

		void fImporter_Processed(object sender, ProcessedEventArgs e)
		{
			ImporterForm.SetProcessProgress(e.PercentageComplete, e.ProcessedCount, e.FailureCount, e.LogEntry);
		}

		void fImporterForm_ProcessCancelled(object sender, EventArgs e)
		{
			Importer.CancelImport = true;
		}

		void fImporter_MatchedDataImported(MatchedDataLine line)
		{
			// TODO by the Accounting team
			// Import data to enterprise			
		}

		#endregion
	}
}
