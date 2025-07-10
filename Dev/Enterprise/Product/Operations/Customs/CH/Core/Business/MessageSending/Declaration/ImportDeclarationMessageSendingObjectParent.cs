using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class ImportDeclarationMessageSendingObjectParent : DeclarationMessageSendingObjectParent<ImportDeclarationMessageSendingObject>
{
	public ImportDeclarationMessageSendingObjectParent(JobDeclaration declaration)
		: base(declaration)
	{
	}

	protected override JobDeclarationMessageSendingObject CreateNewJobDeclarationMessageSendingObject(Customs.Business.CusEntryHeader header) => new ImportDeclarationMessageSendingObject((CusEntryHeader)header);

	public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => columnDefinitions;

	readonly IEnumerable<MessageSendingObjectProperty> columnDefinitions = new MessageSendingObjectProperty[]
	{
			new MessageSendingObjectProperty(DeclarationMessageSendingObject.Schema.MessageType, true, 160),
			new MessageSendingObjectProperty(DeclarationMessageSendingObject.Schema.VOCReason, true, 160),
			new MessageSendingObjectProperty(DeclarationMessageSendingObject.Schema.DeclarationType, true, 100),
			new MessageSendingObjectProperty(DeclarationMessageSendingObject.Schema.SubStyle, true, 100),
			new MessageSendingObjectProperty(DeclarationMessageSendingObject.Schema.Description, false, 200),
			new MessageSendingObjectProperty(DeclarationMessageSendingObject.Schema.LocalReferenceNumber, true, 180),
			new MessageSendingObjectProperty(DeclarationMessageSendingObject.Schema.EntryStatus, true, 100),
	};

	protected override ZString CheckMessageSendingEnvironment() => EnvironmentHelper.CheckMessageSendingEnvironmentForEdec();
}
