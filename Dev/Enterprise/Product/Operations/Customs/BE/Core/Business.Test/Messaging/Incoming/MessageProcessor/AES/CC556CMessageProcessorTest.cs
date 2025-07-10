using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Common.EU;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CC556CMessageProcessor))]
sealed class CC556CMessageProcessorTest : MessageProcessorTestCase<CC556CMessageProcessor, ICC556CDataProvider>
{
	public void TestMessageTypesToInclude()
	{
		AssertSequencesEqual(new ZString[] { BEIncomingMessageSubTypes.Codes.CC556C }, processor.MessageTypesToInclude);
	}

	#region TestValidateCC556CMessageDeclaration

	public void TestValidateCC556CMessageDeclaration_MatchingLRN() => TestValidateCC556CMessageDeclaration("1234567890", "");

	public void TestValidateCC556CMessageDeclaration_MatchingMRN() => TestValidateCC556CMessageDeclaration("", "1234567890T");
	void TestValidateCC556CMessageDeclaration(string lrn, string mrn)
	{
		entry.CH_EntryStatus = "ABC";
		mockProvider.Setup(x => x.LRN).Returns(lrn);
		mockProvider.Setup(x => x.MRN).Returns(mrn);
		mockProvider.Setup(x => x.BusinessRejectionType).Returns("123");
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);

		AssertEquals("EDIMEssage Status Should BE FAL", EDIMessageStatusList.Codes.Failed, incomingMessage.EM_Status);
	}

	#endregion

	public void TestDiscardMessageWithHeaderStatusNotMet()
	{
		entry.CH_EntryStatus = "ABC";
		mockProvider.Setup(x => x.BusinessRejectionType).Returns(ExportOperationBusinessRejectionTypeList.Codes.PresentationNotificationRejection);
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);
		CombineAssertions(() =>
		{
			AssertEquals("EDIMEssage Status Should BE DCD(=Discarded)", EDIMessageStatusList.Codes.Discarded, incomingMessage.EM_Status);
			AssertEquals("Processing Log", "The message with interchange is discarded, because the present is not empty, ACK, PRN, INR or AMR.", incomingMessage.Notes.FindByDescription(Constants.MessageProcessingNotes.ProcessingLog).Single(x => x.ST_IsCustomDescription).ST_NoteText);
		});
	}

	#region TestUpdateHeaderStatusWithRejectType511

	public void TestUpdateHeaderStatusWithRejectType_For511ACK() => TestUpdateHeaderStatusWithRejectType511(StatusCodes.ACK);

	public void TestUpdateHeaderStatusWithRejectType_For511PRN() => TestUpdateHeaderStatusWithRejectType511(StatusCodes.Presented);

	public void TestUpdateHeaderStatusWithRejectType_For511INR() => TestUpdateHeaderStatusWithRejectType511(StatusCodes.InvalidationRequest);

	public void TestUpdateHeaderStatusWithRejectType_For511AMR() => TestUpdateHeaderStatusWithRejectType511(StatusCodes.AmendmentRequest);

	public void TestUpdateHeaderStatusWithRejectType_For511Empty() => TestUpdateHeaderStatusWithRejectType511(String.Empty);
	void TestUpdateHeaderStatusWithRejectType511(string headerStatus)
	{
		entry.CH_EntryStatus = headerStatus;
		mockProvider.Setup(x => x.BusinessRejectionType).Returns(ExportOperationBusinessRejectionTypeList.Codes.PresentationNotificationRejection);
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);
		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("CusEntryHeader Status Should BE RJP(=rejection presentation notification)", StatusCodes.RejectedPresentation, ((CusEntryHeader)incomingMessage.EM_LinkedObject).CH_EntryStatus);
		});
	}

	#endregion

	#region TestUpdateHeaderStatusWithRejectType513

	public void TestUpdateHeaderStatusWithRejectType_For513ACK() => TestUpdateHeaderStatusWithRejectType513(StatusCodes.ACK);

	public void TestUpdateHeaderStatusWithRejectType_For513PRN() => TestUpdateHeaderStatusWithRejectType513(StatusCodes.Presented);

	public void TestUpdateHeaderStatusWithRejectType_For513INR() => TestUpdateHeaderStatusWithRejectType513(StatusCodes.InvalidationRequest);

	public void TestUpdateHeaderStatusWithRejectType_For513AMR() => TestUpdateHeaderStatusWithRejectType513(StatusCodes.AmendmentRequest);

	public void TestUpdateHeaderStatusWithRejectType_For513Empty() => TestUpdateHeaderStatusWithRejectType513(String.Empty);

	void TestUpdateHeaderStatusWithRejectType513(string headerStatus)
	{
		entry.CH_EntryStatus = headerStatus;
		mockProvider.Setup(x => x.BusinessRejectionType).Returns(ExportOperationBusinessRejectionTypeList.Codes.DeclarationAmendmentRejection);
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);
		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("CusEntryHeader Status Should BE RJA(=rejection amendment)", StatusCodes.RejectedAmendment, ((CusEntryHeader)incomingMessage.EM_LinkedObject).CH_EntryStatus);
		});
	}

	#endregion

	#region TestUpdateHeaderStatusWithRejectType514

	public void TestUpdateHeaderStatusWithRejectType_For514ACK() => TestUpdateHeaderStatusWithRejectType514(StatusCodes.ACK);

	public void TestUpdateHeaderStatusWithRejectType_For514PRN() => TestUpdateHeaderStatusWithRejectType514(StatusCodes.Presented);

	public void TestUpdateHeaderStatusWithRejectType_For514INR() => TestUpdateHeaderStatusWithRejectType514(StatusCodes.InvalidationRequest);

	public void TestUpdateHeaderStatusWithRejectType_For514AMR() => TestUpdateHeaderStatusWithRejectType514(StatusCodes.AmendmentRequest);
	public void TestUpdateHeaderStatusWithRejectType_For514Empty() => TestUpdateHeaderStatusWithRejectType514(String.Empty);

	void TestUpdateHeaderStatusWithRejectType514(string headerStatus)
	{
		entry.CH_EntryStatus = headerStatus;
		mockProvider.Setup(x => x.BusinessRejectionType).Returns(ExportOperationBusinessRejectionTypeList.Codes.InvalidationRequestRejection);
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);
		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("CusEntryHeader Status Should BE RJI(=rejection invalidation request)", StatusCodes.RejectedInvalidation, ((CusEntryHeader)incomingMessage.EM_LinkedObject).CH_EntryStatus);
		});
	}

	#endregion

	#region TestUpdateHeaderStatusWithRejectType515

	public void TestUpdateHeaderStatusWithRejectType_For515ACK() => TestUpdateHeaderStatusWithRejectType515(StatusCodes.ACK);

	public void TestUpdateHeaderStatusWithRejectType_For515PRN() => TestUpdateHeaderStatusWithRejectType515(StatusCodes.Presented);

	public void TestUpdateHeaderStatusWithRejectType_For515INR() => TestUpdateHeaderStatusWithRejectType515(StatusCodes.InvalidationRequest);

	public void TestUpdateHeaderStatusWithRejectType_For515AMR() => TestUpdateHeaderStatusWithRejectType515(StatusCodes.AmendmentRequest);
	public void TestUpdateHeaderStatusWithRejectType_For515Empty() => TestUpdateHeaderStatusWithRejectType515(String.Empty);

	void TestUpdateHeaderStatusWithRejectType515(string headerStatus)
	{
		entry.CH_EntryStatus = headerStatus;
		mockProvider.Setup(x => x.BusinessRejectionType).Returns(ExportOperationBusinessRejectionTypeList.Codes.DeclarationRejection);
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);
		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("CusEntryHeader Status Should BE REJ", StatusCodes.Rejected, ((CusEntryHeader)incomingMessage.EM_LinkedObject).CH_EntryStatus);
		});
	}

	#endregion

	public void TestUpdateHeaderStatusWithRejectType583()
	{
		entry.CH_EntryStatus = StatusCodes.ACK;
		mockProvider.Setup(x => x.BusinessRejectionType).Returns(ExportOperationBusinessRejectionTypeList.Codes.NonExitedExportInformationRejection);
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);
		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("CusEntryHeader Status Should BE RJN(=rejection non-exited export)", StatusCodes.RejectedNonExitExport, ((CusEntryHeader)incomingMessage.EM_LinkedObject).CH_EntryStatus);
		});
	}

	public void TestMessageStatusInvalid()
	{
		entry.CH_EntryStatus = StatusCodes.ACK;
		entry.CH_Status = LogicalStatusList.Codes.Accepted;
		mockProvider.Setup(x => x.BusinessRejectionType).Returns(ExportOperationBusinessRejectionTypeList.Codes.NonExitedExportInformationRejection);
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);
		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("CusEntryHeader Status Should BE RJN(=rejection non-exited export)", StatusCodes.RejectedNonExitExport, ((CusEntryHeader)incomingMessage.EM_LinkedObject).CH_EntryStatus);
			AssertEquals("Message Status Should BE INV", LogicalStatusList.Codes.Invalid, entry.CH_Status);
		});
	}
	public void TestCheckMessageNoteText()
	{
		entry.CH_EntryStatus = StatusCodes.DeclarationCancelled;
		mockProvider.Setup(x => x.BusinessRejectionType).Returns(ExportOperationBusinessRejectionTypeList.Codes.PresentationNotificationRejection);
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);
		AssertEquals("The message with interchange is discarded, because the present is not empty, ACK, PRN, INR or AMR.", incomingMessage.EM_MessageInterpretation);
	}

	protected override string ExpectedMessageFriendlyName => BEIncomingMessageSubTypes.Descriptions.CC556C;

	protected override Type ExpectedMessageInterpreterType => typeof(CC556CMessageInterpreter);

	protected override CC556CMessageProcessor Processor => processor;

	protected override void SetUp()
	{
		base.SetUp();
		mockProvider = new Mock<ICC556CDataProvider>();
		mockProvider.CallBase = true;
		mockProvider.Setup(m => m.LRN).Returns("22045281480600000001");
		mockProvider.Setup(m => m.MRN).Returns("22BEE00000000012J1");
		mockProvider.Setup(m => m.BusinessRejectionType).Returns("22BEE00000000012J1");
		mockProvider.Setup(m => m.RejectionDateAndTime).Returns(new DateTime(2022, 4, 1, 12, 34, 56));
		mockProvider.Setup(m => m.RejectionCode).Returns("4");
		mockProvider.Setup(m => m.RejectionReason).Returns("invalid value transport type");
		var mockFunctionError = new Mock<IFunctionalError>();
		mockFunctionError.CallBase = true;
		mockFunctionError.Setup(m => m.ErrorCode).Returns("12");
		mockFunctionError.Setup(m => m.ErrorReason).Returns("Type of transport does not exist");
		mockFunctionError.Setup(m => m.ErrorPointer).Returns("cc515c.DepartureTransportMeans(2).typeOfIdentification");
		mockFunctionError.Setup(m => m.OriginalAttributeValue).Returns("32");
		mockProvider.Setup(m => m.FunctionalErrorList).Returns(new List<IFunctionalError> { mockFunctionError.Object });
		provider = mockProvider.Object;
		mockProcessor = new Mock<CC556CMessageProcessor>(new BatchProcessor.LoggingInformation());
		mockProcessor.CallBase = true;
		mockProcessor.Setup(m => m.GetMessageDataProvider(It.IsAny<BEMessage>())).Returns(provider);
		processor = mockProcessor.Object;
		incomingMessage = CreateIncomingMessage(Factory);
		entry = Factory.NewWithValidTestData<CusEntryHeader>();
		entry.CH_BGMReference = "22045281480600000001";
		entry.MovementReferenceNumberSetter("22BEE00000000012J1");
		Factory.Save();
	}

	Mock<ICC556CDataProvider> mockProvider;
	Mock<CC556CMessageProcessor> mockProcessor;
	CC556CMessageProcessor processor;
	ICC556CDataProvider provider;
	BEMessage incomingMessage;
	CusEntryHeader entry;
}
