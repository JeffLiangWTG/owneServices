using CargoWise.EntityFramework;

namespace Enterprise.Customs.AE.Business.Testing;

public class CusDecHouseBillValidationTest : Customs.Business.Testing.CusDecHouseBillValidationTest
{
	public override void TestExWarehouseDecDoesNotHaveMasterBillValidated()
	{
		Assert("No ExWarehouse in AE", true);
	}

	public void TestMasterBillValidation()
	{
		JobDeclaration declaration = JobDeclaration.New(Factory);
		declaration.Validation.ValidateJE_MasterBill();
		Assert(declaration.JE_MasterBillInfo.HasMessageErrors());
		declaration.JE_MasterBill = "TEST123";
		Assert(!declaration.JE_MasterBillInfo.HasMessageErrors());
	}

	public void TestHouseBillMessageValidation()
	{
		JobDeclaration declaration = JobDeclaration.New(Factory);
		declaration.Validation.ValidateJE_HouseBill();
		Assert(declaration.JE_HouseBillInfo.HasMessageErrors());
		declaration.JE_HouseBill = "TEST123";
		Assert(!declaration.JE_HouseBillInfo.HasMessageErrors());
	}
}
