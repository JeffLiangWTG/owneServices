using System;
using System.Text;
using CargoWise.Data;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DataPurge
{
	class NonProxyOrganizationsScriptRepository : ScriptRepository
	{
		public NonProxyOrganizationsScriptRepository(DbConnection connection)
		{
			this.connection = connection;
		}

		protected override string[] PurgeScripts
		{
			get
			{
				return nonProxyOrganisationPurgeScript ?? (nonProxyOrganisationPurgeScript = new string[] { GetNonProxyOrganisationPurgeScript() });
			}
		}
		string[] nonProxyOrganisationPurgeScript;

		string GetNonProxyOrganisationPurgeScript()
		{
			var result = new StringBuilder();

			result.AppendLine("DECLARE @OrgsToDelete TABLE (OrgPk uniqueidentifier);");
			result.AppendLine(@"
				INSERT @OrgsToDelete SELECT OH_PK FROM dbo.OrgHeader 
					WHERE OH_PK not in (SELECT GC_OH_OrgProxy FROM dbo.GlbCompany WHERE GC_OH_OrgProxy is not null)
					AND   OH_PK not in (SELECT GB_OH_OrgProxy FROM dbo.GlbBranch  WHERE GB_OH_OrgProxy is not null)
					AND   OH_Code not in ('UNMATCHED', 'MISC');");

			using (var cmd = connection.Command(GenerateHierarchicalPurgeSql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					string sqlCmd = String.Format((NoResString)"{0} {1} in (SELECT OrgPk FROM @OrgsToDelete){2};",
						reader.GetBoolean(0)
							? String.Format((NoResString)"UPDATE [{0}] SET [{1}] = null", reader.GetString(1), reader.GetString(2))
							: "DELETE",
						reader.GetString(3),
						new String(')', reader.GetInt32(4))
					);
					result.AppendLine(sqlCmd);
				}
			}

			result.AppendLine("DELETE dbo.OrgHeader WHERE OH_PK in (SELECT OrgPk FROM @OrgsToDelete);");

			return result.ToString();
		}

		readonly DbConnection connection;

		const string GenerateHierarchicalPurgeSql = @"
			WITH FkTreeCte AS (
				SELECT convert(nvarchar(max), 'FROM [' + r.FkSchema + '].[' + r.FkTable + '] WHERE [' + r.FkColumn + ']') AS Predicate,
					r.PkTable, r.PkColumn, r.FkTable, r.FkColumn, 0 AS FkLevel, 0 AS UpdateInsteadOfDelete
					FROM dbo.vw_FKReferences r
					WHERE r.PkTable = 'OrgHeader'
				UNION ALL
				SELECT 'FROM [' + c.FkSchema + '].[' + c.FkTable + '] WHERE [' + c.FkColumn + '] in (SELECT [' + c.PkColumn + '] ' + p.Predicate,
					c.PkTable, c.PkColumn, c.FkTable, c.FkColumn, p.FkLevel + 1, 
					CASE WHEN (c.IsNullable = 1 AND charindex(c.FkTable, p.Predicate) > 0) THEN 1 ELSE 0 END
					FROM dbo.vw_FKReferences c
					INNER JOIN FkTreeCte p ON p.FkTable = c.PkTable AND c.IsSelfReference = 0
					WHERE p.UpdateInsteadOfDelete = 0
			)
			SELECT
				convert(bit, UpdateInsteadOfDelete) AS UpdateInsteadOfDelete,
				FkTable, FkColumn, Predicate, FkLevel
			FROM FkTreeCte
			ORDER BY FkLevel DESC;";
	}
}
