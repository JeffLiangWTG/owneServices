using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using CargoWise.Data;

namespace Enterprise.BusinessObjectGenerator
{
	public class AutoSchema : AutoSourceFile
	{
		public AutoSchema(DataTable[] persistentTablesColumns, DataTable[] nonPersistentTablesColumns)
		{
			this.PersistentTablesColumns = persistentTablesColumns;
			this.NonPersistentTablesColumns = nonPersistentTablesColumns;
		}

		#region Code for Body

		protected override string Body
		{
			get
			{
				string result = @"
using CargoWise.Schema;

namespace Enterprise.ZArchitecture.Schema
{
	public abstract class AutoEnterpriseSchema
	{
		/// <summary>
		/// This method specifically and intentionally only deals with the standard tables, not client specific tables.
		/// This is almost certainly not the method you want.
		/// </summary>
		protected internal static ITableSchema GetStandardTableSchema(string tableName)
		{
			switch (tableName)
			{
" + GetCodeForGetTableSchemaCaseStatements() +
@"			}

			return null;
		}

		/// <summary>
		/// This method specifically and intentionally only deals with the standard tables, not client specific tables.
		/// This is almost certainly not the method you want.
		/// </summary>
		protected internal static ITableSchema GetStandardTableSchemaFromColumnNamePrefix(string columnNamePrefix)
		{
			switch (columnNamePrefix)
			{
" + GetCodeForGetTableSchemaFromPrefixCaseStatements() +
@"			}

			return null;
		}
	}
}";
				return result;
			}
		}

		string GetCodeForGetTableSchemaCaseStatements()
		{
			StringBuilder result = new StringBuilder();
			foreach (string tableName in AllTableNames)
			{
				result.Append(
					"				case " + tableName + "Schema.Constants.TableName" + WhiteSpaceForTableName(tableName) +
					" : return " + tableName + "Schema.Instance;" + System.Environment.NewLine);
			}
			return result.ToString();
		}

		string GetCodeForGetTableSchemaFromPrefixCaseStatements()
		{
			StringBuilder result = new StringBuilder();
			foreach (KeyValuePair<string, string> table in GetTablesAndPrefixes(new HashSet<string>(AllTableNames)))
			{
				string tableName = table.Key;
				string columnNamePrefix = table.Value;

				result.Append(
					"				case \"" + columnNamePrefix + "\"" +
					" : return " + tableName + "Schema.Instance;" + System.Environment.NewLine);
			}
			return result.ToString();
		}

		protected virtual Dictionary<string, string> GetTablesAndPrefixes(HashSet<string> tableNamesToInclude)
		{
			var result = new Dictionary<string, string>();

			AddTablesAndPrefixes(TablesAndPrefixesSqlText, result, tableNamesToInclude);
			AddTablesAndPrefixes(ViewsAndPrefixesSqlText, result, tableNamesToInclude, addUniquePrefixesOnly: true);

			return result;
		}

		const string TablesAndPrefixesSqlText = @"
			SELECT 
				name,
				(SELECT top 1 left(col.name, CASE substring(col.name,3,1) WHEN '_' THEN 2 ELSE 3 END)
					FROM sys.columns col
					WHERE col.object_id = tab.object_id) prefix
			FROM sys.tables tab
			WHERE tab.is_ms_shipped = 0
			AND   tab.name NOT LIKE 'Client%'
			AND   tab.name NOT LIKE 'RptDt%'
			ORDER BY tab.name
			";

		const string ViewsAndPrefixesSqlText = @"
			SELECT 
				name,
				(SELECT top 1 left(col.name, CASE substring(col.name,3,1) WHEN '_' THEN 2 ELSE 3 END)
					FROM sys.columns col
					WHERE col.object_id = vw.object_id) prefix
			FROM sys.views vw
			WHERE vw.is_ms_shipped = 0
			ORDER BY vw.name
			";

		static void AddTablesAndPrefixes(string selectSqlText, Dictionary<string, string> result, HashSet<string> tableNamesToInclude, bool addUniquePrefixesOnly = false)
		{
			var prefixes = addUniquePrefixesOnly ? new HashSet<string>(result.Select(x => x.Value)) : null;

			using (var reader = Db.Connection.Command(selectSqlText).ExecuteReader())
			{
				while (reader.Read())
				{
					var tableName = (string)reader["name"];
					var columnNamePrefix = (string)reader["prefix"];

					if (tableNamesToInclude.Contains(tableName))
					{
						if (prefixes != null)
						{
							if (prefixes.Contains(columnNamePrefix))
							{
								continue;
							}
							else
							{
								prefixes.Add(columnNamePrefix);
							}
						}

						result.Add(tableName, columnNamePrefix);
					}
				}
			}
		}

		#endregion

		#region Implementation

		readonly DataTable[] PersistentTablesColumns;
		readonly DataTable[] NonPersistentTablesColumns;

		string WhiteSpaceForTableName(string tableName)
		{
			return new string(' ', MaxTableNameLength - tableName.Length);
		}

		int MaxTableNameLength
		{
			get
			{
				if (fMaxTableNameLength == -1)
				{
					foreach (string tableName in AllTableNames)
					{
						if (tableName.Length > fMaxTableNameLength)
						{
							fMaxTableNameLength = tableName.Length;
						}
					}
				}

				return fMaxTableNameLength;
			}
		}
		int fMaxTableNameLength = -1;

		string[] AllTableNames
		{
			get
			{
				if (fAllTableNames == null)
				{
					List<string> list = new List<string>();

					foreach (DataTable table in AllTablesColumns)
					{
						if (!list.Contains(table.TableName))
						{
							list.Add(table.TableName);
						}
					}

					list.Sort();
					fAllTableNames = list.ToArray();
				}
				return fAllTableNames;
			}
		}
		string[] fAllTableNames;

		DataTable[] AllTablesColumns
		{
			get
			{
				if (fAllTablesColumns == null)
				{
					fAllTablesColumns = new DataTable[PersistentTablesColumns.Length + NonPersistentTablesColumns.Length];
					PersistentTablesColumns.CopyTo(fAllTablesColumns, 0);
					NonPersistentTablesColumns.CopyTo(fAllTablesColumns, PersistentTablesColumns.Length);
				}
				return fAllTablesColumns;
			}
		}
		DataTable[] fAllTablesColumns;

		#endregion
	}
}
