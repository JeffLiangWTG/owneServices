using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.ES_ctypes;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.ES_PDI400;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.PDI400V1Ent;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.TD11;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

[CodeAlive("This will be used in the next WI")]
public class IncompleteImportH1MessageBuilder : H1ImportCommonMessageBuilder<IIncompleteImportH1MessageDataProvider, Pdi400V1Ent>
{
	public IncompleteImportH1MessageBuilder(IIncompleteImportH1MessageDataProvider provider, ZString messageType, ZString messageSubType)
		: base(provider, messageType, messageSubType)
	{
	}

	protected override Pdi400V1Ent GenerateXMLMessage()
	{
		return new Pdi400V1Ent
		{
			Message = GetPopulatedMessage(),
			Pdi400 = GetPopulatedPdi400Type()
		};
	}

	Pdi400Type GetPopulatedPdi400Type()
	{
		return new Pdi400Type
		{
			ImportOperation = GetPopulatedIncompleteImportOperation(provider.ImportOperation),
			Operation = provider.Operation,
			CustomsOfficeOfImport = GetPopulatedCustomsOffice<MScoType>(provider.CustomsOfficeOfImport),
			CustomsOfficeOfPresentation = GetPopulatedCustomsOffice<MPcoType>(provider.CustomOfficeOfPresentation),
			Importer = GetPopulatedPartyWithAddress<MImporterType, MAddressType01>(provider.Importer),
			Declarant = GetPopulatedPartyIdProviderWithContactPerson<MDeclarantType, MContactPersonType>(provider.Declarant),
			Representative = GetPopulatedCommonRepresentativeWithContactPerson<MRepresentativeType, MContactPersonType>(provider.Representative),
			CountryOfDispatch = GetPopulatedCountryOfDispatch(provider.CountryOfDispatch),
			TransportEquipment = provider.TransportEquipments.ConvertToCollection(GetPopulatedCommonTransportEquipment<MTransportEquipmentType, MGoodsReferenceType>),
			GoodsItem = provider.GoodsShipmentItems.ConvertToCollection(GetPopulatedIncompleteImportGoodsShipmentItem),
		};
	}

	MCciOperationTypeD02 GetPopulatedIncompleteImportOperation(IIncompleteImportH1ImportOperation importOperationProvider)
	{
		var importOperation = GetPopulatedCommonImportOperation<MCciOperationTypeD02>(importOperationProvider);
		if (importOperation != null)
		{
			importOperation.CustomsRegistrationNumber = importOperationProvider.CustomsRegistrationNumber;
		}
		return importOperation;
	}

	GoodsItemTypeD03 GetPopulatedIncompleteImportGoodsShipmentItem(IIncompleteImportH1GoodsShipmentItem goodsShipmentItemProvider)
	{
		var goodsShipmentItem = GetPopulatedCommonGoodsShipmentItem<GoodsItemTypeD03>(goodsShipmentItemProvider);
		if (goodsShipmentItem != null)
		{
			goodsShipmentItem.Commodity = GetPopulatedIncompleteImportCommodity(goodsShipmentItemProvider.Commodity);
			goodsShipmentItem.Origin = GetPopulatedCommonOrigin<OriginTypeD>(goodsShipmentItemProvider.CountryOfOrigin);
			goodsShipmentItem.Procedure = GetPopulatedCommonProcedure<ProcedureTypeD>(goodsShipmentItemProvider.Procedure);
		}
		return goodsShipmentItem;
	}

	CommodityTypeD GetPopulatedIncompleteImportCommodity(IIncompleteImportH1Commodity commodityProvider)
	{
		var commodity = GetPopulatedCommonCommodity<CommodityTypeD>(commodityProvider);
		if (commodity != null)
		{
			commodity.CommodityCode = GetPopulatedCommonCommodityCode<CommodityCodeTypeD>(commodityProvider.CommodityCode);
		}
		return commodity;
	}
}
