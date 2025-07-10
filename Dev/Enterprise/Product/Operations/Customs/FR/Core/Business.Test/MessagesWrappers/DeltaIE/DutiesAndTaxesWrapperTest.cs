using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class DutiesAndTaxesWrapperTest : DataProviderTestCase<DutiesAndTaxesWrapper>
	{
		public void TestConstructorOfCusEntryHeaderCharges()
		{
			var charge = Factory.New<CusEntryHeaderCharges>();
			charge.C1_MethodOfPayment = "X";
			charge.C1_ChargeType = "Y";
			charge.C1_ChargeAmount = 3d;
			var wrapper = DutiesAndTaxesWrapper.New(charge, string.Empty);

			AssertEquals("DutiesAndTaxesWrapper.MethodOfPayment should be mapped to CusEntryHeaderCharges.C1_MethodOfPayment", "X", wrapper.MethodOfPayment);
			AssertEquals("DutiesAndTaxesWrapper.NationalTaxType should be mapped to CusEntryHeaderCharges.C1_ChargeType", "Y", wrapper.NationalTaxType);
			AssertEquals("DutiesAndTaxesWrapper.PayableTaxAmount should be mapped to CusEntryHeaderCharges.C1_ChargeAmount", 3d, wrapper.PayableTaxAmount);
			AssertType<List<ITax>>("DutiesAndTaxesWrapper.TaxBase should be mapped to a List of ITax", wrapper.TaxBase);
			AssertEquals("DutiesAndTaxesWrapper.TaxBase should be mapped to a List of ITax, and the count is 1", 1, wrapper.TaxBase.Count);
			AssertEquals("DutiesAndTaxesWrapper.TaxType should be mapped to CusEntryHeaderCharges.C1_ChargeType", "Y", wrapper.TaxType);
		}

		public void TestCcQualifier()
		{
			AssertEquals("CcQualifier should equal CountryCodes.France as customs office is empty.", CountryCodes.France, Provider.CcQualifier);

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var fee = invoiceLine.CusEntryLine.Fees.AddNew();
			fee.CF_MethodOfPayment = "X";
			fee.NationalFeeTypeCode = "Y";
			fee.CF_ChargeType = "Z";

			fee.CF_BaseValue = 1d;
			fee.CF_MethodOfCalculation = "TEST1";
			fee.CF_ChargeAmount = 3d;
			fee.CF_Rate = 4d;
			var wrapper = DutiesAndTaxesWrapper.New(fee, CountryCodes.Dominica);

			AssertEquals("CcQualifier should equal CountryCodes.France as customs office doesn't start with FR.", CountryCodes.France, wrapper.CcQualifier);

			wrapper = DutiesAndTaxesWrapper.New(fee, CountryCodes.France);
			AssertEquals("CcQualifier shouldbe empty as customs office start with FR.", string.Empty, wrapper.CcQualifier);
		}

		public void TestMethodOfPayment()
		{
			AssertEquals("MethodOfPayment's item should equal collection of fee.CF_MethodOfPayment.", "X", Provider.MethodOfPayment);
		}

		public void TestNationalTaxType()
		{
			AssertEquals("NationalTaxType should equal collection of fee.NationalFeeTypeCode.", "Y", Provider.NationalTaxType);
		}

		public void TestPayableTaxAmount()
		{
			AssertEquals("PayableTaxAmount should equal collection of fee.CF_ChargeAmount.", 3d, Provider.PayableTaxAmount);
		}

		public void TestTaxBase()
		{
			AssertEquals("TaxBase has only 1 element.", 1, Provider.TaxBase.Count);
			AssertEquals("Amount should equal fee.CF_BaseValue.", 1d, Provider.TaxBase.First().Amount);
			AssertEquals("Amount should equal fee.CF_MethodOfCalculation", "TEST1", Provider.TaxBase.First().MeasurementUnitAndQualifier);
			AssertEquals("Quantity should equal fee.CF_Rate.", 4d, Provider.TaxBase.First().Quantity);
			AssertEquals("TaxAmount should equal fee.CF_ChargeAmount.", 3d, Provider.TaxBase.First().TaxAmount);
			AssertEquals("TaxRate should equal fee.CF_Rate.", 4d, Provider.TaxBase.First().TaxRate);
		}

		public void TestTaxType()
		{
			AssertEquals("TaxType should equal collection of fee.CF_ChargeType.", "Z", Provider.TaxType);
		}

		protected override DutiesAndTaxesWrapper GetProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var fee = invoiceLine.CusEntryLine.Fees.AddNew();
			fee.CF_MethodOfPayment = "X";
			fee.NationalFeeTypeCode = "Y";
			fee.CF_ChargeType = "Z";

			fee.CF_BaseValue = 1d;
			fee.CF_MethodOfCalculation = "TEST1";
			fee.CF_ChargeAmount = 3d;
			fee.CF_Rate = 4d;
			return DutiesAndTaxesWrapper.New(fee, string.Empty);
		}
	}
}
