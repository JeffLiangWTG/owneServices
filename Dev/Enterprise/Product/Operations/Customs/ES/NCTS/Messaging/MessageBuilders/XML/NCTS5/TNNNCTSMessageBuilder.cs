using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CCTNNC_v515.CCTNNCV1Ent;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public class TNNNCTSMessageBuilder : NCTSCommonMessageBuilder<ITNNNCTSMessageDataProvider, Cctnncv1Ent>
	{
		public TNNNCTSMessageBuilder(ITNNNCTSMessageDataProvider provider, ZString messageType, ZString messageSubType) : base(provider, messageType, messageSubType)
		{
		}

		protected override ZString GetMessageType() => "CCTNNC";

		protected override Cctnncv1Ent GenerateXMLMessage()
		{
			var declaration = GetPopulatedTransactionId<Cctnncv1Ent>();
			if (declaration != null)
			{
				declaration.Cctnnc = GetPopulatedCCTNNCType();
			}
			return declaration;
		}

		CctnncType GetPopulatedCCTNNCType()
		{
			var cCTNNCv515 = GetPopulatedMessage<CctnncType>();
			if (cCTNNCv515 != null)
			{
				cCTNNCv515.TransitOperation = GetPopulatedTNNTransitOperation();
				cCTNNCv515.CustomsOfficeOfDeparture = GetPopulatedCustomOffice<CustomsOfficeOfDepartureType80>(provider.CustomsOfficeOfDeparture);
				cCTNNCv515.CustomsOfficeOfDestinationDeclared = GetPopulatedCustomOffice<CustomsOfficeOfDestinationDeclaredType80>(provider.CustomsOfficeOfDestinationDeclared);
				cCTNNCv515.CustomsOfficeOfDestinationActual = GetPopulatedCustomOffice<CustomsOfficeOfDestinationActualType80>(provider.CustomsOfficeOfDestinationActual);
				cCTNNCv515.HolderOfTheTransitProcedure = GetPopulatedHolderOfTheTransitProcedureWithAddress<HolderOfTheTransitProcedureType80, AddressType80>(provider.HolderOfTheTransitProcedure);
				cCTNNCv515.TraderAtDestination = GetPopulatedAddressInformationIdCommon<TraderAtDestinationType80>(provider.TraderAtDestination);
				cCTNNCv515.RepresentanteEnDestino = GetPopulatedAddressInformationIdCommon<RepresentanteEnDestinoType80>(provider.RepresentativeAtDestination);
				cCTNNCv515.DocumentoDigitalizadoTnn = GetPopulatedDigitizedDocument(provider.DigitizedDocument);
				cCTNNCv515.Consignment = GetPopulatedTNNConsignment();
			}
			return cCTNNCv515;
		}

		TransitOperationType80 GetPopulatedTNNTransitOperation()
		{
			var providerTransitOperation = provider.TransitOperation;
			var transitOperation = GetPopulatedCommonTransitOperationMRN<TransitOperationType80>(providerTransitOperation);
			if (transitOperation != null)
			{
				GetPopulatedCommonTransitOperation(providerTransitOperation.CommonTransitOperation, transitOperation);
				transitOperation.DeclarationAcceptanceDate = GetDateTimeFromZDateTime(providerTransitOperation.DeclarationAcceptanceDate);
				transitOperation.ReleaseDate = GetDateTimeFromZDateTime(providerTransitOperation.ReleaseDate);
			}
			return transitOperation;
		}

		DocumentoDigitalizadoType80 GetPopulatedDigitizedDocument(ITNNNCTSDigitizedDocument providerDocument)
		{
			var document = GetPopulatedAnnexesCommon<DocumentoDigitalizadoType80>(providerDocument);
			if (providerDocument != null)
			{
				document.TipoDocumentoFisico = providerDocument.DocumentType;
				document.UbicacionRecepcion = providerDocument.DocumentLocation;
			}
			return document;
		}

		ConsignmentType80 GetPopulatedTNNConsignment()
		{
			var providerConsignment = provider.Consignment;
			var consignment = GetPopulatedCommonConsignmentDeparture<ConsignmentType80, TransportEquipmentType80, DepartureTransportMeansType80>(providerConsignment, GetPopulatedTransportEquipment<TransportEquipmentType80, SealType80, GoodsReferenceType80>);
			if (providerConsignment != null)
			{
				GetPopulatedCommonConsignmentData<CountryOfRoutingOfConsignmentType80, ConsigneeType80, AddressType80, TransportChargesType80, SupportingDocumentType80, TransportDocumentType80, AdditionalReferenceType80, AdditionalInformationType80>(providerConsignment.CommonConsignmentData, consignment);
				consignment.Consignor = GetPopulatedNCTSPartyNameProviderWithAddress<ConsignorType80, AddressType80>(providerConsignment.Consignor);
				consignment.ActiveBorderTransportMeans = providerConsignment.ActiveBorderTransportMeans.ConvertToCollection(GetPopulatedActiveBorderTransportMeans<ActiveBorderTransportMeansType80>);
				consignment.HouseConsignment = providerConsignment.HouseConsignment.ConvertToCollection(GetPopulatedTNNHouseConsignment);
			}
			return consignment;
		}

		HouseConsignmentType80 GetPopulatedTNNHouseConsignment(ITNNNCTSHouseConsignment providerHouse)
		{
			var house = GetPopulatedHouseConsignmentDepartureAndAmendmentAndTNN<HouseConsignmentType80, SupportingDocumentType80, TransportDocumentType80, AdditionalReferenceType80, AdditionalInformationType80>(providerHouse);
			if (providerHouse != null)
			{
				house.ConsignmentItem = providerHouse.ConsignmentItem.ConvertToCollection(GetPopulatedTNNConsignmentItem);
			}
			return house;
		}

		ConsignmentItemType80 GetPopulatedTNNConsignmentItem(ITNNNCTSConsignmentItem providerItem)
		{
			var item = GetPopulatedConsigmentItemDepartureAndAmendmentAndTNN<ConsignmentItemType80, PackagingType80, TransportDocumentType80, AdditionalReferenceType80, AdditionalInformationType80>(providerItem);
			if (providerItem != null)
			{
				item.Commodity = GetPopulatedTNNCommodity(providerItem.Commodity);
				item.SupportingDocument = providerItem.SupportingDocument.ConvertToCollection(GetPopulatedCommonDocumentWithInfo<SupportingDocumentType80>);
			}
			return item;
		}

		CommodityType80 GetPopulatedTNNCommodity(ITNNNCTSCommodity providerCommodity)
		{
			var commodity = GetPopulatedCommonCommodity<CommodityType80, CommodityCodeType80>(providerCommodity);
			if (providerCommodity != null)
			{
				commodity.DangerousGoods = providerCommodity.DangerousGoods.ConvertToCollection(GetPopulatedDangerousGoods<DangerousGoodsType80>);
				commodity.GoodsMeasure = GetPopulatedCommonGoodsMeasure<GoodsMeasureType80>(providerCommodity.GoodsMeasure);
			}
			return commodity;
		}
	}
}
