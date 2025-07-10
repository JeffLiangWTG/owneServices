using CargoWise.Types;

namespace Enterprise.Accounting.TaxFramework.Business
{
	/// <summary>
	/// This interface contains properties that are applicable for Lines linked to IncoivingLineBase.
	/// Please add new properties in this interface only if it applies to InvoicingLineBase.
	/// If the new properties correspond to Lines linked to both InvoicingBase and PostingCharge then please consider adding it in the base interface.
	/// </summary>
	public interface ITaxableTransactionLine : ITaxableTransactionLineBase
	{
		ZGuid CopiedFromPK { get; }

		ZString SupplyType { get; }
	}
}
