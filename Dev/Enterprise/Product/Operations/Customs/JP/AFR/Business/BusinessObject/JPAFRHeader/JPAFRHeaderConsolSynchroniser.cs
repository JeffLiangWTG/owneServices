using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class JPAFRHeaderConsolSynchroniser : BusinessObjectSynchroniser
	{
		public JPAFRHeaderConsolSynchroniser(JPAFRHeader destination)
			: base(destination, destination.Consol)
		{
		}

		protected new JPAFRHeader Destination
		{
			get { return (JPAFRHeader)base.Destination; }
		}

		protected new ForwardingConsol Source
		{
			get { return (ForwardingConsol)base.Source; }
		}

		#region Hook and Unhook FieldSynchronisers

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			carrierAddressSynchroniser = new FieldSynchroniser(Destination.Carrier.E2_OA_AddressInfo, GetCarrier, GetInfosAffectingCarrier);
			Synchronisers.Add(carrierAddressSynchroniser);
			carrierCodeSynchroniser = new FieldSynchroniser(Destination.JPH_CarrierCodeInfo, GetCarrierCode, GetInfosAffectingCarrier);
			Synchronisers.Add(carrierCodeSynchroniser);
			vesselSynchroniser = new FieldSynchroniser(Destination.JPH_VesselNameInfo, GetVessel, GetVesselInfo);
			Synchronisers.Add(vesselSynchroniser);
			voyageSynchroniser = new FieldSynchroniser(Destination.JPH_VoyageInfo, GetVoyage, GetVoyageInfo);
			Synchronisers.Add(voyageSynchroniser);
			loadingSynchroniser = new FieldSynchroniser(Destination.JPH_RL_NKLoadingInfo, GetLoading, GetLoadingInfo);
			Synchronisers.Add(loadingSynchroniser);
			dischargeSynchroniser = new FieldSynchroniser(Destination.JPH_RL_NKDischargeInfo, GetDischarge, GetDischargeInfo);
			Synchronisers.Add(dischargeSynchroniser);
			etdSynchroniser = new FieldSynchroniser(Destination.JPH_ETDInfo, GetETD, GetETDInfo);
			Synchronisers.Add(etdSynchroniser);
			etaSynchroniser = new FieldSynchroniser(Destination.JPH_ETAInfo, GetETA, GetETAInfo);
			Synchronisers.Add(etaSynchroniser);
			if (!Destination.JPH_IsShippingLineEntry)
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.JPH_MasterBillNumberInfo, GetMasterBillNumber, GetInfosAffectingMasterBillNumer));
			}

			var billsSynchroniser = new JPAFRBillsCollectionSynchroniser(Destination);
			Synchronisers.Add(billsSynchroniser);

			Source.Transports.CountChanged -= new CollectionCountChangedEventHandler(Transports_CountChanged);
			Source.Transports.CountChanged += new CollectionCountChangedEventHandler(Transports_CountChanged);
		}

		FieldSynchroniser carrierAddressSynchroniser;
		FieldSynchroniser carrierCodeSynchroniser;
		FieldSynchroniser vesselSynchroniser;
		FieldSynchroniser voyageSynchroniser;
		FieldSynchroniser loadingSynchroniser;
		FieldSynchroniser dischargeSynchroniser;
		FieldSynchroniser etdSynchroniser;
		FieldSynchroniser etaSynchroniser;

		protected override void UnHookSynchronisers()
		{
			Source.Transports.CountChanged -= new CollectionCountChangedEventHandler(Transports_CountChanged);
			carrierAddressSynchroniser = null;
			carrierCodeSynchroniser = null;
			vesselSynchroniser = null;
			voyageSynchroniser = null;
			loadingSynchroniser = null;
			dischargeSynchroniser = null;
			etdSynchroniser = null;
			etaSynchroniser = null;
			base.UnHookSynchronisers();
		}

		IZType GetMasterBillNumber()
		{
			var result = Source.JK_MasterBillNum.TrimStart();
			var shouldAddSCAC = JPAFRRegistry.Instance.AFRAddSCACtoBillsDuringSync.GetValueWithoutFallback(Destination.RegistryCompanyPK, Guid.Empty, Guid.Empty);
			var carrierCode = Destination.JPH_CarrierCode;
			if (shouldAddSCAC && !carrierCode.IsEmpty && !result.StartsWith(carrierCode))
			{
				result = carrierCode.PadRight(4, '-') + result.Right(31);
			}
			return result;
		}

		IEnumerable<ZPropertyInfo> GetInfosAffectingMasterBillNumer()
		{
			yield return Source.JK_MasterBillNumInfo;
			yield return Destination.JPH_CarrierCodeInfo;
			yield return Destination.JPH_GB_BranchInfo;
		}

		IZType GetCarrierCode()
		{
			return ConsolDataCalculator.GetSCAC(Destination.Carrier);
		}

		IZType GetCarrier()
		{
			ZPropertyInfo result = null;
			result = Source.JK_OA_ShippingLineAddressInfo;
			return result == null ? null : result.Value;
		}

		IEnumerable<ZPropertyInfo> GetInfosAffectingCarrier()
		{
			yield return Source.JK_OA_ShippingLineAddressInfo;
		}

		IZType GetVessel()
		{
			var leg = ConsolDataCalculator.FirstCountryBoundTransportOrFirstTransportWithTransportMode;
			return leg != null ? leg.JW_Vessel : ZString.Empty;
		}

		IEnumerable<ZPropertyInfo> GetVesselInfo()
		{
			foreach (var info in ConsolDataCalculator.GetInfosAffectingTransportsOrder())
			{
				yield return info;
			}

			foreach (var info in ConsolDataCalculator.GetTransportsInfos(Transport.Schema.JW_Vessel))
			{
				yield return info;
			}
		}

		IZType GetVoyage()
		{
			var leg = ConsolDataCalculator.FirstCountryBoundTransportOrFirstTransportWithTransportMode;
			return leg != null ? leg.JW_VoyageFlight : ZString.Empty;
		}

		IEnumerable<ZPropertyInfo> GetVoyageInfo()
		{
			foreach (var info in ConsolDataCalculator.GetInfosAffectingTransportsOrder())
			{
				yield return info;
			}

			foreach (var info in ConsolDataCalculator.GetTransportsInfos(Transport.Schema.JW_VoyageFlight))
			{
				yield return info;
			}
		}

		IZType GetLoading()
		{
			var leg = ConsolDataCalculator.FirstCountryBoundTransportOrFirstTransportWithTransportMode;
			return leg == null ? ZString.Empty : leg.JW_RL_NKLoadPort;
		}

		IEnumerable<ZPropertyInfo> GetLoadingInfo()
		{
			foreach (var info in ConsolDataCalculator.GetInfosAffectingTransportsOrder())
			{
				yield return info;
			}

			foreach (var info in ConsolDataCalculator.GetTransportsInfos(Transport.Schema.JW_RL_NKLoadPort))
			{
				yield return info;
			}
		}

		IZType GetDischarge()
		{
			var port = ConsolDataCalculator.FirstCountryPortOfDischarge;
			return port != null ? port.RL_Code : ZString.Empty;
		}

		IEnumerable<ZPropertyInfo> GetDischargeInfo()
		{
			return ConsolDataCalculator.GetInfosAffectingFirstCountryPortOfDischarge();
		}

		IZType GetETD()
		{
			var leg = ConsolDataCalculator.FirstCountryBoundTransportOrFirstTransportWithTransportMode;
			var result = ZDateTime.Empty;
			if (leg != null)
			{
				result = leg.JW_ATD.IsEmpty ? leg.JW_ETD : leg.JW_ATD;
			}
			return result;
		}

		IEnumerable<ZPropertyInfo> GetETDInfo()
		{
			foreach (var info in ConsolDataCalculator.GetInfosAffectingTransportsOrder())
			{
				yield return info;
			}

			foreach (var info in ConsolDataCalculator.GetTransportsInfos(Transport.Schema.JW_ATD, Transport.Schema.JW_ETD))
			{
				yield return info;
			}
		}

		IZType GetETA()
		{
			return ConsolDataCalculator.FirstCountryDischargeDate;
		}

		IEnumerable<ZPropertyInfo> GetETAInfo()
		{
			return ConsolDataCalculator.GetInfosAffectingFirstCountryDischargeDate();
		}

		void Transports_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			UpdateInfoEventsAndReSynchronise(carrierAddressSynchroniser);
			UpdateInfoEventsAndReSynchronise(carrierCodeSynchroniser);
			UpdateInfoEventsAndReSynchronise(vesselSynchroniser);
			UpdateInfoEventsAndReSynchronise(voyageSynchroniser);
			UpdateInfoEventsAndReSynchronise(loadingSynchroniser);
			UpdateInfoEventsAndReSynchronise(dischargeSynchroniser);
			UpdateInfoEventsAndReSynchronise(etdSynchroniser);
			UpdateInfoEventsAndReSynchronise(etaSynchroniser);
		}

		#endregion

		#region Implementation

		ConsolDataCalculator ConsolDataCalculator
		{
			get { return consolDataCalculator ?? (consolDataCalculator = new ConsolDataCalculator(Source, Destination)); }
		}
		ConsolDataCalculator consolDataCalculator;

		protected override void DisposeCore()
		{
			base.DisposeCore();
			if (consolDataCalculator != null)
			{
				consolDataCalculator.Dispose();
				consolDataCalculator = null;
			}
		}

		#endregion
	}
}
