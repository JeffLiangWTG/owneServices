using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.EU.Manifest.ICS2.Business.SynchroniserHelper;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AsycudaManifestHeaderSynchroniser : ASYCUDA.Business.AsycudaManifestHeaderSynchroniser
	{
		public AsycudaManifestHeaderSynchroniser(AsycudaManifestHeader destination, ForwardingConsol sourceConsol)
			: base(destination, sourceConsol)
		{
		}

		protected new AsycudaManifestHeader Destination => (AsycudaManifestHeader)base.Destination;

		protected override BusinessObjectCollectionSynchroniser GetNewBillCollectionSynchroniser() => new AsycudaBillCollectionSynchroniser(Destination);

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();

			Synchronisers.Add(new RouteEntryCollectionSynchroniser(Source, Destination));
		}

		protected override IZType GetPortOfLoading()
		{
			if (Source.IsSea)
			{
				var firstTransportOfVesselArrivingInEuCountry = GetFirstTransportOfFirstVesselArrivingInMemberOfICS2(Source);
				if (firstTransportOfVesselArrivingInEuCountry is not null)
				{
					return firstTransportOfVesselArrivingInEuCountry.JW_RL_NKLoadPort;
				}
			}
			return Source.JK_RL_NKLoadPort;
		}

		protected override IEnumerable<ZPropertyInfo> GetSourceInfosAffectingPortOfLoading()
		{
			foreach (var propertyInfo in ConsolDataCalculator.GetInfosAffectingTransportsOrder())
			{
				yield return propertyInfo;
			}

			yield return Source.JK_TransportModeInfo;
			yield return Source.JK_RL_NKLoadPortInfo;
		}

		protected override IZType GetPortOfDischarge()
		{
			if (Source.IsSea)
			{
				return GetLastSeaTransport(Source)?.JW_RL_NKDiscPort;
			}
			return base.GetPortOfDischarge();
		}

		protected override IEnumerable<ZPropertyInfo> GetSourceInfosAffectingPortOfDischarge()
		{
			return base.GetSourceInfosAffectingPortOfDischarge().Append(Source.JK_TransportModeInfo);
		}

		protected override IZType GetPortOfFirstArrival()
		{
			if (Source.IsSea)
			{
				return GetFirstSeaTransportWithDischargePortInEuCountry(Source)?.JW_RL_NKDiscPort;
			}
			return base.GetPortOfFirstArrival();
		}

		protected override IEnumerable<ZPropertyInfo> GetSourceInfosAffectingPortOfFirstArrival()
		{
			foreach (var propertyInfo in ConsolDataCalculator.GetInfosAffectingTransportsOrder())
			{
				yield return propertyInfo;
			}

			foreach (var propertyInfo in base.GetSourceInfosAffectingPortOfFirstArrival())
			{
				yield return propertyInfo;
			}

			yield return Source.JK_TransportModeInfo;
		}

		protected override Transport GetRelevantTransportByNature()
		{
			if (Source.JK_TransportMode == Core.Constants.TransportModes.Sea)
			{
				var seaTransport = GetFirstSeaTransportWithDischargePortInMemberOfICS2(Source);
				return seaTransport ?? base.GetRelevantTransportByNature();
			}

			return base.GetRelevantTransportByNature();
		}

		protected override IZType GetVessel()
		{
			return Source.JK_TransportMode == Core.Constants.TransportModes.Sea
				? GetRelevantTransportByNature()?.JW_Vessel.ToUpper() ?? ZString.Empty
				: ZString.Empty;
		}

		protected override IEnumerable<ZPropertyInfo> GetSourceInfosAffectingVessel()
		{
			foreach (var info in GetSourceInfosAffectingTransportField(JobConsolTransportSchema.Constants.JW_Vessel))
			{
				yield return info;
			}

			foreach (var info in GetSourceInfosAffectingTransportMode())
			{
				yield return info;
			}
		}

		protected override IZType GetVoyage()
		{
			return Source.JK_TransportMode == Core.Constants.TransportModes.Sea
				? GetRelevantTransportByNature()?.JW_VoyageFlight.ToUpper() ?? ZString.Empty
				: ZString.Empty;
		}
	}
}
