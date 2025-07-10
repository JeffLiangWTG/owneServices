using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.TestDataObjects
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class UberChargeCode : IDataObject
	{
		[MaxLength(10), Mandatory, CodeMap(Constants.OrgPatternMatchOverrideRelationships.ChargeCodes)]
		public ZCodeMappedZString? Code { get; set; }
		[MaxLength(35)]
		public ZString? Description { get; set; }
	}
}
