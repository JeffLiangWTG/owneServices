using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	class NctsDepartureContainerSynchroniser : ContainerSynchroniser<NctsDepartureHeaderContainer>
	{
		public NctsDepartureContainerSynchroniser(NctsDepartureHeaderContainer destination, ForwardingContainer source, ForwardingShipment shipmentSource, string departureOrArrivalContainerFlag)
			: base(destination, source, shipmentSource)
		{
			this.departureOrArrivalContainerFlag = departureOrArrivalContainerFlag;
		}

		readonly ZString departureOrArrivalContainerFlag;

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.BC_RCInfo, Source.JC_RCInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.BC_ContainerNumInfo, Source.JC_ContainerNumInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.BC_Seal1Info, Source.JC_SealNumInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.BC_Seal2Info, Source.JC_AdditionalSealNumInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.BC_TypeOfServiceInfo, GetTypeOfService, GetTypeOfServiceInfos));
		}

		IEnumerable<ZPropertyInfo> GetTypeOfServiceInfos() => System.Array.Empty<ZPropertyInfo>();

		IZType GetTypeOfService() => departureOrArrivalContainerFlag;
	}
}
