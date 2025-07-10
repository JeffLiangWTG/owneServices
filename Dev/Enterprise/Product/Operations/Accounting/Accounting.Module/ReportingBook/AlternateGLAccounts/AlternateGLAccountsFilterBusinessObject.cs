using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class AlternateGLAccountsFilterBusinessObject : FilterStripBusinessObject
	{
		public AlternateGLAccountsFilterBusinessObject()
		{
		}

		#region Module Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddNumberFilters(filters);
			AddStatusFilters(filters);
			AddOtherFilters(filters);
			ModuleAuditFilterProvider.AddAuditFilters(filters, AccAlternateGLAccountSchema.Instance, Factory, typeof(AccAlternateGLAccount));
			return filters;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var descriptionFilter = new ModuleTextFilter("Description", AccAlternateGLAccountSchema.AGA_Description);
			descriptionFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AlternateGLAccountsFilter|Description", "Description");
			descriptionFilter.Visibility = FilterVisibility.AlwaysVisible;
			descriptionFilter.MaxLength = AccAlternateGLAccountSchema.AGA_Description.MaxLength;
			filters.AddFilter(descriptionFilter);
		}

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			var codeFilter = new ModuleGuidFilter("Chart Code", ModuleIDs.AlternateChartofAccounts, AccAlternateGLAccountSchema.AGA_AAC_AlternateChart, ChartList);
			codeFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AlternateGLAccountsFilter|ChartCode", "Chart Code");
			codeFilter.Category = FilterCategories.NumbersAndReferences;
			codeFilter.Visibility = FilterVisibility.AlwaysVisible;

			var alternateAccountFilter = new ModuleTextFilter("Alternate Account", GetAlternateAccountWithSeparator);
			alternateAccountFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AlternateGLAccountsFilter|AlternateAccount", "Alternate Account");
			alternateAccountFilter.Visibility = FilterVisibility.AlwaysVisible;
			alternateAccountFilter.Category = FilterCategories.NumbersAndReferences;
			alternateAccountFilter.MaxLength = AccAlternateGLAccountSchema.AGA_AccountNum.MaxLength;

			var parentAccountFilter = new ModuleGuidFilter("Parent Account", ModuleIDs.AccGLHeader, AccAlternateGLAccountAttributeSchema.AAA_AG_GLHeader, ParentAccounts);
			parentAccountFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AlternateGLAccountsFilter|ParentAccount", "Parent Account");
			parentAccountFilter.Category = FilterCategories.NumbersAndReferences;
			parentAccountFilter.IsBlankFilterQueryDelegate = GetParentAccountIsBlankQuery;
			parentAccountFilter.SubGroup = new ParentAccountSubGroup();
			parentAccountFilter.Visibility = FilterVisibility.AlwaysVisible;

			filters.AddFilter(codeFilter);
			filters.AddFilter(alternateAccountFilter);
			filters.AddFilter(parentAccountFilter);

			var percentNumberFilter = new AlternateGLAccountModuleFilter("Percent Number", ModuleIDs.AlternateGLAccounts, AccAlternateGLAccountSchema.AGA_AGA_PercentNum, PercentNumberCollection);
			percentNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AlternateGLAccountsFilter|PercentNum", "Percent Number");
			percentNumberFilter.Category = FilterCategories.NumbersAndReferences;
			percentNumberFilter.SupportsFiltersMatchComparisonOperator = false;
			filters.AddFilter(percentNumberFilter);

			var consolidateNumberFilter = new AlternateGLAccountModuleFilter("Consolidate Number", ModuleIDs.AlternateGLAccounts, AccAlternateGLAccountSchema.AGA_AGA_ConsolidationNum, ConsolidateNumberCollection);
			consolidateNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AlternateGLAccountsFilter|ConsolidationNum", "Consolidate Number");
			consolidateNumberFilter.Category = FilterCategories.NumbersAndReferences;
			consolidateNumberFilter.SupportsFiltersMatchComparisonOperator = false;
			filters.AddFilter(consolidateNumberFilter);

			var alternateNumberFilter = new AlternateGLAccountModuleFilter("Alternate Number", ModuleIDs.AlternateGLAccounts, AccAlternateGLAccountSchema.AGA_AGA_AlternateNum, AlternateNumberCollection);
			alternateNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AlternateGLAccountsFilter|AlternateNum", "Alternate Number");
			alternateNumberFilter.Category = FilterCategories.NumbersAndReferences;
			alternateNumberFilter.SupportsFiltersMatchComparisonOperator = false;
			filters.AddFilter(alternateNumberFilter);

			var totalReferenceNumberFilter = new AlternateGLAccountModuleFilter("Total Reference Number", ModuleIDs.AlternateGLAccounts, AccAlternateGLAccountSchema.AGA_AGA_HeaderDependsOnTotal, TotalReferenceNumberCollection);
			totalReferenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AlternateGLAccountsFilter|HeaderDependsOnTotal", "Total Reference Number");
			totalReferenceNumberFilter.Category = FilterCategories.NumbersAndReferences;
			totalReferenceNumberFilter.SupportsFiltersMatchComparisonOperator = false;
			filters.AddFilter(totalReferenceNumberFilter);
		}

		void AddStatusFilters(ModuleFilterCollection filters)
		{
			var globalStatusFilter = filters.AddTextFilter("Chart Global Status", GetGlobalStatusFilter, GlobalStatus);
			globalStatusFilter.Category = FilterCategories.StatusAndFlags;
			globalStatusFilter.DefaultProperty = "ALL";
			globalStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AlternateGLAccountsFilter|GlobalStatus", "Chart Global Status");
			globalStatusFilter.MaxLength = 10;
		}

		void AddOtherFilters(ModuleFilterCollection filters)
		{
			var accountTypeFilter = filters.AddTextFilter("Account Type", GetAccountTypeFilter, AccountTypes);
			accountTypeFilter.Category = FilterCategories.Other;
			accountTypeFilter.DefaultProperty = "ALL";
			accountTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AlternateGLAccountsFilter|AccountType", "Account Type");
			accountTypeFilter.Visibility = FilterVisibility.AlwaysVisible;
			accountTypeFilter.MaxLength = AccAlternateGLAccountSchema.AGA_AccountType.MaxLength;

			var reportSectionFilter = filters.AddTextFilter("Report Section", GetReportSectionFilter, SectionTypeList);
			reportSectionFilter.Category = FilterCategories.Other;
			reportSectionFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AlternateGLAccountsFilter|ReportSection", "Report Section");
			reportSectionFilter.MaxLength = AccAlternateGLAccountSchema.AGA_ReportSection.MaxLength;

			var cashFlowTypeFilter = filters.AddTextFilter("Cash Flow Type", GetCashFlowTypeFilter, CashFlowTypes);
			cashFlowTypeFilter.Category = FilterCategories.Other;
			cashFlowTypeFilter.DefaultProperty = "ALL";
			cashFlowTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AlternateGLAccountsFilter|CashFlowType", "Cash Flow Type");
			cashFlowTypeFilter.MaxLength = AccGLHeaderSchema.AG_CashFlowType.MaxLength;
		}

		ZQuery GetAccountTypeFilter(ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(AccAlternateGLAccount));
			if (value != "ALL")
			{
				query.AddToFilter(AccAlternateGLAccountSchema.AGA_AccountType, value);
			}

			return query;
		}

		ZQuery GetReportSectionFilter(ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(AccAlternateGLAccount));
			query.AddToFilter(AccAlternateGLAccountSchema.AGA_ReportSection, value);
			return query;
		}

		ZQuery GetCashFlowTypeFilter(ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(AccGLHeader));
			if (value != "ALL")
			{
				query.AddToFilter(AccGLHeaderSchema.AG_CashFlowType, value);
			}

			return query;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter string")]
		ZQuery GetGlobalStatusFilter(ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(AccAlternateGLAccount));

			var query = new ZDBOnlySubQuery(typeof(AccAlternateChart), AccAlternateGLAccountSchema.AGA_AAC_AlternateChart);
			if (value == "GLOBAL")
			{
				query.AddToFilter(AccAlternateChartSchema.AAC_IsGlobal, ZBool.True);
			}
			else if (value == "NOT GLOBAL")
			{
				query.AddToFilter(AccAlternateChartSchema.AAC_IsGlobal, ZBool.False);
			}
			result.AddSubQuery(query, JoinCondition.And);

			return result;
		}

		ZQuery GetParentAccountIsBlankQuery()
		{
			var result = new ZDBOnlyQuery(typeof(AccAlternateGLAccount));
			var subQuery = new ZDBOnlySubQuery(typeof(AccAlternateGLAccountAttribute), AccAlternateGLAccountAttributeSchema.AAA_AGA_AlternateGLAccount, true);

			result.AddSubQuery(subQuery, JoinCondition.And);
			return result;
		}

		class ParentAccountSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(AccAlternateGLAccount));
				var subQuery = new ZDBOnlySubQuery(typeof(AccAlternateGLAccountAttribute), AccAlternateGLAccountAttributeSchema.AAA_AGA_AlternateGLAccount);
				result.AddSubQuery(subQuery, JoinCondition.And);
				result.AddToFilter(filter);
				return result;
			}
		}

		ZQuery GetAlternateAccountWithSeparator(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery();
			if (!value.IsEmpty
				|| comparisonOperator == SQLComparisonOperator.IsBlank
				|| comparisonOperator == SQLComparisonOperator.IsNotBlank)
			{
				if (value.IsLettersAndNumbersOnlyOrEmpty)
				{
					query.AddToFilter(AccAlternateGLAccountSchema.AGA_AccountNum, comparisonOperator, value);
				}
				else
				{
					if (comparisonOperator == SQLComparisonOperator.Equal
						|| comparisonOperator == SQLComparisonOperator.StartsWith
						|| comparisonOperator == SQLComparisonOperator.Contains)
					{
						var originalValue = value.Replace("-", "").Replace(".", "");
						query.AddToFilter(AccAlternateGLAccountSchema.AGA_AccountNum, comparisonOperator, originalValue);
					}
					var glAccountQuery = Factory.Load<AccAlternateGLAccount>(query);
					if (glAccountQuery.Length > 0)
					{
						var glAccountDic = glAccountQuery.ToDictionary(x => x.PK, x => x.AccountNumWithSeparator);
						List<ZGuid> glAccountPKs;
						if (comparisonOperator == SQLComparisonOperator.Equal)
						{
							glAccountPKs = [.. glAccountDic.Where(x => x.Value.Equals(value)).Select(x => x.Key)];
						}
						else if (comparisonOperator == SQLComparisonOperator.StartsWith)
						{
							glAccountPKs = [.. glAccountDic.Where(x => x.Value.StartsWith(value)).Select(x => x.Key)];
						}
						else if (comparisonOperator == SQLComparisonOperator.Contains)
						{
							glAccountPKs = [.. glAccountDic.Where(x => x.Value.Contains(value)).Select(x => x.Key)];
						}
						else if (comparisonOperator == SQLComparisonOperator.NotEqual)
						{
							glAccountPKs = [.. glAccountDic.Where(x => !x.Value.Equals(value)).Select(x => x.Key)];
						}
						else if (comparisonOperator == SQLComparisonOperator.DoesNotStartWith)
						{
							glAccountPKs = [.. glAccountDic.Where(x => !x.Value.StartsWith(value)).Select(x => x.Key)];
						}
						else
						{
							glAccountPKs = [.. glAccountDic.Where(x => !x.Value.Contains(value)).Select(x => x.Key)];
						}

						var pkQuery = new ZQuery(AccAlternateGLAccountSchema.PK, glAccountPKs.Any() ? glAccountPKs : ZGuid.Empty);
						return pkQuery;
					}
				}
			}
			return query;
		}

		#region Chart Code List

		AccAlternateChartCollection ChartList
		{
			get
			{
				if (chartList == null)
				{
					chartList = new AccAlternateChartCollection(Factory);
				}

				return chartList;
			}
		}

		AccAlternateChartCollection chartList;

		#endregion

		#region Account Type List

		CodeDescriptionPairList AccountTypes
		{
			get
			{
				if (fAccountTypes == null)
				{
					fAccountTypes = new CodeDescriptionPairList();
					fAccountTypes.AddPair("ALL", AllDescription);
					fAccountTypes.AddPair(Core.Constants.AccountType.BalanceSheetAccount, Res.GetString("Accounting|AlternateGLAccountsFilter|BalanceSheet", "Balance Sheet"));
					fAccountTypes.AddPair(Core.Constants.AccountType.ProfitAndLossAccount, Res.GetString("Accounting|AlternateGLAccountsFilter|ProfitLoss", "Profit & Loss"));
					fAccountTypes.AddPair(Core.Constants.AccountType.Total, Res.GetString("Accounting|AlternateGLAccountsFilter|TotalAccount", "Total Account"));
					fAccountTypes.AddPair(Core.Constants.AccountType.Header, Res.GetString("Accounting|AlternateGLAccountsFilter|Header", "Header"));
					fAccountTypes.AddPair(Core.Constants.AccountType.Consolidation, Res.GetString("Accounting|AlternateGLAccountsFilter|ConsolidatedAccount", "Consolidated Account"));
					fAccountTypes.AddPair(Core.Constants.AccountType.Alternate, Res.GetString("Accounting|AlternateGLAccountsFilter|AlternateNumber", "Alternate Number"));
					fAccountTypes.AddPair(Core.Constants.AccountType.Note, Res.GetString("Accounting|AlternateGLAccountsFilter|Note", "Note"));
					fAccountTypes.AddPair(Core.Constants.AccountType.Undefined, Res.GetString("Accounting|AlternateGLAccountsFilter|Undefined", "Undefined / Invalid"));
				}
				return fAccountTypes;
			}
		}

		CodeDescriptionPairList fAccountTypes;

		#endregion

		#region Report Section

		public CodeDescriptionPairList SectionTypeList
		{
			get
			{
				if (sectionTypeList == null)
				{
					sectionTypeList = new CodeDescriptionPairList();
					sectionTypeList.AddPair(AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, AccGLHeader.Constants.SectionTypes.Descriptions.TradingStatement);
					sectionTypeList.AddPair(AccGLHeader.Constants.SectionTypes.Codes.Overheads, AccGLHeader.Constants.SectionTypes.Descriptions.Overheads);
					sectionTypeList.AddPair(AccGLHeader.Constants.SectionTypes.Codes.ProfitAndLossAppropriation, AccGLHeader.Constants.SectionTypes.Descriptions.ProfitAndLossAppropriation);
					sectionTypeList.AddPair(AccGLHeader.Constants.SectionTypes.Codes.OwnersEquity, AccGLHeader.Constants.SectionTypes.Descriptions.OwnersEquity);
					sectionTypeList.AddPair(AccGLHeader.Constants.SectionTypes.Codes.Assets, AccGLHeader.Constants.SectionTypes.Descriptions.Assets);
					sectionTypeList.AddPair(AccGLHeader.Constants.SectionTypes.Codes.Liabilities, AccGLHeader.Constants.SectionTypes.Descriptions.Liabilities);
				}
				return sectionTypeList;
			}
		}

		CodeDescriptionPairList sectionTypeList;

		#endregion

		#region Cash Flow Type List

		CodeDescriptionPairList CashFlowTypes
		{
			get
			{
				if (fCashFlowTypes == null)
				{
					fCashFlowTypes = new CodeDescriptionPairList();
					fCashFlowTypes.AddPair("ALL", AllDescription);
					foreach (CashFlowActivityConfiguration cashFlowActivity in AccountingMasterFilesRegistry.Instance.CashFlowActivityConfiguration.Value)
					{
						fCashFlowTypes.AddPair(cashFlowActivity.Code, cashFlowActivity.Description);
					}
				}
				return fCashFlowTypes;
			}
		}

		CodeDescriptionPairList fCashFlowTypes;

		#endregion

		#region Parent Account

		AccGLHeaderCollection ParentAccounts
		{
			get
			{
				if (parentAccounts == null)
				{
					parentAccounts = new AccGLHeaderCollection(Factory);
				}

				return parentAccounts;
			}
		}

		AccGLHeaderCollection parentAccounts;

		#endregion

		#region GlobalStatus List

		CodeDescriptionPairList GlobalStatus
		{
			get
			{
				if (fGlobalStatus == null)
				{
					fGlobalStatus = new CodeDescriptionPairList();
					fGlobalStatus.AddPair("ALL", AllDescription);
					fGlobalStatus.AddPair("GLOBAL", Res.GetString("Accounting|AlternateGLAccountsFilter|Global", "Global"));
					fGlobalStatus.AddPair((NoResString)"NOT GLOBAL", Res.GetString("Accounting|AlternateGLAccountsFilter|NotGlobal", "Not Global"));
				}

				return fGlobalStatus;
			}
		}

		CodeDescriptionPairList fGlobalStatus;

		#endregion

		#region AlternateGLAccountFilter

		public AlternateGLAccountFilterHelper AlternateGLAccountFilter
		{
			get
			{
				var filterHelper = new AlternateGLAccountFilterHelper(Factory);
				filterHelper.SetOuterQuery(base.Filter);
				return filterHelper;
			}
		}

		#endregion

		ZString AllDescription => Res.GetString("Accounting|AlternateGLAccountsFilter|All", "All");

		#endregion

		protected override bool ShouldAddCustomSqlFilter => false;

		protected override bool ShouldAddUserDefinedFiltersCore => false;

		#region Percent Number Collection

		public AccAlternateGLAccountCollection PercentNumberCollection
		{
			get
			{
				var queryFilter = new ZQuery().AddToFilter(AccAlternateGLAccountSchema.AGA_AccountType, new List<string>() { Core.Constants.AccountType.Consolidation, Core.Constants.AccountType.Total });

				if (percentNumberCollection == null)
				{
					percentNumberCollection = new AccAlternateGLAccountCollection(Factory, queryFilter);
					percentNumberCollection.Load();
				}

				return percentNumberCollection;
			}
		}

		AccAlternateGLAccountCollection percentNumberCollection;

		#endregion

		#region Consolidate Number Collection

		AccAlternateGLAccountCollection ConsolidateNumberCollection
		{
			get
			{
				var queryFilter = new ZQuery().AddToFilter(AccAlternateGLAccountSchema.AGA_AccountType, Core.Constants.AccountType.Consolidation);

				if (consolidateNumberCollection == null)
				{
					consolidateNumberCollection = new AccAlternateGLAccountCollection(Factory, queryFilter);
					consolidateNumberCollection.Load();
				}

				return consolidateNumberCollection;
			}
		}

		AccAlternateGLAccountCollection consolidateNumberCollection;

		#endregion

		#region Alternate Number Collection

		AccAlternateGLAccountCollection AlternateNumberCollection
		{
			get
			{
				var queryFilter = new ZQuery().AddToFilter(AccAlternateGLAccountSchema.AGA_AccountType, Core.Constants.AccountType.Alternate);

				if (alternateNumberCollection == null)
				{
					alternateNumberCollection = new AccAlternateGLAccountCollection(Factory, queryFilter);
					alternateNumberCollection.Load();
				}

				return alternateNumberCollection;
			}
		}

		AccAlternateGLAccountCollection alternateNumberCollection;

		#endregion

		#region Total Reference Number Collection

		AccAlternateGLAccountCollection TotalReferenceNumberCollection
		{
			get
			{
				var queryFilter = new ZQuery().AddToFilter(AccAlternateGLAccountSchema.AGA_AccountType, Core.Constants.AccountType.Total);

				if (totalReferenceNumberCollection == null)
				{
					totalReferenceNumberCollection = new AccAlternateGLAccountCollection(Factory, queryFilter);
					totalReferenceNumberCollection.Load();
				}

				return totalReferenceNumberCollection;
			}
		}

		AccAlternateGLAccountCollection totalReferenceNumberCollection;

		#endregion
	}
}
