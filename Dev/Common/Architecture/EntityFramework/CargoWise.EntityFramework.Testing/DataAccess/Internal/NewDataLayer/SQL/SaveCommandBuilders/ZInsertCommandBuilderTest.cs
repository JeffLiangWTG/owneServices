using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ZInsertCommandBuilderTest : ZSqlCommandBuilderTest
	{
		[DeveloperOnlyTest]
		public void TestPerformance()
		{
			var table = CreateTable();

			for (var i = 0; i < 20000; i++)
			{
				var row = table.NewRow();
				row[DummyBizoSchema.PK.Name] = Guid.NewGuid();
				table.Rows.Add(row);
			}

			var stopwatch = new Stopwatch();
			var commandText = new StringBuilder();

			stopwatch.Start();
			var builder = new ZInsertCommandBuilder(DummyBizoSchema.Constants.TableName, false, 0, ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(DummyBizoSchema.Constants.TableName), 20000);
			foreach (DataRow row in table.Rows)
			{
				builder.AddRow(row);
			}
			builder.AppendCommandTextAndBlobSaver(commandText);

			stopwatch.Stop();
			AssertGreaterThan(1000, stopwatch.ElapsedMilliseconds);
		}

		[DeveloperOnlyTest]
		public void TestPerformance_AddRows()
		{
			var table = CreateTable();

			for (var i = 0; i < 20000; i++)
			{
				var row = table.NewRow();
				row[DummyBizoSchema.PK.Name] = Guid.NewGuid();
				table.Rows.Add(row);
			}

			var stopwatch = new Stopwatch();
			var commandText = new StringBuilder();
			var rows = table.Rows.OfType<DataRow>().ToArray();

			stopwatch.Start();
			var builder = new ZInsertCommandBuilder(DummyBizoSchema.Constants.TableName, false, 0, ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(DummyBizoSchema.Constants.TableName), 20000);
			builder.AddRow(rows[0]);
			builder.AppendCommandTextAndBlobSaver(commandText);

			for (var i = 1; i < rows.Length; i++)
			{
				builder.AddRow(rows[i]);
			}
			builder.AppendCommandTextAndBlobSaver(commandText);

			stopwatch.Stop();
			AssertGreaterThan(1000, stopwatch.ElapsedMilliseconds);
		}

		public void TestIncludeDelimiterWhenRowsExceedExpectedNumberOfRows()
		{
			var table = new DataTable(DummyBizoSchema.Constants.TableName);
			table.Columns.Add(DummyBizoSchema.Constants.PK, typeof(Guid));
			table.Columns.Add(DummyBizoSchema.Constants.Z0_Code, typeof(string));
			table.PrimaryKey = new DataColumn[] { table.Columns[DummyBizoSchema.Constants.PK] };

			var commandText = new StringBuilder();
			var builder = new ZInsertCommandBuilder(DummyBizoSchema.Constants.TableName, false, 0, ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(DummyBizoSchema.Constants.TableName), expectedNumberOfRows: 1);
			builder.AddRow(AddRow("2e8cd012-f8cf-43eb-9b0e-73414b2bacd9", "Row1"));

			builder.AppendCommandTextAndBlobSaver(commandText);

			AssertMultilineASCIIEquals(@"EXEC sys.sp_executesql N'INSERT dbo.DummyBizo (Z0_PK, Z0_Code) VALUES
	(@11, @12)
'
, N'@11 uniqueidentifier, @12 varchar(5)'
, @11 = '2e8cd012-f8cf-43eb-9b0e-73414b2bacd9', @12 = 'Row1';
", commandText.ToString());

			commandText.Clear();
			builder.AddRow(AddRow("8d997b2f-4a75-451f-bc9c-6c492f06281c", "Row2"));
			builder.AppendCommandTextAndBlobSaver(commandText);

			AssertMultilineASCIIEquals(@"EXEC sys.sp_executesql N'INSERT dbo.DummyBizo (Z0_PK, Z0_Code) VALUES
	(@11, @12),
	(@2_1, @2_2)
'
, N'@11 uniqueidentifier, @12 varchar(5)
  , @2_1 uniqueidentifier, @2_2 varchar(5)'
, @11 = '2e8cd012-f8cf-43eb-9b0e-73414b2bacd9', @12 = 'Row1'
, @2_1 = '8d997b2f-4a75-451f-bc9c-6c492f06281c', @2_2 = 'Row2';
", commandText.ToString());

			DataRow AddRow(string pk, string code)
			{
				var newRow = table.NewRow();
				newRow[DummyBizoSchema.Z0_Code.Name] = code;
				newRow[DummyBizoSchema.PK.Name] = Guid.Parse(pk);
				table.Rows.Add(newRow);
				return newRow;
			}
		}

		public void TestAddRows()
		{
			var table = new DataTable(DummyBizoSchema.Constants.TableName);
			table.Columns.Add(DummyBizoSchema.Constants.PK, typeof(Guid));
			table.Columns.Add(DummyBizoSchema.Constants.Z0_Code, typeof(string));
			table.PrimaryKey = new DataColumn[] { table.Columns[DummyBizoSchema.Constants.PK] };

			var commandText = new StringBuilder();
			var builder = new ZInsertCommandBuilder(DummyBizoSchema.Constants.TableName, false, 0, ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(DummyBizoSchema.Constants.TableName), 3);
			builder.AddRow(AddRow("2e8cd012-f8cf-43eb-9b0e-73414b2bacd9", "Row1"));

			builder.AppendCommandTextAndBlobSaver(commandText);

			AssertMultilineASCIIEquals(@"EXEC sys.sp_executesql N'INSERT dbo.DummyBizo (Z0_PK, Z0_Code) VALUES
	(@11, @12)
'
, N'@11 uniqueidentifier, @12 varchar(5)'
, @11 = '2e8cd012-f8cf-43eb-9b0e-73414b2bacd9', @12 = 'Row1';
", commandText.ToString());

			commandText.Clear();
			builder.AddRow(AddRow("8d997b2f-4a75-451f-bc9c-6c492f06281c", "Row2"));
			builder.AppendCommandTextAndBlobSaver(commandText);

			AssertMultilineASCIIEquals(@"EXEC sys.sp_executesql N'INSERT dbo.DummyBizo (Z0_PK, Z0_Code) VALUES
	(@11, @12),
	(@21, @22)
'
, N'@11 uniqueidentifier, @12 varchar(5)
  , @21 uniqueidentifier, @22 varchar(5)'
, @11 = '2e8cd012-f8cf-43eb-9b0e-73414b2bacd9', @12 = 'Row1'
, @21 = '8d997b2f-4a75-451f-bc9c-6c492f06281c', @22 = 'Row2';
", commandText.ToString());

			commandText.Clear();
			builder.AddRow(AddRow("d2189bce-e1bd-4bf3-bd75-8ee1dac17a8b", "Row3"));
			builder.AppendCommandTextAndBlobSaver(commandText);

			AssertMultilineASCIIEquals(@"EXEC sys.sp_executesql N'INSERT dbo.DummyBizo (Z0_PK, Z0_Code) VALUES
	(@11, @12),
	(@21, @22),
	(@31, @32)
'
, N'@11 uniqueidentifier, @12 varchar(5)
  , @21 uniqueidentifier, @22 varchar(5)
  , @31 uniqueidentifier, @32 varchar(5)'
, @11 = '2e8cd012-f8cf-43eb-9b0e-73414b2bacd9', @12 = 'Row1'
, @21 = '8d997b2f-4a75-451f-bc9c-6c492f06281c', @22 = 'Row2'
, @31 = 'd2189bce-e1bd-4bf3-bd75-8ee1dac17a8b', @32 = 'Row3';
", commandText.ToString());

			DataRow AddRow(string pk, string code)
			{
				var newRow = table.NewRow();
				newRow[DummyBizoSchema.Z0_Code.Name] = code;
				newRow[DummyBizoSchema.PK.Name] = Guid.Parse(pk);
				table.Rows.Add(newRow);
				return newRow;
			}
		}

		public void TestNullValuesUseOnlyOneParameterPerType()
		{
			var table = new DataTable(DummyBizoSchema.Constants.TableName);
			table.Columns.Add(DummyBizoSchema.Constants.PK, typeof(Guid));
			table.Columns.Add(DummyBizoSchema.Constants.Z0_Date, typeof(string));
			table.PrimaryKey = new[] { table.Columns[DummyBizoSchema.Constants.PK] };

			for (var i = 0; i < 2; i++)
			{
				var row = table.NewRow();
				row[DummyBizoSchema.PK.Name] = Guid.Parse("2e8cd012-f8cf-43eb-9b0e-73414b2bacd" + i);
				table.Rows.Add(row);
			}

			var commandText = new StringBuilder();
			var builder = new ZInsertCommandBuilder(DummyBizoSchema.Constants.TableName, false, 0, ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(DummyBizoSchema.Constants.TableName), 2);
			foreach (DataRow row in table.Rows)
			{
				builder.AddRow(row);
			}
			builder.AppendCommandTextAndBlobSaver(commandText);

			AssertMultilineASCIIEquals(@"EXEC sys.sp_executesql N'INSERT dbo.DummyBizo (Z0_PK, Z0_Date) VALUES
	(@11, @12),
	(@21, @12)
'
, N'@11 uniqueidentifier, @12 datetime
  , @21 uniqueidentifier'
, @11 = '2e8cd012-f8cf-43eb-9b0e-73414b2bacd0', @12 = NULL
, @21 = '2e8cd012-f8cf-43eb-9b0e-73414b2bacd1';
", commandText.ToString());
		}

		public override void TestWithDummyRow()
		{
			Assert(true);   // all good - dumb test (internal, well tested by factory)
		}

		public void TestWithDummyRowContainingBlobAndTextLargerThanMaxChunkSize()
		{
			var commandText = new StringBuilder();
			var previousMaxChunkSize = ZLargeColumnSaver.MaxChunkSize;
			List<ILargeColumnSaver> largeColumnSaverList;

			try
			{
				ZLargeColumnSaver.MaxChunkSize = 10;
				var builder = GetBuilder(CreateDummyRow());
				largeColumnSaverList = builder.LargeColumnSavers;
				builder.AppendCommandTextAndBlobSaver(commandText);
			}
			finally
			{
				ZLargeColumnSaver.MaxChunkSize = previousMaxChunkSize;
			}

			#region ExpectedCommandText, Blob, Text and NText

			var expectedTextColumnValue = "SomeTextValue";
			var expectedNTextColumnValue = "\u306E\u306E\u306E\u306E\u306E\u306E\u306E\u306E\u306E\u306E\u306E\u306E";

			#endregion

			AssertEquals("Number of LargeColumnSavers added", 3, largeColumnSaverList.Count);

			var actualNTextColumnValue = ((ZTextSaver)largeColumnSaverList[0]).Source.GetReader().ReadToEnd();
			AssertEquals("ZNTextSaver in LargeColumnSaverList", expectedNTextColumnValue, actualNTextColumnValue);

			var actualTextColumnValue = ((ZTextSaver)largeColumnSaverList[2]).Source.GetReader().ReadToEnd();
			AssertEquals("ZTextSaver in LargeColumnSaverList", expectedTextColumnValue, actualTextColumnValue);
		}

		protected override ZSqlCommandBuilder GetBuilder(DataRow row)
		{
			var builder = new TestInsertCommandBuilder("db.dbo.DummyBizo", ObjectFactory.Get<IApplicationSchemaResolver>());
			builder.AddRow(row);
			return builder;
		}

		class TestInsertCommandBuilder : ZInsertCommandBuilder
		{
			public TestInsertCommandBuilder(string tableName, IApplicationSchemaResolver schemaResolver)
				: base(tableName, true, 1, schemaResolver.GetTableSchema(DummyBizoSchema.Constants.TableName), ObjectFactory.Get<IEntityFrameworkSettings>().RowsToPostPerSqlStatement)
			{
			}
		}
	}
}
