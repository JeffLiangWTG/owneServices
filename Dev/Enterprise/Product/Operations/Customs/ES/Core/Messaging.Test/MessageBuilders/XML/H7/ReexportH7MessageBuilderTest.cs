using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.ReexportacionH7V1Ent;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(ReexportH7MessageBuilder))]
	class ReexportH7MessageBuilderTest : H7CommonMessageBuilderTest<ReexportH7MessageBuilder, IReexportH7MessageDataProvider, ReexportacionH7V1Ent>
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

		public override void TestMessageTextWhenIsTestIsFalse()
		{
			mockProvider.Setup(m => m.IsTest).Returns(false);
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions(() =>
			{
				AssertContains("<messageRecipient>ES.AEAT</messageRecipient>", messageText);
			});
		}

		public void TestDeclarationMRNCodesNull()
		{
			mockProvider.Setup(m => m.DeclarationMRNCodes).Returns(new ZString[] { null });
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("MRN_H7", messageText);
		}

		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.H7ReExport;

		protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.H7TestFilePath, "TestH7ReexportMessage.txt");

		protected override ReexportH7MessageBuilder CreateMessageBuilder() => MockRandomGenerator(new ReexportH7MessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));
		protected override ReexportH7MessageBuilder CreateMessageBuilderWithNullProvider() => new ReexportH7MessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected override void SetUp()
		{
			base.SetUp();

			mockProvider.Setup(m => m.OperationCode).Returns("0");

			var mockDeclarant = BuilderHelperTest.SetUpPartyName("76688664B", "JUAN MARTINEZ");
			mockProvider.Setup(m => m.Declarant).Returns(mockDeclarant);
			mockProvider.Setup(m => m.DeclarationMRNCodes).Returns(new ZString[] { "99989036AZM0000101", "99989036AZM0000202", "99989036AZM0000303" });
		}
	}
}
