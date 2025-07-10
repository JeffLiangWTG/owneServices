using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

sealed class TemporaryStorageHeaderResponseMessageProcessorTest : TemporaryStorageHeaderResponseMessageProcessorAbstractTest<ResponseMessageProcessor>
{
	public void TestProcessPositiveResponse_UpdateCustomsStatusAsTemporaryStorageActivated()
	{
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent(PositiveResponseManifestResourceKey_AllOutcomesPositive);

		var (header, bill, sentMessage, receivedMessage) = PrepareTestData(messageText: positiveResponse, "RES");

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		var item1 = bill.PackedItems[0];
		var item2 = bill.PackedItems[1];

		var mrnEntryNumber = GetCusEntryNumber(bill, "MRN");
		var item1RegistrationEntryNumber = GetCusEntryNumber(item1, "REG");
		var item2RegistrationEntryNumber = GetCusEntryNumber(item2, "REG");

		CombineAssertions(() =>
		{
			AssertNumberOfResponseMessages(header, 1);
			AssertEquals(nameof(header.CustomsStatus), "TSA", header.CustomsStatus);

			AssertNotNull("AsycudaBill -> MRN", mrnEntryNumber);
			AssertNotNull("AsycudaPackedItem 1 -> REG", item1RegistrationEntryNumber);
			AssertNotNull("AsycudaPackedItem 2 -> REG", item2RegistrationEntryNumber);
		});
		AssertEntryNumber(mrnEntryNumber, "MRN", "24ITQYH300268983U7", "CUS", new ZDate(2024, 02, 07));
		AssertEntryNumber(item1RegistrationEntryNumber, "REG", "A3-29267W", "CUS", new ZDate(2024, 02, 07));
		AssertEntryNumber(item2RegistrationEntryNumber, "REG", "A3-29268X", "CUS", new ZDate(2024, 02, 07));
		AssertEquals("CustomsStatusDate", new ZDateTime(2024, 02, 07, 15, 35, 17), header.CustomsStatusDate);
	}

	public void TestProcessPositiveResponse_UpdateCustomsStatusAsEmpty()
	{
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent(PositiveResponseManifestResourceKey_AllOutcomesNegative);

		var (header, bill, sentMessage, receivedMessage) = PrepareTestData(messageText: positiveResponse, "RES");

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertNumberOfResponseMessages(header, 1);
			AssertEquals(nameof(header.CustomsStatus), ZString.Empty, header.CustomsStatus);
			AssertEquals("CustomsStatusDate", ZDateTime.Empty, header.CustomsStatusDate);
		});
	}

	public void TestProcessPositiveResponse_UpdateCustomsStatusAsTemporaryStoragePartiallyActivated()
	{
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent(PositiveResponseManifestResourceKey_SomeOutcomesPositive);

		var (header, bill, sentMessage, receivedMessage) = PrepareTestData(messageText: positiveResponse, "RES");
		header.AMA_MessageStatus = "SNT";
		header.Bills.AddNew();
		var item = bill.PackedItems.AddNew();
		item.API_LineNo = 3;
		var lrnEntryNumber = GetNewCusEntryNumber(bill, CusEntryNumberTypes.Standard.LocalReferenceNumber);
		lrnEntryNumber.CE_EntryNum = "2024CRI0000000000112";

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		var item1 = bill.PackedItems[0];
		var item2 = bill.PackedItems[1];

		var mrnEntryNumber = GetCusEntryNumber(bill, "MRN");
		var item1RegistrationEntryNumber = GetCusEntryNumber(item1, "REG");
		var item2RegistrationEntryNumber = GetCusEntryNumber(item2, "REG");

		CombineAssertions(() =>
		{
			AssertNumberOfResponseMessages(header, 1);
			AssertEquals(nameof(header.CustomsStatus), "TPA", bill.Header.CustomsStatus);
			AssertEquals(nameof(header.AMA_MessageStatus), ZString.Empty, header.AMA_MessageStatus);

			AssertNotNull("AsycudaBill -> MRN", mrnEntryNumber);
			AssertNotNull("AsycudaPackedItem 1 -> REG", item1RegistrationEntryNumber);
			AssertNotNull("AsycudaPackedItem 2 -> REG", item2RegistrationEntryNumber);
		});
		AssertEntryNumber(mrnEntryNumber, "MRN", "24ITQYH300268983U7", "CUS", new ZDate(2024, 02, 07));
		AssertEntryNumber(item1RegistrationEntryNumber, "REG", "A3-29267W", "CUS", new ZDate(2024, 02, 07));
		AssertEntryNumber(item2RegistrationEntryNumber, "REG", "A3-29268X", "CUS", new ZDate(2024, 02, 07));
		AssertEquals("CustomsStatusDate", new ZDateTime(2024, 02, 07, 15, 35, 17), header.CustomsStatusDate);
	}

	public void TestProcessNegativeResponse()
	{
		var negativeResponseText = ManifestResourceHelper.ReadManifestResourceContent(NegativeResponseManifestResourceKey);

		var (header, bill, sentMessage, receivedMessage) = PrepareTestData(messageText: negativeResponseText, "RES");

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertNumberOfResponseMessages(header, 1);
			AssertEquals(nameof(header.AMA_MessageStatus), "REJ", header.AMA_MessageStatus);
		});
	}

	public void TestAcknowledgementIsPositive()
	{
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent(PositiveAcknowledgementResourceKey);
		(var entryHeader, _, _, var receivedMessage) = PrepareTestData(messageText: positiveResponse, responseMessageType: "ACK");
		entryHeader.AMA_MessageStatus = "SNT";

		var processor = new AcknowledgementResponseMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		CombineAssertions(() =>
		{
			AssertNumberOfResponseMessages(entryHeader, 1, "ACK");
			AssertEquals(nameof(entryHeader.AMA_MessageStatus), "ACK", entryHeader.AMA_MessageStatus);
		});
	}

	public void TestAcknowledgementIsNegative()
	{
		var negativeResponce = ManifestResourceHelper.ReadManifestResourceContent(NegativeAcknowledgementResourceKey);
		(var entryHeader, _, _, var receivedMessage) = PrepareTestData(messageText: negativeResponce, responseMessageType: "ACK");
		entryHeader.AMA_MessageStatus = "SNT";

		var processor = new AcknowledgementResponseMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		CombineAssertions(() =>
		{
			AssertNumberOfResponseMessages(entryHeader, 1, "ACK");
			AssertEquals(nameof(entryHeader.AMA_MessageStatus), "REJ", entryHeader.AMA_MessageStatus);
		});
	}

	public void TestProcessPositiveResponse_UpdatesCustomsStatusDate_WhenNewDateIsLater()
	{
		var firstResponseText = ManifestResourceHelper.ReadManifestResourceContent(PositiveResponseManifestResourceKey_AllOutcomesPositive);

		var (header, bill, sentMessage, receivedMessage) = PrepareTestData(messageText: firstResponseText, "RES");

		header.CustomsStatusDate = new ZDateTime(2024, 02, 06, 15, 35, 17);

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertEquals("CustomsStatusDate should be updated to latest value", new ZDateTime(2024, 02, 07, 15, 35, 17), header.CustomsStatusDate);
	}

	public void TestProcessPositiveResponse_DoesNotUpdateCustomsStatusDate_WhenNewDateIsEarlier()
	{
		var responseWithEarlierDate = ManifestResourceHelper.ReadManifestResourceContent(PositiveResponseManifestResourceKey_AllOutcomesPositive);

		var (header, bill, sentMessage, receivedMessage) = PrepareTestData(messageText: responseWithEarlierDate, "RES");

		header.CustomsStatusDate = new ZDateTime(2024, 02, 08, 10, 00, 00);

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertEquals("CustomsStatusDate should remain unchanged if newer than the response", new ZDateTime(2024, 02, 08, 10, 00, 00), header.CustomsStatusDate);
	}

	public void TestProcessPartialPositiveResponse_UpdatesCustomsStatusDate_WhenNewDateIsLater()
	{
		var partialPositiveResponse = ManifestResourceHelper.ReadManifestResourceContent(PositiveResponseManifestResourceKey_SomeOutcomesPositive);

		var (header, bill, sentMessage, receivedMessage) = PrepareTestData(messageText: partialPositiveResponse, "RES");

		header.CustomsStatusDate = new ZDateTime(2024, 02, 06, 15, 35, 17);

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertEquals("CustomsStatusDate should be updated to latest value", new ZDateTime(2024, 02, 07, 15, 35, 17), header.CustomsStatusDate);
	}

	public void TestProcessPartialPositiveResponse_DoesNotUpdateCustomsStatusDate_WhenNewDateIsEarlier()
	{
		var responseWithEarlierDate = ManifestResourceHelper.ReadManifestResourceContent(PositiveResponseManifestResourceKey_SomeOutcomesPositive);

		var (header, bill, sentMessage, receivedMessage) = PrepareTestData(messageText: responseWithEarlierDate, "RES");

		header.CustomsStatusDate = new ZDateTime(2024, 02, 08, 10, 00, 00);

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertEquals("CustomsStatusDate should remain unchanged if newer than the response", new ZDateTime(2024, 02, 08, 10, 00, 00), header.CustomsStatusDate);
	}

	protected override ResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger)
		=> new ResponseMessageProcessor(logger);
}
