using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CC513C_v514.CC513CV1Ent;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;

[TestedType(typeof(AmendmentAESMessageBuilder))]
public class AmendmentAESMessageBuilderTest : DeclarationAESCommonMessageBuilderTest<AmendmentAESMessageBuilder, IAmendmentAESMessageDataProvider, Cc513Cv1Ent, IAmendmentAESExportOperation>
{
	protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.ExportAmendmentUcc6;

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

	public override void TestPopulateExportOperation()
	{
		mockProvider.Setup(m => m.ExportOperation).Returns((IAmendmentAESExportOperation)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public override void TestPopulateAuthorisation()
	{
		mockLine1.Setup(m => m.Authorisations).Returns((IReadOnlyCollection<ICommonAuthorisation>)null);
		mockGoodsShipment.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		mockProvider.Setup(m => m.Authorisations).Returns((IReadOnlyCollection<ICommonAuthorisation>)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @$"</{XMLTestFileConstants.XmlElementNamespace}ExportOperation>
    <{XMLTestFileConstants.XmlElementNamespace}CustomsOfficeOfPresentation>";
		var expectedResultLine = @$"</{XMLTestFileConstants.XmlElementNamespace}referenceNumberUCR>
        <{XMLTestFileConstants.XmlElementNamespace}Procedure>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
			AssertContains(expectedResultLine, messageText);
		});
	}

	public override void TestPopulateCustomsOffices()
	{
		mockProvider.Setup(m => m.CustomOfficeOfPresentation).Returns(ZString.Empty);
		mockProvider.Setup(m => m.CustomOfficeOfExport).Returns(ZString.Empty);
		mockProvider.Setup(m => m.CustomOfficeOfExit).Returns(ZString.Empty);

		mockLocationOfGoods.Setup(m => m.LocationCustomOffice).Returns(ZString.Empty);
		mockConsignment.Setup(m => m.LocationOfGoods).Returns(mockLocationOfGoods.Object);
		mockGoodsShipment.Setup(m => m.Consignment).Returns(mockConsignment.Object);
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);

		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public override void TestPopulateExporter()
	{
		mockProvider.Setup(m => m.Exporter).Returns((IDeclarationAESExporter)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public override void TestPopulateDeclarant()
	{
		mockProvider.Setup(m => m.Declarant).Returns((IPartyIdProviderWithContactPerson)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public override void TestPopulateRepresentative()
	{
		mockProvider.Setup(m => m.Representative).Returns((ICommonRepresentativeWithContactPerson)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public override void TestPopulateGoodsShipment()
	{
		mockProvider.Setup(m => m.GoodsShipment).Returns((IDeclarationAESGoodsShipment)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public override void TestPopulateAdditionalSupplyChainActor()
	{
		mockLine1.Setup(m => m.AdditionalSupplyActors).Returns((IReadOnlyCollection<ICommonAdditionalSupplyChainActorSeqNum>)null);
		mockGoodsShipment.Setup(m => m.AdditionalSupplyActors).Returns((IReadOnlyCollection<ICommonAdditionalSupplyChainActorSeqNum>)null);
		mockGoodsShipment.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);

		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @$"</{XMLTestFileConstants.XmlElementNamespace}countryOfDestination>
      <{XMLTestFileConstants.XmlElementNamespace}DeliveryTerms>";
		var expectedResultLine = @$"</{XMLTestFileConstants.XmlElementNamespace}Consignee>
        <{XMLTestFileConstants.XmlElementNamespace}Origin>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
			AssertContains(expectedResultLine, messageText);
		});
	}

	public override void TestPopulateDeliveryTerms()
	{
		mockProvider.Setup(m => m.GoodsShipment.DeliveryTerms).Returns((ICommonDeliveryTerms)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public override void TestPopulateWarehouse()
	{
		mockProvider.Setup(m => m.GoodsShipment.Warehouse).Returns((IWarehouseCommon)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public override void TestPopulateConsignment()
	{
		mockProvider.Setup(m => m.GoodsShipment.Consignment).Returns((IDeclarationAESConsignment)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public override void TestPopulateCarrier()
	{
		mockProvider.Setup(m => m.GoodsShipment.Consignment.Carrier).Returns((IPartyIdProvider)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public override void TestPopulateConsignor()
	{
		mockLine1.Setup(m => m.Consignor).Returns((IPartyProvider)null);
		mockGoodsShipment.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);

		mockProvider.Setup(m => m.GoodsShipment.Consignment.Consignor).Returns((IPartyIdProvider)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public override void TestPopulateConsignee()
	{
		mockLine1.Setup(m => m.Consignee).Returns((IDeclarationAESConsignee)null);
		mockGoodsShipment.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);

		mockProvider.Setup(m => m.GoodsShipment.Consignment.Consignee).Returns((IDeclarationAESConsignee)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public override void TestPopulateTransportEquipment()
	{
		mockConsignment.Setup(m => m.TransportEquipment).Returns((IReadOnlyCollection<IAESCommonTransportEquipment>)null);
		mockGoodsShipment.Setup(m => m.Consignment).Returns(mockConsignment.Object);
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @$"</{XMLTestFileConstants.XmlElementNamespace}Consignee>
        <{XMLTestFileConstants.XmlElementNamespace}LocationOfGoods>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
		});
	}

	public override void TestPopulateSeal()
	{
		mockTransportEquipment1.Setup(m => m.Seals).Returns((IReadOnlyCollection<ISealCommon>)null);
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

	public override void TestPopulateGoodsReference()
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

	public override void TestPopulateLocationOfGoods()
	{
		mockProvider.Setup(m => m.GoodsShipment.Consignment.LocationOfGoods).Returns((IAESCommonLocationOfGoods)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public override void TestPopulateLocationGNSS()
	{
		mockProvider.Setup(m => m.GoodsShipment.Consignment.LocationOfGoods.LocationGNSS).Returns((ICommonGNSS)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public override void TestPopulateLocationEconomicOperator()
	{
		mockLocationOfGoods.Setup(m => m.LocationEconomicOperatorId).Returns(ZString.Empty);
		mockConsignment.Setup(m => m.LocationOfGoods).Returns(mockLocationOfGoods.Object);
		mockGoodsShipment.Setup(m => m.Consignment).Returns(mockConsignment.Object);
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public override void TestPopulateLocationLocationAddress()
	{
		mockProvider.Setup(m => m.GoodsShipment.Consignment.LocationOfGoods.LocationAddress).Returns((IPartyAddressProvider)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public override void TestPopulateLocationLocationPostcodeAddress()
	{
		mockProvider.Setup(m => m.GoodsShipment.Consignment.LocationOfGoods.LocationPostcodeAddress).Returns((ICommonPostcodeAddress)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public override void TestPopulateContactPerson()
	{
		mockProvider.Setup(m => m.Declarant.ContactPerson).Returns((IPartyContactProvider)null);
		mockProvider.Setup(m => m.Representative.ContactPerson).Returns((IPartyContactProvider)null);
		mockProvider.Setup(m => m.GoodsShipment.Consignment.LocationOfGoods.LocationContactPerson).Returns((IPartyContactProvider)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public override void TestPopulateDepartureTransportMeans()
	{
		mockConsignment.Setup(m => m.DepartureTransportMeans).Returns((IReadOnlyCollection<ICommonDepartureTransportMeans>)null);
		mockGoodsShipment.Setup(m => m.Consignment).Returns(mockConsignment.Object);
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @$"</{XMLTestFileConstants.XmlElementNamespace}LocationOfGoods>
        <{XMLTestFileConstants.XmlElementNamespace}CountryOfRoutingOfConsignment>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
		});
	}

	public override void TestPopulateCountryOfRoutingOfConsignment()
	{
		mockConsignment.Setup(m => m.CountryOfRoutingOfConsignments).Returns((IReadOnlyCollection<ICommonCountryOfRoutingOfConsignment>)null);
		mockGoodsShipment.Setup(m => m.Consignment).Returns(mockConsignment.Object);
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @$"</{XMLTestFileConstants.XmlElementNamespace}DepartureTransportMeans>
        <{XMLTestFileConstants.XmlElementNamespace}ActiveBorderTransportMeans>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
		});
	}

	public override void TestPopulateActiveBorderTransportMeans()
	{
		mockProvider.Setup(m => m.GoodsShipment.Consignment.ActiveBorderTransportMeans).Returns((ITransportMediumInfoCommon)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public override void TestPopulateTransportCharges()
	{
		mockConsignment.Setup(m => m.TransportChargesMoP).Returns(ZString.Empty);
		mockGoodsShipment.Setup(m => m.Consignment).Returns(mockConsignment.Object);
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public override void TestPopulateLine()
	{
		mockGoodsShipment.Setup(m => m.Lines).Returns((IReadOnlyCollection<IDeclarationAESLine>)null);
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @$"</{XMLTestFileConstants.XmlElementNamespace}Consignment>
    </{XMLTestFileConstants.XmlElementNamespace}GoodsShipment>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
		});
	}

	public override void TestPopulateProcedure()
	{
		mockLine1.Setup(m => m.Procedure).Returns((IDeclarationAESProcedure)null);
		mockGoodsShipment.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @$"</{XMLTestFileConstants.XmlElementNamespace}Authorisation>
        <{XMLTestFileConstants.XmlElementNamespace}Consignor>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
		});
	}

	public override void TestPopulateAdditionalProcedure()
	{
		mockProcedure.Setup(m => m.AdditionalProcedures).Returns((IReadOnlyCollection<ICommonAdditionalCode>)null);
		mockLine1.Setup(m => m.Procedure).Returns(mockProcedure.Object);
		mockGoodsShipment.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = $@"</{XMLTestFileConstants.XmlElementNamespace}previousProcedure>
        </{XMLTestFileConstants.XmlElementNamespace}Procedure>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
		});
	}

	public override void TestPopulateOrigin()
	{
		mockLine1.Setup(m => m.Origin).Returns((IAESCommonOrigin)null);
		mockGoodsShipment.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public override void TestPopulateCommodity()
	{
		mockLine1.Setup(m => m.Commodity).Returns((IDeclarationAESCommodity)null);
		mockGoodsShipment.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public override void TestPopulateCommodityCode()
	{
		mockCommodity.Setup(m => m.CommodityCode).Returns((IDeclarationAESCommodityCode)null);
		mockLine1.Setup(m => m.Commodity).Returns(mockCommodity.Object);
		mockGoodsShipment.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public override void TestPopulateTARICAdditionalCodes()
	{
		mockCommodityCode.Setup(m => m.TariffAdditionalCodes).Returns((IReadOnlyCollection<ICommonAdditionalCode>)null);
		mockCommodity.Setup(m => m.CommodityCode).Returns(mockCommodityCode.Object);
		mockLine1.Setup(m => m.Commodity).Returns(mockCommodity.Object);
		mockGoodsShipment.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @$"</{XMLTestFileConstants.XmlElementNamespace}combinedNomenclatureCode>
            <{XMLTestFileConstants.XmlElementNamespace}NationalAdditionalCode>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
		});
	}

	public override void TestPopulateNationalAdditionalCodes()
	{
		mockCommodityCode.Setup(m => m.NationalAdditionalCodes).Returns((IReadOnlyCollection<ICommonAdditionalCode>)null);
		mockCommodity.Setup(m => m.CommodityCode).Returns(mockCommodityCode.Object);
		mockLine1.Setup(m => m.Commodity).Returns(mockCommodity.Object);
		mockGoodsShipment.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @$"</{XMLTestFileConstants.XmlElementNamespace}TARICAdditionalCode>
          </{XMLTestFileConstants.XmlElementNamespace}CommodityCode>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
		});
	}

	public override void TestPopulateDangerousGood()
	{
		mockCommodity.Setup(m => m.DangerousGoods).Returns((IReadOnlyCollection<ICommonDangerousGoods>)null);
		mockLine1.Setup(m => m.Commodity).Returns(mockCommodity.Object);
		mockGoodsShipment.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @$"</{XMLTestFileConstants.XmlElementNamespace}CommodityCode>
          <{XMLTestFileConstants.XmlElementNamespace}GoodsMeasure>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
		});
	}

	public override void TestPopulateGoodsMeasure()
	{
		mockCommodity.Setup(m => m.GoodsMeasure).Returns((ICommonGoodsMeasureWithSupUnitsAndSpecified)null);
		mockLine1.Setup(m => m.Commodity).Returns(mockCommodity.Object);
		mockGoodsShipment.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public override void TestPopulatePackage()
	{
		mockLine1.Setup(m => m.InternalPackages).Returns((IReadOnlyCollection<ICommonPackageWithSequenceAndPackNum>)null);
		mockGoodsShipment.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @$"</{XMLTestFileConstants.XmlElementNamespace}Commodity>
        <{XMLTestFileConstants.XmlElementNamespace}PreviousDocument>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
		});
	}

	public override void TestPopulatePreviousDocument()
	{
		mockLine1.Setup(m => m.PreviousDocuments).Returns((IReadOnlyCollection<IAESCommonDocument>)null);
		mockGoodsShipment.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @$"</{XMLTestFileConstants.XmlElementNamespace}Packaging>
        <{XMLTestFileConstants.XmlElementNamespace}SupportingDocument>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
		});
	}

	public override void TestPopulateSupportingDocument()
	{
		mockLine1.Setup(m => m.SupportingDocuments).Returns((IReadOnlyCollection<IDeclarationAESSupportingDocumentLine>)null);
		mockGoodsShipment.Setup(m => m.SupportingDocuments).Returns((IReadOnlyCollection<IDeclarationAESSupportingDocumentHeader>)null);
		mockGoodsShipment.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @$"</{XMLTestFileConstants.XmlElementNamespace}Warehouse>
      <{XMLTestFileConstants.XmlElementNamespace}AdditionalReference>";
		var expectedResultLine = @$"</{XMLTestFileConstants.XmlElementNamespace}PreviousDocument>
        <{XMLTestFileConstants.XmlElementNamespace}TransportDocument>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
			AssertContains(expectedResultLine, messageText);
		});
	}

	public override void TestPopulateTransportDocument()
	{
		mockLine1.Setup(m => m.TransportDocuments).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
		mockConsignment.Setup(m => m.TransportDocuments).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
		mockGoodsShipment.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
		mockGoodsShipment.Setup(m => m.Consignment).Returns(mockConsignment.Object);
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @$"</{XMLTestFileConstants.XmlElementNamespace}ActiveBorderTransportMeans>
        <{XMLTestFileConstants.XmlElementNamespace}TransportCharges>";
		var expectedResultLine = @$"</{XMLTestFileConstants.XmlElementNamespace}SupportingDocument>
        <{XMLTestFileConstants.XmlElementNamespace}AdditionalReference>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
			AssertContains(expectedResultLine, messageText);
		});
	}

	public override void TestPopulateAdditionalReference()
	{
		mockLine1.Setup(m => m.AdditionalReferences).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
		mockGoodsShipment.Setup(m => m.AdditionalReferences).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
		mockGoodsShipment.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = $@"</{XMLTestFileConstants.XmlElementNamespace}SupportingDocument>
      <{XMLTestFileConstants.XmlElementNamespace}AdditionalInformation>";
		var expectedResultLine = $@"</{XMLTestFileConstants.XmlElementNamespace}TransportDocument>
        <{XMLTestFileConstants.XmlElementNamespace}AdditionalInformation>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
			AssertContains(expectedResultLine, messageText);
		});
	}

	public override void TestPopulateAdditionalInfo()
	{
		mockLine1.Setup(m => m.AdditionalInfos).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
		mockGoodsShipment.Setup(m => m.AdditionalInfos).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
		mockGoodsShipment.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @$"</{XMLTestFileConstants.XmlElementNamespace}AdditionalReference>
      <{XMLTestFileConstants.XmlElementNamespace}Consignment>";
		var expectedResultLine = $@"</{XMLTestFileConstants.XmlElementNamespace}AdditionalReference>
      </{XMLTestFileConstants.XmlElementNamespace}GoodsItem>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
			AssertContains(expectedResultLine, messageText);
		});
	}

	protected override AmendmentAESMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new AmendmentAESMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

	protected override AmendmentAESMessageBuilder CreateMessageBuilderWithNullProvider() => new AmendmentAESMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

	protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.AESTestFilePath, "TestAESAmendment.txt");

	#region Structures SetUp

	protected override void SetUp()
	{
		base.SetUp();

		mockProvider.Setup(m => m.Message).Returns(SetUpMessage().Object);
		mockProvider.Setup(m => m.IsFinalPeriod).Returns(true);
		mockProvider.Setup(m => m.PhaseIDSpecified).Returns(true);

		var mockAuthorisation1 = BuilderHelperTest.SetUpCommonAuthorisation("1", "BTI", "Reference", "A12345678");
		var mockAuthorisation2 = BuilderHelperTest.SetUpCommonAuthorisation("2", "BTI", "Reference", "A12345678");
		var mockAuthorisations = new ICommonAuthorisation[] { mockAuthorisation1, mockAuthorisation2 };
		mockProvider.Setup(m => m.Authorisations).Returns(mockAuthorisations);

		mockProvider.Setup(m => m.CustomOfficeOfPresentation).Returns("ES009999");
		mockProvider.Setup(m => m.CustomOfficeOfExport).Returns("ES000101");
		mockProvider.Setup(m => m.CustomOfficeOfExit).Returns("ES000102");

		var mockExporter = SetUpExporter("ESA78587268", "Street", "Madrid", "28003", "ES");
		mockProvider.Setup(m => m.Exporter).Returns(mockExporter.Object);

		var mockContactPerson = BuilderHelperTest.SetUpContactInformation("Prometeo", "test_email@taric.es", "616123443");
		var mockDeclarant = BuilderHelperTest.SetUpPartyIdProviderWithContactPerson("ESA78587268", mockContactPerson);
		mockProvider.Setup(m => m.Declarant).Returns(mockDeclarant);

		var mockRepresentative = BuilderHelperTest.SetUpCommonRepresentativeWithContactPerson("ESA78587268", "2", mockContactPerson);
		mockProvider.Setup(m => m.Representative).Returns(mockRepresentative);

		mockGoodsShipment = SetUpHeaderGoodsShipment(mockAuthorisations, mockContactPerson);
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);

		var mockExportOperation = SetUpExportOperation();
		mockProvider.Setup(m => m.ExportOperation).Returns(mockExportOperation.Object);
	}

	Mock<IAmendmentAESExportOperation> SetUpExportOperation()
	{
		var mockExportOperation = new Mock<IAmendmentAESExportOperation>();
		mockExportOperation.Setup(m => m.LRN).Returns("PRLSVNE000006");
		mockExportOperation.Setup(m => m.DeclarationType).Returns("EX");
		mockExportOperation.Setup(m => m.DeclarationSubType).Returns("A");
		mockExportOperation.Setup(m => m.RecapitulationDate).Returns(new ZDateTime(2022, 7, 8, 14, 7, 39, DateTimeKind.Local));
		mockExportOperation.Setup(m => m.RecapitulationDateSpecified).Returns(true);
		mockExportOperation.Setup(m => m.SecurityFlag).Returns("2");
		mockExportOperation.Setup(m => m.SpecificCircumstance).Returns("A20");
		mockExportOperation.Setup(m => m.TotalAmount).Returns(11.879);
		mockExportOperation.Setup(m => m.Currency).Returns("JPY");
		mockExportOperation.Setup(m => m.MRN).Returns("21ES00999910000054");

		return mockExportOperation;
	}

	Mock<IDeclarationAESLine> mockLine1;
	Mock<IDeclarationAESGoodsShipment> mockGoodsShipment;

	Mock<IDeclarationAESExporter> SetUpExporter(ZString id, ZString address, ZString city, ZString postCode, ZString country)
	{
		var mockExporter = new Mock<IDeclarationAESExporter>();
		mockExporter.Setup(m => m.Id).Returns(id);

		mockExporter.Setup(m => m.Address).Returns(BuilderHelperTest.SetUpAddress(address, city, postCode, country));

		return mockExporter;
	}

	Mock<IDeclarationAESGoodsShipment> SetUpHeaderGoodsShipment(IEnumerable<ICommonAuthorisation> authorisations, IPartyContactProvider contactPerson)
	{
		var mockHeader = new Mock<IDeclarationAESGoodsShipment>();
		mockHeader.Setup(m => m.NatureOfTransaction).Returns("91");
		mockHeader.Setup(m => m.CountryOfExport).Returns("ES");
		mockHeader.Setup(m => m.CountryOfDestination).Returns("AE");

		var mockAdditionalSupplyActor1 = BuilderHelperTest.SetUpSupplyActor("1", "CS", "12345678A");
		var mockAdditionalSupplyActor2 = BuilderHelperTest.SetUpSupplyActor("2", "CS", "12345678A");
		var mockAdditionalSupplyActors = new ICommonAdditionalSupplyChainActorSeqNum[] { mockAdditionalSupplyActor1, mockAdditionalSupplyActor2 };
		mockHeader.Setup(m => m.AdditionalSupplyActors).Returns(mockAdditionalSupplyActors);

		var mockDeliveryTerms = BuilderHelperTest.SetUpCommonDeliveryTerms();
		mockHeader.Setup(m => m.DeliveryTerms).Returns(mockDeliveryTerms);

		var mockWarehouse = BuilderHelperTest.SetUpWarehouseCommon();
		mockHeader.Setup(m => m.Warehouse).Returns(mockWarehouse);

		var mockSupportingDocumentHeader1 = SetUpSupportingDocumentHeader("1", "N381", "Factura2", "Custom2", new DateTime(2022, 7, 21), "2");
		var mockSupportingDocumentHeader2 = SetUpSupportingDocumentHeader("2", "N326", "Proforma2", "Custom2", new DateTime(2022, 7, 21), "2");
		var mockSupportingDocumentsHeader = new IDeclarationAESSupportingDocumentHeader[] { mockSupportingDocumentHeader1.Object, mockSupportingDocumentHeader2.Object };
		mockHeader.Setup(m => m.SupportingDocuments).Returns(mockSupportingDocumentsHeader);

		var mockAdditionalReferenceHeader1 = BuilderHelperTest.SetUpDocumentSequenceNumber("1", "Y922", "ES89564");
		var mockAdditionalReferenceHeader2 = BuilderHelperTest.SetUpDocumentSequenceNumber("2", "Y923", "Other2");
		var mockAdditionalReferencesHeader = new ICommonDocumentSequenceNumber[] { mockAdditionalReferenceHeader1, mockAdditionalReferenceHeader2 };
		mockHeader.Setup(m => m.AdditionalReferences).Returns(mockAdditionalReferencesHeader);

		var mockAdditionalInfoHeader1 = BuilderHelperTest.SetUpDocumentSequenceNumber("1", "Y910", "text10");
		var mockAdditionalInfoHeader2 = BuilderHelperTest.SetUpDocumentSequenceNumber("2", "Y911", "text20");
		var mockAdditionalInfosHeader = new ICommonDocumentSequenceNumber[] { mockAdditionalInfoHeader1, mockAdditionalInfoHeader2 };
		mockHeader.Setup(m => m.AdditionalInfos).Returns(mockAdditionalInfosHeader);

		var mockConsignor = new Mock<IPartyIdProvider>();
		mockConsignor.Setup(m => m.Id).Returns("A12345678");

		var mockConsignee = SetUpConsignee("A12345678", "Consignee", "Street", "Madrid", "28003", "ES");

		mockConsignment = SetUpConsignment(mockConsignor.Object, mockConsignee.Object, contactPerson);
		mockHeader.Setup(m => m.Consignment).Returns(mockConsignment.Object);

		var mockTariffAdditionalCode1 = BuilderHelperTest.SetUpAdditionalCode("1", "4321");
		var mockTariffAdditionalCode2 = BuilderHelperTest.SetUpAdditionalCode("2", "5678");
		var mockTariffAdditionalCodes = new ICommonAdditionalCode[] { mockTariffAdditionalCode1, mockTariffAdditionalCode2 };

		var mockTariffNationalAdditionalCode1 = BuilderHelperTest.SetUpAdditionalCode("1", "1234");
		var mockTariffNationalAdditionalCode2 = BuilderHelperTest.SetUpAdditionalCode("2", "1234");
		var mockTariffNationalAdditionalCodes = new ICommonAdditionalCode[] { mockTariffNationalAdditionalCode1, mockTariffNationalAdditionalCode2 };

		mockCommodityCode = SetUpCommodityCode(mockTariffAdditionalCodes, mockTariffNationalAdditionalCodes);
		var mockCommodityCode2 = SetUpCommodityCode(Enumerable.Empty<ICommonAdditionalCode>(), Enumerable.Empty<ICommonAdditionalCode>());

		var mockDangerousGood1 = SetUpDangerousGoods("1", "A10");
		var mockDangerousGood2 = SetUpDangerousGoods("2", "A20");
		var mockDangerousGoods = new ICommonDangerousGoods[] { mockDangerousGood1.Object, mockDangerousGood2.Object };

		var mockPackage1 = BuilderHelperTest.SetUpCommonPackages("1", "VO", "MARCA1", "15");
		var mockPackage2 = BuilderHelperTest.SetUpCommonPackages("2", "CR", "MARCA2", "15");
		var mockPackages = new ICommonPackageWithSequenceAndPackNum[] { mockPackage1, mockPackage2 };

		var mockPreviousDocument1 = SetUpAESCommonDocument("1", "NCLE", "12012021", "1", "NAR", 2.456, true);
		var mockPreviousDocument2 = SetUpAESCommonDocument("2", "NCLE", "12012021", "1", "NAR", 2, false);
		var mockPreviousDocuments = new IAESCommonDocument[] { mockPreviousDocument1.Object, mockPreviousDocument2.Object };

		var mockSupportingDocumentLine1 = SetUpSupportingDocumentLine("1", "N380", "Factura", "Custom", new DateTime(2022, 7, 21), "1", "HL", 50.126m, true);
		var mockSupportingDocumentLine2 = SetUpSupportingDocumentLine("2", "N325", "Proforma", "Custom", new DateTime(2022, 7, 21), "1", "HL", 50, false);
		var mockSupportingDocumentsLine = new IDeclarationAESSupportingDocumentLine[] { mockSupportingDocumentLine1.Object, mockSupportingDocumentLine2.Object };

		var mockTransportDocument1 = BuilderHelperTest.SetUpDocumentSequenceNumber("1", "N760", "SO700039");
		var mockTransportDocument2 = BuilderHelperTest.SetUpDocumentSequenceNumber("2", "N705", "Barco");
		var mockTransportDocuments = new ICommonDocumentSequenceNumber[] { mockTransportDocument1, mockTransportDocument2 };

		var mockAdditionalReferenceLine1 = BuilderHelperTest.SetUpDocumentSequenceNumber("1", "Y924", "DE89564");
		var mockAdditionalReferenceLine2 = BuilderHelperTest.SetUpDocumentSequenceNumber("2", "Y925", "Other");
		var mockAdditionalReferencesLine = new ICommonDocumentSequenceNumber[] { mockAdditionalReferenceLine1, mockAdditionalReferenceLine2 };

		var mockAdditionalInfoLine1 = BuilderHelperTest.SetUpDocumentSequenceNumber("1", "Y900", "text1");
		var mockAdditionalInfoLine2 = BuilderHelperTest.SetUpDocumentSequenceNumber("2", "Y901", "text2");
		var mockAdditionalInfosLine = new ICommonDocumentSequenceNumber[] { mockAdditionalInfoLine1, mockAdditionalInfoLine2 };

		mockLine1 = SetUpLine("1", authorisations, mockConsignor.Object, mockConsignee.Object, mockAdditionalSupplyActors, mockCommodityCode.Object,
			mockDangerousGoods, mockPackages, mockPreviousDocuments, mockSupportingDocumentsLine, mockTransportDocuments, mockAdditionalReferencesLine, mockAdditionalInfosLine);

		var mockLine2 = SetUpLine("2", Enumerable.Empty<ICommonAuthorisation>(), mockConsignor.Object, mockConsignee.Object, Enumerable.Empty<ICommonAdditionalSupplyChainActorSeqNum>(),
			mockCommodityCode2.Object, Enumerable.Empty<ICommonDangerousGoods>(), Enumerable.Empty<ICommonPackageWithSequenceAndPackNum>(),
			Enumerable.Empty<IAESCommonDocument>(), Enumerable.Empty<IDeclarationAESSupportingDocumentLine>(), Enumerable.Empty<ICommonDocumentSequenceNumber>(), Enumerable.Empty<ICommonDocumentSequenceNumber>(),
			Enumerable.Empty<ICommonDocumentSequenceNumber>());

		var mockLines = new IDeclarationAESLine[] { mockLine1.Object, mockLine2.Object };

		mockHeader.Setup(m => m.Lines).Returns(mockLines);

		return mockHeader;
	}

	Mock<IDeclarationAESConsignee> SetUpConsignee(ZString id, ZString name, ZString address, ZString city, ZString postCode, ZString country)
	{
		var mockConsignee = new Mock<IDeclarationAESConsignee>();
		mockConsignee.Setup(m => m.Id).Returns(id);
		mockConsignee.Setup(m => m.Name).Returns(name);

		mockConsignee.Setup(m => m.Address).Returns(BuilderHelperTest.SetUpAddress(address, city, postCode, country));

		return mockConsignee;
	}

	Mock<IDeclarationAESConsignment> mockConsignment;

	Mock<IDeclarationAESConsignment> SetUpConsignment(IPartyIdProvider consignor, IDeclarationAESConsignee consignee, IPartyContactProvider contactPerson)
	{
		var mockConsignment = new Mock<IDeclarationAESConsignment>();
		mockConsignment.Setup(m => m.IsContainerised).Returns(true);
		mockConsignment.Setup(m => m.InlandModeOfTransport).Returns("3");
		mockConsignment.Setup(m => m.ModeOfTransportAtBorder).Returns("3");
		mockConsignment.Setup(m => m.GrossMass).Returns(420.5);
		mockConsignment.Setup(m => m.ReferenceNumberUCR).Returns("AB45612312345C");

		mockConsignment.Setup(m => m.Carrier).Returns(BuilderHelperTest.SetUpPartyId("DE1185233"));

		mockConsignment.Setup(m => m.Consignor).Returns(consignor);
		mockConsignment.Setup(m => m.Consignee).Returns(consignee);

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

		var mockCountryOfRouting1 = SetUpCountryOfRouting("1", "CZ");
		var mockCountryOfRouting2 = SetUpCountryOfRouting("2", "DE");
		var mockCountryOfRoutings = new ICommonCountryOfRoutingOfConsignment[] { mockCountryOfRouting1.Object, mockCountryOfRouting2.Object };
		mockConsignment.Setup(m => m.CountryOfRoutingOfConsignments).Returns(mockCountryOfRoutings);

		var mockActiveBorderTransport = BuilderHelperTest.SetUpTransportMediumInfo("30", "DE", "CZ");
		mockConsignment.Setup(m => m.ActiveBorderTransportMeans).Returns(mockActiveBorderTransport.Object);

		var mockTransportDocument1 = BuilderHelperTest.SetUpDocumentSequenceNumber("1", "N765", "SO700050");
		var mockTransportDocument2 = BuilderHelperTest.SetUpDocumentSequenceNumber("2", "N706", "Camion");
		var mockTransportDocuments = new ICommonDocumentSequenceNumber[] { mockTransportDocument1, mockTransportDocument2 };
		mockConsignment.Setup(m => m.TransportDocuments).Returns(mockTransportDocuments);

		mockConsignment.Setup(m => m.TransportChargesMoP).Returns("H");

		return mockConsignment;
	}

	Mock<IDeclarationAESCommodityCode> mockCommodityCode;

	Mock<IDeclarationAESCommodityCode> SetUpCommodityCode(IEnumerable<ICommonAdditionalCode> tariffAdditionalCodes, IEnumerable<ICommonAdditionalCode> tariffNationalAdditionalCodes)
	{
		var mockCommodityCode = new Mock<IDeclarationAESCommodityCode>();
		mockCommodityCode.Setup(m => m.TariffCode).Returns("640192");
		mockCommodityCode.Setup(m => m.TariffCodeCombined).Returns("10");
		mockCommodityCode.Setup(m => m.TariffAdditionalCodes).Returns((IReadOnlyCollection<ICommonAdditionalCode>)tariffAdditionalCodes);
		mockCommodityCode.Setup(m => m.NationalAdditionalCodes).Returns((IReadOnlyCollection<ICommonAdditionalCode>)tariffNationalAdditionalCodes);

		return mockCommodityCode;
	}

	Mock<ICommonDangerousGoods> SetUpDangerousGoods(ZString sequenceNumber, ZString code)
	{
		var mockDangerousGoods = new Mock<ICommonDangerousGoods>();
		mockDangerousGoods.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockDangerousGoods.Setup(m => m.UNDangerousCode).Returns(code);

		return mockDangerousGoods;
	}

	Mock<IDeclarationAESSupportingDocumentLine> SetUpSupportingDocumentLine(ZString sequenceNumber, ZString name, ZString number, ZString issueAuthorityName, DateTime documentDate, ZString line, ZString measurement, ZDecimal quantity, ZBool quantitySpecified)
	{
		var mockDocument = new Mock<IDeclarationAESSupportingDocumentLine>();
		mockDocument.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockDocument.Setup(m => m.Name).Returns(name);
		mockDocument.Setup(m => m.Number).Returns(number);
		var mockCommonDocument = SetUpCommonSupportingDocumentExtraFields(issueAuthorityName, documentDate);
		mockDocument.Setup(m => m.CommonSupportingDocumentExtraFields).Returns(mockCommonDocument.Object);
		mockDocument.Setup(m => m.LineNumber).Returns(line);
		mockDocument.Setup(m => m.Measurement).Returns(measurement);
		mockDocument.Setup(m => m.Quantity).Returns(quantity);
		mockDocument.Setup(m => m.QuantitySpecified).Returns(quantitySpecified);

		return mockDocument;
	}

	Mock<IDeclarationAESSupportingDocumentHeader> SetUpSupportingDocumentHeader(ZString sequenceNumber, ZString name, ZString number, ZString issueAuthorityName, DateTime documentDate, ZString line)
	{
		var mockDocument = new Mock<IDeclarationAESSupportingDocumentHeader>();
		mockDocument.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockDocument.Setup(m => m.Name).Returns(name);
		mockDocument.Setup(m => m.Number).Returns(number);
		var mockCommonDocument = SetUpCommonSupportingDocumentExtraFields(issueAuthorityName, documentDate);
		mockDocument.Setup(m => m.CommonSupportingDocumentExtraFields).Returns(mockCommonDocument.Object);
		mockDocument.Setup(m => m.LineNumber).Returns(line);

		return mockDocument;
	}

	Mock<IDeclarationAESLine> SetUpLine(ZString sequenceNumber, IEnumerable<ICommonAuthorisation> authorisations, IPartyIdProvider consignor, IDeclarationAESConsignee consignee,
IEnumerable<ICommonAdditionalSupplyChainActorSeqNum> supplyActors, IDeclarationAESCommodityCode commodityCode, IEnumerable<ICommonDangerousGoods> dangerousGoods, IEnumerable<ICommonPackageWithSequenceAndPackNum> packages,
IEnumerable<IAESCommonDocument> previousDocuments, IEnumerable<IDeclarationAESSupportingDocumentLine> documents, IEnumerable<ICommonDocumentSequenceNumber> transportDocuments,
IEnumerable<ICommonDocumentSequenceNumber> additionalReferences, IEnumerable<ICommonDocumentSequenceNumber> additionalInfos)
	{
		var mockLine = new Mock<IDeclarationAESLine>();
		mockLine.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockLine.Setup(m => m.StatisticalValue).Returns(1234.5623m);
		mockLine.Setup(m => m.UCRReferenceNumber).Returns("UCR");
		mockLine.Setup(m => m.Authorisations).Returns((IReadOnlyCollection<ICommonAuthorisation>)authorisations);

		mockProcedure = new Mock<IDeclarationAESProcedure>();
		mockProcedure.Setup(m => m.RequestedCPC).Returns("10");
		mockProcedure.Setup(m => m.PreviousCPC).Returns("00");

		mockProcedure.Setup(m => m.AdditionalProcedures).Returns(new ICommonAdditionalCode[] { BuilderHelperTest.SetUpAdditionalCode("1", "124"), BuilderHelperTest.SetUpAdditionalCode("2", "125") });
		mockLine.Setup(m => m.Procedure).Returns(mockProcedure.Object);

		mockLine.Setup(m => m.Consignor).Returns(consignor);
		mockLine.Setup(m => m.Consignee).Returns(consignee);
		mockLine.Setup(m => m.AdditionalSupplyActors).Returns((IReadOnlyCollection<ICommonAdditionalSupplyChainActorSeqNum>)supplyActors);

		var mockOrigin = new Mock<IAESCommonOrigin>();
		mockOrigin.Setup(m => m.CountryOfOrigin).Returns("ES");
		mockOrigin.Setup(m => m.StateOfOrigin).Returns("28");
		mockLine.Setup(m => m.Origin).Returns(mockOrigin.Object);

		mockCommodity = new Mock<IDeclarationAESCommodity>();
		mockCommodity.Setup(m => m.GoodsDescription).Returns("Waterproof footwear DOS");
		mockCommodity.Setup(m => m.CusCode).Returns("0018896-5");

		mockCommodity.Setup(m => m.CommodityCode).Returns(commodityCode);

		mockCommodity.Setup(m => m.DangerousGoods).Returns((IReadOnlyCollection<ICommonDangerousGoods>)dangerousGoods);

		var mockGoodsMeasure = BuilderHelperTest.SetUpCommonGoodsMeasureWithSupUnitsAndSpecified();
		mockCommodity.Setup(m => m.GoodsMeasure).Returns(mockGoodsMeasure);

		mockLine.Setup(m => m.Commodity).Returns(mockCommodity.Object);

		mockLine.Setup(m => m.InternalPackages).Returns((IReadOnlyCollection<ICommonPackageWithSequenceAndPackNum>)packages);
		mockLine.Setup(m => m.PreviousDocuments).Returns((IReadOnlyCollection<IAESCommonDocument>)previousDocuments);
		mockLine.Setup(m => m.SupportingDocuments).Returns((IReadOnlyCollection<IDeclarationAESSupportingDocumentLine>)documents);
		mockLine.Setup(m => m.TransportDocuments).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)transportDocuments);
		mockLine.Setup(m => m.AdditionalReferences).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)additionalReferences);
		mockLine.Setup(m => m.AdditionalInfos).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)additionalInfos);

		return mockLine;
	}

	Mock<IAESCommonTransportEquipment> mockTransportEquipment1;
	Mock<IAESCommonLocationOfGoods> mockLocationOfGoods;

	Mock<ICommonCountryOfRoutingOfConsignment> SetUpCountryOfRouting(ZString sequenceNumber, ZString country)
	{
		var mockCountryOfRouting = new Mock<ICommonCountryOfRoutingOfConsignment>();
		mockCountryOfRouting.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockCountryOfRouting.Setup(m => m.CountryOfRouting).Returns(country);

		return mockCountryOfRouting;
	}

	Mock<IDeclarationAESProcedure> mockProcedure;
	Mock<IDeclarationAESCommodity> mockCommodity;
	#endregion
}
