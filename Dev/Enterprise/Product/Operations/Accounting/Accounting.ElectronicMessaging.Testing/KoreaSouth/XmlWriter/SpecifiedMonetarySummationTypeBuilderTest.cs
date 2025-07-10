using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Testing
{
	class SpecifiedMonetarySummationTypeBuilderTest : TestCaseWithFactory
	{
		public void TestBuild()
		{
			var transactionInfo = new TransactionInfo();

			transactionInfo.LocalExVATAmount = 170.00m;
			transactionInfo.LocalVATAmount = 30.00m;
			transactionInfo.LocalTotal = 200.00m;

			var builder = new SpecifiedMonetarySummationTypeBuilder("TestNameSpace", transactionInfo);
			var result = builder.Build("SpecifiedMonetarySummation");
			var expected = @"
<SpecifiedMonetarySummation xmlns=""TestNameSpace"">
  <ChargeTotalAmount>170</ChargeTotalAmount>
  <TaxTotalAmount>30</TaxTotalAmount>
  <GrandTotalAmount>200</GrandTotalAmount>
</SpecifiedMonetarySummation>
			".Trim();

			AssertEquals(expected, result.ToString());
		}

		public void TestBuild_NegativeAmount()
		{
			var transactionInfo = new TransactionInfo();

			transactionInfo.LocalExVATAmount = -170.00m;
			transactionInfo.LocalVATAmount = -30.00m;
			transactionInfo.LocalTotal = -200.00m;

			var builder = new SpecifiedMonetarySummationTypeBuilder("TestNameSpace", transactionInfo);
			var result = builder.Build("SpecifiedMonetarySummation");
			var expected = @"
<SpecifiedMonetarySummation xmlns=""TestNameSpace"">
  <ChargeTotalAmount>-170</ChargeTotalAmount>
  <TaxTotalAmount>-30</TaxTotalAmount>
  <GrandTotalAmount>-200</GrandTotalAmount>
</SpecifiedMonetarySummation>
			".Trim();

			AssertEquals(expected, result.ToString());
		}
	}
}
