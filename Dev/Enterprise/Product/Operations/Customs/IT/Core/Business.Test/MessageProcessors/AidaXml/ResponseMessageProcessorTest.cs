using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ResponseMessageProcessorTest : XmlIncomingMessageProcessorTest<ResponseMessageProcessor>
{
	public void TestReceivedMessageNumberIsConsistentWithSentMessageNumber()
	{
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_PositiveResponse.xml");
		var (_, entryHeader, sentMessage, receivedMessage) = PrepareTestData(declarationType: "IMP", messageText: positiveResponse, messageType: "RES");
		entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.AwaitingOriginal;

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertEquals("EM_MessageNum in sent and received message is the same", sentMessage.EM_MessageNum, receivedMessage.EM_MessageNum);
	}

	public void TestProcessUnknownContent_AttachTheMessageButDoesNotChangeStatus()
	{
		var unknownResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_InvalidAcknowledgement.xml");
		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "IMP", messageText: unknownResponse, messageType: "RES");
		entryHeader.CH_Status = "XXX";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		AssertNumberOfResponseMessages(entryHeader, 1);
		AssertEquals(nameof(entryHeader.CH_Status), "XXX", entryHeader.CH_Status);
	}

	public void TestProcessImportNegativeResponse()
	{
		var negativeResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_NegativeResponse.xml");
		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "IMP", messageText: negativeResponse, messageType: "RES");
		entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.AcknowledgedOriginal;

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		AssertNumberOfResponseMessages(entryHeader, 1);
		AssertEquals(nameof(entryHeader.CH_Status), "ERO", entryHeader.CH_Status);
	}

	[TestDate(2022, 09, 05)]
	public void TestProcessImportPositiveResponse()
	{
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_PositiveResponse.xml");
		var dateTime = new ZDateTime(2022, 09, 05, System.DateTimeKind.Utc);
		var localDateTime = dateTime.ToLocalBranchTime();
		(var declaration, var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "IMP", messageText: positiveResponse, messageType: "RES");
		declaration.JE_CustomsOffice = "IT279100";
		entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.AwaitingOriginal;

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertNumberOfResponseMessages(entryHeader, 1);
		AssertEquals(nameof(entryHeader.CH_Status), "ACO", entryHeader.CH_Status);
		AssertEquals(nameof(entryHeader.CH_EntryStatus), "REG", entryHeader.CH_EntryStatus);

		var entryNumber = entryHeader.CusEntryNumber;
		AssertNotNull("EntryHeader -> CusEntryNumber", entryNumber);
		AssertEntryNumber(entryNumber, "MRN", "22ITQXT04CE98155R2", "CUS", "279100", localDateTime);

		var registrationInfoEntryNumber = entryHeader.EntryNumbersProvider.RegistrationInfo;
		AssertNotNull("EntryHeader -> RegistrationInfo", registrationInfoEntryNumber);
		AssertEntryNumber(registrationInfoEntryNumber, "REG", "4 -CE98155", "CUS", "279100", localDateTime);
	}

	public void TestProcessImportPositiveWholeClearanceResponse()
	{
		var wholeClearanceResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_PositiveResponseWithClearance.xml");
		(var declaration, var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "IMP", messageText: wholeClearanceResponse, messageType: "RES");
		declaration.JE_CustomsOffice = "IT279100";
		entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.AcknowledgedOriginal;

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertNumberOfResponseMessages(entryHeader, 1);
		CombineAssertions(() =>
		{
			AssertEquals(nameof(entryHeader.CH_Status), "CLO", entryHeader.CH_Status);
			AssertEquals(nameof(entryHeader.CH_EntryStatus), "ICC", entryHeader.CH_EntryStatus);
		});

		var parsedMrnDate = new ZDateTime(2022, 06, 21, 15, 47, 48, 373);
		var entryNumber = entryHeader.CusEntryNumber;
		AssertNotNull("EntryHeader -> CusEntryNumber", entryNumber);
		AssertEntryNumber(entryNumber, "MRN", "22ITQ0B04FB58459R4", "CUS", "279100", parsedMrnDate);

		var parsedRegistrationDate = new ZDateTime(2022, 06, 21, 15, 47, 48, 373);
		var registrationInfoEntryNumber = entryHeader.EntryNumbersProvider.RegistrationInfo;
		AssertNotNull("EntryHeader -> RegistrationInfo", registrationInfoEntryNumber);
		AssertEntryNumber(registrationInfoEntryNumber, "REG", "4 -FB58459", "CUS", "279100", parsedRegistrationDate);

		var clearanceEntryNumber = entryHeader.EntryNumbersProvider.ReleaseInfo;
		AssertNotNull("Clearance Entry Number", clearanceEntryNumber);
		AssertEntryNumber(clearanceEntryNumber, "CLR", "7JTCQR", "CUS", string.Empty, new ZDateTime(2022, 06, 21));
	}

	public void TestProcessImportPositivePartialClearanceResponse()
	{
		Assert("'Clearance by Entry Line' feature has been disabled due to 'Clearance Codes' release issue.", true);
	}

	public void TestProcessImportPositiveUnderControlDeclarationResponse()
	{
		var underControlDeclarationResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_UnderControlDeclarationResponse.xml");
		(var declaration, var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "IMP", messageText: underControlDeclarationResponse, messageType: "RES");
		declaration.JE_CustomsOffice = "IT279100";
		entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.AcknowledgedOriginal;

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertNumberOfResponseMessages(entryHeader, 1);
		CombineAssertions(() =>
		{
			AssertEquals(nameof(entryHeader.CH_Status), "ACO", entryHeader.CH_Status);
			AssertEquals(nameof(entryHeader.CH_EntryStatus), "UCL", entryHeader.CH_EntryStatus);
		});

		var parsedMrnDate = new ZDateTime(2022, 11, 04, 09, 42, 01, 073);
		var entryNumber = entryHeader.CusEntryNumber;
		AssertNotNull("EntryHeader -> CusEntryNumber", entryNumber);
		AssertEntryNumber(entryNumber, "MRN", "22ITQ0B04AA07327R8", "CUS", "279100", parsedMrnDate);

		var parsedRegistrationDate = new ZDateTime(2022, 11, 04, 09, 42, 01, 073);
		var registrationInfoEntryNumber = entryHeader.EntryNumbersProvider.RegistrationInfo;
		AssertNotNull("EntryHeader -> RegistrationInfo", registrationInfoEntryNumber);
		AssertEntryNumber(registrationInfoEntryNumber, "REG", "4 -AA07327", "CUS", "279100", parsedRegistrationDate);

		AssertNull("Clearance Entry Number", entryHeader.EntryNumbersProvider.ReleaseInfo);
	}

	public void TestProcessImportPositivePartialClearanceWithNoReleaseNumberResponse()
	{
		Assert("'Clearance by Entry Line' feature has been disabled due to 'Clearance Codes' release issue.", true);
	}

	public void TestProcessImportCancellationResponse_Error()
	{
		var cancellationErrorResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_CancellationErrorResponse.xml");
		var (entryHeader, receivedMessage) = PrepareDataForMessageResponse(cancellationErrorResponse, MessageProcessorConstants.InterchangeTypes.Ucc6CancellationType, declarationType: "IMP");
		var messageProcessor = GetMessageProcessor(logger);
		messageProcessor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertEquals(nameof(entryHeader.CH_Status), ITMessageStatusList.Codes.ErrorOriginal, entryHeader.CH_Status);
		AssertEquals(nameof(entryHeader.CH_EntryStatus), ZString.Empty, entryHeader.CH_EntryStatus);
	}

	public void TestProcessImportCancellationResponse_PositiveButNotConfirmed()
	{
		var cancellationPositiveButNotConfirmedResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_CancellationPositiveButNotConfirmedResponse.xml");
		var (entryHeader, receivedMessage) = PrepareDataForMessageResponse(cancellationPositiveButNotConfirmedResponse, MessageProcessorConstants.InterchangeTypes.Ucc6CancellationType, declarationType: "IMP");
		entryHeader.CH_Status = "AWO";
		var messageProcessor = GetMessageProcessor(logger);
		messageProcessor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertEquals(nameof(entryHeader.CH_Status), "ACS", entryHeader.CH_Status);
		AssertEquals(nameof(entryHeader.CH_EntryStatus), ZString.Empty, entryHeader.CH_EntryStatus);
	}

	public void TestProcessImportCancellationResponse_PositiveAndConfirmed()
	{
		var cancellationPositiveAndConfirmedResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_CancellationPositiveAndConfirmedResponse.xml");
		var (entryHeader, receivedMessage) = PrepareDataForMessageResponse(cancellationPositiveAndConfirmedResponse, MessageProcessorConstants.InterchangeTypes.Ucc6CancellationType, declarationType: "IMP");
		var payInfoOne = entryHeader.EntryPayInfos.AddNew();
		payInfoOne.C9_IncomingPayResponseNo = "123";
		payInfoOne.C9_PaymentAmount = 1234.22m;
		payInfoOne.C9_PaymentDate = new ZDateTime(2022, 09, 01);
		payInfoOne.C9_PaymentParty = "E";
		payInfoOne.C9_PaymentStatus = "PEN";

		var payInfoTwo = entryHeader.EntryPayInfos.AddNew();
		payInfoTwo.C9_IncomingPayResponseNo = "XYZ";
		payInfoTwo.C9_PaymentAmount = 789.99m;
		payInfoTwo.C9_PaymentDate = new ZDateTime(2022, 09, 04);
		payInfoTwo.C9_PaymentParty = "G";
		payInfoTwo.C9_PaymentStatus = "PEN";

		var messageProcessor = GetMessageProcessor(logger);
		messageProcessor.ProcessMessage(receivedMessage);

		Factory.Save();
		entryHeader.EntryPayInfos.Load();
		AssertEquals(nameof(entryHeader.CH_Status), ITMessageStatusList.Codes.AcceptedBySystem, entryHeader.CH_Status);
		AssertEquals(nameof(entryHeader.CH_EntryStatus), ITEntryStatusList.Codes.Canceled, entryHeader.CH_EntryStatus);
		AssertEquals("EntryPayInfo Count", 4, entryHeader.EntryPayInfos.Count);

		var invalidPayInfoOne = entryHeader.EntryPayInfos.Cast<CusEntryPayInfo>()
			.SingleOrDefault(e => e.C9_IncomingPayResponseNo == "123" && e.C9_PaymentAmount < 0);

		var invalidPayInfoTwo = entryHeader.EntryPayInfos.Cast<CusEntryPayInfo>()
			.SingleOrDefault(e => e.C9_IncomingPayResponseNo == "XYZ" && e.C9_PaymentAmount < 0);

		AssertEntryPayInfoWithNegativeAmount(invalidPayInfoOne, payInfoOne);
		AssertEntryPayInfoWithNegativeAmount(invalidPayInfoTwo, payInfoTwo);
	}

	public void TestProcessImportAmendmentResponse_Negative()
	{
		var amendmentNegativeResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_AmendmentNegativeResponse.xml");
		var (entryHeader, receivedMessage) = PrepareDataForMessageResponse(amendmentNegativeResponse, MessageProcessorConstants.InterchangeTypes.Ucc6AmendmentType, declarationType: "IMP");
		var messageProcessor = GetMessageProcessor(logger);
		messageProcessor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertEquals(nameof(entryHeader.CH_Status), ITMessageStatusList.Codes.ErrorOriginal, entryHeader.CH_Status);
		AssertEquals(nameof(entryHeader.CH_EntryStatus), ZString.Empty, entryHeader.CH_EntryStatus);
	}

	public void TestProcessImportAmendmentResponse_PositiveButNotConfirmed()
	{
		var amendmentNotConfirmedResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_AmendmentNotConfirmedResponse.xml");
		var (entryHeader, receivedMessage) = PrepareDataForMessageResponse(amendmentNotConfirmedResponse, MessageProcessorConstants.InterchangeTypes.Ucc6AmendmentType, declarationType: "IMP");
		var entryPayInfo = entryHeader.EntryPayInfos.AddNew();
		entryPayInfo.C9_IncomingPayResponseNo = "123";
		entryPayInfo.C9_PaymentAmount = 1234.22m;
		entryPayInfo.C9_PaymentDate = new ZDateTime(2022, 09, 01);
		entryPayInfo.C9_PaymentParty = "E";
		entryPayInfo.C9_PaymentStatus = "PEN";

		var messageProcessor = GetMessageProcessor(logger);
		messageProcessor.ProcessMessage(receivedMessage);
		Factory.Save();

		entryHeader.EntryPayInfos.Load();

		CombineAssertions(() =>
		{
			AssertEquals(nameof(entryHeader.CH_Status), ITMessageStatusList.Codes.AcceptedBySystem, entryHeader.CH_Status);
			AssertEquals(nameof(entryHeader.CH_EntryStatus), "ICC", entryHeader.CH_EntryStatus);

			AssertEquals("CusEntryInfo Record Count", 1, entryHeader.EntryPayInfos.Count);
			var entryNumber = CusEntryNumber.Load(entryHeader, CusEntryNumberConstants.EntryTypes.ClereanceCode, "IT");
			AssertNotNull("CLS Entry Number", entryNumber);
			AssertEquals("CLS->CE_EntryNum", "7JTCQR", entryNumber.CE_EntryNum);
		});
	}

	public void TestProcessImportAmendmentResponse_HasNoReleaseItems()
	{
		var amendmentPositiveResponseWithNoReleaseItemsAndNotConfirmed = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_AmendmentPositiveResponseWithNoReleaseItemsAndNotConfirmed.xml");
		var (entryHeader, receivedMessage) = PrepareDataForMessageResponse(amendmentPositiveResponseWithNoReleaseItemsAndNotConfirmed, MessageProcessorConstants.InterchangeTypes.Ucc6AmendmentType, declarationType: "IMP");
		var entryPayInfo = entryHeader.EntryPayInfos.AddNew();
		entryPayInfo.C9_IncomingPayResponseNo = "123";
		entryPayInfo.C9_PaymentAmount = 1234.22m;
		entryPayInfo.C9_PaymentDate = new ZDateTime(2022, 09, 01);
		entryPayInfo.C9_PaymentParty = "E";
		entryPayInfo.C9_PaymentStatus = "PEN";

		var messageProcessor = GetMessageProcessor(logger);
		messageProcessor.ProcessMessage(receivedMessage);
		Factory.Save();

		entryHeader.EntryPayInfos.Load();
		var entryPayInfoCollection = entryHeader.EntryPayInfos.Cast<CusEntryPayInfo>().ToArray();

		CombineAssertions(() =>
		{
			AssertEquals(nameof(entryHeader.CH_Status), ITMessageStatusList.Codes.AcceptedBySystem, entryHeader.CH_Status);
			AssertEquals(nameof(entryHeader.CH_EntryStatus), "", entryHeader.CH_EntryStatus);

			AssertEquals("CusEntryInfo Record Count", 1, entryPayInfoCollection.Length);
			var entryNumber = CusEntryNumber.Load(entryHeader, CusEntryNumberConstants.EntryTypes.ClereanceCode, "IT");
			AssertNull("No CLS Entry Number is expected", entryNumber);
		});
	}

	public void TestProcessImportAmendmentResponse_PositiveAndConfirmed()
	{
		var amendmentConfirmedResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_AmendmentConfirmedResponse.xml");
		var (entryHeader, receivedMessage) = PrepareDataForMessageResponse(amendmentConfirmedResponse, MessageProcessorConstants.InterchangeTypes.Ucc6AmendmentType, declarationType: "IMP");
		var entryPayInfo = entryHeader.EntryPayInfos.AddNew();
		entryPayInfo.C9_IncomingPayResponseNo = "123";
		entryPayInfo.C9_PaymentAmount = 1234.22m;
		entryPayInfo.C9_PaymentDate = new ZDateTime(2022, 09, 01);
		entryPayInfo.C9_PaymentParty = "E";
		entryPayInfo.C9_PaymentStatus = "PEN";

		var messageProcessor = GetMessageProcessor(logger);
		messageProcessor.ProcessMessage(receivedMessage);
		Factory.Save();

		entryHeader.EntryPayInfos.Load();
		var entryPayInfoCollection = entryHeader.EntryPayInfos.Cast<CusEntryPayInfo>().ToArray();

		CombineAssertions(() =>
		{
			AssertEquals(nameof(entryHeader.CH_Status), ITMessageStatusList.Codes.AcceptedBySystem, entryHeader.CH_Status);
			AssertEquals(nameof(entryHeader.CH_EntryStatus), ITEntryStatusList.Codes.Amended, entryHeader.CH_EntryStatus);

			AssertEquals("CusEntryInfo Record Count", 3, entryPayInfoCollection.Length);
			AssertCollectionContains("Invalidated CusEntryPayInfo",
				entryPayInfoCollection,
				e => e.A93Number == "123" && e.C9_PaymentAmount == (entryPayInfo.C9_PaymentAmount * -1));
			AssertEquals("Having the EntryHeader no fees, Sum of all A93 amounts must be zero", 0.00m, entryPayInfoCollection.Where(x => x.A93Number == "123").Sum(x => x.C9_PaymentAmount));

			var entryNumber = CusEntryNumber.Load(entryHeader, CusEntryNumberConstants.EntryTypes.ClereanceCode, "IT");
			AssertNotNull("CLS Entry Number", entryNumber);
			AssertEquals("CLS->CE_EntryNum", "7JTCQR", entryNumber.CE_EntryNum);
		});
	}

	public void TestProcessImportDeclarationResponse_A93NumberEntryLevel()
	{
		var declarationPositiveResponseWithA93NumberAtHeaderLevel = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_DeclarationPositiveResponseWithA93NumberAtHeaderLevel.xml");
		var (entryHeader, receivedMessage) = PrepareDataForMessageResponse(declarationPositiveResponseWithA93NumberAtHeaderLevel, "NEW", declarationType: "IMP");
		AddEntryLinesAndFees(entryHeader);

		var messageProcessor = GetMessageProcessor(logger);
		messageProcessor.ProcessMessage(receivedMessage);

		AssertA93Numbers(entryHeader, expectedA93Number: "184", expectedTransactionType: "4", expectedAmountForPaymentTypeE: 42.21m, expectedAmountForPaymentTypeG: 119.97m);
	}

	public void TestProcessImportDeclarationResponse_A93NumberEntryLineLevel()
	{
		var declarationPositiveResponseWithA93NumberAtLineLevel = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_DeclarationPositiveResponseWithA93NumberAtLineLevel.xml");
		var (entryHeader, receivedMessage) = PrepareDataForMessageResponse(declarationPositiveResponseWithA93NumberAtLineLevel, "NEW", declarationType: "IMP");
		AddEntryLinesAndFees(entryHeader);

		var messageProcessor = GetMessageProcessor(logger);
		messageProcessor.ProcessMessage(receivedMessage);

		AssertA93Numbers(entryHeader, expectedA93Number: "47-2", expectedTransactionType: "4 T", expectedAmountForPaymentTypeE: 9.99m, expectedAmountForPaymentTypeG: 119.97m);
	}

	public void TestProcessImportDepositedDeclarationPositiveResponse()
	{
		var depositedDeclarationPositiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_DepositedDeclarationPositiveResponse.xml");
		var (entryHeader, receivedMessage) = PrepareDataForMessageResponse(depositedDeclarationPositiveResponse, "NEW", declarationType: "IMP");
		var entryInstruction = entryHeader.Declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		var messageProcessor = GetMessageProcessor(logger);
		messageProcessor.ProcessMessage(receivedMessage);

		AssertEquals(nameof(entryHeader.CH_EntryStatus), "DEP", entryHeader.CH_EntryStatus);
		AssertEquals(nameof(entryHeader.EntryInstruction.PreviousDocuments), 1, entryHeader.EntryInstruction.PreviousDocuments.Count);

		var previousDocument = entryHeader.EntryInstruction.PreviousDocuments[0];
		AssertEquals(nameof(previousDocument.CSI_Procedure), "NUM", previousDocument.CSI_Procedure);
		AssertEquals(nameof(previousDocument.CSI_Code), "ZZZ", previousDocument.CSI_Code);
		AssertEquals(nameof(previousDocument.CSI_ReferenceNumber), "2022FGACOH10010000167", previousDocument.CSI_ReferenceNumber);
	}

	public void TestProcessExportPositiveResponse()
	{
		var exportPositiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_PositiveResponse.xml");
		(var declaration, var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "EXP", messageText: exportPositiveResponse, messageType: "RES");
		declaration.JE_CustomsOffice = "IT279100";
		entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.AwaitingOriginal;

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(entryHeader.Declaration, true))
		{
			var processor = GetMessageProcessor(logger);
			processor.ProcessMessage(receivedMessage);
			Factory.Save();

			entryHeader.Messages.Reload(reLoadExistingRows: false);
			var ivistoMessage = entryHeader.Messages.GetLastMessageByType("IVI");
			AssertNull("Ivisto Message", ivistoMessage);
		}

		AssertNumberOfResponseMessages(entryHeader, 1);
		AssertEquals(nameof(entryHeader.CH_Status), "ACO", entryHeader.CH_Status);
		AssertEquals(nameof(entryHeader.CH_EntryStatus), "REG", entryHeader.CH_EntryStatus);

		AssertEquals("RegistrationNumber", "1 -AA01827", entryHeader.RegistrationNumber);
		AssertEquals("MovementReferenceNumber", "23ITQ0B01AA01827A0", entryHeader.MovementReferenceNumber);

		var registrationInfo = entryHeader.EntryNumbersProvider.RegistrationInfo;
		AssertNotNull("EntryHeader -> RegistrationInfo", registrationInfo);
		AssertEntryNumber(registrationInfo, "REG", "1 -AA01827", "CUS", "279100", new ZDateTime(2023, 04, 26, 15, 14, 39));
	}

	public void TestProcessExportPositiveWholeClearanceResponse()
	{
		var exportPositiveResponseWithClearance = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_PositiveResponseWithClearance.xml");
		(var declaration, var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "EXP", messageText: exportPositiveResponseWithClearance, messageType: "RES");
		declaration.JE_CustomsOffice = "IT279100";
		entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.AcknowledgedOriginal;

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(entryHeader.Declaration, true))
		{
			var processor = GetMessageProcessor(logger);
			processor.ProcessMessage(receivedMessage);
			Factory.Save();

			entryHeader.Messages.Reload(reLoadExistingRows: false);
			var ivistoMessage = entryHeader.Messages.GetLastMessageByType("IVI");
			AssertNotNull("Ivisto Message", ivistoMessage);
		}

		AssertNumberOfResponseMessages(entryHeader, 1);
		CombineAssertions(() =>
		{
			AssertEquals(nameof(entryHeader.CH_Status), "CLO", entryHeader.CH_Status);
			AssertEquals(nameof(entryHeader.CH_EntryStatus), "ECC", entryHeader.CH_EntryStatus);
		});

		AssertEquals("RegistrationNumber", "1 -AA01827", entryHeader.RegistrationNumber);
		AssertEquals("MovementReferenceNumber", "23ITQ0B01AA01827A0", entryHeader.MovementReferenceNumber);

		var registrationInfo = entryHeader.EntryNumbersProvider.RegistrationInfo;
		AssertNotNull("EntryHeader -> RegistrationInfo", registrationInfo);
		AssertEntryNumber(registrationInfo, "REG", "1 -AA01827", "CUS", "279100", new ZDateTime(2023, 04, 26, 15, 14, 39));

		var clearanceEntryNumber = CusEntryNumber.Load(entryHeader, "CLR", "IT");
		AssertNotNull("Clearance Entry Number", clearanceEntryNumber);
		AssertEntryNumber(clearanceEntryNumber, "CLR", "Z7SFLY", "CUS", string.Empty, new ZDateTime(2022, 02, 18));
	}

	public void TestProcessImportResponseFailsIfUnexpectedApplicationReference()
	{
		var depositedDeclarationPositiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_DepositedDeclarationPositiveResponse.xml");
		(_, var entryHeader, var sentMessage, var receivedMessage) = PrepareTestData(declarationType: "IMP", messageText: depositedDeclarationPositiveResponse, messageType: "RES");
		entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.AcknowledgedOriginal;
		sentMessage.EM_ApplicationReference = "ABC";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		AssertFailMessage(receivedMessage, receivedMessage.Interchange, "Unexpected application reference found");
	}

	public void TestProcessExportResponseMessageOriginatedFromIutRequest()
	{
		var exportPositiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_PositiveResponse.xml");
		var (declaration, entryHeader, sentMessage, receivedMessage) = PrepareTestData(declarationType: "EXP", messageText: exportPositiveResponse, messageType: "RES");
		declaration.JE_CustomsOffice = "IT279100";
		sentMessage.EM_MessageText = "<IUT>20230426D16001305248</IUT>";
		sentMessage.EM_MessageType = "IUT";
		sentMessage.EM_ApplicationReference = "IUT";

		var newDeclarationSentInterchange = Factory.New<EDIInterchange>();
		newDeclarationSentInterchange.EI_SessionGUID = new ZGuid("EADC205E-BFD7-44AD-B547-14C4B3C02177");
		var newDeclarationSentMessage = AddNewMessage(messageText: "", "NEW", newDeclarationSentInterchange);
		newDeclarationSentMessage.IsTransmitMessage = true;
		newDeclarationSentMessage.EM_Status = "SNT";
		newDeclarationSentMessage.EM_ApplicationReference = "EXP";
		entryHeader.Messages.Add(newDeclarationSentMessage);

		var ackReceivedInterchange = Factory.New<EDIInterchange>();
		ackReceivedInterchange.EI_SessionGUID = new ZGuid("EADC205E-BFD7-44AD-B547-14C4B3C02177");
		var ackPositiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_PositiveAcknowledgment.xml");
		var ackReceivedMessage = AddNewMessage(messageText: ackPositiveResponse, "ACK", ackReceivedInterchange);
		entryHeader.Messages.Add(ackReceivedMessage);

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		AssertNumberOfResponseMessages(entryHeader, 1);
		AssertEquals(nameof(entryHeader.CH_Status), "ACO", entryHeader.CH_Status);
		AssertEquals(nameof(entryHeader.CH_EntryStatus), "REG", entryHeader.CH_EntryStatus);

		AssertEquals("RegistrationNumber", "1 -AA01827", entryHeader.RegistrationNumber);
		AssertEquals("MovementReferenceNumber", "23ITQ0B01AA01827A0", entryHeader.MovementReferenceNumber);

		var registrationInfo = entryHeader.EntryNumbersProvider.RegistrationInfo;
		AssertNotNull("EntryHeader -> RegistrationInfo", registrationInfo);
		AssertEntryNumber(registrationInfo, "REG", "1 -AA01827", "CUS", "279100", new ZDateTime(2023, 04, 26, 15, 14, 39));
	}

	public void TestProcessExportResponseMessageOriginatedFromIutRequestFailsIfNotAbleToFindTheOriginalMessage()
	{
		var exportPositiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_PositiveResponse.xml");
		var (_, _, sentMessage, receivedMessage) = PrepareTestData(declarationType: "EXP", messageText: exportPositiveResponse, messageType: "RES");
		sentMessage.EM_MessageText = "<IUT>1234567890</IUT>";
		sentMessage.EM_MessageType = "IUT";
		sentMessage.EM_ApplicationReference = "IUT";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		AssertFailMessage(receivedMessage, receivedMessage.Interchange, "Unable to retrieve the original sent message for the IUT request '1234567890'");
	}

	public void TestProcessExportAmendmentPositiveResponseAsRejection()
	{
		var negativeResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_AmendmentPositiveResponseAsRejection.xml");
		(var declaration, var entryHeader, var sentMessage, var receivedMessage) = PrepareTestData(declarationType: "EXP", messageText: negativeResponse, messageType: MessageProcessorConstants.InterchangeTypes.Ucc6AmendmentType);
		declaration.JE_CustomsOffice = "IT279100";
		entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.AwaitingOriginal;
		sentMessage.EM_MessageType = MessageProcessorConstants.InterchangeTypes.Ucc6AmendmentType;

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertEquals(nameof(entryHeader.CH_Status), "ERO", entryHeader.CH_Status);
	}

	public void TestProcessImportAmendmentPositiveResponseAsRejection()
	{
		var negativeResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_AmendmentPositiveResponseAsRejection.xml");
		(var declaration, var entryHeader, var sentMessage, var receivedMessage) = PrepareTestData(declarationType: "IMP", messageText: negativeResponse, messageType: MessageProcessorConstants.InterchangeTypes.Ucc6ResponseMessageType);
		declaration.JE_CustomsOffice = "IT279100";
		entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.AwaitingOriginal;
		sentMessage.EM_MessageType = MessageProcessorConstants.InterchangeTypes.Ucc6AmendmentType;

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertEquals(nameof(entryHeader.CH_Status), "ERO", entryHeader.CH_Status);
	}

	public void TestProcessExportCancellationResponse_PositiveAndConfirmed()
	{
		var cancellationPositiveAndConfirmedResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_CancellationPositiveAndConfirmedResponse.xml");
		var (entryHeader, receivedMessage) = PrepareDataForMessageResponse(cancellationPositiveAndConfirmedResponse, MessageProcessorConstants.InterchangeTypes.Ucc6CancellationType, declarationType: "EXP");
		var payInfoOne = entryHeader.EntryPayInfos.AddNew();
		payInfoOne.C9_IncomingPayResponseNo = "123";
		payInfoOne.C9_PaymentAmount = 1234.22m;
		payInfoOne.C9_PaymentDate = new ZDateTime(2022, 09, 01);
		payInfoOne.C9_PaymentParty = "E";
		payInfoOne.C9_PaymentStatus = "PEN";

		var payInfoTwo = entryHeader.EntryPayInfos.AddNew();
		payInfoTwo.C9_IncomingPayResponseNo = "XYZ";
		payInfoTwo.C9_PaymentAmount = 789.99m;
		payInfoTwo.C9_PaymentDate = new ZDateTime(2022, 09, 04);
		payInfoTwo.C9_PaymentParty = "G";
		payInfoTwo.C9_PaymentStatus = "PEN";

		var messageProcessor = GetMessageProcessor(logger);
		messageProcessor.ProcessMessage(receivedMessage);

		Factory.Save();
		entryHeader.EntryPayInfos.Load();
		AssertEquals(nameof(entryHeader.CH_Status), "ACS", entryHeader.CH_Status);
		AssertEquals(nameof(entryHeader.CH_EntryStatus), "CNC", entryHeader.CH_EntryStatus);
		AssertEquals("EntryPayInfo Count", 4, entryHeader.EntryPayInfos.Count);

		var invalidPayInfoOne = entryHeader.EntryPayInfos.Cast<CusEntryPayInfo>()
			.SingleOrDefault(e => e.C9_IncomingPayResponseNo == "123" && e.C9_PaymentAmount < 0);

		var invalidPayInfoTwo = entryHeader.EntryPayInfos.Cast<CusEntryPayInfo>()
			.SingleOrDefault(e => e.C9_IncomingPayResponseNo == "XYZ" && e.C9_PaymentAmount < 0);

		AssertEntryPayInfoWithNegativeAmount(invalidPayInfoOne, payInfoOne);
		AssertEntryPayInfoWithNegativeAmount(invalidPayInfoTwo, payInfoTwo);
	}

	public void TestProcessExportCancellationResponse_Rejected()
	{
		var rejectedCancellationResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_CancellationRejectedResponse.xml");
		var (_, entryHeader, sentMessage, receivedMessage) = PrepareTestData(declarationType: "EXP", messageText: rejectedCancellationResponse, messageType: "RES");
		sentMessage.EM_MessageType = "CAN";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertEquals(nameof(entryHeader.CH_Status), "ERO", entryHeader.CH_Status);
	}

	public void TestDuplicateInterchangeForEntry()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_DeclarantType = "IMP";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var positiveBodyText = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_PositiveResponse.xml");
		var positiveClearanceBodyText = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_PositiveResponseWithClearance.xml");
		var messageNumber = 1;

		var (receivedInterchange, receivedMessage) = AddReceivedMessage(entryHeader, positiveBodyText, ref messageNumber);
		processor.ProcessMessage(receivedMessage);

		CombineAssertions("Interchange message processed", () =>
		{
			AssertEquals("Message is linked to Entry", entryHeader.PK, receivedMessage.EM_LinkUniqueID);
			AssertEquals("Message status", "RCV", receivedMessage.EM_Status);
			AssertEquals("Interchange status", "RCV", receivedInterchange.EI_Status);
		});

		var (receivedDupInterchange, receivedDupMessage) = AddReceivedMessage(entryHeader, positiveBodyText, ref messageNumber);
		processor.ProcessMessage(receivedDupMessage);

		CombineAssertions("Duplicate interchange message discarded", () =>
		{
			AssertEquals("Message isn't linked to Entry", ZGuid.Empty, receivedDupMessage.EM_LinkUniqueID);
			AssertEquals("Message status", "FAL", receivedDupMessage.EM_Status);
			AssertEquals("Interchange status", "FAL", receivedDupInterchange.EI_Status);
			AssertLoggerContainsLogText("Message discarded because duplicated in the declaration");
		});

		var (receivedDiffInterchange, receivedDiffMessage) = AddReceivedMessage(entryHeader, positiveClearanceBodyText, ref messageNumber);
		processor.ProcessMessage(receivedDiffMessage);

		CombineAssertions("Different interchange message processed", () =>
		{
			AssertEquals("Message is linked to Entry", entryHeader.PK, receivedDiffMessage.EM_LinkUniqueID);
			AssertEquals("Message status", "RCV", receivedDiffMessage.EM_Status);
			AssertEquals("Interchange status", "RCV", receivedDiffInterchange.EI_Status);
		});
	}

	protected override IReadOnlyList<ZString> ExpectedMessageTypesToInclude => new ZString[] { "RES" };

	protected override ResponseMessageProcessor GetMessageProcessor(LoggingInformation logger)
	{
		return new ResponseMessageProcessor(logger);
	}

	void AssertNumberOfResponseMessages(CusEntryHeader entryHeader, int expectedAcknowledgement)
	{
		AssertEquals("Number of Response Messages", expectedAcknowledgement, entryHeader.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == "RES"));
	}

	void AssertEntryPayInfoWithNegativeAmount(CusEntryPayInfo invalidPayInfoOne, CusEntryPayInfo originalPayInfo)
	{
		AssertNotNull("Invalidated CusEntryPayInfo", invalidPayInfoOne);

		CombineAssertions(() =>
		{
			AssertEquals(nameof(CusEntryPayInfo.C9_IncomingPayResponseNo), invalidPayInfoOne.C9_IncomingPayResponseNo, originalPayInfo.C9_IncomingPayResponseNo);
			AssertEquals(nameof(CusEntryPayInfo.C9_PaymentDate), invalidPayInfoOne.C9_PaymentDate, originalPayInfo.C9_PaymentDate);
			AssertEquals(nameof(CusEntryPayInfo.C9_PaymentAmount), invalidPayInfoOne.C9_PaymentAmount, (originalPayInfo.C9_PaymentAmount * -1));
			AssertEquals(nameof(CusEntryPayInfo.C9_PaymentParty), invalidPayInfoOne.C9_PaymentParty, originalPayInfo.C9_PaymentParty);
			AssertEquals(nameof(CusEntryPayInfo.C9_PaymentStatus), invalidPayInfoOne.C9_PaymentStatus, originalPayInfo.C9_PaymentStatus);
		});
	}

	static void AssertA93Numbers(
		CusEntryHeader entryHeader,
		string expectedA93Number,
		string expectedTransactionType,
		decimal expectedAmountForPaymentTypeE,
		decimal expectedAmountForPaymentTypeG)
	{
		entryHeader.EntryPayInfos.Load();
		var entryPayInfoCollection = entryHeader.EntryPayInfos.Cast<CusEntryPayInfo>().ToArray();
		var payInfoHeaderWithTypeE = entryPayInfoCollection.SingleOrDefault(e => e.A93Number == expectedA93Number && e.MethodOfPayment == "E");
		var payInfoHeaderWithTypeG = entryPayInfoCollection.SingleOrDefault(e => e.A93Number == expectedA93Number && e.MethodOfPayment == "G");

		AssertNotNull("A93Number Payment Type E", payInfoHeaderWithTypeE);
		AssertNotNull("A93Number Payment Type G", payInfoHeaderWithTypeG);

		CombineAssertions(() =>
		{
			AssertEntryPayInfo(
				payInfoHeaderWithTypeE,
				"A93Number Payment Type E",
				expectedAmountForPaymentTypeE,
				new ZDateTime(2022, 03, 23),
				expectedTransactionType);

			AssertEntryPayInfo(
				payInfoHeaderWithTypeG,
				"A93Number Payment Type G",
				expectedAmountForPaymentTypeG,
				new ZDateTime(2022, 03, 23),
				expectedTransactionType);
		});
	}

	static void AssertEntryPayInfo(
		CusEntryPayInfo payInfo,
		ZString assertionMessagePrefix,
		ZDecimal expectedPaymentAmount,
		ZDateTime expectedPaymentDate,
		ZString expectedTransactionType)
	{
		AssertEquals(assertionMessagePrefix + " " + nameof(CusEntryPayInfo.C9_PaymentAmount), expectedPaymentAmount, payInfo.C9_PaymentAmount);
		AssertEquals(assertionMessagePrefix + " " + nameof(CusEntryPayInfo.C9_PaymentStatus), "PEN", payInfo.C9_PaymentStatus);
		AssertEquals(assertionMessagePrefix + " " + nameof(CusEntryPayInfo.C9_PaymentDate), expectedPaymentDate, payInfo.C9_PaymentDate);
		AssertEquals(assertionMessagePrefix + " " + nameof(CusEntryPayInfo.C9_TransactionType), expectedTransactionType, payInfo.C9_TransactionType);
	}

	(CusEntryHeader, EDIMessage) PrepareDataForMessageResponse(string messageText, string messageType, string declarationType)
	{
		var (_, entryHeader, sentMessage, receivedMessage) = PrepareTestData(declarationType, messageText: messageText,
			messageType: MessageProcessorConstants.InterchangeTypes.Ucc6ResponseMessageType);
		entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.AcknowledgedOriginal;
		sentMessage.EM_MessageType = messageType;
		return (entryHeader, receivedMessage);
	}

	void AddEntryLinesAndFees(CusEntryHeader entryHeader)
	{
		var lineOne = entryHeader.MergedLines.AddNew();
		lineOne.CL_LineNumber = 1;
		var lineTwo = entryHeader.MergedLines.AddNew();
		lineTwo.CL_LineNumber = 2;

		var feeOne = lineOne.Fees.AddNew();
		feeOne.CF_MethodOfPayment = "E";
		feeOne.CF_ChargeType = "DTY";
		feeOne.CF_ChargeAmount = 32.22m;

		var feeTwo = lineTwo.Fees.AddNew();
		feeTwo.CF_MethodOfPayment = "G";
		feeTwo.CF_ChargeType = "DTY";
		feeTwo.CF_ChargeAmount = 143.99m;

		var feeThree = lineTwo.Fees.AddNew();
		feeThree.CF_MethodOfPayment = "G";
		feeThree.CF_ChargeType = "407";
		feeThree.CF_ChargeAmount = 24.02m;

		var feeFour = lineTwo.Fees.AddNew();
		feeFour.CF_MethodOfPayment = "E";
		feeFour.CF_ChargeType = "VAT";
		feeFour.CF_ChargeAmount = 9.99m;
	}

	(EDIInterchange, EDIMessage) AddReceivedMessage(CusEntryHeader entryHeader, string bodyText, ref int messageNumber)
	{
		var sentInterchange = Factory.New<EDIInterchange>();
		sentInterchange.EI_SessionGUID = ZGuid.NewZGuid();
		sentInterchange.IsTransmitInterchange = true;
		sentInterchange.NumberStrategy = new FixedMessageNumberStrategy(messageNumber++);

		var sentMessage = sentInterchange.ContainedMessages.AddNew();
		sentMessage.EM_ApplicationCode = "ITH";
		sentMessage.EM_ApplicationReference = "IMP";
		sentMessage.MessageNumberStrategy = new FixedMessageNumberStrategy(messageNumber++);
		sentMessage.EM_MessageType = "NEW";

		entryHeader.Messages.Add(sentMessage);

		var receivedInterchange = Factory.New<EDIInterchange>();
		receivedInterchange.NumberStrategy = new FixedMessageNumberStrategy(messageNumber++);
		receivedInterchange.EI_SessionGUID = sentInterchange.EI_SessionGUID;
		receivedInterchange.EI_ApplicationCode = "ITH";
		receivedInterchange.EI_ReceiveTransmit = "RCV";
		receivedInterchange.EI_BodyText = bodyText;

		var receivedMessage = receivedInterchange.ContainedMessages.AddNew();
		receivedMessage.EM_ApplicationCode = receivedInterchange.EI_ApplicationCode;
		receivedMessage.EM_ReceiveTransmit = receivedInterchange.EI_ReceiveTransmit;
		receivedMessage.EM_MessageText = receivedInterchange.EI_BodyText;
		receivedMessage.MessageNumberStrategy = new FixedMessageNumberStrategy(messageNumber++);
		receivedMessage.EM_MessageType = "RES";

		Factory.Save();

		return (receivedInterchange, receivedMessage);
	}
}
