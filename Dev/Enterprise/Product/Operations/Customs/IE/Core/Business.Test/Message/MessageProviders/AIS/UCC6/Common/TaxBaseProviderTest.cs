using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class TaxBaseProviderTest : DataProviderTestCase<TaxBaseProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("CusEntryLineFee missing", () => new TaxBaseProvider(1, null));
		}

		public void TestTaxRate()
		{
			AssertEquals((decimal)123, Provider.TaxRate);
		}

		public void TestMeasurementUnitAndQualifier()
		{
			AssertEquals("%", Provider.MeasurementUnitAndQualifier);
		}

		public void TestQuantity()
		{
			AssertEquals(decimal.Zero, Provider.Quantity);

			cusEntryLineFee.CF_MethodOfCalculation = "ASV";
			AssertEquals((decimal)23, GetProvider().Quantity);
		}

		public void TestAmount()
		{
			AssertEquals((decimal)23, Provider.Amount);

			cusEntryLineFee.CF_MethodOfCalculation = "ASV";
			AssertEquals(decimal.Zero, Provider.Amount);
		}

		public void TestTaxAmount()
		{
			AssertEquals((decimal)222, Provider.TaxAmount);
		}

		public void TestSequenceNumber()
		{
			AssertEquals("1", Provider.SequenceNumber);
		}

		protected override TaxBaseProvider GetProvider()
		{
			return new TaxBaseProvider(1, cusEntryLineFee);
		}

		protected override void SetUp()
		{
			cusEntryLineFee = Factory.NewWithValidTestData<CusEntryLineFee>();
			cusEntryLineFee.CF_BaseValue = 23;
			cusEntryLineFee.CF_Rate = 123;
			cusEntryLineFee.CF_MethodOfCalculation = "%";
			cusEntryLineFee.CF_ChargeAmount = 222;
		}
		CusEntryLineFee cusEntryLineFee;
	}
}
