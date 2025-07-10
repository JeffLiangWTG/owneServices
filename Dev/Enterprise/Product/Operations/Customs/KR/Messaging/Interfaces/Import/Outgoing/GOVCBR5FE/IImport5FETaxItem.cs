using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport5FETaxItem
	{
		ZString DutyTaxType { get; }
		ZDecimal BeforeAmount { get; }
		ZDecimal AfterAmount { get; }
		ZDecimal AmountDifference { get; }
	}
}
