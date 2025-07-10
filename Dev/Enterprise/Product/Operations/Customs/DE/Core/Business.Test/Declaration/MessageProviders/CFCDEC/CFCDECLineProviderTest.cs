using System;
using System.Linq;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CFCDECLineProvider))]
	sealed class CFCDECLineProviderTest : ImportDecLineProviderAbstractTest<CFCDECLineProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new CFCDECLineProvider(null));
		}

		public void TestTobaccoRevenueStampNumber()
		{
			invoiceLine.JI_TobaccoStamp = "AN123";
			AssertEquals("AN123", Provider.TobaccoRevenueStampNumber);
		}

		public void TestCustomsValue()
		{
			declaration.ZG_IsHighValueOvrd = false;

			CombineAssertions(() =>
			{
				AssertNull("Null", Provider.CustomsValue);

				var customsValue = Provider.CustomsValue;
				AssertEquals("Cached", customsValue, Provider.CustomsValue);
			});
		}

		public void TestCustomsValue_ConcessionInE01OrE02()
		{
			declaration.ZG_IsHighValueOvrd = true;
			invoiceLine.JI_Procedure = "7005E01";

			CombineAssertions(() =>
			{
				AssertNull("Null", Provider.CustomsValue);

				var customsValue = Provider.CustomsValue;
				AssertEquals("Cached", customsValue, Provider.CustomsValue);
			});
		}

		public void TestCustomsValue_ConcessionNotInE01OrE02()
		{
			declaration.ZG_IsHighValueOvrd = true;
			invoiceLine.JI_Procedure = "7005F01";

			var customsValue = Provider.CustomsValue;
			AssertEquals("Cached", customsValue, Provider.CustomsValue);
		}

		public void TestAssessmentOutwardProcessingFee()
		{
			TestHelper.SetExchangeRates(Factory);
			invoiceLine.Charges.AddNew(ImportChargeCodeList.Codes.OPF, 1.23m, Core.Constants.CurrencyCodes.UnitedStates);
			invoiceLine.Charges.AddNew(ImportChargeCodeList.Codes.TCE, 30m, Core.Constants.CurrencyCodes.Sweden);
			invoiceLine.ApportionedCharges.AddNew(ImportChargeCodeList.Codes.TCE, 3m, Core.Constants.CurrencyCodes.EuropeanUnion);
			invoiceLine.ApportionedCharges.AddNew(ImportChargeCodeList.Codes.OPF, 2.45m, Core.Constants.CurrencyCodes.UnitedStates);
			invoiceLine.ApportionedCharges.AddNew(ImportChargeCodeList.Codes.TCE, 40m, Core.Constants.CurrencyCodes.Sweden);
			invoiceLine.ApportionedCharges.AddNew(ImportChargeCodeList.Codes._014, 459m, Core.Constants.CurrencyCodes.Japan);
			AssertEquals("EUR", 4.09m, Provider.AssessmentOutwardProcessingFee);
		}

		public void TestAssessmentOutwardProcessingFee_Empty()
		{
			TestHelper.SetExchangeRates(Factory);
			var charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = "UPF";
			charge.J7_Amount = 105.40;
			AssertEquals(decimal.Zero, Provider.AssessmentOutwardProcessingFee);
		}

		public void TestGetAssessmentOutwardProcessingFeeOrTaxCosts_Format()
		{
			TestHelper.SetExchangeRates(Factory);
			invoiceLine.Charges.AddNew(ImportChargeCodeList.Codes.OPF, 1.23m, Core.Constants.CurrencyCodes.EuropeanUnion);
			invoiceLine.ApportionedCharges.AddNew(ImportChargeCodeList.Codes.OPF, 2.47m, Core.Constants.CurrencyCodes.EuropeanUnion);
			AssertEquals("3.7", Provider.AssessmentOutwardProcessingFee.ToString());
		}

		public void TestAssessmentTaxCosts()
		{
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;

			TestHelper.SetExchangeRates(Factory);
			invoiceLine.Charges.AddNew(ImportChargeCodeList.Codes.OPF, 1.23m, Core.Constants.CurrencyCodes.UnitedStates);
			invoiceLine.Charges.AddNew(ImportChargeCodeList.Codes.TCE, 3.46m, Core.Constants.CurrencyCodes.Sweden);
			invoiceLine.ApportionedCharges.AddNew(ImportChargeCodeList.Codes.TCE, 1.65m, Core.Constants.CurrencyCodes.EuropeanUnion);
			invoiceLine.ApportionedCharges.AddNew(ImportChargeCodeList.Codes.OPF, 2.45m, Core.Constants.CurrencyCodes.UnitedStates);
			invoiceLine.ApportionedCharges.AddNew(ImportChargeCodeList.Codes.TCE, 5.87m, Core.Constants.CurrencyCodes.Sweden);
			invoiceLine.ApportionedCharges.AddNew(ImportChargeCodeList.Codes._014, 459m, Core.Constants.CurrencyCodes.Japan);
			invoiceLine2.Charges.AddNew(ImportChargeCodeList.Codes.TCE, 1.00m, Core.Constants.CurrencyCodes.EuropeanUnion);
			invoiceLine2.Charges.AddNew(ImportChargeCodeList.Codes._014, 2.01m, Core.Constants.CurrencyCodes.EuropeanUnion);
			invoiceLine2.ApportionedCharges.AddNew(ImportChargeCodeList.Codes.TCE, 4.00m, Core.Constants.CurrencyCodes.EuropeanUnion);
			invoiceLine2.ApportionedCharges.AddNew(ImportChargeCodeList.Codes._014, 5.00m, Core.Constants.CurrencyCodes.EuropeanUnion);
			invoiceLine2.ApportionedCharges.AddNew(ImportChargeCodeList.Codes.AIR, 6.00m, Core.Constants.CurrencyCodes.EuropeanUnion);
			AssertEquals("EUR", 17.66m, Provider.AssessmentTaxCosts);
		}

		public void TestAssessmentTaxCosts_Empty()
		{
			TestHelper.SetExchangeRates(Factory);
			var charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = "TCI";
			charge.J7_Amount = 105.40;
			AssertEquals(decimal.Zero, Provider.AssessmentTaxCosts);
		}

		public void TestPreferentialTreatment()
		{
			invoiceLine.JI_ConcessionOrder = "A12345";
			invoiceLine.ZG_SecondQuota = "B12345";
			invoiceLine.JI_Procedure = "40005F0";

			var preferentialTreatment = Provider.PreferentialTreatment;
			AssertEquals("Cached", preferentialTreatment, Provider.PreferentialTreatment);
		}

		public void TestPreferentialTreatment_ConcessionF01() => AssertPreferentialTreatment_Concession("F01");

		public void TestPreferentialTreatment_ConcessionF02() => AssertPreferentialTreatment_Concession("F02");

		public void TestPreferentialTreatment_ConcessionF03() => AssertPreferentialTreatment_Concession("F03");

		public void TestPreferentialTreatment_Concession_CO()
		{
			invoiceLine.JI_ConcessionOrder = "A12345";
			invoiceLine.ZG_SecondQuota = "B12345";
			invoiceLine.JI_Procedure = "4000";
			declaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportFromSpecialTerritory;
			AssertNull("Null", Provider.PreferentialTreatment);
		}

		public void TestSpecialCases()
		{
			var specCase = invoiceLine.Taxes.AddNew();
			specCase.JLT_Type = "ABC";
			specCase.JLT_MethodOfCalculation = "XY";
			specCase.JLT_Rate = 12.2;

			CombineAssertions(() =>
			{
				var specialCases = Provider.SpecialCases;
				AssertEquals("Count", 1, specialCases.Count);
				AssertEquals("Cached", specialCases, Provider.SpecialCases);

				var specialCase = specialCases.First();
				AssertEquals("ABC", specialCase.Group);
				AssertEquals("XY", specialCase.ApplicationType);
				AssertEquals(12.2m, specialCase.RateOrAmountOrFactor);
			});
		}

		public void TestPreferentialOriginCountry()
		{
			invoiceLine.JI_PrimaryPreference = "200";
			invoiceLine.ZG_CountryOfSupply = "AU";
			AssertEquals("AU", Provider.PreferentialOriginCountry);
		}

		public void TestPreferentialOriginCountry_NotPopulated()
		{
			invoiceLine.JI_PrimaryPreference = "199";
			invoiceLine.ZG_CountryOfSupply = "AU";
			AssertNull(Provider.PreferentialOriginCountry);
		}

		public void TestPreferentialOriginCountry_NotPopulatedDueToInvalidPrimaryPreference()
		{
			invoiceLine.JI_PrimaryPreference = "A";
			invoiceLine.ZG_CountryOfSupply = "AU";
			AssertNull(Provider.PreferentialOriginCountry);
		}

		public void TestCessionManagementFlag_EmptyProcedure()
		{
			invoiceLine.JI_CessionFlag = "01";
			invoiceLine.JI_Procedure = string.Empty;

			AssertNull("Empty procedure", Provider.CessionManagementFlag);
		}

		public void TestCessionManagementFlag_Procedure()
		{
			invoiceLine.JI_CessionFlag = "01";
			invoiceLine.JI_Procedure = "40005F0";

			AssertEquals("Cession flag", "01", Provider.CessionManagementFlag);
		}

		protected override CFCDECLineProvider GetProvider() => new CFCDECLineProvider(entryLine);

		void AssertPreferentialTreatment_Concession(string concession)
		{
			invoiceLine.JI_ConcessionOrder = "A12345";
			invoiceLine.ZG_SecondQuota = "B12345";
			invoiceLine.JI_Procedure = "4000" + concession;
			declaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportFromEFTAMember;
			AssertNull("Null", Provider.PreferentialTreatment);
		}
	}
}
