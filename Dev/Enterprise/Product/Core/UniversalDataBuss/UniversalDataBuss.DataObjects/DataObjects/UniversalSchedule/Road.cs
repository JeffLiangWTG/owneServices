using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Inner)]
	public class Road : IDataObject
	{
		[MaxLength(10)]
		[Mandatory]
		public ZString? TruckReference { get; set; }   //JV_VoyageFlight
	}
}
