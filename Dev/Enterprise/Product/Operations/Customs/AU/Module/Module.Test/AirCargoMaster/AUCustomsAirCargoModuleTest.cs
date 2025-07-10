using System;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.AirCargo.Testing
{
	[TestedType(typeof(AUCustomsAirCargoModule))]
	public class AUCustomsAirCargoModuleTest : ZModuleBasherTest
	{
		public void TestLicenceCheckPoint()
		{
			AssertEquals(Env.Licence.AirCargoReport, testModule.LicenceCheckPoint);
		}

		public void TestGetNewController()
		{
			var mAWB = Factory.New<CusMAWB>();
			AssertEquals(typeof(AUCustomsAirCargoController), testModule.GetNewControllerInternal(mAWB).GetType());
			var consol = Factory.New<ForwardingConsol>();
			mAWB.CM_JK = consol.PK;
			AssertEquals(typeof(AUCustomsAirCargoConsolController), testModule.GetNewControllerInternal(mAWB).GetType());
		}

		[TestDate(2015, 10, 21)]
		public void TestCSVFileImportMenuExistsAndIsHookedUp_InterfaceConnectorOn()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil()
			{ EnabledUntil = ZDateTime.Today.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			using (AUCustomsAirCargoModule testModule = new AUCustomsAirCargoModule())
			{
				MenuAssertion.AssertHasMenu("Should find the CSV Import menu", testModule.FormActionMenu, "&Actions", "D&ata Transfer", "Import From CSV File");
			}

			tomorrow = new InterfaceConnectorTemporarilyEnabledUntil()
			{ EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
		}

		[TestDate(2015, 10, 21)]
		public void TestCSVFileImportMenuExistsAndIsHookedUp_InterfaceConnectorOff()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil()
			{ EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			using (AUCustomsAirCargoModule testModule = new AUCustomsAirCargoModule())
			{
				MenuAssertion.AssertHasMenu("Should find the CSV Import menu", testModule.FormActionMenu, "&Actions", "D&ata Transfer", "Import From CSV File");
				// BG: For now does not matter what InterfaceConnector registry is set to.
			}
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.AU.AirCargo;

		AUCustomsAirCargoModule testModule;
		protected override void SetUp()
		{
			base.SetUp();
			testModule = new AUCustomsAirCargoModule();
		}

		protected override void TearDown()
		{
			testModule?.Dispose();
			base.TearDown();
		}
	}
}
