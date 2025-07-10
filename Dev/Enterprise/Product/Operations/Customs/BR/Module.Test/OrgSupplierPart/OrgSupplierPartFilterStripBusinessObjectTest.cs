using System;
using System.Collections.Generic;
using Enterprise.Customs.BR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using OrgSupplierPart = Enterprise.Customs.BR.Business.OrgSupplierPart;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartFilterStripBusinessObject))]
	public class OrgSupplierPartFilterStripBusinessObjectTest : Customs.Module.Testing.OrgSupplierPartFilterStripBusinessObjectTest
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new OrgSupplierPartFilterStripBusinessObject();
		}

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

		public void TestCatalogLookupCode()
		{
			var product1 = OrgSupplierPart.New(Factory);
			product1.OP_PartNum = "PartNum1";

			var product2 = OrgSupplierPart.New(Factory);
			product2.OP_PartNum = "PartNum2";

			var product3 = OrgSupplierPart.New(Factory);
			product3.OP_PartNum = "PartNum3";

			var product4 = OrgSupplierPart.New(Factory);
			product4.OP_PartNum = "PartNum4";

			var pivot1 = product1.PivotsForBinding.AddNew();
			pivot1.CI_RN_NKCountry = Core.Constants.CountryCodes.Brazil;

			var pivot2 = product2.PivotsForBinding.AddNew();
			pivot2.CI_RN_NKCountry = Core.Constants.CountryCodes.Brazil;

			var pivot3 = product3.PivotsForBinding.AddNew();
			pivot3.CI_RN_NKCountry = Core.Constants.CountryCodes.Brazil;

			var pivot4 = product4.PivotsForBinding.AddNew();
			pivot4.CI_RN_NKCountry = Core.Constants.CountryCodes.Brazil;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "XXX";
			consignee.OH_FullName = "TEST COMPANY";
			consignee.PrimaryRegistrationNumber.Number = "58.500.398/-04";

			var catalog = Factory.New<CusGoodsCatalog>();
			catalog.CGC_AuthorityIdentifier = "2";
			catalog.CGC_Description = "Description XXX";
			catalog.CGC_Type = GoodsCatalogTypeList.Codes.Export;
			catalog.CGC_Tariff = "40030000";
			catalog.CGC_CatalogCode = "Code 1";
			catalog.CGC_OH_Owner = consignee.PK;

			pivot1.CI_CGC_Catalog = catalog.PK;

			var catalog2 = Factory.New<CusGoodsCatalog>();
			catalog2.CGC_AuthorityIdentifier = "3";
			catalog2.CGC_Description = "Description YYY";
			catalog2.CGC_Type = GoodsCatalogTypeList.Codes.Export;
			catalog2.CGC_Tariff = "20080000";
			catalog2.CGC_CatalogCode = "Code 2";
			catalog2.CGC_OH_Owner = consignee.PK;

			pivot2.CI_CGC_Catalog = catalog2.PK;

			var catalog3 = Factory.New<CusGoodsCatalog>();
			catalog3.CGC_AuthorityIdentifier = "4";
			catalog3.CGC_Description = "Description ZZZ";
			catalog3.CGC_Type = GoodsCatalogTypeList.Codes.Import;
			catalog3.CGC_Tariff = "10034000";
			catalog3.CGC_CatalogCode = "IM Code 3";
			catalog3.CGC_OH_Owner = consignee.PK;

			pivot3.CI_CGC_Catalog = catalog3.PK;

			var catalog4 = Factory.New<CusGoodsCatalog>();
			catalog4.CGC_AuthorityIdentifier = "5";
			catalog4.CGC_Description = "Description ZZZ";
			catalog4.CGC_Type = GoodsCatalogTypeList.Codes.Import;
			catalog4.CGC_Tariff = "30034000";
			catalog4.CGC_CatalogCode = "IM Code 4";
			catalog4.CGC_OH_Owner = consignee.PK;

			pivot4.CI_CGC_Catalog = catalog4.PK;

			var filterBO = (OrgSupplierPartFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterBO[OrgSupplierPartFilterConstants.CatalogLookupCode];
			var coll = new Customs.Business.OrgSupplierPartCollection(Factory);
			filter.IsActive = true;

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleGuidFilterResult<OrgSupplierPart>(filterBO, catalog.PK, new[] { product1 }, OrgSupplierPartFilterConstants.CatalogLookupCode);
				ModuleTestHelper.AssertModuleGuidFilterResult<OrgSupplierPart>(filterBO, catalog2.PK, new[] { product2 }, OrgSupplierPartFilterConstants.CatalogLookupCode);
				ModuleTestHelper.AssertModuleGuidFilterResult<OrgSupplierPart>(filterBO, catalog3.PK, new[] { product3 }, OrgSupplierPartFilterConstants.CatalogLookupCode);
				ModuleTestHelper.AssertModuleGuidFilterResult<OrgSupplierPart>(filterBO, catalog4.PK, new[] { product4 }, OrgSupplierPartFilterConstants.CatalogLookupCode);

				pivot2.CI_CGC_Catalog = catalog2.PK;
				pivot3.CI_CGC_Catalog = catalog2.PK;
				pivot4.CI_CGC_Catalog = catalog.PK;
				Factory.Save();
				ModuleTestHelper.AssertModuleGuidFilterResult<OrgSupplierPart>(filterBO, catalog.PK, new[] { product1, product4 }, OrgSupplierPartFilterConstants.CatalogLookupCode);
				ModuleTestHelper.AssertModuleGuidFilterResult<OrgSupplierPart>(filterBO, catalog2.PK, new[] { product2, product3 }, OrgSupplierPartFilterConstants.CatalogLookupCode);
				AssertEquals("Category should be", FilterCategories.Other, filter.Category);
			});
		}
	}
}
