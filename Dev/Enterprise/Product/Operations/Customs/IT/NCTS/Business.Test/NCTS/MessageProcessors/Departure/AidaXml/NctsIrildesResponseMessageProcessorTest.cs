using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsIrildesResponseMessageProcessorTest : NctsResponseMessageProcessorAbstractTest<IrildesResponseMessageProcessor>
{
	public void TestProcessNegativeResponse()
	{
		var negativeResponseText = ManifestResourceHelper.ReadManifestResourceContent(IrildesNegativeResponse198ResourceKey);
		var (nctsHeader, sentMessage, receivedMessage) = PrepareTestData(messageText: negativeResponseText, responseMessageType: "IRR");

		var movementHeader = nctsHeader.MovementHeader;
		movementHeader.BM_CustomsStatus = "XYZ";
		movementHeader.BM_MessageStatus = "ZYX";

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertNumberOfResponseMessages(movementHeader, 1, responseMessageType: "IRR");
			AssertEquals(nameof(movementHeader.BM_CustomsStatus), "XYZ", movementHeader.BM_CustomsStatus);
			AssertEquals(nameof(movementHeader.BM_MessageStatus), "ZYX", movementHeader.BM_MessageStatus);
		});
	}

	public void TestProcessPositiveResponseMessageResultCode199()
	{
		var positiveResponseText = ManifestResourceHelper.ReadManifestResourceContent(IrildesPositiveResponse199ResourceKey);
		var (nctsHeader, sentMessage, receivedMessage) = PrepareTestData(messageText: positiveResponseText, responseMessageType: "IRR");

		var movementHeader = nctsHeader.MovementHeader;
		movementHeader.BM_CustomsStatus = "XYZ";
		movementHeader.BM_MessageStatus = "ZYX";

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertNumberOfResponseMessages(movementHeader, 1, responseMessageType: "IRR");
			AssertEquals(nameof(movementHeader.BM_CustomsStatus), "WRO", movementHeader.BM_CustomsStatus);
			AssertEquals(nameof(movementHeader.BM_MessageStatus), "ACC", movementHeader.BM_MessageStatus);
		});

		var irildesEntryNumber = GetCusEntryNumber(nctsHeader, "IRI");
		AssertNotNull("EntryHeader -> CusEntryNumber", irildesEntryNumber);
		AssertEntryNumber(irildesEntryNumber, "IRI", "", "CUS", "IT278100", new ZDate(2024, 03, 28), expectedEntryStatus: "WRO");
	}

	public void TestProcessPositiveResponseMessageResultCode200()
	{
		var positiveResponseText = ManifestResourceHelper.ReadManifestResourceContent(IrildesPositiveResponse200ResourceKey);
		var (nctsHeader, sentMessage, receivedMessage) = PrepareTestData(messageText: positiveResponseText, responseMessageType: "IRR");

		var movementHeader = nctsHeader.MovementHeader;
		movementHeader.BM_CustomsStatus = "XYZ";
		movementHeader.BM_MessageStatus = "ZYX";

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertNumberOfResponseMessages(movementHeader, 1, responseMessageType: "IRR");
			AssertEquals(nameof(movementHeader.BM_CustomsStatus), "WRO", movementHeader.BM_CustomsStatus);
			AssertEquals(nameof(movementHeader.BM_MessageStatus), "ACC", movementHeader.BM_MessageStatus);
		});

		var irildesEntryNumber = GetCusEntryNumber(nctsHeader, "IRI");
		AssertNotNull("EntryHeader -> CusEntryNumber", irildesEntryNumber);
		AssertEntryNumber(irildesEntryNumber, "IRI", "", "CUS", "IT278100", new ZDate(2024, 03, 28), expectedEntryStatus: "WRO");
	}

	public void TestProcessPositiveResponseMessageWithoutExitData()
	{
		var positiveResponseWithNoWriteOffDataText = ManifestResourceHelper.ReadManifestResourceContent(IrildesPositiveResponse200WithNoWriteOffDataResourceKey);
		var (nctsHeader, sentMessage, receivedMessage) = PrepareTestData(messageText: positiveResponseWithNoWriteOffDataText, responseMessageType: "IRR");

		var movementHeader = nctsHeader.MovementHeader;
		movementHeader.BM_CustomsStatus = "XYZ";
		movementHeader.BM_MessageStatus = "ZYX";

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertNumberOfResponseMessages(movementHeader, 1, responseMessageType: "IRR");
			AssertEquals(nameof(movementHeader.BM_CustomsStatus), "XYZ", movementHeader.BM_CustomsStatus);
			AssertEquals(nameof(movementHeader.BM_MessageStatus), "ZYX", movementHeader.BM_MessageStatus);
			AssertNull("IRILDES EntryNum", GetCusEntryNumber(nctsHeader, "IRI"));
		});
	}

	public void TestProcessPositiveResponseMessageResultCode200_WhileAmending()
	{
		var positiveResponseText = ManifestResourceHelper.ReadManifestResourceContent(IrildesPositiveResponse200ResourceKey);
		var (nctsHeader, sentMessage, receivedMessage) = PrepareTestData(messageText: positiveResponseText, responseMessageType: "IRR");

		var movementHeader = nctsHeader.MovementHeader;

		movementHeader.BM_Phase = "014";
		movementHeader.BM_CustomsStatus = "";
		movementHeader.BM_MessageStatus = "SNT";

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertNumberOfResponseMessages(movementHeader, 1, responseMessageType: "IRR");
			AssertEquals(nameof(movementHeader.BM_CustomsStatus), "", movementHeader.BM_CustomsStatus);
			AssertEquals(nameof(movementHeader.BM_MessageStatus), "SNT", movementHeader.BM_MessageStatus);
		});

		var irildesEntryNumber = GetCusEntryNumber(nctsHeader, "IRI");
		AssertNotNull("EntryHeader -> CusEntryNumber", irildesEntryNumber);
		AssertEntryNumber(irildesEntryNumber, "IRI", "", "CUS", "IT278100", new ZDate(2024, 03, 28), expectedEntryStatus: "WRO");
	}

	protected override IrildesResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger)
		=> new IrildesResponseMessageProcessor(logger);

	const string IrildesNegativeResponse198ResourceKey = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.Ncts_IrildesNegativeResponseError198.xml";
	const string IrildesPositiveResponse199ResourceKey = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.Ncts_IrildesPositiveResponseResultCode199.xml";
	const string IrildesPositiveResponse200ResourceKey = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.Ncts_IrildesPositiveResponseResultCode200.xml";
	const string IrildesPositiveResponse200WithNoWriteOffDataResourceKey = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.Ncts_IrildesPositiveResponseResultNoWriteOffData.xml";
}
