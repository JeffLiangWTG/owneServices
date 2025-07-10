using System;
using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.Customs.IL.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Module.Testing
{
	[TestedType(typeof(CustomsMessagingController))]
	sealed class CustomsMessagingControllerTest : ZControllerBasherTest
	{
		public void TestCheckPoints()
		{
			var controller = new CustomsMessagingController();

			CombineAssertions("When the business object is a ForwardingShipment",
				() =>
				{
					var shipment = Factory.New<ForwardingShipment>();
					AssertEquals(Env.Security.OrgConfigModifyCountryDefaults, controller.GetCheckPointForDelete(shipment));
					AssertEquals(Env.Security.OrgConfigModifyCountryDefaults, controller.GetCheckPointForEdit(shipment));
					AssertEquals(Env.Security.OrgConfigModifyCountryDefaults, controller.GetCheckPointForNew(shipment));
					AssertEquals(Env.Security.OrgConfigModifyCountryDefaults, controller.GetCheckPointForView(shipment));
				}
			);

			CombineAssertions("When the business object is a ForwardingConsol",
				() =>
				{
					var consol = Factory.New<ForwardingConsol>();
					AssertEquals(Env.Security.OrgConfigModifyCountryDefaults, controller.GetCheckPointForDelete(consol));
					AssertEquals(Env.Security.OrgConfigModifyCountryDefaults, controller.GetCheckPointForEdit(consol));
					AssertEquals(Env.Security.OrgConfigModifyCountryDefaults, controller.GetCheckPointForNew(consol));
					AssertEquals(Env.Security.OrgConfigModifyCountryDefaults, controller.GetCheckPointForView(consol));
				}
			);
		}

		public void TestGetPlugIn_Shipment()
		{
			var controller = new CustomsMessagingController();
			var shipment = Factory.New<ForwardingShipment>();
			var getPlugin = typeof(CustomsMessagingController).GetMethod("GetPlugIn", BindingFlags.Instance | BindingFlags.NonPublic);
			var plugin = getPlugin.Invoke(controller, new object[] { shipment });
			try
			{
				AssertNotNull("Plugin is not null", plugin);
				AssertEquals("PlugIn type", typeof(ShipmentCustomsMessagingPlugIn), plugin.GetType());
			}
			finally
			{
				((CustomsMessagingPlugInBase)plugin).Dispose();
			}
		}

		public void TestGetPlugIn_Consol()
		{
			var controller = new CustomsMessagingController();
			var consol = Factory.New<ForwardingConsol>();
			var getPlugin = typeof(CustomsMessagingController).GetMethod("GetPlugIn", BindingFlags.Instance | BindingFlags.NonPublic);
			var plugin = getPlugin.Invoke(controller, new object[] { consol });
			try
			{
				AssertNotNull("Plugin is not null", plugin);
				AssertEquals("PlugIn type", typeof(ConsolCustomsMessagingPlugIn), plugin.GetType());
			}
			finally
			{
				((CustomsMessagingPlugInBase)plugin).Dispose();
			}
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var obj = Factory.New<ForwardingShipment>();
			Factory.Save();
			return obj;
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.IL.CustomsMessaging;

		public override Type ControllerToBashType => typeof(CustomsMessagingController);
	}
}
