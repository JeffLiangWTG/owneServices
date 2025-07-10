
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Intrastat.Module
{
	public class ReportsFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string GroupNumber = "Group Number";
			public const string Reporter = "Reporter";
			public const string Trader = "Trader";
			public const string Period = "Reporting Period";
			public const string Flow = "Flow";
			public const string Status = "Status";
		}

		public ReportsFilterLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = GetNewLookups();
				}
				return lookups;
			}
		}
		ReportsFilterLookups lookups;

		protected virtual ReportsFilterLookups GetNewLookups()
		{
			return new ReportsFilterLookups(this);
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var groupNumberFilter = result.AddNumberFilter(Schema.GroupNumber, GetGroupNumberQuery);
			groupNumberFilter.MaxLength = CusIntrastatGroupSchema.CIG_GroupNumber.MaxLength;
			groupNumberFilter.MultilingualDescription = ResString.GetMultilingualString("ReportsFilter|ReportNumber", Schema.GroupNumber);

			var periodFilter = new Customs.Module.PeriodFilter(Schema.Period, GetPeriodQuery);
			periodFilter.MultilingualDescription = ResString.GetMultilingualString("ReportsFilter|Period", Schema.Period);
			periodFilter.Category = FilterCategories.Dates;
			result.AddCustomFilter(periodFilter);

			var flowFilter = result.AddTextFilter(Schema.Flow, CusIntrastatGroupSchema.CIG_Flow, Lookups.Flows);
			flowFilter.MultilingualDescription = ResString.GetMultilingualString("ReportsFilter|Flow", Schema.Flow);
			flowFilter.Category = FilterCategories.Locations;

			var statusFilter = result.AddTextFilter(Schema.Status, CusIntrastatGroupSchema.CIG_Status, Lookups.Status);
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("ReportsFilter|Status", Schema.Status);
			statusFilter.Category = FilterCategories.StatusAndFlags;

			AddOrganisationFilters(result);

			return result;
		}

		#region Number Filters

		ZQuery GetGroupNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery().AddToFilter_PossiblyCommaSeparated(CusIntrastatGroupSchema.CIG_GroupNumber, comparisonOperator, value);
		}

		ZQuery GetPeriodQuery(SQLComparisonOperator comparisonOperator, SchemaDateTimeColumn filtercolumn, ZInt year, ZInt month)
		{
			var result = new ZQuery();
			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				_ = result.AddToFilter(ZQuery.NoResultQuery); // cannot be null or empty
			}
			else if (comparisonOperator == SQLComparisonOperator.Equal || comparisonOperator == SQLComparisonOperator.NotEqual)
			{
				var period = $"{year:0000}-{month:00}";
				_ = result.AddToFilter(CusIntrastatGroupSchema.CIG_Period, comparisonOperator, period);
			}
			return result;
		}

		#endregion

		#region OrganisationFilters

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			var reporterFilter = filters.AddGuidFilter(Schema.Reporter, ModuleIDs.Organisation, GetReporterQuery, Lookups.Reporters);
			reporterFilter.MultilingualDescription = ResString.GetMultilingualString("ReportsFilter|Reporter", Schema.Reporter);
			reporterFilter.Category = FilterCategories.Organisations;

			var traderFilter = filters.AddGuidFilter(Schema.Trader, ModuleIDs.Organisation, GetTraderQuery, Lookups.Traders);
			traderFilter.MultilingualDescription = ResString.GetMultilingualString("ReportsFilter|Trader", Schema.Trader);
			traderFilter.Category = FilterCategories.Organisations;
		}

		ZQuery GetReporterQuery(ZGuid reporter)
		{
			var result = new ZQuery();

			if (!reporter.IsEmpty)
			{
				_ = result.AddToFilter(new ZQuery(CusIntrastatGroupSchema.CIG_OH_Reporter, reporter));
			}

			return result;
		}

		ZQuery GetTraderQuery(ZGuid trader)
		{
			var result = new ZQuery();

			if (!trader.IsEmpty)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(CusIntrastatMergedLine), CusIntrastatMergedLineSchema.CIM_CIG_Group);
				_ = subQuery.AddToFilter(CusIntrastatMergedLineSchema.CIM_OH_Trader, trader);

				var query = new ZDBOnlyQuery(typeof(CusIntrastatGroup));
				query.AddSubQuery(subQuery, JoinCondition.And);

				_ = result.AddToFilter(query);
			}

			return result;
		}

		#endregion

		protected override void AddInitialAuditFilters(ModuleFilterCollection filters)
		{
			base.AddInitialAuditFilters(filters);

			var createdTimeFilter = filters[FilterDescriptions.CreatedTime] as ModuleDateFilter;
			if (createdTimeFilter != null)
			{
				createdTimeFilter.Visibility = FilterVisibility.AlwaysVisible;
				createdTimeFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Last3Mths;
			}
		}
	}
}
