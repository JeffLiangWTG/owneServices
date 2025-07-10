using System;
using System.Reflection;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	abstract class SeaCargoPlugInTest : BaseSeaCargoPlugInTest
	{
		public void TestTickCoLoadMasterInCMR()
		{
			FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			var consol = CreateFCLConsol();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "GFCU9933810";
			var shipment = consol.Shipments.AddNew();
			var synchroniser = GetSeaCargoSynchroniser(consol);
			var oceanBill = synchroniser.OceanBill;
			synchroniser.LoadHouseBills();
			var incorrectlyDeclaredHouseBill = synchroniser.GetHouseBill(shipment);
			incorrectlyDeclaredHouseBill.AcceptCurrentAsAcknowledged();
			using (var testPlugIn = GetPlugIn(shipment))
			{
				shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
				AssertNull("Question should have been asked", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var consol = CreateFCLConsol();
			var shipment = consol.Shipments.AddNew();
			using (var plugIn = GetPlugIn(shipment))
			{
				AssertEquals("House bill not created yet", false, ShouldPlugInGUIAndBusinessEntityBeCreated(plugIn));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertEquals("PlugIn can be shown when house bill has been created", true, QueryUserShouldPlugInGUIAndBusinessEntityBeCreated(plugIn));
				AssertEquals("PlugIn can be shown when house bill has been created", true, ShouldPlugInGUIAndBusinessEntityBeCreated(plugIn));
			}
		}

		protected abstract SeaCargoPlugIn GetPlugIn(CommonShipment parent);

		bool ShouldPlugInGUIAndBusinessEntityBeCreated(ZPlugIn plugIn)
		{
			MethodInfo method = typeof(ZPlugIn).GetMethod("ShouldPlugInGUIAndBusinessEntityBeCreated", BindingFlags.NonPublic | BindingFlags.Instance, null, Array.Empty<Type>(), null);
			return (bool)method.Invoke(plugIn, null);
		}

		bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated(ZPlugIn plugIn)
		{
			MethodInfo method = typeof(ZPlugIn).GetMethod("QueryUserShouldPlugInGUIAndBusinessEntityBeCreated", BindingFlags.NonPublic | BindingFlags.Instance, null, Array.Empty<Type>(), null);
			return (bool)method.Invoke(plugIn, null);
		}
	}
}
