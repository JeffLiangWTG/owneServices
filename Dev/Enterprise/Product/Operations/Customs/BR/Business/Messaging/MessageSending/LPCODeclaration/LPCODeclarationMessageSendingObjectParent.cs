using System.Collections.Generic;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public class LPCODeclarationMessageSendingObjectParent : JobDeclarationMessageSendingObjectParent<LPCODeclarationMessageSendingObject>, IMessageSendingObjectParent
	{
		public LPCODeclarationMessageSendingObjectParent(JobDeclaration declaration) : base(declaration)
		{
		}

		public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

		protected override JobDeclarationMessageSendingObject CreateNewJobDeclarationMessageSendingObject(Customs.Business.CusEntryHeader header)
		{
			return new LPCODeclarationMessageSendingObject((CusEntryHeader)header);
		}

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => columnDefinitions;

		readonly IEnumerable<MessageSendingObjectProperty> columnDefinitions = new MessageSendingObjectProperty[]
		{
			new MessageSendingObjectProperty(nameof(LPCODeclarationMessageSendingObject.MessageType), true, 100),
			new MessageSendingObjectProperty(nameof(LPCODeclarationMessageSendingObject.SubmittedDate), true, 140),
			new MessageSendingObjectProperty(nameof(LPCODeclarationMessageSendingObject.CustomsStatus), true, 100),
			new MessageSendingObjectProperty(nameof(LPCODeclarationMessageSendingObject.Reason), true, 140),
			new MessageSendingObjectProperty(nameof(LPCODeclarationMessageSendingObject.Requirement), true, 140),
			new MessageSendingObjectProperty(nameof(LPCODeclarationMessageSendingObject.EntryNumber), true, 100),
			new MessageSendingObjectProperty(nameof(LPCODeclarationMessageSendingObject.EntryLineNumber), true, 100),
			new MessageSendingObjectProperty(nameof(LPCODeclarationMessageSendingObject.Version), true, 80),
			new MessageSendingObjectProperty(nameof(LPCODeclarationMessageSendingObject.MessageStatusDescription), false, 180),
			new MessageSendingObjectProperty(nameof(LPCODeclarationMessageSendingObject.NewEffectiveDate), true, 120),
			new MessageSendingObjectProperty(nameof(LPCODeclarationMessageSendingObject.Message), true, 120),
		};
	}
}
