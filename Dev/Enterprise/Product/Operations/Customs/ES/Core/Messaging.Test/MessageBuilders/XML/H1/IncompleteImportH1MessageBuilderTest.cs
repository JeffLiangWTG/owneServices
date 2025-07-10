using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.PDI400V1Ent;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;

[TestedType(typeof(IncompleteImportH1MessageBuilder))]
class IncompleteImportH1MessageBuilderTest : H1ImportAbstractMessageBuilderTest<IncompleteImportH1MessageBuilder, IIncompleteImportH1MessageDataProvider, Pdi400V1Ent>
{
	#region Tests

	public void TestPopulateImportOperation()
	{
		mockProvider.Setup(m => m.ImportOperation).Returns((IIncompleteImportH1ImportOperation)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateCustomsOffices()
	{
		mockProvider.Setup(m => m.CustomsOfficeOfImport).Returns(ZString.Empty);
		mockProvider.Setup(m => m.CustomOfficeOfPresentation).Returns(ZString.Empty);

		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateImporter()
	{
		mockProvider.Setup(m => m.Importer).Returns((IH1PartyProviderWithAddress)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateDeclarant()
	{
		mockProvider.Setup(m => m.Declarant).Returns((IPartyIdProviderWithContactPerson)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateRepresentative()
	{
		mockProvider.Setup(m => m.Representative).Returns((ICommonRepresentativeWithContactPerson)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateContactPerson()
	{
		mockProvider.Setup(m => m.Declarant.ContactPerson).Returns((IPartyContactProvider)null);
		mockProvider.Setup(m => m.Representative.ContactPerson).Returns((IPartyContactProvider)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateTransportEquipment()
	{
		mockProvider.Setup(m => m.TransportEquipments).Returns((IReadOnlyCollection<ICommonTransportEquipment>)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @"    </countryOfDispatch>
    <GoodsItem>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
		});
	}

	#endregion

	protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.ImportIncompletePreDeclarationH1;

	protected override IncompleteImportH1MessageBuilder CreateMessageBuilder() => MockRandomGenerator(new IncompleteImportH1MessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

	protected override IncompleteImportH1MessageBuilder CreateMessageBuilderWithNullProvider() => new IncompleteImportH1MessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

	protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.H1TestFilePath, "TestIncompleteImportH1Message.txt");

	#region Structures SetUp

	protected override void SetUp()
	{
		base.SetUp();

		var mockImportOperation = SetUpIncompleteImportOperation();
		mockProvider.Setup(m => m.ImportOperation).Returns(mockImportOperation.Object);

		mockProvider.Setup(m => m.Operation).Returns("A");
		mockProvider.Setup(m => m.CustomsOfficeOfImport).Returns("ES000101");
		mockProvider.Setup(m => m.CustomOfficeOfPresentation).Returns("ES009999");
		mockProvider.Setup(m => m.CountryOfDispatch).Returns("CN");

		var mockImporter = SetUpPartyProviderWithAddress("12345678A", ZString.Empty, "SKUTUVOGUR 7", "104 REYKJAVIK", "40025", "IS");
		mockProvider.Setup(m => m.Importer).Returns(mockImporter.Object);

		var mockContactPerson = BuilderHelperTest.SetUpContactInformation("Prometeo", "test_email@taric.es", "616123443");
		var mockDeclarant = BuilderHelperTest.SetUpPartyIdProviderWithContactPerson("ESA78587268", mockContactPerson);
		mockProvider.Setup(m => m.Declarant).Returns(mockDeclarant);

		var mockRepresentative = BuilderHelperTest.SetUpCommonRepresentativeWithContactPerson("ESA78587268", "2", mockContactPerson);
		mockProvider.Setup(m => m.Representative).Returns(mockRepresentative);

		var mockGoodReference1 = SetUpGoodReference("1", "1");
		var mockGoodReference2 = SetUpGoodReference("2", "2");
		var mockGoodReferences = new ICommonGoodsReference[] { mockGoodReference1.Object, mockGoodReference2.Object };

		var mockTransportEquipment1 = SetUpTransportEquipment("1", "HXDU1234567", mockGoodReferences);
		var mockTransportEquipment2 = SetUpTransportEquipment("2", "HXDU1234589", Enumerable.Empty<ICommonGoodsReference>());
		var mockTransportEquipments = new ICommonTransportEquipment[] { mockTransportEquipment1.Object, mockTransportEquipment2.Object };
		mockProvider.Setup(m => m.TransportEquipments).Returns(mockTransportEquipments);

		var mockProcedure = SetCommonUpProcedure<ICommonH1Procedure>();
		var mockCommodity = SetUpIncompleteCommodity();

		var mockGoodsShipmentItem1 = SetUpIncompleteGoodsShipmentItem("1", mockProcedure.Object, "US", mockCommodity.Object);
		var mockGoodsShipmentItem2 = SetUpIncompleteGoodsShipmentItem("2", null, ZString.Empty, null);
		var mockGoodsShipmentItems = new IIncompleteImportH1GoodsShipmentItem[] { mockGoodsShipmentItem1.Object, mockGoodsShipmentItem2.Object };
		mockProvider.Setup(m => m.GoodsShipmentItems).Returns(mockGoodsShipmentItems);
	}

	protected Mock<IIncompleteImportH1ImportOperation> SetUpIncompleteImportOperation()
	{
		var mockExportOperation = SetUpCommonImportOperation<IIncompleteImportH1ImportOperation>();
		mockExportOperation.Setup(m => m.CustomsRegistrationNumber).Returns("PRLSVNE000006");

		return mockExportOperation;
	}

	Mock<IIncompleteImportH1GoodsShipmentItem> SetUpIncompleteGoodsShipmentItem(ZString declarationGoodsItemNumber, ICommonH1Procedure procedure, ZString countryOfOrigin, IIncompleteImportH1Commodity commodity)
	{
		var mockGoodsShipmentItem = SetUpCommonGoodsShipmentItem<IIncompleteImportH1GoodsShipmentItem>(declarationGoodsItemNumber);
		mockGoodsShipmentItem.Setup(m => m.Procedure).Returns(procedure);
		mockGoodsShipmentItem.Setup(m => m.CountryOfOrigin).Returns(countryOfOrigin);
		mockGoodsShipmentItem.Setup(m => m.Commodity).Returns(commodity);
		return mockGoodsShipmentItem;
	}

	protected Mock<IIncompleteImportH1Commodity> SetUpIncompleteCommodity()
	{
		var mockCommodity = SetUpCommonCommodity<IIncompleteImportH1Commodity>();
		mockCommodity.Setup(m => m.CommodityCode).Returns(SetUpCommonCommodityCode<ICommonH1CommodityCode>().Object);

		return mockCommodity;
	}

	#endregion
}
