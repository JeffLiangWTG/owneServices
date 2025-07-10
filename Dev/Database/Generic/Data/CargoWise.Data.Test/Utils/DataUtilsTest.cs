using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using CargoWise.Common;
using CargoWise.Schema;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Moq;
using NUnit.Framework;
using static System.FormattableString;

namespace CargoWise.Data.Testing
{
	public class DataUtilsTest : TestCase
	{
		public void TestLoadDbExtendedPropertyOnMainDb_WhenOtherConnectionHasLock()
		{
			var mockDb_Main = "MockDbTestDatabase";
			using (AdoTestUtils.CreateDbDropExistingDisposable(mockDb_Main))
			using (var testConnection = Db.NewAdminConnection(mockDb_Main))
			using (testConnection.TemporarySetLockTimeout(0))
			using (var otherConnection = Db.NewAdminConnection())
			{
				otherConnection.BeginTransaction();
				var propertyName = "TestName";
				DataUtils.AddDbExtendedProperty(otherConnection, propertyName, "test value");

				AssertEquals("when withNoLock is true", "test value", DataUtils.LoadDbExtendedPropertyOnMainDb(testConnection, propertyName, withNoLock: true));
				var sqlException = AssertExceptionThrown<SqlException>("when withNoLock is false", () => DataUtils.LoadDbExtendedPropertyOnMainDb(testConnection, propertyName, withNoLock: false));
				AssertEquals("lock timeout", 1222, sqlException.Number);
			}
		}

		public void TestLoadDbExtendedPropertyWithNoLock()
		{
			var mockDb_Main = "MockDbTestDatabase";
			using (AdoTestUtils.CreateDbDropExistingDisposable(mockDb_Main))
			using (var testConnection = Db.NewAdminConnection(mockDb_Main))
			using (testConnection.TemporarySetLockTimeout(0))
			using (var otherConnection = Db.NewAdminConnection(mockDb_Main))
			{
				otherConnection.BeginTransaction();
				var propertyName = "TestName";
				DataUtils.AddDbExtendedProperty(otherConnection, propertyName, "test value");

				AssertEquals("when withNoLock is true", "test value", DataUtils.LoadDbExtendedPropertyWithNoLock(testConnection, propertyName));
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1157:Do not use System.AppDomain.", Justification = "Pending migration")]   // WI00669071 - Do not use System.AppDomain.
		public void TestGetDbSeverFullDomainNameIncludingSqlPort_HavingPortNumber()
		{
#if NETFRAMEWORK
			var appDomain = AppDomain.CreateDomain($"{nameof(DataUtilsTest)}_{nameof(TestGetDbSeverFullDomainNameIncludingSqlPort_HavingPortNumber)}");
			try
			{
				appDomain.SetData("ServerName", Db.ServerName);
				appDomain.SetData("DatabaseName", Db.DatabaseName);
				appDomain.SetData("ServerPort", GetConnectionPort());
				appDomain.DoCallBack(() =>
				{
					try
					{
						// Arrange
						var serverName = (string)AppDomain.CurrentDomain.GetData("ServerName");
						var dbName = (string)AppDomain.CurrentDomain.GetData("DatabaseName");
						var actualPort = (int)AppDomain.CurrentDomain.GetData("ServerPort");
						serverName = (serverName.IndexOf(',') == -1) ? (serverName + $",{actualPort}") : serverName;

						Db.InitializeDatabaseDetails(serverName, dbName);

						using (Db.DisableSchemaVersionCheck())
						{
							// Act
							new DataUtilsTest().AssertGetDbSeverFullDomainNameIncludingSqlPort_HavingPortNumber();
						}
						AppDomain.CurrentDomain.SetData("Exception", null);
					}
					catch (Exception ex)
					{
						AppDomain.CurrentDomain.SetData("Exception", ex);
					}
				});

				// Assert
				var exception = (Exception)appDomain.GetData("Exception");
				HtmlAssertNull(exception?.Message, exception as AssertionFailedError);
				AssertNull(exception);
			}
			finally
			{
				AppDomain.Unload(appDomain);
			}
#else
			Fail("This test needs to be migrated to use AppDomainWrapper.");
#endif
		}

		void AssertGetDbSeverFullDomainNameIncludingSqlPort_HavingPortNumber()
		{
			CombineAssertions(() =>
			{
				// Arrange
				var hostName = Environment.MachineName;
				var domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
				var fullName = hostName + "." + domainName;
				var port = Db.SqlServerPort;

				// Act
				// Assert
				AssertEquals($@"{fullName},{port}", DataUtils.GetDbSeverFullDomainNameIncludingSqlPort($@"{hostName},{port}"));
				AssertEquals($@"{fullName},{port}", DataUtils.GetDbSeverFullDomainNameIncludingSqlPort($@"{fullName},{port}"));
				AssertEquals($@"{fullName}\InstanceName,{port}", DataUtils.GetDbSeverFullDomainNameIncludingSqlPort($@"{hostName}\InstanceName,{port}"));
				AssertEquals($@"{fullName}\InstanceName,{port}", DataUtils.GetDbSeverFullDomainNameIncludingSqlPort($@"{fullName}\InstanceName,{port}"));
				AssertExceptionThrown<ArgumentException>("double comma", () => DataUtils.GetDbSeverFullDomainNameIncludingSqlPort($@"{fullName}\InstanceName,,123"));
			});
		}

		public int GetConnectionPort()
		{
			string sql = @"
DECLARE @tcp_port INT

IF @tcp_port IS NULL
  BEGIN
      SELECT @tcp_port = local_tcp_port
      FROM   sys.dm_exec_connections
      WHERE  session_id = @@SPID
  END

IF @tcp_port IS NULL
  BEGIN
      SELECT TOP 1 @tcp_port = port
      FROM   sys.dm_tcp_listener_states
      WHERE  type_desc = 'TSQL'
             AND state_desc = 'ONLINE'
             AND ip_address IN ( '::', '0.0.0.0' )
      ORDER  BY listener_id
  END

SELECT ISNULL(@tcp_port, 1433) ";
			using (var newAdminConnection = Db.NewAdminConnection())
			{
				return newAdminConnection.ExecuteScalar<int>(sql);
			}
		}

		public void TestGetDbSeverFullDomainNameIncludingSqlPort()
		{
			var machineName = System.Environment.MachineName;
			var fullName = GetFQDNFromLocalMachine();

			AssertStartsWith("Ignore port number", fullName, DataUtils.GetDbSeverFullDomainNameIncludingSqlPort(machineName));
			AssertStartsWith("Ignore port number", fullName, DataUtils.GetDbSeverFullDomainNameIncludingSqlPort(fullName));
			AssertStartsWith("Ignore port number", fullName + @"\InstanceName", DataUtils.GetDbSeverFullDomainNameIncludingSqlPort(machineName + @"\InstanceName"));
			AssertStartsWith("Ignore port number", fullName + @"\InstanceName", DataUtils.GetDbSeverFullDomainNameIncludingSqlPort(fullName + @"\InstanceName"));
			AssertExceptionThrown(typeof(ArgumentException), () => DataUtils.GetDbSeverFullDomainNameIncludingSqlPort(machineName + @"\\InstanceName"));
		}

		public void TestAlterDbAuthorisationWithNullExceptionAndDeclinedLock()
		{
			// Arrange

			using (var blockingConnection = Db.NewAdminConnection())
			{
				const string testDatabaseName = nameof(testDatabaseName);
				try
				{
					AdoTestUtils.CreateDbDropExisting(blockingConnection, testDatabaseName);
					blockingConnection.TryGetLock("AlterDbAuthorisation_App_Lock", out var applock, testDatabaseName);
					using (applock)
					{
						using (var adminConnection = Db.NewAdminConnection())
						{
							// Act
							var result = AssertExceptionThrown<OperationCanceledException>(() => DataUtils.AlterDbAuthorisation(adminConnection, testDatabaseName, (SqlException)null));

							// Assert
							result.Message.Contains("AlterDbAuthorisation_App_Lock");
							result.Message.Contains("(null)");
						}
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(blockingConnection, testDatabaseName);
				}
			}
		}

		public void TestAlterDbAuthorisationWithException()
		{
			const string testDatabaseName = "TestDatabaseForTestAlterDbAuthorisationWithException";
			const string exceptionMessage = "Test exception Message";

			using (var adminConnection = Db.NewAdminConnection())
			{
				try
				{
					AdoTestUtils.CreateDbDropExisting(adminConnection, testDatabaseName);
					DataUtils.AlterDbAuthorisation(adminConnection, testDatabaseName, SqlExceptionBuilder.CreateSqlException(99, exceptionMessage));
					AssertEquals(ErrorReporter.LastMessageReported, Invariant($"AlterDbAuthorisation on db: '{testDatabaseName}' failed with error: {exceptionMessage}.(switched to 'sa')."));
					ErrorReporter.Clear();
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(adminConnection, testDatabaseName);
				}
			}
		}

		string GetFQDNFromLocalMachine()
		{
			var domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
			var hostName = Dns.GetHostName();

			if (!hostName.EndsWith(domainName))
			{
				hostName += "." + domainName;
			}

			return hostName;
		}

		public void TestGetPk()
		{
			DataTable table = new DataTable();
			table.Columns.Add("PK", typeof(Guid));
			table.PrimaryKey = new DataColumn[] { table.Columns[0] };

			Guid pK = Guid.NewGuid();
			DataRow row = table.NewRow();
			row["PK"] = pK;
			AssertEquals("Can get PK", pK, DataUtils.GetPk(row));

			table.Rows.Add(row);
			AssertEquals("Can get PK", pK, DataUtils.GetPk(row));
			row.AcceptChanges();

			row.Delete();
			AssertEquals("Can get PK", pK, DataUtils.GetPk(row));

			row = table.NewRow();
			row["PK"] = pK;
			row.Delete();
			AssertEquals("Can get PK if detatched", pK, DataUtils.GetPk(row));

			row.CancelEdit();
			AssertEquals("Doesn't blow up even if row is really broken", Guid.Empty, DataUtils.GetPk(row));
		}

		public void TestIsDataInRowAccessible()
		{
			DataTable table = new DataTable();
			DataRow row = table.NewRow();
			AssertEquals("Row on table without columns.", false, DataUtils.IsDataInRowAccessible(row));
			row.CancelEdit();

			table.Columns.Add();
			row = table.NewRow();
			row.CancelEdit();
			AssertEquals("Row has been cancel-edited.", false, DataUtils.IsDataInRowAccessible(row));

			row.BeginEdit();
			AssertEquals("Row is just fine and dandy.", false, DataUtils.IsDataInRowAccessible(row));

			row.Table.Rows.Add(row);
			AssertEquals("Row is still fine and dandy.", true, DataUtils.IsDataInRowAccessible(row));

			row.CancelEdit();
			AssertEquals("Row has been cancel-edited but is in table.", true, DataUtils.IsDataInRowAccessible(row));

			row.AcceptChanges();
			AssertEquals("Row is just fine and dandy.", true, DataUtils.IsDataInRowAccessible(row));

			row.Delete();
			AssertEquals("Row has been deleted.", false, DataUtils.IsDataInRowAccessible(row));
		}

		public void TestIsDataInRowAccessible_ModifyAfterDelete()
		{
			var table = new DataTable("NewTable");
			var column = new DataColumn("NewColumn");
			table.Columns.Add(column);
			var newRow = table.NewRow();
			newRow["NewColumn"] = 12;
			table.Rows.Add(newRow);
			newRow.AcceptChanges();
			newRow.SetAdded();

			newRow.Delete();
			newRow.BeginEdit();
			newRow["NewColumn"] = 23;
			Assert(newRow.HasVersion(DataRowVersion.Proposed));
			AssertEquals("I deleted the row. Why would I be able to edit it?", false, DataUtils.IsDataInRowAccessible(newRow));
		}

		public void TestStringInterning()
		{
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, "select 'A' union all select 'A'");
			Assert("A not reference equal to other A", object.ReferenceEquals(dataTable.Rows[0][0], dataTable.Rows[1][0]));
		}

		public void TestGetDataTableFromCommand()
		{
			using (var cmd = Db.Connection.Command("SELECT * FROM (VALUES (1, @Desc1), (@Code2, 'Two')) T(ColCode, ColDesc);"))
			{
				cmd.AddParameter("@Desc1", SqlDbType.VarChar, "Uno");
				cmd.AddParameter("@Code2", SqlDbType.Int, 2);

				var testTable = DataUtils.GetDataTableFromCommand(cmd);
				AssertEquals("1st row code", 1, testTable.Rows[0]["ColCode"]);
				AssertEquals("1st row desc", "Uno", testTable.Rows[0]["ColDesc"]);
				AssertEquals("2nd row code", 2, testTable.Rows[1]["ColCode"]);
				AssertEquals("2nd row desc", "Two", testTable.Rows[1]["ColDesc"]);
			}
		}

		public void TestGetListOfValuesFromQuery()
		{
			var values = DataUtils.GetListOfValuesFromQuery(Db.Connection, "SELECT * FROM (VALUES('VAL0'),('VAL1'),('VAL2')) T(c);");
			var valuesList = values.ToList();
			AssertEquals("List Item 0", "VAL0", valuesList[0]);
			AssertEquals("List Item 1", "VAL1", valuesList[1]);
			AssertEquals("List Item 2", "VAL2", valuesList[2]);
		}

		public void TestGetListOfValuesFromQuery_Parameters()
		{
			var sql = "SELECT * FROM (VALUES('VAL0'),('VAL1'),('VAL2')) T(c) WHERE @1=1";

			AssertArrayEqualsByElements(
				expected: new[]
				{
					"VAL0",
					"VAL1",
					"VAL2",
				}
				, actual: DataUtils.GetListOfValuesFromQuery(Db.Connection, sql, cmd => cmd.AddParameter("@1", SqlDbType.Bit, 1)).ToArray());
		}

		public void TestGetListOfValuesFromCommand()
		{
			using (var cmd = Db.Connection.Command("SELECT numero FROM (VALUES('ZERO'),('UNO'),('DUE'),('TRE')) T(numero);"))
			{
				var values = DataUtils.GetListOfValuesFromCommand(cmd);
				var valuesList = values.ToList();
				AssertEquals("List Item 0", "ZERO", valuesList[0]);
				AssertEquals("List Item 1", "UNO", valuesList[1]);
				AssertEquals("List Item 2", "DUE", valuesList[2]);
				AssertEquals("List Item 3", "TRE", valuesList[3]);
			}
		}

		public void TestReplaceSqlLikeWildcard()
		{
			AssertEquals("Empty string", "", DataUtils.ReplaceSqlLikeWildcard(""));
			AssertEquals("Some_Text", "Some[_]Text", DataUtils.ReplaceSqlLikeWildcard("Some_Text"));
			AssertEquals("Some__Text", "Some[_][_]Text", DataUtils.ReplaceSqlLikeWildcard("Some__Text"));
			AssertEquals("Some%Text_", "Some[%]Text[_]", DataUtils.ReplaceSqlLikeWildcard("Some%Text_"));
			AssertEquals("_Some%%Text%", "[_]Some[%][%]Text[%]", DataUtils.ReplaceSqlLikeWildcard("_Some%%Text%"));
		}

		public void TestEscapeSingleQuotes()
		{
			AssertEquals("Empty string", "", DataUtils.EscapeSingleQuotes(""));
			AssertEquals("Some'Text", "Some''Text", DataUtils.EscapeSingleQuotes("Some'Text"));
			AssertEquals("Some''Text", "Some''''Text", DataUtils.EscapeSingleQuotes("Some''Text"));
			AssertEquals("Some'Text'", "Some''Text''", DataUtils.EscapeSingleQuotes("Some'Text'"));
			AssertEquals("'Some''Text'", "''Some''''Text''", DataUtils.EscapeSingleQuotes("'Some''Text'"));
		}

		public void TestGetBetweenEndExpressionForQueryUsingLikeWithFilteredIndex()
		{
			var varcharColumn = new SchemaStringColumn(CargoWise.Schema.Schema.GenericTableSchema, "VarcharColumn", 0, SqlDbType.VarChar, string.Empty, false, 5);
			AssertEquals("C00þ", DataUtils.GetBetweenEndExpressionForQueryUsingLikeWithFilteredIndex("C001", varcharColumn));
			AssertEquals("10þ", DataUtils.GetBetweenEndExpressionForQueryUsingLikeWithFilteredIndex("10Z", varcharColumn));
			AssertEquals("return the input string when the string length is equals to the schemaColumn MaxLength", "12345", DataUtils.GetBetweenEndExpressionForQueryUsingLikeWithFilteredIndex("12345", varcharColumn));

			AssertExceptionThrown<ArgumentException>("The value of the column NvarcharColumn can't be empty.", () => DataUtils.GetBetweenEndExpressionForQueryUsingLikeWithFilteredIndex("", varcharColumn));

			var nvarcharColumn = new SchemaStringColumn(CargoWise.Schema.Schema.GenericTableSchema, "NvarcharColumn", 0, SqlDbType.NVarChar, string.Empty, false, 5);
			AssertExceptionThrown<ArgumentException>("NvarcharColumn column is using UNICODE encoding. This method must be used with ASCII encoding column.", () => DataUtils.GetBetweenEndExpressionForQueryUsingLikeWithFilteredIndex("10Z", nvarcharColumn));
		}

		public void TestSetServerConfigOption()
		{
			using (var adminCnx = Db.NewAdminConnection())
			{
				DataUtils.SetServerConfigOption(adminCnx, "clr enabled", "1");
				AssertServerConfigurationValue("clr enabled", "1");
				DataUtils.SetServerConfigOption(adminCnx, "max text repl size (B)", "-1");
				AssertServerConfigurationValue("max text repl size (B)", "-1");
			}
		}

		void AssertServerConfigurationValue(string name, string expectedValue)
		{
			string sqlText = string.Format("SELECT value FROM sys.configurations WHERE name = '{0}'", name);
			string actualValue = Db.Connection.ExecuteScalar(sqlText).ToString();
			AssertEquals("Set value", expectedValue, actualValue);

			sqlText = string.Format("SELECT value_in_use FROM sys.configurations WHERE name = '{0}'", name);
			string actualRunningValue = Db.Connection.ExecuteScalar(sqlText).ToString();
			AssertEquals("Running value", expectedValue, actualRunningValue);
		}

		public void TestSetCompatibilityLevelBasedOnServerVersion()
		{
			const string testDb = "#TestSetCompatibilityLevelBasedOnServerVersion-DB#";

			using (var adminConnection = Db.NewAdminConnection())
			{
				AssertEquals("adminConnection.ServerVersionNumber.CompatibilityLevel > 100", true, adminConnection.ServerVersionNumber.CompatibilityLevel > 100);
				AdoTestUtils.CreateDbDropExisting(adminConnection, testDb);

				try
				{
					var sqlText = String.Format(CultureInfo.InvariantCulture, "ALTER DATABASE [{0}] SET COMPATIBILITY_LEVEL = 100;", testDb);
					adminConnection.ExecuteNonQuery(sqlText);
					AssertEquals("compatibility_level",
						100,
						GetDbCompatibilityLevel(adminConnection, testDb));

					DataUtils.SetCompatibilityLevelBasedOnServerVersion(adminConnection, testDb);
					AssertEquals("compatibility_level",
						adminConnection.ServerVersionNumber.CompatibilityLevel,
						GetDbCompatibilityLevel(adminConnection, testDb));
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(adminConnection, testDb);
				}
			}
		}

		int GetDbCompatibilityLevel(DbConnection connection, string dbName)
		{
			string sqlText = String.Format("SELECT [compatibility_level] FROM sys.databases WHERE name = '{0}'", dbName);
			return Convert.ToInt32(connection.ExecuteScalar(sqlText));
		}

		public void TestIsWiseTechGlobalInternalComputer()
		{
			AssertEquals(string.Format("Is [{0}] an internal WTG server?", Db.Connection.ServerNameReportedByDatabase), true, DataUtils.IsWiseTechGlobalInternalComputer(Db.Connection));
		}

		public void TestIsCmrMsgTestServer()
		{
			AssertEquals("Is CmrMsgTestServer?", false, DataUtils.IsCmrMsgTestServer(Db.ServerName));
		}

		public void TestWtgDomainRegex()
		{
			AssertEquals("WG domain regex matches [wg.cargowise.com]?", true, DataUtils.WtgDomainRegex.IsMatch("wg.cargowise.com"));
			AssertEquals("WG domain regex matches [something.wg.cargowise.com]?", true, DataUtils.WtgDomainRegex.IsMatch("something.wg.cargowise.com"));
			AssertEquals("WG domain regex matches [other.cargowise.com]?", false, DataUtils.WtgDomainRegex.IsMatch("other.cargowise.com"));
			AssertEquals("WG domain regex matches [wg.cargowise.com.au]?", false, DataUtils.WtgDomainRegex.IsMatch("wg.cargowise.com.au"));

			AssertEquals("WG domain regex matches [wg.corporate.cargowise.com]?", true, DataUtils.WtgDomainRegex.IsMatch("wg.corporate.cargowise.com"));
			AssertEquals("WG domain regex matches [something.wg.corporate.cargowise.com]?", true, DataUtils.WtgDomainRegex.IsMatch("something.wg.corporate.cargowise.com"));
			AssertEquals("WG domain regex matches [other.corporate.cargowise.com]?", true, DataUtils.WtgDomainRegex.IsMatch("other.corporate.cargowise.com"));
			AssertEquals("WG domain regex matches [wg.corporate.cargowise.com.au]?", false, DataUtils.WtgDomainRegex.IsMatch("wg.corporate.cargowise.com.au"));

			AssertEquals("WG domain regex matches [wg.wisetechglobal.com]?", true, DataUtils.WtgDomainRegex.IsMatch("wg.wisetechglobal.com"));
			AssertEquals("WG domain regex matches [something.wg.wisetechglobal.com]?", true, DataUtils.WtgDomainRegex.IsMatch("something.wg.wisetechglobal.com"));
			AssertEquals("WG domain regex matches [other.wisetechglobal.com]?", false, DataUtils.WtgDomainRegex.IsMatch("other.wisetechglobal.com"));
			AssertEquals("WG domain regex matches [wg.wisetechglobal.com.au]?", false, DataUtils.WtgDomainRegex.IsMatch("wg.wisetechglobal.com.au"));

			AssertEquals("WG domain regex matches [wg.corporate.wisetechglobal.com]?", true, DataUtils.WtgDomainRegex.IsMatch("wg.corporate.wisetechglobal.com"));
			AssertEquals("WG domain regex matches [something.wg.corporate.wisetechglobal.com]?", true, DataUtils.WtgDomainRegex.IsMatch("something.wg.corporate.wisetechglobal.com"));
			AssertEquals("WG domain regex matches [other.corporate.wisetechglobal.com]?", true, DataUtils.WtgDomainRegex.IsMatch("other.corporate.wisetechglobal.com"));
			AssertEquals("WG domain regex matches [wg.corporate.wisetechglobal.com.au]?", false, DataUtils.WtgDomainRegex.IsMatch("wg.corporate.wisetechglobal.com.au"));

			AssertEquals("WG domain regex matches [wisecloud.zone]?", true, DataUtils.WtgDomainRegex.IsMatch("wisecloud.zone"));
			AssertEquals("WG domain regex matches [something.wisecloud.zone]?", true, DataUtils.WtgDomainRegex.IsMatch("something.wisecloud.zone"));
			AssertEquals("WG domain regex matches [other.zone]?", false, DataUtils.WtgDomainRegex.IsMatch("other.zone"));
			AssertEquals("WG domain regex matches [wisecloud.zone.au]?", false, DataUtils.WtgDomainRegex.IsMatch("wisecloud.zone.au"));

			AssertEquals("WG domain regex matches [test.wisecloud.zone]?", true, DataUtils.WtgDomainRegex.IsMatch("test.wisecloud.zone"));
			AssertEquals("WG domain regex matches [something.test.wisecloud.zone]?", true, DataUtils.WtgDomainRegex.IsMatch("something.test.wisecloud.zone"));
			AssertEquals("WG domain regex matches [other.wisecloud.zone]?", true, DataUtils.WtgDomainRegex.IsMatch("other.wisecloud.zone"));
			AssertEquals("WG domain regex matches [test.wisecloud.zone.au]?", false, DataUtils.WtgDomainRegex.IsMatch("test.wisecloud.zone.au"));

			AssertEquals("WG domain regex matches [wtg.zone]?", true, DataUtils.WtgDomainRegex.IsMatch("wtg.zone"));
			AssertEquals("WG domain regex matches [something.wtg.zone]?", true, DataUtils.WtgDomainRegex.IsMatch("something.wtg.zone"));
			AssertEquals("WG domain regex matches [wtg.zone.au]?", false, DataUtils.WtgDomainRegex.IsMatch("wtg.zone.au"));

			AssertEquals("WG domain regex matches [test.wtg.zone]?", true, DataUtils.WtgDomainRegex.IsMatch("test.wtg.zone"));
			AssertEquals("WG domain regex matches [something.test.wtg.zone]?", true, DataUtils.WtgDomainRegex.IsMatch("something.test.wtg.zone"));
			AssertEquals("WG domain regex matches [test.wtg.zone.au]?", false, DataUtils.WtgDomainRegex.IsMatch("test.wtg.zone.au"));
		}

		public void TestIsDbNameAlphaNumeric()
		{
			AssertEquals("IsDbNameAlphaNumeric []?", false, DataUtils.IsDbNameAlphaNumeric(" "));
			AssertEquals("IsDbNameAlphaNumeric [Abc]?", true, DataUtils.IsDbNameAlphaNumeric("Abc"));
			AssertEquals("IsDbNameAlphaNumeric [Abc_Test]?", false, DataUtils.IsDbNameAlphaNumeric("Abc_Test"));
			AssertEquals("IsDbNameAlphaNumeric [Abc123]?", true, DataUtils.IsDbNameAlphaNumeric("Abc123"));
			AssertEquals("IsDbNameAlphaNumeric [Abc-Test]?", false, DataUtils.IsDbNameAlphaNumeric("Abc-Test"));
			AssertEquals("Is [123Abc] alphanumeric and starts with a letter?", false, DataUtils.IsDbNameAlphaNumeric("123Abc"));
		}

		public void TestValidateMainDatabaseName()
		{
			AssertExceptionThrown(typeof(OdysseyDataException), "Main database name must not be empty.", () => DataUtils.ValidateMainDatabaseName(null));
			AssertExceptionThrown(typeof(OdysseyDataException), "Main database name must not be empty.", () => DataUtils.ValidateMainDatabaseName(""));
			AssertExceptionThrown(typeof(OdysseyDataException), "Main database name must not be empty.", () => DataUtils.ValidateMainDatabaseName("   "));

			AssertExceptionThrown(
				typeof(OdysseyDataException),
				"Main database must not be longer than 35 characters.\r\n\r\nDatabase: 1234567890123456789012345678901234567890",
				() => DataUtils.ValidateMainDatabaseName("1234567890123456789012345678901234567890"));

			AssertValidateMainDatabaseNameThrowsException("1A");
			AssertValidateMainDatabaseNameThrowsException("A_B");
			AssertValidateMainDatabaseNameThrowsException("A-B");
			AssertValidateMainDatabaseNameThrowsException("A#");

			AssertNoExceptionThrown("AB12 should be a valid main database name.", () => DataUtils.ValidateMainDatabaseName("AB12"));
		}

		void AssertValidateMainDatabaseNameThrowsException(string dbName)
		{
			AssertExceptionThrown(
				typeof(OdysseyDataException),
				String.Format(CultureInfo.InvariantCulture,
					"Main database name must only contain alphanumeric characters, starting with a letter.\r\n\r\nDatabase: {0}",
					dbName),
				() => DataUtils.ValidateMainDatabaseName(dbName));
		}

		[UseSnapshotProtection]
		public void TestObjectExists_Locking()
		{
			var tableName = "_test_" + Guid.NewGuid().ToString().Replace("-", "");

			using (var connection = Db.NewAdminConnection())
			{
				AssertEquals(false, DataUtils.ObjectExists(connection, tableName));

				connection.ExecuteNonQuery($"CREATE TABLE [dbo].[{tableName}] (id int); ");

				AssertEquals(true, DataUtils.ObjectExists(connection, tableName));

				using (connection.TemporarySetLockTimeout(TimeSpan.FromMilliseconds(100)))
				using (var adminConnection = Db.NewAdminConnection())
				{
					adminConnection.ExecuteNonQuery($@"BEGIN TRAN ALTER TABLE [dbo].[{tableName}] ADD new_col bit NULL");

					try
					{
						AssertEquals(true, DataUtils.ObjectExists(connection, tableName));
						Fail("Lock Timeout exception should throw");
					}
					catch (SqlException ex)
					{
						AssertEquals(DbErrorType.LockTimeoutExpired, new DbErrorMatch(ex).ExceptionType);
					}

					AssertEquals(true, DataUtils.ObjectExistsNolock(connection, "dbo", tableName));
				}
			}
		}

		public void TestStripCommentsFromSql()
		{
			var sql = @"SELECT last_name, salary + NVL(commission_pct, 0), 
   job_id, e.department_id
/* Select all employees whose compensation is
greater than that of Pataballa.*/
  FROM employees e, departments d
       /*The DEPARTMENTS table is used to get the department name.*/
  WHERE e.department_id = d.department_id
    AND salary + NVL(commission_pct,0) >   /* Subquery:       */
   (SELECT salary + NVL(commission_pct,0)
                 /* total compensation is salar + commission_pct */
      FROM employees 
      WHERE last_name = 'Pataballa');

SELECT last_name,                    -- select the name
    salary + NVL(commission_pct, 0),-- total compensation
    job_id,                         -- job
    e.department_id                 -- and department
  FROM employees e,                 -- of all employees
       departments d
  WHERE e.department_id = d.department_id
    AND salary + NVL(commission_pct, 0) >  -- whose compensation 
                                           -- is greater than
      (SELECT salary + NVL(commission_pct,0)  -- the compensation
    FROM employees 
    WHERE last_name = 'Pataballa')        -- of Pataballa.
;";
			var noCommentsql = @"SELECT last_name, salary + NVL(commission_pct, 0),
   job_id, e.department_id

  FROM employees e, departments d

  WHERE e.department_id = d.department_id
    AND salary + NVL(commission_pct,0) >
   (SELECT salary + NVL(commission_pct,0)

      FROM employees
      WHERE last_name = 'Pataballa');

SELECT last_name,
    salary + NVL(commission_pct, 0),
    job_id,
    e.department_id
  FROM employees e,
       departments d
  WHERE e.department_id = d.department_id
    AND salary + NVL(commission_pct, 0) >

      (SELECT salary + NVL(commission_pct,0)
    FROM employees
    WHERE last_name = 'Pataballa')
;";
			AssertMultilineASCIIEquals("Should be same", noCommentsql, DataUtils.StripCommentsFromSql(sql));

			sql = @"select /* block comment */ top 1 'a' /* block comment /* nested block comment */*/ from  sys.tables --LineComment
union
select top 1 '/* literal with */-- lots of comments symbols' from sys.tables --FinalLineComment";
			noCommentsql = @"select  top 1 'a'  from  sys.tables
union
select top 1 '/* literal with */-- lots of comments symbols' from sys.tables";
			AssertMultilineASCIIEquals("Should be same", noCommentsql, DataUtils.StripCommentsFromSql(sql));

			sql = @"create table [/*] /* 
  -- huh? */
(
    ""--
     --"" integer identity, -- /*
    [*/] varchar(20) /* -- */
         default '*/ /* -- */' /* /* /* */ */ */
);
            go";
			noCommentsql = @"create table [/*]
(
    ""--
     --"" integer identity,
    [*/] varchar(20)
         default '*/ /* -- */'
);
            go";
			AssertMultilineASCIIEquals("Should be same", noCommentsql, DataUtils.StripCommentsFromSql(sql));
		}

		public void TestGetDataSetFromQuery_CommandText()
		{
			var dataSet = DataUtils.GetDataSetFromQuery("SELECT GS_PK, GS_Code, GS_IsActive From dbo.GlbStaff");
			AssertEquals("Should be one table in the dataset", 1, dataSet.Tables.Count);
			var table = dataSet.Tables[0];
			AssertEquals("Should be no rows in the table", 0, table.Rows.Count);
			AssertEquals("Should fill columns", 3, table.Columns.Count);

			AssertEquals("GS_PK", table.Columns[0].ColumnName);
			AssertEquals(typeof(Guid), table.Columns[0].DataType);

			AssertEquals("GS_Code", table.Columns[1].ColumnName);
			AssertEquals(typeof(string), table.Columns[1].DataType);

			AssertEquals("GS_IsActive", table.Columns[2].ColumnName);
			AssertEquals(typeof(bool), table.Columns[2].DataType);
		}

		[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
		public void TestGetDataSetFromQuery_SqlCommand()
		{
			var sqlConnection = ((IDbConnectionInternals)Db.Connection).ADOConnection;
			using (var command = sqlConnection.CreateCommand()) // Needed for testing the method
			{
				command.CommandText = "SELECT GS_PK, GS_Code, GS_IsActive From dbo.GlbStaff";
				var dataSet = DataUtils.GetDataSetFromQuery(command);
				AssertEquals("Should be one table in the dataset", 1, dataSet.Tables.Count);
				var table = dataSet.Tables[0];
				AssertEquals("Should be no rows in the table", 0, table.Rows.Count);
				AssertEquals("Should fill columns", 3, table.Columns.Count);

				AssertEquals("GS_PK", table.Columns[0].ColumnName);
				AssertEquals(typeof(Guid), table.Columns[0].DataType);

				AssertEquals("GS_Code", table.Columns[1].ColumnName);
				AssertEquals(typeof(string), table.Columns[1].DataType);

				AssertEquals("GS_IsActive", table.Columns[2].ColumnName);
				AssertEquals(typeof(bool), table.Columns[2].DataType);
			}
		}

		public void TestGetDataSetFromQuery_DbCommand()
		{
			using (var command = Db.Connection.Command("SELECT GS_PK, GS_Code, GS_IsActive From dbo.GlbStaff"))
			{
				var dataSet = DataUtils.GetDataSetFromQuery(command);
				AssertEquals("Should be one table in the dataset", 1, dataSet.Tables.Count);
				var table = dataSet.Tables[0];
				AssertEquals("Should be no rows in the table", 0, table.Rows.Count);
				AssertEquals("Should fill columns", 3, table.Columns.Count);

				AssertEquals("GS_PK", table.Columns[0].ColumnName);
				AssertEquals(typeof(Guid), table.Columns[0].DataType);

				AssertEquals("GS_Code", table.Columns[1].ColumnName);
				AssertEquals(typeof(string), table.Columns[1].DataType);

				AssertEquals("GS_IsActive", table.Columns[2].ColumnName);
				AssertEquals(typeof(bool), table.Columns[2].DataType);
			}
		}

		public void TestSetTrustworthyOn()
		{
			const string testDb = "#TestTrustworthyDB#";

			using (var adminConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.CreateDbDropExisting(adminConnection, testDb);
				AssertEquals("Database TRUSTWORTHY", false, GetTrustworthyDatabaseProperty(adminConnection, testDb));
				try
				{
					DataUtils.SetTrustworthyOn(adminConnection, testDb);
					AssertEquals("Database TRUSTWORTHY", true, GetTrustworthyDatabaseProperty(adminConnection, testDb));
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(adminConnection, testDb);
				}
			}
		}

		public void TestEnsureClrEnabledAndTrustworthyOn_CouldNotFindStoredProcedure()
		{
			const string testDb = "#TestTrustworthyDB#";

			using (var adminConnection = Db.NewAdminConnection())
			using (AdoTestUtils.CreateDbDropExistingDisposable(adminConnection, testDb))
			{
				AssertEquals("Database TRUSTWORTHY", false, GetTrustworthyDatabaseProperty(adminConnection, testDb));
				try
				{
					DataUtils.SetServerConfigOption(adminConnection, "clr enabled", "0");

					// Act
					DataUtils.EnsureClrEnabledAndTrustworthyOn(adminConnection, testDb);

					// Assert
					AssertEquals("Database TRUSTWORTHY", true, GetTrustworthyDatabaseProperty(adminConnection, testDb));
				}
				finally
				{
					DataUtils.SetServerConfigOption(adminConnection, "clr enabled", "1");
				}
			}
		}

		public void TestEnsureClrEnabledAndTrustworthyOn_ClrDbOptionDisabled()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				try
				{
					DataUtils.SetServerConfigOption(adminConnection, "clr enabled", "0");
					DataUtils.SetDbPropertyImmediately(adminConnection, Db.DatabaseName, "is_trustworthy_on = 1", "TRUSTWORTHY OFF");

					// Act
					DataUtils.EnsureClrEnabledAndTrustworthyOn(adminConnection, Db.DatabaseName);

					// Assert
					AssertEquals("Database TRUSTWORTHY", true, GetTrustworthyDatabaseProperty(adminConnection, Db.DatabaseName));
				}
				finally
				{
					DataUtils.SetServerConfigOption(adminConnection, "clr enabled", "1");
					DataUtils.SetDbPropertyImmediately(adminConnection, Db.DatabaseName, "is_trustworthy_on = 0", "TRUSTWORTHY ON");
				}
			}
		}

		public void TestEnsureClrEnabledAndTrustworthyOn_ErrorLoadingUntrustedAssembly()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				try
				{
					DataUtils.SetDbPropertyImmediately(adminConnection, Db.DatabaseName, "is_trustworthy_on = 1", "TRUSTWORTHY OFF");

					// Act
					DataUtils.EnsureClrEnabledAndTrustworthyOn(adminConnection, Db.DatabaseName);

					// Assert
					AssertEquals("Database TRUSTWORTHY", true, GetTrustworthyDatabaseProperty(adminConnection, Db.DatabaseName));
				}
				finally
				{
					DataUtils.SetDbPropertyImmediately(adminConnection, Db.DatabaseName, "is_trustworthy_on = 0", "TRUSTWORTHY ON");
				}
			}
		}

		bool GetTrustworthyDatabaseProperty(AdminConnection adminConnection, string testDb)
		{
			var sqlScript = $"SELECT is_trustworthy_on FROM sys.databases WHERE name = '{testDb}'";
			return (bool)adminConnection.ExecuteScalar(sqlScript);
		}

		public void TestBuildUniqueKeyListForCurrentSchema()
		{
			var indexes = DataUtils.BuildUniqueSingleKeyList();

			CombineAssertions(
				() =>
				{
					AssertCollectionContains("[AB_Code] Contains column in a single key index?", "ACCBANKACCOUNT.AB_CODE", indexes);
					AssertCollectionNotContains("[AB_AccountNum] Contains column in a composite key index?", "ACCBANKACCOUNT.AB_ACCOUNTNUM", indexes);
					AssertCollectionContains("[IC_Code] Contains column in a single key index in Ref DB?", "REFDBCMRAU_CMRINSTRUMENTCATEGORY.IC_CODE", indexes);
				}
			);
		}

		public void TestBytesToHexString()
		{
			// Arrange
			var bytesArrays = new[]
			{
				null,
				new byte[] { 0 },
				new byte[] { 1 },
				new byte[] { 2 },
				new byte[] { 3 },
				new byte[] { 4 },
				new byte[] { 5 },
				new byte[] { 6 },
				new byte[] { 7 },
				new byte[] { 8 },
				new byte[] { 9 },
				new byte[] { 10 },
				new byte[] { 11 },
				new byte[] { 12 },
				new byte[] { 13 },
				new byte[] { 14 },
				new byte[] { 15 },
				new byte[] { 255 },
				new byte[] { 100 },
				new byte[] { 50 },
				new byte[] { 10, 9, 8, 7, 11, 12, 6, 0, 254, 255, 200, 100, 14, 15, 16, 21, 33, 45, 66, 78, 81, 92, 151 },
			};

			var expextedHexStrings = new[]
			{
				null,
				"0x00",
				"0x01",
				"0x02",
				"0x03",
				"0x04",
				"0x05",
				"0x06",
				"0x07",
				"0x08",
				"0x09",
				"0x0a",
				"0x0b",
				"0x0c",
				"0x0d",
				"0x0e",
				"0x0f",
				"0xff",
				"0x64",
				"0x32",
				"0x0a0908070b0c0600feffc8640e0f1015212d424e515c97",
			};

			// Act/Assert
			for (var i = 0; i < bytesArrays.Length; i++)
			{
				AssertEquals(expextedHexStrings[i], DataUtils.BytesToHexString(bytesArrays[i]));
			}
		}

		public void TestLockTimeoutDetectionStrategy()
		{
			var strategy = new DataUtils.LockTimeoutDetectionStrategy();

			AssertEquals(false, strategy.IsTransient(null));
			AssertEquals(false, strategy.IsTransient(new Exception("Not a SqlException")));
			AssertEquals(false, strategy.IsTransient(SqlExceptionBuilder.CreateSqlException(8134, "Divide by zero error encountered.")));

			AssertEquals(true, strategy.IsTransient(SqlExceptionBuilder.CreateSqlException(1222, "Lock request timeout exceeded")));
		}

		public void TestLockTimeoutRetryPolicy()
		{
			var retryPolicy = DataUtils.LockTimeoutRetryPolicy;

			AssertType<DataUtils.LockTimeoutDetectionStrategy>(retryPolicy.ErrorDetectionStrategy);
			AssertType<ExponentialBackoff>(retryPolicy.RetryStrategy);
		}

		public void TestGetApproximateRowCountForTableReturnsCorrectType()
		{
			// Since the sql query in GetApproximateRowCountForTable's response and db tables can exceed the 32bit integer limit, using long (or any signed 64bit number) is much more reasonable.
			// Sql queries can output numbers up to 64bit signed integer limit so enforcing and ensuring the response is a long means there is no overflow issues
			// If the return type is changed to an int or a 32bit integer and the query retuens a result that goes over the 32bit integer limit, the returned value will be inacurate and will also have the overflow value.

			var type = typeof(DataUtils).GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).FirstOrDefault(x => x.Name == nameof(DataUtils.GetApproximateRowCountForTable)).ReturnType;

			AssertEquals("GetApproximateRowCountForTable has the correct return type", typeof(long), type);
		}
	}

	public class TransactionDataUtilsTest : TransactionedTestCase
	{
		public void TestGetApproximateRowCountForTableReturnsCorrectRowCount()
		{
			var dbConnection = Db.Connection;
			var testTableName = "GetApproximateRowCountForTableTestTable";

			dbConnection.ExecuteNonQuery(@$"CREATE TABLE [{testTableName}] ([TEST_PK] UNIQUEIDENTIFIER NOT NULL);

											INSERT INTO [{testTableName}] ([TEST_PK])
											VALUES	(NEWID()),
													(NEWID()),
													(NEWID()),
													(NEWID()),
													(NEWID()),
													(NEWID()),
													(NEWID()),
													(NEWID()),
													(NEWID()),
													(NEWID());");

			var rowCount = DataUtils.GetApproximateRowCountForTable(dbConnection, testTableName);

			dbConnection.ExecuteNonQuery(@$"DROP TABLE [{testTableName}];");

			AssertEquals("GetApproximateRowCountForTableTestTable returns correct amount of rows", 10, rowCount);
		}

		public void TestGetApproximateRowCountForTableOverloadReturnsCorrectRowCount()
		{
			var dbConnection = Db.Connection;
			var testTableName = "GetApproximateRowCountForTableTestTable";

			var tableSchema = new Mock<ITableSchema>();
			tableSchema.Setup(x => x.SqlSchemaName).Returns("dbo");
			tableSchema.Setup(x => x.TableName).Returns(testTableName);

			dbConnection.ExecuteNonQuery(@$"CREATE TABLE [{testTableName}] ([TEST_PK] UNIQUEIDENTIFIER NOT NULL);

											INSERT INTO [{testTableName}] ([TEST_PK])
											VALUES	(NEWID()),
													(NEWID()),
													(NEWID()),
													(NEWID()),
													(NEWID()),
													(NEWID()),
													(NEWID()),
													(NEWID()),
													(NEWID()),
													(NEWID());");

			var rowCount = DataUtils.GetApproximateRowCountForTable(dbConnection, tableSchema.Object);

			dbConnection.ExecuteNonQuery(@$"DROP TABLE [{testTableName}];");

			AssertEquals("GetApproximateRowCountForTableTestTable returns correct amount of rows", 10, rowCount);
		}
	}
}
