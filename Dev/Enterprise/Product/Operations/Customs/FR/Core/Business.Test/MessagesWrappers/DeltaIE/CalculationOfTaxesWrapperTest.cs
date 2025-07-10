using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class CalculationOfTaxesWrapperTest : DataProviderTestCase<CalculationOfTaxesWrapper>
	{
		public void TestDutiesAndTaxe()
		{
			AssertEquals("Count should reflect number of fees of the entry line and charges of the entry header.", 4, Provider.DutiesAndTaxe.Count);

			AssertEquals("CcQualifier should be CountryCodes.France.", CountryCodes.France, Provider.DutiesAndTaxe.ElementAt(0).CcQualifier);
			AssertEquals("NationalTaxType should be fee.NationalFeeTypeCode.", "N001", Provider.DutiesAndTaxe.ElementAt(0).NationalTaxType);
			AssertEquals("PayableTaxAmount should be fee.CF_ChargeAmount.", 3d, Provider.DutiesAndTaxe.ElementAt(0).PayableTaxAmount);
			AssertEquals("TaxType should be fee.CF_ChargeType.", "A01", Provider.DutiesAndTaxe.ElementAt(0).TaxType);

			AssertEquals("CcQualifier should be CountryCodes.France.", CountryCodes.France, Provider.DutiesAndTaxe.ElementAt(1).CcQualifier);
			AssertEquals("NationalTaxType should be fee.NationalFeeTypeCode.", "N002", Provider.DutiesAndTaxe.ElementAt(1).NationalTaxType);
			AssertEquals("PayableTaxAmount should be fee.CF_ChargeAmount.", 4d, Provider.DutiesAndTaxe.ElementAt(1).PayableTaxAmount);
			AssertEquals("TaxType should be fee.CF_ChargeType.", "A02", Provider.DutiesAndTaxe.ElementAt(1).TaxType);

			AssertEquals("CcQualifier should be CountryCodes.France.", CountryCodes.France, Provider.DutiesAndTaxe.ElementAt(2).CcQualifier);
			AssertEquals("NationalTaxType should be charge.C1_ChargeType.", "V905", Provider.DutiesAndTaxe.ElementAt(2).NationalTaxType);
			AssertEquals("PayableTaxAmount should be charge.C1_ChargeAmount.", 3d, Provider.DutiesAndTaxe.ElementAt(2).PayableTaxAmount);
			AssertEquals("TaxType should be the code mapped from EUFeeType via charge.C1_ChargeType", "1N1", Provider.DutiesAndTaxe.ElementAt(2).TaxType);

			AssertEquals("CcQualifier should be CountryCodes.France.", CountryCodes.France, Provider.DutiesAndTaxe.ElementAt(3).CcQualifier);
			AssertEquals("NationalTaxType should be charge.C1_ChargeType.", "P635", Provider.DutiesAndTaxe.ElementAt(3).NationalTaxType);
			AssertEquals("PayableTaxAmount should be charge.C1_ChargeAmount.", 4d, Provider.DutiesAndTaxe.ElementAt(3).PayableTaxAmount);
			AssertEquals("TaxType should be the code mapped from EUFeeType via charge.C1_ChargeType", "1J1", Provider.DutiesAndTaxe.ElementAt(3).TaxType);
		}

		public void TestPreference()
		{
			AssertEquals("Preference should equal line.JI_PrimaryPreference.", "100", Provider.Preference);
		}

		public void TestTotalDutiesAndTaxesAmount()
		{
			AssertEquals("TotalDutiesAndTaxesAmount should equal sum of fees amount.", 14d, Provider.TotalDutiesAndTaxesAmount);
		}

		protected override CalculationOfTaxesWrapper GetProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var line = invoiceHeader.InvoiceLines.AddNew();
			line.JI_PrimaryPreference = "100";

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryLine = (CusEntryLine)line.CusEntryLine;
			var entryHeader = entryLine.Header;

			var fee1 = entryLine.Fees.AddNew();
			fee1.CF_ChargeAmount = 3d;
			fee1.NationalFeeTypeCode = "N001";
			fee1.CF_ChargeType = "A01";

			var fee2 = entryLine.Fees.AddNew();
			fee2.CF_ChargeAmount = 4d;
			fee2.NationalFeeTypeCode = "N002";
			fee2.CF_ChargeType = "A02";

			var charge1 = entryHeader.Charges.AddNew();
			charge1.C1_ChargeAmount = 3d;
			charge1.C1_ChargeType = "V905";

			var charge2 = entryHeader.Charges.AddNew();
			charge2.C1_ChargeAmount = 4d;
			charge2.C1_ChargeType = "P635";

			return CalculationOfTaxesWrapper.New(entryLine);
		}
	}
}
