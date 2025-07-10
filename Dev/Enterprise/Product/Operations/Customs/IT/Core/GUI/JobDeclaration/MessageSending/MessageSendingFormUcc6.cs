using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IT.GUI;

public partial class MessageSendingFormUcc6 : MessageSendingForm
{
	public MessageSendingFormUcc6(JobDeclarationMessageSendingObjectParent declarationWrapper) : base(declarationWrapper)
	{
	}

	public new Ucc6JobDeclarationMessageSendingObjectParent BusinessEntity => (Ucc6JobDeclarationMessageSendingObjectParent)base.BusinessEntity;

	protected override void InitialiseForm()
	{
		base.InitialiseForm();
		HookMessageTypeInfoEvents();
		SetDefaultSendSplitButtonStyle();
		SetSendWithAdditionalWarningCheckBoxText();
	}

	protected override void Dispose(bool disposing)
	{
		UnHookMessageTypeInfoEvents();
		base.Dispose(disposing);
	}

	protected override bool IsMessageSendingAllowed(ZString customsMessageSendingMode)
	{
		return base.IsMessageSendingAllowed(customsMessageSendingMode)
			&& CheckNodePresentInCompanyAllowedList()
			&& CheckAndPromptUserIfSendingCancellationMessages();
	}

	protected override bool CheckFileNameNumberRange(ZString customsMessageSendingMode) => true;

	protected override bool CheckSendingNotSendableEntries()
	{
		return BusinessEntity.HasAnySelectedCancelMessage
			|| base.CheckSendingNotSendableEntries();
	}

	protected override void SendMessageWithSendingModeCore(ZString customsMessageSendingMode)
	{
		if (GlbStaff.CurrentUser.HasValidAutomaticSignaturePassword())
		{
			base.SendMessageWithSendingModeCore(customsMessageSendingMode);
			return;
		}

		var handlerResult = XadesCertificatePinHandler.HandleTokenPin();
		switch (handlerResult)
		{
			case XadesCertificatePinHandler.XadesCertificatePinHandlerResult.Completed:
				base.SendMessageWithSendingModeCore(customsMessageSendingMode);
				break;
			case XadesCertificatePinHandler.XadesCertificatePinHandlerResult.Cancelled:
				DialogResult = DialogResult.Cancel;
				Close();
				break;
		}
	}

	bool CheckAndPromptUserIfSendingCancellationMessages()
	{
		return !BusinessEntity.HasAnySelectedCancelMessage || AskUserConfirmationForCancellationMessagesSending();
	}

	bool CheckNodePresentInCompanyAllowedList() => MessageSendingFormValidation.CheckNodePresentInCompanyAllowedList(BusinessEntity.ParentDeclaration?.JE_CustomsProfile ?? ZString.Empty);

	bool AskUserConfirmationForCancellationMessagesSending()
	{
		var userNotificationMessageCaption = Res.GetString("8697AAEC-7699-4BC6-BEF4-C58C56051615", "Cancellation Confirmation");
		var userNotificationMessage = Res.GetString("2CC0BFD1-404A-453F-8D6A-403771A7FF5F", "You are canceling a registered customs declaration. Do you confirm?");
		return Globals.Message.Show(userNotificationMessage, userNotificationMessageCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes) == DialogResult.Yes;
	}

	void MessageTypeInfo_ValueChanged(object sender, EventArgs e)
	{
		ChangeSendWithValidationErrorsCheckBoxAvailability();
		ChangeSendWithAdditionalWarningCheckBoxAvailability();
		ChangeSendButtonAvailability();
	}

	void HookMessageTypeInfoEvents()
	{
		foreach (var sendingObject in SendingObjects)
		{
			sendingObject.MessageTypeInfo.ValueChanged += MessageTypeInfo_ValueChanged;
		}
	}

	void UnHookMessageTypeInfoEvents()
	{
		foreach (var sendingObject in SendingObjects)
		{
			sendingObject.MessageTypeInfo.ValueChanged -= MessageTypeInfo_ValueChanged;
		}
	}

	XadesCertificatePinHandler XadesCertificatePinHandler
		=> xadesCertificatePinHandler ?? (xadesCertificatePinHandler = new XadesCertificatePinHandler(this));
	XadesCertificatePinHandler xadesCertificatePinHandler;

	void SetSendWithAdditionalWarningCheckBoxText()
	{
		var (enabled, text) = GetSendWithAdditionalWarningCheckBoxLayoutData();
		SendWithAdditionalWarningCheckBox.Enabled = enabled;
		SendWithAdditionalWarningCheckBox.Text = text;
	}

	(bool Enable, string Text) GetSendWithAdditionalWarningCheckBoxLayoutData()
	{
		return BusinessEntity.SecurityCheckpointToSendWithMessageError.IsAllowed
			? (true, SendWithAdditionalWarningCheckBoxText)
			: (false, DontHaveSecurityRightsText);
	}

	protected virtual UserEnterableTokenPin GetNewUserEnterableTokenPin() => new UserEnterableTokenPin();
	static string SendWithAdditionalWarningCheckBoxText => Res.GetString("6F3A965F-8289-42E7-BB78-6BD5E44CB13E", "Continue to send even though the selected message contains XML errors?");
	static string DontHaveSecurityRightsText => Res.GetString("1E36FB8D-F8E2-4258-9939-FB67DE36FA7C", "You don't have security rights to send with XML errors");

	#region SendSplitButton Style

	void SetDefaultSendSplitButtonStyle()
	{
		SendSplitButton.SetButtonStyle(SendingModeSplitButton.ButtonStyle.AutomaticAction);
	}

	protected override void SetSendSplitButtonStyleOnShouldSendChange()
	{
	}

	#endregion

#if DEBUG
	protected void SetXadesCertificatePinHandler(XadesCertificatePinHandler handler) => xadesCertificatePinHandler = handler;
#endif
}
