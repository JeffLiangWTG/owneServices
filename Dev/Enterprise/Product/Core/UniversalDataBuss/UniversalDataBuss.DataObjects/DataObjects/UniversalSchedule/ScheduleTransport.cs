using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Inner)]
	public class ScheduleTransport : IDataObject
	{
		public Air Air { get; set; }
		public Sea Sea { get; set; }
		public Rail Rail { get; set; }
		public Road Road { get; set; }
	}
}
