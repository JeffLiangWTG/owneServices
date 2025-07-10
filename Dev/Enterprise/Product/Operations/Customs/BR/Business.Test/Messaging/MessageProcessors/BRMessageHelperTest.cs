using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRMessageHelperTest : TestCaseWithFactory
	{
		public void TestFormatIdLote()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Id Lote should be 123", "123", BRMessageHelper.FormatIdLote("123"));
				AssertEquals("Id Lote should be 234567", "234567", BRMessageHelper.FormatIdLote("1234567"));
				AssertEquals("Id Lote should be empty", string.Empty, BRMessageHelper.FormatIdLote(string.Empty));
				AssertEquals("Id Lote should be empty", string.Empty, BRMessageHelper.FormatIdLote(null));
			});
		}

		public void TestGetFormattedDate()
		{
			CombineAssertions(() =>
			{
				AssertEquals("2050-12-30", BRMessageHelper.GetFormattedDate("30/12/2050"));
				AssertEquals(string.Empty, BRMessageHelper.GetFormattedDate(""));
			});
		}

		public void TestGetOutgoingMessage()
		{
			var incomingInterchange1 = BRCResponseMessageProcessorTest.CreateInterchange(Factory, ZGuid.NewZGuid(), EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued, MessageTypeList.Codes.CAT);
			var incomingMessage1 = BRCResponseMessageProcessorTest.CreateMessage(Factory, incomingInterchange1.PK, null, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Success);
			var incomingInterchange2 = BRCResponseMessageProcessorTest.CreateInterchange(Factory, ZGuid.NewZGuid(), EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued, MessageTypeList.Codes.CAT);
			var incomingMessage2 = BRCResponseMessageProcessorTest.CreateMessage(Factory, incomingInterchange2.PK, null, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Success);
			Factory.Save();

			AssertNull(BRMessageHelper.GetOutgoingMessage(incomingInterchange1));
			AssertNull(BRMessageHelper.GetOutgoingMessage(incomingInterchange2));
			AssertNull(BRMessageHelper.GetOutgoingMessage(incomingMessage1));
			AssertNull(BRMessageHelper.GetOutgoingMessage(incomingMessage2));

			var outgoingInterchange1 = BRCResponseMessageProcessorTest.CreateInterchange(Factory, incomingInterchange1.EI_SessionGUID, EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Queued, MessageTypeList.Codes.CAT);
			var outgoingInterchange2 = BRCResponseMessageProcessorTest.CreateInterchange(Factory, incomingInterchange2.EI_SessionGUID, EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Queued, MessageTypeList.Codes.CAT);
			Factory.Save();

			AssertNull(BRMessageHelper.GetOutgoingMessage(incomingInterchange1));
			AssertNull(BRMessageHelper.GetOutgoingMessage(incomingInterchange2));
			AssertNull(BRMessageHelper.GetOutgoingMessage(incomingMessage1));
			AssertNull(BRMessageHelper.GetOutgoingMessage(incomingMessage2));

			var outgoingMessage11 = BRCResponseMessageProcessorTest.CreateMessage(Factory, outgoingInterchange1.PK, null, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Success);
			outgoingMessage11.EM_MessageNum = "1";
			outgoingMessage11.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
			var outgoingMessage12 = BRCResponseMessageProcessorTest.CreateMessage(Factory, outgoingInterchange1.PK, null, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Success);
			outgoingMessage12.EM_MessageNum = "2";
			outgoingMessage12.EM_SystemCreateTimeUtc = ZDateTime.Now;

			var outgoingMessage21 = BRCResponseMessageProcessorTest.CreateMessage(Factory, outgoingInterchange2.PK, null, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Success);
			outgoingMessage21.EM_MessageNum = "1";
			outgoingMessage21.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
			var outgoingMessage22 = BRCResponseMessageProcessorTest.CreateMessage(Factory, outgoingInterchange2.PK, null, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Success);
			outgoingMessage22.EM_MessageNum = "2";
			outgoingMessage22.EM_SystemCreateTimeUtc = ZDateTime.Now;
			Factory.Save();

			AssertEquals(outgoingMessage12, BRMessageHelper.GetOutgoingMessage(incomingInterchange1));
			AssertEquals(outgoingMessage22, BRMessageHelper.GetOutgoingMessage(incomingInterchange2));
			AssertEquals(outgoingMessage12, BRMessageHelper.GetOutgoingMessage(incomingMessage1));
			AssertEquals(outgoingMessage22, BRMessageHelper.GetOutgoingMessage(incomingMessage2));

			AssertEquals(outgoingMessage11, BRMessageHelper.GetOutgoingMessage(incomingInterchange1, 1));
			AssertEquals(outgoingMessage21, BRMessageHelper.GetOutgoingMessage(incomingInterchange2, 1));
			AssertEquals(outgoingMessage11, BRMessageHelper.GetOutgoingMessage(incomingMessage1, 1));
			AssertEquals(outgoingMessage21, BRMessageHelper.GetOutgoingMessage(incomingMessage2, 1));

			AssertEquals(outgoingMessage12, BRMessageHelper.GetOutgoingMessage(incomingInterchange1, 2));
			AssertEquals(outgoingMessage22, BRMessageHelper.GetOutgoingMessage(incomingInterchange2, 2));
			AssertEquals(outgoingMessage12, BRMessageHelper.GetOutgoingMessage(incomingMessage1, 2));
			AssertEquals(outgoingMessage22, BRMessageHelper.GetOutgoingMessage(incomingMessage2, 2));

			AssertContainsExactElementsInExactOrder(new[] { outgoingMessage12, outgoingMessage11 }, BRMessageHelper.GetOutgoingMessages(incomingMessage1));
			AssertContainsExactElementsInExactOrder(new[] { outgoingMessage22, outgoingMessage21 }, BRMessageHelper.GetOutgoingMessages(incomingMessage2));
		}

		public void TestGetOutgoingMessagesByApplicationReference()
		{
			var message1 = BRCResponseMessageProcessorTest.CreateMessage(Factory, ZGuid.Empty, null, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.CatalogZipFile);
			message1.EM_ApplicationReference = "12345678";
			var message2 = BRCResponseMessageProcessorTest.CreateMessage(Factory, ZGuid.Empty, null, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.ManufacturerZipFile);
			message2.EM_ApplicationReference = "12345678";
			var message3 = BRCResponseMessageProcessorTest.CreateMessage(Factory, ZGuid.Empty, null, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Sent, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.OperatorZipFile);
			message3.EM_ApplicationReference = "12345678";
			var message4 = BRCResponseMessageProcessorTest.CreateMessage(Factory, ZGuid.Empty, null, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.OperatorZipFile);
			message4.EM_ApplicationReference = "87654321";
			var message5 = BRCResponseMessageProcessorTest.CreateMessage(Factory, ZGuid.Empty, null, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Discarded, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.OperatorZipFile);
			message5.EM_ApplicationReference = "12345678";
			var message6 = BRCResponseMessageProcessorTest.CreateMessage(Factory, ZGuid.Empty, null, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.OperatorZipFile);
			message6.EM_ApplicationReference = "12345678";
			var message7 = BRCResponseMessageProcessorTest.CreateMessage(Factory, ZGuid.Empty, null, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CIH, EDIMessageSubTypeList.Codes.Original);
			message7.EM_ApplicationReference = "12345678";
			var message8 = BRCResponseMessageProcessorTest.CreateMessage(Factory, ZGuid.Empty, null, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.OperatorZipFile);
			message8.EM_ApplicationReference = "12345678";
			message8.EM_ApplicationCode = "CAC";

			AssertContainsExactElementsInExactOrder(new[] { message1, message2, message3 }, BRMessageHelper.GetOutgoingMessagesByApplicationReference(Factory, "12345678", MessageTypeList.Codes.CAT));
			AssertContainsExactElementsInExactOrder(new[] { message3 }, BRMessageHelper.GetOutgoingMessagesByApplicationReference(Factory, "12345678", MessageTypeList.Codes.CAT, new[] { EDIMessageSubTypeList.Codes.OperatorZipFile }));
		}

		public void TestGetOutgoingMessagesSentAtTheSameTime()
		{
			var date = ZDateTime.UtcNow;
			var goodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			var (requestManufacturerZipFile, responseManufacturerZipFile) = BRCResponseMessageProcessorTest.CreateResponseMessage(goodsCatalog, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.ManufacturerZipFile, requestMessageSubType: EDIMessageSubTypeList.Codes.ManufacturerZipFile);
			requestManufacturerZipFile.EM_SystemCreateTimeUtc = date;
			Factory.Save();
			AssertEquals(0, BRMessageHelper.GetOutgoingMessagesSentAtTheSameTime(responseManufacturerZipFile, MessageTypeList.Codes.CAT, new[] { EDIMessageSubTypeList.Codes.CatalogZipFile }).Length);

			var requestCatalogZipFileMessage = BRCResponseMessageProcessorTest.CreateMessage(Factory, ZGuid.Empty, goodsCatalog, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Sent, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.CatalogZipFile);
			requestCatalogZipFileMessage.EM_SystemCreateTimeUtc = date.AddSeconds(1);
			Factory.Save();
			AssertEquals(0, BRMessageHelper.GetOutgoingMessagesSentAtTheSameTime(responseManufacturerZipFile, MessageTypeList.Codes.CAT, new[] { EDIMessageSubTypeList.Codes.CatalogZipFile }).Length);

			requestCatalogZipFileMessage.EM_SystemCreateTimeUtc = date;
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { requestCatalogZipFileMessage }, BRMessageHelper.GetOutgoingMessagesSentAtTheSameTime(responseManufacturerZipFile, MessageTypeList.Codes.CAT, new[] { EDIMessageSubTypeList.Codes.CatalogZipFile }));

			var requestOperatorZipFileMessage = BRCResponseMessageProcessorTest.CreateMessage(Factory, ZGuid.Empty, goodsCatalog, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Sent, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.OperatorZipFile);
			requestOperatorZipFileMessage.EM_SystemCreateTimeUtc = date;
			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { requestCatalogZipFileMessage },
				BRMessageHelper.GetOutgoingMessagesSentAtTheSameTime(responseManufacturerZipFile, MessageTypeList.Codes.CAT, new[] { EDIMessageSubTypeList.Codes.CatalogZipFile }));
			AssertContainsExactElementsInAnyOrder(new[] { requestCatalogZipFileMessage, requestOperatorZipFileMessage },
				BRMessageHelper.GetOutgoingMessagesSentAtTheSameTime(responseManufacturerZipFile, MessageTypeList.Codes.CAT, new[] { EDIMessageSubTypeList.Codes.CatalogZipFile, EDIMessageSubTypeList.Codes.OperatorZipFile }));
			AssertContainsExactElementsInAnyOrder(new[] { requestCatalogZipFileMessage, requestOperatorZipFileMessage, requestManufacturerZipFile },
				BRMessageHelper.GetOutgoingMessagesSentAtTheSameTime(responseManufacturerZipFile, MessageTypeList.Codes.CAT, new[] { EDIMessageSubTypeList.Codes.CatalogZipFile, EDIMessageSubTypeList.Codes.OperatorZipFile, EDIMessageSubTypeList.Codes.ManufacturerZipFile }));
		}

		public void TestAllMessagesHaveResponseAndBeenProcessed()
		{
			var goodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			var outgoingMessage1 = BRCResponseMessageProcessorTest.CreateMessage(Factory, ZGuid.Empty, goodsCatalog, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.CatalogZipFile);
			var outgoingMessage2 = BRCResponseMessageProcessorTest.CreateMessage(Factory, ZGuid.Empty, goodsCatalog, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.OperatorZipFile);
			Factory.Save();

			AssertAllMessagesHaveResponseAndBeenProcessed(outgoingMessage1, new[] { outgoingMessage1 });
			AssertAllMessagesHaveResponseAndBeenProcessed(outgoingMessage2, new[] { outgoingMessage1, outgoingMessage2 });

			void AssertAllMessagesHaveResponseAndBeenProcessed(EDIMessage outgoingMessage, EDIMessage[] messagesSentAtTheSameTime)
			{
				Assert("Outgoing EDIMessage in queue", !BRMessageHelper.AllMessagesHaveResponseAndBeenProcessed(Factory, messagesSentAtTheSameTime));

				outgoingMessage.EM_Status = EDIMessageStatusList.Codes.Failed;
				Factory.Save();
				Assert("Outgoing EDIMessage failed", BRMessageHelper.AllMessagesHaveResponseAndBeenProcessed(Factory, messagesSentAtTheSameTime));

				outgoingMessage.EM_Status = EDIMessageStatusList.Codes.Sent;
				var outgoingInterchange = BRCResponseMessageProcessorTest.CreateInterchange(Factory, ZGuid.NewZGuid(), EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Queued, MessageTypeList.Codes.CAT);
				outgoingMessage.EM_EI = outgoingInterchange.PK;
				Factory.Save();
				Assert("Outgoing EDIInterchange in queue", !BRMessageHelper.AllMessagesHaveResponseAndBeenProcessed(Factory, messagesSentAtTheSameTime));

				outgoingInterchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;
				Factory.Save();
				Assert("Outgoing EDIInterchange failed", BRMessageHelper.AllMessagesHaveResponseAndBeenProcessed(Factory, messagesSentAtTheSameTime));

				outgoingInterchange.EI_Status = EDIInterchangeStatusList.Codes.Sent;
				Assert("No Incoming EDIInterchange", !BRMessageHelper.AllMessagesHaveResponseAndBeenProcessed(Factory, messagesSentAtTheSameTime));

				var incomingInterchange = BRCResponseMessageProcessorTest.CreateInterchange(Factory, outgoingInterchange.EI_SessionGUID, EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued, MessageTypeList.Codes.CAT);
				Factory.Save();
				Assert("Incoming EDIInterchange in queue", !BRMessageHelper.AllMessagesHaveResponseAndBeenProcessed(Factory, messagesSentAtTheSameTime));

				incomingInterchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;
				Factory.Save();
				Assert("Incoming EDIInterchange failed", BRMessageHelper.AllMessagesHaveResponseAndBeenProcessed(Factory, messagesSentAtTheSameTime));

				incomingInterchange.EI_Status = EDIInterchangeStatusList.Codes.Received;
				var incomingMessage1 = BRCResponseMessageProcessorTest.CreateMessage(Factory, ZGuid.Empty, goodsCatalog, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.CatalogZipFile);
				var incomingMessage2 = BRCResponseMessageProcessorTest.CreateMessage(Factory, ZGuid.Empty, goodsCatalog, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.CatalogZipFile);
				incomingInterchange.ContainedMessages.Add(incomingMessage1);
				incomingInterchange.ContainedMessages.Add(incomingMessage2);
				Factory.Save();
				Assert("2 Incoming EDIMessages in Queued", !BRMessageHelper.AllMessagesHaveResponseAndBeenProcessed(Factory, messagesSentAtTheSameTime));

				incomingMessage1.EM_Status = EDIMessageStatusList.Codes.Received;
				Factory.Save();
				Assert("1 EDIMessages in Queued", !BRMessageHelper.AllMessagesHaveResponseAndBeenProcessed(Factory, messagesSentAtTheSameTime));

				incomingMessage2.EM_Status = EDIMessageStatusList.Codes.PreProcessedOK;
				Factory.Save();
				Assert("1 EDIMessages Pre Processed", !BRMessageHelper.AllMessagesHaveResponseAndBeenProcessed(Factory, messagesSentAtTheSameTime));

				incomingMessage2.EM_Status = EDIMessageStatusList.Codes.Failed;
				Factory.Save();
				Assert("All EDIMessages Processed", BRMessageHelper.AllMessagesHaveResponseAndBeenProcessed(Factory, messagesSentAtTheSameTime));
			}
		}

		public void TestDeserializeObject()
		{
			var jsonNullRequiredValueForTesting = @"{
				""id"": null,
				""name"": null
			}";

			var jsonSuccess = @"{
				""id"": 1,
				""name"": ""TEST""
			}";

			var deserializedObject = BRMessageHelper.DeserializeObject<ObjectForTestingDeserialize>(jsonNullRequiredValueForTesting);
			AssertNotNull(deserializedObject);
			AssertEquals(0, deserializedObject.Id);
			AssertEquals(null, deserializedObject.Name);

			deserializedObject = BRMessageHelper.DeserializeObject<ObjectForTestingDeserialize>(string.Empty);
			AssertNull(deserializedObject);

			deserializedObject = BRMessageHelper.DeserializeObject<ObjectForTestingDeserialize>(jsonSuccess);
			AssertNotNull(deserializedObject);
			AssertEquals(1, deserializedObject.Id);
			AssertEquals("TEST", deserializedObject.Name);
		}

		public void TestDeserializeObjectCanConvertStringToNumber()
		{
			var json = @"{
				""id"": ""1"",
				""name"": ""TEST"",
			}";

			var model = BRMessageHelper.DeserializeObject<ObjectForTestingDeserialize>(json);
			AssertNotNull(model);
			AssertEquals(1, model.Id);
		}

		public void TestDeserializeObjectCanConvertNumberToString()
		{
			var json = @"{
				""number"": 1,
				""name"": ""TEST"",
			}";

			var model = BRMessageHelper.DeserializeObject<ObjectForTestingDeserialize>(json);
			AssertNotNull(model);
			AssertEquals("1", model.Number);
		}

		public void TestDeserializeObjectCanHandleDateTimeWithTimezoneOffset()
		{
			var json = @"{
				""date"": ""2021-04-21T00:00:00+00:00"",
				""name"": ""TEST"",
			}";

			var model = BRMessageHelper.DeserializeObject<ObjectForTestingDeserialize>(json);
			AssertNotNull(model);
			AssertEquals(new DateTime(2021, 4, 21, 0, 0, 0, DateTimeKind.Utc), model.Date.ToUniversalTime());
		}

		public void TestDeserializeObjectCanConvertNullToDefaultForInt()
		{
			var json = @"{
				""id"": null,
				""name"": ""TEST"",
			}";

			var model = BRMessageHelper.DeserializeObject<ObjectForTestingDeserialize>(json);
			AssertNotNull(model);
			AssertEquals(0, model.Id);
		}

		public void TestGetOutgoingInterchange()
		{
			var incomingInterchange1 = CreateInterchange(Factory, ZGuid.NewZGuid(), EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued, MessageTypeList.Codes.CAT);
			AssertNull(BRMessageHelper.GetOutgoingInterchange(incomingInterchange1));

			var outgoingInterchange1 = CreateInterchange(Factory, incomingInterchange1.EI_SessionGUID, EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Queued, MessageTypeList.Codes.CAT);
			var incomingInterchange2 = CreateInterchange(Factory, ZGuid.NewZGuid(), EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued, MessageTypeList.Codes.CAT);
			var outgoingInterchange2 = CreateInterchange(Factory, incomingInterchange2.EI_SessionGUID, EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Queued, MessageTypeList.Codes.CAT);

			AssertEquals(outgoingInterchange1.PK, BRMessageHelper.GetOutgoingInterchange(incomingInterchange1).PK);
			AssertEquals(outgoingInterchange2.PK, BRMessageHelper.GetOutgoingInterchange(incomingInterchange2).PK);

			EDIInterchange CreateInterchange(BusinessObjectFactory factory, ZGuid sessionGUID, ZString direction, ZString status, ZString type)
			{
				var interchange = factory.New<EDIInterchange>();
				interchange.EI_Status = status;
				interchange.EI_IsActive = true;
				interchange.EI_ReceiveTransmit = direction;
				interchange.EI_ApplicationCode = ApplicationCodeList.Codes.BRCustoms;
				interchange.EI_InterchangeType = type;
				interchange.EI_From = direction == EDIInterchange.Direction.Transmit ? GlbCompany.CurrentCompany.LicenceKeyIdentifier : new ZString("BRCustoms");
				interchange.EI_To = direction == EDIInterchange.Direction.Receive ? GlbCompany.CurrentCompany.LicenceKeyIdentifier : new ZString("BRCustoms");
				interchange.EI_GB = GlbBranch.CurrentBranch.PK;
				interchange.EI_SessionGUID = sessionGUID;
				return interchange;
			}
		}

		class ObjectForTestingDeserialize
		{
			public int Id { get; set; }
			public string Name { get; set; }
			public string Number { get; set; }
			public DateTime Date { get; set; }
		}
	}
}
