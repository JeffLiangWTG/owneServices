using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(AEEDIMessage))]
sealed class AEEDIMessageTest : Messaging.Testing.EDIMessageTest
{
	public void TestMessageDefaults()
	{
		AssertEquals("Default ApplicationCode", ApplicationCodeList.Codes.UAECustoms, CreateNewMessage().EM_ApplicationCode);
	}

	public void TestEDIMessageTypeDecider()
	{
		AssertEquals(typeof(EDIMessageTypeDecider), AEEDIMessage.TypeDecider.GetType());
	}

	[TestDate(2024, 07, 01, 01, 23, 45)]
	public void TestGetMessageReferenceNumberUsingEM_MessageNum()
	{
		CombineAssertions(() =>
		{
			var message1 = CreateNewMessageWithTestData();
			AssertNullOrEmpty(message1.EM_MessageNum);
			message1.AssignMessageNumber();
			AssertEquals("First message", "012345H0000001", message1.EM_MessageNum);

			var message2 = CreateNewMessageWithTestData();
			message2.AssignMessageNumber();
			AssertEquals("Second message", "012345H0000002", message2.EM_MessageNum);
		});
	}

	[TestDate(2024, 07, 01, 01, 23, 45)]
	public void TestMessageNumberPlaceHolderOverrideUsingEM_MessageText()
	{
		var message = CreateNewMessageWithTestData();
		message.EM_MessageText = $"Test message - {AEConstants.Messaging.Placeholders.MessageNumber}";
		message.AssignMessageNumber();
		AssertEquals("Test message - 012345H0000001", message.EM_MessageText);
	}

	public void TestSendersReferencePlaceHolderOverride()
	{
		var message = CreateNewMessageWithTestData();
		const string messageText = $"Test Message - {Messaging.Business.EDIMessage.SendersReferencePlaceHolder}";
		SaveMessageWithData(message, Factory.New<LinkedObjectForTest>(), messageText);

		CombineAssertions(() =>
		{
			AssertEquals("Senders Reference set from linked object", "Test Message - DocumentIdentifier", message.EM_MessageText);

			SaveMessageWithData(message, Factory.New<DummyBusinessObject>(), messageText);
			AssertEquals("Linked object does not provide Document Identifier", "Test Message -", message.EM_MessageText);
		});
	}

	void SaveMessageWithData(AEEDIMessage message, BusinessObject linkedObject, string messageText)
	{
		message.EM_LinkedObject = linkedObject;
		message.EM_MessageText = messageText;
		message.OnSaving();
	}

	AEEDIMessage CreateNewMessageWithTestData()
	{
		var message = CreateNewMessage();
		message.EM_MessageType = AEConstants.Messaging.MessageTypes.CUSCAR;
		return message;
	}

	AEEDIMessage CreateNewMessage() => Factory.New<AEEDIMessage>();
}
