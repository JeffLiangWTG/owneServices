using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DataConverters
{
	#region DataLoaderDetails

	public struct DataLoaderDetails
	{
		public int RecordsImporting;
		public int CurrentRow;
		public int RecordsExcluded;
		public int RecordsUpdated;
		public StringBuilder Log;

		public DataLoaderDetails(int recordsImporting, int currentRow, int recordsExcluded, int recordsUpdated, StringBuilder log)
		{
			this.RecordsImporting = recordsImporting;
			this.CurrentRow = currentRow;
			this.RecordsExcluded = recordsExcluded;
			this.RecordsUpdated = recordsUpdated;
			this.Log = log;
		}
	}

	#endregion

	public class ProgressLogger
	{
		public ProgressLogger() : this(new StringCollection())
		{
		}

		public ProgressLogger(OleDBConnectionTypes dataType) : this(new StringCollection())
		{
			FromInterbase = dataType == OleDBConnectionTypes.Interbase;
		}

		public ProgressLogger(StringCollection collection)
		{
			this.Collection = collection;
		}

		readonly StringCollection Collection;

		public string this[int index]
		{
			get { return Collection[index]; }
			set { Collection[index] = value; }
		}

		public int Count
		{
			get { return Collection.Count; }
		}

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();
			foreach (string line in Collection)
			{
				result.Append(line + "\r\n");
			}
			return result.ToString();
		}

		public int RecordsToBeProcessed;
		public int RecordsExcluded;
		public int RecordsCreated;
		public int RecordsUpdated;
		public int RecordsInvalid;
		public int CurrentRecord;

		public int RecordsProcessed
		{
			get { return RecordsExcluded + RecordsCreated + RecordsUpdated + RecordsInvalid; }
		}

		void InitialiseCounters()
		{
			RecordsToBeProcessed = 0;
			RecordsExcluded = 0;
			RecordsCreated = 0;
			RecordsUpdated = 0;
			RecordsInvalid = 0;
			CurrentRecord = 0;
			OnUpdateCounters();
		}

		public void Add(string value)
		{
			Add(value, false);
		}

		public void Add(string value, bool notifyDataImportScreen)
		{
			Collection.Add(value);
			EventArgs args = EventArgs.Empty;
			OnUpdateLogText(value, notifyDataImportScreen);
		}

		public void StartLog(string dataTypeDescription, string dataSourcePath)
		{
			InitialiseCounters();
			Add("=".PadRight(70, '='), true);
			Add("Loading " + dataTypeDescription + " from: " + dataSourcePath, true);
			Add("=".PadRight(70, '='), true);
		}

		public void DisplayFormatLogMessage(ZString rowDescription, ZString exceptionMessage)
		{
			string logMessage = "ERROR READING ROW " + CurrentRecord.ToString() + ": "
				+ (rowDescription.IsEmpty ? "" : rowDescription + "\r\n")
				+ exceptionMessage;
			Add(logMessage);
			OnUpdateCounters();
		}

		public void OutputFinalTotals()
		{
			string dataSourcePath = "";
			string importDataType = "";
			OutputFinalTotals(dataSourcePath, importDataType);
		}

		public void OutputFinalTotals(string dataSourcePath, string importDataType)
		{
			Add("=".PadRight(70, '='), true);
			Add("F I N A L   T O T A L S : ", true);
			Add("Records Created  = " + RecordsCreated, true);
			Add("Records Updated  = " + RecordsUpdated, true);
			Add("Records Excluded = " + RecordsExcluded, true);
			Add("Records Invalid  = " + RecordsInvalid, true);
			Add("=".PadRight(70, '='), true);
			OnUpdateCounters();
			if (!FromInterbase)
			{
				SaveLogOutput(dataSourcePath, importDataType);
			}
		}

		public void OnUpdateCounters()
		{
			if (UpdateCounters != null)
			{
				UpdateCounters(this, null);
			}

			if (OnProgress != null)
			{
				OnProgress(new DataLoaderDetails(RecordsToBeProcessed, CurrentRecord, RecordsExcluded, RecordsUpdated + RecordsCreated, Log), EventArgs.Empty);
			}
		}

		void OnUpdateLogText(string textBeingAdded, bool notifyDataImportScreen)
		{
			if (UpdateLogText != null)
			{
				UpdateLogText(textBeingAdded, null);
			}

			if (Log != null && notifyDataImportScreen)
			{
				Log.Append(textBeingAdded);
				Log.Append(System.Environment.NewLine);
			}

			if (OutputToFileLog != null)
			{
				OutputToFileLog.Append(textBeingAdded);
				OutputToFileLog.Append(System.Environment.NewLine);
			}
		}

		public event EventHandler UpdateCounters;
		public event EventHandler UpdateLogText;
		public event EventHandler OnProgress;
		readonly StringBuilder Log = new StringBuilder();
		readonly StringBuilder OutputToFileLog = new StringBuilder();
		protected bool FromInterbase;

		#region RejectionLogging

		void SaveLogOutput(string directory, string importType)
		{
			if (!Globals.IsTest)
			{
				CreateRejectedDataLog(OutputToFileLog, directory, importType);
			}
		}

		void CreateRejectedDataLog(StringBuilder logData, string dataDirectory, string importType)
		{
			ZDateTime currentDateTime = ZDateTime.Now;
			ZString currentDateString = currentDateTime.ToString("yyyyMMdd");
			ZString currentTimeString = currentDateTime.ToString("HHmmss");

			try
			{
				DirectoryInfo directory = new DirectoryInfo(Path.Combine(dataDirectory, currentDateString));
				if (!directory.Exists)
				{
					directory.Create();
				}

				string logFileName = "";
				if (importType == "NZ Parts")
				{
					logFileName += "RejectedProducts." + currentTimeString + ".xls";
				}
				else
				{
					logFileName += "RejectedLookups." + currentTimeString + ".xls";
				}

				string fileName = Path.Combine(directory.FullName, logFileName);
				using (StreamWriter sw = new StreamWriter(fileName, true))
				{
					sw.WriteLine(logData);
					sw.Flush();
				}

				string logFileLocationNotificationMessage = "Summary information and rejected " + importType + " have been output to: " + fileName;
				OnUpdateLogText(logFileLocationNotificationMessage, true);
			}
			catch (Exception e) when (!e.IsCriticalException()) { }
			OnUpdateCounters();
		}

		#endregion
	}
}
