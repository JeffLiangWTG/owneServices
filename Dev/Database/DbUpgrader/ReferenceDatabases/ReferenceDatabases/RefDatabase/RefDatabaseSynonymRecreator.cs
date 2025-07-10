using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.RefDbRepo.Client.Common;

namespace Enterprise.DbUpgrader.ReferenceDatabases
{
	public class RefDatabaseSynonymRecreator
	{
		const int ChunkSize = 5000;

		public RefDatabaseSynonymRecreator(DbConnection dbConnection)
		{
			this.dbConnection = dbConnection;
		}

		public void RecreateSynonym()
		{
			const string synonymPrefix = RefDbTableNameResolver.SingleRefDatabaseNameSynonymPrefix;
			var mappings = ConvertMappingsToDbReferences(synonymPrefix);

			var databaseSynonyms = GetExistingRefDbSynonyms(synonymPrefix, mappings);

			var createSynonymsSqlStatements = mappings
				.Where(x => !databaseSynonyms.existingSynonyms.Contains(x.Key) || databaseSynonyms.orphanedSynonyms.Contains(x.Key))
				.Select(x => $"CREATE SYNONYM [dbo].[{x.Key}] FOR {x.Value};");

			var allStatements = databaseSynonyms.orphanedSynonyms.Select(x => $"DROP SYNONYM [dbo].[{x}];").ToList();
			allStatements.AddRange(createSynonymsSqlStatements);

			ExecuteStatements(allStatements);
		}

		void ExecuteStatements(List<string> sqlStatements)
		{
			var scriptBuilder = new StringBuilder();
			foreach (var scriptStatement in sqlStatements)
			{
				scriptBuilder.AppendLine(scriptStatement);
				if (scriptBuilder.Length > ChunkSize)
				{
					ExecuteScript(scriptBuilder.ToString());
					scriptBuilder.Clear();
				}
			}

			if (scriptBuilder.Length > 0)
			{
				ExecuteScript(scriptBuilder.ToString());
			}
		}

		protected virtual void ExecuteScript(string sqlScript)
		{
			using var command = ((IDbConnectionWithSettings)dbConnection).CreateCommand(sqlScript);
			RefServiceExceptionHandler.Execute(command.ExecuteNonQuery, RefServiceExceptionHandler.IgnoreOption.IgnoreTimeoutException);
		}

		static Dictionary<string, string> ConvertMappingsToDbReferences(string synonymPrefix)
		{
			var refDatabaseName = RefDbTableNameResolver.SingleRefDatabaseName;
			var mappings = RefDatabaseVersionMapHelper.CombinedViewMappings
				.Select(mapping => new KeyValuePair<string, string>($"{synonymPrefix}_{mapping.Key}", $"[{refDatabaseName}].[dbo].[{mapping.Value}]"))
				.ToDictionary(x => x.Key, x => x.Value);
			return mappings;
		}

		public virtual (HashSet<string> orphanedSynonyms, HashSet<string> existingSynonyms) GetExistingRefDbSynonyms(string synonymPrefix, Dictionary<string, string> mappings)
		{
			var synonymsToDrop = new HashSet<string>();
			var existingSynonyms = new HashSet<string>();

			var sql = @$"
SELECT
	sn.name
	, base_object_name = ISNULL(sn.base_object_name, N'')
FROM
	sys.synonyms AS sn
WHERE 1=1
	AND sn.name LIKE N'{synonymPrefix}[_]%'
	AND sn.schema_id = SCHEMA_ID(N'dbo')
";
			RefServiceExceptionHandler.Execute(() =>
			{
				dbConnection.ExecuteReader(sql, record =>
				{
					var synonymName = record.GetString(0);
					if (mappings.TryGetValue(synonymName, out var synonymBaseObjectName))
					{
						var baseObjectName = record.GetString(1);
						if (!baseObjectName.Equals(synonymBaseObjectName, StringComparison.OrdinalIgnoreCase))
						{
							synonymsToDrop.Add(synonymName);
						}
						existingSynonyms.Add(synonymName);
					}
					else
					{
						synonymsToDrop.Add(synonymName);
					}
				});
			},
			RefServiceExceptionHandler.IgnoreOption.IgnoreTimeoutException);

			return (synonymsToDrop, existingSynonyms);
		}

		readonly DbConnection dbConnection;
	}
}
