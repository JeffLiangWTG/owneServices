using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CC515C_v514.CC515CV1Ent;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
public class DeclarationAESMessageBuilder : AESCommonMessageBuilder<IDeclarationAESMessageDataProvider, Cc515Cv1Ent>
{
	public DeclarationAESMessageBuilder(IDeclarationAESMessageDataProvider provider, ZString messageType, ZString messageSubType)
		: base(provider, messageType, messageSubType)
	{
	}

	protected override ZString GetMessageType() => "CC515C";

	protected override Cc515Cv1Ent GenerateXMLMessage()
	{
		var declaration = GetPopulatedTransactionId<Cc515Cv1Ent>();
		if (declaration != null)
		{
			declaration.Cc515C = GetPopulatedCC515C();
		}

		return declaration;
	}

	Cc515CType GetPopulatedCC515C()
	{
		var cC515Cv514 = GetPopulatedMessage<Cc515CType>();
		if (cC515Cv514 != null)
		{
			cC515Cv514.ExportOperation = GetPopulatedDeclarationExportOperation<ExportOperationType09>(provider.ExportOperation);
			cC515Cv514.Authorisation = provider.Authorisations.ConvertToCollection(GetPopulatedDeclarationAuthorisation<AuthorisationType03>);
			cC515Cv514.CustomsOfficeOfPresentation = GetPopulatedCustomsOffice<CustomsOfficeOfPresentationType01>(provider.CustomOfficeOfPresentation);
			cC515Cv514.CustomsOfficeOfExport = GetPopulatedCustomsOffice<CustomsOfficeOfExportType01>(provider.CustomOfficeOfExport);
			cC515Cv514.CustomsOfficeOfExitDeclared = GetPopulatedCustomsOffice<CustomsOfficeOfExitDeclaredType02>(provider.CustomOfficeOfExit);
			cC515Cv514.Exporter = GetPopulatedExporter<ExporterType03, AddressType02>(provider.Exporter);
			cC515Cv514.Declarant = GetPopulatedPartyIdProviderWithContactPerson<DeclarantType06, ContactPersonType02>(provider.Declarant);
			cC515Cv514.Representative = GetPopulatedCommonRepresentativeWithContactPerson<RepresentativeType03, ContactPersonType02>(provider.Representative);
			cC515Cv514.GoodsShipment = GetPopulatedDeclarationGoodsShipment<GoodsShipmentType04, AdditionalSupplyChainActorType, DeliveryTermsType02, WarehouseType, SupportingDocumentType05, AdditionalReferenceType01, AdditionalInformationType, ConsignmentType15, ConsigneeType03, TransportEquipmentType03, LocationOfGoodsType04, GoodsItemType03, CommodityType03>(
											provider.GoodsShipment,
											GetPopulatedDeclarationConsignment<ConsignmentType15, CarrierType01, ConsignorType03, ConsigneeType03, TransportEquipmentType03, LocationOfGoodsType04, DepartureTransportMeansType04, CountryOfRoutingOfConsignmentType01, ActiveBorderTransportMeansType01, TransportDocumentType02, TransportChargesType>,
											GetPopulatedConsignee<ConsigneeType03, AddressType02>,
											GetPopulatedTransportEquipment<TransportEquipmentType03, SealType01, GoodsReferenceType02>,
											GetPopulatedLocationOfGoods<LocationOfGoodsType04, CustomsOfficeType02, GnssType, EconomicOperatorType, AddressType01, PostcodeAddressType, ContactPersonType01>,
											GetPopulatedDeclarationLine<GoodsItemType03, AuthorisationType02, ProcedureType, AdditionalProcedureType, ConsignorType03, ConsigneeType03, AdditionalSupplyChainActorType, OriginType, CommodityType03, PackagingType01, PreviousDocumentType01, SupportingDocumentType04, TransportDocumentType02, AdditionalReferenceType01, AdditionalInformationType>,
											GetPopulatedDeclarationCommodity<CommodityType03, CommodityCodeType04, TaricAdditionalCodeType01, NationalAdditionalCodeType, DangerousGoodsType01, GoodsMeasureType02>);
		}

		return cC515Cv514;
	}
}
