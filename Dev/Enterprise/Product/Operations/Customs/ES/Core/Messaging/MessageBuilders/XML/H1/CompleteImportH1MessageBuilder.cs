using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.CC415AV1Ent;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.ES_CC415A;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.ES_ctypes;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

[CodeAlive("This will be used in the next WI")]
public class CompleteImportH1MessageBuilder : H1ImportCommonMessageBuilder<ICompleteImportH1MessageDataProvider, Cc415Av1Ent>
{
	public CompleteImportH1MessageBuilder(ICompleteImportH1MessageDataProvider provider, ZString messageType, ZString messageSubType)
		: base(provider, messageType, messageSubType)
	{
	}

	protected override Cc415Av1Ent GenerateXMLMessage()
	{
		return new Cc415Av1Ent
		{
			Message = GetPopulatedMessage(),
			Mrn = provider.DataProviderMRN.MRN,
			Operation = provider.Operation,
			CustomsOfficeOfImport = GetPopulatedCustomsOffice<MScoType>(provider.CustomsOfficeOfImport),
			RecapitulationPeriod = provider.RecapitulationPeriod.ToCustomsFormatDateStringyyyyMMWithDash(),
			ModoActivacionDeclaracion = provider.ActivationMode,
			Cc415A = GetPopulatedCc415A()
		};
	}

	Cc415AType GetPopulatedCc415A()
	{
		return new Cc415AType
		{
			ImportOperation = GetPopulatedImportOperation(provider.ImportOperation),
			Authorisation = provider.Authorisations.ConvertToCollection(GetPopulatedDeclarationAuthorisation<MAuthorisationType04>),
			CustomsOfficeOfPresentation = GetPopulatedCustomsOffice<MPcoType>(provider.CustomOfficeOfPresentation),
			SupervisingCustomsOffice = GetPopulatedCustomsOffice<MScoType>(provider.SupervisingCustomOffice),
			Importer = GetPopulatedPartyWithAddress<MImporterType, MAddressType01>(provider.Importer),
			Declarant = GetPopulatedPartyIdProviderWithContactPerson<MDeclarantType, MContactPersonType>(provider.Declarant),
			PersonProvidingAGuarantee = GetPopulatedAddressInformationIdCommon<MPersonProvidingGuaranteeType>(provider.PersonProvidingAGuarantee),
			PersonPayingCustomsDuty = GetPopulatedAddressInformationIdCommon<MPersonPayingCustomsDutyType>(provider.PersonPayingCustomsDuty),
			Representative = GetPopulatedCommonRepresentativeWithContactPerson<MRepresentativeType, MContactPersonType>(provider.Representative),
			Guarantee = provider.Guarantees.ConvertToCollection(GetPopulatedGuarantee),
			CurrencyExchange = new MCurrencyExchangeType
			{
				InternalCurrencyUnit = provider.Currency
			},
			DeferredPayment = provider.DeferredPayments.ConvertToCollection(GetPopulatedDeferredPayment),
			GoodsShipment = provider.GoodsShipments.ConvertToCollection(GetPopulatedGoodsShipment),
		};
	}

	MGuaranteeType GetPopulatedGuarantee(ICompleteImportH1Guarantee guaranteeProvider)
	{
		return guaranteeProvider == null ? null : new MGuaranteeType
		{
			SequenceNumber = guaranteeProvider.SequenceNumber,
			GuaranteeType = guaranteeProvider.GuaranteeType,
			GuaranteeReference = guaranteeProvider.GuaranteeReferences.ConvertToCollection(GetPopulatedGuaranteeReference),
		};
	}

	MGuaranteeReferenceType GetPopulatedGuaranteeReference(ICompleteImportH1GuaranteeReference guaranteeReferenceProvider)
	{
		return guaranteeReferenceProvider == null ? null : new MGuaranteeReferenceType
		{
			SequenceNumber = guaranteeReferenceProvider.SequenceNumber,
			Grn = guaranteeReferenceProvider.GRN,
			CcQualifier = guaranteeReferenceProvider.CcQualifier,
			AccessCode = guaranteeReferenceProvider.AccessCode,
			CurrencyCode = guaranteeReferenceProvider.CurrencyCode,
			AmountToBeCovered = guaranteeReferenceProvider.AmountToBeCovered,
			OtherGuaranteeReference = guaranteeReferenceProvider.OtherGuaranteeReference,
			CustomsOfficeOfGuarantee = new MCustomsOfficeOfGuaranteeType
			{
				ReferenceNumber = guaranteeReferenceProvider.CustomsOfficeOfGuarantee,
			},
		};
	}

	MGoodsShipmentType05 GetPopulatedGoodsShipment(ICompleteImportH1GoodsShipment goodsShipmentProvider)
	{
		var goodsShipment = GetPopulatedCommonGoodsShipment<MGoodsShipmentType05, MAdditionalSupplyChainActorType, MPreviousDocumentType04, MSupportingDocumentType02, MAdditionalReferenceType, MAdditionalInformationType>(goodsShipmentProvider);
		if (goodsShipment != null)
		{
			goodsShipment.SequenceNumber = goodsShipmentProvider.SequenceNumber;
			goodsShipment.NatureOfTransaction = goodsShipmentProvider.NatureOfTransaction;
			goodsShipment.TotalAmountInvoiced = goodsShipmentProvider.TotalAmountInvoiced;
			goodsShipment.DateOfAcceptance = goodsShipmentProvider.DateOfAcceptance.ToCustomsFormatString(CustomsDateTimeExtension.DateTimeFormatyyyyMMddTHHmmss);
			goodsShipment.Buyer = GetPopulatedPartyWithAddress<MBuyerType, MAddressType01>(goodsShipmentProvider.Buyer);
			goodsShipment.Seller = GetPopulatedPartyWithAddress<MSellerType, MAddressType01>(goodsShipmentProvider.Seller);
			goodsShipment.DeliveryTerms = GetPopulatedDeliveryTerms<MDeliveryTermsType>(goodsShipmentProvider.DeliveryTerms);
			goodsShipment.Destination = GetPopulatedDestination(goodsShipmentProvider.Destination);
			goodsShipment.Warehouse = GetPopulatedWarehouse<MWarehouseType>(goodsShipmentProvider.Warehouse);
			goodsShipment.Consignment = GetPopulatedConsignment(goodsShipmentProvider.Consignment);
			goodsShipment.GoodsShipmentItem = goodsShipmentProvider.GoodsShipmentItems.ConvertToCollection(GetPopulatedGoodsShipmentItem);
		}
		return goodsShipment;
	}

	MConsignmentType04 GetPopulatedConsignment(ICompleteImportH1Consigment consignmentProvider)
	{
		var consigment = GetPopulatedCommonConsignment<MConsignmentType04, MTransportEquipmentType, MLocationOfGoodsType03, MTransportDocumentType>(consignmentProvider);
		if (consigment != null)
		{
			consigment.InlandModeOfTransport = consignmentProvider.InlandModeOfTransport;
			consigment.ModeOfTransportAtTheBorder = consignmentProvider.ModeOfTransportAtBorder;
			consigment.ArrivalTransportMeans = GetPopulatedArrivalTransportMeans(consignmentProvider.ArrivalTransportMeans);
			consigment.ActiveBorderTransportMeans = new MActiveBorderTransportMeansType
			{
				Nationality = consignmentProvider.ActiveBorderTransportMeansNationality,
			};
		}
		return consigment;
	}

	MGoodsShipmentItemType04 GetPopulatedGoodsShipmentItem(ICompleteImportH1GoodsShipmentItem goodsShipmentItemProvider)
	{
		var goodsShipmentItem = GetPopulatedCompleteAndSimplifiedGoodsShipmentItem<MGoodsShipmentItemType04, MAuthorisationType05, MAdditionalSupplyChainActorType, MPackagingType01, MSupportingDocumentType01, MTransportDocumentType, MAdditionalReferenceType, MAdditionalInformationType>(goodsShipmentItemProvider);
		if (goodsShipmentItem != null)
		{
			goodsShipmentItem.SequenceNumber = goodsShipmentItemProvider.SequenceNumber;
			goodsShipmentItem.StatisticalValue = goodsShipmentItemProvider.StatisticalValue;
			goodsShipmentItem.NatureOfTransaction = goodsShipmentItemProvider.NatureOfTransaction;
			goodsShipmentItem.Buyer = GetPopulatedPartyWithAddress<MBuyerType, MAddressType01>(goodsShipmentItemProvider.Buyer);
			goodsShipmentItem.Seller = GetPopulatedPartyWithAddress<MSellerType, MAddressType01>(goodsShipmentItemProvider.Seller);
			goodsShipmentItem.Destination = GetPopulatedDestination(goodsShipmentItemProvider.Destination);
			goodsShipmentItem.Commodity = GetPopulatedCommodity(goodsShipmentItemProvider.Commodity);
			goodsShipmentItem.CustomsValuation = GetPopulatedCustomsValuation(goodsShipmentItemProvider.CustomsValuation);
			goodsShipmentItem.ValuationAdjustment = new MValuationAdjustmentType
			{
				ValuationIndicators = goodsShipmentItemProvider.ValuationAdjustmentIndicator,
			};
		}
		return goodsShipmentItem;
	}

	protected MCommodityType04 GetPopulatedCommodity(ICompleteImportH1Commodity commodityProvider)
	{
		var commodity = GetPopulatedCompleteAndSimplifiedCommodity<MCommodityType04>(commodityProvider);
		if (commodity != null)
		{
			commodity.CalculationOfTaxes = GetPopulatedCalculationOfTaxes(commodityProvider.CalculationOfTaxes);
		}
		return commodity;
	}
}
