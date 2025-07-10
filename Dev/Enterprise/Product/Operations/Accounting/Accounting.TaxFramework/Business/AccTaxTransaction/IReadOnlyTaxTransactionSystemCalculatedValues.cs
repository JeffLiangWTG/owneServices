using CargoWise.Types;

namespace Enterprise.Accounting.TaxFramework.Business
{
	interface IReadOnlyTaxTransactionSystemCalculatedValues
	{
		ZDecimal OSTaxBaseAmount { get; }
		ZDecimal OSTaxAmount { get; }
		ZInt RateNumerator { get; }
		ZInt RateDenominator { get; }
		ZDate TaxDate { get; }
		ZString TaxAuthorityServiceCode { get; }
		ZString TaxAuthorityServiceCodeDescription { get; }
	}
}
