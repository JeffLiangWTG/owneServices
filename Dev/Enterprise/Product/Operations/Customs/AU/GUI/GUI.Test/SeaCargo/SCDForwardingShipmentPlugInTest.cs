using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class SCDForwardingShipmentPlugInTest : SCDForwardingPlugInTest
	{
		public void TestChangeConsolUnpackDepotWithImpendingContainers()
		{
			var loadList = GetConsolWithContainer();
			var shipment = loadList.Shipments.AddNew();
			loadList.Containers[0].Logs.AddNew(AutoEvents.SeaCargoDepotEvent, DepotEvents.ImpendingCargo);
			Factory.Save();
			var forwardingFactory = new BusinessObjectFactory();
			var forwardingShipment = forwardingFactory.Load<ForwardingShipment>(shipment.PK);
			using (var testPlugIn = new SCDForwardingShipmentPlugIn(forwardingShipment))
			{
				forwardingShipment.Consols[0].JK_OA_UnpackDepotAddress = ZGuid.Empty;
				AssertEquals("Should not have allowed the change and it should have been reverted", GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, forwardingShipment.Consols[0].JK_OA_UnpackDepotAddress);
				Assert("Last message should reference Sea Cargo Depot", UnitTestUserNotification.Instance.LastMessage.Text.IndexOf("Sea Cargo Depot") != -1);
				Assert("Last message should reference Sea Cargo Depot", UnitTestUserNotification.Instance.LastMessage.WasError);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				forwardingShipment.Consols[0].Containers[0].Logs.AddNew(AutoEvents.SeaCargoDepotEvent, DepotEvents.ImpendingCargoCancelled);
				forwardingShipment.Consols[0].JK_OA_UnpackDepotAddress = ZGuid.Empty;
				AssertEquals("Should have allowed the change as Impending Arrival was cancelled", ZGuid.Empty, forwardingShipment.Consols[0].JK_OA_UnpackDepotAddress);
				Assert("No messages should have been shown", UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}
	}
}
