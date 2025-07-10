using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsPhase5DepartureShipmentSynchroniser : BusinessObjectSynchroniser
	{
		public NctsPhase5DepartureShipmentSynchroniser(NctsHeader nctsHeader, ICusInBondParent source)
			: base(nctsHeader, (BusinessObject)source)
		{
			Source.OnJobDeclarationCreated += Shipment_OnJobDeclarationCreated;
		}

		protected new NctsHeader Destination => (NctsHeader)base.Destination;

		protected new ForwardingShipment Source => (ForwardingShipment)base.Source;

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();

			var departureMovementHeader = Destination.MovementHeader;
			Synchronisers.Add(new FieldSynchroniser(departureMovementHeader.BM_AdditionalTextInfo, Source.JS_UniqueConsignRefInfo));
			Synchronisers.Add(new FieldSynchroniser(departureMovementHeader.BM_PortOfPresentationCodeInfo, Source.JS_RL_NKOriginInfo));
			Synchronisers.Add(new FieldSynchroniser(departureMovementHeader.BM_PlaceOfLoadingInfo, Source.JS_RL_NKOriginInfo));
			Synchronisers.Add(new FieldSynchroniser(departureMovementHeader.BM_InlandTransportModeInfo, GetTransportModeConverted, GetTransportModeInfos));

			var shouldSynchroniseTransportDetails = ShouldSynchroniseTransportDetails;
			if (shouldSynchroniseTransportDetails)
			{
				Synchronisers.Add(new FieldSynchroniser(departureMovementHeader.BM_ExportTransportModeInfo, GetTransportModeConverted, GetTransportModeInfos));
			}

			var relevantConsol = RelevantConsol;
			if (relevantConsol != null)
			{
				Synchronisers.Add(new FieldSynchroniser(departureMovementHeader.BM_ConveyanceNumberInfo, relevantConsol.JK_JX_JV_VoyageFlightInfo));
				if (shouldSynchroniseTransportDetails)
				{
					Synchronisers.Add(new FieldSynchroniser(departureMovementHeader.BM_TransportAtDepartureInfo, relevantConsol.JK_JX_JV_VoyageFlightInfo));
					Synchronisers.Add(new FieldSynchroniser(departureMovementHeader.BM_TOLCarrierIDInfo, relevantConsol.JK_JX_JV_VoyageFlightInfo));
				}
			}

			AddConsigneeFieldSynchroniser();
			AddConsignorFieldSynchroniser();

			AddPrincipalSynchroniser();

			Synchronisers.Add(new FieldSynchroniser(departureMovementHeader.BM_ForeignDestPortKCodeInfo, Source.JS_RL_NKDestinationInfo));
			Synchronisers.Add(new FieldSynchroniser(departureMovementHeader.BM_PlaceOfUnloadingInfo, Source.JS_RL_NKDestinationInfo));
			Synchronisers.Add(new FieldSynchroniser(departureMovementHeader.BM_GrossWeightInfo, Source.JS_ActualWeightInfo));
			Synchronisers.Add(new FieldSynchroniser(departureMovementHeader.BM_GrossWeightUQInfo, Source.JS_UnitOfWeightInfo));

			Synchronisers.Add(new NctsPhase5DepartureContainerCollectionSynchroniser(Source, Destination, relevantConsol, Destination.ContainersAsICusInBondContainerCollectionForSynching));
			AddShipmentToLineItemSynch();
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

		void AddPrincipalSynchroniser()
		{
			if (Source.JobHeader is JobHeader jobHeader && jobHeader.LocalChargesAddr != null && !Destination.DefaultPrincipalRegistryManager.IsRegistryEnabled())
			{
				Synchronisers.Add(new JobDocAddressAddressOnlySynchroniser(Destination.Principal, (ZPropertyInfoGuid)jobHeader.JH_OA_LocalChargesAddrInfo));
			}
		}

		ForwardingConsol RelevantConsol => Destination != null && !Destination.IsDeleted ? Source.DepartureConsol : null;

		void AddShipmentToLineItemSynch()
		{
			var bills = Destination.Bills.OrderBy(x => x.SequenceNumber).ToArray();
			bills.Skip(1).DeleteAll();
			var lineSynch = GetNewNctsShipmentToBillPhase5DepartureSynchroniser(bills.FirstOrDefault() ?? Destination.Bills.AddNew());
			Synchronisers.Add(lineSynch);
		}

		protected virtual ISynchroniser GetNewNctsShipmentToBillPhase5DepartureSynchroniser(NctsBill bill) => new NctsPhase5DepartureShipmentToBillSynchroniser(bill, Source);

		IEnumerable<ZPropertyInfo> GetTransportModeInfos()
		{
			yield return Source.JS_TransportModeInfo;
		}

		IZType GetTransportModeConverted() => Destination.TransportModeTranslator.TranslateToWCOCode(GetTransportMode());

		ZString GetTransportMode() => SyncHelper.GetCalculatedTransportMode(Source.JS_TransportMode);

		bool ShouldSynchroniseTransportDetails => SyncHelper.ShouldSyncTransportDetails(Source.JS_TransportMode, Destination.ShouldTransportDetailsSyncDependsOnTransportMode);

		void Shipment_OnJobDeclarationCreated(object sender, JobDeclarationCreationEventArgs e)
		{
			Source.OnJobDeclarationCreated -= Shipment_OnJobDeclarationCreated;
		}

		protected override void DisposeCore()
		{
			Source.OnJobDeclarationCreated -= Shipment_OnJobDeclarationCreated;
			base.DisposeCore();
		}
	}
}
