using Enterprise.DbUpgrader.Data.BaseData.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data.BaseData.StmData
{
	class StmDataDataFile : BaseDataFile
	{
		public StmDataDataFile()
			: base(DataFileRelativePath, DataFileTables)
		{
		}

		const string DataFileRelativePath = @"BaseData\StmData\StmData.xml";

		static readonly string[] DataFileTables = new string[] { StmDataSchema.Constants.TableName, };

		protected override string SelectQuery
		{
			get
			{
				return string.Format("SELECT * FROM {0} WHERE {1} LIKE 'GL[_]%[_]Account' OR {1} = 'ClearingJournalClearingAccount' ORDER BY 1",
					StmDataSchema.Constants.TableName,
					StmDataSchema.SD_Name.Name);
			}
		}
	}
}
