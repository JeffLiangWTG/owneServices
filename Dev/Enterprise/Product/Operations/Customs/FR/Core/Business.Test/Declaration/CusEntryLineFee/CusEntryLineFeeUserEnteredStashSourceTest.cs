using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class CusEntryLineFeeUserEnteredStashSourceTest : TestCaseWithFactory
	{
		public void TestStashedPropertiesAndConditions()
		{
			var entryLineFee = Factory.New<CusEntryLineFee>();
			var stashFee = entryLineFee.UserEnteredStashSource;
			AssertContainsExactElementsInAnyOrder(new ZPropertyInfo[] { entryLineFee.CF_MethodOfPaymentInfo, entryLineFee.NationalFeeTypeCodeInfo, entryLineFee.CF_ChargeAmountInfo }, stashFee.StashedPropertiesAndConditions.Keys.ToArray());
			Assert(stashFee.StashedPropertiesAndConditions[entryLineFee.CF_MethodOfPaymentInfo]);
			Assert(stashFee.StashedPropertiesAndConditions[entryLineFee.NationalFeeTypeCodeInfo]);
			Assert(!stashFee.StashedPropertiesAndConditions[entryLineFee.CF_ChargeAmountInfo]);

			entryLineFee.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Precalcule;
			Assert(stashFee.StashedPropertiesAndConditions[entryLineFee.CF_ChargeAmountInfo]);
		}
	}
}
