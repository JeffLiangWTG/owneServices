using System.IO;
using System.Linq;
using CargoWise.Bi.Development.SchemaSync.Parser;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;

namespace CargoWise.Bi.Development.SchemaSync.Testing.Parser;

public class TableInfoTest : TestCase
{
	public void TestDefaultSchemaIsDbo()
	{
		var tableInfo = GetTableInfoFromBody("col int");

		AssertEquals("dbo", tableInfo.SchemaName);
	}

	public void TestSpecifiedSchema()
	{
		var tableInfo = GetTableInfoFromDefinition(@"
			CREATE TABLE Car.TableWithSchema (
				COL INT
			);
		");

		AssertEquals("Car", tableInfo.SchemaName);
	}

	public void TestTableName()
	{
		var tableInfo = GetTableInfoFromDefinition(@"
			CREATE TABLE TestTableName (
				COL INT
			);
		");

		AssertEquals("TestTableName", tableInfo.TableName);
	}

	public void TestFirstColumnInPKIsPrimaryKey()
	{
		var tableInfo = GetTableInfoFromBody(@"
			COL1 INT,
			COL2 INT,
			CONSTRAINT PK_Constraint PRIMARY KEY (COL2, COL1)");

		AssertEquals("COL1 should be a primary key.", true, tableInfo.Columns[0].IsPrimaryKey);
	}

	public void TestFirstColumnInPKIsPrimaryKeyWithDifferentOrder()
	{
		var tableInfo = GetTableInfoFromBody(@"
			COL2 INT,
			COL1 INT,
			CONSTRAINT PK_Constraint PRIMARY KEY (COL2, COL1)");

		AssertEquals("COL1 should be a primary key.", true, tableInfo.Columns[0].IsPrimaryKey);
	}

	public void TestColumnWithPKConstraintReferencingOtherColumnIsNotPK()
	{
		var tableInfo = GetTableInfoFromBody("COL1 INT CONSTRAINT PK_Constraint PRIMARY KEY (COL2)");

		AssertEquals("COL1 should not be a primary key.", false, tableInfo.Columns[0].IsPrimaryKey);
	}

	TableInfo GetTableInfoFromBody(string tableBody)
	{
		var createTableScript = $@"
		CREATE TABLE DummyTable (
			{tableBody}
		);";

		return GetTableInfoFromDefinition(createTableScript);
	}

	TableInfo GetTableInfoFromDefinition(string tableDefinition)
	{
		var parser = new TSql160Parser(true);
		var statementList = parser.ParseStatementList(
			new StringReader(tableDefinition),
			out var errors);

		AssertEquals("There should be no parse errors.", errors.Count, 0);

		var createTableStatement = statementList.Statements.Single() as CreateTableStatement;

		return TableInfo.Create(createTableStatement);
	}
}
