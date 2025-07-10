using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.CommonAnnex.EnvioDeDocumentosV1Ent;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;

[TestedType(typeof(CommonAnnexMessageBuilder))]
class CommonAnnexMessageBuilderTest : XMLMessageBuilderTest<CommonAnnexMessageBuilder, ICommonAnnexMessageDataProvider, EnvioDeDocumentosV1Ent>
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

	public void TestPopulateSegmentosDeServicioIsTestFalse()
	{
		mockProvider.Setup(m => m.IsTest).Returns(false);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains("Test=", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateDocument()
	{
		mockProvider.Setup(m => m.Document).Returns((IAnnexDocCommon)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains("<Documento>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	#endregion

	protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.T2lDocumentationPous;

	protected override CommonAnnexMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new CommonAnnexMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

	protected override CommonAnnexMessageBuilder CreateMessageBuilderWithNullProvider() => new CommonAnnexMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

	protected override sealed ZString GetSignedMessageTestFileContent() => GetTestFile();
	protected override sealed ZString GetUnsignedMessageTestFileContent() => GetTestFile();
	protected ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.T2LPOUSTestFilePath, "TestT2LPOUSDocumentation.txt");

	#region Structures SetUp

	protected override void SetUp()
	{
		base.SetUp();

		mockProvider.Setup(m => m.Operation).Returns("03");
		mockProvider.Setup(m => m.Reference).Returns("T2LReference");
		mockProvider.Setup(m => m.RequestDispatchTagName).Returns("SolicitudDespacho");
		mockProvider.Setup(m => m.DispatchRequest).Returns("S");
		mockProvider.Setup(m => m.AdministrationCode).Returns("ATC");

		var mockDocument = SetUpAnnexDocument();
		mockProvider.Setup(m => m.Document).Returns(mockDocument.Object);
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

	#endregion
}
