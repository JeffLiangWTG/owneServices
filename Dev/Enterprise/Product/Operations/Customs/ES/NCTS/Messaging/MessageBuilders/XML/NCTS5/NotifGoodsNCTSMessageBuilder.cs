using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC170C_v515.CC170CV1Ent;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public class NotifGoodsNCTSMessageBuilder : NCTSCommonMessageBuilder<INotifGoodsNCTSMessageDataProvider, Cc170Cv1Ent>
	{
		public NotifGoodsNCTSMessageBuilder(INotifGoodsNCTSMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override ZString GetMessageType() => "CC170C";

		protected override Cc170Cv1Ent GenerateXMLMessage()
		{
			var declaration = GetPopulatedTransactionId<Cc170Cv1Ent>();
			if (declaration != null)
			{
				declaration.Cc170C = GetPopulatedCC170CType();
			}
			return declaration;
		}

		Cc170CType GetPopulatedCC170CType()
		{
			var cC170Cv515 = GetPopulatedMessage<Cc170CType>();
			if (cC170Cv515 != null)
			{
				cC170Cv515.TransitOperation = GetPopulatedCommonTransitOperationLRN<TransitOperationType24>(provider.TransitOperation);
				cC170Cv515.CustomsOfficeOfDeparture = GetPopulatedCustomOffice<CustomsOfficeOfDepartureType03>(provider.CustomsOfficeOfDeparture);
				cC170Cv515.HolderOfTheTransitProcedure = GetPopulatedCommonHolderOfTheTransitProcedure<HolderOfTheTransitProcedureType19>(provider.HolderOfTheTransitProcedure);
				cC170Cv515.Representative = GetPopulatedRepresentative<RepresentativeType05, ContactPersonType05>(provider.Representative);
				cC170Cv515.Consignment = GetPopulatedConsignmentNotifGoods();
			}
			return cC170Cv515;
		}

		ConsignmentType08 GetPopulatedConsignmentNotifGoods()
		{
			var consignment = GetPopulatedCommonConsignmentDepartureAndNotif<ConsignmentType08, TransportEquipmentType06, LocationOfGoodsType03, DepartureTransportMeansType05, ActiveBorderTransportMeansType03, PlaceOfLoadingType03>(provider.Consignment,
											GetPopulatedTransportEquipment<TransportEquipmentType06, SealType05, GoodsReferenceType02>);
			if (consignment != null)
			{
				consignment.HouseConsignment = provider.Consignment.HouseConsignment.ConvertToCollection(GetPopulatedHouseConsignmentSeqNumCommon<HouseConsignmentType06>);
			}
			return consignment;
		}
	}
}
