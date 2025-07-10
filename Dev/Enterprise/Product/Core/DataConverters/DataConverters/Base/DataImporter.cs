using System;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.DataConverters
{
	public enum OleDBConnectionTypes  { Cargoweb, Excel, Interbase }

	public abstract class DataImporter
	{
		public DataImporter(ProgressLogger logger, ZString dataSourcePath, ZBool excludeExistingRecords)
		{
			this.Logger = logger;
			this.DataSourcePath = dataSourcePath;
			this.ExcludeExistingRecords = excludeExistingRecords;
			fRecordCount = 0;
			fCurrentRecord = 0;
		}

		public DataImporter(ProgressLogger logger, ZString dataSourcePath, ZBool excludeExistingRecords, ZBool importToCSVFile) : this(logger, dataSourcePath, excludeExistingRecords)
		{
			this.ImportToCSVFile = importToCSVFile;
		}

		public void Import()
		{
			if (ReadData())
			{
				Factory = new BusinessObjectFactory();
				Factory.RefreshEnabled = false;
				RecordsProcessedInFactory = 0;

				if (ImportToCSVFile)
				{
					ImportToCSVFileOnly();
				}
				else
				{
					ImportToDatabase();
				}
			}

			Logger.OutputFinalTotals(DataSourcePath, DataTypeDescription);
		}

		protected void ImportToDatabase()
		{
			while (HasRecordsNotProcessedYet)
			{
				RecordsProcessedInFactory++;
				DataWriter writer = GetNextDataWriter(Factory);
				writer.SaveRecordToEnterprise(ExcludeExistingRecords, Logger);
				RecycleFactory(true);
			}
			Factory.Save();
		}

		protected void ImportToCSVFileOnly()
		{
			CreateCSVTemplate();
			while (HasRecordsNotProcessedYet)
			{
				RecordsProcessedInFactory++;
				DataWriter writer = GetNextDataWriter(Factory);
				writer.AddRecordToCSVFile(TemplateFile, Logger);
				RecycleFactory(false);
			}
			Logger.Add("Template file " + TemplateFile);
		}

		void RecycleFactory(bool save)
		{
			if (RecordsProcessedInFactory == RecordToSaveAtOnce)
			{
				Logger.OnUpdateCounters();
				if (save)
				{
					try
					{
						Factory.Save();
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						Logger.Add(e.Message);
					}
				}
				Factory = new BusinessObjectFactory();
				Factory.RefreshEnabled = false;
				RecordsProcessedInFactory = 0;
			}
		}

		public void ImportDataForMasterOnly(BusinessObjectFactory factory)
		{
			if (ReadData())
			{
				while (HasRecordsNotProcessedYet)
				{
					DataWriter writer = GetNextDataWriter(factory);
				}
			}
		}

		BusinessObjectFactory Factory;
		int RecordsProcessedInFactory;
		protected virtual int RecordToSaveAtOnce
		{
			get { return 100; }
		}

		protected readonly ProgressLogger Logger;
		protected readonly ZString DataSourcePath;
		protected readonly ZBool ExcludeExistingRecords;
		protected internal ZBool ImportToCSVFile;

		protected abstract string DataTypeDescription { get; }
		protected abstract internal bool ReadData();
		protected abstract internal DataWriter GetNextDataWriter(BusinessObjectFactory factory);

		protected virtual void CreateCSVTemplate()
		{
			using (StreamWriter sw = File.CreateText(TemplateFile))
			{
				sw.WriteLine(CSVOutputHeader);
			}
		}

		protected virtual internal ZString TemplateFile
		{
			get
			{
				if (fTemplateFile == null)
				{
					fTemplateFile = Env.GetTempFileName(Env.TempPath, "csv");
				}
				return fTemplateFile;
			}
		}
		string fTemplateFile;

		protected virtual internal ZString CSVOutputHeader
		{
			get { return ZString.Empty; }
		}

		#region HasRecordsNotProcessedYet

		protected internal bool HasRecordsNotProcessedYet
		{
			get { return CurrentRecord < RecordCount; }
		}

		#endregion

		#region RecordCount

		protected internal int RecordCount
		{
			get { return fRecordCount; }
			set
			{
				fRecordCount = value;
				Logger.RecordsToBeProcessed = value;
			}
		}

		int fRecordCount;

		#endregion

		#region CurrentRecord

		protected int CurrentRecord
		{
			get { return fCurrentRecord; }
			set
			{
				fCurrentRecord = value;
				Logger.CurrentRecord = value;
			}
		}

		int fCurrentRecord;

		#endregion
	}
}
