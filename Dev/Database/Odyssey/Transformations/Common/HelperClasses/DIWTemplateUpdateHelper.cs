using System.Data;
using System.Linq;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.Transformation.Common
{
	public static class DIWTemplateUpdateHelper
	{
		public static void UpdateTemplateMappingPrefix(string oldPrefix, string newPrefix, string countryCode, params string[] moduleIDs)
		{
			using (var cmd = Db.Connection.Command($@"
UPDATE dbo.StmModuleFilter
SET S9_FilterData = dbo.CLRCompressStringAsBytes(REPLACE(CAST(dbo.CLRUncompressAsString(S9_FilterData) AS VARCHAR(MAX)), @MatchValue, @ReplaceValue)),
	S9_SystemLastEditTimeUtc = GETUTCDATE(),
	S9_SystemLastEditUser = '~BP'
FROM dbo.StmModuleFilter
WHERE S9_ModuleID IN ({string.Join(", ", moduleIDs.Select(x => $"'{x}'"))})
AND S9_GC IN (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = @CountryCode);
"))
			{
				cmd.AddParameter("@MatchValue", SqlDbType.VarChar, $"<Name>{oldPrefix}_");
				cmd.AddParameter("@ReplaceValue", SqlDbType.VarChar, $"<Name>{newPrefix}_");
				cmd.AddParameter("@CountryCode", SqlDbType.VarChar, countryCode);
				cmd.ExecuteNonQuery();
			}
		}
	}
}
