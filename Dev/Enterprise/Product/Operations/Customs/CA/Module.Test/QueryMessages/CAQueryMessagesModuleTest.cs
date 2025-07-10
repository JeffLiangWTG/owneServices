using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(CAQueryMessagesModule))]
	sealed class CAQueryMessagesModuleTest : ZModuleBasherTest
	{
		public void TestSecurityCheckPoint()
		{
			using (var module = new CAQueryMessagesModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.CAQueryMessages, module.SecurityCheckpoint);
			}
		}

		public void TestFilterControl()
		{
			using (var module = new CAQueryMessagesModule())
			{
				AssertType<CAQueryMessagesFilterControl>(module.EmbeddedControl);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.CA.CAQueryMessages;

		protected override string CountryCode => Core.Constants.CountryCodes.Canada;
	}
}
