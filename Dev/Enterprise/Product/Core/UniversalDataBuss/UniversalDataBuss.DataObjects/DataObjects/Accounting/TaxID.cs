using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class TaxID : IDataObject
	{
		[MaxLength(10), Mandatory]
		public ZString? TaxCode { get; set; }
		[MaxLength(35)]
		public ZString? Description { get; set; }
		public ZDecimal? TaxRate { get; set; }
		public CodeDescriptionPair TaxType { get; set; }
		public ZDecimal? ExtraTaxRate { get; set; }
		public CodeDescriptionPair ExtraTaxType { get; set; }
	}
}

