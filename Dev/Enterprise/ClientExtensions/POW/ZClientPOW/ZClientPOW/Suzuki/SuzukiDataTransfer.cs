using System;
using System.IO;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.ZClientPOW.Suzuki
{
	public class SuzukiDataTransfer : IDataTransfer
	{
		public SuzukiDataTransfer()
		{
		}

		public bool ImportInvoices(BaseJobDeclaration toJobDec, string fileName)
		{
			bool result = false;

			if (File.Exists(fileName))
			{
				try
				{
					result = DoFlatFileImport(toJobDec, fileName);
				}
				catch (IOException)
				{
					fErrorMessage = "File cannot be accessed. It might be in use by another program";
				}
			}
			else
			{
				fErrorMessage = string.Format("The File '{0}' does not exist", fileName);
			}

			return result;
		}

		#region Implementation

		protected bool DoFlatFileImport(BaseJobDeclaration toJobDec, string fileName)
		{
			Importer = GetFlatFileImporter(fileName, toJobDec);
			DoImport(Importer, toJobDec);
			return true;
		}

		protected void DoImport(SuzukiInvoiceDataImporter importer, BaseJobDeclaration toJobDec)
		{
			using (toJobDec.InvoiceLines.SuspendInvoiceLineListChanged())
			{
				toJobDec.InvoiceLines.SuspendValidation();
				try
				{
					importer.LogEvent += new EventHandler(Importer_LogEvent);

					importer.Import();
					if (ProcessCompleted != null)
					{
						ProcessCompleted(this, new EventArgs());
					}
				}
				finally
				{
					toJobDec.InvoiceLines.ResumeValidation();
				}
			}
		}

		protected virtual SuzukiInvoiceDataImporter GetFlatFileImporter(string fileName, BaseJobDeclaration toJobDec)
		{
			return new SuzukiInvoiceDataImporter(fileName, toJobDec);
		}

		void Importer_LogEvent(object sender, EventArgs e)
		{
			if (Importer != null)
			{
				FireProcessedEvent(Importer.PercentageComplete, Importer.ProcessedRecordCount, Importer.FailedRecordCount, Importer.LogEntry);
			}
		}

		protected void FireProcessedEvent(int percentageComplete, int processedRecordCount, int failedRecordCount, string logEntry)
		{
			if (Processed != null)
			{
				Processed(this, new ProcessedEventArgs(percentageComplete, processedRecordCount, failedRecordCount, logEntry));
			}
		}

		public void CancelImport()
		{
			if (Importer != null)
			{
				Importer.CancelImport = true;
			}
		}

		public string ErrorMessage
		{
			get { return fErrorMessage ?? string.Empty; }
		}
		protected string fErrorMessage;

		protected SuzukiInvoiceDataImporter Importer;

		#endregion

		#region events
		public event ProcessedEventHandler Processed;
		public event EventHandler ProcessCompleted;
		#endregion
	}
}

#region Setup
#endregion
