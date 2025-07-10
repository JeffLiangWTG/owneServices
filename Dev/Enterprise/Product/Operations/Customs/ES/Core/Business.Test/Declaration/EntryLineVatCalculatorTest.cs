using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	class EntryLineVatCalculatorTest : TestCaseWithFactory
	{
		public void TestCalculateVatFee()
		{
			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = ZDateTime.Today.AddYears(-1);
			var endDate = ZDateTime.Today.AddYears(1);
			var ordVat = refDataHelper.CreateTaxOrFee("ORD", 0.22m, currentCountryCode, startDate, endDate);
			var dtyRateType = refDataHelper.CreateCusRateType(dataGroupingCode: "EUN", rateType: "DTY", description: "Customs duties on industrial products");
			var addRateType = refDataHelper.CreateCusRateType(dataGroupingCode: "EUN", rateType: "ADD", description: "Definitive antidumping duties");
			var cvdRateType = refDataHelper.CreateCusRateType(dataGroupingCode: "EUN", rateType: "CVD", description: "Definitive countervailing duties");
			refDataHelper.CreateCusRateCode(Factory, zy1RateCode: "A00", dtyRateType.PK);
			refDataHelper.CreateCusRateCode(Factory, zy1RateCode: "A30", addRateType.PK);
			refDataHelper.CreateCusRateCode(Factory, zy1RateCode: "A40", cvdRateType.PK);
			var eunDataGrouping = refDataHelper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
			refDataHelper.CreateNewOrGetExistingDataGrouping(currentCountryCode, "", parent: eunDataGrouping);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine = (JobComInvoiceLine)entryLine.Declaration.InvoiceLines[0];
			invoiceLine.JI_ZZF_NKTaxType = ordVat.ZZF_Code;
			entryLine.InvoiceLines.Add(invoiceLine);

			using (entryLine.Fees.SuspendSystemAddedVatFeeRecalculation())
			{
				VatCalculationTestHelper.CreateEntryLineFee(entryLine, EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, "%", 10m, 1000m, 100m, "ADD", true);
				VatCalculationTestHelper.CreateEntryLineFee(entryLine, EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, "%", 25m, 1212m, 456m, "", false);
				VatCalculationTestHelper.CreateEntryLineFee(entryLine, EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, "%", 45m, 771m, 657m, "", false);
				VatCalculationTestHelper.CreateEntryLineFee(entryLine, EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, "%", 33m, 123m, 543m, "ADD", true);
				VatCalculationTestHelper.CreateEntryLineFee(entryLine, EU.Business.UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty, "%", 1m, 50m, 123.32m, "ADD", false);
				VatCalculationTestHelper.CreateEntryLineFee(entryLine, EU.Business.UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty, "%", 1m, 50m, 123.32m, "ADD", false);
				VatCalculationTestHelper.CreateEntryLineFee(entryLine, "110", "%", 1m, 50m, 123.32m, "ADD", false);
				VatCalculationTestHelper.CreateEntryLineFee(entryLine, "110", "%", 1m, 50m, 123.32m, "ADD", true);
				VatCalculationTestHelper.CreateEntryLineFee(entryLine, "310", "%", 1m, 50m, 123.32m, "ADD", false);
			}

			var vatCalculator = new EntryLineVatCalculator(entryLine);
			var calculatedVat = vatCalculator.CalculateVatFee();
			AssertNotNull(nameof(calculatedVat), calculatedVat);

			CombineAssertions("Calculated VAT", () =>
			{
				AssertEquals("Rate", ordVat.ZZF_Value, calculatedVat.Rate);
				AssertEquals("BaseValue", 825.96m, calculatedVat.BaseValue);
				AssertEquals("Amount", 181.7112m, calculatedVat.Amount);
				AssertEquals("MethodOfCalculation", "%", calculatedVat.MethodOfCalculation);
				AssertEquals("AdjustedRate", ordVat.ZZF_Value * 100, calculatedVat.AdjustedRate);
			});
		}
	}
}
