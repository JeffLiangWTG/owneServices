using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaManifestQueryMessageSendingObjectParent : BaseMessageSendingObjectParent<AsycudaManifestQueryMessageSendingObject>
	{
		public AsycudaManifestQueryMessageSendingObjectParent(AsycudaManifestHeader header) : base(header.Factory)
		{
			this.header = Argument.NotNull(header, nameof(header));
		}

		public override BusinessObject TopLevelBusinessObject => header;

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties
		{
			get
			{
				yield return new MessageSendingObjectProperty(AsycudaManifestQueryMessageSendingObject.Schema.MessageType, true);
				yield return new MessageSendingObjectProperty(AsycudaManifestQueryMessageSendingObject.Schema.MessageSubType, true);
				yield return new MessageSendingObjectProperty(AsycudaManifestQueryMessageSendingObject.Schema.MessageSubTypeDescription, true);
				yield return new MessageSendingObjectProperty(AsycudaManifestQueryMessageSendingObject.Schema.ManifestNumber, true);
				yield return new MessageSendingObjectProperty(AsycudaManifestQueryMessageSendingObject.Schema.ParentDealNumber, true);
			}
		}

		public override SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.GlobalManifestSendWithMessageErrors;

		protected override NonPersistentBusinessObjectCollection<AsycudaManifestQueryMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			messageSendingObjectCollection = new AsycudaManifestQueryMessageSendingObjectCollection(header);
			messageSendingObjectCollection.AddNew();
			return messageSendingObjectCollection;
		}

		AsycudaManifestQueryMessageSendingObjectCollection messageSendingObjectCollection;

		protected readonly AsycudaManifestHeader header;
	}
}
