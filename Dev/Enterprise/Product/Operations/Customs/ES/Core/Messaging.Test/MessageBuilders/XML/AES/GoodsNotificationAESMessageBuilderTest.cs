using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CC511C_v514.CC511CV1Ent;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;

[TestedType(typeof(GoodsNotificationAESMessageBuilder))]
public class GoodsNotificationAESMessageBuilderTest : AESCommonMessageBuilderTest<GoodsNotificationAESMessageBuilder, IGoodsNotificationAESMessageDataProvider, Cc511Cv1Ent>
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

	public override void TestPopulatePhaseID()
	{
		mockProvider.Setup(m => m.PhaseIDSpecified).Returns(false);
		var messageText = CreateMessageBuilder().GetSignedMessageText();
		AssertNotContains("PhaseID", messageText);
	}

	public void TestPopulateExportOperation()
	{
		mockProvider.Setup(m => m.ExportOperation).Returns((IGoodsNotificationAESExportOperationLRN)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateCustomsOffices()
	{
		mockProvider.Setup(m => m.CustomOfficeOfPresentation).Returns(ZString.Empty);
		mockProvider.Setup(m => m.CustomOfficeOfExport).Returns(ZString.Empty);

		mockLocationOfGoods.Setup(m => m.LocationCustomOffice).Returns(ZString.Empty);
		mockConsignment.Setup(m => m.LocationOfGoods).Returns(mockLocationOfGoods.Object);
		mockGoodsShipment.Setup(m => m.Consignment).Returns(mockConsignment.Object);
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
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
		mockProvider.Setup(m => m.GoodsShipment.Consignment.LocationOfGoods.LocationContactPerson).Returns((IPartyContactProvider)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateGoodsShipment()
	{
		mockProvider.Setup(m => m.GoodsShipment).Returns((IGoodsNotificationAESGoodsShipment)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateConsignment()
	{
		mockProvider.Setup(m => m.GoodsShipment.Consignment).Returns((IGoodsNotificationAESConsignment)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateTransportEquipment()
	{
		mockConsignment.Setup(m => m.TransportEquipment).Returns((IReadOnlyCollection<IAESCommonTransportEquipment>)null);
		mockGoodsShipment.Setup(m => m.Consignment).Returns(mockConsignment.Object);
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @$"</{XMLTestFileConstants.XmlElementNamespace}inlandModeOfTransport>
        <{XMLTestFileConstants.XmlElementNamespace}LocationOfGoods>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
		});
	}

	public void TestPopulateSeal()
	{
		mockTransportEquipment1.Setup(m => m.Seals).Returns((IReadOnlyCollection<ISealCommon>)(IEnumerable<ZString>)null);
		mockConsignment.Setup(m => m.TransportEquipment).Returns(new[] { mockTransportEquipment1.Object });
		mockGoodsShipment.Setup(m => m.Consignment).Returns(mockConsignment.Object);
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @$"</{XMLTestFileConstants.XmlElementNamespace}numberOfSeals>
          <{XMLTestFileConstants.XmlElementNamespace}GoodsReference>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
		});
	}

	public void TestPopulateGoodsReference()
	{
		mockTransportEquipment1.Setup(m => m.GoodsReference).Returns((IReadOnlyCollection<ICommonGoodsReference>)null);
		mockConsignment.Setup(m => m.TransportEquipment).Returns(new[] { mockTransportEquipment1.Object });
		mockGoodsShipment.Setup(m => m.Consignment).Returns(mockConsignment.Object);
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @$"</{XMLTestFileConstants.XmlElementNamespace}Seal>
        </{XMLTestFileConstants.XmlElementNamespace}TransportEquipment>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
		});
	}

	public void TestPopulateLocationOfGoods()
	{
		mockProvider.Setup(m => m.GoodsShipment.Consignment.LocationOfGoods).Returns((IAESCommonLocationOfGoods)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLocationGNSS()
	{
		mockProvider.Setup(m => m.GoodsShipment.Consignment.LocationOfGoods.LocationGNSS).Returns((ICommonGNSS)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLocationEconomicOperator()
	{
		mockLocationOfGoods.Setup(m => m.LocationEconomicOperatorId).Returns(ZString.Empty);
		mockConsignment.Setup(m => m.LocationOfGoods).Returns(mockLocationOfGoods.Object);
		mockGoodsShipment.Setup(m => m.Consignment).Returns(mockConsignment.Object);
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLocationLocationAddress()
	{
		mockProvider.Setup(m => m.GoodsShipment.Consignment.LocationOfGoods.LocationAddress).Returns((IPartyAddressProvider)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLocationLocationPostcodeAddress()
	{
		mockProvider.Setup(m => m.GoodsShipment.Consignment.LocationOfGoods.LocationPostcodeAddress).Returns((ICommonPostcodeAddress)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateDepartureTransportMeans()
	{
		mockConsignment.Setup(m => m.DepartureTransportMeans).Returns((IReadOnlyCollection<ICommonDepartureTransportMeans>)null);
		mockGoodsShipment.Setup(m => m.Consignment).Returns(mockConsignment.Object);
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @$"</{XMLTestFileConstants.XmlElementNamespace}LocationOfGoods>
      </{XMLTestFileConstants.XmlElementNamespace}Consignment>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
		});
	}

	#endregion

	protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.ExportNotification;

	protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.AESTestFilePath, "TestAESGoodsNotification.txt");

	protected override GoodsNotificationAESMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new GoodsNotificationAESMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

	protected override GoodsNotificationAESMessageBuilder CreateMessageBuilderWithNullProvider() => new GoodsNotificationAESMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

	#region Structures SetUp

	protected override void SetUp()
	{
		base.SetUp();

		mockProvider.Setup(m => m.Message).Returns(SetUpMessage().Object);
		mockProvider.Setup(m => m.IsFinalPeriod).Returns(true);
		mockProvider.Setup(m => m.PhaseIDSpecified).Returns(true);

		var mockExportOperation = new Mock<IGoodsNotificationAESExportOperationLRN>();
		mockExportOperation.Setup(m => m.LRN).Returns("PRLSVNE000006");
		mockProvider.Setup(m => m.ExportOperation).Returns(mockExportOperation.Object);

		mockProvider.Setup(m => m.CustomOfficeOfPresentation).Returns("ES009999");
		mockProvider.Setup(m => m.CustomOfficeOfExport).Returns("ES000101");

		var mockContactPerson = BuilderHelperTest.SetUpContactInformation("Prometeo", "test_email@taric.es", "616123443");
		var mockDeclarant = BuilderHelperTest.SetUpPartyIdProviderWithContactPerson("ESA78587268", mockContactPerson);
		mockProvider.Setup(m => m.Declarant).Returns(mockDeclarant);

		var mockRepresentative = BuilderHelperTest.SetUpCommonRepresentativeWithContactPerson("ESA78587268", "2", mockContactPerson);
		mockProvider.Setup(m => m.Representative).Returns(mockRepresentative);

		mockGoodsShipment = SetUpHeaderGoodsShipment(mockContactPerson);
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
	}

	Mock<IGoodsNotificationAESGoodsShipment> mockGoodsShipment;
	Mock<IGoodsNotificationAESConsignment> mockConsignment;
	Mock<IAESCommonTransportEquipment> mockTransportEquipment1;

	Mock<IGoodsNotificationAESGoodsShipment> SetUpHeaderGoodsShipment(IPartyContactProvider contactPerson)
	{
		var mockHeader = new Mock<IGoodsNotificationAESGoodsShipment>();

		mockConsignment = SetUpConsignment(contactPerson);
		mockHeader.Setup(m => m.Consignment).Returns(mockConsignment.Object);

		return mockHeader;
	}

	protected Mock<IGoodsNotificationAESConsignment> SetUpConsignment(IPartyContactProvider contactPerson)
	{
		var mockConsignment = new Mock<IGoodsNotificationAESConsignment>();
		mockConsignment.Setup(m => m.IsContainerised).Returns(true);
		mockConsignment.Setup(m => m.InlandModeOfTransport).Returns("3");

		var mockSeal1 = BuilderHelperTest.SetUpSeal("1", "F742");
		var mockSeal2 = BuilderHelperTest.SetUpSeal("2", "F742");
		var mockSeals = new ISealCommon[] { mockSeal1.Object, mockSeal2.Object };

		var mockGoodReference1 = SetUpGoodReference("1", "1");
		var mockGoodReference2 = SetUpGoodReference("2", "2");
		var mockGoodReferences = new ICommonGoodsReference[] { mockGoodReference1.Object, mockGoodReference2.Object };

		mockTransportEquipment1 = SetUpTransportEquipment("1", "2", mockSeals, mockGoodReferences);
		var mockTransportEquipment2 = SetUpTransportEquipment("2", "0", Enumerable.Empty<ISealCommon>(), Enumerable.Empty<ICommonGoodsReference>());
		var mockTransportEquipments = new IAESCommonTransportEquipment[] { mockTransportEquipment1.Object, mockTransportEquipment2.Object };
		mockConsignment.Setup(m => m.TransportEquipment).Returns(mockTransportEquipments);

		mockLocationOfGoods = SetUpLocationOfGoods(contactPerson);
		mockConsignment.Setup(m => m.LocationOfGoods).Returns(mockLocationOfGoods.Object);

		var mockDepartureTransportMean1 = SetUpDepartureTransportMeans("1");
		var mockDepartureTransportMean2 = SetUpDepartureTransportMeans("2");
		var mockDepartureTransportMeans = new ICommonDepartureTransportMeans[] { mockDepartureTransportMean1.Object, mockDepartureTransportMean2.Object };
		mockConsignment.Setup(m => m.DepartureTransportMeans).Returns(mockDepartureTransportMeans);

		return mockConsignment;
	}
	Mock<IAESCommonLocationOfGoods> mockLocationOfGoods;

	#endregion
}
