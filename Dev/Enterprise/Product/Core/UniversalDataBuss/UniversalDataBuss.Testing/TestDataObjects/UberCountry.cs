using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.TestDataObjects
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class UberCountry : ICodeNameDataObject
	{
		[MaxLength(2), Mandatory, CodeMap(Constants.OrgPatternMatchOverrideRelationships.Country)]
		public ZString? Code { get; set; }
		[MaxLength(35)]
		public ZString? Name { get; set; }
	}
}
