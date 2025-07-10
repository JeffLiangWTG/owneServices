using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	[TestedType(typeof(DutiesAndTaxesTypeProvider))]
	sealed class DutiesAndTaxesTypeProviderTest : TestCase
	{
		public void TestTaxType()
		{
			AssertEquals("A00", provider.TaxType);
		}

		public void TestMethodOfPayment()
		{
			AssertEquals("A", provider.MethodOfPayment);
		}

		public void TestUnit()
		{
			AssertEquals("KG", provider.Unit);
		}

		public void TestQuantity()
		{
			AssertEquals(1m, provider.Quantity);
		}

		public void TestAmount()
		{
			AssertEquals(2m, provider.Amount);
		}

		public void TestTaxRate()
		{
			AssertEquals(3m, provider.TaxRate);
		}

		public void TestTaxAmount()
		{
			AssertEquals(5m, provider.TaxAmount);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new DutiesAndTaxesTypeProvider(new MDutiesAndTaxesType03
			{
				MethodOfPayment = "A",
				TaxType = "A00",
				TaxBase = new Collection<MTaxBaseType01>
				{
					new MTaxBaseType01
					{
						SequenceNumber = "1",
						MeasurementUnitAndQualifier = "KG",
						Quantity = 1m,
						Amount = 2m,
						TaxRate = 3m,
						TaxAmount = 5m,
					}
				}
			});
		}

		DutiesAndTaxesTypeProvider provider;
	}
}
