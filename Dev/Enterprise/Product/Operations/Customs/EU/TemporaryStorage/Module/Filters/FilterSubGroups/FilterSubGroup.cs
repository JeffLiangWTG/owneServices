using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public abstract class FilterSubGroup : ModuleFilterSubGroup
{
	protected ZDBOnlyQuery GetAsycudaMainQuery(SchemaColumn schemaColumn, ZDBOnlySubQuery subQuery)
	{
		var asycudaBillQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
		asycudaBillQuery.AddSubQuery(schemaColumn, subQuery, JoinCondition.And);

		var query = new ZDBOnlyQuery(typeof(TemporaryStorageHeader));
		query.AddSubQuery(AsycudaManifestHeaderSchema.PK, asycudaBillQuery, JoinCondition.And);

		return query;
	}

	protected ZDBOnlySubQuery GetCusSupportingInfoBillQuery(ZQuery filter, string filterType)
	{
		var billCusSupportingInfoQuery = new ZDBOnlySubQuery(typeof(CusSupportingInfo), CusSupportingInfoSchema.CSI_ParentID);
		billCusSupportingInfoQuery.AddToFilter(filter);
		billCusSupportingInfoQuery.AddToFilter(CusSupportingInfoSchema.CSI_Type, SQLComparisonOperator.Equal, filterType);

		var billQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
		billQuery.AddSubQuery(AsycudaBillSchema.PK, billCusSupportingInfoQuery, JoinCondition.And);

		return billQuery;
	}

	protected ZDBOnlySubQuery GetCusSupportingInfoPackedItemQuery(ZQuery filter, string filterType)
	{
		var packItemCusSupportingInfoQuery = new ZDBOnlySubQuery(typeof(CusSupportingInfo), CusSupportingInfoSchema.CSI_ParentID);
		packItemCusSupportingInfoQuery.AddToFilter(filter);
		packItemCusSupportingInfoQuery.AddToFilter(CusSupportingInfoSchema.CSI_Type, SQLComparisonOperator.Equal, filterType);

		var packedItemQuery = new ZDBOnlySubQuery(typeof(AsycudaPackedItem), AsycudaPackedItemSchema.API_ABL_Bill);
		packedItemQuery.AddSubQuery(AsycudaPackedItemSchema.PK, packItemCusSupportingInfoQuery, JoinCondition.And);

		return packedItemQuery;
	}
}
