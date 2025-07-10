using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Inner)]
	public class Air : IDataObject
	{
		[MaxLength(35)]
		[Mandatory]
		public ZString? FlightNumber { get; set; } //  JV_RV_NKVessel

		public ZBool? IsCargoOnly { get; set; }  //JV_IsCargoOnly
	}
}
