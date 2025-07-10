using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace CargoWise.Bi.Development.SchemaSync.Parser;

#pragma warning disable CW1161 // Res.GetString Analyzer

public class TableInfo
{
	public string SchemaName { get; private set; }
	public string TableName { get; private set; }
	public List<ColumnInfo> Columns { get; private set; }

	public static TableInfo Create(CreateTableStatement createTableStatement)
	{
		var tableName = createTableStatement.SchemaObjectName.BaseIdentifier.Value;

		var schemaIdentifier = createTableStatement.SchemaObjectName.SchemaIdentifier;
		var schemaName = schemaIdentifier == null ? "dbo" : schemaIdentifier.Value;

		var info = new TableInfo()
		{
			SchemaName = schemaName,
			TableName = tableName,
			Columns = new List<ColumnInfo>()
		};

		var pkColumns = new List<string>();
		foreach (var constraint in createTableStatement.Definition.TableConstraints)
		{
			if (constraint is UniqueConstraintDefinition uniqueConstraint &&
				uniqueConstraint.IsPrimaryKey)
			{
				pkColumns.AddRange(uniqueConstraint.Columns.Select(column => 
					column.Column.MultiPartIdentifier.Identifiers.Last().Value));
			}
		}

		// the fact that SQL server allows this is disgusting
		// https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=%2FEnterprise%2FClientExtensions%2FEDI%2FZClientEDI%2FZClientEDI%2FClientConfiguration%2FEDIClientDbSchemaUpgradeInfo.cs&version=GBmaster&line=4333&lineEnd=4342&lineStartColumn=1&lineEndColumn=2&lineStyle=plain&_a=contents
		foreach (var column in createTableStatement.Definition.ColumnDefinitions)
		{
			foreach (var uniqueConstraint in column.Constraints.OfType<UniqueConstraintDefinition>())
			{
				if (!uniqueConstraint.IsPrimaryKey)
				{
					continue;
				}

				var columns = uniqueConstraint.Columns;

				if (columns.Count == 0)
				{
					continue;
				}

				var referencedColumn = columns.Single();
				pkColumns.Add(referencedColumn.Column.MultiPartIdentifier.Identifiers.Last().Value);
			}
		}

		foreach (var column in createTableStatement.Definition.ColumnDefinitions)
		{
			var columnInfo = ColumnInfo.Create(column, pkColumns);

			if (columnInfo == null)
			{
				continue;
			}

			info.Columns.Add(columnInfo);
		}

		return info;
	}
}

#pragma warning restore CW1161 // Res.GetString Analyzer
