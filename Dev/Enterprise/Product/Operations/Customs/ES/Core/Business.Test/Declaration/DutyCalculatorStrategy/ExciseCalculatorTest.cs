using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Testing;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	sealed class ExciseCalculatorTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Should be exception when entryLine parameter is null", () => new ExciseCalculator(null));
			AssertNoExceptionThrown("No exception expected", () => new ExciseCalculator(Factory.New<CusEntryLine>()));
		}

		public void TestCalculateExtraFees()
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
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];

			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			invoiceLine.JI_Tariff = "11112222";
			var exciseIntermediateResults = new ExciseCalculator(entryLine).CalculateExtraFees();
			AssertEquals("When InvoiceLine.ZG_ExciseCode is empty, exciseIntermediateResults count is 0", 0, exciseIntermediateResults.Count());

			invoiceLine.ZG_ExciseCode = "XX";
			exciseIntermediateResults = new ExciseCalculator(entryLine).CalculateExtraFees();
			AssertEquals("When InvoiceLine.ZG_ExciseCode does not have a valid value, exciseIntermediateResults count is 0", 0, exciseIntermediateResults.Count());

			invoiceLine.ZG_ExciseCode = "0A7";
			invoiceLine.ZG_TotalRetailPrice = 30;
			exciseIntermediateResults = new ExciseCalculator(entryLine).CalculateExtraFees();
			AssertEquals("exciseIntermediateResults count is 1", 1, exciseIntermediateResults.Count());

			var excise = exciseIntermediateResults.Single();
			CombineAssertions(() =>
			{
				AssertExcise(excise, 6m, "PVP", 0.2m, 30m);

				invoiceLine.ZG_ExciseCode = "0A0";
				invoiceLine.JI_CustomsThirdUnitQty = "HG";
				invoiceLine.JI_CustomsThirdQuantity = 50;
				exciseIntermediateResults = new ExciseCalculator(entryLine).CalculateExtraFees();
				AssertEquals("exciseIntermediateResults count is 1", 1, exciseIntermediateResults.Count());
				excise = exciseIntermediateResults.Single();
				AssertExcise(excise, 37518m, "HG", 750.36m, 50m);

				invoiceLine.ZG_ExciseCode = "0A1";
				entryLine.CL_CustomsValue = 30;
				exciseIntermediateResults = new ExciseCalculator(entryLine).CalculateExtraFees();
				AssertEquals("exciseIntermediateResults count is 1", 1, exciseIntermediateResults.Count());
				excise = exciseIntermediateResults.Single();
				AssertExcise(excise, 15m, "%", 0.5m, 30m);
			});
		}

		public void TestCalculateExtraFees_FluorinatedGases()
		{
			var helper = new ESUniversalReferenceTestDataHelper(Factory);
			var countryCode = Core.Constants.CountryCodes.Spain;
			var impTariffType = helper.CreateTariffType(countryCode, "IMP");
			var esexcTariffType = helper.CreateTariffType(countryCode, "ESEXC");
			var rateType = helper.CreateCusRateType(countryCode, "EXC");
			Factory.Save();
			var tariff = helper.LoadOrCreateNewTariff(countryCode, impTariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			CreateNewFormula(helper, esexcTariffType.PK, "1CF", "0.5*PCA*[GF]", rateType.PK, impTariffType.PK, tariff.ZZ1_TariffCode);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			invoiceLine.JI_Tariff = "11112222";
			CombineAssertions(() =>
			{
				invoiceLine.ZG_ExciseCode = "1CF";
				invoiceLine.JI_CustomsThirdUnitQty = "GF";
				invoiceLine.JI_CustomsThirdQuantity = 30;
				entryLine.CL_CustomsValue = 30;
				var exciseIntermediateResults = new ExciseCalculator(entryLine).CalculateExtraFees();
				AssertEquals("exciseIntermediateResults count is 1", 1, exciseIntermediateResults.Count());
				var excise = exciseIntermediateResults.Single();
				AssertExcise(excise, 3000m, "GF", 100m, 30m);

				invoiceLine.ZG_GlobalWarmingPotential = 1;
				exciseIntermediateResults = new ExciseCalculator(entryLine).CalculateExtraFees();
				AssertEquals("exciseIntermediateResults count is 1", 1, exciseIntermediateResults.Count());
				excise = exciseIntermediateResults.Single();
				AssertExcise(excise, 15m, "GF", 0.5m, 30m);

				invoiceLine.ZG_GlobalWarmingPotential = 10;
				exciseIntermediateResults = new ExciseCalculator(entryLine).CalculateExtraFees();
				AssertEquals("exciseIntermediateResults count is 1", 1, exciseIntermediateResults.Count());
				excise = exciseIntermediateResults.Single();
				AssertExcise(excise, 150m, "GF", 5m, 30m);
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
