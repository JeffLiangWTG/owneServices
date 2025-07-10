using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(CASSBillingModule))]
	public class CassBillingModuleBasherTest : ZPopupModuleBasherTest
	{
		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.CASSCostFileImport, Module.ID);
		}

		public void TestLicenseCheckpoint()
		{
			AssertEquals(Env.Licence.Accountant, Module.LicenceCheckPoint);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.APCASSCostFileImport, Module.SecurityCheckpoint);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Module = new CASSBillingModule();
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.CASSCostFileImport;
		}

		protected override void TearDown()
		{
			if (Module != null)
			{
				Module.Dispose();
			}
			base.TearDown();
		}

		protected CASSBillingModule Module;

		#endregion
	}
}
