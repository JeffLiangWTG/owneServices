using System;
using CargoWise.Data;
using Enterprise.ZArchitecture.Core.Encryption;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class LicenceBuilderTest : TransactionedTestCase
	{
		public void TestGetProductRegistrationKey()
		{
			string keyXml =
	@"<?xml version=""1.0"" encoding=""utf-16""?>
	<RegistrationKey xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
	  <DatabaseNumber>19</DatabaseNumber>
	  <DbUniqueKey>
		<ServerName>MyServer</ServerName>
		<DatabaseName>MyDb</DatabaseName>
		<DatabaseCreated>2014-03-03T00:00:00</DatabaseCreated>
		<GroupId xsi:nil=""true"" />
	  </DbUniqueKey>
	  <IssueDate>2014-08-22T00:00:00</IssueDate>
	  <ExpiryDate>2014-09-22T00:00:00</ExpiryDate>
	  <EnterpriseCode>ENT</EnterpriseCode>
	  <ServerCode>SYD</ServerCode>
	  <Password>{password}</Password>
	  <DbType>PRD</DbType>
	  <DbSecurityMode>LCK</DbSecurityMode>
	  <HostedLocation>SYD</HostedLocation>
	</RegistrationKey>";

			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			string encrypted = encoder.Encrypt(keyXml);

			string sqlText = String.Format(@"
				INSERT dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_BinaryValue) VALUES
					(newid(), '{0}', null, null, 'STR', convert(varbinary(max), N'{1}'));",
				LicenceBuilder.ProductRegItem, encrypted);
			TestConnection.ExecuteNonQuery(sqlText);

			var key = LicenceBuilder.GetProductRegistrationKey(TestConnection, Db.DatabaseName);

			AssertEquals(19, key.DatabaseNumber);
			AssertEquals("ENT", key.EnterpriseCode);
			AssertEquals("SYD", key.ServerCode);
			AssertEquals("{password}", key.Password);
			AssertEquals("PRD", key.DbType);
			AssertEquals("SYD", key.HostedLocation);
			AssertEquals("LCK", key.DbSecurityMode);
		}

		public void TestAdjustCompanyLicencesToFitTestSystem()
		{
			var encoder = LicenceBuilder.GetNewLicenceEncoder();
			const string originalXml1 = @"<LicenceKey><CheckPoints><COR Type=""PUR"" User=""1"" Expiry=""00010101"" /><VIM Type=""REN"" User=""2"" Expiry=""00010101"" /><ACP Type=""SRU"" User=""3"" Expiry=""00010101"" /><UKS Type=""TRI"" User=""4"" Expiry=""00010101"" /></CheckPoints><Company CountryPK=""e4eb6e97-aa78-4c46-bf86-35b7fbb5fb0e"" PhysicalServerID=""PRD"" /><Branches/><InstallationDetails/></LicenceKey>";
			const string originalXml2 = @"<LicenceKey><CheckPoints><NXC Type=""REN"" User=""5"" Expiry=""00010101"" /><ACC Type=""OPN"" User=""6"" Expiry=""00010101"" /><CMD Type=""OTM"" User=""7"" Expiry=""00010101"" /><VAU Type=""PUR"" User=""8"" Expiry=""00010101"" /></CheckPoints><Company CountryPK=""e4eb6e97-aa78-4c46-bf86-35b7fbb5fb0e"" PhysicalServerID='PRD' /><Branches/><InstallationDetails/></LicenceKey>";
			string encrypted1 = encoder.Encrypt(originalXml1);
			string encrypted2 = encoder.Encrypt(originalXml2);
			Guid testCompanyPk1 = Guid.NewGuid();
			Guid testCompanyPk2 = Guid.NewGuid();

			string sqlText = String.Format(@"
				INSERT dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_BinaryValue) VALUES
					(newid(), '{0}', '{1}', null, 'STR', convert(varbinary(max), N'{2}')),
					(newid(), '{0}', '{3}', null, 'STR', convert(varbinary(max), N'{4}'));",
				LicenceBuilder.LicenceRegItem,
				testCompanyPk1.ToString(), encrypted1,
				testCompanyPk2.ToString(), encrypted2);
			TestConnection.ExecuteNonQuery(sqlText);

			var licenceBuilder = new LicenceBuilder();
			licenceBuilder.AdjustCompanyLicencesToFitTestSystem(TestConnection, Db.DatabaseName, "TZZ");

			string xml1 = encoder.Decrypt(LoadCompanyLicence(testCompanyPk1).Trim());
			string xml2 = encoder.Decrypt(LoadCompanyLicence(testCompanyPk2).Trim());

			AssertPhysicalServerId(xml1, "TZZ");
			AssertPhysicalServerId(xml2, "TZZ");

			licenceBuilder = new LicenceBuilder();
			licenceBuilder.AdjustCompanyLicencesToFitTestSystem(TestConnection, Db.DatabaseName, "");
			xml1 = encoder.Decrypt(LoadCompanyLicence(testCompanyPk1).Trim());
			xml2 = encoder.Decrypt(LoadCompanyLicence(testCompanyPk2).Trim());
			AssertPhysicalServerId(xml1, "");
			AssertPhysicalServerId(xml2, "");
		}

		string LoadCompanyLicence(Guid companyPk)
		{
			string sqlText = String.Format(@"
				SELECT convert(nvarchar(max), SD_BinaryValue)
				FROM dbo.StmData
				WHERE SD_Name = '{0}'
				AND SD_Owner = '{1}'
				AND SD_DepartmentGuid is null",
				LicenceBuilder.LicenceRegItem, companyPk.ToString());
			string encryptedKey = TestConnection.ExecuteScalar(sqlText).ToString();
			return encryptedKey;
		}

		public static void AssertPhysicalServerId(string xml, string serverId)
		{
			Assert("PhysicalServerId=" + serverId + " (" + xml + ")", xml.Contains(@"PhysicalServerID=""" + serverId + @""""));
		}

		public static string DecryptedLicenceXml(string encryptedKey)
		{
			var encoder = LicenceBuilder.GetNewLicenceEncoder();
			return encoder.Decrypt(encryptedKey.Trim());
		}

		public void TestAdjustCompanyLicencesToFitTestSystemHandlesInvalidKeyValues()
		{
			Guid testCompanyPkDbNullValue = Guid.NewGuid();
			Guid testCompanyPkEmptyValue = Guid.NewGuid();
			Guid testCompanyPkInvalidValue = Guid.NewGuid();

			string sqlText = String.Format(@"
				DELETE dbo.StmData WHERE SD_Name = '{0}';
				INSERT dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_BinaryValue) VALUES
					(newid(), '{0}', '{1}', null, 'STR', convert(varbinary(max), {2})),
					(newid(), '{0}', '{3}', null, 'STR', convert(varbinary(max), N'{4}')),
					(newid(), '{0}', '{5}', null, 'STR', convert(varbinary(max), N'{6}'));",
				LicenceBuilder.LicenceRegItem,
				testCompanyPkDbNullValue.ToString(), "null",
				testCompanyPkEmptyValue.ToString(), "",
				testCompanyPkInvalidValue.ToString(), "_some_invalid_encrypted_key_");
			TestConnection.ExecuteNonQuery(sqlText);

			var licenceBuilder = new LicenceBuilder();
			licenceBuilder.AdjustCompanyLicencesToFitTestSystem(TestConnection, Db.DatabaseName, "");

			AssertAdjustedLicenceRegistryValue(testCompanyPkDbNullValue, DBNull.Value, "DBNull");
			AssertAdjustedLicenceRegistryValue(testCompanyPkEmptyValue, "", "Empty");
			AssertAdjustedLicenceRegistryValue(testCompanyPkInvalidValue, "_some_invalid_encrypted_key_", "Invalid");
		}

		void AssertAdjustedLicenceRegistryValue(Guid companyPk, object expectedValue, string originalLabel)
		{
			string sqlText = String.Format(@"
				SELECT convert(nvarchar(max), SD_BinaryValue)
				FROM dbo.StmData
				WHERE SD_Name = '{0}'
				AND SD_Owner = '{1}'",
				LicenceBuilder.LicenceRegItem, companyPk.ToString());
			object adjustedValue = TestConnection.ExecuteScalar(sqlText);
			AssertEquals(originalLabel + "Original Key => Adjusted value:", expectedValue, adjustedValue);
		}
	}

	class LicenceBuilderNonTransactionedTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestGetSystemRegistrationKey_StmDataBelongsToSchemaOtherThanDbo_ReturnsNull()
		{
			WithTestStmData(() =>
			{
				AssertNull(LicenceBuilder.GetSystemRegistrationKey(Db.Connection, TestDbName));
			});
		}

		[ExpectNoExceptions]
		public void TestGetProductRegistrationKey_StmDataBelongsToSchemaOtherThanDbo_ReturnsNull()
		{
			WithTestStmData(() =>
			{
				AssertNull(LicenceBuilder.GetProductRegistrationKey(Db.Connection, TestDbName));
			});
		}

		static void WithTestStmData(Action testAction)
		{
			using (AdoTestUtils.CreateDbDropExistingDisposable(TestDbName, Db.DatabaseName))
			using (var connection = Db.NewAdminConnection(TestDbName))
			{
				connection.ExecuteNonQuery(CreateTestStmDataScript);
				testAction?.Invoke();
			}
		}

		const string TestDbName = nameof(LicenceBuilderTest);

		const string CreateTestStmDataScript = @"
IF NOT EXISTS(SELECT null FROM sys.schemas WHERE name = 'Test') EXEC('CREATE SCHEMA [Test]');

CREATE TABLE [Test].[StmData] (PK uniqueidentifier NOT NULL)
";
	}
}
