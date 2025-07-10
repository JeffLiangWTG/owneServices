using System.Data;
using CargoWise.Types;
using Enterprise.DataConverters.InterbaseInterface;

namespace Enterprise.DataConverters
{
	public abstract class InterbaseImporter : DataImporter
	{
		public InterbaseImporter(ProgressLogger logger, ZString dataSourcePath, ZBool excludeExistingRecords, ZBool toCSV) : base(logger, dataSourcePath, excludeExistingRecords, toCSV)
		{
		}

		#region Overriden

		protected override internal bool ReadData()
		{
			if (UseLogWhenStarting)
			{
				Logger.StartLog(DataTypeDescription, DataSourcePath);
			}

			bool result = false;
			RecordCount = 0;
			CurrentRecord = 0;

			Table = GetDataTableFromSqlText();
			if (Table != null)
			{
				RecordCount = Table.Rows.Count;
				result = true;
			}
			else
			{
				Logger.Add(DataTypeDescription + " data could not be imported.");
			}
			return result;
		}

		protected override int RecordToSaveAtOnce
		{
			get { return 25; }
		}

		#endregion

		#region Abstracts

		protected abstract internal ZString SqlText { get; }

		#endregion

		#region Implementation

		protected virtual bool UseLogWhenStarting
		{
			get { return true; }
		}

		protected virtual DataTable GetDataTableFromSqlText()
		{
			DataTable result = null;
			if (!SqlText.IsEmpty)
			{
				result = InterbaseRunner.GetTableFromQuery(SqlText);
			}
			return result;
		}

		protected internal DataTable Table;

		protected DataRow GetNextDataRow()
		{
			DataRow result = this.Table.Rows[CurrentRecord];
			CurrentRecord++;
			return result;
		}

		InterbaseCommandRunner InterbaseRunner
		{
			get
			{
				if (fInterbaseRunner == null)
				{
					fInterbaseRunner = new InterbaseCommandRunner(DataSourcePath, ConnectionProviderType.Odbc);
				}
				return fInterbaseRunner;
			}
		}
		InterbaseCommandRunner fInterbaseRunner;

		#endregion
	}
}
