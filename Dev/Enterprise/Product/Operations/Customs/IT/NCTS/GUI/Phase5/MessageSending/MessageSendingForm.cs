using System;
using System.Linq;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.GUI;
using Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.GUI;

public partial class MessageSendingForm : EU.NCTS.GUI.MessageSendingForm
{
	public MessageSendingForm(NctsHeaderMessageSendingObjectParent sendingObjectParent) : base(sendingObjectParent)
	{
		InitializeComponent();
		HookMessageTypeInfoEvents();
	}

	void HookMessageTypeInfoEvents()
	{
		foreach (NctsHeaderMessageSendingObject sendingObject in ((NctsHeaderMessageSendingObjectParent)MessageSendingObjectParent).SendingObjectsCollection)
		{
			sendingObject.MessageTypeInfo.ValueChanged += MessageTypeInfo_ValueChanged;
		}
	}

	void UnHookMessageTypeInfoEvents()
	{
		foreach (NctsHeaderMessageSendingObject sendingObject in ((NctsHeaderMessageSendingObjectParent)MessageSendingObjectParent).SendingObjectsCollection)
		{
			sendingObject.MessageTypeInfo.ValueChanged -= MessageTypeInfo_ValueChanged;
		}
	}

	void MessageTypeInfo_ValueChanged(object sender, EventArgs e)
	{
		ChangeSendWithValidationErrorsCheckBoxAvailability();
		ChangeSendButtonAvailability();
	}

	protected override bool CheckIsOKToSend()
	{
		return base.CheckIsOKToSend()
			&& CheckNodePresentInCompanyAllowedList()
			&& CheckEntryAllowedSendingValidation()
			&& CheckUserSignatureValidity()
			&& CheckCurrentUserHasFiscalCode();

		bool CheckUserSignatureValidity()
			=> GlbStaff.CurrentUser.HasValidAutomaticSignaturePassword()
			|| XadesCertificatePinHandler.HandleTokenPin() == XadesCertificatePinHandler.XadesCertificatePinHandlerResult.Completed;
	}

	bool CheckEntryAllowedSendingValidation()
	{
		var sendingNotAllowedEntriesWarner = new EntryAllowedSendingValidation(BusinessEntity.SelectedSendingObjects.Cast<IEntryMessageSendingObjectInfo>());
		return sendingNotAllowedEntriesWarner.CheckAndWarn();
	}

	bool CheckNodePresentInCompanyAllowedList() => MessageSendingFormValidation.CheckNodePresentInCompanyAllowedList(NctsHeader.BH_CustomsProfile);

	bool CheckCurrentUserHasFiscalCode() => MessageSendingFormValidation.CheckCurrentUserHasFiscalCode();

	IMessageSendingFormValidation MessageSendingFormValidation => messageSendingFormValidation ?? (messageSendingFormValidation = new MessageSendingFormValidation());

	IMessageSendingFormValidation messageSendingFormValidation;

	XadesCertificatePinHandler XadesCertificatePinHandler
		=> xadesCertificatePinHandler ?? (xadesCertificatePinHandler = new XadesCertificatePinHandler(this));

	XadesCertificatePinHandler xadesCertificatePinHandler;

#if DEBUG

	protected void SetXadesCertificatePinHandler(XadesCertificatePinHandler handler) => xadesCertificatePinHandler = handler;

#endif
}
