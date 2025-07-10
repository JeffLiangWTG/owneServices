using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.AirCargo.Testing
{
	[TestedType(typeof(AUCustomsHouseAirCargoModule))]
	sealed class AUCustomsHouseAirCargoModuleTest : ZModuleBasherTest
	{
		public void TestReleaseConsignmentsFromBondStore()
		{
			var cusHawb1 = Factory.NewWithValidTestData<CusHAWB>();
			cusHawb1.CS_IsHeldAtOutturn = true;
			cusHawb1.CS_CustomsStatus = "HLD";
			var cusHawb2 = Factory.NewWithValidTestData<CusHAWB>();
			cusHawb2.CS_IsHeldAtOutturn = true;
			cusHawb2.CS_CustomsStatus = "CLR";
			Factory.Save();
			Registry.Business.HVLVDataRegistry.HasHVLVClearance = true;
			Env.Security.ReleaseConsignmentsFromBondStore.IsAllowed = true;
			using (var module = new AUCustomsHouseAirCargoModuleForTest())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module.fSelectedBusinessObjects = new[] { cusHawb1, cusHawb2 };
				module.DisplayGrid.ContextMenu.MenuItems.FindByText("Release Cleared Consignment(s) from Bond Store").PerformClick();
				AssertContains("Released 1 Cleared Consignment(s) from Bond Store.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(cusHawb1.CS_IsHeldAtOutturn);
				Assert(!cusHawb2.CS_IsHeldAtOutturn);
			}
		}

		public void TestAllowNew()
		{
			Assert(!testModule.AllowNew);
		}

		public void TestLicenceCheckPoint()
		{
			AssertEquals(Env.Licence.AirCargoReport, testModule.LicenceCheckPoint);
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.AU.HouseAirCargo;

		AUCustomsHouseAirCargoModule testModule;
		protected override void SetUp()
		{
			base.SetUp();
			testModule = new AUCustomsHouseAirCargoModule();
		}

		protected override void TearDown()
		{
			testModule?.Dispose();
			base.TearDown();
		}

		sealed class AUCustomsHouseAirCargoModuleForTest : AUCustomsHouseAirCargoModule
		{
			public BusinessObject[] fSelectedBusinessObjects;

			protected override BusinessObject[] SelectedBusinessObjects => fSelectedBusinessObjects;
		}
	}
}
