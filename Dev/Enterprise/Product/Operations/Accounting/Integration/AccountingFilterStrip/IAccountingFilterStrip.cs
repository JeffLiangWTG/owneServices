using Enterprise.Security;

namespace Enterprise.Accounting.Integration
{
	public interface IAccountingFilterStrip
	{
		void Initialize(bool addRevenueFilters = true, bool addWIPAccrualHasFilters = true, bool addSupplierCostReferenceFilters = true, bool addOrganisationFilters = true, bool addDateFilters = true, bool addAmountFilters = true, bool addNumbersAndReferencesFilters = true, bool addProfitLossReasonFilters = true);
		string APInvoiceNumber { get; }
		string ARTransactionNumber { get; }
		object BillingFilterCategory { get; }
		void AddBillingFilters(object moduleFilterCollection);
		void AddJobManagementFilters(object moduleFilterCollection, SecurityCheckpoint jobManagementSecurity);
	}
}
