#if DEBUG

using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Module
{
	public partial class TransactionFilterStripBusinessObject
	{
		public bool IsSingleLedger_ForTestOnly => IsSingleLedger;

		public bool ShouldAddComplianceDocumentRecordFilter_ForTestOnly => ShouldAddComplianceDocumentRecordFilter;

		public ModuleFilterCollection GetModuleFiltersCore_ForTestOnly()
		{
			return GetModuleFiltersCore();
		}

		public ZString CreditorDebtorGroupText_ForTestOnly => CreditorDebtorGroupText;

		public SecurityCheckpoint ViewingNonLoginBranchTransactions_ForTestOnly => ViewingNonLoginBranchTransactions;

		public bool ShouldAddRelatedTransactionsNotPaidFilter_ForTestOnly => ShouldAddRelatedTransactionsNotPaidFilter;

		public ZBool UseCreditor_ForTestOnly => UseCreditor;

		public OrgHeaderCollection AH_OHList_ForTestOnly => AH_OHList;

		public ZBool UseDebtor_ForTestOnly => UseDebtor;
	}
}

#endif
