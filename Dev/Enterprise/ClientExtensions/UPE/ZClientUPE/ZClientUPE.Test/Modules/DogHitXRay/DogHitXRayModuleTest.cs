using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.Module.Testing
{
	internal class DogHitXRayModuleTest : TestCaseWithDummy
	{
		public void TestLicenceAndSecurityCheckpoint()
		{
			AssertEquals(Env.Licence.AlwaysAllow, Module.LicenceCheckPoint);
			AssertEquals(Env.Security.None, Module.SecurityCheckpoint);
		}

		public void TestModuleID()
		{
			AssertEquals(ClientModuleRegistration.DogHitXRay, Module.ID);
		}

		public void TestGetNewController()
		{
			AssertEquals(typeof(DogHitXRayController), Module.GetNewController().GetType());
		}

		protected override void TearDown()
		{
			Module.Dispose();
			base.TearDown();
		}

		#region DogHitXRayModuleForTest
		DogHitXRayModuleForTest Module
		{
			get
			{
				if (fModule == null)
				{
					fModule = new DogHitXRayModuleForTest();
				}

				return fModule;
			}
		}

		DogHitXRayModuleForTest fModule;
		class DogHitXRayModuleForTest : DogHitXRayModule
		{
			public new ZController GetNewController()
			{
				return base.GetNewController();
			}
		}
		#endregion
	}
}
