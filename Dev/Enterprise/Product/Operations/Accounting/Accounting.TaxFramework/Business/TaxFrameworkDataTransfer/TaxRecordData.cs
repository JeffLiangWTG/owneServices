using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public class TaxRecordData : IReadOnlyTaxRecordData
	{
		public ZGuid TaxMessagePK { get; set; }

		public ZBool AffectsSourceTransactionTotal { get; set; }

		public ZGuid LedgerControlGLAccountPK { get; set; }

		public ZGuid TaxControlGLAccountPK { get; set; }

		public ZGuid TaxExpenseGLAccountPK { get; set; }

		public ZGuid TaxPendingControlGLAccountPK { get; set; }

		public ZGuid TaxIDPK { get; set; }

		public ZString TaxBasis { get; set; }

		public ZGuid TaxConfigurationPK { get; set; }

		public ZDecimal LocalTaxAmount { get; set; }

		public ZDecimal LocalTaxBaseAmount { get; set; }

		public ZDecimal OSTaxAmount { get; set; }

		public ZDecimal OSTaxBaseAmount { get; set; }

		public ZGuid BranchPK { get; set; }

		public ZGuid DepartmentPK { get; set; }

		public ZString Ledger { get; set; }

		public ZDate PostDate { get; set; }

		public ZInt RateDenominator { get; set; }

		public ZInt RateNumerator { get; set; }

		public ZDate RealisationDate { get; set; }

		public ZString OSTaxCurrency { get; set; }

		public ZString TaxAuthorityServiceCode { get; set; }

		public ZString TaxAuthorityServiceCodeDescription { get; set; }

		public ZDate TaxDate { get; set; }

		public ZString TaxSuperType { get; set; }

		public ZString TaxSystemCode { get; set; }

		public TaxRecordDataSystemCalculatedValues SystemCalculatedValues { get; set; }

		public IReadOnlyCollection<ZGuid> TransactionLinePKs { get; set; }
	}
}
