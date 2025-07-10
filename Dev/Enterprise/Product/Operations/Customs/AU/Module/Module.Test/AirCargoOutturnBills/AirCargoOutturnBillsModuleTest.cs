using System;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(AirCargoOutturnBillsModule))]
	sealed class AirCargoOutturnBillsModuleTest : CMRModuleTest
	{
		public void TestID()
		{
			AssertEquals(ModuleIDs.Customs.AU.AirCargoOutturnBills, testModule.ID);
		}

		public void TestLicenceCheckPoint()
		{
			AssertEquals(Env.Licence.AirCargoReport, testModule.LicenceCheckPoint);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.ACAOutturnBills, testModule.SecurityCheckpoint);
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.AU.AirCargoOutturnBills;

		protected override Type GetTypeOfFilterControl() => typeof(AirCargoOutturnBillsFilterControl);

		AirCargoOutturnBillsModule testModule;
		protected override void SetUp()
		{
			testModule = new AirCargoOutturnBillsModule();
			base.SetUp();
		}

		protected override void TearDown()
		{
			testModule?.Dispose();
			base.TearDown();
		}
	}
}
