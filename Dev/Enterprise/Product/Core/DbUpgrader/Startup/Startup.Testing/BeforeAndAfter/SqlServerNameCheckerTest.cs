using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	sealed class SqlServerNameCheckerTest : TestCase
	{
		public void TestServerNameAndMachineNameMatch()
		{
			AssertServerNameAndMachineNameMatch();
		}

		void AssertServerNameAndMachineNameMatch()
		{
			string serverName;
			string machineName;
			GetServerNameAndMachineName(out serverName, out machineName);
			AssertEquals("Server name does not match Machine name", machineName, serverName);
		}

		void GetServerNameAndMachineName(out string serverName, out string machineName)
		{
			serverName = machineName = "";

			using (var cmd = Db.Connection.Command("SELECT @@SERVERNAME, CONVERT(nvarchar(128), SERVERPROPERTY('ServerName'));"))
			using (var reader = cmd.ExecuteReader())
			{
				if (reader.Read())
				{
					serverName = reader.GetString(0);
					machineName = reader.GetString(1);
				}
			}
		}
	}
}
