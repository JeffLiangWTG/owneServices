using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public class Crew : IDataObject
	{
		public CrewType? CrewType { get; set; }
		[MaxLength(256)]
		public ZString? FullName { get; set; }
		[MaxLength(25)]
		public ZString? LicenseNumber { get; set; }
	}

	public enum CrewType
	{
		Driver,
		Crew,
		Passenger
	}
}
