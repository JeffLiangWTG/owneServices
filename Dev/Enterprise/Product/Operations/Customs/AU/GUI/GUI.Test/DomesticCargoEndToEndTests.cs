using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.GUI.PlugIn;
using Enterprise.Customs.AU.SeaCargo.GUI;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class DomesticCargoEndToEndTests : TestCaseWithFactory
	{
		public void TestSeaCargoImportVisibilityForDomesticCargoOnConsol()
		{
			ForwardingConsol consol = GetDomesticConsol();

			using (ConsolForm form = new ConsolForm(consol))
			{
				AssertPluginVisibility(form, typeof(SeaCargoConsolWithScanPlugin), false);
			}
		}

		public void TestSeaCargoExportManifestVisibilityForDomesticCargoOnConsol()
		{
			ForwardingConsol consol = GetDomesticConsol();

			using (ConsolForm form = new ConsolForm(consol))
			{
				AssertPluginVisibility(form, typeof(AUCustomsExit2PluginToConsol), true);
			}
		}

		public void TestSeaCargoImportVisibilityForDomesticCargoOnShipment()
		{
			ForwardingConsol consol = GetDomesticConsol();
			ForwardingShipment shipment = consol.Shipments.AddNew();

			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_RL_NKOrigin = "AUSYD";
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (ShipmentForm form = new ShipmentForm(shipment))
			{
				AssertPluginVisibility(form, typeof(SeaCargoShipmentPlugIn), false);
			}
		}

		ForwardingConsol GetDomesticConsol()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "AUSYD";
			return consol;
		}

		static void AssertPluginVisibility(ZForm form, Type pluginToFind, bool shouldBeVisible)
		{
			bool pluginFound = false;

			foreach (ZPlugIn plugin in form.PlugIns.Instances)
			{
				if (plugin.GetType() == pluginToFind)
				{
					AssertEquals(plugin.Name + " plugin should be " + (shouldBeVisible ? "enabled" : "disabled"), shouldBeVisible, plugin.Enabled);
					pluginFound = true;
					break;
				}
			}

			Assert("Plugin is missing", pluginFound);
		}
	}
}
