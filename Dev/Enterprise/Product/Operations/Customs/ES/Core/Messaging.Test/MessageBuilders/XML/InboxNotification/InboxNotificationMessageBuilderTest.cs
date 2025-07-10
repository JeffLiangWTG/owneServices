using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.InboxNotification.LISTADECV4ENT;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(InboxNotificationMessageBuilder))]
	public class InboxNotificationMessageBuilderTest : XMLMessageBuilderTest<InboxNotificationMessageBuilder, IInboxNotificationMessageDataProvider, ListaDecV4Ent>
	{
		#region Tests
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

		public void TestPopulateDeclarantName()
		{
			mockProvider.Setup(m => m.DeclarantName).Returns("");
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

				var messageText = CreateMessageBuilder().GetSignedMessageText();
				var expectedResult = @$"<{XMLTestFileConstants.XmlElementNamespace}NombreDeclarante />";
				AssertContains(expectedResult, messageText);
			});
		}

		public void TestPopulateDeclarantID()
		{
			mockProvider.Setup(m => m.DeclarantID).Returns("");
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

				var messageText = CreateMessageBuilder().GetSignedMessageText();
				var expectedResult = @$"<{XMLTestFileConstants.XmlElementNamespace}NifDeclarante />";
				AssertContains(expectedResult, messageText);
			});
		}

		#endregion

		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.InBoxNotificationForExport;

		protected override InboxNotificationMessageBuilder CreateMessageBuilder() => new InboxNotificationMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType);

		protected override InboxNotificationMessageBuilder CreateMessageBuilderWithNullProvider() => new InboxNotificationMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected override ZString GetSignedMessageTestFileContent() => GetTestFile();
		protected override ZString GetUnsignedMessageTestFileContent() => GetTestFile();
		ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.InboxNotificationTestFilePath, "TestInboxNotification.txt");

		#region Structures SetUp

		protected override void SetUp()
		{
			base.SetUp();

			mockProvider.Setup(m => m.ResponseType).Returns("https://www3.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/dit/adu/adex/ws/predua/NotifPreDUAV1.wsdl");
			mockProvider.Setup(m => m.DeclarantName).Returns("Declarant Full Name");
			mockProvider.Setup(m => m.DeclarantID).Returns("NIF22222222");
		}

		#endregion
	}
}
