using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class ChargeCode : IDataObject
	{
		[MaxLength(10), Mandatory, CodeMap(Constants.OrgPatternMatchOverrideRelationships.ChargeCodes)]
		public ZCodeMappedZString? Code { get; set; }
		[MaxLength(80)]
		public ZString? Description { get; set; }
		public CodeDescriptionPair ChargeType { get; set; }     // AC_ChargeType
		public CodeDescriptionPair Class { get; set; }          // AC_GoodsServiceType
	}
}

