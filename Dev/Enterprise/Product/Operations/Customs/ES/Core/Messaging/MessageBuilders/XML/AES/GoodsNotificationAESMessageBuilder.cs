using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CC511C_v514.CC511CV1Ent;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
public class GoodsNotificationAESMessageBuilder : AESCommonMessageBuilder<IGoodsNotificationAESMessageDataProvider, Cc511Cv1Ent>
{
	public GoodsNotificationAESMessageBuilder(IGoodsNotificationAESMessageDataProvider provider, ZString messageType, ZString messageSubType)
		: base(provider, messageType, messageSubType)
	{
	}

	protected override ZString GetMessageType() => "CC511C";

	protected override Cc511Cv1Ent GenerateXMLMessage()
	{
		var declaration = GetPopulatedTransactionId<Cc511Cv1Ent>();
		if (declaration != null)
		{
			declaration.Cc511C = GetPopulatedCC511C();
		}

		return declaration;
	}

	Cc511CType GetPopulatedCC511C()
	{
		var cC511Cv514 = GetPopulatedMessage<Cc511CType>();
		if (cC511Cv514 != null)
		{
			cC511Cv514.ExportOperation = GetPopulatedExportOperationLRN();
			cC511Cv514.CustomsOfficeOfPresentation = GetPopulatedCustomsOffice<CustomsOfficeOfPresentationType01>(provider.CustomOfficeOfPresentation);
			cC511Cv514.CustomsOfficeOfExport = GetPopulatedCustomsOffice<CustomsOfficeOfExportType04>(provider.CustomOfficeOfExport);
			cC511Cv514.Declarant = GetPopulatedPartyIdProviderWithContactPerson<DeclarantType06, ContactPersonType02>(provider.Declarant);
			cC511Cv514.Representative = GetPopulatedCommonRepresentativeWithContactPerson<RepresentativeType03, ContactPersonType02>(provider.Representative);
			cC511Cv514.GoodsShipment = GetPopulatedGoodsShipment();
		}

		return cC511Cv514;
	}

	ExportOperationType51 GetPopulatedExportOperationLRN()
	{
		var exportOperation = provider.ExportOperation;
		return exportOperation == null ? null : new ExportOperationType51
		{
			Lrn = exportOperation.LRN,
		};
	}

	GoodsShipmentType02 GetPopulatedGoodsShipment()
	{
		var goodsShipment = provider.GoodsShipment;
		return goodsShipment == null ? null : new GoodsShipmentType02
		{
			Consignment = GetPopulatedConsignment()
		};
	}

	ConsignmentType03 GetPopulatedConsignment()
	{
		var providerConsignment = provider.GoodsShipment.Consignment;
		var consignment = GetPopulatedCommonConsignment<ConsignmentType03>(providerConsignment);
		if (consignment != null)
		{
			consignment.TransportEquipment = providerConsignment.TransportEquipment.ConvertToCollection(GetPopulatedTransportEquipment<TransportEquipmentType03, SealType01, GoodsReferenceType02>);
			consignment.LocationOfGoods = GetPopulatedLocationOfGoods<LocationOfGoodsType03, CustomsOfficeType02, GnssType, EconomicOperatorType, AddressType01, PostcodeAddressType, ContactPersonType01>(provider.GoodsShipment.Consignment.LocationOfGoods);
			consignment.DepartureTransportMeans = providerConsignment.DepartureTransportMeans.ConvertToCollection(GetPopulatedDepartureTransportMeans<DepartureTransportMeansType02>);
		}
		return consignment;
	}
}
