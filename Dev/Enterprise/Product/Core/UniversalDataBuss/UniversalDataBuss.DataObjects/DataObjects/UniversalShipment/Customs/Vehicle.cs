using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[XsdSchema(Placement.Inner)]
	public class Vehicle : IDataObject
	{
		[MaxLength(17)]
		public ZString? VIN { get; set; }
		[MaxLength(60)]
		public ZString? Brand { get; set; }
		[MaxLength(50)]
		public ZString? Model { get; set; }
		[MaxLength(17)]
		public ZString? RegistrationNumber { get; set; }
	}
}
