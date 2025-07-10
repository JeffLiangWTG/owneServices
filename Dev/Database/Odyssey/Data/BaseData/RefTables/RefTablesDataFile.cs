namespace Enterprise.DbUpgrader.Data.BaseData.RefTables
{
	using Enterprise.DbUpgrader.Data.BaseData.Common;
	using Enterprise.ZArchitecture.Schema;

	class RefTablesDataFile : BaseDataFile
	{
		public RefTablesDataFile()
			: base(DataFileRelativePath, DataFileTables)
		{
		}

		const string DataFileRelativePath = @"BaseData\RefTables\RefTables.xml";

		static readonly string[] DataFileTables = new string[]
		{
			RefCommodityCodeSchema.Constants.TableName,
			RefContainerSchema.Constants.TableName,
			RefContainerCodeMapSchema.Constants.TableName,
			RefPacksSchema.Constants.TableName,
			RefServiceLevelSchema.Constants.TableName,
		};
	}
}
