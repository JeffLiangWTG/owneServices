using System.Collections.Generic;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public partial class VehicleRun : IDataObject
	{
		public Vehicle Vehicle { get; set; }
		public List<Crew> CrewCollection { get; private set; }
	}
}
