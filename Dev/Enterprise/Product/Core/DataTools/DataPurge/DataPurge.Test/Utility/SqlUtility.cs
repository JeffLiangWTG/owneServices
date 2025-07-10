using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.Types;

namespace Enterprise.DataPurge.Utility
{
	public class SqlUtility
	{
		public SqlUtility(DbConnection testConnection)
		{
			this.testConnection = testConnection;
		}

		public IEnumerable<ForeignKeyInfo> GetForeignKeysReferToTable(string tableName)
		{
			var sqlTemplate = $@"SELECT
	tab1.name AS [TableName],
	col1.name AS [ColumnName]
FROM
	sys.foreign_key_columns fkc
	JOIN sys.objects AS obj ON obj.object_id = fkc.constraint_object_id
	JOIN sys.tables AS tab1 ON tab1.object_id = fkc.parent_object_id
	JOIN sys.schemas AS sch ON tab1.schema_id = sch.schema_id
	JOIN sys.columns AS col1 ON col1.column_id = parent_column_id AND col1.object_id = tab1.object_id
	JOIN sys.tables AS tab2 ON tab2.object_id = fkc.referenced_object_id
	JOIN sys.columns AS col2 ON col2.column_id = referenced_column_id AND col2.object_id = tab2.object_id
WHERE
	tab2.name = '{tableName}'
	AND sch.name = 'dbo'
";

			var command = testConnection.Command(sqlTemplate);
			var foreignKeyTable = new DataTable() { Locale = CultureInfo.InvariantCulture };
			var results = new List<ForeignKeyInfo>();
			foreignKeyTable.Load(command.ExecuteReader());
			foreach (DataRow row in foreignKeyTable.Rows)
			{
				var fkInfo = new ForeignKeyInfo(row["ColumnName"].ToString(), row["TableName"].ToString());
				results.Add(fkInfo);
			}

			return results;
		}

		public IEnumerable<ForeignKeyInfo> GetForeignKeysUsedByTable(string tableName)
		{
			var sqlTemplate = $@"SELECT
	c.name AS [ColumnName],
	ft.name AS [ForeignTableName]
FROM
	sys.foreign_keys AS fk
	JOIN sys.foreign_key_columns AS fkc ON fk.object_id = fkc.constraint_object_id
	JOIN sys.columns AS c ON fkc.parent_object_id = c.object_id AND fkc.parent_column_id = c.column_id
	JOIN sys.tables AS pt ON fk.parent_object_id = pt.object_id
	JOIN sys.tables AS ft ON fk.referenced_object_id = ft.object_id
	JOIN sys.schemas AS sch ON pt.schema_id = sch.schema_id
WHERE
	pt.name = '{tableName}'
	AND sch.name = 'dbo'
";

			var command = testConnection.Command(sqlTemplate);
			var foreignKeyTable = new DataTable() { Locale = CultureInfo.InvariantCulture };
			var results = new List<ForeignKeyInfo>();
			foreignKeyTable.Load(command.ExecuteReader());
			foreach (DataRow row in foreignKeyTable.Rows)
			{
				var fkInfo = new ForeignKeyInfo(row["ColumnName"].ToString(), row["ForeignTableName"].ToString());
				results.Add(fkInfo);
			}

			return results;
		}

		public string GetPrimaryKeyNameByTableName(string tableName)
		{
			var sqlTemplate = $@"SELECT COLUMN_NAME
FROM
	INFORMATION_SCHEMA.KEY_COLUMN_USAGE
WHERE
	OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + QUOTENAME(CONSTRAINT_NAME)), 'IsPrimaryKey') = 1
	AND TABLE_NAME = '{tableName}'
	AND TABLE_SCHEMA = 'dbo'
";

			try
			{
				var pkName = testConnection.ExecuteScalar<string>(sqlTemplate);
				return pkName;
			}
			catch (ExecuteScalarReturnedNullException)
			{
				return null;
			}
		}

		public IEnumerable<string> ConvertNodePathToSql(IEnumerable<Node> nodes, ZGuid companyPK)
		{
			var nodesToBePermutation = new List<List<SqlNode>>();
			foreach (var node in nodes)
			{
				var currentGroup = new List<SqlNode>();
				var fkInfos = GetForeignKeysUsedByTable(node.PKTableName);
				var companyFKInfo = fkInfos.FirstOrDefault(x => x.FKTableName == "GlbCompany");
				if (companyFKInfo == null)
				{
					currentGroup.Add(new SqlNode(null, ZGuid.Invalid, node.PKTableName, node.FKTableName, node.PKName, node.FKName));
				}
				else
				{
					currentGroup.Add(new SqlNode(companyFKInfo.FKName, ZGuid.Empty, node.PKTableName, node.FKTableName, node.PKName, node.FKName));
					currentGroup.Add(new SqlNode(companyFKInfo.FKName, companyPK, node.PKTableName, node.FKTableName, node.PKName, node.FKName));
				}
				nodesToBePermutation.Add(currentGroup);
			}
			var rawNodeGroup = CreateTraversalPath(nodesToBePermutation);
			var validNodeGroup = GetPathsWithNonEmptyGlbCompany(rawNodeGroup);
			var sqlTemplates = GenerateInsertNodeGroupSql(validNodeGroup);
			return sqlTemplates;
		}

		public ZGuid GenerateCompany()
		{
			var companyPK = ZGuid.NewZGuid();
			var sqlTemplate = $"INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name) VALUES ('{companyPK}', 'DAN', 'AN company')";
			testConnection.ExecuteNonQuery(sqlTemplate);
			return companyPK;
		}

		public ZGuid GenerateBranchBelongToCompany(ZGuid companyPK)
		{
			var branchPK = ZGuid.NewZGuid();
			var sqlTemplate = $"INSERT dbo.GlbBranch(GB_PK,GB_GC) VALUES ('{branchPK}','{companyPK}')";
			testConnection.ExecuteNonQuery(sqlTemplate);
			return branchPK;
		}

		public void BatchInsertRows(List<string> sqlTemplates, int batchSize = 500)
		{
			var sqlTemplate = new StringBuilder();
			for (var i = 0; i < sqlTemplates.Count; i++)
			{
				sqlTemplate.Append(sqlTemplates[i]);
				if (i % batchSize == 0)
				{
					testConnection.ExecuteNonQuery(sqlTemplate.ToString());
					sqlTemplate = new StringBuilder();
				}
			}

			if (!string.IsNullOrWhiteSpace(sqlTemplate.ToString()))
			{
				testConnection.ExecuteNonQuery(sqlTemplate.ToString());
			}
		}

		public IEnumerable<IEnumerable<T>> CreateTraversalPath<T>(List<List<T>> nodeGroups)
		{
			if (nodeGroups.Count == 0)
			{
				return new List<List<T>>();
			}

			var result = nodeGroups[0].Select(eachNodeInFirstGroup => new List<T>() { eachNodeInFirstGroup }).ToList();

			for (var i = 1; i < nodeGroups.Count; i++)
			{
				var pre = result;
				result = new List<List<T>>();
				foreach (var nodeGroup in pre)
				{
					foreach (var currentNode in nodeGroups[i])
					{
						var tempNodes = new List<T>(nodeGroup);
						tempNodes.Add(currentNode);
						result.Add(tempNodes);
					}
				}
			}

			return result;
		}

		void SetValueForCompanyFK(SqlNode node, ZGuid pkToUpdate, List<string> sqlTemplates)
		{
			if (string.IsNullOrWhiteSpace(node.CompanyFKName))
			{
				return;
			}

			if (!string.IsNullOrWhiteSpace(node.PKName))
			{
				var sqlTemplate = $"UPDATE dbo.{node.PKTableName} SET {node.CompanyFKName}='{node.CompanyPk}' WHERE {node.PKName} = '{pkToUpdate}'";
				sqlTemplates.Add(sqlTemplate);
				return;
			}

			if (!string.IsNullOrWhiteSpace(node.FKName))
			{
				var sqlTemplate = $"UPDATE dbo.{node.PKTableName} SET {node.CompanyFKName}='{node.CompanyPk}' WHERE {node.FKName} = '{pkToUpdate}'";
				sqlTemplates.Add(sqlTemplate);
				return;
			}
		}

		IEnumerable<string> GenerateInsertNodeGroupSql(IEnumerable<IEnumerable<SqlNode>> nodePathGroup)
		{
			var sqlTemplates = new List<string>();
			foreach (var path in nodePathGroup)
			{
				var previousPK = ZGuid.Invalid;
				foreach (var node in path)
				{
					var currentPK = ZGuid.NewZGuid();
					var sqlTemplate = string.Empty;

					if (previousPK == ZGuid.Invalid)
					{
						sqlTemplate = $"INSERT dbo.{node.PKTableName} ({node.PKName}) VALUES ('{currentPK}')";
						sqlTemplates.Add(sqlTemplate);
						SetValueForCompanyFK(node, currentPK, sqlTemplates);
						previousPK = currentPK;
					}
					else if (!string.IsNullOrWhiteSpace(node.PKName))
					{
						sqlTemplate = $"INSERT dbo.{node.PKTableName} ({node.PKName}, {node.FKName}) VALUES ('{currentPK}', '{previousPK}')";
						sqlTemplates.Add(sqlTemplate);
						SetValueForCompanyFK(node, currentPK, sqlTemplates);
						previousPK = currentPK;
					}
					else
					{
						sqlTemplate = $"INSERT dbo.{node.PKTableName} ({node.FKName}) VALUES ('{previousPK}')";
						sqlTemplates.Add(sqlTemplate);
						SetValueForCompanyFK(node, previousPK, sqlTemplates);
					}
				}
			}

			return sqlTemplates;
		}

		IEnumerable<IEnumerable<SqlNode>> GetPathsWithNonEmptyGlbCompany(IEnumerable<IEnumerable<SqlNode>> nodePathGroup)
		{
			var filteredPaths = new List<List<SqlNode>>();
			foreach (var path in nodePathGroup)
			{
				var isValidPath = false;
				foreach (var node in path)
				{
					if (node.CompanyPk != ZGuid.Empty && node.CompanyPk != ZGuid.Invalid)
					{
						isValidPath = true;
					}
				}

				if (isValidPath)
				{
					filteredPaths.Add(path.ToList());
				}
			}

			return filteredPaths;
		}

		readonly DbConnection testConnection;
	}
}
