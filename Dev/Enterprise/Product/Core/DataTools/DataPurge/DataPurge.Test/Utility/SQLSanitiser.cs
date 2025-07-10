using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace Enterprise.DataPurge.Test.Utility;

static class SQLSanitiser
{
	/// <summary>
	/// <para>Remove CREATE INDEX commands</para>
	/// <para>Remove ADD CONSTRAINTS (which are not PKs and FKs) commands</para>
	/// <para>Remove ADD CONSTRAINTS for multipart keys (since we already removed all the indexes)</para>
	/// <para>Make nullable columns which are not UNIQUEIDENTIFIER nor have a DEFAULT nor are computed 'PERSISTED NOT NULL'</para>
	/// <para>Add DEFAULT = newid() for UNIQUEIDENTIFIER PK columns</para>
	/// <para>Add a DEFAULT = empty_guid for UNIQUEIDENTIFIER non-nullable non-PK columns</para>
	/// <para>Remove XML Schema References</para>
	/// </summary>
	public static string Sanitise(string sql)
	{
		var parser = new TSql160Parser(true);
		var statements = parser.ParseStatementList(new StringReader(sql), out var errors);

		if (errors.Count > 0)
		{
			throw new ArgumentException($"Error parsing SQL:{System.Environment.NewLine}{string.Join(System.Environment.NewLine, errors.Select(e => e.Message))}");
		}

		statements.Accept(new RemovalVisitor());

		(statements.Statements as List<TSqlStatement>)
			.RemoveAll(statement => statement is AlterTableAddTableElementStatement alterTableStatement && AlterTableAddElementStatementIsEmpty(alterTableStatement) ||
									statement is CreateIndexStatement ||
									statement is AlterTableSetStatement ||
									statement is CreateSpatialIndexStatement ||
									statement is IndexStatement ||
									statement is CreateXmlIndexStatement);

		var generator = new Sql160ScriptGenerator();
		generator.GenerateScript(statements, out var sanitisedSql);

		return sanitisedSql;
	}

	static bool AlterTableAddElementStatementIsEmpty(AlterTableAddTableElementStatement node)
	{
		var definition = node.Definition;
		return definition.ColumnDefinitions.Count == 0 && definition.TableConstraints.Count == 0 && definition.Indexes.Count == 0;
	}
}

class RemovalVisitor : TSqlFragmentVisitor
{
	public override void Visit(CreateTableStatement node)
	{
		var definition = node.Definition;
		definition.Indexes.Clear();
		RemoveNonPKorFKConstraints(definition.TableConstraints);
	}

	public override void Visit(ColumnDefinition node)
	{
		node.Index = null;
		RemoveNonPKorFKConstraints(node.Constraints);

		var nullabilityConstraint = node.Constraints.FirstOrDefault(constraint => constraint is NullableConstraintDefinition) as NullableConstraintDefinition;
		if (nullabilityConstraint != null && !nullabilityConstraint.Nullable &&
			!(
			(node.DataType is SqlDataTypeReference sqlDataType && sqlDataType.SqlDataTypeOption == SqlDataTypeOption.UniqueIdentifier) ||
			node.DefaultConstraint != null ||
			node.IdentityOptions != null ||
			node.ComputedColumnExpression != null
			))
		{
			nullabilityConstraint.Nullable = true;
		}

		var nodeNameSplit = node.ColumnIdentifier.Value.Split('_');
		if (nodeNameSplit.Length == 2 && nodeNameSplit[1].ToUpperInvariant() == "PK" && node.DefaultConstraint == null)
		{
			node.DefaultConstraint = new DefaultConstraintDefinition
			{
				Expression = new ParenthesisExpression
				{
					Expression = new FunctionCall
					{
						FunctionName = new Identifier { Value = "newid" }
					}
				}
			};
		}
		else if (nodeNameSplit.Last().ToUpperInvariant() != "PK" && node.DefaultConstraint == null &&
				(node.DataType is SqlDataTypeReference sqlType && sqlType.SqlDataTypeOption == SqlDataTypeOption.UniqueIdentifier) &&
				 nullabilityConstraint != null && !nullabilityConstraint.Nullable)
		{
			node.DefaultConstraint = new DefaultConstraintDefinition
			{
				Expression = new ParenthesisExpression
				{
					Expression = new StringLiteral
					{
						Value = "00000000-0000-0000-0000-000000000000"
					}
				}
			};
		}

		if (node.DataType is XmlDataTypeReference xmlDataType &&
			xmlDataType.XmlDataTypeOption == XmlDataTypeOption.Content &&
			xmlDataType.XmlSchemaCollection != null)
		{
			xmlDataType.XmlSchemaCollection = null;
			xmlDataType.XmlDataTypeOption = XmlDataTypeOption.None;
		}
	}

	public override void Visit(AlterTableAddTableElementStatement node)
	{
		var definition = node.Definition;
		definition.Indexes.Clear();
		RemoveNonPKorFKConstraints(definition.TableConstraints, removeMultipart: true);
	}

	void RemoveNonPKorFKConstraints(IList<ConstraintDefinition> constraints, bool removeMultipart = false)
	{
		Func<ConstraintDefinition, bool> constraintPredicate = removeMultipart ? IsPrimaryKeyOrForeignKeyConstraintAndNotMultipart : IsPrimaryKeyOrForeignKeyConstraint;
		var constraintsToKeep = constraints.Where(ShouldKeepConstraint).ToArray();

		constraints.Clear();
		foreach (var constraintToKeep in constraintsToKeep)
		{
			constraints.Add(constraintToKeep);
		}

		bool ShouldKeepConstraint(ConstraintDefinition constraint)
		{
			return constraintPredicate(constraint) || constraint.ConstraintIdentifier == null;
		}
	}

	bool IsPrimaryKeyOrForeignKeyConstraint(ConstraintDefinition constraint)
	{
		return constraint is UniqueConstraintDefinition uniqueConstraint && uniqueConstraint.IsPrimaryKey
			|| constraint is ForeignKeyConstraintDefinition;
	}

	bool IsPrimaryKeyOrForeignKeyConstraintAndNotMultipart(ConstraintDefinition constraint)
	{
		return constraint is UniqueConstraintDefinition uniqueConstraint && uniqueConstraint.IsPrimaryKey && uniqueConstraint.Columns.Count <= 1
			|| constraint is ForeignKeyConstraintDefinition foreignKeyConstraint && foreignKeyConstraint.Columns.Count <= 1;
	}
}
