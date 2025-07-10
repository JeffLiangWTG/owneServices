using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business.CusTempStorage
{
	public class TemporaryStorageMessageSendingObjectParent : EU.Business.CusTempStorage.TemporaryStorageMessageSendingObjectParent<TemporaryStorageMessageSendingObject, TemporaryStorageHeader>
	{
		public TemporaryStorageMessageSendingObjectParent(TemporaryStorageHeader header) : base(header)
		{
		}

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties
		{
			get
			{
				yield return new MessageSendingObjectProperty(TemporaryStorageMessageSendingObject.Schema.MessageType, true);
				yield return new MessageSendingObjectProperty(TemporaryStorageMessageSendingObject.Schema.DeclarationType, false);
				yield return new MessageSendingObjectProperty(TemporaryStorageMessageSendingObject.Schema.EntryStatus, false);
				yield return new MessageSendingObjectProperty(TemporaryStorageMessageSendingObject.Schema.MessageStatus, false);
				yield return new MessageSendingObjectProperty(TemporaryStorageMessageSendingObject.Schema.AlternativeDateOfAcceptance, false);
				yield return new MessageSendingObjectProperty(TemporaryStorageMessageSendingObject.Schema.CustomsReference, false);
				yield return new MessageSendingObjectProperty(TemporaryStorageMessageSendingObject.Schema.CustomsJustification, false);
			}
		}

		protected override NonPersistentBusinessObjectCollection<TemporaryStorageMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			if (messageSendingObjectCollection == null)
			{
				messageSendingObjectCollection = new EU.Business.CusTempStorage.TemporaryStorageMessageSendingObjectCollection<TemporaryStorageMessageSendingObject, TemporaryStorageHeader>(header);
				messageSendingObjectCollection.Add(GetSingleMessageSendingObject());
			}

			RegisterEditableChildObject(messageSendingObjectCollection);
			return messageSendingObjectCollection;
		}
		EU.Business.CusTempStorage.TemporaryStorageMessageSendingObjectCollection<TemporaryStorageMessageSendingObject, TemporaryStorageHeader> messageSendingObjectCollection;

		TemporaryStorageMessageSendingObject GetSingleMessageSendingObject()
		{
			return new TemporaryStorageMessageSendingObject(header);
		}
	}
}
