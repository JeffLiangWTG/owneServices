using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(NCTSCustomsServiceErrorUniversalEventResponseMessageProcessor))]
	sealed class NCTSCustomsServiceErrorUniversalEventResponseMessageProcessorTest : EventMessageProcessorAbstractTest<NCTSCustomsServiceErrorUniversalEventResponseMessageProcessor>
	{
		protected override NCTSCustomsServiceErrorUniversalEventResponseMessageProcessor GetProcessorCore(LoggingInformation logger) => new NCTSCustomsServiceErrorUniversalEventResponseMessageProcessor(logger);

		protected override EventMessageProcessor Processor => new NCTSCustomsServiceErrorUniversalEventResponseMessageProcessor(new LoggingInformation());

		public override void TestProcessServiceErrorMessage()
		{
			var nctsHeader = SetUpNctsHeader(true);
			var message = CreateNewEDIMessage(null, serviceErrorMessage, ZGuid.BrettsGuid);

			processor.PreProcessMessage(message);
			processor.ProcessMessage(message);

			var expectedMessageInterpretation = "<H3>Universal Event Service Error</H3>" +
						"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
						"<tr><td><strong>Event Type</strong></td><td><strong>Message Type</strong></td><td><strong>Reason</strong></td></tr>" +
						"<tr><td>IRJ</td><td>NPI</td><td>The MRN 21ES00999930MXRZN5 exceeded the retry count of 5 and still receives no update</td></tr>" +
						"</table>";

			CombineAssertions(() =>
			{
				AssertContains("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, message.EM_MessageInterpretation);
				AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("EffectiveMessageStatus", LogicalStatusList.Codes.Error, nctsHeader.EffectiveMessageStatus);
			});
		}

		public void TestNoteForUnableToFindALinkedBusinessObject()
		{
			var entryHeader = SetUpNctsHeader(false);
			var message = CreateNewEDIMessage(null, serviceErrorMessage, ZGuid.BrettsGuid);
			processor.PreProcessMessage(message);
			AssertEquals("The processing of the message with interchange failed because the message could not be linked to a NCTS declaration.", message.Notes.FindByDescription(Constants.MessageProcessingNotes.ProcessingLog).Single(x => x.ST_IsCustomDescription).ST_NoteText);
		}

		NctsHeader SetUpNctsHeader(bool createInterchange)
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			if (createInterchange)
			{
				SetSentInterchange(nctsHeader);
			}

			return nctsHeader;
		}
	}
}
