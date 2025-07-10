using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(BankReconcilliationModule))]
	public class BankReconcilliationModuleTest : ZArchitecture.Modules.Testing.ZPopupModuleBasherTest
	{
		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.BankReconcilliation, Module.ID);
		}

		public void TestLicenseCheckpoint()
		{
			AssertEquals(Env.Licence.Core, Module.LicenceCheckPoint);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.BankReconciliation, Module.SecurityCheckpoint);
		}

		public void TestGetNewController()
		{
			AssertEquals(typeof(BankReconcilliationController), Module.GetNewController_ForTestOnly().GetType());
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Module = new BankReconcilliationModule();
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.BankReconcilliation;
		}

		protected override void TearDown()
		{
			if (Module != null)
			{
				Module.Dispose();
			}
			base.TearDown();
		}

		BankReconcilliationModule Module;

		#endregion
	}
}
