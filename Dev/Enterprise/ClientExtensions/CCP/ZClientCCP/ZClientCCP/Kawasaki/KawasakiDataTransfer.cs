using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.ZClientCCP.Kawasaki
{
	public class KawasakiDataTransfer : IDataTransfer
	{
		public KawasakiDataTransfer()
		{
		}

		public bool ImportInvoices(BaseJobDeclaration toJobDec, string fileName)
		{
			bool result = false;

			if (File.Exists(fileName))
			{
				try
				{
					Importer = GetFlatFileImporter(fileName, toJobDec);
					result = DoFlatFileImport(toJobDec);
				}
				catch (IndexOutOfRangeException)
				{
					var indexedLines = Importer.DataReader.InternalFileLines.Select((line, index) => string.Concat(index, line)).ToList();
					var lines = indexedLines.Where(line => new OCsvLine(line).FieldValues.Length < KawasakiInvoiceDataFileReader.Constants.RecordLength);
					if (lines.Any())
					{
						StringBuilder errorMessage = new StringBuilder();
						errorMessage.AppendLine("The following lines do not have enough fields:");
						foreach (var line in lines)
						{
							errorMessage.Append("Line ");
							errorMessage.Append((indexedLines.IndexOf(line) + 1).ToString(CultureInfo.CurrentCulture));
							if (line != lines.Last())
							{
								errorMessage.AppendLine();
							}
						}
						fErrorMessage = errorMessage.ToString();
					}
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

		protected bool DoFlatFileImport(BaseJobDeclaration toJobDec)
		{
			DoImport(toJobDec);
			return true;
		}

		protected void DoImport(BaseJobDeclaration toJobDec)
		{
			using (toJobDec.InvoiceLines.SuspendInvoiceLineListChanged())
			{
				toJobDec.InvoiceLines.SuspendValidation();
				try
				{
					Importer.LogEvent += new EventHandler(Importer_LogEvent);

					Importer.Import();
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

		protected virtual KawasakiInvoiceDataImporter GetFlatFileImporter(string fileName, BaseJobDeclaration toJobDec)
		{
			return new KawasakiInvoiceDataImporter(fileName, toJobDec);
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

		protected KawasakiInvoiceDataImporter Importer;

		#endregion

		#region events
		public event ProcessedEventHandler Processed;
		public event EventHandler ProcessCompleted;
		#endregion
	}
}

#region Setup
#endregion
