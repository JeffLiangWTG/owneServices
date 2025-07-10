using System;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Licensing;
using CargoWise.ProductRegistration.Service;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.ProductRegistration.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.ProductRegistration.Test
{
	public class RegistrationRepositoryTest : TestCaseWithFactory
	{
		public void TestSchema()
		{
			AssertEquals(LicenceDatabaseSchema.Constants.LD_DatabaseNumber, RegistrationRepository.DatabaseSchema.DatabaseNumber);
			AssertEquals(LicenceDatabaseSchema.Constants.LD_DBServerSecurityMode, RegistrationRepository.DatabaseSchema.DatabaseSecurityMode);
			AssertEquals(LicenceDatabaseSchema.Constants.LD_LicenceType, RegistrationRepository.DatabaseSchema.DatabaseType);
			AssertEquals(LicenceDatabaseSchema.Constants.LD_HostedLocation, RegistrationRepository.DatabaseSchema.HostedLocation);
			AssertEquals(LicenceDatabaseSchema.Constants.LD_LicenceExpiry, RegistrationRepository.DatabaseSchema.LicenceExpiry);
			AssertEquals(LicenceDatabaseSchema.Constants.LD_ServerCode, RegistrationRepository.DatabaseSchema.ServerCode);
		}

		public void TestGetStatus()
		{
			var db1 = CreateLicenceDatabase("ENT", "DB1", "", "", ZDateTime.Empty);
			var db2 = CreateLicenceDatabase("ENT", "DB2", "", "", ZDateTime.Empty);
			db1.LD_Status = DatabaseStatusList.Codes.REG;
			db2.LD_Status = DatabaseStatusList.Codes.NON;
			Factory.Save();
			IDbConnectionInternals conn = Db.Connection;
			var repository = new RegistrationRepository(conn.ADOConnection, conn.ADOTransaction);
			var status1reg = repository.GetStatus("ENTDB1").Result;
			var status2non = repository.GetStatus("ENTDB2").Result;
			var status3bad = repository.GetStatus("ENTZZZ").Result;
			var status4bad = repository.GetStatus("ZZZDB1").Result;
			AssertEquals(DatabaseStatusList.Codes.REG, status1reg.Status);
			AssertEquals(db1.LD_DatabaseNumber, status1reg.DatabaseNumber);
			AssertEquals("NON", status2non.Status);
			AssertEquals(db2.LD_DatabaseNumber, status2non.DatabaseNumber);
			AssertNull("product key not found", status3bad);
			AssertNull("product key not found", status4bad);
		}

		public void TestRegister()
		{
			var licenceDb = CreateLicenceDatabase("ENT", "SRV", "", "", ZDateTime.Empty);
			var licenceDb2 = CreateLicenceDatabase("EN2", "SRV", "", "", ZDateTime.Empty);
			Factory.Save();
			IDbConnectionInternals conn = Db.Connection;
			var repository = new RegistrationRepository(conn.ADOConnection, conn.ADOTransaction);
			var request = new RegisterRequest();
			request.ProductKey = "ENTSRV";
			request.UniqueKey = new DatabaseUniqueKey();
			request.UniqueKey.DatabaseCreated = new DateTime(2014, 9, 29);
			request.UniqueKey.DatabaseName = "mydb";
			request.UniqueKey.ServerName = "myserver";
			request.ProductVersion = "1.2.3.4";
			var x = repository.Register(request, "pwd");
			Assert("task completed", x.Wait(30000));
			var result = x.Result;
			licenceDb.Reload();
			AssertEquals((int)RegisterStatus.Success, result.ReturnCode);
			AssertEquals(DatabaseStatusList.Codes.REG, licenceDb.LD_Status);
			AssertEquals("", licenceDb.LD_HostConnectionServerName);
			AssertEquals("myserver", licenceDb.LD_HostServerName);
			AssertEquals("mydb", licenceDb.LD_HostDBName);
			request = new RegisterRequest();
			request.ProductKey = "EN2SRV";
			request.UniqueKey = new DatabaseUniqueKey();
			request.UniqueKey.DatabaseCreated = new DateTime(2014, 9, 29);
			request.UniqueKey.DatabaseName = "mydb";
			request.UniqueKey.ServerName = "myserver";
			request.UniqueKey.ConnectionServerName = "server.wisecloud.com";
			request.ProductVersion = "1.2.3.4";
			x = repository.Register(request, "pwd");
			Assert("task completed", x.Wait(30000));
			result = x.Result;
			licenceDb2.Reload();
			AssertEquals((int)RegisterStatus.Success, result.ReturnCode);
			AssertEquals(DatabaseStatusList.Codes.REG, licenceDb2.LD_Status);
			AssertEquals("server.wisecloud.com", licenceDb2.LD_HostConnectionServerName);
			AssertEquals("myserver", licenceDb2.LD_HostServerName);
			AssertEquals("mydb", licenceDb2.LD_HostDBName);
		}

		public void TestVerify()
		{
			var build1 = Factory.New<ReleaseBuild>();
			build1.HL_ExeVersionDate = ZDateTime.Now.AddDays(-1);
			build1.HL_ReleaseStatus = "ALP";
			build1.HL_MajorVersion = 14;
			build1.HL_MinorVersion = 5;
			build1.HL_Release = 15;
			build1.HL_Patch = 2;
			build1.HL_Product = "ENT";
			var build2 = Factory.New<ReleaseBuild>();
			build2.HL_ExeVersionDate = ZDateTime.Now.AddDays(-1);
			build2.HL_ReleaseStatus = "ALP";
			build2.HL_MajorVersion = 15;
			build2.HL_MinorVersion = 6;
			build2.HL_Release = 22;
			build2.HL_Patch = 1;
			build2.HL_Product = "CW1";
			var buildWrongProduct = Factory.New<ReleaseBuild>();
			buildWrongProduct.HL_ExeVersionDate = ZDateTime.Now.AddDays(-1);
			buildWrongProduct.HL_ReleaseStatus = "ALP";
			buildWrongProduct.HL_MajorVersion = 1;
			buildWrongProduct.HL_MinorVersion = 2;
			buildWrongProduct.HL_Release = 3;
			buildWrongProduct.HL_Patch = 4;
			buildWrongProduct.HL_Product = "SPH";
			var uniqueKey = new DatabaseUniqueKey();
			uniqueKey.DatabaseCreated = new DateTime(2014, 9, 29);
			uniqueKey.DatabaseName = "mydb";
			uniqueKey.ServerName = "myserver";
			var uniqueKey3 = new DatabaseUniqueKey();
			uniqueKey3.DatabaseCreated = new DateTime(2015, 5, 26);
			uniqueKey3.DatabaseName = "mydb";
			uniqueKey3.ServerName = "myserver3";
			uniqueKey3.ConnectionServerName = "server.wisecloud.com";
			Guid groupId = Guid.NewGuid();
			var db1 = CreateLicenceDatabase("ENT", "DB1", "myserver", "mydb", new DateTime(2014, 9, 29));
			var db2 = CreateLicenceDatabase("ENT", "DB2", "myserver2", "mydb", new DateTime(2014, 9, 29, 0, 0, 1));
			var db3 = CreateLicenceDatabase("ENT", "DB3", "myserver3", "mydb", new DateTime(2015, 5, 26));
			db3.LD_HostConnectionServerName = "server.wisecloud.com";
			db3.LD_HostedLocation = "SYD";
			db1.LD_Status = DatabaseStatusList.Codes.REG;
			db2.LD_Status = DatabaseStatusList.Codes.REG;
			db3.LD_Status = DatabaseStatusList.Codes.REG;
			db1.LD_Password = SHA512Encryptor.Encrypt(db1.LD_DatabaseNumber.ToString() + "pwd1");
			db3.LD_Password = "pwd3";
			Factory.Save();
			IDbConnectionInternals conn = Db.Connection;
			var repository = new RegistrationRepository(conn.ADOConnection, conn.ADOTransaction);
			var request = new VerifyRequest();
			request.UniqueKey = uniqueKey;
			var verify1 = repository.Verify(db1.LD_DatabaseNumber, db1.LD_Password, request).Result;
			// they add an availability group
			uniqueKey.GroupId = groupId;
			var verify1group = repository.Verify(db1.LD_DatabaseNumber, db1.LD_Password, request).Result;
			// failover to another server in the group
			uniqueKey.ServerName = "myfailoverserver";
			uniqueKey.DatabaseCreated = uniqueKey.DatabaseCreated.AddDays(1);
			var verify1failover = repository.Verify(db1.LD_DatabaseNumber, db1.LD_Password, request).Result;
			uniqueKey.GroupId = Guid.NewGuid();
			var verify1newGroup = repository.Verify(db1.LD_DatabaseNumber, db1.LD_Password, request).Result;
			uniqueKey.GroupId = null;
			var verify1ungroup = repository.Verify(db1.LD_DatabaseNumber, db1.LD_Password, request).Result;
			var verify1ungroupRepeat = repository.Verify(db1.LD_DatabaseNumber, db1.LD_Password, request).Result;
			request.ProductVersion = buildWrongProduct.VersionNumber.ToString();
			repository.Verify(db1.LD_DatabaseNumber, db1.LD_Password, request);
			request.ProductVersion = "Z.Z";
			repository.Verify(db1.LD_DatabaseNumber, db1.LD_Password, request);
			request.ProductVersion = build1.VersionNumber.ToString();
			repository.Verify(db1.LD_DatabaseNumber, db1.LD_Password, request);
			request.ProductVersion = build2.VersionNumber.ToString();
			uniqueKey.DatabaseName = "anotherdb";
			var verifyWrongDbName = repository.Verify(db1.LD_DatabaseNumber, db1.LD_Password, request).Result;
			request.ProductVersion = "";
			uniqueKey.DatabaseName = "mydb";
			uniqueKey.ServerName = "anotherserver";
			var verifyWrongServerName = repository.Verify(db1.LD_DatabaseNumber, db1.LD_Password, request).Result;
			request.ProductVersion = null;
			uniqueKey.ServerName = "myfailoverserver";
			uniqueKey.DatabaseCreated = uniqueKey.DatabaseCreated.AddDays(1);
			var verifyWrongDate = repository.Verify(db1.LD_DatabaseNumber, db1.LD_Password, request).Result;
			var request3 = new VerifyRequest();
			request3.UniqueKey = uniqueKey3;
			request3.ProductVersion = build2.VersionNumber.ToString();
			var verify3 = repository.Verify(db3.LD_DatabaseNumber, db3.LD_Password, request3).Result;
			request3.ProductVersion = "9.9.9.9";
			uniqueKey3.ServerName = "newServer";
			var verify3HostedMoved = repository.Verify(db3.LD_DatabaseNumber, db3.LD_Password, request3).Result;
			request3.ProductVersion = "0.0";
			uniqueKey3.DatabaseCreated = new DateTime(2015, 5, 1);
			var verify3HostedMoved2 = repository.Verify(db3.LD_DatabaseNumber, db3.LD_Password, request3).Result;
			request3.ProductVersion = null;
			uniqueKey3.DatabaseName = "anotherDb";
			var verify3WrongDb = repository.Verify(db3.LD_DatabaseNumber, db3.LD_Password, request3).Result;
			db1.Reload();
			db3.Reload();
			AssertEquals((int)RegisterStatus.Success, verify1.ReturnCode);
			AssertEquals((int)RegisterStatus.UniqueKeyUpdated, verify1group.ReturnCode);
			AssertEquals((int)RegisterStatus.UniqueKeyUpdated, verify1failover.ReturnCode);
			AssertEquals((int)RegisterStatus.UniqueKeyUpdated, verify1newGroup.ReturnCode);
			AssertEquals((int)RegisterStatus.UniqueKeyUpdated, verify1ungroup.ReturnCode);
			AssertEquals((int)RegisterStatus.Success, verify1ungroupRepeat.ReturnCode);
			AssertEquals("ENT", verify1ungroupRepeat.EnterpriseCode);
			AssertEquals("DB1", verify1ungroupRepeat.ServerCode);
			AssertEquals(db1.LD_DatabaseNumber, verify1ungroupRepeat.DatabaseNumber);
			AssertEquals((int)RegisterStatus.UniqueKeyNotMatched, verifyWrongDbName.ReturnCode);
			AssertEquals((int)RegisterStatus.UniqueKeyNotMatched, verifyWrongServerName.ReturnCode);
			AssertEquals((int)RegisterStatus.UniqueKeyNotMatched, verifyWrongDate.ReturnCode);
			AssertEquals((int)RegisterStatus.Success, verify3.ReturnCode);
			AssertEquals((int)RegisterStatus.UniqueKeyUpdated, verify3HostedMoved.ReturnCode);
			AssertEquals((int)RegisterStatus.UniqueKeyUpdated, verify3HostedMoved2.ReturnCode);
			AssertEquals((int)RegisterStatus.UniqueKeyNotMatched, verify3WrongDb.ReturnCode);
		}

		public void TestUnregister()
		{
			var db1 = CreateLicenceDatabase("ENT", "DB1", "myserver", "mydb", new DateTime(2014, 9, 29));
			var db2 = CreateLicenceDatabase("ENT", "DB2", "myserver2", "mydb", new DateTime(2014, 9, 29, 0, 0, 1));
			db1.LD_Status = DatabaseStatusList.Codes.REG;
			db1.LD_Password = "pwd";
			db2.LD_Status = DatabaseStatusList.Codes.REG;
			db2.LD_Password = "pwd";
			Factory.Save();
			IDbConnectionInternals conn = Db.Connection;
			var repository = new RegistrationRepository(conn.ADOConnection, conn.ADOTransaction);
			AssertEquals(0, repository.Unregister(db1.LD_DatabaseNumber, "pwd").Result);
			AssertEquals(2, repository.Unregister(db1.LD_DatabaseNumber, "pwd").Result);
			AssertEquals(0, repository.Unregister(db2.LD_DatabaseNumber, "pwd").Result);
			AssertEquals(2, repository.Unregister(db2.LD_DatabaseNumber, "pwd").Result);
			AssertEquals(1, repository.Unregister(98765, "pwd").Result);
		}

		LicenceDatabase CreateLicenceDatabase(string entCode, string serverCode, string serverName, string dbName, ZDateTime created)
		{
			var db = Factory.New<LicenceDatabase>();
			var entBizo = Factory.LoadFromNaturalKey<LicenceEnterprise>(LicenceEnterpriseSchema.LE_EnterpriseCode, entCode) ?? Factory.NewWithValidTestData<LicenceEnterprise>();
			entBizo.LE_EnterpriseCode = entCode;
			db.LD_LE = entBizo.PK;
			db.LD_ServerCode = serverCode;
			db.LD_HostDBCreateDate = created;
			db.LD_HostDBName = dbName;
			db.LD_HostServerName = serverName;
			return db;
		}
	}
}