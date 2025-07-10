using System.Collections.Generic;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Business
{
	public class JobDeclarationMessageSendingObjectParent : JobDeclarationMessageSendingObjectParent<JobDeclarationMessageSendingObject>
	{
		public JobDeclarationMessageSendingObjectParent(JobDeclaration declaration) : base(declaration)
		{
		}

		public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

		protected override Customs.Business.JobDeclarationMessageSendingObject CreateNewJobDeclarationMessageSendingObject(Customs.Business.CusEntryHeader header)
		{
			return new JobDeclarationMessageSendingObject((CusEntryHeader)header);
		}

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => new List<MessageSendingObjectProperty>
		{
			new MessageSendingObjectProperty(JobDeclarationMessageSendingObject.Schema.MessageType, ismandatory: true, 80),
			new MessageSendingObjectProperty(JobDeclarationMessageSendingObject.Schema.MessageTypeDescription, ismandatory: true, 120),
			new MessageSendingObjectProperty(JobDeclarationMessageSendingObject.Schema.DeclarationType, ismandatory: true, 80),
			new MessageSendingObjectProperty(JobDeclarationMessageSendingObject.Schema.EntryStatus, ismandatory: true, 80),
			new MessageSendingObjectProperty(JobDeclarationMessageSendingObject.Schema.Procedure, ismandatory: true, 100),
			new MessageSendingObjectProperty(JobDeclarationMessageSendingObject.Schema.EntryInstructionDescription, ismandatory: true, 100)
		};
	}
}
