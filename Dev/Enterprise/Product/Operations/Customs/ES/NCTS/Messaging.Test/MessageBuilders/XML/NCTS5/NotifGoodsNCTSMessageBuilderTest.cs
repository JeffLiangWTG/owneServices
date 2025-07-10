using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC170C_v515.CC170CV1Ent;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders.Testing;

[TestedType(typeof(NotifGoodsNCTSMessageBuilder))]
class NotifGoodsNCTSMessageBuilderTest : NCTSCommonMessageBuilderTest<NotifGoodsNCTSMessageBuilder, INotifGoodsNCTSMessageDataProvider, Cc170Cv1Ent>
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

	public void TestPopulatePhaseID()
	{
		mockProvider.Setup(m => m.PhaseIDSpecified).Returns(false);
		var messageText = CreateMessageBuilder().GetSignedMessageText();
		AssertNotContains("PhaseID", messageText);
	}

	public void TestPopulateTransitOperation()
	{
		mockProvider.Setup(m => m.TransitOperation).Returns((INCTSCommonTransitOperationLRN)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}TransitOperation>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateCustomsOfficeOfDeparture()
	{
		mockProvider.Setup(m => m.CustomsOfficeOfDeparture).Returns("");
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}CustomsOfficeOfDeparture>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateHolderOfTheTransitProcedure()
	{
		mockProvider.Setup(m => m.HolderOfTheTransitProcedure).Returns((INCTSCommonHolderOfTheTransitProcedure)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}HolderOfTheTransitProcedure>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateRepresentative()
	{
		mockProvider.Setup(m => m.Representative).Returns((ICommonRepresentativeWithContactPerson)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Representative>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateContactPerson()
	{
		mockProvider.Setup(m => m.Representative.ContactPerson).Returns((IPartyContactProvider)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}ContactPerson>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateConsignment()
	{
		mockProvider.Setup(m => m.Consignment).Returns((INotifGoodsNCTSConsignment)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Consignment>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateTransportEquipment()
	{
		mockConsignment.Setup(m => m.TransportEquipment).Returns((IReadOnlyCollection<INCTSCommonTransportEquipment>)null);
		mockProvider.Setup(m => m.Consignment).Returns(mockConsignment.Object);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}TransportEquipment>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateSeals()
	{
		mockTransportEquipment1.Setup(m => m.Seals).Returns((IReadOnlyCollection<ISealCommon>)null);
		mockConsignment.Setup(m => m.TransportEquipment).Returns(new[] { mockTransportEquipment1.Object });
		mockProvider.Setup(m => m.Consignment).Returns(mockConsignment.Object);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Seal>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateGoodsReference()
	{
		mockTransportEquipment1.Setup(m => m.GoodsReference).Returns((IReadOnlyCollection<INCTSCommonGoodsReference>)null);
		mockConsignment.Setup(m => m.TransportEquipment).Returns(new[] { mockTransportEquipment1.Object });
		mockProvider.Setup(m => m.Consignment).Returns(mockConsignment.Object);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}GoodsReference>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateLocationOfGoods()
	{
		mockProvider.Setup(m => m.Consignment.LocationOfGoods).Returns((INCTSCommonLocationOfGoods)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}LocationOfGoods>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateDepartureTransportMeans()
	{
		mockConsignment.Setup(m => m.DepartureTransportMeans).Returns((IReadOnlyCollection<ICommonDepartureTransportMeans>)null);
		mockProvider.Setup(m => m.Consignment).Returns(mockConsignment.Object);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}DepartureTransportMeans>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateActiveBorderTransportMeans()
	{
		mockConsignment.Setup(m => m.ActiveBorderTransportMeans).Returns((IReadOnlyCollection<INCTSCommonActiveBorderTransportMeansWithOffice>)null);
		mockProvider.Setup(m => m.Consignment).Returns(mockConsignment.Object);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}ActiveBorderTransportMeans>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateConsignmentPlaceOfLoading()
	{
		mockProvider.Setup(m => m.Consignment.PlaceOfLoading).Returns((INCTSCommonPlace)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}PlaceOfLoading>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateHouseConsignment()
	{
		mockConsignment.Setup(m => m.HouseConsignment).Returns((IReadOnlyCollection<INCTSCommonHouseConsignment>)null);
		mockProvider.Setup(m => m.Consignment).Returns(mockConsignment.Object);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}HouseConsignment>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	#endregion

	protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.Ncts5DepartureNotification;

	protected override NotifGoodsNCTSMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new NotifGoodsNCTSMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

	protected override NotifGoodsNCTSMessageBuilder CreateMessageBuilderWithNullProvider() => new NotifGoodsNCTSMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

	protected override ZString GetTestFile() => GetTestFileContents(NCTSXMLTestFileConstants.NCTSTestFilePath, "TestNotifGoodsNCTS.txt");

	#region Structures SetUp

	protected override void SetUp()
	{
		base.SetUp();

		mockProvider.Setup(m => m.MessageSender).Returns("A78587268");
		mockProvider.Setup(m => m.MessageIdentification).Returns("20221213141220798000");
		mockProvider.Setup(m => m.IsFinalPeriod).Returns(true);
		mockProvider.Setup(m => m.PhaseIDSpecified).Returns(true);
		mockProvider.Setup(m => m.TransitOperation).Returns(SetUpTransitOperationLRN().Object);
		mockProvider.Setup(m => m.CustomsOfficeOfDeparture).Returns("ES000101");
		mockProvider.Setup(m => m.HolderOfTheTransitProcedure).Returns(SetUpCommonHolderOfTheTransitProcedure().Object);

		mockRepresentative = SetUpRepresentative();
		mockProvider.Setup(m => m.Representative).Returns(mockRepresentative.Object);

		mockTransportEquipment1 = SetUpTransportEquipment("1", "CSQU3054383", "2", SetUpSeals(), SetUpGoodsReference());
		mockConsignment = SetUpNotifGoodsConsignment();
		var mockHouseConsignment1 = SetUpCommonHouseConsignment("1");
		var mockHouseConsignment2 = SetUpCommonHouseConsignment("2");
		var mockHouseConsignment = new INCTSCommonHouseConsignmentSeqNum[] { mockHouseConsignment1.Object, mockHouseConsignment2.Object };
		mockConsignment.Setup(m => m.HouseConsignment).Returns(mockHouseConsignment);
		mockProvider.Setup(m => m.Consignment).Returns(mockConsignment.Object);
	}

	Mock<ICommonRepresentativeWithContactPerson> mockRepresentative;
	Mock<INotifGoodsNCTSConsignment> mockConsignment;
	Mock<INCTSCommonTransportEquipment> mockTransportEquipment1;

	protected Mock<INotifGoodsNCTSConsignment> SetUpNotifGoodsConsignment()
	{
		var mockConsignment = new Mock<INotifGoodsNCTSConsignment>();
		mockConsignment.Setup(m => m.ContainerIndicator).Returns(true);
		mockConsignment.Setup(m => m.InlandModeOfTransport).Returns("2");
		mockConsignment.Setup(m => m.ModeOfTransportAtTheBorder).Returns("3");

		mockTransportEquipment1 = SetUpTransportEquipment("1", "CSQU3054383", "2", SetUpSeals(), SetUpGoodsReference());
		var mockTransportEquipment2 = SetUpTransportEquipment("2", "CSQJ3054383", "0", Enumerable.Empty<ISealCommon>(), Enumerable.Empty<INCTSCommonGoodsReference>());
		var mockTransportEquipments = new INCTSCommonTransportEquipment[] { mockTransportEquipment1.Object, mockTransportEquipment2.Object };
		mockConsignment.Setup(m => m.TransportEquipment).Returns(mockTransportEquipments);

		mockConsignment.Setup(m => m.LocationOfGoods).Returns(SetUpLocationOfGoods().Object);

		var mockDepartureTransportMeans1 = SetUpDepartureTransportMeans("1", "20", "Transport", "ES");
		var mockDepartureTransportMeans2 = SetUpDepartureTransportMeans("2", "21", "Ship", "PT");
		var mockDepatureTransportMeans = new ICommonDepartureTransportMeans[] { mockDepartureTransportMeans1.Object, mockDepartureTransportMeans2.Object };
		mockConsignment.Setup(m => m.DepartureTransportMeans).Returns(mockDepatureTransportMeans);

		var mockActiveBorderTransportMeans1 = SetUpActiveBorderTransportMeansWithOffice("1", "ES004321", "21", "Transport", "ES", "Conveyance1");
		var mockActiveBorderTransportMeans2 = SetUpActiveBorderTransportMeansWithOffice("2", "ES004322", "22", "Ship", "PT", "Conveyance2");
		var mockActiveBorderTransportMeans = new INCTSCommonActiveBorderTransportMeansWithOffice[] { mockActiveBorderTransportMeans1.Object, mockActiveBorderTransportMeans2.Object };
		mockConsignment.Setup(m => m.ActiveBorderTransportMeans).Returns(mockActiveBorderTransportMeans);

		mockConsignment.Setup(m => m.PlaceOfLoading).Returns(SetUpCommonPlace("Code", "ES", "Madrid").Object);

		return mockConsignment;
	}

	#endregion
}
