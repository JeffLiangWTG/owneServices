using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Inner)]
	public class Rail : IDataObject
	{
		[MaxLength(35)]
		[Mandatory]
		public ZString? Journey { get; set; }  //  JV_RV_NKVessel

		[MaxLength(10)]
		[Mandatory]
		public ZString? JourneyNumber { get; set; }   //JV_VoyageFlight
	}
}
