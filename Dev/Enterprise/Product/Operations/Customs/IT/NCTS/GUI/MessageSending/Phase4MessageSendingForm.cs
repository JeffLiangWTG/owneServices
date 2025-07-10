using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.GUI;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

public partial class Phase4MessageSendingForm : MessageSendingFormWithValidationDetails
{
	public Phase4MessageSendingForm(NctsHeaderDepartureMessageSendingObjectParent sendingObjectWrapper) : base(sendingObjectWrapper)
	{
	}

	public new NctsHeaderDepartureMessageSendingObjectParent BusinessEntity => (NctsHeaderDepartureMessageSendingObjectParent)base.BusinessEntity;

	protected override void InitialiseForm()
	{
		base.InitialiseForm();
		InitializeComponent();
		SendSplitButton.AutomaticSendSelected += SendSplitButton_AutomaticSendSelected;
		SendSplitButton.ManualSendSelected += SendSplitButton_ManualSendSelected;
		SendSplitButton.FallbackProcedureSelected += SendSplitButton_FallbackProcedureSelected;
	}

	protected override void Dispose(bool disposing)
	{
		SendSplitButton.AutomaticSendSelected -= SendSplitButton_AutomaticSendSelected;
		SendSplitButton.ManualSendSelected -= SendSplitButton_ManualSendSelected;
		SendSplitButton.FallbackProcedureSelected -= SendSplitButton_FallbackProcedureSelected;
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
		if (IsMessageSendingAllowed(customsMessageSendingMode))
		{
			BusinessEntity.CustomsMessageSendingMode = customsMessageSendingMode;
			DialogResult = DialogResult.OK;
			Close();
		}
	}

	bool IsMessageSendingAllowed(ZString customsMessageSendingMode)
	{
		if (CheckIsOKToSend(customsMessageSendingMode))
		{
			var validator = new FileNameNumberRangeValidator(BusinessEntity.Factory, BusinessEntity.CustomsProfile);
			return validator.CheckFilenameNumberRangeValidity(customsMessageSendingMode) && CheckSendingNotSendableEntry();
		}

		return false;
	}

	bool CheckSendingNotSendableEntry()
	{
		var sendingNotAllowedEntriesWarner = new EntryAllowedSendingValidation(BusinessEntity.SelectedSendingObjects.Cast<IEntryMessageSendingObjectInfo>());
		return sendingNotAllowedEntriesWarner.CheckAndWarn();
	}

	bool CheckIsOKToSend(ZString customsMessageSendingMode)
	{
		if (CheckIsOKToSend())
		{
			var messageSendingFormValidation = (IMessageSendingFormValidation)new MessageSendingFormValidation();
			var subscriber = ((NctsHeader)BusinessEntity.TopLevelBusinessObject).Subscriber;
			return messageSendingFormValidation.CheckSubscriberForMessageSending(customsMessageSendingMode, subscriber);
		}

		return false;
	}
}
