using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.TemporaryStorage.Business;

public sealed class G5TemporaryStorageMessageSendingObjectParent : EU.Business.CusTempStorage.TemporaryStorageMessageSendingObjectParent<G5TemporaryStorageMessageSendingObject, TemporaryStorageHeader>
{
	public G5TemporaryStorageMessageSendingObjectParent(G5MessageSendingObject sendingObject) : base(sendingObject.Header)
	{
		this.sendingObject = sendingObject;
	}

	readonly G5MessageSendingObject sendingObject;

	public ICertificateProvider CertificateData => sendingObject;

	public ZBool ShouldEditMessage => sendingObject.ShouldEditMessage;

	public new EU.Business.CusTempStorage.TemporaryStorageMessageSendingObjectCollection<G5TemporaryStorageMessageSendingObject, TemporaryStorageHeader> SendingObjectsCollection
		=> (EU.Business.CusTempStorage.TemporaryStorageMessageSendingObjectCollection<G5TemporaryStorageMessageSendingObject, TemporaryStorageHeader>)base.SendingObjectsCollection;

	protected override NonPersistentBusinessObjectCollection<G5TemporaryStorageMessageSendingObject> GetSendingObjectsCollectionCore()
	{
		var sendingObjectCollection = new EU.Business.CusTempStorage.TemporaryStorageMessageSendingObjectCollection<G5TemporaryStorageMessageSendingObject, TemporaryStorageHeader>(header);
		sendingObjectCollection.Add(new G5TemporaryStorageMessageSendingObject(header));
		return sendingObjectCollection;
	}

	public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties
	{
		get
		{
			yield return new MessageSendingObjectProperty(G5TemporaryStorageMessageSendingObject.Schema.LRN, true, 100);
			yield return new MessageSendingObjectProperty(G5TemporaryStorageMessageSendingObject.Schema.EntryStatus, true, 100);
			yield return new MessageSendingObjectProperty(G5TemporaryStorageMessageSendingObject.Schema.MRN, true, 100);
			yield return new MessageSendingObjectProperty(G5TemporaryStorageMessageSendingObject.Schema.MessageSubType, true, 100);
			yield return new MessageSendingObjectProperty(G5TemporaryStorageMessageSendingObject.Schema.MessageStatus, true, 100);
			yield return new MessageSendingObjectProperty(G5TemporaryStorageMessageSendingObject.Schema.MessageType, true, 100);
		}
	}
}
