using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageBuilders;

namespace Enterprise.Customs.GB.H7.Business;

public sealed class UploadDocumentsSendingActionParent : EU.H7.Business.UploadDocumentsSendingActionParent<UploadDocumentsSendingAction>, ISupportingDocSendingObjectParent
{
	public UploadDocumentsSendingActionParent(EU.H7.Business.AsycudaManifestHeader manifestHeader) : base(manifestHeader)
	{
	}

	IEnumerable<ISupportingDocumentMessageDataProvider> ISupportingDocSendingObjectParent.SendingObjects => SendingObjectsCollection.Cast<UploadDocumentsSendingAction>().SelectMany(sendingAction => sendingAction.EDocsCollection.OfType<DocumentSendingObject>());

	public AsycudaManifestHeader Header => (AsycudaManifestHeader)manifestHeader;

	protected override NonPersistentBusinessObjectCollection<UploadDocumentsSendingAction> GetSendingObjectsCollectionCore()
	{
		var result = new UploadDocumentsSendingActionCollection(Factory);
		foreach (var bill in Header.Bills)
		{
			result.Add(CreateNewMessageSendingObject(bill));
		}

		return result;
	}

	UploadDocumentsSendingAction CreateNewMessageSendingObject(AsycudaBill bill)
	{
		return new UploadDocumentsSendingAction(bill);
	}
}
