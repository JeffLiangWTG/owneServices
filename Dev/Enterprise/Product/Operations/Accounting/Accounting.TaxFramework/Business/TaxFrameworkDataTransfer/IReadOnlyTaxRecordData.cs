using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public interface IReadOnlyTaxRecordData
	{
		ZGuid TaxMessagePK { get; }
		ZBool AffectsSourceTransactionTotal { get; }
		ZGuid LedgerControlGLAccountPK { get; }
		ZGuid TaxControlGLAccountPK { get; }
		ZGuid TaxExpenseGLAccountPK { get; }
		ZGuid TaxPendingControlGLAccountPK { get; }
		ZGuid TaxIDPK { get; }
		ZString TaxBasis { get; }
		ZGuid TaxConfigurationPK { get; }
		ZDecimal LocalTaxAmount { get; }
		ZDecimal LocalTaxBaseAmount { get; }
		ZDecimal OSTaxAmount { get; }
		ZDecimal OSTaxBaseAmount { get; }
		ZGuid BranchPK { get; }
		ZGuid DepartmentPK { get; }
		ZString Ledger { get; }
		ZDate PostDate { get; }
		ZInt RateDenominator { get; }
		ZInt RateNumerator { get; }
		ZDate RealisationDate { get; }
		ZString OSTaxCurrency { get; }
		ZString TaxAuthorityServiceCode { get; }
		ZString TaxAuthorityServiceCodeDescription { get; }
		ZDate TaxDate { get; }
		ZString TaxSuperType { get; }
		ZString TaxSystemCode { get; }
		TaxRecordDataSystemCalculatedValues SystemCalculatedValues { get; }
		IReadOnlyCollection<ZGuid> TransactionLinePKs { get; }
	}

	public class TaxRecordDataSystemCalculatedValues : IReadOnlyTaxTransactionSystemCalculatedValues
	{
		public TaxRecordDataSystemCalculatedValues(ZDecimal osTaxBaseAmount, ZDecimal osTaxAmount, ZInt rateNumerator, ZInt rateDenominator, ZDate taxDate, ZString taxAuthorityServiceCode, ZString taxAuthorityServiceCodeDescription)
		{
			OSTaxBaseAmount = osTaxBaseAmount;
			OSTaxAmount = osTaxAmount;
			RateNumerator = rateNumerator;
			RateDenominator = rateDenominator;
			TaxDate = taxDate;
			TaxAuthorityServiceCode = taxAuthorityServiceCode;
			TaxAuthorityServiceCodeDescription = taxAuthorityServiceCodeDescription;
		}

		public ZDecimal OSTaxBaseAmount { get; }
		public ZDecimal OSTaxAmount { get; }
		public ZInt RateNumerator { get; }
		public ZInt RateDenominator { get; }
		public ZDate TaxDate { get; }
		public ZString TaxAuthorityServiceCode { get; }
		public ZString TaxAuthorityServiceCodeDescription { get; }
	}
}
