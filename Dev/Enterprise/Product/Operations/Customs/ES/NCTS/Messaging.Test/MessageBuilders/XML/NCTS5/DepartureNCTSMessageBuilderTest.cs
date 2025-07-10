using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC015C_v515.CC015CV1Ent;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders.Testing;

[TestedType(typeof(DepartureNCTSMessageBuilder))]
class DepartureNCTSMessageBuilderTest : NCTSCommonMessageBuilderTest<DepartureNCTSMessageBuilder, IDepartureNCTSMessageDataProvider, Cc015Cv1Ent>
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

	public void TestGetXMLObjectForComparison()
	{
		CombineAssertions(() =>
		{
			var messageBuilder = CreateMessageBuilder();
			var comparable = messageBuilder.GetXMLObjectForComparison();
			AssertNotNull(comparable);
			AssertType<Cc015Cv1Ent>(comparable);
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
		mockProvider.Setup(m => m.TransitOperation).Returns((IDepartureNCTSTransitOperation)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}TransitOperation>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateAuthorisation()
	{
		mockProvider.Setup(m => m.Authorisations).Returns((IReadOnlyCollection<INCTSCommonAuthorisation>)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Authorisation>", CreateMessageBuilder().GetSignedMessageText());
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

	public void TestPopulateCustomsOfficeOfDestination()
	{
		mockProvider.Setup(m => m.CustomsOfficeOfDeparture).Returns("");
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}CustomsOfficeOfDestination>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateOfficeOfTransit()
	{
		mockProvider.Setup(m => m.CustomsOfficeOfTransitDeclared).Returns((IReadOnlyCollection<INCTSCommonCustomsOffice>)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}CustomsOfficeOfTransitDeclared>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateOfficeOfExit()
	{
		mockProvider.Setup(m => m.CustomsOfficeOfExitForTransitDeclared).Returns((IReadOnlyCollection<INCTSCommonCustomsOffice>)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}CustomsOfficeOfExitForTransitDeclared>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateHolderOfTheTransitProcedure()
	{
		mockProvider.Setup(m => m.HolderOfTheTransitProcedure).Returns((INCTSCompleteHolderOfTheTransitProcedure)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}HolderOfTheTransitProcedure>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateAddress()
	{
		mockProvider.Setup(m => m.HolderOfTheTransitProcedure.Address).Returns((INCTSCommonAddressInfo)null);
		mockProvider.Setup(m => m.Consignment.Consignor.Address).Returns((INCTSCommonAddressInfo)null);
		mockProvider.Setup(m => m.Consignment.CommonConsignmentData.Consignee.Address).Returns((INCTSCommonAddressInfo)null);
		mockHouseConsignment1.Setup(m => m.Consignor.Address).Returns((INCTSCommonAddressInfo)null);
		mockHouseConsignment1.Setup(m => m.Consignee.Address).Returns((INCTSCommonAddressInfo)null);
		mockLine1.Setup(m => m.Consignee.Address).Returns((INCTSCommonAddressInfo)null);
		mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new INCTSConsignmentItemDepartureAndAmendment[] { mockLine1.Object });
		mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new INCTSCommonHouseConsignmentDepartureAndAmendment[] { mockHouseConsignment1.Object });
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Address>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateContactPerson()
	{
		mockProvider.Setup(m => m.HolderOfTheTransitProcedure.ContactPerson).Returns((IPartyContactProvider)null);
		mockProvider.Setup(m => m.Representative.ContactPerson).Returns((IPartyContactProvider)null);
		mockProvider.Setup(m => m.Consignment.Carrier.ContactPerson).Returns((IPartyContactProvider)null);
		mockProvider.Setup(m => m.Consignment.Consignor.ContactPerson).Returns((IPartyContactProvider)null);
		mockHouseConsignment1.Setup(m => m.Consignor.ContactPerson).Returns((IPartyContactProvider)null);
		mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new INCTSCommonHouseConsignmentDepartureAndAmendment[] { mockHouseConsignment1.Object });
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}ContactPerson>", CreateMessageBuilder().GetSignedMessageText());
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

	public void TestPopulateGuarantee()
	{
		mockProvider.Setup(m => m.Guarantee).Returns((IReadOnlyCollection<INCTSCommonGuarantee>)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Guarantee>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateGuaranteeReference()
	{
		mockGuarantee1.Setup(m => m.GuaranteeReference).Returns((IReadOnlyCollection<INCTSCommonGuaranteeReference>)null);
		mockProvider.Setup(m => m.Guarantee).Returns(new INCTSCommonGuarantee[] { mockGuarantee1.Object });
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}GuaranteeReference>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateConsignment()
	{
		mockProvider.Setup(m => m.Consignment).Returns((INCTSCommonConsignmentDepartureAndAmendment)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Consignment>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateCarrier()
	{
		mockProvider.Setup(m => m.Consignment.Carrier).Returns((INCTSCommonCarrier)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Carrier>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateConsignor()
	{
		mockProvider.Setup(m => m.Consignment.Consignor).Returns((INCTSCommonConsignor)null);
		mockHouseConsignment1.Setup(m => m.Consignor).Returns((INCTSCommonConsignor)null);
		mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new INCTSCommonHouseConsignmentDepartureAndAmendment[] { mockHouseConsignment1.Object });
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Consignor>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateConsignee()
	{
		mockProvider.Setup(m => m.Consignment.CommonConsignmentData.Consignee).Returns((INCTSPartyNameProviderWithAddress)null);
		mockHouseConsignment1.Setup(m => m.Consignee).Returns((INCTSPartyNameProviderWithAddress)null);
		mockLine1.Setup(m => m.Consignee).Returns((INCTSPartyNameProviderWithAddress)null);
		mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new INCTSConsignmentItemDepartureAndAmendment[] { mockLine1.Object });
		mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new INCTSCommonHouseConsignmentDepartureAndAmendment[] { mockHouseConsignment1.Object });
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Consignee>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateAdditionalSupplyChainActor()
	{
		mockProvider.Setup(m => m.Consignment.AdditionalSupplyChainActor).Returns((IReadOnlyCollection<ICommonAdditionalSupplyChainActorSeqNum>)null);
		mockLine1.Setup(m => m.AdditionalSupplyChainActor).Returns((IReadOnlyCollection<ICommonAdditionalSupplyChainActorSeqNum>)null);
		mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new INCTSConsignmentItemDepartureAndAmendment[] { mockLine1.Object });
		mockHouseConsignment1.Setup(m => m.AdditionalSupplyChainActor).Returns((IReadOnlyCollection<ICommonAdditionalSupplyChainActorSeqNum>)null);
		mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new INCTSCommonHouseConsignmentDepartureAndAmendment[] { mockHouseConsignment1.Object });
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}AdditionalSupplyChainActor>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateTransportEquipment()
	{
		mockProvider.Setup(m => m.Consignment.TransportEquipment).Returns((IReadOnlyCollection<INCTSCommonTransportEquipment>)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}TransportEquipment>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateSeals()
	{
		mockTransportEquipment1.Setup(m => m.Seals).Returns((IReadOnlyCollection<ISealCommon>)null);
		mockProvider.Setup(m => m.Consignment.TransportEquipment).Returns(new INCTSCommonTransportEquipment[] { mockTransportEquipment1.Object });
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Seal>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateGoodsReference()
	{
		mockTransportEquipment1.Setup(m => m.GoodsReference).Returns((IReadOnlyCollection<INCTSCommonGoodsReference>)null);
		mockProvider.Setup(m => m.Consignment.TransportEquipment).Returns(new INCTSCommonTransportEquipment[] { mockTransportEquipment1.Object });
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
		mockProvider.Setup(m => m.Consignment.DepartureTransportMeans).Returns((IReadOnlyCollection<ICommonDepartureTransportMeans>)null);
		mockHouseConsignment1.Setup(m => m.DepartureTransportMeans).Returns((IReadOnlyCollection<ICommonDepartureTransportMeans>)null);
		mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new INCTSCommonHouseConsignmentDepartureAndAmendment[] { mockHouseConsignment1.Object });
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}DepartureTransportMeans>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateCountryOfRouting()
	{
		mockProvider.Setup(m => m.Consignment.CommonConsignmentData.CountryOfRoutingOfConsignment).Returns((IReadOnlyCollection<ICommonCountryOfRoutingOfConsignment>)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}CountryOfRoutingOfConsignment>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateActiveBorderTransportMeans()
	{
		mockProvider.Setup(m => m.Consignment.ActiveBorderTransportMeans).Returns((IReadOnlyCollection<INCTSCommonActiveBorderTransportMeansWithOffice>)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}ActiveBorderTransportMeans>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateConsignmentPlaceOfLoading()
	{
		mockProvider.Setup(m => m.Consignment.PlaceOfLoading).Returns((INCTSCommonPlace)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}PlaceOfLoading>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateConsignmentPlaceOfUnloading()
	{
		mockProvider.Setup(m => m.Consignment.PlaceOfUnloading).Returns((INCTSCommonPlace)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}PlaceOfUnloading>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateTransportCharges()
	{
		mockProvider.Setup(m => m.Consignment.CommonConsignmentData.MethodOfPayment).Returns("");
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}TransportCharges>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateHouseConsignment()
	{
		mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns((IReadOnlyCollection<INCTSCommonHouseConsignmentDepartureAndAmendment>)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}HouseConsignment>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateConsignmentItem()
	{
		mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns((IReadOnlyCollection<INCTSConsignmentItemDepartureAndAmendment>)null);
		mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new INCTSCommonHouseConsignmentDepartureAndAmendment[] { mockHouseConsignment1.Object });
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}ConsignmentItem>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateCommodity()
	{
		mockLine1.Setup(m => m.Commodity).Returns((INCTSCommodityDepartureAndAmendment)null);
		mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new INCTSConsignmentItemDepartureAndAmendment[] { mockLine1.Object });
		mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new INCTSCommonHouseConsignmentDepartureAndAmendment[] { mockHouseConsignment1.Object });
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Commodity>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateCommodityCode()
	{
		mockLine1.Setup(m => m.Commodity.CommodityCode).Returns((INCTSCommonCommodityCode)null);
		mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new INCTSConsignmentItemDepartureAndAmendment[] { mockLine1.Object });
		mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new INCTSCommonHouseConsignmentDepartureAndAmendment[] { mockHouseConsignment1.Object });
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}CommodityCode>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateDangerousGoods()
	{
		mockLine1.Setup(m => m.Commodity.DangerousGoods).Returns((IReadOnlyCollection<ICommonDangerousGoods>)null);
		mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new INCTSConsignmentItemDepartureAndAmendment[] { mockLine1.Object });
		mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new INCTSCommonHouseConsignmentDepartureAndAmendment[] { mockHouseConsignment1.Object });
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}DangerousGoods>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateGoodsMeasure()
	{
		mockLine1.Setup(m => m.Commodity.GoodsMeasure).Returns((INCTSGoodsMeasureDepartureAndAmendment)null);
		mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new INCTSConsignmentItemDepartureAndAmendment[] { mockLine1.Object });
		mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new INCTSCommonHouseConsignmentDepartureAndAmendment[] { mockHouseConsignment1.Object });
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}GoodsMeasure>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateSupplementaryUnits()
	{
		mockLine1.Setup(m => m.Commodity.GoodsMeasure.SupplementaryUnitsSpecified).Returns(false);
		mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new INCTSConsignmentItemDepartureAndAmendment[] { mockLine1.Object });
		mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new INCTSCommonHouseConsignmentDepartureAndAmendment[] { mockHouseConsignment1.Object });
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}GoodsMeasure>", CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}supplementaryUnits>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateGrossMass()
	{
		mockLine1.Setup(m => m.Commodity.GoodsMeasure.GrossMassSpecified).Returns(false);
		mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new INCTSConsignmentItemDepartureAndAmendment[] { mockLine1.Object });
		mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new INCTSCommonHouseConsignmentDepartureAndAmendment[] { mockHouseConsignment1.Object });
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}GoodsMeasure>", CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}GoodsMeasure>\r\n              <grossMass>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateNetMass()
	{
		mockLine1.Setup(m => m.Commodity.GoodsMeasure.NetMassSpecified).Returns(false);
		mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new INCTSConsignmentItemDepartureAndAmendment[] { mockLine1.Object });
		mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new INCTSCommonHouseConsignmentDepartureAndAmendment[] { mockHouseConsignment1.Object });
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}GoodsMeasure>", CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}netMass>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulatePackaging()
	{
		mockLine1.Setup(m => m.Packaging).Returns((IReadOnlyCollection<INCTSCommonPackaging>)null);
		mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new INCTSConsignmentItemDepartureAndAmendment[] { mockLine1.Object });
		mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new INCTSCommonHouseConsignmentDepartureAndAmendment[] { mockHouseConsignment1.Object });
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Packaging>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulatePreviousDocument()
	{
		mockHouseConsignment1.Setup(m => m.PreviousDocument).Returns((IReadOnlyCollection<INCTSCommonDocumentWithInfo>)null);
		mockLine1.Setup(m => m.PreviousDocument).Returns((IReadOnlyCollection<INCTSCommonPreviousDocument>)null);
		mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new INCTSConsignmentItemDepartureAndAmendment[] { mockLine1.Object });
		mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new INCTSCommonHouseConsignmentDepartureAndAmendment[] { mockHouseConsignment1.Object });
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}PreviousDocument>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateSupportingDocument()
	{
		mockProvider.Setup(m => m.Consignment.CommonConsignmentData.SupportingDocument).Returns((IReadOnlyCollection<INCTSCommonDocumentWithItem>)null);
		mockHouseConsignment1.Setup(m => m.SupportingDocument).Returns((IReadOnlyCollection<INCTSCommonDocumentWithItem>)null);
		mockLine1.Setup(m => m.SupportingDocument).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
		mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new INCTSConsignmentItemDepartureAndAmendment[] { mockLine1.Object });
		mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new INCTSCommonHouseConsignmentDepartureAndAmendment[] { mockHouseConsignment1.Object });
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}SupportingDocument>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateTransportDocument()
	{
		mockProvider.Setup(m => m.Consignment.CommonConsignmentData.TransportDocument).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
		mockHouseConsignment1.Setup(m => m.TransportDocument).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
		mockLine1.Setup(m => m.TransportDocument).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
		mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new INCTSConsignmentItemDepartureAndAmendment[] { mockLine1.Object });
		mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new INCTSCommonHouseConsignmentDepartureAndAmendment[] { mockHouseConsignment1.Object });
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}TransportDocument>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateAdditionalReference()
	{
		mockProvider.Setup(m => m.Consignment.CommonConsignmentData.AdditionalReference).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
		mockHouseConsignment1.Setup(m => m.AdditionalReference).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
		mockLine1.Setup(m => m.AdditionalReference).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
		mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new INCTSConsignmentItemDepartureAndAmendment[] { mockLine1.Object });
		mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new INCTSCommonHouseConsignmentDepartureAndAmendment[] { mockHouseConsignment1.Object });
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}AdditionalReference>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateAdditionalInfo()
	{
		mockProvider.Setup(m => m.Consignment.CommonConsignmentData.AdditionalInformation).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
		mockHouseConsignment1.Setup(m => m.AdditionalInformation).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
		mockLine1.Setup(m => m.AdditionalInformation).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
		mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new INCTSConsignmentItemDepartureAndAmendment[] { mockLine1.Object });
		mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new INCTSCommonHouseConsignmentDepartureAndAmendment[] { mockHouseConsignment1.Object });
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}AdditionalInformation>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	#endregion

	protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.Ncts5Departure;

	protected override DepartureNCTSMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new DepartureNCTSMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

	protected override DepartureNCTSMessageBuilder CreateMessageBuilderWithNullProvider() => new DepartureNCTSMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

	protected override ZString GetTestFile() => GetTestFileContents(NCTSXMLTestFileConstants.NCTSTestFilePath, "TestDepartureNCTS.txt");

	#region Structures SetUp

	protected override void SetUp()
	{
		base.SetUp();

		mockProvider.Setup(m => m.MessageSender).Returns("A78587268");
		mockProvider.Setup(m => m.MessageIdentification).Returns("20221213141220798000");
		mockProvider.Setup(m => m.IsFinalPeriod).Returns(true);
		mockProvider.Setup(m => m.PhaseIDSpecified).Returns(true);
		mockProvider.Setup(m => m.TransitOperation).Returns(SetUpDepartureTransitOperation().Object);

		var mockAuthorisation1 = SetUpCommonAuthorisation("1", "ACR", "000001");
		var mockAuthorisation2 = SetUpCommonAuthorisation("2", "SSE", "000002");
		var mockAuthorisations = new INCTSCommonAuthorisation[] { mockAuthorisation1.Object, mockAuthorisation2.Object };
		mockProvider.Setup(m => m.Authorisations).Returns(mockAuthorisations);

		mockProvider.Setup(m => m.CustomsOfficeOfDeparture).Returns("ES000101");
		mockProvider.Setup(m => m.CustomsOfficeOfDestinationDeclared).Returns("CH006251");

		var mockCustomsOffice1 = SetUpCommonCustomsOffice("1", "CH006251");
		var mockCustomsOffice2 = SetUpCommonCustomsOffice("2", "CH006252");
		var mockCustomsOffice3 = SetUpCommonCustomsOffice("3", ZString.Empty);
		var mockCustomsOffices = new INCTSCommonCustomsOffice[] { mockCustomsOffice1.Object, mockCustomsOffice2.Object, mockCustomsOffice3.Object };
		mockProvider.Setup(m => m.CustomsOfficeOfTransitDeclared).Returns(mockCustomsOffices);
		mockProvider.Setup(m => m.CustomsOfficeOfExitForTransitDeclared).Returns(mockCustomsOffices);

		mockProvider.Setup(m => m.HolderOfTheTransitProcedure).Returns(SetUpHolderOfTheTransitProcedure().Object);
		mockProvider.Setup(m => m.Representative).Returns(SetUpRepresentative().Object);

		var mockGuaranteeReference1 = SetUpGuaranteeReference("1", "GRN1234567890", "0123", 55.251m);
		var mockGuaranteeReference2 = SetUpGuaranteeReference("2", "GRN1234567890", "4567", 105.951m);
		var mockGuaranteeReferences = new INCTSCommonGuaranteeReference[] { mockGuaranteeReference1.Object, mockGuaranteeReference2.Object };

		mockGuarantee1 = SetUpGuarantee("1", "6", mockGuaranteeReferences);
		var mockGuarantee2 = SetUpGuarantee("2", "6", Enumerable.Empty<INCTSCommonGuaranteeReference>());
		var mockGuarantees = new INCTSCommonGuarantee[] { mockGuarantee1.Object, mockGuarantee2.Object };
		mockProvider.Setup(m => m.Guarantee).Returns(mockGuarantees);

		mockConsignment = SetUpDepartureConsignment();
		mockProvider.Setup(m => m.Consignment).Returns(mockConsignment.Object);
	}

	protected Mock<IDepartureNCTSTransitOperation> SetUpDepartureTransitOperation()
	{
		var mockTransitOperation = new Mock<IDepartureNCTSTransitOperation>();
		mockTransitOperation.Setup(m => m.LRN).Returns("20220804120632");
		mockTransitOperation.Setup(m => m.CommonTransitOperation).Returns(SetUpTransitOperationCommonComplete().Object);
		return mockTransitOperation;
	}

	Mock<INCTSCommonConsignmentDepartureAndAmendment> mockConsignment;
	Mock<INCTSCommonGuarantee> mockGuarantee1;
	Mock<INCTSCommonTransportEquipment> mockTransportEquipment1;
	Mock<INCTSCommonHouseConsignmentDepartureAndAmendment> mockHouseConsignment1;
	Mock<INCTSConsignmentItemDepartureAndAmendment> mockLine1;
	Mock<INCTSCommodityDepartureAndAmendment> mockCommodity;

	protected Mock<INCTSCommonConsignmentDepartureAndAmendment> SetUpDepartureConsignment()
	{
		var mockConsignment = new Mock<INCTSCommonConsignmentDepartureAndAmendment>();
		mockConsignment.Setup(m => m.ContainerIndicator).Returns(ZBool.True);
		mockConsignment.Setup(m => m.InlandModeOfTransport).Returns("2");
		mockConsignment.Setup(m => m.ModeOfTransportAtTheBorder).Returns("3");
		mockConsignment.Setup(m => m.Carrier).Returns(SetUpCommonCarrier().Object);
		mockConsignment.Setup(m => m.Consignor).Returns(SetUpCommonConsignor().Object);

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
		mockConsignment.Setup(m => m.PlaceOfUnloading).Returns(SetUpCommonPlace("Code", "PT", "Lisbon").Object);

		var mockSupportingDocConsigment1 = SetUpCommonDocumentWithItem("1", "N821", "DocRef1", "Complement1", "1");
		var mockSupportingDocConsigment2 = SetUpCommonDocumentWithItem("2", "N380", "DocRef2", "Complement2", "2");
		var mockSupportingDocsConsigment = new INCTSCommonDocumentWithItem[] { mockSupportingDocConsigment1.Object, mockSupportingDocConsigment2.Object };

		var mockTransportDoc1 = SetUpCommonDocument("1", "N785", "DocRef3");
		var mockTransportDoc2 = SetUpCommonDocument("2", "N705", "DocRef4");
		var mockTransportDocs = new ICommonDocumentSequenceNumber[] { mockTransportDoc1.Object, mockTransportDoc2.Object };

		var mockAdditionalRef1 = SetUpCommonDocument("1", "Y025", "DocRef5");
		var mockAdditionalRef2 = SetUpCommonDocument("2", "Y026", "DocRef6");
		var mockAdditionalRefs = new ICommonDocumentSequenceNumber[] { mockAdditionalRef1.Object, mockAdditionalRef2.Object };

		var mockAdditionalInfo1 = SetUpCommonDocument("1", "20100", "Text1");
		var mockAdditionalInfo2 = SetUpCommonDocument("2", "20200", "Text2");
		var mockAdditionalInfos = new ICommonDocumentSequenceNumber[] { mockAdditionalInfo1.Object, mockAdditionalInfo2.Object };

		var mockCommonConsignmentData = new Mock<INCTSCommonConsignmentDepartureAndAmendmentAndTNN>();
		mockCommonConsignmentData.Setup(m => m.CountryOfDispatch).Returns("PT");
		mockCommonConsignmentData.Setup(m => m.CountryOfDestination).Returns("ES");
		mockCommonConsignmentData.Setup(m => m.GrossMass).Returns(60.204m);
		mockCommonConsignmentData.Setup(m => m.ReferenceNumberUCR).Returns("UCR");
		mockCommonConsignmentData.Setup(m => m.Consignee).Returns(SetUpNCTSPartyNameProviderWithAddress().Object);
		mockCommonConsignmentData.Setup(m => m.SupportingDocument).Returns(mockSupportingDocsConsigment);
		mockCommonConsignmentData.Setup(m => m.TransportDocument).Returns(mockTransportDocs);
		mockCommonConsignmentData.Setup(m => m.AdditionalReference).Returns(mockAdditionalRefs);
		mockCommonConsignmentData.Setup(m => m.AdditionalInformation).Returns(mockAdditionalInfos);

		var mockCountryOfRouting1 = SetUpCommonCountryOfRouting("1", "ES");
		var mockCountryOfRouting2 = SetUpCommonCountryOfRouting("2", "PT");
		var mockCountriesOfRouting = new ICommonCountryOfRoutingOfConsignment[] { mockCountryOfRouting1.Object, mockCountryOfRouting2.Object };
		mockCommonConsignmentData.Setup(m => m.CountryOfRoutingOfConsignment).Returns(mockCountriesOfRouting);
		mockCommonConsignmentData.Setup(m => m.MethodOfPayment).Returns("A");
		mockConsignment.Setup(m => m.CommonConsignmentData).Returns(mockCommonConsignmentData.Object);

		var mockActor1 = SetUpCommonAdditionalSupplyChainActor("1", "CS", "ES89890001K");
		var mockActor2 = SetUpCommonAdditionalSupplyChainActor("2", "FW", "ES89890001B");
		var mockActors = new ICommonAdditionalSupplyChainActorSeqNum[] { mockActor1.Object, mockActor2.Object };
		mockConsignment.Setup(m => m.AdditionalSupplyChainActor).Returns(mockActors);

		mockCommodity = SetUpCommodity();

		var mockPackage1 = SetUpCommonPackages("1", "NE", "16", "Mark1");
		var mockPackage2 = SetUpCommonPackages("2", "BX", "1", "Mark2");
		var mockPackages = new INCTSCommonPackaging[] { mockPackage1.Object, mockPackage2.Object };

		var mockPreviousDocHouse1 = SetUpCommonDocumentWithInfo("1", "EQV0", "20220726", "INFO");
		var mockPreviousDocHouse2 = SetUpCommonDocumentWithInfo("2", "N380", "Number", "INFO2");
		var mockPreviousDocsHouse = new INCTSCommonDocumentWithInfo[] { mockPreviousDocHouse1.Object, mockPreviousDocHouse2.Object };

		var mockPreviousDoc1 = SetUpCommonPreviousDocument("1", "EQV0", "20220726", "1", "UNDF", 16.2012m, true, "INFO");
		var mockPreviousDoc2 = SetUpCommonPreviousDocument("2", "N380", "Number", ZString.Empty, "FDNU", 1.00m, false, "INFO2");
		var mockPreviousDocs = new INCTSCommonPreviousDocument[] { mockPreviousDoc1.Object, mockPreviousDoc2.Object };

		var mockSupportingDoc1 = SetUpCommonDocument("1", "N821", "DocRef1");
		var mockSupportingDoc2 = SetUpCommonDocument("2", "N380", "DocRef2");
		var mockSupportingDocs = new ICommonDocumentSequenceNumber[] { mockSupportingDoc1.Object, mockSupportingDoc2.Object };

		mockLine1 = SetUpCommonLine("1", "1", "T1", "PT", "ES", "UCR", mockActors, mockPackages, mockPreviousDocs, mockSupportingDocs, mockTransportDocs, mockAdditionalRefs, mockAdditionalInfos,
			SetUpNCTSPartyNameProviderWithAddress().Object, mockCommodity.Object);
		var mockLine2 = SetUpCommonLine("2", "2", "T2", "FR", "IT", "UCA", Enumerable.Empty<ICommonAdditionalSupplyChainActorSeqNum>(), Enumerable.Empty<INCTSCommonPackaging>(),
			Enumerable.Empty<INCTSCommonPreviousDocument>(), Enumerable.Empty<ICommonDocumentSequenceNumber>(), Enumerable.Empty<ICommonDocumentSequenceNumber>(), Enumerable.Empty<ICommonDocumentSequenceNumber>(), Enumerable.Empty<ICommonDocumentSequenceNumber>());
		var mockLines = new INCTSConsignmentItemDepartureAndAmendment[] { mockLine1.Object, mockLine2.Object };

		mockHouseConsignment1 = SetUpCommonHouseConsignmentDepartureAndAmendment("1", 60.204m, "PT", "ES", "UCRHC1", mockActors, mockLines, mockDepatureTransportMeans, mockPreviousDocsHouse, mockSupportingDocsConsigment, mockTransportDocs, mockAdditionalRefs, mockAdditionalInfos, SetUpCommonConsignor().Object, SetUpNCTSPartyNameProviderWithAddress().Object);
		var mockHouseConsignment2 = SetUpCommonHouseConsignmentDepartureAndAmendment("2", 10.000m, ZString.Empty, ZString.Empty, ZString.Empty, Enumerable.Empty<ICommonAdditionalSupplyChainActorSeqNum>(), Enumerable.Empty<INCTSConsignmentItemDepartureAndAmendment>(), Enumerable.Empty<ICommonDepartureTransportMeans>(), Enumerable.Empty<INCTSCommonDocumentWithInfo>(), Enumerable.Empty<INCTSCommonDocumentWithItem>(), Enumerable.Empty<ICommonDocumentSequenceNumber>(), Enumerable.Empty<ICommonDocumentSequenceNumber>(), Enumerable.Empty<ICommonDocumentSequenceNumber>());
		var mockHouseConsignments = new INCTSCommonHouseConsignmentDepartureAndAmendment[] { mockHouseConsignment1.Object, mockHouseConsignment2.Object };
		mockConsignment.Setup(m => m.HouseConsignment).Returns(mockHouseConsignments);

		return mockConsignment;
	}

	#endregion
}
