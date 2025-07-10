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
	[TestedType(typeof(CC035CMessageProcessor))]
	sealed class CC035CMessageProcessorTest : NctsMessageProcessorTestCase<CC035CMessageProcessor, ICC035CDataProvider>
	{
		public void TestProcessCC035CMessageDeclaration()
		{
			var currentDateTime = DateTime.Now;
			MockProvider.Setup(x => x.NotificationDate).Returns(currentDateTime);
			MockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");
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
				AssertEquals($"EDIMEssage Status Should BE {EDIMessageStatusList.Codes.ProcessedOK}", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
				AssertEquals("CusInBondMoveHeader Customs Status should be 'URP'", NCTS5DepartureCustomsStatusList.Codes.UnderRecoveryProcedure, movementHeader.BM_CustomsStatus);
				AssertEquals("CusInBondMoveHeader Phase Should BE '015'", NctsMovementHeaderTransactionStatusList.Codes.Declaration, movementHeader.BM_Phase);
				AssertEquals("Message Status Should BE 'ACC'", LogicalStatusList.Codes.Accepted, nctsHeader.EffectiveMessageStatus);
				AssertContains("ST_NoteText", "Recovery notification for NCTS departure received at", incomingMessage.EM_MessageInterpretation);
			});
		}

		protected override string MovementType => NctsMovementType.Codes.Departure;

		protected override bool SupportsSearchByLRN => false;

		protected override bool SupportsSearchByMRN => true;

		protected override Mock<ICC035CDataProvider> MockProvider => mockProvider ?? (mockProvider = new Mock<ICC035CDataProvider>());
		Mock<ICC035CDataProvider> mockProvider;

		protected override Mock<CC035CMessageProcessor> MockProcessor => mockProcessor ?? (mockProcessor = new Mock<CC035CMessageProcessor>(new BatchProcessor.LoggingInformation()));
		Mock<CC035CMessageProcessor> mockProcessor;

		protected override ZString MessageTypeToInclude => BEIncomingMessageTypes.Codes.CC035C;

		protected override string ExpectedMessageFriendlyName => "Recovery Notification";

		protected override ZString[] PreProcessedOKCustomsStatus => new ZString[] { NCTS5DepartureCustomsStatusList.Codes.MrnAllocated };

		protected override Type ExpectedMessageInterpreterType => typeof(CC035CMessageInterpreter);
	}
}
