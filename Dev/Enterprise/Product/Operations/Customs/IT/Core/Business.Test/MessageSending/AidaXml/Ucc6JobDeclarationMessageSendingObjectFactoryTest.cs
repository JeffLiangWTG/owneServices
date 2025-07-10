using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;
using Enterprise.Customs.IT.Business.Testing;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Testing;

sealed class Ucc6JobDeclarationMessageSendingObjectFactoryTest : JobDeclarationMessageSendingObjectAbstractFactoryTest
{
	protected override JobDeclarationMessageSendingObjectAbstractFactory GetNewSendingObjectFactory(CusEntryHeader entryHeader, JobDeclarationMessageSendingObjectParent sendingObjectParent)
	{
		return new Ucc6XmlJobDeclarationMessageSendingObjectFactory(entryHeader, sendingObjectParent);
	}

	public override void TestTryGetNewMessageSendingObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var sendingObjectParent = new JobDeclarationMessageSendingObjectParent(declaration);
		var sendingObjectFactory = new Ucc6XmlJobDeclarationMessageSendingObjectFactory(entryHeader, sendingObjectParent);

		declaration.JE_MessageType = "IMP";
		AssertType<ImportMessageSendingObject>("Sending Object Type", sendingObjectFactory.TryGetNewMessageSendingObject());

		declaration.JE_MessageType = "EXP";
		AssertType<ExportMessageSendingObject>("Sending Object Type", sendingObjectFactory.TryGetNewMessageSendingObject());

		declaration.JE_MessageType = "XXX";
		AssertNull("Sending Object", sendingObjectFactory.TryGetNewMessageSendingObject());
	}
}
