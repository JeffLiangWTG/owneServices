using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class CusEntryHeaderChargeUserEnteredStashSourceTest : TestCaseWithFactory
	{
		public void TestStashedPropertiesAndConditions()
		{
			var charge = Factory.New<CusEntryHeaderCharges>();
			var stashFee = charge.UserEnteredStashSource;
			AssertContainsExactElementsInAnyOrder(new ZPropertyInfo[] { charge.C1_MethodOfPaymentInfo }, stashFee.StashedPropertiesAndConditions.Keys.ToArray());
		}

		public void TestShouldNotStash_WhenAllEmpty()
		{
			var charge = Factory.New<CusEntryHeaderCharges>();
			var stashFee = charge.UserEnteredStashSource;
			AssertEquals(false, stashFee.ShouldStash());
		}

		public void TestShouldStash_WhenMethodOfPaymentHasValue()
		{
			var charge = Factory.New<CusEntryHeaderCharges>();
			charge.C1_MethodOfPayment = "MP";
			var stashFee = charge.UserEnteredStashSource;
			AssertEquals(true, stashFee.ShouldStash());
		}

		public void TestGetStashKey()
		{
			var charge = Factory.New<CusEntryHeaderCharges>();
			charge.C1_ChargeType = "A001";
			var stashFee = charge.UserEnteredStashSource;
			AssertEquals("A001", stashFee.GetStashKey());
		}
	}
}
