using Enterprise.Accounting.CountryCompliance.Interfaces;

namespace Enterprise.Accounting.CountryCompliance.Implementation
{
	public class KoreaSouthRelatedDisbursementTransactionProvider : IRelatedDisbursementTransaction
	{
		public bool IsEnableRelatedDisbursementTransaction() => true;

		public string AppendAdditionalDescription(string description) => $" 총합계 {description} 원";
	}
}
