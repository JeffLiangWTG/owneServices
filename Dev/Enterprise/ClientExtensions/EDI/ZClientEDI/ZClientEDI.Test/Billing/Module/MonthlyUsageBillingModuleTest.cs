using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Modules;
using Enterprise.Environment;

namespace Enterprise.Client.EDI.Billing.Module.Testing
{
	public class MonthlyUsageBillingModuleTest : TestCaseWithFactory
	{
		public void TestModuleID()
		{
			AssertEquals(ClientModuleRegistration.MonthlyUsageBilling, Module.ID);
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
			Module = new MonthlyUsageBillingModule();
		}

		protected override void TearDown()
		{
			if (Module != null)
			{
				Module.Dispose();
			}

			base.TearDown();
		}

		protected MonthlyUsageBillingModule Module;
		#endregion
	}
}