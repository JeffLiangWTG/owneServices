using System;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(SeaCargoDepotModule))]
	sealed class SeaCargoDepotModuleTest : CMRModuleTest
	{
		public void TestID()
		{
			AssertEquals(ModuleIDs.Customs.AU.SeaCargoDepot, testModule.ID);
		}

		public void TestLicenceCheckPoint()
		{
			AssertEquals(Env.Licence.SeaCargoDepot, testModule.LicenceCheckPoint);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.AUCustomsSCADepot, testModule.SecurityCheckpoint);
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.AU.SeaCargoDepot;

		protected override Type GetTypeOfFilterControl() => typeof(SeaCargoDepotFilterControl);

		SeaCargoDepotModule testModule;
		protected override void SetUp()
		{
			testModule = new SeaCargoDepotModule();
			base.SetUp();
		}

		protected override void TearDown()
		{
			testModule?.Dispose();
			base.TearDown();
		}
	}
}
