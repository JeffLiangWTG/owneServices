using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Yard.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class CYDTransportationUnitWrapper : GenericWrapper
	{
		public CYDTransportationUnitWrapper(CYDTransportationUnit transportationUnitBO, BusinessObjectFactory factory)
			: base(transportationUnitBO, factory)
		{
			this.transportationUnitBO = transportationUnitBO ?? factory.GetNull<CYDTransportationUnit>();
		}
		readonly CYDTransportationUnit transportationUnitBO;

		public ZString TransportationReference => transportationUnitBO?.YTU_TransportationReference ?? string.Empty;

		public ZString TransportationUnitID => transportationUnitBO?.YTU_TransportationUnitID ?? string.Empty;

		public ZDateTimeOffset? EstimatedGateInTime => transportationUnitBO?.YTU_EstimatedGateInTime;

		public ZDateTimeOffset? GateInTime => transportationUnitBO?.YTU_GateInTime;

		public ZDateTimeOffset? GateOutTime => transportationUnitBO?.YTU_GateOutTime;

		public ZString? WaitingBayLocation => transportationUnitBO?.WaitingBayLocation?.WLV_LocationString;
	}
}
