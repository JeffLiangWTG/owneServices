using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Inner)]
	public class Loading : IDataObject
	{
		[Mandatory]
		public UNLOCO Port { get; set; }

		public ZDateTime? EstimatedArrival { get; set; }
		public ZDateTime? ActualArrival { get; set; }

		public ZDateTime? EstimatedDeparture { get; set; }
		public ZDateTime? ActualDeparture { get; set; }

		[MaxLength(10)]
		public ZString? DepartureBerth { get; set; }
		[MaxLength(20)]
		public ZString? DepartureReference { get; set; }
		public OrganizationAddress DepartureCTO { get; set; }

		public ZDateTime? DocumentCutOff { get; set; }

		public ZDateTime? FCLReceivalCommences { get; set; }
		public ZDateTime? FCLCutOff { get; set; }

		public ZDateTime? HazzardReceivalCommences { get; set; }
		public ZDateTime? HazzardCutOffDate { get; set; }

		[MaxLength(10)]
		public ZString? TerminalCode { get; set; }
		[MaxLength(35)]
		public ZString? TerminalName { get; set; }
	}
}
