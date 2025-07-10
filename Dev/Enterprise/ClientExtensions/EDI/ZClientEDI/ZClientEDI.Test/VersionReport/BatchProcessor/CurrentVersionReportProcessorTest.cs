using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Xml;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Licensing;
using Enterprise.MailManager.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.VersionReporting.BatchProcessor.Testing
{
	class CurrentVersionReportProcessorTest : VersionReportProcessorTest
	{
		[TestDate(2015, 7, 6, 16, 7, 0)]
		public void TestBasicFormat()
		{
			SetupTestData();
			var encryptedRegistrationKey = SetupRegisteredDatabaseAndReturnKey(Database);
			SetVersionNumber(NewBuild, "1.1.1990.10000");
			Database.LD_PublicEmailAddressForUpdate = "test@test.com";
			Database.LD_InternalSmtpEmailAddress = "blah@test.com";
			Database.LD_NoOfActivePrintQueues = 7;
			Factory.Save();
			var reportTimeUtc = new ZDateTime(2015, 7, 5, 11, 30, 0, DateTimeKind.Utc);
			const string xmlTemplate = @"<?xml version=""1.0"" encoding=""utf-8""?>
<VersionReport>
  <DatabaseNumber>98765</DatabaseNumber>
  <CurrentVersion>1.1.1990.10000</CurrentVersion>
  <CurrentDate>{CurrentDate}</CurrentDate>
  <RegistrationData>{test reg key}</RegistrationData>
</VersionReport>";
			var reportXml = xmlTemplate.Replace("{CurrentDate}", reportTimeUtc.ToString("o")).Replace("{test reg key}", encryptedRegistrationKey);
			var logger = new TestServiceLogger();
			var processor = new CurrentVersionReportProcessorForTest(logger);
			processor.Process(reportXml);
			var factory2 = new BusinessObjectFactory();
			var dbReloaded = factory2.Load<LicenceDatabase>(Database.PK);
			AssertEquals(NewBuild.PK, dbReloaded.LD_HL_CurrentRunningVersion);
			AssertEquals(reportTimeUtc, dbReloaded.LD_CurrentVersionFirstReportUtc);
			AssertEquals(reportTimeUtc, dbReloaded.LD_CurrentVersionLastReportUtc);
			AssertEquals("no change", "test@test.com", Database.LD_PublicEmailAddressForUpdate);
			AssertEquals("no change", "blah@test.com", Database.LD_InternalSmtpEmailAddress);
			AssertEquals("no change", 7, (int)Database.LD_NoOfActivePrintQueues);
			var reportTimeUtc2 = new ZDateTime(2015, 7, 6, 12, 30, 0, DateTimeKind.Utc);
			reportXml = xmlTemplate.Replace("{CurrentDate}", reportTimeUtc2.ToString("o")).Replace("{test reg key}", encryptedRegistrationKey);
			logger = new TestServiceLogger();
			processor = new CurrentVersionReportProcessorForTest(logger);
			processor.Process(reportXml);
			factory2 = new BusinessObjectFactory();
			dbReloaded = factory2.Load<LicenceDatabase>(Database.PK);
			AssertEquals(NewBuild.PK, dbReloaded.LD_HL_CurrentRunningVersion);
			AssertEquals(reportTimeUtc, dbReloaded.LD_CurrentVersionFirstReportUtc);
			AssertEquals(reportTimeUtc2, dbReloaded.LD_CurrentVersionLastReportUtc);
		}

		[TestDate(2015, 4, 23, 16, 21, 0)]
		public void TestFormat201504()
		{
			SetupTestData();
			var encryptedRegistrationKey = SetupRegisteredDatabaseAndReturnKey(Database);
			Factory.Save();
			var builder = BuildXmlInFormat201504(Database, encryptedRegistrationKey);
			var reportXml = builder.ToString();
			var logger = new TestServiceLogger();
			var processor = new CurrentVersionReportProcessorForTest(logger);
			processor.Process(reportXml);
			var factory2 = new BusinessObjectFactory();
			var dbReloaded = factory2.Load<LicenceDatabase>(Database.PK);
			var clientCompanyList = factory2.Load<ClientCompany>(new ZQuery(ClientCompanySchema.LCC_LD, Database.PK));
			AssertEquals("last heartbeat updated", TestDateAttribute.Date, dbReloaded.LD_LastHeartbeat);
			AssertEquals("ediproduction@xyz.com", dbReloaded.LD_PublicEmailAddressForUpdate);
			AssertEquals("Information|Report for ABC-123 [DB# 98765] for SQL Server [DBS] database [DBN] has been received", logger[0]);
			AssertEquals("Information|Report for ABC-123 [DB# 98765] processed OK", logger[1]);
			AssertEquals(2, logger.Count);
			AssertEquals("ClientCompany", 1, clientCompanyList.Length);
			var client1 = clientCompanyList[0];
			CombineAssertions(() =>
			{
				AssertEquals("Code", "XYZ", client1.LCC_Code);
				AssertEquals("Client PK", new ZGuid("878d7aca-ffc3-49fc-9710-969ca0c0f2ac"), client1.LCC_ClientPK);
				AssertEquals("Xyz Co", client1.LCC_Name);
				AssertEquals("AU", client1.LCC_RN_NKCountryCode);
			});
			client1.LCC_Name = "renamed";
			client1.Factory.Save();
			processor = new CurrentVersionReportProcessorForTest(logger);
			processor.Process(reportXml);
			factory2 = new BusinessObjectFactory();
			dbReloaded = factory2.Load<LicenceDatabase>(Database.PK);
			clientCompanyList = factory2.Load<ClientCompany>(new ZQuery(ClientCompanySchema.LCC_LD, Database.PK));
			client1 = clientCompanyList[0];
			AssertEquals("Name is updated for an existing ClientCompany", "Xyz Co", client1.LCC_Name);
		}

		[TestDate(2015, 4, 23, 16, 21, 0)]
		public void TestFormat201504_PasswordIncorrect()
		{
			SetupTestData();
			var encryptedRegistrationKey = SetupRegisteredDatabaseAndReturnKey(Database);
			Database.LD_Password = "-";
			Factory.Save();
			var data = new LicenceConsumptionLogSchema();
			data.Items.AddNew();
			string encryptedUsage = LicenceUsageReportBuilder.GetCompressedEncryptedText(data);
			var builder = BuildXmlInFormat201504(Database, encryptedRegistrationKey, encryptedUsage);
			var reportXml = builder.ToString();
			var logger = new TestServiceLogger();
			var processor = new CurrentVersionReportProcessorForTest(logger);
			processor.Process(reportXml);
			var dbReloaded = new BusinessObjectFactory().Load<LicenceDatabase>(Database.PK);
			AssertEquals("heartbeat not processed", ZDateTime.Empty, dbReloaded.LD_LastHeartbeat);
			var logs = logger.ToString();
			AssertContains("Information|Report for ABC-123 [DB# 98765] password incorrect - ignoring", logs);
			AssertNotContains("Information|Licence", logs);
			AssertEquals(2, logger.Count);
		}

		[TestDate(2015, 4, 23, 16, 21, 0)]
		public void TestFormat201504_LicenceUsage()
		{
			SetupTestData();
			var encryptedRegistrationKey = SetupRegisteredDatabaseAndReturnKey(Database);
			Factory.Save();
			var data = new LicenceConsumptionLogSchema();
			data.Items.AddNew();
			string encryptedUsage = LicenceUsageReportBuilder.GetCompressedEncryptedText(data);
			var builder = BuildXmlInFormat201504(Database, encryptedRegistrationKey, encryptedUsage);
			var reportXml = builder.ToString();
			var logger = new TestServiceLogger();
			var processor = new CurrentVersionReportProcessorForTest(logger);
			processor.ShouldHandleLicenceUsage = false;
			processor.Process(reportXml);
			AssertEquals(encryptedUsage, processor.LastLicenceUsage);
			AssertEquals(1, processor.CallsToHandleLicenceUsage);
			ReportProcessorHelper.ClearReportsFromTesting("LicenceUsage");
		}

		[TestDate(2020, 01, 05)]
		public void TestFormat201504ImportDeactivationOfUserAccounts()
		{
			SetupTestData();
			Database.LD_LicenceType = DatabaseTypes.Codes.Test;
			var encryptedRegistrationKey = SetupRegisteredDatabaseAndReturnKey(Database);
			Factory.Save();
			var org = Database.LicEnterprise.Organisation;
			org.Contacts.RemoveAndDeleteAll();
			Database.LD_OH_WebAccessOrg = org.PK;
			Factory.Save();
			var existingUserAccount1 = Factory.New<EdiCustomerUserAccount>();
			existingUserAccount1.EUA_LD = Database.PK;
			existingUserAccount1.EUA_UserID = "AE1";
			existingUserAccount1.EUA_Email = "aaa@aaa.com";
			existingUserAccount1.EUA_FullName = "aaa";
			existingUserAccount1.EUA_IsActive = true;
			var existingUserAccount2 = Factory.New<EdiCustomerUserAccount>();
			existingUserAccount2.EUA_LD = Database.PK;
			existingUserAccount2.EUA_UserID = "IE1";
			existingUserAccount2.EUA_Email = "bbb@aaa.com";
			existingUserAccount2.EUA_FullName = "bbb";
			existingUserAccount2.EUA_IsActive = true;
			var inactiveExistingUserAccount1 = Factory.New<EdiCustomerUserAccount>();
			inactiveExistingUserAccount1.EUA_LD = Database.PK;
			inactiveExistingUserAccount1.EUA_UserID = "IE2";
			inactiveExistingUserAccount1.EUA_Email = "vvv@aaa.com";
			inactiveExistingUserAccount1.EUA_FullName = "ccc";
			inactiveExistingUserAccount1.EUA_IsActive = false;
			var inactiveExistingUserAccount2 = Factory.New<EdiCustomerUserAccount>();
			inactiveExistingUserAccount2.EUA_LD = Database.PK;
			inactiveExistingUserAccount2.EUA_UserID = "IE3";
			inactiveExistingUserAccount2.EUA_Email = "vva@aaa.com";
			inactiveExistingUserAccount2.EUA_FullName = "cca";
			inactiveExistingUserAccount2.EUA_IsActive = false;
			Factory.Save();
			var builder = BuildXmlInFormat201504ForDeactivationImport(Database, encryptedRegistrationKey);
			var reportXml = builder.ToString();
			var logger = new TestServiceLogger();
			var processor = new CurrentVersionReportProcessorForTest(logger);
			processor.Process(reportXml);
			var query = new ZQuery();
			query.OrderBy = "EUA_UserID";
			var userAccounts = new BusinessObjectFactory().Load<EdiCustomerUserAccount>(query);
			AssertEquals("New users should not be imported", 4, userAccounts.Length);
			AssertEquals(existingUserAccount1.PK, userAccounts[0].PK);
			AssertEquals("Should remain unchanged", "aaa@aaa.com", userAccounts[0].EUA_Email);
			AssertEquals("Should remain unchanged", "aaa", userAccounts[0].EUA_FullName);
			AssertEquals("Should remain unchanged", true, userAccounts[0].EUA_IsActive);
			AssertEquals("Should remain unchanged", ZDateTime.Empty, userAccounts[0].EUA_SystemVerifiedDateUtc);
			AssertEquals(existingUserAccount2.PK, userAccounts[1].PK);
			AssertEquals("Should remain unchanged", "bbb@aaa.com", userAccounts[1].EUA_Email);
			AssertEquals("Should remain unchanged", "bbb", userAccounts[1].EUA_FullName);
			AssertEquals("Should be updated", false, userAccounts[1].EUA_IsActive);
			AssertEquals("Should be updated", new ZDateTime(2020, 01, 05), userAccounts[1].EUA_SystemVerifiedDateUtc);
			AssertEquals(inactiveExistingUserAccount1.PK, userAccounts[2].PK);
			AssertEquals("Should remain unchanged", "vvv@aaa.com", userAccounts[2].EUA_Email);
			AssertEquals("Should remain unchanged", "ccc", userAccounts[2].EUA_FullName);
			AssertEquals("Should remain unchanged", false, userAccounts[2].EUA_IsActive);
			AssertEquals("Should remain unchanged", ZDateTime.Empty, userAccounts[2].EUA_SystemVerifiedDateUtc);
			AssertEquals(inactiveExistingUserAccount2.PK, userAccounts[3].PK);
			AssertEquals("Should remain unchanged", "vva@aaa.com", userAccounts[3].EUA_Email);
			AssertEquals("Should remain unchanged", "cca", userAccounts[3].EUA_FullName);
			AssertEquals("Should remain unchanged", false, userAccounts[3].EUA_IsActive);
			AssertEquals("Should remain unchanged", ZDateTime.Empty, userAccounts[3].EUA_SystemVerifiedDateUtc);
		}

		#region Format 2015-04
		string SetupRegisteredDatabaseAndReturnKey(LicenceDatabase db)
		{
			db.LD_LastHeartbeat = ZDateTime.Empty;
			db.LD_HostDBCreateDate = new ZDateTime(2011, 1, 1, 0, 0, 0);
			db.LD_HostServerName = "DBS";
			db.LD_HostDBName = "DBN";
			db.LD_Status = DatabaseStatusList.Codes.REG;
			string password = "123";
			var keyBuilder = BuildRegistrationKeyXml(db, password);
			var keyXml = keyBuilder.ToString();
			var signedXml = CargoWise.ProductRegistration.Service.MessageSigner.SignXml(keyXml);
			var passwordHash = CargoWise.eHub.Common.SHA512Encryptor.Encrypt(db.LD_DatabaseNumber.ToString() + password);
			db.LD_Password = passwordHash;
			return TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt(signedXml);
		}

		StringBuilder BuildRegistrationKeyXml(LicenceDatabase db, string password)
		{
			var s = new StringBuilder();
			s.AppendLine(@"<?xml version=""1.0"" encoding=""utf-16""?>");
			s.AppendLine(@"<RegistrationKey xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">");
			s.AppendFormat("  <DatabaseNumber>{0}</DatabaseNumber>\r\n", db.LD_DatabaseNumber);
			s.AppendLine(@"  <DbUniqueKey>");
			s.AppendFormat("    <ServerName>{0}</ServerName>\r\n", db.LD_HostServerName);
			s.AppendFormat("    <DatabaseName>{0}</DatabaseName>\r\n", db.LD_HostDBName);
			s.AppendFormat("    <DatabaseCreated>{0}</DatabaseCreated>\r\n", XmlConvert.ToString(db.LD_HostDBCreateDate.ToDateTime(), XmlDateTimeSerializationMode.RoundtripKind));
			s.AppendLine(@"    <GroupId xsi:nil=""true"" />");
			s.AppendLine(@"  </DbUniqueKey>");
			s.AppendLine(@"  <IssueDate>2014-08-22T00:00:12.8019493</IssueDate>");
			s.AppendLine(@"  <ExpiryDate>2014-10-21T00:00:12.8019493</ExpiryDate>");
			s.AppendFormat("  <EnterpriseCode>{0}</EnterpriseCode>\r\n", db.LicEnterprise.LE_EnterpriseCode);
			s.AppendFormat("  <ServerCode>{0}</ServerCode>\r\n", db.LD_ServerCode);
			s.AppendFormat("  <Password>{0}</Password>\r\n", password);
			s.AppendFormat("  <DbType>{0}</DbType>", db.LD_LicenceType);
			s.AppendFormat("  <DbSecurityMode>{0}</DbSecurityMode>\r\n", db.LD_DBServerSecurityMode);
			s.AppendFormat("  <HostedLocation>{0}</HostedLocation>\r\n", db.LD_HostedLocation);
			s.AppendLine(@"</RegistrationKey>");
			return s;
		}

		StringBuilder BuildXmlInFormat201504(LicenceDatabase db, string encryptedRegistrationKey, string licenceUsage = null)
		{
			var s = new StringBuilder();
			s.AppendLine(@"<?xml version=""1.0"" encoding=""utf-8""?>");
			s.AppendLine(@"<VersionReport>");
			s.AppendFormat("  <DatabaseNumber>{0}</DatabaseNumber>\r\n", db.LD_DatabaseNumber);
			s.AppendFormat("  <EnterpriseCode>{0}</EnterpriseCode>\r\n", db.LicEnterprise.LE_EnterpriseCode);
			s.AppendFormat("  <PhysicalServerID>{0}</PhysicalServerID>\r\n", db.LD_ServerCode);
			s.AppendFormat("  <DBServerName>{0}</DBServerName>\r\n", db.LD_HostServerName);
			s.AppendFormat("  <DBName>{0}</DBName>\r\n", db.LD_HostDBName);
			s.AppendLine(@"  <CurrentVersion>1.2.3.4</CurrentVersion>");
			s.AppendLine(@"  <CurrentDate>23/04/2015 11:01:02 AM</CurrentDate>");
			s.AppendLine(@"  <CurrentRelease>Alpha Release 2000 Jan 04 patch 4</CurrentRelease>");
			s.AppendLine(@"  <PreferredUpgradeMethod>DEF</PreferredUpgradeMethod>");
			s.AppendLine(@"  <DBServerSecurityMode>LCK</DBServerSecurityMode>");
			s.AppendLine(@"  <SQLServerName>DBS</SQLServerName>");
			s.AppendLine(@"  <SQLServerInstanceName />");
			s.AppendLine(@"  <POP3>");
			s.AppendLine(@"    <EmailAddress>ediproduction@xyz.com</EmailAddress>");
			s.AppendLine(@"    <UserName />");
			s.AppendLine(@"    <MailServer />");
			s.AppendLine(@"    <Port>110</Port>");
			s.AppendLine(@"  </POP3>");
			s.AppendLine(@"  <SMTP>");
			s.AppendLine(@"    <MailServer>XCH</MailServer>");
			s.AppendLine(@"    <Port>25</Port>");
			s.AppendLine(@"  </SMTP>");
			s.AppendLine(@"  <DBFileNameList>");
			s.AppendLine(@"    <DBFileName>C:\PROGRAM FILES\MICROSOFT SQL SERVER\MSSQL11.MSSQLSERVER\MSSQL\DATA\ODYSSEY.MDF</DBFileName>");
			s.AppendLine(@"    <DBFileName>C:\PROGRAM FILES\MICROSOFT SQL SERVER\MSSQL11.MSSQLSERVER\MSSQL\DATA\ODYSSEY_LOG.LDF</DBFileName>");
			s.AppendLine(@"    <DBFileName>C:\PROGRAM FILES\MICROSOFT SQL SERVER\MSSQL11.MSSQLSERVER\MSSQL\DATA\ODYSSEY_DATA02.NDF</DBFileName>");
			s.AppendLine(@"    <DBFileName>C:\PROGRAM FILES\MICROSOFT SQL SERVER\MSSQL11.MSSQLSERVER\MSSQL\DATA\ODYSSEY_SD001_DATA.MDF</DBFileName>");
			s.AppendLine(@"    <DBFileName>C:\PROGRAM FILES\MICROSOFT SQL SERVER\MSSQL11.MSSQLSERVER\MSSQL\DATA\ODYSSEY_SD001_LOG.LDF</DBFileName>");
			s.AppendLine(@"  </DBFileNameList>");
			s.AppendLine(@"  <CompanyList>");
			s.AppendLine(@"    <Company>");
			s.AppendLine(@"      <Code>XYZ</Code>");
			s.AppendLine(@"      <Name>Xyz Co</Name>");
			s.AppendLine(@"      <CountryCode>AU</CountryCode>");
			s.AppendLine(@"      <CurrencyCode>AUD</CurrencyCode>");
			s.AppendLine(@"      <Address1>184 Bourke Road</Address1>");
			s.AppendLine(@"      <Address2 />");
			s.AppendLine(@"      <City>Alexandria</City>");
			s.AppendLine(@"      <State>NSW</State>");
			s.AppendLine(@"      <PostCode>2015</PostCode>");
			s.AppendLine(@"      <Phone>02 9025 1100</Phone>");
			s.AppendLine(@"      <BusinessRegNo>41 065 894 724</BusinessRegNo>");
			s.AppendLine(@"      <BusinessRegNo2>C065894724</BusinessRegNo2>");
			s.AppendLine(@"      <CustomsRegistrationNo>AAA374M</CustomsRegistrationNo>");
			s.AppendLine(@"      <WebAddress>www.edi.com.au</WebAddress>");
			s.AppendLine(@"      <IsActive>1</IsActive>");
			s.AppendLine(@"      <IsGSTRegistered>1</IsGSTRegistered>");
			s.AppendLine(@"      <IsGSTCashBasis>0</IsGSTCashBasis>");
			s.AppendLine(@"      <IsWHTRegistered>0</IsWHTRegistered>");
			s.AppendLine(@"      <IsWHTCashBasis>1</IsWHTCashBasis>");
			s.AppendLine(@"      <IsReciprocal>0</IsReciprocal>");
			s.AppendLine(@"      <PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>");
			s.AppendLine(@"    </Company>");
			s.AppendLine(@"  </CompanyList>");
			s.AppendLine(@"  <DatabaseBackupPath />");
			s.AppendLine(@"  <SystemHealthData />");
			s.AppendFormat("  <RegistrationData>{0}</RegistrationData>\r\n", SecurityElement.Escape(encryptedRegistrationKey));
			s.AppendLine(@"  <DocEngine>");
			s.AppendLine(@"    <ActivePrintersCount>0</ActivePrintersCount>");
			s.AppendLine(@"  </DocEngine>");
			s.AppendLine(@"  <SqlServerVersionDetails>");
			s.AppendLine(@"    <SqlServerCpuArchitecture>X64</SqlServerCpuArchitecture>");
			s.AppendLine(@"    <SqlServerEdition>EnterpriseDeveloper</SqlServerEdition>");
			s.AppendLine(@"    <SqlServerVersion>Sql2012</SqlServerVersion>");
			s.AppendLine(@"    <SqlServerFullVersionText>Microsoft SQL Server 2012 - 11.0.5058.0 (X64) ");
			s.AppendLine(@"	May 14 2014 18:34:29 ");
			s.AppendLine(@"	Copyright (c) Microsoft Corporation");
			s.AppendLine(@"	Developer Edition (64-bit) on Windows NT 6.3 &lt;X64&gt; (Build 9600: ) (Hypervisor)");
			s.AppendLine(@"</SqlServerFullVersionText>");
			s.AppendLine(@"  </SqlServerVersionDetails>");
			s.AppendFormat(@"  <LicenceUsage>{0}</LicenceUsage>", licenceUsage ?? "");
			s.AppendLine(@"</VersionReport>");
			return s;
		}

		StringBuilder BuildXmlInFormat201504ForDeactivationImport(LicenceDatabase db, string encryptedRegistrationKey, string licenceUsage = null)
		{
			var s = new StringBuilder();
			s.AppendLine(@"<?xml version=""1.0"" encoding=""utf-8""?>");
			s.AppendLine(@"<VersionReport>");
			s.AppendFormat("  <DatabaseNumber>{0}</DatabaseNumber>\r\n", db.LD_DatabaseNumber);
			s.AppendFormat("  <EnterpriseCode>{0}</EnterpriseCode>\r\n", db.LicEnterprise.LE_EnterpriseCode);
			s.AppendFormat("  <PhysicalServerID>{0}</PhysicalServerID>\r\n", db.LD_ServerCode);
			s.AppendFormat("  <DBServerName>{0}</DBServerName>\r\n", db.LD_HostServerName);
			s.AppendFormat("  <DBName>{0}</DBName>\r\n", db.LD_HostDBName);
			s.AppendLine(@"  <CurrentVersion>1.2.3.4</CurrentVersion>");
			s.AppendLine(@"  <CurrentDate>23/04/2015 11:01:02 AM</CurrentDate>");
			s.AppendLine(@"  <CurrentRelease>Alpha Release 2000 Jan 04 patch 4</CurrentRelease>");
			s.AppendLine(@"  <PreferredUpgradeMethod>DEF</PreferredUpgradeMethod>");
			s.AppendLine(@"  <DBServerSecurityMode>LCK</DBServerSecurityMode>");
			s.AppendLine(@"  <SQLServerName>DBS</SQLServerName>");
			s.AppendLine(@"  <SQLServerInstanceName />");
			s.AppendLine(@"  <POP3>");
			s.AppendLine(@"    <EmailAddress>ediproduction@xyz.com</EmailAddress>");
			s.AppendLine(@"    <UserName />");
			s.AppendLine(@"    <MailServer />");
			s.AppendLine(@"    <Port>110</Port>");
			s.AppendLine(@"  </POP3>");
			s.AppendLine(@"  <SMTP>");
			s.AppendLine(@"    <MailServer>XCH</MailServer>");
			s.AppendLine(@"    <Port>25</Port>");
			s.AppendLine(@"  </SMTP>");
			s.AppendLine(@"  <DBFileNameList>");
			s.AppendLine(@"    <DBFileName>C:\PROGRAM FILES\MICROSOFT SQL SERVER\MSSQL11.MSSQLSERVER\MSSQL\DATA\ODYSSEY.MDF</DBFileName>");
			s.AppendLine(@"    <DBFileName>C:\PROGRAM FILES\MICROSOFT SQL SERVER\MSSQL11.MSSQLSERVER\MSSQL\DATA\ODYSSEY_LOG.LDF</DBFileName>");
			s.AppendLine(@"    <DBFileName>C:\PROGRAM FILES\MICROSOFT SQL SERVER\MSSQL11.MSSQLSERVER\MSSQL\DATA\ODYSSEY_DATA02.NDF</DBFileName>");
			s.AppendLine(@"    <DBFileName>C:\PROGRAM FILES\MICROSOFT SQL SERVER\MSSQL11.MSSQLSERVER\MSSQL\DATA\ODYSSEY_SD001_DATA.MDF</DBFileName>");
			s.AppendLine(@"    <DBFileName>C:\PROGRAM FILES\MICROSOFT SQL SERVER\MSSQL11.MSSQLSERVER\MSSQL\DATA\ODYSSEY_SD001_LOG.LDF</DBFileName>");
			s.AppendLine(@"  </DBFileNameList>");
			s.AppendLine(@"  <StaffList>");
			s.AppendLine(@"    <Staff>");
			s.AppendLine(@"      <Code>AU1</Code>");
			s.AppendLine(@"      <Name>ActiveUser</Name>");
			s.AppendLine(@"      <EmailAddress>activeuser@.cw1.com</EmailAddress>");
			s.AppendLine(@"      <IsActive>1</IsActive>");
			s.AppendLine(@"    </Staff>");
			s.AppendLine(@"    <Staff>");
			s.AppendLine(@"      <Code>IU1</Code>");
			s.AppendLine(@"      <Name>InactiveUser</Name>");
			s.AppendLine(@"      <EmailAddress>inactiveuser@.cw1.com</EmailAddress>");
			s.AppendLine(@"      <IsActive>0</IsActive>");
			s.AppendLine(@"    </Staff>");
			s.AppendLine(@"    <Staff>");
			s.AppendLine(@"      <Code>AE1</Code>");
			s.AppendLine(@"      <Name>ActiveExistingUser</Name>");
			s.AppendLine(@"      <EmailAddress>activeexistinguser@.cw1.com</EmailAddress>");
			s.AppendLine(@"      <IsActive>1</IsActive>");
			s.AppendLine(@"    </Staff>");
			s.AppendLine(@"    <Staff>");
			s.AppendLine(@"      <Code>IE1</Code>");
			s.AppendLine(@"      <Name>InactiveExistingUser</Name>");
			s.AppendLine(@"      <EmailAddress>inactiveexistinguser@.cw1.com</EmailAddress>");
			s.AppendLine(@"      <IsActive>0</IsActive>");
			s.AppendLine(@"    </Staff>");
			s.AppendLine(@"    <Staff>");
			s.AppendLine(@"      <Code>IE2</Code>");
			s.AppendLine(@"      <Name>ReactivateExistingUser</Name>");
			s.AppendLine(@"      <EmailAddress>inactiveexistinguser2@.cw1.com</EmailAddress>");
			s.AppendLine(@"      <IsActive>1</IsActive>");
			s.AppendLine(@"    </Staff>");
			s.AppendLine(@"    <Staff>");
			s.AppendLine(@"      <Code>IE3</Code>");
			s.AppendLine(@"      <Name>DeactivateExistingDeactivatedUser</Name>");
			s.AppendLine(@"      <EmailAddress>inactiveexistinguser3@.cw1.com</EmailAddress>");
			s.AppendLine(@"      <IsActive>0</IsActive>");
			s.AppendLine(@"    </Staff>");
			s.AppendLine(@"  </StaffList>");
			s.AppendLine(@"  <DatabaseBackupPath />");
			s.AppendLine(@"  <SystemHealthData />");
			s.AppendFormat("  <RegistrationData>{0}</RegistrationData>\r\n", SecurityElement.Escape(encryptedRegistrationKey));
			s.AppendLine(@"  <DocEngine>");
			s.AppendLine(@"    <ActivePrintersCount>0</ActivePrintersCount>");
			s.AppendLine(@"  </DocEngine>");
			s.AppendLine(@"  <SqlServerVersionDetails>");
			s.AppendLine(@"    <SqlServerCpuArchitecture>X64</SqlServerCpuArchitecture>");
			s.AppendLine(@"    <SqlServerEdition>EnterpriseDeveloper</SqlServerEdition>");
			s.AppendLine(@"    <SqlServerVersion>Sql2012</SqlServerVersion>");
			s.AppendLine(@"    <SqlServerFullVersionText>Microsoft SQL Server 2012 - 11.0.5058.0 (X64) ");
			s.AppendLine(@"	May 14 2014 18:34:29 ");
			s.AppendLine(@"	Copyright (c) Microsoft Corporation");
			s.AppendLine(@"	Developer Edition (64-bit) on Windows NT 6.3 &lt;X64&gt; (Build 9600: ) (Hypervisor)");
			s.AppendLine(@"</SqlServerFullVersionText>");
			s.AppendLine(@"  </SqlServerVersionDetails>");
			s.AppendFormat(@"  <LicenceUsage>{0}</LicenceUsage>", licenceUsage ?? "");
			s.AppendLine(@"</VersionReport>");
			return s;
		}

		#endregion
		public void TestShouldUpdateExpiryDateOnClient()
		{
			SetupTestData();
			var report = new EDIVersionReport(GetXmlData(Organisation.PK));
			var processor = new CurrentVersionReportProcessorForTest(null);
			Database.LD_LicenceExpiry = new ZDateTime(2005, 12, 12);
			Database.LD_HostServerName = "";
			Database.LD_HostDBName = "";
			Database.LD_LicenceType = "";
			Database.LD_HostServerSID = ZGuid.Empty;
			Database.LD_IsActive = false;
			Database.LD_HostedLocation = "";
			AssertEquals("No Server Name, DB Name, Licence Type or SID", false, processor.ShouldUpdateExpiryDateOnClient_Exposed(Database, report));
			AssertEquals("ReasonForNotUpdating", "Database inactive", processor.ReasonForNotUpdating);
			Database.LD_HostServerName = "ABCD";
			Database.LD_HostDBName = "ABCD";
			Database.LD_LicenceType = "PRD";
			Database.LD_HostServerSID = report.ServerSID;
			Database.LD_DBServerSecurityMode = DatabaseSecurityModePairList.Codes.ExOpen;
			AssertEquals("Everything matches, but LicenceDatabase is not marked as Active", false, processor.ShouldUpdateExpiryDateOnClient_Exposed(Database, report));
			AssertEquals("ReasonForNotUpdating", "Database inactive", processor.ReasonForNotUpdating);
			Database.LD_IsActive = true;
			AssertEquals("Can Send", true, processor.ShouldUpdateExpiryDateOnClient_Exposed(Database, report));
			AssertEquals("ReasonForNotUpdating", "", processor.ReasonForNotUpdating);
			// Change each property, assert it shouldn't update, change it back, assert it should update
			AssertShouldNotUpdateExpiryDateIfPtyValueDoesNotMatchRequirement(Database.LD_HostServerNameInfo, ZString.Empty, processor, report, "Database HostServerName is blank");
			AssertShouldNotUpdateExpiryDateIfPtyValueDoesNotMatchRequirement(Database.LD_HostDBNameInfo, ZString.Empty, processor, report, "Database HostDBName is blank");
			AssertShouldNotUpdateExpiryDateIfPtyValueDoesNotMatchRequirement(Database.LD_LicenceTypeInfo, ZString.Empty, processor, report, "Database type is blank");
			AssertShouldNotUpdateExpiryDateIfPtyValueDoesNotMatchRequirement(Database.LD_HostServerSIDInfo, ZGuid.Empty, processor, report, "Database Server SID is blank");
			AssertShouldNotUpdateExpiryDateIfPtyValueDoesNotMatchRequirement(Database.LD_DBServerSecurityModeInfo, new ZString("LCK"), processor, report, "Reported DB security invalid");
			AssertShouldNotUpdateExpiryDateIfPtyValueDoesNotMatchRequirement(Database.LD_IsActiveInfo, ZBool.False, processor, report, "Database inactive");
			Database.LD_HostServerSID = ZGuid.NewZGuid();
			AssertEquals("Server SID mismatch", false, processor.ShouldUpdateExpiryDateOnClient_Exposed(Database, report));
			AssertEquals("ReasonForNotUpdating", "Database Server SID does not match report SID", processor.ReasonForNotUpdating);
			Database.LD_HostServerSID = report.ServerSID;
			// Check client with Open2012 security mode are allowed to have open reported security mode
			Database.LD_DBServerSecurityMode = DatabaseSecurityModePairList.Codes.OpenMode;
			AssertEquals("Security mode reported as OPEN, and it's Open2012 in our records", true, processor.ShouldUpdateExpiryDateOnClient_Exposed(Database, report));
			// Check HOSTED DATABASE are allowed to have open reported security mode
			Database.LD_DBServerSecurityMode = DatabaseSecurityModePairList.Codes.Locked;
			AssertEquals("Security mode expected to be locked but reported as open", false, processor.ShouldUpdateExpiryDateOnClient_Exposed(Database, report));
			AssertEquals("ReasonForNotUpdating", "Reported DB security invalid", processor.ReasonForNotUpdating);
			Database.LD_HostedLocation = "SYD";
			AssertEquals("Security mode reported as OPEN, but it's hosted by us", true, processor.ShouldUpdateExpiryDateOnClient_Exposed(Database, report));
			// Check no renewal if eHub not working
			SetVersionNumber(NewBuild, "1.4." + (LicenceDatabase.FirstReleaseSystemMessageRDU - 1) + ".0");
			Database.LD_HL_CurrentRunningVersion = NewBuild.PK;
			processor = new CurrentVersionReportProcessorForTest(null, false);
			AssertEquals("Report via email from an pre-eHub system", true, processor.ShouldUpdateExpiryDateOnClient_Exposed(Database, report));
			AssertEquals("ReasonForNotUpdating", "", processor.ReasonForNotUpdating);
			SetVersionNumber(NewBuild, "1.4." + LicenceDatabase.FirstReleaseSystemMessageRDU + ".0");
			AssertEquals("Report via email from an eHub system", false, processor.ShouldUpdateExpiryDateOnClient_Exposed(Database, report));
			AssertEquals("ReasonForNotUpdating", "EHub is not working - Database supports eHub heartbeats, but report received via email", processor.ReasonForNotUpdating);
			Database.LD_HL_CurrentRunningVersion = ZGuid.Empty;
			AssertEquals("Report via email from an unknown version", true, processor.ShouldUpdateExpiryDateOnClient_Exposed(Database, report));
			AssertEquals("ReasonForNotUpdating", "", processor.ReasonForNotUpdating);
			processor = new CurrentVersionReportProcessorForTest(null, true);
			AssertEquals("Report via eHub from an unknown version", true, processor.ShouldUpdateExpiryDateOnClient_Exposed(Database, report));
			AssertEquals("ReasonForNotUpdating", "", processor.ReasonForNotUpdating);
			Database.LD_HL_CurrentRunningVersion = NewBuild.PK;
			AssertEquals("Report via eHub from an eHub system", true, processor.ShouldUpdateExpiryDateOnClient_Exposed(Database, report));
			AssertEquals("ReasonForNotUpdating", "", processor.ReasonForNotUpdating);
		}

		void AssertShouldNotUpdateExpiryDateIfPtyValueDoesNotMatchRequirement(ZPropertyInfo ptyToChange, IZType changedValue, CurrentVersionReportProcessorForTest processor, EDIVersionReport report, string reason)
		{
			var previousValue = ptyToChange.Value;
			ptyToChange.Value = changedValue;
			AssertEquals(string.Format("Property [{0}] changed to [{1}], should update expiry date?", ptyToChange.Name, changedValue), false, processor.ShouldUpdateExpiryDateOnClient_Exposed(Database, report));
			AssertEquals("ReasonForNotUpdating", reason, processor.ReasonForNotUpdating);
			ptyToChange.Value = previousValue;
			AssertEquals(string.Format("Property [{0}] changed back to [{1}], should update expiry date?", ptyToChange.Name, previousValue), true, processor.ShouldUpdateExpiryDateOnClient_Exposed(Database, report));
		}

		[TestDate(2005, 11, 1)]
		public void TestDatabaseAlreadyUpdatedFromHeartbeat()
		{
			SetupTestData();
			string xmlData = GetXmlData(Organisation.PK);
			EDIVersionReport report = new EDIVersionReport(xmlData);
			TestDateAttribute.Date = SystemRegistrationKey.NewFromEncryptedXmlKey(report.EncryptedSystemExpirationKey).SystemExpiryDate.AddDays(-Licences.DefaultLicenceGracePeriodInDays);
			Assert("The last heartbeat should be empty", Database.LD_LastHeartbeat.IsEmpty);
			VersionReportProcessorForTest.Process(xmlData);
			Assert("The last heartbeat should NOT be empty", !Database.LD_LastHeartbeat.IsEmpty);
			ZDateTime oldVal = Database.LD_LastHeartbeat;
			TestServiceLogger logger = new TestServiceLogger();
			var processor = new CurrentVersionReportProcessorForTest(logger);
			processor.Process(xmlData);
			AssertEquals("The last heartbeat has not changed", oldVal, Database.LD_LastHeartbeat);
			AssertContains("Information|Report for ABC-XYZ-123 not processed - too soon since last report (" + TestDateAttribute.Date.ToString("dd-MMM-yyyy HH:mm:ss") + ")", logger.ToString());
		}

		[TestDate(2005, 11, 1)]
		public void TestLastHeartbeatAlwaysUpdatedEvenWhenThereAreNoOtherChanges()
		{
			SetupTestData();
			SetVersionNumber(NewBuild, "1.1.1990.10000");
			Factory.Save();
			string xmlData = GetXmlData(Organisation.PK);
			EDIVersionReport report = new EDIVersionReport(xmlData);
			TestDateAttribute.Date = SystemRegistrationKey.NewFromEncryptedXmlKey(report.EncryptedSystemExpirationKey).SystemExpiryDate.AddDays(-Licences.DefaultLicenceGracePeriodInDays);
			Assert("The last heartbeat should be empty", Database.LD_LastHeartbeat.IsEmpty);
			VersionReportProcessorForTest.Process(xmlData);
			Assert("The last heartbeat should NOT be empty", !Database.LD_LastHeartbeat.IsEmpty);
			Database.LD_LastHeartbeat = TestDateAttribute.Date.AddDays(-2);
			Factory.Save();
			ZDateTime oldVal = Database.LD_LastHeartbeat;
			VersionReportProcessorForTest.Process(xmlData);
			AssertNotEquals("The last heartbeat has changed", oldVal, Database.LD_LastHeartbeat);
		}

		public void TestDocEngineStats()
		{
			SetupTestData();
			string xmlData = GetXmlData(Organisation.PK);
			EDIVersionReport report = new EDIVersionReport(xmlData);
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals("Active Printer Count", report.ActivePrintersCount, Database.LD_NoOfActivePrintQueues);
		}

		public void TestPOP3Details()
		{
			SetupTestData();
			string xmlData = GetXmlData(Organisation.PK);
			EDIVersionReport report = new EDIVersionReport(xmlData);
			Database.LD_PublicEmailAddressForUpdate = ""; // Reset value
			Database.LD_InternalPop3EmailAddress = "";
			Database.LD_InternalPop3Port = 0;
			Database.LD_InternalPop3UserName = "";
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals("Public Email Address For Update", report.InternalPOP3EmailAddress, Database.LD_PublicEmailAddressForUpdate);
			AssertEquals("POP3 Server", report.InternalPOP3MailServer, Database.LD_InternalPop3EmailAddress);
			AssertEquals("POP3 Port", report.InternalMailServerPort, Database.LD_InternalPop3Port);
			AssertEquals("POP3 User Name", report.InternalPOP3UserName, Database.LD_InternalPop3UserName);
		}

		public void TestSMTPDetails()
		{
			SetupTestData();
			string xmlData = GetXmlData(Organisation.PK);
			EDIVersionReport report = new EDIVersionReport(xmlData);
			Database.LD_InternalSmtpEmailAddress = "";
			Database.LD_InternalSmtpPort = 0;
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals("SMTP Server", report.InternalSMTPMailServer, Database.LD_InternalSmtpEmailAddress);
			AssertEquals("SMTP Port", report.InternalSMTPPort, Database.LD_InternalSmtpPort);
		}

		public void TestDatabaseLocationsAndPaths()
		{
			SetupTestData();
			string xmlData = GetXmlData(Organisation.PK);
			EDIVersionReport report = new EDIVersionReport(xmlData);
			Database.LD_DatabaseFilePathDetail = "";
			VersionReportProcessorForTest.Process(xmlData);
			AssertContains("Files", @" C:\PROGRAM FILES\MICROSOFT SQL SERVER\MSSQL11.MSSQLSERVER\MSSQL\DATA\ODYSSEY.MDF", Database.LD_DatabaseFilePathDetail);
			AssertContains("Files", @" C:\PROGRAM FILES\MICROSOFT SQL SERVER\MSSQL11.MSSQLSERVER\MSSQL\DATA\ODYSSEY_LOG.LDF", Database.LD_DatabaseFilePathDetail);
			AssertContains("Files", @" C:\PROGRAM FILES\MICROSOFT SQL SERVER\MSSQL11.MSSQLSERVER\MSSQL\DATA\ODYSSEY_SD001_DATA.MDF", Database.LD_DatabaseFilePathDetail);
			AssertContains("Files", @" C:\PROGRAM FILES\MICROSOFT SQL SERVER\MSSQL11.MSSQLSERVER\MSSQL\DATA\ODYSSEY_SD001_LOG.LDF", Database.LD_DatabaseFilePathDetail);
		}

		public void TestHostServerSID()
		{
			SetupTestData();
			string xmlData = GetXmlData(Organisation.PK);
			EDIVersionReport report = new EDIVersionReport(xmlData);
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals("Server SID is set", report.ServerSID, Database.LD_HostServerSID);
			ZGuid testGuid = ZGuid.NewZGuid();
			Database.LD_HostServerSID = testGuid;
			Database.LD_LastHeartbeat = ZDateTime.Empty;
			Factory.Save();
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals("Server SID is updated since database fields match", report.ServerSID, Database.LD_HostServerSID);
			Database.LD_HostServerSID = testGuid;
			Database.LD_HostServerName = "changed";
			Database.LD_LastHeartbeat = ZDateTime.Empty;
			Factory.Save();
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals("Server SID not updated - server name mismatch", testGuid, Database.LD_HostServerSID);
			Database.LD_HostServerName = "DBS";
			Database.LD_HostDBName = "changed";
			Database.LD_LastHeartbeat = ZDateTime.Empty;
			Factory.Save();
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals("Server SID not updated - DB name mismatch", testGuid, Database.LD_HostServerSID);
			Database.LD_HostServerName = "DBS";
			Database.LD_HostDBName = "DBN";
			Database.LD_HostDBInstance = "changed";
			Database.LD_LastHeartbeat = ZDateTime.Empty;
			Factory.Save();
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals("Server SID not updated - DB instance mismatch", testGuid, Database.LD_HostServerSID);
			Database.LD_HostServerName = "DBS";
			Database.LD_HostDBName = "DBN";
			Database.LD_HostDBInstance = "";
			Database.LD_LastHeartbeat = ZDateTime.Empty;
			Factory.Save();
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals("Server SID updated - DB fields all match", report.ServerSID, Database.LD_HostServerSID);
			Database.LD_HostServerName = "changed";
			Database.LD_HostDBName = "changed";
			Database.LD_HostDBInstance = "changed";
			Database.LD_HostServerSID = testGuid;
			Database.LD_HostedLocation = "SYD";
			Database.LD_LastHeartbeat = ZDateTime.Empty;
			Factory.Save();
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals("Server SID for hosted clients is changed", report.ServerSID, Database.LD_HostServerSID);
		}

		public void TestDBServerName()
		{
			SetupTestData();
			string xmlData = GetXmlData(Organisation.PK);
			EDIVersionReport report = new EDIVersionReport(xmlData);
			Database.LD_HostServerSID = ZGuid.Empty;
			Database.LD_HostServerName = "";
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals("Host DB Server Name Set", report.SQLServerName, Database.LD_HostServerName);
			Database.LD_HostServerSID = ZGuid.NewZGuid();
			Database.LD_HostServerName = "";
			Factory.Save();
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals("Host DB Server Name Blank", "", Database.LD_HostServerName);
		}

		public void TestDBInstanceName()
		{
			SetupTestData();
			string xmlData = GetXmlData(Organisation.PK, true);
			EDIVersionReport report = new EDIVersionReport(xmlData);
			Database.LD_HostServerSID = ZGuid.Empty;
			Database.LD_HostDBInstance = "";
			Factory.Save();
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals("Host DB Instance Name Set", report.SQLServerInstanceName, Database.LD_HostDBInstance);
			Database.LD_HostServerSID = ZGuid.NewZGuid();
			Database.LD_HostDBInstance = "";
			Factory.Save();
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals("Host DB Instance Name Blank", "", Database.LD_HostDBInstance);
		}

		public void TestDBName()
		{
			SetupTestData();
			string xmlData = GetXmlData(Organisation.PK);
			EDIVersionReport report = new EDIVersionReport(xmlData);
			Database.LD_HostServerSID = ZGuid.Empty;
			Database.LD_HostDBName = "";
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals("Host DB Name Set", report.DBName, Database.LD_HostDBName);
			Database.LD_HostServerSID = ZGuid.NewZGuid();
			Database.LD_HostDBName = "";
			Factory.Save();
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals("Host DB Name Blank", "", Database.LD_HostDBName);
		}

		[TestDate(2005, 11, 16, 10, 10, 10)]
		public void TestLicenceKeyProcessing()
		{
			SetupTestData();
			var report = new EDIVersionReport(Organisation.PK, Enterprise.LE_EnterpriseCode, Company.LC_CompanyCode, Database.LD_ServerCode, "DBS", "DBN", "1.1.1990.10000", "", new ZDateTime(2004, 12, 31, 13, 56, 0));
			string xmlData = report.GenerateLegacyXmlForTest();
			VersionReportProcessorForTest.Process(xmlData);
			const string expected = @"MODULE: Core
 TYPE:
  CLIENT: PUR
  PROD: NON
 USER COUNT:
  CLIENT: 999
  PROD: 0

MODULE: Accountant
 TYPE:
  CLIENT: PUR
  PROD: NON
 USER COUNT:
  CLIENT: 999
  PROD: 0";
			var factory2 = new BusinessObjectFactory();
			var headerReloaded = factory2.Load<LicenceHeader>(Header.PK);
			AssertContains(expected, headerReloaded.DiscrepancyText);
		}

		public void TestSqlServerVersionDetails()
		{
			SetupTestData(false);
			string xmlData = GetXmlData(Organisation.PK);
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xmlData);
			xmlDocument.DocumentElement.RemoveChild(xmlDocument.DocumentElement.SelectSingleNode("SqlServerVersionDetails"));
			VersionReportProcessorForTest.Process(xmlDocument.OuterXml);
			AssertEquals("LD_SQLEdition", "", Database.LD_SQLEdition);
			AssertEquals("LD_SQLVersion", "", Database.LD_SQLVersion);
			AssertEquals("LD_SQLVerString", "", Database.LD_SQLVerString);
			Database.LD_LastHeartbeat = ZDateTime.Now.AddDays(-2);
			Factory.Save();
			VersionReportProcessorForTest.Process(xmlData);
			DbConnection connection = Db.Connection;
			AssertEquals("LD_SQLEdition", SqlServerVersionDetailsConverter.ToCode(connection.ServerEdition), Database.LD_SQLEdition);
			AssertEquals("LD_SQLVersion", connection.ServerVersionNumber.SqlServerGeneration, Database.LD_SQLVersion);
			AssertEquals("LD_SQLVerString", connection.ServerFullVersionText, Database.LD_SQLVerString);
		}

		public void TestAdditionalDatabaseServerInfo()
		{
			SetupTestData(false);
			string xmlData = GetXmlData(Organisation.PK);
			xmlData = xmlData.Replace("(Hypervisor)", "");
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals("Microsoft Windows 7 Enterprise", Database.LD_OSName);
			AssertEquals("6.1.7600 N/A Build 7600", Database.LD_OSVersion);
			AssertEquals("Gigabyte Technology Co., Ltd.", Database.LD_SystemManufacturer);
			AssertEquals(new ZDate(2008, 6, 18), Database.LD_BIOSDate);
			AssertEquals(8190, Database.LD_TotalPhysicalMemoryMB);
			AssertEquals(1, Database.LD_NoOfProcessorCores);
			AssertEquals("Intel64 Family 6 Model 23 Stepping 10 GenuineIntel", Database.LD_ProcessorType);
			AssertEquals(new ZDecimal(3000), Database.LD_ProcessorSpeedMHz);
			AssertEquals(false, Database.LD_VirtualMachineDetected);
			Assert(LicenceFilters.OSNames.ContainsCode("Microsoft Windows 7 Enterprise"));
			Assert(LicenceFilters.OSVersions.ContainsCode("6.1.7600 N/A Build 7600"));
			Assert(LicenceFilters.SystemManufacturers.ContainsCode("Gigabyte Technology Co., Ltd."));
			Assert(LicenceFilters.ProcessorTypes.ContainsCode("Intel64 Family 6 Model 23 Stepping 10 GenuineIntel"));
		}

		public void TestUpdateVirtualMachineDetected()
		{
			SetupTestData(false);
			string xmlData = GetXmlData(Organisation.PK);
			xmlData = xmlData.Replace("(Hypervisor)", "");
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals(false, Database.LD_VirtualMachineDetected);
			xmlData = xmlData.Insert(xmlData.LastIndexOf("</SqlServerFullVersionText>") - 1, " ####### (Hypervisor) #######");
			Database.LD_LastHeartbeat = ZDateTime.Now.AddDays(-2);
			Factory.Save();
			VersionReportProcessorForTest.Process(xmlData);
			Assert(Database.LD_SQLVerStringForDisplay.Contains("####### (Hypervisor) #######"));
			AssertEquals(true, Database.LD_VirtualMachineDetected);
			IEnumerable<string> virtualMachineKeywords = EDIDataRegistry.Instance.VirtualMachineDetectionKeywords.Value.Cast<ICodeDescription>().Select(p => p.Code);
			string keyWord = virtualMachineKeywords.First();
			xmlData = xmlData.Insert(xmlData.LastIndexOf("</SystemInfo>") - 1, " " + keyWord);
			Database.LD_LastHeartbeat = ZDateTime.Now.AddDays(-2);
			Factory.Save();
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals(true, Database.LD_VirtualMachineDetected);
			keyWord = virtualMachineKeywords.Last();
			xmlData = xmlData.Insert(xmlData.LastIndexOf("</SystemInfo>") - 1, " " + keyWord);
			Database.LD_LastHeartbeat = ZDateTime.Now.AddDays(-2);
			Factory.Save();
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals(true, Database.LD_VirtualMachineDetected);
		}

		[ExpectNoExceptions]
		[TestDate(2005, 11, 1)]
		public void TestAdditionalDatabaseServerInfo_NonEnglish()
		{
			//Cannot handle non-english info at the moment, but should still be able to process version report without exception.
			SetupTestData(false);
			Assert("The last heartbeat should be empty", Database.LD_LastHeartbeat.IsEmpty);
			var xmlData = GetXmlDataWithAdditionalInfoInChinese(Organisation.PK);
			xmlData = xmlData.Replace("(Hypervisor)", "");
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals("", Database.LD_OSName);
			AssertEquals("", Database.LD_OSVersion);
			AssertEquals("", Database.LD_SystemManufacturer);
			AssertEquals(ZDate.Empty, Database.LD_BIOSDate);
			AssertEquals(0, Database.LD_TotalPhysicalMemoryMB);
			AssertEquals(0, Database.LD_NoOfProcessorCores);
			AssertEquals("", Database.LD_ProcessorType);
			AssertEquals((ZDecimal)0, Database.LD_ProcessorSpeedMHz);
			AssertEquals(false, Database.LD_VirtualMachineDetected);
			Assert("The last heartbeat should NOT be empty", !Database.LD_LastHeartbeat.IsEmpty);
		}

		public void TestAdditionalDatabaseServerInfo_VMWare()
		{
			SetupTestData(false);
			string xmlData = GetXmlDataWithVMWare(Organisation.PK);
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals(true, Database.LD_VirtualMachineDetected);
		}

		[ExpectNoExceptions]
		public void TestAdditionalDatabaseServerInfo_CpuSpeedOverflow()
		{
			SetupTestData(false);
			List<string> additionalInfoList = new List<string>();
			additionalInfoList.Add("Host Name:                 SYD-WSCW-1");
			additionalInfoList.Add("OS Name:                   Microsoft Windows 7 Enterprise");
			additionalInfoList.Add("OS Version:                6.1.7600 N/A Build 7600");
			additionalInfoList.Add("System Manufacturer:       Gigabyte Technology Co., Ltd.");
			additionalInfoList.Add("BIOS Version:              Award Software International, Inc. F5, 18/06/2008");
			additionalInfoList.Add("System Locale:             en-us;English (United States)");
			additionalInfoList.Add("Total Physical Memory:     8,190 MB");
			additionalInfoList.Add("Time Zone:                 (UTC+10:00) Canberra, Melbourne, Sydney");
			additionalInfoList.Add("Processor(s):              Processor(s): 4 Processor(s) Installed");
			additionalInfoList.Add("                           [01]: Intel64 Family 6 Model 23 Stepping 10 GenuineIntel ~597456472 Mhz");
			additionalInfoList.Add("                           [02]: Intel64 Family 6 Model 23 Stepping 10 GenuineIntel ~-725376375 Mhz");
			additionalInfoList.Add("                           [03]: Intel64 Family 6 Model 23 Stepping 10 GenuineIntel ~20050647 Mhz");
			additionalInfoList.Add("                           [04]: Intel64 Family 6 Model 23 Stepping 10 GenuineIntel ~-285691684 Mhz");
			additionalInfoList.Add("System Model:              EP45-DS3P");
			var report = new EDIVersionReport(Organisation.PK, "ABC", "XYZ", "123", "DBS", "DBN", "1.1.1990.10000", "", new ZDateTime(2004, 12, 31, 13, 56, 0), additionalInfoList);
			string xmlData = report.GenerateLegacyXmlForTest();
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals("CPU speed should not be set", 0m, Database.LD_ProcessorSpeedMHz);
			Factory.Save();
		}

		[TestDate(2006, 10, 11, 10, 10, 10)]
		public void TestLastLicenceSyncCheck()
		{
			SetupTestData();
			Header.LA_LastLicenceCheckInSync = true;
			Factory.Save();
			var report = new EDIVersionReport(Organisation.PK, "ABC", "XYZ", "123", "DBS", "DBN", "1.1.1990.10000", "", new ZDateTime(2004, 12, 31, 13, 56, 0));
			var reportXml = report.GenerateLegacyXmlForTest();
			VersionReportProcessorForTest.Process(reportXml);
			AssertEquals("Last Licence Sync Check Date has been updated", new ZDateTime(2006, 10, 11, 10, 10, 10), Header.LA_LastLicenceSyncCheck);
			AssertEquals("Last Licence Sync In Check should be false", false, Header.LA_LastLicenceCheckInSync);
		}

		[TestDate(2005, 12, 01)]
		public void TestShouldUpdateExpiryDateOnClient_RequireUpdate()
		{
			SetupTestData();
			SetVersionNumber(NewBuild, "1.1.1990.10000");
			NewBuild.HL_ExeVersionDate = LicenceDatabase.DateFromWhichDatabaseHeartbeatCanBeReset;
			Factory.Save();
			string xmlData = GetXmlData(Organisation.PK).Replace("<DBServerSecurityMode>OPN</DBServerSecurityMode>", "<DBServerSecurityMode>LCK</DBServerSecurityMode>");
			var logger = new TestServiceLogger();
			var processor = new CurrentVersionReportProcessorForTest(logger);
			processor.Process(xmlData);
			AssertEquals("Number of emails sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef systemUpdatePacket = Env.OutgoingMailManager.EmailsCreated[0];
			var emails = Factory.Load<MailItem>(new ZQuery(MailDBItemsSchema.MI_Subject, LicenceDatabase.LegacyVersionReportRequestSubject));
			AssertEquals("new version report should be requested", 1, emails.Length);
			AssertEquals("after a delay", TestDateAttribute.Date + TimeSpan.FromMinutes(10), emails[0].MI_SendDateTime);
			AssertEquals("last heartbeat empty", ZDateTime.Empty, Database.LD_LastHeartbeat);
			AssertEquals("System Update Packet Subject", "ediEnterprise Reference Data Update", systemUpdatePacket.Subject);
			AssertContains("sent new system expiry via email", logger.ToString());
		}

		[TestDate(2006, 06, 01)] // More than 60 days away from date of next update
		public void TestShouldUpdateExpiryDateOnClient_Over60DaysRequireUpdate()
		{
			SetupTestData();
			string xmlData = GetXmlData(Organisation.PK).Replace("<DBServerSecurityMode>OPN</DBServerSecurityMode>", "<DBServerSecurityMode>LCK</DBServerSecurityMode>");
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals("Number of emails sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef systemUpdatePacket = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("System Update Packet Subject", "ediEnterprise Reference Data Update", systemUpdatePacket.Subject);
		}

		[TestDate(2005, 10, 25)] // Between 30 and 60 days of requiring update
		public void TestShouldUpdateExpiryDateOnClient_Between30And60DaysDontRequireUpdate()
		{
			var productRegistrationKey = SystemRegistrationKey.Current;
			try
			{
				var keyForTest = new SystemRegistrationKey(productRegistrationKey.SystemExpiryDate, productRegistrationKey.ServerSid, productRegistrationKey.DbInstanceName, productRegistrationKey.DatabaseName, productRegistrationKey.DatabaseType, productRegistrationKey.DbSecurityMode, productRegistrationKey.HostedLocation, "", "", "", "", 10.0d, 11.0d, new DateTime(2005, 10, 29, 16, 0, 0), "");
				EnvProxy.Instance.Registry.LegacyEncryptedSystemRegistrationKey = ((ISystemRegistrationKey)keyForTest).ToEncryptedKeyString();
				SetupTestData();
				var xmlData = GetXmlData(Organisation.PK);
				var report = new EDIVersionReport(xmlData);
				var processor = VersionReportProcessorForTest;
				processor.Process(xmlData);
				var logger = (TestServiceLogger)processor.ServiceLogger;
				AssertEquals("Number of emails sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertContains("no expiry update - Licence expiry doesn't need updating", logger[logger.Count - 1]);
			}
			finally
			{
				EnvProxy.Instance.Registry.LegacyEncryptedSystemRegistrationKey = productRegistrationKey.ToEncryptedKeyString();
			}
		}

		public void TestDbConfigXml()
		{
			SetupTestData();
			VersionReportProcessorForTest.Process(GetConfigXmlData());
			var expectedXml = @"<DbConfig>
  <MAXDOP>Ooh</MAXDOP>
  <CostOfParallelism>Ahh</CostOfParallelism>
  <OptimizeForAdHocWorkloads>Oo</OptimizeForAdHocWorkloads>
  <MinMemory>Ah</MinMemory>
  <MaxMemory>Aa</MaxMemory>
  <TotalAvailableMemory>Bing</TotalAvailableMemory>
  <TraceFlags>
    <TraceFlag>Bang</TraceFlag>
    <TraceFlag>Chitty</TraceFlag>
    <TraceFlag>Chity</TraceFlag>
    <TraceFlag>Baang</TraceFlag>
    <TraceFlag>Baaang</TraceFlag>
  </TraceFlags>
  <DbAlwaysOn>Now</DbAlwaysOn>
  <ParameterizationForced>Okay</ParameterizationForced>
  <AutoCreate>I'm</AutoCreate>
  <AutoUpdate>Done</AutoUpdate>
  <AutoUpdateAsync>Here</AutoUpdateAsync>
  <CompatibilityLevel>I</CompatibilityLevel>
  <Cardinality>Guess</Cardinality>
</DbConfig>";
			AssertEquals(expectedXml, Database.LD_DatabaseConfig);
		}

		string GetConfigXmlData()
		{
			List<string> additionalInfoList = new List<string>();
			additionalInfoList.Add("MAXDOP:                    Ooh");
			additionalInfoList.Add("CostOfParallelism:         Ahh");
			additionalInfoList.Add("OptimizeForAdHocWorkloads: Oo");
			additionalInfoList.Add("MinMemory:                 Ah");
			additionalInfoList.Add("MaxMemory:                 Aa");
			additionalInfoList.Add("TotalAvailableMemory:      Bing");
			additionalInfoList.Add("TraceFlags:                Bang;Chitty;Chity;Baang;Baaang");
			additionalInfoList.Add("ParameterizationForced:    Okay");
			additionalInfoList.Add("AutoCreate:                I'm");
			additionalInfoList.Add("AutoUpdate:                Done");
			additionalInfoList.Add("AutoUpdateAsync:           Here");
			additionalInfoList.Add("DbAlwaysOn:                Now");
			additionalInfoList.Add("CompatibilityLevel:        I");
			additionalInfoList.Add("Cardinality:               Guess");
			var report = new EDIVersionReport(Guid.Empty, "ABC", "XYZ", "123", "DBS", "DBN", "1.1.1990.10000", "", new ZDateTime(2004, 12, 31, 13, 56, 0), additionalInfoList);
			return report.GenerateLegacyXmlForTest();
		}

		[TestDate(2005, 10, 25)] // Between 30 and 60 days of requiring update
		public void TestShouldUpdateExpiryDateOnClient_SystemKeyBlank()
		{
			var productRegistrationKey = SystemRegistrationKey.Current;
			try
			{
				var keyForTest = new SystemRegistrationKey(productRegistrationKey.SystemExpiryDate, productRegistrationKey.ServerSid, productRegistrationKey.DbInstanceName, productRegistrationKey.DatabaseName, productRegistrationKey.DatabaseType, productRegistrationKey.DbSecurityMode, productRegistrationKey.HostedLocation, "", "", "", "", 10.0d, 11.0d, new DateTime(2005, 10, 29, 16, 0, 0), "");
				EnvProxy.Instance.Registry.LegacyEncryptedSystemRegistrationKey = ((ISystemRegistrationKey)keyForTest).ToEncryptedKeyString();
				SetupTestData();
				string xmlData = GetXmlData(Organisation.PK);
				EDIVersionReport report = new EDIVersionReport(xmlData);
				report.EncryptedSystemExpirationKey = "";
				report.DBServerSecurityMode = DatabaseSecurityModePairList.Codes.Locked;
				xmlData = report.GenerateLegacyXmlForTest();
				var processor = VersionReportProcessorForTest;
				processor.Process(xmlData);
				AssertEquals("Number of emails sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef systemUpdatePacket = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("System Update Packet Subject", "ediEnterprise Reference Data Update", systemUpdatePacket.Subject);
			}
			finally
			{
				EnvProxy.Instance.Registry.LegacyEncryptedSystemRegistrationKey = productRegistrationKey.ToEncryptedKeyString();
			}
		}

		[TestDate(2005, 10, 25)] // Between 30 and 60 days of requiring update
		public void TestShouldUpdateExpiryDateOnClient_SystemKeyBlank_ServerSidMismatch()
		{
			var productRegistrationKey = SystemRegistrationKey.Current;
			try
			{
				var keyForTest = new SystemRegistrationKey(productRegistrationKey.SystemExpiryDate, productRegistrationKey.ServerSid, productRegistrationKey.DbInstanceName, productRegistrationKey.DatabaseName, productRegistrationKey.DatabaseType, productRegistrationKey.DbSecurityMode, productRegistrationKey.HostedLocation, "", "", "", "", 10.0d, 11.0d, new DateTime(2005, 10, 29, 16, 0, 0), "");
				EnvProxy.Instance.Registry.LegacyEncryptedSystemRegistrationKey = ((ISystemRegistrationKey)keyForTest).ToEncryptedKeyString();
				SetupTestData();
				string xmlData = GetXmlData(Organisation.PK);
				EDIVersionReport report = new EDIVersionReport(xmlData);
				report.EncryptedSystemExpirationKey = "";
				report.DBServerSecurityMode = DatabaseSecurityModePairList.Codes.Locked;
				Database.LD_HostServerSID = ZGuid.NewZGuid();
				Database.LD_HostServerName = "SomeName";
				Database.LD_HostDBName = "SomeDb";
				Factory.Save();
				xmlData = report.GenerateLegacyXmlForTest();
				var processor = VersionReportProcessorForTest;
				processor.Process(xmlData);
				var logger = (TestServiceLogger)processor.ServiceLogger;
				AssertEquals("Number of emails sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertContains("no expiry update - Database Server SID does not match report SID", logger[logger.Count - 1]);
			}
			finally
			{
				EnvProxy.Instance.Registry.LegacyEncryptedSystemRegistrationKey = productRegistrationKey.ToEncryptedKeyString();
			}
		}

		[TestDate(2005, 6, 10)] // 10 days to expiry
		public void TestShouldUpdateExpiryDateOnClient_ManualExpiry()
		{
			var productRegistrationKey = SystemRegistrationKey.Current;
			try
			{
				var keyForTest = new SystemRegistrationKey(new DateTime(2005, 6, 20), productRegistrationKey.ServerSid, productRegistrationKey.DbInstanceName, productRegistrationKey.DatabaseName, productRegistrationKey.DatabaseType, DatabaseSecurityModePairList.Codes.ExOpen, productRegistrationKey.HostedLocation, "", "", "", "", 10.0d, 11.0d, new DateTime(2005, 10, 29, 16, 0, 0), "");
				EnvProxy.Instance.Registry.LegacyEncryptedSystemRegistrationKey = ((ISystemRegistrationKey)keyForTest).ToEncryptedKeyString();
				SetupTestData();
				Database.LD_ManualLicenceExpiry = new DateTime(2005, 6, 20);
				Database.LD_DBServerSecurityMode = DatabaseSecurityModePairList.Codes.ExOpen;
				Factory.Save();
				var xmlData = GetXmlData(Organisation.PK);
				var report = new EDIVersionReport(xmlData);
				var processor = VersionReportProcessorForTest;
				processor.Process(xmlData);
				var logger = (TestServiceLogger)processor.ServiceLogger;
				AssertEquals("Number of emails sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertContains("no expiry update - Licence expiry doesn't need updating", logger[logger.Count - 1]);
				Database.LD_ManualLicenceExpiry = new DateTime(2005, 6, 21);
				Database.LD_LastHeartbeat = ZDateTime.Empty;
				Factory.Save();
				processor = VersionReportProcessorForTest;
				processor.Process(xmlData);
				logger = (TestServiceLogger)processor.ServiceLogger;
				AssertEquals("Number of emails sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			}
			finally
			{
				EnvProxy.Instance.Registry.LegacyEncryptedSystemRegistrationKey = productRegistrationKey.ToEncryptedKeyString();
			}
		}

		public void TestUpdateDatabaseServerName()
		{
			SetupTestData();
			ServerVersionPK = PreviousBuild.PK;
			SetVersionNumber(PreviousBuild, "1.1.1885.10000");
			SetVersionNumber(NewBuild, "1.1.1990.10000");
			Factory.Save();
			string xmlData = GetXmlData(Organisation.PK);
			EDIVersionReport report = new EDIVersionReport(xmlData);
			Database.LD_HostServerName = ""; // Reset value
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals("Server Name", report.SQLServerName, Database.LD_HostServerName);
		}

		public void TestUpdateDatabaseInstanceName()
		{
			SetupTestData();
			ServerVersionPK = PreviousBuild.PK;
			SetVersionNumber(PreviousBuild, "1.1.1885.10000");
			SetVersionNumber(NewBuild, "1.1.1990.10000");
			Factory.Save();
			string xmlData = GetXmlData(Organisation.PK);
			EDIVersionReport report = new EDIVersionReport(xmlData);
			Database.LD_HostDBInstance = ""; // Reset value
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals("DB Instance Name is blank", "", Database.LD_HostDBInstance);
		}

		public void TestUpdateDatabaseInstanceNameWithSplitNameAndInstance()
		{
			SetupTestData();
			ServerVersionPK = PreviousBuild.PK;
			SetVersionNumber(PreviousBuild, "1.1.1885.10000");
			SetVersionNumber(NewBuild, "1.1.1990.10000");
			Factory.Save();
			string xmlData = GetXmlData(Organisation.PK, true);
			EDIVersionReport report = new EDIVersionReport(xmlData);
			Database.LD_HostDBInstance = ""; // Reset value
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals("DB Instance Name is ServerName", report.SQLServerInstanceName, Database.LD_HostDBInstance);
		}

		public void TestUpdateInternalPop3UserName()
		{
			SetupTestData();
			ServerVersionPK = PreviousBuild.PK;
			SetVersionNumber(PreviousBuild, "1.1.1885.10000");
			SetVersionNumber(NewBuild, "1.1.1990.10000");
			Factory.Save();
			var report = new EDIVersionReport(Organisation.PK, "ABC", "XYZ", "123", "DBS", "DBN", "1.1.1990.10000", "", new ZDateTime(2004, 12, 31, 13, 56, 0));
			report.InternalPOP3UserName = new ZString('A', Database.LD_InternalPop3UserNameInfo.MaxLength + 5);
			string xmlData = report.GenerateLegacyXmlForTest();
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals("LD_InternalPop3UserName truncated to max length", Database.LD_InternalPop3UserNameInfo.MaxLength, Database.LD_InternalPop3UserName.Length);
		}

		[TestDate(2013, 1, 19)]
		public void TestUpdateExeVersion()
		{
			SetupTestData();
			ServerVersionPK = PreviousBuild.PK;
			SetVersionNumber(PreviousBuild, "1.1.1885.10000");
			SetVersionNumber(NewBuild, "1.4.4766.0");
			SetVersionNumber(NextBuild, "1.5.1885.10000");
			Database.LD_HL_CurrentRunningVersion = PreviousBuild.PK;
			Database.LD_HL_CurrentSentVersion = PreviousBuild.PK;
			Factory.Save();
			var report = new EDIVersionReport(Organisation.PK, "ABC", "XYZ", "123", "DBS", "DBN", "1.4.4766.0", "", new ZDateTime(2013, 1, 18, 13, 56, 0));
			var xml = report.GenerateLegacyXmlForTest();
			VersionReportProcessorForTest.Process(xml);
			AssertEquals("LD_HL_CurrentRunningVersion updated", NewBuild.PK, Database.LD_HL_CurrentRunningVersion);
			AssertEquals("LD_HL_CurrentSentVersion updated", NewBuild.PK, Database.LD_HL_CurrentSentVersion);
			AssertEquals(report.CurrentDate, Database.LD_CurrentVersionFirstReportUtc);
			AssertEquals(report.CurrentDate, Database.LD_CurrentVersionLastReportUtc);
			Database.LD_HL_CurrentRunningVersion = PreviousBuild.PK;
			Database.LD_HL_CurrentSentVersion = NewBuild.PK;
			Database.LD_LastHeartbeat = ZDateTime.Empty;
			Factory.Save();
			VersionReportProcessorForTest.Process(xml);
			AssertEquals("LD_HL_CurrentRunningVersion updated", NewBuild.PK, Database.LD_HL_CurrentRunningVersion);
			AssertEquals("LD_HL_CurrentSentVersion not updated", NewBuild.PK, Database.LD_HL_CurrentSentVersion);
			AssertEquals(report.CurrentDate, Database.LD_CurrentVersionFirstReportUtc);
			AssertEquals(report.CurrentDate, Database.LD_CurrentVersionLastReportUtc);
			Database.LD_HL_CurrentRunningVersion = PreviousBuild.PK;
			Database.LD_HL_CurrentSentVersion = NextBuild.PK;
			Database.LD_LastHeartbeat = ZDateTime.Empty;
			Factory.Save();
			VersionReportProcessorForTest.Process(xml);
			AssertEquals("LD_HL_CurrentRunningVersion updated", NewBuild.PK, Database.LD_HL_CurrentRunningVersion);
			AssertEquals("LD_HL_CurrentSentVersion not updated", NextBuild.PK, Database.LD_HL_CurrentSentVersion);
			AssertEquals(report.CurrentDate, Database.LD_CurrentVersionFirstReportUtc);
			AssertEquals(report.CurrentDate, Database.LD_CurrentVersionLastReportUtc);
			TestDateAttribute.Date = new DateTime(2015, 1, 1);
			Database.LD_HL_CurrentRunningVersion = PreviousBuild.PK;
			Database.LD_LastHeartbeat = ZDateTime.Empty;
			Factory.Save();
			VersionReportProcessorForTest.Process(xml);
			AssertEquals("LD_HL_CurrentRunningVersion updated", NewBuild.PK, Database.LD_HL_CurrentRunningVersion);
			AssertEquals(report.CurrentDate, Database.LD_CurrentVersionFirstReportUtc);
			AssertEquals(report.CurrentDate, Database.LD_CurrentVersionLastReportUtc);
			TestDateAttribute.Date = new DateTime(2013, 7, 1);
			Database.LD_HL_CurrentRunningVersion = PreviousBuild.PK;
			Database.LD_LastHeartbeat = ZDateTime.Empty;
			Factory.Save();
			VersionReportProcessorForTest.Process(xml);
			AssertEquals("LD_HL_CurrentRunningVersion updated", NewBuild.PK, Database.LD_HL_CurrentRunningVersion);
			AssertEquals(report.CurrentDate, Database.LD_CurrentVersionFirstReportUtc);
			AssertEquals(report.CurrentDate, Database.LD_CurrentVersionLastReportUtc);
			Database.LD_LastHeartbeat = ZDateTime.Empty;
			Factory.Save();
			report = new EDIVersionReport(Organisation.PK, "ABC", "XYZ", "123", "DBS", "DBN", "1.4.4766.0", "", new ZDateTime(2013, 1, 18, 23, 56, 0));
			xml = report.GenerateLegacyXmlForTest();
			VersionReportProcessorForTest.Process(xml);
			AssertEquals(new ZDateTime(2013, 1, 18, 13, 56, 0), Database.LD_CurrentVersionFirstReportUtc);
			AssertEquals(new ZDateTime(2013, 1, 18, 23, 56, 0), Database.LD_CurrentVersionLastReportUtc);
			Database.LD_LastHeartbeat = ZDateTime.Empty;
			Factory.Save();
			report = new EDIVersionReport(Organisation.PK, "ABC", "XYZ", "123", "DBS", "DBN", "1.4.4766.0", "", new ZDateTime(2013, 1, 20, 11, 30, 0));
			xml = report.GenerateLegacyXmlForTest();
			VersionReportProcessorForTest.Process(xml);
			AssertEquals(new ZDateTime(2013, 1, 18, 13, 56, 0), Database.LD_CurrentVersionFirstReportUtc);
			AssertEquals(new ZDateTime(2013, 1, 20, 11, 30, 0), Database.LD_CurrentVersionLastReportUtc);
		}

		[TestDate(2024, 12, 1)]
		public void TestUpdateExeVersion_CargoWiseNext()
		{
			SetupTestData();
			ServerVersionPK = PreviousBuild.PK;
			SetVersionNumber(PreviousBuild, "24.10.9.254");
			SetVersionNumber(NewBuild, "24.11.1.4");
			NewBuild.HL_Product = ProductTypes.Codes.CargoWiseNext;
			Database.LD_HL_CurrentRunningVersion = PreviousBuild.PK;
			Database.LD_HL_CurrentSentVersion = PreviousBuild.PK;
			Factory.Save();
			var report = new EDIVersionReport(Organisation.PK, "ABC", "XYZ", "123", "DBS", "DBN", "24.11.1.4", "", new ZDateTime(2024, 12, 1, 19, 0, 0));
			var xml = report.GenerateLegacyXmlForTest();
			VersionReportProcessorForTest.Process(xml);
			AssertEquals("LD_HL_CurrentRunningVersion updated", NewBuild.PK, Database.LD_HL_CurrentRunningVersion);
			AssertEquals("LD_HL_CurrentSentVersion updated", NewBuild.PK, Database.LD_HL_CurrentSentVersion);
			AssertEquals(report.CurrentDate, Database.LD_CurrentVersionFirstReportUtc);
			AssertEquals(report.CurrentDate, Database.LD_CurrentVersionLastReportUtc);
		}

		[TestDate(2024, 12, 1)]
		public void TestUpdateExeVersion_CargoWise()
		{
			SetupTestData();
			ServerVersionPK = PreviousBuild.PK;
			SetVersionNumber(PreviousBuild, "24.10.9.254");
			SetVersionNumber(NewBuild, "24.11.1.4");
			NewBuild.HL_Product = ProductTypes.Codes.CargoWise;
			Database.LD_HL_CurrentRunningVersion = PreviousBuild.PK;
			Database.LD_HL_CurrentSentVersion = PreviousBuild.PK;
			Factory.Save();
			var report = new EDIVersionReport(Organisation.PK, "ABC", "XYZ", "123", "DBS", "DBN", "24.11.1.4", "", new ZDateTime(2024, 12, 1, 19, 0, 0));
			var xml = report.GenerateLegacyXmlForTest();
			VersionReportProcessorForTest.Process(xml);
			AssertEquals("LD_HL_CurrentRunningVersion updated", NewBuild.PK, Database.LD_HL_CurrentRunningVersion);
			AssertEquals("LD_HL_CurrentSentVersion updated", NewBuild.PK, Database.LD_HL_CurrentSentVersion);
			AssertEquals(report.CurrentDate, Database.LD_CurrentVersionFirstReportUtc);
			AssertEquals(report.CurrentDate, Database.LD_CurrentVersionLastReportUtc);
		}

		public void TestEmptyLicenceExpiryForcesExpiryUpdate()
		{
			SetupTestData();
			Database.LD_LicenceExpiry = ZDateTime.Empty;
			Database.LD_HostServerSID = ZGuid.NewZGuid();
			Database.LD_HostServerName = "DBS";
			Database.LD_HostDBName = "DBN";
			Factory.Save();
			var currentExpiryDate = ZDateTime.Now.AddDays(Licences.DefaultNumberOfDaysBeforeUpdateSystemKey + 2);
			EDIVersionReport report = new EDIVersionReport(Organisation.PK, "ABC", "XYZ", "123", "DBS", "DBN", "1.1.1990.10000", "", new ZDateTime(2004, 12, 31, 13, 56, 0));
			report.ServerSID = Database.LD_HostServerSID;
			ISystemRegistrationKey newKey = new SystemRegistrationKey(currentExpiryDate.ToDateTime(), Database.LD_HostServerSID.ToGuid(), "", "DBN", "PRD", "LCK");
			report.EncryptedSystemExpirationKey = newKey.ToEncryptedKeyString();
			report.DBServerSecurityMode = DatabaseSecurityModePairList.Codes.Locked;
			string xmlData = report.GenerateLegacyXmlForTest();
			Assert("The licence expiry should be empty", Database.LD_LicenceExpiry.IsEmpty);
			var sender = new LicenceKeyBuilder.Business.Test.SystemUpdatePacketSenderForTest();
			var processor = new CurrentVersionReportProcessorForTest(new TestServiceLogger());
			processor.SystemUpdatePacketSender = sender;
			processor.Process(xmlData);
			AssertEquals("The expiry should be set from report", currentExpiryDate.ToSmallDateTimeFloor(), Database.LD_LicenceExpiry.ToDateTime());
			AssertEquals(1, sender.SendCalls);
		}

		[TestDate(2016, 1, 28)]
		public void TestEmptyBillingTimeZoneInfoForcesExpiryUpdate()
		{
			var currentExpiryDate = ZDateTime.Now.AddDays(45);
			SetupTestData();
			Database.LD_LicenceExpiry = ZDateTime.Empty;
			Database.LD_HostServerSID = ZGuid.NewZGuid();
			Database.LD_HostServerName = "DBS";
			Database.LD_HostDBName = "DBN";
			Database.LD_LicenceExpiry = currentExpiryDate;
			Factory.Save();
			EDIVersionReport report = new EDIVersionReport(Organisation.PK, "ABC", "XYZ", "123", "DBS", "DBN", "1.1.1990.10000", "", new ZDateTime(2016, 1, 28, 17, 1, 0));
			report.ServerSID = Database.LD_HostServerSID;
			ISystemRegistrationKey newKey = new SystemRegistrationKey(currentExpiryDate.ToDateTime(), Database.LD_HostServerSID.ToGuid(), "", "DBN", "PRD", "LCK");
			report.EncryptedSystemExpirationKey = newKey.ToEncryptedKeyString();
			report.DBServerSecurityMode = DatabaseSecurityModePairList.Codes.Locked;
			string xmlData = report.GenerateLegacyXmlForTest();
			var sender = new LicenceKeyBuilder.Business.Test.SystemUpdatePacketSenderForTest();
			var processor = new CurrentVersionReportProcessorForTest(new TestServiceLogger());
			processor.SystemUpdatePacketSender = sender;
			processor.Process(xmlData);
			AssertEquals(1, sender.SendCalls);
		}

		public void TestUnknownCompanyCodeDiscrepancy()
		{
			SetupDummyInternalOrg();
			// Their version report service task could be logging in under an inactive company.
			// While in ediProd the company may have been moved to another database or enterprise.
			SetupTestData();
			Database.Factory.Save();
			var report = new EDIVersionReport(Organisation.PK, "ABC", "ZZZ", "123", "DBS", "DBN", "1.1.1990.10000", "", new ZDateTime(2004, 12, 31, 13, 56, 0));
			VersionReportProcessorForTest.Process(report.GenerateLegacyXmlForTest());
			AssertEquals("Nothing to do", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			// unknown ENT code
			Env.OutgoingMailManager.EmailsCreated.Clear();
			report = new EDIVersionReport(Organisation.PK, "XXX", "XYZ", "123", "DBS", "DBN", "1.1.1990.10000", "", new ZDateTime(2004, 12, 31, 13, 56, 0));
			VersionReportProcessorForTest.Process(report.GenerateLegacyXmlForTest());
			AssertEquals("Client Record Not Found Email Sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestLicenceUsage()
		{
			var data = new LicenceConsumptionLogSchema();
			data.Items.AddNew();
			string encryptedtext = LicenceUsageReportBuilder.GetCompressedEncryptedText(data);
			string versionXml = @"<?xml version=""1.0"" encoding=""utf-8""?><VersionReport><LicenceUsage>" + encryptedtext + @"</LicenceUsage></VersionReport>";
			var processor = new CurrentVersionReportProcessorForTest(null);
			processor.ShouldHandleLicenceUsage = false;
			processor.Process(versionXml);
			AssertEquals(encryptedtext, processor.LastLicenceUsage);
			AssertEquals(1, processor.CallsToHandleLicenceUsage);
			ReportProcessorHelper.ClearReportsFromTesting("LicenceUsage");
		}

		[TestDate(2016, 1, 13, 9, 0, 0)]
		public void TestAutoLogDisabling()
		{
			var tempValue = new EnableAddEditAndDeleteLogsItemCollection
			{
				new EnableAddEditAndDeleteLogsItem
				{
					Table = LicenceDatabaseSchema.Constants.TableName,
					EnableADDLogs = false,
					EnableEDTLogs = false,
					EnableDELLogs = false
				}
			};

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValue))
			{
				SetupTestData();
				var encryptedRegistrationKey = SetupRegisteredDatabaseAndReturnKey(Database);
				Factory.Save();
				TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
				var builder = BuildXmlInFormat201504(Database, encryptedRegistrationKey);
				var reportXml = builder.ToString();
				var logger = new TestServiceLogger();
				var processor = new CurrentVersionReportProcessorForTest(logger);
				processor.Process(reportXml);
				TestDateAttribute.Date = TestDateAttribute.Date.AddHours(24);
				logger = new TestServiceLogger();
				processor = new CurrentVersionReportProcessorForTest(logger);
				processor.Process(reportXml);
				var factory2 = new BusinessObjectFactory();
				var dbReloaded = factory2.Load<LicenceDatabase>(Database.PK);
				AssertEquals("last heartbeat updated", TestDateAttribute.Date, dbReloaded.LD_LastHeartbeat);
				var lastLog = dbReloaded.Logs.MostRecentLog;
				AssertNull(lastLog);
			}
		}

		public void TestUpdateLicenceExpiryWithInvalidSmallDateTime()
		{
			SetupTestData();
			var logger = new TestServiceLogger();
			var processor = new CurrentVersionReportProcessorForTest(logger);
			var expected = Database.LD_LicenceExpiry;
			processor.UpdateLicenceExpiry_Exposed(Database, new DateTime(1869, 11, 16, 9, 46, 0));
			AssertEquals(expected, Database.LD_LicenceExpiry);
		}

		public void TestOutboundEAdaptorUrl()
		{
			SetupTestData();
			string xmlData = GetXmlData(Organisation.PK);
			EDIVersionReport report = new EDIVersionReport(xmlData);
			Database.LD_OutboundEAdaptorUrl = ZString.Empty;
			VersionReportProcessorForTest.Process(xmlData);
			AssertNotEquals(ZString.Empty, report.OutboundEAdaptorUrl);
			AssertEquals("Outbound eAdaptor Url Set", report.OutboundEAdaptorUrl, Database.LD_OutboundEAdaptorUrl);
		}

		public void TestOutboundEAdaptorUrl_MaxLength()
		{
			SetupTestData();
			var longUrl = $"http://www.cw1.com/{new string('0', LicenceDatabase.Schema.LD_OutboundEAdaptorUrlMaxLength + 100)}";
			string xmlData = GetXmlData(Organisation.PK, false, longUrl);
			var report = new EDIVersionReport(xmlData);
			AssertEquals(longUrl, report.OutboundEAdaptorUrl);
			Database.LD_OutboundEAdaptorUrl = ZString.Empty;
			VersionReportProcessorForTest.Process(xmlData);
			AssertEquals(longUrl.Substring(0, LicenceDatabase.Schema.LD_OutboundEAdaptorUrlMaxLength), Database.LD_OutboundEAdaptorUrl);
		}

		[TestDate(2015, 7, 6, 16, 7, 0)]
		public void TestUpdateReportedLicenceDatabaseNames()
		{
			SetupTestData();
			var encryptedRegistrationKey = SetupRegisteredDatabaseAndReturnKey(Database);
			SetVersionNumber(NewBuild, "1.1.1990.10000");
			Database.LD_PublicEmailAddressForUpdate = "test@test.com";
			Database.LD_InternalSmtpEmailAddress = "blah@test.com";
			Database.LD_NoOfActivePrintQueues = 7;
			Factory.Save();
			var reportTimeUtc = new ZDateTime(2015, 7, 5, 11, 30, 0, DateTimeKind.Utc);
			string reportXml = FormattableString.Invariant($@"<?xml version=""1.0"" encoding=""utf-8""?>
<VersionReport>
  <DatabaseNumber>98765</DatabaseNumber>
  <CurrentVersion>1.1.1990.10000</CurrentVersion>
  <CurrentDate>{reportTimeUtc.ToString("o")}</CurrentDate>
  <RegistrationData>{encryptedRegistrationKey}</RegistrationData>
  <DBName>Hello</DBName>
  <SQLServerInstanceName>There</SQLServerInstanceName>
  <DBServerName>ObiWanKenobi</DBServerName>
</VersionReport>");
			var logger = new TestServiceLogger();
			var processor = new CurrentVersionReportProcessorForTest(logger);
			processor.Process(reportXml);
			var factory2 = new BusinessObjectFactory();
			var dbReloaded = factory2.Load<LicenceDatabase>(Database.PK);
			AssertEquals("Hello", dbReloaded.LD_ReportedHostDBName);
			AssertEquals("There", dbReloaded.LD_ReportedHostDBInstance);
			AssertEquals("ObiWanKenobi", dbReloaded.LD_ReportedHostServerName);
		}

		public void TestUpdateReportedLicenceDatabaseUpgradeInfo()
		{
			SetupTestData();
			var xmlData = GetXmlData(Organisation.PK);
			var report = new EDIVersionReport(xmlData);
			Database.LD_ScheduleStateUPG = ZString.Empty;
			Database.LD_ScheduleStateMUG = ZString.Empty;
			VersionReportProcessorForTest.Process(xmlData);

			AssertNotEquals(ZString.Empty, report.ScheduleStateUPG);
			AssertNotEquals(ZString.Empty, report.ScheduleStateMUG);
			AssertEquals(new ZDateTime(2023, 6, 8, 15, 0, 0), report.NextRunTimeUtcUPG);
			AssertEquals(new ZDateTime(2023, 6, 8, 16, 0, 0), report.NextRunTimeUtcMUG);
			AssertEquals("Schedule State UPG Set", report.ScheduleStateUPG, Database.LD_ScheduleStateUPG);
			AssertEquals("Schedule State MUG Set", report.ScheduleStateMUG, Database.LD_ScheduleStateMUG);
		}

		public void TestTokenAuthenticationEnabledToTrue()
		{
			SetupTestData();
			var xmlData = GetXmlData(Organisation.PK, false,null, true);
			var report = new EDIVersionReport(xmlData);
			VersionReportProcessorForTest.Process(xmlData);

			AssertEquals(true, report.TokenAuthenticationEnabled);
			AssertEquals(true, report.TokenAuthenticationEnabledSpecified);
		}

		public void TestTokenAuthenticationEnabledToFalse()
		{
			SetupTestData();
			var xmlData = GetXmlData(Organisation.PK, false, null, false);
			var report = new EDIVersionReport(xmlData);
			VersionReportProcessorForTest.Process(xmlData);

			AssertEquals(false, report.TokenAuthenticationEnabled);
			AssertEquals(true, report.TokenAuthenticationEnabledSpecified);
		}

		public void TestEnterpriseTokenAuthenticationEnabledToTrue()
		{
			SetupTestData();
			var encryptedRegistrationKey = SetupRegisteredDatabaseAndReturnKey(Database);
			SetVersionNumber(NewBuild, "1.1.1990.10000");
			Database.LD_PublicEmailAddressForUpdate = "test@test.com";
			Database.LD_InternalSmtpEmailAddress = "blah@test.com";
			Database.LD_NoOfActivePrintQueues = 7;
			Database.LD_TokenAuthenticationEnabled = false;
			Factory.Save();

			var reportTimeUtc = new ZDateTime(2015, 7, 5, 11, 30, 0, DateTimeKind.Utc);
			string xmlData = FormattableString.Invariant($@"<?xml version=""1.0"" encoding=""utf-8""?>
<VersionReport>
  <DatabaseNumber>98765</DatabaseNumber>
  <CurrentVersion>1.1.1990.10000</CurrentVersion>
  <CurrentDate>{reportTimeUtc.ToString("o")}</CurrentDate>
  <RegistrationData>{encryptedRegistrationKey}</RegistrationData>
  <DBName>Hello</DBName>
  <SQLServerInstanceName>There</SQLServerInstanceName>
  <DBServerName>ObiWanKenobi</DBServerName>
  <TokenAuthenticationEnabled>true</TokenAuthenticationEnabled>
</VersionReport>");
			VersionReportProcessorForTest.Process(xmlData);
			Assert(Database.LD_TokenAuthenticationEnabled);
			Assert(Enterprise.LE_TokenAuthenticationEnabled);
		}

		public void TestEnterpriseTokenAuthenticationEnabledToFalseWhenAllDatabaseNotEnable()
		{
			SetupTestData();
			var encryptedRegistrationKey = SetupRegisteredDatabaseAndReturnKey(Database);
			SetVersionNumber(NewBuild, "1.1.1990.10000");
			Database.LD_PublicEmailAddressForUpdate = "test@test.com";
			Database.LD_InternalSmtpEmailAddress = "blah@test.com";
			Database.LD_NoOfActivePrintQueues = 7;
			Database.LD_TokenAuthenticationEnabled = true;
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase.LD_TokenAuthenticationEnabled = false;
			licenceDatabase.LD_DatabaseNumber = 101;
			licenceDatabase.LD_LE = Enterprise.PK;
			var licenceDatabase2 = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase2.LD_TokenAuthenticationEnabled = false;
			licenceDatabase2.LD_DatabaseNumber = 121;
			licenceDatabase2.LD_LE = Enterprise.PK;
			Factory.Save();
			
			var reportTimeUtc = new ZDateTime(2015, 7, 5, 11, 30, 0, DateTimeKind.Utc);
			string xmlData = FormattableString.Invariant($@"<?xml version=""1.0"" encoding=""utf-8""?>
<VersionReport>
  <DatabaseNumber>98765</DatabaseNumber>
  <CurrentVersion>1.1.1990.10000</CurrentVersion>
  <CurrentDate>{reportTimeUtc.ToString("o")}</CurrentDate>
  <RegistrationData>{encryptedRegistrationKey}</RegistrationData>
  <DBName>Hello</DBName>
  <SQLServerInstanceName>There</SQLServerInstanceName>
  <DBServerName>ObiWanKenobi</DBServerName>
  <TokenAuthenticationEnabled>false</TokenAuthenticationEnabled>
</VersionReport>");
			VersionReportProcessorForTest.Process(xmlData);
			Assert(!Enterprise.LE_TokenAuthenticationEnabled);
		}

		public void TestEnterpriseTokenAuthenticationEnabledStillIsTrueWhenNotAllDatabaseAreEnabled()
		{
			SetupTestData();
			var encryptedRegistrationKey = SetupRegisteredDatabaseAndReturnKey(Database);
			SetVersionNumber(NewBuild, "1.1.1990.10000");
			Database.LD_PublicEmailAddressForUpdate = "test@test.com";
			Database.LD_InternalSmtpEmailAddress = "blah@test.com";
			Database.LD_NoOfActivePrintQueues = 7;
			Database.LD_TokenAuthenticationEnabled = true;
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase.LD_TokenAuthenticationEnabled = true;
			licenceDatabase.LD_DatabaseNumber = 101;
			licenceDatabase.LD_LE = Enterprise.PK;
			var licenceDatabase2 = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase2.LD_TokenAuthenticationEnabled = true;
			licenceDatabase2.LD_DatabaseNumber = 121;
			licenceDatabase2.LD_LE = Enterprise.PK;
			Factory.Save();

			var reportTimeUtc = new ZDateTime(2015, 7, 5, 11, 30, 0, DateTimeKind.Utc);
			string xmlData = FormattableString.Invariant($@"<?xml version=""1.0"" encoding=""utf-8""?>
<VersionReport>
  <DatabaseNumber>98765</DatabaseNumber>
  <CurrentVersion>1.1.1990.10000</CurrentVersion>
  <CurrentDate>{reportTimeUtc.ToString("o")}</CurrentDate>
  <RegistrationData>{encryptedRegistrationKey}</RegistrationData>
  <DBName>Hello</DBName>
  <SQLServerInstanceName>There</SQLServerInstanceName>
  <DBServerName>ObiWanKenobi</DBServerName>
  <TokenAuthenticationEnabled>false</TokenAuthenticationEnabled>
</VersionReport>");
			VersionReportProcessorForTest.Process(xmlData);
			Assert(!Database.LD_TokenAuthenticationEnabled);
			Assert(Enterprise.LE_TokenAuthenticationEnabled);
		}

		#region Implementation

		protected override ZGuid GetServerVersionPK()
		{
			return Header.Database.LD_HL_CurrentRunningVersion;
		}

		protected override void SetServerVersionPK(ZGuid versionPK)
		{
			Header.Database.LD_HL_CurrentRunningVersion = versionPK;
		}

		protected override VersionReportProcessor GetVersionReportProcessorForTest()
		{
			return new CurrentVersionReportProcessorForTest(new TestServiceLogger());
		}

		protected override void SetupTestData()
		{
			SetupTestData(true);
		}

		void SetupTestData(bool setSqlServerVersionDetails)
		{
			base.SetupTestData();
			if (setSqlServerVersionDetails)
			{
				DbConnection connection = Db.Connection;
				Database.LD_SQLEdition = SqlServerVersionDetailsConverter.ToCode(connection.ServerEdition);
				Database.LD_SQLVersion = connection.ServerVersionNumber.SqlServerGeneration;
				Database.LD_SQLVerString = connection.ServerFullVersionText;
				Factory.Save();
			}
		}

		void SetupDummyInternalOrg()
		{
			EDIOrgHeader internalOrg = Factory.NewWithValidTestData<EDIOrgHeader>();
			internalOrg.OH_FullName = "Dummy Internal Org";
			internalOrg.OH_Code = "DUMORG";
			internalOrg.OH_RL_NKClosestPort = "AUBNE";
			internalOrg.CreateAndLoadLicenceForOrg();
			internalOrg.LicEnterprise.LE_EnterpriseCode = "DIO";
			LicenceCompany internalCompany = internalOrg.LicCompany;
			internalCompany.LC_RX_NKCurrency = "AUD";
			LicenceDatabase internalDb = internalCompany.LicDatabases.AddNew();
			internalDb.LD_ServerCode = "PRD";
			LicenceHeader internalHeader = internalCompany.GetHeader(internalDb);
			Factory.Save();
			InternalIncidentLicenceSettings licenceSettings = new InternalIncidentLicenceSettings();
			LicenceEnterpriseKey internalEnterprise = licenceSettings.LicenceEnterpriseKeys.AddNew();
			internalEnterprise.LE_PK = internalOrg.LicEnterprise.PK;
			licenceSettings.EdiProd_LicencePK = internalHeader.PK;
			licenceSettings.UAT_ALP_LicencePK = internalHeader.PK;
			licenceSettings.UAT_DPR_LicencePK = internalHeader.PK;
			licenceSettings.UAT_STD_LicencePK = internalHeader.PK;
			licenceSettings.UAT_GPC_LicencePK = internalHeader.PK;
			licenceSettings.UAT_GPR_LicencePK = internalHeader.PK;
			EDIDataRegistry.Instance.InternalIncidentLicenceSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, licenceSettings);
		}

		protected override void TearDown()
		{
			ReportProcessorHelper.ClearReportsFromTesting("CurrentVersionReport");
			base.TearDown();
		}

		#region CurrentVersionReportProcessor Subclasses For Test
		class CurrentVersionReportProcessorForTest : CurrentVersionReportProcessor, IVersionReportProcessorForTest
		{
			public CurrentVersionReportProcessorForTest(ILogger serviceLogger) : base(serviceLogger, false)
			{
			}

			public CurrentVersionReportProcessorForTest(ILogger serviceLogger, bool receivedViaEhub = false) : base(serviceLogger, receivedViaEhub)
			{
			}

			protected override LicenceDatabase GetLicenceDatabase(EDIVersionReport report)
			{
				var db = base.GetLicenceDatabase(report);
				if (db != null)
				{
					db.SystemUpdatePacketSender = SystemUpdatePacketSender;
				}

				return db;
			}

			internal ISystemUpdatePacketSender SystemUpdatePacketSender;
			public bool ProcessVersionReportWasCalled()
			{
				return ProcessVersionReportCalled;
			}

			public string ReasonForNotUpdating;
			public bool ShouldUpdateExpiryDateOnClient_Exposed(LicenceDatabase database, EDIVersionReport report)
			{
				return base.ShouldUpdateExpiryDateOnClient(database, report, SystemRegistrationKey.NewFromEncryptedXmlKey(report.EncryptedSystemExpirationKey), out ReasonForNotUpdating);
			}

			public void ProcessVersionReport_Exposed(EDIVersionReport report)
			{
				base.ProcessVersionReport(report);
			}

			public void UpdateLicenceExpiry_Exposed(LicenceDatabase database, DateTime expiryDate)
			{
				UpdateLicenceExpiry(database, expiryDate);
			}

			protected bool ProcessVersionReportCalled;
			protected override void ProcessVersionReport(EDIVersionReport report)
			{
				ProcessVersionReportCalled = true;
				base.ProcessVersionReport(report);
			}

			protected override VersionNumber FirstVersionToEnforceDbSecurity
			{
				get
				{
					return new VersionNumber(0, 0, 0, 0);
				}
			}

			public bool ShouldHandleLicenceUsage = true;
			public string LastLicenceUsage;
			public int CallsToHandleLicenceUsage;
			protected override void HandleLicenceUsage(string licenceUsage)
			{
				++CallsToHandleLicenceUsage;
				LastLicenceUsage = licenceUsage;
				if (ShouldHandleLicenceUsage)
				{
					base.HandleLicenceUsage(licenceUsage);
				}
			}
		}
		#endregion
		#endregion
	}
}
