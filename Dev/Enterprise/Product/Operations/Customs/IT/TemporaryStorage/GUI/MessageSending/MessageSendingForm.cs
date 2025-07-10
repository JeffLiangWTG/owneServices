using System.Linq;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.GUI;
using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI;

public partial class MessageSendingForm : Customs.GUI.MessageSendingFormWithValidationDetails
{
	public MessageSendingForm(TemporaryStorageMessageSendingObjectParent sendingObjectParent) : base(sendingObjectParent)
	{
		InitializeComponent();
	}

	public new TemporaryStorageMessageSendingObjectParent BusinessEntity => (TemporaryStorageMessageSendingObjectParent)base.BusinessEntity;

	protected override bool CheckIsOKToSend()
	{
		return base.CheckIsOKToSend()
			&& CheckUserSignatureValidity()
			&& CheckEntryAllowedSendingValidation()
			&& CheckNodePresentInCompanyAllowedList();

		bool CheckNodePresentInCompanyAllowedList() => MessageSendingFormValidation.CheckNodePresentInCompanyAllowedList(BusinessEntity.CustomsProfile);

		bool CheckUserSignatureValidity()
			=> GlbStaff.CurrentUser.HasValidAutomaticSignaturePassword()
			|| XadesCertificatePinHandler.HandleTokenPin() == XadesCertificatePinHandler.XadesCertificatePinHandlerResult.Completed;
	}

	bool CheckEntryAllowedSendingValidation()
	{
		var sendingNotAllowedEntriesWarner = new EntryAllowedSendingValidation(BusinessEntity.SelectedSendingObjects.Cast<IEntryMessageSendingObjectInfo>());
		return sendingNotAllowedEntriesWarner.CheckAndWarn();
	}

	IMessageSendingFormValidation MessageSendingFormValidation => messageSendingFormValidation ??= new MessageSendingFormValidation();
	IMessageSendingFormValidation messageSendingFormValidation;

	XadesCertificatePinHandler XadesCertificatePinHandler => xadesCertificatePinHandler ??= new XadesCertificatePinHandler(this);
	XadesCertificatePinHandler xadesCertificatePinHandler;

#if DEBUG

	protected void SetXadesCertificatePinHandler(XadesCertificatePinHandler handler) => xadesCertificatePinHandler = handler;

#endif
}
