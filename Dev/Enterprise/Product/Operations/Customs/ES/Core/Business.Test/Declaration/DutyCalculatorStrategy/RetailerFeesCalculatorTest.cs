using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	public class RetailerFeesCalculatorTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Should be exception when entryLine parameter is null", () => new RetailerFeesCalculator(null));
			AssertNoExceptionThrown("No exception expected", () => new RetailerFeesCalculator(Factory.New<CusEntryLine>()));
		}

		public void TestCalculateExtraFeesInland()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var countryCode = Core.Constants.CountryCodes.Spain;
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);

			var mapType = helper.CreateCusMapType(UniversalReferenceConstants.RefCusMapType.RetailerFee, Universal.MapDirectionList.Codes.BTH, "Retailer fee Spain", true);
			helper.CreateCusMap(mapType.ZZP_MapType, "IV1", "RQ1", startDate, endDate, countryCode);

			var taxOrFeeType = helper.CreateRefCusTaxOrFeeType("VAT");
			var taxOrFee = helper.CreateTaxOrFee("RQ1", 0.0520, countryCode, startDate, endDate);
			taxOrFee.ZZF_ZX0_NKTaxOrFeeType = taxOrFeeType.ZX0_TaxOrFeeType;

			helper.CreateCusMap(mapType.ZZP_MapType, "IV2", "RQ2", startDate, endDate, countryCode);
			helper.CreateTaxOrFee("RQ2", 0.0400, countryCode, startDate, endDate);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.ZG_DestinationState = "1";
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			CombineAssertions(() =>
			{
				var retailerIntermediateResults = new RetailerFeesCalculator(entryLine).CalculateExtraFees();
				AssertEquals("When InvoiceLine.JI_ZZF_NKTaxType is empty, retailerIntermediateResults count is 0", 0, retailerIntermediateResults.Count());

				invoiceLine.JI_ZZF_NKTaxType = "XX";
				retailerIntermediateResults = new RetailerFeesCalculator(entryLine).CalculateExtraFees();
				AssertEquals("When InvoiceLine.JI_ZZF_NKTaxType does not have a valid value, retailerIntermediateResults count is 0", 0, retailerIntermediateResults.Count());

				var fee = entryLine.Fees.AddNew();
				fee.G4_Type = "AH3";
				fee.G4_BaseAmount = 30m;

				retailerIntermediateResults = new RetailerFeesCalculator(entryLine).CalculateExtraFees();
				AssertEquals("When entryLine does not have B00 Fee, retailerIntermediateResults count is 0", 0, retailerIntermediateResults.Count());

				var fee2 = entryLine.Fees.AddNew();
				fee2.G4_Type = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
				fee2.G4_BaseAmount = 30m;
				invoiceLine.JI_ZZF_NKTaxType = "IV1";
				retailerIntermediateResults = new RetailerFeesCalculator(entryLine).CalculateExtraFees();
				AssertEquals("retailerIntermediateResults count is 1", 1, retailerIntermediateResults.Count());
				var retailerFee = retailerIntermediateResults.Single();
				AssertEquals("Amount", 1.56m, retailerFee.Amount);
				AssertEquals("MethodOfCalculation", "%", retailerFee.MethodOfCalculation);
				AssertEquals("Rate", 0.0520m, retailerFee.Rate);
				AssertEquals("BaseValue", 30m, retailerFee.BaseValue);

				invoiceLine.JI_ZZF_NKTaxType = "IV2";
				retailerIntermediateResults = new RetailerFeesCalculator(entryLine).CalculateExtraFees();
				AssertEquals("retailerIntermediateResults count is 0 when the RefCusTaxOrFee is not VAT Type", 0, retailerIntermediateResults.Count());
			});
		}

		public void TestCalculateExtraFeesCanaryIslands()
		{
			var helper = new ESUniversalReferenceTestDataHelper(Factory);
			var countryCode = Core.Constants.CountryCodes.Spain;

			#region CanaryIslands Setup
			helper.CreateCusCodeListCanaryIsland(countryCode, "61", "Test 61");
			#endregion

			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);

			var mapType = helper.CreateCusMapType(UniversalReferenceConstants.RefCusMapType.RetailerFee, Universal.MapDirectionList.Codes.BTH, "Retailer fee Spain", true);
			helper.CreateCusMap(mapType.ZZP_MapType, "4TD", "RM7", startDate, endDate, countryCode);

			var taxOrFeeType = helper.CreateRefCusTaxOrFeeType("VAT");
			var taxOrFee = helper.CreateTaxOrFee("RM7", 0.0200, countryCode, startDate, endDate);
			taxOrFee.ZZF_ZX0_NKTaxOrFeeType = taxOrFeeType.ZX0_TaxOrFeeType;

			helper.CreateCusMap(mapType.ZZP_MapType, "4TA", "RM8", startDate, endDate, countryCode);
			helper.CreateTaxOrFee("RM8", 0.0300, countryCode, startDate, endDate);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			CombineAssertions(() =>
			{
				var retailerIntermediateResults = new RetailerFeesCalculator(entryLine).CalculateExtraFees();
				AssertEquals("When InvoiceLine.JI_ZZF_NKTaxType is empty, retailerIntermediateResults count is 0", 0, retailerIntermediateResults.Count());

				invoiceLine.JI_ZZF_NKTaxType = "XX";
				retailerIntermediateResults = new RetailerFeesCalculator(entryLine).CalculateExtraFees();
				AssertEquals("When InvoiceLine.JI_ZZF_NKTaxType does not have a valid value, retailerIntermediateResults count is 0", 0, retailerIntermediateResults.Count());

				var fee = entryLine.Fees.AddNew();
				fee.G4_Type = "AH3";
				fee.G4_BaseAmount = 30m;
				invoiceLine.JI_ZZF_NKTaxType = "4TD";

				retailerIntermediateResults = new RetailerFeesCalculator(entryLine).CalculateExtraFees();
				AssertEquals("When entryLine does not have 3IG or B00 Fee, retailerIntermediateResults count is 0", 0, retailerIntermediateResults.Count());

				var fee2 = entryLine.Fees.AddNew();
				fee2.G4_Type = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
				fee2.G4_BaseAmount = 30m;
				retailerIntermediateResults = new RetailerFeesCalculator(entryLine).CalculateExtraFees();
				AssertEquals("retailerIntermediateResults count is 1 with B00", 1, retailerIntermediateResults.Count());

				fee2.G4_Type = "AH";
				retailerIntermediateResults = new RetailerFeesCalculator(entryLine).CalculateExtraFees();
				AssertEquals("retailerIntermediateResults count is 0 with no 3IG or B00 Fee", 0, retailerIntermediateResults.Count());

				fee2.G4_Type = UniversalReferenceConstants.RefCusRateCode.IGIC;
				retailerIntermediateResults = new RetailerFeesCalculator(entryLine).CalculateExtraFees();
				AssertEquals("retailerIntermediateResults count is 1 with 3IG", 1, retailerIntermediateResults.Count());

				fee.G4_Type = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
				retailerIntermediateResults = new RetailerFeesCalculator(entryLine).CalculateExtraFees();
				AssertEquals("retailerIntermediateResults count is 1 with 3IG and B00 for calculation", 1, retailerIntermediateResults.Count());
				var retailerFee = retailerIntermediateResults.Single();
				AssertEquals("Amount", 0.6m, retailerFee.Amount);
				AssertEquals("MethodOfCalculation", "%", retailerFee.MethodOfCalculation);
				AssertEquals("Rate", 0.0200m, retailerFee.Rate);
				AssertEquals("BaseValue", 30m, retailerFee.BaseValue);

				invoiceLine.JI_ZZF_NKTaxType = "4TA";
				retailerIntermediateResults = new RetailerFeesCalculator(entryLine).CalculateExtraFees();
				AssertEquals("retailerIntermediateResults count is 0 when the RefCusTaxOrFee is not VAT Type", 0, retailerIntermediateResults.Count());

				fee = entryLine.Fees.AddNew();
				fee.G4_Type = UniversalReferenceConstants.RefCusRateCode.AIEM;
				fee.G4_Amount = "20";
				invoiceLine.JI_ZZF_NKTaxType = "4TD";
				retailerIntermediateResults = new RetailerFeesCalculator(entryLine).CalculateExtraFees();
				AssertEquals("retailerIntermediateResults count is 1", 1, retailerIntermediateResults.Count());
				retailerFee = retailerIntermediateResults.Single();
				AssertEquals("Amount", 1m, retailerFee.Amount);
				AssertEquals("MethodOfCalculation", "%", retailerFee.MethodOfCalculation);
				AssertEquals("Rate", 0.0200m, retailerFee.Rate);
				AssertEquals("BaseValue (B00 + 3AI)", 50m, retailerFee.BaseValue);
			});
		}

		public void TestCalculateExtraFeesWhenPVPApplicable()
		{
			#region SetUp
			var helper = new ESUniversalReferenceTestDataHelper(Factory);
			var countryCode = Core.Constants.CountryCodes.Spain;
			helper.CreateCusCodeListCanaryIsland(countryCode, "61", "Test 61");
			var impTariffType = helper.CreateTariffType(countryCode, "IMP");
			var canexcTariffType = helper.CreateTariffType(countryCode, "CANEX");
			var rateType = helper.CreateCusRateType(countryCode, "EXC");

			var tariff = helper.LoadOrCreateNewTariff(countryCode, impTariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			var tariffExcise = helper.LoadOrCreateNewTariff(countryCode, canexcTariffType.PK, "0A0", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc1");
			var rateCodePVP = helper.LoadOrCreateNewCusRateCode(Factory, "0A0", rateType.PK);
			helper.CreateRate(tariffExcise, rateCodePVP.PK, ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), rateFormula: "0.2*PVP");
			helper.CreateTariffRelationship(tariffExcise.PK, impTariffType.PK, tariff.ZZ1_TariffCode);

			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);

			var mapType = helper.CreateCusMapType(UniversalReferenceConstants.RefCusMapType.RetailerFee, Universal.MapDirectionList.Codes.BTH, "Retailer fee Spain", true);
			var taxOrFeeType = helper.CreateRefCusTaxOrFeeType("VAT");

			helper.CreateCusMap(mapType.ZZP_MapType, "IV1", "RQ1", startDate, endDate, countryCode);
			var taxOrFee = helper.CreateTaxOrFee("RQ1", 0.0520, countryCode, startDate, endDate);
			taxOrFee.ZZF_ZX0_NKTaxOrFeeType = taxOrFeeType.ZX0_TaxOrFeeType;

			helper.CreateCusMap(mapType.ZZP_MapType, "0A0", "RQ2", startDate, endDate, countryCode);
			taxOrFee = helper.CreateTaxOrFee("RQ2", 0.0400, countryCode, startDate, endDate);
			taxOrFee.ZZF_ZX0_NKTaxOrFeeType = taxOrFeeType.ZX0_TaxOrFeeType;
			Factory.Save();
			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.ZG_DestinationState = CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Tariff = "11112222";

			CombineAssertions(() =>
			{
				var retailerIntermediateResults = new RetailerFeesCalculator(entryLine).CalculateExtraFees();
				AssertEquals("When ZG_ExciseCode and JI_ZZF_NKTaxType are empty, retailerIntermediateResults count is 0", 0, retailerIntermediateResults.Count());

				invoiceLine.JI_ZZF_NKTaxType = "XX";
				retailerIntermediateResults = new RetailerFeesCalculator(entryLine).CalculateExtraFees();
				AssertEquals("When JI_ZZF_NKTaxType does not have a valid value, retailerIntermediateResults count is 0", 0, retailerIntermediateResults.Count());

				invoiceLine.ZG_ExciseCode = "XX";
				AssertEquals("When ZG_ExciseCode does not have a valid value, retailerIntermediateResults count is 0", 0, retailerIntermediateResults.Count());

				var fee = entryLine.Fees.AddNew();
				fee.G4_Type = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
				fee.G4_BaseAmount = 30m;
				invoiceLine.JI_ZZF_NKTaxType = "IV1";
				retailerIntermediateResults = new RetailerFeesCalculator(entryLine).CalculateExtraFees();
				AssertEquals("retailerIntermediateResults count is 1 due to valid JI_ZZF_NKTaxType", 1, retailerIntermediateResults.Count());
				var retailerFee = retailerIntermediateResults.Single();
				AssertEquals("Amount", 1.56m, retailerFee.Amount);
				AssertEquals("MethodOfCalculation", "%", retailerFee.MethodOfCalculation);
				AssertEquals("Rate", 0.0520m, retailerFee.Rate);
				AssertEquals("BaseValue", 30m, retailerFee.BaseValue);

				invoiceLine.ZG_ExciseCode = "0A0";
				retailerIntermediateResults = new RetailerFeesCalculator(entryLine).CalculateExtraFees();
				AssertEquals("retailerIntermediateResults count is 1 due to valid ZG_ExciseCode that overrides JI_ZZF_NKTaxType calculation", 1, retailerIntermediateResults.Count());
				retailerFee = retailerIntermediateResults.Single();
				AssertEquals("Amount", 1.2m, retailerFee.Amount);
				AssertEquals("MethodOfCalculation", "%", retailerFee.MethodOfCalculation);
				AssertEquals("Rate", 0.0400m, retailerFee.Rate);
				AssertEquals("BaseValue", 30m, retailerFee.BaseValue);
			});
		}
	}
}
