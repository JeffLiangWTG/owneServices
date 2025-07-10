using System;
using System.Data;
using System.Text;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.Integration.Licensing;
using Enterprise.RemotePrinting.Server.RPSCore;
using Enterprise.xTMessaging.Business;
using Enterprise.xTMessaging.Shared.Test;
using NUnit.Framework;
using static Enterprise.xTMessaging.Shared.Test.TestUtils;

namespace Enterprise.RemotePrinting.Server.Testing
{
	sealed class CLSMSServerForTest : CLSMSServer
	{
		public CLSMSServerForTest(DbConnection testConnection)
		{
			this.testConnection = testConnection;
		}

		protected override DbConnection NewConnection() => testConnection ??= Db.NewExtraConnectionToMainDb();
		DbConnection testConnection;
	}

	[UseSnapshotProtection]
	sealed class CLSMSServerTest : TestCase
	{
		public void TestGetSettingByMachineName()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(connection);
				InsertRefSysConfig(factory);

				try
				{
					connection.BeginTransaction();

					var insertSetting = $@"
DELETE FROM dbo.StmData WHERE SD_Name = 'CLSMSMessageSending';
DELETE FROM dbo.StmData WHERE SD_Name = 'ServerAddress';
INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Type, SD_BinaryValue) VALUES(NEWID(), 'CLSMSMessageSending', 'BIN', {NormalSettingBinaryString});
INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Type, SD_BinaryValue) VALUES(NEWID(), 'ServerAddress', 'STR', 0x65006800750062002D00610075007300790064002E0063006100720067006F0077006900730065002E006E0065007400);
";
					connection.ExecuteNonQuery(insertSetting);

					var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
					registrationKey.EnterpriseCodeForTest = "CUS";
					registrationKey.ServerCodeForTest = "TST";
					registrationKey.PasswordForTest = "xyz123";

					using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtTest.Code))
					{
						var server = new CLSMSServerForTest(connection);
						var testResult = server.GetSettingByMachineName("MACHINE");

						CombineAssertions(() =>
						{
							AssertEquals("Registration key", "CUSTST", testResult.RegistrationKey);
							AssertEquals("Server address", TestUtils.XTServerAddressValue, testResult.ServerAddress);
							AssertEquals("Server certificate", TestUtils.XTServerCertificateValue, testResult.ServerCertificate);
							AssertEquals("Application node name", "AN", testResult.ApplicationNodeName);
							AssertEquals("Application node password", Convert.ToBase64String(Encoding.UTF8.GetBytes("1234")), testResult.ApplicationNodePassword);
							AssertEquals("Running Interval", 60, testResult.RunningIntervalInSeconds);
							AssertEquals("Send folder", @"C:\FOLDER1", testResult.SendFolder);
							AssertEquals("Unknown folder", @"C:\FOLDER2", testResult.UnknownFolder);
							AssertEquals("Invalid folder", @"C:\FOLDER3", testResult.InvalidFolder);
							AssertEquals("Rejected folder", @"C:\FOLDER5", testResult.RejectedFolder);
							AssertEquals("Receive folder", @"C:\FOLDER6", testResult.ReceiveFolder);
							AssertEquals("Accepted folder", @"C:\FOLDER7", testResult.AcceptedFolder);
						});
					}
				}
				finally
				{
					if (connection.State == ConnectionState.Open && connection.IsInTransaction)
					{
						connection.RollbackTransaction();
					}
				}
			}
		}

		public void TestGetSettingByMachineNameWithXPathInject()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				try
				{
					connection.BeginTransaction();

					var insertSetting = $@"
DELETE FROM dbo.StmData WHERE SD_Name = 'CLSMSMessageSending';
INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Type, SD_BinaryValue) VALUES(NEWID(), 'CLSMSMessageSending', 'BIN', {NormalSettingBinaryString});";
					connection.ExecuteNonQuery(insertSetting);

					using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtTest.Code))
					{
						var server = new CLSMSServerForTest(connection);
						var testResult = server.GetSettingByMachineName("XXXName'+or+'1'='1'");
						AssertSame("IsEmpty", CLSMSClientSetting.Empty, testResult);
					}
				}
				finally
				{
					if (connection.State == ConnectionState.Open && connection.IsInTransaction)
					{
						connection.RollbackTransaction();
					}
				}
			}
		}

		public void TestEmptyBinaryValue()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(connection);
				InsertRefSysConfig(factory);

				try
				{
					connection.BeginTransaction();

					var insertSetting = @"
DELETE FROM dbo.StmData WHERE SD_Name = 'CLSMSMessageSending';
DELETE FROM dbo.StmData WHERE SD_Name = 'ServerAddress';
INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Type, SD_BinaryValue) VALUES(NEWID(), 'CLSMSMessageSending', 'BIN', NULL);
INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Type, SD_BinaryValue) VALUES(NEWID(), 'ServerAddress', 'STR', 0x65006800750062002D00610075007300790064002E0063006100720067006F0077006900730065002E006E0065007400)";
					connection.ExecuteNonQuery(insertSetting);

					AssertNoExceptionThrown("Should execute without exception thrown.", () => new CLSMSServerForTest(connection).GetSettingByMachineName("MACHINE 1"));
				}
				finally
				{
					if (connection.State == ConnectionState.Open && connection.IsInTransaction)
					{
						connection.RollbackTransaction();
					}
				}
			}
		}

		#region Common long binary string
		readonly string NormalSettingBinaryString = "0x3C003F0078006D006C002000760065007200730069006F006E003D00220031002E0030002200200065006E0063006F00640069006E0067003D0022007500740066002D003100360022003F003E003C0043004C0053004D0053004D00650073007300610067006500530065006E00640069006E0067003E003C004D0061006300680069006E0065004E0061006D0065003E004D0041004300480049004E0045003C002F004D0061006300680069006E0065004E0061006D0065003E003C004100700070006C00690063006100740069006F006E004E006F00640065004E0061006D0065003E0041004E003C002F004100700070006C00690063006100740069006F006E004E006F00640065004E0061006D0065003E003C004100700070006C00690063006100740069006F006E004E006F0064006500500061007300730077006F00720064003E0031003200330034003C002F004100700070006C00690063006100740069006F006E004E006F0064006500500061007300730077006F00720064003E003C00520075006E006E0069006E00670049006E00740065007200760061006C0049006E005300650063006F006E00640073003E00360030003C002F00520075006E006E0069006E00670049006E00740065007200760061006C0049006E005300650063006F006E00640073003E003C00530065006E00640046006F006C006400650072003E0043003A005C0046004F004C0044004500520031003C002F00530065006E00640046006F006C006400650072003E003C0055006E006B006E006F0077006E0046006F006C006400650072003E0043003A005C0046004F004C0044004500520032003C002F0055006E006B006E006F0077006E0046006F006C006400650072003E003C0049006E00760061006C006900640046006F006C006400650072003E0043003A005C0046004F004C0044004500520033003C002F0049006E00760061006C006900640046006F006C006400650072003E003C00500065006E00640069006E00670046006F006C006400650072003E0043003A005C0046004F004C0044004500520034003C002F00500065006E00640069006E00670046006F006C006400650072003E003C00520065006A006500630074006500640046006F006C006400650072003E0043003A005C0046004F004C0044004500520035003C002F00520065006A006500630074006500640046006F006C006400650072003E003C00520065006300650069007600650046006F006C006400650072003E0043003A005C0046004F004C0044004500520036003C002F00520065006300650069007600650046006F006C006400650072003E003C004100630063006500700074006500640046006F006C006400650072003E0043003A005C0046004F004C0044004500520037003C002F004100630063006500700074006500640046006F006C006400650072003E003C002F0043004C0053004D0053004D00650073007300610067006500530065006E00640069006E0067003E00";
		#endregion
	}
}
