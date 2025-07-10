using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business.CusTempStorage;

public class TemporaryStorageMessageSendingObjectParent : EU.Business.CusTempStorage.TemporaryStorageMessageSendingObjectParent<TemporaryStorageMessageSendingObject, TemporaryStorageHeader>
{
	public TemporaryStorageMessageSendingObjectParent(TemporaryStorageHeader header) : base(header)
	{
	}

	NonPersistentBusinessObjectCollection<TemporaryStorageMessageSendingObject> messageSendingObjectCollection;

	protected override NonPersistentBusinessObjectCollection<TemporaryStorageMessageSendingObject> GetSendingObjectsCollectionCore()
	{
		if (messageSendingObjectCollection == null)
		{
			messageSendingObjectCollection = new TemporaryStorageMessageSendingObjectCollection(header);
			messageSendingObjectCollection.Add(GetSingleMessageSendingObject());
		}

		RegisterEditableChildObject(messageSendingObjectCollection);
		return messageSendingObjectCollection;
	}

	TemporaryStorageMessageSendingObject GetSingleMessageSendingObject() => new TemporaryStorageMessageSendingObject(header);

	public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties
	{
		get
		{
			yield return new MessageSendingObjectProperty(TemporaryStorageMessageSendingObject.Schema.DeclarationType, true, 100);
			yield return new MessageSendingObjectProperty(TemporaryStorageMessageSendingObject.BeSchema.IsTestDeclaration, columnWidth: 50);
			yield return new MessageSendingObjectProperty(TemporaryStorageMessageSendingObject.Schema.MessageType, true, 75, Res.GetData("79D22704-A1E7-426D-8DFD-75B84665BA2A", "Entry Type"));
			yield return new MessageSendingObjectProperty(TemporaryStorageMessageSendingObject.Schema.EntryStatus, true, 100);
			yield return new MessageSendingObjectProperty(TemporaryStorageMessageSendingObject.BeSchema.ReferenceNumber, true);
		}
	}
}
