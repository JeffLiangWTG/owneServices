using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Module.Testing;

[TestedType(typeof(OrgSupplierPartFilterStripBusinessObject))]
class OrgSupplierPartFilterStripBusinessObjectTest : Customs.Module.Testing.OrgSupplierPartFilterStripBusinessObjectTest
{
	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new OrgSupplierPartFilterStripBusinessObject();

	protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheck()
	{
		base.GetFiltersExcludedFromSubgroupCheck();
		var exclusions = new List<Tuple<string, string>>();
		exclusions.Add(TableFilter(WhsABCCategorySchema.Constants.TableName, "ABC Category / Warehouse"));
		exclusions.Add(TableFilter(CusClassificationSchema.Constants.TableName, "Classification"));
		exclusions.Add(TableFilter(CusClassPartPivotSchema.Constants.TableName, "Classification"));
		exclusions.Add(TableFilter(OrgPartRelationSchema.Constants.TableName, "Importer/Supplier"));
		exclusions.Add(TableFilter(OrgSupplierPartSchema.Constants.TableName, "Importer/Supplier"));

		exclusions.Add(TableFilter(CusClassificationSchema.Constants.TableName, "Tariff Code"));
		exclusions.Add(TableFilter(CusClassPartPivotSchema.Constants.TableName, "Customs Type"));
		exclusions.Add(TableFilter(CusClassPartPivotSchema.Constants.TableName, "Tariff Code"));

		return exclusions;
	}

	protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
	{
		var exclusions = new List<Tuple<string, string>>();
		exclusions.Add(TableFilter(CusClassificationSchema.Constants.TableName, "Tariff Code"));
		exclusions.Add(TableFilter(CusClassPartPivotSchema.Constants.TableName, "Customs Type"));
		exclusions.Add(TableFilter(CusClassPartPivotSchema.Constants.TableName, "Tariff Code"));
		exclusions.Add(TableFilter(OrgPartRelationSchema.Constants.TableName, "Importer/Supplier"));

		return exclusions;
	}
}
