using System.Collections.Generic;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.IE.PBN.Business
{
	public class PBNMessageSendingObjectParent : BaseMessageSendingObjectParent<PBNMessageSendingObject>
	{
		public PBNMessageSendingObjectParent(AsycudaManifestHeader header) : base(header.Factory)
		{
			Header = Argument.NotNull(header, nameof(header));
		}

		public AsycudaManifestHeader Header { get; }

		public override BusinessObject TopLevelBusinessObject => Header;

		public override SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.GlobalManifestSendWithMessageErrors;

		protected override NonPersistentBusinessObjectCollection<PBNMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			return new PBNMessageSendingObjectCollection(Factory)
			{
				new PBNMessageSendingObject(Header)
			};
		}

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => new MessageSendingObjectProperty[]
		{
			new(nameof(PBNMessageSendingObject.MessageType), ismandatory: true, columnWidth: 85),
			new(nameof(PBNMessageSendingObject.JobNumber), ismandatory: true, columnWidth: 110),
			new(nameof(PBNMessageSendingObject.MessageStatus), ismandatory: true, columnWidth: 90),
		};
	}
}
