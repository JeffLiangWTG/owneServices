using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NCTSPrettierGuaranteeDataTest : TestCase
	{
		public void TestType() => AssertEquals("TestType", prettierGuaranteeData.Type);
		public void TestGRN() => AssertEquals("TestGRN", prettierGuaranteeData.GRN);
		public void TestOtherNumber() => AssertEquals("TestNumber", prettierGuaranteeData.OtherNumber);
		public void TestAmount() => AssertEquals("100", prettierGuaranteeData.Amount);
		public void TestCurrency() => AssertEquals("USD", prettierGuaranteeData.Currency);

		protected override void SetUp()
		{
			base.SetUp();

			prettierGuaranteeData = new NCTSPrettierGuaranteeData(
				type: "TestType",
				grn: "TestGRN",
				otherNumber: "TestNumber",
				amount: "100",
				currency: "USD");
		}

		NCTSPrettierGuaranteeData prettierGuaranteeData;
	}
}
