using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class JobDeclarationMessageSendingObjectFactoryTest : JobDeclarationMessageSendingObjectAbstractFactoryTest
{
	protected override JobDeclarationMessageSendingObjectAbstractFactory GetNewSendingObjectFactory(CusEntryHeader entryHeader, JobDeclarationMessageSendingObjectParent sendingObjectParent)
	{
		return new JobDeclarationMessageSendingObjectFactory(entryHeader, sendingObjectParent);
	}

	public override void TestTryGetNewMessageSendingObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		instruction.CEI_JE = declaration.PK;

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;

		AddPreviousDocumentsForNb(declaration, invoiceLine);

		var entry = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entry.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		entry.CH_JE = declaration.PK;
		entry.CH_CEI_Instruction = instruction.PK;

		declaration.JE_MessageType = "IMP";
		AssertResultOfGetMessageSendingObject<IMMessageSendingObject>("When MessageType is IMP", entry, declaration);

		declaration.JE_MessageType = "EXP";
		AssertResultOfGetMessageSendingObject<ETMessageSendingObject>("When MessageType is EXP", entry, declaration);
	}

	void AssertResultOfGetMessageSendingObject<T>(string assertionMessage, CusEntryHeader entry, JobDeclaration declaration)
		where T : JobDeclarationMessageSendingObject
	{
		var jobDeclarationMessageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(declaration);
		var jobDeclarationMessageSendingObjectFactory = GetNewSendingObjectFactory(entry, jobDeclarationMessageSendingObjectParent);
		var jobDeclarationMessageSendingObject = jobDeclarationMessageSendingObjectFactory.TryGetNewMessageSendingObject();
		AssertNotNull($"{assertionMessage}, result of GetMessageSendingObject()", jobDeclarationMessageSendingObject);
		AssertType<T>($"{assertionMessage}, result GetMessageSendingObject()", jobDeclarationMessageSendingObject);
	}

	void AddPreviousDocumentsForNb(JobDeclaration declaration, JobComInvoiceLine invoiceLine)
	{
		var previousDocument1 = invoiceLine.PreviousDocuments.AddNew();
		previousDocument1.CSI_Procedure = "A3";
		var previousDocument2 = invoiceLine.PreviousDocuments.AddNew();
		previousDocument2.CSI_Procedure = "MRN";
		declaration.ResetApportionedPreviousDocuments();
	}
}
