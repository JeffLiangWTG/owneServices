using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

class InlandTransportValidationTest : CusCodeDataValidationTest
{
	public void TestCheckCY_Order()
	{
		const string message = "value cannot be negative.";
		var inlandTransport = Factory.New<InlandTransport>();

		CombineAssertions(() =>
		{
			inlandTransport.CY_Order = -1;
			AssertHasError("Negative", inlandTransport.CY_OrderInfo, message);

			inlandTransport.CY_Order = 0;
			AssertNoError("Not negative", inlandTransport.CY_OrderInfo, message);
		});
	}
}
