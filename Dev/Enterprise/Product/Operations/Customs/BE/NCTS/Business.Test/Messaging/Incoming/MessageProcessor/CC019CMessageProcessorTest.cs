using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC019CMessageProcessor))]
	sealed class CC019CMessageProcessorTest : NctsMessageProcessorTestCase<CC019CMessageProcessor, ICC019CDataProvider>
	{
		public void TestProcessCC019CMessageDeclaration()
		{
			var currentDateTime = DateTime.Now;
			mockProvider.Setup(x => x.WriteOffDate).Returns(currentDateTime);
			mockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");
			mockProvider.Setup(x => x.NotificationText).Returns("Not null");
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var movementHeader = nctsHeader.MovementHeader;
			Extensions.CreateMovementReferenceNumber(nctsHeader, "22BE000000000012J1");
			Factory.Save();
			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			CombineAssertions(() =>
			{
				AssertEquals("EDIMEssage Status Should BE " + EDIMessageStatusList.Codes.ProcessedOK, EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
				AssertEquals("CusInBondMoveHeader Customs Status should be DIS", NCTS5DepartureCustomsStatusList.Codes.DiscrepanciesAtDestination, movementHeader.BM_CustomsStatus);
				AssertEquals("CusInBondMoveHeader Phase should be '015'", NctsMovementHeaderTransactionStatusList.Codes.Declaration, movementHeader.BM_Phase);
				AssertEquals("Message Status Should BE PRS", LogicalStatusList.Codes.Accepted, nctsHeader.EffectiveMessageStatus);
			});
		}

		protected override string MovementType => NctsMovementType.Codes.Departure;

		protected override bool SupportsSearchByLRN => false;

		protected override bool SupportsSearchByMRN => true;

		protected override Mock<ICC019CDataProvider> MockProvider => mockProvider ?? (mockProvider = new Mock<ICC019CDataProvider>());
		Mock<ICC019CDataProvider> mockProvider;

		protected override Mock<CC019CMessageProcessor> MockProcessor => mockProcessor ?? (mockProcessor = new Mock<CC019CMessageProcessor>(new BatchProcessor.LoggingInformation()));
		Mock<CC019CMessageProcessor> mockProcessor;

		protected override ZString MessageTypeToInclude => BEIncomingMessageTypes.Codes.CC019C;

		protected override string ExpectedMessageFriendlyName => "Discrepancies";

		protected override ZString[] PreProcessedOKCustomsStatus => new ZString[] { NCTS5DepartureCustomsStatusList.Codes.MrnAllocated };

		protected override Type ExpectedMessageInterpreterType => typeof(CC019CMessageInterpreter);
	}
}
