using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.PBN.Business
{
	public sealed class PBNMessageSendingObject : AutoPBNMessageSendingObject, IMessageSendingAction
	{
		public PBNMessageSendingObject(AsycudaManifestHeader header) : base(header.Factory)
		{
			this.Header = header;
			JobNumber = header.RegistrationNumber;
		}
		public readonly AsycudaManifestHeader Header;

		[List(nameof(Lookups) + "." + nameof(PBNMessageSendingObjectLookups.MessageTypes))]
		public override ZString MessageType { get => base.MessageType; set => base.MessageType = value; }

		public PBNMessageSendingObjectLookups Lookups => lookups ??= new PBNMessageSendingObjectLookups(this);
		PBNMessageSendingObjectLookups lookups;

		public PBNMessageSender CreateSender() => new PBNMessageSender(this);

		public IMessageAttachee MessageAttachee => Header;

		public void AddMessage(OutboundEDIMessage message)
		{
			Header.Messages.Add(message);
		}
	}
}
