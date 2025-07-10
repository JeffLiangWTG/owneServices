using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName), FlattenedIntoAttributes("Code")]
	public class NoteContext : ICodeDescriptionDataObject
	{
		[MaxLength(3), Mandatory]
		public ZString? Code { get; set; }
		[MaxLength(100)]
		public ZString? Description { get; set; }
	}
}
