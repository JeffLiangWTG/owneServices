using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CC513C_v514.CC513CV1Ent;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
public class AmendmentAESMessageBuilder : AESCommonMessageBuilder<IAmendmentAESMessageDataProvider, Cc513Cv1Ent>
{
	public AmendmentAESMessageBuilder(IAmendmentAESMessageDataProvider provider, ZString messageType, ZString messageSubType)
		: base(provider, messageType, messageSubType)
	{
	}

	protected override ZString GetMessageType() => "CC513C";

	protected override Cc513Cv1Ent GenerateXMLMessage()
	{
		var declaration = GetPopulatedTransactionId<Cc513Cv1Ent>();
		if (declaration != null)
		{
			declaration.Cc513C = GetPopulatedCC513C();
		}
		return declaration;
	}

	Cc513CType GetPopulatedCC513C()
	{
		var cC513Cv514 = GetPopulatedMessage<Cc513CType>();
		if (cC513Cv514 != null)
		{
			cC513Cv514.ExportOperation = GetPopulatedExportOperation();
			cC513Cv514.Authorisation = provider.Authorisations.ConvertToCollection(GetPopulatedDeclarationAuthorisation<AuthorisationType03>);
			cC513Cv514.CustomsOfficeOfPresentation = GetPopulatedCustomsOffice<CustomsOfficeOfPresentationType01>(provider.CustomOfficeOfPresentation);
			cC513Cv514.CustomsOfficeOfExport = GetPopulatedCustomsOffice<CustomsOfficeOfExportType01>(provider.CustomOfficeOfExport);
			cC513Cv514.CustomsOfficeOfExitDeclared = GetPopulatedCustomsOffice<CustomsOfficeOfExitDeclaredType02>(provider.CustomOfficeOfExit);
			cC513Cv514.Exporter = GetPopulatedExporter<ExporterType03, AddressType02>(provider.Exporter);
			cC513Cv514.Declarant = GetPopulatedPartyIdProviderWithContactPerson<DeclarantType06, ContactPersonType02>(provider.Declarant);
			cC513Cv514.Representative = GetPopulatedCommonRepresentativeWithContactPerson<RepresentativeType03, ContactPersonType02>(provider.Representative);
			cC513Cv514.GoodsShipment = GetPopulatedDeclarationGoodsShipment<GoodsShipmentType513, AdditionalSupplyChainActorType, DeliveryTermsType02, WarehouseType, SupportingDocumentType05, AdditionalReferenceType01, AdditionalInformationType, ConsignmentType15, ConsigneeType03, TransportEquipmentType03, LocationOfGoodsType04, GoodsItemType513, CommodityType03>(
											provider.GoodsShipment,
											GetPopulatedDeclarationConsignment<ConsignmentType15, CarrierType01, ConsignorType03, ConsigneeType03, TransportEquipmentType03, LocationOfGoodsType04, DepartureTransportMeansType04, CountryOfRoutingOfConsignmentType01, ActiveBorderTransportMeansType01, TransportDocumentType02, TransportChargesType>,
											GetPopulatedConsignee<ConsigneeType03, AddressType02>,
											GetPopulatedTransportEquipment<TransportEquipmentType03, SealType01, GoodsReferenceType02>,
											GetPopulatedLocationOfGoods<LocationOfGoodsType04, CustomsOfficeType02, GnssType, EconomicOperatorType, AddressType01, PostcodeAddressType, ContactPersonType01>,
											GetPopulatedDeclarationLine<GoodsItemType513, AuthorisationType02, ProcedureType, AdditionalProcedureType, ConsignorType03, ConsigneeType03, AdditionalSupplyChainActorType, OriginType, CommodityType03, PackagingType01, PreviousDocumentType01, SupportingDocumentType04, TransportDocumentType02, AdditionalReferenceType01, AdditionalInformationType>,
											GetPopulatedDeclarationCommodity<CommodityType03, CommodityCodeType04, TaricAdditionalCodeType01, NationalAdditionalCodeType, DangerousGoodsType01, GoodsMeasureType02>);
		}
		return cC513Cv514;
	}

	ExportOperationType07 GetPopulatedExportOperation()
	{
		var exportOperationProvider = provider.ExportOperation;
		var exportOperation = GetPopulatedDeclarationExportOperation<ExportOperationType07>(exportOperationProvider);
		if (exportOperation != null)
		{
			exportOperation.Mrn = exportOperationProvider.MRN;
		}
		return exportOperation;
	}
}
