using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC044C_v515.CC044CV1Ent;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public class NotifUnloadingNCTSMessageBuilder : NCTSCommonMessageBuilder<INotifUnloadingNCTSMessageDataProvider, Cc044Cv1Ent>
	{
		public NotifUnloadingNCTSMessageBuilder(INotifUnloadingNCTSMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override ZString GetMessageType() => "CC044C";

		protected override Cc044Cv1Ent GenerateXMLMessage()
		{
			var declaration = GetPopulatedTransactionId<Cc044Cv1Ent>();
			if (declaration != null)
			{
				declaration.Cc044C = GetPopulatedCC044CType();
			}
			return declaration;
		}

		Cc044CType GetPopulatedCC044CType()
		{
			var cC044Cv515 = GetPopulatedMessage<Cc044CType>();
			if (cC044Cv515 != null)
			{
				cC044Cv515.TransitOperation = GetPopulatedNotifUnloadingNCTSTransitOperation();
				cC044Cv515.CustomsOfficeOfDestinationActual = GetPopulatedCustomOffice<CustomsOfficeOfDestinationActualType03>(provider.CustomsOfficeOfDestinationActual);
				cC044Cv515.TraderAtDestination = GetPopulatedAddressInformationIdCommon<TraderAtDestinationType02>(provider.TraderAtDestination);
				cC044Cv515.RepresentanteEnDestino = GetPopulatedAddressInformationIdCommon<RepresentanteEnDestinoType99>(provider.RepresentativeAtDestination);
				cC044Cv515.UnloadingRemark = GetPopulatedUnloadingRemarksNCTS();
				cC044Cv515.Consignment = GetPopulatedNotifUnloadingConsigment();
			}
			return cC044Cv515;
		}

		TransitOperationType15 GetPopulatedNotifUnloadingNCTSTransitOperation()
		{
			var transitOperation = GetPopulatedCommonTransitOperationMRN<TransitOperationType15>(provider.TransitOperation);
			if (transitOperation != null)
			{
				transitOperation.OtherThingsToReport = provider.TransitOperation.OtherThingsToReport;
			}
			return transitOperation;
		}

		UnloadingRemarkType GetPopulatedUnloadingRemarksNCTS()
		{
			var providerUnloadingRemarks = provider.UnloadingRemarks;
			return providerUnloadingRemarks == null ? null : new UnloadingRemarkType
			{
				Conform = providerUnloadingRemarks.Conform ? Flag.Item1 : Flag.Item0,
				UnloadingCompletion = Flag.Item1,
				UnloadingDate = new DateTime(providerUnloadingRemarks.UnloadingDate.Year, providerUnloadingRemarks.UnloadingDate.Month, providerUnloadingRemarks.UnloadingDate.Day),
				StateOfSeals = providerUnloadingRemarks.StateOfSealsSpecified ? providerUnloadingRemarks.StateOfSeals ? Flag.Item1 : Flag.Item0 : null,
				UnloadingRemark = providerUnloadingRemarks.UnloadingRemark
			};
		}

		ConsignmentType06 GetPopulatedNotifUnloadingConsigment()
		{
			var consignment = GetPopulatedCommonConsignment<ConsignmentType06, TransportEquipmentType03, DepartureTransportMeansType04>(provider.Consignment,
				GetPopulatedTransportEquipment<TransportEquipmentType03, SealType02, GoodsReferenceType01>);
			if (provider.Consignment != null)
			{
				var suppotingDocCollection = new Collection<SupportingDocumentType03>();
				foreach (var suppotingDoc in provider.Consignment.SupportingDocument.ConvertToCollection(GetPopulatedCommonDocumentWithInfo<SupportingDocumentType03>) ?? Enumerable.Empty<SupportingDocumentType03>())
				{
					suppotingDocCollection.Add(suppotingDoc);
				}
				var transportDocCollection = new Collection<TransportDocumentType03>();
				foreach (var transportDoc in provider.Consignment.TransportDocument.ConvertToCollection(GetPopulatedCommonDocument<TransportDocumentType03>) ?? Enumerable.Empty<TransportDocumentType03>())
				{
					transportDocCollection.Add(transportDoc);
				}
				var additionalRefCollection = new Collection<AdditionalReferenceType08>();
				foreach (var additionalRef in provider.Consignment.AdditionalReference.ConvertToCollection(GetPopulatedCommonDocument<AdditionalReferenceType08>) ?? Enumerable.Empty<AdditionalReferenceType08>())
				{
					additionalRefCollection.Add(additionalRef);
				}
				var houseCollection = new Collection<HouseConsignmentType05>();
				foreach (var house in provider.Consignment.HouseConsignment.ConvertToCollection(GetPopulatedNotifUnloadingNCTSHouseConsignment) ?? Enumerable.Empty<HouseConsignmentType05>())
				{
					houseCollection.Add(house);
				}

				if (provider.Consignment.GrossMassSpecified)
				{
					GetPopulatedCommonGrossMass(provider.Consignment.GrossMass, consignment);
				}
				consignment.SupportingDocument = suppotingDocCollection;
				consignment.TransportDocument = transportDocCollection;
				consignment.AdditionalReference = additionalRefCollection;
				consignment.HouseConsignment = houseCollection;
			}
			return consignment;
		}

		HouseConsignmentType05 GetPopulatedNotifUnloadingNCTSHouseConsignment(INotifUnloadingNCTSHouseConsignment providerHouse)
		{
			var house = GetPopulatedHouseConsignmentCommon<HouseConsignmentType05, TransportDocumentType03, AdditionalReferenceType08>(providerHouse);
			if (providerHouse != null)
			{
				var departureTransportCollection = new Collection<DepartureTransportMeansType04>();
				foreach (var departureTransport in providerHouse.DepartureTransportMeans.ConvertToCollection(GetPopulatedDepartureTransportMeans<DepartureTransportMeansType04>) ?? Enumerable.Empty<DepartureTransportMeansType04>())
				{
					departureTransportCollection.Add(departureTransport);
				}
				var suppotingDocCollection = new Collection<SupportingDocumentType03>();
				foreach (var suppotingDoc in providerHouse.SupportingDocument.ConvertToCollection(GetPopulatedCommonDocumentWithInfo<SupportingDocumentType03>) ?? Enumerable.Empty<SupportingDocumentType03>())
				{
					suppotingDocCollection.Add(suppotingDoc);
				}
				var itemCollection = new Collection<ConsignmentItemType05>();
				foreach (var item in providerHouse.ConsignmentItem.ConvertToCollection(GetPopulatedNotifUnloadingNCTSConsignmentItem) ?? Enumerable.Empty<ConsignmentItemType05>())
				{
					itemCollection.Add(item);
				}

				house.GrossMassValueSpecified = providerHouse.GrossMassSpecified;
				house.DepartureTransportMeans = departureTransportCollection;
				house.SupportingDocument = suppotingDocCollection;
				house.ConsignmentItem = itemCollection;
			}
			return house;
		}

		ConsignmentItemType05 GetPopulatedNotifUnloadingNCTSConsignmentItem(INotifUnloadingNCTSConsignmentItem providerItem)
		{
			var item = GetPopulatedCommonConsigmentItem<ConsignmentItemType05, PackagingType04, TransportDocumentType03, AdditionalReferenceType07>(providerItem);
			if (providerItem != null)
			{
				var suppotingDocCollection = new Collection<SupportingDocumentType03>();
				foreach (var suppotingDoc in providerItem.SupportingDocument.ConvertToCollection(GetPopulatedCommonDocumentWithInfo<SupportingDocumentType03>) ?? Enumerable.Empty<SupportingDocumentType03>())
				{
					suppotingDocCollection.Add(suppotingDoc);
				}

				item.Commodity = GetPopulatedNotifUnloadingCommodity(providerItem.Commodity);
				item.SupportingDocument = suppotingDocCollection;
			}
			return item;
		}

		CommodityType02 GetPopulatedNotifUnloadingCommodity(INotifUnloadingCommodity providerCommodity)
		{
			var commodity = GetPopulatedCommonCommodityWithCusCode<CommodityType02, CommodityCodeType03>(providerCommodity);
			if (providerCommodity != null)
			{
				commodity.GoodsMeasure = GetPopulatedCommonGoodsMeasure<GoodsMeasureType04>(providerCommodity.GoodsMeasure);
			}
			return commodity;
		}
	}
}
