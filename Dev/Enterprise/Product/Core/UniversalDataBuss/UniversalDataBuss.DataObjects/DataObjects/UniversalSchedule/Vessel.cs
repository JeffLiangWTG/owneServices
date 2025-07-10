using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Inner)]
	public class Vessel : IDataObject
	{
		[MaxLength(35)]
		[Mandatory]
		public ZString? VesselName { get; set; }  //  RV_Code

		[MaxLength(7)]
		public ZString? LloydsNumber { get; set; }   //RV_LloydsNumber

		[MaxLength(10)]
		public ZString? CallSign { get; set; }
	}
}
