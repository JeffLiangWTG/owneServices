using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data.Diagnostics;
using CargoWise.Database.Abstractions;
using CargoWise.Database.Shared;
using CargoWise.Schema;
using Moq;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class DbCommandTransactionalTest : TransactionedTestCase
	{
		protected override DbConnection TestConnection
		{
			get { return testConnection; }
		}
		readonly DbConnection testConnection = Db.Connection;

		public void TestParameterBasedOnDbColumnDoesNotAppendSpacesToCharValues()
		{
			// Create TestTable Code column max length must be greater than 3 char (> len'~X%')
			int codeColSize = 5;
			string testTable = "Test_AFF613A789C14D06B3B8C9D5C24F52BD";
			string sqlText = string.Format(@"
				CREATE TABLE {0} (
					ColCode varchar({1}) not null,
					ColDesc varchar(35) not null
				)
				INSERT {0} VALUES ('~XSYD', 'Sydney')
				INSERT {0} VALUES ('~XMEL', 'Melbourne')
				INSERT {0} VALUES ('~YSSA', 'Salvador')",
				testTable, codeColSize.ToString());
			TestConnection.ExecuteNonQuery(sqlText);

			string selectSql = string.Format("SELECT TOP 1 ColCode FROM {0} WHERE ColCode LIKE @CodePattern", testTable);
			string likeString = "~X%";

			using (DbCommand testControlCmd = TestConnection.Command(selectSql))
			{
				testControlCmd.AddParameter("@CodePattern", SqlDbType.Char, codeColSize, likeString);
				object testControlOutput = testControlCmd.ExecuteScalar();
				AssertNull("Control command should return no rows", testControlOutput);
			}

			using (DbCommand testCmd = TestConnection.Command(selectSql))
			{
				var varchar5ColSchema = new SchemaStringColumn(CargoWise.Schema.Schema.GenericTableSchema, "Column", 0, SqlDbType.VarChar, string.Empty, false, 5);
				testCmd.AddParameterBasedOnDbColumn("@CodePattern", likeString, varchar5ColSchema);
				string testOutput = testCmd.ExecuteScalar().ToString();
				AssertEquals("Result should be a valid code", likeString.Substring(0, 2), testOutput.Substring(0, 2));
			}
		}

		public void TestInsertCommandTruncatesCharParameterBiggerThanMaximum()
		{
			int codeColSize = 5;
			int descColSize = 35;
			string testTable = "Test_D02DD9E8D75E44FD86718DF03A526116";
			string sqlText = string.Format(@"
				CREATE TABLE {0} (
					ColCode varchar({1}) not null,
					ColDesc varchar({2}) not null
				)
				INSERT {0} VALUES ('AUSYD', 'Sydney')
				INSERT {0} VALUES ('AUMEL', 'Melbourne')
				INSERT {0} VALUES ('BRSSA', 'Salvador')",
				testTable, codeColSize.ToString(), descColSize.ToString());
			TestConnection.ExecuteNonQuery(sqlText);

			// Code and Description used to insert should be longer than the actual column max lengths
			string codeString = new string('@', codeColSize + 10);
			string descriptionString = new string('@', descColSize + 20);
			string insertSql = string.Format("INSERT {0} VALUES (@Code, @Description)", testTable);

			using (DbCommand testControlCmd = TestConnection.Command(insertSql))
			{
				testControlCmd.AddParameter("@Code", SqlDbType.Char, codeString);
				testControlCmd.AddParameter("@Description", SqlDbType.VarChar, 1000, descriptionString);

				try
				{
					testControlCmd.ExecuteNonQuery();
					Fail("Insert Command should have failed");
				}
				catch (SqlException e)
				{
					var actualError = new DbErrorMatch(e);
					AssertEquals("Wrong exception was caught", actualError.ExceptionType, DbErrorType.StringOrBinaryDataWouldBeTruncated);
				}
			}

			var varchar5Column = new SchemaStringColumn(CargoWise.Schema.Schema.GenericTableSchema, "Column", 0, SqlDbType.VarChar, string.Empty, false, 5);
			var varchar35Column = new SchemaStringColumn(CargoWise.Schema.Schema.GenericTableSchema, "Column", 0, SqlDbType.VarChar, string.Empty, false, 35);
			using (DbCommand testCmd = TestConnection.Command(insertSql))
			{
				testCmd.AddParameterBasedOnDbColumn("@Code", codeString, varchar5Column);
				testCmd.AddParameterBasedOnDbColumn("@Description", descriptionString, varchar35Column);
				testCmd.ExecuteNonQuery();
			}

			string selectSql = string.Format("SELECT count(*) FROM {0} WHERE ColCode = @Code", testTable);
			using (DbCommand checkCommand = TestConnection.Command(selectSql))
			{
				checkCommand.AddParameterBasedOnDbColumn("@Code", codeString, varchar5Column);
				int result = Convert.ToInt32(checkCommand.ExecuteScalar());
				AssertEquals("Row should have been inserted", 1, result);
			}
		}

		public void TestParameterBasedOnDecimalDbColumn()
		{
			string testTable = "Test_509FDCC004BF4138B138D4943075E124";
			string sqlText = string.Format(@"
				CREATE TABLE {0} (
					ColCode varchar(2) not null,
					ColDecimal decimal(6, 2) not null
				)
				INSERT {0} VALUES ('C1', 1234.56)
				INSERT {0} VALUES ('C2', 12.34)
				INSERT {0} VALUES ('C3', 1.23)",
				testTable);
			TestConnection.ExecuteNonQuery(sqlText);

			string selectSql = string.Format("SELECT TOP 1 ColCode FROM {0} WHERE ColDecimal = @DecimalParam", testTable);

			using (DbCommand testControlCmd = TestConnection.Command(selectSql))
			{
				testControlCmd.AddParameter("@DecimalParam", SqlDbType.Decimal, 0, 1.234D);
				object testControlOutput = testControlCmd.ExecuteScalar();
				AssertNull("Control command should return no rows", testControlOutput);
			}

			using (DbCommand testCmd = TestConnection.Command(selectSql))
			{
				var decimalColSchema = new SchemaDecimalColumn(CargoWise.Schema.Schema.GenericTableSchema, "Column", 0, SqlDbType.Decimal, 0, false, 6, 2);
				testCmd.AddParameterBasedOnDbColumn("@DecimalParam", 1.234D, decimalColSchema);
				object testOutputObj = testCmd.ExecuteScalar();
				AssertNotNull("Should return a value", testOutputObj);
				string testOutput = testOutputObj.ToString();
				AssertEquals("Result code", "C3", testOutput);
			}

			using (DbCommand testInterfaceCmd = TestConnection.Command(selectSql))
			{
				ISchemaColumn decimalColInterfaceSchema = new SchemaDecimalColumn(CargoWise.Schema.Schema.GenericTableSchema, "Column", 0, SqlDbType.Decimal, 0, false, 6, 2);
				testInterfaceCmd.AddParameterBasedOnDbColumn("@DecimalParam", 1.234D, decimalColInterfaceSchema);
				object testInterfaceOutputObj = testInterfaceCmd.ExecuteScalar();
				AssertNotNull("Should return a value (Interface)", testInterfaceOutputObj);
				string testInterfaceOutput = testInterfaceOutputObj.ToString();
				AssertEquals("Result code (Interface)", "C3", testInterfaceOutput);
			}
		}

		public void TestExecuteProcedureWithReturnValue()
		{
			string sqlText = "CREATE PROCEDURE TestProcedure1 AS SELECT TOP 1 * FROM dbo.RefCountry";
			Db.Connection.ExecuteNonQuery(sqlText);
			sqlText = "CREATE PROCEDURE TestProcedure2 AS SELECT TOP 1 * FROM dbo.RefCountry RETURN -2";
			Db.Connection.ExecuteNonQuery(sqlText);
			sqlText = "CREATE PROCEDURE TestProcedure3 AS SELECT TOP 1 * FROM dbo.RefCountry RETURN";
			Db.Connection.ExecuteNonQuery(sqlText);
			sqlText = "CREATE PROCEDURE TestProcedure4 AS RETURN 4";
			Db.Connection.ExecuteNonQuery(sqlText);

			DbCommand procedureCommand = Db.Connection.Command("");
			procedureCommand.CommandType = CommandType.StoredProcedure;
			int returnValue = 999;

			procedureCommand.CommandText = "TestProcedure1";
			returnValue = procedureCommand.ExecuteProcedureWithReturnValue();
			AssertEquals("Return Value if no RETURN statement", 0, returnValue);

			procedureCommand.CommandText = "TestProcedure2";
			returnValue = procedureCommand.ExecuteProcedureWithReturnValue();
			AssertEquals("Explicitly returned ReturnValue and ResultSet", -2, returnValue);

			procedureCommand.CommandText = "TestProcedure3";
			returnValue = procedureCommand.ExecuteProcedureWithReturnValue();
			AssertEquals("Return Value if no value is explicitly returned", 0, returnValue);

			procedureCommand.CommandText = "TestProcedure4";
			returnValue = procedureCommand.ExecuteProcedureWithReturnValue();
			AssertEquals("Explicitly returned ReturnValue only", 4, returnValue);

			procedureCommand.CommandText = "TestProcedure2";
			procedureCommand.CommandType = CommandType.Text;
			returnValue = procedureCommand.ExecuteProcedureWithReturnValue();
			AssertEquals("Command type not initially declared as StoredProcedure", -2, returnValue);
		}

		#region TVP

		public void TestAddTableValuedParameter()
		{
			var serviceProvider = new Mock<IServiceProvider>();
			var currentServiceProvider = GlobalServiceProvider.Instance;
			serviceProvider.Setup(x => x.GetService(It.IsAny<Type>())).Returns<Type>(t => currentServiceProvider.GetService(t));
			serviceProvider.Setup(x => x.GetService(typeof(IDbValueConversion))).Returns(SomeCustomValueConversion.Instance);

			using (GlobalServiceProvider.Configure(serviceProvider.Object))
			{
				var column = new SchemaStringColumn(CargoWise.Schema.Schema.GenericTableSchema, "Column", 0, SqlDbType.VarChar, string.Empty, false, 5);
				var table = "Test_Table";
				var sql = string.Format(@"
CREATE TABLE dbo.{0}
(
	PK      uniqueidentifier NOT NULL,
	ColCode varchar({1})     NOT NULL,
	ColDesc varchar(35)      NOT NULL,
);

INSERT dbo.{0} (PK, ColCode, ColDesc) VALUES
-- (PK   , ColCode, ColDesc    )
	('{2}', 'AUSYD', 'Sydney'   ),
	('{2}', 'AUMEL', 'Melbourne'),
	(NEWID(), 'UAIEV', 'Kiev'     )
"
					, table                    // 0
					, column.MaxLength         // 1
					, Guid.Empty.ToString()   // 2
					);

				TestConnection.ExecuteNonQuery(sql);

				sql = string.Format("SELECT ColDesc FROM dbo.[{0}] WHERE ColCode in (SELECT Value FROM @Codes) AND PK in (SELECT Value FROM @PKs);", table);
				using (var cmd = TestConnection.Command(sql))
				{
					cmd.AddTableValuedParameter("@Codes", column, new object[] { "UAIEV", "AUSYD", "123456789" });
					cmd.AddTableValuedParameter("@PKs", column, new object[] { new SomeCustomValueType() });

					var actual = new List<string>(2);
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							actual.Add(reader.GetString(0));
						}
					}

					AssertContainsExactElementsInAnyOrder(new string[] { "Sydney" }, actual);
				}
			}
		}

		public void TestAddTableValuedParameter_WithLatinCharacters()
		{
			var column = new SchemaStringColumn(CargoWise.Schema.Schema.GenericTableSchema, "Column", 0, SqlDbType.NVarChar, null, true, 100);
			var table = "Test_Table";
			var sql = FormattableString.Invariant($@"
CREATE TABLE dbo.{table}
(
	ColDesc varchar(50)      NOT NULL,
);

INSERT dbo.{table} ( ColDesc) VALUES
-- (ColDesc)
	('Arla Foods AmBa Lillebaelt Mejeri - BIHOG'),
	('Arla Foods AmBa Lillebælt Mejeri - BIHOG')
"
				);

			using (var cmd = TestConnection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}

			sql = string.Format("SELECT ColDesc FROM dbo.{0} WHERE ColDesc in (SELECT Value FROM @Desc);", table);
			using (var cmd = TestConnection.Command(sql))
			{
				cmd.AddTableValuedParameter("@Desc", column, new object[] { "Arla Foods AmBa Lillebaelt Mejeri - BIHOG", "Arla Foods AmBa Lillebælt Mejeri - BIHOG" });

				var actual = new List<string>(2);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						actual.Add(reader.GetString(0));
					}
				}

				AssertContainsExactElementsInAnyOrder(new string[] { "Arla Foods AmBa Lillebaelt Mejeri - BIHOG", "Arla Foods AmBa Lillebælt Mejeri - BIHOG" }, actual);
			}
		}

		public void TestAddTableValuedParameter_WithExplicitTypeNameAndDataTable()
		{
			var dataTable = new DataTable();
			dataTable.Columns.Add("Value", typeof(string));
			dataTable.Rows.Add(new object[] { "Sydney" });
			dataTable.Rows.Add(new object[] { "Chisinau" });

			using (var command = TestConnection.Command("SELECT Value FROM @table"))
			{
				command.AddTableValuedParameter("table", TVPHelper.TVP_varchar, dataTable);

				var actual = new List<string>(2);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						actual.Add(reader.GetString(0));
					}
				}

				AssertContainsExactElementsInAnyOrder(new string[] { "Sydney", "Chisinau" }, actual);
			}
		}

		public void TestAddTableValuedParameter_Exceptions()
		{
			var errorMessage = "SchemaColumn should support Table Valued Parameter.";

			using (var cmd = TestConnection.Command(""))
			{
				var column = new SchemaStringColumn(CargoWise.Schema.Schema.GenericTableSchema, "Column", 0, SqlDbType.VarChar, string.Empty, false, -1);
				AssertExceptionThrown(typeof(InvalidOperationException), errorMessage, () =>
				{
					cmd.AddTableValuedParameter("@t", column, new string[] { "UAIEV", "AUSYD" });
				});
			}
		}

		public void TestAddTableValuedParameter_WithStringParameterTypeName()
		{
			var sql = @"SELECT Value FROM @TVPParams";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddTableValuedParameter("@TVPParams", "dbo.TVP_char_3", new[] { "AAA", "BBB" });

				var actual = new List<string>(2);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						actual.Add(reader.GetString(0));
					}
				}

				AssertContainsExactElementsInAnyOrder(new string[] { "AAA", "BBB" }, actual);
			}
		}

		#endregion // TVP

		#region Excute Command With Enabled StackTraceRecorder

		[ExpectNoExceptions]
		public void TestExecuteCommandWithEnabledStackTraceRecorder_ExecuteProcedure()
		{
			try
			{
				QueryStackTraceRecorderCore.InstanceCore.Enabled = true;

				string sqlText = "CREATE PROCEDURE TestProcedure AS SELECT TOP 1 * FROM dbo.RefCountry";
				Db.Connection.ExecuteNonQuery(sqlText);

				DbCommand cmd = Db.Connection.Command("TestProcedure");
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.ExecuteNonQuery();
			}
			finally
			{
				QueryStackTraceRecorderCore.InstanceCore.Enabled = false;
			}
		}

		/// <summary>
		/// Call Stack NOT inserted due to return value parameter
		/// </summary>
		[ExpectNoExceptions]
		public void TestExecuteCommandWithEnabledStackTraceRecorder_ExecuteProcedureWithReturnValue()
		{
			try
			{
				QueryStackTraceRecorderCore.InstanceCore.Enabled = true;
				TestExecuteProcedureWithReturnValue();
			}
			finally
			{
				QueryStackTraceRecorderCore.InstanceCore.Enabled = false;
			}
		}

		/// <summary>
		/// Call Stack NOT inserted due to output parameter
		/// </summary>
		public void TestExecuteCommandWithEnabledStackTraceRecorder_ExecuteProcedureWithOutputParameter()
		{
			try
			{
				QueryStackTraceRecorderCore.InstanceCore.Enabled = true;

				string sqlText = "CREATE PROCEDURE TestProcedure @OutParam varchar(20) output AS SET @OutParam = 'ReturnedValue'";
				Db.Connection.ExecuteNonQuery(sqlText);

				DbCommand cmd = Db.Connection.Command("TestProcedure");
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddOutputParameter("@OutParam", SqlDbType.VarChar, 20, 0, 0, "PassedValue");
				cmd.ExecuteNonQuery();
				AssertEquals("Output Parameter Value", "ReturnedValue", cmd.GetParameterValue("@OutParam").ToString());
			}
			finally
			{
				QueryStackTraceRecorderCore.InstanceCore.Enabled = false;
			}
		}

		[ExpectNoExceptions]
		public void TestExecuteCommandWithEnabledStackTraceRecorder_ExecuteNonQuery()
		{
			try
			{
				QueryStackTraceRecorderCore.InstanceCore.Enabled = true;
				DbCommand cmd = Db.Connection.Command("SELECT null FROM dbo.OrgHeader");
				cmd.ExecuteNonQuery();
			}
			finally
			{
				QueryStackTraceRecorderCore.InstanceCore.Enabled = false;
			}
		}

		public void TestExecuteCommandWithEnabledStackTraceRecorder_ExecuteScalar()
		{
			try
			{
				QueryStackTraceRecorderCore.InstanceCore.Enabled = true;
				DbCommand cmd = Db.Connection.Command("SELECT TOP 1 OH_PK FROM dbo.OrgHeader");
				Guid pk = (Guid)cmd.ExecuteScalar();
				AssertNotNull(pk);
				AssertNotEquals(Guid.Empty, pk);
			}
			finally
			{
				QueryStackTraceRecorderCore.InstanceCore.Enabled = false;
			}
		}

		public void TestExecuteCommandWithEnabledStackTraceRecorder_ExecuteReader()
		{
			try
			{
				QueryStackTraceRecorderCore.InstanceCore.Enabled = true;

				DbCommand cmd = Db.Connection.Command("SELECT null FROM dbo.OrgHeader");

				using (var reader = cmd.ExecuteReader())
				{
					Assert("Data Reader should have rows", reader.Read());
					Assert("Data Reader should have columns", reader.FieldCount > 0);
				}
			}
			finally
			{
				QueryStackTraceRecorderCore.InstanceCore.Enabled = false;
			}
		}

		#endregion

		#region Custom Value Conversion Helpers

		sealed class SomeCustomValueType
		{
		}

		sealed class SomeCustomValueTypeConverter : TypeConverter
		{
			public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
			{
				if (value is SomeCustomValueType)
				{
					return Guid.Empty;
				}

				return base.ConvertTo(context, culture, value, destinationType);
			}

			public static TypeConverter Instance { get; } = new SomeCustomValueTypeConverter();
		}

		sealed class SomeCustomValueConversion : IDbValueConversion
		{
			public static SomeCustomValueConversion Instance { get; } = new SomeCustomValueConversion();

			public TypeConverter TryGetConverterForValues(IEnumerable values) => values.OfType<SomeCustomValueType>().Any() ? SomeCustomValueTypeConverter.Instance : null;

			public bool TryUnwrapSimpleValue(object value, out object unwrapped)
			{
				unwrapped = null;
				return false;
			}
		}

		#endregion
	}
}
