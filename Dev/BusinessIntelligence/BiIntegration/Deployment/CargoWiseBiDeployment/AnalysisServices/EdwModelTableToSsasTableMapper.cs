using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Bi.Common;
using CargoWise.Bi.ConfigLoader;
using CargoWise.Data;

namespace CargoWise.Bi.Deployment.AnalysisServices
{
	public class EdwModelTableToSsasTableMapper
	{
		public EdwModelTableToSsasTableMapper(BiConfigurationData configData, DbConnection biConnection)
		{
			this.configData = configData;
			this.biConnection = biConnection;
		}
		readonly BiConfigurationData configData;
		readonly DbConnection biConnection;

		public void PopulateMapping()
		{
			var mappings = GetEdwModelTableToSsasTableMapping();

			if (mappings.Any())
			{
				var values = new List<string>();
				foreach (var mapping in mappings.OrderBy(m => m.EdwModelSchema).ThenBy(m => m.EdwModelTable).ThenBy(m => m.SsasModelFileName).ThenBy(m => m.SsasTable))
				{
					values.Add(string.Format(CultureInfo.InvariantCulture, @"'{0}', '{1}', '{2}', '{3}'", mapping.EdwModelSchema, mapping.EdwModelTable, mapping.SsasModelFileName, mapping.SsasTable));
				}

				var sqlText = string.Format(CultureInfo.InvariantCulture, @"
TRUNCATE TABLE [{0}].[EdwModelTableToSsasTableMapping];

CREATE TABLE #TempTableMapping
(
	EdwModelSchema VARCHAR(128) COLLATE database_default,
	EdwModelTable VARCHAR(128) COLLATE database_default,
	SsasModelFileName VARCHAR(128) COLLATE database_default,
	SsasTable VARCHAR(128) COLLATE database_default
)

INSERT INTO #TempTableMapping (EdwModelSchema, EdwModelTable, SsasModelFileName, SsasTable)
VALUES
	({1});

INSERT INTO [{0}].[EdwModelTableToSsasTableMapping] (EdwModelSchema, EdwModelTable, SsasTableID)
SELECT t.EdwModelSchema, t.EdwModelTable, st.SsasTableID
FROM [{0}].[SsasCube] sc
INNER JOIN [{0}].[SsasTable] st
	ON sc.SsasCubeID = st.SsasCubeID
INNER JOIN  #TempTableMapping t
	ON t.SsasModelFileName = sc.SsasModelFileName AND t.SsasTable = st.TableName",
					BiConstants.BiAdminSchemaName,
					string.Join("),\r\n\t(", values));

				using (((ICurrentDbControl)biConnection).UseDatabase(Db.EdwDatabaseName))
				{
					biConnection.ExecuteNonQuery(sqlText);
				}
			}
		}

		List<MappingDefinition> GetEdwModelTableToSsasTableMapping()
		{
			var tableList = new List<MappingDefinition>();
			foreach (var ssasCube in configData.SsasCubes)
			{
				foreach (var ssasTable in ssasCube.GetSsasTablesRows())
				{
					if (!ssasTable.IsCalculated)
					{
						var edwModelViews = ParseSources(ssasTable.Query);
						if (edwModelViews.Any())
						{
							foreach (var edwModelViewDefinition in edwModelViews)
							{
								var edwModelView = configData.EdwModelViewTableConfig.FirstOrDefault(m => m.Name == edwModelViewDefinition.SourceName && m.Schema == edwModelViewDefinition.SourceSchema);
								if (edwModelView != null)
								{
									var edwModelTables = ParseSources(edwModelView.Expression);
									foreach (var edwModelTable in edwModelTables)
									{
										tableList.Add(new MappingDefinition(edwModelTable.SourceSchema, edwModelTable.SourceName, ssasCube.SsasModelFileName, ssasTable.TableName));
									}
								}
							}
						}
					}
				}
			}
			return tableList;
		}

		internal static IEnumerable<SourceTableDefinition> ParseSources(string expression)
		{
			var result = new List<SourceTableDefinition>();

			if (!string.IsNullOrEmpty(expression))
			{
				var regex = new Regex(@"(?:^|JOIN\s+|FROM\s+)(?:\[*|\b)(?<Schema>\w+)(?:\]*|\b)\.(?:\[*|\b)(?<Source>\w+)(?:\[*|\b)", RegexOptions.IgnoreCase);
				foreach (Match match in regex.Matches(expression))
				{
					var schema = match.Groups["Schema"].Value;
					var source = match.Groups["Source"].Value;

					if (!result.Any(d => d.SourceSchema == schema && d.SourceName == source))
					{
						result.Add(new SourceTableDefinition(schema, source));
					}
				}
			}

			return result;
		}

		internal class SourceTableDefinition
		{
			public SourceTableDefinition(string sourceSchema, string sourceName)
			{
				SourceSchema = sourceSchema;
				SourceName = sourceName;
			}

			public string SourceSchema { get; private set; }
			public string SourceName { get; private set; }
		}

		internal class MappingDefinition
		{
			public MappingDefinition(string edwModelSchema, string edwModelTable, string ssasModelFileName, string ssasTable)
			{
				EdwModelSchema = edwModelSchema;
				EdwModelTable = edwModelTable;
				SsasModelFileName = ssasModelFileName;
				SsasTable = ssasTable;
			}

			public string EdwModelSchema { get; private set; }
			public string EdwModelTable { get; private set; }
			public string SsasModelFileName { get; private set; }
			public string SsasTable { get; private set; }
		}
	}
}
