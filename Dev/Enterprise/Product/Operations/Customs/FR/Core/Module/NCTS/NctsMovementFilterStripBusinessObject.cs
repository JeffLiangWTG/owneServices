using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Module
{
	public class NctsMovementFilterStripBusinessObject : EU.NCTS.Module.NctsMovementFilterStripBusinessObject
	{
		public static class FRFilterConstants
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "FilterConstants")]
			public const string DetailedStatus = "Detailed Status";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();

			var detailedStatus = filters.AddTextFilter(FRFilterConstants.DetailedStatus, GetDetailedStatusQuery, DetailedStatusCodeList).WithMaxLengthOf<ModuleTextFilter>(CusFRNctsHeaderSchema.CFN_DetailedDepartureStatusCode);
			detailedStatus.Category = FilterCategories.StatusAndFlags;

			return filters;
		}

		ZQuery GetDetailedStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var frHeaderQuery = new ZDBOnlySubQuery(typeof(CusFRNctsHeader), CusFRNctsHeaderSchema.CFN_BH);
			frHeaderQuery.AddToFilter(CusFRNctsHeaderSchema.CFN_DetailedDepartureStatusCode, comparisonOperator, value);
			query.AddSubQuery(frHeaderQuery, JoinCondition.And);
			return query;
		}

		#region Lookups
		public CodeDescriptionPairList DetailedStatusCodeList => Factory.GetCachedValue<NctsDetailedStatusList>();

		protected override CodeDescriptionPairList GetMessagingStatusListNCTS4Core() => Factory.GetCachedValue<FrNctsMessageStatusList>();

		protected override CodeDescriptionPairList NctsStatusListPhase4Core => new NctsTransitStatusList();
		#endregion

	}
}
