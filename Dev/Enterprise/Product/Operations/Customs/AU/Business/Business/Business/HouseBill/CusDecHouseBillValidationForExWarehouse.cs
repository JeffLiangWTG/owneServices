namespace Enterprise.Customs.AU.Declaration.Business;

public class CusDecHouseBillValidationForExWarehouse : BillValidation
{
	public CusDecHouseBillValidationForExWarehouse(Bill houseBill)
		: base(houseBill)
	{
	}

	protected override void CheckCU_BillNum()
	{
		// do not validate
	}
}
