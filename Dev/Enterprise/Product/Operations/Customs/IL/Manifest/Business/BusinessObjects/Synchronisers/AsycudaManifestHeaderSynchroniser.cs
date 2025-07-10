using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaManifestHeaderSynchroniser : ASYCUDA.Business.AsycudaManifestHeaderSynchroniser
	{
		public AsycudaManifestHeaderSynchroniser(AsycudaManifestHeader destination, ForwardingConsol sourceConsol)
			: base(destination, sourceConsol)
		{
		}

		protected new AsycudaManifestHeader Destination => (AsycudaManifestHeader)base.Destination;

		protected override BusinessObjectCollectionSynchroniser GetNewBillCollectionSynchroniser()
		{
			return new AsycudaBillCollectionSynchroniser(Destination);
		}

		protected override BusinessObjectCollectionSynchroniser GetNewConsolContainerCollectionSynchroniser()
			=> new AsycudaConsolContainerCollectionSynchroniser(Source, Destination);

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(portOfDischargeSynchroniser = new FieldSynchroniser(Destination.AMA_ManifestNumberInfo, GetManifestNumber, GetSourceInfosAffectingManifestNumber));
		}

		protected override void UnHookSynchronisers()
		{
			portOfDischargeSynchroniser = null;
			base.UnHookSynchronisers();
		}

		protected override void Transports_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			base.Transports_CountChanged(sender, e);
			UpdateInfoEventsAndReSynchronise(portOfDischargeSynchroniser);
		}

		IZType GetManifestNumber()
		{
			if (Destination.IsExport)
			{
				var transport = Source.Transports
					.Cast<Transport>()
					.OrderBy(x => x.JW_LegOrder)
					.FirstOrDefault(x => x.JW_RL_NKLoadPort == Source.JK_RL_NKLoadPort);

				if (transport != null)
				{
					return transport.JW_DeparturePortRouteId;
				}
			}
			else
			{
				var transport = Source.Transports
					.Cast<Transport>()
					.OrderBy(x => x.JW_LegOrder)
					.LastOrDefault(x => x.JW_RL_NKDiscPort == Source.JK_RL_NKDischargePort);

				if (transport != null)
				{
					return transport.JW_ArrivalPortRouteId;
				}
			}

			return ZString.Empty;
		}

		IEnumerable<ZPropertyInfo> GetSourceInfosAffectingManifestNumber()
		{
			foreach (var info in ConsolDataCalculator.GetInfosAffectingTransportsOrder())
			{
				yield return info;
			}

			foreach (var info in ConsolDataCalculator.GetTransportsInfos(JobConsolTransportSchema.Constants.JW_ArrivalPortRouteId))
			{
				yield return info;
			}

			foreach (var info in ConsolDataCalculator.GetTransportsInfos(JobConsolTransportSchema.Constants.JW_DeparturePortRouteId))
			{
				yield return info;
			}

			yield return Destination.AMA_NatureInfo;
		}

		protected override ZPropertyInfo GetTargetShippingAgentAtDischarge()
		{
			return Source.JK_TransportMode == Core.Constants.TransportModes.Sea
				? null
				: base.GetTargetShippingAgentAtDischarge();
		}

		protected override ZPropertyInfo GetTargetShippingAgentAtLoad()
		{
			return Source.JK_TransportMode == Core.Constants.TransportModes.Sea
				? null
				: base.GetTargetShippingAgentAtLoad();
		}

		FieldSynchroniser portOfDischargeSynchroniser;
	}
}
