using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.ApplicationLogging.GUI.Testing
{
	[TestedType(typeof(ApplicationLoggerModule))]
	internal class ApplicationLoggerModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ClientModuleRegistration.ApplicationLogger;

		public void TestSecurityCheckpoint()
		{
			AssertEquals(EDISecurityCheckpoints.ApplicationLoggingApplicationLogger, Module.SecurityCheckpoint);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			Module = new ApplicationLoggerModule();
		}

		protected override void TearDown()
		{
			Module?.Dispose();
			base.TearDown();
		}

		protected ApplicationLoggerModule Module;
		#endregion
	}
}
