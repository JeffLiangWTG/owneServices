using System.Collections.Generic;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public class ExportJobDeclarationMessageSendingObjectParent : JobDeclarationMessageSendingObjectParent<ExportDeclarationMessageSendingObject>, IMessageSendingObjectParent
	{
		public ExportJobDeclarationMessageSendingObjectParent(JobDeclaration declaration) : base(declaration)
		{
		}

		public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

		protected override JobDeclarationMessageSendingObject CreateNewJobDeclarationMessageSendingObject(Customs.Business.CusEntryHeader header)
		{
			return new ExportDeclarationMessageSendingObject((CusEntryHeader)header);
		}

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => columnDefinitions;

		readonly IEnumerable<MessageSendingObjectProperty> columnDefinitions = new MessageSendingObjectProperty[]
		{
			new MessageSendingObjectProperty(nameof(ExportDeclarationMessageSendingObject.MessageType), true, 100),
			new MessageSendingObjectProperty(nameof(ExportDeclarationMessageSendingObject.SubmittedDate), true, 140),
			new MessageSendingObjectProperty(nameof(ExportDeclarationMessageSendingObject.CustomsStatus), true, 100),
			new MessageSendingObjectProperty(nameof(ExportDeclarationMessageSendingObject.VOCReason), true, 140),
			new MessageSendingObjectProperty(nameof(ExportDeclarationMessageSendingObject.MessageStatusDescription), false, 180),
		};
	}
}
