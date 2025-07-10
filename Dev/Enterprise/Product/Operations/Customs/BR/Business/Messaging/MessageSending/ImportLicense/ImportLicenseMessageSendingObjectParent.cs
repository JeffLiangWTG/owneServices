using System.Collections.Generic;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public class ImportLicenseMessageSendingObjectParent : JobDeclarationMessageSendingObjectParent<ImportLicenseMessageSendingObject>, IMessageSendingObjectParent
	{
		public ImportLicenseMessageSendingObjectParent(JobDeclaration declaration) : base(declaration)
		{
		}

		public new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

		protected override JobDeclarationMessageSendingObject CreateNewJobDeclarationMessageSendingObject(Customs.Business.CusEntryHeader header)
		{
			return new ImportLicenseMessageSendingObject((CusEntryHeader)header);
		}

		public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => columnDefinitions;

		readonly IEnumerable<MessageSendingObjectProperty> columnDefinitions = new MessageSendingObjectProperty[]
		{
			new MessageSendingObjectProperty(nameof(ImportLicenseMessageSendingObject.MessageType), true, 100),
			new MessageSendingObjectProperty(nameof(ImportLicenseMessageSendingObject.SubmittedDate), true, 140),
			new MessageSendingObjectProperty(nameof(ImportLicenseMessageSendingObject.CustomsStatus), true, 100),
			new MessageSendingObjectProperty(nameof(ImportLicenseMessageSendingObject.MessageStatusDescription), false, 180),
			new MessageSendingObjectProperty(nameof(ImportLicenseMessageSendingObject.EntryInstructionDescription), false, 180),
			new MessageSendingObjectProperty(nameof(ImportLicenseMessageSendingObject.LocalReferenceNumber), false, 180),
		};
	}
}
