using System;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(SeaCargoModule))]
	sealed class SeaCargoModuleTest : CMRModuleTest
	{
		public void TestGetNewController()
		{
			using (SeaCargoModule testModule = new SeaCargoModule())
			{
				CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
				AssertEquals("Enterprise.Customs.AU.Module.SeaCargoStandAloneController", typeof(SeaCargoStandAloneController), testModule.GetNewControllerInternal(oceanBill).GetType());
				ForwardingConsol consol = Factory.New<ForwardingConsol>();
				oceanBill.CB_ParentId = consol.PK;
				oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
				AssertEquals("Enterprise.Customs.AU.Module.SeaCargo.AUCustomsSeaCargoController", typeof(SeaCargo.AUCustomsSeaCargoController), testModule.GetNewControllerInternal(oceanBill).GetType());
			}
		}

		[TestDate(2015, 10, 21)]
		public void TestCSVFileImportMenuExistsAndIsHookedUp_InterfaceConnectorOn()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil()
			{ EnabledUntil = ZDateTime.Today.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			using (SeaCargoModule testModule = new SeaCargoModule())
			{
				var importMenuItems = ((IFilterModuleInternalsForTesting)testModule).ImportMenuItems;
				AssertEquals(1, importMenuItems.Count);
				AssertEquals("OnClick", importMenuItems[0].OnClick.Method.Name);
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
			using (SeaCargoModule testModule = new SeaCargoModule())
			{
				AssertEquals(1, ((IFilterModuleInternalsForTesting)testModule).ImportMenuItems.Count);
				// BG: For now does not matter what InterfaceConnector registry is set to.
			}
		}

		public void TestHelShipmentScanning()
		{
			var oldValue = Env.Security.AUCustomsSCAHeldShipmentsScanning.IsAllowed;
			try
			{
				Env.Security.AUCustomsSCAHeldShipmentsScanning.IsAllowed = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var heldScanningMenuItem = testModule.FormActionMenu.FindByText("Actions").MenuItems.FindByText("Held Shipments Scanning");
				heldScanningMenuItem.PerformClick();
				AssertEquals(SeaCargoModule.HaveNotSecurityRightsForHeldScanning, UnitTestUserNotification.Instance.LastMessage.Text);
			}
			finally
			{
				Env.Security.AUCustomsSCAHeldShipmentsScanning.IsAllowed = oldValue;
				Env.Security.AUCustomsSCAHeldShipmentsScanning.ClearIsAllowedCache();
			}
		}

		public void TestSupportWorkflow()
		{
			Assert(testModule.SupportsWorkflow);
		}

		public void TestID()
		{
			AssertEquals(ModuleIDs.Customs.AU.SeaCargo, testModule.ID);
		}

		public void TestLicenceCheckPoint()
		{
			AssertEquals(Env.Licence.SeaCargoReport, testModule.LicenceCheckPoint);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.AUCustomsSCA, testModule.SecurityCheckpoint);
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.AU.SeaCargo;

		protected override Type GetTypeOfFilterControl() => typeof(SeaCargoFilterControl);

		SeaCargoModule testModule;
		protected override void SetUp()
		{
			testModule = new SeaCargoModule();
			base.SetUp();
		}

		protected override void TearDown()
		{
			testModule?.Dispose();
			base.TearDown();
		}
	}
}
