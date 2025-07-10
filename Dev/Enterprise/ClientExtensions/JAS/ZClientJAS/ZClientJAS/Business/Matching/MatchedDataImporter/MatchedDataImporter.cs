using System;
using System.IO;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.JAS.Business.Matching
{
	public delegate void MatchedDataImportedEventHandler(MatchedDataLine line);

	public class MatchedDataImporter : NonPersistentBusinessObject, IObsoleteValidation, IDataTransfer
	{
		public MatchedDataImporter()
		{
		}

		string fErrorMessage;
		public string ErrorMessage
		{
			get { return fErrorMessage ?? string.Empty; }
		}

		public void Import(string fileName)
		{
			if (File.Exists(fileName))
			{
				try
				{
					using (StreamReader reader = new StreamReader(fileName))
					{
						DoImport(reader);
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
		}

		public bool CancelImport;
		public event MatchedDataImportedEventHandler MatchedDataImported;

		protected void DoImport(StreamReader reader)
		{
			int totalNumOfLines = GetNumberOfLines(reader);
			int failureCounter = 0;
			int processedCounter = 0;

			string rawString = reader.ReadLine();
			while (rawString != null)
			{
				string lineError = "";

				if (rawString.Length >= MatchedDataLine.MinStringLength)
				{
					if (MatchedDataImported != null)
					{
						MatchedDataImported(new MatchedDataLine(rawString));
					}
				}
				else
				{
					lineError = string.Concat("Invalid line, row excluded. Content: ", rawString);
					failureCounter++;
				}
				processedCounter++;
				FireLogEvent((int)((float)processedCounter / totalNumOfLines * 100), processedCounter, failureCounter, lineError);
				if (CancelImport)
				{
					break;
				}

				rawString = reader.ReadLine();
			}

			if (ProcessCompleted != null)
			{
				string logEntry = string.Format("-----------------\n{0}", (CancelImport) ? "Import Canceled" : "Import Successful");
				FireLogEvent(100, processedCounter, failureCounter, logEntry);
				ProcessCompleted(this, EventArgs.Empty);
			}
		}

		protected int GetNumberOfLines(StreamReader reader)
		{
			int result = 0;
			reader.BaseStream.Seek(0, SeekOrigin.Begin);
			while (reader.ReadLine() != null)
			{
				result++;
			}

			reader.BaseStream.Seek(0, SeekOrigin.Begin);
			return result;
		}

		protected void FireLogEvent(int processCompleted, int processedCounter, int failureCounter, string logEntry)
		{
			if (Processed != null)
			{
				Processed(this, new ProcessedEventArgs(processCompleted, processedCounter, failureCounter, logEntry));
			}
		}

		#region IDataTransfer Members

		public event ProcessedEventHandler Processed;
		public event EventHandler ProcessCompleted;

		#endregion
	}
}

#region Implementation
#endregion
