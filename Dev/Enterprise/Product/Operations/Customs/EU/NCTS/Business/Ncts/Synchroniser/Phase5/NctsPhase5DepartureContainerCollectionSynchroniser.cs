using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsPhase5DepartureContainerCollectionSynchroniser : ContainerCollectionSynchroniser<NctsDepartureHeaderContainer>
	{
		public NctsPhase5DepartureContainerCollectionSynchroniser(ForwardingShipment source, BusinessObject destination, ForwardingConsol consolSource, ICusInBondContainerCollection containers)
			: base(source, destination, consolSource, true, containers)
		{
		}

		protected override ContainerSynchroniser<NctsDepartureHeaderContainer> GetContainerSynchroniser(NctsDepartureHeaderContainer destContainer, ForwardingContainer container)
		{
			return new NctsPhase5DepartureContainerSynchroniser(destContainer, container, Source);
		}

		protected override GenericNonContainerizedNumberSynchroniser<NctsDepartureHeaderContainer> GetNewNonContainerisedNumberSynchroniser(NctsDepartureHeaderContainer destContainer, ForwardingShipment source) => null;

		protected override bool RequiresNonContainerisedRows => false;
	}
}
