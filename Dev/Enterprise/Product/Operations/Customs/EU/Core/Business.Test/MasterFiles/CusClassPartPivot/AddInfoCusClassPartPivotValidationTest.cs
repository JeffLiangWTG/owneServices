using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.MasterFiles.Testing;

public class AddInfoCusClassPartPivotValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCI_GoodsCategory()
	{
		var pivot = Factory.New<CusClassPartPivot>();
		pivot.CI_GoodsCategory = "X";
		AssertHasMessageError(pivot.CI_GoodsCategoryInfo, ListValidation.InvalidCodeMessageError);

		pivot.CI_GoodsCategory = "";
		AssertNoMessageError(pivot.CI_GoodsCategoryInfo, ListValidation.InvalidCodeMessageError);
	}
}
