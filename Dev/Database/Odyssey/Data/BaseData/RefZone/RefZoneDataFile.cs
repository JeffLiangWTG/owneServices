namespace Enterprise.DbUpgrader.Data.BaseData.RefZone
{
	using System.Diagnostics.CodeAnalysis;
	using System.Text;
	using Enterprise.DbUpgrader.Data.BaseData.Common;
	using Enterprise.ZArchitecture.Schema;

	class RefZoneDataFile : BaseDataFile
	{
		public RefZoneDataFile()
			: base(DataFileRelativePath, DataFileTables)
		{
		}

		const string DataFileRelativePath = @"BaseData\RefZone\RefZone.xml.gz";

		static readonly string[] DataFileTables = new string[]
		{
			RefZoneHeaderSchema.Constants.TableName,
			RefZonePivotSchema.Constants.TableName,
		};

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "All the arguments are string constants so no need for IFormatProvider")]
		protected override void BuildQueryForTable(StringBuilder builder, string tableName)
		{
			switch (tableName)
			{
				case RefZoneHeaderSchema.Constants.TableName:
					builder.Append($"SELECT * FROM {RefZoneHeaderSchema.Constants.SqlSchemaName}.{RefZoneHeaderSchema.Constants.TableName} WHERE {RefZoneHeaderSchema.Constants.FZ_ZoneType} != 'WRS' ORDER BY 1");
					break;

				case RefZonePivotSchema.Constants.TableName:
					builder.Append($@"SELECT * FROM {RefZonePivotSchema.Constants.SqlSchemaName}.{RefZonePivotSchema.Constants.TableName} WHERE {RefZonePivotSchema.Constants.F2_ParentID} IN (SELECT {RefZoneHeaderSchema.Constants.PK} FROM {RefZoneHeaderSchema.Constants.SqlSchemaName}.{RefZoneHeaderSchema.Constants.TableName}
					WHERE {RefZoneHeaderSchema.Constants.FZ_ZoneType} != 'WRS') ORDER BY 1");
					break;
			}
		}
	}
}
