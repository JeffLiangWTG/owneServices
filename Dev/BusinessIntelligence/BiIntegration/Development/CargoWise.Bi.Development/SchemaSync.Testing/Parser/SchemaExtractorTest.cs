using System.IO;
using System.Linq;
using CargoWise.Bi.Development.SchemaSync.DataSets;
using CargoWise.Bi.Development.SchemaSync.Parser;
using NUnit.Framework;

namespace CargoWise.Bi.Development.SchemaSync.Testing.Parser;

public class SchemaExtractorTest : TestCase
{
	public void TestExtractingOneColumnWorksCorrectly()
	{
		var createTableStatement = @"
			CREATE TABLE [abc].[TestTable] (
				[Id] INT NOT NULL PRIMARY KEY
			);";

		var dataSet = ExtractDatasetFromScript(createTableStatement);

		var table = dataSet.Definition;

		AssertEquals(1, table.Rows.Count);
		var row = table[0];

		CombineAssertions(() =>
		{
			AssertEquals("abc", row.SourceSchema);
			AssertEquals("TestTable", row.SourceTable);

			AssertEquals("Id", row.SourceColumn);
			AssertEquals("int", row.DataType);
			AssertEquals((short)4, row.MaxLength);
			AssertEquals((short)10, row.Precision);
			AssertEquals((byte)0, row.Scale);
			AssertEquals(false, row.Nullable);
			AssertEquals(true, row.IsPrimaryKey);

			AssertEquals(null, row.ReferenceSchema);
			AssertEquals(null, row.ReferenceTable);
		});
	}

	public void TestInlineWithColumnForeignKeyMatchesReferencedTable()
	{
		var script = @"
			CREATE TABLE [abc].[TestTable] (
				[TT_Id] INT NOT NULL PRIMARY KEY,
				[TT_ForeignKeyColumn] INT NOT NULL FOREIGN KEY REFERENCES [xyz].[ReferencedTable]([RT_Id])
			);

			CREATE TABLE [xyz].[ReferencedTable] (
				[RT_Id] INT NOT NULL PRIMARY KEY
			);";

		var dataSet = ExtractDatasetFromScript(script);

		var table = dataSet.Definition;

		var row = table.Single(table => table.SourceTable == "TestTable" && table.SourceColumn == "TT_ForeignKeyColumn");

		CombineAssertions(() =>
		{
			AssertEquals("xyz", row.ReferenceSchema);
			AssertEquals("ReferencedTable", row.ReferenceTable);
		});
	}

	public void TestInlineWithTableForeignKeyMatchesReferencedTable()
	{
		var script = @"
			CREATE TABLE [abc].[TestTable] (
				[TT_Id] INT NOT NULL PRIMARY KEY,
				[TT_ForeignKeyColumn] INT NOT NULL,
				FOREIGN KEY (TT_ForeignKeyColumn) REFERENCES [xyz].[ReferencedTable] ([RT_Id])
			);

			CREATE TABLE [xyz].[ReferencedTable] (
				[RT_Id] INT NOT NULL PRIMARY KEY
			);";

		var dataSet = ExtractDatasetFromScript(script);

		var table = dataSet.Definition;

		var row = table.Single(table => table.SourceTable == "TestTable" && table.SourceColumn == "TT_ForeignKeyColumn");

		CombineAssertions(() =>
		{
			AssertEquals("xyz", row.ReferenceSchema);
			AssertEquals("ReferencedTable", row.ReferenceTable);
		});
	}

	public void TestAlterTableForeignKeyMatchesReferencedTableWithSchema()
	{
		var script = @"
			CREATE TABLE [abc].[TestTable] (
				[TT_Id] INT NOT NULL PRIMARY KEY,
				[TT_ReferenceId] INT NULL,
			);

			CREATE TABLE [xyz].[ReferencedTable] (
				[RT_Id] INT NOT NULL PRIMARY KEY
			);

			ALTER TABLE [abc].[TestTable]
				ADD CONSTRAINT ForeignKeyConstraint FOREIGN KEY (TT_ReferenceId) REFERENCES xyz.ReferencedTable (RT_Id)";

		var dataSet = ExtractDatasetFromScript(script);

		var table = dataSet.Definition;

		var row = table.Single(table => table.SourceTable == "TestTable" && table.SourceColumn == "TT_ReferenceId");

		CombineAssertions(() =>
		{
			AssertEquals("xyz", row.ReferenceSchema);
			AssertEquals("ReferencedTable", row.ReferenceTable);
		});
	}

	public void TestExtractSchemaWorksWithMultipleScripts()
	{
		var table1 = @"
			CREATE TABLE [abc].[TestTable1] (
				[Id1] INT NOT NULL PRIMARY KEY
			);";

		var table2 = @"
			CREATE TABLE [abc].[TestTable2] (
				[Id2] INT NOT NULL PRIMARY KEY
			);";

		var table3 = @"
			CREATE TABLE [abc].[TestTable3] (
				[Id3] INT NOT NULL PRIMARY KEY
			);";

		var dataSet = new SchemaDataSet();

		var schemaExtractor = new SchemaExtractor(new[] { new StringReader(table1), new StringReader(table2), new StringReader(table3) });
		schemaExtractor.ExtractSchema(dataSet);

		AssertEquals(3, dataSet.Definition.Rows.Count);
	}

	public void TestPrimaryKeyIsDetectedFromAlterTableStatement()
	{
		var script = @"
			CREATE TABLE TestTable (
				COL INT
			);

			ALTER TABLE TestTable
				ADD CONSTRAINT PK_Constraint PRIMARY KEY (COL)";

		var dataSet = ExtractDatasetFromScript(script);

		var table = dataSet.Definition;

		AssertEquals(1, table.Rows.Count);
		var row = table[0];

		CombineAssertions(() =>
		{
			AssertEquals("dbo", row.SourceSchema);
			AssertEquals("TestTable", row.SourceTable);
			AssertEquals("COL", row.SourceColumn);

			AssertEquals(true, row.IsPrimaryKey);
		});
	}

	public void TestNonPrimaryKeyColumn()
	{
		var createTableScript = @"
			CREATE TABLE DummyTable (
				STR_Column VARCHAR
			);";

		var dataSet = ExtractDatasetFromScript(createTableScript);

		var table = dataSet.Definition;

		AssertEquals(1, table.Rows.Count);
		var row = table[0];

		CombineAssertions(() =>
		{
			AssertEquals("dbo", row.SourceSchema);
			AssertEquals("DummyTable", row.SourceTable);
			AssertEquals("STR_Column", row.SourceColumn);

			AssertEquals(false, row.IsPrimaryKey);
		});
	}

	SchemaDataSet ExtractDatasetFromScript(string sqlScript)
	{
		var dataSet = new SchemaDataSet();
		var schemaExtractor = new SchemaExtractor(new[] { new StringReader(sqlScript) });

		schemaExtractor.ExtractSchema(dataSet);

		return dataSet;
	}
}
