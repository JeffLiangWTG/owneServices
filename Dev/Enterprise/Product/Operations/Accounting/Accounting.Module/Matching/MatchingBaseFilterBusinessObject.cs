using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Unmatching;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using AccountingConstants = Enterprise.Accounting.Business.AccountingUtils;

namespace Enterprise.Accounting.Module
{
	public abstract class MatchingBaseFilterBusinessObject : AccountingFilterStripBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddNumberFilters(filters);
			AddDateFilters(filters);
			AddOrganisationFilters(filters);
			AddModesAndTypesFilters(filters);
			return filters;
		}

		public override ZQuery Filter
		{
			get
			{
				ZQuery resultFilter = base.Filter;
				AddLedgerFilter(resultFilter);
				return resultFilter;
			}
		}

		protected abstract void AddNumberFilters(ModuleFilterCollection filters);
		protected abstract void AddDateFilters(ModuleFilterCollection filters);
		protected abstract void AddOrganisationFilters(ModuleFilterCollection filters);

		#region Transaction Type Filter

		void AddModesAndTypesFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter transactionTypeFilter = filters.AddTextFilter(AccountingConstants.ModesAndTypesFilterTypes.TransactionType, GetTransactionTypeQuery, TransactionTypeList);
			transactionTypeFilter.Category = FilterCategories.ModesAndTypes;
			transactionTypeFilter.MultilingualDescription = AccountingConstants.ModesAndTypesFilterTypes.TransactionTypeDescription;
		}

		ZQuery GetTransactionTypeQuery(ZString value)
		{
			ZQuery result = new ZQuery();
			if (value != "ALL")
			{
				result.AddToFilter(ViewMatchGroupSchema.MG_TransactionType, value);
			}
			return result;
		}

		#endregion

		#region Ledger Filter

		public ZString LedgerFilterValue { get; set; }

		protected virtual void AddLedgerFilter(ZQuery filter)
		{
			if (!LedgerFilterValue.IsEmpty)
			{
				filter.AddToFilter(ViewMatchGroupSchema.MG_Ledger, LedgerFilterValue);
			}
		}

		#endregion

		#endregion

		#region Public MatchingFilterBusinessObject Members

		#region SetCurrentTransactionsForMatchGroup

		public void SetCurrentTransactionsForMatchGroup(BusinessObject selectedMatchGroup)
		{
			if (selectedMatchGroup is UnmatchingRow matchGroup)
			{
				TransactionHeaders.RemoveAll();

				var transactionLoadQuery = GetQueryForTransactionLoadWithFK(matchGroup);
				var loadFactory = GetFactoryForTransactionLoad();
				var matchedTransactions = loadFactory.Load(typeof(TransactionHeader), transactionLoadQuery);
				using (TransactionHeaders.SuspendListChanged())
				{
					foreach (TransactionHeader matchedTransaction in matchedTransactions)
					{
						if (matchedTransaction != null)
						{
							var matchLinks = matchGroup.MatchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, matchedTransaction.PK));
							if (matchLinks.Length > 0)
							{
								var matchLink = (TransactionMatchLink)matchLinks[0];
								matchedTransaction.SetMatchedAmount(matchLink, true);
								matchedTransaction.MatchedDate = matchLink.AP_MatchDate;
								TransactionHeaders.Add(matchedTransaction);
							}
						}
					}
				}
			}
		}

		ZQuery GetQueryForTransactionLoadWithFK(UnmatchingRow matchGroup)
		{
			var transactionLoadFilter = new ZQuery();
			transactionLoadFilter.DefaultJoinCondition = JoinCondition.Or;

			var guidList = new List<ZGuid>();

			foreach (TransactionMatchLink matchLink in matchGroup.MatchLinks)
			{
				guidList.Add(matchLink.AP_AH);
			}

			transactionLoadFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.Equal, guidList);

			return transactionLoadFilter;
		}

		BusinessObjectFactory GetFactoryForTransactionLoad()
		{
			return new BusinessObjectFactory()
			{
				NameForDebugging = GetTransactionFactoryName
			};
		}

		protected abstract string GetTransactionFactoryName { get; }

		public void ClearCurrentTransactionsForMatchGroup()
		{
			TransactionHeaders.RemoveAll();
		}

		#endregion

		#region MatchGroupFilter

		public MatchGroupFilterHelper MatchGroupFilter => MatchGroupFilterCore;
		protected abstract MatchGroupFilterHelper MatchGroupFilterCore { get; }

		#endregion

		#region TransactionHeaders

		public TransactionHeaderCollection TransactionHeaders => TransactionHeadersCore;
		protected abstract TransactionHeaderCollection TransactionHeadersCore { get; }

		#endregion

		#endregion

		#region Lists

		public OrgHeaderCollection Organisations => OrganisationsCore;
		protected abstract OrgHeaderCollection OrganisationsCore { get; }

		protected virtual CodeDescriptionPairList TransactionTypeList => AHTransactionTypeList;

		#endregion
	}
}
