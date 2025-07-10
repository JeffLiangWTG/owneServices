using System;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC140CMessageProcessor))]
	sealed class CC140CMessageProcessorTest : NctsMessageProcessorTestCase<CC140CMessageProcessor, ICC140CDataProvider>
	{
		public void TestProcessCC140CMessageDeclaration()
		{
			var currentDateTime = DateTime.Now;
			MockProvider.Setup(x => x.RequestOnNonArrivedMovementDate).Returns(currentDateTime);
			MockProvider.Setup(x => x.LimitForResponseDate).Returns(currentDateTime);
			MockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;
			Extensions.CreateMovementReferenceNumber(nctsHeader, "22BE000000000012J1");
			Factory.Save();
			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("EDIMEssage Status Should BE " + EDIMessageStatusList.Codes.ProcessedOK, EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
				AssertEquals("CusInBondMoveHeader Customs Status Should BE 'ENQ'", NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry, movementHeader.BM_CustomsStatus);
				AssertEquals("CusInBondMoveHeader Phase Should BE '015'", NctsMovementHeaderTransactionStatusList.Codes.Declaration, movementHeader.BM_Phase);
				AssertEquals("Message Status Should BE 'ACC'", LogicalStatusList.Codes.Accepted, nctsHeader.EffectiveMessageStatus);
				AssertNotNull("CusInBondHeader Should have a CIP event logged", nctsHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code).SingleOrDefault());
				AssertEquals("Only 1 CES-event should have been created", 1, movementHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && x.SL_Reference == movementHeader.BM_CustomsStatus).Count());
			});
		}

		protected override string MovementType => NctsMovementType.Codes.Departure;

		protected override string ExpectedMessageFriendlyName => "Request on Non-Arrived Movement";

		protected override Type ExpectedMessageInterpreterType => typeof(CC140CMessageInterpreter);

		protected override bool SupportsSearchByLRN => false;

		protected override bool SupportsSearchByMRN => true;

		protected override Mock<ICC140CDataProvider> MockProvider => mockProvider ?? (mockProvider = new Mock<ICC140CDataProvider>());
		Mock<ICC140CDataProvider> mockProvider;

		protected override Mock<CC140CMessageProcessor> MockProcessor => mockProcessor ?? (mockProcessor = new Mock<CC140CMessageProcessor>(new BatchProcessor.LoggingInformation()));
		Mock<CC140CMessageProcessor> mockProcessor;

		protected override ZString MessageTypeToInclude => BEIncomingMessageTypes.Codes.CC140C;

		protected override ZString[] PreProcessedOKCustomsStatus => new ZString[] { NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit };
	}
}
