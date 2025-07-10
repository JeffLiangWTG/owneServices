using System.Collections.Generic;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public class ImportSiscomexMessageSendingObjectParent : JobDeclarationMessageSendingObjectParent<ImportSiscomexMessageSendingObject>, IMessageSendingObjectParent
	{
		public ImportSiscomexMessageSendingObjectParent(JobDeclaration declaration) : base(declaration)
		{
		}

		public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

		protected override JobDeclarationMessageSendingObject CreateNewJobDeclarationMessageSendingObject(Customs.Business.CusEntryHeader header)
		{
			return new ImportSiscomexMessageSendingObject((CusEntryHeader)header);
		}

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => columnDefinitions;

		readonly IEnumerable<MessageSendingObjectProperty> columnDefinitions = new MessageSendingObjectProperty[]
		{
			new MessageSendingObjectProperty(nameof(ImportSiscomexMessageSendingObject.MessageType), true, 100),
			new MessageSendingObjectProperty(nameof(ImportSiscomexMessageSendingObject.SubmittedDate), true, 140),
			new MessageSendingObjectProperty(nameof(ImportSiscomexMessageSendingObject.CustomsStatus), true, 100),
			new MessageSendingObjectProperty(nameof(ImportSiscomexMessageSendingObject.MessageStatusDescription), false, 180),
		};
	}
}
