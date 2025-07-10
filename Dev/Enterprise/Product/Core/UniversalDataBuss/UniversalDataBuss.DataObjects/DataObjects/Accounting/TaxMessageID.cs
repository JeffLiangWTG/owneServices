using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class TaxMessageID : IDataObject
	{
		[MaxLength(10), Mandatory]
		public ZString? TaxMessageCode { get; set; }
		[MaxLength(80)]
		public ZString? Description { get; set; }
		[MaxLength(255)]
		public ZString? EnglishTaxMessage { get; set; }
		public CodeDescriptionPair VATGSTExemptionDocumentType { get; set; }
		public TaxGroupCodeType TaxGroupCode { get; set; }
	}
}

