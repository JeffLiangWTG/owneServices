using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module.Transaction
{
	public class APIncompleteInvoicesFilterBusinessObject : APTransactionFilterStripBusinessObject
	{
		protected override ZString[] LedgersToUse
		{
			get { return new ZString[] { LedgerTypes.IncompleteTransactions }; }
		}

		protected override ZString[] TransactionTypesForSupplierCostReferenceFilter
		{
			get { return null; }
		}

		protected override void AddInternalReferenceNumberFilter(ModuleFilterCollection filters)
		{
		}

		protected override void AddFlagsFilters(ModuleFilterCollection filters)
		{
		}

		protected override void AddJobNumberFilter(ModuleFilterCollection filters)
		{
		}

		protected override void AddMiscellaneousNumberFilters(ModuleFilterCollection filters)
		{
		}

		protected override void AddPaymentStatusFilter(ModuleFilterCollection filters)
		{
		}

		protected override void AddPrintedStatusFilter(ModuleFilterCollection filters)
		{
		}

		protected override void AddOtherFilters(ModuleFilterCollection filters)
		{
		}

		protected override void AddOperationalFilters(ModuleFilterCollection filters)
		{
		}

		protected override bool ShouldAddRelatedTransactionsNotPaidFilter
		{
			get { return false; }
		}

		protected override bool ShouldAddEInvoicingFilter
		{
			get { return false; }
		}

		protected override bool ShouldAddComplianceDocumentRecordFilter => false;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			SetActiveStatusFilter(null, false);

			return base.GetModuleFiltersCore();
		}

		protected override void AddStatusesFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter cancelledStatusFilter = filters.AddTextFilter("Canceled Status", GetCancelledStatusQuery, CancelledStatusList);
			cancelledStatusFilter.Category = FilterCategories.StatusAndFlags;
			cancelledStatusFilter.DefaultProperty = "ALL";
			cancelledStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|IncompleteTransactionFilter|CanceledStatus", "Canceled Status");
			base.AddStatusesFilters(filters);
		}

		ZQuery GetCancelledStatusQuery(ZString value)
		{
			ZQuery result = new ZQuery();
			if (value != "ALL")
			{
				result.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, value == "CAN");
			}
			return result;
		}

		#region Lookups

		#region TransactionTypeList

		CodeDescriptionPairList fTransactionTypeList;
		protected override CodeDescriptionPairList TransactionTypeList
		{
			get
			{
				if (fTransactionTypeList == null)
				{
					fTransactionTypeList = new CodeDescriptionPairList();
					fTransactionTypeList.AddPair("ALL", Res.GetString("9909f418-20b5-4ca8-9ecb-c62a68cbb514", "All Transactions"));
					fTransactionTypeList.AddPair(TransactionTypes.IncompleteAdjustmentNote, Res.GetString("52b43aad-1e34-4188-9b7e-587d02a79891", "Adjustment Note"));
					fTransactionTypeList.AddPair(TransactionTypes.IncompleteCreditNote, Res.GetString("5bd4ba3b-d8b5-4b5e-929a-4452fb2123b4", "Credit Note"));
					fTransactionTypeList.AddPair(TransactionTypes.IncompleteInvoice, Res.GetString("104aa476-a106-4486-bb7a-e96fc2e42ffc", "Invoice"));
				}
				return fTransactionTypeList;
			}
		}

		#endregion

		#region CancelledStatusList

		CodeDescriptionPairList fCancelledStatusList;
		new CodeDescriptionPairList CancelledStatusList
		{
			get
			{
				if (fCancelledStatusList == null)
				{
					fCancelledStatusList = new CodeDescriptionPairList();
					fCancelledStatusList.AddPair("ACT", Res.GetString("Accounting|IncompleteTransactionFilter|DisplayActiveTransactions", "Only list Active Transactions"));
					fCancelledStatusList.AddPair("CAN", Res.GetString("Accounting|IncompleteTransactionFilter|DisplayCancelledTransactions", "Only list Canceled Transactions"));
					fCancelledStatusList.AddPair("ALL", Res.GetString("Accounting|IncompleteTransactionFilter|DisplayAllTransactions", "List All Transactions"));
				}
				return fCancelledStatusList;
			}
		}

		#endregion

		#endregion
	}
}