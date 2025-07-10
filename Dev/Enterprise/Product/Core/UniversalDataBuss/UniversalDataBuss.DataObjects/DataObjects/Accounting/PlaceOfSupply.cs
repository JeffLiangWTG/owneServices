using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class PlaceOfSupply : IDataObject
	{
		public CodeDescriptionPair LocationType { get; set; }

		public CodeDescriptionPair5Char Location { get; set; }
	}
}