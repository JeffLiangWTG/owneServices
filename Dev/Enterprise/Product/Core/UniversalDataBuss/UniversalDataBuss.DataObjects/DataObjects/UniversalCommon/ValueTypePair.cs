using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName), FlattenedIntoAttributes("Value")]
	public class ValueTypePair : IDataObject
	{
		[MaxLength(35), Mandatory]
		public ZString? Value { get; set; }
		[MaxLength(35), Mandatory]
		public ZString? Type { get; set; }
	}
}