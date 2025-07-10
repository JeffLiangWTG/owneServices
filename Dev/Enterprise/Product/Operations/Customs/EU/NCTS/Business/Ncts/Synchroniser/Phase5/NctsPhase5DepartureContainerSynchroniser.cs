using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsPhase5DepartureContainerSynchroniser : ContainerSynchroniser<NctsDepartureHeaderContainer>
	{
		public NctsPhase5DepartureContainerSynchroniser(NctsDepartureHeaderContainer destination, ForwardingContainer source, ForwardingShipment shipmentSource)
			: base(destination, source, shipmentSource)
		{
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.BC_RCInfo, Source.JC_RCInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.BC_TypeOfServiceInfo, GetTypeOfService, GetTypeOfServiceInfos));
			Synchronisers.Add(new FieldSynchroniser(Destination.BC_ModeInfo, GetContainerMode, GetContainerModeInfos));
			Synchronisers.Add(new NctsPhase5DepartureContainerThirdSealSynchroniser(Destination, Source));
		}

		static IEnumerable<ZPropertyInfo> GetTypeOfServiceInfos() =>  Array.Empty<ZPropertyInfo>();

		static IZType GetTypeOfService() => (ZString)ContainerTypeOfServiceList.Codes.DepartureContainer;

		static IEnumerable<ZPropertyInfo> GetContainerModeInfos() => Array.Empty<ZPropertyInfo>();

		static IZType GetContainerMode() => (ZString)Core.Constants.ContainerModes.Containerised;

		protected override void OnSynchronised()
		{
			base.OnSynchronised();

			if (Destination.Header is NctsHeader header)
			{
				header.DepartureHeaderContainers.Reload(true);
				header.DepartureHeaderContainers.RefreshBindingIncludingChildren();
				header.DepartureHeaderContainers.ForEach(x => x.AdditionalSeals.RefreshBindingIncludingChildren());
			}
		}
	}
}
