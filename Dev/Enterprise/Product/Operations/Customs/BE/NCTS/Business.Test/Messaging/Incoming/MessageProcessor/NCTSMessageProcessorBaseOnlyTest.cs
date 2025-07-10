using System;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.Business.Testing;
using Enterprise.Customs.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(NCTSMessageProcessorForTest))]
	sealed class NCTSMessageProcessorBaseOnlyTest : MessageProcessorTestCase<NCTSMessageProcessorForTest, IInboundProvider>
	{
		public void TestNoteForUnableToFindALinkedBusinessObject()
		{
			processor.PreProcessMessage(incomingMessage);
			AssertEquals("The processing of the message with interchange failed because the message could not be linked to a NCTS declaration.", incomingMessage.Notes.FindByDescription(Constants.MessageProcessingNotes.ProcessingLog).Single(x => x.ST_IsCustomDescription).ST_NoteText);
		}

		public void TestGetBranchPkFromJobBO()
		{
			var header = Factory.New<NctsArrivalMovementHeader>();
			processor.Header = header;
			processor.PreProcessMessage(incomingMessage);
			AssertEquals(header.RegistryBranchPK, incomingMessage.EM_GB);
		}

		protected override string ExpectedMessageFriendlyName => "NCTS Message Processing Base";

		protected override Type ExpectedMessageInterpreterType => null;

		protected override NCTSMessageProcessorForTest Processor => processor;

		protected override void SetUp()
		{
			base.SetUp();
			processor = new NCTSMessageProcessorForTest(new LoggingInformation());
			incomingMessage = CreateIncomingMessage(Factory);
		}

		NCTSMessageProcessorForTest processor;
		BEMessage incomingMessage;
	}

	sealed class NCTSMessageProcessorForTest : NCTSMessageProcessor<IInboundProvider>
	{
		public NCTSMessageProcessorForTest(LoggingInformation logger) : base(logger) { }

		protected override string MessageFriendlyNameCore => "NCTS Message Processing Base";

		public CusInBondMoveHeader Header { get; set; }

		public (ZString jobStatus, ZString messageStatus, ZString processLog) JobAndMessageStatus { get; set; }

		protected internal override IInboundProvider GetMessageDataProvider(BEMessage message) => new Mock<IInboundProvider>().Object;

		protected override BusinessObject FindParentOfMessage(BEMessage message, IInboundProvider messageDataProvider) => Header;

		protected override void ProcessMessageCore(BEMessage message, IInboundProvider messageDataProvider)
		{
			throw new NotImplementedException();
		}
	}
}
