using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName), FlattenedIntoAttributes("Code")]
	public class CodeDescriptionPair10Char : ICodeDescriptionDataObject
	{
		[MaxLength(10), Mandatory]
		public ZString? Code { get; set; }
		[MaxLength(80)]
		public ZString? Description { get; set; }
	}
}
