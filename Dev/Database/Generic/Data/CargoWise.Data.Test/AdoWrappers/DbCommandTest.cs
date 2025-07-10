using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.ServiceModel;
using System.Threading;
using CargoWise.Common;
using CargoWise.DataProtection.TestFramework;
using CargoWise.Schema;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class DbCommandTest : TestCase
	{
		public void TestDataAdapterIsSqlDataAdapterWithTimeout()
		{
			var adapter1 = Db.Connection.Command("test").NewDataAdapter() as ReadOnlyDataAdapter;
			AssertEquals(false, adapter1.DbDataAdapter_Exposed is DbDataAdapterWithTimeout);

			var adapter2 = Db.Connection.Command("test").NewDataAdapter(10) as ReadOnlyDataAdapter;
			Assert(adapter2.DbDataAdapter_Exposed is DbDataAdapterWithTimeout);
		}

		async public void TestAsyncDisabledOnGuiThread()
		{
			try
			{
				using (var cts = new CancellationTokenSource())
				{
					await Db.Connection.Command("select 0").ExecuteNonQueryAsync(cts.Token);
					Fail("Did not throw any exception");
				}
			}
			catch (InvalidOperationException)
			{
				Assert(true);
			}
		}

		public void TestReadPastWithReadCommittedLock()
		{
			var correctQueries = new List<string>()
			{
				"SELECT TOP 1 [name] FROM sys.tables WITH (READPAST, READCOMMITTEDLOCK)",
				@"SELECT [name] FROM 
					(SELECT TOP 1 [name] FROM sys.tables WITH (READCOMMITTEDLOCK, ROWLOCK, READPAST)) t",
				@"DECLARE @1 NVARCHAR(1000);
				SET @1 = 'SELECT * FROM sys.tables WITH (READPAST)';",
				@"DECLARE @1 NVARCHAR(1000);
				SET @1 = '''a'' is WITH (READPAST)';",
				@"DECLARE @1 NVARCHAR(1000);
				SET @1 = 'SELECT * FROM sys.tables WITH (READPAST)';
				EXEC(N'SELECT [name] FROM sys.tables WITH (READPAST, READCOMMITTEDLOCK)')",
			};

			var incorrectQueries = new List<string>()
			{
				"SELECT TOP 1 [name] FROM sys.tables WITH (READPAST)",
				@"SELECT TOP 1 o.[name] FROM sys.objects o WITH (READPAST, READCOMMITTEDLOCK)
				INNER JOIN sys.tables t WITH (READPAST)
				ON o.name = t.name",
				@"EXEC(N'SELECT [name] FROM sys.tables WITH (READPAST)')",
				@"SELECT * FROM StmLink WITH (INDEX([PK_UX__STL_PK]), READPAST)",
			};

			//check incorrect queries
			foreach (var s in incorrectQueries)
			{
				AssertEquals(0, ErrorReporter.TotalErrorCount);

				Db.Connection.ExecuteNonQuery(s);

				AssertEquals(1, ErrorReporter.TotalErrorCount);

				AssertEquals("ReadpastWithoutReadCommittedLock", ErrorReporter.LastKeyReported);
				AssertEquals("There is READPAST without READCOMMITTEDLOCK in sql.\r\nIf this is sql in a comment or non sql field, please ensure \"READPAST\" is not wrapped with parenthesis - ie. change from (* READPAST *) to * READPAST *:\r\n\r\n" + s, ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();
			}

			//check correct queries
			foreach (var s in correctQueries)
			{
				Db.Connection.ExecuteNonQuery(s);

				AssertEquals(0, ErrorReporter.TotalErrorCount);
			}
		}

		public void TestRunCustomSQLAndRevert()
		{
			var sqlText = "CREATE TABLE tabe_tmp1_D1D7D9CD0373470698904D92A7039B17 (ColCode VARCHAR(3) NOT NULL)";
			AssertNoExceptionThrown(() => Db.Connection.ExecuteNonQuery(sqlText));
			sqlText = "SELECT COUNT(*) FROM tabe_tmp1_D1D7D9CD0373470698904D92A7039B17" + DbCommand.ExecuteAsReaderFlagComments;
			AssertNoExceptionThrown(() => Db.Connection.ExecuteScalar(sqlText));
			sqlText = "DROP TABLE tabe_tmp1_D1D7D9CD0373470698904D92A7039B17";
			AssertNoExceptionThrown(() => Db.Connection.ExecuteNonQuery(sqlText));
		}

		public void TestUseRevertAsNormalWord()
		{
			var sqlText = "CREATE TABLE tabe_tmp1_D1D7D9CD0373470698904D92A7039B17 (ColCode VARCHAR(3) NOT NULL)";
			AssertNoExceptionThrown(() => Db.Connection.ExecuteNonQuery(sqlText));
			sqlText = "SELECT COUNT(*) FROM tabe_tmp1_D1D7D9CD0373470698904D92A7039B17 WHERE ColCode = 'REVERT'" + DbCommand.ExecuteAsReaderFlagComments;
			AssertNoExceptionThrown(() => Db.Connection.ExecuteScalar(sqlText));
			sqlText = "DROP TABLE tabe_tmp1_D1D7D9CD0373470698904D92A7039B17";
			AssertNoExceptionThrown(() => Db.Connection.ExecuteNonQuery(sqlText));
		}

		[UseSnapshotProtection]
		public void TestCatchExceptionsWithCustomSqlFilter()
		{
			var sqlText = "delete top (1) from dbo.stmNumberCache" + DbCommand.ExecuteAsReaderFlagComments;
			AssertExceptionThrown(typeof(SqlException), "The DELETE permission was denied", () => Db.Connection.ExecuteNonQuery(sqlText), true);

			sqlText = "CREATE TABLE tabe_tmp1_D1D7D9CD0373470698904D92A7039B17 (ColCode VARCHAR(3) NOT NULL)";
			AssertNoExceptionThrown(() => Db.Connection.ExecuteNonQuery(sqlText));

			sqlText = "SELECT * FROM NotExisted" + DbCommand.ExecuteAsReaderFlagComments;
			AssertExceptionThrown(typeof(SqlException), "Invalid object name 'NotExisted'", () => Db.Connection.ExecuteScalar(sqlText), true);

			sqlText = "DROP TABLE tabe_tmp1_D1D7D9CD0373470698904D92A7039B17";
			AssertNoExceptionThrown(() => Db.Connection.ExecuteNonQuery(sqlText));

			sqlText = "revert; delete top (1) from dbo.stmNumberCache" + DbCommand.ExecuteAsReaderFlagComments;
			AssertExceptionThrown(typeof(SqlException), "The current security context cannot be reverted using this statement.", () => Db.Connection.ExecuteNonQuery(sqlText), true);

			var expectedLoginName = DataProtectionTestBed.Current.GetExpectedCredentials(TestEnvironmentWellKnownSecret.TestServerOdysseyAdmin).UserName;
			sqlText = string.Format(@"execute as user = '{0}'; delete top (1) from dbo.stmNumberCache", expectedLoginName) + DbCommand.ExecuteAsReaderFlagComments;
			AssertExceptionThrown(typeof(SqlException), "Cannot execute as the database principal", () => Db.Connection.ExecuteNonQuery(sqlText), true);
		}

		public void TestConversionExceptionsDoesNotThrowTransactionExceptionWhenExecutedAsReader()
		{
			var sqlText = "select TOP 1 * FROM dbo.StmALog where SL_FireWorkflow = @value" + DbCommand.ExecuteAsReaderFlagComments;
			AssertExceptionThrown(typeof(SqlException), "Conversion failed when converting the varchar value 'Y' to data type bit",
								  () => Db.Connection.ExecuteNonQuery(sqlText, cmd => cmd.AddParameter("@value", SqlDbType.VarChar, "Y")), true);

			sqlText = "select TOP 1 * FROM dbo.StmALog where SL_FireWorkflow = @value";
			AssertExceptionThrown(typeof(SqlException), "Conversion failed when converting the varchar value 'Y' to data type bit",
								  () => Db.Connection.ExecuteNonQuery(sqlText, cmd => cmd.AddParameter("@value", SqlDbType.VarChar, "Y")), true);

			Db.Connection.BeginTransaction();
			AssertExceptionThrown(typeof(SqlException), "Conversion failed when converting the varchar value 'Y' to data type bit",
								  () => Db.Connection.ExecuteNonQuery(sqlText, cmd => cmd.AddParameter("@value", SqlDbType.VarChar, "Y")), true);
			Db.Connection.RollbackTransaction();

			Db.Connection.BeginTransaction();
			sqlText = "select TOP 1 * FROM dbo.StmALog where SL_FireWorkflow = @value" + DbCommand.ExecuteAsReaderFlagComments;
			AssertExceptionThrown(typeof(SqlException), "Conversion failed when converting the varchar value 'Y' to data type bit",
								  () => Db.Connection.ExecuteNonQuery(sqlText, cmd => cmd.AddParameter("@value", SqlDbType.VarChar, "Y")), true);
			Db.Connection.RollbackTransaction();
		}

		public void TestErrorLevel16ExceptionsDoNotThrowTransactionExceptionWhenExecutedAsReader()
		{
			Db.Connection.BeginTransaction();
			var sqlText = "select * from NonExistantTable" + DbCommand.ExecuteAsReaderFlagComments;
			AssertExceptionThrown(typeof(SqlException), "Invalid object name 'NonExistantTable'", () => Db.Connection.ExecuteNonQuery(sqlText), true);
			Assert("we should be in a transaction", Db.Connection.IsInTransaction);
			AssertEquals("transaction count should be 1", 1, Db.Connection.AppTransactionCount);
			Db.Connection.RollbackTransaction();

			Db.Connection.BeginTransaction();
			sqlText = "SELECT TOP 1 NonExistantColumn FROM dbo.StmALog" + DbCommand.ExecuteAsReaderFlagComments;
			AssertExceptionThrown(typeof(SqlException), "Invalid column name 'NonExistantColumn'", () => Db.Connection.ExecuteNonQuery(sqlText), true);
			Assert("we should be in a transaction", Db.Connection.IsInTransaction);
			AssertEquals("transaction count should be 1", 1, Db.Connection.AppTransactionCount);
			Db.Connection.RollbackTransaction();

			Db.Connection.BeginTransaction();
			sqlText = "select TOP 1 * FROM dbo.StmActivityLog where S7_MouseClicks / 0 = 10" + DbCommand.ExecuteAsReaderFlagComments;
			AssertExceptionThrown(typeof(SqlException), "Divide by zero error encountered", () => Db.Connection.ExecuteNonQuery(sqlText), true);
			Assert("we should be in a transaction", Db.Connection.IsInTransaction);
			AssertEquals("transaction count should be 1", 1, Db.Connection.AppTransactionCount);
			Db.Connection.RollbackTransaction();

			Db.Connection.BeginTransaction();
			sqlText = "select TOP 1 * FROM dbo.StmALog where SL_FireWorkflow = @value" + DbCommand.ExecuteAsReaderFlagComments;
			AssertExceptionThrown(typeof(SqlException), "Conversion failed when converting the varchar value 'Y' to data type bit",
								  () => Db.Connection.ExecuteNonQuery(sqlText, cmd => cmd.AddParameter("@value", SqlDbType.VarChar, "Y")), true);
			AssertEquals("transaction count should be 1", 1, Db.Connection.AppTransactionCount);
			Assert("we should be in a transaction BUT we are not due to the conversion exception!", !Db.Connection.IsInTransaction);
			Db.Connection.RollbackTransaction();
		}

		public void TestExecuteReaderWithConversionExceptionInTransactionUsingDispose()
		{
			Db.Connection.BeginTransaction();
			var sqlText = "select TOP 1 * FROM dbo.StmALog where SL_FireWorkflow = @value" + DbCommand.ExecuteAsReaderFlagComments;
			AssertExceptionThrown(typeof(SqlException), "Conversion failed when converting the varchar value 'Y' to data type bit", () =>
			{
				using var cmd = Db.Connection.Command(sqlText);
				cmd.AddParameter("@value", SqlDbType.VarChar, "Y");
				using (var reader = cmd.ExecuteReader())
				{
					reader.Read();
				}
			}, true);
			Assert("we should be in a transaction BUT we are not due to the conversion exception!", !Db.Connection.IsInTransaction);
			Db.Connection.RollbackTransaction();
		}

		public void TestExecuteScalarWithConversionExceptionInTransaction()
		{
			Db.Connection.BeginTransaction();
			var sqlText = "select TOP 1 * FROM dbo.StmALog where SL_FireWorkflow = @value" + DbCommand.ExecuteAsReaderFlagComments;
			AssertExceptionThrown(typeof(SqlException), "Conversion failed when converting the varchar value 'Y' to data type bit",
								  () => Db.Connection.ExecuteScalar(sqlText, cmd => cmd.AddParameter("@value", SqlDbType.VarChar, "Y")), true);
			Assert("we should be in a transaction BUT we are not due to the conversion exception!", !Db.Connection.IsInTransaction);
			Db.Connection.RollbackTransaction();
		}

		public void TestUsernameShouldRevertWhenTransactionFailsDueToExceptionWhenExecutedAsReader()
		{
			var connection = Db.NewAdminConnection();
			var username = connection.ExecuteScalar("SELECT user_name()").ToString();
			var sqlText = "select TOP 1 * FROM dbo.StmALog where SL_FireWorkflow = 'Y'" + DbCommand.ExecuteAsReaderFlagComments;
			AssertExceptionThrown(typeof(SqlException), "Conversion failed when converting the varchar value 'Y' to data type bit", () => connection.ExecuteNonQuery(sqlText), true);
			AssertEquals("username as above", username, connection.ExecuteScalar("SELECT user_name()").ToString());

			connection.BeginTransaction();
			Assert("we should be in a transaction", connection.IsInTransaction);
			AssertEquals("username as above", username, connection.ExecuteScalar("SELECT user_name()").ToString());

			try
			{
				connection.ExecuteNonQuery(sqlText);
			}
			catch
			{
				//whatever exception is thrown, we need to make sure user is not left as reader, reset AppTransactionCount if no transaction.
				if (!connection.IsInTransaction && connection.AppTransactionCount > 0)
				{
					connection.RollbackTransaction();
				}
			}
			finally
			{
				AssertEquals(username, connection.ExecuteScalar("SELECT user_name()").ToString());
			}
			connection.RollbackTransaction();
			AssertEquals(username, connection.ExecuteScalar("SELECT user_name()").ToString());
		}

		[UseSnapshotProtection]
		public void TestPreventDangerousCustomSQL()
		{
			var sqlText = "CREATE TABLE tabe_tmp1_D1D7D9CD0373470698904D92A7039B17 (ColCode VARCHAR(3) NOT NULL)";
			AssertNoExceptionThrown(() => Db.Connection.ExecuteNonQuery(sqlText));

			sqlText = "CREATE TABLE tabe_tmp2_D1D7D9CD0373470698904D92A7039B18 (ColCode VARCHAR(3) NOT NULL)" + DbCommand.ExecuteAsReaderFlagComments;
			AssertExceptionThrown(typeof(SqlException), "CREATE TABLE permission denied in database", () => Db.Connection.ExecuteNonQuery(sqlText), true);

			sqlText = "DROP TABLE tabe_tmp1_D1D7D9CD0373470698904D92A7039B17";
			AssertNoExceptionThrown(() => Db.Connection.ExecuteNonQuery(sqlText));
		}

		public void TestConnection()
		{
			DbCommand cmd = Db.Connection.Command("SELECT TOP 1 * FROM sys.columns");

			using (var reader = cmd.ExecuteReader())
			{
				Assert("Sample Recordset Retrieval", reader.FieldCount > 0);
			}
		}

		public void TestConnectionWhenMultiThreadedWebEnvironment()
		{
			try
			{
				Db.Instance.IsWebTestOverride = true;

				Thread[] threads = new Thread[2];

				for (int x = 0; x < threads.Length; x++)
				{
					threads[x] = new Thread(new ThreadStart(ThreadingTestMethod));
				}

				for (int x = 0; x < threads.Length; x++)
				{
					threads[x].Start();
				}

				for (int x = 0; x < threads.Length; x++)
				{
					threads[x].Join();
				}
			}
			catch (Exception ex)
			{
				ErrorMessage += "An Exception was thrown in the test method. (Original Thread)" + System.Environment.NewLine;
				ErrorMessage += "Exception Type: [" + ex.GetType() + "]" + System.Environment.NewLine;
				ErrorMessage += "Exception Message: [" + ex.Message + "]" + System.Environment.NewLine;
				ErrorMessage += System.Environment.NewLine + ex.ToString() + System.Environment.NewLine;
			}
			finally
			{
				Db.Instance.IsWebTestOverride = null;
			}

			AssertEquals("Error Message", string.Empty, ErrorMessage);
		}

		void ThreadingTestMethod()
		{
			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					for (int x = 0; x < 2; x++)
					{
						using (var selectCmd = Db.Connection.Command("SELECT null FROM dbo.RefCurrency"))
						{
							using (var dataReader = selectCmd.ExecuteReader())
							{
							}
						}
					}
				}
				catch (Exception ex)
				{
					ErrorMessage += "An Exception was thrown in the ThreadingTestMethod. (New Thread)" + System.Environment.NewLine;
					ErrorMessage += "Exception Type: [" + ex.GetType() + "]" + System.Environment.NewLine;
					ErrorMessage += "Exception Message: [" + ex.Message + "]" + System.Environment.NewLine;
					ErrorMessage += System.Environment.NewLine + ex.ToString() + System.Environment.NewLine;
				}
			}
		}

		string ErrorMessage = string.Empty;

		[ExpectException(typeof(ReadOnlyException))]
		public void TestConnectionPropertyIsReadOnly()
		{
			DbCommand testCmd = Db.Connection.Command("");
			testCmd.Connection = null;
		}

		[ExpectException(typeof(ReadOnlyException))]
		public void TestTransactionPropertyIsReadOnly()
		{
			DbCommand testCmd = Db.Connection.Command("");
			testCmd.Transaction = null;
		}

		[ExpectException(typeof(InvalidOperationException))]
		public void TestReferencingParameterCollectionThrowsException()
		{
			DbCommand testCmd = Db.Connection.Command("");
			IDataParameterCollection testCollection = ((IDbCommand)testCmd).Parameters;
		}

		[ExpectException(typeof(InvalidOperationException))]
		public void TestCallingCreateParameterThrowsException()
		{
			DbCommand testCmd = Db.Connection.Command("");
			IDbDataParameter testParameter = ((IDbCommand)testCmd).CreateParameter();
		}

		public void TestExecuteScalarHandlesGeneralNetworkError()
		{
			AdoTestUtils.KillMainConnection();
			string value = Db.Connection.ExecuteScalar("SELECT '" + TestValue + "'").ToString();
			AssertEquals("Connection should have been reopened and value retrieved", TestValue, value);
		}

		public void TestExecuteReaderHandlesGeneralNetworkError()
		{
			AdoTestUtils.KillMainConnection();

			using (var reader = Db.Connection.Command("SELECT '" + TestValue + "'").ExecuteReader())
			{
				Assert("A record should have been read", reader.Read());
				AssertEquals("Connection should have been reopened and reader returned", TestValue, reader.GetString(0));
			}
		}

		public void TestExecuteReaderWithBehaviorHandlesGeneralNetworkError()
		{
			AdoTestUtils.KillMainConnection();

			using (var reader = Db.Connection.Command("SELECT '" + TestValue + "'").ExecuteReader(CommandBehavior.SingleRow))
			{
				Assert("A record should have been read", reader.Read());
				AssertEquals("Connection should have been reopened and reader returned", TestValue, reader.GetString(0));
			}
		}

		[ExpectNoExceptions]
		public void TestExecuteNonQueryHandlesGeneralNetworkError()
		{
			AdoTestUtils.KillMainConnection();
			Db.Connection.ExecuteNonQuery("--");
		}

		public void TestSetParameterValue()
		{
			DateTime dateTimeBefore = new DateTime(2004, 5, 26, 3, 20, 0);
			DateTime dateTimeAfter = new DateTime(2004, 5, 26, 3, 20, 15, 100);
			using (var conn = Db.NewExtraConnectionToMainDb())
			{
				NonExecutableDbCommandForTest testCmd = new NonExecutableDbCommandForTest(conn.Command("").Connection);
				testCmd.AddParameter("@ParamInt", SqlDbType.Int, 1);
				testCmd.AddParameter("@ParamVarchar", SqlDbType.VarChar, 10, "Before");
				testCmd.AddParameter("@ParamSmallDateTime", SqlDbType.SmallDateTime, dateTimeBefore);
				testCmd.AddParameter("@ParamDecimal42", SqlDbType.Decimal, 0, 4, 2, 23.78D);

				AssertEquals("ParamInt Type (1)", DbType.Int32, testCmd.ExposedParameters["@ParamInt"].DbType);
				AssertEquals("ParamInt Value (1)", 1, (int)testCmd.ExposedParameters["@ParamInt"].Value);
				AssertEquals("ParamVarchar Type (1)", DbType.AnsiString, testCmd.ExposedParameters["@ParamVarchar"].DbType);
				AssertEquals("ParamVarchar Value (1)", "Before", (string)testCmd.ExposedParameters["@ParamVarchar"].Value);
				AssertEquals("ParamSmallDateTime Type (1)", DbType.DateTime, testCmd.ExposedParameters["@ParamSmallDateTime"].DbType);
				AssertEquals("ParamSmallDateTime Value (1)", dateTimeBefore, (DateTime)testCmd.ExposedParameters["@ParamSmallDateTime"].Value);
				AssertEquals("ParamDecimal42 Type (1)", DbType.Decimal, testCmd.ExposedParameters["@ParamDecimal42"].DbType);
				AssertEquals("ParamDecimal42 Precision (1)", (byte)4, ((SqlParameter)testCmd.ExposedParameters["@ParamDecimal42"]).Precision);
				AssertEquals("ParamDecimal42 Scale (1)", (byte)2, ((SqlParameter)testCmd.ExposedParameters["@ParamDecimal42"]).Scale);
				AssertEquals("ParamDecimal42 Value (1)", 23.78D, (double)testCmd.ExposedParameters["@ParamDecimal42"].Value);

				testCmd.SetParameterValue("@ParamInt", 2);
				testCmd.SetParameterValue("@ParamVarchar", "After");
				testCmd.SetParameterValue("@ParamSmallDateTime", dateTimeAfter);
				testCmd.SetParameterValue("@ParamDecimal42", 35.789D);

				AssertEquals("ParamInt Type (2)", DbType.Int32, testCmd.ExposedParameters["@ParamInt"].DbType);
				AssertEquals("ParamInt Value (2)", 2, (int)testCmd.ExposedParameters["@ParamInt"].Value);
				AssertEquals("ParamVarchar Type (2)", DbType.AnsiString, testCmd.ExposedParameters["@ParamVarchar"].DbType);
				AssertEquals("ParamVarchar Value (2)", "After", (string)testCmd.ExposedParameters["@ParamVarchar"].Value);
				AssertEquals("ParamSmallDateTime Type (2)", DbType.DateTime, testCmd.ExposedParameters["@ParamSmallDateTime"].DbType);
				AssertEquals("ParamSmallDateTime Value (2)", dateTimeAfter, (DateTime)testCmd.ExposedParameters["@ParamSmallDateTime"].Value);
				AssertEquals("ParamDecimal42 Type (2)", DbType.Decimal, testCmd.ExposedParameters["@ParamDecimal42"].DbType);
				AssertEquals("ParamDecimal42 Precision (2)", (byte)4, ((SqlParameter)testCmd.ExposedParameters["@ParamDecimal42"]).Precision);
				AssertEquals("ParamDecimal42 Scale (2)", (byte)2, ((SqlParameter)testCmd.ExposedParameters["@ParamDecimal42"]).Scale);
				AssertEquals("ParamDecimal42 Value (2)", 35.789D, (double)testCmd.ExposedParameters["@ParamDecimal42"].Value);
			}
		}

		[ExpectException(typeof(IndexOutOfRangeException))]
		public void TestSetParameterValueThrowsExceptionIfParameterDoesNotExist()
		{
			DbCommand testCmd = Db.Connection.Command("");
			testCmd.AddParameter("@TestParam", SqlDbType.Int, 1);
			testCmd.SetParameterValue("@NonexistingParameterName", 2);
		}

		public void TestVarBinaryMaxParameter()
		{
			var value = new byte[] { 0x01, 0x02, };
			var column_1 = new SchemaBinaryColumn(CargoWise.Schema.Schema.GenericTableSchema, "Col_1", 0, SqlDbType.VarBinary, DBNull.Value, true, -1);
			var column_2 = new SchemaBinaryColumn(CargoWise.Schema.Schema.GenericTableSchema, "Col_2", 0, SqlDbType.VarBinary, DBNull.Value, true, 9000);
			using (var cmd = Db.Connection.Command(""))
			{
				cmd.AddParameterBasedOnDbColumn("@col_1", value, column_1);
				cmd.AddParameterBasedOnDbColumn("@col_2", value, column_2);

				AssertEquals(-1, cmd.GetParameter("@col_1").Size);
				AssertEquals(-1, cmd.GetParameter("@col_2").Size);
			}
		}

		public void TestRemoveParameterIfExists()
		{
			DbCommand testCmd = Db.Connection.Command("");
			testCmd.AddParameter("@TestParam1", SqlDbType.Int, 1);
			testCmd.AddParameter("@TestParam2", SqlDbType.VarChar, "Two");
			testCmd.AddParameter("@TestParam3", SqlDbType.Int, 3);

			Assert("Should contain @TestParam1 (1)", testCmd.ContainParameter("@TestParam1"));
			Assert("Should contain @TestParam2 (1)", testCmd.ContainParameter("@TestParam2"));
			Assert("Should contain @TestParam3 (1)", testCmd.ContainParameter("@TestParam3"));

			testCmd.RemoveParameterIfExists("@TestParam2");

			Assert("Should contain @TestParam1 (2)", testCmd.ContainParameter("@TestParam1"));
			Assert("Should NOT contain @TestParam2 anymore", !testCmd.ContainParameter("@TestParam2"));
			Assert("Should contain @TestParam3 (2)", testCmd.ContainParameter("@TestParam3"));
		}

		public void TestRemoveParameterIfExistsThrowsNoExceptionIfParameterDoesNotExist()
		{
			DbCommand testCmd = Db.Connection.Command("");
			testCmd.AddParameter("@TestParam", SqlDbType.Int, 1);

			// Should NOT throw exception
			testCmd.RemoveParameterIfExists("@TestParamNotPartOfCollection");

			Assert("Should contain @TestParam", testCmd.ContainParameter("@TestParam"));
		}

		public void TestContainParameter()
		{
			DbCommand testCmd = Db.Connection.Command("");
			testCmd.AddParameter("@TestParam1", SqlDbType.Int, 0);
			testCmd.AddParameter("@TestParam2", SqlDbType.VarChar, 4, "Test");

			Assert("Should contain @TestParam1", testCmd.ContainParameter("@TestParam1"));
			Assert("Should contain @TestParam2", testCmd.ContainParameter("@TestParam2"));
			Assert("Should NOT contain @TestParam3", !testCmd.ContainParameter("@TestParam3"));
		}

		public void TestParameterCollectionEquals()
		{
			DbCommand testCmd1 = Db.Connection.Command("");
			testCmd1.AddParameter("@TestParam1", SqlDbType.Int, 0);
			testCmd1.AddParameter("@TestParam2", SqlDbType.VarChar, 4, "Test");

			DbCommand testCmd2 = Db.Connection.Command("");
			testCmd2.AddParameter("@TestParam1", SqlDbType.Int, 0);
			testCmd2.AddParameter("@TestParam2", SqlDbType.VarChar, 4, "Test");

			Assert("TestCmd1 ParameterCollection should be the same as TestCmd2's", testCmd1.ParameterCollectionEquals(testCmd2));
			Assert("TestCmd2 ParameterCollection should be the same as TestCmd1's", testCmd2.ParameterCollectionEquals(testCmd1));

			// Create TestCmd3 with 3 parameters
			DbCommand testCmd3 = Db.Connection.Command("");
			testCmd3.AddParameter("@TestParam1", SqlDbType.Int, 0);
			testCmd3.AddParameter("@TestParam2", SqlDbType.VarChar, 4, "Test");
			testCmd3.AddParameter("@TestParam3", SqlDbType.UniqueIdentifier, Guid.Empty);

			Assert("ParameterCollections should be different - count is different", !testCmd1.ParameterCollectionEquals(testCmd3));

			// Add a 3rd parameter to TestCmd1 (with a different value from TestCmd3's 3rd parameter)
			testCmd1.AddParameter("@TestParam3", SqlDbType.UniqueIdentifier, Guid.NewGuid());
			Assert("ParameterCollections should be different - value of a parameter is different", !testCmd1.ParameterCollectionEquals(testCmd3));

			// Add a 3rd parameter to TestCmd2 (with a different data type from TestCmd3's 3rd parameter)
			testCmd2.AddParameter("@TestParam3", SqlDbType.Char, Guid.Empty.ToString());
			Assert("ParameterCollections should be different - 3rd param data type is different", !testCmd2.ParameterCollectionEquals(testCmd3));

			// Commands with different parameter names
			DbCommand testCmd4 = Db.Connection.Command("");
			testCmd4.AddParameter("@TestParam1", SqlDbType.Int, 0);
			DbCommand testCmd5 = Db.Connection.Command("");
			testCmd5.AddParameter("@TestParam2", SqlDbType.Int, 0);
			Assert("ParameterCollections should be different - Parameter names are different", !testCmd4.ParameterCollectionEquals(testCmd5));
		}

		[ExpectException(typeof(DbException))]
		public void TestExecuteProcedureWithReturnValueThrowsExceptionIfCommandIsNotAStoredProcedure()
		{
			DbCommand procedureCommand = Db.Connection.Command("SELECT TOP 1 * FROM dbo.RefCountry");
			procedureCommand.ExecuteProcedureWithReturnValue();
		}

		public void TestExecuteThrowsExceptionWhenAppTransactionRolledBackInDbServer()
		{
			try
			{
				Db.Connection.BeginTransaction();
				Db.Connection.ExecuteNonQuery("CREATE VIEW TestView5EF3AFE23F8643338B3D2663A1BE0580 AS SELECT 1 Col1");

				try
				{
					Db.Connection.ExecuteNonQuery("ALTER PROCEDURE TestView5EF3AFE23F8643338B3D2663A1BE0580 AS SELECT 0");
					Fail("[PRE-CONDITION] Should have thrown SqlException");
				}
				catch (SqlException ex)
				{
					DbErrorMatch dbError = new DbErrorMatch(ex);
					AssertEquals("[PRE-CONDITION] Caught Exception", DbErrorType.CannotAlterObjectOfIncompatibleType, dbError.ExceptionType);
				}

				try
				{
					Db.Connection.ExecuteScalar("SELECT 0");
					Fail("Should have thrown InvalidOperationException");
				}
				catch (TransactionException ex)
				{
					AssertEquals("Caught Exception", "Transaction has been rolled back in the server (application transaction count pending reset).", ex.Message);
				}
			}
			finally
			{
				while (Db.Connection.AppTransactionCount > 0)
				{
					Db.Connection.RollbackTransaction();
				}
			}
		}

		[UseSnapshotProtection]
		public void TestRowcountOnReconnection()
		{
			string sqltext = @"
				UPDATE dbo.StmData SET SD_Type = 'STR' WHERE SD_Name = 'AUCustomsEdificeSenderID'  AND SD_Owner = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC' AND SD_Type = 'STR'
				IF (@@rowcount = 0)
				BEGIN
					INSERT dbo.StmData (SD_PK, SD_Name , SD_Owner , SD_Type, SD_BinaryValue, SD_GuidValue, SD_IsLogged) VALUES (NEWID(), 'APMAA' , null , 'GID', null, null,  1)
				END";

			using (var conn = Db.NewExtraConnectionToMainDb())
			{
				var command = conn.Command(sqltext);
				var mockCommand = new MockCommandForTest(command);
				AssertNoExceptionThrown(() => { mockCommand.ExecuteForTest(new MockCommandRunnerWithReconnectForTest()); });
			}
		}

		public void TestCommunicationExceptionIsThownIfUnableToExecuteCommandAfterRetries()
		{
			// Arrange
			var command = Db.Connection.Command("SELECT 1");
			var mockCommand = new MockCommandForTest(command);
			// Act/Assert
			AssertExceptionThrown<CommunicationException>(() => { mockCommand.ExecuteForTest(new MockCommandRunnerThrowingInvalidOperationExcepionForTest()); });
		}

		#region Implementation

		class MockCommandForTest : DbCommand
		{
			public MockCommandForTest(DbCommand command)
				: base(command.CommandText, command.DbConnection, command.Connection, command.Transaction, 5)
			{
			}

			public object ExecuteForTest(CommandRunner runner)
			{
				return base.Execute(runner);
			}
		}

		class MockCommandRunnerThrowingInvalidOperationExcepionForTest : CommandRunner
		{
			protected override object ExecuteCore(IDbCommand command)
			{
				var ex = new InvalidOperationException();
				ex.Source = "System.Data";
				throw ex;
			}
		}

		class MockCommandRunnerWithReconnectForTest : CommandRunner
		{
			public MockCommandRunnerWithReconnectForTest()
			{
				reconnect = false;
			}

			bool reconnect;

			protected override object ExecuteCore(IDbCommand command)
			{
				if (!reconnect)
				{
					SqlError sqlError = CargoWise.Data.Testing.SqlExceptionBuilder.CreateSqlError(4083, byte.MaxValue, byte.MinValue, "CargoWise One", "The connection was recovered and rowcount in the first query is not available. Please execute another query to get a valid rowcount.", "@@lols", 5);
					SqlErrorCollection errors = CargoWise.Data.Testing.SqlExceptionBuilder.CreateSqlErrorCollection(sqlError);
					var exception = CargoWise.Data.Testing.SqlExceptionBuilder.CreateSqlException(errors);
					reconnect = true;
					throw exception;
				}
				else
				{
					reconnect = false;
					return command.ExecuteNonQuery();
				}
			}
		}

		const string TestValue = "TestValue";

		#endregion
	}
}
