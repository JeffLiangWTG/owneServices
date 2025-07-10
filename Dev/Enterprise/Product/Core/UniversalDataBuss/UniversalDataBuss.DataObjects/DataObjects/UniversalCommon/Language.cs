using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName), FlattenedIntoAttributes("Code")]
	public class Language : ICodeDescriptionDataObject
	{
		[MaxLength(7), Mandatory]
		public ZString? Code { get; set; }

		[MaxLength(35)]
		public ZString? Description { get; set; }
	}
}