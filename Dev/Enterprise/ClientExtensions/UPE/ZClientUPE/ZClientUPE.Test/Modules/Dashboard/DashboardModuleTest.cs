using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.Module.Testing
{
	internal class DashboardModuleTest : TestCaseWithDummy
	{
		public void TestLicenceAndSecurityCheckpoint()
		{
			AssertEquals(Env.Licence.AlwaysAllow, Module.LicenceCheckPoint);
			AssertEquals(Env.Security.None, Module.SecurityCheckpoint);
		}

		public void TestModuleID()
		{
			AssertEquals(ClientModuleRegistration.Dashboard, Module.ID);
		}

		public void TestGetNewController()
		{
			AssertEquals(typeof(DashboardController), Module.GetNewController().GetType());
		}

		protected override void TearDown()
		{
			Module.Dispose();
			base.TearDown();
		}

		#region DashboardModuleForTest
		DashboardModuleForTest Module
		{
			get
			{
				if (fModule == null)
				{
					fModule = new DashboardModuleForTest();
				}

				return fModule;
			}
		}

		DashboardModuleForTest fModule;
		class DashboardModuleForTest : DashboardModule
		{
			public new ZController GetNewController()
			{
				return base.GetNewController();
			}
		}
		#endregion
	}
}
