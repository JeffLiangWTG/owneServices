using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class TaxWrapperTest : DataProviderTestCase<TaxWrapper>
	{
		public void TestConstructorOfCusEntryHeaderCharges()
		{
			var charge = Factory.New<CusEntryHeaderCharges>();
			charge.C1_ChargeAmount = 3d;
			var wrapper = TaxWrapper.New(charge);

			AssertEquals("TaxWrapper.Amount should be mapped to CusEntryHeaderCharges.C1_ChargeAmount", 3d, wrapper.Amount);
			AssertEquals("TaxWrapper.MeasurementUnitAndQualifier should be mapped to %", "%", wrapper.MeasurementUnitAndQualifier);
			AssertEquals("TaxWrapper.Quantity should be mapped to 100d", 100d, wrapper.Quantity);
			AssertEquals("TaxWrapper.TaxAmount should be mapped to  CusEntryHeaderCharges.C1_ChargeAmount", 3d, wrapper.TaxAmount);
			AssertEquals("TaxWrapper.TaxRate should be mapped to 100d", 100d, wrapper.TaxRate);
		}

		public void TestAmount()
		{
			AssertEquals("Amount should equal fee.CF_BaseValue.", 1d, Provider.Amount);
		}

		public void TestMeasurementUnitAndQualifier()
		{
			AssertEquals("Amount should equal fee.CF_MethodOfCalculation.", "TEST1", Provider.MeasurementUnitAndQualifier);
		}

		public void TestQuantity()
		{
			AssertEquals("Quantity should equal fee.CF_Rate.", 4d, Provider.Quantity);
		}

		public void TestTaxAmount()
		{
			AssertEquals("TaxAmount should equal fee.CF_ChargeAmount.", 3d, Provider.TaxAmount);
		}

		public void TestTaxRate()
		{
			AssertEquals("TaxRate should equal fee.CF_Rate.", 4d, Provider.TaxRate);
		}

		protected override TaxWrapper GetProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var fee = invoiceLine.CusEntryLine.Fees.AddNew();
			fee.CF_BaseValue = 1d;
			fee.CF_MethodOfCalculation = "TEST1";
			fee.CF_ChargeAmount = 3d;
			fee.CF_Rate = 4d;
			return TaxWrapper.New(fee);
		}
	}
}
