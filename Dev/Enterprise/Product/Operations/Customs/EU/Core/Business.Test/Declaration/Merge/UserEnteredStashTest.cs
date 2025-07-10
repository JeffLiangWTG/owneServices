using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class UserEnteredStashTest : TestCaseWithFactory
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

		public void TestStashAndApply_NationFeeTypeCode()
		{
			lineFee.NationalFeeTypeCode = "NT";
			stash.Stash(lineFee.UserEnteredStashSource);
			lineFee.NationalFeeTypeCodeInfo.ClearValue();
			AssertEquals("", lineFee.NationalFeeTypeCode);

			stash.Apply(lineFee.UserEnteredStashSource);
			AssertEquals("NT", lineFee.NationalFeeTypeCode);
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
