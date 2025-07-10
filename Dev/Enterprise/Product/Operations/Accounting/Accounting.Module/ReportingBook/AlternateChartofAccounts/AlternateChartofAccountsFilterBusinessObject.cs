using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class AlternateChartofAccountsFilterBusinessObject : FilterStripBusinessObject
	{
		public AlternateChartofAccountsFilterBusinessObject()
		{ }

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddStatusFilters(filters);
			AddModesAndTypesFilters(filters);
			return filters;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var codeTextFilter = new ModuleTextFilter("Code", AccAlternateChartSchema.AAC_Code);
			codeTextFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AlternateChartofAccountsFilter|Code", "Code");
			codeTextFilter.MaxLength = AccAlternateChartSchema.AAC_Code.MaxLength;

			var nameTextFilter = new ModuleTextFilter("Name", AccAlternateChartSchema.AAC_Description);
			nameTextFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AlternateChartofAccountsFilter|Name", "Name");
			nameTextFilter.MaxLength = AccAlternateChartSchema.AAC_Description.MaxLength;

			RemoveIsBlankAndIsNotBlank(codeTextFilter);
			RemoveIsBlankAndIsNotBlank(nameTextFilter);
			filters.AddFilter(codeTextFilter);
			filters.AddFilter(nameTextFilter);
		}

		void RemoveIsBlankAndIsNotBlank(ModuleTextFilter filter)
		{
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
		}

		void AddStatusFilters(ModuleFilterCollection filters)
		{
			var accountLengthFilter = filters.AddTextFilter("Account Length", GetAccountLengthFilter, AccountLength);
			accountLengthFilter.Category = FilterCategories.StatusAndFlags;
			accountLengthFilter.DefaultProperty = "ALL";
			accountLengthFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AlternateChartofAccountsFilter|AccountLength", "Account Length");
			accountLengthFilter.MaxLength = 8;

			var globalStatusFilter = filters.AddTextFilter("Global Status", GetGlobalStatusFilter, GlobalStatus);
			globalStatusFilter.Category = FilterCategories.StatusAndFlags;
			globalStatusFilter.DefaultProperty = "ALL";
			globalStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AlternateChartofAccountsFilter|GlobalStatus", "Global Status");
			globalStatusFilter.MaxLength = 10;
		}

		void AddModesAndTypesFilters(ModuleFilterCollection filters)
		{
			var balanceSheetStyleFilter = filters.AddTextFilter("Balance Sheet Style", AccAlternateChartSchema.AAC_BalanceSheetStyle, AccAlternateChartLookups.GetBaseBalanceSheetStyleList());
			balanceSheetStyleFilter.RemoveComparisonOperatorsLeavingOne(ModuleNumberFilter.ComparisonConstants.Exact);
			balanceSheetStyleFilter.Category = FilterCategories.ModesAndTypes;
			balanceSheetStyleFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AlternateChartofAccountsFilter|BalanceSheetStyle", "Balance Sheet Style");
			balanceSheetStyleFilter.MaxLength = AccAlternateChartSchema.AAC_BalanceSheetStyle.MaxLength;
		}

		ZQuery GetAccountLengthFilter(ZString value)
		{
			var query = new ZQuery();
			if (value == "FIXED")
			{
				query.AddToFilter(AccAlternateChartSchema.AAC_IsFixedLength, ZBool.True);
			}
			else if (value == "VARIABLE")
			{
				query.AddToFilter(AccAlternateChartSchema.AAC_IsFixedLength, ZBool.False);
			}

			return query;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter string")]
		ZQuery GetGlobalStatusFilter(ZString value)
		{
			var query = new ZQuery();
			if (value == "GLOBAL")
			{
				query.AddToFilter(AccAlternateChartSchema.AAC_IsGlobal, ZBool.True);
			}
			else if (value == "NOT GLOBAL")
			{
				query.AddToFilter(AccAlternateChartSchema.AAC_IsGlobal, ZBool.False);
			}

			return query;
		}

		#region SetCurrentAlternateChartFormat

		public void SetCurrentAlternateChartFormat(AccAlternateChart chart)
		{
			AlternateChartFormats.RemoveAll();

			if (chart != null)
			{
				AlternateChartFormats.AddRange(chart.AlternateChartFormats);
			}
		}
		#endregion

		#region Fixed Length Status List

		CodeDescriptionPairList AccountLength
		{
			get
			{
				if (fAccountLength == null)
				{
					fAccountLength = new CodeDescriptionPairList();
					fAccountLength.AddPair("ALL", Res.GetString("Accounting|AlternateChartofAccountsFilter|All", "All"));
					fAccountLength.AddPair("FIXED", Res.GetString("Accounting|AlternateChartofAccountsFilter|Fixed", "Fixed"));
					fAccountLength.AddPair("VARIABLE", Res.GetString("Accounting|AlternateChartofAccountsFilter|Variable", "Variable"));
				}

				return fAccountLength;
			}
		}

		CodeDescriptionPairList fAccountLength;

		#endregion

		#region GlobalStatus List

		CodeDescriptionPairList GlobalStatus
		{
			get
			{
				if (fGlobalStatus == null)
				{
					fGlobalStatus = new CodeDescriptionPairList();
					fGlobalStatus.AddPair("ALL", Res.GetString("Accounting|AlternateChartofAccountsFilter|All", "All"));
					fGlobalStatus.AddPair("GLOBAL", Res.GetString("Accounting|AlternateChartofAccountsFilter|Global", "Global"));
					fGlobalStatus.AddPair((NoResString)"NOT GLOBAL", Res.GetString("Accounting|AlternateChartofAccountsFilter|NotGlobal", "Not Global"));
				}

				return fGlobalStatus;
			}
		}

		CodeDescriptionPairList fGlobalStatus;

		#endregion

		AccAlternateChartFormatCollection fAlternateChartFormats;
		public AccAlternateChartFormatCollection AlternateChartFormats
		{
			get
			{
				if (fAlternateChartFormats == null)
				{
					fAlternateChartFormats = new AccAlternateChartFormatCollection(Factory);
					fAlternateChartFormats.SetReadOnlyIncludingChildren(true);
				}
				return fAlternateChartFormats;
			}
		}

		protected override bool ShouldAddCustomSqlFilter => false;

		protected override bool ShouldAddUserDefinedFiltersCore => false;
	}
}
