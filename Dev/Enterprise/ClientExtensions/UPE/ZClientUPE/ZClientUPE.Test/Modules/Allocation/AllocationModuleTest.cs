using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.Module.Testing
{
	internal class AllocationModuleTest : TestCaseWithDummy
	{
		public void TestLicenceAndSecurityCheckpoint()
		{
			AssertEquals(Env.Licence.AlwaysAllow, Module.LicenceCheckPoint);
			AssertEquals(Env.Security.None, Module.SecurityCheckpoint);
		}

		public void TestModuleID()
		{
			AssertEquals(ClientModuleRegistration.Allocation, Module.ID);
		}

		public void TestGetNewController()
		{
			AssertEquals(typeof(AllocationController), Module.GetNewController().GetType());
		}

		protected override void TearDown()
		{
			Module.Dispose();
			base.TearDown();
		}

		#region AllocationModuleForTest
		AllocationModuleForTest Module
		{
			get
			{
				if (fModule == null)
				{
					fModule = new AllocationModuleForTest();
				}

				return fModule;
			}
		}

		AllocationModuleForTest fModule;
		class AllocationModuleForTest : AllocationModule
		{
			public new ZController GetNewController()
			{
				return base.GetNewController();
			}
		}
		#endregion
	}
}
