using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.NCTS.Module;

public class NctsMovementFilterStripBusinessObject : EU.NCTS.Module.NctsMovementFilterStripBusinessObject
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	public static class ESFilterConstants
	{
		public const string CustomsBroker = "Customs Broker";
		public const string ClearanceDate = "Dep. Clearance Date";
		public const string ArrivalLimitDate = "Arrival Limit Date";
		public const string ArrivalSummaryDeclarationNumber = "Arrival Summary Declaration Number";
	}

	protected override ModuleFilterCollection GetModuleFiltersCore()
	{
		var filters = base.GetModuleFiltersCore();
		AddCustomsBrokerFilter(filters);
		AddClearanceDateFilter(filters);
		AddArrivalLimitDateFilter(filters);
		AddArrivalSummaryDeclarationNumberFilter(filters);

		return filters;
	}

	protected override CodeDescriptionPairList NctsStatusListPhase4Core => Factory.GetCachedValue<NctsTransitStatusList>();

	protected override CodeDescriptionPairList NctsDepartureStatusListPhase5Core => Factory.GetCachedValue<ESNCTS5DepartureCustomsStatusList>();

	protected override CodeDescriptionPairList NctsArrivalStatusListPhase5Core => Factory.GetCachedValue<ESNCTS5ArrivalCustomsStatusList>();

	protected override CodeDescriptionPairList NctsDeparturePhaseStatusListPhase5Core
	{
		get
		{
			var list = new CodeDescriptionPairList(base.NctsDeparturePhaseStatusListPhase5Core);
			list.AddPair(ESNctsMovementHeaderTransactionStatusList.Codes.Annexes, ESNctsMovementHeaderTransactionStatusList.Descriptions.Annexes);
			list.AddPair(ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData, ESNctsMovementHeaderTransactionStatusList.Descriptions.OriginalDepartureData);
			return list;
		}
	}

	protected override CodeDescriptionPairList GetNctsArrivalPhaseStatusList() => ApplicationCode == CusInBondApplicationCodeList.Codes.NCTS5 ? Factory.GetCachedValue<ESNcts5ArrivalPhaseList>() : base.GetNctsArrivalPhaseStatusList();

	#region Customs Broker Filter

	void AddCustomsBrokerFilter(ModuleFilterCollection filters)
	{
		var brokerFilter = filters.AddNkFilter(ESFilterConstants.CustomsBroker, GetCustomsBrokerQuery, ModuleIDs.GlbStaff, StaffList);
		brokerFilter.Category = FilterCategories.Organisations;
		brokerFilter.IsPublishedOnWeb = false;
		brokerFilter.MultilingualDescription = ResString.GetMultilingualString("046E14E8-E2BC-4425-92AC-52D2FE32AACC", ESFilterConstants.CustomsBroker);
	}

	ZQuery GetCustomsBrokerQuery(SQLComparisonOperator comparisonOperator, ZString value)
	{
		var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
		var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
		moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_GS_NKCusAgent, comparisonOperator, value);
		query.AddSubQuery(moveHeaderQuery, JoinCondition.And);
		return query;
	}

	#endregion

	#region Clearance Info Filter

	void AddClearanceDateFilter(ModuleFilterCollection filters)
	{
		var issueFilter = filters.AddDateFilter(ESFilterConstants.ClearanceDate, GetClearanceDateQuery);
		issueFilter.Category = FilterCategories.Dates;
		issueFilter.MultilingualDescription = ResString.GetMultilingualString("9D0D39B1-3D75-45D1-A790-8FEEA97C03B9", ESFilterConstants.ClearanceDate);
	}

	ZQuery GetClearanceDateQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate) => GetCusEntryNumberCLRDateFilterQuery(comparisonOperator, fromDate, toDate, CusEntryNumSchema.CE_IssueDate);

	void AddArrivalLimitDateFilter(ModuleFilterCollection filters)
	{
		var issueFilter = filters.AddDateFilter(ESFilterConstants.ArrivalLimitDate, GetArrivalLimitDateQuery);
		issueFilter.Category = FilterCategories.Dates;
		issueFilter.MultilingualDescription = ResString.GetMultilingualString("3EA63D80-8FEC-4336-86EE-0B3B078AAF85", ESFilterConstants.ArrivalLimitDate);
	}

	ZQuery GetArrivalLimitDateQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate) => GetCusEntryNumberCLRDateFilterQuery(comparisonOperator, fromDate, toDate, CusEntryNumSchema.CE_ExpiryDate);

	ZQuery GetCusEntryNumberCLRDateFilterQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate, SchemaDateTimeColumn dateField)
	{
		var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
		var cusEntryNumSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
		cusEntryNumSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, "CLR");
		AddDateTimeRange(cusEntryNumSubQuery, comparisonOperator, JoinCondition.And, dateField, fromDate, toDate);
		query.AddSubQuery(cusEntryNumSubQuery, JoinCondition.And);
		return query;
	}

	#endregion

	#region Arrival Summary Declaration Number

	void AddArrivalSummaryDeclarationNumberFilter(ModuleFilterCollection filters)
	{
		var summaryFilter = filters.AddTextFilter(ESFilterConstants.ArrivalSummaryDeclarationNumber, GetCusEntryNumberSUMTextFilter);
		summaryFilter.Category = FilterCategories.TextSearch;
		summaryFilter.MultilingualDescription = ResString.GetMultilingualString("00F96694-C18F-4E76-9782-B48B4970340A", ESFilterConstants.ArrivalSummaryDeclarationNumber);
	}

	ZQuery GetCusEntryNumberSUMTextFilter(SQLComparisonOperator comparisonOperator, ZString summaryNumber)
	{
		var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
		var cusEntryNumSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
		cusEntryNumSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Spain.SummaryEntryNumber);
		cusEntryNumSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, comparisonOperator, summaryNumber);
		query.AddSubQuery(cusEntryNumSubQuery, JoinCondition.And);
		return query;
	}

	#endregion

	#region Goods Location Filters

	protected override ZQuery GetGoodLocationQuery(SQLComparisonOperator comparisonOperator, ZString value, ZString nctsMovementType)
	{
		var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
		var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
		moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, nctsMovementType);

		if (ApplicationCode == CusInBondApplicationCodeList.Codes.NCTS4)
		{
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_LocationOfGoodsCode, comparisonOperator, value);
		}
		else
		{
			var cusGoodsLocationQuery = new ZDBOnlySubQuery(typeof(CusGoodsLocation), CusGoodsLocationSchema.CGL_ParentID);
			cusGoodsLocationQuery.AddToFilter(CusGoodsLocationSchema.CGL_AdditionalIdentifier, comparisonOperator, value);
			moveHeaderQuery.AddSubQuery(cusGoodsLocationQuery, JoinCondition.And);
		}

		query.AddSubQuery(moveHeaderQuery, JoinCondition.And);

		return query;
	}

	#endregion

	#region Filter Lookups

	GlbStaffCollection StaffList => new GlbStaffCollection(Factory);

	#endregion
}
