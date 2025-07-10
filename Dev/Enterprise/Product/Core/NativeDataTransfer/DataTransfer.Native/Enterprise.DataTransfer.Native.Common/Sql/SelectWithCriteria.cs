using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.DataTransfer.Native.DB;
using Enterprise.DataTransfer.Native.DB.Sql;
using Enterprise.DataTransfer.Native.Utils;
using Enterprise.DataTransfer.Native.Utils.Models;

namespace Enterprise.DataTransfer.Native.Common.Sql
{
	public class SelectWithCriteria
	{
		public SelectWithCriteria(IEntityDefinition startDefinition, IEntityDefinition endDefinition, params MultipleValueCriteria[] criterias)
		{
			this.startDefinition = startDefinition;
			this.endDefinition = endDefinition;
			this.allCriterias = criterias;
		}

		public SelectWithCriteria(IEnumerable<IEntityDefinition> startDefinitions, IEntityDefinition endDefinition, MultipleValueCriteria multipleValueCriteria)
		{
			this.startDefinitions = startDefinitions;
			this.endDefinition = endDefinition;
			this.multipleValueCriteria = multipleValueCriteria;
		}

		readonly IEnumerable<IEntityDefinition> startDefinitions;
		readonly IEntityDefinition startDefinition;
		readonly IEntityDefinition endDefinition;
		readonly MultipleValueCriteria[] allCriterias;
		readonly MultipleValueCriteria multipleValueCriteria;

		const int maxNoOfSqlParameters = 500;
		/*
		 * Sql server limitation is 2,100. However sp_executesql takes up 2 of the parameters which leaves 2,098 free totally at the end.
		 * We actually don't care about that anymore since we're now using a Table valued parameter.
		 * We do have a 5 minute SQL timeout however, so loading too large a set is still not a good idea.
		 */
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public IEnumerable<DataRow> GetDataSet()
		{
			var keyToParameters = new Dictionary<string, DB.Sql.SqlParameter>();
			var sqlString = GenerateSQL(keyToParameters);
			var command = DataSetContext.Connection.Command(sqlString);
			SqlStatement.AddParameters(command, keyToParameters);
			return command.ExecuteDataTable();
		}

		public IEnumerable<DataRow>[] GetDataSets()
		{
			var sqlStringsAndParameterCollection = ParallelizeIfNotInTransaction(multipleValueCriteria.SplitMyself(maxNoOfSqlParameters)).Select((CreateSqlStringsAndParametersForDataTableLoad));
			var dataTables = GetDataTables(sqlStringsAndParameterCollection);

			var result = new List<DataRow>[dataTables[0].Count];
			for (var i = 0; i < result.Length; ++i)
			{
				result[i] = new List<DataRow>();
			}

			var interner = new StringInterner();
			foreach (var dataTable in dataTables)
			{
				Intern(interner, dataTable);
				var count = dataTable.Count;

				for (var i = 0; i < count; ++i)
				{
					result[i].AddRange(dataTable[i].AsEnumerable());
				}
			}

			return result.ToArray();
		}

		IEnumerable<T> ParallelizeIfNotInTransaction<T>(IEnumerable<T> collection) // Transactions cause deadlocks.
		{
			if (Db.Connection.IsInTransaction)
			{
				return collection;
			}
			else
			{
				return collection.AsParallel();
			}
		}

		Tuple<string, Dictionary<string, TvpItem>> CreateSqlStringsAndParametersForDataTableLoad(MultipleValueCriteria splittedCriteria)
		{
			var tvpParameters = new Dictionary<string, TvpItem>();
			var sqls = GenerateSQLs(splittedCriteria, null, tvpParameters);
			var sqlString = string.Join("\r\n", sqls);
			return Tuple.Create(sqlString, tvpParameters);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		DataTableCollection[] GetDataTables(IEnumerable<Tuple<string, Dictionary<string, TvpItem>>> sqlStringAndParametersCollection)
		{
			return ParallelizeIfNotInTransaction(sqlStringAndParametersCollection).Select(tuple =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var command = DataSetContext.Connection.Command(tuple.Item1);
					foreach (var tvpItem in tuple.Item2)
					{
						tvpItem.Value.AddTvpParamToCommand(command);
					}
					return command.ExecuteDataSet();
				}
			}).ToArray();
		}

		void Intern(StringInterner interner, DataTableCollection dataTables)
		{
			foreach (DataTable dataTable in dataTables)
			{
				foreach (DataColumn dataColumn in dataTable.Columns)
				{
					if (dataColumn.DataType == typeof(string))
					{
						foreach (DataRow row in dataTable.Rows)
						{
							var value = row[dataColumn];
							if (value is string)
							{
								var internedValue = interner.InternValue(value);

								if (!ReferenceEquals(value, internedValue))
								{
									row[dataColumn] = internedValue;
								}
							}
						}
					}
				}
			}
		}

		public string GenerateSQL(Dictionary<string, DB.Sql.SqlParameter> keyToParameters)
		{
			var end = endDefinition.Table;
			var reverseRelations = GetReverseRelations(startDefinition);

			var criterias = allCriterias.ToList();
			var endSql = ToSql(end, criterias, keyToParameters, null);

			foreach (var relation in reverseRelations)
			{
				endSql = GenerateNextSQL(endSql, end, relation, criterias, keyToParameters, null, startDefinition);
				end = relation.Mate(end);
			}
			return endSql;
		}

		public IEnumerable<string> GenerateSQLs(MultipleValueCriteria criteria, Dictionary<string, DB.Sql.SqlParameter> keyToParameters, Dictionary<string, TvpItem> tvpParams)
		{
			foreach (var startDef in startDefinitions)
			{
				lock (startDef)
				{
					var end = endDefinition.Table;
					var reverseRelations = GetReverseRelations(startDef);

					var parentColumnName = criteria.ColumnName;
					var criterias = new List<MultipleValueCriteria>() { criteria };
					var endSql = ToSql(end, criterias, keyToParameters, tvpParams);

					var parentColumnNameSuffixInSELECTClause = string.Empty;
					var parentColumnNameSuffixCounter = reverseRelations.Count() - 1;
					var parentColumnNameSuffixInASClause = parentColumnNameSuffixCounter.ToString();

					foreach (var relation in reverseRelations)
					{
						endSql = GenerateNextSQLWithParentColumn(endSql, end, relation, criterias, keyToParameters, tvpParams, startDef, parentColumnName, parentColumnNameSuffixInSELECTClause, parentColumnNameSuffixInASClause);
						end = relation.Mate(end);

						parentColumnNameSuffixInSELECTClause = parentColumnNameSuffixInASClause;
						parentColumnNameSuffixCounter--;
						parentColumnNameSuffixInASClause = parentColumnNameSuffixCounter.ToString();
					}

					yield return endSql;
				}
			}
		}

		IEnumerable<Relation> GetReverseRelations(IEntityDefinition startDef)
		{
			var path = startDef.FindPath(endDefinition);
			var relations = path.GetRelations();
			return relations.Reverse();
		}

		public string GenerateNextSQL(string endSql, Table end, Relation relation, List<MultipleValueCriteria> criterias, Dictionary<string, DB.Sql.SqlParameter> keyToParameters, Dictionary<string, TvpItem> tvpParams, IEntityDefinition startDef)
		{
			string startSql, startAlias, endAlias, innerJoinSQLOnStatement, whereClause;
			GenerateNextSQLParts(end, relation, criterias, keyToParameters, tvpParams, startDef, out startSql, out startAlias, out endAlias, out innerJoinSQLOnStatement, out whereClause);

			var sql = string.Format(CultureInfo.InvariantCulture, "SELECT {2}.* FROM ({0}) AS {2} INNER JOIN ({1}) AS {3}",
				startSql,
				endSql,
				startAlias,
				endAlias);

			return new StringBuilder(sql).Append(innerJoinSQLOnStatement).Append(whereClause).ToString();
		}

		public string GenerateNextSQLWithParentColumn(string endSql, Table end, Relation relation, List<MultipleValueCriteria> criterias, Dictionary<string, DB.Sql.SqlParameter> keyToParameters, Dictionary<string, TvpItem> tvpParams, IEntityDefinition startDef, string parentColumnName, string parentColumnNameSuffixInSELECTClause, string parentColumnNameSuffixInASClause)
		{
			string startSql, startAlias, endAlias, innerJoinSQLOnStatement, whereClause;
			GenerateNextSQLParts(end, relation, criterias, keyToParameters, tvpParams, startDef, out startSql, out startAlias, out endAlias, out innerJoinSQLOnStatement, out whereClause);

			var sql = string.Format(CultureInfo.InvariantCulture, "SELECT {3}.{4}{5} AS {4}{6}, {2}.* FROM ({0}) AS {2} INNER JOIN ({1}) AS {3}",
				startSql,
				endSql,
				startAlias,
				endAlias,
				parentColumnName,
				parentColumnNameSuffixInSELECTClause,
				parentColumnNameSuffixInASClause);

			return new StringBuilder(sql).Append(innerJoinSQLOnStatement).Append(whereClause).ToString();
		}

		string GenerateSQLInnerJoinOnStatement(string startAlias, string endAlias, string[] startKeyNames, string[] endKeyNames)
		{
			return " ON " + string.Join(" AND ", startKeyNames.Select(
					(startKeyName, index) => string.Format(CultureInfo.InvariantCulture, "{0}.{2} = {1}.{3}", startAlias, endAlias, startKeyName, endKeyNames[index])));
		}

		void GenerateNextSQLParts(Table end, Relation relation, List<MultipleValueCriteria> criterias, Dictionary<string, DB.Sql.SqlParameter> keyToParameters, Dictionary<string, TvpItem> tvpParams, IEntityDefinition startDef, out string startSql, out string startAlias, out string endAlias, out string innerJoinSQLOnStatement, out string whereClause)
		{
			if (!relation.IsMateOf(end))
			{
				throw new NativeXMLUserVisibleException("Table does not belong to Relation");
			}

			var start = relation.Mate(end);
			startSql = ToSql(start, criterias, keyToParameters, tvpParams);
			startAlias = start.Name;

			var startKeys = relation.GetKeys(start);
			var endKeys = relation.GetKeys(end);
			endAlias = end.Name;

			if (startAlias == endAlias)
			{
				startAlias = startAlias + "_1";
				endAlias = endAlias + "_2";
			}

			var startKeyNames = startKeys.Select(x => x.Name).ToArray();
			var endKeyNames = endKeys.Select(x => x.Name).ToArray();

			innerJoinSQLOnStatement = GenerateSQLInnerJoinOnStatement(startAlias, endAlias, startKeyNames, endKeyNames);
			whereClause = GenerateWhereClause(relation.Keys.Select(x => x.FromKey), end == relation.From ? endAlias : startAlias, startDef);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		string GenerateWhereClause(IEnumerable<IColumnDef> endKeys, string endAlias, IEntityDefinition startDef)
		{
			return string.Format(CultureInfo.InvariantCulture, " WHERE {0}", string.Join(" AND ", endKeys.Select(x => GenerateKeyIsNotNullStatement(x, endAlias, startDef))));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		string GenerateKeyIsNotNullStatement(IColumnDef endKey, string endAlias, IEntityDefinition startDef)
		{
			if (endKey.DataType == DbDataType.UniqueIdentifier)
			{
				return string.Format(CultureInfo.InvariantCulture, "{0}.{1} IS NOT NULL{2}", endAlias, endKey.Name, MaybeExcludeRowsWhereColumnIsBlank(endAlias, startDef));
			}
			else
			{
				return string.Format(CultureInfo.InvariantCulture, "NULLIF({0}.{1}, '') IS NOT NULL", endAlias, endKey.Name);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		string MaybeExcludeRowsWhereColumnIsBlank(string endAlias, IEntityDefinition startDef)
		{
			var maybeExcludeRowsWhereThisIsBlank = "";
			var col = startDef.AssociationCollection.FirstOrDefault(a => !String.IsNullOrEmpty(a.WhereThisColumnIsNull) && a.To.TableName == endDefinition.TableName);
			if (col != null)
			{
				var notBlankCol = col.WhereThisColumnIsNull;
				maybeExcludeRowsWhereThisIsBlank = string.Format(CultureInfo.InvariantCulture, " AND {0}.{1} IS NULL", endAlias, notBlankCol);
			}
			return maybeExcludeRowsWhereThisIsBlank;
		}

		string ToSql(Table table, List<MultipleValueCriteria> criterias, Dictionary<string, DB.Sql.SqlParameter> keyToParameters, Dictionary<string, TvpItem> tvpParams)
		{
			var criteriasToUse = criterias.Where(criteria => criteria.TableName == table.Name).ToArray();
			criteriasToUse.ForEach(c => criterias.Remove(c));
			return table.ToSql(keyToParameters, tvpParams, criteriasToUse);
		}
	}
}
