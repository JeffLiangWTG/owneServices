using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CO.Manifest.Business
{
	public class AsycudaManifestHeaderSynchroniser : ASYCUDA.Business.AsycudaManifestHeaderSynchroniser
	{
		public AsycudaManifestHeaderSynchroniser(AsycudaManifestHeader destination, ForwardingConsol sourceConsol)
			: base(destination, sourceConsol)
		{
		}

		protected override BusinessObjectCollectionSynchroniser GetNewBillCollectionSynchroniser()
		{
			return new AsycudaBillCollectionSynchroniser((AsycudaManifestHeader)Destination);
		}

		protected override void SynchronisersMasterBill()
		{
			if (ShouldSynchronisersFromCoLoadToSeaManifest())
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.AMA_MasterBillInfo, Source.JK_CoLoadMasterBillInfo));
			}
			else
			{
				base.SynchronisersMasterBill();
			}
			Synchronisers.Add(new FieldSynchroniser(((AsycudaManifestHeader)Destination).DeliveryModeInfo, GetSourceDeliveryMode, GetInfosAffectingDeliveryMode));
		}

		protected override IZType GetCarrier()
		{
			var carrier = base.GetCarrier();
			if (Source.IsSea)
			{
				OrgAddress address = null;  
				if (Source.JK_AgentType == Constants.AgentType.CoLoad)
				{
					address = Source.CreditorAddress;
				}
				else
				{
					address = GetRelevantTransportByNature()?.CarrierAddress;
				}
				if (address != null && address.Country.Code != Constants.CountryCodes.Colombia)
				{
					return address.Header.CarrierAppointedAgentPorts_Agency.Cast<OrgCarrierAppointedAgentPorts>()
						.FirstOrDefault(agent => agent.O5_PortOrCountry == Constants.CountryCodes.Colombia)?.AgentOfficeAddress?.PK;
				}
				else
				{
					return address?.PK;
				}
			}
			return carrier;
		}

		protected override IEnumerable<ZPropertyInfo> GetSourceInfosAffectingCarrier()
		{
			if (ShouldSynchronisersFromCoLoadToSeaManifest())
			{
				return new[] { Source.JK_OA_CreditorAddressInfo };
			}
			return base.GetSourceInfosAffectingCarrier();
		}

		bool ShouldSynchronisersFromCoLoadToSeaManifest() => Source.IsSea && Source.JK_AgentType == Core.Constants.AgentType.CoLoad;

		protected IZType GetSourceDeliveryMode() => CODeliveryModeList.MapDeliveryMode(SourceContainer?.DeliveryModeForBinding ?? ZString.Empty);

		protected IEnumerable<ZPropertyInfo> GetInfosAffectingDeliveryMode()
		{
			if (SourceContainer == null)
			{
				return Enumerable.Empty<ZPropertyInfo>();
			}
			return new[] { new ZPropertyInfoString(SourceContainer, nameof(SourceContainer.DeliveryModeForBinding)) };
		}

		CommonContainer SourceContainer => Source.Containers?.Where(w => !w.JC_IsNonOperativeReefer)?.FirstOrDefault();
	}
}
