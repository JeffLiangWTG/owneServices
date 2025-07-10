using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.ctypes;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC057CMessageProcessor))]
	sealed class CC057CMessageProcessorTest : MessageProcessorTestCase<CC057CMessageProcessor, ICC057CDataProvider>
	{
		public void TestMessageTypesToInclude()
		{
			AssertSequencesEqual(new ZString[] { BEIncomingMessageTypes.Codes.CC057C }, Processor.MessageTypesToInclude);
		}

		public void TestPreProcessCC057C_NonMatchingMRN() => AssertPreProcess("1234567890", EDIMessageStatusList.Codes.Failed);

		public void TestPreProcessCC057C_MatchingMRN_Discarded_007()
		{
			var moveHeader = header.ArrivalMovementHeader;
			moveHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
			Factory.Save();
			AssertPreProcess("22BE000000000012J1", EDIMessageStatusList.Codes.Discarded);
		}

		public void TestPreProcessCC057C_MatchingMRN_Discarded_044()
		{
			var moveHeader = header.ArrivalMovementHeader;
			moveHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
			moveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			header.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;

			var transitOperationProvider = TransitOperationXmlProvider.New(new TransitOperationType21
			{
				BusinessRejectionType = BEOutgoingMessageTypes.Codes.CC044C,
				RejectionDateAndTime = new DateTime(2022, 04, 01, 12, 34, 56),
				RejectionCode = RejectionCodes.Codes.Code4,
				RejectionReason = "Invalid arrival date",
			});
			mockProvider.Setup(x => x.TransitOperation).Returns(transitOperationProvider);

			Factory.Save();
			AssertPreProcess("22BE000000000012J1", EDIMessageStatusList.Codes.Discarded);
		}

		public void TestPreProcessCC057C_MatchingMRN_Processed_007()
		{
			var moveHeader = header.ArrivalMovementHeader;
			moveHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.Unknown;
			moveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			header.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			Factory.Save();
			AssertPreProcess("22BE000000000012J1", EDIMessageStatusList.Codes.PreProcessedOK);
		}

		public void TestPreProcessCC057C_MatchingMRN_Processed_044()
		{
			var moveHeader = header.ArrivalMovementHeader;
			moveHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
			moveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			header.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;

			var transitOperationProvider = TransitOperationXmlProvider.New(new TransitOperationType21
			{
				BusinessRejectionType = BEOutgoingMessageTypes.Codes.CC044C,
				RejectionDateAndTime = new DateTime(2022, 04, 01, 12, 34, 56),
				RejectionCode = RejectionCodes.Codes.Code4,
				RejectionReason = "Invalid arrival date",
			});
			mockProvider.Setup(x => x.TransitOperation).Returns(transitOperationProvider);

			Factory.Save();
			AssertPreProcess("22BE000000000012J1", EDIMessageStatusList.Codes.PreProcessedOK);
		}

		void AssertPreProcess(string mrn, string expectedStatus)
		{
			mockProvider.Setup(x => x.MRN).Returns(mrn);
			Processor.PreProcessMessage(incomingMessage);

			AssertEquals("EDIMessage status should be " + expectedStatus, expectedStatus, incomingMessage.EM_Status);
		}

		public void TestProcessMessage_007()
		{
			var moveHeader = header.ArrivalMovementHeader;
			moveHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.Unknown;
			moveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			header.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;

			processor.PreProcessMessage(incomingMessage);
			processor.ProcessMessage(incomingMessage);

			CombineAssertions(() =>
			{
				AssertEquals("Message status", LogicalStatusList.Codes.Invalid, header.EffectiveMessageStatus);
				AssertEquals("EDI Message status", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			});
		}

		public void TestProcessMessage_044()
		{
			var moveHeader = header.ArrivalMovementHeader;
			moveHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
			moveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			header.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;

			var transitOperationProvider = TransitOperationXmlProvider.New(new TransitOperationType21
			{
				BusinessRejectionType = BEOutgoingMessageTypes.Codes.CC044C,
				RejectionDateAndTime = new DateTime(2022, 04, 01, 12, 34, 56),
				RejectionCode = RejectionCodes.Codes.Code4,
				RejectionReason = "Invalid arrival date",
			});
			mockProvider.Setup(x => x.TransitOperation).Returns(transitOperationProvider);

			processor.PreProcessMessage(incomingMessage);
			processor.ProcessMessage(incomingMessage);

			CombineAssertions(() =>
			{
				AssertEquals("Message status", LogicalStatusList.Codes.Invalid, header.EffectiveMessageStatus);
				AssertEquals("EDI Message status", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			});
		}

		public void TestFindParentOfMessage()
		{
			mockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");
			var movementHeader = header.ArrivalMovementHeader;
			movementHeader.BM_CustomsStatus = "ULR";
			movementHeader.BM_Phase = "044";
			header.EffectiveMessageStatus = "SNT";
			Extensions.CreateMovementReferenceNumber(header, "22BE000000000012J1");

			var departureHeader = Factory.New<NctsHeader>();
			departureHeader.SetMovementType(NctsMovementType.Codes.Departure);
			Extensions.CreateMovementReferenceNumber(departureHeader, "22BE000000000012J1");

			Factory.Save();

			var processor = new CC057CMessageProcessorForTest(new LoggingInformation());
			var parentHeader = (NctsHeader)processor.FindParentOfMessageExposed(incomingMessage, mockProvider.Object);

			AssertEquals("We should have found the arrival header", true, parentHeader.IsArrivalMovement);
		}

		protected override string ExpectedMessageFriendlyName => BEIncomingMessageTypes.Descriptions.CC057C;

		protected override CC057CMessageProcessor Processor => processor;

		protected override Type ExpectedMessageInterpreterType => typeof(CC057CMessageInterpreter);

		protected override void SetUp()
		{
			base.SetUp();
			mockProvider = new Mock<ICC057CDataProvider>();
			mockProvider.CallBase = true;
			mockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");

			var transitOperationProvider = TransitOperationXmlProvider.New(new TransitOperationType21
			{
				BusinessRejectionType = BEOutgoingMessageTypes.Codes.CC007C,
				RejectionDateAndTime = new DateTime(2022, 04, 01, 12, 34, 56),
				RejectionCode = RejectionCodes.Codes.Code4,
				RejectionReason = "Invalid arrival date",
			});
			mockProvider.Setup(x => x.TransitOperation).Returns(transitOperationProvider);

			var functionalError = FunctionalErrorXmlProvider.New(new FunctionalErrorType01());
			functionalError.ErrorPointer = "cc015c.DepartureTransportMeans(2).typeOfIdentification";
			functionalError.ErrorCode = EU.NCTS.Business.FunctionalErrorCodes.Codes.Code12;
			functionalError.ErrorReason = "Type of transport does not exist";
			functionalError.OriginalAttributeValue = "32";
			var functionalErrorList = new List<FunctionalErrorXmlProvider> { functionalError };
			mockProvider.Setup(x => x.FunctionalErrors).Returns(new ReadOnlyCollection<FunctionalErrorXmlProvider>(functionalErrorList));

			mockProcessor = new Mock<CC057CMessageProcessor>(new LoggingInformation());
			mockProcessor.CallBase = true;
			provider = mockProvider.Object;
			mockProcessor.Setup(x => x.GetMessageDataProvider(It.IsAny<BEMessage>())).Returns(provider);
			processor = mockProcessor.Object;

			incomingMessage = CreateIncomingMessage(Factory);
			header = Factory.NewWithValidTestData<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, header.Company.Country.Code).CE_EntryNum = "22BE000000000012J1";

			Factory.Save();
		}

		Mock<ICC057CDataProvider> mockProvider;
		Mock<CC057CMessageProcessor> mockProcessor;
		CC057CMessageProcessor processor;
		ICC057CDataProvider provider;
		BEMessage incomingMessage;
		NctsHeader header;
	}

	class CC057CMessageProcessorForTest : CC057CMessageProcessor
	{
		public CC057CMessageProcessorForTest(LoggingInformation logger) : base(logger) { }

		public BusinessObject FindParentOfMessageExposed(BEMessage message, ICC057CDataProvider messageDataProvider) => base.FindParentOfMessage(message, messageDataProvider);
	}
}
