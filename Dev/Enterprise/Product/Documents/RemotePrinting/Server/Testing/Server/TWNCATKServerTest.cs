using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.RemotePrinting.Server.RPSCore;

namespace Enterprise.RemotePrinting.Server.Testing
{
	sealed class NCATKServerForTest : TWNCATKServer
	{
		public NCATKServerForTest(DbConnection testConnection)
		{
			this.testConnection = testConnection;
		}

		protected override DbConnection NewConnection() => testConnection ??= Db.NewExtraConnectionToMainDb();
		DbConnection testConnection;
	}

	sealed class TWNCATKServerTest : TestCaseWithFactory
	{
		public void TestGetSettingByMachineName()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				try
				{
					connection.BeginTransaction();
					var insertSetting = $@"
DELETE FROM dbo.StmData WHERE SD_Name = 'TWNCATKClientSetting';
DELETE FROM dbo.StmData WHERE SD_Name = 'EHubSendInterchangesToTestGateway';
DELETE FROM dbo.StmData WHERE SD_Name = 'EHubTestGatewayServerAddress';
INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Type, SD_BinaryValue) VALUES(NEWID(), 'TWNCATKClientSetting', 'BIN', {NormalSettingBinaryString});
INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Type, SD_BinaryValue) VALUES(NEWID(), 'Ehubgatewayserveraddress', 'STR', 0x65006800750062002D00610075007300790064002E0063006100720067006F0077006900730065002E006E0065007400)";
					connection.ExecuteNonQuery(insertSetting);

					var server = new NCATKServerForTest(connection);
					var testResult = server.GetSettingByMachineName("FFF");

					AssertEquals("MachineName", "FFF", testResult.MachineName);
					AssertEquals("EHubClientID", "HYEDTWCMT_TCA", testResult.EHubClientID);
					AssertEquals("EHubClientStatus", "OK", testResult.EHubClientStatus);
					AssertEquals("EHubClientPassword", string.Empty, testResult.EHubClientPassword);
					AssertEquals("RunningIntervalInSeconds", 60, testResult.RunningIntervalInSeconds);
					AssertEquals("SendToFolder", @"D:\NCATK", testResult.SendToFolder);
					AssertEquals("EHubGatewayServerAddress", "ehub-ausyd.cargowise.net", testResult.EHubGatewayServerAddress);
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
DELETE FROM dbo.StmData WHERE SD_Name = 'TWNCATKClientSetting';
INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Type, SD_BinaryValue) VALUES(NEWID(), 'TWNCATKClientSetting', 'BIN', {NormalSettingBinaryString})";
					connection.ExecuteNonQuery(insertSetting);

					var server = new NCATKServerForTest(connection);
					var testResult = server.GetSettingByMachineName("XXXName'+or+'1'='1'");
					AssertSame("IsEmpty", TWNCATKClientSetting.Empty, testResult);
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
DELETE FROM dbo.StmData WHERE SD_Name = 'TWNCATKClientSetting';
DELETE FROM dbo.StmData WHERE SD_Name = 'EHubSendInterchangesToTestGateway';
DELETE FROM dbo.StmData WHERE SD_Name = 'EHubTestGatewayServerAddress';
INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Type, SD_BinaryValue) VALUES(NEWID(), 'TWNCATKClientSetting', 'BIN', {NormalSettingBinaryString});
INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Type, SD_BinaryValue) VALUES(NEWID(), 'EHubSendInterchangesToTestGateway', 'BOL', 0x5400720075006500)
INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Type, SD_BinaryValue) VALUES(NEWID(), 'EHubTestGatewayServerAddress', 'STR', 0x65006800750062002D00610075007300790064002D0074006500730074002E00770069007300650067007200690064002E006E0065007400)
";
					connection.ExecuteNonQuery(insertSetting);
					var testResult = new NCATKServerForTest(connection).GetSettingByMachineName("FFF");
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
		const string NormalSettingBinaryString = "0x3C003F0078006D006C002000760065007200730069006F006E003D00220031002E0030002200200065006E0063006F00640069006E0067003D0022007500740066002D003100360022003F003E003C00540057004E004300410054004B0043006C00690065006E007400530065007400740069006E0067003E003C004D0061006300680069006E0065004E0061006D0065003E004600460046003C002F004D0061006300680069006E0065004E0061006D0065003E003C00530065006E00640054006F0046006F006C006400650072003E0044003A005C004E004300410054004B003C002F00530065006E00640054006F0046006F006C006400650072003E003C00520075006E006E0069006E00670049006E00740065007200760061006C0049006E005300650063006F006E00640073003E00360030003C002F00520075006E006E0069006E00670049006E00740065007200760061006C0049006E005300650063006F006E00640073003E003C00450048007500620043006C00690065006E007400490044003E0048005900450044005400570043004D0054005F005400430041003C002F00450048007500620043006C00690065006E007400490044003E003C00450048007500620043006C00690065006E0074005300740061007400750073003E004F004B003C002F00450048007500620043006C00690065006E0074005300740061007400750073003E003C002F00540057004E004300410054004B0043006C00690065006E007400530065007400740069006E0067003E00";
		#endregion
	}
}
