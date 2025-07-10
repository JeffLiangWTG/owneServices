using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[XsdSchema(Placement.Outer)]
	public class CommercialCharge : IDataObject
	{
		[MaxLength(3), Mandatory]
		public CodeDescriptionPair ChargeType { get; set; }
		public CodeDescriptionPair ApportionmentType { get; set; }
		public ZDecimal? Amount { get; set; }
		public Currency Currency { get; set; }
		public ZBool? IsDutiable { get; set; }
		public ZBool? IsGSTApplicable { get; set; }
		public ZBool? IsIncludedInITOT { get; set; }
		public ZBool? IsNotIncludedInInvoice { get; set; }
		public ZBool? IsApportionedCharge { get; set; }
		public CodeDescriptionPair PrepaidCollect { get; set; }
		public ZDecimal? PercentageOfLinePrice { get; set; }
		public CodeDescriptionPair DistributeBy { get; set; }
		public CodeDescriptionPair ExchangeRateType { get; set; }
		public ZDecimal? AgreedExchangeRate { get; set; }
		public ZBool? AdjustedCharge { get; set; }
		public ZBool? IsStatisticalValueApplicable { get; set; }
	}
}

