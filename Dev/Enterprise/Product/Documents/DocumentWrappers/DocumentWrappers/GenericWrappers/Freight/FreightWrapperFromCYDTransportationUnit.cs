using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Yard.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	class FreightWrapperFromCYDTransportationUnit : FreightWrapper
	{
		readonly CYDTransportationUnit transportationUnit;

		public FreightWrapperFromCYDTransportationUnit(CYDTransportationUnit transportationUnit, BusinessObjectFactory factory)
			: base(transportationUnit, factory)
		{
			Argument.NotNull(transportationUnit, "Transportation unit must not be null");
			this.transportationUnit = transportationUnit;
		}

		protected override ZString GetJobNumber()
		{
			return transportationUnit.JobNumber;
		}

		protected override YardUnitWrapperCollection GetYardUnits()
		{
			return base.GetYardUnits();
		}

		protected override ZDateTimeOffset GetGateInTime()
		{
			return transportationUnit.YTU_GateInTime;
		}

		protected override ZDateTimeOffset GetGateOutTime()
		{
			return transportationUnit.YTU_GateOutTime;
		}

		protected override ZString GetTransportReference()
		{
			return transportationUnit.YTU_TransportationReference;
		}
	}
}
