using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.TaxFramework.Business
{
	/// <summary>
	/// This interface contains properties that are applicable for Lines linked to TaxRecordParent that can represent both PostingCharge and InvoicingBase.
	/// While adding a new property please consider if the property is applicable for Lines linked to both PostingCharge and InvoicingBase.
	/// Please add new properties in this interface only if it applies to Lines for both PostingCharge and InvoicingBase.
	/// If the new properties correspond to InvoicingLineBase only then please consider adding it in the child interface.
	/// </summary>
	public interface ITaxableTransactionLineBase
	{
		ZGuid PK { get; }

		BusinessObjectFactory Factory { get; }

		GlbBranch Branch { get; }

		AccChargeCode ChargeCode { get; }

		ZString Currency { get; }

		ZDate TaxDate { get; }

		AccChargeTaxOverrideMatcher.TaxCalculationParameters GetTaxCalculationParameters();

		ZGuid JobPK { get; }

		ZDecimal BaseOSAmount { get; }

		ZDecimal LocalAmount { get; }
	}
}
