using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaManifestMessageSendingObjectParent : BaseMessageSendingObjectParent<AsycudaManifestMessageSendingObject>
	{
		public AsycudaManifestMessageSendingObjectParent(AsycudaManifestHeader header) : base(header.Factory)
		{
			this.header = Argument.NotNull(header, nameof(header));
		}

		public override BusinessObject TopLevelBusinessObject => header;

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties
		{
			get
			{
				yield return new MessageSendingObjectProperty(AsycudaManifestMessageSendingObject.Schema.MessageType, true);
				yield return new MessageSendingObjectProperty(AsycudaManifestMessageSendingObject.Schema.MessageSubType, true);
				yield return new MessageSendingObjectProperty(AsycudaManifestMessageSendingObject.Schema.MessageSubTypeDescription, true);
			}
		}

		public override SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.GlobalManifestSendWithMessageErrors;

		protected override NonPersistentBusinessObjectCollection<AsycudaManifestMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			messageSendingObjectCollection = new AsycudaManifestMessageSendingObjectCollection(header);
			messageSendingObjectCollection.AddNew();
			return messageSendingObjectCollection;
		}

		AsycudaManifestMessageSendingObjectCollection messageSendingObjectCollection;

		protected readonly AsycudaManifestHeader header;
	}
}
