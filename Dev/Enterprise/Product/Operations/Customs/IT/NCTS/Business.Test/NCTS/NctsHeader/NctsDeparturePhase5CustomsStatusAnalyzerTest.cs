using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsDeparturePhase5CustomsStatusAnalyzerTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception when ncts parameter is null",
			() => new NctsDeparturePhase5CustomsStatusAnalyzer(null));

		AssertExceptionThrown<ArgumentException>("Exception when the ncts is not a phase5",
			"The provided NctsDepartureMovementHeader object must be configured for a Phase 5. Ensure that 'IsPhase5' is set to true.",
			() => new NctsDeparturePhase5CustomsStatusAnalyzer(Factory.New<NctsDepartureMovementHeader>()));
	}

	public void TestTryDetermineCustomsStatusBasedOnLastMessageSent()
	{
		var customsStatusAnalyzer = new NctsDeparturePhase5CustomsStatusAnalyzer(movementHeader);
		var statusIsValid = customsStatusAnalyzer.TryDetermineCustomsStatusBasedOnLastMessageSent(out var customsStatus);

		CombineAssertions("When MovementHeader has no Messages, TryDetermineCustomsStatusBasedOnLastMessageSent()", () =>
		{
			AssertEquals("Status is Valid", false, statusIsValid);
			AssertEquals("Customs Status", "", customsStatus);
		});
	}

	public void TestTryDetermineCustomsStatusBasedOnLastMessageSent_WhenHasNoRelevantMessages()
	{
		var sentCanSessionGuid = ZGuid.NewZGuid();
		AddNewMessageWithInterchange("CAN", sentCanSessionGuid);
		AddNewMessageWithInterchange("RES", sentCanSessionGuid, PositiveResponseManifestResourceKey);
		AddNewMessageWithInterchange("RES", sentCanSessionGuid, ClearanceResponseManifestResourceKey);

		var sentAmdSessionGuid = ZGuid.NewZGuid();
		AddNewMessageWithInterchange("AMD", sentAmdSessionGuid);
		AddNewMessageWithInterchange("RES", sentAmdSessionGuid, ClearanceResponseManifestResourceKey);

		var statusIsValid = new NctsDeparturePhase5CustomsStatusAnalyzer(movementHeader).TryDetermineCustomsStatusBasedOnLastMessageSent(out var customsStatus);

		CombineAssertions("When MovementHeader has no relevant NEW Messages, TryDetermineCustomsStatusBasedOnLastMessageSent()", () =>
		{
			AssertEquals("Status is Valid", false, statusIsValid);
			AssertEquals("Customs Status", "", customsStatus);
		});
	}

	public void TestTryDetermineCustomsStatusBasedOnLastMessageSent_WhenNewHasNoResponseMessages()
	{
		var sentSessionGuid = ZGuid.NewZGuid();
		AddNewMessageWithInterchange("NEW", sentSessionGuid);

		var statusIsValid = new NctsDeparturePhase5CustomsStatusAnalyzer(movementHeader).TryDetermineCustomsStatusBasedOnLastMessageSent(out var customsStatus);

		CombineAssertions("When MovementHeader NEW Message has no response messages, TryDetermineCustomsStatusBasedOnLastMessageSent()", () =>
		{
			AssertEquals("Status is Valid", false, statusIsValid);
			AssertEquals("Customs Status", "", customsStatus);
		});
	}

	public void TestTryDetermineCustomsStatusBasedOnLastMessageSent_WhenHasMrnResponseMessage()
	{
		var sentSessionGuid = ZGuid.NewZGuid();
		AddNewMessageWithInterchange("NEW", sentSessionGuid);
		AddNewMessageWithInterchange("RES", sentSessionGuid, PositiveResponseManifestResourceKey);

		var statusIsValid = new NctsDeparturePhase5CustomsStatusAnalyzer(movementHeader).TryDetermineCustomsStatusBasedOnLastMessageSent(out var customsStatus);

		CombineAssertions("When MovementHeader NEW Message has only one MRN response messages, TryDetermineCustomsStatusBasedOnLastMessageSent()", () =>
		{
			AssertEquals("Status is Valid", true, statusIsValid);
			AssertEquals("Customs Status", "MRN", customsStatus);
		});
	}

	public void TestTryDetermineCustomsStatusBasedOnLastMessageSent_WhenHasMrnAndReleaseResponseMessages()
	{
		var sentSessionGuid = ZGuid.NewZGuid();
		AddNewMessageWithInterchange("NEW", sentSessionGuid);
		AddNewMessageWithInterchange("RES", sentSessionGuid, ClearanceResponseManifestResourceKey);
		AddNewMessageWithInterchange("RES", sentSessionGuid, PositiveResponseManifestResourceKey);

		var statusIsValid = new NctsDeparturePhase5CustomsStatusAnalyzer(movementHeader).TryDetermineCustomsStatusBasedOnLastMessageSent(out var customsStatus);

		CombineAssertions("When MovementHeader NEW Message has MRN and REL response messages, TryDetermineCustomsStatusBasedOnLastMessageSent()", () =>
		{
			AssertEquals("Status is Valid", true, statusIsValid);
			AssertEquals("Customs Status", "REL", customsStatus);
		});
	}

	public void TestTryDetermineCustomsStatusBasedOnLastMessageSent_WhenHasMrnAndUnderControlResponseMessages()
	{
		var sentSessionGuid = ZGuid.NewZGuid();
		AddNewMessageWithInterchange("NEW", sentSessionGuid);
		AddNewMessageWithInterchange("RES", sentSessionGuid, UnderControlResponseManifestResourceKey);
		AddNewMessageWithInterchange("RES", sentSessionGuid, ClearanceResponseManifestResourceKey);

		var statusIsValid = new NctsDeparturePhase5CustomsStatusAnalyzer(movementHeader).TryDetermineCustomsStatusBasedOnLastMessageSent(out var customsStatus);

		CombineAssertions("When MovementHeader NEW Message has MRN and UCL response messages, TryDetermineCustomsStatusBasedOnLastMessageSent()", () =>
		{
			AssertEquals("Status is Valid", true, statusIsValid);
			AssertEquals("Customs Status", "REL", customsStatus);
		});
	}

	public void TestTryDetermineCustomsStatusBasedOnLastMessageSent_WhenHasMultipleNewMessages()
	{
		var sentSessionGuid1 = ZGuid.NewZGuid();
		AddNewMessageWithInterchange("NEW", sentSessionGuid1);
		AddNewMessageWithInterchange("RES", sentSessionGuid1, ClearanceResponseManifestResourceKey);

		var sentSessionGuid2 = ZGuid.NewZGuid();
		AddNewMessageWithInterchange("NEW", sentSessionGuid2);
		AddNewMessageWithInterchange("RES", sentSessionGuid2, UnderControlResponseManifestResourceKey);

		var statusIsValid = new NctsDeparturePhase5CustomsStatusAnalyzer(movementHeader).TryDetermineCustomsStatusBasedOnLastMessageSent(out var customsStatus);

		CombineAssertions("When MovementHeader has more NEW Messages, TryDetermineCustomsStatusBasedOnLastMessageSent()", () =>
		{
			AssertEquals("Status is Valid", true, statusIsValid);
			AssertEquals("Customs Status", "CO3", customsStatus);
		});
	}

	public void TestTryDetermineCustomsStatusBasedOnLastMessageSent_WhenHasIrildesNegative198ResponseMessage()
	{
		var sentSessionGuid = ZGuid.NewZGuid();
		AddNewMessageWithInterchange("NEW", sentSessionGuid);
		AddNewMessageWithInterchange("RES", sentSessionGuid, ClearanceResponseManifestResourceKey);
		AddNewMessageWithInterchange("IRR", sentSessionGuid, IrildesNegativeResponse198ResourceKey);
		AddNewMessageWithInterchange("RES", sentSessionGuid, PositiveResponseManifestResourceKey);

		var statusIsValid = new NctsDeparturePhase5CustomsStatusAnalyzer(movementHeader).TryDetermineCustomsStatusBasedOnLastMessageSent(out var customsStatus);

		CombineAssertions("When MovementHeader NEW Message has MRN, IRR negative 198 and REL response messages, TryDetermineCustomsStatusBasedOnLastMessageSent()", () =>
		{
			AssertEquals("Status is Valid", true, statusIsValid);
			AssertEquals("Customs Status", "REL", customsStatus);
		});
	}

	public void TestTryDetermineCustomsStatusBasedOnLastMessageSent_WhenHasIrildesPositive199ResponseMessage()
	{
		var sentSessionGuid = ZGuid.NewZGuid();
		AddNewMessageWithInterchange("NEW", sentSessionGuid);
		AddNewMessageWithInterchange("RES", sentSessionGuid, ClearanceResponseManifestResourceKey);
		AddNewMessageWithInterchange("IRR", sentSessionGuid, IrildesPositiveResponse199ResourceKey);
		AddNewMessageWithInterchange("RES", sentSessionGuid, PositiveResponseManifestResourceKey);

		var statusIsValid = new NctsDeparturePhase5CustomsStatusAnalyzer(movementHeader).TryDetermineCustomsStatusBasedOnLastMessageSent(out var customsStatus);

		CombineAssertions("When MovementHeader NEW Message has MRN, IRR positive 199 and REL response messages, TryDetermineCustomsStatusBasedOnLastMessageSent()", () =>
		{
			AssertEquals("Status is Valid", true, statusIsValid);
			AssertEquals("Customs Status", "WRO", customsStatus);
		});
	}

	public void TestTryDetermineCustomsStatusBasedOnLastMessageSent_WhenHasIrildesPositive200ResponseMessage()
	{
		var sentSessionGuid = ZGuid.NewZGuid();
		AddNewMessageWithInterchange("NEW", sentSessionGuid);
		AddNewMessageWithInterchange("RES", sentSessionGuid, ClearanceResponseManifestResourceKey);
		AddNewMessageWithInterchange("IRR", sentSessionGuid, IrildesPositiveResponse200ResourceKey);
		AddNewMessageWithInterchange("RES", sentSessionGuid, PositiveResponseManifestResourceKey);

		var statusIsValid = new NctsDeparturePhase5CustomsStatusAnalyzer(movementHeader).TryDetermineCustomsStatusBasedOnLastMessageSent(out var customsStatus);

		CombineAssertions("When MovementHeader NEW Message has MRN, IRR positive 200 and REL response messages, TryDetermineCustomsStatusBasedOnLastMessageSent()", () =>
		{
			AssertEquals("Status is Valid", true, statusIsValid);
			AssertEquals("Customs Status", "WRO", customsStatus);
		});
	}

	public void TestTryDetermineCustomsStatusBasedOnLastMessageSent_WhenHasIrildesPositive200ResponseMessageWithNoWriteOffData()
	{
		var sentSessionGuid = ZGuid.NewZGuid();
		AddNewMessageWithInterchange("NEW", sentSessionGuid);
		AddNewMessageWithInterchange("RES", sentSessionGuid, ClearanceResponseManifestResourceKey);
		AddNewMessageWithInterchange("IRR", sentSessionGuid, IrildesPositiveResponse200WithNoWriteOffDataResourceKey);
		AddNewMessageWithInterchange("RES", sentSessionGuid, PositiveResponseManifestResourceKey);

		var statusIsValid = new NctsDeparturePhase5CustomsStatusAnalyzer(movementHeader).TryDetermineCustomsStatusBasedOnLastMessageSent(out var customsStatus);

		CombineAssertions("When MovementHeader NEW Message has MRN, IRR positive 200 (with no write off data) and REL response messages, TryDetermineCustomsStatusBasedOnLastMessageSent()", () =>
		{
			AssertEquals("Status is Valid", true, statusIsValid);
			AssertEquals("Customs Status", "REL", customsStatus);
		});
	}

	public void TestTryDetermineCustomsStatusBasedOnLastMessageSent_WhenHasMultipleIrildesResponseMessages()
	{
		var sentSessionGuid = ZGuid.NewZGuid();
		AddNewMessageWithInterchange("NEW", sentSessionGuid);
		AddNewMessageWithInterchange("RES", sentSessionGuid, ClearanceResponseManifestResourceKey);
		AddNewMessageWithInterchange("IRR", sentSessionGuid, IrildesNegativeResponse198ResourceKey);
		AddNewMessageWithInterchange("IRR", sentSessionGuid, IrildesPositiveResponse200ResourceKey);
		AddNewMessageWithInterchange("IRR", sentSessionGuid, IrildesPositiveResponse200WithNoWriteOffDataResourceKey);
		AddNewMessageWithInterchange("RES", sentSessionGuid, PositiveResponseManifestResourceKey);

		var statusIsValid = new NctsDeparturePhase5CustomsStatusAnalyzer(movementHeader).TryDetermineCustomsStatusBasedOnLastMessageSent(out var customsStatus);

		CombineAssertions("When MovementHeader NEW Message has MRN, multiple IRR messages and REL response messages, TryDetermineCustomsStatusBasedOnLastMessageSent()", () =>
		{
			AssertEquals("Status is Valid", true, statusIsValid);
			AssertEquals("Customs Status", "WRO", customsStatus);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		var header = Factory.NewDepartureNctsHeader();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		movementHeader = header.MovementHeader;
	}

	void AddNewMessageWithInterchange(ZString messageType, ZGuid sessionGuid, string resourceKey = null)
	{
		var message = movementHeader.Messages.AddNew();
		message.EM_ApplicationCode = "ITH";
		message.EM_MessageType = messageType;
		message.EM_SystemCreateTimeUtc = DateTime.UtcNow;
		message.EM_SystemLastEditTimeUtc = DateTime.UtcNow;
		message.EM_Status = (messageType == "RES" || messageType == "IRR")
			? Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Received
			: Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Sent;

		if (!string.IsNullOrEmpty(resourceKey))
		{
			var messageText = ManifestResourceHelper.ReadManifestResourceContent(resourceKey);
			message.EM_MessageText = messageText;
		}

		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_SessionGUID = sessionGuid;
		message.EM_EI = interchange.PK;
	}

	NctsDepartureMovementHeader movementHeader;

	const string UnderControlResponseManifestResourceKey = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.NctsResponsePositiveWithMRNNotReleased.xml";
	const string PositiveResponseManifestResourceKey = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.NctsResponsePositiveWithMRN.xml";
	const string ClearanceResponseManifestResourceKey = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.NctsResponsePositiveWithMRNandClearance.xml";
	const string IrildesNegativeResponse198ResourceKey = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.Ncts_IrildesNegativeResponseError198.xml";
	const string IrildesPositiveResponse199ResourceKey = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.Ncts_IrildesPositiveResponseResultCode199.xml";
	const string IrildesPositiveResponse200ResourceKey = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.Ncts_IrildesPositiveResponseResultCode200.xml";
	const string IrildesPositiveResponse200WithNoWriteOffDataResourceKey = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.Ncts_IrildesPositiveResponseResultNoWriteOffData.xml";
}
