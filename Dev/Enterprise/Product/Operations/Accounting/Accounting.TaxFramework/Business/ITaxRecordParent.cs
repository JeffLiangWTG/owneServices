using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Accounting.TaxFramework.Business
{
	/// <summary>
	/// This interface contains properties that are applicable for TaxRecordParent that only represents InvoicingBase.
	/// Please add new properties in this interface only if it applies to InvoicingBase.
	/// If the new properties correspond to both InvoicingBase and PostingCharge then please consider adding it in the base interface.
	/// </summary>
	public interface ITaxRecordParent : ITaxRecordParentBase
	{
		ZDecimal OSTaxAmount { get; set; }

		ZDecimal LocalTaxAmount { get; set; }

		bool IsTaxTransactionsCalculatedBeforePosting { get; set; }

		ITaxableTransactionLine AddTaxRecoveryLine(ZGuid chargeCodePK, ZGuid jobPK, ZGuid branchPK, ZGuid departmentPK, ZString currencyCode, ZDecimal lineAmount, ZDate taxDate, ZGuid orgPK, ZString supplyType);

		void DeleteAllAddedTaxRecoveryLines();

		void SetTransactionHeaderBranch();

		new IReadOnlyList<ITaxableTransactionLine> GetLines(); //Pre C# 9.0, Covariant Return (Overloading with return type) is not permitted. This is why new is used as a workaround to change the return type from parent interface. Once we upgrade CW1 to use C#9.0 please remove the new keyword and override return type instead.
	}
}
