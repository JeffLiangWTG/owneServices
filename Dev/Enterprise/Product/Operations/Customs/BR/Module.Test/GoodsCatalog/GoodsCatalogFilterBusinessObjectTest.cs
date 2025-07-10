using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(GoodsCatalogFilterBusinessObject))]
	class GoodsCatalogFilterBusinessObjectTest : Customs.Module.Testing.GoodsCatalogFilterBusinessObjectTest
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GoodsCatalogFilterBusinessObject();
		}

		public void TestLocalPartNumberFilter()
		{
			((CusGoodsCatalog)goodsCatalog1).LocalPartNumbers.AddNew().CGI_Reference = "REF1";
			((CusGoodsCatalog)goodsCatalog1).ForeignOperators.AddNew().CountryCode = "DE";
			((CusGoodsCatalog)goodsCatalog2).LocalPartNumbers.AddNew().CGI_Reference = "REF2";
			((CusGoodsCatalog)goodsCatalog2).ForeignOperators.AddNew().CountryCode = "UY";

			var goodsCatalog3 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			goodsCatalog3.CGC_CatalogCode = "ZZZ";
			goodsCatalog3.CGC_Description = "DESC 3";
			goodsCatalog3.CGC_Tariff = "001144";
			goodsCatalog3.CGC_Type = GoodsCatalogTypeList.Codes.Export;
			goodsCatalog3.LocalPartNumbers.AddNew().CGI_Reference = "AREF3";
			goodsCatalog3.ForeignOperators.AddNew().CountryCode = "AU";
			Factory.Save();

			ModuleTestHelper.AssertModuleTextFilterResult<CusGoodsCatalog>(filterBO, SQLComparisonOperator.Equal, "REF2", new[] { goodsCatalog2 }, "Local Part Number");
			ModuleTestHelper.AssertModuleTextFilterResult<CusGoodsCatalog>(filterBO, SQLComparisonOperator.StartsWith, "RE", new[] { goodsCatalog1, goodsCatalog2 }, "Local Part Number");
			ModuleTestHelper.AssertModuleTextFilterResult<CusGoodsCatalog>(filterBO, SQLComparisonOperator.Contains, "EF", new[] { goodsCatalog1, goodsCatalog2, goodsCatalog3 }, "Local Part Number");
			ModuleTestHelper.AssertModuleTextFilterResult<CusGoodsCatalog>(filterBO, SQLComparisonOperator.NotEqual, "REF2", new[] { goodsCatalog1, goodsCatalog3 }, "Local Part Number");
			ModuleTestHelper.AssertModuleTextFilterResult<CusGoodsCatalog>(filterBO, SQLComparisonOperator.DoesNotStartWith, "REF", new[] { goodsCatalog3 }, "Local Part Number");
			ModuleTestHelper.AssertModuleTextFilterResult<CusGoodsCatalog>(filterBO, SQLComparisonOperator.NotContains, "A", new[] { goodsCatalog1, goodsCatalog2 }, "Local Part Number");
			ModuleTestHelper.AssertModuleTextFilterResult<CusGoodsCatalog>(filterBO, SQLComparisonOperator.IsNotBlank, ZString.Empty, new[] { goodsCatalog1, goodsCatalog2, goodsCatalog3 }, "Local Part Number");
			ModuleTestHelper.AssertModuleTextFilterResult<CusGoodsCatalog>(filterBO, SQLComparisonOperator.Equal, "AU", Array.Empty<BusinessObject>(), "Local Part Number");
			ModuleTestHelper.AssertModuleTextFilterResult<CusGoodsCatalog>(filterBO, SQLComparisonOperator.Equal, "DE", Array.Empty<BusinessObject>(), "Local Part Number");
			ModuleTestHelper.AssertModuleTextFilterResult<CusGoodsCatalog>(filterBO, SQLComparisonOperator.Equal, "UY", Array.Empty<BusinessObject>(), "Local Part Number");
		}
	}
}
