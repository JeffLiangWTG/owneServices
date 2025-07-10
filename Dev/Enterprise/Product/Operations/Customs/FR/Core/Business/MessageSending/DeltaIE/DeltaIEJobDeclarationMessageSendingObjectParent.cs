using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessageSending
{
	public class DeltaIEJobDeclarationMessageSendingObjectParent : JobDeclarationMessageSendingObjectParent<DeltaIEJobDeclarationMessageSendingObject>
	{
		public DeltaIEJobDeclarationMessageSendingObjectParent(JobDeclaration declaration) : base(declaration)
		{
		}

		public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

		protected override Customs.Business.JobDeclarationMessageSendingObject CreateNewJobDeclarationMessageSendingObject(Customs.Business.CusEntryHeader header)
		{
			return new DeltaIEJobDeclarationMessageSendingObject((Declaration.CusEntryHeader)header);
		}

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => new List<MessageSendingObjectProperty>
		{
			new MessageSendingObjectProperty(DeltaIEJobDeclarationMessageSendingObject.Schema.Update, false, 50),
			new MessageSendingObjectProperty(EU.Business.JobDeclarationMessageSendingObject.Schema.MessageType, true, 70),
			new MessageSendingObjectProperty(EU.Business.JobDeclarationMessageSendingObject.Schema.EntryType, true, 70),
			new MessageSendingObjectProperty(DeltaIEJobDeclarationMessageSendingObject.Schema.SubStyle, true, 70),
			new MessageSendingObjectProperty(DeltaIEJobDeclarationMessageSendingObject.Schema.Description, true, 175),
			new MessageSendingObjectProperty(DeltaIEJobDeclarationMessageSendingObject.Schema.DateTime, true, 100),
			new MessageSendingObjectProperty(EU.Business.JobDeclarationMessageSendingObject.Schema.EntryStatus, true, 70),
			new MessageSendingObjectProperty(DeltaIEJobDeclarationMessageSendingObject.Schema.ChangeAcknowledgementIndicator, true, 70),
			new MessageSendingObjectProperty(DeltaIEJobDeclarationMessageSendingObject.Schema.VOCReason, true, 170),
		};

		public IEnumerable<DeltaIEJobDeclarationMessageSendingObject> ObjectsToSend
		{
			get => SendingObjectsCollection.OfType<DeltaIEJobDeclarationMessageSendingObject>().Where(x => x.ShouldSend);
		}
	}
}
