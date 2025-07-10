using System;
using Enterprise.DbUpgrader.Data.BaseData.Common;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.BaseData.System
{
	sealed class SystemUpgradeTaskTest : TransactionedTestCase
	{
		public void TestRun()
		{
			// Prepare test data
			Guid existingOrgPk = new Guid("6034C9C6-A8D0-4D07-B85D-14F73C1A8FB0");

			string sqlText = String.Format(@"
				delete from dbo.FaxRoute;
				delete from dbo.FaxDeviceConfig;
				INSERT into dbo.FaxDeviceConfig (FX_PK, FX_Name, FX_ServerName, FX_TerminalSessionID) VALUES ('{0}', 'existingFax', 'existingServer', 0);",
				existingOrgPk.ToString());
			TestConnection.ExecuteNonQuery(sqlText);

			AssertEquals("[BEFORE] 1 record in table FaxDeviceConfig", 1, CountRecordsInDatabase(FaxDeviceConfigSchema.Constants.TableName));
			AssertEquals("[BEFORE] Fax device (existingFax) in database", true, IsRecordInDatabase(existingOrgPk, FaxDeviceConfigSchema.Constants.PK, FaxDeviceConfigSchema.Constants.TableName));
			AssertEquals("[BEFORE] 0 record in table FaxRoute", 0, CountRecordsInDatabase(FaxRouteSchema.Constants.TableName));

			var testTask = new SystemUpgradeTask();
			testTask.Run();

			Guid internetFaxPk = new Guid("DEC106C8-231C-400E-8080-DECF7E2F755A");

			AssertEquals("[AFTER] 2 records in table FaxDeviceConfig", 1, CountRecordsInDatabase(FaxDeviceConfigSchema.Constants.TableName));
			AssertEquals("[AFTER] Fax device (INTERNET) in database", true, IsRecordInDatabase(internetFaxPk, FaxDeviceConfigSchema.Constants.PK, FaxDeviceConfigSchema.Constants.TableName));
			AssertEquals("[AFTER] 1 record in table FaxRoute", 1, CountRecordsInDatabase(FaxRouteSchema.Constants.TableName));
			AssertEquals("[AFTER] Fax Route for device (INTERNET) in database", true, IsRecordInDatabase(internetFaxPk, FaxRouteSchema.Constants.FR_FX, FaxRouteSchema.Constants.TableName));
		}

		public void TestRunWhenInternetDeviceAlreadyExists()
		{
			// Prepare test data
			Guid internetDevicePk = Guid.NewGuid();

			string sqlText = String.Format(@"
				delete from dbo.FaxDeviceConfig;
				INSERT into dbo.FaxDeviceConfig (FX_PK, FX_Name, FX_ServerName, FX_TerminalSessionID) VALUES ('{0}', 'INTERNET', 'ediFaxService', 0);
				INSERT into dbo.Faxroute (FR_PK, FR_RouteName, FR_Prefix, FR_FX) VALUES (NEWID(), 'All Faxes', '+', '{0}');",
				internetDevicePk.ToString());
			TestConnection.ExecuteNonQuery(sqlText);

			AssertEquals("[BEFORE] 1 record in table FaxDeviceConfig", 1, CountRecordsInDatabase(FaxDeviceConfigSchema.Constants.TableName));
			AssertEquals("[BEFORE] Fax device (INTERNET) in database", true, IsRecordInDatabase(internetDevicePk, FaxDeviceConfigSchema.Constants.PK, FaxDeviceConfigSchema.Constants.TableName));
			AssertEquals("[BEFORE] 1 record in table Faxroute", 1, CountRecordsInDatabase(FaxRouteSchema.Constants.TableName));
			AssertEquals("[BEFORE] Fax route for device (INTERNET) in database", true, IsRecordInDatabase(internetDevicePk, FaxRouteSchema.Constants.FR_FX, FaxRouteSchema.Constants.TableName));

			var testTask = new SystemUpgradeTask();
			testTask.Run();

			AssertEquals("[AFTER] 1 record in database", 1, CountRecordsInDatabase(FaxDeviceConfigSchema.Constants.TableName));
			AssertEquals("[AFTER] Fax device (INTERNET) populated from XML", true, IsRecordInDatabase(internetFaxPkFromXml, FaxDeviceConfigSchema.Constants.PK, FaxDeviceConfigSchema.Constants.TableName));
			AssertEquals("[AFTER] 1 record in table Faxroute", 1, CountRecordsInDatabase(FaxRouteSchema.Constants.TableName));
			AssertEquals("[AFTER] Fax route for device (INTERNET) in database", true, IsRecordInDatabase(internetFaxPkFromXml, FaxRouteSchema.Constants.FR_FX, FaxRouteSchema.Constants.TableName));
		}

		public void TestRunWhenInternetDeviceAlreadyExists2()
		{
			// Prepare test data
			Guid internetDevicePk = Guid.NewGuid();

			string sqlText = String.Format(@"
				delete from dbo.FaxDeviceConfig;
				INSERT into dbo.FaxDeviceConfig (FX_PK, FX_Name, FX_ServerName, FX_TerminalSessionID) VALUES ('{0}', 'INTERNET', 'ediFaxService', 0);
				INSERT into dbo.Faxroute (FR_PK, FR_RouteName, FR_Prefix, FR_FX) VALUES (NEWID(), 'International', '+', '{0}');",
				internetDevicePk.ToString());
			TestConnection.ExecuteNonQuery(sqlText);

			AssertEquals("[BEFORE] 1 record in table FaxDeviceConfig", 1, CountRecordsInDatabase(FaxDeviceConfigSchema.Constants.TableName));
			AssertEquals("[BEFORE] Fax device (INTERNET) in database", true, IsRecordInDatabase(internetDevicePk, FaxDeviceConfigSchema.Constants.PK, FaxDeviceConfigSchema.Constants.TableName));
			AssertEquals("[BEFORE] 1 record in table Faxroute", 1, CountRecordsInDatabase(FaxRouteSchema.Constants.TableName));
			AssertEquals("[BEFORE] Fax route for device (INTERNET) in database", true, IsRecordInDatabase(internetDevicePk, FaxRouteSchema.Constants.FR_FX, FaxRouteSchema.Constants.TableName));

			var testTask = new SystemUpgradeTask();
			testTask.Run();

			AssertEquals("[AFTER] 1 record in database", 1, CountRecordsInDatabase(FaxDeviceConfigSchema.Constants.TableName));
			AssertEquals("[AFTER] Fax device (INTERNET) populated from XML", true, IsRecordInDatabase(internetFaxPkFromXml, FaxDeviceConfigSchema.Constants.PK, FaxDeviceConfigSchema.Constants.TableName));
			AssertEquals("[AFTER] 1 record in table Faxroute", 1, CountRecordsInDatabase(FaxRouteSchema.Constants.TableName));
			AssertEquals("[AFTER] Fax route for device (INTERNET) in database", true, IsRecordInDatabase(internetFaxPkFromXml, FaxRouteSchema.Constants.FR_FX, FaxRouteSchema.Constants.TableName));
		}

		public void TestRunWhenInternetDeviceAlreadyExists3()
		{
			// Prepare test data
			Guid internetDevicePk = Guid.NewGuid();

			string sqlText = String.Format(@"
				delete from dbo.FaxDeviceConfig;
				INSERT into dbo.FaxDeviceConfig (FX_PK, FX_Name, FX_ServerName, FX_TerminalSessionID) VALUES ('{0}', 'INTERNET', 'ediFaxService', 0);",
				internetDevicePk.ToString());
			TestConnection.ExecuteNonQuery(sqlText);

			AssertEquals("[BEFORE] 1 record in table FaxDeviceConfig", 1, CountRecordsInDatabase(FaxDeviceConfigSchema.Constants.TableName));
			AssertEquals("[BEFORE] Fax device (INTERNET) in database", true, IsRecordInDatabase(internetDevicePk, FaxDeviceConfigSchema.Constants.PK, FaxDeviceConfigSchema.Constants.TableName));
			AssertEquals("[BEFORE] 0 record in table Faxroute", 0, CountRecordsInDatabase(FaxRouteSchema.Constants.TableName));

			var testTask = new SystemUpgradeTask();
			testTask.Run();

			AssertEquals("[AFTER] 1 record in database", 1, CountRecordsInDatabase(FaxDeviceConfigSchema.Constants.TableName));
			AssertEquals("[AFTER] Fax device (INTERNET) populated from XML", true, IsRecordInDatabase(internetFaxPkFromXml, FaxDeviceConfigSchema.Constants.PK, FaxDeviceConfigSchema.Constants.TableName));
			AssertEquals("[AFTER] 1 record in table Faxroute", 1, CountRecordsInDatabase(FaxRouteSchema.Constants.TableName));
			AssertEquals("[AFTER] Fax route for device (INTERNET) in database", true, IsRecordInDatabase(internetFaxPkFromXml, FaxRouteSchema.Constants.FR_FX, FaxRouteSchema.Constants.TableName));
		}

		public void TestAllFaxesRouteWithDifferentFaxDeviceConfig()
		{
			var internetConfigGuid = Guid.NewGuid();
			var anotherConfigGuid = Guid.NewGuid();

			var sqlText = String.Format(@"
				delete from dbo.FaxDeviceConfig;
				delete from dbo.FaxRoute;
				INSERT into dbo.FaxDeviceConfig (FX_PK, FX_Name, FX_ServerName, FX_TerminalSessionID) VALUES ('{0}', 'INTERNET', 'ediFaxService', 0);
				INSERT into dbo.FaxDeviceConfig (FX_PK, FX_Name, FX_ServerName, FX_TerminalSessionID) VALUES ('{1}', 'All Faxes Config', 'ediFaxService', 0);
				INSERT into dbo.FaxRoute (FR_PK, FR_RouteName, FR_Prefix, FR_FX) VALUES (NEWID(), 'Internet settings', '+', '{0}')
				INSERT into dbo.FaxRoute (FR_PK, FR_RouteName, FR_Prefix, FR_FX) VALUES (NEWID(), 'All Foxes', '+61', '{1}')
				INSERT into dbo.FaxRoute (FR_PK, FR_RouteName, FR_Prefix, FR_FX) VALUES (NEWID(), 'All Faxes', '+', '{1}');",
				internetConfigGuid,
				anotherConfigGuid);

			TestConnection.ExecuteNonQuery(sqlText);

			AssertEquals("Pre-condition", 2, CountRecordsInDatabase(FaxDeviceConfigSchema.Constants.TableName));
			AssertEquals("Pre-condition", 3, CountRecordsInDatabase(FaxRouteSchema.Constants.TableName));

			var testTask = new SystemUpgradeTask();
			testTask.Run();

			AssertEquals("Should populate from XML 1 fax device config", 1, CountRecordsInDatabase(FaxDeviceConfigSchema.Constants.TableName));
			AssertEquals("[AFTER] Fax device (INTERNET) in database", true, IsRecordInDatabase(internetFaxPkFromXml, FaxDeviceConfigSchema.Constants.PK, FaxDeviceConfigSchema.Constants.TableName));

			AssertEquals("Should populate from XML 1 fax route", 1, CountRecordsInDatabase(FaxRouteSchema.Constants.TableName));
			var sql = String.Format("SELECT * FROM {0} WHERE {1} = '{2}' AND {3} = '{4}'",
				FaxRouteSchema.Constants.TableName,
				FaxRouteSchema.Constants.FR_RouteName,
				"All Faxes",
				FaxRouteSchema.Constants.FR_FX,
				internetFaxPkFromXml);

			Assert("Expected route named 'All Faxes' to belong to the INTERNET config", BaseDataUpgradeTask.IsRecordInDatabase(sql));
		}

		#region Implementation

		int CountRecordsInDatabase(string tableName)
		{
			string sql = string.Format(@"select count(*) from {0}", tableName);
			return (int)TestConnection.ExecuteScalar(sql);
		}

		bool IsRecordInDatabase(Guid pk, string guidColumnName, string tableName)
		{
			return BaseDataUpgradeTask.IsRecordInDatabase(String.Format("SELECT {0} FROM {1} WHERE {0} = '{2}'", guidColumnName, tableName, pk));
		}

		protected override void SetUp()
		{
			base.SetUp();

			internetFaxPkFromXml = new Guid("dec106c8-231c-400e-8080-decf7e2f755a");
		}

		Guid internetFaxPkFromXml;

		#endregion
	}
}
