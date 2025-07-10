using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.SUP_DOC_V1Ent;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;

[TestedType(typeof(H1ImportAmendmentC44MessageBuilder))]
sealed class H1ImportAmendmentC44MessageBuilderTest : H1ImportAbstractMessageBuilderTest<H1ImportAmendmentC44MessageBuilder, IH1ImportAmendmentC44MessageDataProvider, SupDocV1Ent>
{
	public void TestPopulateImportOperation()
	{
		mockProvider.Setup(m => m.LRN).Returns(ZString.Empty);
		mockProvider.Setup(m => m.MRN).Returns(ZString.Empty);
		AssertNoExceptionThrown("Empty LRN/MRN", () => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateGoodsItem()
	{
		mockProvider.Setup(m => m.GoodsItems).Returns((IReadOnlyCollection<IH1ImportAmendmentC44GoodsItem>)null);
		AssertNoExceptionThrown("Null GoodsItems", () => CreateMessageBuilder().GetSignedMessageText());

		mockProvider.Setup(m => m.GoodsItems).Returns([]);
		AssertNoExceptionThrown("Empty GoodsItems", () => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateGoodsItemSupportingDocuments()
	{
		goodsItem1.Setup(m => m.SupportingDocuments).Returns((IReadOnlyCollection<IH1CommonLineSupportingDocument>)null);
		AssertNoExceptionThrown("Null GoodsItems[0].SupportingDocuments", () => CreateMessageBuilder().GetSignedMessageText());

		goodsItem1.Setup(m => m.SupportingDocuments).Returns([]);
		AssertNoExceptionThrown("Empty GoodsItems[0].SupportingDocuments", () => CreateMessageBuilder().GetSignedMessageText());
	}

	protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.Box44DocumentsH1;

	protected override H1ImportAmendmentC44MessageBuilder CreateMessageBuilder() => MockRandomGenerator(new H1ImportAmendmentC44MessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

	protected override H1ImportAmendmentC44MessageBuilder CreateMessageBuilderWithNullProvider() => new H1ImportAmendmentC44MessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

	protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.H1TestFilePath, "TestH1ImportAmendmentC44Message.txt");

	protected override void SetUp()
	{
		base.SetUp();

		goodsItem1SupportingDocument1 = SetUpSupportingDocumentsMock(1, 1);
		var goodsItem1SupportingDocument2 = SetUpSupportingDocumentsMock(1, 2);

		goodsItem1 = new Mock<IH1ImportAmendmentC44GoodsItem>();
		goodsItem1.Setup(m => m.DeclarationGoodsItemNumber).Returns("1");
		goodsItem1.Setup(m => m.SupportingDocuments).Returns([goodsItem1SupportingDocument1.Object, goodsItem1SupportingDocument2.Object]);

		var goodsItem2SupportingDocument1 = SetUpSupportingDocumentsMock(2, 1);
		var goodsItem2SupportingDocument2 = SetUpSupportingDocumentsMock(2, 2);

		var goodsItem2 = new Mock<IH1ImportAmendmentC44GoodsItem>();
		goodsItem2.Setup(m => m.DeclarationGoodsItemNumber).Returns("2");
		goodsItem2.Setup(m => m.SupportingDocuments).Returns([goodsItem2SupportingDocument1.Object, goodsItem2SupportingDocument2.Object]);

		mockProvider.Setup(m => m.IsTest).Returns(false);
		mockProvider.Setup(m => m.LRN).Returns("LSVPDI0029");
		mockProvider.Setup(m => m.MRN).Returns("24ES009999I000PER0");
		mockProvider.Setup(m => m.GoodsItems).Returns([goodsItem1.Object, goodsItem2.Object]);
	}

	Mock<IH1CommonLineSupportingDocument> SetUpSupportingDocumentsMock(int sequenceNumber, int lineNumber)
	{
		var mock = new Mock<IH1CommonLineSupportingDocument>();
		mock.Setup(m => m.SequenceNumber).Returns(sequenceNumber.ToString());
		mock.Setup(m => m.Name).Returns("1403");
		mock.Setup(m => m.Number).Returns("ES S/N 123");
		mock.Setup(m => m.LineNumber).Returns(lineNumber.ToString());
		mock.Setup(m => m.IssuingAuthorityName).Returns("Issuing Authority");
		mock.Setup(m => m.DocumentDate).Returns(new ZDateTime(2024, 12, 17, 23, 10, 53));
		mock.Setup(m => m.Quantity).Returns(2);
		mock.Setup(m => m.MeasurementUnitAndQualifier).Returns("KGM");
		mock.Setup(m => m.Currency).Returns("EUR");
		mock.Setup(m => m.Amount).Returns(123);

		return mock;
	}

	Mock<IH1ImportAmendmentC44GoodsItem> goodsItem1;
	Mock<IH1CommonLineSupportingDocument> goodsItem1SupportingDocument1;
}
