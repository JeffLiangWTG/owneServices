using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Modules;
using Enterprise.Environment;

namespace Enterprise.Client.EDI.Billing.Module.Testing
{
	public class MaintenanceBillingModuleTest : TestCaseWithFactory
	{
		public void TestModuleID()
		{
			AssertEquals(ClientModuleRegistration.MaintenanceBilling, Module.ID);
		}

		public void TestLicenseCheckpoint()
		{
			AssertEquals(Env.Licence.AlwaysAllow, Module.LicenceCheckPoint);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.None, Module.SecurityCheckpoint);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			Module = new MaintenanceBillingModule();
		}

		protected override void TearDown()
		{
			if (Module != null)
			{
				Module.Dispose();
			}

			base.TearDown();
		}

		protected MaintenanceBillingModule Module;
		#endregion
	}
}