using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DirectSqlLoader
	{
		readonly ZSqlConnectionInfo connectionInfo = new ZSqlConnectionInfo(null, null);

		public List<ZGuid> LoadPKs(ZDBOnlyQuery query)
		{
			// note: Do not change type of parameter from ZDBOnlyQuery to ZQuery. In some cases OrderBy is ignored on ZQuery that is not ZDBOnlyQuery during SQL generation.

			if (query.IsNoResultQuery)
			{
				return new List<ZGuid>();
			}

			string sql = query.GetAsCompleteSQLStatement(query.PKColumn.TableName, false, new SchemaColumn[] { query.PKColumn });
			return LoadZGuids(sql, query.Params);
		}

		List<ZGuid> LoadZGuids(string sql, params ZSqlParameter[] parameters)
		{
			var result = new List<ZGuid>();

			using (var cmd = connectionInfo.GetNewDbCommandForSelect(sql, parameters))
			{
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add((Guid)reader[0]);
					}
				}
			}

			return result;
		}
	}
}
