using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class HouseBillAndCloseMessageSendingExtensionTest : TestCaseWithFactory
	{
		public void TestOnlyOneMessageIsSent_When_NeedToAutoSendWithdrawCloseMessage_And_NeedToAutoSendAmendHouseBillMessages_AreTrue()
		{
			var needToAutoSendWithdrawCloseMessageMethod = typeof(HouseBillAndCloseMessageSendingExtension).GetMethod("NeedToAutoSendWithdrawCloseMessage", BindingFlags.NonPublic | BindingFlags.Static);
			var needToAutoSendAmendHouseBillMessagesMethod = typeof(HouseBillAndCloseMessageSendingExtension).GetMethod("NeedToAutoSendAmendHouseBillMessages", BindingFlags.NonPublic | BindingFlags.Static);

			var master = Factory.New<CusCAeMHMaster>();
			Factory.Save();
			using (var form = new CusCAeMHMasterForm(master))
			{
				form.Show();
				AssertNull("Message", UnitTestUserNotification.Instance.LastMessage.Text);
				CreateOutgoingWithdrawMessage(master);
				Factory.Save();
				AssertEquals("Count", 3, master.Messages.Count);

				master.BP_MasterHouseCCN = "CCN";
				master.BP_PrimaryCCN = "CCN";
				var houseBill = master.HouseBills.AddNew();
				houseBill.BW_CustomsStatus = "CLR";
				houseBill.BW_Weight = 1;
				var needToAutoSendWithdrawCloseMessage = (ZBool)needToAutoSendWithdrawCloseMessageMethod.Invoke(null, new object[] { master });
				Assert(needToAutoSendWithdrawCloseMessage);
				var needToAutoSendAmendHouseBillMessages = (ZBool)needToAutoSendAmendHouseBillMessagesMethod.Invoke(null, new object[] { master });
				Assert(needToAutoSendAmendHouseBillMessages);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.FireSaveButton();
				master.Messages.Reload(false);
				AssertEquals("Count", 4, master.Messages.Count);
				var newFactory = new BusinessObjectFactory();
				var master1 = newFactory.Load<CusCAeMHMaster>(master.PK);
				AssertEquals("Count", 4, master1.Messages.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				master.BP_MasterHouseCCN = "CCN1";
				form.FireSaveButton();
				master.Messages.Reload(false);
				AssertEquals("Count", 5, master.Messages.Count);
				master1.Messages.Reload(false);
				AssertEquals("Count", 5, master1.Messages.Count);
			}
		}

		public void TestNeedToAutoSendWithdrawCloseMessage()
		{
			var methodInfo = typeof(HouseBillAndCloseMessageSendingExtension).GetMethod("NeedToAutoSendWithdrawCloseMessage", BindingFlags.NonPublic | BindingFlags.Static);
			var master = Factory.New<CusCAeMHMaster>();
			var actualValue = (ZBool)methodInfo.Invoke(null, new object[] { master });
			Assert("NeedToAutoSendWithdrawCloseMessage", !actualValue);
			CreateOutgoingWithdrawMessage(master);
			Factory.Save();
			master.BP_MasterHouseCCN = "CCN";
			actualValue = (ZBool)methodInfo.Invoke(null, new object[] { master });
			Assert("NeedToAutoSendWithdrawCloseMessage", actualValue);
		}

		public void TestShowAutoSendingWithdrawalMessageDialog()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();
			house.BW_Weight = 1;
			Factory.Save();
			using (var form = new CusCAeMHMasterForm(master))
			{
				form.Show();
				master.BP_MasterHouseCCN = "1234";
				AssertNull("Message", UnitTestUserNotification.Instance.LastMessage.Text);
				master.BP_CBSACarrierCode = "FFFF";
				AssertNull("Message", UnitTestUserNotification.Instance.LastMessage.Text);
				CreateOutgoingWithdrawMessage(master);
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				master.BP_MasterHouseCCN = "5678";
				form.FireSaveButton();
				AssertEquals("Message", "1 cancel message(s) have been generated.", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
				AssertEquals("Message", HouseBillAndCloseMessageSendingExtension.CloseMessageHasBeenSubmittedMessage, UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				master.Messages.Reload(false);
				AssertEquals("Count", 4, master.Messages.Count);
				Assert("MessageText", master.Messages[3].EM_MessageText.Contains("BGM+87+7000007531+1'"));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				master.BP_CBSACarrierCode = "CCCC";
				form.FireSaveButton();
				AssertEquals("Message", "1 cancel message(s) have been generated.", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
				AssertEquals("Message", HouseBillAndCloseMessageSendingExtension.CloseMessageHasBeenSubmittedMessage, UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				master.Messages.Reload(false);
				AssertEquals("Count", 5, master.Messages.Count);
				var closeMessage = master.Messages[4] as ACIForwarderCloseMessage;
				AssertNotNull("ACIForwarderCloseMessage", closeMessage);
				Assert("MessageText", closeMessage.EM_MessageText.Contains("NAD+FW+1020'"));
				Assert("NeedAutoCloseReport", closeMessage.NeedAutoCloseReport);
			}
		}

		public void TestNeedToAutoSendAmendHouseBillMessages()
		{
			var methodInfo = typeof(HouseBillAndCloseMessageSendingExtension).GetMethod("NeedToAutoSendAmendHouseBillMessages", BindingFlags.NonPublic | BindingFlags.Static);
			var master = Factory.New<CusCAeMHMaster>();
			var actualValue = (ZBool)methodInfo.Invoke(null, new object[] { master });
			Assert("NeedToAutoSendAmendHouseBillMessages", !actualValue);
			Factory.Save();
			master.BP_PrimaryCCN = "CCN";
			var houseBill = master.HouseBills.AddNew();
			houseBill.BW_CustomsStatus = "CLR";
			actualValue = (ZBool)methodInfo.Invoke(null, new object[] { master });
			Assert("NeedToAutoSendAmendHouseBillMessages", actualValue);
		}

		public void TestShowAutoSendingAmendHouseBillsMessageDialog()
		{
			var master = Factory.New<CusCAeMHMaster>();
			Factory.Save();
			using (var form = new CusCAeMHMasterForm(master))
			{
				form.Show();
				AssertNull("Message", UnitTestUserNotification.Instance.LastMessage.Text);
				CreateOutgoingWithdrawMessage(master);
				Factory.Save();

				AssertEquals("Count", 3, master.Messages.Count);

				master.BP_PrimaryCCN = "CCN";
				var houseBill = master.HouseBills.AddNew();
				houseBill.BW_CustomsStatus = "CLR";
				houseBill.BW_Weight = 1;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.FireSaveButton();

				AssertEquals("Message before sending a message", HouseBillAndCloseMessageSendingExtension.HouseBillsHaveBeenAcceptedMessage, UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				AssertEquals("1 Message sent", "1 cancel message(s) have been generated.", UnitTestUserNotification.Instance.LastMessage.Text);

				master.Messages.Reload(false);
				AssertEquals("Count After Sending WithDrawal Message", 4, master.Messages.Count);
				var closeMessage = master.Messages[3] as ACIForwarderCloseMessage;
				AssertNotNull("ACIForwarderCloseMessage", closeMessage);
			}
		}

		void CreateOutgoingWithdrawMessage(CusCAeMHMaster master)
		{
			var outgoingMessage = master.Messages.AddNew();
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAACI;
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.ACIForwarderClose;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_MessageSubType = MessageSubTypeCodes.Codes.Cancellation;
			outgoingMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			var incomingMessage1 = Factory.New<ACIForwarderCloseMessage>();
			incomingMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAACI;
			incomingMessage1.EM_MessageType = MessageTypeList.Codes.ACIForwarderClose;
			incomingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage1.EM_Status = EDIMessage.Status.Received;
			incomingMessage1.EM_MessageSubType = MessageSubTypeCodes.Codes.Cancellation;
			incomingMessage1.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddHours(1);
			var incomingMessage2 = Factory.New<ACIForwarderCloseMessage>();
			incomingMessage1.EM_LinkedObject = master;
			incomingMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAACI;
			incomingMessage2.EM_MessageType = MessageTypeList.Codes.ACIForwarderClose;
			incomingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage2.EM_Status = EDIMessage.Status.Received;
			incomingMessage2.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddHours(1);
			incomingMessage2.EM_MessageText = @"UNH+1+GOVCBR:D:11B:UN
BGM+313+10207000007531+11
DTM+9:201407292127:203
RFF+AGO:CLS-C10001000
RCS+11
FTX+AAO+++29
GEI+5+66
ERC+463
ERP+2:973:29
UNS+D
HYN+3
UNS+S
UNT+13+1
".Replace("\r\n", "'");
			incomingMessage2.EM_LinkedObject = master;
			master.Messages.Reload(true);
			master.BP_CustomsStatus = "CLR";
		}
	}
}
