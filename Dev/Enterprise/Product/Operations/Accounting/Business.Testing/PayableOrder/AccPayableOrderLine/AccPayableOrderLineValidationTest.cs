namespace Enterprise.Accounting.Business.PayableOrder.Testing
{
	using CargoWise.EntityFramework.Testing;

	internal class AccPayableOrderLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateAllValidateGenericCharge()
		{
			AccPayableOrderHeader order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			AccPayableOrderLine orderLine = Factory.NewWithValidTestData<AccPayableOrderLine>();
			order.OrderLines.Add(orderLine);
			AssertEquals(null, orderLine.GenericChargeBizO);
			orderLine.Validation.ValidateAll();
			Assert(orderLine.GenericChargeInfo.HasError("Please enter a Charge Code."));
		}
	}
}