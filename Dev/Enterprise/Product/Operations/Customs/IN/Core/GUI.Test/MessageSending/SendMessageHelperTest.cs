using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.Certificates;
using Enterprise.Customs.IN.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(SendMessageHelper))]
sealed class SendMessageHelperTest : TestCaseWithFactory
{
	public void TestGenerateMessageEmail()
	{
		SetupDialog();
		var messageSendingObject = GetMessageSendingObject();
		messageSendingObject.ParentDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		messageSendingObject.ParentDeclaration.JE_CustomsOffice = "PQR123";
		SendMessageHelper.GenerateMessage(messageSendingObject, () => GetSendMessageForm(messageSendingObject));
		AssertEquals("Message success", "Message sent successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

		AssertEDIMessage(messageSendingObject.ParentDeclaration.ActiveEntryHeaders[0].Messages[0]);
		AssertEDIMessage(messageSendingObject.ParentDeclaration.ActiveEntryHeaders[1].Messages[0]);

		void AssertEDIMessage(Messaging.Business.EDIMessage message)
		{
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Queued, message.EM_Status);
			AssertEquals("EM_ApplicationReference", ZString.Empty, message.EM_ApplicationReference);
		}

		messageSendingObject.ParentDeclaration.JE_MessageType = "XXX";
		SendMessageHelper.GenerateMessage(messageSendingObject, () => GetSendMessageForm(messageSendingObject));
		AssertEquals("Message success", "Message has not been sent. This may be caused by a failure or because sending has not been implemented.", UnitTestUserNotification.Instance.LastMessage.Text);
	}

	public void TestGenerateMessageDownload()
	{
		SetupDialog();
		SetupDialogDownloadClick();
		var tempPath = Path.Combine(EnvProxy.Instance.TempPath, Guid.NewGuid().ToString());
		Directory.CreateDirectory(tempPath);

		var testFileName1 = Path.Combine(tempPath, "0000001.SB");
		var testFileName2 = Path.Combine(tempPath, "0000002.SB");
		ZFormModaliser.PathToSelectInShowCommonDialog = tempPath;

		try
		{
			var messageSendingObject = GetMessageSendingObject();

			CombineAssertions(() =>
			{
				messageSendingObject.ParentDeclaration.JE_MessageType = "XXX";
				messageSendingObject.ParentDeclaration.JE_CustomsOffice = "PQR123";
				SendMessageHelper.GenerateMessage(messageSendingObject, () => GetSendMessageForm(messageSendingObject));
				AssertEquals("Message success", "Message has not been downloaded. This may be caused by a failure or because downloading has not been implemented.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("File not created for Download", 0, Directory.EnumerateFiles(tempPath).Count());
			});

			CombineAssertions(() =>
			{
				messageSendingObject.ParentDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				SendMessageHelper.GenerateMessage(messageSendingObject, () => GetSendMessageForm(messageSendingObject));
				AssertEquals("Message success", $"Message downloaded successfully to\r\n{testFileName1}\r\n{testFileName2}", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("File created for Download", 2, Directory.EnumerateFiles(tempPath).Count());
				Assert("File created for Download Message for Entry 1", File.Exists(testFileName1));
				Assert("File created for Download Message for Entry 2", File.Exists(testFileName2));
				var folderDialog = (FolderBrowserDialog)ZFormModaliser.LastCommonDialogShownDialogForTest;
				AssertEquals("Folder Dialog Description", "Please select a valid directory to download the message file.", folderDialog?.Description);

				AssertEDIMessage(messageSendingObject.ParentDeclaration.ActiveEntryHeaders[0].Messages[0], testFileName1);
				AssertEDIMessage(messageSendingObject.ParentDeclaration.ActiveEntryHeaders[1].Messages[0], testFileName2);

				void AssertEDIMessage(Messaging.Business.EDIMessage message, string downloadedFilePath)
				{
					AssertEquals("File content for Download", message.EM_MessageText, File.ReadAllText(downloadedFilePath));
					AssertEquals("EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
					AssertEquals("EM_ApplicationReference", Constants.Messaging.MessageDownloaded, message.EM_ApplicationReference);
				}
			});
		}
		finally
		{
			Directory.Delete(tempPath, true);
		}
	}

	public void TestSetTokenPinIfNeeded()
	{
		Assert("No need to set token pin for Current user CWSupport by default", SendMessageHelper.SetTokenPinIfNeeded());

		var declaration = Factory.New<JobDeclaration>();
		declaration.CustomsEntryHeaders.AddNew();
		var messageSendingObjectParent = new DeclarationMessageSendingObjectParent(declaration);
		messageSendingObjectParent.SendingObjectsCollection.Cast<DeclarationMessageSendingObject>().First().ShouldSend = ZBool.True;

		GlbStaff.CurrentUser.GS_LoginName = "Enterprise User";
		var password = IN.Business.GlbStaffWrapper.GetWrapperForCurrentUser()?.LoginPassword;
		password.GP_GS = GlbStaff.CurrentUser.PK;
		password.GP_UserID = "test";
		password.GP_MailBoxID = "test@test.com";

		var tokenPinStore = (ITokenPinStore)CertificateTokenPinStore.Instance;
		tokenPinStore.ResetPin();
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
		var result = SendMessageHelper.SetTokenPinIfNeeded();

		CombineAssertions(() =>
		{
			Assert("Need to set token pin", !result);
			AssertType<EnterCryptokiCertificatePinForm>("Enter token pin form displayed", ZFormModaliser.LastFormShownDialogForTest);
			AssertContains("Token pin not set for EMAIL", "Message sending canceled.", UnitTestUserNotification.Instance.LastMessage.Text);
			ZFormModaliser.LastFormShownDialogForTest = null;

			tokenPinStore.ResetPin();
			tokenPinStore.SetPin("1234");
			result = SendMessageHelper.SetTokenPinIfNeeded();
			Assert("Token pin already set", result);
			AssertNull("Enter token pin form not displayed", ZFormModaliser.LastFormShownDialogForTest);
		});
	}

	DeclarationMessageSendingObjectParent GetMessageSendingObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.ActiveEntryHeaders.AddNew();
		declaration.ActiveEntryHeaders.AddNew();
		var sendingObjectParent = new DeclarationMessageSendingObjectParent(declaration);
		sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
		sendingObjectParent.SendingObjectsCollection[1].ShouldSend = true;
		return sendingObjectParent;
	}

	void SetupDialog()
	{
		ZFormModaliser.ShowDialogsInTest = true;
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
	}

	void SetupDialogDownloadClick()
	{
		ZFormModaliser.SetDelegateToCallOnFormShown((dialog) =>
		{
			if (dialog is SendMessageFormForTest form)
			{
				form.DownloadButtonExposed.Enabled = true;
				form.DownloadButtonExposed.PerformClick();
			}
		});
	}

	SendMessageFormForTest GetSendMessageForm(BaseMessageSendingObjectParent messageParent) => new SendMessageFormForTest(messageParent);
}
