using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(K84ReportsModule))]
	sealed class K84ReportsModuleTest : ZModuleBasherTest
	{
		public void TestSecurityCheckPoint()
		{
			using (var module = new K84ReportsModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.CAK84Reports, module.SecurityCheckpoint);
			}
		}

		public void TestFilterControl()
		{
			using (var module = new K84ReportsModule())
			{
				AssertEquals("Filter control type", typeof(K84ReportsFilterControl), module.EmbeddedControl.GetType());
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.CA.K84Reports;

		protected override string CountryCode => Core.Constants.CountryCodes.Canada;
	}
}
