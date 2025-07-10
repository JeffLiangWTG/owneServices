using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.GUI;
using Enterprise.Customs.CH.GUI.Testing;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using GlbCompanyWrapper = Enterprise.Customs.CH.Business.GlbCompanyWrapper;
using NctsHeader = Enterprise.Customs.CH.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

abstract class MessagingMenuProviderTest : TestCaseWithFactory
{
	public void TestGetProvider()
	{
		var nctsHeader = PrepareNctsHeader();
		using (var nctsForm = GetNctsMovementForm(nctsHeader))
		{
			var nctsMenuProvider = Phase5MessagingMenuProvider.GetProvider(nctsHeader);
			AssertType<MessagingMenuProvider>(nctsMenuProvider);
		}
	}

	public void TestPreSaveInvoked()
	{
		PrepareSendingEnvironment();

		var nctsHeader = PrepareNctsHeader();
		nctsHeader.BH_JobReference = "123";
		var movementHeader = nctsHeader.IsDepartureMovement ? (NctsCommonMovementHeader)nctsHeader.MovementHeader : nctsHeader.ArrivalMovementHeader;
		movementHeader.BM_PaperlessInbondNum = "123456789";

		var nctsMovementForm = GetNctsMovementForm(nctsHeader);

		using (nctsMovementForm)
		{
			var sendMenuItem = GetSendToCustomsMenuItem(nctsMovementForm);

			CombineAssertions(() =>
			{
				AssertEquals("Pre-condition not saved", true, nctsHeader.HasChanges);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				sendMenuItem.PerformClick();
				AssertEquals("Message for deny save", MessagePreSave, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Save denied", true, nctsHeader.HasChanges);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendMenuItem.PerformClick();
				AssertEquals("Message for confirm save", MessagePreSave, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Save confirmed", false, nctsHeader.HasChanges);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				sendMenuItem.PerformClick();
				AssertNotEquals("No message when no changes", MessagePreSave, UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}
	}

	public void TestSendToCustomsClick_CannotSend()
	{
		var nctsHeader = PrepareNctsHeader();
		var nctsMovementForm = GetNctsMovementForm(nctsHeader);

		using (nctsMovementForm)
		{
			var sendMenuItem = GetSendToCustomsMenuItem(nctsMovementForm);

			CombineAssertions(() =>
			{
				PrepareSendingEnvironment(setupBID: false);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				sendMenuItem.PerformClick();
				AssertErrorMessage("When no BID", MessageNoBID, MessageCaptionUnableToSend);

				PrepareSendingEnvironment(setupCommunicationToken: false);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				sendMenuItem.PerformClick();
				AssertErrorMessage("When no token", MessageNoToken, MessageCaptionUnableToSend);

				PrepareSendingEnvironment();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				sendMenuItem.PerformClick();
				AssertNotEquals("With BID", MessageNoBID, UnitTestUserNotification.Instance.LastMessage);
				AssertNotEquals("With token", MessageNoToken, UnitTestUserNotification.Instance.LastMessage);
			});
		}
	}

	public void TestSendToCustoms_Enabled() => CombineAssertions(() =>
	{
		var nctsHeader = PrepareNctsHeader();
		var menu = new MessagingMenuProvider(nctsHeader);
		var menuItems = menu.CreateMenuItems().ToArray();
		var sendToCustomsMenuItem = menuItems.FindByText(MenuTextSendToCustoms);

		foreach (var messageStatus in new CHLogicalStatusList().GetAllCodes().Append(string.Empty))
		{
			var isNotSend = messageStatus != CHLogicalStatusList.Codes.Sent && messageStatus != CHLogicalStatusList.Codes.Acknowledged;
			AssertEnabled(isNotSend, messageStatus, false);
			AssertEnabled(true, messageStatus, true);
		}

		void AssertEnabled(bool expectedEnabled, string messageStatus, bool isResendAllowed)
		{
			Env.Security.CHNCTSAllowResendToCustoms.IsAllowed = isResendAllowed;
			nctsHeader.EffectiveMessageStatus = messageStatus;
			menu.RefreshMenu();
			AssertEquals($"MessageStatus={messageStatus} IsAllowed={isResendAllowed}", expectedEnabled, sendToCustomsMenuItem?.Enabled);
		}
	});

	public void TestSendToCustoms()
	{
		if (MessageTypeForSending.IsEmpty)
		{
			Assert(true);
			return;
		}

		var nctsHeader = PrepareNctsHeader();
		Factory.Save();

		PrepareSendingEnvironment();
		CreateCurrentCompanyCredential();

		using (var nctsMovementForm = GetNctsMovementForm(nctsHeader))
		{
			nctsMovementForm.Show();
			var sendMenuItem = nctsMovementForm.FindMenuItem_ForTest(MenuTextSendToCustoms);

			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
			{
				var dialog = (MessageSendingFormWithValidationDetails)obj;
				var messageSendingObject = (NctsHeaderCommonMessageSendingObject)dialog.MessageSendingObjectParent.SendingObjectsCollection[0];
				messageSendingObject.ShouldSend = true;
				messageSendingObject.MessageType = MessageTypeForSending;
			});
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			sendMenuItem.PerformClick();

			var expectedEDIMessageCollection = NctsHeaderMovementType == NctsMovementType.Codes.Departure ? nctsHeader.MovementHeader.Messages : nctsHeader.Messages;

			CombineAssertions(() =>
			{
				AssertType(ExpectedMessageSendingFormType, ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals(MessageOneMessageHasBeenSent, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("1 message has been sent.", 1, expectedEDIMessageCollection.Count);
			});
		}
	}

	public void TestDocumentSearchRequestMenuItem() => CombineAssertions(() =>
	{
		using (var nctsMovementForm = new Phase5DepartureMovementForm(PrepareNctsHeader()))
		{
			var documentSearchMenuItem = nctsMovementForm.FindMenuItem_ForTest(MenuTextDocumentSearchRequest);
			AssertNotNull("Document Search menu item exists", documentSearchMenuItem);

			var sendToCustomsMenuItem = nctsMovementForm.FindMenuItem_ForTest(MenuTextSendToCustoms);
			Assert(@"""Document Search Request"" below ""Send to Customs""", sendToCustomsMenuItem.Index < documentSearchMenuItem.Index);
		}
	});

	public void TestDocumentSearchRequestMenuItem_SecurityRight() => CombineAssertions(() =>
	{
		Env.Security.CHManualDocumentSearch.IsAllowed = true;
		using (var nctsMovementForm = new Phase5DepartureMovementForm(PrepareNctsHeader()))
		{
			var documentSearchMenuItem = nctsMovementForm.FindMenuItem_ForTest(MenuTextDocumentSearchRequest);
			AssertEquals("IsAllowed=true", true, documentSearchMenuItem?.Visible);
		}
		Env.Security.CHManualDocumentSearch.IsAllowed = false;
		using (var nctsMovementForm = new Phase5DepartureMovementForm(PrepareNctsHeader()))
		{
			var documentSearchMenuItem = nctsMovementForm.FindMenuItem_ForTest(MenuTextDocumentSearchRequest);
			AssertNull("IsAllowed=true", documentSearchMenuItem);
		}
	});

	public void TestDocumentSearchRequestMenuItem_Environment() => CombineAssertions(() =>
	{
		using (var nctsMovementForm = new Phase5DepartureMovementForm(PrepareNctsHeader()))
		{
			var documentSearchMenuItem = nctsMovementForm.FindMenuItem_ForTest(MenuTextDocumentSearchRequest);

			PrepareSendingEnvironment(setupBID: false);
			documentSearchMenuItem.PerformClick();
			UserNotificationTestHelper.AssertLastMessage("TokenCredentials not enabled", MessageCaptionUnableToSend, MessageNoBID, expectedWasError: true);

			PrepareSendingEnvironment(setupCommunicationToken: false);
			documentSearchMenuItem.PerformClick();
			UserNotificationTestHelper.AssertLastMessage("TokenCredentials not enabled", MessageCaptionUnableToSend, MessageNoToken, expectedWasError: true);

			PrepareSendingEnvironment();
			documentSearchMenuItem.PerformClick();
			AssertNotEquals("With BID", MessageNoBID, UnitTestUserNotification.Instance.LastMessage);
			AssertNotEquals("With token", MessageNoToken, UnitTestUserNotification.Instance.LastMessage);
		}
	});

	public void TestDocumentSearchRequestMenuItem_Click() => CombineAssertions(() =>
	{
		PrepareSendingEnvironment();
		CreateCurrentCompanyCredential();

		using (var nctsMovementForm = new Phase5DepartureMovementForm(PrepareNctsHeader()))
		{
			var documentSearchMenuItem = nctsMovementForm.FindMenuItem_ForTest(MenuTextDocumentSearchRequest);

			var processId = ZString.Empty;
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
			{
				var dialog = (CharteraOutputDocumentSearchRequestSendingForm)obj;
				var sendingObject = (CharteraOutputDocumentSearchSendingObject)dialog.BusinessEntity;
				sendingObject.CreationTimeFrom = ZDateTime.Now;
				processId = sendingObject.ProcessId;
			});
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			documentSearchMenuItem.PerformClick();
			AssertType<CharteraOutputDocumentSearchRequestSendingForm>(ZFormModaliser.LastFormShownDialogForTest);

			AssertEquals(MessageOneMessageHasBeenSent, UnitTestUserNotification.Instance.LastMessage.Text);

			var ediMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationReference, processId));
			var transaction = Factory.LoadTop1<CusPollingTransaction>(new ZQuery(CusPollingTransactionSchema.CPT_TransactionID, processId));
			AssertNotNull("EDIMessage created", ediMessage);
			AssertNotNull("Transaction created", transaction);
			GlbCompany.CurrentCompany.Logs.MostRecentLogByEventTime(Events.DocumentAllocated); //WI00650967 Event MDS when checked-in
		}
	});

	protected abstract ZString NctsHeaderMovementType { get; }
	protected abstract ZString MessageTypeForSending { get; }
	protected abstract Type ExpectedMessageSendingFormType { get; }
	protected abstract ZTemplateForm GetNctsMovementForm(NctsHeader nctsHeader);
	protected virtual bool ProvidesSendActivationMenuEntry => true;

	protected NctsHeader PrepareNctsHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsHeaderMovementType);
		return nctsHeader;
	}

	protected void PrepareSendingEnvironment(bool setupBID = true, bool setupCommunicationToken = true)
	{
		GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.RemoveAll();
		GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.RemoveAll();

		if (setupBID)
		{
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, "123");
		}

		var companyWrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany);
		companyWrapper.TokenCredentialsEnabled = setupCommunicationToken;
	}

	protected ZMenuItem GetSendToCustomsMenuItem(ZTemplateForm nctsMovementForm)
	{
		var menuItem = (ZMenuItem)nctsMovementForm.FindMenuItem_ForTest(MenuTextSendToCustoms);
		AssertNotNull($@"Menu item ""{MenuTextSendToCustoms}"" not found", menuItem);
		return menuItem;
	}

	protected void AssertErrorMessage(string assertionMessage, string errorText, string errorCaption)
	{
		AssertEquals($"{assertionMessage}: Text", errorText, UnitTestUserNotification.Instance.LastMessage.Text);
		AssertEquals($"{assertionMessage}: Caption", errorCaption, UnitTestUserNotification.Instance.LastMessage.Caption);
		AssertEquals($"{assertionMessage}: WasError", true, UnitTestUserNotification.Instance.LastMessage.WasError);
	}

	protected void CreateCurrentCompanyCredential()
	{
		var credential = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword;
		credential.GP_Certificate = new ZBlob(X509Certificate2TestHelper.ValidCertificate);
		credential.GP_UserID = "clientKey";
		credential.CurrentDecryptedPassword = "clientSecret";
		credential.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		GlbCompany.CurrentCompany.Factory.Save();
	}

	protected const string MenuTextSendToCustoms = "Send to Customs";
	protected const string MenuTextDocumentSearchRequest = "Chartera Output Documents Search";

	protected const string MessageCaptionUnableToSend = "Unable to send to customs";
	protected const string MessageNoBID = "Business Partner ID (BID) is not configured for the company or branch. Please contact your system administrator.";
	protected const string MessageNoToken = "Communication Tokens are not configured for the company. Please contact your system administrator.";
	protected const string MessagePreSave = "The Job has not yet been saved. Do you want to save and proceed?";
	protected const string MessageOneMessageHasBeenSent = "1 message(s) have been sent.";
}
