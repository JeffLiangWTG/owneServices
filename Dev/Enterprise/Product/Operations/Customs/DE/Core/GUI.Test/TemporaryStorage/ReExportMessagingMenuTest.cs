using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	public class ReExportMessagingMenuTest : TestCaseWithFactory
	{
		public void TestFormAddsMenu()
		{
			var cusTempStorageDec = Factory.NewWithValidTestData<REXDISCusTempStorageDec>();
			cusTempStorageDec.STH_SJH = header.PK;
			cusTempStorageDec.STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.ReExportDispatch;

			using (var reExportForm = new TemporaryStorageForm(header))
			{
				var topLevelMenu = reExportForm.Menu.MenuItems.FindByText("Messages");
				AssertNotNull(topLevelMenu);
				var sendReExportMenu = topLevelMenu.MenuItems.FindByText("Send Re-Export");
				AssertNotNull(sendReExportMenu);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				sendReExportMenu.PerformClick();
				AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(header.HasChanges);
				AssertEquals(0, header.REXDISCusTempStorageDec.Messages.Count);
			}
		}

		public void TestSendReExportMessage()
		{
			using (var reExportForm = new TemporaryStorageForm(header))
			{
				var topLevelMenu = reExportForm.Menu.MenuItems.FindByText("Messages");
				var sendReExportMenu = topLevelMenu.MenuItems.FindByText("Send Re-Export");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				Factory.Save();
				sendReExportMenu.PerformClick();
				AssertEquals("No declaration", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("No Declaration has been created."));

				UnitTestUserNotification.Instance.ClearMessages();
				var rexdisStorageDec = REXDISCusTempStorageDec.New(header);
				Factory.Save();
				sendReExportMenu.PerformClick();
				AssertEquals("No lines", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Declaration has no lines, message cannot be created"));

				UnitTestUserNotification.Instance.ClearMessages();
				rexdisStorageDec.CusTempStorageLines.AddNew();
				Environment.Env.Security.CustomsTemporaryStorageSendWithMessageErrors.IsAllowed = false;
				Factory.Save();
				sendReExportMenu.PerformClick();
				AssertEquals("Has message errors and send with message errors is not allowed", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Please fix the following message errors before sending any messages"));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				Environment.Env.Security.CustomsTemporaryStorageSendWithMessageErrors.IsAllowed = true;
				sendReExportMenu.PerformClick();
				AssertEquals("Has message errors and send with message errors is allowed", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("It is likely that your message(s) will be rejected"));
				AssertEquals(1, header.REXDISCusTempStorageDec.Messages.Count);

				rexdisStorageDec.STH_MessageStatus = Common.Shared.MessageStatusList.Codes.NotSent;
				Factory.Save();
			}

			var newFactory = new BusinessObjectFactory();
			newFactory.SuspendValidation();
			var reloadedHeader = newFactory.Load<CusTempStorageJobHeader>(header.PK);

			using (var reExportForm = new TemporaryStorageForm(reloadedHeader))
			{
				var topLevelMenu = reExportForm.Menu.MenuItems.FindByText("Messages");
				var sendReExportMenu = topLevelMenu.MenuItems.FindByText("Send Re-Export");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Environment.Env.Security.CustomsTemporaryStorageSendWithMessageErrors.IsAllowed = false;
				sendReExportMenu.PerformClick();
				AssertEquals("Validation is Suspended so no Message Errors", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("The message has been sent"));
				AssertEquals(2, header.REXDISCusTempStorageDec.Messages.Count);
			}
		}

		public void TestSubsequentMessagesCannotBeSent()
		{
			var rexdisStorageDec = REXDISCusTempStorageDec.New(header);
			rexdisStorageDec.CusTempStorageLines.AddNew();
			Factory.Save();

			using (var reExportForm = new TemporaryStorageForm(header))
			{
				var topLevelMenu = reExportForm.Menu.MenuItems.FindByText("Messages");
				var sendReExportMenu = topLevelMenu.MenuItems.FindByText("Send Re-Export");

				AssertEquals(Common.Shared.MessageStatusList.Codes.NotSent, rexdisStorageDec.STH_MessageStatus);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				Environment.Env.Security.CustomsTemporaryStorageSendWithMessageErrors.IsAllowed = true;
				sendReExportMenu.PerformClick();
				AssertEquals("Message has been sent", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("The message has been sent."));
				AssertEquals(Common.Shared.MessageStatusList.Codes.Sent, rexdisStorageDec.STH_MessageStatus);

				Factory.Save();
				sendReExportMenu.PerformClick();
				AssertEquals("Cannot Send multiple messages", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Cannot send further messages, still awaiting a response."));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			header.SJH_AppCode = TemporaryStorageApplicationCodeList.Codes.REX;
			setTemporaryCurrentUser = Factory.SetTemporaryCurrentUser("Senior Logistics Manager", "Owen Daniels", "+61 2 8001 2200");
		}
		IDisposable setTemporaryCurrentUser;

		protected override void TearDown()
		{
			base.TearDown();
			setTemporaryCurrentUser.Dispose();
		}

		CusTempStorageJobHeader header;
	}
}
