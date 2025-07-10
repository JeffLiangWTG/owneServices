using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

class CusPermitHeaderValidationTest : CargoWise.EntityFramework.Testing.BusinessObjectValidationTestCase
{
	public void TestCPH_QtyValIndicator()
	{
		var permit = Factory.New<CusPermitHeader>();
		permit.CPH_QtyValIndicator = ZString.Empty;
		AssertNoNotifications("CPH_QtyValIndicator can be empty.", permit.CPH_QtyValIndicatorInfo);

		permit.CPH_QtyValIndicator = "%";
		AssertHasErrorContaining("CPH_QtyValIndicator cannot be invalid code.", permit.CPH_QtyValIndicatorInfo, ListValidation.InvalidCodeError);
	}

	public void TestCheckCPH_Type()
	{
		RefCusCodeTestHelper.CreatePermitAuthorityCodeList(Factory);

		var permit = Factory.New<CusPermitHeader>();

		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertErrorIfNotEntered(permit.CPH_TypeInfo);
			ValidationTestHelper.AssertErrorIfInvalidCode(permit.CPH_TypeInfo, RefCusCodeTestHelper.InvalidPermitAuthorityCode, RefCusCodeTestHelper.ValidPermitAuthorityCode);
		});
	}

	public void TestCheckCPH_UnitOfMeasure()
	{
		RefCusCodeTestHelper.CreateCustomsUnitOfQuantityCodeList(Factory);

		var permit = Factory.New<CusPermitHeader>();
		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(permit.CPH_UnitOfMeasureInfo, RefCusCodeTestHelper.InvalidCustomsUnitOfQuantityCode, RefCusCodeTestHelper.ValidCustomsUnitOfQuantityCode);
		});
	}

	public void TestCheckCPH_QtyValIndicator()
	{
		var permit1 = Factory.New<CusPermitHeader>();
		permit1.CPH_QtyValIndicator = "ZZZ";
		AssertHasErrorContaining("Error if invalid code", permit1.CPH_QtyValIndicatorInfo, ListValidation.InvalidCodeError);
		permit1.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.BTH;
		AssertNoErrorContaining("Error if CPH_Type is empty", permit1.CPH_QtyValIndicatorInfo, ListValidation.InvalidCodeError);

		permit1.CPH_Type = "REB";
		permit1.CPH_QtyValIndicator = ZString.Empty;
		AssertNoErrors("No error if empty and CPH_Type=REB", permit1.CPH_QtyValIndicatorInfo);
	}
}
