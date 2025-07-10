using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsConsolSynchroniser : BusinessObjectSynchroniser
	{
		public NctsConsolSynchroniser(NctsHeader nctsHeader, ICusInBondParent source)
			: base(nctsHeader, (BusinessObject)source)
		{
			Source.OnUpdatedByDataRefreshForCustomsSynchronisation += HookedConsol_OnUpdatedByDataRefreshForCustomsSynchronisation;
		}

		protected new NctsHeader Destination => (NctsHeader)base.Destination;

		protected new ForwardingConsol Source => (ForwardingConsol)base.Source;

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();

			if (Destination.IsArrivalMovement)
			{
				Synchronisers.Add(new JobDocAddressAddressOnlySynchroniser(Destination.DestinationTrader, (ZPropertyInfoGuid)Source.JK_OA_ReceivingForwarderAddressInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.ArrivalMovementHeader.BM_PlaceOfUnloadingInfo, Source.JK_RL_NKDischargePortInfo));
			}
			else if (Destination.IsDepartureMovement)
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.MovementHeader.BM_InlandTransportModeInfo, GetTransportModeConverted, GetTransportModeInfos));
				Synchronisers.Add(new FieldSynchroniser(Destination.MovementHeader.BM_ConveyanceNumberInfo, Source.JK_JX_JV_VoyageFlightInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.MovementHeader.BM_AdditionalTextInfo, Source.JK_UniqueConsignRefInfo));

				if (ShouldSynchroniseTransportDetails)
				{
					Synchronisers.Add(new FieldSynchroniser(Destination.MovementHeader.BM_ExportTransportModeInfo, GetTransportModeConverted, GetTransportModeInfos));
					Synchronisers.Add(new FieldSynchroniser(Destination.MovementHeader.BM_TransportAtDepartureInfo, Source.JK_JX_JV_VoyageFlightInfo));
					Synchronisers.Add(new FieldSynchroniser(Destination.MovementHeader.BM_TOLCarrierIDInfo, Source.JK_JX_JV_VoyageFlightInfo));
				}

				AddPrincipalSynchroniser();
				AddForeignDestPortSynchroniser();

				//Don't swap the order of these two.
				// The containers on the header must be created, or at least hooked, before the line item.
				// Otherwise the part that forces a tick on the line's non-persistent container pivots loads the header containers collection too soon and the rsult is that no headercontainers are created.
				foreach (ForwardingShipment shipment in Source.Shipments)
				{
					Synchronisers.Add(new NctsDepartureContainerCollectionSynchroniser(shipment, Destination, Source, Destination.ContainersAsICusInBondContainerCollectionForSynching, departureOrArrivalContainerFlag: ContainerTypeOfServiceList.Codes.DepartureContainer));
				}

				AddShipmentsToLineItemSynch();

				if (Destination.DefaultConsignorConsigneeRegistryManager.IsConsignor() && Source.JobDirection == Directions.Export)
				{
					if (Destination.Consignor.IsEmpty)
					{
						Synchronisers.Add(new JobDocAddressAddressOnlySynchroniser(Destination.Consignor, (ZPropertyInfoGuid)Source.JK_OA_PackDepotAddressInfo));
					}
				}

				if (Destination.DefaultConsignorConsigneeRegistryManager.IsConsignee() && Source.JobDirection == Directions.Import)
				{
					if (Destination.Consignee.IsEmpty)
					{
						Synchronisers.Add(new JobDocAddressAddressOnlySynchroniser(Destination.Consignee, (ZPropertyInfoGuid)Source.JK_OA_UnpackDepotAddressInfo));
					}
				}
			}
		}

		void AddShipmentsToLineItemSynch()
		{
			var departureMovementHeader = Destination.MovementHeader;
			var hasCommonCountryOfDispatch = Source.Shipments.OfType<ForwardingShipment>().Select(x => x.JS_RL_NKOrigin.Left(2)).Distinct().Take(2).Count() < 2;
			var hasCommonCountryOfDestination = Source.Shipments.OfType<ForwardingShipment>().Select(x => x.JS_RL_NKDestination.Left(2)).Distinct().Take(2).Count() < 2;
			var hasCommonConsignor = Source.Shipments.OfType<ForwardingShipment>().Select(x => x.Consignor != null ? x.Consignor.PK : ZGuid.Empty).Distinct().Take(2).Count() < 2;
			var hasCommonConsignee = Source.Shipments.OfType<ForwardingShipment>().Select(x => x.Consignee != null ? x.Consignee.PK : ZGuid.Empty).Distinct().Take(2).Count() < 2;
			var hasCommonCTStatus = Source.Shipments.OfType<ForwardingShipment>().Select(x => x.JS_CommunityTransitStatus).Distinct().Take(2).Count() < 2;

			foreach (ForwardingShipment shipment in Source.Shipments)
			{
				var uniqueConsignRef = shipment.JS_UniqueConsignRef;
				var lineItem = departureMovementHeader.GoodsItems
					.Cast<NctsDepartureCargoDesc>()
					.FirstOrDefault(existingLineItem => existingLineItem.BY_CommercialReferenceNumber == uniqueConsignRef);

				var lineSynch = GetNewNctsDepartureGoodsItemSynchroniser(lineItem ?? departureMovementHeader.GoodsItems.AddNew(), shipment
									, hasCommonCountryOfDispatch, hasCommonCountryOfDestination, hasCommonConsignor, hasCommonConsignee, hasCommonCTStatus);

				Synchronisers.Add(lineSynch);
			}
		}

		protected virtual ISynchroniser GetNewNctsDepartureGoodsItemSynchroniser(NctsDepartureCargoDesc goodsItem, ForwardingShipment source, bool hasCommonCountryOfDispatch, bool hasCommonCountryOfDestination, bool hasCommonConsignor, bool hasCommonConsignee, bool hasCommonCTStatus)
		{
			return new ShipmentToNctsDepartureGoodsItemSynchroniser(goodsItem, source, hasCommonCountryOfDispatch, hasCommonCountryOfDestination, hasCommonConsignor, hasCommonConsignee, hasCommonCTStatus);
		}

		protected virtual void AddForeignDestPortSynchroniser()
		{
			Synchronisers.Add(new FieldSynchroniser(Destination.MovementHeader.BM_RL_NKForeignDestPortInfo, Source.JK_JX_JA_RL_NKPortOfLoadingInfo));
		}

		void AddPrincipalSynchroniser()
		{
			if (!Destination.DefaultPrincipalRegistryManager.IsRegistryEnabled())
			{
				Synchronisers.Add(new JobDocAddressAddressOnlySynchroniser(Destination.Principal, (ZPropertyInfoGuid)Source.JK_OA_SendingForwarderAddressInfo));
			}
		}

		IEnumerable<ZPropertyInfo> GetTransportModeInfos()
		{
			yield return Source.JK_TransportModeInfo;
		}

		protected IZType GetTransportModeConverted() => Destination.TransportModeTranslator.TranslateToWCOCode(GetTransportMode());

		ZString GetTransportMode() => GetTransportModeCore();

		protected virtual ZString GetTransportModeCore()
		{
			var transportMode = Source.JK_TransportMode;

			if (!ShouldSynchroniseTransportDetails)
			{
				transportMode = TransportTypeList.Codes.Road;
			}

			return transportMode;
		}

		void HookedConsol_OnUpdatedByDataRefreshForCustomsSynchronisation(object sender, EventArgs e)
		{
			Source.OnUpdatedByDataRefreshForCustomsSynchronisation -= HookedConsol_OnUpdatedByDataRefreshForCustomsSynchronisation;
		}

		protected override void DisposeCore()
		{
			Source.OnUpdatedByDataRefreshForCustomsSynchronisation -= HookedConsol_OnUpdatedByDataRefreshForCustomsSynchronisation;
			base.DisposeCore();
		}

		protected bool ShouldSynchroniseTransportDetails => SyncHelper.ShouldSyncTransportDetails(Source.JK_TransportMode, Destination.ShouldTransportDetailsSyncDependsOnTransportMode);
	}
}
