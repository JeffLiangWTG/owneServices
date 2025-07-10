using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Testing;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	sealed class ExciseSpecialRateCalculatorTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Should be exception when entryLine parameter is null", () => new ExciseSpecialRateCalculator(null));
			AssertNoExceptionThrown("No exception expected", () => new ExciseSpecialRateCalculator(Factory.New<CusEntryLine>()));
		}

		public void TestSpecialCalculateExtraFees()
		{
			var helper = new ESUniversalReferenceTestDataHelper(Factory);
			var countryCode = Core.Constants.CountryCodes.Spain;
			helper.CreateCusCodeListCanaryIsland(countryCode, "61", "Test 61");

			var impTariffType = helper.CreateTariffType(countryCode, "IMP");
			var canexcTariffType = helper.CreateTariffType(countryCode, "CANEX");
			var rateType = helper.CreateCusRateType(countryCode, "EXC");
			Factory.Save();
			var tariff = helper.LoadOrCreateNewTariff(countryCode, impTariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

			CreateNewFormula(helper, canexcTariffType.PK, "0A7", "0.2*PVP", rateType.PK, impTariffType.PK, tariff.ZZ1_TariffCode);
			CreateNewFormula(helper, canexcTariffType.PK, "0A0", "750.36*[HG]", rateType.PK, impTariffType.PK, tariff.ZZ1_TariffCode);
			CreateNewFormula(helper, canexcTariffType.PK, "0A1", "0.5*VFD", rateType.PK, impTariffType.PK, tariff.ZZ1_TariffCode);
			CreateNewFormula(helper, canexcTariffType.PK, "5A7", "0.3*PVP", rateType.PK, impTariffType.PK, tariff.ZZ1_TariffCode);
			CreateNewFormula(helper, canexcTariffType.PK, "5A0", "751.36*[HG]", rateType.PK, impTariffType.PK, tariff.ZZ1_TariffCode);
			CreateNewFormula(helper, canexcTariffType.PK, "5A1", "0.6*VFD", rateType.PK, impTariffType.PK, tariff.ZZ1_TariffCode);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];

			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			CombineAssertions(() =>
			{
				invoiceLine.JI_Tariff = "11112222";
				var exciseSpecialIntermediateResults = new ExciseSpecialRateCalculator(entryLine).CalculateExtraFees();
				AssertEquals("When InvoiceLine.ZG_ExciseCode is empty, exciseIntermediateResults count is 0", 0, exciseSpecialIntermediateResults.Count());

				invoiceLine.ZG_ExciseCode = "XX";
				exciseSpecialIntermediateResults = new ExciseSpecialRateCalculator(entryLine).CalculateExtraFees();
				AssertEquals("When InvoiceLine.ZG_ExciseCode does not have a valid value, exciseIntermediateResults count is 0", 0, exciseSpecialIntermediateResults.Count());

				invoiceLine.ZG_ExciseCode = "0A7";
				invoiceLine.ZG_TotalRetailPrice = 31;
				exciseSpecialIntermediateResults = new ExciseSpecialRateCalculator(entryLine).CalculateExtraFees();
				AssertEquals("For 5A7 exciseIntermediateResults count is 1", 1, exciseSpecialIntermediateResults.Count());

				var excise = exciseSpecialIntermediateResults.Single();

				AssertExcise(excise, 9.3m, "PVP", 0.3m, 31m);

				invoiceLine.ZG_ExciseCode = "0A0";
				invoiceLine.JI_CustomsThirdUnitQty = "HG";
				invoiceLine.JI_CustomsThirdQuantity = 51;
				exciseSpecialIntermediateResults = new ExciseSpecialRateCalculator(entryLine).CalculateExtraFees();
				AssertEquals("For 5A0 exciseIntermediateResults count is 1", 1, exciseSpecialIntermediateResults.Count());
				excise = exciseSpecialIntermediateResults.Single();
				AssertExcise(excise, 38319.36m, "HG", 751.36m, 51m);

				invoiceLine.ZG_ExciseCode = "0A1";
				entryLine.CL_CustomsValue = 32;
				exciseSpecialIntermediateResults = new ExciseSpecialRateCalculator(entryLine).CalculateExtraFees();
				AssertEquals("For 5A1 exciseIntermediateResults count is 1", 1, exciseSpecialIntermediateResults.Count());
				excise = exciseSpecialIntermediateResults.Single();
				AssertExcise(excise, 19.2m, "%", 0.6m, 32m);
			});
		}

		void CreateNewFormula(Universal.Testing.UniversalReferenceTestDataHelper helper, ZGuid tariffTypePK, string rateCode, string formula, ZGuid rateTypePK, ZGuid impTariffTypePK, ZString tariffCode)
		{
			var tariffExcise = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Spain, tariffTypePK, rateCode, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc2");
			var rate = helper.LoadOrCreateNewCusRateCode(Factory, rateCode, rateTypePK);
			helper.CreateRate(tariffExcise, rate.PK, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), rateFormula: formula);

			helper.CreateTariffRelationship(tariffExcise.PK, impTariffTypePK, tariffCode);
		}

		void AssertExcise(IDutyCalculationIntermediateResult excise, decimal amount, string methodOfCalc, decimal rate, decimal baseValue)
		{
			AssertEquals("Amount", amount, excise.Amount);
			AssertEquals("MethodOfCalculation", methodOfCalc, excise.MethodOfCalculation);
			AssertEquals("Rate", rate, excise.Rate);
			AssertEquals("BaseValue", baseValue, excise.BaseValue);
		}
	}
}
