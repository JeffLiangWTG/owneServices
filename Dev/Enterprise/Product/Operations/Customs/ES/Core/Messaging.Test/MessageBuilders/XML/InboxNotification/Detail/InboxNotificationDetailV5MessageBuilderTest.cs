using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.InboxNotification.DETALLEV5ENT;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(InboxNotificationDetailV5MessageBuilder))]
	public class InboxNotificationDetailV5MessageBuilderTest : InboxNotificationDetailCommonMessageBuilderTest<InboxNotificationDetailV5MessageBuilder, DetalleV5Ent>
	{
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

		protected override void SetUp()
		{
			base.SetUp();

			mockProvider.Setup(m => m.Key).Returns("20201021190135141811");
		}

		protected override InboxNotificationDetailV5MessageBuilder CreateMessageBuilder() => new InboxNotificationDetailV5MessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType);

		protected override InboxNotificationDetailV5MessageBuilder CreateMessageBuilderWithNullProvider() => new InboxNotificationDetailV5MessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.InboxNotificationTestFilePath, "TestInboxNotificationDetailV5.txt");
	}
}
