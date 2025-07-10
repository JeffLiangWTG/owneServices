using System.Linq;
using CargoWise.Data;
using CargoWise.Main.Startup.SqlSecurity;
using NUnit.Framework;

namespace CargoWise.Main.Test.Startup.SqlSecurity
{
	class SqlSecurityMessageToProgressMapperTest : TestCase
	{
		public void TestProgressIsCompleteWhenReachesCorrectMessage()
		{
			// Arrange
			var messageToProgressMapper = new SqlSecurityMessageToProgressMapper();

			// Act
			var hasProgressed = messageToProgressMapper.Progress("Building Sql security took 1.");

			// Assert
			Assert("Message starting with 'Building Sql security took' indicates that the progress should have move to the end.", hasProgressed);
			AssertEquals("Current progress should have moved to 100%", 100, messageToProgressMapper.CurrentProgress);
			AssertEquals("Status should show last message", messageToProgressMapper.CurrentStatus, "Building Sql security took 1.");
		}

		public void TestStatusIsSetCorrectlyWhenReachesCorrectMessage()
		{
			// Arrange
			var messageToProgressMapper = new SqlSecurityMessageToProgressMapper();
			var numberOfDatabases = Db.Connection.GetDatabases(DatabaseType.All).Count();
			var totalProgressSteps = numberOfDatabases + 1;

			// Act
			var hasProgressed1 = messageToProgressMapper.Progress("Building Sql security for server 'server name'.");
			var progress1 = messageToProgressMapper.CurrentProgress;
			var status1 = messageToProgressMapper.CurrentStatus;

			var hasProgressed2 = messageToProgressMapper.Progress("Building Sql security for database 'MainDatabase'.");
			var progress2 = messageToProgressMapper.CurrentProgress;
			var status2 = messageToProgressMapper.CurrentStatus;

			var hasProgressed3 = messageToProgressMapper.Progress("Building Sql security for database 'SDDatabase'.");
			var progress3 = messageToProgressMapper.CurrentProgress;
			var status3 = messageToProgressMapper.CurrentStatus;

			// Assert
			Assert("Message starting with 'Building Sql security for database '<some database name>' took' indicates that the progress should have moved.", hasProgressed1);
			AssertEquals("Current progress should have moved by one step.", 0 / totalProgressSteps, progress1);
			AssertEquals("Status should show last message.", "Building Sql security for server 'server name'.", status1);

			Assert("Message starting with 'Building Sql security for database '<some database name>' took' indicates that the progress should have moved.", hasProgressed2);
			AssertEquals("Current progress should have moved by one more step.", 100 / totalProgressSteps, progress2);
			AssertEquals("Status should show last message.", "Building Sql security for database 'MainDatabase'.", status2);

			Assert("Message starting with 'Building Sql security for database '<some database name>' took' indicates that the progress should have moved.", hasProgressed3);
			AssertEquals("Current progress should have moved by one more step.", 200 / totalProgressSteps, progress3);
			AssertEquals("Status should show last message.", "Building Sql security for database 'SDDatabase'.", status3);
		}
	}
}
