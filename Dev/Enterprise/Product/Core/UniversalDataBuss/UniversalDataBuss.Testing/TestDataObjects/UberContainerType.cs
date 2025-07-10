using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.TestDataObjects
{
	[XsdSchema(Placement.Inner)]
	public class UberContainerType : IDataObject
	{
		[MaxLength(4), Mandatory, CodeMap(Constants.OrgPatternMatchOverrideRelationships.ContainerType)]
		public ZString? Code { get; set; }
		[MaxLength(35)]
		public ZString? Description { get; set; }
		[MaxLength(4)]
		public ZString? ISOCode { get; set; }
	}
}
