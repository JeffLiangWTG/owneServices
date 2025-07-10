using System;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using NUnit.Framework;

namespace CargoWise.Bi.Maintenance
{
	public class LinkedServerCreatorTest : TestCase
	{
		public void TestServerPortNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals(LinkedServerCreator.GetMainDBServerPortNumber(), LinkedServerCreator.GetServerPortNumber("XB268"));
				AssertEquals(LinkedServerCreator.GetMainDBServerPortNumber(), LinkedServerCreator.GetServerPortNumber("XB268,"));
				AssertEquals(LinkedServerCreator.GetMainDBServerPortNumber(), LinkedServerCreator.GetServerPortNumber("XB268, "));
				AssertEquals("2525", LinkedServerCreator.GetServerPortNumber("XB268,2525"));
			});
		}

		public void TestServerNameWithoutPortNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("XB268", LinkedServerCreator.GetServerNameWithoutPortNumber("XB268"));
				AssertEquals("XB268", LinkedServerCreator.GetServerNameWithoutPortNumber("XB268,"));
				AssertEquals("XB268", LinkedServerCreator.GetServerNameWithoutPortNumber("XB268, "));
				AssertEquals("XB268", LinkedServerCreator.GetServerNameWithoutPortNumber("XB268,2525"));

				AssertEquals("XB268", LinkedServerCreator.GetServerNameWithoutPortNumber("XB268,2525"));
				AssertEquals("XB268\\", LinkedServerCreator.GetServerNameWithoutPortNumber("XB268\\,2525"));
				AssertEquals("XB268\\ ", LinkedServerCreator.GetServerNameWithoutPortNumber("XB268\\ ,2525"));
				AssertEquals("XB268\\Instance", LinkedServerCreator.GetServerNameWithoutPortNumber("XB268\\Instance,2525"));
			});
		}

		public void TestLinkedServerNames()
		{
			var serverName = "LOCALHOST";
			var serverNameWithDefaultPort = serverName + ",1433";
			var serverNameWithNonDefaultPort = serverName + LinkedServerCreator.Comma + LinkedServerCreator.GetMainDBServerPortNumber();
			var logger = new LoggerForTest();

			using (var connection = Db.NewAdminConnection())
			{
				var linkedServerCreator = new LinkedServerCreator(connection, serverName, logger);
				var serverNameWithPort = serverName + LinkedServerCreator.Comma + LinkedServerCreator.GetMainDBServerPortNumber();
				CombineAssertions($"Constructed with {serverName}", () =>
				{
					AssertEquals("Constructor", serverName, linkedServerCreator.LinkedServerName);
					AssertEquals("Without Port", serverName, linkedServerCreator.LinkedServerNameWithoutPortNumber);
					AssertEquals("With Port", serverNameWithPort, linkedServerCreator.LinkedServerNameWithPortNumber);
				});
			}

			using (var connection = Db.NewAdminConnection())
			{
				var linkedServerCreator = new LinkedServerCreator(connection, serverNameWithNonDefaultPort, logger);

				CombineAssertions($"Constructed with {serverNameWithNonDefaultPort}", () =>
				{
					AssertEquals("Constructor", serverNameWithNonDefaultPort, linkedServerCreator.LinkedServerName);
					AssertEquals("Without Port", serverName, linkedServerCreator.LinkedServerNameWithoutPortNumber);
					AssertEquals("With Port", serverNameWithNonDefaultPort, linkedServerCreator.LinkedServerNameWithPortNumber);
				});
			}

			using (var connection = Db.NewAdminConnection())
			{
				var linkedServerCreator = new LinkedServerCreator(connection, serverNameWithDefaultPort, logger);

				CombineAssertions($"Constructed with {serverNameWithDefaultPort}", () =>
				{
					AssertEquals("Constructor", serverNameWithDefaultPort, linkedServerCreator.LinkedServerName);
					AssertEquals("Without Port", serverName, linkedServerCreator.LinkedServerNameWithoutPortNumber);
					AssertEquals("With Port", serverNameWithDefaultPort, linkedServerCreator.LinkedServerNameWithPortNumber);
				});
			}
		}

		public void TestDropLinkedServerWillNotDropLocalServer()
		{
			// Arrange
			using (var connection = Db.NewAdminConnection())
			{
				var portNumber = LinkedServerCreator.GetMainDBServerPortNumber();
				var testLinkedServer = "LOCALHOST," + portNumber;
				var logger = new LoggerForTest();
				var linkedServerCreator = new LinkedServerCreator(connection, testLinkedServer, logger);

				// Act
				linkedServerCreator.DropLinkedServer();

				// Assert
				AssertEquals(
					$"Cannot drop local server {connection.ServerName}",
					true,
					connection.Exists("FROM sys.servers WHERE IS_LINKED = 0 AND NAME = @@ServerName"));
			}
		}

		public void TestCanLogWithEitherTypeOfLogger()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var portNumber = LinkedServerCreator.GetMainDBServerPortNumber();
				var testLinkedServer = "LOCALHOST," + portNumber;
				var logger = new LoggerForTest();
				var linkedServerCreator = new LinkedServerCreator(connection, testLinkedServer, logger);

				CombineAssertions(() =>
				{
					AssertEquals($"ILogger should contain 0 messages but was:\n{string.Join("\n", logger.LogEntries)}", 0, logger.LogEntries.Count());
					linkedServerCreator.LogInfo("ILogger works");
					AssertEquals($"ILogger should contain 1 message but was:\n{string.Join("\n", logger.LogEntries)}", 1, logger.LogEntries.Count());
					Assert("UpgradeTaskWorkflowLogger does not exist", !(linkedServerCreator.upgradeTaskWorkflowLogger is IUpgradeTaskWorkflowLogger));
				});
			}

			using (var connection = Db.NewAdminConnection())
			{
				var portNumber = LinkedServerCreator.GetMainDBServerPortNumber();
				var testLinkedServer = "LOCALHOST," + portNumber;
				var upgradeTaskWorkflowLogger = new UpgradeTaskWorkflowLoggerForTest();
				var linkedServerCreator = new LinkedServerCreator(connection, testLinkedServer, upgradeTaskWorkflowLogger);

				CombineAssertions(() =>
				{
					AssertEquals($"UpgradeTaskWorkflowLogger should contain 0 messages but was:\n{string.Join("\n", upgradeTaskWorkflowLogger.LogEntries)}", 0, upgradeTaskWorkflowLogger.LogEntries.Count);
					linkedServerCreator.LogInfo("IUpgradeTaskWorkflowLogger works");
					AssertEquals($"UpgradeTaskWorkflowLogger should contain 1 message but was:\n{string.Join("\n", upgradeTaskWorkflowLogger.LogEntries)}", 1, upgradeTaskWorkflowLogger.LogEntries.Count);
					Assert("Logger does not exist", !(linkedServerCreator.logger is ILogger));
				});
			}
		}

		public void TestCreateLinkedServer_UserCanAccessServer()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var portNumber = LinkedServerCreator.GetMainDBServerPortNumber();
				var testLinkedServer = "LOCALHOST," + portNumber;
				var logger = new LoggerForTest();
				var linkedServerCreator = new LinkedServerCreatorForTest(connection, testLinkedServer, logger);
				linkedServerCreator.DropLinkedServer();
				AssertLinkedServerExists(connection, testLinkedServer, expected: false);

				try
				{
					((IDbLoginRepair)connection).EnsureUnrestrictedWriterDbLogin();
					var registration = ObjectFactory.Get<IProductRegistration>();
					registration.KeyForTest.IsInternalSystemForTest = false;

					linkedServerCreator.CreateLinkedServer();

					CombineAssertions(() =>
					{
						AssertLinkedServerExists(connection, testLinkedServer, expected: true);
					});
				}
				finally
				{
					linkedServerCreator.DropLinkedServer();
				}
			}
		}

		public void TestGetCreateLinkedServerWithMsOleDBQueryForInternalSystems()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var registration = ObjectFactory.Get<IProductRegistration>();
				registration.KeyForTest.EnterpriseCodeForTest = "WTL";

				var portNumber = LinkedServerCreator.GetMainDBServerPortNumber();
				var testLinkedServer = "LOCALHOST," + portNumber;
				var logger = new LoggerForTest();
				var linkedServerCreator = new LinkedServerCreatorForTest(connection, testLinkedServer, logger);

				var query = $@"
			IF NOT EXISTS (SELECT NULL FROM sys.servers WHERE name = @ServerName)
			BEGIN
							EXEC master.dbo.sp_MSset_oledb_prop N'MSOLEDBSQL', N'AllowInProcess', 1;
							EXEC dbo.sp_addlinkedserver @server = @ServerName, @srvproduct=N'', @provider=N'MSOLEDBSQL', @datasrc = @ServerNameWithPortNumber;
			END";

				var ensureServerOptionsQuery = @"
                                                EXEC dbo.sp_addlinkedsrvlogin @rmtsrvname = @ServerName, @locallogin = NULL , @useself = N'True', @rmtuser = N'';
                                                EXEC dbo.sp_serveroption @server = @ServerName, @optname = N'data access', @optvalue = N'True';
                                                EXEC dbo.sp_serveroption @server = @ServerName, @optname = N'remote proc transaction promotion', @optvalue = N'False';
                                                EXEC dbo.sp_serveroption @server = @ServerName, @optname = N'rpc', @optvalue = N'True'
                                                EXEC dbo.sp_serveroption @server = @ServerName, @optname = N'rpc out', @optvalue = N'True'";

				var expectedQuery = string.Concat(query, System.Environment.NewLine, ensureServerOptionsQuery);
				AssertEquals(expectedQuery, linkedServerCreator.GetCreateLinkedServerWithMsOleDBQuery());

				registration = ObjectFactory.Get<IProductRegistration>();
				registration.KeyForTest.EnterpriseCodeForTest = "EDI";
				linkedServerCreator = new LinkedServerCreatorForTest(connection, testLinkedServer, logger);

				query = $@"
			IF NOT EXISTS (SELECT NULL FROM sys.servers WHERE name = @ServerName)
			BEGIN
							EXEC master.dbo.sp_MSset_oledb_prop N'MSOLEDBSQL', N'AllowInProcess', 1;
							EXEC dbo.sp_addlinkedserver @server = @ServerName, @srvproduct=N'', @provider=N'MSOLEDBSQL', @provstr = @ProviderString, @datasrc = @ServerNameWithPortNumber;
			END";

				expectedQuery = string.Concat(query, System.Environment.NewLine, ensureServerOptionsQuery);
				AssertEquals(expectedQuery, linkedServerCreator.GetCreateLinkedServerWithMsOleDBQuery());

				registration = ObjectFactory.Get<IProductRegistration>();
				registration.KeyForTest.EnterpriseCodeForTest = "PRD";
				linkedServerCreator = new LinkedServerCreatorForTest(connection, testLinkedServer, logger);

				query = $@"
			IF NOT EXISTS (SELECT NULL FROM sys.servers WHERE name = @ServerName)
			BEGIN
							EXEC master.dbo.sp_MSset_oledb_prop N'MSOLEDBSQL', N'AllowInProcess', 1;
							EXEC dbo.sp_addlinkedserver @server = @ServerName, @srvproduct=N'', @provider=N'MSOLEDBSQL', @provstr = @ProviderString, @datasrc = @ServerNameWithPortNumber;
			END";

				expectedQuery = string.Concat(query, System.Environment.NewLine, ensureServerOptionsQuery);
				AssertEquals(expectedQuery, linkedServerCreator.GetCreateLinkedServerWithMsOleDBQuery());
			}
		}

		public void TestGetCreateLinkedServerWithNativeClientQueryForInternalSystems()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var registration = ObjectFactory.Get<IProductRegistration>();
				registration.KeyForTest.IsInternalSystemForTest = true;
				registration.KeyForTest.EnterpriseCodeForTest = "WTL";
				var portNumber = LinkedServerCreator.GetMainDBServerPortNumber();
				var testLinkedServer = "LOCALHOST," + portNumber;
				var logger = new LoggerForTest();
				var linkedServerCreator = new LinkedServerCreatorForTest(connection, testLinkedServer, logger);

				var query = $@"
			IF NOT EXISTS (SELECT NULL FROM sys.servers WHERE name = @ServerName)
			BEGIN
							EXEC dbo.sp_addlinkedserver @server = @ServerName, @srvproduct=N'', @provider=N'SQLNCLI', @datasrc = @ServerNameWithPortNumber;
							EXEC dbo.sp_serveroption @server = @ServerName, @optname = N'connect timeout', @optvalue = N'60';
			END";

				var ensureServerOptionsQuery = @"
                                                EXEC dbo.sp_addlinkedsrvlogin @rmtsrvname = @ServerName, @locallogin = NULL , @useself = N'True', @rmtuser = N'';
                                                EXEC dbo.sp_serveroption @server = @ServerName, @optname = N'data access', @optvalue = N'True';
                                                EXEC dbo.sp_serveroption @server = @ServerName, @optname = N'remote proc transaction promotion', @optvalue = N'False';
                                                EXEC dbo.sp_serveroption @server = @ServerName, @optname = N'rpc', @optvalue = N'True'
                                                EXEC dbo.sp_serveroption @server = @ServerName, @optname = N'rpc out', @optvalue = N'True'";

				var expectedQuery = string.Concat(query, System.Environment.NewLine, ensureServerOptionsQuery);
				AssertEquals(expectedQuery, linkedServerCreator.GetCreateLinkedServerWithNativeClientQuery());

				registration.KeyForTest.IsInternalSystemForTest = true;
				registration.KeyForTest.EnterpriseCodeForTest = "EDI";
				linkedServerCreator = new LinkedServerCreatorForTest(connection, testLinkedServer, logger);

				query = $@"
			IF NOT EXISTS (SELECT NULL FROM sys.servers WHERE name = @ServerName)
			BEGIN
							EXEC dbo.sp_addlinkedserver @server = @ServerName, @srvproduct=N'', @provider=N'SQLNCLI', @provstr = @ProviderString, @datasrc = @ServerNameWithPortNumber;
							EXEC dbo.sp_serveroption @server = @ServerName, @optname = N'connect timeout', @optvalue = N'60';
			END";

				expectedQuery = string.Concat(query, System.Environment.NewLine, ensureServerOptionsQuery);
				AssertEquals(expectedQuery, linkedServerCreator.GetCreateLinkedServerWithNativeClientQuery());

				registration = ObjectFactory.Get<IProductRegistration>();
				registration.KeyForTest.IsInternalSystemForTest = false;
				registration.KeyForTest.EnterpriseCodeForTest = "PRD";
				linkedServerCreator = new LinkedServerCreatorForTest(connection, testLinkedServer, logger);

				query = $@"
			IF NOT EXISTS (SELECT NULL FROM sys.servers WHERE name = @ServerName)
			BEGIN
							EXEC dbo.sp_addlinkedserver @server = @ServerName, @srvproduct=N'', @provider=N'SQLNCLI', @provstr = @ProviderString, @datasrc = @ServerNameWithPortNumber;
							EXEC dbo.sp_serveroption @server = @ServerName, @optname = N'connect timeout', @optvalue = N'60';
			END";

				expectedQuery = string.Concat(query, System.Environment.NewLine, ensureServerOptionsQuery);
				AssertEquals(expectedQuery, linkedServerCreator.GetCreateLinkedServerWithNativeClientQuery());
			}
		}

		public void TestLinkedServerConnectionCheckOnInvalidLinkedServer()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var testLinkedServer = "NonExistentLinkedServer";
				var logger = new LoggerForTest();
				var linkedServerCreator = new LinkedServerCreator(connection, testLinkedServer, logger);

				CombineAssertions(() =>
				{
					AssertEquals("Logger should be empty", 0, logger.LogEntries.Count());

					linkedServerCreator.IsLinkedServerConnectionAccessible();
					AssertEquals("Logger should have one entry", 1, logger.LogEntries.Count());

					AssertExceptionThrown<SqlException>(() =>
					{
						linkedServerCreator.IsLinkedServerConnectionAccessible(throwError: true);
					});

					foreach (var errorLog in logger.LogEntries)
					{
						AssertContains($"Creating linked server failed for {testLinkedServer} with the below message", errorLog);
					}
				});
			}
		}

		public void TestLinkedServerConnectionCheckWithPortNumber()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var testLinkedServer = "RealLinkedServer,1234";
				var logger = new LoggerForTest();
				var linkedServerCreator = new LinkedServerCreator(connection, testLinkedServer, logger);
				linkedServerCreator.DropLinkedServer();

				var createLinkedServerQuery = $"EXEC dbo.sp_addlinkedserver @server = '{testLinkedServer}', @srvproduct=N'', @provider=N'SQLNCLI', @provstr = '', @datasrc = '.';";
				connection.ExecuteNonQuery(createLinkedServerQuery);

				CombineAssertions(() =>
				{
					AssertEquals("Logger should be empty", 0, logger.LogEntries.Count());

					Assert(linkedServerCreator.IsLinkedServerConnectionAccessible());
					AssertEquals("Logger should still be empty", 0, logger.LogEntries.Count());

					AssertNoExceptionThrown(() =>
					{
						linkedServerCreator.IsLinkedServerConnectionAccessible(throwError: true);
					});
				});
			}
		}

		public void TestCreateLinkedServerWithNativeClientExternalSystem()
		{
			TestCreateLinkedServerWithNativeClient(enterpriseCode: "PRD", loginMapExisted: true);
		}

		public void TestCreateLinkedServerWithNativeClientInternalSystem()
		{
			TestCreateLinkedServerWithNativeClient(enterpriseCode: "WTL", loginMapExisted: false);
		}

		public void TestCreateLinkedServerWithNativeClientInternalEDISystem()
		{
			TestCreateLinkedServerWithNativeClient(enterpriseCode: "EDI", loginMapExisted: true);
		}

		void TestCreateLinkedServerWithNativeClient(string enterpriseCode, bool loginMapExisted)
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				var registration = ObjectFactory.Get<IProductRegistration>();
				registration.KeyForTest.EnterpriseCodeForTest = enterpriseCode;

				var portNumber = LinkedServerCreator.GetMainDBServerPortNumber();
				var linkedServerName = "LOCALHOST," + portNumber;
				var logger = new LoggerForTest();
				var linkedServerCreator = new LinkedServerCreatorForTest(adminConnection, linkedServerName, logger);
				linkedServerCreator.EnsureLinkedServerNotExists();
				try
				{
					((IDbLoginRepair)adminConnection).EnsureUnrestrictedWriterDbLogin();
					linkedServerCreator.CreateLinkedServerWithNativeClientForTest();
					CombineAssertions(() =>
					{
						AssertLinkedServerExists(adminConnection, linkedServerName, expected: true);
						Assert(
							"Provider should NOT have Multisubnetfailover=yes in connection string",
							!LinkedServerExistsWithMultiSubnetFailover(adminConnection, linkedServerName)
						);
					});
				}
				finally
				{
					linkedServerCreator.EnsureLinkedServerNotExists();
				}
			}
		}

		[RequiresSoftware(RequiredSoftware.MsOledbSqlProvider)]
		public void TestCreateLinkedServerWithMsOledbExternalSystem()
		{
			TestCreateLinkedServerWithMsOledb(enterpriseCode: "PRD", loginMapExisted: true);
		}

		[RequiresSoftware(RequiredSoftware.MsOledbSqlProvider)]
		public void TestCreateLinkedServerWithMsOledbInternalSystem()
		{
			TestCreateLinkedServerWithMsOledb(enterpriseCode: "WTL", loginMapExisted: false);
		}

		[RequiresSoftware(RequiredSoftware.MsOledbSqlProvider)]
		public void TestCreateLinkedServerWithMsOledbInternalEDISystem()
		{
			TestCreateLinkedServerWithMsOledb(enterpriseCode: "EDI", loginMapExisted: true);
		}

		void TestCreateLinkedServerWithMsOledb(string enterpriseCode, bool loginMapExisted)
		{
			var registration = ObjectFactory.Get<IProductRegistration>();
			using (var adminConnection = Db.NewAdminConnection())
			{
				registration = ObjectFactory.Get<IProductRegistration>();
				registration.KeyForTest.EnterpriseCodeForTest = enterpriseCode;
				var portNumber = LinkedServerCreator.GetMainDBServerPortNumber();
				var linkedServerName = "LOCALHOST," + portNumber;
				var logger = new LoggerForTest();
				var linkedServerCreator = new LinkedServerCreatorForTest(adminConnection, linkedServerName, logger);
				linkedServerCreator.EnsureLinkedServerNotExists();

				try
				{
					((IDbLoginRepair)adminConnection).EnsureUnrestrictedWriterDbLogin();
					linkedServerCreator.CreateLinkedServerWithMSOLEDBSQLForTest();

					CombineAssertions(() =>
					{
						Assert("Linked Server should exist and accessible.", linkedServerCreator.LinkedServerExistsAndIsAccessible(adminConnection, linkedServerName));
						AssertEquals(
							"Provider should have Multisubnetfailover=yes in connection string",
							loginMapExisted,
							LinkedServerExistsWithMultiSubnetFailover(adminConnection, linkedServerName)
						);
					});
				}
				finally
				{
					linkedServerCreator.EnsureLinkedServerNotExists();
				}
			}
		}

		bool LinkedServerExistsWithMultiSubnetFailover(AdminConnection adminConnection, string linkedServerName)
		{
			var query = $@"
IF EXISTS (
	SELECT NULL FROM SYS.SERVERS WHERE NAME = '{linkedServerName}'
	AND PROVIDER_STRING LIKE '%MultiSubnetFailover=YES%'
) SELECT 1 ELSE SELECT 0
";
			return Convert.ToBoolean(adminConnection.Command(query).ExecuteScalar(), CultureInfo.InvariantCulture);
		}

		public void TestCreateLinkedServerWithLocalServer()
		{
			using (var connection = Db.NewAdminConnection())
			{
				bool dataAccessOptionBeforeTest = GetLocalServerDataAccessOption(connection);

				try
				{
					SetLocalServerDataAccessOption(connection, value: false);
					AssertLocalServerDataAccessEnabled(connection, expected: false);

					SetLocalServerDistributedTransactionPromotionOption(connection, value: true);
					AssertLocalServerDistributedTransactionPromotionEnabled(connection, expected: true);

					var logger = new LoggerForTest();
					new LinkedServerCreator(connection, connection.ServerName, logger).CreateLinkedServer();
					AssertLocalServerDataAccessEnabled(connection, expected: true);
					AssertLocalServerDistributedTransactionPromotionEnabled(connection, expected: false);
				}
				finally
				{
					SetLocalServerDataAccessOption(connection, value: dataAccessOptionBeforeTest);
				}
			}
		}

		void AssertLinkedServerExists(DbConnection connection, string linkedServer, bool expected)
		{
			AssertRegisteredServerExists(connection, linkedServer, isLinked: true, expected: expected);
		}

		void AssertRegisteredServerExists(DbConnection connection, string serverName, bool isLinked, bool expected)
		{
			var isLinkedString = isLinked ? "1" : "0";
			var sqlText = $"IF EXISTS (SELECT NULL FROM sys.servers WHERE name = '{serverName}' AND is_linked = {isLinkedString}) SELECT 1 ELSE SELECT 0";

			AssertEquals(
								 (isLinked ? "Linked" : "Local") + " server [" + serverName + "] exists",
								 expected,
								 Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture)
			);

			if (expected)
			{
				var canConnectToLinkedServer = false;
				sqlText = $"SELECT * FROM OPENQUERY([{serverName}], 'SELECT 1')";
				canConnectToLinkedServer = Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture);
				AssertEquals("Should be able to connect to linked server without exception from CargowiseWriter login", expected, canConnectToLinkedServer);
			}
		}

		public static void AssertLocalServerDataAccessEnabled(DbConnection connection, bool expected)
		{
			var sqlText = $"SELECT is_data_access_enabled FROM sys.servers WHERE name = '{connection.ServerName}'";

			AssertEquals(
								 "Local server is_data_access_enabled",
								 expected,
								 Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture)
			);
		}

		static void AssertLocalServerDistributedTransactionPromotionEnabled(DbConnection connection, bool expected)
		{
			var sqlText = string.Format(
				CultureInfo.InvariantCulture,
				"SELECT is_remote_proc_transaction_promotion_enabled FROM sys.servers WHERE name = '{0}'",
				connection.ServerName);

			AssertEquals(
								 "Local server is_remote_proc_transaction_promotion_enabled",
								 expected,
								 Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture)
			);
		}

		bool GetLocalServerDataAccessOption(DbConnection connection)
		{
			string sqlText = "SELECT is_data_access_enabled FROM sys.servers WHERE name = @@SERVERNAME";
			return Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Test Cases")]
		bool GetLocalServerDistributedTransactionPromotionOption(DbConnection connection)
		{
			string sqlText = "SELECT is_remote_proc_transaction_promotion_enabled FROM sys.servers WHERE name = @@SERVERNAME";
			return Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture);
		}

		public static void SetLocalServerDataAccessOption(AdminConnection connection, bool value)
		{
			string strValue = value ? "True" : "False";
			var sqlText = $"EXEC dbo.sp_serveroption @server = N'{connection.ServerName}', @optname = N'data access', @optvalue = N'{strValue}'";
			connection.ExecuteNonQuery(sqlText);
		}

		public static void SetLocalServerDistributedTransactionPromotionOption(AdminConnection connection, bool value)
		{
			string strValue = value ? "True" : "False";
			var sqlText = $"EXEC dbo.sp_serveroption @server = N'{connection.ServerName}', @optname = N'remote proc transaction promotion', @optvalue = N'{strValue}'";
			connection.ExecuteNonQuery(sqlText);
		}
	}
}
