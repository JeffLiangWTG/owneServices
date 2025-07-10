using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(ITEDIMessageCollection))]
sealed class ITEDIMessageCollectionTest : BusinessObjectCollectionTestCase
{
	public void TestGetLastSuccesfullySentIdoc()
	{
		var messageCollection = GetNewMessageCollection();
		AssertNull($"When there are no sent idoc messages, {nameof(messageCollection.GetLastSuccessfullySentIdoc)}", messageCollection.GetLastSuccessfullySentIdoc());

		var message1 = messageCollection.AddNew();
		message1.IsTransmitMessage = false;
		AssertNull($"When there are no sent idoc transmit messages, {nameof(messageCollection.GetLastSuccessfullySentIdoc)}", messageCollection.GetLastSuccessfullySentIdoc());

		message1.EM_MessageType = SADConstants.CustomsInterchangeType.IdocR;
		message1.IsTransmitMessage = true;
		message1.EM_Status = EDIMessageStatusList.Codes.Queued;
		AssertNull($"When there is one queued idoc transmit message, {nameof(messageCollection.GetLastSuccessfullySentIdoc)}", messageCollection.GetLastSuccessfullySentIdoc());

		message1.EM_Status = EDIMessageStatusList.Codes.Sent;
		message1.EM_SystemCreateTimeUtc = ZDateTime.Now;
		AssertNotNull($"When there is one sent idoc transmit message, {nameof(messageCollection.GetLastSuccessfullySentIdoc)}", messageCollection.GetLastSuccessfullySentIdoc());
		AssertSame($"{nameof(message1)} and {nameof(messageCollection.GetLastSuccessfullySentIdoc)}", message1, messageCollection.GetLastSuccessfullySentIdoc());

		var message2 = messageCollection.AddNew();
		message2.EM_MessageType = SADConstants.CustomsInterchangeType.IdocR;
		message2.IsTransmitMessage = true;
		message2.EM_Status = EDIMessageStatusList.Codes.Sent;
		message2.EM_SystemCreateTimeUtc = message1.EM_SystemCreateTimeUtc.AddDays(1);
		AssertNotNull($"When there are more than one sent idoc transmit messages, {nameof(messageCollection.GetLastSuccessfullySentIdoc)}", messageCollection.GetLastSuccessfullySentIdoc());
		AssertSame($"{nameof(message2)} and {nameof(messageCollection.GetLastSuccessfullySentIdoc)}", message2, messageCollection.GetLastSuccessfullySentIdoc());

		var message3 = messageCollection.AddNew();
		message3.EM_MessageType = SADConstants.CustomsInterchangeType.IdocR;
		message3.IsTransmitMessage = true;
		message3.EM_Status = EDIMessageStatusList.Codes.Manual;
		message3.EM_SystemCreateTimeUtc = message2.EM_SystemCreateTimeUtc.AddDays(1);
		AssertSame($"Last manual idoc transmit message (${nameof(message3)})", message3, messageCollection.GetLastSuccessfullySentIdoc());

		var message4 = messageCollection.AddNew();
		message4.EM_MessageType = MessageProcessorConstants.InterchangeTypes.SingleWindowRequest;
		message4.IsTransmitMessage = true;
		message4.EM_Status = EDIMessageStatusList.Codes.Sent;
		message4.EM_SystemCreateTimeUtc = message3.EM_SystemCreateTimeUtc.AddDays(1);
		AssertSame($"When the last sent message is not an IDOC message, {nameof(message3)} and {nameof(messageCollection.GetLastSuccessfullySentIdoc)}", message3, messageCollection.GetLastSuccessfullySentIdoc());
	}

	public void TestGetLastSuccesfullySentMessageBySubType()
	{
		var messageCollection = GetNewMessageCollection();
		AssertNull($"When there are no H1 sent messages, {nameof(messageCollection.GetLastSuccessfullySentMessageBySubType)}", messageCollection.GetLastSuccessfullySentMessageBySubType("H1"));

		var message1 = messageCollection.AddNew();
		message1.IsTransmitMessage = false;
		AssertNull($"When there are no sent H1 transmit messages, {nameof(messageCollection.GetLastSuccessfullySentMessageBySubType)}", messageCollection.GetLastSuccessfullySentMessageBySubType("H1"));

		message1.EM_MessageSubType = ImportUCC6DeclarationTypeList.Codes.ImmissioneLiberaPraticaH1;
		message1.IsTransmitMessage = true;
		message1.EM_Status = EDIMessageStatusList.Codes.Queued;
		AssertNull($"When there is one queued H1 transmit message, {nameof(messageCollection.GetLastSuccessfullySentMessageBySubType)}", messageCollection.GetLastSuccessfullySentMessageBySubType("H1"));

		message1.EM_Status = EDIMessageStatusList.Codes.Sent;
		message1.EM_SystemCreateTimeUtc = ZDateTime.Now;
		AssertNotNull($"When there is one sent H1 transmit message, {nameof(messageCollection.GetLastSuccessfullySentMessageBySubType)}", messageCollection.GetLastSuccessfullySentMessageBySubType("H1"));
		AssertSame($"{nameof(message1)} and {nameof(messageCollection.GetLastSuccessfullySentMessageBySubType)}", message1, messageCollection.GetLastSuccessfullySentMessageBySubType("H1"));
		AssertNull($"When there is one queued H2 transmit message, {nameof(messageCollection.GetLastSuccessfullySentMessageBySubType)}", messageCollection.GetLastSuccessfullySentMessageBySubType("H2"));

		var message2 = messageCollection.AddNew();
		message2.EM_MessageSubType = ImportUCC6DeclarationTypeList.Codes.ImmissioneLiberaPraticaH1;
		message2.IsTransmitMessage = true;
		message2.EM_Status = EDIMessageStatusList.Codes.Sent;
		message2.EM_SystemCreateTimeUtc = message1.EM_SystemCreateTimeUtc.AddDays(1);
		AssertNotNull($"When there are more than one sent transmit messages, {nameof(messageCollection.GetLastSuccessfullySentMessageBySubType)}", messageCollection.GetLastSuccessfullySentMessageBySubType("H1"));
		AssertSame($"{nameof(message2)} and {nameof(messageCollection.GetLastSuccessfullySentMessageBySubType)}", message2, messageCollection.GetLastSuccessfullySentMessageBySubType("H1"));

		var message3 = messageCollection.AddNew();
		message3.EM_MessageSubType = ImportUCC6DeclarationTypeList.Codes.ImmissioneLiberaPraticaH1;
		message3.IsTransmitMessage = true;
		message3.EM_Status = EDIMessageStatusList.Codes.Manual;
		message3.EM_SystemCreateTimeUtc = message2.EM_SystemCreateTimeUtc.AddDays(1);
		AssertSame($"Last manual H1 transmit message (${nameof(message3)})", message3, messageCollection.GetLastSuccessfullySentMessageBySubType("H1"));

		var message4 = messageCollection.AddNew();
		message4.EM_MessageSubType = ImportUCC6DeclarationTypeList.Codes.RegimeSpecialeDepositoDoganaleH2;
		message4.IsTransmitMessage = true;
		message4.EM_Status = EDIMessageStatusList.Codes.Sent;
		message4.EM_SystemCreateTimeUtc = message3.EM_SystemCreateTimeUtc.AddDays(1);
		AssertSame($"When the last sent message is not an H1 message, {nameof(message3)} and {nameof(messageCollection.GetLastSuccessfullySentMessageBySubType)}", message3, messageCollection.GetLastSuccessfullySentMessageBySubType("H1"));
	}

	public void TestGetLastMessageByType()
	{
		var messageCollection = GetNewMessageCollection();
		AssertNull($"When there are no messages, {nameof(messageCollection.GetLastMessageByType)}", messageCollection.GetLastMessageByType(SADConstants.CustomsInterchangeType.IdocR));

		var message1 = messageCollection.AddNew();
		message1.EM_MessageType = SADConstants.CustomsInterchangeType.IdocR;
		message1.EM_SystemCreateTimeUtc = ZDateTime.Now;
		AssertSame($"When there is 1 message of type '{SADConstants.CustomsInterchangeType.IdocR}', {nameof(messageCollection.GetLastMessageByType)} and {nameof(message1)}", message1, messageCollection.GetLastMessageByType(SADConstants.CustomsInterchangeType.IdocR));

		var message2 = messageCollection.AddNew();
		message2.EM_MessageType = SADConstants.CustomsInterchangeType.IdocR;
		message2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(1);
		AssertSame($"When there are more than 1 messages of type '{SADConstants.CustomsInterchangeType.IdocR}', {nameof(messageCollection.GetLastMessageByType)} and {nameof(message2)}", message2, messageCollection.GetLastMessageByType(SADConstants.CustomsInterchangeType.IdocR));
	}

	public void TestGetIdocMessage()
	{
		const string messageNumber = "000001";
		var messageCollection = GetNewMessageCollection();
		AssertNull($"When there are no messages, {nameof(messageCollection.GetIdocMessage)}", messageCollection.GetIdocMessage(messageNumber));

		var message1 = messageCollection.AddNew();
		message1.EM_MessageType = SADConstants.CustomsInterchangeType.IdocR;
		message1.EM_MessageNum = "000002";
		message1.EM_SystemCreateTimeUtc = ZDateTime.Now;
		AssertNull($"When there is 1 IDOC message but its EM_MessageNum is not {messageNumber}, {nameof(messageCollection.GetIdocMessage)}", messageCollection.GetIdocMessage(messageNumber));

		message1.EM_MessageNum = messageNumber;
		AssertSame($"When there is 1 IDOC message and its EM_MessageNum is {messageNumber}, {nameof(messageCollection.GetIdocMessage)} and {nameof(message1)}", message1, messageCollection.GetIdocMessage(messageNumber));

		var message2 = messageCollection.AddNew();
		message2.EM_MessageType = SADConstants.CustomsInterchangeType.IdocR;
		message2.EM_MessageNum = messageNumber;
		message2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(1);
		AssertSame($"When there are more than 1 IDOC messages and their EM_MessageNum is {messageNumber}, {nameof(messageCollection.GetIdocMessage)} and {nameof(message2)}", message2, messageCollection.GetIdocMessage(messageNumber));
	}

	public void TestGetLastSuccessfullySentMessageForDepositedStatus()
	{
		var utcNow = ZDateTime.UtcNow;

		var messageCollection = GetNewMessageCollection();
		AddNewMessage("I2", utcNow.AddDays(2));
		AssertNull("When there are no NEW-H? messages, Last NEW declaration successfully sent message", messageCollection.GetLastSuccessfullySentMessageForDepositedStatus());

		var message1 = AddNewMessage("H1", utcNow.AddDays(-1));
		AssertSame("When only one NEW-H? declaration message is found, Last NEW declaration successfully sent message", message1, messageCollection.GetLastSuccessfullySentMessageForDepositedStatus());

		var message2 = AddNewMessage("H1", utcNow.AddDays(1));
		AssertSame("When more than one NEW-H? declaration message is found, Last NEW declaration successfully sent message", message2, messageCollection.GetLastSuccessfullySentMessageForDepositedStatus());

		var message3 = AddNewMessage("B1", utcNow.AddDays(1));
		AssertSame("When more than one NEW-H?/B? declaration message is found, Last NEW declaration successfully sent message", message3, messageCollection.GetLastSuccessfullySentMessageForDepositedStatus());

		ITEDIMessage AddNewMessage(ZString subType, ZDateTime createTime)
		{
			var message = messageCollection.AddNew();
			message.EM_MessageSubType = subType;
			message.EM_MessageType = "NEW";
			message.EM_SystemCreateTimeUtc = createTime;
			message.IsTransmitMessage = true;
			message.EM_Status = EDIMessageStatusList.Codes.Sent;
			return message;
		}
	}

	public void TestGetFirstAcknowledgmentByUniqueTransactionID()
	{
		CombineAssertions(() =>
		{
			var messageCollection = GetNewMessageCollection();

			var message1 = messageCollection.AddNew();
			message1.EM_MessageType = "ACK";
			message1.EM_MessageText = "<IUT>20220307D11000328189</IUT>";
			message1.EM_SystemCreateTimeUtc = ZDateTime.Now;

			var message2 = messageCollection.AddNew();
			message2.EM_MessageType = "ACK";
			message2.EM_MessageText = "<IUT>20220307D11000328189</IUT>";
			message2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(1);

			AssertNull("When the passed IUT is empty, found message", messageCollection.GetFirstAcknowledgmentByUniqueTransactionID(""));
			AssertNull("When the collection contains an ACK message but IUT does not match, found message", messageCollection.GetFirstAcknowledgmentByUniqueTransactionID("XYZ"));
			AssertSame("When the collection contains an ACK message and IUT matches, found message", message1, messageCollection.GetFirstAcknowledgmentByUniqueTransactionID("20220307D11000328189"));
		});
	}

	public void TestGetFirstSentMessageBySessionGuid()
	{
		var messageCollection = GetNewMessageCollection();

		var message1 = messageCollection.AddNew();
		message1.IsTransmitMessage = true;
		message1.EM_Status = "SNT";

		var message2 = messageCollection.AddNew();
		message2.IsTransmitMessage = true;
		message2.EM_Status = "SNT";
		var interchangeForMessage2 = Factory.New<EDIInterchange>();
		interchangeForMessage2.ContainedMessages.Add(message2);
		interchangeForMessage2.EI_SessionGUID = new ZGuid("44CBF82C-E731-41C6-98B5-F99E0869CAD0");

		var message3 = messageCollection.AddNew();
		message3.IsTransmitMessage = false;
		var interchangeForMessage3 = Factory.New<EDIInterchange>();
		interchangeForMessage3.ContainedMessages.Add(message3);
		interchangeForMessage3.EI_SessionGUID = new ZGuid("EADC205E-BFD7-44AD-B547-14C4B3C02177");

		var message4 = messageCollection.AddNew();
		message4.IsTransmitMessage = true;
		message4.EM_Status = "QUE";
		var interchangeForMessage4 = Factory.New<EDIInterchange>();
		interchangeForMessage4.ContainedMessages.Add(message4);
		interchangeForMessage4.EI_SessionGUID = new ZGuid("6792CFAC-2E37-4B48-9548-EBCF448D6081");

		CombineAssertions(() =>
		{
			AssertNull("When the passed SessionGUID is empty, found message", messageCollection.GetFirstSentMessageBySessionGuid(ZGuid.Empty));
			AssertSame("When the passes SessionGUID matches a sent message, found message", message2, messageCollection.GetFirstSentMessageBySessionGuid(new ZGuid("44CBF82C-E731-41C6-98B5-F99E0869CAD0")));
			AssertNull("When the passes SessionGUID matches a received message, found message", messageCollection.GetFirstSentMessageBySessionGuid(new ZGuid("EADC205E-BFD7-44AD-B547-14C4B3C02177")));
			AssertNull("When the passes SessionGUID matches an outgoing message which is not sent, found message", messageCollection.GetFirstSentMessageBySessionGuid(new ZGuid("6792CFAC-2E37-4B48-9548-EBCF448D6081")));
		});
	}

	protected override BusinessObjectCollection GetCollectionToTest() => GetNewMessageCollection();

	ITEDIMessageCollection GetNewMessageCollection() => new ITEDIMessageCollection(Factory.New<DummyBusinessObject>());
}
