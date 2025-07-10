using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class LicenceBuilderNonTransactionalTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestCreateSystemRegistrationKeyForNewTestSystem()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				RecreateSysRegistrationKey(adminConnection, new LicenceBuilder(), "~!@");
				string selectSysKeySql = String.Format(
					"SELECT convert(nvarchar(max), SD_BinaryValue) FROM dbo.StmData WHERE SD_Name = '{0}'",
					LicenceBuilder.SysKeyRegItem);
				string encryptedKey = adminConnection.ExecuteScalar(selectSysKeySql).ToString();

				ISystemRegistrationKey actualSystemKey = SystemRegistrationKey.NewFromEncryptedXmlKey(encryptedKey);

				AssertEquals("System Expiry Date in the future?", true, actualSystemKey.SystemExpiryDate > DateTime.UtcNow);
				AssertEquals("Server SID", AdminConnection.ServerSid, actualSystemKey.ServerSid);
				AssertEquals("Server Instance", adminConnection.ServerInstanceName, actualSystemKey.DbInstanceName);
				AssertEquals("Database Name", Db.DatabaseName, actualSystemKey.DatabaseName);
				AssertEquals("Database Type", DatabaseTypes.Codes.Test, actualSystemKey.DatabaseType);
				AssertEquals("DB Security Mode", DatabaseSecurityModePairList.Codes.Locked, actualSystemKey.DbSecurityMode);
				AssertEquals("Hosted Location", "~!@", actualSystemKey.HostedLocation);
			}
		}

		[UseSnapshotProtection]
		public void TestGetHostedLocationFromRestoredDatabaseBeforeClearingProductionData()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				var licenceBuilder = new LicenceBuilder();

				CleanupAndRecreateProductRegistrationKey(adminConnection, licenceBuilder, "~T4");
				var testHostedLocation = licenceBuilder.GetHostedLocationFromRestoredDatabaseBeforeClearingProductionData(adminConnection, Db.DatabaseName);
				AssertEquals("Hosted Location (4)", "~T4", testHostedLocation);

				CleanupAndRecreateProductRegistrationKey(adminConnection, licenceBuilder, null);
				RecreateSysRegistrationKey(adminConnection, licenceBuilder, "~T1");
				testHostedLocation = licenceBuilder.GetHostedLocationFromRestoredDatabaseBeforeClearingProductionData(adminConnection, Db.DatabaseName);
				AssertEquals("Hosted Location (1)", "~T1", testHostedLocation);

				RecreateSysRegistrationKey(adminConnection, licenceBuilder, "~T2");
				testHostedLocation = licenceBuilder.GetHostedLocationFromRestoredDatabaseBeforeClearingProductionData(adminConnection, Db.DatabaseName);
				AssertEquals("Hosted Location (2)", "~T2", testHostedLocation);

				RecreateSysRegistrationKey(adminConnection, licenceBuilder, null);
				testHostedLocation = licenceBuilder.GetHostedLocationFromRestoredDatabaseBeforeClearingProductionData(adminConnection, Db.DatabaseName);
				AssertEquals("Hosted Location (3)", "", testHostedLocation);
			}
		}

		void RecreateSysRegistrationKey(AdminConnection adminConnection, LicenceBuilder licenceBuilder, string hostedLocation)
		{
			adminConnection.ExecuteNonQuery(String.Format("DELETE dbo.StmData WHERE SD_Name = '{0}'", LicenceBuilder.SysKeyRegItem));

			if (hostedLocation != null)
			{
				licenceBuilder.CreateSystemRegistrationKeyForNewTestSystem(adminConnection, Db.DatabaseName, hostedLocation);
			}
		}

		void CleanupAndRecreateProductRegistrationKey(AdminConnection adminConnection, LicenceBuilder licenceBuilder, string hostedLocation)
		{
			adminConnection.ExecuteNonQuery(String.Format("DELETE dbo.StmData WHERE SD_Name = '{0}'", LicenceBuilder.ProductRegItem));

			if (hostedLocation == null)
			{
				return;
			}

			var keyXml =
	@$"<?xml version=""1.0"" encoding=""utf-16""?>
	<RegistrationKey xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
	  <HostedLocation>{hostedLocation}</HostedLocation>
	</RegistrationKey>";

			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			var encrypted = encoder.Encrypt(keyXml);

			var sqlText = String.Format(@"
				INSERT dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_BinaryValue) VALUES
					(newid(), '{0}', null, null, 'STR', convert(varbinary(max), N'{1}'));",
				LicenceBuilder.ProductRegItem, encrypted);
			adminConnection.ExecuteNonQuery(sqlText);
		}
	}
}
