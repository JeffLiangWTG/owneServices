using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC015C_v515.CC015CV1Ent;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public class DepartureNCTSMessageBuilder : NCTSCommonMessageBuilder<IDepartureNCTSMessageDataProvider, Cc015Cv1Ent>
	{
		public DepartureNCTSMessageBuilder(IDepartureNCTSMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override ZString GetMessageType() => "CC015C";

		protected override Cc015Cv1Ent GenerateXMLMessage()
		{
			var declaration = GetPopulatedTransactionId<Cc015Cv1Ent>();
			if (declaration != null)
			{
				declaration.Cc015C = GetPopulatedCC015CType();
			}
			return declaration;
		}

		public IComparablePredeclaration GetXMLObjectForComparison() => GenerateXMLMessage();

		Cc015CType GetPopulatedCC015CType()
		{
			var cC015Cv515 = GetPopulatedMessage<Cc015CType>();
			if (cC015Cv515 != null)
			{
				cC015Cv515.TransitOperation = GetPopulatedDepartureTransitOperation();
				cC015Cv515.Authorisation = provider.Authorisations.ConvertToCollection(GetPopulatedAuthorisation<AuthorisationType03>);
				cC015Cv515.CustomsOfficeOfDeparture = GetPopulatedCustomOffice<CustomsOfficeOfDepartureType03>(provider.CustomsOfficeOfDeparture);
				cC015Cv515.CustomsOfficeOfDestinationDeclared = GetPopulatedCustomOffice<CustomsOfficeOfDestinationDeclaredType01>(provider.CustomsOfficeOfDestinationDeclared);
				cC015Cv515.CustomsOfficeOfTransitDeclared = provider.CustomsOfficeOfTransitDeclared.ConvertToCollection(GetPopulatedCommonCustomOffice<CustomsOfficeOfTransitDeclaredType03>);
				cC015Cv515.CustomsOfficeOfExitForTransitDeclared = provider.CustomsOfficeOfExitForTransitDeclared.ConvertToCollection(GetPopulatedCommonCustomOffice<CustomsOfficeOfExitForTransitDeclaredType02>);
				cC015Cv515.HolderOfTheTransitProcedure = GetPopulatedHolderOfTheTransitProcedure<HolderOfTheTransitProcedureType14, ContactPersonType05, AddressType17>(provider.HolderOfTheTransitProcedure);
				cC015Cv515.Representative = GetPopulatedRepresentative<RepresentativeType05, ContactPersonType05>(provider.Representative);
				cC015Cv515.Guarantee = provider.Guarantee.ConvertToCollection(GetPopulatedGuarantee<GuaranteeType02, GuaranteeReferenceType03>);
				cC015Cv515.Consignment = GetPopulatedConsignmentDepartureAndAmendment<ConsignmentType20, TransportEquipmentType06, LocationOfGoodsType05, DepartureTransportMeansType03, ActiveBorderTransportMeansType02,
											PlaceOfLoadingType03, CarrierType04, ConsignorType07, ConsigneeType05, ContactPersonType05, AddressType17, AdditionalSupplyChainActorType, CountryOfRoutingOfConsignmentType01,
											PlaceOfUnloadingType01, TransportChargesType, HouseConsignmentType10, SupportingDocumentType05, TransportDocumentType04, AdditionalReferenceType06, AdditionalInformationType03>(provider.Consignment,
											GetPopulatedTransportEquipment<TransportEquipmentType06, SealType05, GoodsReferenceType02>,
											GetPopulatedHouseConsignmentDepartureAndAmendment<HouseConsignmentType10, AdditionalSupplyChainActorType, ConsignmentItemType09, ConsigneeType02, ConsigneeType05, ConsignorType07, ContactPersonType05, AddressType17, AddressType12, CommodityType06, CommodityCodeType02, DangerousGoodsType01, GoodsMeasureType02, PackagingType03, DepartureTransportMeansType05, PreviousDocumentType08, PreviousDocumentType10, SupportingDocumentType05, TransportDocumentType04, AdditionalReferenceType05, AdditionalReferenceType06, AdditionalInformationType03>);
			}
			return cC015Cv515;
		}

		TransitOperationType06 GetPopulatedDepartureTransitOperation()
		{
			var transitOperation = GetPopulatedCommonTransitOperationLRN<TransitOperationType06>(provider.TransitOperation);
			if (transitOperation != null)
			{
				GetPopulatedCommonCompleteTransitOperation(provider.TransitOperation.CommonTransitOperation, transitOperation);
			}
			return transitOperation;
		}
	}
}
