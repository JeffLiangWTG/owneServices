using System;
using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class DutiesandTaxesProviderTest : DataProviderTestCase<DutiesandTaxesProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("CusEntryLineFee missing", () => new DutiesandTaxesProvider(1, null));
		}

		public void TestTaxType()
		{
			AssertEquals("Test", Provider.TaxType);
		}

		public void TestCcQualifier()
		{
			AssertNull(Provider.CcQualifier);
		}

		public void TestPayableTaxAmount()
		{
			AssertEquals((decimal)123, Provider.PayableTaxAmount);
		}

		public void TestMethodOfPayment()
		{
			AssertEquals("Pay", Provider.MethodOfPayment);
		}

		public void TestSequenceNumber()
		{
			AssertEquals("1", Provider.SequenceNumber);
		}

		public void TestTaxBase()
		{
			var taxBase = Provider.TaxBase.Single();
			AssertType<TaxBaseProvider>("Type", taxBase);
			AssertEquals("Sequence", "1", taxBase.SequenceNumber);
		}

		protected override DutiesandTaxesProvider GetProvider()
		{
			return new DutiesandTaxesProvider(1, cusEntryLineFee);
		}

		protected override void SetUp()
		{
			cusEntryLineFee = Factory.NewWithValidTestData<CusEntryLineFee>();
			cusEntryLineFee.CF_ChargeType = "Test";
			cusEntryLineFee.CF_ChargeAmount = 123;
			cusEntryLineFee.CF_MethodOfPayment = "Pay";
		}
		CusEntryLineFee cusEntryLineFee;
	}
}
