using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.RemotePrinting.Server.RPSCore;

namespace Enterprise.RemotePrinting.Server.Testing
{
	sealed class CNSWServerForTest : CNSWServer
	{
		public CNSWServerForTest(DbConnection testConnection)
		{
			this.testConnection = testConnection;
		}

		protected override DbConnection NewConnection() => testConnection ??= Db.NewExtraConnectionToMainDb();
		DbConnection testConnection;
	}

	sealed class CNSWServerTest : TestCaseWithFactory
	{
		public void TestGetSettingByMachineName()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				try
				{
					connection.BeginTransaction();

					var insertSetting = $@"
DELETE FROM dbo.StmData WHERE SD_Name = 'CNSWClientApplicationSetting';
DELETE FROM dbo.StmData WHERE SD_Name = 'EHubSendInterchangesToTestGateway';
DELETE FROM dbo.StmData WHERE SD_Name = 'EHubTestGatewayServerAddress';
INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Type, SD_BinaryValue) VALUES(NEWID(), 'CNSWClientApplicationSetting', 'BIN' ,{NormalSettingBinaryString});
INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Type, SD_BinaryValue) VALUES(NEWID(), 'Ehubgatewayserveraddress', 'STR' ,0x65006800750062002D00610075007300790064002E0063006100720067006F0077006900730065002E006E0065007400)";
					connection.ExecuteNonQuery(insertSetting);

					var server = new CNSWServerForTest(connection);
					var testResult = server.GetSettingByMachineName("MACHINE 1");

					AssertEquals("Send folder", @"D:\发送目录\", testResult.SendFolder);
					AssertEquals("Receive folder", @"D:\接收目录\", testResult.ReceiveFolder);
					AssertEquals("Error folder", @"D:\错误目录", testResult.ErrorResponseFolder);
					AssertEquals("Archive folder", @"D:\归档目录", testResult.ArchiveFolder);
					AssertEquals("Running Interval", 60, testResult.RunningIntervalInSeconds);
					AssertEquals("eHub Client ID", "HYECN2CMT_CSW", testResult.EHubClientID);
					AssertEquals("eHub Client Password", string.Empty, testResult.EHubClientPassword);
					AssertEquals("eHub Client Status", "OK", testResult.EHubClientStatus);
					AssertEquals("eHub Gateway Server Address", "ehub-ausyd.cargowise.net", testResult.EHubGatewayServerAddress);
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
DELETE FROM dbo.StmData WHERE SD_Name = 'CNSWClientApplicationSetting';
INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Type, SD_BinaryValue) VALUES(NEWID(), 'CNSWClientApplicationSetting', 'BIN' ,{NormalSettingBinaryString})";
					connection.ExecuteNonQuery(insertSetting);

					var server = new CNSWServerForTest(connection);
					var testResult = server.GetSettingByMachineName("XXXName'+or+'1'='1'");
					AssertSame("IsEmpty", CNSWClientSetting.Empty, testResult);
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
				connection.BeginTransaction();
				var insertSetting = @"
DELETE FROM dbo.StmData WHERE SD_Name = 'CNSWClientApplicationSetting';
DELETE FROM dbo.StmData WHERE SD_Name = 'EHubSendInterchangesToTestGateway';
DELETE FROM dbo.StmData WHERE SD_Name = 'EHubTestGatewayServerAddress';
INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Type, SD_BinaryValue) VALUES(NEWID(), 'CNSWClientApplicationSetting', 'BIN', NULL);
INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Type, SD_BinaryValue) VALUES(NEWID(), 'Ehubgatewayserveraddress', 'STR', 0x65006800750062002D00610075007300790064002E0063006100720067006F0077006900730065002E006E0065007400)";
				connection.ExecuteNonQuery(insertSetting);

				AssertNoExceptionThrown("Should execute without exception thrown.", () => new CNSWServerForTest(connection).GetSettingByMachineName("MACHINE 1"));
			}
		}

		public void TestEhubgatewayserveraddressDefaultValue()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				try
				{
					connection.BeginTransaction();

					var insertSetting = $@"
DELETE FROM dbo.StmData WHERE SD_Name = 'CNSWClientApplicationSetting';
DELETE FROM dbo.StmData WHERE SD_Name = 'EHubSendInterchangesToTestGateway';
DELETE FROM dbo.StmData WHERE SD_Name = 'EHubTestGatewayServerAddress';
INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Type, SD_BinaryValue) VALUES(NEWID(), 'CNSWClientApplicationSetting', 'BIN', {NormalSettingBinaryString})";
					connection.ExecuteNonQuery(insertSetting);

					var testResult = new CNSWServerForTest(connection).GetSettingByMachineName("MACHINE 1");
					AssertEquals("Should have returned a default value.", "ehubgateway.wisegrid.net", testResult.EHubGatewayServerAddress);
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

		public void TestGetTestEHubGatewayServerAddress()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				try
				{
					connection.BeginTransaction();

					var insertSetting = $@"
DELETE FROM dbo.StmData WHERE SD_Name = 'CNSWClientApplicationSetting';
DELETE FROM dbo.StmData WHERE SD_Name = 'EHubSendInterchangesToTestGateway';
DELETE FROM dbo.StmData WHERE SD_Name = 'EHubTestGatewayServerAddress';
INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Type, SD_BinaryValue) VALUES(NEWID(), 'CNSWClientApplicationSetting', 'BIN', {NormalSettingBinaryString})
INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Type, SD_BinaryValue) VALUES(NEWID(), 'EHubSendInterchangesToTestGateway', 'BOL', 0x5400720075006500)
INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Type, SD_BinaryValue) VALUES(NEWID(), 'EHubTestGatewayServerAddress', 'STR', 0x65006800750062002D00610075007300790064002D0074006500730074002E00770069007300650067007200690064002E006E0065007400)
";

					connection.ExecuteNonQuery(insertSetting);

					var testResult = new CNSWServerForTest(connection).GetSettingByMachineName("MACHINE 1");
					AssertEquals("EHubTestGatewayServerAddress", "ehub-ausyd-test.wisegrid.net", testResult.EHubGatewayServerAddress);
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
		readonly string NormalSettingBinaryString = "0x3C003F0078006D006C002000760065007200730069006F006E003D00220031002E0030002200200065006E0063006F00640069006E0067003D0022007500740066002D003100360022003F003E003C0043004E005300570043006C00690065006E007400530065007400740069006E0067003E003C004D0061006300680069006E0065004E0061006D0065003E004D0041004300480049004E004500200031003C002F004D0061006300680069006E0065004E0061006D0065003E003C00530065006E00640046006F006C006400650072003E0044003A005C00D1530190EE76555F5C003C002F00530065006E00640046006F006C006400650072003E003C00520065006300650069007600650046006F006C006400650072003E0044003A005C00A5633665EE76555F5C003C002F00520065006300650069007600650046006F006C006400650072003E003C004500720072006F00720052006500730070006F006E007300650046006F006C006400650072003E0044003A005C001995EF8BEE76555F3C002F004500720072006F00720052006500730070006F006E007300650046006F006C006400650072003E003C00410072006300680069007600650046006F006C006400650072003E0044003A005C00525F6368EE76555F3C002F00410072006300680069007600650046006F006C006400650072003E003C00520075006E006E0069006E00670049006E00740065007200760061006C0049006E005300650063006F006E00640073003E00360030003C002F00520075006E006E0069006E00670049006E00740065007200760061006C0049006E005300650063006F006E00640073003E003C00450048007500620043006C00690065006E007400490044003E0048005900450043004E00320043004D0054005F004300530057003C002F00450048007500620043006C00690065006E007400490044003E003C00450048007500620043006C00690065006E0074005300740061007400750073003E004F004B003C002F00450048007500620043006C00690065006E0074005300740061007400750073003E003C002F0043004E005300570043006C00690065006E007400530065007400740069006E0067003E00";
		#endregion

	}
}
