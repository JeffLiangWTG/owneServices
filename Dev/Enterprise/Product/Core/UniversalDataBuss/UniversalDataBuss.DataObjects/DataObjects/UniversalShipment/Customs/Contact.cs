using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[XsdSchema(Placement.Outer)]
	public class Contact : IDataObject
	{
		[MaxLength(256)]
		public ZString? Name { get; set; }
		[MaxLength(20)]
		public ZString? PhoneNumber { get; set; }
		[MaxLength(254)]
		public ZString? Email { get; set; }
	}
}
