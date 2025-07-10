using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public sealed class TemporaryStorageMessageSendingObjectParent : EU.Business.CusTempStorage.TemporaryStorageMessageSendingObjectParent<TemporaryStorageMessageSendingObject, TemporaryStorageHeader>
{
	public TemporaryStorageMessageSendingObjectParent(TemporaryStorageHeader header) : base(header)
	{
	}

	public void SendMessage()
	{
		var sendingObject = SelectedSendingObjects.Cast<TemporaryStorageMessageSendingObject>().Single();
		var temporaryStorageHeader = sendingObject.Header;
		var messageCreationStrategy = new AidaXmlOutgoingCustomsMessageCreationStrategy(Factory, sendingObject);
		var messageSender = new ITMessageSender(Factory, messageCreationStrategy, new TemporaryStorageSendableCustomsEntry(temporaryStorageHeader));
		messageSender.Send();
	}

	public ZString CustomsProfile => header.AMA_CustomsProfile;

	protected override NonPersistentBusinessObjectCollection<TemporaryStorageMessageSendingObject> GetSendingObjectsCollectionCore()
	{
		messageSendingObjectCollection ??= new EU.Business.CusTempStorage.TemporaryStorageMessageSendingObjectCollection<TemporaryStorageMessageSendingObject, TemporaryStorageHeader>(header)
		{
			new TemporaryStorageMessageSendingObject(header)
		};

		RegisterEditableChildObject(messageSendingObjectCollection);
		return messageSendingObjectCollection;
	}

	NonPersistentBusinessObjectCollection<TemporaryStorageMessageSendingObject> messageSendingObjectCollection;

	public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties
	{
		get
		{
			yield return new MessageSendingObjectProperty(TemporaryStorageMessageSendingObject.Schema.MessageType, true);
			yield return new MessageSendingObjectProperty(TemporaryStorageMessageSendingObject.Schema.JobReferenceNumber, true);
			yield return new MessageSendingObjectProperty(TemporaryStorageMessageSendingObject.Schema.DeclarationType, true);
		}
	}
}
