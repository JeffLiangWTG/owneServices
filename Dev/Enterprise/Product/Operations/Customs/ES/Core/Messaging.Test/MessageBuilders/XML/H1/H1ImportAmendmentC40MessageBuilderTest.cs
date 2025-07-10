using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.ADD_DDT_V1Ent;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;

[TestedType(typeof(H1ImportAmendmentC40MessageBuilder))]
sealed class H1ImportAmendmentC40MessageBuilderTest : H1ImportAbstractMessageBuilderTest<H1ImportAmendmentC40MessageBuilder, IH1ImportAmendmentC40MessageDataProvider, AddDdtV1Ent>
{
	public void TestPopulateImportOperation()
	{
		mockProvider.Setup(m => m.LRN).Returns(ZString.Empty);
		mockProvider.Setup(m => m.CustomsRegistrationNumber).Returns(ZString.Empty);
		AssertNoExceptionThrown("Empty LRN/CustomsRegistrationNumber", () => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateGoodsItem()
	{
		mockProvider.Setup(m => m.GoodsItems).Returns((IReadOnlyCollection<IH1ImportAmendmentC40GoodsItem>)null);
		AssertNoExceptionThrown("Null GoodsItems", () => CreateMessageBuilder().GetSignedMessageText());

		mockProvider.Setup(m => m.GoodsItems).Returns([]);
		AssertNoExceptionThrown("Empty GoodsItems", () => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateGoodsItemPreviousDocuments()
	{
		goodsItem1.Setup(m => m.PreviousDocuments).Returns((IReadOnlyCollection<IH1CommonPreviousDocument>)null);
		AssertNoExceptionThrown("Null GoodsItems[0].PreviousDocuments", () => CreateMessageBuilder().GetSignedMessageText());

		goodsItem1.Setup(m => m.PreviousDocuments).Returns([]);
		AssertNoExceptionThrown("Empty GoodsItems[0].PreviousDocuments", () => CreateMessageBuilder().GetSignedMessageText());
	}

	protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.ImportAmendmentBox40H1;

	protected override H1ImportAmendmentC40MessageBuilder CreateMessageBuilder() => MockRandomGenerator(new H1ImportAmendmentC40MessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

	protected override H1ImportAmendmentC40MessageBuilder CreateMessageBuilderWithNullProvider() => new H1ImportAmendmentC40MessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

	protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.H1TestFilePath, "TestH1ImportAmendmentC40Message.txt");

	protected override void SetUp()
	{
		base.SetUp();

		goodsItem1PreviousDocument1 = SetUpPreviousDocumentsMock(1, 1);
		var goodsItem1PreviousDocument2 = SetUpPreviousDocumentsMock(1, 2);

		goodsItem1 = new Mock<IH1ImportAmendmentC40GoodsItem>();
		goodsItem1.Setup(m => m.DeclarationGoodsItemNumber).Returns("1");
		goodsItem1.Setup(m => m.PreviousDocuments).Returns([goodsItem1PreviousDocument1.Object, goodsItem1PreviousDocument2.Object]);

		var goodsItem2 = new Mock<IH1ImportAmendmentC40GoodsItem>();
		goodsItem2.Setup(m => m.DeclarationGoodsItemNumber).Returns("2");

		mockProvider.Setup(m => m.IsTest).Returns(false);
		mockProvider.Setup(m => m.LRN).Returns("LSVPC00022");
		mockProvider.Setup(m => m.CustomsRegistrationNumber).Returns("24ES009998I0007HR1");
		mockProvider.Setup(m => m.GoodsItems).Returns([goodsItem1.Object, goodsItem2.Object]);
	}
	Mock<IH1ImportAmendmentC40GoodsItem> goodsItem1;
	Mock<IH1CommonPreviousDocument> goodsItem1PreviousDocument1;

	Mock<IH1CommonPreviousDocument> SetUpPreviousDocumentsMock(int sequenceNumber, int goodsItemId)
	{
		var mock = new Mock<IH1CommonPreviousDocument>();
		mock.Setup(m => m.SequenceNumber).Returns(sequenceNumber.ToString());
		mock.Setup(m => m.Name).Returns("N337");
		mock.Setup(m => m.Number).Returns("24ES00999880002800");
		mock.Setup(m => m.TypeOfPackages).Returns("JPB");
		mock.Setup(m => m.NumberOfPackages).Returns("1");
		mock.Setup(m => m.MeasurementUnitAndQualifier).Returns("KGMG");
		mock.Setup(m => m.Quantity).Returns(2);
		mock.Setup(m => m.GoodsItemId).Returns(goodsItemId.ToString());

		return mock;
	}
}
