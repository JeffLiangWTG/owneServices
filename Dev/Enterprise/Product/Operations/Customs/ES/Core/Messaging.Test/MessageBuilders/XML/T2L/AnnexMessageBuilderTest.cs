using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LanexosV1Ent;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(AnnexMessageBuilder))]
	public class AnnexMessageBuilderTest : T2LCommonMessageBuilderTest<AnnexMessageBuilder, IAnnexMessageDataProvider, T2LanexosV1Ent, IT2LHeaderCommon, IT2LLineCommon>
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

		public void TestPopulateAnnexDeclarante()
		{
			mockAnnexHeader.Setup(m => m.Declarant).Returns((IPartyNameProvider)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateDocumento()
		{
			mockProvider.Setup(m => m.Document).Returns((IAnnexDocCommon)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.T2lAnnex;

		protected override ZString GetSignedMessageTestFileContent() => GetTestFileContents(XMLTestFileConstants.T2LTestFilePath, "TestAnnexMessageSigned.txt");
		protected override ZString GetUnsignedMessageTestFileContent() => GetTestFileContents(XMLTestFileConstants.T2LTestFilePath, "TestAnnexMessage.txt");

		protected override AnnexMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new AnnexMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));
		protected override AnnexMessageBuilder CreateMessageBuilderWithNullProvider() => new AnnexMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected override void SetUp()
		{
			base.SetUp();

			mockAnnexHeader = SetUpHeader();
			mockProvider.Setup(m => m.Header).Returns(mockAnnexHeader.Object);

			var mockDocument = SetUpAnnexDocument();
			mockProvider.Setup(m => m.Document).Returns(mockDocument.Object);
		}

		protected Mock<IAnnexHeader> mockAnnexHeader;

		Mock<IAnnexHeader> SetUpHeader()
		{
			var mockHeader = new Mock<IAnnexHeader>();
			mockHeader.Setup(m => m.T2LReferenceNumber).Returns("20IT003456733287");
			mockHeader.Setup(m => m.FinalAnnexIndicator).Returns(false);

			var mockDeclarant = BuilderHelperTest.SetUpPartyName("A78587268", "EJEMPLO");
			mockHeader.Setup(m => m.Declarant).Returns(mockDeclarant);
			return mockHeader;
		}

		Mock<IAnnexDocCommon> SetUpAnnexDocument()
		{
			var mockDocument = new Mock<IAnnexDocCommon>();
			mockDocument.Setup(m => m.Description).Returns("XXX");
			mockDocument.Setup(m => m.ReferenceNumber).Returns("TEST.JPG");
			mockDocument.Setup(m => m.Image).Returns(ZBlob.FromAscii("Test Image data"));
			mockDocument.Setup(m => m.Extension).Returns("JPG");
			return mockDocument;
		}
	}
}
