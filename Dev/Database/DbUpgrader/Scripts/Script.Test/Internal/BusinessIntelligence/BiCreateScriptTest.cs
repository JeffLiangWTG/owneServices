using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Abstractions;
using Enterprise.Build.Database.Script.Testing.Internal.BusinessIntelligence;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script
{
	[TestsSubclassesOf(typeof(BiCreateScript))]
	abstract class BiCreateScriptTest : BaseDbScriptTest
	{
		public void AssertDataTables(string message, DataTable expectedDataTable, DataTable actualDataTable)
		{
			var columnIndex = 0;
			foreach (DataColumn column in actualDataTable.Columns.OfType<DataColumn>().OrderBy(c => c.ColumnName))
			{
				column.SetOrdinal(columnIndex);
				columnIndex++;
			}

			columnIndex = 0;
			foreach (DataColumn column in expectedDataTable.Columns.OfType<DataColumn>().OrderBy(c => c.ColumnName))
			{
				column.SetOrdinal(columnIndex);
				column.ColumnName = column.ColumnName.Replace("_x2024_", ".");
				columnIndex++;
			}

			CombineAssertions(() =>
			{
				foreach (DataColumn column in actualDataTable.Columns)
				{
					Assert("The column " + column.ColumnName + " doesnt exist in expected result set", expectedDataTable.Columns.Contains(column.ColumnName));
					if (expectedDataTable.Columns.Contains(column.ColumnName) &&
						!column.DataType.FullName.Equals("Microsoft.SqlServer.Types.SqlGeography", StringComparison.OrdinalIgnoreCase))
					{
						Assert(string.Format(CultureInfo.InvariantCulture, "{0} \n\r{1} column datatype doesn't match expected column datatype.", message, column.ColumnName), expectedDataTable.Columns[column.ColumnName]?.DataType == column.DataType);
					}
				}
			});

			var actualDataTableArray = actualDataTable.Rows.Cast<DataRow>().Select(r => string.Join(",", r.ItemArray.Select(x => x.ToString()))).ToArray();
			var expectedDataTableArray = expectedDataTable.Rows.Cast<DataRow>().Select(r => string.Join(",", r.ItemArray.Select(x => x.ToString()))).ToArray();
			AssertContainsExactElementsInAnyOrder(message, expectedDataTableArray, actualDataTableArray);
		}

		public void TestColumnsAreAsExpected(string viewName, HashSet<string> columns)
		{
			//Prepare
			string sqlText = string.Format(CultureInfo.InvariantCulture,
				@"Select c.[name]
				From [{0}].sys.columns c
				Join [{0}].sys.views v on v.object_id = c.object_id
				Where v.[name] = '{1}'
				Order by c.[name] asc",
				Db.EdwDatabaseName,
				viewName
			);

			//Execute
			var resultsTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);

			//Test
			var columnCount = 0;
			CombineAssertions(() =>
			{
				foreach (DataRow row in resultsTable.Rows)
				{
					var columnExists = columns.Any(x => x == row[0].ToString());
					AssertEquals($"Expected {row[0]} to exist in existing list of columns", columnExists, true);
					if (columnExists)
					{
						columnCount++;
					}
				}
			});

			AssertEquals(columns.Count, columnCount);
		}

		protected void CheckContainsTPTTComments(string prefix, int expectedCount)
		{
			var actualCount = Regex.Matches(String.Join("\n", ScriptToTest.Text), @$"Table:{prefix}_").Count;
			AssertEquals(expectedCount, actualCount);
		}

		public void TestCheckSqlRules()
		{
			var parser = new TSql150Parser(true, SqlEngineType.All);
			IReadOnlyCollection<RuleVisitor> rules = new List<RuleVisitor>
			{
				new DoNotExplicitlyDropTemporaryTables(),
				new DoNotTruncateTemporaryTables(),
				new DoNotAlterTemporaryTables(),
				new IndexOnTemporaryTablesShouldUsingInlineSyntax(),
			};

			using (var reader = new StringReader(ScriptToTest.Text))
			{
				var fragment = parser.Parse(reader, out _);

				foreach (var visitor in rules)
				{
					if (visitor.BaseLine.Contains(ScriptToTest.Name))
					{
						continue;
					}
					fragment.Accept(visitor);
				}
			}

			CombineAssertions($"File: {ScriptToTest.Name}.sql", () =>
			{
				foreach (var visitor in rules)
				{
					AssertEquals(visitor.Message + Environment.NewLine + string.Join(Environment.NewLine, visitor.Objects.ToArray()), 0, visitor.Objects.Count);
				}
			});
		}

		class DoNotExplicitlyDropTemporaryTables : RuleVisitor
		{
			public override string Message => "Do not explicitly drop temp tables at the end of a stored procedure, they will get cleaned up when the session that created them ends.";

			public override string[] BaseLine => new string[] {
				"usp_InitialLoadTable",
				"usp_RecreatePartitionsAndPurgeOldData",
			};

			public override void Visit(DropTableStatement node)
			{
				foreach (var obj in node.Objects.Select(o => o.BaseIdentifier.Value).Where(o => o.StartsWith("#")))
				{
					Objects.Add(new NameAndLocation { Name = obj, Line = node.StartLine });
				}
				base.Visit(node);
			}
		}

		class DoNotTruncateTemporaryTables : RuleVisitor
		{
			public override string Message => "Do not truncate temp tables";

			public override string[] BaseLine => new string[] {
				"usp_MasterTransform",
				"usp_RecreatePartitionsAndPurgeOldData",
			};

			public override void Visit(TruncateTableStatement node)
			{
				if (node.TableName.BaseIdentifier.Value.StartsWith("#"))
				{
					Objects.Add(new NameAndLocation { Name = node.TableName.BaseIdentifier.Value, Line = node.StartLine });
				}
				base.Visit(node);
			}
		}

		class DoNotAlterTemporaryTables : RuleVisitor
		{
			public override string Message => "Do not alter temp tables after they have been created.";

			public override string[] BaseLine => new string[] {
				"ProfitAndLossReportPeriodAnalysis",
				"ProfitAndLossReportPeriodAnalysisReportingBook",
			};

			public override void Visit(AlterTableStatement node)
			{
				if (node.SchemaObjectName.BaseIdentifier.Value.StartsWith("#"))
				{
					Objects.Add(new NameAndLocation { Name = node.SchemaObjectName.BaseIdentifier.Value, Line = node.StartLine });
				}
				base.Visit(node);
			}
		}

		class IndexOnTemporaryTablesShouldUsingInlineSyntax : RuleVisitor
		{
			public override string Message => "Move index creation statements on temp tables to the new inline index creation syntax that was introduced in SQL Server 2014.";

			public override void Visit(CreateIndexStatement node)
			{
				if (node.OnName.BaseIdentifier.Value.StartsWith("#"))
				{
					Objects.Add(new NameAndLocation { Name = node.Name.Value, Line = node.StartLine });
				}
				base.Visit(node);
			}
		}
	}
}

