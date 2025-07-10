using System;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ITCInboundInterchangeProcessorTest : TestCaseWithFactory
{
	public void TestGenerateMessageFromInterchangeMrnExitVerification()
	{
		var interchange = GetInterchange(MessageProcessorConstants.InterchangeTypes.ExitVerification, "<ITMessage><PAN>123456</PAN></ITMessage>", "<table><webSite>NIT</webSite></table>", sessionGuid: ZGuid.NewZGuid());
		Factory.Save();

		var logger = new LoggingInformationForTesting();
		ITCInboundInterchangeProcessor.UnpackInterchanges(logger, CancellationToken.None);
		interchange.Reload();

		CombineAssertions(() => AssertProcessingResult(interchange, logger, $"Information: Interchange '{interchange.EI_InterchangeNum}' has been processed successfully.", EDIInterchange.Status.Received, 1));
	}

	public void TestGenerateMessageFromInterchangeMrnExitVerificationFail()
	{
		var interchange = GetInterchange(MessageProcessorConstants.InterchangeTypes.ExitVerification, ZString.Empty, ZString.Empty, sessionGuid: ZGuid.NewZGuid());
		Factory.Save();

		var logger = new LoggingInformationForTesting();
		ITCInboundInterchangeProcessor.UnpackInterchanges(logger, CancellationToken.None);
		interchange.Reload();

		CombineAssertions(() => AssertProcessingResult(interchange, logger, "Error: Invalid <PAN> tag value", EDIInterchange.Status.Failed, 0));
	}

	public void TestApplicationCodes()
	{
		var logger = new LoggingInformationForTesting();
		var inboundInterchangeProcessor = new ITCInboundInterchangeProcessorForTest(logger);

		AssertArrayEqualsByElements("ApplicationCodes", new string[] { "ITM", "ITH" }, inboundInterchangeProcessor.ApplicationCodesExposed);
	}

	public void TestAddNoteOnException()
	{
		var logger = new LoggingInformationForTesting();
		var inboundInterchangeProcessor = new ITCInboundInterchangeProcessorForTest(logger);

		AssertEquals("AddNoteOnException", true, inboundInterchangeProcessor.AddNoteOnExceptionExposed);

		var sessionGuid = ZGuid.NewZGuid();

		var interchange = GetInterchange("H1", "HEADER", "BODY", sessionGuid, "ITH");
		interchange.EI_SystemCreateTimeUtc = DateTime.UtcNow;
		var duplicatedInterchange = GetInterchange("H1", "HEADER", "BODY", sessionGuid, "ITH");
		duplicatedInterchange.EI_SystemCreateTimeUtc = DateTime.UtcNow.AddMinutes(5);
		Factory.Save();

		ITCInboundInterchangeProcessor.UnpackInterchanges(logger, CancellationToken.None);

		interchange.Reload();
		AssertEquals("Status", "RCV", interchange.EI_Status);
		AssertEquals("Notes count", 0, interchange.Notes.GetAllNotes().Count);

		duplicatedInterchange.Reload();
		AssertEquals("Status", "FAL", duplicatedInterchange.EI_Status);
		var notes = duplicatedInterchange.Notes.GetAllNotes().Cast<StmNote>();
		AssertEquals("Notes count", 1, notes.Count());
		AssertEquals("Note Description", $"Duplicated message of EI_PK = '{interchange.PK}'", notes.Single().ST_NoteDataAsText);
	}

	EDIInterchange GetInterchange(ZString interchangeType, ZString headerText, ZString bodyText, ZGuid sessionGuid, string applicationCode = ApplicationCodeList.Codes.ITCustoms)
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_From = "ITCUS";
		interchange.EI_To = "TEST";
		interchange.EI_ApplicationCode = applicationCode;
		interchange.EI_InterchangeType = interchangeType;
		interchange.EI_InterchangeNum = ZGuid.NewZGuid().ToString();
		interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		interchange.EI_Status = EDIInterchange.Status.Queued;
		interchange.EI_HeaderText = headerText;
		interchange.EI_BodyText = bodyText;
		interchange.EI_SessionGUID = sessionGuid;
		return interchange;
	}

	void AssertProcessingResult(EDIInterchange interchange, LoggingInformationForTesting logger, ZString expectedLog, ZString expectedStatus, ZInt expectedContainedMessagesCount)
	{
		AssertEquals("Logged Info", expectedLog, logger.AccumulatedLogMessages.ToString().Trim());
		AssertEquals("EI_Status", expectedStatus, interchange.EI_Status);
		AssertEquals("ContainedMessages.Count", expectedContainedMessagesCount, interchange.ContainedMessages.Count);
	}

	class ITCInboundInterchangeProcessorForTest : ITCInboundInterchangeProcessor
	{
		public ITCInboundInterchangeProcessorForTest(LoggingInformation logger) : base(logger)
		{
		}

		public string[] ApplicationCodesExposed => ApplicationCodes;

		public bool AddNoteOnExceptionExposed => AddNoteOnException;
	}
}
