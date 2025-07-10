using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class UserEnteredStashTest : TestCaseWithFactory
	{
		public void TestStashAndApply_MethodOfPayment()
		{
			lineFee.CF_MethodOfPayment = "MP";
			stash.Stash(lineFee.UserEnteredStashSource);
			lineFee.CF_MethodOfPaymentInfo.ClearValue();
			AssertEquals("", lineFee.CF_MethodOfPayment);

			stash.Apply(lineFee.UserEnteredStashSource);
			AssertEquals("MP", lineFee.CF_MethodOfPayment);
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
