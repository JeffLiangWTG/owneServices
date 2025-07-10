using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.IE;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.ExitControl.Module
{
	public class ExitControlReportFilterBusinessObject : EU.ExitControl.Module.ExitControlReportFilterBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter constants")]
		public static class IEFilterConstants
		{
			public const string ExitArrivalDate = "Exit/Arrival Date";
			public const string Discrepancies = "Discrepancies";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var moduleFilterCollection = base.GetModuleFiltersCore();
			AddExitArrivalDateFilter(moduleFilterCollection);
			AddHasDiscrepanciesFilter(moduleFilterCollection);
			return moduleFilterCollection;
		}

		protected override IList MessageStatusList => Factory.GetCachedValue<IELogicalStatusList>();

		protected override ModuleFilter AddNewStatusFilter(ModuleFilterCollection moduleFilterCollection)
		{
			var filter = moduleFilterCollection.AddTextFilter(FilterConstants.Status, CusExitReportSchema.CER_Status, Factory.GetCachedValue<AESEntryStatusList>());
			ClearupStatusList(filter);
			return filter;
		}

		void AddExitArrivalDateFilter(ModuleFilterCollection moduleFilterCollection)
		{
			var arrivalDateFilter = moduleFilterCollection.AddDateFilter(IEFilterConstants.ExitArrivalDate, CusExitReportSchema.CER_DateTime);
			arrivalDateFilter.Category = FilterCategories.Dates;
			arrivalDateFilter.MultilingualDescription = ResString.GetMultilingualString("141F939C-340D-4898-AC1B-35E965540D8A", IEFilterConstants.ExitArrivalDate);
		}

		void AddHasDiscrepanciesFilter(ModuleFilterCollection moduleFilterCollection)
		{
			var discrepanciesFilter = moduleFilterCollection.AddFlagsFilter(
				description: IEFilterConstants.Discrepancies,
				queryDelegates: new GetFlagsQuery[] { GetIsDiscrepanciesQuery },
				flagNames: new[] { Res.GetString("E5170165-5011-4974-8D96-15536AF1A48A", "Has Discrepancies") }
			);
			discrepanciesFilter.MultilingualDescription = ResString.GetMultilingualString("FC563828-4EA6-4BE4-85C9-A01BE7BFF3E8", IEFilterConstants.Discrepancies);
			discrepanciesFilter.Category = FilterCategories.StatusAndFlags;
		}

		ZQuery GetIsDiscrepanciesQuery(ZBool isDiscrepancies) => new ZQuery(
				CusExitReportSchema.CER_Behavior,
				isDiscrepancies ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual,
				ExitReportDiscrepancyTypeList.Codes.Discrepancies
		);
	}
}
