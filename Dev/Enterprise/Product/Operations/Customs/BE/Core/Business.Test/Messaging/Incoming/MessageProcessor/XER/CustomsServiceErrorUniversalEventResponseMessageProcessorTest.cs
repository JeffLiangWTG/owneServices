using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Common.EU;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CustomsServiceErrorUniversalEventResponseMessageProcessor))]
sealed class CustomsServiceErrorUniversalEventResponseMessageProcessorTest : EventMessageProcessorAbstractTest<CustomsServiceErrorUniversalEventResponseMessageProcessor>
{
	protected override CustomsServiceErrorUniversalEventResponseMessageProcessor GetProcessorCore(LoggingInformation logger) => new CustomsServiceErrorUniversalEventResponseMessageProcessor(logger);

	protected override EventMessageProcessor Processor => new CustomsServiceErrorUniversalEventResponseMessageProcessor(new LoggingInformation());

	public override void TestProcessServiceErrorMessage()
	{
		var entryHeader = SetUpEntryHeader(true);
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, serviceErrorMessage, ZGuid.BrettsGuid);

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
			AssertEquals("CH_EntryStatus", LogicalStatusList.Codes.Error, entryHeader.CH_EntryStatus);
		});
	}

	public void TestNoteForUnableToFindALinkedBusinessObject()
	{
		var entryHeader = SetUpEntryHeader(false);
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, serviceErrorMessage, ZGuid.BrettsGuid);
		processor.PreProcessMessage(message);
		AssertEquals("The processing of the message with interchange failed because the message could not be linked to a declaration.", message.Notes.FindByDescription(Constants.MessageProcessingNotes.ProcessingLog).Single(x => x.ST_IsCustomDescription).ST_NoteText);
	}

	CusEntryHeader SetUpEntryHeader(bool createInterchange)
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();

		if (createInterchange)
		{
			SetSentInterchange(entryHeader);
		}

		return entryHeader;
	}
}
