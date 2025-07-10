using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.AnulaPreH7V1Ent;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(CancelH7MessageBuilder))]
	class CancelH7MessageBuilderTest : H7CommonMessageBuilderTest<CancelH7MessageBuilder, ICancelH7MessageDataProvider, AnulaPreH7V1Ent>
	{
		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.H7Cancellation;

		protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.H7TestFilePath, "TestH7CancelMessage.txt");

		protected override CancelH7MessageBuilder CreateMessageBuilder() => MockRandomGenerator(new CancelH7MessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));
		protected override CancelH7MessageBuilder CreateMessageBuilderWithNullProvider() => new CancelH7MessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		public override void TestMessageTextWhenIsTestIsFalse()
		{
			mockProvider.Setup(m => m.IsTest).Returns(false);
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions(() =>
			{
				AssertContains("<messageRecipient>ES.AEAT</messageRecipient>", messageText);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			mockProvider.Setup(m => m.DeclarationMRN).Returns("99989036AZM0000101");
		}

		public override void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("When provider is null", () => CreateMessageBuilderWithNullProvider());
		}

		[TestDate(2020, 1, 9, 15, 13, 23, 456)]
		public override void TestCreateEDIMessage()
		{
			var messageBuilder = CreateMessageBuilder();

			CombineAssertions(() =>
			{
				AssertEquals("messageBuilder.MessageType", ExpectedMessageType, messageBuilder.MessageType);
				AssertEquals("messageBuilder.MessageSubType", ExpectedMessageSubType, messageBuilder.MessageSubType);
				AssertEquals("messageBuilder.Provider", mockProvider.Object, messageBuilder.Provider);
				AssertUnsignedMessageText(messageBuilder.UnsignedMessageText);
				AssertSignedMessageText(messageBuilder.GetSignedMessageText());
			});
		}
	}
}
