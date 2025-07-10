
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.ApplicationLogging.GUI.Testing
{
	[TestedType(typeof(ApplicationActiveLoggerModule))]
	internal class ApplicationActiveLoggerModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ClientModuleRegistration.ApplicationActiveLogger;

		public void TestSecurityCheckpoint()
		{
			AssertEquals(EDISecurityCheckpoints.ApplicationLoggingApplicationActiveLogger, Module.SecurityCheckpoint);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			Module = new ApplicationActiveLoggerModule();
		}

		protected override void TearDown()
		{
			Module?.Dispose();
			base.TearDown();
		}

		protected ApplicationActiveLoggerModule Module;
		#endregion
	}
}
