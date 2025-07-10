#if DEBUG
using System;
using System.Collections.Generic;
using CargoWise.Data;

namespace CargoWise.EntityFramework
{
	internal class DbConstraintReader
	{
		DbConstraintReader()
		{
		}

		static Dictionary<string, DbConstraintReader> instances
		{
			get { return _instances ?? (_instances = new Dictionary<string, DbConstraintReader>()); }
		}
		[ThreadStatic]
		static Dictionary<string, DbConstraintReader> _instances;

		protected DbConstraintReader(string tableName)
		{
			this.TableName = tableName;
		}

		public static DbConstraintReader GetInstance(string tableName)
		{
			DbConstraintReader result;
			if (!instances.TryGetValue(tableName, out result))
			{
				result = new DbConstraintReader(tableName);
				instances[tableName] = result;
			}
			return result;
		}

		public readonly string TableName;

		public DbConstraintCollection Constraints
		{
			get
			{
				if (constraints == null)
				{
					List<DbConstraint> result = new List<DbConstraint>();
					var sqlTableName = GetFullyQualifiedSqlTableNameObject(TableName);
					result.AddRange(ReadActualDbConstraints(sqlTableName));
					result.AddRange(ReadUniqueConstraintsFromIndexes(sqlTableName));
					constraints = new DbConstraintCollection(result.ToArray());
				}
				return constraints;
			}
		}

		/// <summary>
		/// Get FullyQualifiedSqlTableName for table catering for reference database sysnonyms
		/// </summary>
		FullyQualifiedSqlTableName GetFullyQualifiedSqlTableNameObject(string tableOrSynonymName)
		{
			string possibleRefDbName;
			string possibleRefDbBaseTable;
			RefDbTableNameResolverTestHelper.GetDbAndBaseTableFromSynonym(tableOrSynonymName, out possibleRefDbName, out possibleRefDbBaseTable);
			var fullTableName = (possibleRefDbName == null) ? tableOrSynonymName : ("[" + possibleRefDbName + "].dbo." + possibleRefDbBaseTable);

			return new FullyQualifiedSqlTableName() { FullyQualifiedName = fullTableName };
		}

		#region Implementation

		DbConstraint[] ReadActualDbConstraints(FullyQualifiedSqlTableName sqlTableName)
		{
			List<DbConstraint> result = new List<DbConstraint>();
			string sqlText = String.Format("EXEC {0}sp_helpconstraint '{1}';",
				(sqlTableName.Database.Length == 0) ? "" : sqlTableName.Database + "..",
				sqlTableName.FullyQualifiedName);
			DbCommand command = Db.Connection.Command(sqlText);

			using (var reader = command.ExecuteReader())
			{
				reader.NextResult();
				while (reader.Read())
				{
					string typeString = reader["constraint_type"].ToString();
					string constraintName = reader["constraint_name"].ToString();
					string foreignTable = null;
					string additionalData = reader["constraint_keys"].ToString();
					DbConstraintType type = GetConstraintTypeFromString(typeString);
					string[] columnNames;
					if (type == DbConstraintType.ForeignKey)
					{
						columnNames = SplitColumnNames(additionalData);
						reader.Read();
						string referencesForeignTable = reader["constraint_keys"].ToString();
						int tableNameStart = referencesForeignTable.LastIndexOf(".") + 1;
						int tableNameEnd = referencesForeignTable.LastIndexOf("(") - 1;
						foreignTable = referencesForeignTable.Substring(tableNameStart, tableNameEnd - tableNameStart);
					}
					else
					{
						columnNames = new string[] { GetColumnNameFromTypeString(typeString) };
					}
					result.Add(new DbConstraint(constraintName, type, columnNames, foreignTable, additionalData));
				}
			}
			return result.ToArray();
		}

		DbConstraint[] ReadUniqueConstraintsFromIndexes(FullyQualifiedSqlTableName sqlTableName)
		{
			List<DbConstraint> result = new List<DbConstraint>();

			string sqlText = String.Format("EXEC {0}sp_helpindex '{1}';",
				(sqlTableName.Database.Length == 0) ? "" : sqlTableName.Database + "..",
				sqlTableName.FullyQualifiedName);
			DbCommand command = Db.Connection.Command(sqlText);

			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					string indexName = reader["index_name"].ToString();
					string description = reader["index_description"].ToString();
					string keyColumns = reader["index_keys"].ToString();
					if (description.IndexOf("unique") != -1)
					{
						string[] columnNames = SplitColumnNames(keyColumns);
						result.Add(new DbConstraint(indexName, DbConstraintType.Unique, columnNames, null, description));
					}
				}
			}
			return result.ToArray();
		}

		protected string[] SplitColumnNames(string keyColumns)
		{
			string[] result = keyColumns.Split(',');
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = result[i].Trim();
			}
			return result;
		}

		protected string GetColumnNameFromTypeString(string value)
		{
			string result = "";
			string onColumnText = " on column ";
			int index = value.IndexOf(onColumnText);
			if (index != -1)
			{
				result = value.Substring(index + onColumnText.Length);
			}
			return result;
		}

		protected DbConstraintType GetConstraintTypeFromString(string value)
		{
			DbConstraintType result = DbConstraintType.Unknown;
			if (value.StartsWith("DEFAULT"))
			{
				result = DbConstraintType.Default;
			}

			if (value.StartsWith("PRIMARY KEY"))
			{
				result = DbConstraintType.PrimaryKey;
			}

			if (value.StartsWith("CHECK"))
			{
				result = DbConstraintType.Check;
			}

			if (value.StartsWith("FOREIGN KEY"))
			{
				result = DbConstraintType.ForeignKey;
			}

			return result;
		}

		DbConstraintCollection constraints;

		#endregion
	}
}

#endif
