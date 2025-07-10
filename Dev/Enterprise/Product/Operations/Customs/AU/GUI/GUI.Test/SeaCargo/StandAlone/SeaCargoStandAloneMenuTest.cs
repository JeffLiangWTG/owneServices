using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class SeaCargoStandAloneMenuTest : TestCaseWithFactory
	{
		public void TestSeaCargoStandAloneMenu()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var manager = new CusSCAOceanBillMessageManager(oceanBill);
			using (var menu = new SeaCargoStandAloneMenu(manager))
			{
				menu.ShowPopupMenu();
				CombineAssertions(() =>
				{
					AssertEquals("MenuItems[0].Text", "Send &Underbond Requests", menu.MenuItems[0].Text);
					AssertEquals("MenuItems[1].Text", "&Send Message(s)", menu.MenuItems[1].Text);
					AssertEquals("MenuItems[2].Text", "&Amend Message(s)", menu.MenuItems[2].Text);
					AssertEquals("MenuItems[3].Text", "&Withdraw Message(s)", menu.MenuItems[3].Text);
					AssertEquals("MenuItems[4].Text", "&Reset to Original", menu.MenuItems[4].Text);
					AssertEquals("MenuItems[5].Text", "Messaging Problems? Click for HELP.", menu.MenuItems[5].Text);
					AssertEquals("MenuItems[6].Text", "Schedule Out-of-Hours Original Message Sending", menu.MenuItems[6].Text);
					AssertEquals("MenuItems[7].Text", "-", menu.MenuItems[7].Text);
					AssertEquals("MenuItems[8].Text", "Create Contingency Data", menu.MenuItems[8].Text);
					AssertEquals("MenuItems[9].Text", "Export Sea Cargo Containers Data", menu.MenuItems[9].Text);
				});
				AssertEquals("MenuItems.Count", 10, menu.MenuItems.Count);
			}
		}

		public void TestSendUnderbondRequests()
		{
			Common.AU.CMR.Testing.CMRUtilitiesTest.SetupCertificates();
			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "67094168242");
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "OB1";
			oceanBill.CB_PrincipalID = "67094168242";
			oceanBill.CB_RL_NKPortOfLoading = "NZAKL";
			oceanBill.CB_RL_NKPortOfDischarge = "AUSYD";
			oceanBill.CB_LloydsIMO = "8811924";
			oceanBill.CB_Voyage = "AA123";
			var container = oceanBill.Containers.AddNew();
			var underbond = ((ICusUnderbondDependentCollectionParent)container).Underbonds.AddNew();
			underbond.C4_OriginPremiseID = "9532M";
			underbond.C4_DestinationPremiseID = "DP41B";
			var manager = new CusSCAOceanBillMessageManager(oceanBill);
			using (var menu = new SeaCargoStandAloneMenu(manager))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.ShowPopupMenu();
				var sendUnderbondRequestsMenuItem = menu.MenuItems.FindByText("Send Underbond Requests");
				sendUnderbondRequestsMenuItem.PerformClick();
				AssertEquals("1 original message has been generated.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConcurrencyOnUnderbondPreAlert()
		{
			Factory.RefreshEnabled = false;
			var oceanBill = Factory.New<CusSCAOceanBill>();
			Factory.Save();
			var mockManager = new Mock<CusSCAOceanBillMessageManager>(oceanBill) { CallBase = true };
			mockManager.Setup(m => m.SendOriginalMessages(It.IsAny<Business.ISendsMessagesToCustoms>()))
				.Returns(new[] { Factory.NewWithValidTestData<EDIMessage>() });
			var messageManager = mockManager.Object;
			using (var menu = new SeaCargoStandAloneMenu(messageManager))
			{
				menu.ShowPopupMenu();
				var underbondMenuItem = SeaCargoShipmentMenuTest.GetMenuItemByName(menu, "Send &Underbond Requests");
				AssertNotNull("Failed to get Menu Item", underbondMenuItem);
				var factory2 = new BusinessObjectFactory();
				factory2.RefreshEnabled = false;
				var oceanBillReloaded = factory2.Load<CusSCAOceanBill>(oceanBill.PK);
				oceanBillReloaded.CB_OceanBill = "AAA";
				factory2.Save();
				oceanBill.CB_OceanBill = "BBB";
				underbondMenuItem.PerformClick();
				Assert("OceanBill should still have changes and no exception thrown", oceanBill.HasChanges);
			}
		}

		public void TestScheduleOriginalSendingClick()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_DateOfArrival = ZDateTime.Now.AddDays(4);
			oceanBill.CB_OceanBill = "OB002938";
			var manager = new CusSCAOceanBillMessageManager(oceanBill);
			using (var menu = new SeaCargoStandAloneMenu(manager))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.OnPopup(EventArgs.Empty);
				var scheduleOriginalSendingMenuItem = menu.MenuItems.FindByText("Schedule Out-of-Hours Original Message Sending");
				AssertNotNull("Failed to get Menu Item", scheduleOriginalSendingMenuItem);
				scheduleOriginalSendingMenuItem.PerformClick();
				AssertEquals(3, UnitTestUserNotification.Instance.PreviousMessages.Length);
				Assert(string.IsNullOrEmpty(UnitTestUserNotification.Instance.PreviousMessages[2].Text));
				Assert(UnitTestUserNotification.Instance.PreviousMessages[1].Text.Contains("It is likely that your message(s) will be rejected by Customs"));
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("Message sending scheduled"));
				oceanBill.SendSEACRMutex.Lock();
				try
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					scheduleOriginalSendingMenuItem.PerformClick();
					AssertEquals(2, UnitTestUserNotification.Instance.PreviousMessages.Length);
					Assert(string.IsNullOrEmpty(UnitTestUserNotification.Instance.PreviousMessages[1].Text));
					AssertEquals(string.Format("Messages are currently being generated and sent by {0} for this master, please try again later.", GlbStaff.CurrentUser.GS_FullName), UnitTestUserNotification.Instance.LastMessage.Text);
				}
				finally
				{
					oceanBill.SendSEACRMutex.Unlock();
				}
			}
		}

		public void TestScheduleOriginalSendingClickWithoutCustomsRegistrationNumber()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_DateOfArrival = ZDateTime.Now.AddDays(4);
			oceanBill.CB_OceanBill = "GAZ054928";
			var manager = new CusSCAOceanBillMessageManager(oceanBill);
			using (var menu = new SeaCargoStandAloneMenu(manager))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.OnPopup(EventArgs.Empty);
				var scheduleOriginalSendingMenuItem = menu.MenuItems.FindByText("Schedule Out-of-Hours Original Message Sending");
				AssertNotNull("Failed to get Menu Item", scheduleOriginalSendingMenuItem);
				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = ZString.Empty;
				Factory.Save();
				scheduleOriginalSendingMenuItem.PerformClick();
				AssertEquals("Please enter a Customs Registration Number before sending CMR messages", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAddCargoMessagingEventIfRequiredWhenScheduleMessagesClicked()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_DateOfArrival = ZDateTime.Now.AddDays(4);
			oceanBill.CB_OceanBill = "GAZ054928";
			var manager = new CusSCAOceanBillMessageManager(oceanBill);
			using (var menu = new SeaCargoStandAloneMenu(manager))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.OnPopup(EventArgs.Empty);
				var scheduleOriginalSendingMenuItem = menu.MenuItems.FindByText("Schedule Out-of-Hours Original Message Sending");
				AssertNotNull("Failed to get Menu Item", scheduleOriginalSendingMenuItem);
				scheduleOriginalSendingMenuItem.PerformClick();
				AssertEquals("Scheduled Event should have been posted", 1, new LogsForNominatedEvent(oceanBill.GetLogs(), AutoEvents.DeferredScheduledMessage).Count);
				AssertEquals("Messaging Event should not have been posted", 0, new LogsForNominatedEvent(oceanBill.GetLogs(), AutoEvents.HVLVReady).Count);
			}

			oceanBill.CancelDeferredScheduledMessageLogs();
		}
	}
}
