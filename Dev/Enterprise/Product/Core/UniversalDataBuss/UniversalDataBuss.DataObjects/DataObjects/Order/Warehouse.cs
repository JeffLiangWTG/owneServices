using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Inner), FlattenedIntoAttributes("Code")]
	public class Warehouse : ICodeNameDataObject
	{
		[MaxLength(3), Mandatory, CodeMap(Constants.OrgPatternMatchOverrideRelationships.Warehouse)]
		public ZString? Code { get; set; }
		[MaxLength(50)]
		public ZString? Name { get; set; }
	}
}
