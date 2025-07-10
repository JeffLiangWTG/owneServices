using CargoWise.Data.Testing;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core.Test.Utilities;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Startup.Testing
{
	public class ValidatePurgeDataRunStatusTest : AbstractApplicationStartupTaskTest<ValidatePurgeDataRunStatus>
	{
		[UseSnapshotProtection]
		public void TestPurgeDataRunStatus()
		{
			Env.Registry.PurgeDataRunStatusFlag = "CMP";
			Assert(new ValidatePurgeDataRunStatus().Execute(new ApplicationArguments(new string[] { "TestServerName", "TestDatabaseName" })));
		}

		[UseSnapshotProtection]
		public void TestPurgeDataRunStatus_Runing()
		{
			Env.Registry.PurgeDataRunStatusFlag = "RUN";

			Assert(!new ValidatePurgeDataRunStatus().Execute(new ApplicationArguments(new string[] { "TestServerName", "TestDatabaseName" })));
			AssertEquals("A Data Purge is in progress. Please wait until the Data Purge is completed.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[UseSnapshotProtection]
		public void TestPurgeDataRunStatus_Error()
		{
			Env.Registry.PurgeDataRunStatusFlag = "ERR";

			Assert(!new ValidatePurgeDataRunStatus().Execute(new ApplicationArguments(new string[] { "TestServerName", "TestDatabaseName" })));
			AssertEquals("There was an error when purging data. Please restore the database from the latest backup.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public override int DefaultErrorExitCode => ExitCodes.ValidatePurgeDataRunStatusError;
	}
}
