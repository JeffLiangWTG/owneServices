using System;
using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G5.G5ExpCancelV1Ent;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(ExpCancelG5MessageBuilder))]
	class ExpCancelG5MessageBuilderTest : G5CommonMessageBuilderTest<ExpCancelG5MessageBuilder, IExpCancelG5MessageDataProvider, G5ExpCancelV1Ent>
	{
		#region Test
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

		public void TestPopulateTestIndicatorIsTestFalse()
		{
			mockProvider.Setup(m => m.IsTest).Returns(false);
			CombineAssertions(() =>
			{
				var messageText = CreateMessageBuilder().GetSignedMessageText();
				AssertNotContains("test indicator is not set", "<TestIndicator", messageText);
				AssertContains("Recipient is correct", ">ES.AEAT</Recipient>", messageText);
			});
		}

		public void TestPopulateHeader()
		{
			mockProvider.Setup(m => m.Header).Returns((IG5SimplifiedHeader)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateDeclarant()
		{
			mockHeader.Setup(m => m.Declarant).Returns((IG5PartyInfo)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateRepresentative()
		{
			mockHeader.Setup(m => m.Representative).Returns((IG5RepresentativeInfo)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateAdditionalInfos()
		{
			mockHeader.Setup(m => m.AdditionalInfos).Returns((IReadOnlyCollection<IDocumentsCommon>)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateAdditionalInfo()
		{
			mockHeader.Setup(m => m.AdditionalInfos).Returns(new IDocumentsCommon[] { null });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		#endregion

		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.G5v1ExpeditionCancellation;

		protected override ExpCancelG5MessageBuilder CreateMessageBuilder() => MockRandomGenerator(new ExpCancelG5MessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

		protected override ExpCancelG5MessageBuilder CreateMessageBuilderWithNullProvider() => new ExpCancelG5MessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.G5TestFilePath, "TestG5ExpCancelMessage.txt");

		#region Structures SetUp

		Mock<IG5SimplifiedHeader> mockHeader;

		protected override void SetUp()
		{
			base.SetUp();

			mockProvider.Setup(m => m.SenderId).Returns("A78587268");
			mockProvider.Setup(m => m.MRN).Returns("PRLSVNE000006");

			mockHeader = SetUpSimplifiedHeader();
			mockProvider.Setup(m => m.Header).Returns(mockHeader.Object);
		}

		Mock<IG5SimplifiedHeader> SetUpSimplifiedHeader()
		{
			var mockHeader = new Mock<IG5SimplifiedHeader>();

			mockHeader.Setup(m => m.LRN).Returns("MY0000215654884455");

			var mockDeclarant = SetUpIG5PartyInfo("12345678A", "NORON EHF", "1", "SKUTUVOGUR", "Extra Address", "7", "po box", "subdivision", "104 REYKJAVIK", "40025", "IS", "EM", "mail.mail@mail.com");
			mockHeader.Setup(m => m.Declarant).Returns(mockDeclarant);

			var mockRepresentative = SetUpRepresentativeInfo();
			mockHeader.Setup(m => m.Representative).Returns(mockRepresentative);

			var mockAddInfo1 = BuilderHelperTest.SetUpDocument(ZString.Empty, "Cambio de ultima hora de vuelo");
			var mockAddInfo2 = BuilderHelperTest.SetUpDocument("12345", ZString.Empty);
			mockHeader.Setup(m => m.AdditionalInfos).Returns(new[] { mockAddInfo1, mockAddInfo2 });

			return mockHeader;
		}

		#endregion
	}
}
