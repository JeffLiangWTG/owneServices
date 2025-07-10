using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromAgencyShipmentContainer : FreightWrapperFromAgencyShipmentCore
	{
		public FreightWrapperFromAgencyShipmentContainer(AgencyShipmentContainer agencyShipmentContainerBO, BusinessObjectFactory factory)
			: base(agencyShipmentContainerBO.Booking, factory, agencyShipmentContainerBO)
		{
			Argument.NotNull(factory, "factory");
			this.agencyShipmentContainerBO = agencyShipmentContainerBO;
		}

		readonly AgencyShipmentContainer agencyShipmentContainerBO;

		protected override ContainerWrapperCollection GetContainers()
		{
			return new ContainerWrapperCollection(agencyShipmentContainerBO, Factory);
		}
	}
}
