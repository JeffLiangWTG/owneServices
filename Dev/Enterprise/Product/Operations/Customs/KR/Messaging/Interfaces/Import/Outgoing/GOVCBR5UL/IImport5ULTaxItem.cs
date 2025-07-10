using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport5ULTaxItem
	{
		ZString TaxItem { get; }
		ZDecimal Tax { get; }
		ZDecimal PenaltyAmount { get; }
	}
}
