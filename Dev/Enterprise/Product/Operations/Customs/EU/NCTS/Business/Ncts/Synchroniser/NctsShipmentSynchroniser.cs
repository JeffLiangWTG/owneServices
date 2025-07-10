using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsShipmentSynchroniser : BusinessObjectSynchroniser
	{
		public NctsShipmentSynchroniser(NctsHeader nctsHeader, ICusInBondParent source)
			: base(nctsHeader, (BusinessObject)source)
		{
			Source.OnJobDeclarationCreated += Shipment_OnJobDeclarationCreated;
		}

		protected new NctsHeader Destination => (NctsHeader)base.Destination;

		protected new ForwardingShipment Source => (ForwardingShipment)base.Source;

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();

			if (Destination.IsArrivalMovement)
			{
				if (Source.JobHeader != null && Source.JobHeader.LocalChargesAddr != null)
				{
					Synchronisers.Add(new JobDocAddressAddressOnlySynchroniser(Destination.DestinationTrader, (ZPropertyInfoGuid)Source.JobHeader.JH_OA_LocalChargesAddrInfo));
				}
				AddDestinationPortFieldSynchroniserForArrival();
			}
			else if (Destination.IsDepartureMovement)
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.MovementHeader.BM_AdditionalTextInfo, Source.JS_UniqueConsignRefInfo));

				if (Destination.Configuration?.FullLoadPortSupport ?? false)
				{
					AddPortofDispatchSynchroniser();
				}
				else
				{
					AddImportLoadPortSynchroniser();
				}

				AddDestinationPortFieldSynchroniserForDeparture();
				AddInlandTransportModeFieldSynchroniser();
				if (ShouldSynchroniseTransportDetails)
				{
					AddExportTransportModeFieldSynchroniser();
				}
				if (RelevantConsol != null)
				{
					Synchronisers.Add(new FieldSynchroniser(Destination.MovementHeader.BM_ConveyanceNumberInfo, RelevantConsol.JK_JX_JV_VoyageFlightInfo));
					if (ShouldSynchroniseTransportDetails)
					{
						AddTransportAtDepartureFieldSynchroniser();
						AddTOLCarrierIDFieldSynchroniser();
					}
				}

				AddConsigneeFieldSynchroniser();
				AddConsignorFieldSynchroniser();

				AddPrincipalSynchroniser();

				AddForeignDestPortSynchroniser();
				Synchronisers.Add(new FieldSynchroniser(Destination.MovementHeader.BM_PlaceOfUnloadingInfo, Source.JS_RL_NKDestinationInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.BH_UniqueVoyageIdentifierInfo, GetSourceCountriesOfRouting, GetSourceCountriesOfRoutingInfos)); // Itinerary

				// Don't swap the order of these two.
				// The containers on the header must be created, or at least hooked, before the line item.
				// Otherwise the part that forces a tick on the line's non-persistent container pivots loads the header containers collection too soon and the rsult is that no headercontainers are created.
				Synchronisers.Add(new NctsDepartureContainerCollectionSynchroniser(Source, Destination, RelevantConsol, Destination.ContainersAsICusInBondContainerCollectionForSynching, departureOrArrivalContainerFlag: ContainerTypeOfServiceList.Codes.DepartureContainer));
				AddShipmentToLineItemSynch();
			}
		}

		void AddConsignorFieldSynchroniser()
		{
			if (!Destination.DefaultConsignorConsigneeRegistryManager.IsRegistryEnabled())
			{
				Synchronisers.Add(new JobDocAddressSynchroniser(Destination.Consignor, Source.ConsignorDocumentaryAddress));
			}
			else
			{
				if (Destination.DefaultConsignorConsigneeRegistryManager.IsConsignor() && Source.JobDirection == Directions.Export)
				{
					Synchronisers.Add(new JobDocAddressAddressOnlySynchroniser(Destination.Consignor, (ZPropertyInfoGuid)Source.JS_OA_ExportReceivingDepotInfo));
				}
			}
		}

		void AddConsigneeFieldSynchroniser()
		{
			if (!Destination.DefaultConsignorConsigneeRegistryManager.IsRegistryEnabled())
			{
				Synchronisers.Add(new JobDocAddressSynchroniser(Destination.Consignee, Source.ConsigneeDocumentaryAddress));
			}
			else
			{
				if (Destination.DefaultConsignorConsigneeRegistryManager.IsConsignee() && Source.JobDirection == Directions.Import)
				{
					Synchronisers.Add(new JobDocAddressSynchroniser(Destination.Consignee, Source.ConsigneeDeliveryAddress));
				}
			}
		}

		void AddTOLCarrierIDFieldSynchroniser()
		{
			Synchronisers.Add(new FieldSynchroniser(Destination.MovementHeader.BM_TOLCarrierIDInfo, RelevantConsol.JK_JX_JV_VoyageFlightInfo));
		}

		void AddTransportAtDepartureFieldSynchroniser()
		{
			Synchronisers.Add(new FieldSynchroniser(Destination.MovementHeader.BM_TransportAtDepartureInfo, RelevantConsol.JK_JX_JV_VoyageFlightInfo));
		}

		void AddInlandTransportModeFieldSynchroniser()
		{
			Synchronisers.Add(new FieldSynchroniser(Destination.MovementHeader.BM_InlandTransportModeInfo, GetTransportModeConverted, GetTransportModeInfos));
		}

		protected virtual void AddExportTransportModeFieldSynchroniser()
		{
			Synchronisers.Add(new FieldSynchroniser(Destination.MovementHeader.BM_ExportTransportModeInfo, GetTransportModeConverted, GetTransportModeInfos));
		}

		void AddPrincipalSynchroniser()
		{
			if (Source.JobHeader != null && Source.JobHeader.LocalChargesAddr != null && !Destination.DefaultPrincipalRegistryManager.IsRegistryEnabled())
			{
				Synchronisers.Add(new JobDocAddressAddressOnlySynchroniser(Destination.Principal, (ZPropertyInfoGuid)Source.JobHeader.JH_OA_LocalChargesAddrInfo));
			}
		}

		ForwardingConsol RelevantConsol
		{
			get
			{
				ForwardingConsol result = null;
				if (Destination != null && !Destination.IsDeleted)
				{
					result = Destination.IsArrivalMovement ? Source.ArrivalConsol : Source.DepartureConsol;
				}
				return result;
			}
		}

		void AddShipmentToLineItemSynch()
		{
			NctsDepartureCargoDesc lineItem = null;
			var departureMovementHeader = Destination.MovementHeader;
			if (departureMovementHeader != null)
			{
				foreach (NctsDepartureCargoDesc existingLineItem in departureMovementHeader.GoodsItems.ToArray())
				{
					NctsDepartureCargoDesc lineItemThatShouldBeDeleted = null;
					if (existingLineItem.MoveHeader != null && existingLineItem.MoveHeader.Header != null && existingLineItem.MoveHeader.Header.PK == Destination.PK)
					{
						if (lineItem == null)
						{
							lineItem = existingLineItem;
						}
						else
						{
							if (lineItem.BY_LineNo > existingLineItem.BY_LineNo)
							{
								lineItemThatShouldBeDeleted = lineItem;
								lineItem = existingLineItem;
							}
							else
							{
								lineItemThatShouldBeDeleted = existingLineItem;
							}
						}
						if (lineItemThatShouldBeDeleted != null)
						{
							lineItemThatShouldBeDeleted.Delete();
						}
					}
				}

				var lineSynch = GetNewNctsDepartureGoodsItemSynchroniser(lineItem ?? departureMovementHeader.GoodsItems.AddNew());
				Synchronisers.Add(lineSynch);
			}
		}

		protected virtual void AddDestinationPortFieldSynchroniserForArrival()
		{
			Synchronisers.Add(new FieldSynchroniser(Destination.ArrivalMovementHeader.BM_RL_NKDestinationPortInfo, Source.JS_RL_NKDestinationInfo));
		}

		protected virtual void AddDestinationPortFieldSynchroniserForDeparture()
		{
			Synchronisers.Add(new FieldSynchroniser(Destination.MovementHeader.BM_RL_NKDestinationPortInfo, Source.JS_RL_NKDestinationInfo));
		}

		protected virtual void AddPortofDispatchSynchroniser()
		{
			Synchronisers.Add(new FieldSynchroniser(Destination.PortOfDispatchInfo, Source.JS_RL_NKOriginInfo));
		}

		protected virtual void AddImportLoadPortSynchroniser()
		{
			Synchronisers.Add(new FieldSynchroniser(Destination.BH_RL_NKImportLoadPortInfo, Source.JS_RL_NKOriginInfo));
		}

		protected virtual void AddForeignDestPortSynchroniser()
		{
			Synchronisers.Add(new FieldSynchroniser(Destination.MovementHeader.BM_RL_NKForeignDestPortInfo, Source.JS_RL_NKOriginInfo));
		}

		protected virtual ISynchroniser GetNewNctsDepartureGoodsItemSynchroniser(NctsDepartureCargoDesc goodsItem)
		{
			return new ShipmentToNctsDepartureGoodsItemSynchroniser(goodsItem, Source, hasCommonCountryOfDispatch: true, hasCommonCountryOfDestination: true, hasCommonConsignor: false, hasCommonConsignee: false, hasCommonCTStatus: true);
		}

		protected IEnumerable<ZPropertyInfo> GetTransportModeInfos()
		{
			yield return Source.JS_TransportModeInfo;
		}

		IZType GetTransportModeConverted() => Destination.TransportModeTranslator.TranslateToWCOCode(GetTransportMode());

		ZString GetTransportMode() => GetTransportModeCore();

		protected virtual ZString GetTransportModeCore()
		{
			var transportMode = Source.JS_TransportMode;

			if (!ShouldSynchroniseTransportDetails)
			{
				transportMode = TransportTypeList.Codes.Road;
			}

			return transportMode;
		}

		IZType GetSourceCountriesOfRouting()
		{
			return ZString.Join("", Source.CountriesOfRouting.ToArray());
		}

		IEnumerable<ZPropertyInfo> GetSourceCountriesOfRoutingInfos()
		{
			foreach (Transport leg in Source.Transports)
			{
				yield return leg.JW_RL_NKLoadPortInfo;
				yield return leg.JW_RL_NKDiscPortInfo;
			}
		}

		void Shipment_OnJobDeclarationCreated(object sender, JobDeclarationCreationEventArgs e)
		{
			Source.OnJobDeclarationCreated -= Shipment_OnJobDeclarationCreated;
		}

		protected override void DisposeCore()
		{
			Source.OnJobDeclarationCreated -= Shipment_OnJobDeclarationCreated;
			base.DisposeCore();
		}

		protected bool ShouldSynchroniseTransportDetails => SyncHelper.ShouldSyncTransportDetails(Source.JS_TransportMode, Destination.ShouldTransportDetailsSyncDependsOnTransportMode);
	}
}
