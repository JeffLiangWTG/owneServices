using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class SCDForwardingConsolPlugInTest : SCDForwardingPlugInTest
	{
		public void TestGetBusinessEntityDoesNotFailOnWithDeletedHost()
		{
			var forwardingFactory = new BusinessObjectFactory();
			var forwardingConsol = forwardingFactory.New<ForwardingConsol>();
			using (var testPlugIn = new SCDForwardingConsolPlugIn(forwardingConsol))
			{
				forwardingConsol.Delete();
				AssertNull(testPlugIn.BusinessEntity);
			}
		}

		public void TestChangeConsolUnpackDepotWithImpendingContainers()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var loadList = GetConsolWithContainer();
			loadList.Containers[0].Logs.AddNew(AutoEvents.SeaCargoDepotEvent, DepotEvents.ImpendingCargo, ZDateTimeOffset.Now.AddHours(-1));
			Factory.Save();
			var forwardingFactory = new BusinessObjectFactory();
			var forwardingConsol = forwardingFactory.Load<ForwardingConsol>(loadList.PK);
			using (new SCDForwardingConsolPlugIn(forwardingConsol))
			{
				forwardingConsol.JK_OA_UnpackDepotAddress = ZGuid.Empty;
				AssertEquals("Should not have allowed the change and it should have been reverted", GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, forwardingConsol.JK_OA_UnpackDepotAddress);
				Assert("Last message should reference Sea Cargo Depot", UnitTestUserNotification.Instance.LastMessage.Text.IndexOf("Sea Cargo Depot") != -1);
				Assert("Last message should reference Sea Cargo Depot", UnitTestUserNotification.Instance.LastMessage.WasError);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				forwardingConsol.Containers[0].Logs.AddNew(AutoEvents.SeaCargoDepotEvent, DepotEvents.ImpendingCargoCancelled);
				forwardingConsol.JK_OA_UnpackDepotAddress = ZGuid.Empty;
				AssertEquals("Should have allowed the change as Impending Arrival was cancelled", ZGuid.Empty, forwardingConsol.JK_OA_UnpackDepotAddress);
				Assert("No messages should have been shown", UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestChangeConsolUnpackDepotWithCargoArrival()
		{
			var loadList = GetConsolWithContainer();
			loadList.Containers[0].Logs.AddNew(AutoEvents.SeaCargoDepotEvent, DepotEvents.CargoArrived);
			Factory.Save();
			var forwardingFactory = new BusinessObjectFactory();
			var forwardingConsol = forwardingFactory.Load<ForwardingConsol>(loadList.PK);
			using (var testPlugIn = new SCDForwardingConsolPlugIn(forwardingConsol))
			{
				forwardingConsol.JK_OA_UnpackDepotAddress = ZGuid.Empty;
				AssertEquals("Should not have allowed the change and it should have been reverted", GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, forwardingConsol.JK_OA_UnpackDepotAddress);
				Assert("Last message should reference Sea Cargo Depot", UnitTestUserNotification.Instance.LastMessage.Text.IndexOf("Sea Cargo Depot") != -1);
				Assert("Last message should reference Sea Cargo Depot", UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		public void TestChangeConsolUnpackDepotWithOutturnComplete()
		{
			var loadList = GetConsolWithContainer();
			var unpackedShipment = loadList.Shipments.AddNew();
			loadList.Containers[0].Logs.AddNew(AutoEvents.SeaCargoDepotEvent, DepotEvents.CargoUnpacked);
			Factory.Save();
			var forwardingFactory = new BusinessObjectFactory();
			var forwardingConsol = forwardingFactory.Load<ForwardingConsol>(loadList.PK);
			using (var testPlugIn = new SCDForwardingConsolPlugIn(forwardingConsol))
			{
				forwardingConsol.JK_OA_UnpackDepotAddress = ZGuid.Empty;
				AssertEquals("Should not have allowed the change and it should have been reverted", GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, forwardingConsol.JK_OA_UnpackDepotAddress);
				Assert("Last message should reference Sea Cargo Depot", UnitTestUserNotification.Instance.LastMessage.Text.IndexOf("Sea Cargo Depot") != -1);
				Assert("Last message should reference Sea Cargo Depot", UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}
	}
}
