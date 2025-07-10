using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Unmatching;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using AccountingConstants = Enterprise.Accounting.Business.AccountingUtils;

namespace Enterprise.Accounting.Module
{
	public class APMatchingFilterBusinessObject : MatchingBaseFilterBusinessObject
	{
		public APMatchingFilterBusinessObject()
			: base()
		{
		}

		#region Filters

		protected override void AddNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(AccountingConstants.NumberFilterTypes.MatchGroupNumber, ViewMatchGroupSchema.MG_MatchGroupNum).
				MultilingualDescription = ResString.GetMultilingualString("Accounting|APMatchingFilter|MatchGroupNumber", "Match Group #");
			filters.AddNumberFilter(AccountingConstants.NumberFilterTypes.TransactionNumber, ViewMatchGroupSchema.MG_TransactionNum).
				MultilingualDescription = ResString.GetMultilingualString("Accounting|APMatchingFilter|TransactionNumber", "Transaction #");
			filters.AddNumberFilter(AccountingConstants.NumberFilterTypes.ChequeReferenceNumber, ViewMatchGroupSchema.MG_ChequeOrReference).
				MultilingualDescription = ResString.GetMultilingualString("Accounting|APMatchingFilter|ChequeReferenceNumber", "Check/Reference #");
			filters.AddNumberFilter(AccountingConstants.NumberFilterTypes.ConsolidationNumber, ViewMatchGroupSchema.MG_ConsolidatedInvoiceRef).
				MultilingualDescription = ResString.GetMultilingualString("Accounting|APMatchingFilter|ConsolidationNumber", "Job Invoice #");
		}

		protected override void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(AccountingConstants.DateFilterTypes.DueDate, ViewMatchGroupSchema.MG_DueDate).
				MultilingualDescription = ResString.GetMultilingualString("Accounting|APMatchingFilter|DueDate", "Due Date");
			filters.AddDateFilter(AccountingConstants.DateFilterTypes.PostDate, ViewMatchGroupSchema.MG_PostDate).
				MultilingualDescription = ResString.GetMultilingualString("Accounting|APMatchingFilter|PostDate", "Post Date");
			filters.AddDateFilter(AccountingConstants.DateFilterTypes.TransactionDate, ViewMatchGroupSchema.MG_InvoiceDate).
				MultilingualDescription = ResString.GetMultilingualString("Accounting|APMatchingFilter|TransactionDate", "Transaction Date");
			filters.AddDateFilter(AccountingConstants.DateFilterTypes.MatchDate, ViewMatchGroupSchema.MG_MatchDate).
				MultilingualDescription = ResString.GetMultilingualString("Accounting|APMatchingFilter|MatchDate", "Match Date");
		}

		protected override void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			filters.AddGuidFilter("Organisation", ModuleIDs.Organisation, ViewMatchGroupSchema.MG_OH, Organisations).MultilingualDescription = ResString.GetMultilingualString("Accounting|APMatchingFilter|Organisation", "Organization");
		}

		#endregion

		#region IMatchingFilterBusinessObject Members

		protected override string GetTransactionFactoryName => "LoadTransactionForAPMatchGroupMatching";

		#region MatchGroupFilter

		protected override MatchGroupFilterHelper MatchGroupFilterCore
		{
			get
			{
				MatchGroupFilterHelper filterHelper = new APMatchGroupFilterHelper(Factory);
				filterHelper.SetOuterMatchGroupQuery(base.Filter);
				return filterHelper;
			}
		}

		#endregion

		#region TransactionHeaders

		TransactionHeaderCollection fTransactionHeaders;
		protected override TransactionHeaderCollection TransactionHeadersCore
		{
			get
			{
				if (fTransactionHeaders == null)
				{
					fTransactionHeaders = new TransactionHeaderCollection(Factory);
					fTransactionHeaders.SetReadOnlyIncludingChildren(true);
				}

				return fTransactionHeaders;
			}
		}

		#endregion

		#endregion

		#region Lists

		OrgHeaderCollection fOrganisations;
		protected override OrgHeaderCollection OrganisationsCore
		{
			get
			{
				if (fOrganisations == null)
				{
					fOrganisations = new OrgHeaderCollection(Factory);
				}

				return fOrganisations;
			}
		}

		#endregion
	}
}
