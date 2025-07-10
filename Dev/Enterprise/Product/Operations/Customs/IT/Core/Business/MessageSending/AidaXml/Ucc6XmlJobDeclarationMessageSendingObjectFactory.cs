using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml;

public sealed class Ucc6XmlJobDeclarationMessageSendingObjectFactory : JobDeclarationMessageSendingObjectAbstractFactory
{
	public Ucc6XmlJobDeclarationMessageSendingObjectFactory(CusEntryHeader entryHeader, JobDeclarationMessageSendingObjectParent sendingObjectParent) : base(entryHeader, sendingObjectParent)
	{
	}

	protected override JobDeclarationMessageSendingObject TryGetNewMessageSendingObjectCore()
	{
		var declaration = EntryHeader.Declaration;
		if (declaration.IsImport)
		{
			return new ImportMessageSendingObject(EntryHeader, SendingObjectParent);
		}
		if (declaration.IsExport)
		{
			return new ExportMessageSendingObject(EntryHeader, SendingObjectParent);
		}
		return null;
	}
}
