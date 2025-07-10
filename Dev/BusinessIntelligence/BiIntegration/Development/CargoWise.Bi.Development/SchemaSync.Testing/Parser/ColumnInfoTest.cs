using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Bi.Development.SchemaSync.Parser;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;

namespace CargoWise.Bi.Development.SchemaSync.Testing.Parser;

public class ColumnInfoTest : TestCase
{
	public void TestComputedTypesAreIgnored()
	{
		var columnInfo = GetColumnInfoFromDefinition("a AS 1");

		AssertEquals("Computed columns should be ignored.", null, columnInfo);
	}

	public void TestBigNVarCharIsIgnored()
	{
		var columnInfo = GetColumnInfoFromDefinition("a nvarchar(2049)");

		AssertEquals("Big nvarchar columns should be ignored.", null, columnInfo);
	}

	public void TestBigVarCharIsIgnored()
	{
		var columnInfo = GetColumnInfoFromDefinition("a varchar(4097)");

		AssertEquals("Big varchar columns should be ignored.", null, columnInfo);
	}

	public void TestColumnIsPKIfInPrimaryKeyList()
	{
		var columnInfo = GetColumnInfoFromDefinition("a int", new List<string> { "a" });

		AssertEquals("Column should be a primary key.", true, columnInfo.IsPrimaryKey);
	}

	public void TestColumnIsPKNotInPrimaryKeyList()
	{
		var columnInfo = GetColumnInfoFromDefinition("a int");

		AssertEquals("Column should not be a primary key.", false, columnInfo.IsPrimaryKey);
	}

	public void TestIsPKWithInlinePKConstraint()
	{
		var columnInfo = GetColumnInfoFromDefinition("col iNT PRIMARY KEY");

		AssertEquals(true, columnInfo.IsPrimaryKey);
	}

	// why is this even a thing??
	public void TestIsPKWithInlineConstraintReferencingOtherColumns()
	{
		var columnInfo = GetColumnInfoFromDefinition("Col INT CONSTRAINT pk_constraint PRIMARY KEY (OtherCol)");

		AssertEquals(false, columnInfo.IsPrimaryKey);
	}

	public void TestIsNullableTrueExplicit()
	{
		var columnInfo = GetColumnInfoFromDefinition("COL INT NULL");

		AssertEquals(true, columnInfo.Nullable);
	}

	public void TestIsNullableFalseExplicit()
	{
		var columnInfo = GetColumnInfoFromDefinition("COL INT NOT NULL");

		AssertEquals(false, columnInfo.Nullable);
	}

	public void TestIsNullableByDefault()
	{
		var columnInfo = GetColumnInfoFromDefinition("COL INT");

		AssertEquals(true, columnInfo.Nullable);
	}

	ColumnInfo GetColumnInfoFromDefinition(string columnDefinition, List<string> pkColumns = null)
	{
		var createTableScript = $@"
		CREATE TABLE DummyTable (
			{columnDefinition}
		);";

		var parser = new TSql160Parser(true);
		var statementList = parser.ParseStatementList(
			new StringReader(createTableScript),
			out var errors);

		AssertEquals("There should be no parse errors.", errors.Count, 0);

		var createTableStatement = statementList.Statements.Single() as CreateTableStatement;
		var column = createTableStatement.Definition.ColumnDefinitions.Single();

		if (pkColumns == null)
		{
			pkColumns = new List<string>();
		}

		return ColumnInfo.Create(column, pkColumns);
	}
}
