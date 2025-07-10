using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EdecStatusMessageProcessor))]
sealed class EdecStatusMessageProcessorTest : BaseInboundMessageProcessorTest
{
	protected override (string MessageType, Event ExpectedEvent)[] MessageTypes => new[] { (JobMessageTypeList.Codes.Import, Events.CustomsEntryStatus), (JobMessageTypeList.Codes.Export, Events.CustomsEntryStatus) };

	protected override ZString ApplicationCode => ApplicationCodeList.Codes.CHCustomsEdec;

	protected override ZString MessageSubType => MessageSubTypeCodeList.Codes.Status;

	protected override ApplicationTypeMessageProcessor GetMessageProcessor(LoggingInformation logger) => new EdecStatusMessageProcessor(logger);

	protected override ZString ExpectedMessageFriendlyName => "Status Message Processor";

	protected override ZString BGMReference => "35253";

	ZString receivedDeclarationStatus = SwissCustomsConstants.CustomsStatusCodes.ShipmentRelease;

	ZString goodsDeclarationStatusRelease = "1";

	readonly ZString statusDate = "2022-05-23";

	readonly ZString statusTime = "08:21:05";

	protected override ZString ReceivedMessage => $@"<?xml version='1.0' encoding='UTF-8'?>
<goodsDeclarationsResponse xmlns='http://www.e-dec.ch/xml/schema/edecResponse/v4' xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' schemaVersion='4.0' xsi:schemaLocation='http://www.e-dec.ch/xml/schema/edecResponse/v4 http://www.ezv.admin.ch/pdf_linker.php?doc=edecResponse_v_4_0'>
  <goodsDeclarationStatus>
    <traderDeclarationNumber>{BGMReference}</traderDeclarationNumber>
    <traderReference>DEC-5147</traderReference>
    <customsOfficeNumber>CH001251</customsOfficeNumber>
    <customsDeclarationNumber>22CHEI000043115420</customsDeclarationNumber>
    <customsDeclarationVersion>1</customsDeclarationVersion>
    <statusDate>{statusDate}</statusDate>
    <statusTime>{statusTime}</statusTime>
    <status>{receivedDeclarationStatus}</status>
    <materialCheck>0</materialCheck>
    <release>{goodsDeclarationStatusRelease}</release>
  </goodsDeclarationStatus>
</goodsDeclarationsResponse>
";
	public void TestCusEntryHeader() => CombineAssertions(() =>
	{
		foreach (var messageType in MessageTypes)
		{
			var factory = new BusinessObjectFactory();

			var (entryHeader, ediMessage) = MessageProcessorTestHelper.CreateHeaderAndMessage(factory, messageType.MessageType, MessageSubType, ReceivedMessage, BGMReference);
			factory.Save();

			ProcessMessage(ediMessage);
			factory.Save();

			var logEntry = factory.Load<StmALog>(new ZQuery(ZArchitecture.Schema.StmALogSchema.SL_Parent, entryHeader.PK)).FirstOrDefault();

			AssertEquals("CH_EntryStatus", SwissCustomsConstants.CustomsStatusCodes.ShipmentRelease, entryHeader.CH_EntryStatus);
			AssertNotNull(logEntry);
			AssertEquals("Event type", messageType.ExpectedEvent, logEntry.Event);
			AssertEquals("Event", SwissCustomsConstants.CustomsStatusCodes.ShipmentRelease, logEntry?.SL_Reference);
		}
	});

	public void TestCH_StatusHandling() => CombineAssertions(() =>
	{
		foreach (var messageType in MessageTypes)
		{
			AssertCH_StatusValue(messageType.MessageType, SwissCustomsConstants.CustomsStatusCodes.ShipmentRelease, CHLogicalStatusList.Codes.Sent, CHLogicalStatusList.Codes.Sent);
			AssertCH_StatusValue(messageType.MessageType, SwissCustomsConstants.CustomsStatusCodes.CustomsDeclarationReceived, CHLogicalStatusList.Codes.Sent, CHLogicalStatusList.Codes.Acknowledged);
		}
	});

	public void TestEntryReleaseDateAfterStatusUpdate()
	{
		CombineAssertions(() =>
		{
			goodsDeclarationStatusRelease = "0";
			AssertNotEquals("CH_EntryReleaseDate", $"{statusDate} {statusTime}", GetEntryReleaseDateAfterStatusUpdate(ReceivedMessage).ToString("yyyy-MM-dd HH:mm:ss"));
			goodsDeclarationStatusRelease = "1";
			AssertEquals("CH_EntryReleaseDate", $"{statusDate} {statusTime}", GetEntryReleaseDateAfterStatusUpdate(ReceivedMessage).ToString("yyyy-MM-dd HH:mm:ss"));
		});

		ZDateTime GetEntryReleaseDateAfterStatusUpdate(string response)
		{
			var factory = new BusinessObjectFactory();
			var (entryHeader, ediMessage) = MessageProcessorTestHelper.CreateHeaderAndMessage(factory, JobMessageTypeList.Codes.Import, MessageSubType, response, BGMReference);

			entryHeader.CH_PhaseStatus = PassarDeclarationPhaseList.Codes.Amendment;
			factory.Save();

			ProcessMessage(ediMessage);
			factory.Save();

			return entryHeader.CH_EntryReleaseDate;
		}
	}

	void AssertCH_StatusValue(ZString messageType, ZString entryStatus, ZString currentStatus, ZString expectedStatus)
	{
		receivedDeclarationStatus = entryStatus;
		var (entryHeader, ediMessage) = MessageProcessorTestHelper.CreateHeaderAndMessage(Factory, messageType, MessageSubType, ReceivedMessage, BGMReference);
		entryHeader.CH_Status = currentStatus;
		Factory.Save();

		ProcessMessage(ediMessage);
		Factory.Save();
		AssertEquals($"{messageType}: Received status={entryStatus}", expectedStatus, entryHeader.CH_Status);

		entryHeader.Delete();
		Factory.Save();
	}
}
