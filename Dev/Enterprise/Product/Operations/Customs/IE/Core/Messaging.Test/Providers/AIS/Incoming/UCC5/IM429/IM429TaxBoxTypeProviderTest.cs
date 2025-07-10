using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex;
using Enterprise.Customs.IE.Messaging.UCC5;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing.UCC5
{
	[TestedType(typeof(IM429TaxBoxTypeProvider))]
	sealed class IM429TaxBoxTypeProviderTest : TestCase
	{
		public void TestTaxType()
		{
			AssertEquals("A00", provider.TaxType);
		}

		public void TestQuantity()
		{
			AssertEquals(1m, provider.Quantity);
		}

		public void TestUnit()
		{
			AssertEquals("KG", provider.Unit);
		}

		public void TestAmount()
		{
			AssertEquals(123.45m, provider.Amount);
		}

		public void TestTaxRate()
		{
			AssertEquals(6.5m, provider.TaxRate);
		}

		public void TestTaxPayableAmount()
		{
			AssertEquals(12.34m, provider.TaxAmount);
		}

		public void TestTaxPaymentMethod()
		{
			AssertEquals("A", provider.MethodOfPayment);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new IM429TaxBoxTypeProvider(new TaxBoxType
			{
				BoxTaxType = "A00",
				BoxQuantity = 1,
				BoxTaxBaseUnit = "KG",
				BoxAmount = 123.45m,
				BoxTaxRate = 6.5m,
				BoxTaxPayableAmount = 12.34m,
				BoxTaxPaymentMethod = "A"
			});
		}

		IM429TaxBoxTypeProvider provider;
	}
}
