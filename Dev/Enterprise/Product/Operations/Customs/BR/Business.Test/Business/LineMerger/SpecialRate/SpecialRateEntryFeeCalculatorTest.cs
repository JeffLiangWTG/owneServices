using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.BR.Business.Constants;

namespace Enterprise.Customs.BR.Business.Testing
{
	class SpecialRateEntryFeeCalculatorTest : TestCaseWithFactory
	{
		public void TestUpdateFeesOnEntryLine()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "99999999", 10m, tariffType: Universal.Constants.TariffTypes.HarmonizedSystem, rateType: RateTypes.IPI, rateCode: RateCodes.IPI);
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "99999999", 15m, tariffType: Universal.Constants.TariffTypes.HarmonizedSystem, rateType: RateTypes.Antidumping, rateCode: RateCodes.Antidumping);

			var usdCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);
			var rate = usdCurrency.ExchangeRates.AddNew();
			rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			rate.RE_StartDate = ZDateTime.Now.AddDays(-2);
			rate.RE_ExpiryDate = ZDateTime.Now.AddDays(2);
			rate.RE_SellRate = 5;
			rate.RE_GC = GlbCompany.CurrentCompany.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = cusEntryHeader.MergedLines.AddNew();

			for (var i = 0; i < 2; i++)
			{
				var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;

				var specialCaseTax1 = invoiceLine.SpecialCaseTaxes.AddNew();
				specialCaseTax1.TaxGroup = Constants.RateCodes.IPI;
				specialCaseTax1.TaxType = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
				specialCaseTax1.RateOrUnitValue = 7.333349m;
				specialCaseTax1.Quantity = 1000;
				specialCaseTax1.UnitOfMeasure = "KG";
				specialCaseTax1.CurrencyCode = Core.Constants.CurrencyCodes.Brazil;

				var specialCaseTax2 = invoiceLine.SpecialCaseTaxes.AddNew();
				specialCaseTax2.TaxGroup = Constants.RateCodes.Antidumping;
				specialCaseTax2.TaxType = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
				specialCaseTax2.RateOrUnitValue = 9m;
				specialCaseTax2.Quantity = 1500;
				specialCaseTax2.UnitOfMeasure = "KG";
				specialCaseTax2.CurrencyCode = Core.Constants.CurrencyCodes.UnitedStates;
			}

			var calculator = new SpecialRateEntryFeeCalculator(entryLine);
			calculator.UpdateFeesOnEntryLine();

			CombineAssertions("IPI", () =>
			{
				var fee1 = entryLine.Fees.GetElementWithThisCode(Constants.RateTypes.IPI);
				AssertEquals("CF_ChargeAmount", 14666.70m, fee1.CF_ChargeAmount);
				AssertEquals("CF_BaseValue", 2000m, fee1.CF_BaseValue);
				AssertEquals("CF_Rate", 7.33335m, fee1.CF_Rate);
				AssertEquals("CF_MethodOfCalculation", SpecialCaseTaxTypeList.Codes.QuantityPerUnit, fee1.CF_MethodOfCalculation);
			});

			CombineAssertions("Antidumping", () =>
			{
				var fee2 = entryLine.Fees.GetElementWithThisCode(Constants.RateTypes.Antidumping);
				AssertEquals("CF_ChargeAmount", 5400m, fee2.CF_ChargeAmount);
				AssertEquals("CF_BaseValue", 3000m, fee2.CF_BaseValue);
				AssertEquals("CF_Rate", 1.8m, fee2.CF_Rate);
				AssertEquals("CF_MethodOfCalculation", SpecialCaseTaxTypeList.Codes.QuantityPerUnit, fee2.CF_MethodOfCalculation);
			});
		}

		public void TestUpdateFeesOnEntryLine_RateIsZero()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "99999999", 10m, tariffType: Universal.Constants.TariffTypes.HarmonizedSystem, rateType: RateTypes.IPI, rateCode: RateCodes.IPI, tradeGroupCountry: Core.Constants.CountryCodes.Zimbabwe, customsValueFormula: "CV + DTY");

			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			var cusEntryLine = cusEntryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = cusEntryLine.PK;

			var specialCaseTax = invoiceLine.SpecialCaseTaxes.AddNew();
			specialCaseTax.TaxGroup = Constants.RateCodes.IPI;
			specialCaseTax.TaxType = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
			specialCaseTax.RateOrUnitValue = 0m;
			specialCaseTax.Quantity = 1000;
			specialCaseTax.UnitOfMeasure = "KG";
			specialCaseTax.CurrencyCode = Core.Constants.CurrencyCodes.Brazil;

			var calculator = new SpecialRateEntryFeeCalculator(cusEntryLine);
			calculator.UpdateFeesOnEntryLine();

			AssertNull(cusEntryLine.Fees.GetElementWithThisCode(Constants.RateTypes.IPI));
		}
	}
}
