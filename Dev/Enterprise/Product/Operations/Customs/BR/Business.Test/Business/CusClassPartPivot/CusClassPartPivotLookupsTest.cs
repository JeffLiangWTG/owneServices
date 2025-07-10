using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class CusClassPartPivotLookupsTest : TestCaseWithFactory
	{
		public void TestTariffs()
		{
			AssertType<TariffViewCollection>(pivot.Lookups.Tariffs);
		}

		public void TestGoodsCatalogList()
		{
			AssertType<CusGoodsCatalogCollection>(pivot.Lookups.GoodsCatalogList);

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var pivotFilterObj = ((CusGoodsCatalogCollection)pivot.Lookups.GoodsCatalogList).FilterBusinessObjectDefaults;
			AssertEquals(2, pivotFilterObj.Count);
			AssertEquals("IMP", pivotFilterObj["Type:Property"].Value);
			Assert("Should NOT be Removable", !pivotFilterObj["Type:Property"].IsRemovable);

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivotFilterObj = ((CusGoodsCatalogCollection)pivot.Lookups.GoodsCatalogList).FilterBusinessObjectDefaults;
			AssertEquals(2, pivotFilterObj.Count);
			AssertEquals("EXP", pivotFilterObj["Type:Property"].Value);
			Assert("Should NOT be Removable", !pivotFilterObj["Type:Property"].IsRemovable);

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			pivotFilterObj = ((CusGoodsCatalogCollection)pivot.Lookups.GoodsCatalogList).FilterBusinessObjectDefaults;
			AssertEquals(1, pivotFilterObj.Count);
			AssertEquals("1234567", pivotFilterObj["Tariff:Property"].Value);
			Assert("Should NOT be Removable", !pivotFilterObj["Tariff:Property"].IsRemovable);
		}

		#region Implementation

		protected override void SetUp()
		{
			part = Factory.New<OrgSupplierPart>();
			pivot = part.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = "1234567";
		}

		OrgSupplierPart part;
		CusClassPartPivot pivot;

		#endregion
	}
}
