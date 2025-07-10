using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaManifestHeaderSynchroniser : BusinessObjectSynchroniser
	{
		public AsycudaManifestHeaderSynchroniser(AsycudaManifestHeader destination, ForwardingConsol sourceConsol)
			: base(destination, sourceConsol)
		{ }

		protected new AsycudaManifestHeader Destination => (AsycudaManifestHeader)base.Destination;

		protected new ForwardingConsol Source => (ForwardingConsol)base.Source;

		protected Customs.Business.ConsolDataCalculator ConsolDataCalculator
		{
			get { return new ConsolDataCalculator(Source, Destination.AMA_RN_NKCountry); }
		}

		protected virtual BusinessObjectCollectionSynchroniser GetNewBillCollectionSynchroniser()
		{
			return new AsycudaBillCollectionSynchroniser(Destination);
		}

		protected virtual BusinessObjectCollectionSynchroniser GetNewConsolContainerCollectionSynchroniser()
			=> new AsycudaConsolContainerCollectionSynchroniser(Source, Destination);

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();

			Synchronisers.Add(transportModeSynchroniser = new FieldSynchroniser(Destination.AMA_TransportModeInfo, GetTransportMode, GetSourceInfosAffectingTransportMode));
			Synchronisers.Add(carrierSynchroniser = new FieldSynchroniser(Destination.AMA_OA_CarrierInfo, GetCarrier, GetSourceInfosAffectingCarrier));
			SynchronisersAMA_VesselName();
			SynchronisersAMA_Voyage();
			Synchronisers.Add(vehicleRegistrationSynchroniser = new FieldSynchroniser(Destination.AMA_VehicleRegistrationInfo, GetVehicleRegistration, GetSourceInfosAffectingVehicleRegistration));
			Synchronisers.Add(portOfDischargeSynchroniser = new FieldSynchroniser(Destination.AMA_RL_NKPortOfDischargeInfo, GetPortOfDischarge, GetSourceInfosAffectingPortOfDischarge));
			Synchronisers.Add(portOfFirstArrivalSynchroniser = new FieldSynchroniser(Destination.AMA_RL_NKPortOfFirstArrivalInfo, GetPortOfFirstArrival, GetSourceInfosAffectingPortOfFirstArrival));
			SynchronisersAMA_RL_NKPortOfLoading();
			Synchronisers.Add(etaSynchroniser = new FieldSynchroniser(Destination.AMA_E_ARVInfo, GetETA, GetSourceInfosAffectingETA));
			Synchronisers.Add(etdSynchroniser = new FieldSynchroniser(Destination.AMA_E_DEPInfo, GetETD, GetSourceInfosAffectingETD));

			Synchronisers.Add(GetNewConsolContainerCollectionSynchroniser());   // Must come before Bill and Pack, otherwise we try to synch bills and bills' packs before containers, so can't fully wire. Instead hook containers first bwefore hooking bills & bills' packs, so when we come to synch pack the conts already exist.
			var billsSynchroniser = GetNewBillCollectionSynchroniser();
			Synchronisers.Add(billsSynchroniser);

			var containers = Source.Containers;
			containers.CountChanged -= new CollectionCountChangedEventHandler(Containers_CountChanged);
			containers.CountChanged += new CollectionCountChangedEventHandler(Containers_CountChanged);
			var transports = Source.Transports;
			transports.CountChanged -= new CollectionCountChangedEventHandler(Transports_CountChanged);
			transports.CountChanged += new CollectionCountChangedEventHandler(Transports_CountChanged);

			foreach (CommonContainer container in containers)
			{
				container.JC_ContainerModeInfo.ValueChanged -= JC_ContainerModeInfo_ValueChanged;
				container.JC_ContainerModeInfo.ValueChanged += JC_ContainerModeInfo_ValueChanged;
			}

			SynchronisersAMA_AgentType();

			var agent = Source.JK_AgentTypeInfo;
			agent.ValueChanged -= JK_AgentTypeInfo_ValueChanged;
			agent.ValueChanged += JK_AgentTypeInfo_ValueChanged;

			SynchronisersShippingAgent();
			SynchronisersMasterBOL();
			SynchronisersMasterBill();

			Synchronisers.Add(new FieldSynchroniser(Destination.AMA_ContainerModeInfo, GetConvertedSourceContainerMode, GetSourceInfosAffectingConvertedContainerMode));
			Synchronisers.Add(new FieldSynchroniser(Destination.AMA_MasterBillIssueDateInfo, GetIssueDate, GetSourceInfosAffectingIssueDate));
		}

		protected virtual void SynchronisersShippingAgent()
		{
			Synchronisers.Add(new FieldSynchroniser(GetTargetShippingAgentAtLoad, GetSourceShippingAgentAtLoad, GetSourceInfosAffectingShippingAgentAtLoad, () => false));
			Synchronisers.Add(new FieldSynchroniser(GetTargetShippingAgentAtDischarge, GetSourceShippingAgentAtDischarge, GetSourceInfosAffectingShippingAgentAtDischarge, () => false));
		}

		protected virtual void SynchronisersAMA_VesselName()
		{
			Synchronisers.Add(vesselSynchroniser = new FieldSynchroniser(Destination.AMA_VesselNameInfo, GetVessel, GetSourceInfosAffectingVessel));
		}

		protected virtual void SynchronisersAMA_Voyage()
		{
			Synchronisers.Add(voyageSynchroniser = new FieldSynchroniser(Destination.AMA_VoyageInfo, GetVoyage, GetSourceInfosAffectingVoyage));
		}

		protected virtual void SynchronisersAMA_AgentType()
		{
			Synchronisers.Add(new FieldSynchroniser(Destination.AMA_AgentTypeInfo, Source.JK_AgentTypeInfo));
		}

		protected virtual void SynchronisersAMA_RL_NKPortOfLoading()
		{
			Synchronisers.Add(portOfLoadingSynchroniser = new FieldSynchroniser(Destination.AMA_RL_NKPortOfLoadingInfo, GetPortOfLoading, GetSourceInfosAffectingPortOfLoading));
		}

		protected virtual void SynchronisersMasterBOL()
		{
			Synchronisers.Add(mblSynchroniser = new FieldSynchroniser(Destination.MasterBOLInfo, Source.JK_CoLoadMasterBillInfo));
		}

		protected virtual void SynchronisersMasterBill()
		{
			Synchronisers.Add(new FieldSynchroniser(Destination.AMA_MasterBillInfo, Source.JK_MasterBillNumInfo));
		}

		FieldSynchroniser transportModeSynchroniser;
		FieldSynchroniser carrierSynchroniser;
		FieldSynchroniser vehicleRegistrationSynchroniser;
		FieldSynchroniser portOfDischargeSynchroniser;
		FieldSynchroniser etaSynchroniser;
		FieldSynchroniser etdSynchroniser;
		FieldSynchroniser mblSynchroniser;
		FieldSynchroniser portOfFirstArrivalSynchroniser;
		FieldSynchroniser vesselSynchroniser;
		FieldSynchroniser voyageSynchroniser;
		FieldSynchroniser portOfLoadingSynchroniser;

		#region Hook Events

		protected override void UnHookSynchronisers()
		{
			foreach (CommonContainer container in Source.Containers)
			{
				container.JC_ContainerModeInfo.ValueChanged -= JC_ContainerModeInfo_ValueChanged;
			}
			Source.Transports.CountChanged -= new CollectionCountChangedEventHandler(Transports_CountChanged);
			Source.Containers.CountChanged -= new CollectionCountChangedEventHandler(Containers_CountChanged);
			transportModeSynchroniser = null;
			carrierSynchroniser = null;
			vesselSynchroniser = null;
			voyageSynchroniser = null;
			vehicleRegistrationSynchroniser = null;
			portOfDischargeSynchroniser = null;
			portOfLoadingSynchroniser = null;
			portOfFirstArrivalSynchroniser = null;
			etaSynchroniser = null;
			etdSynchroniser = null;
			mblSynchroniser = null;
			base.UnHookSynchronisers();
		}

		protected virtual void Transports_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			UpdateInfoEventsAndReSynchronise(carrierSynchroniser);
			UpdateInfoEventsAndReSynchronise(vesselSynchroniser);
			UpdateInfoEventsAndReSynchronise(voyageSynchroniser);
			UpdateInfoEventsAndReSynchronise(vehicleRegistrationSynchroniser);
			UpdateInfoEventsAndReSynchronise(portOfDischargeSynchroniser);
			UpdateInfoEventsAndReSynchronise(portOfLoadingSynchroniser);
			UpdateInfoEventsAndReSynchronise(portOfFirstArrivalSynchroniser);
			UpdateInfoEventsAndReSynchronise(etaSynchroniser);
			UpdateInfoEventsAndReSynchronise(etdSynchroniser);
		}

		void Containers_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			var container = e.BizObject as CommonContainer;
			if (container != null)
			{
				container.JC_ContainerModeInfo.ValueChanged -= JC_ContainerModeInfo_ValueChanged;
				if (e.ItemAdded)
				{
					container.JC_ContainerModeInfo.ValueChanged += JC_ContainerModeInfo_ValueChanged;
				}
			}
			UpdateInfoEventsAndReSynchronise(transportModeSynchroniser);
		}

		void JC_ContainerModeInfo_ValueChanged(object sender, System.EventArgs e)
		{
			UpdateInfoEventsAndReSynchronise(transportModeSynchroniser);
		}

		void JK_AgentTypeInfo_ValueChanged(object sender, System.EventArgs e)
		{
			UpdateInfoEventsAndReSynchronise(mblSynchroniser);
		}

		#endregion

		#region Transport Mode

		IZType GetTransportMode() => Source.JK_TransportMode;

		protected ZPropertyInfo[] GetSourceInfosAffectingTransportMode() => new[] { Source.JK_TransportModeInfo };

		#endregion

		#region Carrier

		protected virtual IZType GetCarrier() => GetRelevantTransportByNature()?.JW_OA_CarrierAddress ?? ZGuid.Empty;

		protected virtual IEnumerable<ZPropertyInfo> GetSourceInfosAffectingCarrier() => GetSourceInfosAffectingTransportField(JobConsolTransportSchema.Constants.JW_OA_CarrierAddress);

		#endregion

		#region Vessel

		protected virtual IZType GetVessel() => GetRelevantTransportByNature()?.JW_Vessel.ToUpper() ?? ZString.Empty;

		protected virtual IEnumerable<ZPropertyInfo> GetSourceInfosAffectingVessel() => GetSourceInfosAffectingTransportField(JobConsolTransportSchema.Constants.JW_Vessel);

		#endregion

		#region Voyage

		protected virtual IZType GetVoyage()
		{
			ZString result;
			if (!Core.Constants.TransportModes.Road.Equals(GetTransportMode()))
			{
				result = GetRelevantTransportByNature()?.JW_VoyageFlight.ToUpper() ?? ZString.Empty;
			}
			else
			{
				result = ZString.Empty;
			}
			return result;
		}

		IEnumerable<ZPropertyInfo> GetSourceInfosAffectingVoyage()
		{
			foreach (var info in GetSourceInfosAffectingTransportField(Transport.Schema.JW_VoyageFlight))
			{
				yield return info;
			}

			foreach (var info in GetSourceInfosAffectingTransportMode())
			{
				yield return info;
			}
		}

		#endregion Voyage

		#region Vehicle Registration

		IZType GetVehicleRegistration()
		{
			ZString result;
			if (Core.Constants.TransportModes.Road.Equals(GetTransportMode()))
			{
				result = GetRelevantTransportByNature()?.JW_VoyageFlight.ToUpper() ?? ZString.Empty;
			}
			else
			{
				result = ZString.Empty;
			}
			return result;
		}

		IEnumerable<ZPropertyInfo> GetSourceInfosAffectingVehicleRegistration()
		{
			foreach (var info in GetSourceInfosAffectingTransportField(Transport.Schema.JW_VoyageFlight))
			{
				yield return info;
			}

			foreach (var info in GetSourceInfosAffectingTransportMode())
			{
				yield return info;
			}
		}

		#endregion

		#region PortOfLoading

		protected virtual IZType GetPortOfLoading() => GetRelevantTransportByNature()?.JW_RL_NKLoadPort ?? ZString.Empty;

		protected virtual IEnumerable<ZPropertyInfo> GetSourceInfosAffectingPortOfLoading() => GetSourceInfosAffectingTransportField(JobConsolTransportSchema.Constants.JW_RL_NKLoadPort);

		#endregion

		#region PortOfDischarge

		protected virtual IZType GetPortOfDischarge() => GetRelevantTransportByNature()?.JW_RL_NKDiscPort ?? ZString.Empty;

		protected virtual IEnumerable<ZPropertyInfo> GetSourceInfosAffectingPortOfDischarge() => GetSourceInfosAffectingTransportField(JobConsolTransportSchema.Constants.JW_RL_NKDiscPort);

		#endregion

		#region PortOfFirstArrival

		protected virtual IZType GetPortOfFirstArrival() => Source.JK_RL_NKPortOfFirstArrival;

		protected virtual IEnumerable<ZPropertyInfo> GetSourceInfosAffectingPortOfFirstArrival() => new [] { Source.JK_RL_NKPortOfFirstArrivalInfo };

		#endregion

		#region ETA

		IZType GetETA()
		{
			IZType result;
			var transport = GetRelevantTransportByNature();
			if (transport != null)
			{
				result = transport.JW_ATA.IsEmpty ? transport.JW_ETA : transport.JW_ATA;
			}
			else
			{
				result = ZDateTime.Empty;
			}
			return result;
		}

		IEnumerable<ZPropertyInfo> GetSourceInfosAffectingETA()
		{
			foreach (var info in ConsolDataCalculator.GetInfosAffectingTransportsOrder())
			{
				yield return info;
			}

			foreach (var info in ConsolDataCalculator.GetTransportsInfos(JobConsolTransportSchema.Constants.JW_ETA, JobConsolTransportSchema.Constants.JW_ATA))
			{
				yield return info;
			}

			yield return Destination.AMA_NatureInfo;
		}

		#endregion

		#region ETD

		IZType GetETD()
		{
			IZType result;
			var transport = GetRelevantTransportByNature();
			if (transport != null)
			{
				result = transport.JW_ATD.IsEmpty ? transport.JW_ETD : transport.JW_ATD;
			}
			else
			{
				result = ZDateTime.Empty;
			}
			return result;
		}

		IEnumerable<ZPropertyInfo> GetSourceInfosAffectingETD()
		{
			foreach (var info in ConsolDataCalculator.GetInfosAffectingTransportsOrder())
			{
				yield return info;
			}

			foreach (var info in ConsolDataCalculator.GetTransportsInfos(JobConsolTransportSchema.Constants.JW_ETD, JobConsolTransportSchema.Constants.JW_ATD))
			{
				yield return info;
			}

			yield return Destination.AMA_NatureInfo;
		}

		#endregion

		#region IssueDate

		protected IZType GetIssueDate() => Source.JK_MasterBillIssueDate.Date;

		protected IEnumerable<ZPropertyInfo> GetSourceInfosAffectingIssueDate() => new[] { Source.JK_MasterBillIssueDateInfo };

		#endregion

		#region ContainerMode

		IZType GetConvertedSourceContainerMode()
		{
			var consolMode = Source.JK_ConsolMode;
			switch (consolMode)
			{
				case Core.Constants.ContainerModes.BreakBulk:
				case Core.Constants.ContainerModes.Bulk:
				case Core.Constants.ContainerModes.Containerised:
				case Core.Constants.ContainerModes.Liquid:
				case Core.Constants.ContainerModes.Other:
					return consolMode;

				case Core.Constants.ContainerModes.FCL:
				case Core.Constants.ContainerModes.LCL:
				case Core.Constants.ContainerModes.FTL:
				case Core.Constants.ContainerModes.LTL:
				case Core.Constants.ContainerModes.Groupage:
				case Core.Constants.ContainerModes.BuyersConsol:
					return (ZString)Core.Constants.ContainerModes.Containerised;

				default:
					return (ZString)Core.Constants.ContainerModes.Other;
			}
		}

		protected IEnumerable<ZPropertyInfo> GetSourceInfosAffectingConvertedContainerMode() => new[] { Source.JK_ConsolModeInfo };

		#endregion

		#region ShippingAgentAtLoad

		protected virtual ZPropertyInfo GetTargetShippingAgentAtLoad()
		{
			var consolLoad = Source.JK_RL_NKLoadPort.Left(2);
			return Destination.AMA_RN_NKCountry == consolLoad ? Destination.AMA_OA_ShippingAgentInfo : null;
		}

		IZType GetSourceShippingAgentAtLoad()
		{
			return Source.SendingForwarderAddress?.PK;
		}

		IEnumerable<ZPropertyInfo> GetSourceInfosAffectingShippingAgentAtLoad()
		{
			yield return Source.JK_OA_SendingForwarderAddressInfo;
			yield return Source.JK_RL_NKLoadPortInfo;
		}

		#endregion

		#region ShippingAgentAtDischarge()

		protected virtual ZPropertyInfo GetTargetShippingAgentAtDischarge()
		{
			var consolDischarge = Source.JK_RL_NKDischargePort.Left(2);
			return Destination.AMA_RN_NKCountry == consolDischarge ? Destination.AMA_OA_ShippingAgentInfo : null;
		}

		IZType GetSourceShippingAgentAtDischarge()
		{
			return Source.ReceivingForwarderAddress?.PK;  // We lose address info here. When we come to serialise, we'll have to assume Agent.MainAddress in order to get address details.
		}

		IEnumerable<ZPropertyInfo> GetSourceInfosAffectingShippingAgentAtDischarge()
		{
			yield return Source.JK_OA_ReceivingForwarderAddressInfo;
			yield return Source.JK_RL_NKDischargePortInfo;
		}

		#endregion

		protected virtual Transport GetRelevantTransportByNature()
		{
			Transport transport;

			if (Destination.AMA_Nature == ShipmentTypeList.Codes.Export22)
			{
				transport = ConsolDataCalculator.LastCountryDepartureTransportOrLastTransportWithTransportMode;
			}
			else
			{
				transport = ConsolDataCalculator.FirstCountryBoundTransportOrFirstTransportWithTransportMode;
			}

			return transport;
		}

		protected IEnumerable<ZPropertyInfo> GetSourceInfosAffectingTransportField(string schemaColumn)
		{
			foreach (var info in ConsolDataCalculator.GetInfosAffectingTransportsOrder())
			{
				yield return info;
			}

			foreach (var info in ConsolDataCalculator.GetTransportsInfos(schemaColumn))
			{
				yield return info;
			}

			yield return Destination.AMA_NatureInfo;
		}
	}
}
