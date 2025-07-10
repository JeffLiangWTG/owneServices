using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ProductRegistration.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ZClientEDI.Business.Test;

namespace Enterprise.Client.EDI.Test
{
	public class ClientRoutinesTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestClientEDIArap()
		{
			var cmd = Db.Connection.Command("dbo.ClientEDIArap");
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.ExecuteNonQuery();
		}

		[ExpectNoExceptions]
		public void TestClientEDIArapDetail()
		{
			var cmd = Db.Connection.Command("dbo.ClientEDIArapDetail");
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.ExecuteNonQuery();
		}

		public void TestProductRegistrationGetStatus()
		{
			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			db1.LicEnterprise.LE_EnterpriseCode = "HYE";
			db1.LD_ServerCode = "SYD";
			db1.LD_Product = "CW1";
			var db2 = Factory.NewWithValidTestData<LicenceDatabase>();
			db2.LicEnterprise.LE_EnterpriseCode = "ENT";
			db2.LD_ServerCode = "SYD";
			db2.LD_Status = DatabaseStatusList.Codes.REG;
			db2.LD_Product = "CW1";
			var db3 = Factory.NewWithValidTestData<LicenceDatabase>();
			db3.LicEnterprise.LE_EnterpriseCode = "EN3";
			db3.LD_ServerCode = "MEL";
			db3.LD_Status = DatabaseStatusList.Codes.Preregistered;
			db3.LD_Product = "CWN";
			Factory.Save();
			AssertProductRegistrationGetStatus("HYE", "SYD", db1.PK.ToGuid(), db1.LD_DatabaseNumber, "", "CW1");
			AssertProductRegistrationGetStatus("ENT", "SYD", db2.PK.ToGuid(), db2.LD_DatabaseNumber, DatabaseStatusList.Codes.REG, "CW1");
			AssertProductRegistrationGetStatus("EN3", "MEL", db3.PK.ToGuid(), db3.LD_DatabaseNumber, DatabaseStatusList.Codes.NON, "CWN");
		}

		void AssertProductRegistrationGetStatus(string enterpriseCode, string serverCode, Guid expectedPk, int expectedNumber, string expectedStatus, string expectedProduct)
		{
			var cmd = Db.Connection.Command("select * from dbo.ProductRegistrationGetStatus(@EnterpriseCode, @ServerCode)");
			cmd.AddParameter("@EnterpriseCode", SqlDbType.VarChar, enterpriseCode);
			cmd.AddParameter("@ServerCode", SqlDbType.VarChar, serverCode);
			using (var reader = cmd.ExecuteReader())
			{
				if (reader.Read())
				{
					var pk = (Guid)reader[LicenceDatabaseSchema.Constants.PK];
					var databaseNumber = (int)reader[LicenceDatabaseSchema.Constants.LD_DatabaseNumber];
					var status = (string)reader[LicenceDatabaseSchema.Constants.LD_Status];
					var product = (string)reader[LicenceDatabaseSchema.Constants.LD_Product];
					reader.Close();
					AssertEquals(expectedStatus, status);
					AssertEquals(expectedNumber, databaseNumber);
					AssertEquals(expectedPk, pk);
					AssertEquals(expectedProduct, product);
				}
			}
		}

		public void TestProductRegistrationAdd()
		{
			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			db1.LicEnterprise.LE_EnterpriseCode = "EN1";
			db1.LD_ServerCode = "SYD";
			db1.LD_ManualLicenceExpiry = ZDateTime.Today.AddDays(45);
			db1.LD_HostedLocation = "TRA";
			var dbPreFail = Factory.NewWithValidTestData<LicenceDatabase>();
			dbPreFail.LicEnterprise.LE_EnterpriseCode = "EN2";
			dbPreFail.LD_ServerCode = "MEL";
			dbPreFail.LD_ManualLicenceExpiry = ZDateTime.Empty;
			dbPreFail.LD_Status = DatabaseStatusList.Codes.Preregistered;
			dbPreFail.LD_HostServerName = "server2";
			dbPreFail.LD_HostDBName = "db2";
			var dbPreOk = Factory.NewWithValidTestData<LicenceDatabase>();
			dbPreOk.LicEnterprise.LE_EnterpriseCode = "EN3";
			dbPreOk.LD_ServerCode = "BNE";
			dbPreOk.LD_ManualLicenceExpiry = ZDateTime.Today.AddDays(40);
			dbPreOk.LD_Status = DatabaseStatusList.Codes.Preregistered;
			dbPreOk.LD_HostServerName = "myserver";
			dbPreOk.LD_HostDBName = "mydb";
			var db4 = Factory.NewWithValidTestData<LicenceDatabase>();
			db4.LicEnterprise.LE_EnterpriseCode = "EN4";
			db4.LD_ServerCode = "SYD";
			db4.LD_ManualLicenceExpiry = ZDateTime.Empty;
			var dbReReg = Factory.NewWithValidTestData<LicenceDatabase>();
			dbReReg.LicEnterprise.LE_EnterpriseCode = "HYE";
			dbReReg.LD_ServerCode = "SYD";
			dbReReg.LD_ManualLicenceExpiry = ZDateTime.Empty;
			dbReReg.LD_HostServerName = "myserver";
			dbReReg.LD_HostDBName = "AnotherDb";
			dbReReg.LD_HostConnectionServerName = "SomeServer.com";
			dbReReg.LD_CanReregisterToSameServer = true;
			Factory.Save();
			AssertProductRegistrationAdd("EN1", "SYD", DatabaseStatusList.Codes.REG, (int)RegisterStatus.Success);
			AssertProductRegistrationAdd("EN1", "SYD", null, (int)RegisterStatus.ProductKeyUnavailable);
			AssertProductRegistrationAdd("ZZZ", "SYD", null, (int)RegisterStatus.ProductKeyNotFound);
			AssertProductRegistrationAdd("EN2", "MEL", null, (int)RegisterStatus.ProductKeyUnavailable);
			AssertProductRegistrationAdd("EN3", "BNE", DatabaseStatusList.Codes.REG, (int)RegisterStatus.Success);
			AssertProductRegistrationAdd("EN4", "SYD", DatabaseStatusList.Codes.REG, (int)RegisterStatus.Success, "SomeRandomDNSName.cargowise.com");
			AssertProductRegistrationAdd("HYE", "SYD", DatabaseStatusList.Codes.REG, (int)RegisterStatus.Success, "SomeServer.com");
			db1.Reload();
			AssertEquals("LD_HostServerName", "myserver", db1.LD_HostServerName);
			AssertEquals("LD_HostDBName", "mydb", db1.LD_HostDBName);
			AssertEquals("LD_Password", "1234", db1.LD_Password);
			AssertEquals("LD_HostDBCreateDate", new DateTime(2014, 9, 26, 10, 15, 25), db1.LD_HostDBCreateDate);
		}

		public void TestProductRegistrationAdd_Concurrency()
		{
			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			db1.LicEnterprise.LE_EnterpriseCode = "EN1";
			db1.LD_ServerCode = "SYD";
			db1.LD_ManualLicenceExpiry = ZDateTime.Today.AddDays(45);
			db1.LD_HostedLocation = "TRA";
			Factory.Save();

			using (var otherConnection = Db.NewExtraConnectionToMainDb())
			{
				SqlApplicationLock mutex = null;
				var lockAcquired = false;
				int retryCount = 0;
				const int maxRetries = 3;

				while (retryCount < maxRetries && !lockAcquired)
				{
					try
					{
						lockAcquired = otherConnection.TryGetLock(db1.PK.ToString().ToUpperInvariant(), TimeSpan.FromSeconds(5), out mutex);
						if (!lockAcquired)
						{
							retryCount++;
							Thread.Sleep(5000);
						}
					}
					catch (SqlException ex) when (ex.Number == 1222)
					{
						retryCount++;
						Thread.Sleep(5000);
					}
				}
				Assert("Precondition: a lock was acquired", lockAcquired);

				using (mutex)
				{
					AssertExceptionThrown<SqlException>(() => AssertProductRegistrationAdd("EN1", "SYD", null, -1));
				}
			}
		}

		void AssertProductRegistrationAdd(string enterpriseCode, string serverCode, string expectedStatus, int expectedReturnValue, string connectionServerName = null)
		{
			var cmd = Db.Connection.Command("dbo.ProductRegistrationAdd");
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.AddParameter("@EnterpriseCode", SqlDbType.VarChar, enterpriseCode);
			cmd.AddParameter("@ServerCode", SqlDbType.VarChar, serverCode);
			cmd.AddParameter("@ServerName", SqlDbType.VarChar, "myserver");
			cmd.AddParameter("@DatabaseName", SqlDbType.VarChar, "mydb");
			cmd.AddParameter("@DatabaseCreateDate", SqlDbType.DateTime, new DateTime(2014, 9, 26, 10, 15, 25));
			cmd.AddParameter("@GroupId", SqlDbType.UniqueIdentifier, Guid.Empty);
			cmd.AddParameter("@Password", SqlDbType.VarChar, "1234");
			cmd.AddParameter("@LockTimeoutMs", SqlDbType.Int, 0);
			if (connectionServerName != null)
			{
				cmd.AddParameter("@ConnectionServerName", SqlDbType.VarChar, connectionServerName);
			}

			cmd.AddReturnValueParameter();
			// Note, when you use a DataReader object, you must close it or read to the end of the data before you can view the output parameters.
			using (var reader = cmd.ExecuteReader())
			{
				if (reader.Read())
				{
					var utcNow = (DateTime)reader["UtcNow"];
					var hostedLocation = (string)reader[LicenceDatabaseSchema.Constants.LD_HostedLocation];
					var securityMode = (string)reader[LicenceDatabaseSchema.Constants.LD_DBServerSecurityMode];
					var databaseNumber = (int)reader[LicenceDatabaseSchema.Constants.LD_DatabaseNumber];
					var licenceType = (string)reader[LicenceDatabaseSchema.Constants.LD_LicenceType];
					var licenceExpiry = reader[LicenceDatabaseSchema.Constants.LD_LicenceExpiry];
					reader.Close();
					var licDb = new BusinessObjectFactory().LoadTop1<LicenceDatabase>(new ZQuery(LicenceDatabaseSchema.LD_DatabaseNumber, databaseNumber));
					AssertEquals(expectedStatus, licDb.LD_Status);
					if (licDb.LD_HostedLocation == "TRA")
					{
						AssertEquals("Trans-Soft treated as Not Hosted with CW", "NCW", hostedLocation);
					}
					else
					{
						AssertEquals("hosted location", licDb.LD_HostedLocation, hostedLocation);
					}

					if (licDb.LD_ManualLicenceExpiry.IsEmpty)
					{
						AssertEquals(DBNull.Value, licenceExpiry);
					}
					else
					{
						AssertEquals(licDb.LD_ManualLicenceExpiry.ToDateTime(), (DateTime)licenceExpiry);
					}

					AssertEquals("LD_HostConnectionServerName", connectionServerName ?? "", licDb.LD_HostConnectionServerName);
				}
			}

			int returnValue = cmd.ReturnValue;
			AssertEquals("ReturnValue", expectedReturnValue, returnValue);
		}

		public void TestProductRegistrationVerify()
		{
			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			db1.LicEnterprise.LE_EnterpriseCode = "HYE";
			db1.LD_ServerCode = "SYD";
			db1.LD_HostServerName = "server1";
			db1.LD_HostDBName = "db1";
			db1.LD_HostDBCreateDate = new ZDateTime(2014, 10, 15);
			db1.LD_HostGroupId = ZGuid.NewZGuid();
			db1.CustomExpiredNote.Text = "MSG1";
			db1.CustomExpiryWeekNote.Text = "MSG2";
			db1.CustomExpiryMonthNote.Text = "MSG3";
			db1.LD_Status = DatabaseStatusList.Codes.REG;
			db1.LD_Password = "passwordhash";
			db1.LD_HostedLocation = "TRA";
			db1.LD_ManualLicenceExpiry = ZDateTime.Today.AddDays(35);
			var dbHosted = Factory.NewWithValidTestData<LicenceDatabase>();
			dbHosted.LicEnterprise.LE_EnterpriseCode = "ENH";
			dbHosted.LD_ServerCode = "SYD";
			dbHosted.LD_HostServerName = "serverH";
			dbHosted.LD_HostDBName = "dbHosted";
			dbHosted.LD_HostDBCreateDate = new ZDateTime(2015, 5, 26);
			dbHosted.LD_Status = DatabaseStatusList.Codes.REG;
			dbHosted.LD_Password = "passwordhash";
			dbHosted.LD_HostedLocation = "SYD";
			dbHosted.LD_HostConnectionServerName = "server.wisecloud.com";
			var dbUnregistered = Factory.NewWithValidTestData<LicenceDatabase>();
			dbUnregistered.LicEnterprise.LE_EnterpriseCode = "ENT";
			dbUnregistered.LD_ServerCode = "SYD";
			dbUnregistered.LD_Status = "";
			dbUnregistered.LD_HostServerName = "server1";
			dbUnregistered.LD_HostDBName = "db1";
			dbUnregistered.LD_HostDBCreateDate = new ZDateTime(2014, 10, 15);
			dbUnregistered.LD_HostedLocation = "SYD";
			var dbInactive = Factory.NewWithValidTestData<LicenceDatabase>();
			dbInactive.LicEnterprise.LE_EnterpriseCode = "EN3";
			dbInactive.LD_ServerCode = "SYD";
			dbInactive.LD_Status = DatabaseStatusList.Codes.REG;
			dbInactive.LD_Password = "passwordhash";
			dbInactive.LD_HostServerName = "server1";
			dbInactive.LD_HostDBName = "db1";
			dbInactive.LD_HostDBCreateDate = new ZDateTime(2014, 10, 15);
			dbInactive.LD_IsActive = false;
			Factory.Save();
			AssertProductRegistrationVerify(db1.LD_DatabaseNumber, "server1", "db1", new DateTime(2014, 10, 15), db1.LD_HostGroupId.ToGuid(), null, (int)RegisterStatus.Success, "HYE", "SYD", "MSG1");
			AssertProductRegistrationVerify(db1.LD_DatabaseNumber, "server1", "db1", new DateTime(2014, 10, 15), db1.LD_HostGroupId.ToGuid(), "passwordhash", (int)RegisterStatus.Success, "HYE", "SYD", "MSG1");
			AssertProductRegistrationVerify(db1.LD_DatabaseNumber, "server1", "db1", new DateTime(2014, 10, 15), "selfhosted.yourdomain.com", db1.LD_HostGroupId.ToGuid(), "passwordhash", (int)RegisterStatus.UniqueKeyUpdated, "HYE", "SYD", "MSG1");
			AssertProductRegistrationVerify(db1.LD_DatabaseNumber, "server1", "OtherDb", new DateTime(2014, 10, 15), db1.LD_HostGroupId.ToGuid(), null, (int)RegisterStatus.UniqueKeyNotMatched, null, null, null);
			AssertProductRegistrationVerify(db1.LD_DatabaseNumber, "server1", "OtherDb", new DateTime(2014, 10, 15), db1.LD_HostGroupId.ToGuid(), "passwordhash", (int)RegisterStatus.UniqueKeyNotMatched, null, null, null);
			AssertProductRegistrationVerify(dbUnregistered.LD_DatabaseNumber, "server1", "db1", new DateTime(2014, 10, 15), null, null, (int)RegisterStatus.Unregistered, null, null, null);
			AssertProductRegistrationVerify(dbUnregistered.LD_DatabaseNumber, "server1", "db1", new DateTime(2014, 10, 15), null, "passwordhash", (int)RegisterStatus.Unregistered, null, null, null);
			AssertProductRegistrationVerify(dbInactive.LD_DatabaseNumber, "server1", "db1", new DateTime(2014, 10, 15), null, null, (int)RegisterStatus.ProductKeyNotFound, null, null, null);
			AssertProductRegistrationVerify(dbInactive.LD_DatabaseNumber, "server1", "db1", new DateTime(2014, 10, 15), null, "passwordhash", (int)RegisterStatus.ProductKeyNotFound, null, null, null);
			AssertProductRegistrationVerify(dbHosted.LD_DatabaseNumber, "serverH", "dbHosted", new DateTime(2015, 5, 26), "server.wisecloud.com", null, null, (int)RegisterStatus.Success, "ENH", "SYD", "");
			AssertProductRegistrationVerify(dbHosted.LD_DatabaseNumber, "newServer", "dbHosted", new DateTime(2015, 5, 27), "server.wisecloud.com", null, null, (int)RegisterStatus.UniqueKeyUpdated, "ENH", "SYD", "");
			AssertProductRegistrationVerify(dbHosted.LD_DatabaseNumber, "newServer", "dbHosted", new DateTime(2015, 5, 27), "server2.wisecloud.com", null, null, (int)RegisterStatus.UniqueKeyUpdated, "ENH", "SYD", "");
			AssertProductRegistrationVerify(dbHosted.LD_DatabaseNumber, "newServer", "dbHosted2", new DateTime(2015, 5, 27), "server2.wisecloud.com", null, null, (int)RegisterStatus.UniqueKeyNotMatched, "ENH", "SYD", "");
		}

		void AssertProductRegistrationVerify(int dbNumber, string serverName, string dbName, DateTime dbCreated, Guid? groupId, string passwordHash, int expectedReturnValue, string expectedEnterpriseCode, string expectedServerCode, string expectedMsg)
		{
			AssertProductRegistrationVerify(dbNumber, serverName, dbName, dbCreated, null, groupId, passwordHash, expectedReturnValue, expectedEnterpriseCode, expectedServerCode, expectedMsg);
		}

		void AssertProductRegistrationVerify(int dbNumber, string serverName, string dbName, DateTime dbCreated, string connectionServerName, Guid? groupId, string passwordHash, int expectedReturnValue, string expectedEnterpriseCode, string expectedServerCode, string expectedMsg)
		{
			var cmd = Db.Connection.Command("dbo.ProductRegistrationVerify");
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.AddParameter("@DatabaseNumber", SqlDbType.Int, dbNumber);
			cmd.AddParameter("@ServerName", SqlDbType.VarChar, serverName);
			cmd.AddParameter("@DatabaseName", SqlDbType.VarChar, dbName);
			cmd.AddParameter("@DatabaseCreateDate", SqlDbType.DateTime, dbCreated);
			cmd.AddParameter("@GroupId", SqlDbType.UniqueIdentifier, groupId ?? (object)DBNull.Value);
			if (passwordHash != null)
			{
				cmd.AddParameter("@Password", SqlDbType.VarChar, passwordHash);
			}

			if (connectionServerName != null)
			{
				cmd.AddParameter("@ConnectionServerName", SqlDbType.VarChar, connectionServerName);
			}

			cmd.AddReturnValueParameter();
			using (var reader = cmd.ExecuteReader())
			{
				if (reader.Read())
				{
					var utcNow = (DateTime)reader["UtcNow"];
					var enterpriseCode = (string)reader[LicenceEnterpriseSchema.Constants.LE_EnterpriseCode];
					var serverCode = (string)reader[LicenceDatabaseSchema.Constants.LD_ServerCode];
					var licenceType = (string)reader[LicenceDatabaseSchema.Constants.LD_LicenceType];
					var licenceExpiry = reader[LicenceDatabaseSchema.Constants.LD_LicenceExpiry];
					var hostedLocation = reader[LicenceDatabaseSchema.Constants.LD_HostedLocation];
					var securityMode = reader[LicenceDatabaseSchema.Constants.LD_DBServerSecurityMode];
					var expiredMessage = reader["CustomExpiredMessage"].ToString();
					reader.Close();
					AssertEquals(expectedEnterpriseCode, enterpriseCode);
					AssertEquals(expectedServerCode, serverCode);
					AssertEquals(expectedMsg, expiredMessage);
					var licDb = new BusinessObjectFactory().LoadTop1<LicenceDatabase>(new ZQuery(LicenceDatabaseSchema.LD_DatabaseNumber, dbNumber));
					if (licDb.LD_HostedLocation == "TRA")
					{
						AssertEquals("Trans-Soft treated as Not Hosted with CW", "NCW", hostedLocation);
					}
					else
					{
						AssertEquals("hosted location", licDb.LD_HostedLocation, hostedLocation);
					}

					AssertEquals(connectionServerName ?? "", licDb.LD_HostConnectionServerName);
					AssertEquals(serverName, licDb.LD_HostServerName);
					AssertEquals(dbCreated, licDb.LD_HostDBCreateDate);
					if (groupId.HasValue)
					{
						AssertEquals(groupId.Value, licDb.LD_HostGroupId.ToGuid());
					}
					else
					{
						Assert("LD_HostGroupId IsEmpty", licDb.LD_HostGroupId.IsEmpty);
					}

					if (licDb.LD_ManualLicenceExpiry.IsEmpty)
					{
						AssertEquals(DBNull.Value, licenceExpiry);
					}
					else
					{
						AssertEquals(licDb.LD_ManualLicenceExpiry.ToDateTime(), (DateTime)licenceExpiry);
					}
				}
			}

			int returnValue = cmd.ReturnValue;
			AssertEquals("ReturnValue", expectedReturnValue, returnValue);
		}

		public void TestProductRegistrationRemove()
		{
			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			db1.LicEnterprise.LE_EnterpriseCode = "HYE";
			db1.LD_ServerCode = "SYD";
			db1.LD_Status = DatabaseStatusList.Codes.REG;
			db1.LD_Password = "passwordhash";
			var db2 = Factory.NewWithValidTestData<LicenceDatabase>();
			db2.LicEnterprise.LE_EnterpriseCode = "ENT";
			db2.LD_ServerCode = "SYD";
			db2.LD_Status = DatabaseStatusList.Codes.REG;
			db2.LD_Password = "passwordhash";
			var dbInactive = Factory.NewWithValidTestData<LicenceDatabase>();
			dbInactive.LicEnterprise.LE_EnterpriseCode = "EN2";
			dbInactive.LD_ServerCode = "SYD";
			dbInactive.LD_Status = DatabaseStatusList.Codes.REG;
			dbInactive.LD_Password = "passwordhash";
			dbInactive.LD_IsActive = false;
			Factory.Save();
			AssertProductRegistrationRemove(db1.LD_DatabaseNumber, "wrongpassword", 2);
			AssertProductRegistrationRemove(db1.LD_DatabaseNumber, "passwordhash", (int)RegisterStatus.Success);
			AssertProductRegistrationRemove(db1.LD_DatabaseNumber, "passwordhash", 2);
			AssertProductRegistrationRemove(db2.LD_DatabaseNumber, "wrongpassword", 2);
			AssertProductRegistrationRemove(db2.LD_DatabaseNumber, "passwordhash", (int)RegisterStatus.Success);
			AssertProductRegistrationRemove(db2.LD_DatabaseNumber, "passwordhash", 2);
			AssertProductRegistrationRemove(dbInactive.LD_DatabaseNumber, "passwordhash", 2);
			AssertProductRegistrationRemove(9876, "blah", 1);
			db1.Reload();
			db2.Reload();
			AssertEquals("NON", db1.LD_Status);
			AssertEquals("NON", db2.LD_Status);
			AssertEquals("-", db1.LD_Password);
			AssertEquals("-", db2.LD_Password);
		}

		void AssertProductRegistrationRemove(int dbNumber, string passwordHash, int expectedReturnValue)
		{
			var cmd = Db.Connection.Command("dbo.ProductRegistrationRemove");
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.AddParameter("@DatabaseNumber", SqlDbType.Int, dbNumber);
			cmd.AddParameter("@Password", SqlDbType.VarChar, passwordHash);
			int returnValue = cmd.ExecuteProcedureWithReturnValue();
			AssertEquals("ReturnValue", expectedReturnValue, returnValue);
		}

		public void TestProductRegistrationGetDatabaseUniqueKey()
		{
			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			db1.LD_ServerCode = "SYD";

			var db2 = Factory.NewWithValidTestData<LicenceDatabase>();
			db2.LD_ServerCode = "SYD";
			db2.LD_HostDBName = "OdysseySYD";
			db2.LD_HostServerName = "SYD-Cloud-1";
			db2.LD_HostGroupId = Guid.Parse("C85B44C8-CB13-4B81-BE6B-EBDA40CC665A");
			db2.LD_HostDBCreateDate = new ZDateTime(2023, 3, 20);
			db2.LD_HostConnectionServerName = "SYD.DB.WTG.ZONE";
			Factory.Save();

			AssertProductRegGetDatabaseUniqueKey(db1.LD_DatabaseNumber, "", "", Guid.Empty, new DateTime(1900, 1, 1), "");
			AssertProductRegGetDatabaseUniqueKey(db2.LD_DatabaseNumber, db2.LD_HostDBName, db2.LD_HostServerName, db2.LD_HostGroupId.ToGuid(), db2.LD_HostDBCreateDate.ToDateTime(), db2.LD_HostConnectionServerName);
		}

		void AssertProductRegGetDatabaseUniqueKey(int dbNumber, string expectedDbName, string expectedServerName, Guid expectedGroupId, DateTime expeactedDbCreateDate, string expectedConnectionServerName)
		{
			var cmd = Db.Connection.Command("select * from dbo.ProductRegistrationGetDatabaseUniqueKey(@DatabaseNumber)");
			cmd.AddParameter("@DatabaseNumber", SqlDbType.Int, dbNumber);
			using (var reader = cmd.ExecuteReader())
			{
				if (reader.Read())
				{
					var dbName = (string)reader[LicenceDatabaseSchema.Constants.LD_HostDBName];
					var serverName = (string)reader[LicenceDatabaseSchema.Constants.LD_HostServerName];
					var groupId = (Guid)reader[LicenceDatabaseSchema.Constants.LD_HostGroupId];
					var dbCreateDate = (DateTime)reader[LicenceDatabaseSchema.Constants.LD_HostDBCreateDate];
					var connectionServerName = (string)reader[LicenceDatabaseSchema.Constants.LD_HostConnectionServerName];
					reader.Close();
					AssertEquals(expectedDbName, dbName);
					AssertEquals(expectedServerName, serverName);
					AssertEquals(expectedGroupId, groupId);
					AssertEquals(expeactedDbCreateDate, dbCreateDate);
					AssertEquals(expectedConnectionServerName, connectionServerName);
				}
			}
		}

		#region Clienttg_INS_HelpErrorLogKey
		public void TestClienttg_INS_HelpErrorLogKey_IgnoreVisibility()
		{
			InsertLogKey("01", "House", true);
			AssertNoExceptionThrown("Non-duplicate keys are allowed", () => InsertLogKey("02", "Car", true));
			try
			{
				InsertLogKey("03", "House", false);
				Fail("Should have SqlException - Duplicate keys with different client visibility are actually still duplicates");
			}
			catch (SqlException ex)
			{
				AssertEquals(@"Duplicate HK_Key NOT allowed on HelpErrorLogKey. Existing issue number: 01. Duplicate issue number: 03
The transaction ended in the trigger. The batch has been aborted.", ex.Message);
			}
		}

		public void TestClienttg_INS_HelpErrorLogKey_IgnoreCase()
		{
			InsertLogKey("01", "House", true);
			AssertNoExceptionThrown("Keys that are seen as duplicates when ignoring case are allowed", () => InsertLogKey("02", "HOUSE", true));
			try
			{
				InsertLogKey("03", "House", true);
				Fail("Should have SqlException");
			}
			catch (SqlException ex)
			{
				AssertEquals(@"Duplicate HK_Key NOT allowed on HelpErrorLogKey. Existing issue number: 01. Duplicate issue number: 03
The transaction ended in the trigger. The batch has been aborted.", ex.Message);
			}
		}

		void InsertLogKey(string issueNumber, string key, bool isClientVisible)
		{
			Guid logPK = Guid.NewGuid();
			string queryLog = string.Format("INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}) VALUES (@PK, @HE_IsClientVisible, @HE_IssueNumber, @User, @User, @Time, @Time)", HelpErrorLogSchema.Constants.TableName, HelpErrorLogSchema.PK.Name, HelpErrorLogSchema.HE_IsClientVisible.Name, HelpErrorLogSchema.HE_IssueNumber.Name, HelpErrorLogSchema.HE_SystemCreateUser.Name, HelpErrorLogSchema.HE_SystemLastEditUser.Name, HelpErrorLogSchema.HE_SystemCreateTimeUtc.Name, HelpErrorLogSchema.HE_SystemLastEditTimeUtc.Name);
			string queryLogKey = string.Format("INSERT INTO {0} ({1}, {2}, {3}, {4}) VALUES (newid(), @HK_Key, @HK_HashCode, @HK_HE)", HelpErrorLogKeySchema.Constants.TableName, HelpErrorLogKeySchema.PK.Name, HelpErrorLogKeySchema.HK_Key.Name, HelpErrorLogKeySchema.HK_HashCode.Name, HelpErrorLogKeySchema.HK_HE.Name);
			using (DbCommand command = TestConnection.Command(queryLog))
			{
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, logPK);
				command.AddParameter("@HE_IsClientVisible", SqlDbType.Bit, isClientVisible);
				command.AddParameter("@HE_IssueNumber", SqlDbType.VarChar, issueNumber);
				command.AddParameter("@User", SqlDbType.VarChar, "~BP");
				command.AddParameter("@Time", SqlDbType.SmallDateTime, DateTime.UtcNow);
				command.ExecuteNonQuery();
			}

			using (DbCommand command = TestConnection.Command(queryLogKey))
			{
				command.AddParameter("@HK_Key", SqlDbType.VarChar, key);
				command.AddParameter("@HK_HashCode", SqlDbType.Int, HelpErrorLogKey.GetKeyHashCode(key));
				command.AddParameter("@HK_HE", SqlDbType.UniqueIdentifier, logPK);
				command.ExecuteNonQuery();
			}
		}

		#endregion
		#region TestReport_CompetencyCompletionResults
		public void TestReport_CompetencyCompletionResults()
		{
			Guid orgGuid = Guid.NewGuid();
			Guid org2Guid = Guid.NewGuid();
			Guid skillGuid1 = Guid.NewGuid();
			Guid skillGuid2 = Guid.NewGuid();
			Guid testGuid1 = Guid.NewGuid();
			Guid testGuid2 = Guid.NewGuid();
			Guid skillRatingGuid1 = Guid.NewGuid();
			Guid skillRatingGuid2 = Guid.NewGuid();
			Guid skillRatingGuid3 = Guid.NewGuid();
			Guid campaignGuid = Guid.NewGuid();
			Guid tedGuid = Guid.NewGuid();
			Guid madmanGuid = Guid.NewGuid();
			Guid samuelGuid = Guid.NewGuid();
			Guid licEnterpriseGuild = Guid.NewGuid();
			var tedCampaignItemGuid = Guid.NewGuid();
			var personPk = Guid.NewGuid();
			var examSettings = Guid.NewGuid();
			string commandText = string.Format(@"
INSERT into dbo.GlbPerson (PER_PK, PER_FullName) VALUES ('{15}', 'sdf')
INSERT INTO dbo.GlbCompanyCampaign (G0_PK, G0_GC, G0_CampaignName, G0_CampaignID, G0_SystemCreateTimeUtc, G0_SystemCreateUser, G0_SystemLastEditTimeUtc, G0_SystemLastEditUser)
VALUES ('{4}', '03052ED3-2C64-49AC-97D8-C6079D5015B5', 'sdfsdfsdf', '23534', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobSkill (HS_PK, HS_SkillDescription, HS_Code, HS_SystemCreateTimeUtc, HS_SystemCreateUser, HS_SystemLastEditTimeUtc, HS_SystemLastEditUser) VALUES ('{0}', 'Some Skill', 'HS1', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobSkill (HS_PK, HS_SkillDescription, HS_Code, HS_SystemCreateTimeUtc, HS_SystemCreateUser, HS_SystemLastEditTimeUtc, HS_SystemLastEditUser) VALUES ('{11}', 'Some Skill', 'HS2', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.ExamSetting (EXS_PK, EXS_Code, EXS_G0) VALUES ('{16}', 'ZZZ', '{4}')

INSERT INTO dbo.HRJobSkillTest (HT_PK, HT_HS, HT_TestName, HT_EXS, HT_SystemCreateTimeUtc, HT_SystemCreateUser, HT_SystemLastEditTimeUtc, HT_SystemLastEditUser) VALUES ('{1}', '{0}', 'Some form of Test', '{16}', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobSkillTest (HT_PK, HT_HS, HT_TestName, HT_EXS, HT_SystemCreateTimeUtc, HT_SystemCreateUser, HT_SystemLastEditTimeUtc, HT_SystemLastEditUser) VALUES ('{13}', '{11}', 'Some form of Test', '{16}', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.HRJobApplicant (HA_PK, HA_EmailAddress, HA_PER, HA_SystemCreateTimeUtc, HA_SystemCreateUser, HA_SystemLastEditTimeUtc, HA_SystemLastEditUser) VALUES ('{2}', 'ted@cargowise.com', '{15}', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobApplicant (HA_PK, HA_EmailAddress, HA_PER, HA_SystemCreateTimeUtc, HA_SystemCreateUser, HA_SystemLastEditTimeUtc, HA_SystemLastEditUser) VALUES ('{3}', 'madman@gmail.com', '{15}', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobApplicant (HA_PK, HA_EmailAddress, HA_PER, HA_SystemCreateTimeUtc, HA_SystemCreateUser, HA_SystemLastEditTimeUtc, HA_SystemLastEditUser) VALUES ('{9}', 'samuel@test.com', '{15}', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.GlbCompanyCampaignItem (G8_PK, G8_G0, G8_RecipientTableCode, G8_RecipientID, G8_ClosedDateUtc, G8_SystemCreateTimeUtc, G8_SystemCreateUser, G8_SystemLastEditTimeUtc, G8_SystemLastEditUser)
VALUES	('{14}', '{4}', 'HA', '{2}', GETDATE(), GetUtcDate(), 'E', GetUtcDate(), 'E'),
		(NEWID(), '{4}', 'HA', '{3}', GETDATE(), GetUtcDate(), 'E', GetUtcDate(), 'E'),
		(NEWID(), '{4}', 'HA', '{9}', GETDATE(), GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.HRJobApplicantSkillRating (HR_PK, HR_HS, HR_HA, HR_SystemCreateTimeUtc, HR_SystemCreateUser, HR_SystemLastEditTimeUtc, HR_SystemLastEditUser) VALUES ('{5}', '{0}', '{2}', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobApplicantSkillRatingTest (H3_PK, H3_HR, H3_HT, H3_SystemCreateTimeUtc, H3_SystemCreateUser, H3_SystemLastEditTimeUtc, H3_SystemLastEditUser) VALUES (NEWID(), '{5}', '{1}', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.HRJobApplicantSkillRating (HR_PK, HR_HS, HR_HA, HR_SystemCreateTimeUtc, HR_SystemCreateUser, HR_SystemLastEditTimeUtc, HR_SystemLastEditUser) VALUES ('{6}', '{11}', '{3}', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobApplicantSkillRatingTest (H3_PK, H3_HR, H3_HT, H3_SystemCreateTimeUtc, H3_SystemCreateUser, H3_SystemLastEditTimeUtc, H3_SystemLastEditUser) VALUES (NEWID(), '{6}', '{13}', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.HRJobApplicantSkillRating (HR_PK, HR_HS, HR_HA, HR_SystemCreateTimeUtc, HR_SystemCreateUser, HR_SystemLastEditTimeUtc, HR_SystemLastEditUser) VALUES ('{10}', '{0}', '{9}', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobApplicantSkillRatingTest (H3_PK, H3_HR, H3_HT, H3_SystemCreateTimeUtc, H3_SystemCreateUser, H3_SystemLastEditTimeUtc, H3_SystemLastEditUser) VALUES (NEWID(), '{10}', '{1}', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_RL_NKClosestPort) VALUES ('{7}', 'TESTORG', 'Test Organisation', 'AUSYD')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_Email, OC_IsActive, OC_OH) VALUES (NEWID(), 'mm', 'madman@gmail.com', 0, '{7}')
-- same email, slightly different name
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_Email, OC_IsActive, OC_OH) VALUES (NEWID(), 'Mr mm', 'madman@gmail.com', 0, '{7}')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_Email, OC_IsActive, OC_OH) VALUES (NEWID(), 'tb', 'ted@cargowise.com', 1, '{7}')

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_RL_NKClosestPort) VALUES ('{8}', 'TESTORG2', 'Test Organisation 2', 'USLAX')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_Email, OC_IsActive, OC_OH) VALUES (NEWID(), 'scw', 'samuel@test.com', 1, '{8}')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_Email, OC_IsActive, OC_OH) VALUES (NEWID(), 'Mr. mm', 'madman@gmail.com', 1, '{8}')

INSERT INTO dbo.OrgRelatedParty (PR_PK, PR_PartyType, PR_FreightTransportMode, PR_FreightContainerMode, PR_FreightDirection, PR_Service, PR_OH_RelatedParty, PR_OH_Parent) VALUES (NEWID(), 'MNG', '', '', '', '', '{7}', '{7}')
INSERT INTO dbo.OrgRelatedParty (PR_PK, PR_PartyType, PR_FreightTransportMode, PR_FreightContainerMode, PR_FreightDirection, PR_Service, PR_OH_RelatedParty, PR_OH_Parent) VALUES (NEWID(), 'MNG', '', '', '', '', '{7}', '{8}')

INSERT INTO dbo.LicenceEnterprise (LE_EnterpriseID, LE_PK, LE_OH, LE_SystemCreateTimeUtc, LE_SystemCreateUser, LE_SystemLastEditTimeUtc, LE_SystemLastEditUser ) VALUES ('LE001', '{12}', '{7}', getutcdate(), 'E', getutcdate(), 'E')
INSERT INTO dbo.LicenceCompany (LC_PK, LC_LE, LC_OH, LC_CompanyCode) VALUES (newid(), '{12}', '{7}', 'CO1')
", skillGuid1, testGuid1, tedGuid, madmanGuid, campaignGuid, skillRatingGuid1, skillRatingGuid2, orgGuid, org2Guid, samuelGuid, skillRatingGuid3, skillGuid2, licEnterpriseGuild, testGuid2, tedCampaignItemGuid, personPk, examSettings);
			TestConnection.ExecuteNonQuery(commandText);
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT * FROM Report_CompetencyCompletionResults('{0}', '', NULL,DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT)", orgGuid));
			AssertEquals("Result should have no row because Ted is Cargowise, which is excluded", 0, result.Rows.Count);
			commandText = "UPDATE dbo.OrgContact SET OC_IsActive = 1 WHERE OC_Email = 'madman@gmail.com'";
			TestConnection.ExecuteNonQuery(commandText);
			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT * FROM Report_CompetencyCompletionResults('{0}', '', NULL,DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT)", orgGuid));
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals("sdf", result.Rows[0][0]);
			AssertEquals("madman@gmail.com", result.Rows[0][1]);
			commandText = string.Format("UPDATE dbo.OrgHeader SET OH_FullName = 'CargoWise Testing' WHERE OH_PK = '{0}'", orgGuid);
			TestConnection.ExecuteNonQuery(commandText);
			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT * FROM Report_CompetencyCompletionResults('{0}', '', NULL,DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT) ORDER BY ApplicantName", orgGuid));
			AssertEquals("Result should have two rows", 2, result.Rows.Count);
			AssertEquals("sdf", result.Rows[0][0]);
			AssertEquals("sdf", result.Rows[1][0]);
			AssertContainsExactElementsInAnyOrder(new[] { "ted@cargowise.com", "madman@gmail.com" }, new[] { result.Rows[0][1], result.Rows[1][1] });
			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT * FROM Report_CompetencyCompletionResults('{0}', '', '{1}',DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT) ORDER BY ApplicantName", orgGuid, skillGuid2));
			AssertEquals("Result should have three rows", 1, result.Rows.Count);
			AssertEquals("sdf", result.Rows[0][0]);
			AssertEquals("madman@gmail.com", result.Rows[0][1]);
			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT * FROM Report_CompetencyCompletionResults('{0}', '', '{1}',DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT) ORDER BY ApplicantName", orgGuid, skillGuid1));
			AssertEquals("Result should have three rows", 1, result.Rows.Count);
			AssertEquals("sdf", result.Rows[0][0]);
			AssertEquals("ted@cargowise.com", result.Rows[0][1]);
			var sql = $@"
INSERT INTO dbo.ExamAttempt (EXA_PK, EXA_G8, EXA_Score, EXA_TestCommencedUtc, EXA_TestCompletedUtc)
VALUES (NEWID(), '{tedCampaignItemGuid}', 81, '2016-2-1', '2016-2-2');
";
			using (var cmd2 = TestConnection.Command(sql))
			{
				cmd2.ExecuteNonQuery();
			}

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ApplicantName, ApplicantEmailAddress, CompetencyCommenceDate, CompetencyCompletionDate, IsCompleted FROM Report_CompetencyCompletionResults('{0}', '', '{1}',DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT) ORDER BY ApplicantName", orgGuid, skillGuid1));
			AssertEquals(1, result.Rows.Count);
			AssertEquals("sdf", result.Rows[0][0]);
			AssertEquals("ted@cargowise.com", result.Rows[0][1]);
			AssertEquals(new DateTime(2016, 2, 1), result.Rows[0][2]);
			AssertEquals(new DateTime(2016, 2, 2), result.Rows[0][3]);
			AssertEquals("Completed", result.Rows[0][4]);
		}

		#endregion
		#region TestReport_CertificationStatus
		public void TestReport_CertificationStatus()
		{
			Guid tedGuid = Guid.NewGuid();
			Guid madmanGuid = Guid.NewGuid();
			Guid samuelGuid = Guid.NewGuid();
			Guid certGuid = Guid.NewGuid();
			Guid certGuid2 = Guid.NewGuid();
			Guid certGuid3 = Guid.NewGuid();
			Guid orgGuid = Guid.NewGuid();
			Guid org2Guid = Guid.NewGuid();
			Guid licEnterpriseGuid = Guid.NewGuid();
			var personPk = Guid.NewGuid();
			string commandText = string.Format(@"
INSERT into dbo.GlbPerson (PER_PK, PER_FullName) VALUES ('{9}', 'sdf')
INSERT INTO dbo.HRJobApplicant (HA_PK, HA_EmailAddress, HA_PER, HA_SystemCreateTimeUtc, HA_SystemCreateUser, HA_SystemLastEditTimeUtc, HA_SystemLastEditUser) VALUES ('{0}', 'ted@cargowise.com', '{9}', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobApplicant (HA_PK, HA_EmailAddress, HA_PER, HA_SystemCreateTimeUtc, HA_SystemCreateUser, HA_SystemLastEditTimeUtc, HA_SystemLastEditUser) VALUES ('{1}', 'madman@gmail.com', '{9}', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobApplicant (HA_PK, HA_EmailAddress, HA_PER, HA_SystemCreateTimeUtc, HA_SystemCreateUser, HA_SystemLastEditTimeUtc, HA_SystemLastEditUser) VALUES ('{2}', 'samuel@test.com', '{9}', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.GenRegCertAccredMaintList (XZ_PK, XZ_ParentID, XZ_Type, XZ_IssueDate, XZ_ExpiryOrDueDate, XZ_RefNumber) VALUES ('{3}', '{0}', 'CUS', '2017-8-2', '2017-8-3', 'A001')
INSERT INTO dbo.GenRegCertAccredMaintList (XZ_PK, XZ_ParentID, XZ_Type, XZ_IssueDate, XZ_ExpiryOrDueDate, XZ_RefNumber) VALUES ('{4}', '{1}', 'CUS', '2017-8-1', '2017-8-2', 'A002')
INSERT INTO dbo.GenRegCertAccredMaintList (XZ_PK, XZ_ParentID, XZ_Type, XZ_IssueDate, XZ_ExpiryOrDueDate, XZ_RefNumber) VALUES ('{8}', '{1}', 'PIP', '2017-8-1', '2017-8-2', 'A003')

INSERT INTO dbo.GenRegCertAccredMaintList (XZ_PK, XZ_ParentID, XZ_Type, XZ_IssueDate, XZ_ExpiryOrDueDate, XZ_RefNumber) VALUES (NEWID(), '{0}', 'CUS', '2017-8-2', '2017-8-3', '')
INSERT INTO dbo.GenRegCertAccredMaintList (XZ_PK, XZ_ParentID, XZ_Type, XZ_IssueDate, XZ_ExpiryOrDueDate, XZ_RefNumber) VALUES (NEWID(), '{1}', 'CUS', '2017-8-1', '2017-8-2', '')
INSERT INTO dbo.GenRegCertAccredMaintList (XZ_PK, XZ_ParentID, XZ_Type, XZ_IssueDate, XZ_ExpiryOrDueDate, XZ_RefNumber) VALUES (NEWID(), '{1}', 'PIP', '2017-8-1', '2017-8-2', '')

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_RL_NKClosestPort) VALUES ('{5}', 'TESTORG', 'Test Organisation', 'AUSYD')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_Email, OC_IsActive, OC_OH) VALUES (NEWID(), 'mm', 'madman@gmail.com', 0, '{5}')
-- same email, slightly different name
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_Email, OC_IsActive, OC_OH) VALUES (NEWID(), 'Mr mm', 'madman@gmail.com', 0, '{5}')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_Email, OC_IsActive, OC_OH) VALUES (NEWID(), 'tb', 'ted@cargowise.com', 1, '{5}')

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_RL_NKClosestPort) VALUES ('{6}', 'TESTORG2', 'Test Organisation 2', 'USLAX')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_Email, OC_IsActive, OC_OH) VALUES (NEWID(), 'scw', 'samuel@test.com', 1, '{6}')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_Email, OC_IsActive, OC_OH) VALUES (NEWID(), 'Mr. mm', 'madman@gmail.com', 1, '{6}')

INSERT INTO dbo.OrgRelatedParty (PR_PK, PR_PartyType, PR_FreightTransportMode, PR_FreightContainerMode, PR_FreightDirection, PR_Service, PR_OH_RelatedParty, PR_OH_Parent) VALUES (NEWID(), 'MNG', '', '', '', '', '{5}', '{5}')
INSERT INTO dbo.OrgRelatedParty (PR_PK, PR_PartyType, PR_FreightTransportMode, PR_FreightContainerMode, PR_FreightDirection, PR_Service, PR_OH_RelatedParty, PR_OH_Parent) VALUES (NEWID(), 'MNG', '', '', '', '', '{5}', '{6}')

INSERT INTO dbo.LicenceEnterprise (LE_EnterpriseID, LE_PK, LE_OH, LE_SystemCreateTimeUtc, LE_SystemCreateUser, LE_SystemLastEditTimeUtc, LE_SystemLastEditUser) VALUES ('LE002', '{7}', '{5}', getutcdate(), 'E', getutcdate(), 'E')
INSERT INTO dbo.LicenceCompany (LC_PK, LC_LE, LC_OH, LC_CompanyCode) VALUES (newid(), '{7}', '{5}', 'CO1')
", tedGuid, madmanGuid, samuelGuid, certGuid, certGuid2, orgGuid, org2Guid, licEnterpriseGuid, certGuid3, personPk);
			TestConnection.ExecuteNonQuery(commandText);
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT * FROM Report_CertificationStatus('{0}', '', DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT)", orgGuid));
			AssertEquals("Result should have no row because Ted is Cargowise, which is excluded", 0, result.Rows.Count);
			commandText = "UPDATE dbo.OrgContact SET OC_IsActive = 1 WHERE OC_Email = 'madman@gmail.com'";
			TestConnection.ExecuteNonQuery(commandText);
			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT * FROM Report_CertificationStatus('{0}', '', DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT)", orgGuid));
			AssertEquals("Result should have two rows", 2, result.Rows.Count);
			AssertEquals("sdf", result.Rows[0][0]);
			AssertEquals("sdf", result.Rows[1][0]);
			AssertContainsExactElementsInAnyOrder(new[] { "madman@gmail.com", "madman@gmail.com" }, new[] { result.Rows[0][1], result.Rows[1][1] });
			commandText = string.Format("UPDATE dbo.OrgHeader SET OH_FullName = 'CargoWise Testing' WHERE OH_PK = '{0}'", orgGuid);
			TestConnection.ExecuteNonQuery(commandText);
			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT * FROM Report_CertificationStatus('{0}', '', DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT) ORDER BY ApplicantName", orgGuid));
			AssertEquals("Result should have three rows", 3, result.Rows.Count);
			AssertEquals("sdf", result.Rows[0][0]);
			AssertEquals("sdf", result.Rows[1][0]);
			AssertEquals("sdf", result.Rows[2][0]);
			AssertContainsExactElementsInAnyOrder(new[] { "ted@cargowise.com", "madman@gmail.com", "madman@gmail.com" }, new[] { result.Rows[0][1], result.Rows[1][1], result.Rows[2][1] });
			AssertContainsExactElementsInAnyOrder(new[] { "ted@cargowise.com", "madman@gmail.com", "madman@gmail.com" }, new[] { result.Rows[0][1], result.Rows[1][1], result.Rows[2][1] });
			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT * FROM Report_CertificationStatus('{0}', '', DEFAULT,'{1}',DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT) ORDER BY ApplicantName", orgGuid, licEnterpriseGuid));
			AssertEquals("Result should have three rows", 3, result.Rows.Count);
			AssertEquals("sdf", result.Rows[0][0]);
			AssertEquals("sdf", result.Rows[1][0]);
			AssertEquals("sdf", result.Rows[2][0]);
			AssertContainsExactElementsInAnyOrder(new[] { "ted@cargowise.com", "madman@gmail.com", "madman@gmail.com" }, new[] { result.Rows[0][1], result.Rows[1][1], result.Rows[2][1] });
			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT * FROM Report_CertificationStatus('{0}', '', DEFAULT,DEFAULT,'CUS',DEFAULT,DEFAULT,DEFAULT,DEFAULT) ORDER BY ApplicantName", orgGuid));
			AssertEquals("Result should have three rows", 2, result.Rows.Count);
			AssertEquals("sdf", result.Rows[0][0]);
			AssertEquals("sdf", result.Rows[1][0]);
			AssertContainsExactElementsInAnyOrder(new[] { "ted@cargowise.com", "madman@gmail.com" }, new[] { result.Rows[0][1], result.Rows[1][1] });
			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT * FROM Report_CertificationStatus('{0}', 'Y',DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT) ORDER BY ApplicantName", orgGuid));
			AssertEquals("Result should have five rows including applicants from managed orgs (Since Mad Man is an OrgContact for 2 matching Orgs, his 2 certificates are returned twice", 5, result.Rows.Count);
			AssertEquals("sdf", result.Rows[0][0]);
			AssertEquals("sdf", result.Rows[3][0]);
			AssertEquals("sdf", result.Rows[4][0]);
			AssertContainsExactElementsInAnyOrder(new[] { "ted@cargowise.com", "madman@gmail.com", "madman@gmail.com", "PIP", "PIP" }, new[] { result.Rows[0][1], result.Rows[1][1], result.Rows[2][1], result.Rows[3][3], result.Rows[4][3] });
			AssertContainsExactElementsInAnyOrder(new[] { "PIP", "CUS", "CUS", "CUS", "PIP" }, new[] { result.Rows[0][3], result.Rows[1][3], result.Rows[2][3], result.Rows[3][3], result.Rows[4][3] });
			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ApplicantEmailAddress, CertificateIssueDate, CertificateExpiryOrDueDate FROM Report_CertificationStatus('{0}', '', '{1}',DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT) ORDER BY ApplicantName", org2Guid, madmanGuid));
			AssertEquals("Result should only return Mad Man", 2, result.Rows.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "madman@gmail.com", "madman@gmail.com" }, new[] { result.Rows[0][0], result.Rows[1][0] });
			AssertEquals(new DateTime(2017, 8, 1), result.Rows[0][1]);
			AssertEquals(new DateTime(2017, 8, 2), result.Rows[0][2]);
			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ApplicantEmailAddress, CertificateIssueDate, CertificateExpiryOrDueDate FROM Report_CertificationStatus('{0}', '', DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT) ORDER BY ApplicantName", org2Guid));
			AssertEquals("Result should return only Mad Man since Samuel has no certificates", 2, result.Rows.Count);
			AssertEquals(new DateTime(2017, 8, 1), result.Rows[0][1]);
			AssertEquals(new DateTime(2017, 8, 2), result.Rows[0][2]);
			AssertEquals(new DateTime(2017, 8, 1), result.Rows[1][1]);
			AssertEquals(new DateTime(2017, 8, 2), result.Rows[1][2]);
			AssertContainsExactElementsInAnyOrder(new[] { "madman@gmail.com", "madman@gmail.com" }, new[] { result.Rows[0][0], result.Rows[1][0] });
			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ApplicantEmailAddress, CertificateIssueDate, CertificateExpiryOrDueDate FROM Report_CertificationStatus('{0}', '', DEFAULT,DEFAULT,DEFAULT,'2017-7-31','2017-8-2','2017-8-1','2017-8-3') ORDER BY ApplicantName", org2Guid));
			AssertEquals("Result should only return Mad Man", 2, result.Rows.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "madman@gmail.com", "madman@gmail.com" }, new[] { result.Rows[0][0], result.Rows[1][0] });
			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ApplicantEmailAddress, CertificateIssueDate, CertificateExpiryOrDueDate FROM Report_CertificationStatus('{0}', '', DEFAULT,DEFAULT,DEFAULT,'2017-7-31','2017-8-2','2017-8-4','2017-8-5') ORDER BY ApplicantName", org2Guid));
			AssertEquals("Result should have no row because no one has this expiry date", 0, result.Rows.Count);
		}

		#endregion
		#region TestReport_ApplicantExamResults
		public void TestReport_ApplicantExamResults()
		{
			Guid orgGuid = Guid.NewGuid();
			Guid org2Guid = Guid.NewGuid();
			Guid skillGuid1 = Guid.NewGuid();
			Guid skillGuid2 = Guid.NewGuid();
			Guid testGuid1 = Guid.NewGuid();
			Guid testGuid2 = Guid.NewGuid();
			Guid skillRatingGuid1 = Guid.NewGuid();
			Guid skillRatingGuid2 = Guid.NewGuid();
			Guid skillRatingGuid3 = Guid.NewGuid();
			Guid campaignGuid = Guid.NewGuid();
			Guid tedGuid = Guid.NewGuid();
			Guid madmanGuid = Guid.NewGuid();
			Guid samuelGuid = Guid.NewGuid();
			Guid licEnterpriseGuild = Guid.NewGuid();
			var tedCampaignItemGuid = Guid.NewGuid();
			var personPk = Guid.NewGuid();
			var personPk2 = Guid.NewGuid();
			var settingPk = Guid.NewGuid();
			var primaryContactPk1 = Guid.NewGuid();
			var primaryContactPk2 = Guid.NewGuid();
			var address1 = Guid.NewGuid();
			var address2 = Guid.NewGuid();
			var address3 = Guid.NewGuid();
			var address4 = Guid.NewGuid();
			string commandText = string.Format(@"
INSERT into dbo.GlbPerson (PER_PK, PER_FullName) VALUES ('{15}', 'sdf')
INSERT into dbo.GlbPerson (PER_PK, PER_FullName) VALUES ('{23}', 'xxx')
INSERT INTO dbo.GlbCompanyCampaign (G0_PK, G0_GC, G0_CampaignName, G0_CampaignID, G0_SystemCreateTimeUtc, G0_SystemCreateUser, G0_SystemLastEditTimeUtc, G0_SystemLastEditUser)
VALUES ('{4}', '03052ED3-2C64-49AC-97D8-C6079D5015B5', '~test~name!', '234', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobSkill (HS_PK, HS_SkillDescription, HS_Code, HS_SystemCreateTimeUtc, HS_SystemCreateUser, HS_SystemLastEditTimeUtc, HS_SystemLastEditUser) VALUES ('{0}', 'Some Skill', 'HS1', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobSkill (HS_PK, HS_SkillDescription, HS_Code, HS_SystemCreateTimeUtc, HS_SystemCreateUser, HS_SystemLastEditTimeUtc, HS_SystemLastEditUser) VALUES ('{11}', 'Some Skill', 'HS2', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.ExamSetting (EXS_PK, EXS_Code, EXS_G0) VALUES ('{16}', 'ZZZ', '{4}')

INSERT INTO dbo.HRJobSkillTest (HT_PK, HT_HS, HT_TestName, HT_EXS, HT_SystemCreateTimeUtc, HT_SystemCreateUser, HT_SystemLastEditTimeUtc, HT_SystemLastEditUser) VALUES ('{1}', '{0}', 'Some form of Test', '{16}', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobSkillTest (HT_PK, HT_HS, HT_TestName, HT_EXS, HT_SystemCreateTimeUtc, HT_SystemCreateUser, HT_SystemLastEditTimeUtc, HT_SystemLastEditUser) VALUES ('{13}', '{11}', 'Some form of Test', '{16}', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.HRJobApplicant (HA_PK, HA_EmailAddress, HA_PER, HA_SystemCreateTimeUtc, HA_SystemCreateUser, HA_SystemLastEditTimeUtc, HA_SystemLastEditUser) VALUES ('{2}', 'ted@cargowise.com', '{15}', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobApplicant (HA_PK, HA_EmailAddress, HA_PER, HA_SystemCreateTimeUtc, HA_SystemCreateUser, HA_SystemLastEditTimeUtc, HA_SystemLastEditUser) VALUES ('{3}', 'madman@gmail.com', '{15}', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobApplicant (HA_PK, HA_EmailAddress, HA_PER, HA_SystemCreateTimeUtc, HA_SystemCreateUser, HA_SystemLastEditTimeUtc, HA_SystemLastEditUser) VALUES ('{9}', 'samuel@test.com', '{15}', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.GlbCompanyCampaignItem (G8_PK, G8_G0, G8_RecipientTableCode, G8_RecipientID, G8_ClosedDateUtc, G8_SystemCreateTimeUtc, G8_SystemCreateUser, G8_SystemLastEditTimeUtc, G8_SystemLastEditUser)
VALUES	('{14}', '{4}', 'HA', '{2}', GETDATE(), GetUtcDate(), 'E', GetUtcDate(), 'E'),
		(NEWID(), '{4}', 'HA', '{3}', GETDATE(), GetUtcDate(), 'E', GetUtcDate(), 'E'),
		(NEWID(), '{4}', 'HA', '{9}', GETDATE(), GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.HRJobApplicantSkillRating (HR_PK, HR_HS, HR_HA, HR_SystemCreateTimeUtc, HR_SystemCreateUser, HR_SystemLastEditTimeUtc, HR_SystemLastEditUser) VALUES ('{5}', '{0}', '{2}', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobApplicantSkillRatingTest (H3_PK, H3_HR, H3_HT, H3_SystemCreateTimeUtc, H3_SystemCreateUser, H3_SystemLastEditTimeUtc, H3_SystemLastEditUser) VALUES (NEWID(), '{5}', '{1}', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.HRJobApplicantSkillRating (HR_PK, HR_HS, HR_HA, HR_SystemCreateTimeUtc, HR_SystemCreateUser, HR_SystemLastEditTimeUtc, HR_SystemLastEditUser) VALUES ('{6}', '{11}', '{3}', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobApplicantSkillRatingTest (H3_PK, H3_HR, H3_HT, H3_SystemCreateTimeUtc, H3_SystemCreateUser, H3_SystemLastEditTimeUtc, H3_SystemLastEditUser) VALUES (NEWID(), '{6}', '{13}', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.HRJobApplicantSkillRating (HR_PK, HR_HS, HR_HA, HR_SystemCreateTimeUtc, HR_SystemCreateUser, HR_SystemLastEditTimeUtc, HR_SystemLastEditUser) VALUES ('{10}', '{0}', '{9}', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobApplicantSkillRatingTest (H3_PK, H3_HR, H3_HT, H3_SystemCreateTimeUtc, H3_SystemCreateUser, H3_SystemLastEditTimeUtc, H3_SystemLastEditUser) VALUES (NEWID(), '{10}', '{1}', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_RL_NKClosestPort) VALUES ('{7}', 'TESTORG', 'Test Organisation', 'AUSYD')
INSERT INTO dbo.OrgAddress (OA_PK, OA_Code, OA_Address1, OA_CompanyNameOverride, OA_OH, OA_RL_NKRelatedPortCode) values ('{19}', 'AD1', 'address 1', 'company 1', '{7}', 'AUMEL')
INSERT INTO dbo.OrgAddress (OA_PK, OA_Code, OA_Address1, OA_OH, OA_RL_NKRelatedPortCode) values ('{20}', 'AD2', 'address 2', '{7}', 'AUBNE')

INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_Email, OC_IsActive, OC_OH, OC_PER) VALUES (NEWID(), 'mm', 'madman@gmail.com', 0, '{7}', '{15}')
-- same email, slightly different name
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_Email, OC_IsActive, OC_OH, OC_PER) VALUES ('{17}', 'Mr mm', 'madman@gmail.com', 0, '{7}', '{15}')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_Email, OC_IsActive, OC_OH, OC_PER, OC_OA_OrgAddress) VALUES ('{18}', 'tb', 'ted@cargowise.com', 1, '{7}', '{15}', '{20}')

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_RL_NKClosestPort) VALUES ('{8}', 'TESTORG2', 'Test Organisation 2', 'USLAX')
INSERT INTO dbo.OrgAddress (OA_PK, OA_Code, OA_Address1, OA_CompanyNameOverride, OA_OH, OA_RL_NKRelatedPortCode) values ('{21}', 'AD3', 'address 3', 'company 3', '{8}', 'GBLON')
INSERT INTO dbo.OrgAddress (OA_PK, OA_Code, OA_Address1, OA_CompanyNameOverride, OA_OH) values ('{22}', 'AD4', 'address 4', 'company 4', '{8}')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_Email, OC_IsActive, OC_OH) VALUES (NEWID(), 'scw', 'samuel@test.com', 1, '{8}')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_Email, OC_IsActive, OC_OH, OC_PER, OC_OA_OrgAddress) VALUES (newid(), 'Mr. mm', 'madman@gmail.com', 1, '{8}', '{15}', '{22}')

INSERT INTO dbo.GlbPersonPrimaryRelationship (PPR_PK, PPR_PER, PPR_PrimaryId, PPR_PrimaryTableCode) values (newid(), '{15}', '{17}', 'OC')
INSERT INTO dbo.GlbPersonPrimaryRelationship (PPR_PK, PPR_PER, PPR_PrimaryId, PPR_PrimaryTableCode) values (newid(), '{23}', '{18}', 'OC')

INSERT INTO dbo.OrgRelatedParty (PR_PK, PR_PartyType, PR_FreightTransportMode, PR_FreightDirection, PR_Service, PR_OH_RelatedParty, PR_OH_Parent) VALUES (NEWID(), 'MNG', '', '', '', '{7}', '{7}')
INSERT INTO dbo.OrgRelatedParty (PR_PK, PR_PartyType, PR_FreightTransportMode, PR_FreightDirection, PR_Service, PR_OH_RelatedParty, PR_OH_Parent) VALUES (NEWID(), 'MNG', '', '', '', '{7}', '{8}')

INSERT INTO dbo.LicenceEnterprise (LE_EnterpriseID, LE_PK, LE_OH, LE_SystemCreateTimeUtc, LE_SystemCreateUser, LE_SystemLastEditTimeUtc, LE_SystemLastEditUser ) VALUES ('LE003', '{12}', '{7}', getutcdate(), 'E', getutcdate(), 'E')
INSERT INTO dbo.LicenceCompany (LC_PK, LC_LE, LC_OH, LC_CompanyCode) VALUES (newid(), '{12}', '{7}', 'CO1')
", skillGuid1, testGuid1, tedGuid, madmanGuid, campaignGuid, skillRatingGuid1, skillRatingGuid2, orgGuid, org2Guid, samuelGuid, skillRatingGuid3, skillGuid2, licEnterpriseGuild, testGuid2, tedCampaignItemGuid, personPk, settingPk, primaryContactPk1, primaryContactPk2, address1, address2, address3, address4, personPk2);
			TestConnection.ExecuteNonQuery(commandText);
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT * FROM Report_ApplicantExamResults('{0}', '', NULL, DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT)", orgGuid));
			AssertEquals("Result should have no row because Ted is Cargowise, which is excluded", 0, result.Rows.Count);
			commandText = "UPDATE dbo.OrgContact SET OC_IsActive = 1 WHERE OC_Email = 'madman@gmail.com'";
			TestConnection.ExecuteNonQuery(commandText);
			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT PersonName, PersonEmailAddress, WorkingLocation, Branch, PrimaryWorkplace FROM Report_ApplicantExamResults('{0}', '', NULL, DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT)", orgGuid));
			AssertEquals("Result should have one row", 3, result.Rows.Count);
			commandText = string.Format("UPDATE dbo.OrgHeader SET OH_FullName = 'CargoWise Testing' WHERE OH_PK = '{0}'", orgGuid);
			TestConnection.ExecuteNonQuery(commandText);
			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT PersonName, PersonEmailAddress, WorkingLocation, Branch, PrimaryWorkplace FROM Report_ApplicantExamResults('{0}', '', NULL, DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT) ORDER BY PersonName", orgGuid));
			AssertEquals("Result should have two rows", 3, result.Rows.Count);
			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT PersonName, PersonEmailAddress, WorkingLocation, Branch, PrimaryWorkplace FROM Report_ApplicantExamResults('{0}', '', '{1}', DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT) ORDER BY PersonName", orgGuid, skillGuid2));
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals("sdf", result.Rows[0][0]);
			AssertEquals("madman@gmail.com", result.Rows[0][1]);
			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT * FROM Report_ApplicantExamResults('{0}', '', '{1}', DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT) ORDER BY PersonName", orgGuid, skillGuid1));
			AssertEquals("Result should have one row", 2, result.Rows.Count);
			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT * FROM Report_ApplicantExamResults('{0}', 'Y', NULL, DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT) ORDER BY PersonName", orgGuid));
			AssertEquals("Result should have four rows including applicants from managed orgs", 6, result.Rows.Count);
			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT * FROM Report_ApplicantExamResults('{0}', 'Y', '{1}', DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT) ORDER BY PersonName", orgGuid, skillGuid2));
			AssertEquals("Result should have two rows including applicants from managed orgs", 2, result.Rows.Count);
			AssertEquals("sdf", result.Rows[0][0]);
			AssertEquals("sdf", result.Rows[1][0]);
			AssertContainsExactElementsInAnyOrder(new[] { "madman@gmail.com", "madman@gmail.com" }, new[] { result.Rows[0][1], result.Rows[1][1] });
			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT * FROM Report_ApplicantExamResults('{0}', 'Y', '{1}', DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT,DEFAULT) ORDER BY PersonName", orgGuid, skillGuid1));
			AssertEquals("Result should have two rows including applicants from managed orgs", 4, result.Rows.Count);
		}

		#endregion
		#region TestReport_OrgExamResults
		public void TestReport_OrgExamResults()
		{
			var person = Guid.NewGuid();
			var org1 = Guid.NewGuid();
			var org2 = Guid.NewGuid();
			var exam = Guid.NewGuid();
			var skill = Guid.NewGuid();
			var examSetting = Guid.NewGuid();
			var test = Guid.NewGuid();
			var applicant = Guid.NewGuid();
			var campaignItem = Guid.NewGuid();
			var rating = Guid.NewGuid();
			var contact1 = Guid.NewGuid();
			var contact2 = Guid.NewGuid();
			var address1 = Guid.NewGuid();
			var address2 = Guid.NewGuid();
			var address3 = Guid.NewGuid();
			var address4 = Guid.NewGuid();
			var relation = Guid.NewGuid();
			string sql = $@"
insert into dbo.GlbPerson (PER_PK, PER_FullName) values ('{person}', 'name')

INSERT INTO dbo.GlbCompanyCampaign (G0_PK, G0_GC, G0_CampaignName, G0_CampaignID, G0_SystemCreateTimeUtc, G0_SystemCreateUser, G0_SystemLastEditTimeUtc, G0_SystemLastEditUser)
VALUES ('{exam}', '03052ED3-2C64-49AC-97D8-C6079D5015B5', '~test~name!', '234', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobSkill (HS_PK, HS_SkillDescription, HS_Code, HS_SystemCreateTimeUtc, HS_SystemCreateUser, HS_SystemLastEditTimeUtc, HS_SystemLastEditUser) VALUES ('{skill}', 'Some Skill', 'HS1', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.ExamSetting (EXS_PK, EXS_Code, EXS_G0) VALUES ('{examSetting}', 'ZZZ', '{exam}')

INSERT INTO dbo.HRJobSkillTest (HT_PK, HT_HS, HT_TestName, HT_EXS, HT_SystemCreateTimeUtc, HT_SystemCreateUser, HT_SystemLastEditTimeUtc, HT_SystemLastEditUser) VALUES ('{test}', '{skill}', 'Some form of Test', '{examSetting}', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.HRJobApplicant (HA_PK, HA_EmailAddress, HA_PER, HA_SystemCreateTimeUtc, HA_SystemCreateUser, HA_SystemLastEditTimeUtc, HA_SystemLastEditUser) VALUES ('{applicant}', 'anton@gmail.com', '{person}', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.GlbCompanyCampaignItem (G8_PK, G8_G0, G8_RecipientTableCode, G8_RecipientID, G8_ClosedDateUtc, G8_SystemCreateTimeUtc, G8_SystemCreateUser, G8_SystemLastEditTimeUtc, G8_SystemLastEditUser)
VALUES ('{campaignItem}', '{exam}', 'HA', '{applicant}', GETDATE(), GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.HRJobApplicantSkillRating (HR_PK, HR_HS, HR_HA, HR_SystemCreateTimeUtc, HR_SystemCreateUser, HR_SystemLastEditTimeUtc, HR_SystemLastEditUser) VALUES ('{rating}', '{skill}', '{applicant}', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobApplicantSkillRatingTest (H3_PK, H3_HR, H3_HT, H3_SystemCreateTimeUtc, H3_SystemCreateUser, H3_SystemLastEditTimeUtc, H3_SystemLastEditUser) VALUES (NEWID(), '{rating}', '{test}', GetUtcDate(), 'E', GetUtcDate(), 'E')

insert into dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_RL_NKClosestPort) values ('{org1}', '~~1', '~~1 name', 'AUSYD')
INSERT INTO dbo.OrgAddress (OA_PK, OA_Code, OA_Address1, OA_OH, OA_RL_NKRelatedPortCode) values ('{address1}', 'main 1', 'address 1', '{org1}', 'AUMEL')
INSERT INTO dbo.OrgAddress (OA_PK, OA_Code, OA_Address1, OA_CompanyNameOverride, OA_OH, OA_RL_NKRelatedPortCode) values ('{address2}', 'AD2', 'address 2', 'name override 2', '{org1}', 'AUBNE')

insert into dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_RL_NKClosestPort) values ('{org2}', '~~2', '~~2 name', 'AUOOL')
INSERT INTO dbo.OrgAddress (OA_PK, OA_Code, OA_Address1, OA_OH, OA_RL_NKRelatedPortCode) values ('{address3}', 'main 2', 'address 3', '{org2}', 'GBLON')
INSERT INTO dbo.OrgAddress (OA_PK, OA_Code, OA_Address1, OA_CompanyNameOverride, OA_OH, OA_RL_NKRelatedPortCode) values ('{address4}', 'AD4', 'address 4', 'name override 4', '{org2}', 'UAIEV')

INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_Email, OC_IsActive, OC_OH, OC_PER, OC_OA_OrgAddress) VALUES ('{contact1}', 'contact 1', 'anton@gmail.com', 1, '{org1}', '{person}', '{address2}')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_Email, OC_IsActive, OC_OH, OC_PER) VALUES ('{contact2}', 'contact 2', 'anton@gmail.com', 1, '{org2}', '{person}')

INSERT INTO dbo.GlbPersonPrimaryRelationship (PPR_PK, PPR_PER, PPR_PrimaryId, PPR_PrimaryTableCode) values ('{relation}', '{person}', '{contact2}', 'OC')
";
			TestConnection.ExecuteNonQuery(sql);
			var result = GetReport_OrgExamResults(org1, Enumerable.Empty<string>());
			AssertEquals("Should return no rows because primary relationship is org2", 0, result.Rows.Count);
			result = GetReport_OrgExamResults(org2, Enumerable.Empty<string>());
			AssertEquals("Should return one row because primary relationship is org2", 1, result.Rows.Count);
			AssertEquals("name", result.Rows[0][0]);
			AssertEquals("anton@gmail.com", result.Rows[0][1]);
			AssertEquals("GBLON", result.Rows[0][2]);
			AssertEquals("main 2", result.Rows[0][3]);
			AssertEquals("~~2 name", result.Rows[0][4]);
			TestConnection.ExecuteNonQuery($"update dbo.GlbPersonPrimaryRelationship set PPR_PrimaryId = '{contact1}', PPR_SystemLastEditUser = 'E', PPR_SystemLastEditTimeUtc = GetDate() WHERE PPR_PK = '{relation}'");
			result = GetReport_OrgExamResults(org1, Enumerable.Empty<string>());
			AssertEquals("Should return one row because primary relationship is now org1", 1, result.Rows.Count);
			AssertEquals("name", result.Rows[0][0]);
			AssertEquals("anton@gmail.com", result.Rows[0][1]);
			AssertEquals("AUBNE", result.Rows[0][2]);
			AssertEquals("AD2", result.Rows[0][3]);
			AssertEquals("name override 2", result.Rows[0][4]);
			AssertEquals("Working location AU should match", 1, GetReport_OrgExamResults(org1, new[] { "AU" }).Rows.Count);
			AssertEquals("Working location AUBNE should match", 1, GetReport_OrgExamResults(org1, new[] { "AUBNE" }).Rows.Count);
			AssertEquals("Working location AUSYD should not match", 0, GetReport_OrgExamResults(org1, new[] { "AUSYD" }).Rows.Count);
			AssertEquals("Working location AU should match", 1, GetReport_OrgExamResults(org1, new[] { "AU", "ZACPT" }).Rows.Count);
		}

		DataTable GetReport_OrgExamResults(Guid orgPk, IEnumerable<string> workingLocationCountryPorts)
		{
			var command = TestConnection.Command(FormattableString.Invariant($"SELECT PersonName, PersonEmailAddress, WorkingLocation, Branch, PrimaryWorkplace FROM Report_OrgExamResults(@OrgPk, NULL, DEFAULT, DEFAULT, @WorkingCountryPortCodes, @WorkingCountryPortCodesIsEmpty, DEFAULT,DEFAULT,DEFAULT,DEFAULT)"));
			command.AddParameter("@OrgPk", SqlDbType.UniqueIdentifier, orgPk);
			command.AddTableValuedParameter("@WorkingCountryPortCodes", "dbo.TVP_varchar_250", workingLocationCountryPorts);
			command.AddParameter("@WorkingCountryPortCodesIsEmpty", SqlDbType.Bit, (workingLocationCountryPorts ?? Enumerable.Empty<string>()).Any() ? 0 : 1);
			return DataUtils.GetDataTableFromCommand(command);
		}

		#endregion

		#region TestReport_StlPriceListDiscounts

		public void TestReport_StlPriceListDiscounts()
		{
			var orgHeader1 = Guid.NewGuid();
			var orgHeader2 = Guid.NewGuid();
			var licenceEnterprise1 = Guid.NewGuid();
			var licenceEnterprise2 = Guid.NewGuid();
			var licenceCompany1 = Guid.NewGuid();
			var licenceCompany2 = Guid.NewGuid();
			var licenceDatabase = Guid.NewGuid();
			var clientLicencePriceHeader = Guid.NewGuid();

			string sql = $@"
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_IsActive)
VALUES	('{orgHeader1}', 'TESTORGSTL', 1),
		('{orgHeader2}', 'TESTORGSTL2', 1)

INSERT INTO dbo.LicenceEnterprise (LE_PK, LE_EnterpriseCode, LE_EnterpriseID, LE_OH, LE_SystemCreateTimeUtc, LE_SystemCreateUser, LE_SystemLastEditTimeUtc, LE_SystemLastEditUser)
VALUES	('{licenceEnterprise1}', 'EDI', 'ID1', '{orgHeader1}', getutcdate(), 'E', getutcdate(), 'E'),
		('{licenceEnterprise2}', 'EDT', 'ID2', '{orgHeader2}', getutcdate(), 'E', getutcdate(), 'E')

INSERT INTO dbo.LicenceCompany (LC_PK, LC_CompanyCode, LC_OH, LC_LE)
VALUES	('{licenceCompany1}', 'EDI', '{orgHeader1}', '{licenceEnterprise1}'),
		('{licenceCompany2}', 'TST', '{orgHeader2}', '{licenceEnterprise2}')

INSERT INTO dbo.LicenceDatabase (LD_PK, LD_ServerCode, LD_LicenceType, LD_DatabaseNumber, LD_LE, LD_OH_WebAccessOrg)
VALUES ('{licenceDatabase}', 'SYD', 'PRD', 104, '{licenceEnterprise1}', '{orgHeader1}')
 
INSERT INTO dbo.LicenceHeader (LA_PK, LA_LC, LA_LD, LA_SupportMode) 
VALUES (NEWID(), '{licenceCompany1}', '{licenceDatabase}', 'AAA')

INSERT INTO dbo.ClientInvoiceDelivery (L9_PK, L9_LC, L9_ServerCode, L9_SystemCode, L9_OH_InvoiceTo, L9_IsBilled, L9_Note, L9_GroupBy, L9_GB_InvoicingBranch, L9_RX_NKInvoiceCurrency, L9_UseParentPrices)
VALUES (NEWID(), '{licenceCompany1}', ' ', 'ALL', '{orgHeader1}', 'Y', '', 'ALL', null, '', 0)

INSERT INTO dbo.ClientLicencePriceHeader (L6_PK, L6_PricelistVersion, L6_SystemCode, L6_ValidFrom, L6_ValidTo, L6_SystemCreateTimeUtc, L6_SystemCreateUser, L6_RX_NkCurrency, L6_HasExchangeRates, L6_DiscountCode, L6_LC)
VALUES ('{clientLicencePriceHeader}', 'STL v1', 'STL', GETDATE() - 2, GETDATE() + 2, GetUtcDate(), 'E', 'USD', 1, 'STL1', '{licenceCompany1}'),
	   (NEWID(), 'STL v2', 'STL', GETDATE() - 4, GETDATE() - 2, GetUtcDate(), 'E', 'USD', 1, 'STL2', '{licenceCompany1}'),
	   (NEWID(), 'STL v3', 'STL', GETDATE() - 2, GETDATE() + 2, GetUtcDate(), 'E', 'AUD', 1, 'STL3', '{licenceCompany2}'),
	   (NEWID(), 'STL v4', 'STL', GETDATE() - 2, GETDATE() + 2, GetUtcDate(), 'E', 'USD', 0, 'STL4', '{licenceCompany2}'),
	   (NEWID(), 'STL v5', 'STL', GETDATE() - 1, GETDATE() + 1, GetUtcDate(), 'E', 'USD', 1, 'STL5', '{licenceCompany2}')

INSERT INTO dbo.EdiPriceHeaderLink (PHL_PK, PHL_L6, PHL_LD, PHL_RX_NKCurrency, PHL_ValidFrom, PHL_SystemCreateTimeUtc, 
PHL_SystemCreateUser, PHL_CorePackCode, PHL_CoreUpliftPercent, PHL_VolumeCode, PHL_VolumePercent) 
VALUES (NEWID(), '{clientLicencePriceHeader}', '{licenceDatabase}', 'CAD', GETDATE() - 10, GETDATE(), 'MLA', 'INC', 0, 'STD', 0)

INSERT INTO dbo.EdiPriceHeaderDiscount (PHD_PK, PHD_Version, PHD_Name, PHD_Type, PHD_Percent, PHD_IsDefaultEnabled, PHD_ConfigXml)
VALUES (NEWID(), 'STL1', 'WISECLOUD', 'WIS', 10.00, 1, ''),
	   (NEWID(), 'STL2', 'WISECLOUD', 'WIS', 15.00, 1, ''),
	   (NEWID(), 'STL3', 'WISECLOUD', 'WIS', 20.00, 1, ''),
	   (NEWID(), 'STL4', 'WISECLOUD', 'WIS', 15.00, 1, ''),
	   (NEWID(), 'STL5', 'WISECLOUD', 'WIS', 30.00, 1, ''),
	   (NEWID(), 'STL1', 'DEVPARTNER', 'PER', 10.00, 1, ''),
	   (NEWID(), 'STL2', 'DEVPARTNER', 'PER', 15.00, 1, ''),
	   (NEWID(), 'STL3', 'DEVPARTNER', 'PER', 20.00, 1, ''),
	   (NEWID(), 'STL4', 'DEVPARTNER', 'PER', 15.00, 1, ''),
	   (NEWID(), 'STL5', 'DEVPARTNER', 'PER', 30.00, 1, ''),
	   (NEWID(), 'STL1', 'PREPAY', 'PER', 10.00, 1, ''),
	   (NEWID(), 'STL2', 'PREPAY', 'PER', 15.00, 1, ''),
	   (NEWID(), 'STL3', 'PREPAY', 'PER', 20.00, 1, ''),
	   (NEWID(), 'STL4', 'PREPAY', 'PER', 15.00, 1, ''),
	   (NEWID(), 'STL5', 'PREPAY', 'PER', 30.00, 1, ''),
	   (NEWID(), 'STL1', 'WISEPARTNER', 'PER', 10.00, 1, ''),
	   (NEWID(), 'STL2', 'WISEPARTNER', 'PER', 15.00, 1, ''),
	   (NEWID(), 'STL3', 'WISEPARTNER', 'PER', 20.00, 1, ''),
	   (NEWID(), 'STL4', 'WISEPARTNER', 'PER', 15.00, 1, ''),
	   (NEWID(), 'STL5', 'WISEPARTNER', 'PER', 30.00, 1, ''),
	   (NEWID(), 'STL1', 'CCLPATTAIN', 'PER', 10.00, 1, ''),
	   (NEWID(), 'STL2', 'CCLPATTAIN', 'PER', 15.00, 1, ''),
	   (NEWID(), 'STL3', 'CCLPATTAIN', 'PER', 20.00, 1, ''),
	   (NEWID(), 'STL4', 'CCLPATTAIN', 'PER', 15.00, 1, ''),
	   (NEWID(), 'STL5', 'CCLPATTAIN', 'PER', 30.00, 1, ''),
	   (NEWID(), 'STL1', 'CCLPRETAIN', 'PER', 10.00, 1, ''),
	   (NEWID(), 'STL2', 'CCLPRETAIN', 'PER', 15.00, 1, ''),
	   (NEWID(), 'STL3', 'CCLPRETAIN', 'PER', 20.00, 1, ''),
	   (NEWID(), 'STL4', 'CCLPRETAIN', 'PER', 15.00, 1, ''),
	   (NEWID(), 'STL5', 'CCLPRETAIN', 'PER', 30.00, 1, '')
";
			TestConnection.ExecuteNonQuery(sql);

			var command = TestConnection.Command(FormattableString.Invariant($"SELECT * FROM Report_StlPriceListDiscounts(@DiscountName, GETDATE(), @DiscountDescriptions)"));
			command.AddParameter("@DiscountName", SqlDbType.VarChar, "CCLPRETAIN");
			command.AddTableValuedParameter("@DiscountDescriptions", "dbo.TVP_CodeDescriptionMapping", GetRegistryDiscountDescriptionTable());

			var result = DataUtils.GetDataTableFromCommand(command);
			AssertEquals("Result should have 1 CCLPRETAIN row", 1, result.Rows.Count);

			var row = result.Rows[0];
			AssertEquals("StlPriceListVersion", "STL v1", row["StlPriceListVersion"]);
			AssertEquals("DiscountPercent", 10m, row["DiscountPercent"]);
			AssertEquals("DiscountCode", "CCLPRETAIN", row["DiscountCode"]);
			AssertEquals("DiscountDescription", "CCLP Retain Certification Discount", row["DiscountDescription"]);
		}

		DataTable GetRegistryDiscountDescriptionTable()
		{
			var registryDescriptionTable = new DataTable();
			registryDescriptionTable.Locale = CultureInfo.InvariantCulture;
			registryDescriptionTable.Columns.Add("Code", typeof(string));
			registryDescriptionTable.Columns.Add("Description", typeof(string));

			registryDescriptionTable.Rows.Add("WISECLOUD", "WiseCloud");
			registryDescriptionTable.Rows.Add("DEVPARTNER", "Dev Partner");
			registryDescriptionTable.Rows.Add("PREPAY", "Prepayment");
			registryDescriptionTable.Rows.Add("WISEPARTNER", "Wise Industry Partner Discount");
			registryDescriptionTable.Rows.Add("CCLPATTAIN", "CCLP Attain Certification Discount");
			registryDescriptionTable.Rows.Add("CCLPRETAIN", "CCLP Retain Certification Discount");

			return registryDescriptionTable;
		}

		#endregion

		#region TestReport_EdiPricelistMaster

		[TestDate(2023, 11, 1)]
		public void TestReport_EdiPricelistMaster()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var borderWisePrices = BillingTestHelper.CreateLegacyBorderWisePriceList(stdLicCompany);
			var stlPrices1 = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR");
			stlPrices1.L6_RX_NKCurrency = "USD";
			stlPrices1.L6_HasExchangeRates = true;
			stlPrices1.L6_ValidFrom = stlPrices1.L6_ValidFrom.Date;
			var rate1 = stlPrices1.ExchangeRates.AddNew();
			rate1.PHE_GroupCode = "STL";
			rate1.PHE_RX_NKCurrency = "AUD";
			rate1.PHE_Rate = 1.0m;
			rate1.PHE_UpliftPercent = 0m;

			var discount1a = AddPercentageDiscount(stlPrices1, "Discount 1", 10m);
			var discount1b = AddPercentageDiscount(stlPrices1, "Discount 2", 15m);

			var stlPrices2 = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR");
			stlPrices2.L6_DiscountCode = "STL2";
			stlPrices2.L6_PricelistVersion = "STL v10";
			stlPrices2.L6_RX_NKCurrency = "USD";
			stlPrices2.L6_HasExchangeRates = true;
			stlPrices2.L6_ValidFrom = stlPrices2.L6_ValidFrom.Date;
			var rate2 = stlPrices2.ExchangeRates.AddNew();
			rate2.PHE_GroupCode = "STL";
			rate2.PHE_RX_NKCurrency = "GBP";
			rate2.PHE_Rate = 1.5m;
			rate2.PHE_UpliftPercent = 0m;

			var stlPricesWTA = BillingTestHelper.CreateStlPriceList(stdLicCompany, "CLI");
			stlPricesWTA.L6_SystemCode = "WTA";
			stlPricesWTA.L6_PricelistVersion = "STL WTA v20";
			stlPricesWTA.L6_RX_NKCurrency = "USD";
			stlPricesWTA.L6_ValidFrom = stlPricesWTA.L6_ValidFrom.Date;

			var discount2a = AddPercentageDiscount(stlPrices2, "Discount 1", 20m);
			var discount2b = AddPercentageDiscount(stlPrices2, "Discount 2", 25m);
			AddDiscountStructure(stlPrices1, "Standard", discount1a, discount1b);
			AddDiscountStructure(stlPrices1, "BorderWise", discount1b);
			AddDiscountStructure(stlPrices2, "Standard", discount2a, discount2b);
			AddDiscountStructure(stlPrices2, "BorderWise", discount2b);

			var discountVOL = stlPrices1.StlDiscounts.AddNew();
			discountVOL.PHD_Type = "VOL";
			discountVOL.PHD_Name = "discountVOL";
			var volumeDiscount = (VolumeDiscount)discountVOL.Config;
			volumeDiscount.Lines.RemoveAndDeleteAll();
			var line1 = volumeDiscount.Lines.AddNew();
			line1.UnitCount = 400 * 15 + 1;
			line1.Percent = 20m;
			var line2 = volumeDiscount.Lines.AddNew();
			line2.UnitCount = 400 * 15 + 10;
			line2.Percent = 32m;

			var discountDCO = stlPrices1.StlDiscounts.AddNew();
			discountDCO.PHD_Type = "DCO";
			discountDCO.PHD_Name = "discountDCO";
			var countryDiscount = (CountryDiscount)discountDCO.Config;
			countryDiscount.Lines.RemoveAndDeleteAll();
			var countryLine1 = countryDiscount.Lines.AddNew();
			countryLine1.Country = "AU";
			countryLine1.Percent = 20m;
			var countryLine2 = countryDiscount.Lines.AddNew();
			countryLine2.Country = "US";
			countryLine2.Percent = 32m;
			countryLine2.RequiresDomesticDiscount = true;

			BillingTestHelper.AddUsageMap(stlPrices1, "P01", "U01");

			foreach (var priceItem in borderWisePrices.Items)
			{
				priceItem.L7_PGM_DiscountGroupCode = "BorderWise";
			}
			foreach (var priceItem in stlPrices1.Items)
			{
				priceItem.L7_PGM_DiscountGroupCode = "Standard";
			}
			foreach (var priceItem in stlPrices2.Items)
			{
				priceItem.L7_PGM_DiscountGroupCode = "Standard";
			}

			var companyWithoutDatabase = BillingTestHelper.CreateLicenceCompany(Factory, "EN1", "CO1");
			BillingTestHelper.SetInvoicing(companyWithoutDatabase.Header, Env.CurrentBranch.PK, "AUD");

			var licBOR1 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN2", "CO2", "BW1");
			var licBOR2 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN3", "CO3", "BW2");
			var licBORNoCW = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN4", "CO4", "BW4");
			var licCW1 = BillingTestHelper.CreateAnotherDatabase(licBOR1, "CW1");
			licCW1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			var licCW2 = BillingTestHelper.CreateAnotherDatabase(licBOR2, "CW2");
			licCW2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			BillingTestHelper.SetInvoicing(licCW1, Env.CurrentBranch.PK, "AUD");
			BillingTestHelper.SetInvoicing(licCW2, Env.CurrentBranch.PK, "USD");
			BillingTestHelper.SetInvoicing(licBORNoCW, Env.CurrentBranch.PK, "AUD");
			BillingTestHelper.CreatePriceLink(licCW1.Database, stlPrices1, periodStart);
			BillingTestHelper.CreatePriceLink(licCW2.Database, stlPrices2, periodStart, "USD");
			BillingTestHelper.CreateExchangeRate(Factory, "USD", 2);
			BillingTestHelper.AddPriceItemRates(borderWisePrices, "USD", 2);
			BillingTestHelper.AddPriceItemRates(stlPrices1, "USD", 2);
			BillingTestHelper.AddPriceItemRates(stlPrices2, "USD", 2);
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			Factory.Save();
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);

			CombineAssertions(() =>
			{
				AssertReport("Global Settings",
$@"STLPRICELISTVERSION STLPRICELISTPK GROUPCODE  CURRENCY   USDBASERATE UPLIFT
STL V10    {stlPrices2.PK} STL        GBP        1.500000000 0.00
STL V5.0   {stlPrices1.PK} STL        AUD        1.000000000 0.00
");

				AssertReport("Client Specific Settings",
$@"ORGCODE    ORGNAME    SERVERCODE ENTERPRISECODE STLPRICELISTVERSION STLPRICELISTPK CURRENCY   VOLUME     VOLUMEPERCENT VOLUMEBASEDPRICEITEMS COREPACK   COREUPLIFT
EN2CO2     EN2CO2 COMPANY CW1        EN2        STL V5.0   {stlPrices1.PK} AUD        STD        100.00                INC        0.00
EN3CO3     EN3CO3 COMPANY CW2        EN3        STL V10    {stlPrices2.PK} USD        STD        100.00                INC        0.00
");

				AssertReport("Pricelist Master",
$@"SYSTEM     PARENTCURRENCY VALIDFROM  DISCOUNTVERSION PRICELISTVERSION PRICELISTPK L7_ORDER   L7_CATEGORY L7_CODE    L7_DESCRIPTION L7_FEETYPE L7_PRICE   L7_LICENCEUNITS L7_PARENTCATEGORY L7_PARENTCODE L7_UNITBREAK L7_UNITBREAKPARENTCODE L7_WEBPARENTCODE L7_RX_NKCURRENCY L7_REF4    L7_PGM_DISCOUNTGROUPCODE L7_CHARGECODE L7_DEPOSITCHARGECODE L7_DISCOUNTCHARGECODE L7_CHARGEBASIS L7_LANGUAGE L7_EXCHANGERATEGROUPCODE L7_ISVOLUMEADJUSTMENTELIGIBLE L7_PRODUCTAVAILABILITY L7_PRODUCTDISPLAYCATEGORY L7_COUNTRYTIERCODE
BOR        AUD        01/10/2017 12:00:00 AM BOR1       BOR V1     {borderWisePrices.PK} 0          BOR        BW1        REGISTERED USER TRA        200.000000 1000.000000                       0                                                      BORDERWISE ODPLMTHUSE DEPOSIT    DISCODPL                                    TRUE
BOR        AUD        01/10/2017 12:00:00 AM BOR1       BOR V1     {borderWisePrices.PK} 0          BOR        BW9        EXTRA MACHINE TRA        30.000000  150.000000                       0                                                      BORDERWISE ODPLMTHUSE DEPOSIT    DISCODPL                                    TRUE
BOR        AUD        01/10/2017 12:00:00 AM BOR1       BOR V1     {borderWisePrices.PK} 0          BOR        BW2        REGISTERED USER PART 2 TRA        50.000000  250.000000                       0                                                      BORDERWISE ODPLMTHUSE DEPOSIT    DISCODPL                                    TRUE
BOR        AUD        01/10/2017 12:00:00 AM BOR1       BOR V1     {borderWisePrices.PK} 0          BOR        BW3        REGISTERED USER PART 3 TRA        40.000000  200.000000                       0                                                      BORDERWISE ODPLMTHUSE DEPOSIT    DISCODPL                                    TRUE
BOR        AUD        01/10/2017 12:00:00 AM BOR1       BOR V1     {borderWisePrices.PK} 0          BOR        BT1        LEGACY USER PART 1 TRA        200.000000 1000.000000                       0                                                      BORDERWISE ODPLMTHUSE DEPOSIT    DISCODPL                                    TRUE
BOR        AUD        01/10/2017 12:00:00 AM BOR1       BOR V1     {borderWisePrices.PK} 0          BOR        BT2        LEGACY USER PART 2 TRA        50.000000  250.000000                       0                                                      BORDERWISE ODPLMTHUSE DEPOSIT    DISCODPL                                    TRUE
BOR        AUD        01/10/2017 12:00:00 AM BOR1       BOR V1     {borderWisePrices.PK} 0          BOR        BT3        LEGACY USER PART 3 TRA        40.000000  200.000000                       0                                                      BORDERWISE ODPLMTHUSE DEPOSIT    DISCODPL                                    TRUE
BOR        AUD        01/10/2017 12:00:00 AM BOR1       BOR V1     {borderWisePrices.PK} 0          BOR        BT9        LEGACY USER EXTRA MACHINE TRA        30.000000  150.000000                       0                                                      BORDERWISE ODPLMTHUSE DEPOSIT    DISCODPL                                    TRUE
STL        USD        01/11/2015 12:00:00 AM STL2       STL V10    {stlPrices2.PK} 1          STL        USR        ITEM USR              1.000000   0.000000                         0                                                      STANDARD   ODPLMTHUSE DEPOSIT    DISCODPL                                    TRUE
STL        USD        01/11/2015 12:00:00 AM STL1       STL V5.0   {stlPrices1.PK} 1          STL        USR        ITEM USR              1.000000   0.000000                         0                                                      STANDARD   ODPLMTHUSE DEPOSIT    DISCODPL                                    TRUE
WTA        USD        01/11/2015 12:00:00 AM STL1       STL WTA V20 {stlPricesWTA.PK} 1          STL        CLI        ITEM CLI              1.000000   0.000000                         0                                                                 ODPLMTHUSE DEPOSIT    DISCODPL                                    TRUE
");

				AssertReport("STL-DiscountVolume",
$@"STLPRICELISTVERSION STLPRICELISTPK UNITCOUNT  DISCOUNT
STL V5.0   {stlPrices1.PK} 6001       20.00
STL V5.0   {stlPrices1.PK} 6010       32.00
STL WTA V20 {stlPricesWTA.PK} 6001       20.00
STL WTA V20 {stlPricesWTA.PK} 6010       32.00
");

				AssertReport("STL Discounts",
@"PRICELISTVERSION NAME       DESCRIPTION TYPE       TYPEDESCRIPTION DISCOUNTPERCENT ACTIVE
STL V10    DISCOUNT 1            PER        PERCENTAGE 20.00      Y
STL V10    DISCOUNT 2            PER        PERCENTAGE 25.00      Y
STL V5.0   DISCOUNT 1            PER        PERCENTAGE 10.00      Y
STL V5.0   DISCOUNT 2            PER        PERCENTAGE 15.00      Y
STL V5.0   DISCOUNTDCO            DCO        DEVELOPING COUNTRY 0.00       Y
STL V5.0   DISCOUNTVOL            VOL        VOLUME     0.00       Y
STL WTA V20 DISCOUNT 1            PER        PERCENTAGE 10.00      Y
STL WTA V20 DISCOUNT 2            PER        PERCENTAGE 15.00      Y
STL WTA V20 DISCOUNTDCO            DCO        DEVELOPING COUNTRY 0.00       Y
STL WTA V20 DISCOUNTVOL            VOL        VOLUME     0.00       Y
");

				AssertReport("Discount Structure",
@"PRICELISTVERSION STRUCTURE  DISCOUNT   DISCOUNTDESCRIPTION
STL V10    BORDERWISE DISCOUNT 2
STL V10    STANDARD   DISCOUNT 1
STL V10    STANDARD   DISCOUNT 2
STL V5.0   BORDERWISE DISCOUNT 2
STL V5.0   STANDARD   DISCOUNT 1
STL V5.0   STANDARD   DISCOUNT 2
STL WTA V20 BORDERWISE DISCOUNT 2
STL WTA V20 STANDARD   DISCOUNT 1
STL WTA V20 STANDARD   DISCOUNT 2
");

				AssertReport("Usage Mapping",
@"PRICELISTVERSION PRICECATEGORY PRICECODE  USAGECATEGORY USAGECODE  PRICEDESCRIPTION
STL V5.0   STL        P01        STL        U01
");

				AssertReport("STL-DiscountDeveloping Country",
@"PRICELISTVERSION COUNTRY    PERCENT    REQUIRESDOMESTICDISCOUNT
STL V5.0   AU         20.00      Y
STL V5.0   US         32.00      Y
STL WTA V20 AU         20.00      Y
STL WTA V20 US         32.00      Y
");

				AssertExceptionThrown<SqlException>(()
					=> AssertReport("abc123", @"123"));
			});

			void AssertReport(string reportType, string expectedData)
			{
				var rows = new StringBuilder();
				var sql = $"EXEC Report_EdiPricelistMaster '{reportType}', '{periodStart:yyyy-MM-dd}'";
				using (var table = DataUtils.GetDataTableFromQuery(Db.Connection, sql))
				{
					rows.AppendLine(string.Join(" ", table.Columns.OfType<DataColumn>().Select(x => x.ColumnName.ToUpper().PadRight(10))).Trim());
					foreach (DataRow row in table.Rows)
					{
						rows.AppendLine(string.Join(" ", row.ItemArray.Select(x => (x is DateTime ? ((DateTime)x).ToString("dd/MM/yyyy h:mm:00 tt") : x.ToString()).ToUpper().PadRight(10))).Trim());
					}
				}

				AssertEquals(reportType, expectedData.ToUpper(), rows.ToString());
			}
		}

		EdiPriceHeaderDiscount AddPercentageDiscount(ClientLicencePriceHeader prices, string name, decimal percent, bool isDefaultEnabled = true)
		{
			var discount = prices.StlDiscounts.AddNew();
			discount.PHD_Type = BillingConstants.DiscountCalculator.Percentage;
			discount.PHD_Name = name;
			discount.PHD_Percent = percent;
			discount.PHD_IsDefaultEnabled = isDefaultEnabled;
			discount.PHD_Version = prices.L6_DiscountCode;
			return discount;
		}

		void AddDiscountStructure(ClientLicencePriceHeader prices, string name, params EdiPriceHeaderDiscount[] discounts)
		{
			foreach (var discount in discounts)
			{
				var itemDiscount = prices.StlItemDiscounts.AddNew();
				itemDiscount.PGM_GroupCode = name;
				itemDiscount.PGM_PHD = discount.PK;
			}
		}

		#endregion

		#region TestReport_EdiStlBilling

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2022, 9, 1)]
		public void TestReport_EdiStlBilling()
		{
			var mappings = EDIDataRegistry.Instance.SystemProductMappings.Value;
			mappings.AddNew("ABC", "ABC Product", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

			var settings = new UsageBillingSettings();
			var priceLists = settings.PriceLists;
			var priceList1 = priceLists.AddNew();
			priceList1.ProductCode = "ABC";
			priceList1.RawUsageCategory = "SAT";
			priceList1.PriceListCode = "DEF";
			priceList1.Description = "ABC - Price List #0";
			EDIDataRegistry.Instance.UsageBillingSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch, new[] { "CW1", "CW2", "WTA1", "WTA2" });
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch, new[] { "DISC1", "DISC2", "DISC3", "DISC4" });
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch, new[] { "DEP1", "DEP2", "DEP3", "DEP4" });

			var periodStart = ZDateTime.Today;
			periodStart = periodStart.AddDays(1 - periodStart.Day);

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var abcPriceHeader = BillingTestHelper.CreateStlPriceList(stdLicCompany);
			abcPriceHeader.L6_DiscountCode = "STL1";
			abcPriceHeader.L6_UseStandardDiscount = false;
			abcPriceHeader.L6_SystemCode = "DEF";

			var item1 = BillingTestHelper.AddPriceItem(abcPriceHeader, "P01", "", "", 4m, "", 40m);
			item1.L7_ChargeCode = "WTA1";
			item1.L7_DiscountChargeCode = "DISC1";
			item1.L7_DepositChargeCode = "DEP1";
			item1.L7_Category = "SAT";

			var item2 = BillingTestHelper.AddPriceItem(abcPriceHeader, "P02", "", "", 8m, "", 80m);
			item2.L7_ChargeCode = "WTA2";
			item2.L7_DiscountChargeCode = "DISC2";
			item2.L7_DepositChargeCode = "DEP2";
			item2.L7_Category = "SAT";

			var stlPriceHeader = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR", "CUS", "US3", "US4");
			var item3 = stlPriceHeader.Items.Single(x => x.L7_Code == "US3");
			item3.L7_ChargeCode = "CW1";
			item3.L7_DiscountChargeCode = "DISC3";
			item3.L7_DepositChargeCode = "DEP3";
			var item4 = stlPriceHeader.Items.Single(x => x.L7_Code == "US4");
			item4.L7_ChargeCode = "CW2";
			item4.L7_DiscountChargeCode = "DISC4";
			item4.L7_DepositChargeCode = "DEP4";

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			var abcDatabase = lic1.Database;
			abcDatabase.LD_Product = "ABC";

			var lic2 = BillingTestHelper.CreateAnotherDatabase(lic1, "CW2");
			var cw1Database = lic2.Database;
			cw1Database.LD_Product = "CW1";
			BillingTestHelper.CreatePriceLink(lic1.Database, abcPriceHeader, periodStart);
			BillingTestHelper.CreatePriceLink(lic2.Database, stlPriceHeader, periodStart);
			BillingTestHelper.CreateChargeableUsage(Factory, "SAT", "P01", periodStart, lic1.Database.ClientCompanies[0], 6);
			BillingTestHelper.CreateChargeableUsage(Factory, "SAT", "P02", periodStart, lic1.Database.ClientCompanies[0], 9);
			lic1.Database.LD_OH_WebAccessOrg = lic2.Database.LD_OH_WebAccessOrg = lic1.Company.LC_OH;
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic2.Database.ClientCompanies[0], 1);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "US3", periodStart, lic2.Database.ClientCompanies[0], 2);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "US4", periodStart, lic2.Database.ClientCompanies[0], 3);
			Factory.Save();

			var chargeCodes = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, GlbBranch.CurrentBranch.GB_GC)
							.AddToFilter(AccChargeCodeSchema.AC_Code, new string[] { "ODPLMTHUSE", "CW1", "CW2", "WTA1", "WTA2" }))
							.ToDictionary(k => k.AC_Code, v => v);

			var billing = new StlBilling(Factory);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);

			AssertEquals(1, billing.Bills.Count);
			var bill1 = billing.Bills.Cast<StlBill>().Single();
			AssertNoErrors("bill1", bill1);
			var invoice1 = bill1.CreateInvoices(ZDateTime.Empty).Single();
			invoice1.Factory.Save();

			CombineAssertions(() =>
			{
				AssertReport("CW1 product",
$@"PAYINGORG  LD_DATABASENUMBER AG_ACCOUNTNUM AG_DESCRIPTION GB_CODE    AH_POSTDATE BU9_PERIODSTART BU9_UNITCOUNT BU9_PRICECODE BU9_UNITPRICE BU9_PRICECURRENCY BU9_LOCALAMOUNTPREDISCOUNT BU9_LOCALAMOUNTPOSTDISCOUNT BU9_TRANSACTIONAMOUNTPREDISCOUNT BU9_TRANSACTIONAMOUNTPOSTDISCOUNT BU9_BILLINGMODEL L7_ORDER   L7_FEETYPE L7_PRICE   L7_PARENTCODE L7_WEBPARENTCODE L7_LICENCEUNITS L7_DESCRIPTION L7_DISCOUNTCHARGECODE L6_PRICELISTVERSION LE_ENTERPRISECODE USINGORG   AH_ISCANCELLED AH_TRANSACTIONNUM AH_RX_NKTRANSACTIONCURRENCY BU9_USAGECODE BU9_USAGESUBCODE MEMBERSHIP AC_CODE    PAYENT     COMPANYCOUNTRY HOSTEDLOCATION
AAAAAA     102        {chargeCodes["ODPLMTHUSE"].RevenueAccount.AG_AccountNum}            BNE        01/09/2022 12:00:00 AM 01/09/2022 12:00:00 AM 1.0000     USR        1.0000     AUD        1.0000     1.0000     1.0000     1.0000     STL        1                     1.000000                         0.000000   ITEM USR   DISCODPL   STL V5.0   AAA        AAAAAA     FALSE      00001000   AUD        STL        USR                   ODPLMTHUSE AAA                   NCW
AAAAAA     102        {chargeCodes["CW1"].RevenueAccount.AG_AccountNum}            BNE        01/09/2022 12:00:00 AM 01/09/2022 12:00:00 AM 2.0000     US3        1.0000     AUD        2.0000     2.0000     2.0000     2.0000     STL        3                     1.000000                         0.000000   ITEM US3   DISC3      STL V5.0   AAA        AAAAAA     FALSE      00001000   AUD        STL        US3                   CW1        AAA                   NCW
AAAAAA     102        {chargeCodes["CW2"].RevenueAccount.AG_AccountNum}            BNE        01/09/2022 12:00:00 AM 01/09/2022 12:00:00 AM 3.0000     US4        1.0000     AUD        3.0000     3.0000     3.0000     3.0000     STL        4                     1.000000                         0.000000   ITEM US4   DISC4      STL V5.0   AAA        AAAAAA     FALSE      00001000   AUD        STL        US4                   CW2        AAA                   NCW
"
					, ZGuid.Empty, "CW1");

				AssertReport("CWN product",
$@"PAYINGORG  LD_DATABASENUMBER AG_ACCOUNTNUM AG_DESCRIPTION GB_CODE    AH_POSTDATE BU9_PERIODSTART BU9_UNITCOUNT BU9_PRICECODE BU9_UNITPRICE BU9_PRICECURRENCY BU9_LOCALAMOUNTPREDISCOUNT BU9_LOCALAMOUNTPOSTDISCOUNT BU9_TRANSACTIONAMOUNTPREDISCOUNT BU9_TRANSACTIONAMOUNTPOSTDISCOUNT BU9_BILLINGMODEL L7_ORDER   L7_FEETYPE L7_PRICE   L7_PARENTCODE L7_WEBPARENTCODE L7_LICENCEUNITS L7_DESCRIPTION L7_DISCOUNTCHARGECODE L6_PRICELISTVERSION LE_ENTERPRISECODE USINGORG   AH_ISCANCELLED AH_TRANSACTIONNUM AH_RX_NKTRANSACTIONCURRENCY BU9_USAGECODE BU9_USAGESUBCODE MEMBERSHIP AC_CODE    PAYENT     COMPANYCOUNTRY HOSTEDLOCATION
AAAAAA     102        {chargeCodes["ODPLMTHUSE"].RevenueAccount.AG_AccountNum}            BNE        01/09/2022 12:00:00 AM 01/09/2022 12:00:00 AM 1.0000     USR        1.0000     AUD        1.0000     1.0000     1.0000     1.0000     STL        1                     1.000000                         0.000000   ITEM USR   DISCODPL   STL V5.0   AAA        AAAAAA     FALSE      00001000   AUD        STL        USR                   ODPLMTHUSE AAA                   NCW
AAAAAA     102        {chargeCodes["CW1"].RevenueAccount.AG_AccountNum}            BNE        01/09/2022 12:00:00 AM 01/09/2022 12:00:00 AM 2.0000     US3        1.0000     AUD        2.0000     2.0000     2.0000     2.0000     STL        3                     1.000000                         0.000000   ITEM US3   DISC3      STL V5.0   AAA        AAAAAA     FALSE      00001000   AUD        STL        US3                   CW1        AAA                   NCW
AAAAAA     102        {chargeCodes["CW2"].RevenueAccount.AG_AccountNum}            BNE        01/09/2022 12:00:00 AM 01/09/2022 12:00:00 AM 3.0000     US4        1.0000     AUD        3.0000     3.0000     3.0000     3.0000     STL        4                     1.000000                         0.000000   ITEM US4   DISC4      STL V5.0   AAA        AAAAAA     FALSE      00001000   AUD        STL        US4                   CW2        AAA                   NCW
"
					, ZGuid.Empty, "CWN");

				AssertReport("ABC product with org filter",
$@"PAYINGORG  LD_DATABASENUMBER AG_ACCOUNTNUM AG_DESCRIPTION GB_CODE    AH_POSTDATE BU9_PERIODSTART BU9_UNITCOUNT BU9_PRICECODE BU9_UNITPRICE BU9_PRICECURRENCY BU9_LOCALAMOUNTPREDISCOUNT BU9_LOCALAMOUNTPOSTDISCOUNT BU9_TRANSACTIONAMOUNTPREDISCOUNT BU9_TRANSACTIONAMOUNTPOSTDISCOUNT BU9_BILLINGMODEL L7_ORDER   L7_FEETYPE L7_PRICE   L7_PARENTCODE L7_WEBPARENTCODE L7_LICENCEUNITS L7_DESCRIPTION L7_DISCOUNTCHARGECODE L6_PRICELISTVERSION LE_ENTERPRISECODE USINGORG   AH_ISCANCELLED AH_TRANSACTIONNUM AH_RX_NKTRANSACTIONCURRENCY BU9_USAGECODE BU9_USAGESUBCODE MEMBERSHIP AC_CODE    PAYENT     COMPANYCOUNTRY HOSTEDLOCATION
AAAAAA     101        {chargeCodes["WTA1"].RevenueAccount.AG_AccountNum}            BNE        01/09/2022 12:00:00 AM 01/09/2022 12:00:00 AM 6.0000     P01        4.0000     AUD        24.0000    24.0000    24.0000    24.0000    ABC        1                     4.000000                         40.000000             DISC1      STL V5.0   AAA        AAAAAA     FALSE      00001000   AUD        SAT        P01                   WTA1       AAA                   NCW
AAAAAA     101        {chargeCodes["WTA2"].RevenueAccount.AG_AccountNum}            BNE        01/09/2022 12:00:00 AM 01/09/2022 12:00:00 AM 9.0000     P02        8.0000     AUD        72.0000    72.0000    72.0000    72.0000    ABC        2                     8.000000                         80.000000             DISC2      STL V5.0   AAA        AAAAAA     FALSE      00001000   AUD        SAT        P02                   WTA2       AAA                   NCW
",
					lic1.Database.LD_OH_WebAccessOrg, "ABC");
			});

			void AssertReport(string message, string expectedData, ZGuid orgPK, string product)
			{
				var rows = new StringBuilder();
				var sql = $"EXEC Report_EdiStlBilling {(orgPK.IsEmpty ? "null" : $"'{orgPK}'")}, '{billing.DateTo:yyyy-MM-dd}', '{product}'";
				using (var table = DataUtils.GetDataTableFromQuery(Db.Connection, sql))
				{
					rows.AppendLine(string.Join(" ", table.Columns.OfType<DataColumn>().Select(x => x.ColumnName.ToUpper().PadRight(10))).Trim());
					foreach (DataRow row in table.Rows)
					{
						rows.AppendLine(string.Join(" ", row.ItemArray.Select(x => (x is DateTime ? ((DateTime)x).ToString("dd/MM/yyyy h:mm:00 tt") : x.ToString()).ToUpper().PadRight(10))).Trim());
					}
				}

				AssertEquals(message, expectedData.ToUpper(), rows.ToString());
			}
		}

		#endregion

		#region TestReport_IncidentFeatureRequestDetails

		[TestDate(2024, 1, 1)]
		public void TestReport_IncidentFeatureRequestDetails()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "U01";
			staff.GS_FullName = "User 01";

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Test Contact 1";
			contact.Header.OH_Code = "TESTOH001";
			contact.Header.OH_FullName = "Test Org 001";
			contact.Header.MiscServ.OM_CMClientSize = "S03";

			var project = Factory.NewWithValidTestData<EDIProject>();
			project.WKP_Summary = "Project 002";

			Factory.Save();

			var job = BMSTestHelper.CreateJobHeader<SupportIncident>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(job, "Fun Workflow");
			BMSTestHelper.CreateTask(workflow, "U01", 60);

			var incident = (SupportIncident)job.Parent;
			incident.IM_IncidentNumber = "INC001";
			incident.IM_OH_Client = contact.OC_OH;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Product = "ENT";
			incident.IM_Module = "AAA";
			incident.IM_Priority = "CR7";
			incident.IM_RN_NKCountry = "AU";
			incident.FeatureRequestClientPK = contact.Header.PK;
			incident.FeatureRequestContactPK = contact.PK;
			incident.IM_Description = "IM_Description 002";
			incident.IM_RequiredBy = new ZDateTime(2024, 1, 2);
			incident.RelatedProjectPK = project.PK;
			incident.IM_ResolutionCode = "ADE";
			incident.CurrentTask.P9_GS_NKAssignedStaffMember = "U01";
			incident.CurrentTask.P9_Description = "P9_Description 003";
			incident.CurrentTask.P9_ScheduledDate = new ZDateTime(2024, 1, 15);
			incident.IM_Status = "WRK";

			incident.Estimate.CIE_MinDevelopmentHours = 100;
			incident.Estimate.CIE_MaxDevelopmentHours = 200;
			incident.Estimate.CIE_EstimateSentDateUTC = new ZDateTime(2024, 2, 1);
			incident.Estimate.CIE_EstimateExpiryDateUTC = new ZDateTime(2024, 3, 1);
			incident.Estimate.CIE_QuoteRequestedUTC = new ZDateTime(2024, 4, 1);
			incident.Estimate.CIE_PaymentTerms = "COD";
			incident.Estimate.CIE_RX_NKCurrency = "AUD";
			incident.Estimate.CIE_MinEstimateMonthly = 300;
			incident.Estimate.CIE_MaxEstimateMonthly = 400;
			incident.Estimate.CIE_MinEstimateOneoff = 500;
			incident.Estimate.CIE_MaxEstimateOneoff = 600;
			incident.Estimate.CIE_CancellationFee = 700;
			incident.Estimate.CIE_ExpressDeliveryOptionCutOffDateUTC = new ZDateTime(2024, 5, 1);

			incident.Quote.CIQ_MinDevelopmentHours = 10;
			incident.Quote.CIQ_MaxDevelopmentHours = 20;
			incident.Quote.CIQ_QuoteSentDateUTC = new ZDateTime(2024, 6, 1);
			incident.Quote.CIQ_QuoteExpiryDateUTC = new ZDateTime(2024, 7, 1);
			incident.Quote.CIQ_QuoteAcceptedDateUTC = new ZDateTime(2024, 8, 1);
			incident.Quote.CIQ_DeliveredDateUTC = new ZDateTime(2024, 9, 1);
			incident.Quote.CIQ_Type = "CHK";
			incident.Quote.CIQ_PaymentTerms = "PAY";
			incident.Quote.CIQ_RX_NKCurrency = "USD";
			incident.Quote.CIQ_QuoteAmount = 30;
			incident.Quote.CIQ_OneoffUpfront = 40;
			incident.Quote.CIQ_CancellationFee = 50;
			incident.Quote.CIQ_HeadStartOptionIncluded = true;
			incident.Quote.CIQ_HeadStartSurcharge = 60;
			incident.Quote.CIQ_ExpressDeliveryOptionIncluded = false;
			incident.Quote.CIQ_ExpressDeliverySurcharge = 70;

			incident.BusinessRequirementsAsBlob = ZBlob.FromUTF8("#1, item a, 100\r\n#2, item b, 200,\r\nDollar Symbol - $\r\nEuro Symbol - €\r\nPound Sterling Symbol – £\r\nYen Symbol – ¥");
			Factory.Save();

			AssertReport("SMF", "CR7", "", new ZDate(2024, 1, 1), new ZDate(2024, 1, 10), "");
			AssertReport("ENT", "CR9", "", new ZDate(2024, 1, 1), new ZDate(2024, 1, 10), "");
			AssertReport("ENT", "CR7", "", new ZDate(2024, 2, 1), new ZDate(2024, 2, 10), "");
			AssertReport("ENT", "CR7", "", new ZDate(2024, 1, 1), new ZDate(2024, 1, 2),
@"IM_Product = ENT
IM_Priority = CR7
IM_Module = AAA
IM_RN_NKCountry = AU
IM_SystemCreateTimeUtc = 01/01/2024
IM_IncidentNumber = INC001
OH_Code = TESTOH001
OH_FullName = Test Org 001
OC_ContactName = Test Contact 1
IM_Description = IM_Description 002
IM_RequiredBy = 02/01/2024
WKP_ProjectNumber = PRJ00000001
WKP_Summary = Project 002
IM_ResolutionCode = ADE
CurrentTask = P9_Description 003
CurrentTaskEstimatedDate = 15/01/2024
OM_CMClientSize = S03
IM_Status = OPN
LastTaskClosedBy = U01
CIE_MinDevelopmentHours = 100.00
CIE_MaxDevelopmentHours = 200.00
CIE_EstimateSentDateUTC = 01/02/2024
CIE_EstimateExpiryDateUTC = 01/03/2024
CIE_QuoteRequestedUTC = 01/04/2024
CIE_PaymentTerms = COD
CIE_RX_NKCurrency = AUD
CIE_MinEstimateMonthly = 300.0000
CIE_MaxEstimateMonthly = 400.0000
CIE_MinEstimateOneoff = 500.0000
CIE_MaxEstimateOneoff = 600.0000
CIE_CancellationFee = 700.0000
CIE_ExpressDeliveryOptionCutOffDateUTC = 01/05/2024
CIQ_MinDevelopmentHours = 10.00
CIQ_MaxDevelopmentHours = 20.00
CIQ_QuoteSentDateUTC = 01/06/2024
CIQ_QuoteExpiryDateUTC = 01/07/2024
CIQ_QuoteAcceptedDateUTC = 01/08/2024
CIQ_DeliveredDateUTC = 01/09/2024
CIQ_Type = CHK
CIQ_PaymentTerms = PAY
CIQ_RX_NKCurrency = USD
CIQ_QuoteAmount = 30.0000
CIQ_OneoffUpfront = 40.0000
CIQ_CancellationFee = 50.0000
CIQ_HeadStartOptionIncluded = True
CIQ_HeadStartSurcharge = 60.0000
CIQ_ExpressDeliveryOptionIncluded = False
CIQ_ExpressDeliverySurcharge = 70.0000
BusinessRequirement = #1, item a, 100
#2, item b, 200,
Dollar Symbol - $
Euro Symbol - €
Pound Sterling Symbol – £
Yen Symbol – ¥
");

			void AssertReport(string product, string criticality, string section, ZDate startDate, ZDate endDate, string expectedData)
			{
				var rows = new StringBuilder();
				var sql = $"EXEC Report_IncidentFeatureRequestDetails '{product}', '{criticality}', '{section}', '{startDate:yyyy-MM-dd}', '{endDate:yyyy-MM-dd}', '2024-9-1', '2024-9-2';";
				using (var table = DataUtils.GetDataTableFromQuery(Db.Connection, sql))
				{
					if (table.Rows.Count == 0)
					{
						AssertEquals(expectedData, "");
						return;
					}

					for (var index = 0; index < table.Columns.Count; index++)
					{
						var colValue = table.Rows[0].ItemArray[index];
						var colValueAsString = "";
						if (colValue is DateTime dateTimeValue)
						{
							colValueAsString = dateTimeValue.ToString("dd/MM/yyyy");
						}
						else if (colValue is byte[] bytes)
						{
							colValueAsString = Encoding.UTF8.GetString(Compressor.Uncompress(bytes));
						}
						else
						{
							colValueAsString = colValue.ToString();
						}

						rows.AppendLine($"{table.Columns[index].ColumnName} = {colValueAsString}");
					}
				}

				AssertEquals(expectedData, rows.ToString());
			}
		}

		#endregion

		#region TestClientPrepayTransactions
		public void TestClientPrepayTransactions()
		{
			var today = ZDateTime.Today;
			var firstDayThisMonth = new ZDateTime(today.Year, today.Month, 1);
			var date1 = firstDayThisMonth.AddMonths(-2);
			var date2 = firstDayThisMonth.AddMonths(-1);
			var date3 = firstDayThisMonth;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "DDDABCSYD";
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "CHARGE111";
			chargeCode.AC_ChargeType = "REV";
			// Simulate exchanges rate created via data import
			// Rates created from UI have expiry date set to last minute of the day, i.e. 23:59:00
			var rate1 = Factory.New<RefExchangeRateForTest>();
			rate1.RE_GC = Env.CurrentCompanyPK;
			rate1.RE_RX_NKExCurrency = "NZD";
			rate1.RE_ExRateType = "SEL";
			rate1.RE_SellRate = 1.2m;
			rate1.RE_StartDate = date1;
			rate1.RE_ExpiryDate = date1.AddMonths(1).AddDays(-1);
			var rate2 = Factory.New<RefExchangeRateForTest>();
			rate2.RE_GC = Env.CurrentCompanyPK;
			rate2.RE_RX_NKExCurrency = "NZD";
			rate2.RE_ExRateType = "SEL";
			rate2.RE_SellRate = 1.25m;
			rate2.RE_StartDate = date2;
			rate2.RE_ExpiryDate = date2.AddMonths(1).AddDays(-1);
			var rate3 = Factory.New<RefExchangeRateForTest>();
			rate3.RE_GC = Env.CurrentCompanyPK;
			rate3.RE_RX_NKExCurrency = "NZD";
			rate3.RE_ExRateType = "SEL";
			rate3.RE_SellRate = 1.1m;
			rate3.RE_StartDate = date3;
			rate3.RE_ExpiryDate = date3.AddMonths(2).AddDays(-1);
			var invoice = Factory.New<AccTransactionHeader>();
			invoice.AH_OH = org.PK;
			invoice.AH_Ledger = "AR";
			invoice.AH_TransactionType = "INV";
			invoice.AH_Desc = "Billing Invoice";
			invoice.AH_PostDate = date2.AddMonths(1).AddDays(-1);
			invoice.AH_InvoiceDate = date2.AddMonths(1).AddDays(-1);
			invoice.AH_GB = Env.CurrentBranchPK;
			invoice.AH_GC = Env.CurrentCompanyPK;
			invoice.AH_GE = Env.CurrentDepartmentPK;
			invoice.AH_TransactionNum = "00089641";
			invoice.AH_RX_NKTransactionCurrency = "NZD";
			invoice.AH_InvoiceAmount = 800m;
			invoice.AH_ExchangeRate = 1.25m;
			invoice.AH_OSTotal = 1000m;
			invoice.AH_OutstandingAmount = 800m;
			var line = Factory.New<AccTransactionLines>();
			line.AL_AH = invoice.PK;
			line.AL_LineType = TransactionLineTypes.Revenue;
			line.AL_AC = chargeCode.PK;
			line.AL_ExchangeRate = 1.25m;
			line.AL_LineAmount = 800m;
			line.AL_GB = Env.CurrentBranchPK;
			line.AL_GC = Env.CurrentCompanyPK;
			line.AL_GE = Env.CurrentDepartmentPK;
			line.AL_RX_NKTransactionCurrency = "NZD";
			var receipt1 = Factory.New<AccTransactionHeader>();
			receipt1.AH_OH = org.PK;
			receipt1.AH_Ledger = "AR";
			receipt1.AH_TransactionType = "REC";
			receipt1.AH_Desc = "DD AR Receipt";
			receipt1.AH_PostDate = date2.AddDays(15);
			receipt1.AH_InvoiceDate = date2.AddDays(15);
			receipt1.AH_GB = Env.CurrentBranchPK;
			receipt1.AH_GC = Env.CurrentCompanyPK;
			receipt1.AH_GE = Env.CurrentDepartmentPK;
			receipt1.AH_TransactionNum = "00065001";
			receipt1.AH_RX_NKTransactionCurrency = "NZD";
			receipt1.AH_InvoiceAmount = -500m;
			receipt1.AH_ExchangeRate = 1.25m;
			receipt1.AH_OSTotal = -625m;
			receipt1.AH_OutstandingAmount = -500m;
			var receipt2 = Factory.New<AccTransactionHeader>();
			receipt2.AH_OH = org.PK;
			receipt2.AH_Ledger = "AR";
			receipt2.AH_TransactionType = "REC";
			receipt2.AH_Desc = "DD AR Receipt";
			receipt2.AH_PostDate = date1.AddDays(24);
			receipt2.AH_InvoiceDate = date1.AddDays(24);
			receipt2.AH_GB = Env.CurrentBranchPK;
			receipt2.AH_GC = Env.CurrentCompanyPK;
			receipt2.AH_GE = Env.CurrentDepartmentPK;
			receipt2.AH_TransactionNum = "00067043";
			receipt2.AH_RX_NKTransactionCurrency = "NZD";
			receipt2.AH_InvoiceAmount = -800m;
			receipt2.AH_ExchangeRate = 1.2m;
			receipt2.AH_OSTotal = -960m;
			receipt2.AH_OutstandingAmount = -800m;
			var depositAdjust = Factory.New<EdiDepositAdjust>();
			depositAdjust.DEA_OH = org.PK;
			depositAdjust.DEA_GC = Env.CurrentCompanyPK;
			depositAdjust.DEA_ChargeCode = "DEPOSIT";
			depositAdjust.DEA_RX_NKCurrency = "NZD";
			depositAdjust.DEA_Amount = 125m;
			Factory.Save();
			using (var cmd = Db.Connection.Command("dbo.ClientPrepayTransactions"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@Company", SqlDbType.UniqueIdentifier, Env.CurrentCompanyPK);
				cmd.AddParameter("@OrgList", SqlDbType.VarChar, "");
				cmd.AddParameter("@TransactionTypeList", SqlDbType.VarChar, "INV, CRD, ADJ");
				cmd.AddParameter("@PostDateFrom", SqlDbType.DateTime, date2.AddDays(20).ToDateTime());
				cmd.AddParameter("@PostDateTo", SqlDbType.DateTime, date3.AddDays(20).ToDateTime());
				cmd.AddParameter("@ChargeCodeList", SqlDbType.VarChar, "");
				cmd.AddParameter("@DepositCodeList", SqlDbType.VarChar, "DEPOSIT");
				cmd.AddReturnValueParameter();
				var resultLinesCount = 0;
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						resultLinesCount++;
						AssertEquals("NZD", (string)reader["CurrencyCode"]);
						AssertEquals("(-800) + 500 + 800", 500m, (decimal)reader["OutstandingAmount"]);
						AssertEquals("(-800 * 1.25) + (500 * 1.25) + (800 * 1.2)", 585m, (decimal)reader["OSOutstandingAmount"]);
						AssertEquals("125 * (1.25 / 1.25)", 125m, (decimal)reader["OSDepositExTax"]);
					}
				}

				AssertEquals(1, resultLinesCount);
			}
		}

		class RefExchangeRateForTest : AutoRefExchangeRate
		{
			public RefExchangeRateForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
				SuspendValidation();
			}
		}

		#endregion
		#region TestAllDatesInRange
		public void TestAllDatesInRange()
		{
			var command = "SELECT * FROM dbo.AllDatesInRange('1995-12-23', '1995-12-25')";
			using (var cmd = Db.Connection.Command(command))
			using (var reader = cmd.ExecuteReader())
			{
				var expectedElements = new DateTime[] { new DateTime(1995, 12, 23, 0, 0, 0), new DateTime(1995, 12, 24, 0, 0, 0), new DateTime(1995, 12, 25, 0, 0, 0) };
				var resultCount = 0;
				while (reader.Read())
				{
					AssertEquals(expectedElements[resultCount], reader.GetDateTime(0));
					resultCount++;
				}

				AssertEquals(3, resultCount);
			}
		}

		public void TestAYearsWorthOfDates()
		{
			var command = "SELECT * FROM dbo.AllDatesInRange('1995-12-23', '1996-12-23')";
			using (var cmd = Db.Connection.Command(command))
			using (var reader = cmd.ExecuteReader())
			{
				var resultCount = 0;
				while (reader.Read())
				{
					resultCount++;
				}

				AssertEquals(367, resultCount);
			}
		}

		public void TestInversed()
		{
			var command = "SELECT * FROM dbo.AllDatesInRange('1995-12-25','1995-12-23')";
			using (var cmd = Db.Connection.Command(command))
			using (var reader = cmd.ExecuteReader())
			{
				var expectedElements = Array.Empty<string>();
				var resultCount = 0;
				while (reader.Read())
				{
					AssertEquals(expectedElements[resultCount], reader[resultCount]);
					resultCount++;
				}

				AssertEquals(0, resultCount);
			}
		}

		#endregion
		#region EdiAddContactToPersonMergeQueue
		public void TestEdiAddContactToPersonMergeQueue()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence1.Database;
			var org1 = database.LicEnterprise.Organisation;
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "User One";
			contact1.OC_Email = "user.one@test.org";
			var licence2 = BillingTestHelper.CreateAnotherLicence(database, "DEF");
			var org2 = licence2.Company.Header;
			var contact2a = org2.Contacts.AddNew();
			contact2a.OC_ContactName = "User One";
			contact2a.OC_Email = "user.one@test.org";
			var contact2b = org2.Contacts.AddNew();
			contact2b.OC_ContactName = "User One (1)";
			contact2b.OC_Email = "user.one@test.org";
			var licence3 = BillingTestHelper.CreateLicence(Factory, "YYY", "TTT", "DDD");
			var org3 = licence3.Company.Header;
			var contact3 = org3.Contacts.AddNew();
			contact3.OC_ContactName = "User One";
			contact3.OC_Email = "user.one@test.org";
			Factory.Save();
			contact1.Reload();
			contact2a.Reload();
			contact2b.Reload();
			contact3.Reload();
			using (var command = Db.Connection.Command("EdiAddContactToPersonMergeQueue"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@DatabasePk", SqlDbType.UniqueIdentifier, database.PK.ToGuid());
				command.ExecuteNonQuery();
			}

			var queueItems = Factory.Load<EdiPersonMergeQueue>(new ZQuery(EdiPersonMergeQueueSchema.EMQ_PER_RetainPerson, contact1.OC_PER));
			AssertEquals(2, queueItems.Length);
			AssertEquals(1, queueItems.Count(x => x.EMQ_PER_DissolvePerson == contact2a.OC_PER));
			AssertEquals(1, queueItems.Count(x => x.EMQ_PER_DissolvePerson == contact2b.OC_PER));
			AssertEquals(0, queueItems.Count(x => x.EMQ_PER_DissolvePerson == contact3.OC_PER));
			using (var command = Db.Connection.Command("EdiAddContactToPersonMergeQueue"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@DatabasePk", SqlDbType.UniqueIdentifier, database.PK.ToGuid());
				command.ExecuteNonQuery();
			}

			queueItems = Factory.Load<EdiPersonMergeQueue>(new ZQuery(EdiPersonMergeQueueSchema.EMQ_PER_RetainPerson, contact1.OC_PER));
			AssertEquals("No new queue items", 2, queueItems.Length);
			var contact2c = org2.Contacts.AddNew();
			contact2c.OC_ContactName = "User One (BW)";
			contact2c.OC_Email = "user.one@test.org";
			Factory.Save();
			contact2c.Reload();
			using (var command = Db.Connection.Command("EdiAddContactToPersonMergeQueue"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@DatabasePk", SqlDbType.UniqueIdentifier, database.PK.ToGuid());
				command.ExecuteNonQuery();
			}

			queueItems = new BusinessObjectFactory().Load<EdiPersonMergeQueue>(new ZQuery(EdiPersonMergeQueueSchema.EMQ_PER_RetainPerson, contact1.OC_PER));
			AssertEquals("New queue item is added", 3, queueItems.Length);
			AssertEquals(1, queueItems.Count(x => x.EMQ_PER_DissolvePerson == contact2c.OC_PER));
		}

		#endregion
		#region EdiUserAgreement
		public void TestEdiUserAgreementNR_UX__ERA_Type_ERA_VariantCode_ERA_RN_NKCountryCode_ERA_VersionNumber_ERA_MinorVersion()
		{
			var agreement1PK = Guid.NewGuid();
			var agreement2PK = Guid.NewGuid();
			var agreement3PK = Guid.NewGuid();
			var agreement4PK = Guid.NewGuid();
			var agreement5PK = Guid.NewGuid();
			var commandText = FormattableString.Invariant($@"
DISABLE TRIGGER TG_INS_EdiUserAgreement ON EdiUserAgreement

INSERT INTO dbo.EdiUserAgreement (ERA_PK, ERA_Type, ERA_RN_NKCountryCode, ERA_VersionNumber, ERA_MinorVersion, ERA_EffectiveTimeUtc, ERA_SystemCreateTimeUtc, ERA_SystemCreateUser, ERA_SystemLastEditTimeUtc, ERA_SystemLastEditUser) VALUES ('{agreement1PK}', 'DCA', 'AU', 1, 0, '2019-01-01', '2019-01-01', 'E', '2019-01-01', 'E')
INSERT INTO dbo.EdiUserAgreement (ERA_PK, ERA_Type, ERA_RN_NKCountryCode, ERA_VersionNumber, ERA_MinorVersion, ERA_EffectiveTimeUtc, ERA_SystemCreateTimeUtc, ERA_SystemCreateUser, ERA_SystemLastEditTimeUtc, ERA_SystemLastEditUser) VALUES ('{agreement2PK}', 'DCA', 'NZ', 1, 0, '2019-01-01', '2019-01-01', 'E', '2019-01-01', 'E')
INSERT INTO dbo.EdiUserAgreement (ERA_PK, ERA_Type, ERA_RN_NKCountryCode, ERA_VersionNumber, ERA_MinorVersion, ERA_EffectiveTimeUtc, ERA_SystemCreateTimeUtc, ERA_SystemCreateUser, ERA_SystemLastEditTimeUtc, ERA_SystemLastEditUser) VALUES ('{agreement3PK}', 'EUA', 'AU', 1, 0, '2019-01-01', '2019-01-01', 'E', '2019-01-01', 'E')
INSERT INTO dbo.EdiUserAgreement (ERA_PK, ERA_Type, ERA_RN_NKCountryCode, ERA_VersionNumber, ERA_MinorVersion, ERA_EffectiveTimeUtc, ERA_SystemCreateTimeUtc, ERA_SystemCreateUser, ERA_SystemLastEditTimeUtc, ERA_SystemLastEditUser) VALUES ('{agreement4PK}', 'DCA', 'AU', 2, 0, '2019-01-02', '2019-01-01', 'E', '2019-01-01', 'E')
INSERT INTO dbo.EdiUserAgreement (ERA_PK, ERA_Type, ERA_RN_NKCountryCode, ERA_VersionNumber, ERA_MinorVersion, ERA_EffectiveTimeUtc, ERA_SystemCreateTimeUtc, ERA_SystemCreateUser, ERA_SystemLastEditTimeUtc, ERA_SystemLastEditUser) VALUES ('{agreement5PK}', 'DCA', 'AU', 2, 1, '2019-01-03', '2019-01-01', 'E', '2019-01-01', 'E')

; ENABLE TRIGGER TG_INS_EdiUserAgreement ON EdiUserAgreement
");
			TestConnection.ExecuteNonQuery(commandText);
			var agreement6PK = Guid.NewGuid();
			commandText = FormattableString.Invariant($@"
INSERT INTO dbo.EdiUserAgreement (ERA_PK, ERA_Type, ERA_RN_NKCountryCode, ERA_VersionNumber, ERA_MinorVersion, ERA_EffectiveTimeUtc) VALUES ('{agreement6PK}', 'DCA', 'AU', 1, 0, '2019-01-03')
");
			try
			{
				TestConnection.ExecuteNonQuery(commandText);
				Fail("Should have SqlException");
			}
			catch (SqlException ex)
			{
				AssertEquals("Should only allow 1 agreement per Type+CountryCode+VersionNumber+MinorVersion combination", @"Cannot insert duplicate key row in object 'dbo.EdiUserAgreement' with unique index 'NR_UX__ERA_Type_ERA_VariantCode_ERA_RN_NKCountryCode_ERA_VersionNumber_ERA_MinorVersion'. The duplicate key value is (DCA, , AU, 1, 0).
The statement has been terminated.", ex.Message);
			}
		}

		public void TestEdiUserAgreementNR_UX__ERA_Type_ERA_VariantCode_ERA_RN_NKCountryCode_ERA_EffectiveTimeUtc()
		{
			var agreement1PK = Guid.NewGuid();
			var agreement2PK = Guid.NewGuid();
			var agreement3PK = Guid.NewGuid();
			var agreement4PK = Guid.NewGuid();
			var agreement5PK = Guid.NewGuid();
			var agreement6PK = Guid.NewGuid();
			var commandText = FormattableString.Invariant($@"
DISABLE TRIGGER TG_INS_EdiUserAgreement ON EdiUserAgreement

INSERT INTO dbo.EdiUserAgreement (ERA_PK, ERA_Type, ERA_RN_NKCountryCode, ERA_VersionNumber, ERA_EffectiveTimeUtc, ERA_SystemCreateTimeUtc, ERA_SystemCreateUser, ERA_SystemLastEditTimeUtc, ERA_SystemLastEditUser) VALUES ('{agreement1PK}', 'DCA', 'AU', 1, '2019-01-01', '2019-01-01', 'E', '2019-01-01', 'E')
INSERT INTO dbo.EdiUserAgreement (ERA_PK, ERA_Type, ERA_RN_NKCountryCode, ERA_VersionNumber, ERA_EffectiveTimeUtc, ERA_SystemCreateTimeUtc, ERA_SystemCreateUser, ERA_SystemLastEditTimeUtc, ERA_SystemLastEditUser) VALUES ('{agreement2PK}', 'DCA', 'AU', 2, '2019-01-02', '2019-01-01', 'E', '2019-01-01', 'E')
INSERT INTO dbo.EdiUserAgreement (ERA_PK, ERA_Type, ERA_RN_NKCountryCode, ERA_VersionNumber, ERA_EffectiveTimeUtc, ERA_SystemCreateTimeUtc, ERA_SystemCreateUser, ERA_SystemLastEditTimeUtc, ERA_SystemLastEditUser) VALUES ('{agreement3PK}', 'DCA', 'NZ', 1, '2019-01-01', '2019-01-01', 'E', '2019-01-01', 'E')
INSERT INTO dbo.EdiUserAgreement (ERA_PK, ERA_Type, ERA_RN_NKCountryCode, ERA_VersionNumber, ERA_EffectiveTimeUtc, ERA_SystemCreateTimeUtc, ERA_SystemCreateUser, ERA_SystemLastEditTimeUtc, ERA_SystemLastEditUser) VALUES ('{agreement4PK}', 'DCA', 'NZ', 2, '2019-01-02', '2019-01-01', 'E', '2019-01-01', 'E')
INSERT INTO dbo.EdiUserAgreement (ERA_PK, ERA_Type, ERA_RN_NKCountryCode, ERA_VersionNumber, ERA_EffectiveTimeUtc, ERA_SystemCreateTimeUtc, ERA_SystemCreateUser, ERA_SystemLastEditTimeUtc, ERA_SystemLastEditUser) VALUES ('{agreement5PK}', 'ECA', 'NZ', 1, '2019-01-01', '2019-01-01', 'E', '2019-01-01', 'E')
INSERT INTO dbo.EdiUserAgreement (ERA_PK, ERA_Type, ERA_RN_NKCountryCode, ERA_VersionNumber, ERA_EffectiveTimeUtc, ERA_SystemCreateTimeUtc, ERA_SystemCreateUser, ERA_SystemLastEditTimeUtc, ERA_SystemLastEditUser) VALUES ('{agreement6PK}', 'ECA', 'NZ', 2, '2019-01-02', '2019-01-01', 'E', '2019-01-01', 'E')

; ENABLE TRIGGER TG_INS_EdiUserAgreement ON EdiUserAgreement
");
			TestConnection.ExecuteNonQuery(commandText);
			var agreement7PK = Guid.NewGuid();
			commandText = FormattableString.Invariant($@"
INSERT INTO dbo.EdiUserAgreement (ERA_PK, ERA_Type, ERA_RN_NKCountryCode, ERA_VersionNumber, ERA_EffectiveTimeUtc) VALUES ('{agreement7PK}', 'DCA', 'AU', 3, '2019-01-02')
");
			try
			{
				TestConnection.ExecuteNonQuery(commandText);
				Fail("Should have SqlException");
			}
			catch (SqlException ex)
			{
				AssertEquals("Should only allow 1 current agreement per Type+CountryCode combination", @"Cannot insert duplicate key row in object 'dbo.EdiUserAgreement' with unique index 'NR_UX__ERA_Type_ERA_VariantCode_ERA_RN_NKCountryCode_ERA_EffectiveTimeUtc'. The duplicate key value is (DCA, , AU, Jan  2 2019 12:00AM).
The statement has been terminated.", ex.Message);
			}
		}

		public void TestEdiUserAgreementShouldNotDeleteIfEffectiveTimeInPast()
		{
			var agreement1PK = Guid.NewGuid();
			var agreement2PK = Guid.NewGuid();
			var pastTime = ZDateTime.UtcNow.AddHours(-1).ToISO8601String();
			var futureTime = ZDateTime.UtcNow.AddHours(1).ToISO8601String();
			var commandText = FormattableString.Invariant($@"
DISABLE TRIGGER TG_INS_EdiUserAgreement ON EdiUserAgreement

INSERT INTO dbo.EdiUserAgreement (ERA_PK, ERA_Type, ERA_RN_NKCountryCode, ERA_VersionNumber, ERA_EffectiveTimeUtc, ERA_SystemCreateTimeUtc, ERA_SystemCreateUser, ERA_SystemLastEditTimeUtc, ERA_SystemLastEditUser) VALUES ('{agreement1PK}', 'DCA', 'AU', 1, '{pastTime}', '2019-01-01', 'E', '2019-01-01', 'E')
INSERT INTO dbo.EdiUserAgreement (ERA_PK, ERA_Type, ERA_RN_NKCountryCode, ERA_VersionNumber, ERA_EffectiveTimeUtc, ERA_SystemCreateTimeUtc, ERA_SystemCreateUser, ERA_SystemLastEditTimeUtc, ERA_SystemLastEditUser) VALUES ('{agreement2PK}', 'DCA', 'AU', 2, '{futureTime}', '2019-01-01', 'E', '2019-01-01', 'E')

; ENABLE TRIGGER TG_INS_EdiUserAgreement ON EdiUserAgreement
");
			TestConnection.ExecuteNonQuery(commandText);
			commandText = FormattableString.Invariant($"DELETE FROM dbo.EdiUserAgreement WHERE ERA_PK = '{agreement2PK}'");
			TestConnection.ExecuteNonQuery(commandText);
			commandText = FormattableString.Invariant($"DELETE FROM dbo.EdiUserAgreement WHERE ERA_PK = '{agreement1PK}'");
			try
			{
				TestConnection.ExecuteNonQuery(commandText);
				Fail("Should have SqlException");
			}
			catch (SqlException ex)
			{
				AssertEquals("Delete not allowed if effective time is in the past", @"Delete operation NOT allowed on EdiUserAgreement for records which are already effective.
The transaction ended in the trigger. The batch has been aborted.", ex.Message);
			}
		}

		public void TestEdiUserAgreementShouldNotEditERA_TitleIfEffectiveTimeInPast()
		{
			SetupEdiUserAgreementForEditConstraintTest();
			var commandText = FormattableString.Invariant($"UPDATE dbo.EdiUserAgreement SET ERA_Title = 'Abacad' WHERE ERA_PK = '{agreement2PK}'");
			TestConnection.ExecuteNonQuery(commandText);
			commandText = FormattableString.Invariant($"UPDATE dbo.EdiUserAgreement SET ERA_Title = 'Abacad' WHERE ERA_PK = '{agreement1PK}'");
			try
			{
				TestConnection.ExecuteNonQuery(commandText);
				Fail("Should have SqlException");
			}
			catch (SqlException ex)
			{
				AssertEquals("Changing title should be disallowed once effective time has passed", @"Update operation NOT allowed on EdiUserAgreement for this/these column(s) if already effective.
The transaction ended in the trigger. The batch has been aborted.", ex.Message);
			}
		}

		public void TestEdiUserAgreementShouldNotEditERA_ContentIfEffectiveTimeInPast()
		{
			SetupEdiUserAgreementForEditConstraintTest();
			var commandText = FormattableString.Invariant($"UPDATE dbo.EdiUserAgreement SET ERA_Content = 'Abacad' WHERE ERA_PK = '{agreement2PK}'");
			TestConnection.ExecuteNonQuery(commandText);
			commandText = FormattableString.Invariant($"UPDATE dbo.EdiUserAgreement SET ERA_Content = 'Abacad' WHERE ERA_PK = '{agreement1PK}'");
			try
			{
				TestConnection.ExecuteNonQuery(commandText);
				Fail("Should have SqlException");
			}
			catch (SqlException ex)
			{
				AssertEquals("Changing content should be disallowed once effective time has passed", @"Update operation NOT allowed on EdiUserAgreement for this/these column(s) if already effective.
The transaction ended in the trigger. The batch has been aborted.", ex.Message);
			}
		}

		public void TestEdiUserAgreementShouldNotEditEffectiveTimeIfEffectiveTimeInPast()
		{
			SetupEdiUserAgreementForEditConstraintTest();
			var futureTime = ZDateTime.UtcNow.AddHours(3).ToISO8601String();
			var commandText = FormattableString.Invariant($"UPDATE dbo.EdiUserAgreement SET ERA_EffectiveTimeUtc = '{futureTime}' WHERE ERA_PK = '{agreement1PK}'");
			try
			{
				TestConnection.ExecuteNonQuery(commandText);
				Fail("Should have SqlException");
			}
			catch (SqlException ex)
			{
				AssertEquals("Effective time cannot be changed once already effective", @"Update operation NOT allowed on EdiUserAgreement for this/these column(s) if already effective.
The transaction ended in the trigger. The batch has been aborted.", ex.Message);
			}
		}

		public void TestEdiUserAgreementShouldNotEditERA_VersionNumberIfEffectiveTimeInPast()
		{
			SetupEdiUserAgreementForEditConstraintTest();
			var commandText = FormattableString.Invariant($"UPDATE dbo.EdiUserAgreement SET ERA_VersionNumber = 3 WHERE ERA_PK = '{agreement2PK}'");
			TestConnection.ExecuteNonQuery(commandText);
			commandText = FormattableString.Invariant($"UPDATE dbo.EdiUserAgreement SET ERA_VersionNumber = 4 WHERE ERA_PK = '{agreement1PK}'");
			try
			{
				TestConnection.ExecuteNonQuery(commandText);
				Fail("Should have SqlException");
			}
			catch (SqlException ex)
			{
				AssertEquals("Changing version number should be disallowed once effective time has passed", @"Update operation NOT allowed on EdiUserAgreement for this/these column(s) if already effective.
The transaction ended in the trigger. The batch has been aborted.", ex.Message);
			}
		}

		public void TestEdiUserAgreementShouldNotEditERA_RN_NKCountryCodeIfEffectiveTimeInPast()
		{
			SetupEdiUserAgreementForEditConstraintTest();
			var commandText = FormattableString.Invariant($"UPDATE dbo.EdiUserAgreement SET ERA_RN_NKCountryCode = 'NZ' WHERE ERA_PK = '{agreement2PK}'");
			TestConnection.ExecuteNonQuery(commandText);
			commandText = FormattableString.Invariant($"UPDATE dbo.EdiUserAgreement SET ERA_RN_NKCountryCode = 'NZ' WHERE ERA_PK = '{agreement1PK}'");
			try
			{
				TestConnection.ExecuteNonQuery(commandText);
				Fail("Should have SqlException");
			}
			catch (SqlException ex)
			{
				AssertEquals("Changing country code should be disallowed once effective time has passed", @"Update operation NOT allowed on EdiUserAgreement for this/these column(s) if already effective.
The transaction ended in the trigger. The batch has been aborted.", ex.Message);
			}
		}

		public void TestEdiUserAgreementShouldNotEditERA_TypeIfEffectiveTimeInPast()
		{
			SetupEdiUserAgreementForEditConstraintTest();
			var commandText = FormattableString.Invariant($"UPDATE dbo.EdiUserAgreement SET ERA_Type = 'AAA' WHERE ERA_PK = '{agreement2PK}'");
			TestConnection.ExecuteNonQuery(commandText);
			commandText = FormattableString.Invariant($"UPDATE dbo.EdiUserAgreement SET ERA_Type = 'ABB' WHERE ERA_PK = '{agreement1PK}'");
			try
			{
				TestConnection.ExecuteNonQuery(commandText);
				Fail("Should have SqlException");
			}
			catch (SqlException ex)
			{
				AssertEquals("Changing type should be disallowed once effective time has passed", @"Update operation NOT allowed on EdiUserAgreement for this/these column(s) if already effective.
The transaction ended in the trigger. The batch has been aborted.", ex.Message);
			}
		}

		public void TestEdiUserAgreementAuditColumnsShouldBeEditableIfEffectiveTimeInPast()
		{
			SetupEdiUserAgreementForEditConstraintTest();
			var commandText = FormattableString.Invariant($@"
UPDATE dbo.EdiUserAgreement SET ERA_SystemLastEditUser = 'E' WHERE ERA_PK = '{agreement1PK}'
UPDATE dbo.EdiUserAgreement SET ERA_SystemLastEditTimeUtc = '2019-01-01' WHERE ERA_PK = '{agreement1PK}'");
			AssertNoExceptionThrown("Changing Audit columns should not cause exception", () =>
			{
				TestConnection.ExecuteNonQuery(commandText);
			});
		}

		public void TestEdiUserAgreementChangeEffectiveTimeToPastTimeShouldNotBeAllowed()
		{
			SetupEdiUserAgreementForEditConstraintTest();
			var futureTime = ZDateTime.UtcNow.AddHours(3).ToISO8601String();
			var pastTime = ZDateTime.UtcNow.AddMinutes(-1).ToISO8601String();
			//Change update trigger checking that effective time is not in the past
			//Change update trigger checking that effective time is not in the past
			//Change update trigger checking that effective time is not in the past
			//Change update trigger checking that effective time is not in the past
			//Change update trigger checking that effective time is not in the past
			//Change update trigger checking that effective time is not in the past
			var commandText = FormattableString.Invariant($"UPDATE dbo.EdiUserAgreement SET ERA_EffectiveTimeUtc = '{futureTime}' WHERE ERA_PK = '{agreement2PK}'");
			AssertNoExceptionThrown("Setting this date should not cause exception", () =>
			{
				TestConnection.ExecuteNonQuery(commandText);
			});
			commandText = FormattableString.Invariant($"UPDATE dbo.EdiUserAgreement SET ERA_EffectiveTimeUtc = '{pastTime}' WHERE ERA_PK = '{agreement2PK}'");
			try
			{
				TestConnection.ExecuteNonQuery(commandText);
				Fail("Should have SqlException");
			}
			catch (SqlException ex)
			{
				AssertEquals("Changing to past time should not be allowed", @"Update operation NOT allowed to set Effective Time of EdiUserAgreement to past time.
The transaction ended in the trigger. The batch has been aborted.", ex.Message);
			}
		}

		public void TestEdiUserAgreementInsertPastEffectiveTimeShouldNotBeAllowed()
		{
			agreement1PK = Guid.NewGuid();
			agreement2PK = Guid.NewGuid();
			var futureTime = ZDateTime.UtcNow.AddHours(1).ToISO8601String();
			var pastTime = ZDateTime.UtcNow.AddMinutes(-1).ToISO8601String();
			var commandText = FormattableString.Invariant($@"
INSERT INTO dbo.EdiUserAgreement (ERA_PK, ERA_Type, ERA_RN_NKCountryCode, ERA_VersionNumber, ERA_EffectiveTimeUtc, ERA_SystemCreateTimeUtc, ERA_SystemCreateUser, ERA_SystemLastEditTimeUtc, ERA_SystemLastEditUser) VALUES ('{agreement2PK}', 'DCA', 'AU', 1, '{futureTime}', '2019-01-01', 'E', '2019-01-01', 'E')
");
			AssertNoExceptionThrown("Setting this date should not cause exception", () =>
			{
				TestConnection.ExecuteNonQuery(commandText);
			});
			commandText = FormattableString.Invariant($@"
INSERT INTO dbo.EdiUserAgreement (ERA_PK, ERA_Type, ERA_RN_NKCountryCode, ERA_VersionNumber, ERA_EffectiveTimeUtc, ERA_SystemCreateTimeUtc, ERA_SystemCreateUser, ERA_SystemLastEditTimeUtc, ERA_SystemLastEditUser) VALUES ('{agreement1PK}', 'DCA', 'AU', 2, '{pastTime}', '2019-01-01', 'E', '2019-01-01', 'E')
");
			try
			{
				TestConnection.ExecuteNonQuery(commandText);
				Fail("Should have SqlException");
			}
			catch (SqlException ex)
			{
				AssertEquals("Setting to past time should not be allowed", @"Insert operation NOT allowed to set Effective Time of EdiUserAgreement to past time.
The transaction ended in the trigger. The batch has been aborted.", ex.Message);
			}
		}

		void SetupEdiUserAgreementForEditConstraintTest()
		{
			agreement1PK = Guid.NewGuid();
			agreement2PK = Guid.NewGuid();
			var pastTime = ZDateTime.UtcNow.AddHours(-1).ToISO8601String();
			var futureTime = ZDateTime.UtcNow.AddHours(1).ToISO8601String();
			var commandText = FormattableString.Invariant($@"
DISABLE TRIGGER TG_INS_EdiUserAgreement ON EdiUserAgreement

INSERT INTO dbo.EdiUserAgreement (ERA_PK, ERA_Type, ERA_RN_NKCountryCode, ERA_VersionNumber, ERA_EffectiveTimeUtc, ERA_SystemCreateTimeUtc, ERA_SystemCreateUser, ERA_SystemLastEditTimeUtc, ERA_SystemLastEditUser) VALUES ('{agreement1PK}', 'DCA', 'AU', 1, '{pastTime}', '2019-01-01', 'E', '2019-01-01', 'E')
INSERT INTO dbo.EdiUserAgreement (ERA_PK, ERA_Type, ERA_RN_NKCountryCode, ERA_VersionNumber, ERA_EffectiveTimeUtc, ERA_SystemCreateTimeUtc, ERA_SystemCreateUser, ERA_SystemLastEditTimeUtc, ERA_SystemLastEditUser) VALUES ('{agreement2PK}', 'DCA', 'AU', 2, '{futureTime}', '2019-01-01', 'E', '2019-01-01', 'E')

; ENABLE TRIGGER TG_INS_EdiUserAgreement ON EdiUserAgreement
");
			TestConnection.ExecuteNonQuery(commandText);
		}

		Guid agreement1PK;
		Guid agreement2PK;
		#endregion
		#region ViewClientProcessHeader
		public void TestOverridesViewClientProcessHeaderForIncidents()
		{
			var job = BMSTestHelper.CreateJobHeader<SupportIncident>(Factory, addDefaultProcessHeaderIfNone: false);
			var incident = (SupportIncident)job.Parent;
			AssertNotEquals("Incident number should not be empty.", "", incident.IM_IncidentNumber);
			Factory.Save();
			var result = Factory.LoadFromPrimaryKeysAndTableCode(new ZGuid[] { job.PK }, new ZString[] { new ZString(IncidentMainSchema.Constants.Prefix) });
			AssertEquals(1, result.Length);
			var processHeader = result.FirstOrDefault();
			AssertNotNull("The process header for the incident should be able to be loaded as a ViewProcessHeader.", processHeader);
			AssertEquals("The process header's job code should be the incident number.", incident.IM_IncidentNumber, processHeader.VFH_JobCode);
		}

		public void TestViewProcessHeader_WhenIncidentProcessHeadersHaveInconsistentParentTableCodesAndWorkflowTypes_ShouldOnlyHaveOneRowPerProcessHeader()
		{
			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<SupportIncident>(Factory, "Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<SupportIncident>(Factory, "Workflow 2");
			AssertEquals("Workflows should have their fields properly set. If this doesn't even work then please address this problem first.", IncidentMainSchema.Constants.Prefix, workflow1.FH_ParentTableCode);
			AssertEquals("Workflows should have their fields properly set. If this doesn't even work then please address this problem first.", EDIJobInvoicingConsumerTypes.Incident.Code, workflow1.FH_WorkflowType);
			workflow1.FH_ParentTableCode = "ZZ";
			workflow2.FH_WorkflowType = "ZZZ";
			Factory.Save();
			var rowCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.ViewProcessHeader");
			AssertEquals("There should be exactly four rows (2 workflows and 2 job-level workflows), not duplicated ones, even though the data on the ProcessHeader rows is bad. SAD!", 4, rowCount);
		}

		#endregion
		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			EDIClientDbSchemaUpgradeForTest.UpgradeViewClientProcessHeader(TestConnection);
			LicenceCompany.ClearStandardPricesCompanyCache();
		}

		protected override void TearDown()
		{
			LicenceCompany.ClearStandardPricesCompanyCache();
			base.TearDown();
		}

		#endregion
	}
}
