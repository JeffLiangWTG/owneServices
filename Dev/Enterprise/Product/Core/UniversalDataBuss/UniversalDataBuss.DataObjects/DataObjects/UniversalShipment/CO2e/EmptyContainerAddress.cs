using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public class EmptyContainerAddress : IDataObject
	{
		public OrganizationAddress From { get; set; }
		public OrganizationAddress To { get; set; }
		public CodeDescriptionPair TransportMode { get; set; }
		public GreenhouseGasEmission GreenhouseGasEmission { get; set; }
	}
}
