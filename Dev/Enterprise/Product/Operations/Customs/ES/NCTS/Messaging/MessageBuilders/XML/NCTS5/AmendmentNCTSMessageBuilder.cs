using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC013C_v515.CC013CV1Ent;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public class AmendmentNCTSMessageBuilder : NCTSCommonMessageBuilder<IAmendmentNCTSMessageDataProvider, Cc013Cv1Ent>
	{
		public AmendmentNCTSMessageBuilder(IAmendmentNCTSMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override ZString GetMessageType() => "CC013C";

		protected override Cc013Cv1Ent GenerateXMLMessage()
		{
			var declaration = GetPopulatedTransactionId<Cc013Cv1Ent>();
			if (declaration != null)
			{
				declaration.Cc013C = GetPopulatedCC013CType();
			}
			return declaration;
		}

		Cc013CType GetPopulatedCC013CType()
		{
			var cC013Cv515 = GetPopulatedMessage<Cc013CType>();
			if (cC013Cv515 != null)
			{
				cC013Cv515.TransitOperation = GetPopulatedAmendmentTransitOperation();
				cC013Cv515.Authorisation = provider.Authorisations.ConvertToCollection(GetPopulatedAuthorisation<AuthorisationType03>);
				cC013Cv515.CustomsOfficeOfDeparture = GetPopulatedCustomOffice<CustomsOfficeOfDepartureType03>(provider.CustomsOfficeOfDeparture);
				cC013Cv515.CustomsOfficeOfDestinationDeclared = GetPopulatedCustomOffice<CustomsOfficeOfDestinationDeclaredType01>(provider.CustomsOfficeOfDestinationDeclared);
				cC013Cv515.CustomsOfficeOfTransitDeclared = provider.CustomsOfficeOfTransitDeclared.ConvertToCollection(GetPopulatedCommonCustomOffice<CustomsOfficeOfTransitDeclaredType03>);
				cC013Cv515.CustomsOfficeOfExitForTransitDeclared = provider.CustomsOfficeOfExitForTransitDeclared.ConvertToCollection(GetPopulatedCommonCustomOffice<CustomsOfficeOfExitForTransitDeclaredType02>);
				cC013Cv515.HolderOfTheTransitProcedure = GetPopulatedHolderOfTheTransitProcedure<HolderOfTheTransitProcedureType14, ContactPersonType05, AddressType17>(provider.HolderOfTheTransitProcedure);
				cC013Cv515.Representative = GetPopulatedRepresentative<RepresentativeType05, ContactPersonType05>(provider.Representative);
				cC013Cv515.Guarantee = provider.Guarantee.ConvertToCollection(GetPopulatedGuarantee<GuaranteeType01, GuaranteeReferenceType03>);
				cC013Cv515.Consignment = GetPopulatedConsignmentDepartureAndAmendment<ConsignmentType20, TransportEquipmentType06, LocationOfGoodsType05, DepartureTransportMeansType03, ActiveBorderTransportMeansType02,
											PlaceOfLoadingType03, CarrierType04, ConsignorType07, ConsigneeType05, ContactPersonType05, AddressType17, AdditionalSupplyChainActorType, CountryOfRoutingOfConsignmentType01,
											PlaceOfUnloadingType01, TransportChargesType, HouseConsignmentType10, SupportingDocumentType05, TransportDocumentType04, AdditionalReferenceType06, AdditionalInformationType03>(provider.Consignment,
											GetPopulatedTransportEquipment<TransportEquipmentType06, SealType05, GoodsReferenceType02>,
											GetPopulatedHouseConsignmentDepartureAndAmendment<HouseConsignmentType10, AdditionalSupplyChainActorType, ConsignmentItemType09, ConsigneeType02, ConsigneeType05, ConsignorType07, ContactPersonType05, AddressType17, AddressType12, CommodityType06, CommodityCodeType02, DangerousGoodsType01, GoodsMeasureType02, PackagingType03, DepartureTransportMeansType05, PreviousDocumentType08, PreviousDocumentType10, SupportingDocumentType05, TransportDocumentType04, AdditionalReferenceType05, AdditionalReferenceType06, AdditionalInformationType03>);
			}
			return cC013Cv515;
		}

		TransitOperationType04 GetPopulatedAmendmentTransitOperation()
		{
			var transitOperation = GetPopulatedCommonTransitOperationMRN<TransitOperationType04>(provider.TransitOperation);
			if (transitOperation != null)
			{
				GetPopulatedCommonCompleteTransitOperation(provider.TransitOperation.CommonTransitOperation, transitOperation);
				transitOperation.AmendmentTypeFlag = Flag.Item0;
			}
			return transitOperation;
		}
	}
}
