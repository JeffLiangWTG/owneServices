using System;
using System.Collections.Specialized;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Business
{
	public abstract class DataSave
	{
		public void ExportData(string dataLocation, string dataExporting)
		{
			Log.Clear();
			ExportDataCore(dataLocation, dataExporting);
		}

		protected virtual void ExportDataCore(string dataLocation, string dataExporting)
		{
			FileName = dataLocation;
			ProcessData(dataExporting);
			ExportComplete();
		}

		#region Log

		public StringCollection Log
		{
			get { return fLog ?? (fLog = new StringCollection()); }
		}
		StringCollection fLog;

		#endregion

		#region Factory

		protected BusinessObjectFactory Factory
		{
			get { return fFactory ?? (fFactory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory fFactory;

		#endregion

		#region Counters

		public sealed class Counters
		{
			public Counters()
			{
				CurrentRow = 1;
			}

			public int CurrentRow;
			public int BizosExpectedToExportPlusHeader;
			public int LinesCreated;
			public int BizosExcluded;
		}

		public Counters RunCounters
		{
			get { return fRunCounters ?? (fRunCounters = new Counters()); }
		}

		void ClearRunCounters()
		{
			fRunCounters = null;
		}
		Counters fRunCounters;

		#endregion

		#region Export To .csv file

		protected void ProcessData(string dataType)
		{
			ClearRunCounters();
			SetupBeforeExport();

			BusinessObjectCollection bizoCollection = NewBusinessObjectCollection();
			if (bizoCollection == null || bizoCollection.Count == 0)
			{
				DisplayLogMessage(Res.GetString("1ca66bcd-d023-4cbc-9fb4-911ddd91514b", "No {0}s to export.", dataType));
			}
			else
			{
				DisplayLogMessage(Res.GetString("f9518cd7-0211-4e3e-9c49-f08f8f22240c", "{0}s to export = {1}", dataType, bizoCollection.Count.ToString()));
				RunCounters.BizosExpectedToExportPlusHeader = bizoCollection.Count + 1;

				using (StreamWriter sr = new StreamWriter(ZSaveFileDialog.OpenFile(FileName)))
				{
					OCsvLine firstLine = new OCsvLine(ColumnHeadings());
					sr.WriteLine(firstLine.ToString());
					foreach (BusinessObject bizo in bizoCollection)
					{
						RunCounters.CurrentRow++;
						sr.WriteLine(ProcessDataForThisBizo(bizo).ToString());
					}
				}
			}
			bizoCollection = null;

			OutputFinalTotals(dataType);
			RunSubsequentDataParsingIfRequired();
		}

		#endregion

		#region Utilities

		protected void UpdateAndDisplayIfRequired(Guid transactionPK, string tableName)
		{
			AddDataExportEvent(transactionPK, tableName);
			OnProgressChanged();
		}

		protected void OnProgressChanged()
		{
			if (ProgressChanged != null)
			{
				ProgressChanged(this, new SaveProgressChangedEventArgs(RunCounters.BizosExpectedToExportPlusHeader, RunCounters.CurrentRow, Log));
			}
		}

		protected void DisplayLogMessage(string logMessageToAdd)
		{
			Log.Add(logMessageToAdd);

			if (LogUpdated != null)
			{
				LogUpdated(this, new SaveLogUpdatedEventArgs(logMessageToAdd));
			}
		}

		protected void DisplayExcludedRecordMessage(string logMessage)
		{
			RunCounters.BizosExcluded++;
			DisplayLogMessage(Res.GetString("0713da80-fe21-4943-9040-f262d09896ea", "Row {0}{1}", RunCounters.CurrentRow.ToString(), logMessage));
		}

		protected void AddDataExportEvent(Guid transactionPK, string tableName)
		{
			EventManager eventInserter = new EventManager();
			eventInserter.AddAuditLogEvent(AutoEvents.DataExport.Code, tableName, transactionPK, Env.Time.CurrentLocalDateTime);
		}

		#endregion

		#region Virtual methods

		protected virtual void SetupBeforeExport() {	}

		protected virtual void RunSubsequentDataParsingIfRequired() {	}

		protected virtual void ExportComplete() { }

		internal protected virtual void OutputFinalTotals(string dataType)
		{
			DisplayLogMessage("\r\n" + Res.GetString("09223dc6-1bdf-4a82-8703-a18553f0a2df", "T O T A L : {0}s created = {1}, {2}s excluded = {3}", dataType, RunCounters.LinesCreated, dataType, RunCounters.BizosExcluded) + "\r\n");
		}

		#endregion

		#region Abstract methods

		protected abstract OCsvLine ProcessDataForThisBizo(BusinessObject bizo);

		protected abstract String[] ColumnHeadings();

		protected abstract BusinessObjectCollection NewBusinessObjectCollection();

		#endregion

		public event SaveProgressChangedEventHandler ProgressChanged;
		public event SaveLogUpdatedEventHandler LogUpdated;

		string FileName;
		}

	#region Event Handlers
	public delegate void SaveProgressChangedEventHandler(DataSave sender, SaveProgressChangedEventArgs e);

	public class SaveProgressChangedEventArgs : EventArgs
	{
		public SaveProgressChangedEventArgs(int productsExporting, int currentRow, StringCollection log)
		{
			this.ProductsExporting = productsExporting;
			this.CurrentRow = currentRow;
			this.Log = log;
		}

		public readonly int ProductsExporting;
		public readonly int CurrentRow;
		public readonly StringCollection Log;
	}

	public delegate void SaveLogUpdatedEventHandler(DataSave sender, SaveLogUpdatedEventArgs e);

	public class SaveLogUpdatedEventArgs : EventArgs
	{
		public SaveLogUpdatedEventArgs(string logMessage)
		{
			this.LogMessage = logMessage;
		}

		public readonly string LogMessage;
	}
	#endregion
}
