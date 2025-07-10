using CargoWise.Definitions;
using Enterprise.ZArchitecture.Core.Test.Utilities;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Startup.Testing
{
	public class ValidateDbArgumentsTest : AbstractApplicationStartupTaskTest<ValidateDbArguments>
	{
		public void TestExecuteWithDbArgsSignature()
		{
			Assert(!new ValidateDbArgumentsForTest().Execute(new ApplicationArguments(new string[] { "TestServerName", "TestDatabaseName", "-DbArgsSignature:" })));
			AssertEquals("Your database parameter signature check failed", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public override int DefaultErrorExitCode => ExitCodes.ValidateDbArgumentsError;
	}

	public class ValidateDbArgumentsForTest : ValidateDbArguments
	{
		internal override bool MockValidationForTest => false;
	}
}
