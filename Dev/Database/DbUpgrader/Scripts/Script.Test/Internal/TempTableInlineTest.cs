using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace Enterprise.Build.Database.Script.Testing.Internal
{
	internal class TempTableInlineTest
	{
		public List<string> TestTempTableIndexesAreInline(string sqlText)
		{
			return AssertNoVisitorFailures<TestIndexVisitor>(sqlText);
		}

		public List<string> TestTempTableConstraintsAreInline(string sqlText)
		{
			return AssertNoVisitorFailures<TestConstraintVisitor>(sqlText);
		}

		public List<string> TestNoAlterTempTableColumns(string sqlText)
		{
			return AssertNoVisitorFailures<TestAlterAddColumnVisitor>(sqlText);
		}

		List<string> AssertNoVisitorFailures<TVisitor>(string sqlText) where TVisitor : BaseTestVisitor, new()
		{
			var parser = new TSql160Parser(false);
			var failures = new List<string>();

			using var stream = new MemoryStream(Encoding.UTF8.GetBytes(sqlText));
			using var reader = new StreamReader(stream);
			var tokens = parser.Parse(reader, out var parseErrors);
			if (parseErrors.Count > 0)
			{
				failures.Add("Script failed to parse");
				return failures;
			}
			var visitor = new TVisitor();

			tokens.Accept(visitor);

			if (visitor.Failures.Count > 0)
			{
				failures.AddRange(visitor.Failures);
			}
			return failures;
		}
	}

	public class BaseTestVisitor : TSqlFragmentVisitor
	{
		protected readonly Regex tempTableRegex = new Regex(@"\[?#.*");
		public List<string> Failures { get; set; }

		public BaseTestVisitor()
		{
			Failures = new List<string>();
		}

		public override void ExplicitVisit(CreateProcedureStatement node)
		{
			//Get the statements within the create procedure statement
			foreach (var statement in node.StatementList.Statements)
			{
				ExplicitVisit(statement);
			}
		}
	}

	public class TestIndexVisitor : BaseTestVisitor
	{
		public TestIndexVisitor() : base()
		{
		}

		public override void ExplicitVisit(CreateIndexStatement node)
		{
			if (!tempTableRegex.IsMatch(node.OnName.BaseIdentifier.Value))
			{
				return;
			}

			Failures.Add(node.Name.Value);
			base.ExplicitVisit(node);
		}
	}

	public class TestConstraintVisitor : BaseTestVisitor
	{
		public TestConstraintVisitor() : base()
		{
		}

		public override void ExplicitVisit(AlterTableAddTableElementStatement node)
		{
			if (!tempTableRegex.IsMatch(node.SchemaObjectName.BaseIdentifier.Value))
			{
				return;
			}

			foreach (var constraint in node.Definition.TableConstraints)
			{
				if (constraint is UniqueConstraintDefinition uCon && uCon.IsPrimaryKey)
				{
					continue;
				}

				Failures.Add(constraint.ConstraintIdentifier.Value);
			}
		}
	}

	public class TestAlterAddColumnVisitor : BaseTestVisitor
	{
		public TestAlterAddColumnVisitor() : base()
		{
		}

		public override void ExplicitVisit(AlterTableAddTableElementStatement node)
		{
			if (!tempTableRegex.IsMatch(node.SchemaObjectName.BaseIdentifier.Value))
			{
				return;
			}

			foreach (var column in node.Definition.ColumnDefinitions)
			{
				Failures.Add(column.ColumnIdentifier.Value);
			}
		}
	}
}
