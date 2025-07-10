using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	sealed class ConsolACIMessageManagerTest : TestCaseWithFactory
	{
		public void TestSendWithMessageErrors()
		{
			Env.Security.ConsolCAeManifestSendWithMessageErrors.IsAllowed = false;
			Manager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManager };
			Manager.TopLevelBusinessObject.AddRowMessageError("TEST");
			AssertEquals("PreCondition: Top BizO has a message errors", true, Manager.TopLevelBusinessObject.HasMessageErrors);
			AssertEquals("CanSend", false, Manager.CanSendOriginal_Exposed(Sender, SingleManager));

			Env.Security.ConsolCAeManifestSendWithMessageErrors.IsAllowed = true;
			AssertEquals("PreCondition: Top BizO has a message errors", true, Manager.TopLevelBusinessObject.HasMessageErrors);
			AssertEquals("CanSend", true, Manager.CanSendOriginal_Exposed(Sender, SingleManager));
		}

		public void TestResetToOriginal_WithSecurityRight()
		{
			Env.Security.ConsolCAeManifestResetToOriginal.IsAllowed = false;
			Manager.allMessageManagers = new TestHelperSingleMessageManager[] { SingleManager };
			Sender.ReturnAllForWhichMessagesShouldWeReset = true;
			Manager.ResetToOriginal(Sender);
			string securityWarning = @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Customs Global -> CA eManifest Forwarder -> Reset to Original";
			AssertEquals(securityWarning, UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			Env.Security.ConsolCAeManifestResetToOriginal.IsAllowed = true;
			Manager.ResetToOriginal(Sender);
			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestCannotSaveWhenWaitingForResponseMessage()
		{
			OceanBill.HouseBills[0].CA_MessageStatus = "AWO";
			OceanBill.HouseBills[1].CA_MessageStatus = "AWO";
			var notifications = Manager.GetAllMessageManagers_Exposed()[0].GetNotificationsForSendingAnOriginal();
			Assert(notifications.Any(x => x.Message == ConsolACIMessageManager.ShipmentCannotSaveWhenWaitingForResponse));
		}

		public void TestAllMessageManagers()
		{
			SingleMessageManager[] allMessageManagers = Manager.GetAllMessageManagers_Exposed();
			AssertEquals("AllMessageManagers.Length", 2, allMessageManagers.Length);
			AssertEquals("AllMessageManagers[0]", typeof(SupplementaryCargoReportMessageManager), allMessageManagers[0].GetType());
			AssertEquals("AllMessageManagers[1]", typeof(SupplementaryCargoReportMessageManager), allMessageManagers[1].GetType());
		}

		public void TestAllMessageManagersWithNull()
		{
			SingleMessageManager[] allMessageManagers = ManagerWithNull.GetAllMessageManagers_Exposed();
			AssertEquals("AllMessageManagers.Length", 0, allMessageManagers.Length);
		}

		public void TestRefreshAll()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKDestination = "AUSYD";

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_RL_NKDestination = "AUSYD";

			var house1 = OceanBill.HouseBills[0];
			house1.CA_JS = shipment.PK;
			house1.isCalledFromConsolPlugin = true;
			house1.CA_RL_NK_PortOfDestination = "CAYVR";

			var house2 = OceanBill.HouseBills[1];
			house2.CA_JS = shipment2.PK;
			house2.isCalledFromConsolPlugin = true;
			var sender = new SendsMessagesToCustomsShutterUppererForTest();
			Manager.RefreshAll(sender);
			AssertEquals("details should be refreshed from shipment", "AUSYD", OceanBill.HouseBills[0].CA_RL_NK_PortOfDestination);

			OceanBill.HouseBills.DeleteAll();
			Manager.RefreshAll(sender);
			AssertEquals("nothing for refresh", 0, OceanBill.HouseBills.Count);
		}

		#region Implemenetation

		SendsMessagesToCustomsShutterUpperer sender;
		SendsMessagesToCustomsShutterUpperer Sender
		{
			get
			{
				if (sender == null)
				{
					sender = new SendsMessagesToCustomsShutterUpperer(false);
				}
				return sender;
			}
		}

		TestHelperSingleMessageManager singleManager;
		TestHelperSingleMessageManager SingleManager
		{
			get
			{
				if (singleManager == null)
				{
					singleManager = new TestHelperSingleMessageManager();
				}
				return singleManager;
			}
		}

		ConsolACIMessageManagerForTesting Manager
		{
			get { return new ConsolACIMessageManagerForTesting(() => OceanBill); }
		}

		ConsolACIMessageManagerForTesting ManagerWithNull
		{
			get { return new ConsolACIMessageManagerForTesting(null); }
		}

		CusSCAOceanBill OceanBill
		{
			get
			{
				if (oceanBill == null)
				{
					oceanBill = Factory.New<CusSCAOceanBill>();
					oceanBill.HouseBills.AddNew();
					oceanBill.HouseBills.AddNew();
				}
				return oceanBill;
			}
		}
		CusSCAOceanBill oceanBill;

		#region ConsolACIMessageManagerForTesting

		class ConsolACIMessageManagerForTesting : ConsolACIMessageManager
		{
			public ConsolACIMessageManagerForTesting(GetCusSCAOceanBillDelegate getCusSCAOceanBillDelegate)
				: base(getCusSCAOceanBillDelegate)
			{
			}

			public SingleMessageManager[] GetAllMessageManagers_Exposed()
			{
				return GetAllMessageManagers();
			}

			protected override bool SendWheneverPossibleOnceMessagingActive
			{
				get
				{
					return true;
				}
			}

			public bool CanSendOriginal_Exposed(ISendsMessagesToCustoms sender, params SingleMessageManager[] managersToSend)
			{
				return CanSendOriginal(sender, managersToSend);
			}

			public TestHelperSingleMessageManager[] allMessageManagers;
		}

		#endregion

		class SendsMessagesToCustomsShutterUppererForTest : SendsMessagesToCustomsShutterUpperer, IConsolSendsMessagesToCustoms
		{
			SingleMessageManager[] IConsolSendsMessagesToCustoms.WhichMessagesShouldWeRefresh(SingleMessageManager[] allManagers)
			{
				return allManagers;
			}
		}

		#endregion
	}
}
