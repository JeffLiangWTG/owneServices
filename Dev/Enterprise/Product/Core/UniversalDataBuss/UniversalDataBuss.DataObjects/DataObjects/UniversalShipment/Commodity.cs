using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer), FlattenedIntoAttributes("Code")]
	public class Commodity : ICodeDescriptionDataObject
	{
		[MaxLength(4), Mandatory, CodeMap(Constants.OrgPatternMatchOverrideRelationships.Commodities)]
		public ZString? Code { get; set; }
		[MaxLength(35)]
		public ZString? Description { get; set; }
	}
}
