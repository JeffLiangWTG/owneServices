using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Testing;

sealed class Ucc6JobDeclarationMessageSendingObjectValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckMessageTypeMandatoryValidation()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.MergedLines.AddNew();
		var messageSendingObjectParent = new Ucc6JobDeclarationMessageSendingObjectParent(declaration);
		var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];

		sendingObject.MessageType = "";
		AssertHasErrorContaining(sendingObject.MessageTypeInfo, MandatoryValidation.MustBeEntered);

		sendingObject.MessageType = "NEW";
		AssertNoErrorContaining(sendingObject.MessageTypeInfo, MandatoryValidation.MustBeEntered);
	}

	public void TestCheckMessageTypeListValidation()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.MergedLines.AddNew();
		var messageSendingObjectParent = new Ucc6JobDeclarationMessageSendingObjectParent(declaration);
		var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];

		sendingObject.MessageType = "XYZ";
		AssertHasErrorContaining(sendingObject.MessageTypeInfo, ListValidation.InvalidCodeError);

		sendingObject.MessageType = "NEW";
		AssertNoErrorContaining(sendingObject.MessageTypeInfo, ListValidation.InvalidCodeError);
	}

	public void TestCheckVOCReasonMandatoryValidation()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
		entryHeader1.MergedLines.AddNew();
		var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
		entryHeader2.MergedLines.AddNew();
		var messageSendingObjectParent = new Ucc6JobDeclarationMessageSendingObjectParent(declaration);
		var sendingObject1 = messageSendingObjectParent.SendingObjectsCollection[0];
		var sendingObject2 = messageSendingObjectParent.SendingObjectsCollection[1];
		sendingObject1.ShouldSend = true;

		CombineAssertions("Validations for ShouldSend selected for sendingObject", () =>
		{
			sendingObject1.MessageType = "CAN";
			sendingObject1.VOCReason = "";
			AssertHasErrorContaining(sendingObject1.VOCReasonInfo, MandatoryValidation.MustBeEntered);

			sendingObject1.VOCReason = "A";
			AssertNoErrorContaining(sendingObject1.VOCReasonInfo, MandatoryValidation.MustBeEntered);

			sendingObject1.MessageType = "AMD";
			sendingObject1.VOCReason = "";
			AssertHasErrorContaining(sendingObject1.VOCReasonInfo, MandatoryValidation.MustBeEntered);

			sendingObject1.VOCReason = "A";
			AssertNoErrorContaining(sendingObject1.VOCReasonInfo, MandatoryValidation.MustBeEntered);

			sendingObject1.MessageType = "";
			sendingObject1.VOCReason = "";
			AssertNoErrorContaining(sendingObject1.VOCReasonInfo, MandatoryValidation.MustBeEntered);
		});

		CombineAssertions("Validations for ShouldSend not selected for sendingObject", () =>
		{
			AssertEquals("Pre: ShouldSend", false, sendingObject2.ShouldSend);
			sendingObject2.MessageType = "CAN";
			sendingObject2.VOCReason = "";
			AssertNoErrorContaining(sendingObject2.VOCReasonInfo, MandatoryValidation.MustBeEntered);

			sendingObject2.VOCReason = "A";
			AssertNoErrorContaining(sendingObject2.VOCReasonInfo, MandatoryValidation.MustBeEntered);

			sendingObject2.MessageType = "AMD";
			sendingObject2.VOCReason = "";
			AssertNoErrorContaining(sendingObject2.VOCReasonInfo, MandatoryValidation.MustBeEntered);

			sendingObject2.VOCReason = "A";
			AssertNoErrorContaining(sendingObject2.VOCReasonInfo, MandatoryValidation.MustBeEntered);

			sendingObject2.MessageType = "";
			sendingObject2.VOCReason = "";
			AssertNoErrorContaining(sendingObject2.VOCReasonInfo, MandatoryValidation.MustBeEntered);
		});
	}

	public void TestCheckVOCReasonListValidation()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
		entryHeader1.MergedLines.AddNew();
		var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
		entryHeader2.MergedLines.AddNew();
		var messageSendingObjectParent = new Ucc6JobDeclarationMessageSendingObjectParent(declaration);
		var sendingObject1 = messageSendingObjectParent.SendingObjectsCollection[0];
		var sendingObject2 = messageSendingObjectParent.SendingObjectsCollection[1];
		sendingObject1.ShouldSend = true;

		CombineAssertions("Validations for ShouldSend selected for sendingObject", () =>
		{
			sendingObject1.MessageType = "CAN";
			sendingObject1.VOCReason = "X";
			AssertHasErrorContaining(sendingObject1.VOCReasonInfo, ListValidation.InvalidCodeError);

			sendingObject1.VOCReason = "A";
			AssertNoErrorContaining(sendingObject1.VOCReasonInfo, ListValidation.InvalidCodeError);

			sendingObject1.MessageType = "AMD";
			sendingObject1.VOCReason = "X";
			AssertHasErrorContaining(sendingObject1.VOCReasonInfo, ListValidation.InvalidCodeError);

			sendingObject1.VOCReason = "A";
			AssertNoErrorContaining(sendingObject1.VOCReasonInfo, ListValidation.InvalidCodeError);

			sendingObject1.MessageType = "";
			sendingObject1.VOCReason = "X";
			AssertNoErrorContaining(sendingObject1.VOCReasonInfo, ListValidation.InvalidCodeError);
		});

		CombineAssertions("Validations for ShouldSend not selected for sendingObject", () =>
		{
			AssertEquals("Pre: ShouldSend", false, sendingObject2.ShouldSend);
			sendingObject2.MessageType = "CAN";
			sendingObject2.VOCReason = "X";
			AssertNoErrorContaining(sendingObject2.VOCReasonInfo, ListValidation.InvalidCodeError);

			sendingObject2.VOCReason = "A";
			AssertNoErrorContaining(sendingObject2.VOCReasonInfo, ListValidation.InvalidCodeError);

			sendingObject2.MessageType = "AMD";
			sendingObject2.VOCReason = "X";
			AssertNoErrorContaining(sendingObject2.VOCReasonInfo, ListValidation.InvalidCodeError);

			sendingObject2.VOCReason = "A";
			AssertNoErrorContaining(sendingObject2.VOCReasonInfo, ListValidation.InvalidCodeError);

			sendingObject2.MessageType = "";
			sendingObject2.VOCReason = "X";
			AssertNoErrorContaining(sendingObject2.VOCReasonInfo, ListValidation.InvalidCodeError);
		});
	}

	public void TestCheckCancellationandAmendmentLegislativeReferenceMandatoryValidation()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
		entryHeader1.MergedLines.AddNew();
		var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
		entryHeader2.MergedLines.AddNew();
		var messageSendingObjectParent = new Ucc6JobDeclarationMessageSendingObjectParent(declaration);
		var sendingObject1 = messageSendingObjectParent.SendingObjectsCollection[0];
		var sendingObject2 = messageSendingObjectParent.SendingObjectsCollection[1];
		sendingObject1.ShouldSend = true;

		CombineAssertions("Validations for ShouldSend selected for sendingObject", () =>
		{
			sendingObject1.MessageType = "CAN";
			sendingObject1.CancellationAndAmendmentLegislativeReference = "";
			AssertHasErrorContaining(sendingObject1.CancellationAndAmendmentLegislativeReferenceInfo, MandatoryValidation.MustBeEntered);

			sendingObject1.CancellationAndAmendmentLegislativeReference = "1";
			AssertNoErrorContaining(sendingObject1.CancellationAndAmendmentLegislativeReferenceInfo, MandatoryValidation.MustBeEntered);

				sendingObject1.MessageType = "AMD";
				sendingObject1.CancellationAndAmendmentLegislativeReference = "";
				AssertNoErrorContaining(sendingObject1.CancellationAndAmendmentLegislativeReferenceInfo, MandatoryValidation.MustBeEntered);

			sendingObject1.CancellationAndAmendmentLegislativeReference = "1";
			AssertNoErrorContaining(sendingObject1.CancellationAndAmendmentLegislativeReferenceInfo, MandatoryValidation.MustBeEntered);

			sendingObject1.MessageType = "";
			sendingObject1.CancellationAndAmendmentLegislativeReference = "";
			AssertNoErrorContaining(sendingObject1.CancellationAndAmendmentLegislativeReferenceInfo, MandatoryValidation.MustBeEntered);
		});

		CombineAssertions("Validations for ShouldSend not selected for sendingObject", () =>
		{
			AssertEquals("Pre: ShouldSend", false, sendingObject2.ShouldSend);
			sendingObject2.MessageType = "CAN";
			sendingObject2.CancellationAndAmendmentLegislativeReference = "";
			AssertNoErrorContaining(sendingObject2.CancellationAndAmendmentLegislativeReferenceInfo, MandatoryValidation.MustBeEntered);

			sendingObject2.CancellationAndAmendmentLegislativeReference = "1";
			AssertNoErrorContaining(sendingObject2.CancellationAndAmendmentLegislativeReferenceInfo, MandatoryValidation.MustBeEntered);

			sendingObject2.MessageType = "AMD";
			sendingObject2.CancellationAndAmendmentLegislativeReference = "";
			AssertNoErrorContaining(sendingObject2.CancellationAndAmendmentLegislativeReferenceInfo, MandatoryValidation.MustBeEntered);

			sendingObject2.CancellationAndAmendmentLegislativeReference = "1";
			AssertNoErrorContaining(sendingObject2.CancellationAndAmendmentLegislativeReferenceInfo, MandatoryValidation.MustBeEntered);

			sendingObject2.MessageType = "";
			sendingObject2.CancellationAndAmendmentLegislativeReference = "";
			AssertNoErrorContaining(sendingObject2.CancellationAndAmendmentLegislativeReferenceInfo, MandatoryValidation.MustBeEntered);
		});
	}

	public void TestCheckCancellationAndAmendmentLegislativeReferenceListValidation()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
		entryHeader1.MergedLines.AddNew();
		var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
		entryHeader2.MergedLines.AddNew();
		var messageSendingObjectParent = new Ucc6JobDeclarationMessageSendingObjectParent(declaration);
		var sendingObject1 = messageSendingObjectParent.SendingObjectsCollection[0];
		var sendingObject2 = messageSendingObjectParent.SendingObjectsCollection[1];
		sendingObject1.ShouldSend = true;

		CombineAssertions("Validations for ShouldSend selected for sendingObject", () =>
		{
			sendingObject1.MessageType = "CAN";
			sendingObject1.CancellationAndAmendmentLegislativeReference = "A";
			AssertHasErrorContaining("When MessageType is CAN and CancellationAndAmendmentLegislativeReference is A", sendingObject1.CancellationAndAmendmentLegislativeReferenceInfo, ListValidation.InvalidCodeError);

			sendingObject1.CancellationAndAmendmentLegislativeReference = "2";
			AssertNoErrorContaining("When MessageType is CAN and CancellationAndAmendmentLegislativeReference is 2", sendingObject1.CancellationAndAmendmentLegislativeReferenceInfo, ListValidation.InvalidCodeError);

			sendingObject1.CancellationAndAmendmentLegislativeReference = "1";
			AssertHasErrorContaining("When MessageType is CAN and CancellationAndAmendmentLegislativeReference is 1", sendingObject1.CancellationAndAmendmentLegislativeReferenceInfo, ListValidation.InvalidCodeError);

			sendingObject1.MessageType = "AMD";
			sendingObject1.CancellationAndAmendmentLegislativeReference = "A";
			AssertHasErrorContaining("When MessageType is AMD and CancellationAndAmendmentLegislativeReference is A", sendingObject1.CancellationAndAmendmentLegislativeReferenceInfo, ListValidation.InvalidCodeError);

			sendingObject1.CancellationAndAmendmentLegislativeReference = "1";
			AssertNoErrorContaining("When MessageType is AMD and CancellationAndAmendmentLegislativeReference is 1", sendingObject1.CancellationAndAmendmentLegislativeReferenceInfo, ListValidation.InvalidCodeError);

			sendingObject1.CancellationAndAmendmentLegislativeReference = "2";
			AssertHasErrorContaining("When MessageType is AMD and CancellationAndAmendmentLegislativeReference is 2", sendingObject1.CancellationAndAmendmentLegislativeReferenceInfo, ListValidation.InvalidCodeError);

			sendingObject1.MessageType = "";
			sendingObject1.CancellationAndAmendmentLegislativeReference = "A";
			AssertNoErrorContaining("When MessageType is empty and CancellationAndAmendmentLegislativeReference is A", sendingObject1.CancellationAndAmendmentLegislativeReferenceInfo, ListValidation.InvalidCodeError);
		});

		CombineAssertions("Validations for ShouldSend not selected for sendingObject", () =>
		{
			AssertEquals("Pre: ShouldSend", false, sendingObject2.ShouldSend);
			sendingObject2.MessageType = "CAN";
			sendingObject2.CancellationAndAmendmentLegislativeReference = "A";
			AssertNoErrorContaining("When MessageType is CAN and CancellationAndAmendmentLegislativeReference is A", sendingObject2.CancellationAndAmendmentLegislativeReferenceInfo, ListValidation.InvalidCodeError);

			sendingObject2.CancellationAndAmendmentLegislativeReference = "1";
			AssertNoErrorContaining("When MessageType is CAN and CancellationAndAmendmentLegislativeReference is 1", sendingObject2.CancellationAndAmendmentLegislativeReferenceInfo, ListValidation.InvalidCodeError);

			sendingObject2.MessageType = "AMD";
			sendingObject2.CancellationAndAmendmentLegislativeReference = "A";
			AssertNoErrorContaining("When MessageType is AMD and CancellationAndAmendmentLegislativeReference is A", sendingObject2.CancellationAndAmendmentLegislativeReferenceInfo, ListValidation.InvalidCodeError);

			sendingObject2.CancellationAndAmendmentLegislativeReference = "1";
			AssertNoErrorContaining("When MessageType is AMD and CancellationAndAmendmentLegislativeReference is 1", sendingObject2.CancellationAndAmendmentLegislativeReferenceInfo, ListValidation.InvalidCodeError);

			sendingObject2.MessageType = "";
			sendingObject2.CancellationAndAmendmentLegislativeReference = "A";
			AssertNoErrorContaining("When MessageType is empty and CancellationAndAmendmentLegislativeReference is A", sendingObject2.CancellationAndAmendmentLegislativeReferenceInfo, ListValidation.InvalidCodeError);
		});
	}

	public void TestCheckShouldSendStatusAllowsSending()
	{
		const string expectedWarningMessage =
			"The Entry A00001 was already sent and is already registered or waiting for a message from Customs." +
			" Resending this entry could result in a duplicated declaration.";
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.MergedLines.AddNew();
		var messageSendingObjectParent = new Ucc6JobDeclarationMessageSendingObjectParent(declaration);
		var sendingObject = messageSendingObjectParent.SendingObjectsCollection[0];

		entryHeader.CH_BGMReference = "A00001";
		entryHeader.CH_Status = "";
		entryHeader.CH_EntryStatus = "";

		sendingObject.ShouldSend = ZBool.True;
		sendingObject.MessageType = "NEW";
		AssertNoWarningContaining(sendingObject.ShouldSendInfo, expectedWarningMessage);

		entryHeader.CH_Status = "AWO";
		sendingObject.Validation.ValidateShouldSend();
		AssertHasWarningContaining(sendingObject.ShouldSendInfo, expectedWarningMessage);

		sendingObject.MessageType = "CAN";
		AssertNoWarningContaining(sendingObject.ShouldSendInfo, expectedWarningMessage);

		sendingObject.MessageType = "NEW";
		AssertHasWarningContaining(sendingObject.ShouldSendInfo, expectedWarningMessage);
	}
}
