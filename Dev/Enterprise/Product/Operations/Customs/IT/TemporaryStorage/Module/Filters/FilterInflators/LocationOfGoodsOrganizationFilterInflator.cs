using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

sealed class LocationOfGoodsOrganizationFilterInflator : EU.TemporaryStorage.Module.FilterInflator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter Description Constant")]
	const string FilterDescription = "Location of Goods: Organization";

	public LocationOfGoodsOrganizationFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var locationOfGoodsOrganizationFilter = filterCollection.AddGuidFilter(FilterDescription, ModuleIDs.Organisation, LocationOfGoodsOrgQuery, new OrganisationsFindBoxCollection(Factory));
		locationOfGoodsOrganizationFilter.Category = FilterCategories.Organisations;
		locationOfGoodsOrganizationFilter.MultilingualDescription = ResString.GetMultilingualString("3F5627E4-870B-42C5-BEC6-3E142BB05F8F", FilterDescription);
	}

	ZQuery LocationOfGoodsOrgQuery(SQLComparisonOperator comparisonOperator, object value)
	{
		var query = new ZDBOnlyQuery(typeof(TemporaryStorageHeader));
		var parameters = new ZSqlParameterCollection();

		var conditionMap = new Dictionary<SQLComparisonOperator, string>
		{
			{ SQLComparisonOperator.Equal, "IN (SELECT OH_PK FROM dbo.OrgHeader WHERE OH_PK = @OrgHeaderPK)" },
			{ SQLComparisonOperator.NotEqual, "NOT IN (SELECT OH_PK FROM dbo.OrgHeader WHERE OH_PK = @OrgHeaderPK)" },
			{ SQLComparisonOperator.IsBlank, (NoResString)"IS NULL" },
			{ SQLComparisonOperator.IsNotBlank, (NoResString)"IS NOT NULL" }
		};

		if (!conditionMap.TryGetValue(comparisonOperator, out var condition))
		{
			throw new ArgumentException($"Unsupported comparison operator: {comparisonOperator}");
		}

		if (comparisonOperator == SQLComparisonOperator.Equal || comparisonOperator == SQLComparisonOperator.NotEqual)
		{
			parameters.Add("@OrgHeaderPK", value, OrgHeaderSchema.PK);
		}

		query.AddFilterAndZSQLParameterCollection(
			$@"
			AMA_PK IN
			(
				SELECT CGL_ParentID FROM dbo.CusGoodsLocation WHERE CGL_PK IN
				(
					SELECT E2_ParentID FROM dbo.JobDocAddress WHERE 
						TRY_CONVERT(uniqueidentifier, E2_AdditionalAddressInformation) {condition}
				)
			)",
			parameters
		);
		return query;
	}
}
