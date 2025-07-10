using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Testing
{
	class SpecifiedPaymentMeansTypeBuilderTest : TestCaseWithFactory
	{
		public void TestBuildRecipt()
		{
			var transactionInfo = new TransactionInfo();
			transactionInfo.LocalTotal = 200.00m;
			transactionInfo.FullyPaidDate = ZDateTime.Now;

			var builder = new SpecifiedPaymentMeansTypeBuilder("TestNameSpace", transactionInfo);
			var result = builder.Build("SpecifiedPaymentMeans");
			var expected = @"
<SpecifiedPaymentMeans xmlns=""TestNameSpace"">
  <TypeCode>10</TypeCode>
  <PaidAmount>200</PaidAmount>
</SpecifiedPaymentMeans>
			".Trim();

			AssertEquals(expected, result.ToString());
		}

		public void TestBuildBilling()
		{
			var transactionInfo = new TransactionInfo();
			transactionInfo.LocalTotal = 200.00m;

			var builder = new SpecifiedPaymentMeansTypeBuilder("TestNameSpace", transactionInfo);
			var result = builder.Build("SpecifiedPaymentMeans");
			var expected = @"
<SpecifiedPaymentMeans xmlns=""TestNameSpace"">
  <TypeCode>40</TypeCode>
  <PaidAmount>200</PaidAmount>
</SpecifiedPaymentMeans>
			".Trim();

			AssertEquals(expected, result.ToString());
		}

		public void TestBuildCreditNote()
		{
			var transactionInfo = new TransactionInfo();
			transactionInfo.TransactionType = TransactionType.CRD;
			transactionInfo.LocalTotal = -200.00m;

			var builder = new SpecifiedPaymentMeansTypeBuilder("TestNameSpace", transactionInfo);
			var result = builder.Build("SpecifiedPaymentMeans");

			AssertEquals(null, result);
		}
	}
}
