using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsPhase5DepartureConsolSynchroniser : BusinessObjectSynchroniser
	{
		public NctsPhase5DepartureConsolSynchroniser(NctsHeader nctsHeader, ICusInBondParent source)
			: base(nctsHeader, (BusinessObject)source)
		{
			Source.OnUpdatedByDataRefreshForCustomsSynchronisation += HookedConsol_OnUpdatedByDataRefreshForCustomsSynchronisation;
		}

		protected new NctsHeader Destination => (NctsHeader)base.Destination;

		protected new ForwardingConsol Source => (ForwardingConsol)base.Source;

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();

			var departureMovementHeader = Destination.MovementHeader;

			Synchronisers.Add(new FieldSynchroniser(departureMovementHeader.InlandTransportModeAtDepartureInfo, GetTransportModeConverted, GetTransportModeInfos));

			if (ShouldSynchroniseTransportDetails)
			{
				Synchronisers.Add(new FieldSynchroniser(departureMovementHeader.BM_ExportTransportModeInfo, GetTransportModeConverted, GetTransportModeInfos));
				if (GetTransportModeConverted().ToString() != ModeOfTransportList.Codes._5_PostalConsignment)
				{
					Synchronisers.Add(new FieldSynchroniser(departureMovementHeader.BM_ConveyanceNumberInfo, Source.JK_JX_JV_VoyageFlightInfo));
					Synchronisers.Add(new FieldSynchroniser(departureMovementHeader.BM_TOLCarrierIDInfo, Source.JK_JX_JV_VoyageFlightInfo));
				}
			}

			if (!Destination.DefaultPrincipalRegistryManager.IsRegistryEnabled())
			{
				Synchronisers.Add(new JobDocAddressAddressOnlySynchroniser(Destination.Principal, (ZPropertyInfoGuid)Source.JK_OA_SendingForwarderAddressInfo));
			}

			Synchronisers.Add(new FieldSynchroniser(departureMovementHeader.BM_ForeignDestPortKCodeInfo, Source.JK_JX_JA_RL_NKPortOfLoadingInfo));

			foreach (ForwardingShipment shipment in Source.Shipments)
			{
				Synchronisers.Add(new NctsPhase5DepartureContainerCollectionSynchroniser(shipment, Destination, Source, Destination.ContainersAsICusInBondContainerCollectionForSynching));
			}

			Synchronisers.Add(new NctsPhase5DepartureConsolShipmentsSynchroniser(Destination, Source));

			var jobDirection = Source.JobDirection;
			if (Destination.DefaultConsignorConsigneeRegistryManager.IsConsignor() && jobDirection == Directions.Export && Destination.Consignor.IsEmpty)
			{
				Synchronisers.Add(new JobDocAddressAddressOnlySynchroniser(Destination.Consignor, (ZPropertyInfoGuid)Source.JK_OA_PackDepotAddressInfo));
			}

			if (Destination.DefaultConsignorConsigneeRegistryManager.IsConsignee() && jobDirection == Directions.Import && Destination.Consignee.IsEmpty)
			{
				Synchronisers.Add(new JobDocAddressAddressOnlySynchroniser(Destination.Consignee, (ZPropertyInfoGuid)Source.JK_OA_UnpackDepotAddressInfo));
			}

			if (!Destination.IsInPhase5TransitionPeriod && !Source.JK_MasterBillNum.IsEmpty)
			{
				AddTransportDocumentToSync();
			}
		}

		void AddTransportDocumentToSync()
		{
			var transportDocuments = Destination.AdditionalDocuments.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument).ToArray();
			transportDocuments.Skip(1).DeleteAll();
			var lineSynch = GetNewNctsConsolToTransportDocumentPhase5DepartureSynchroniser(transportDocuments.FirstOrDefault() ?? Destination.AdditionalDocuments.AddNew());
			Synchronisers.Add(lineSynch);
		}

		protected virtual ISynchroniser GetNewNctsConsolToTransportDocumentPhase5DepartureSynchroniser(NctsAdditionalInfo transportDocument) => new NctsPhase5ConsolToTransportDocumentPhase5DepartureSynchroniser(transportDocument, Source);

		IEnumerable<ZPropertyInfo> GetTransportModeInfos()
		{
			yield return Source.JK_TransportModeInfo;
		}

		IZType GetTransportModeConverted() => Destination.TransportModeTranslator.TranslateToWCOCode(GetTransportMode());

		ZString GetTransportMode() => SyncHelper.GetCalculatedTransportMode(Source.JK_TransportMode);

		bool ShouldSynchroniseTransportDetails => SyncHelper.ShouldSyncTransportDetails(Source.JK_TransportMode, Destination.ShouldTransportDetailsSyncDependsOnTransportMode);

		void HookedConsol_OnUpdatedByDataRefreshForCustomsSynchronisation(object sender, EventArgs e)
		{
			Source.OnUpdatedByDataRefreshForCustomsSynchronisation -= HookedConsol_OnUpdatedByDataRefreshForCustomsSynchronisation;
		}

		protected override void DisposeCore()
		{
			Source.OnUpdatedByDataRefreshForCustomsSynchronisation -= HookedConsol_OnUpdatedByDataRefreshForCustomsSynchronisation;
			base.DisposeCore();
		}
	}
}
