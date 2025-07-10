using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName), FlattenedIntoAttributes("Code")]
	public class CodeGroupPair : ICodeDataObject, IDataObject
	{
		[MaxLength(10), Mandatory]
		public ZString? Code { get; set; }
		[MaxLength(25), Mandatory]
		public ZString? Group { get; set; }
	}
}
