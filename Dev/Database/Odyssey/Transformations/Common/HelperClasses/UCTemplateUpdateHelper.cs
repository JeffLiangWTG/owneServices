using System.Data;
using System.Linq;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.Transformation.Common
{
	public static class UCTemplateUpdateHelper
	{
		public static void UpdateTemplateMappingPrefix(string oldPrefix, string newPrefix, string countryCode, params string[] moduleIDs)
		{
			// Property names used in UC template
			// 1. Mapping target property name (N="Prefix_)
			//    <P N="Prefix_TargetPropertyName" Do="Default" />
			//
			// 2. Mapping source property name (>Prefix_)
			//    <P N="Prefix_TargetPropertyName" Do="Property">
			//      <Value xsi:type="xsd:string">Prefix_SourcePropertyName</Value>
			//    </P>
			//
			// 3. Inside macro expression (&lt;Prefix_)
			//    <P N="Prefix_TargetPropertyName" Do="Macro">
			//      <Value xsi:type="xsd:string">&lt;Prefix_SourcePropertyName&gt;</Value>
			//    </P>

			var sql = $"""
				UPDATE dbo.StmModuleFilter
					SET S9_FilterData = dbo.CLRCompressAsBytes(CAST(
						REPLACE(REPLACE(REPLACE(CAST(dbo.CLRUncompressAsBytes(S9_FilterData) AS NVARCHAR(MAX)),
							'N="' + @OldPrefix + '_', 'N="' + @NewPrefix + '_'),
							'>' + @OldPrefix + '_', '>' + @NewPrefix + '_'),
							'&lt;' + @OldPrefix + '_', '&lt;' + @NewPrefix + '_')
					AS VARBINARY(MAX))),
					S9_SystemLastEditTimeUtc = GETUTCDATE(),
					S9_SystemLastEditUser = '~BP'
				WHERE
					S9_ModuleID IN ({string.Join(", ", moduleIDs.Select(x => $"'{x}'"))})
					AND S9_GC IN (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = @CountryCode)
				""";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@OldPrefix", SqlDbType.VarChar, oldPrefix);
				cmd.AddParameter("@NewPrefix", SqlDbType.VarChar, newPrefix);
				cmd.AddParameter("@CountryCode", SqlDbType.VarChar, countryCode);
				cmd.ExecuteNonQuery();
			}
		}
	}
}
