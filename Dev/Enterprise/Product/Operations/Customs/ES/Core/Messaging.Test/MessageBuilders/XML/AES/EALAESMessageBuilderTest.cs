using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CC507C_v514.CC507CV1Ent;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;

[TestedType(typeof(EALAESMessageBuilder))]
class EALAESMessageBuilderTest : AESCommonMessageBuilderTest<EALAESMessageBuilder, IEALAESMessageDataProvider, Cc507Cv1Ent>
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
		mockProvider.Setup(m => m.ExportOperation).Returns((IEALAESExportOperation)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateCustomsOfficeOfExitActual()
	{
		mockProvider.Setup(m => m.CustomsOfficeOfExitActual).Returns(ZString.Empty);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateGoodsShipment()
	{
		mockProvider.Setup(m => m.GoodsShipment).Returns((IEALAESGoodsShipment)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateConsignment()
	{
		mockProvider.Setup(m => m.GoodsShipment.Consignment).Returns((IEALAESConsignment)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateExitCarrier()
	{
		mockProvider.Setup(m => m.GoodsShipment.Consignment.ExitCarrier).Returns((IPartyIdProviderWithContactPerson)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateContactPerson()
	{
		mockProvider.Setup(m => m.GoodsShipment.Consignment.ExitCarrier.ContactPerson).Returns((IPartyContactProvider)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateTransportEquipment()
	{
		mockProvider.Setup(m => m.GoodsShipment.Consignment.TransportEquipment).Returns((IReadOnlyCollection<IAESCommonTransportEquipment>)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLocationOfGoods()
	{
		mockProvider.Setup(m => m.GoodsShipment.Consignment.LocationOfGoods).Returns((IEALAESLocationOfGoods)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateActiveBorderTransportMeans()
	{
		mockProvider.Setup(m => m.GoodsShipment.Consignment.ActiveBorderTransportMeans).Returns((ITransportMediumInfoCommon)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateTransportDocument()
	{
		mockProvider.Setup(m => m.GoodsShipment.Consignment.TransportDocument).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateGoodsItem()
	{
		mockProvider.Setup(m => m.GoodsShipment.GoodsItem).Returns((IReadOnlyCollection<IEALAESGoodsItem>)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.ArrivalAtExitUcc6;

	protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.AESTestFilePath, "TestAESEAL.txt");

	protected override EALAESMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new EALAESMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

	protected override EALAESMessageBuilder CreateMessageBuilderWithNullProvider() => new EALAESMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

	protected override void SetUp()
	{
		base.SetUp();

		mockProvider.Setup(m => m.Message).Returns(SetUpMessage().Object);
		mockProvider.Setup(m => m.IsFinalPeriod).Returns(true);
		mockProvider.Setup(m => m.PhaseIDSpecified).Returns(true);

		var mockExportOperation = new Mock<IEALAESExportOperation>();
		mockExportOperation.Setup(m => m.MRN).Returns("PRLSVNE000006");
		mockExportOperation.Setup(m => m.StoringFlag).Returns(false);
		mockExportOperation.Setup(m => m.DiscrepanciesExist).Returns(true);
		mockProvider.Setup(m => m.ExportOperation).Returns(mockExportOperation.Object);

		mockProvider.Setup(m => m.CustomsOfficeOfExitActual).Returns("ES000101");

		mockGoodsShipment = SetUpHeaderGoodsShipment();
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
	}
	Mock<IEALAESGoodsShipment> mockGoodsShipment;

	Mock<IEALAESGoodsShipment> SetUpHeaderGoodsShipment()
	{
		var mockHeader = new Mock<IEALAESGoodsShipment>();

		mockConsignment = SetUpConsignment();
		mockHeader.Setup(m => m.Consignment).Returns(mockConsignment.Object);

		var mockCommodity1 = new Mock<IEALAESCommodity>();
		var mockGoodsMeasure1 = new Mock<ICommonGoodsMeasureWithSpecified>();
		mockGoodsMeasure1.Setup(m => m.GrossWeight).Returns((decimal)30.2);
		mockGoodsMeasure1.Setup(m => m.GrossWeightSpecified).Returns(true);
		mockGoodsMeasure1.Setup(m => m.NetWeight).Returns((decimal)28.5);
		mockGoodsMeasure1.Setup(m => m.NetWeightSpecified).Returns(true);
		mockCommodity1.Setup(m => m.GoodsMeasure).Returns(mockGoodsMeasure1.Object);

		var mockCommodity2 = new Mock<IEALAESCommodity>();
		var mockGoodsMeasure2 = new Mock<ICommonGoodsMeasureWithSpecified>();
		mockGoodsMeasure2.Setup(m => m.GrossWeight).Returns((decimal)30.2);
		mockGoodsMeasure2.Setup(m => m.GrossWeightSpecified).Returns(false);
		mockGoodsMeasure2.Setup(m => m.NetWeight).Returns((decimal)28.5);
		mockGoodsMeasure2.Setup(m => m.NetWeightSpecified).Returns(false);
		mockCommodity2.Setup(m => m.GoodsMeasure).Returns(mockGoodsMeasure2.Object);

		var mockPackage1 = BuilderHelperTest.SetUpCommonPackages("1", "VO", "MARCA1", "15");
		var mockPackage2 = BuilderHelperTest.SetUpCommonPackages("2", "CR", "MARCA2", "15");
		var mockPackages = new ICommonPackageWithSequenceAndPackNum[] { mockPackage1, mockPackage2 };

		var mockTransportDocument1 = BuilderHelperTest.SetUpDocumentSequenceNumber("1", "N760", "SO700039");
		var mockTransportDocument2 = BuilderHelperTest.SetUpDocumentSequenceNumber("2", "N705", "Barco");
		var mockTransportDocuments = new ICommonDocumentSequenceNumber[] { mockTransportDocument1, mockTransportDocument2 };

		var mockGoodsItem1 = SetUpGoodsItem("1", "REF1234", mockCommodity1.Object, mockPackages, mockTransportDocuments);

		var mockGoodsItem2 = SetUpGoodsItem("2", "REF6789", mockCommodity2.Object, Enumerable.Empty<ICommonPackageWithSequenceAndPackNum>(), Enumerable.Empty<ICommonDocumentSequenceNumber>());

		var mockGoodsItems = new IEALAESGoodsItem[] { mockGoodsItem1.Object, mockGoodsItem2.Object };

		mockHeader.Setup(m => m.GoodsItem).Returns(mockGoodsItems);

		return mockHeader;
	}
	Mock<IEALAESConsignment> mockConsignment;

	Mock<IEALAESConsignment> SetUpConsignment()
	{
		var mockConsignment = new Mock<IEALAESConsignment>();
		mockConsignment.Setup(m => m.ModeOfTransportAtTheBorder).Returns("3");
		mockConsignment.Setup(m => m.ReferenceNumberUCR).Returns("AB45612312345C");

		var mockContactPerson = BuilderHelperTest.SetUpContactInformation("Prometeo", "test_email@taric.es", "616123443");

		var mockExitCarrier = new Mock<IPartyIdProviderWithContactPerson>();
		mockExitCarrier.Setup(m => m.Id).Returns("DE1185233");
		mockExitCarrier.Setup(m => m.ContactPerson).Returns(mockContactPerson);
		mockConsignment.Setup(m => m.ExitCarrier).Returns(mockExitCarrier.Object);

		var mockSeal1 = BuilderHelperTest.SetUpSeal("1", "F742");
		var mockSeal2 = BuilderHelperTest.SetUpSeal("2", "F742");
		var mockSeals = new ISealCommon[] { mockSeal1.Object, mockSeal2.Object };

		var mockGoodReference1 = SetUpGoodReference("1", "1");
		var mockGoodReference2 = SetUpGoodReference("2", "2");
		var mockGoodReferences = new ICommonGoodsReference[] { mockGoodReference1.Object, mockGoodReference2.Object };

		var mockTransportEquipment1 = SetUpTransportEquipment("1", "2", mockSeals, mockGoodReferences);
		var mockTransportEquipment2 = SetUpTransportEquipment("2", "0", Enumerable.Empty<ISealCommon>(), Enumerable.Empty<ICommonGoodsReference>());
		var mockTransportEquipments = new IAESCommonTransportEquipment[] { mockTransportEquipment1.Object, mockTransportEquipment2.Object };
		mockConsignment.Setup(m => m.TransportEquipment).Returns(mockTransportEquipments);

		var mockLocationOfGoods = new Mock<IEALAESLocationOfGoods>();
		mockLocationOfGoods.Setup(m => m.SequenceNumber).Returns("1");
		mockLocationOfGoods.Setup(m => m.LocationType).Returns("B");
		mockLocationOfGoods.Setup(m => m.LocationQualifier).Returns("Y");
		mockLocationOfGoods.Setup(m => m.LocationId).Returns("AUTH3AH3");
		mockConsignment.Setup(m => m.LocationOfGoods).Returns(mockLocationOfGoods.Object);

		var mockActiveBorderTransportMeans = new Mock<ITransportMediumInfoCommon>();
		mockActiveBorderTransportMeans.Setup(m => m.TransportMode).Returns("30");
		mockActiveBorderTransportMeans.Setup(m => m.TransportId).Returns("Plane");
		mockActiveBorderTransportMeans.Setup(m => m.TransportNationality).Returns("ES");
		mockConsignment.Setup(m => m.ActiveBorderTransportMeans).Returns(mockActiveBorderTransportMeans.Object);

		var mockTransportDocument1 = BuilderHelperTest.SetUpDocumentSequenceNumber("1", "DOC", "12345A");
		var mockTransportDocument2 = BuilderHelperTest.SetUpDocumentSequenceNumber("2", "DOC", "67890A");
		var mockTransportDocuments = new ICommonDocumentSequenceNumber[] { mockTransportDocument1, mockTransportDocument2 };
		mockConsignment.Setup(m => m.TransportDocument).Returns(mockTransportDocuments);

		return mockConsignment;
	}

	Mock<IEALAESGoodsItem> SetUpGoodsItem(string itemNumber, string referenceNumber, IEALAESCommodity commodity, IEnumerable<ICommonPackageWithSequenceAndPackNum> packaging, IEnumerable<ICommonDocumentSequenceNumber> transportDocuments)
	{
		var mockGoodsItem = new Mock<IEALAESGoodsItem>();
		mockGoodsItem.Setup(m => m.SequenceNumber).Returns(itemNumber);
		mockGoodsItem.Setup(m => m.ReferenceNumberUCR).Returns(referenceNumber);
		mockGoodsItem.Setup(m => m.Commodity).Returns(commodity);
		mockGoodsItem.Setup(m => m.Packaging).Returns((IReadOnlyCollection<ICommonPackageWithSequenceAndPackNum>)packaging);
		mockGoodsItem.Setup(m => m.TransportDocuments).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)transportDocuments);

		return mockGoodsItem;
	}
}
