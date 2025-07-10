using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CC514C_v514.CC514CV1Ent;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;

[TestedType(typeof(CancelAESMessageBuilder))]
class CancelAESMessageBuilderTest : AESCommonMessageBuilderTest<CancelAESMessageBuilder, ICancelAESMessageDataProvider, Cc514Cv1Ent>
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

	public override void TestPopulatePhaseID()
	{
		mockProvider.Setup(m => m.PhaseIDSpecified).Returns(false);
		var messageText = CreateMessageBuilder().GetSignedMessageText();
		AssertNotContains("PhaseID", messageText);
	}

	public void TestPopulateExportOperation()
	{
		mockProvider.Setup(m => m.ExportOperation).Returns((ICancelAESExportOperation)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateCustomsOfficeOfExport()
	{
		mockProvider.Setup(m => m.CustomsOfficeOfExport).Returns(ZString.Empty);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateExporter()
	{
		mockProvider.Setup(m => m.Exporter).Returns((IPartyIdProvider)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateDeclarant()
	{
		mockProvider.Setup(m => m.Declarant).Returns((IPartyIdProvider)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateRepresentative()
	{
		mockProvider.Setup(m => m.Representative).Returns((ICommonRepresentative)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.ExportCancellation;

	protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.AESTestFilePath, "TestAESCancel.txt");

	protected override CancelAESMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new CancelAESMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

	protected override CancelAESMessageBuilder CreateMessageBuilderWithNullProvider() => new CancelAESMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

	protected override void SetUp()
	{
		base.SetUp();

		mockProvider.Setup(m => m.Message).Returns(SetUpMessage().Object);
		mockProvider.Setup(m => m.IsFinalPeriod).Returns(true);
		mockProvider.Setup(m => m.PhaseIDSpecified).Returns(true);

		var mockExportOperation = new Mock<ICancelAESExportOperation>();
		mockExportOperation.Setup(m => m.MRN).Returns("PRLSVNE000006");
		mockExportOperation.Setup(m => m.InvalidationReason).Returns("EX");
		mockProvider.Setup(m => m.ExportOperation).Returns(mockExportOperation.Object);

		mockProvider.Setup(m => m.CustomsOfficeOfExport).Returns("ES000101");

		var mockExporter = new Mock<IPartyIdProvider>();
		mockExporter.Setup(m => m.Id).Returns("ESA78587268");
		mockProvider.Setup(m => m.Exporter).Returns(mockExporter.Object);

		var mockDeclarant = new Mock<IPartyIdProvider>();
		mockDeclarant.Setup(m => m.Id).Returns("ESA78587268");
		mockProvider.Setup(m => m.Declarant).Returns(mockDeclarant.Object);

		var mockRepresentative = new Mock<ICommonRepresentative>();
		mockRepresentative.Setup(m => m.Id).Returns("ESA78587268");
		mockRepresentative.Setup(m => m.Status).Returns("2");
		mockProvider.Setup(m => m.Representative).Returns(mockRepresentative.Object);
	}
}
