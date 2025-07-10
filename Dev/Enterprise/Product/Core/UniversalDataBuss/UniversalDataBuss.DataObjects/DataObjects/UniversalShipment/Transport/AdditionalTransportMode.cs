using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class AdditionalTransportMode : IDataObject
	{
		public TransportMode? TransportMode { get; set; }
	}
}
