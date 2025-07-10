using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.TestDataObjects
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName), FlattenedIntoAttributes("Code")]
	public class UberUNLOCO : ICodeNameDataObject
	{
		[MaxLength(5), Mandatory, CodeMap(Constants.OrgPatternMatchOverrideRelationships.Port)]
		public ZString? Code { get; set; }
		[MaxLength(35)]
		public ZString? Name { get; set; }
	}
}

