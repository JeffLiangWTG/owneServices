using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[XsdSchema(Placement.Outer)]
	public class TaxOrFee : IDataObject
	{
		[Mandatory]
		public CodeDescriptionPair6Char Type { get; set; }
		public ZDecimal? BaseQuantity { get; set; }
		public ZDecimal? BaseValue { get; set; }
		public ZDecimal? Amount { get; set; }
		public CodeDescriptionPair RateReasonOverride { get; set; }
		public CodeDescriptionPair MethodOfPayment { get; set; }
		public CodeDescriptionPair4Char MethodOfCalculation { get; set; }
		public CodeDescriptionPair BaseQuantityUQ { get; set; }
		public ZDecimal? Rate { get; set; }
	}
}
