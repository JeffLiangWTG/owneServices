using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsAmendmentResponseMessageProcessorTest : NctsResponseMessageProcessorAbstractTest<ResponseMessageProcessor>
{
	public void TestProcessAmendmentConfirmation()
	{
		var positiveAckText = ManifestResourceHelper.ReadManifestResourceContent(PositiveAckManifestResourceKey);
		var positiveResponseText = ManifestResourceHelper.ReadManifestResourceContent(PositiveResponseManifestResourceKey);
		var clearanceResponseText = ManifestResourceHelper.ReadManifestResourceContent(ClearanceResponseManifestResourceKey);
		var amendmentConfirmationText = ManifestResourceHelper.ReadManifestResourceContent(AmendmentConfirmationResponseManifestResourceKey);

		var goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
		goodsItem.BY_Status = "DLR";

		(_, var newSentMessageSessionGuid) = AddNewSentMessage(movementHeader, "NEW");

		var ackMessage = GetReceivedMessage(positiveAckText, "ACK", newSentMessageSessionGuid);
		movementHeader.Messages.Add(ackMessage);

		var clearanceMessage = GetReceivedMessage(clearanceResponseText, "RES", newSentMessageSessionGuid);
		movementHeader.Messages.Add(clearanceMessage);

		var mrnMessage = GetReceivedMessage(positiveResponseText, "RES", newSentMessageSessionGuid);
		movementHeader.Messages.Add(mrnMessage);

		movementHeader.BM_Phase = "013";
		(_, var amdSentMessageSessionGuid) = AddNewSentMessage(movementHeader, "AMD");
		var amdConfirmationMessage = GetReceivedMessage(amendmentConfirmationText, "RES", amdSentMessageSessionGuid);

		var processor = GetMessageProcessor();
		processor.ProcessMessage(amdConfirmationMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals(nameof(movementHeader.BM_CustomsStatus), "REL", movementHeader.BM_CustomsStatus);
			AssertEquals(nameof(movementHeader.BM_MessageStatus), "ACC", movementHeader.BM_MessageStatus);
			AssertEquals(nameof(movementHeader.BM_Phase), "015", movementHeader.BM_Phase);
			AssertEquals("GoodsItem BY_Status", "DEL", goodsItem.BY_Status);
		});
	}

	public void TestProcessAmendmentConfirmationForIrildesNegative198Response()
	{
		var positiveAckText = ManifestResourceHelper.ReadManifestResourceContent(PositiveAckManifestResourceKey);
		var positiveResponseText = ManifestResourceHelper.ReadManifestResourceContent(PositiveResponseManifestResourceKey);
		var irildesNegative198ResponseText = ManifestResourceHelper.ReadManifestResourceContent(IrildesNegativeResponse198ResourceKey);
		var clearanceResponseText = ManifestResourceHelper.ReadManifestResourceContent(ClearanceResponseManifestResourceKey);
		var amendmentConfirmationText = ManifestResourceHelper.ReadManifestResourceContent(AmendmentConfirmationResponseManifestResourceKey);

		var goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
		goodsItem.BY_Status = "DLR";

		(_, var newSentMessageSessionGuid) = AddNewSentMessage(movementHeader, "NEW");

		var ackMessage = GetReceivedMessage(positiveAckText, "ACK", newSentMessageSessionGuid);
		movementHeader.Messages.Add(ackMessage);

		var clearanceMessage = GetReceivedMessage(clearanceResponseText, "RES", newSentMessageSessionGuid);
		movementHeader.Messages.Add(clearanceMessage);

		var irildesMessage = GetReceivedMessage(irildesNegative198ResponseText, "IRR", newSentMessageSessionGuid);
		movementHeader.Messages.Add(irildesMessage);

		var mrnMessage = GetReceivedMessage(positiveResponseText, "RES", newSentMessageSessionGuid);
		movementHeader.Messages.Add(mrnMessage);

		movementHeader.BM_Phase = "013";
		(_, var amdSentMessageSessionGuid) = AddNewSentMessage(movementHeader, "AMD");
		var amdConfirmationMessage = GetReceivedMessage(amendmentConfirmationText, "RES", amdSentMessageSessionGuid);

		var processor = GetMessageProcessor();
		processor.ProcessMessage(amdConfirmationMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals(nameof(movementHeader.BM_CustomsStatus), "REL", movementHeader.BM_CustomsStatus);
			AssertEquals(nameof(movementHeader.BM_MessageStatus), "ACC", movementHeader.BM_MessageStatus);
			AssertEquals(nameof(movementHeader.BM_Phase), "015", movementHeader.BM_Phase);
			AssertEquals("GoodsItem BY_Status", "DEL", goodsItem.BY_Status);
		});
	}

	public void TestProcessAmendmentConfirmationForIrildesPositive199Response()
	{
		var positiveAckText = ManifestResourceHelper.ReadManifestResourceContent(PositiveAckManifestResourceKey);
		var positiveResponseText = ManifestResourceHelper.ReadManifestResourceContent(PositiveResponseManifestResourceKey);
		var irildesPositive199ResponseText = ManifestResourceHelper.ReadManifestResourceContent(IrildesPositiveResponse199ResourceKey);
		var clearanceResponseText = ManifestResourceHelper.ReadManifestResourceContent(ClearanceResponseManifestResourceKey);
		var amendmentConfirmationText = ManifestResourceHelper.ReadManifestResourceContent(AmendmentConfirmationResponseManifestResourceKey);

		var goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
		goodsItem.BY_Status = "DLR";

		(_, var newSentMessageSessionGuid) = AddNewSentMessage(movementHeader, "NEW");

		var ackMessage = GetReceivedMessage(positiveAckText, "ACK", newSentMessageSessionGuid);
		movementHeader.Messages.Add(ackMessage);

		var clearanceMessage = GetReceivedMessage(clearanceResponseText, "RES", newSentMessageSessionGuid);
		movementHeader.Messages.Add(clearanceMessage);

		var irildesMessage = GetReceivedMessage(irildesPositive199ResponseText, "IRR", newSentMessageSessionGuid);
		movementHeader.Messages.Add(irildesMessage);

		var mrnMessage = GetReceivedMessage(positiveResponseText, "RES", newSentMessageSessionGuid);
		movementHeader.Messages.Add(mrnMessage);

		movementHeader.BM_Phase = "013";
		(_, var amdSentMessageSessionGuid) = AddNewSentMessage(movementHeader, "AMD");
		var amdConfirmationMessage = GetReceivedMessage(amendmentConfirmationText, "RES", amdSentMessageSessionGuid);

		var processor = GetMessageProcessor();
		processor.ProcessMessage(amdConfirmationMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals(nameof(movementHeader.BM_CustomsStatus), "WRO", movementHeader.BM_CustomsStatus);
			AssertEquals(nameof(movementHeader.BM_MessageStatus), "ACC", movementHeader.BM_MessageStatus);
			AssertEquals(nameof(movementHeader.BM_Phase), "015", movementHeader.BM_Phase);
			AssertEquals("GoodsItem BY_Status", "DEL", goodsItem.BY_Status);
		});
	}

	public void TestProcessAmendmentConfirmationForIrildesPositive200Response()
	{
		var positiveAckText = ManifestResourceHelper.ReadManifestResourceContent(PositiveAckManifestResourceKey);
		var positiveResponseText = ManifestResourceHelper.ReadManifestResourceContent(PositiveResponseManifestResourceKey);
		var irildesPositive200ResponseText = ManifestResourceHelper.ReadManifestResourceContent(IrildesPositiveResponse200ResourceKey);
		var clearanceResponseText = ManifestResourceHelper.ReadManifestResourceContent(ClearanceResponseManifestResourceKey);
		var amendmentConfirmationText = ManifestResourceHelper.ReadManifestResourceContent(AmendmentConfirmationResponseManifestResourceKey);

		var goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
		goodsItem.BY_Status = "DLR";

		(_, var newSentMessageSessionGuid) = AddNewSentMessage(movementHeader, "NEW");

		var ackMessage = GetReceivedMessage(positiveAckText, "ACK", newSentMessageSessionGuid);
		movementHeader.Messages.Add(ackMessage);

		var clearanceMessage = GetReceivedMessage(clearanceResponseText, "RES", newSentMessageSessionGuid);
		movementHeader.Messages.Add(clearanceMessage);

		var irildesMessage = GetReceivedMessage(irildesPositive200ResponseText, "IRR", newSentMessageSessionGuid);
		movementHeader.Messages.Add(irildesMessage);

		var mrnMessage = GetReceivedMessage(positiveResponseText, "RES", newSentMessageSessionGuid);
		movementHeader.Messages.Add(mrnMessage);

		movementHeader.BM_Phase = "013";
		(_, var amdSentMessageSessionGuid) = AddNewSentMessage(movementHeader, "AMD");
		var amdConfirmationMessage = GetReceivedMessage(amendmentConfirmationText, "RES", amdSentMessageSessionGuid);

		var processor = GetMessageProcessor();
		processor.ProcessMessage(amdConfirmationMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals(nameof(movementHeader.BM_CustomsStatus), "WRO", movementHeader.BM_CustomsStatus);
			AssertEquals(nameof(movementHeader.BM_MessageStatus), "ACC", movementHeader.BM_MessageStatus);
			AssertEquals(nameof(movementHeader.BM_Phase), "015", movementHeader.BM_Phase);
			AssertEquals("GoodsItem BY_Status", "DEL", goodsItem.BY_Status);
		});
	}

	public void TestProcessAmendmentConfirmationForIrildesPositive200ResponseWithNoWriteOffData()
	{
		var positiveAckText = ManifestResourceHelper.ReadManifestResourceContent(PositiveAckManifestResourceKey);
		var positiveResponseText = ManifestResourceHelper.ReadManifestResourceContent(PositiveResponseManifestResourceKey);
		var irildesPositive200WithNoWriteOffDataResponseText = ManifestResourceHelper.ReadManifestResourceContent(IrildesPositiveResponse200WithNoWriteOffDataResourceKey);
		var clearanceResponseText = ManifestResourceHelper.ReadManifestResourceContent(ClearanceResponseManifestResourceKey);
		var amendmentConfirmationText = ManifestResourceHelper.ReadManifestResourceContent(AmendmentConfirmationResponseManifestResourceKey);

		var goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
		goodsItem.BY_Status = "DLR";

		(_, var newSentMessageSessionGuid) = AddNewSentMessage(movementHeader, "NEW");

		var ackMessage = GetReceivedMessage(positiveAckText, "ACK", newSentMessageSessionGuid);
		movementHeader.Messages.Add(ackMessage);

		var clearanceMessage = GetReceivedMessage(clearanceResponseText, "RES", newSentMessageSessionGuid);
		movementHeader.Messages.Add(clearanceMessage);

		var irildesMessage = GetReceivedMessage(irildesPositive200WithNoWriteOffDataResponseText, "IRR", newSentMessageSessionGuid);
		movementHeader.Messages.Add(irildesMessage);

		var mrnMessage = GetReceivedMessage(positiveResponseText, "RES", newSentMessageSessionGuid);
		movementHeader.Messages.Add(mrnMessage);

		movementHeader.BM_Phase = "013";
		(_, var amdSentMessageSessionGuid) = AddNewSentMessage(movementHeader, "AMD");
		var amdConfirmationMessage = GetReceivedMessage(amendmentConfirmationText, "RES", amdSentMessageSessionGuid);

		var processor = GetMessageProcessor();
		processor.ProcessMessage(amdConfirmationMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals(nameof(movementHeader.BM_CustomsStatus), "REL", movementHeader.BM_CustomsStatus);
			AssertEquals(nameof(movementHeader.BM_MessageStatus), "ACC", movementHeader.BM_MessageStatus);
			AssertEquals(nameof(movementHeader.BM_Phase), "015", movementHeader.BM_Phase);
			AssertEquals("GoodsItem BY_Status", "DEL", goodsItem.BY_Status);
		});
	}

	public void TestProcessAmendmentConfirmationForMultipleIrildesResponses()
	{
		var positiveAckText = ManifestResourceHelper.ReadManifestResourceContent(PositiveAckManifestResourceKey);
		var positiveResponseText = ManifestResourceHelper.ReadManifestResourceContent(PositiveResponseManifestResourceKey);
		var irildesNegative198ResponseText = ManifestResourceHelper.ReadManifestResourceContent(IrildesNegativeResponse198ResourceKey);
		var irildesPositive200ResponseText = ManifestResourceHelper.ReadManifestResourceContent(IrildesPositiveResponse200ResourceKey);
		var irildesPositive200WithNoWriteOffDataResponseText = ManifestResourceHelper.ReadManifestResourceContent(IrildesPositiveResponse200WithNoWriteOffDataResourceKey);
		var clearanceResponseText = ManifestResourceHelper.ReadManifestResourceContent(ClearanceResponseManifestResourceKey);
		var amendmentConfirmationText = ManifestResourceHelper.ReadManifestResourceContent(AmendmentConfirmationResponseManifestResourceKey);

		var goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
		goodsItem.BY_Status = "DLR";

		(_, var newSentMessageSessionGuid) = AddNewSentMessage(movementHeader, "NEW");

		var ackMessage = GetReceivedMessage(positiveAckText, "ACK", newSentMessageSessionGuid);
		movementHeader.Messages.Add(ackMessage);

		var clearanceMessage = GetReceivedMessage(clearanceResponseText, "RES", newSentMessageSessionGuid);
		movementHeader.Messages.Add(clearanceMessage);

		movementHeader.Messages.Add(GetReceivedMessage(irildesNegative198ResponseText, "IRR", newSentMessageSessionGuid));
		movementHeader.Messages.Add(GetReceivedMessage(irildesPositive200ResponseText, "IRR", newSentMessageSessionGuid));
		movementHeader.Messages.Add(GetReceivedMessage(irildesPositive200WithNoWriteOffDataResponseText, "IRR", newSentMessageSessionGuid));

		var mrnMessage = GetReceivedMessage(positiveResponseText, "RES", newSentMessageSessionGuid);
		movementHeader.Messages.Add(mrnMessage);

		movementHeader.BM_Phase = "013";
		(_, var amdSentMessageSessionGuid) = AddNewSentMessage(movementHeader, "AMD");
		var amdConfirmationMessage = GetReceivedMessage(amendmentConfirmationText, "RES", amdSentMessageSessionGuid);

		var processor = GetMessageProcessor();
		processor.ProcessMessage(amdConfirmationMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals(nameof(movementHeader.BM_CustomsStatus), "WRO", movementHeader.BM_CustomsStatus);
			AssertEquals(nameof(movementHeader.BM_MessageStatus), "ACC", movementHeader.BM_MessageStatus);
			AssertEquals(nameof(movementHeader.BM_Phase), "015", movementHeader.BM_Phase);
			AssertEquals("GoodsItem BY_Status", "DEL", goodsItem.BY_Status);
		});
	}

	protected override ITEDIMessage GetReceivedMessage(string messageText, string messageType, ZGuid sentSessionGuid)
	{
		var receivedMessage = base.GetReceivedMessage(messageText, messageType, sentSessionGuid);
		receivedMessage.EM_ApplicationCode = "ITH";
		return receivedMessage;
	}

	protected override void SetUp()
	{
		base.SetUp();

		header = Factory.NewDepartureNctsHeader();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		movementHeader = header.MovementHeader;
	}

	protected override ResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger)
		=> new ResponseMessageProcessor(logger);

	NctsHeader header;
	NctsDepartureMovementHeader movementHeader;

	const string AmendmentConfirmationResponseManifestResourceKey = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.NctsAmendmentResponseConfirmation.xml";
	const string IrildesNegativeResponse198ResourceKey = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.Ncts_IrildesNegativeResponseError198.xml";
	const string IrildesPositiveResponse199ResourceKey = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.Ncts_IrildesPositiveResponseResultCode199.xml";
	const string IrildesPositiveResponse200ResourceKey = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.Ncts_IrildesPositiveResponseResultCode200.xml";
	const string IrildesPositiveResponse200WithNoWriteOffDataResourceKey = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.Ncts_IrildesPositiveResponseResultNoWriteOffData.xml";
}
