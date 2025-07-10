using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.InboxNotification.DETALLEV4ENT;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(InboxNotificationDetailV4MessageBuilder))]
	public class InboxNotificationDetailV4MessageBuilderTest : InboxNotificationDetailCommonMessageBuilderTest<InboxNotificationDetailV4MessageBuilder, DetalleV4Ent>
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

		protected override InboxNotificationDetailV4MessageBuilder CreateMessageBuilder() => new InboxNotificationDetailV4MessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType);

		protected override InboxNotificationDetailV4MessageBuilder CreateMessageBuilderWithNullProvider() => new InboxNotificationDetailV4MessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.InboxNotificationTestFilePath, "TestInboxNotificationDetailV4.txt");
	}
}
