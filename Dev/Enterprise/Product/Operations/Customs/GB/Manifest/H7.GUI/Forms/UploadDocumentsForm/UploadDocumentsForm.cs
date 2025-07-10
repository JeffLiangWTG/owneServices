using System.Linq;
using CargoWise.Application;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.Customs.GB.H7.Messaging;
using Enterprise.Customs.GUI.DocumentSending;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.H7.GUI;

public class UploadDocumentsForm : EU.H7.GUI.UploadDocumentsForm
{
	public UploadDocumentsForm(UploadDocumentsSendingActionParent parent) : base(parent)
	{
		actionParent = parent;
	}
	readonly UploadDocumentsSendingActionParent actionParent;

	public UploadDocumentsSendingActionParent UploadDocumentParent => (UploadDocumentsSendingActionParent)BusinessEntity;

	protected override ResourceStringData MessageSendingObjectsGroupBoxCaption => Res.GetData("08ab977c-4bba-4fe9-98e4-7e707864243f", "Documents to be sent");

	protected override ZUserControl GetBottomSectionUserControl()
	{
		return new UploadDocumentsAdditionalInfoUserControl();
	}

	protected override IGridColumnLayoutProvider GetNewColumnLayoutProvider() => new UploadDocumentsGridColumnLayout();

	protected override void SendButton_ClickCore()
	{
		new CDSH7SupportingDocSendingManager(actionParent, new MessageNotificationCollector()).SendMessages();
		foreach (UploadDocumentsSendingAction sendingObject in actionParent.SelectedSendingObjects)
		{
			sendingObject.Bill.Messages.Reload(false);
		}
	}

	protected sealed override bool CheckIsOKToSend()
	{
		var isValidForSending = true;
		var billsWithoutDocument = GetBillReferencesWithoutDocument();

		if (!string.IsNullOrEmpty(billsWithoutDocument))
		{
			isValidForSending = false;
			Globals.Message.ShowError(Res.GetString("b442d9ed-8932-4095-882e-d954f3057e98", @"There’s nothing selected to be sent to Customs for the following Bill Number(s).
Either untick the ‘Send?’ checkbox or add eDoc(s):

{0}", billsWithoutDocument));
		}

		if (isValidForSending)
		{
			BusinessEntity.RunPreSaveValidation();

			if (SelectedSendingObjectHasErrors())
			{
				isValidForSending = false;
				Globals.Message.ShowError(Res.GetString("75029f5c-0a8a-4eec-ac46-08f5f28923d4", "Please fix the error(s) before sending any documents."));
			}
		}

		if (isValidForSending)
		{
			isValidForSending = CheckCredentials();
		}

		return isValidForSending;
	}

	protected override FilterStripBusinessObject GetUploadDocumentActionFilterBusinessObjectCore() => ObjectFactory.Get<FilterStripBusinessObject>("GBH7BillFilterBusinessObject");

	string GetBillReferencesWithoutDocument()
	{
		var billsWithoutAttachment = BusinessEntity.SelectedSendingObjects
			.Cast<UploadDocumentsSendingAction>()
			.Where(a => !a.DocumentsAttached)
			.Select(a => a.BillNumber);

		return string.Join("\r\n", billsWithoutAttachment);
	}

	bool SelectedSendingObjectHasErrors() => BusinessEntity.SelectedSendingObjects.Any(o => o.HasErrors);

	bool CheckCredentials()
	{
		var notificationCollection = new MessageSendingNotificationCollection();
		var checker = new AsycudaManifestHeaderCdsGlbExternalPasswordChecker(UploadDocumentParent.Header, notificationCollection);

		var isOkToSend = checker.PasswordExistsAndOkToSendToCds;

		if (!isOkToSend)
		{
			Globals.Message.ShowError(notificationCollection.ErrorNotificationsAsString());
		}

		return isOkToSend;
	}
}
