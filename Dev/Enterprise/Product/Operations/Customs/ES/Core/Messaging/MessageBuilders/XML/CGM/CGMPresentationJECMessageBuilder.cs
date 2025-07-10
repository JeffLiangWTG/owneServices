using CargoWise.Customs.ES.MessageDefinitions.Version1.CGM.CGMEJECV1Ent;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
public class CGMPresentationJECMessageBuilder : CGMCommonMessageBuilder<ICGMPresentationJECMessageDataProvider, Cgmejecv1EntType>
{
	public CGMPresentationJECMessageBuilder(ICGMPresentationJECMessageDataProvider provider, ZString messageType, ZString messageSubType) : base(provider, messageType, messageSubType)
	{
	}

	protected override Cgmejecv1EntType GenerateXMLMessage() => GetPopulatedCGMEJECv1();

	Cgmejecv1EntType GetPopulatedCGMEJECv1()
	{
		return new Cgmejecv1EntType
		{
			Message = GetPopulatedCommonMessage<MessageTdJ>(),
			PresentationCustomsOffice = provider.PresentationCustomsOffice,
			Mrncgm = provider.MRN,
			PersonPresentingGoodsToCustomsForCgm = GetPopulatedCGMPartyProviderWithAddressAndContactPerson<PersonReqPresTypej, AddressType, ContactPersonInformationTypeEs>(provider.PersonPresentingGoodsToCustomsForCGM),
			RepresentativeAtArrivalForCgm = GetPopulatedCGMPartyProviderWithAddressAndContactPerson<PersonReqPresTypej, AddressType, ContactPersonInformationTypeEs>(provider.RepresentativeAtArrivalForCGM),
			ContainerIndication = provider.IsContainerised ? BooleanContentType.Item1 : BooleanContentType.Item0,
			TransportEquipment = provider.TransportEquipments.ConvertToCollection(GetPopulatedTransportEquipment),
			PresentationGoodsItems = provider.GoodsItems.ConvertToCollection(GetPopulatedGoodsItem),
		};
	}

	TransportEquipmentType GetPopulatedTransportEquipment(ICGMPresentationTransportEquipment transportEquipmentProvider)
	{
		return transportEquipmentProvider == null ? null : new TransportEquipmentType
		{
			ContainerIdentificationNumber = transportEquipmentProvider.ContainerNumber,
			GoodsReference = transportEquipmentProvider.GoodsReference.ConvertToIntCollectionWithZero(),
		};
	}

	GoodsItemForJeccgmType GetPopulatedGoodsItem(ICGMPresentationGoodsItem goodsItemProvider)
	{
		return goodsItemProvider == null ? null : new GoodsItemForJeccgmType
		{
			GoodsItemNumber = goodsItemProvider.GoodsItemNumber,
			PreviousDocument = goodsItemProvider.PreviousDocuments.ConvertToCollection(GetPopulatedPreviousDocument),
			CgmcgmFgoodsItemNumber = goodsItemProvider.T2LT2LFgoodsItemNumber,
		};
	}

	PreviousDocumentTypeEs01 GetPopulatedPreviousDocument(ICGMPresentationPreviousDocument previousDocProvider)
	{
		var previousDocument = GetPopulatedDocumentCommon<PreviousDocumentTypeEs01>(previousDocProvider);
		if (previousDocument != null)
		{
			previousDocument.TypeOfPackages = previousDocProvider.TypeOfPackages;
			previousDocument.NumberOfPackages = previousDocProvider.NumberOfPackages;
			previousDocument.MeasurementUnitAndQualifier = previousDocProvider.MeasurementUnitAndQualifier;
			previousDocument.Quantity = previousDocProvider.Quantity;
			previousDocument.GoodsItemIdentifier = previousDocProvider.GoodsItemId;
		}
		return previousDocument;
	}
}
