using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[XsdSchema(Placement.Inner)]
	public class EntryLineCharge : IDataObject
	{
		[CandidateKey, Mandatory]
		public CodeDescriptionPair Type { get; set; }
		[Mandatory]
		public ZDecimal? Amount { get; set; }
		public ZBool? IsLandedCostOnly { get; set; }
		public ZDecimal? BaseValue { get; set; }
		public ZDecimal? Rate { get; set; }
		[MaxLength(3)]
		public ZString? Source { get; set; }
		public CodeDescriptionPair RateOverrideReason { get; set; }
		public CodeDescriptionPair MethodOfPayment  { get; set; }
		[CandidateKey]
		public CodeDescriptionPair6Char MethodOfCalculation  { get; set; }
	}
}

