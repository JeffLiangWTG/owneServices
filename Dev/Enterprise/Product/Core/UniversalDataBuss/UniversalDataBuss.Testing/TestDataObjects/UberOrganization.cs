using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.TestDataObjects
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class UberOrganization : IDataObject
	{
		[MaxLength(12), CodeMap(Constants.OrgPatternMatchOverrideRelationships.Organisation)]
		public ZCodeMappedZString? Code { get; set; }
		[MaxLength(50)]
		public ZString? Address1 { get; set; }
		[MaxLength(50)]
		public ZString? Address2 { get; set; }
		[MaxLength(25)]
		public ZString? City { get; set; }
		[MaxLength(50), Mandatory]
		public ZString? CompanyName { get; set; }
		public UberCountry Country { get; set; }
		[MaxLength(20)]
		public ZString? Phone { get; set; }
		[MaxLength(10)]
		public ZString? Postcode { get; set; }
		[MaxLength(25)]
		public ZString? State { get; set; }
	}
}

