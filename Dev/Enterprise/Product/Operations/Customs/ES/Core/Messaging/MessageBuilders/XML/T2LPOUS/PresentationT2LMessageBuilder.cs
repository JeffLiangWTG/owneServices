using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LPOUS.CCIEJECV1Ent;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public class PresentationT2LMessageBuilder : T2LPOUSCommonMessageBuilder<IPresentationT2LMessageDataProvider, IejecType>
	{
		public PresentationT2LMessageBuilder(IPresentationT2LMessageDataProvider provider, ZString messageType, ZString messageSubType) : base(provider, messageType, messageSubType)
		{
		}

		protected override IejecType GenerateXMLMessage() => GetPopulatedIEJECType();

		IejecType GetPopulatedIEJECType()
		{
			var iEJECType = GetPopulatedTypeCommon<IejecType, MessageTdJ, PersonReqPresType, AddressType, ContactPersonInformationTypeEs>();
			if (iEJECType != null)
			{
				iEJECType.Mrnt2Lt2Lf = provider.PreviousMRN;
				iEJECType.LocationOfGoods = provider.LocationOfGoods;
				iEJECType.TransportEquipment = provider.TransportEquipment.ConvertToCollection(GetPopulatedTransportEquipment<TransportEquipmentType>);
				iEJECType.GoodsShipmentItem = provider.GoodItems.ConvertToCollection(GetPopulatedGoodItem);
				GetPopulatedContainerIndication(provider.ContainerIndication, iEJECType);
			}
			return iEJECType;
		}

		GoodsItemForJecType GetPopulatedGoodItem(IT2LPOUSPresentationGoodItem goodItemProvider)
		{
			var goodItem = GetPopulatedGoodsItem<GoodsItemForJecType>(goodItemProvider);
			if (goodItem != null)
			{
				goodItem.GoodsItemNumber = goodItemProvider.GoodsItemNumber;
				goodItem.PreviousDocument = goodItemProvider.PreviousDocuments.ConvertToCollection(GetPopulatedPreviousDocument);
				goodItem.T2Lt2LFgoodsItemNumber = goodItemProvider.T2LT2LFgoodsItemNumber;
			}
			return goodItem;

			PreviousDocumentTypeEs01 GetPopulatedPreviousDocument(IT2LPOUSPresentationPreviousDocument previousDocProvider)
			{
				var previousDocument = GetPopulatedDocumentCommon<PreviousDocumentTypeEs01>(previousDocProvider);
				if (previousDocument != null)
				{
					previousDocument.MeasurementUnitAndQualifier = previousDocProvider.MeasurementUnitAndQualifier;
					previousDocument.Quantity = previousDocProvider.Quantity;
					previousDocument.QuantityValueSpecified = previousDocProvider.QuantityValueSpecified;
					previousDocument.GoodsItemIdentifier = previousDocProvider.GoodsItemIdentifier;
					GetPopulatedPackageCommon(previousDocProvider.Packaging, previousDocument);
				}
				return previousDocument;
			}
		}
	}
}
