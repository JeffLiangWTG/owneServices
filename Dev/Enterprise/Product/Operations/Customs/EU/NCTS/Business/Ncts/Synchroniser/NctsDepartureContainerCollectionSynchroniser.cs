using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	class NctsDepartureContainerCollectionSynchroniser : ContainerCollectionSynchroniser<NctsDepartureHeaderContainer>
	{
		public NctsDepartureContainerCollectionSynchroniser(ForwardingShipment source, BusinessObject destination, ForwardingConsol consolSource, ICusInBondContainerCollection containers, string departureOrArrivalContainerFlag)
			: base(source, destination, consolSource, true, containers)
		{
			this.departureOrArrivalContainerFlag = departureOrArrivalContainerFlag;
		}

		protected override ContainerSynchroniser<NctsDepartureHeaderContainer> GetContainerSynchroniser(NctsDepartureHeaderContainer destContainer, ForwardingContainer container)
		{
			return new NctsDepartureContainerSynchroniser(destContainer, container, Source, departureOrArrivalContainerFlag);
		}

		protected override GenericNonContainerizedNumberSynchroniser<NctsDepartureHeaderContainer> GetNewNonContainerisedNumberSynchroniser(NctsDepartureHeaderContainer destContainer, ForwardingShipment source)
		{
			return null;
		}

		protected override bool RequiresNonContainerisedRows
		{
			get { return false; }
		}

		readonly ZString departureOrArrivalContainerFlag;
	}
}
