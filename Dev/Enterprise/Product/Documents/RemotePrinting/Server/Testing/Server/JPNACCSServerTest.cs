using System;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.eHub.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.RemotePrinting.Server.RPSCore;
using Enterprise.xTMessaging.Business;
using Enterprise.xTMessaging.Shared.Test;
using Enterprise.ZArchitecture.Core;
using static Enterprise.xTMessaging.Shared.Test.TestUtils;

namespace Enterprise.RemotePrinting.Server.Testing
{
	sealed class JPNACCSServerForTest : JPNACCSServer
	{
		public JPNACCSServerForTest(DbConnection testConnection)
		{
			this.testConnection = testConnection;
		}

		protected override DbConnection NewConnection() => testConnection ??= Db.NewExtraConnectionToMainDb();
		DbConnection testConnection;
	}

	[UseSnapshotProtection]
	sealed class JPNACCSServerTest : TestCaseWithFactory
	{
		public void TestGetSettingByMachineName()
		{
			using var connection = Db.NewExtraConnectionToMainDb();
			var factory = new BusinessObjectFactory(connection);
			InsertRefSysConfig(factory);
			InsertRefSysConfig(factory, "NACCSMailP", "NACCS@MAIL.PROD.NACCS6");
			InsertRefSysConfig(factory, "NACCSMailT", "NACCS@MAIL.TEST.NACCS6");

			var company = GlbCompany.CurrentCompany;
			var companyPk = company.PK;
			var validPasswordPk = Guid.Parse("3D696537-8368-4365-8C87-CD13CF578697");
			var invalidPasswordPk = Guid.NewGuid();

			var insertSetting = $@"
-- Valid Password
INSERT INTO GlbExternalPassword
(GP_PK, GP_Certificate, GP_CertificateAuthority, GP_CertificatePassPhrase, GP_CertificateSerialNumber, GP_CurrentPassword, GP_ExpiryDate, GP_GB, GP_GC, GP_GG, GP_GS, GP_IssueDate, GP_MailBoxID, GP_Name, GP_NextPassword, GP_PasswordStatus, GP_PasswordType, GP_StatusReason, GP_SystemCreateTimeUtc, GP_SystemCreateUser, GP_SystemLastEditTimeUtc, GP_SystemLastEditUser, GP_UserID)
VALUES
('{validPasswordPk}', Null, '', '', '', 'G1a2+iKXOBeUvMkAVlU4kA==', Null, Null, '{companyPk}', Null, Null, Null, 'PILOTUSER', '', '', '', 'NMC', '', GETDATE(), 'E', GETDATE(), 'E', '')

INSERT INTO GenAddOnColumn
(XA_PK, XA_Data, XA_Name, XA_ParentID, XA_ParentTableCode, XA_SystemCreateTimeUtc, XA_SystemCreateUser, XA_SystemLastEditTimeUtc, XA_SystemLastEditUser, XA_Type)
VALUES
(NEWID(), 'Y', 'JP_ShouldReceive', '{validPasswordPk}', 'GP', GETDATE(), 'E', GETDATE(), 'E', 'BOO')

-- Invalid Password
INSERT INTO GlbExternalPassword
(GP_PK, GP_Certificate, GP_CertificateAuthority, GP_CertificatePassPhrase, GP_CertificateSerialNumber, GP_CurrentPassword, GP_ExpiryDate, GP_GB, GP_GC, GP_GG, GP_GS, GP_IssueDate, GP_MailBoxID, GP_Name, GP_NextPassword, GP_PasswordStatus, GP_PasswordType, GP_StatusReason, GP_SystemCreateTimeUtc, GP_SystemCreateUser, GP_SystemLastEditTimeUtc, GP_SystemLastEditUser, GP_UserID)
VALUES
('{invalidPasswordPk}', Null, '', '', '', 'G1a2+iKXOBeUvMkAVlU4kA==', Null, Null, '{companyPk}', Null, Null, Null, 'PILOTUSERINVALID', '', '', '', 'NMC', '', GETDATE(), 'E', GETDATE(), 'E', '')

INSERT INTO GenAddOnColumn
(XA_PK, XA_Data, XA_Name, XA_ParentID, XA_ParentTableCode, XA_SystemCreateTimeUtc, XA_SystemCreateUser, XA_SystemLastEditTimeUtc, XA_SystemLastEditUser, XA_Type)
VALUES
(NEWID(), 'N', 'JP_ShouldReceive', '{invalidPasswordPk}', 'GP', GETDATE(), 'E', GETDATE(), 'E', 'BOO')

-- Registry Item JPMailboxAndRemoteWebPrintClientCredentials
DELETE FROM dbo.StmData WHERE SD_Name = 'JPMailboxAndRemoteWebPrintClientCredentials';
INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Type, SD_BinaryValue) VALUES(NEWID(), 'JPMailboxAndRemoteWebPrintClientCredentials', 'BIN', {NormalSettingBinaryString});";
			connection.ExecuteNonQuery(insertSetting);

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "HYE";
			registrationKey.ServerCodeForTest = "CMR";
			registrationKey.PasswordForTest = "TST123";

			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtTest.Code))
			{
				var server = new JPNACCSServerForTest(connection);
				var setting = server.GetSettingByMachineName("TST2024");

				CombineAssertions(() =>
				{
					AssertEquals("MailBoxInfos.CompanyCode", company.GC_Code, setting.MailBoxInfos.Single().CompanyCode);
					AssertEquals("MailBoxInfos.MailBox", "PILOTUSER@MAIL.TEST.NACCS6", setting.MailBoxInfos.Single().MailBox);
					AssertEquals("MailBoxInfos.MailBoxPassword", Convert.ToBase64String(Encoding.UTF8.GetBytes("123456")), setting.MailBoxInfos.Single().MailBoxPassword);
					AssertEquals("NACCSMailbox", "NACCS@MAIL.TEST.NACCS6", setting.NACCSMailbox);
					AssertEquals("DomainName", "WEBPRINT.WISETECHGLOBAL.COM", setting.DomainName);
					AssertEquals("xTServerAddress", TestUtils.XTServerAddressValue, setting.xTServerAddress);
					AssertEquals("xTServerCertificate", TestUtils.XTServerCertificateValue, setting.xTServerCertificate);
					AssertEquals("xTApplicationNode", "HYECMR_JPC", setting.xTApplicationNode);
					AssertEquals("xTPassword", Convert.ToBase64String(Encoding.UTF8.GetBytes(SHA512Encryptor.Encrypt("TST2024" + setting.DomainName))), setting.xTPassword);
					AssertEquals("ReceivingInterval", 3, setting.ReceivingInterval);
					AssertEquals("SendingInterval", 12, setting.SendingInterval);
					AssertEquals("DownTimeStart", new DateTime(2024, 9, 23, 18, 5, 0), setting.DownTimeStart);
					AssertEquals("DownTimeEnd", new DateTime(2024, 9, 24, 18, 5, 0), setting.DownTimeEnd);
					AssertEquals("XTIdleConnectionKeepAliveInSecondsValue", 60d, setting.XTIdleConnectionKeepAliveInSecondsValue);
					AssertEquals("XTIdleConnectionRetryPauseInSecondsValue", 15d, setting.XTIdleConnectionRetryPauseInSecondsValue);
					AssertEquals("InterchangeCountPerBatchOnReceivingValue", 100, setting.InterchangeCountPerBatchOnReceivingValue);
					Assert("Verbose", setting.Verbose);
				});

				var reg = ObjectFactory.Get<IProductRegistration>();
				reg.ResetKeyToDefault();
				reg.KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
				setting = server.GetSettingByMachineName("TST2024");

				CombineAssertions(() =>
				{
					AssertEquals("MailBoxInfos.MailBox", "PILOTUSER@MAIL.PROD.NACCS6", setting.MailBoxInfos.Single().MailBox);
					AssertEquals("NACCSMailbox", "NACCS@MAIL.PROD.NACCS6", setting.NACCSMailbox);
				});
			}
		}

		public void TestGetSettingByMachineNameWithXPathInject()
		{
			using var connection = Db.NewExtraConnectionToMainDb();
			var insertSetting = $@"
DELETE FROM dbo.StmData WHERE SD_Name = 'JPMailboxAndRemoteWebPrintClientCredentials';
INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Type, SD_BinaryValue) VALUES(NEWID(), 'JPMailboxAndRemoteWebPrintClientCredentials', 'BIN', {NormalSettingBinaryString})";
			connection.ExecuteNonQuery(insertSetting);

			var server = new JPNACCSServerForTest(connection);
			var testResult = server.GetSettingByMachineName("XXXName'+or+'1'='1'");
			AssertSame("IsEmpty", JPNACCSClientSetting.Empty, testResult);
		}

		public void TestEmptyBinaryValue()
		{
			using var connection = Db.NewExtraConnectionToMainDb();
			var factory = new BusinessObjectFactory(connection);
			InsertRefSysConfig(factory);

			var insertSetting = @"
DELETE FROM dbo.StmData WHERE SD_Name = 'JPMailboxAndRemoteWebPrintClientCredentials';
INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Type, SD_BinaryValue) VALUES(NEWID(), 'JPMailboxAndRemoteWebPrintClientCredentials', 'BIN', NULL)";
			connection.ExecuteNonQuery(insertSetting);

			AssertNoExceptionThrown("Should execute without exception thrown.", () => new JPNACCSServerForTest(connection).GetSettingByMachineName("Invalid Name"));
		}

		#region Common long binary string
		readonly string NormalSettingBinaryString = "0x3C003F0078006D006C002000760065007200730069006F006E003D00220031002E0030002200200065006E0063006F00640069006E0067003D0022007500740066002D003100360022003F003E003C004D00610069006C0062006F00780041006E006400520065006D006F00740065005700650062005000720069006E00740043006C00690065006E007400430072006500640065006E007400690061006C0073003E003C004C006F00630061006C0043006F006D007000750074006500720041006C006900610073003E0054005300540032003000320034003C002F004C006F00630061006C0043006F006D007000750074006500720041006C006900610073003E003C0044006F006D00610069006E004E0061006D0065003E005700450042005000520049004E0054002E005700490053004500540045004300480047004C004F00420041004C002E0043004F004D003C002F0044006F006D00610069006E004E0061006D0065003E003C0052006500630065006900760069006E00670049006E00740065007200760061006C003E0033003C002F0052006500630065006900760069006E00670049006E00740065007200760061006C003E003C00530065006E00640069006E00670049006E00740065007200760061006C003E00310032003C002F00530065006E00640069006E00670049006E00740065007200760061006C003E003C005300740061007400750073003E0055004E0047003C002F005300740061007400750073003E003C0056006500720062006F00730065003E0059003C002F0056006500720062006F00730065003E003C004600610069006C007500720065004E006F00740069006600690063006100740069006F006E00470072006F007500700020002F003E003C0044006F0077006E00540069006D006500530074006100720074003E003200300032003400300039003200330031003800300035003C002F0044006F0077006E00540069006D006500530074006100720074003E003C0044006F0077006E00540069006D00650045006E0064003E003200300032003400300039003200340031003800300035003C002F0044006F0077006E00540069006D00650045006E0064003E003C002F004D00610069006C0062006F00780041006E006400520065006D006F00740065005700650062005000720069006E00740043006C00690065006E007400430072006500640065006E007400690061006C0073003E00";
		#endregion
	}
}
