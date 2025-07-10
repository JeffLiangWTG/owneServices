using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.TestDataObjects
{
	[XsdSchema(Placement.Outer)]
	public class UberWarehouse : IDataObject
	{
		[MaxLength(3), Mandatory, CodeMap(Constants.OrgPatternMatchOverrideRelationships.Warehouse)]
		public ZString? Code { get; set; }
		[MaxLength(50)]
		public ZString? Name { get; set; }
	}
}
