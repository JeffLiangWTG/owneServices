using System.Collections.Generic;
using System.Text;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(RefZoneHeaderSchema))]
[assembly: UsesConstants(typeof(RefZonePivotSchema))]

namespace Enterprise.DbUpgrader.Data
{
	public class RefWRSZoneHeaderDataFile : EmbeddedDataFile
	{
		public RefWRSZoneHeaderDataFile() : base(DataFileRelativePath, RefZoneHeaderSchema.Constants.TableName, RefZonePivotSchema.Constants.TableName)
		{
		}

		public override string ResourceRelativeName => "RefWRSZoneHeader.RefWRSZoneHeader.xml";

		const string DataFileRelativePath = @"Public\RefWRSZoneHeader\RefWRSZoneHeader.xml";

		protected override void BuildQueryForTable(StringBuilder builder, string tableName)
		{
			switch (tableName)
			{
				case RefZoneHeaderSchema.Constants.TableName:
					builder.AppendFormat($"SELECT * FROM {RefZoneHeaderSchema.Constants.SqlSchemaName}.{RefZoneHeaderSchema.Constants.TableName} WHERE {RefZoneHeaderSchema.Constants.FZ_ZoneType} = 'WRS' ORDER BY 1");
					break;

				case RefZonePivotSchema.Constants.TableName:
					builder.Append($@"SELECT * FROM {RefZonePivotSchema.Constants.SqlSchemaName}.{RefZonePivotSchema.Constants.TableName} WHERE {RefZonePivotSchema.Constants.F2_FZ} IN 
(SELECT {RefZoneHeaderSchema.Constants.PK} FROM {RefZoneHeaderSchema.Constants.SqlSchemaName}.{RefZoneHeaderSchema.Constants.TableName} WHERE {RefZoneHeaderSchema.Constants.FZ_ZoneType} = 'WRS') ORDER BY 1");
					break;
			}
		}

		protected override List<UniqueIndexInfo> GetUniqueIndexesToDropBeforeSaveAndRecreateAfterwards()
		{
			return new List<UniqueIndexInfo>
			{
				new ViewUniqueIndexInfo("vw_ZoneUNLOCOByCountry", RefZoneHeaderSchema.Constants.FZ_Code +  ", " + RefZoneHeaderSchema.Constants.FZ_ZoneType + ", " + RefZoneHeaderSchema.Constants.FZ_ZoneMode + ", " + RefUNLOCOSchema.Constants.RL_Code),
				new ViewUniqueIndexInfo("vw_ZoneUNLOCO", RefZoneHeaderSchema.Constants.FZ_Code +  ", " + RefZoneHeaderSchema.Constants.FZ_ZoneType + ", " + RefZoneHeaderSchema.Constants.FZ_ZoneMode + ", " + RefUNLOCOSchema.Constants.RL_Code),
				new ViewUniqueIndexInfo("vw_ZoneCountry", RefZoneHeaderSchema.Constants.FZ_Code +  ", "  + RefZoneHeaderSchema.Constants.FZ_ZoneType + ", " + RefZoneHeaderSchema.Constants.FZ_ZoneMode + ", " + RefCountrySchema.Constants.RN_Code),
			};
		}
	}
}
