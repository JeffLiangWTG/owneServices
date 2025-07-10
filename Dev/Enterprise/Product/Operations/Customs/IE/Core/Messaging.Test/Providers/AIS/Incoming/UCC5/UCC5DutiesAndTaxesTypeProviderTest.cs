using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Messaging.Testing.Providers.AIS.Incoming.UCC5
{
	sealed class UCC5DutiesAndTaxesTypeProviderTest : TestCaseWithFactory
	{
		public void TestTaxType()
		{
			AssertEquals("B00", provider.TaxType);
		}

		public void TestUnit()
		{
			AssertEquals("U", provider.Unit);
		}

		public void TestQuantity()
		{
			AssertEquals(0m, provider.Quantity);
		}

		public void TestAmount()
		{
			AssertEquals(9000m, provider.Amount);
		}

		public void TestTaxRate()
		{
			AssertEquals(4.7m, provider.TaxRate);
		}

		public void TestTaxAmount()
		{
			AssertEquals(423m, provider.TaxAmount);
		}

		public void TestMethodOfPayment()
		{
			AssertEquals("A", provider.MethodOfPayment);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var tax = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex.TaxBoxType
			{
				BoxTaxType = "B00",
				BoxTaxBaseUnit = "U",
				BoxAmount = 9000m,
				BoxTaxRate = 4.7m,
				BoxTaxPayableAmount = 423m,
				BoxTaxPaymentMethod = "A"
			};
			provider = new UCC5DutiesAndTaxesTypeProvider(tax);
		}

		UCC5DutiesAndTaxesTypeProvider provider;
	}
}
