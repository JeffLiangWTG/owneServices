using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(AcknowledgementMessageProcessor))]
	sealed class AcknowledgementMessageProcessorTest : NctsMessageProcessorTestCase<AcknowledgementMessageProcessor, IAcknowledgementMessageDataProvider>
	{
		protected override string MovementType => NctsMovementType.Codes.Arrival;

		protected override bool SupportsSearchByLRN => false;

		protected override bool SupportsSearchByMRN => false;

		protected override bool SupportsSearchByEDIInterchange => true;

		protected override Mock<IAcknowledgementMessageDataProvider> MockProvider => mockProvider ?? (mockProvider = new Mock<IAcknowledgementMessageDataProvider>());
		Mock<IAcknowledgementMessageDataProvider> mockProvider;

		protected override Mock<AcknowledgementMessageProcessor> MockProcessor => mockProcessor ?? (mockProcessor = new Mock<AcknowledgementMessageProcessor>(new BatchProcessor.LoggingInformation()));
		Mock<AcknowledgementMessageProcessor> mockProcessor;

		protected override ZString[] PreProcessedOKCustomsStatus => new ZString[] { LogicalStatusList.Codes.Acknowledged };

		protected override ZString MessageTypeToInclude => BEIncomingMessageTypes.Codes.Acknowledgement;

		protected override string ExpectedMessageFriendlyName => BEIncomingMessageTypes.Descriptions.Acknowledgement;

		protected override Type ExpectedMessageInterpreterType => typeof(AcknowledgementMessageInterpreter);

		protected override bool MessageCannotBeDiscarded => true;

		public void TestCIDCreated()
		{
			MockProvider.Setup(m => m.CorrelationId).Returns("CIDNumberTest");
			SetupEDIInterchangeWithLink();
			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			var header = (NctsHeader)incomingMessage.EM_LinkedObject;
			var cidEntryNumber = CusEntryNumber.Load(header.ArrivalMovementHeader, "CID", "BE");
			CombineAssertions(() =>
			{
				AssertEquals("CorrelationID in CE_EntryNum", "CIDNumberTest", cidEntryNumber.CE_EntryNum);
				AssertEquals("CorrelationID in CE_EntryLineReference", "CIDNumberTest", cidEntryNumber.CE_EntryLineReference);
			});
		}
	}
}
