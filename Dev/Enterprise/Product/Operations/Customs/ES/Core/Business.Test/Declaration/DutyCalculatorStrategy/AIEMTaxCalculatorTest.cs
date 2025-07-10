using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Testing;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	sealed class AIEMTaxCalculatorTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Should be exception when entryLine parameter is null", () => new AIEMTaxCalculator(null));
			AssertNoExceptionThrown("No exception expected", () => new AIEMTaxCalculator(Factory.New<CusEntryLine>()));
		}

		public void TestCalculateExtraFees()
		{
			var helper = new ESUniversalReferenceTestDataHelper(Factory);
			var countryCode = Core.Constants.CountryCodes.Spain;
			helper.CreateCusCodeListCanaryIsland(countryCode, "61", "Test 61");

			var impTariffType = helper.CreateTariffType(countryCode, "IMP");
			var aiemTariffType = helper.CreateTariffType(countryCode, UniversalReferenceConstants.RefCusTariffType.AIEM);
			var rateType = helper.CreateCusRateType(countryCode, "EXC");
			Factory.Save();
			var tariff = helper.LoadOrCreateNewTariff(countryCode, impTariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

			var tariffExcise = helper.LoadOrCreateNewTariff(countryCode, aiemTariffType.PK, "11112222_AIEM11", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "AIEM", rateType.PK);
			helper.CreateRate(tariffExcise, rateCode.PK, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), rateFormula: "0.2*VFD");

			helper.CreateTariffRelationship(tariffExcise.PK, impTariffType.PK, tariff.ZZ1_TariffCode);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];

			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			invoiceLine.JI_Tariff = "11112222";
			var aiemIntermediateResults = new AIEMTaxCalculator(entryLine).CalculateExtraFees();
			AssertEquals("When InvoiceLine.ZG_AIEMType is empty, aiemIntermediateResults count is 0", 0, aiemIntermediateResults.Count());

			invoiceLine.ZG_AIEMType = "XX";
			aiemIntermediateResults = new AIEMTaxCalculator(entryLine).CalculateExtraFees();
			AssertEquals("When InvoiceLine.ZG_AIEMType does not have a valid value, aiemIntermediateResults count is 0", 0, aiemIntermediateResults.Count());

			var fee = entryLine.Fees.AddNew();
			fee.G4_Type = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
			fee.G4_BaseAmount = 30m;
			invoiceLine.ZG_AIEMType = "AIEM11";
			aiemIntermediateResults = new AIEMTaxCalculator(entryLine).CalculateExtraFees();
			AssertEquals("aiemIntermediateResults count is 1", 1, aiemIntermediateResults.Count());

			var tobaccoExcise = aiemIntermediateResults.Single();
			CombineAssertions(() =>
			{
				AssertEquals("Amount", 6m, tobaccoExcise.Amount);
				AssertEquals("MethodOfCalculation", "%", tobaccoExcise.MethodOfCalculation);
				AssertEquals("Rate", 0.2m, tobaccoExcise.Rate);
				AssertEquals("BaseValue", 30m, tobaccoExcise.BaseValue);
			});
		}
	}
}
