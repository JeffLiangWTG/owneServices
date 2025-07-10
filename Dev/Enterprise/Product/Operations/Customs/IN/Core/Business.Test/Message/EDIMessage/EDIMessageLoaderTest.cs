using System;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(EDIMessage.Loader))]
sealed class EDIMessageLoaderTest : LoaderTestCase
{
	public void TestMessageHasBeenSentOn()
	{
		var messageType = EDIMessageTypeList.Codes.ShippingBill;
		var messageSubTypes = new[] { EDIMessageTypeList.Codes.ShippingBill + DeclarationMessageTypeList.Codes.Fresh };

		var message = Factory.NewWithValidTestData<EDIMessage>();
		message.EM_MessageType = messageType;
		message.EM_MessageSubType = messageSubTypes[0];
		message.EM_MessageOwner = "SampleMessageOwner";
		message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		Factory.Save();
		var messageLoader = (EDIMessage.Loader)GetNewLoaderToTest();

		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Throw Exception for null BO", () => messageLoader.MessageHasBeenSentOn(null, messageType, messageSubTypes));

			Assert("LinkedObject not set", !messageLoader.MessageHasBeenSentOn(entryHeader, messageType, messageSubTypes));

			message.EM_LinkedObject = entryHeader;
			Factory.Save();
			Assert("LinkedObject set", messageLoader.MessageHasBeenSentOn(entryHeader, messageType, messageSubTypes));
			Assert("Message type empty", !messageLoader.MessageHasBeenSentOn(entryHeader, "", messageSubTypes));
			Assert("Message sub-type empty", messageLoader.MessageHasBeenSentOn(entryHeader, messageType, Array.Empty<string>()));

			message.EM_MessageType = "XXX";
			Factory.Save();
			Assert("Message type not match", !messageLoader.MessageHasBeenSentOn(entryHeader, messageType, messageSubTypes));
			Assert("Ignore Message type", messageLoader.MessageHasBeenSentOn(entryHeader, null, messageSubTypes));

			message.EM_MessageType = messageType;
			message.EM_MessageSubType = "XXX";
			Factory.Save();
			Assert("Message sub-type not match", !messageLoader.MessageHasBeenSentOn(entryHeader, messageType, messageSubTypes));
			Assert("Ingore Message sub-type", messageLoader.MessageHasBeenSentOn(entryHeader, messageType, null));

			message.EM_ApplicationCode = ApplicationCodeList.Codes.AUCMR;
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = messageSubTypes[0];
			Factory.Save();
			Assert("Application Code not match", !messageLoader.MessageHasBeenSentOn(entryHeader, messageType, messageSubTypes));

			message.EM_ApplicationCode = ApplicationCodeList.Codes.INCustoms;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();
			Assert("Direction not match", !messageLoader.MessageHasBeenSentOn(entryHeader, messageType, messageSubTypes));
		});
	}

	protected override BusinessObject.Loader GetNewLoaderToTest() => new EDIMessage.Loader(Factory);
}
