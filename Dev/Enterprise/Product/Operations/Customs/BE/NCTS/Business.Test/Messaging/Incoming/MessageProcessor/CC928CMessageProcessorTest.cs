using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC928CMessageProcessor))]
	sealed class CC928CMessageProcessorTest : NctsMessageProcessorTestCase<CC928CMessageProcessor, ICC928CDataProvider>
	{
		public void TestProcessing()
		{
			var currentDateTime = ZDateTime.Now;
			mockProvider.Setup(x => x.LRN).Returns("2204528148060XXXXXX");
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_PaperlessInbondNum = "2204528148060XXXXXX";
			movementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.Unknown;
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			Extensions.CreateMovementReferenceNumber(nctsHeader, "22BE000000000012J1");
			Factory.Save();
			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			CombineAssertions(() =>
			{
				AssertEquals("Customs Status", NCTS5DepartureCustomsStatusList.Codes.Acknowledged, movementHeader.BM_CustomsStatus);
				AssertEquals("Phase", NctsMovementHeaderTransactionStatusList.Codes.Declaration, movementHeader.BM_Phase);
				AssertEquals("Header Message Status", LogicalStatusList.Codes.Accepted, nctsHeader.EffectiveMessageStatus);
				AssertEquals("Message Status", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			});
		}

		public void TestProcessing_AdditionalDeclTypeD()
		{
			var currentDateTime = ZDateTime.Now;
			mockProvider.Setup(x => x.LRN).Returns("2204528148060XXXXXX");
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_PaperlessInbondNum = "2204528148060XXXXXX";
			movementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.Unknown;
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
			Extensions.CreateMovementReferenceNumber(nctsHeader, "22BE000000000012J1");
			Factory.Save();
			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			CombineAssertions(() =>
			{
				AssertEquals("Customs Status", NCTS5DepartureCustomsStatusList.Codes.PreLodged, movementHeader.BM_CustomsStatus);
				AssertEquals("Phase", NctsMovementHeaderTransactionStatusList.Codes.Declaration, movementHeader.BM_Phase);
				AssertEquals("Header Message Status", LogicalStatusList.Codes.Accepted, nctsHeader.EffectiveMessageStatus);
				AssertEquals("Message Status", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			});
		}

		protected override string MovementType => NctsMovementType.Codes.Departure;

		protected override bool SupportsSearchByLRN => true;

		protected override bool SupportsSearchByMRN => false;

		protected override Mock<ICC928CDataProvider> MockProvider => mockProvider ?? (mockProvider = new Mock<ICC928CDataProvider>());
		Mock<ICC928CDataProvider> mockProvider;

		protected override Mock<CC928CMessageProcessor> MockProcessor => mockProcessor ?? (mockProcessor = new Mock<CC928CMessageProcessor>(new BatchProcessor.LoggingInformation()));
		Mock<CC928CMessageProcessor> mockProcessor;

		protected override ZString MessageTypeToInclude => BE.Business.BEIncomingMessageSubTypes.Codes.CC928C;

		protected override string ExpectedMessageFriendlyName => BE.Business.BEIncomingMessageSubTypes.Descriptions.CC928C;

		protected override ZString[] PreProcessedOKCustomsStatus => new ZString[] { NctsTransitStatusList.Codes.Unknown };

		protected override ZString[] PreProcessedOKPhase => new ZString[] { NctsMovementHeaderTransactionStatusList.Codes.Declaration };

		protected override ZString[] PreProcessOKMessageStatus => new ZString[] { LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Sent };

		protected override ZString[] PreProcessNOKMessageStatus => new ZString[] { LogicalStatusList.Codes.Invalid, LogicalStatusList.Codes.Error, NctsMessageStatusList.Codes.Unknown };

		protected override Type ExpectedMessageInterpreterType => typeof(CC928CMessageInterpreter);
	}
}
