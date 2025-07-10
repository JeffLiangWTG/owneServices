using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.CCSimplificadaV1Ent;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.ES_CCSimplificada;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.ES_ctypes;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.TD11;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

[CodeAlive("This will be used in the next WI")]
public class SimplifiedImportH1MessageBuilder : H1ImportCommonMessageBuilder<ISimplifiedImportH1MessageDataProvider, CcSimplificadaV1Ent>
{
	public SimplifiedImportH1MessageBuilder(ISimplifiedImportH1MessageDataProvider provider, ZString messageType, ZString messageSubType) : base(provider, messageType, messageSubType)
	{
	}

	protected override CcSimplificadaV1Ent GenerateXMLMessage()
	{
		return new CcSimplificadaV1Ent
		{
			Message = GetPopulatedMessage(),
			Mrn = provider.DataProviderMRN.MRN,
			Operation = provider.Operation,
			CustomsOfficeOfImport = GetPopulatedCustomsOffice<MScoType>(provider.CustomsOfficeOfImport),
			ModoActivacionDeclaracion = provider.ActivationMode,
			CcSimplificada = GetPopulatedCcSimplificadaType()
		};
	}

	CcSimplificadaType GetPopulatedCcSimplificadaType()
	{
		return new CcSimplificadaType()
		{
			ImportOperation = GetPopulatedImportOperation(provider.ImportOperation),
			Authorisation = provider.Authorisations.ConvertToCollection(GetPopulatedDeclarationAuthorisation<MAuthorisationType01>),
			CustomsOfficeOfPresentation = GetPopulatedCustomsOffice<MPcoType>(provider.CustomOfficeOfPresentation),
			SupervisingCustomsOffice = GetPopulatedCustomsOffice<MScoType>(provider.SupervisingCustomOffice),
			Importer = GetPopulatedPartyWithAddress<MImporterType, MAddressType01>(provider.Importer),
			Declarant = GetPopulatedPartyIdProviderWithContactPerson<MDeclarantType, MContactPersonType>(provider.Declarant),
			PersonPayingCustomsDuty = GetPopulatedAddressInformationIdCommon<MPersonPayingCustomsDutyType>(provider.PersonPayingCustomsDuty),
			Representative = GetPopulatedCommonRepresentativeWithContactPerson<MRepresentativeType, MContactPersonType>(provider.Representative),
			CurrencyExchange = new MCurrencyExchangeTypeD
			{
				InternalCurrencyUnit = provider.Currency
			},
			GoodsShipment = GetPopulatedSimplifiedGoodsShipment(provider.GoodsShipment),
		};
	}

	MGoodsShipmentTypeDs GetPopulatedSimplifiedGoodsShipment(ISimplifiedImportH1GoodsShipment goodsShipmentProvider)
	{
		var goodsShipment = GetPopulatedCommonGoodsShipment<MGoodsShipmentTypeDs, MAdditionalSupplyChainActorType, MPreviousDocumentType04, MSupportingDocumentType02, MAdditionalReferenceType, MAdditionalInformationType>(goodsShipmentProvider);
		if (goodsShipment != null)
		{
			goodsShipment.Consignment = GetPopulatedCommonConsignment<MConsignmentTypeDs, MTransportEquipmentType, MLocationOfGoodsType01, MTransportDocumentType>(goodsShipmentProvider.Consignment);
			goodsShipment.GoodsShipmentItem = goodsShipmentProvider.GoodsShipmentItems.ConvertToCollection(GetPopulatedSimplifiedGoodsShipmentItem);
		}
		return goodsShipment;
	}

	MGoodsShipmentItemTypeDs GetPopulatedSimplifiedGoodsShipmentItem(ISimplifiedImportH1GoodsShipmentItem goodsShipmentItemProvider)
	{
		var goodsShipmentItem = GetPopulatedCompleteAndSimplifiedGoodsShipmentItem<MGoodsShipmentItemTypeDs, MAuthorisationType02, MAdditionalSupplyChainActorType, MPackagingType01, MSupportingDocumentType01D, MTransportDocumentType, MAdditionalReferenceType, MAdditionalInformationType>(goodsShipmentItemProvider);
		if (goodsShipmentItem != null)
		{
			goodsShipmentItem.Commodity = GetPopulatedSimplifiedCommodity(goodsShipmentItemProvider.Commodity);
		}
		return goodsShipmentItem;
	}

	protected MCommodityTypeD04 GetPopulatedSimplifiedCommodity(ISimplifiedH1Commodity commodityProvider)
	{
		var commodity = GetPopulatedCompleteAndSimplifiedCommodity<MCommodityTypeD04>(commodityProvider);
		if (commodity != null)
		{
			commodity.Preference = commodityProvider.Preference;
		}
		return commodity;
	}
}
