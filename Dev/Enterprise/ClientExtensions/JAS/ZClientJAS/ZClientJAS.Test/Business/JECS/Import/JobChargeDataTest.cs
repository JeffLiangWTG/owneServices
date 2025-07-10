using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal class JobChargeDataTest : TestCase
	{
		public void TestConstructor()
		{
			JobChargeData chargeData = new JobChargeData("123", "Charge desc", "USD", 3848m);
			AssertEquals("123", chargeData.ChargeCode);
			AssertEquals("Charge desc", chargeData.ChargeDescription);
			AssertEquals("USD", chargeData.Currency);
			AssertEquals(3848m, chargeData.ChargeAmount);
			AssertEquals(true, chargeData.IsCollect);
		}
	}
}
