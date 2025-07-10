using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Modules;
using Enterprise.Environment;

namespace Enterprise.Client.EDI.Billing.Module.Testing
{
	public class StlBillingModuleTest : TestCaseWithFactory
	{
		public void TestModuleID()
		{
			AssertEquals(ClientModuleRegistration.StlBilling, Module.ID);
		}

		public void TestLicenseCheckpoint()
		{
			AssertEquals(Env.Licence.AlwaysAllow, Module.LicenceCheckPoint);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(EDISecurityCheckpoints.STLBilling, Module.SecurityCheckpoint);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			Module = new StlBillingModule();
		}

		protected override void TearDown()
		{
			if (Module != null)
			{
				Module.Dispose();
			}

			base.TearDown();
		}

		protected StlBillingModule Module;
		#endregion
	}
}
