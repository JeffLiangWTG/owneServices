using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Filters;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class AccReportingBookFilterBusinessObject : FilterStripBusinessObject
	{
		public AccReportingBookFilterBusinessObject()
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			SetActiveStatusFilter(AccReportingBookSchema.ARB_IsActive, true);

			var filters = new ModuleFilterCollection();
			AddCodeFilter(filters);
			AddGlobalStatusFilter(filters);
			AddPresentationJournalFilter(filters);
			AddAlternateChartFilter(filters);

			return filters;
		}

		void AddCodeFilter(ModuleFilterCollection filters)
		{
			var codeFilter = filters.AddTextFilter("Code", AccReportingBookSchema.ARB_Code);
			codeFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccReportingBook|Code", "Code");
			codeFilter.MaxLength = AccReportingBookSchema.ARB_Code.MaxLength;
		}

		#region Global Status

		void AddGlobalStatusFilter(ModuleFilterCollection filters)
		{
			var globalStatusFilter = filters.AddTextFilter("Global Status", GlobalStatusQuery, GlobalStatus);
			globalStatusFilter.Category = FilterCategories.StatusAndFlags;
			globalStatusFilter.DefaultProperty = "ALL";
			globalStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccReportingBook|GlobalStatus", "Global Status");
			globalStatusFilter.MaxLength = 10;
		}

		ZQuery GlobalStatusQuery(ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(AccReportingBook));
			var subQuery = new ZDBOnlySubQuery(typeof(AccAlternateChart), AccReportingBookSchema.ARB_AAC_AlternateChart);
			if (value == GlobalStatus[1].Code)
			{
				subQuery.AddToFilter(AccAlternateChartSchema.AAC_IsGlobal, ZBool.True);
			}
			else if (value == GlobalStatus[2].Code)
			{
				subQuery.AddToFilter(AccAlternateChartSchema.AAC_IsGlobal, ZBool.False);
			}
			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}

		CodeDescriptionPairList GlobalStatus
		{
			get
			{
				if (fGlobalStatus == null)
				{
					fGlobalStatus = new CodeDescriptionPairList();
					fGlobalStatus.AddPair("ALL", Res.GetString("Accounting|AccReportingBook|All", "All"));
					fGlobalStatus.AddPair("GLOBAL", Res.GetString("Accounting|AccReportingBook|Global", "Global"));
					fGlobalStatus.AddPair((NoResString)"NOT GLOBAL", Res.GetString("Accounting|AccReportingBook|NotGlobal", "Not Global"));
				}

				return fGlobalStatus;
			}
		}

		CodeDescriptionPairList fGlobalStatus;

		#endregion

		#region PresentationCategoryList

		void AddPresentationJournalFilter(ModuleFilterCollection filters)
		{
			var presentationJournalFilter = new PresentationCategoryFilter("Presentation Journal", GetIncludePresentationJournalsQuery, PresentationCategoryList);
			presentationJournalFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccReportingBook|PresentationJournal", "Presentation Journal");
			presentationJournalFilter.MaxLength = AccReportingBookSchema.ARB_IncludePresentationJournals.MaxLength;
			filters.AddFilter(presentationJournalFilter);
		}

		ZQuery GetIncludePresentationJournalsQuery(SQLComparisonOperator @operator, ZString value)
		{
			var operatorList = new List<SQLComparisonOperator>() { SQLComparisonOperator.Contains, SQLComparisonOperator.NotContains, SQLComparisonOperator.Equal, SQLComparisonOperator.NotEqual };
			var query = new ZDBOnlyQuery(typeof(AccReportingBook));
			if (string.IsNullOrEmpty(value) || !operatorList.Contains(@operator))
			{
				query.AddToFilter(AccReportingBookSchema.ARB_IncludePresentationJournals, @operator, value);
				return query;
			}

			var glPresentationJournalCategoryCollection = AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.Value.Cast<GLPresentationJournalCategory>();
			var childCategories = glPresentationJournalCategoryCollection.Where(x => x.Bool && x.Code.Contains(value)).Select(x => x.Code).ToList();
			if (@operator == SQLComparisonOperator.Equal || @operator == SQLComparisonOperator.NotEqual)
			{
				childCategories = childCategories.Where(x => x.Equals(value)).ToList();
			}

			if(childCategories.Count == 0)
			{
				query.AddToFilter(AccReportingBookSchema.ARB_IncludePresentationJournals, @operator, value);
				return query;
			}
			var realOperator = @operator == SQLComparisonOperator.Contains || @operator == SQLComparisonOperator.Equal ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual;

			var parentCategories = glPresentationJournalCategoryCollection.Where(x => x.Bool && childCategories.Contains(x.Code) && x.ParentCode != "").Select(x => x.ParentCode).Distinct().ToList();
			query.AddToFilter(AccReportingBookSchema.ARB_IncludePresentationJournals, realOperator, childCategories);
			if(parentCategories.Count > 0)
			{
				if(@operator == SQLComparisonOperator.NotContains || @operator == SQLComparisonOperator.NotEqual)
				{
					query.AddToFilter(JoinCondition.And, AccReportingBookSchema.ARB_IncludeChildPresentation, SQLComparisonOperator.Equal, ZBool.False);
				}

				var subQuery = new ZQuery();
				subQuery.AddToFilter(AccReportingBookSchema.ARB_IncludePresentationJournals, realOperator, parentCategories);
				subQuery.AddToFilter(JoinCondition.And, AccReportingBookSchema.ARB_IncludeChildPresentation, SQLComparisonOperator.Equal, ZBool.True);
				query.AddToFilter(subQuery, JoinCondition.Or);
			}
			return query;
		}

		CodeDescriptionPairList fPresentationCategoryList;
		CodeDescriptionPairList PresentationCategoryList
		{
			get
			{
				if (fPresentationCategoryList == null)
				{
					fPresentationCategoryList = new CodeDescriptionPairList(AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).GetActiveCodeDescriptionPairList());
					fPresentationCategoryList.AddRangeOverwriteIfExists(AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).GetActiveCodeDescriptionPairList());
				}

				return fPresentationCategoryList;
			}
		}

		#endregion

		#region Alternate Chart

		void AddAlternateChartFilter(ModuleFilterCollection filters)
		{
			filters.AddGuidFilter("Alternate Chart", ModuleIDs.AlternateChartofAccounts, AccReportingBookSchema.ARB_AAC_AlternateChart, AlternateChartCollection).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccReportingBook|AlternateChart", "Alternate Chart");
		}

		AccAlternateChartCollection fAccAlternateChartCollection;
		public AccAlternateChartCollection AlternateChartCollection
		{
			get
			{
				if (fAccAlternateChartCollection == null)
				{
					fAccAlternateChartCollection = new AccAlternateChartCollection(Factory);
				}
				return fAccAlternateChartCollection;
			}
		}

		#endregion

		protected override bool ShouldAddCustomSqlFilter => false;

		protected override bool ShouldAddUserDefinedFiltersCore => false;
	}
}
