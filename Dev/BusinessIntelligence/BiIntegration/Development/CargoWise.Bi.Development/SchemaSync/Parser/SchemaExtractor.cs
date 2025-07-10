using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Bi.Development.Common.SQL;
using CargoWise.Bi.Development.SchemaSync.DataSets;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace CargoWise.Bi.Development.SchemaSync.Parser;

public class SchemaExtractor
{
	readonly Dictionary<string, TableInfo> Tables = new Dictionary<string, TableInfo>();

	readonly Dictionary<string, string> ReferencedTables = new Dictionary<string, string>();

	readonly Dictionary<string, byte> PrimaryKeyColumns = new Dictionary<string, byte>();

	readonly IEnumerable<TextReader> scripts;

	public SchemaExtractor(IEnumerable<TextReader> scripts)
	{
		this.scripts = scripts;
	}

	public void ExtractSchema(SchemaDataSet schema)
	{
		var parser = new TSql160Parser(true);
		var visitor = new TableVisitor(this);

		foreach (var script in scripts)
		{
			var statementList = parser.ParseStatementList(script, out var errors);
			if (errors.Count > 0)
			{
				var errorBuilder = new StringBuilder();
				foreach (var error in errors)
				{
					errorBuilder.AppendLine($"Error: {error.Message}");
				}

				throw new InvalidOperationException(errorBuilder.ToString());
			}

			foreach (var statement in statementList.Statements)
			{
				if (statement is CreateTableStatement createTableStatement)
				{
					visitor.ExplicitVisit(createTableStatement);
				}
				else if (statement is AlterTableAddTableElementStatement alterTableAddTableElementStatement)
				{
					visitor.ExplicitVisit(alterTableAddTableElementStatement);
				}
			}
		}

		var dataTable = schema.Definition;

		foreach (var table in Tables.Values)
		{
			if (SQLConstants.ExcludedTables.Contains(table.TableName, StringComparer.Ordinal))
			{
				continue;
			}

			foreach (var column in table.Columns)
			{
				var (referenceSchema, referenceTable) = GetReference(column.Name);
				var dataType = column.DataTypeInfo;

				dataTable.Rows.Add(
					table.SchemaName,
					table.TableName,
					column.Name,
					dataType.Name,
					dataType.MaxLength,
					dataType.Precision,
					dataType.Scale,
					column.Nullable,
					column.IsPrimaryKey || PrimaryKeyColumns.ContainsKey(column.Name),
					referenceSchema,
					referenceTable
				);
			}
		}
	}

	(string referenceSchema, string referenceTable) GetReference(string column)
	{
		if (ReferencedTables.TryGetValue(column, out var referenceTable))
		{
			var table = Tables[referenceTable.ToLowerInvariant()];
			return (table.SchemaName, table.TableName);
		}

		return (null, null);
	}

	class TableVisitor : TSqlFragmentVisitor
	{
		readonly SchemaExtractor schemaExtractor;

		public TableVisitor(SchemaExtractor schemaExtractor)
		{
			this.schemaExtractor = schemaExtractor;
		}

		public override void ExplicitVisit(CreateTableStatement node)
		{
			var info = TableInfo.Create(node);

			foreach (var column in node.Definition.ColumnDefinitions)
			{
				var foreignKeys = column.Constraints.OfType<ForeignKeyConstraintDefinition>();
				var foreignKey = foreignKeys.SingleOrDefault();

				if (foreignKey != null)
				{
					var columnName = column.ColumnIdentifier.Value;
					var referencedTable = foreignKey.ReferenceTableName.BaseIdentifier.Value;

					schemaExtractor.ReferencedTables.Add(columnName, referencedTable);
				}
			}

			foreach (var foreignKeyConstraint in node.Definition.TableConstraints.OfType<ForeignKeyConstraintDefinition>())
			{
				var referencedTable = foreignKeyConstraint.ReferenceTableName.BaseIdentifier.Value;
				foreach (var column in foreignKeyConstraint.Columns)
				{
					var columnName = column.Value;

					schemaExtractor.ReferencedTables.Add(columnName, referencedTable);
				}
			}

			schemaExtractor.Tables.Add(info.TableName.ToLowerInvariant(), info);
		}

		public override void ExplicitVisit(AlterTableAddTableElementStatement node)
		{
			foreach (var constraint in node.Definition.TableConstraints)
			{
				if (constraint is ForeignKeyConstraintDefinition foreignKeyConstraint)
				{
					if (foreignKeyConstraint.Columns.Count > 1 || foreignKeyConstraint.ReferenceTableName == null)
					{
						continue;
					}

					var column = foreignKeyConstraint.Columns.First().Value;
					var referencedTable = foreignKeyConstraint.ReferenceTableName.BaseIdentifier.Value;

					schemaExtractor.ReferencedTables.Add(column, referencedTable);
				}
				else if (constraint is UniqueConstraintDefinition uniqueConstraint && uniqueConstraint.IsPrimaryKey)
				{
					var pkColumn = uniqueConstraint.Columns.First();
					var pkColumnName = pkColumn.Column.MultiPartIdentifier.Identifiers.Last().Value;

					schemaExtractor.PrimaryKeyColumns.Add(pkColumnName, 0);
				}
			}
		}
	}	
}
