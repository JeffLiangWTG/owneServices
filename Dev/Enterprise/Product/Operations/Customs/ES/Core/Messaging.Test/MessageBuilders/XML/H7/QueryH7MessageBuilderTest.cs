using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.ConsultaH7V1Ent;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(QueryH7MessageBuilder))]
	class QueryH7MessageBuilderTest : H7CommonMessageBuilderTest<QueryH7MessageBuilder, IQueryH7MessageDataProvider, ConsultaH7V1Ent>
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

		public void TestDeclarationMRNEmpty()
		{
			mockProvider.Setup(m => m.DeclarationMRN).Returns(ZString.Empty);
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("MRN_H7>", messageText);
		}

		public void TestG3DeclarationMRNEmpty()
		{
			mockProvider.Setup(m => m.G3DeclarationMRN).Returns(ZString.Empty);
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("MRN_G3", messageText);
		}

		public void TestNextH7DeclarationMRNEmpty()
		{
			mockProvider.Setup(m => m.NextH7DeclarationMRN).Returns(ZString.Empty);
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("MRN_H7_Next", messageText);
		}

		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.H7Query;

		protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.H7TestFilePath, "TestH7QueryMessage.txt");

		protected override QueryH7MessageBuilder CreateMessageBuilder() => MockRandomGenerator(new QueryH7MessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));
		protected override QueryH7MessageBuilder CreateMessageBuilderWithNullProvider() => new QueryH7MessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected override void SetUp()
		{
			base.SetUp();

			mockProvider.Setup(m => m.DeclarationMRN).Returns("99989036AZM0000101");
			mockProvider.Setup(m => m.G3DeclarationMRN).Returns("99989036AZM0000202");
			mockProvider.Setup(m => m.NextH7DeclarationMRN).Returns("99989036AZM0000303");
		}
	}
}
