using System.Collections.Generic;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.MX.Business
{
	public class DeclarationMessageSendingObjectParent : JobDeclarationMessageSendingObjectParent<DeclarationMessageSendingObject>
	{
		public DeclarationMessageSendingObjectParent(BaseJobDeclaration declaration) : base(declaration)
		{
		}

		public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

		protected override JobDeclarationMessageSendingObject CreateNewJobDeclarationMessageSendingObject(Customs.Business.CusEntryHeader header)
		{
			return new DeclarationMessageSendingObject((CusEntryHeader)header);
		}

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => columnDefinitions;

		readonly IEnumerable<MessageSendingObjectProperty> columnDefinitions = new MessageSendingObjectProperty[]
		{
			new MessageSendingObjectProperty(nameof(DeclarationMessageSendingObject.MessageType), true, 100),
		};
	}
}
