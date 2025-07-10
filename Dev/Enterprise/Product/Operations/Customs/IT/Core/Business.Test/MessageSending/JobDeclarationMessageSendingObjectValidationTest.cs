using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class JobDeclarationMessageSendingObjectValidationTest : TestCaseWithFactory
{
	public void TestCheckShouldSendStatusAllowsSending()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = entryLine.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_JZ = invoiceHeader.PK;

		entryHeader.CH_BGMReference = "A00001";
		entryHeader.CH_Status = "";
		entryHeader.CH_EntryStatus = "";

		var imMessageSendingObject = new IMMessageSendingObject(entryHeader, new JobDeclarationMessageSendingObjectParent(declaration));
		var validation = imMessageSendingObject.Validation;

		var expectedWarningMessage = "The Entry A00001 was already sent and is already registered or waiting for a message from Customs. Resending this entry could result in a duplicated declaration.";

		imMessageSendingObject.ShouldSend = ZBool.True;
		AssertNoWarningContaining(imMessageSendingObject.ShouldSendInfo, expectedWarningMessage);

		entryHeader.CH_Status = "AWO";
		validation.ValidateShouldSend();
		AssertHasWarningContaining(imMessageSendingObject.ShouldSendInfo, expectedWarningMessage);

		entryHeader.CH_Status = "ACO";
		entryHeader.CH_EntryStatus = "ICC";
		validation.ValidateShouldSend();
		AssertHasWarningContaining(imMessageSendingObject.ShouldSendInfo, expectedWarningMessage);

		entryHeader.CH_EntryStatus = "";
		entryHeader.CH_Status = "ERO";
		validation.ValidateShouldSend();
		AssertNoWarningContaining(imMessageSendingObject.ShouldSendInfo, expectedWarningMessage);

		entryHeader.CH_Status = "ACO";
		entryHeader.CH_EntryStatus = "NBR";
		validation.ValidateShouldSend();
		AssertNoWarningContaining(imMessageSendingObject.ShouldSendInfo, expectedWarningMessage);

		entryHeader.CH_Status = "AWO";
		entryHeader.CH_EntryStatus = "NBR";
		validation.ValidateShouldSend();
		AssertHasWarningContaining(imMessageSendingObject.ShouldSendInfo, expectedWarningMessage);

		entryHeader.CH_Status = "FFT";
		validation.ValidateShouldSend();
		AssertNoWarningContaining(imMessageSendingObject.ShouldSendInfo, expectedWarningMessage);

		entryHeader.CH_Status = "ACO";
		entryHeader.CH_EntryStatus = "DEP";
		validation.ValidateShouldSend();
		AssertNoWarningContaining(imMessageSendingObject.ShouldSendInfo, expectedWarningMessage);
	}

	public void TestCheckShouldSendEntryHeaderDoesNotHaveAnAssociatedEntryInstruction()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.MergedLines.AddNew();
		var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(declaration);
		var singleMessageSendingObject = messageSendingObjectParent.SendingObjectsCollection[0];

		CombineAssertions("When the entry header does not have an associated entry instruction", () =>
		{
			singleMessageSendingObject.ShouldSend = true;
			AssertHasErrorContaining(singleMessageSendingObject.ShouldSendInfo, ValidationCaptions.MessageSendingObject.SelectedEntryDoesNotHaveAnAssociatedEntryInstruction);
			singleMessageSendingObject.ShouldSend = false;
			AssertNoErrorContaining(singleMessageSendingObject.ShouldSendInfo, ValidationCaptions.MessageSendingObject.SelectedEntryDoesNotHaveAnAssociatedEntryInstruction);
		});

		CombineAssertions("When the entry header has an associated entry instruction", () =>
		{
			entryHeader.CH_CEI_Instruction = declaration.CustomsEntryInstructions.AddNew().PK;
			singleMessageSendingObject.ShouldSend = true;
			AssertNoErrorContaining(singleMessageSendingObject.ShouldSendInfo, ValidationCaptions.MessageSendingObject.SelectedEntryDoesNotHaveAnAssociatedEntryInstruction);
			singleMessageSendingObject.ShouldSend = false;
			AssertNoErrorContaining(singleMessageSendingObject.ShouldSendInfo, ValidationCaptions.MessageSendingObject.SelectedEntryDoesNotHaveAnAssociatedEntryInstruction);
		});
	}
}
