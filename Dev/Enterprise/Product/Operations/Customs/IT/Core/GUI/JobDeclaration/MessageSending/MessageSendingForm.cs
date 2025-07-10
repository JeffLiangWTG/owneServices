using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public partial class MessageSendingForm : MessageSendingFormWithValidationDetails
{
	[Obsolete("Required by the designer on subclass. Please do not use.")]
	public MessageSendingForm()
	{
	}

	public MessageSendingForm(JobDeclarationMessageSendingObjectParent declarationWrapper) : base(declarationWrapper)
	{
	}

	protected override void InitialiseForm()
	{
		base.InitialiseForm();
		InitializeComponent();
		SendSplitButton.AutomaticSendSelected += SendSplitButton_AutomaticSendSelected;
		SendSplitButton.ManualSendSelected += SendSplitButton_ManualSendSelected;
		SendSplitButton.FallbackProcedureSelected += SendSplitButton_FallbackProcedureSelected;

		AttachShouldSendValueChangedEvent();
	}

	protected override void Dispose(bool disposing)
	{
		SendSplitButton.AutomaticSendSelected -= SendSplitButton_AutomaticSendSelected;
		SendSplitButton.ManualSendSelected -= SendSplitButton_ManualSendSelected;
		SendSplitButton.FallbackProcedureSelected -= SendSplitButton_FallbackProcedureSelected;

		foreach (var sendingObject in SendingObjects)
		{
			sendingObject.ShouldSendInfo.ValueChanged -= ShouldSendInfo_ValueChanged;
		}

		if (disposing)
		{
			components?.Dispose();
		}
		base.Dispose(disposing);
	}

	protected override ZButton GetEffectiveSendButton() => SendSplitButton;

	void SendSplitButton_AutomaticSendSelected(object sender, EventArgs e)
	{
		SendMessageWithSendingMode(CustomsMessageSendingModeList.Codes.AutomaticProcedure);
	}

	void SendSplitButton_ManualSendSelected(object sender, EventArgs e)
	{
		SendMessageWithSendingMode(CustomsMessageSendingModeList.Codes.ManualProcedure);
	}

	void SendSplitButton_FallbackProcedureSelected(object sender, EventArgs e)
	{
		SendMessageWithSendingMode(CustomsMessageSendingModeList.Codes.FallbackProcedure);
	}

	void SendMessageWithSendingMode(ZString customsMessageSendingMode)
	{
		if (!IsMessageSendingAllowed(customsMessageSendingMode))
		{
			return;
		}

		SendMessageWithSendingModeCore(customsMessageSendingMode);
	}

	protected virtual void SendMessageWithSendingModeCore(ZString customsMessageSendingMode)
	{
		BusinessEntity.CustomsMessageSendingMode = customsMessageSendingMode;
		DialogResult = DialogResult.OK;
		Close();
	}

	void AttachShouldSendValueChangedEvent()
	{
		foreach (var sendingObject in SendingObjects)
		{
			sendingObject.ShouldSendInfo.ValueChanged += ShouldSendInfo_ValueChanged;
		}
	}

	void ShouldSendInfo_ValueChanged(object sender, EventArgs e)
	{
		SetSendSplitButtonStyleOnShouldSendChange();
	}

	protected virtual void SetSendSplitButtonStyleOnShouldSendChange()
	{
		var anySendingObjectCanSendOnlyInFallback = SendingObjects.Any(x => x.ShouldSend && x.CanBeSentOnlyInFallbackMode);
		SendSplitButton.SetButtonStyle(anySendingObjectCanSendOnlyInFallback ? SendingModeSplitButton.ButtonStyle.FallbackAction : SendingModeSplitButton.ButtonStyle.MultipleActions);
	}

	protected IEnumerable<JobDeclarationMessageSendingObject> SendingObjects => BusinessEntity?.SendingObjectsCollection.Cast<JobDeclarationMessageSendingObject>() ?? Enumerable.Empty<JobDeclarationMessageSendingObject>();

	public new JobDeclarationMessageSendingObjectParent BusinessEntity => (JobDeclarationMessageSendingObjectParent)base.BusinessEntity;

	protected virtual bool IsMessageSendingAllowed(ZString customsMessageSendingMode)
	{
		return CheckIsOKToSend(customsMessageSendingMode)
			&& CheckFileNameNumberRange(customsMessageSendingMode)
			&& CheckSendingNotSendableEntries();
	}

	protected virtual bool CheckFileNameNumberRange(ZString customsMessageSendingMode)
	{
		var fileNameNumberRangeValidator = new FileNameNumberRangeValidator(BusinessEntity.Factory, JobDeclaration?.JE_CustomsProfile ?? ZString.Empty);
		return fileNameNumberRangeValidator.CheckFilenameNumberRangeValidity(customsMessageSendingMode);
	}

	protected virtual bool CheckSendingNotSendableEntries()
	{
		var sendingNotAllowedEntriesWarner = new EntryAllowedSendingValidation(BusinessEntity.SelectedSendingObjects.Cast<IEntryMessageSendingObjectInfo>());
		return sendingNotAllowedEntriesWarner.CheckAndWarn();
	}

	bool CheckIsOKToSend(ZString customsMessageSendingMode)
	{
		if (CheckIsOKToSend())
		{
			var subscriber = JobDeclaration?.JE_GS_NKCusAgent ?? ZString.Empty;
			var isUCC6 = JobDeclaration?.IsUCC6 ?? false;
			return isUCC6
				? MessageSendingFormValidation.CheckCurrentUserHasFiscalCode()
				: MessageSendingFormValidation.CheckSubscriberForMessageSending(customsMessageSendingMode, subscriber);
		}

		return false;
	}

	JobDeclaration JobDeclaration => BusinessEntity.ParentDeclaration;

	protected IMessageSendingFormValidation MessageSendingFormValidation => messageSendingFormValidation ?? (messageSendingFormValidation = new MessageSendingFormValidation());
	IMessageSendingFormValidation messageSendingFormValidation;
}
