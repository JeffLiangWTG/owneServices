using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.TestDataObjects
{
	[XsdSchema(Placement.Outer)]
	public class UberEventType : IDataObject
	{
		[MaxLength(3), Mandatory, CodeMap(Constants.OrgPatternMatchOverrideRelationships.EventCode)]
		public ZString? Code { get; set; }
		[MaxLength(35)]
		public ZString? Description { get; set; }
	}
}
