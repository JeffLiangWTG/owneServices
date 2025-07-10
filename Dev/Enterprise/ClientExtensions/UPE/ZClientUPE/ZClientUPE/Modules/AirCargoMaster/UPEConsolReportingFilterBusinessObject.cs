using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Customs.AU.Module.AirCargo;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Module
{
	public class UPEConsolReportingFilterBusinessObject : AUCustomsAirCargoFilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = base.GetModuleFiltersCore();

			var loadedDateFilter = filters.AddDateFilter("Loaded Date", GetLoadedDateSubQuery);
			var loadedDateSubGroup = new LoadedDateSubGroup();
			loadedDateFilter.SubGroup = loadedDateSubGroup;

			return filters;
		}

		class LoadedDateSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery uPECusMawbQuery = new ZDBOnlyQuery(typeof(UPECusMAWB));
				ZDBOnlySubQuery stmALogQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent);
				stmALogQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.AddedARecordToTheSystem.Code);
				stmALogQuery.AddToFilter(filter);

				uPECusMawbQuery.AddSubQuery(stmALogQuery, JoinCondition.And);
				return uPECusMawbQuery;
			}
		}

		ZQuery GetLoadedDateSubQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZQuery query = new ZQuery();
			AddDateRange(query, comparisonOperator, JoinCondition.And, StmALogSchema.SL_EventTime, fromDate.Date, toDate.Date);
			return query;
		}
	}
}
