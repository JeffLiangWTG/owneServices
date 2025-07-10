using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	class AirCargoMasterMenuTest : TestCaseWithFactory
	{
		public void TestSendMessagesClick()
		{
			MasterBill.CM_ArrivalDate = ZDateTime.Now.AddDays(4);
			using (var menu = new AirCargoMasterMenuForTest(MasterBill, new CusMAWBMessageManager(() => MasterBill)))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.OnPopup(EventArgs.Empty);
				menu.SendMessagesInternal.PerformClick();
				AssertEquals(2, UnitTestUserNotification.Instance.PreviousMessages.Length);
				Assert(string.IsNullOrEmpty(UnitTestUserNotification.Instance.PreviousMessages[1].Text));
				AssertEquals("There is nothing available for sending", UnitTestUserNotification.Instance.LastMessage.Text);
				MasterBill.SendAIRCRMutex.Lock();
				try
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menu.SendMessagesInternal.PerformClick();
					AssertEquals(2, UnitTestUserNotification.Instance.PreviousMessages.Length);
					Assert(string.IsNullOrEmpty(UnitTestUserNotification.Instance.PreviousMessages[1].Text));
					AssertEquals(string.Format("Messages are currently being generated and sent by {0} for this master, please try again later.", GlbStaff.CurrentUser.GS_FullName), UnitTestUserNotification.Instance.LastMessage.Text);
				}
				finally
				{
					MasterBill.SendAIRCRMutex.Unlock();
				}

				AUCustomsDataRegistry.Instance.SendAirCargoMessagesInBackGroundDefault.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				MasterBill.CM_ArrivalDate = ZDateTime.Empty;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				menu.SendMessagesInternal.PerformClick();
				AssertEquals(3, UnitTestUserNotification.Instance.PreviousMessages.Length);
				Assert(string.IsNullOrEmpty(UnitTestUserNotification.Instance.PreviousMessages[2].Text));
				AssertEquals("Do you wish to schedule the generation and sending of cargo reports for this job in the background?", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				AssertEquals("There is nothing available for sending", UnitTestUserNotification.Instance.LastMessage.Text);
				MasterBill.CM_ArrivalDate = ZDateTime.Now.AddDays(4);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.SendMessagesInternal.PerformClick();
				AssertEquals(3, UnitTestUserNotification.Instance.PreviousMessages.Length);
				Assert(string.IsNullOrEmpty(UnitTestUserNotification.Instance.PreviousMessages[2].Text));
				Assert(UnitTestUserNotification.Instance.PreviousMessages[1].Text.Contains("Do you wish to schedule the generation and sending of cargo reports for this job in the background?"));
				Assert(UnitTestUserNotification.Instance.PreviousMessages[0].Text.Contains("Please fix these errors before sending any messages"));
				MasterBill.CM_MAWB = "08111111111";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.SendMessagesInternal.PerformClick();
				AssertEquals(3, UnitTestUserNotification.Instance.PreviousMessages.Length);
				Assert(string.IsNullOrEmpty(UnitTestUserNotification.Instance.PreviousMessages[2].Text));
				Assert(UnitTestUserNotification.Instance.PreviousMessages[1].Text.Contains("Do you wish to schedule the generation and sending of cargo reports for this job in the background?"));
				Assert(UnitTestUserNotification.Instance.PreviousMessages[0].Text.Contains("It is likely that your message(s) will be rejected by Customs"));
				MasterBill.AddDeferredScheduledMessagesEventAndConsolCargoMessagingEvent(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.SendMessagesInternal.PerformClick();
				AssertEquals(3, UnitTestUserNotification.Instance.PreviousMessages.Length);
				Assert(string.IsNullOrEmpty(UnitTestUserNotification.Instance.PreviousMessages[2].Text));
				Assert(UnitTestUserNotification.Instance.PreviousMessages[1].Text.Contains("Do you wish to schedule the generation and sending of cargo reports for this job in the background?"));
				Assert(UnitTestUserNotification.Instance.PreviousMessages[0].Text.Contains("Deferred sending of messages has already been scheduled"));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.SendMessagesInternal.PerformClick();
				AssertEquals(4, UnitTestUserNotification.Instance.PreviousMessages.Length);
				Assert(string.IsNullOrEmpty(UnitTestUserNotification.Instance.PreviousMessages[3].Text));
				Assert(UnitTestUserNotification.Instance.PreviousMessages[2].Text.Contains("Do you wish to schedule the generation and sending of cargo reports for this job in the background?"));
				Assert(UnitTestUserNotification.Instance.PreviousMessages[1].Text.Contains("Deferred sending of messages has already been scheduled"));
				Assert(UnitTestUserNotification.Instance.PreviousMessages[0].Text.Contains("It is likely that your message(s) will be rejected by Customs"));
				MasterBill.CancelDeferredScheduledMessageLogs(CusMAWBBase.AirCargoReportLogReference);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.SendMessagesInternal.PerformClick();
				AssertEquals(4, UnitTestUserNotification.Instance.PreviousMessages.Length);
				Assert(string.IsNullOrEmpty(UnitTestUserNotification.Instance.PreviousMessages[3].Text));
				Assert(UnitTestUserNotification.Instance.PreviousMessages[2].Text.Contains("Do you wish to schedule the generation and sending of cargo reports for this job in the background?"));
				Assert(UnitTestUserNotification.Instance.PreviousMessages[1].Text.Contains("It is likely that your message(s) will be rejected by Customs"));
				Assert(UnitTestUserNotification.Instance.PreviousMessages[0].Text.Contains("Message sending scheduled"));
			}
		}

		public void TestScheduleOriginalSendingClick()
		{
			MasterBill.CM_ArrivalDate = ZDateTime.Now.AddDays(4);
			MasterBill.CM_MAWB = "08111111111";
			using (var menu = GetMenuToTest(MasterBill, new CusMAWBMessageManager(() => MasterBill)))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.OnPopup(EventArgs.Empty);
				menu.scheduleOriginalSendingMenuItem.PerformClick();
				AssertEquals(3, UnitTestUserNotification.Instance.PreviousMessages.Length);
				Assert(string.IsNullOrEmpty(UnitTestUserNotification.Instance.PreviousMessages[2].Text));
				Assert(UnitTestUserNotification.Instance.PreviousMessages[1].Text.Contains("It is likely that your message(s) will be rejected by Customs"));
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("Message sending scheduled"));
				MasterBill.SendAIRCRMutex.Lock();
				try
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menu.scheduleOriginalSendingMenuItem.PerformClick();
					AssertEquals(2, UnitTestUserNotification.Instance.PreviousMessages.Length);
					Assert(string.IsNullOrEmpty(UnitTestUserNotification.Instance.PreviousMessages[1].Text));
					AssertEquals(string.Format("Messages are currently being generated and sent by {0} for this master, please try again later.", GlbStaff.CurrentUser.GS_FullName), UnitTestUserNotification.Instance.LastMessage.Text);
				}
				finally
				{
					MasterBill.SendAIRCRMutex.Unlock();
				}
			}
		}

		public void TestScheduleOriginalSendingClickWithoutCustomsRegistrationNumber()
		{
			MasterBill.CM_ArrivalDate = ZDateTime.Now.AddDays(4);
			MasterBill.CM_MAWB = "08111111111";
			using (var menu = GetMenuToTest(MasterBill, new CusMAWBMessageManager(() => MasterBill)))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.OnPopup(EventArgs.Empty);
				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = ZString.Empty;
				Factory.Save();
				menu.scheduleOriginalSendingMenuItem.PerformClick();
				AssertEquals("Please enter a Customs Registration Number before sending CMR messages", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAddConsolCargoMessagingEventIfRequiredWhenScheduleMessagesClicked()
		{
			MasterBill.CM_ArrivalDate = ZDateTime.Now.AddDays(4);
			MasterBill.CM_MAWB = "08111111111";
			using (var menu = GetMenuToTest(MasterBill, new CusMAWBMessageManager(() => MasterBill)))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.OnPopup(EventArgs.Empty);
				menu.scheduleOriginalSendingMenuItem.PerformClick();
				AssertEquals("Scheduled Event should have been posted", 1, new LogsForNominatedEvent(MasterBill.GetLogs(), AutoEvents.DeferredScheduledMessage).Count);
				AssertEquals("Messaging Event should not have been posted", 0, new LogsForNominatedEvent(masterShipment.GetLogs(), AutoEvents.HVLVReady).Count);
			}

			MasterBill.CancelDeferredScheduledMessageLogs(CusMAWBBase.AirCargoReportLogReference);
			MasterBill.CM_JK = consol.PK;
			using (var menu = GetMenuToTest(MasterBill, new CusMAWBMessageManager(() => MasterBill)))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.OnPopup(EventArgs.Empty);
				menu.scheduleOriginalSendingMenuItem.PerformClick();
				AssertEquals("Scheduled Event should have been posted", 1, new LogsForNominatedEvent(MasterBill.GetLogs(), AutoEvents.DeferredScheduledMessage).Count);
				AssertEquals("Messaging Event should not have been posted", 0, new LogsForNominatedEvent(masterShipment.GetLogs(), AutoEvents.HVLVReady).Count);
			}

			MasterBill.CancelDeferredScheduledMessageLogs(CusMAWBBase.AirCargoReportLogReference);
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;
			using (var menu = GetMenuToTest(MasterBill, new CusMAWBMessageManager(() => MasterBill)))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.OnPopup(EventArgs.Empty);
				menu.scheduleOriginalSendingMenuItem.PerformClick();
				AssertEquals("Scheduled Event should have been posted", 1, new LogsForNominatedEvent(MasterBill.GetLogs(), AutoEvents.DeferredScheduledMessage).Count);
			}
		}

		public void TestVisibility_ForMAWB()
		{
			using (var menu = new AirCargoMasterMenuForTest(MasterBill, new CusMAWBMessageManager(() => MasterBill)))
			{
				menu.OnPopup(EventArgs.Empty);
				AssertEquals("SendMessages.Visible", true, menu.SendMessagesInternal.Visible);
				AssertEquals("WithdrawMessages.Visible", true, menu.WithdrawMessagesInternal.Visible);
				AssertEquals("ResetToOriginal.Visible", true, menu.ResetToOriginalInternal.Visible);
				AssertEquals("ScheduleOriginlaSendingMenuItem.Visible", true, menu.scheduleOriginalSendingMenuItem.Visible);
				AssertEquals("AirCargoReportContingencyMenuItem.Visible", true, menu.airCargoReportContingencyMenuItem.Visible);
				Assert("RefreshAirCargoDataMenuItem is visible", menu.refreshAirCargoDataMenuItem.Visible);
				AssertNotNull("HouseMenu", menu.houseMenu);
			}
		}

		public void TestVisibility_ForConsol()
		{
			using (var menu = new AirCargoMasterMenuForTest(consol, new CusMAWBMessageManager(null)))
			{
				menu.OnPopup(EventArgs.Empty);
				AssertEquals("SendMessages.Visible", true, menu.SendMessagesInternal.Visible);
				AssertEquals("WithdrawMessages.Visible", true, menu.WithdrawMessagesInternal.Visible);
				AssertEquals("ResetToOriginal.Visible", true, menu.ResetToOriginalInternal.Visible);
				AssertEquals("ScheduleOriginlaSendingMenuItem.Visible", true, menu.scheduleOriginalSendingMenuItem.Visible);
				AssertEquals("AirCargoReportContingencyMenuItem.Visible", true, menu.airCargoReportContingencyMenuItem.Visible);
				Assert("RefreshAirCargoDataMenuItem is invisible", !menu.refreshAirCargoDataMenuItem.Visible);
				AssertNull("HouseMenu", menu.houseMenu);
			}
		}

		public void TestHouseMenuText()
		{
			CusHAWB houseBill = MasterBill.ChildBills.AddNew();
			houseBill.CS_HAWB = "HAWB";
			using (var menu = GetMenuToTest(MasterBill, new CusMAWBMessageManager(() => MasterBill)))
			{
				menu.OnPopup(EventArgs.Empty);
				MasterBill.CurrentHouseBill = null;
				menu.OnPopup(EventArgs.Empty);
				AssertEquals("HAWB Level", menu.houseMenu.Text);
				MasterBill.CurrentHouseBill = houseBill;
				menu.OnPopup(EventArgs.Empty);
				AssertEquals("House Bill:HAWB", menu.houseMenu.Text);
			}
		}

		[ExpectNoExceptions]
		public void TestOnPopupWhenCurrentHouseBillDeleted()
		{
			CusHAWB houseBill = MasterBill.ChildBills.AddNew();
			houseBill.CS_HAWB = "HAWB";
			using (var menu = GetMenuToTest(MasterBill, new CusMAWBMessageManager(() => MasterBill)))
			{
				MasterBill.CurrentHouseBill = houseBill;
				houseBill.Delete();
				menu.OnPopup(EventArgs.Empty);
			}
		}

		public void TestOverriddenNewMAWBDelegates()
		{
			CusMAWB masterbill = Factory.New<CusMAWB>();
			AirCargoMasterMenuWithRegisterThisTypeOverrideForTest.RegisterThisTypeOverride();
			using (AirCargoMasterMenu airCargoMasterMenu = AirCargoMasterMenu.New(masterbill, new CusMAWBMessageManager(() => masterBill)))
			{
				AssertEquals(typeof(AirCargoMasterMenuWithRegisterThisTypeOverrideForTest), airCargoMasterMenu.GetType());
			}
		}

		public void TestOverriddenNewConsolDelegates()
		{
			CusMAWB masterbill = Factory.New<CusMAWB>();
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			AirCargoMasterMenuWithRegisterThisTypeOverrideForTest.RegisterThisTypeOverride();
			using (AirCargoMasterMenu airCargoMasterMenu = AirCargoMasterMenu.New(consol, new CusMAWBMessageManager(() => masterBill)))
			{
				AssertEquals(typeof(AirCargoMasterMenuWithRegisterThisTypeOverrideForTest), airCargoMasterMenu.GetType());
			}
		}

		protected virtual AirCargoMasterMenu GetMenuToTest(CusMAWB cusMAWB, CusMAWBMessageManager manager) => new AirCargoMasterMenu(cusMAWB, manager);

		protected virtual AirCargoMasterMenu GetMenuToTest(ForwardingConsol consol, CusMAWBMessageManager manager) => new AirCargoMasterMenu(consol, manager);

		protected CusMAWB MasterBill
		{
			get
			{
				if (masterBill == null)
				{
					var consol = Factory.New<ForwardingConsol>();
					masterBill = CusMAWB.CreateNew(consol);
				}

				return masterBill;
			}
		}

		CusMAWB masterBill;
		protected ForwardingConsol consol;
		protected CommonShipment masterShipment;
		protected CommonShipment coLoadShipment;
		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = "AUSYD";
			coLoadShipment = consol.Shipments.AddNew();
			masterShipment = consol.Shipments.AddNew();
			coLoadShipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			masterShipment.CoLoadShipments.Add(coLoadShipment);
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			Factory.Save();
		}

		protected void PopulateDataForSendingMessage(ForwardingConsol consol)
		{
			Transport transport = consol.Transports[0];
			transport.JW_VoyageFlight = "QF2";
			transport.JW_ETA = new ZDateTime(2004, 3, 22);
			transport.JW_RL_NKLoadPort = "NZAKL";
			transport.JW_RL_NKDiscPort = "AUSYD";
			consol.JK_MasterBillNum = "08178945101";
			int index = 0;
			foreach (CommonShipment shipment in consol.Shipments) //There are two shipments already set. one is master/the other is sub-master
			{
				shipment.JS_HouseBill = "H" + index++;
				shipment.JS_ActualWeight = 2;
				shipment.JS_UnitOfWeight = "KG";
				shipment.JS_OuterPacks = 10;
				ZQuery consigneeFilter = new ZQuery(OrgHeaderSchema.OH_IsConsignee, true);
				consigneeFilter.AddToFilter(JoinCondition.And, OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.StartsWith, "AU");
				ZQuery consignorFilter = new ZQuery(OrgHeaderSchema.OH_IsConsignor, true);
				consignorFilter.AddToFilter(JoinCondition.And, OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.DoesNotStartWith, "AU");
				shipment.ConsigneePK = shipment.Factory.LoadTop1<OrgHeader>(consigneeFilter).PK;
				shipment.ConsignorPK = shipment.Factory.LoadTop1<OrgHeader>(consignorFilter).PK;
				shipment.JS_GoodsDescription = "Description";
				shipment.JS_GoodsValue = 1000m;
				shipment.JS_RX_NKGoodsValueCurr = "USD";
				shipment.JS_INCO = Core.Constants.IncoTerms.DefaultIncoFromPaymentType(Core.Constants.PaymentType.Prepaid);
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "AUSYD";
			}

			consol.Factory.Save();
		}

		sealed class AirCargoMasterMenuWithRegisterThisTypeOverrideForTest : AirCargoMasterMenu
		{
			internal AirCargoMasterMenuWithRegisterThisTypeOverrideForTest(CusMAWB masterBill, CusMAWBMessageManager manager) : base(masterBill, manager)
			{
			}

			internal AirCargoMasterMenuWithRegisterThisTypeOverrideForTest(ForwardingConsol consol, CusMAWBMessageManager manager) : base(consol, manager)
			{
			}

			public static void RegisterThisTypeOverride()
			{
				OverridableNewMAWBDelegate.Value = new NewMAWBDelegate(OverriddenNewMAWB);
				OverridableNewConsolDelegate.Value = new NewConsolDelegate(OverriddenNewConsol);
			}

			static AirCargoMasterMenu OverriddenNewMAWB(CusMAWB masterBill, CusMAWBMessageManager manager) => new AirCargoMasterMenuWithRegisterThisTypeOverrideForTest(masterBill, manager);

			static AirCargoMasterMenu OverriddenNewConsol(ForwardingConsol consol, CusMAWBMessageManager manager) => new AirCargoMasterMenuWithRegisterThisTypeOverrideForTest(consol, manager);
		}

		sealed class AirCargoMasterMenuForTest : AirCargoMasterMenu
		{
			internal AirCargoMasterMenuForTest(CusMAWB masterBill, CusMAWBMessageManager manager)
				: base(masterBill, manager)
			{
			}

			internal AirCargoMasterMenuForTest(ForwardingConsol consol, CusMAWBMessageManager manager) : base(consol, manager)
			{
			}

			internal MenuItem SendMessagesInternal => sendMessages;

			internal MenuItem WithdrawMessagesInternal => withdrawMessages;

			internal MenuItem ResetToOriginalInternal => resetToOriginal;
		}
	}
}
