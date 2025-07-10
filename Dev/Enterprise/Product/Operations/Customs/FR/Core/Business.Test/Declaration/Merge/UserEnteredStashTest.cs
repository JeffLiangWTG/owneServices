using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class UserEnteredStashTest : TestCaseWithFactory
	{
		public void TestStashAndApply_ChargeAmount()
		{
			lineFee.CF_ChargeAmount = 1m;
			stash.Stash(lineFee.UserEnteredStashSource);
			lineFee.CF_ChargeAmountInfo.ClearValue();
			AssertEquals(0m, lineFee.CF_ChargeAmount);

			stash.Apply(lineFee.UserEnteredStashSource);
			AssertEquals(0m, lineFee.CF_ChargeAmount);

			lineFee.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Precalcule;
			stash.Apply(lineFee.UserEnteredStashSource);
			AssertEquals(1m, lineFee.CF_ChargeAmount);
		}

		protected override void SetUp()
		{
			base.SetUp();
			lineFee = Factory.New<CusEntryLineFee>();
			stash = new UserEnteredStash();
		}

		CusEntryLineFee lineFee;
		UserEnteredStash stash;
	}
}
