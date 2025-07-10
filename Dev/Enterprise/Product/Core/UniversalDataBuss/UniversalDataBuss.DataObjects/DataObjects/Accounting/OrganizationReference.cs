using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class OrganizationReference : IDataObject
	{
		[MaxLength(35), Mandatory, CandidateKey]
		public ZString? Type { get; set; }
		[MaxLength(35), CodeMap(Constants.OrgPatternMatchOverrideRelationships.Organisation)]
		public ZCodeMappedZString? Key { get; set; }
	}
}
