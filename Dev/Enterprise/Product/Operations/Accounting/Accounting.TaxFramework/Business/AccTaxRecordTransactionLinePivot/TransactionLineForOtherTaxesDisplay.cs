using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public abstract class TransactionLineForOtherTaxesDisplay : NonPersistentBusinessObject
	{
		public abstract ZString ChargeCode { get; }

		public abstract ZString ChargeCodeDescription { get; }

		public abstract ZString Branch { get; }

		public abstract ZString Department { get; }

		public abstract ZString Job { get; }

		public abstract ZString Currency { get; }

		public abstract ZDecimal OSExTaxAmount { get; }

		public abstract ZDecimal OSTaxAmount { get; }

		public abstract ZDecimal LocalExTaxAmount { get; }

		public abstract ZDecimal LocalTaxAmount { get; }

		public abstract ZDecimal OSTotalAmount { get; }

		public abstract ZDecimal LocalTotalAmount { get; }

		public abstract ZString GovtChargeCode { get; }

		public abstract ZInt OSCurrencyDecimals { get; }
		public abstract ZInt LocalCurrencyDecimals { get; }

		public abstract ZString TaxBranch { get; }

		public abstract ZString SupplyType { get; }

		public abstract OtherTaxesLinkedToTransactionLinesCollection OtherTaxesLinkedToTransactionLinesCollection { get; }
	}
}
