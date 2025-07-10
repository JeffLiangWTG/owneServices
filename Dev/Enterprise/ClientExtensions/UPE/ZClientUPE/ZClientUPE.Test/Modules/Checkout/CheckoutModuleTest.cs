using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.Module.Testing
{
	internal class CheckoutModuleTest : TestCaseWithDummy
	{
		public void TestLicenceAndSecurityCheckpoint()
		{
			AssertEquals(Env.Licence.AlwaysAllow, Module.LicenceCheckPoint);
			AssertEquals(Env.Security.None, Module.SecurityCheckpoint);
		}

		public void TestModuleID()
		{
			AssertEquals(ClientModuleRegistration.Checkout, Module.ID);
		}

		public void TestGetNewController()
		{
			AssertEquals(typeof(CheckoutController), Module.GetNewController().GetType());
		}

		protected override void TearDown()
		{
			Module.Dispose();
			base.TearDown();
		}

		#region CheckoutModuleForTest
		CheckoutModuleForTest Module
		{
			get
			{
				if (fModule == null)
				{
					fModule = new CheckoutModuleForTest();
				}

				return fModule;
			}
		}

		CheckoutModuleForTest fModule;
		class CheckoutModuleForTest : CheckoutModule
		{
			public new ZController GetNewController()
			{
				return base.GetNewController();
			}
		}
		#endregion
	}
}
