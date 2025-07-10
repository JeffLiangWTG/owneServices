using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public class Vehicle : IDataObject
	{
		[MaxLength(35)]
		public ZString? Color { get; set; }
		[MaxLength(35)]
		public ZString? Make { get; set; }
		[MaxLength(35)]
		public ZString? Model { get; set; }
		public ZByte? NumberOfDoors { get; set; }
		public CodeDescriptionPair Transmission { get; set; }
		public ZShort? Year { get; set; }
		public CodeDescriptionPair10Char VehicleType { get; set; }
		public Registration Registration { get; set; }
	}
}


