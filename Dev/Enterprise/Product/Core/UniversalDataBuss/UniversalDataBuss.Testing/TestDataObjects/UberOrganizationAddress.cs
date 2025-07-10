using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.TestDataObjects
{
	[XsdSchema(Placement.Inner)]
	public class UberOrganizationAddress : IDataObject
	{
		[MaxLength(40)]
		public ZString? AddressType { get; set; }

		[MaxLength(12), CodeMap(Constants.OrgPatternMatchOverrideRelationships.Organisation)]
		public ZCodeMappedZString? OrganizationCode { get; set; }

		[MaxLength(256)]
		public ZString? CompanyName { get; set; }

		[MaxLength(256), AllowLineControlWhiteSpace]
		public ZString? Contact { get; set; }
	}
}
