using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.Customs.BR.Business.Constants;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(DutyCalculatorStrategy))]
	class DutyCalculatorStrategyTest : Customs.Business.Testing.DutyCalculatorStrategyAbstractTest<DutyCalculatorStrategy>
	{
		public void TestCalculatePISTax()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "99999999", 20m, tariffType: Universal.Constants.TariffTypes.HarmonizedSystem, rateType: RateTypes.PIS, rateCode: RateCodes.PIS, tradeGroupCountry: Core.Constants.CountryCodes.Zimbabwe);

			var declaration = CreateTestDeclarationAndInvoiceLines(RatePreferenceType.Normal, false);

			DoMerge(declaration);

			var entryLineFees = declaration.ActiveEntryHeaders[0].MergedLines[0].Fees[0];

			CombineAssertions(() =>
			{
				AssertEquals("CF_ChargeType", RateTypes.PIS, entryLineFees.CF_ChargeType);

				AssertEquals("CF_BaseValue", 200m, entryLineFees.CF_BaseValue);
				AssertEquals("CF_Rate", 20m, entryLineFees.CF_Rate);
				AssertEquals("CF_ChargeAmount", 40m, entryLineFees.CF_ChargeAmount);
			});

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Brazil, ProcedureCategories.PisCofins, ZString.Empty, "2", ZString.Empty, ZString.Empty, Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex, false);

			declaration = CreateTestDeclarationAndInvoiceLines(RatePreferenceType.Normal, false);

			var invLine = declaration.Invoices[0].InvoiceLines[0];
			invLine.PisCofinsTaxRegime = "2";

			DoMerge(declaration);

			entryLineFees = declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.GetElementWithThisCode(RateTypes.PIS);

			CombineAssertions(() =>
			{
				AssertEquals("CF_ChargeType", RateTypes.PIS, entryLineFees.CF_ChargeType);
				AssertEquals("CF_BaseValue", 200m, entryLineFees.CF_BaseValue);
				AssertEquals("CF_Rate", 20m, entryLineFees.CF_Rate);
				AssertEquals("CF_ChargeAmount", 0m, entryLineFees.CF_ChargeAmount);
			});

			invLine.PisCofinsTaxRegime = ZString.Empty;
			invLine.PisRateIsOverridden = true;
			invLine.PisVigentRateValue = 10m;

			DoMerge(declaration);

			CombineAssertions(() =>
			{
				AssertEquals("CF_ChargeType", RateTypes.PIS, entryLineFees.CF_ChargeType);
				AssertEquals("CF_BaseValue", 200m, entryLineFees.CF_BaseValue);
				AssertEquals("CF_Rate", 10m, entryLineFees.CF_Rate);
				AssertEquals("CF_ChargeAmount", 20m, entryLineFees.CF_ChargeAmount);
			});

			invLine.PisRateIsOverridden = false;

			var specialCase = invLine.SpecialCaseTaxes.AddNew();
			specialCase.TaxGroup = Constants.RateCodes.PIS;
			specialCase.TaxType = SpecialCaseTaxTypeList.Codes.Reduced;
			specialCase.RateOrUnitValue = 5m;

			DoMerge(declaration);

			CombineAssertions(() =>
			{
				AssertEquals("CF_ChargeType", RateTypes.PIS, entryLineFees.CF_ChargeType);
				AssertEquals("CF_BaseValue", 200m, entryLineFees.CF_BaseValue);
				AssertEquals("CF_Rate", 5m, entryLineFees.CF_Rate);
				AssertEquals("CF_ChargeAmount", 10m, entryLineFees.CF_ChargeAmount);
			});
		}

		public void TestCalculateCOFINSTax()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "99999999", 15m, tariffType: Universal.Constants.TariffTypes.HarmonizedSystem, rateType: RateTypes.Cofins, rateCode: RateCodes.Cofins, tradeGroupCountry: Core.Constants.CountryCodes.Zimbabwe);

			var declaration = CreateTestDeclarationAndInvoiceLines(RatePreferenceType.Normal, false);

			DoMerge(declaration);

			var entryLineFees = declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.GetElementWithThisCode(RateTypes.Cofins);

			CombineAssertions(() =>
			{
				AssertEquals("CF_ChargeType", RateTypes.Cofins, entryLineFees.CF_ChargeType);
				AssertEquals("CF_BaseValue", 200m, entryLineFees.CF_BaseValue);
				AssertEquals("CF_Rate", 15m, entryLineFees.CF_Rate);
				AssertEquals("CF_ChargeAmount", 30m, entryLineFees.CF_ChargeAmount);
			});

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Brazil, ProcedureCategories.PisCofins, ZString.Empty, "2", ZString.Empty, ZString.Empty, Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex, false);

			declaration = CreateTestDeclarationAndInvoiceLines(RatePreferenceType.Normal, false);

			var invLine = declaration.Invoices[0].InvoiceLines[0];
			invLine.PisCofinsTaxRegime = "2";

			DoMerge(declaration);

			entryLineFees = declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.GetElementWithThisCode(RateTypes.Cofins);

			CombineAssertions(() =>
			{
				AssertEquals("CF_ChargeType", RateTypes.Cofins, entryLineFees.CF_ChargeType);
				AssertEquals("CF_BaseValue", 200m, entryLineFees.CF_BaseValue);
				AssertEquals("CF_Rate", 15m, entryLineFees.CF_Rate);
				AssertEquals("CF_ChargeAmount", 0m, entryLineFees.CF_ChargeAmount);
			});

			invLine.PisCofinsTaxRegime = ZString.Empty;
			invLine.CofinsRateIsOverridden = true;
			invLine.CofinsVigentRateValue = 10m;

			DoMerge(declaration);

			CombineAssertions(() =>
			{
				AssertEquals("CF_ChargeType", RateTypes.Cofins, entryLineFees.CF_ChargeType);
				AssertEquals("CF_BaseValue", 200m, entryLineFees.CF_BaseValue);
				AssertEquals("CF_Rate", 10m, entryLineFees.CF_Rate);
				AssertEquals("CF_ChargeAmount", 20m, entryLineFees.CF_ChargeAmount);
			});

			invLine.CofinsRateIsOverridden = false;

			var specialCase = invLine.SpecialCaseTaxes.AddNew();
			specialCase.TaxGroup = Constants.RateCodes.Cofins;
			specialCase.TaxType = SpecialCaseTaxTypeList.Codes.Reduced;
			specialCase.RateOrUnitValue = 5m;

			DoMerge(declaration);

			CombineAssertions(() =>
			{
				AssertEquals("CF_ChargeType", RateTypes.Cofins, entryLineFees.CF_ChargeType);
				AssertEquals("CF_BaseValue", 200m, entryLineFees.CF_BaseValue);
				AssertEquals("CF_Rate", 5m, entryLineFees.CF_Rate);
				AssertEquals("CF_ChargeAmount", 10m, entryLineFees.CF_ChargeAmount);
			});
		}

		public void TestCalculateIPITax()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "99999999", 50m, rateType: ChargeTypesList.Codes.DTY, rateCode: RateCodes.ImportDuty, tradeGroupCountry: Core.Constants.CountryCodes.Zimbabwe);
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "99999999", 10m, rateType: RateTypes.IPI, rateCode: RateCodes.IPI, tradeGroupCountry: Core.Constants.CountryCodes.Zimbabwe, customsValueFormula: "CV + DTY");

			var declaration = CreateTestDeclarationAndInvoiceLines(RatePreferenceType.Normal, false);
			DoMerge(declaration);

			var entryLine = declaration.ActiveEntryHeaders[0].MergedLines[0];
			var feeDTY = entryLine.Fees.GetElementWithThisCode(ChargeTypesList.Codes.DTY);
			var feeIPI = entryLine.Fees.GetElementWithThisCode(RateTypes.IPI);

			CombineAssertions(() =>
			{
				AssertEquals("Duty CF_ChargeType should be", ChargeTypesList.Codes.DTY, feeDTY.CF_ChargeType);
				AssertEquals("Duty CF_BaseValue should be", 200m, feeDTY.CF_BaseValue);
				AssertEquals("Duty CF_Rate should be", 50m, feeDTY.CF_Rate);
				AssertEquals("Duty CF_ChargeAmount should be", 100m, feeDTY.CF_ChargeAmount);
				AssertEquals("Duty CF_MethodOfCalculation should be", Constants.MethodOfCalculation.Percentage, feeDTY.CF_MethodOfCalculation);

				AssertEquals("IPI CF_ChargeType should be", RateTypes.IPI, feeIPI.CF_ChargeType);
				AssertEquals("IPI CF_BaseValue should be", 300m, feeIPI.CF_BaseValue);
				AssertEquals("IPI CF_Rate should be", 10m, feeIPI.CF_Rate);
				AssertEquals("IPI CF_ChargeAmount should be", 30m, feeIPI.CF_ChargeAmount);
				AssertEquals("IPI CF_MethodOfCalculation should be", Constants.MethodOfCalculation.Percentage, feeIPI.CF_MethodOfCalculation);
			});

			var invLine = declaration.Invoices[0].InvoiceLines[0];
			invLine.IPIRateIsOverridden = true;
			invLine.IPIVigentRateValue = 20m;

			DoMerge(declaration);

			CombineAssertions(() =>
			{
				AssertEquals("Duty CF_ChargeType should be", ChargeTypesList.Codes.DTY, feeDTY.CF_ChargeType);
				AssertEquals("Duty CF_BaseValue should be", 200m, feeDTY.CF_BaseValue);
				AssertEquals("Duty CF_Rate should be", 50m, feeDTY.CF_Rate);
				AssertEquals("Duty CF_ChargeAmount should be", 100m, feeDTY.CF_ChargeAmount);

				AssertEquals("IPI CF_ChargeType should be", RateTypes.IPI, feeIPI.CF_ChargeType);
				AssertEquals("IPI CF_BaseValue should be", 300m, feeIPI.CF_BaseValue);
				AssertEquals("IPI CF_Rate should be", 20m, feeIPI.CF_Rate);
				AssertEquals("IPI CF_ChargeAmount should be", 60m, feeIPI.CF_ChargeAmount);
			});

			invLine.IPITaxRegime = "1";

			DoMerge(declaration);

			var entryLineFees = declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.GetElementWithThisCode(RateTypes.IPI);

			CombineAssertions(() =>
			{
				AssertEquals("CF_ChargeType", RateTypes.IPI, entryLineFees.CF_ChargeType);
				AssertEquals("CF_BaseValue", 300m, entryLineFees.CF_BaseValue);
				AssertEquals("CF_Rate", 20m, entryLineFees.CF_Rate);
				AssertEquals("CF_ChargeAmount", 0m, entryLineFees.CF_ChargeAmount);
			});

			invLine.IPITaxRegime = ZString.Empty;
			invLine.IPIRateIsOverridden = false;

			var specialCase = invLine.SpecialCaseTaxes.AddNew();
			specialCase.TaxGroup = Constants.RateCodes.IPI;
			specialCase.TaxType = SpecialCaseTaxTypeList.Codes.Reduced;
			specialCase.RateOrUnitValue = 5m;

			DoMerge(declaration);

			CombineAssertions(() =>
			{
				AssertEquals("CF_ChargeType", RateTypes.IPI, entryLineFees.CF_ChargeType);
				AssertEquals("CF_BaseValue", 300m, entryLineFees.CF_BaseValue);
				AssertEquals("CF_Rate", 5m, entryLineFees.CF_Rate);
				AssertEquals("CF_ChargeAmount", 15m, entryLineFees.CF_ChargeAmount);
			});
		}

		public void TestCalculateIPITax_SpecifyBaseAmount()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "99999999", 50m, tradeGroupCountry: Core.Constants.CountryCodes.Zimbabwe);
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "99999999", 10m, rateType: RateTypes.IPI, rateCode: RateCodes.IPI, tradeGroupCountry: Core.Constants.CountryCodes.Zimbabwe, customsValueFormula: "CV + DTY");

			var declaration = CreateTestDeclarationAndInvoiceLines(RatePreferenceType.Normal, false);
			var invoiceLine = declaration.Invoices[0].InvoiceLines[0];
			invoiceLine.DutyTaxRegime = TaxRegimeList.Codes.Suspension;

			invoiceLine.JI_PrimaryPreference = RatePreferenceType.ReductionMargin;
			invoiceLine.JI_Tariff = "99999999";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Zimbabwe;
			DoMerge(declaration);

			var entryLine = declaration.ActiveEntryHeaders[0].MergedLines[0];
			var feeDTY = entryLine.Fees.GetElementWithThisCode(ChargeTypesList.Codes.DTY);
			var feeIPI = entryLine.Fees.GetElementWithThisCode(RateTypes.IPI);

			CombineAssertions("DutyRateIsOverridden = false && RatePreference = ReductionMargin", () =>
			{
				AssertEquals("Duty CF_ChargeType should be", ChargeTypesList.Codes.DTY, feeDTY.CF_ChargeType);
				AssertEquals("Duty CF_BaseValue should be", 200m, feeDTY.CF_BaseValue);
				AssertEquals("Duty CF_Rate should be", 0m, feeDTY.CF_Rate);
				AssertEquals("Duty CF_ChargeAmount should be", 0m, feeDTY.CF_ChargeAmount);

				AssertEquals("IPI CF_ChargeType", RateTypes.IPI, feeIPI.CF_ChargeType);
				AssertEquals("IPI CF_BaseValue", 200m, feeIPI.CF_BaseValue);
				AssertEquals("IPI CF_Rate", 10m, feeIPI.CF_Rate);
				AssertEquals("IPI CF_ChargeAmount", 20m, feeIPI.CF_ChargeAmount);
			});

			AssertDutyAndIPIFeeWhenDutyRateIsOverridden(RatePreferenceType.ReductionMargin);

			foreach (var ratePreference in new[] { RatePreferenceType.Normal, RatePreferenceType.FreeTradeAgreement, RatePreferenceType.ExTariff, RatePreferenceType.ReducedRate })
			{
				AssertDutyAndIPIFeeWhenDutyRateIsNotOverridden(ratePreference);
				AssertDutyAndIPIFeeWhenDutyRateIsOverridden(ratePreference);
			}

			void AssertDutyAndIPIFeeWhenDutyRateIsNotOverridden(ZString ratePreference)
			{
				invoiceLine.JI_PrimaryPreference = ratePreference;
				invoiceLine.DutyRateIsOverridden = false;
				DoMerge(declaration);

				CombineAssertions("DutyRateIsOverridden = false && RatePreference = " + ratePreference, () =>
				{
					AssertEquals("Duty CF_ChargeType", ChargeTypesList.Codes.DTY, feeDTY.CF_ChargeType);
					AssertEquals("Duty CF_BaseValue", 200m, feeDTY.CF_BaseValue);
					AssertEquals("Duty CF_Rate", ratePreference == RatePreferenceType.Normal ? 50m : 0m, feeDTY.CF_Rate);
					AssertEquals("Duty CF_ChargeAmount", ratePreference == RatePreferenceType.Normal ? 100m : 0m, feeDTY.CF_ChargeAmount);

					AssertEquals("IPI CF_ChargeType", RateTypes.IPI, feeIPI.CF_ChargeType);
					AssertEquals("IPI CF_BaseValue = Customs Value + (Customs Value * (NormalDutyRateValue / 100))", 300m, feeIPI.CF_BaseValue);
					AssertEquals("IPI CF_Rate", 10m, feeIPI.CF_Rate);
					AssertEquals("IPI CF_ChargeAmount = CF_BaseValue * CF_Rate / 100", 30m, feeIPI.CF_ChargeAmount);
				});
			}

			void AssertDutyAndIPIFeeWhenDutyRateIsOverridden(ZString ratePreference)
			{
				if (ratePreference != RatePreferenceType.Normal)
				{
					invoiceLine.DutyRateIsOverridden = true;
					switch (ratePreference)
					{
						case RatePreferenceType.FreeTradeAgreement:
							invoiceLine.FTAMarginRateValue = 20m;
							break;
						case RatePreferenceType.ReducedRate:
							invoiceLine.ReducedDutyRateValue = 40m;
							break;
						case RatePreferenceType.ReductionMargin:
							invoiceLine.ReductionMarginRateValue = 20m;
							break;
						default:
							invoiceLine.OverriddenDutyRateValue = 40m;
							break;
					}
					DoMerge(declaration);

					CombineAssertions("DutyRateIsOverridden = true && RatePreference = " + ratePreference, () =>
					{
						AssertEquals("Duty CF_ChargeType should be", ChargeTypesList.Codes.DTY, feeDTY.CF_ChargeType);
						AssertEquals("Duty CF_BaseValue should be", 200m, feeDTY.CF_BaseValue);
						AssertEquals("Duty CF_Rate should be", 40m, feeDTY.CF_Rate);
						AssertEquals("Duty CF_ChargeAmount should be", 80m, feeDTY.CF_ChargeAmount);

						AssertEquals("IPI CF_ChargeType", RateTypes.IPI, feeIPI.CF_ChargeType);
						AssertEquals("IPI CF_BaseValue = Customs Value + (Customs Value * JLT_Rate)", 280m, feeIPI.CF_BaseValue);
						AssertEquals("IPI CF_Rate", 10m, feeIPI.CF_Rate);
						AssertEquals("IPI CF_ChargeAmount = CF_BaseValue * CF_Rate / 100", 28m, feeIPI.CF_ChargeAmount);
					});
				}
			}
		}

		public void TestCalculateDutyWithExTariff()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "99999999", 50m, tariffType: Universal.Constants.TariffTypes.HarmonizedSystem, tradeGroupCountry: Core.Constants.CountryCodes.Zimbabwe);
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "99999999_001", 40m, tariffType: ChildTariffTypeList.Codes.LEBIT, tradeGroupCountry: Core.Constants.CountryCodes.Zimbabwe);
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "99999999_002", 30m, tariffType: ChildTariffTypeList.Codes.LEBIT, tradeGroupCountry: Core.Constants.CountryCodes.Zimbabwe);

			var declaration = CreateTestDeclarationAndInvoiceLines(RatePreferenceType.ExTariff, true);
			DoMerge(declaration);

			var feeDTY = declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.GetElementWithThisCode(ChargeTypesList.Codes.DTY);
			var entryLine = declaration.ActiveEntryHeaders[0].MergedLines[0];
			CombineAssertions(() =>
			{
				AssertEquals("Duty CL_CustomsValue should be", 200m, entryLine.CL_CustomsValue);
				AssertEquals("Duty CF_ChargeType should be", ChargeTypesList.Codes.DTY, feeDTY.CF_ChargeType);
				AssertEquals("Duty CF_BaseValue should be", 200m, feeDTY.CF_BaseValue);
				AssertEquals("Duty CF_Rate should be", 40m, feeDTY.CF_Rate);
				AssertEquals("Duty CF_ChargeAmount should be", 80m, feeDTY.CF_ChargeAmount);
			});

			var invLine = declaration.Invoices[0].InvoiceLines[0];
			invLine.JI_PrimaryPreference = RatePreferenceType.Normal;

			DoMerge(declaration);

			feeDTY = declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.GetElementWithThisCode(ChargeTypesList.Codes.DTY);

			CombineAssertions(() =>
			{
				AssertEquals("Duty CF_ChargeType should be", ChargeTypesList.Codes.DTY, feeDTY.CF_ChargeType);
				AssertEquals("Duty CF_BaseValue should be", 200m, feeDTY.CF_BaseValue);
				AssertEquals("Duty CF_Rate should be", 50m, feeDTY.CF_Rate);
				AssertEquals("Duty CF_ChargeAmount should be", 100m, feeDTY.CF_ChargeAmount);
			});

			invLine.JI_PrimaryPreference = RatePreferenceType.ExTariff;
			var additionalTariff = invLine.AdditionalTariffs[0];
			additionalTariff.TariffType = ChildTariffTypeList.Codes.LEBIT;
			additionalTariff.ExNumber = "002";

			DoMerge(declaration);

			feeDTY = declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.GetElementWithThisCode(ChargeTypesList.Codes.DTY);

			CombineAssertions(() =>
			{
				AssertEquals("Duty CF_ChargeType should be", ChargeTypesList.Codes.DTY, feeDTY.CF_ChargeType);
				AssertEquals("Duty CF_BaseValue should be", 200m, feeDTY.CF_BaseValue);
				AssertEquals("Duty CF_Rate should be", 30m, feeDTY.CF_Rate);
				AssertEquals("Duty CF_ChargeAmount should be", 60m, feeDTY.CF_ChargeAmount);
			});
		}

		public void TestCalculateICMSTax()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "99999999", 50m, tariffType: Universal.Constants.TariffTypes.HarmonizedSystem, rateType: ChargeTypesList.Codes.DTY, rateCode: RateCodes.ImportDuty, tradeGroupCountry: Core.Constants.CountryCodes.Zimbabwe);
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "99999999", 10m, tariffType: Universal.Constants.TariffTypes.HarmonizedSystem, rateType: RateTypes.IPI, rateCode: RateCodes.IPI, tradeGroupCountry: Core.Constants.CountryCodes.Zimbabwe, customsValueFormula: "CV + DTY");
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "99999999", 20m, tariffType: Universal.Constants.TariffTypes.HarmonizedSystem, rateType: RateTypes.PIS, rateCode: RateCodes.PIS, tradeGroupCountry: Core.Constants.CountryCodes.Zimbabwe);
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "99999999", 15m, tariffType: Universal.Constants.TariffTypes.HarmonizedSystem, rateType: RateTypes.Cofins, rateCode: RateCodes.Cofins, tradeGroupCountry: Core.Constants.CountryCodes.Zimbabwe);

			ReferenceTestDataHelper.CreateSiscomexUsageEntryFees(Factory);

			var declaration = CreateTestDeclarationAndInvoiceLines(RatePreferenceType.Normal, false, BRJobMessageTypeList.Codes.ImportSiscomex);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;

			var entryInstruction = declaration.CustomsEntryInstructions[0];
			entryInstruction.IsAFRMMRateOverridden = true;
			entryInstruction.CEI_AFRMMRateOverride = 10m;
			entryInstruction.CEI_UtilizationFeeOverride = 15m;

			declaration.ResumeApportionment();
			DoMerge(declaration);

			var feeICMS = declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.GetElementWithThisCode(Constants.RateTypes.ICMS);

			CombineAssertions(() =>
			{
				AssertEquals("ICMS CF_ChargeType should be", Constants.RateTypes.ICMS, feeICMS.CF_ChargeType);
				AssertEquals("ICMS CF_BaseValue should be", 646.85m, feeICMS.CF_BaseValue);
				AssertEquals("ICMS CF_Rate should be", 12m, feeICMS.CF_Rate);
				AssertEquals("ICMS CF_ChargeAmount should be", 77.62m, feeICMS.CF_ChargeAmount);
				AssertEquals("ICMS CF_MethodOfCalculation should be", Constants.MethodOfCalculation.Percentage, feeICMS.CF_MethodOfCalculation);
			});

			var invoiceLine = declaration.Invoices[0].InvoiceLines[0];
			invoiceLine.JI_ICMSRate = 0m;
			DoMerge(declaration);

			Assert("ICMS CF_ChargeType should be deleted", feeICMS.IsDeleted);
		}

		public void TestCalculateAfrmmEntryFee_IMP()
		{
			AssertCalculateAfrmmEntryFee(BRJobMessageTypeList.Codes.Import);
		}

		public void TestCalculateAfrmmEntryFee_ISW()
		{
			AssertCalculateAfrmmEntryFee(BRJobMessageTypeList.Codes.ImportSiscomex);
		}

		void AssertCalculateAfrmmEntryFee(string messageType)
		{
			ReferenceTestDataHelper.CreateAfrmmTaxes(Factory);

			var declaration = CreateTestDeclarationAndInvoiceLines(RatePreferenceType.Normal, false, messageType);

			foreach (var transportMode in new[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.Lake, TransportTypeList.Codes.River })
			{
				declaration.JE_TransportMode = transportMode;

				var entryInstruction = declaration.CustomsEntryInstructions[0];
				entryInstruction.CEI_AFRMMMethodOfCalculation = ZString.Empty;
				entryInstruction.IsAFRMMRateOverridden = true;
				entryInstruction.CEI_AFRMMRateOverride = 10m;
				entryInstruction.CEI_UtilizationFeeOverride = 20m;

				declaration.ResumeApportionment();
				DoMerge(declaration);
				var feeFMM = declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.GetElementWithThisCode(RefCusTaxOrFee.Types.AfrmmTax);
				AssertEquals($"When Transport Mode = {transportMode}, FMM should be", 20m, feeFMM.CF_ChargeAmount);

				entryInstruction.IsAFRMMRateOverridden = false;
				entryInstruction.CEI_AFRMMMethodOfCalculation = "FMM1";
				declaration.ResumeApportionment();
				DoMerge(declaration);
				AssertNotEquals($"When Transport Mode = {transportMode}, FMM should NOT be null", null, declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.GetElementWithThisCode(RefCusTaxOrFee.Types.AfrmmTax));

				entryInstruction.CEI_AFRMMMethodOfCalculation = ZString.Empty;
				declaration.ResumeApportionment();
				DoMerge(declaration);
				AssertEquals($"When Transport Mode = {transportMode}, FMM should be null", null, declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.GetElementWithThisCode(RefCusTaxOrFee.Types.AfrmmTax));
			}

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.ResumeApportionment();
			DoMerge(declaration);
			AssertEquals("feeFMM should be null", null, declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.GetElementWithThisCode(RefCusTaxOrFee.Types.AfrmmTax));
		}

		public void TestCalculateSiscomexUsageFeeEntryFee_ISW()
		{
			ReferenceTestDataHelper.CreateSiscomexUsageEntryFees(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var instructions = declaration.CustomsEntryInstructions.AddNew();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 1800m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_NoOfPacks = 55;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instructions.PK;
			invoiceLine1.JI_Tariff = "1";
			invoiceLine1.JI_LinePrice = 600m;
			invoiceLine1.JI_NetWeight = 100m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instructions.PK;
			invoiceLine2.JI_Tariff = "1";
			invoiceLine2.JI_LinePrice = 600m;
			invoiceLine2.JI_NetWeight = 100m;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = instructions.PK;
			invoiceLine3.JI_Tariff = "1";
			invoiceLine3.JI_LinePrice = 600m;
			invoiceLine3.JI_NetWeight = 100m;

			declaration.ResumeApportionment();
			DoMerge(declaration);

			var formalEntry = declaration.ActiveEntryHeaders.FormalEntries.FirstOrDefault();
			var supEntry = declaration.ActiveEntryHeaders.SiscomexUsageFeeEntries.FirstOrDefault();

			CombineAssertions(() =>
			{
				AssertEquals("First Line Total fee", 74.54m, formalEntry.MergedLines[0].Fees.GetAmount(RefCusTaxOrFee.Types.SiscomexUsageEntryFee));
				AssertEquals("Second Line Total fee", 74.55m, formalEntry.MergedLines[1].Fees.GetAmount(RefCusTaxOrFee.Types.SiscomexUsageEntryFee));
				AssertEquals("Third Line Total fee", 74.55m, formalEntry.MergedLines[2].Fees.GetAmount(RefCusTaxOrFee.Types.SiscomexUsageEntryFee));
				AssertEquals("Total SUF Fee", 223.64m, formalEntry.MergedLines.Sum(t => t.Fees.GetAmount(RefCusTaxOrFee.Types.SiscomexUsageEntryFee)));
			});
		}

		public void TestCalculationSiscomexUsageFeeEntryFee_IMP()
		{
			ReferenceTestDataHelper.CreateSiscomexUsageEntryFees(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var instructions = declaration.CustomsEntryInstructions.AddNew();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 1800m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_NoOfPacks = 55;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instructions.PK;
			invoiceLine1.JI_Tariff = "1";
			invoiceLine1.JI_LinePrice = 550m;
			invoiceLine1.JI_NetWeight = 100m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instructions.PK;
			invoiceLine2.JI_Tariff = "1";
			invoiceLine2.JI_LinePrice = 250m;
			invoiceLine2.JI_NetWeight = 100m;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = instructions.PK;
			invoiceLine3.JI_Tariff = "1";
			invoiceLine3.JI_LinePrice = 1000m;
			invoiceLine3.JI_NetWeight = 100m;
			var fee = invoiceLine3.Charges.AddNew(ImportCommonChargesProvider.OverseasFreightPrepaid.Code);
			fee.J7_Amount = 30m;
			fee.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Brazil;

			declaration.ResumeApportionment();
			DoMerge(declaration);

			var formalEntry = declaration.ActiveEntryHeaders.FormalEntries.FirstOrDefault();
			var supEntry = declaration.ActiveEntryHeaders.SiscomexUsageFeeEntries.FirstOrDefault();

			CombineAssertions(() =>
			{
				AssertEquals("First Line Total fee", 46.35m, formalEntry.MergedLines[0].Fees.GetAmount(RefCusTaxOrFee.Types.SiscomexUsageEntryFee));
				AssertEquals("Second Line Total fee", 21.07m, formalEntry.MergedLines[1].Fees.GetAmount(RefCusTaxOrFee.Types.SiscomexUsageEntryFee));
				AssertEquals("Third Line Total fee", 86.81m, formalEntry.MergedLines[2].Fees.GetAmount(RefCusTaxOrFee.Types.SiscomexUsageEntryFee));
				AssertEquals("Total SUF Fee", 154.23m, formalEntry.MergedLines.Sum(t => t.Fees.GetAmount(RefCusTaxOrFee.Types.SiscomexUsageEntryFee)));

				AssertEquals("SUF Entry Header - First Line SUF fee", 154.23m, supEntry.MergedLines.FirstOrDefault().Fees.GetAmount(RefCusTaxOrFee.Types.SiscomexUsageEntryFee));
			});
		}

		public override void TestCalculateDuties()
		{
			SetupReferenceData();
			var declaration = Declaration;
			var entryLine1 = GetEntryHeader(declaration, TariffCode_0101100000).MergedLines[0];
			GetDutyCalculatorStrategy().CalculateDuties();
			CombineAssertions(() =>
			{
				var fee1 = entryLine1.Fees[0];
				AssertEquals("entryLine1: CF_ChargeAmount", 17.60m, fee1.CF_ChargeAmount);
				AssertEquals("entryLine1: EntryLineFeeType", ExpectedEntryLineFeeType, fee1.CF_ChargeType);
				AssertEquals("CF_BaseValue should be equal to 22m", 22m, entryLine1.Fees[0].CF_BaseValue);
				AssertEquals("CF_Rate should be equal to EntryLineUniversalRate.ValueForDuty", 80m, entryLine1.Fees[0].CF_Rate);
				AssertEquals("CF_MethodOfCalculation should be equal to", MethodOfCalculation.Percentage, entryLine1.Fees[0].CF_MethodOfCalculation);
			});

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Brazil, ProcedureCategories.Duty, MessageSubTypeList.Codes._01, "1", ZString.Empty, ZString.Empty, Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex, false);
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "99999999", 15m, tariffType: Universal.Constants.TariffTypes.HarmonizedSystem, rateType: ProcedureCategories.Duty, rateCode: RateCodes.ImportDuty, tradeGroupCountry: Core.Constants.CountryCodes.Brazil);

			#region Import Siscomex
			declaration = CreateTestDeclarationAndInvoiceLines(RatePreferenceType.Normal, false);

			DoMerge(declaration as JobDeclaration);

			var entryLineFees = declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.GetElementWithThisCode(ProcedureCategories.Duty);

			CombineAssertions(() =>
			{
				AssertEquals("CF_ChargeType", ProcedureCategories.Duty, entryLineFees.CF_ChargeType);
				AssertEquals("CF_BaseValue", 200m, entryLineFees.CF_BaseValue);
				AssertEquals("CF_Rate", 0m, entryLineFees.CF_Rate);
				AssertEquals("CF_ChargeAmount", 0m, entryLineFees.CF_ChargeAmount);
			});
			#endregion

			#region Import
			declaration = CreateTestDeclarationAndInvoiceLines(RatePreferenceType.Normal, false, BRJobMessageTypeList.Codes.Import);

			DoMerge(declaration as JobDeclaration);

			entryLineFees = declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.GetElementWithThisCode(ProcedureCategories.Duty);

			var entryLineFeesSUF = declaration.ActiveEntryHeaders[1].MergedLines[0].Fees;
			CombineAssertions(() =>
			{
				AssertEquals("CF_ChargeType", ProcedureCategories.Duty, entryLineFees.CF_ChargeType);
				AssertEquals("CF_BaseValue", 200m, entryLineFees.CF_BaseValue);
				AssertEquals("CF_Rate", 0m, entryLineFees.CF_Rate);
				AssertEquals("CF_ChargeAmount", 0m, entryLineFees.CF_ChargeAmount);

				AssertNull(entryLineFeesSUF.GetElementWithThisCode(ProcedureCategories.Duty));
			});
			#endregion
		}

		protected override BaseJobDeclaration GetDeclaration()
		{
			var declaration = base.GetDeclaration();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.Invoices[0].InvoiceLines[0].JI_PrimaryPreference = RatePreferenceType.Normal;
			declaration.Invoices[1].InvoiceLines[0].JI_PrimaryPreference = RatePreferenceType.Normal;
			return declaration;
		}

		protected override DutyCalculatorStrategy GetDutyCalculatorStrategy() => new DutyCalculatorStrategy((JobDeclaration)Declaration);

		protected override bool ExpectedShouldCalculateDuties => true;

		protected override ZString ExpectedEntryLineFeeType => ChargeTypesList.Codes.DTY;

		JobDeclaration CreateTestDeclarationAndInvoiceLines(ZString primaryPreference, bool addAdditionalTariff, string messageType = BRJobMessageTypeList.Codes.ImportSiscomex)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(true);
			declaration.JE_MessageType = messageType;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var freightPrepaid = invoice.Charges.AddNew(ImportCommonChargesProvider.OverseasFreightPrepaid.Code);
			freightPrepaid.J7_Amount = 30m;
			freightPrepaid.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Brazil;
			var freightCollect = invoice.Charges.AddNew(ImportCommonChargesProvider.OverseasFreightCollect.Code);
			freightCollect.J7_Amount = 20m;
			freightCollect.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Brazil;
			var freightComponents = invoice.Charges.AddNew(ImportChargesProvider.FreightComponents.Code);
			freightComponents.J7_Amount = 10m;
			freightComponents.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Brazil;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invLine = invoice.InvoiceLines.AddNew();
			invLine.JI_CEI = entryInstruction.PK;
			invLine.JI_Tariff = "1";
			invLine.JI_LinePrice = 1000m;
			invLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Zimbabwe;
			invLine.JI_Tariff = "99999999";
			invLine.JI_PrimaryPreference = primaryPreference;
			invLine.JI_ICMSRate = 12m;
			invLine.ICMSTaxRegime = ICMSTaxRegimeList.Codes.FullCollection;
			invLine.AdditionalTariffs.RemoveAndDeleteAll();

			if (addAdditionalTariff)
			{
				var additionalTariff = invLine.AdditionalTariffs.AddNew();
				additionalTariff.TariffType = ChildTariffTypeList.Codes.LEBIT;
				additionalTariff.ExNumber = "001";
			}

			return declaration;
		}

		public void TestCalculateImportLicenseFineEntryFee()
		{
			CalculateImportLicenseFineEntryFee(BRJobMessageTypeList.Codes.ImportSiscomex);
			CalculateImportLicenseFineEntryFee(BRJobMessageTypeList.Codes.Import);
		}

		public void CalculateImportLicenseFineEntryFee(ZString messageType)
		{
			ReferenceTestDataHelper.CreateReferenceDataForILFFeeTypeList(Factory);
			var date = ZDateTime.Now;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			declaration.JE_MessageType = messageType;
			declaration.JE_ExportDate = date.AddDays(-5);

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "TEST_ENTRY";
			entryInstruction.IsAFRMMRateOverridden = true;
			entryInstruction.CEI_AFRMMRateOverride = 10m;
			entryInstruction.CEI_UtilizationFeeOverride = 10m;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 600m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_NoOfPacks = 55;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1";
			invoiceLine.JI_LinePrice = 600m;
			invoiceLine.JI_NetWeight = 100m;
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.ImportLicenseNumber = "123";
			invoiceLine.ImportLicenseFeeType = "F1ND";

			invoiceLine.ImportLicenseAuthorizationDate = date;
			invoiceLine.ImportLicenseType = ImportLicenseType.Codes.PostBoarding;
			declaration.ResumeApportionment();
			DoMerge(declaration);

			var cusEntryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.First();
			var fees = cusEntryHeader.MergedLines[0].Fees;
			AssertNull("Must NOT contain F1ND fee when ImportLicenseType different then 1", fees.GetElementWithThisCode(RefCusTaxOrFee.Codes.NoDiscountCode));
			AssertNull("Must NOT contain F1D5 fee when ImportLicenseType different then 1", fees.GetElementWithThisCode(RefCusTaxOrFee.Codes.FiftyPercentDiscountCode));

			invoiceLine.ImportLicenseType = ImportLicenseType.Codes.PreBoarding;
			invoiceLine.ImportLicenseAuthorizationDate = date.AddDays(-10);
			DoMerge(declaration);
			AssertNull("Must NOT contain F1ND fee when ImportLicenseAuthorizationDate lower than Departure Date (JE_ExportDate)", fees.GetElementWithThisCode(RefCusTaxOrFee.Codes.NoDiscountCode));
			AssertNull("Must NOT contain F1D5 fee when ImportLicenseAuthorizationDate lower than Departure Date (JE_ExportDate)", fees.GetElementWithThisCode(RefCusTaxOrFee.Codes.FiftyPercentDiscountCode));

			invoiceLine.ImportLicenseAuthorizationDate = ZDateTime.Invalid;
			DoMerge(declaration);
			AssertNull("Must NOT contain F1ND fee when ImportLicenseAuthorizationDate is Invalid", fees.GetElementWithThisCode(RefCusTaxOrFee.Codes.NoDiscountCode));
			AssertNull("Must NOT contain F1D5 fee when ImportLicenseAuthorizationDate is Invalid", fees.GetElementWithThisCode(RefCusTaxOrFee.Codes.FiftyPercentDiscountCode));

			invoiceLine.ImportLicenseAuthorizationDate = date;
			declaration.JE_ExportDate = ZDateTime.Invalid;
			DoMerge(declaration);
			AssertNull("Must NOT contain F1ND fee when JE_ExportDate is Invalid", fees.GetElementWithThisCode(RefCusTaxOrFee.Codes.NoDiscountCode));
			AssertNull("Must NOT contain F1D5 fee when JE_ExportDate is Invalid", fees.GetElementWithThisCode(RefCusTaxOrFee.Codes.FiftyPercentDiscountCode));

			declaration.JE_ExportDate = date.AddDays(-5);
			declaration.ResumeApportionment();
			DoMerge(declaration);
			AssertNotNull("Must contain F1ND fee when ImportLicenseFeeType = F1ND", fees.GetElementWithThisCode(RefCusTaxOrFee.Codes.NoDiscountCode));
			AssertNull("Must NOT contain F1D5 fee when ImportLicenseFeeType = F1ND", fees.GetElementWithThisCode(RefCusTaxOrFee.Codes.FiftyPercentDiscountCode));

			invoiceLine.ImportLicenseFeeType = "F1D5";
			declaration.ResumeApportionment();
			DoMerge(declaration);
			AssertNull("Must NOT contain F1ND fee when ImportLicenseFeeType = F1D5", fees.GetElementWithThisCode(RefCusTaxOrFee.Codes.NoDiscountCode));
			AssertNotNull("Must contain F1D5 fee when ImportLicenseFeeType = F1D5", fees.GetElementWithThisCode(RefCusTaxOrFee.Codes.FiftyPercentDiscountCode));
		}

		public void TestCalculateAntidumpingDuty()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "99999999", 20m, tariffType: Universal.Constants.TariffTypes.HarmonizedSystem, rateType: RateTypes.Antidumping, rateCode: RateCodes.Antidumping, tradeGroupCountry: Core.Constants.CountryCodes.Zimbabwe);

			var declaration = CreateTestDeclarationAndInvoiceLines(RatePreferenceType.Normal, false);

			DoMerge(declaration);

			var entryLineFee = declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.GetElementWithThisCode(Constants.RateTypes.Antidumping);

			CombineAssertions(() =>
			{
				AssertEquals("CF_ChargeType", RateTypes.Antidumping, entryLineFee.CF_ChargeType);
				AssertEquals("CF_BaseValue", 200m, entryLineFee.CF_BaseValue);
				AssertEquals("CF_Rate", 20m, entryLineFee.CF_Rate);
				AssertEquals("CF_ChargeAmount", 40m, entryLineFee.CF_ChargeAmount);
			});

			var invLine = declaration.Invoices[0].InvoiceLines[0];
			var antidumping = invLine.Taxes.AddNew();
			antidumping.JLT_Type = Constants.RateCodes.Antidumping;
			antidumping.JLT_Rate = 10m;

			DoMerge(declaration);

			CombineAssertions(() =>
			{
				AssertEquals("CF_ChargeType", RateTypes.Antidumping, entryLineFee.CF_ChargeType);
				AssertEquals("CF_BaseValue", 200m, entryLineFee.CF_BaseValue);
				AssertEquals("CF_Rate", 10m, entryLineFee.CF_Rate);
				AssertEquals("CF_ChargeAmount", 20m, entryLineFee.CF_ChargeAmount);
			});

			antidumping.JLT_MethodOfCalculation = SpecialCaseTaxTypeList.Codes.AdValoremRate;
			antidumping.JLT_Rate = 5m;

			DoMerge(declaration);

			CombineAssertions(() =>
			{
				AssertEquals("CF_ChargeType", RateTypes.Antidumping, entryLineFee.CF_ChargeType);
				AssertEquals("CF_BaseValue", 200m, entryLineFee.CF_BaseValue);
				AssertEquals("CF_Rate", 5m, entryLineFee.CF_Rate);
				AssertEquals("CF_ChargeAmount", 10m, entryLineFee.CF_ChargeAmount);
			});
		}

		public void TestCalculateOtherExpensesICMSEntryFee()
		{
			CalculateOtherExpensesICMSEntryFee(BRJobMessageTypeList.Codes.ImportSiscomex);
			CalculateOtherExpensesICMSEntryFee(BRJobMessageTypeList.Codes.Import);
		}

		public void CalculateOtherExpensesICMSEntryFee(ZString messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_MessageType = messageType;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 600m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var charge = invoice.Charges.AddNew(ImportChargesProvider.OtherExpensesICMS.Code);
			charge.J7_Amount = 1000m;
			charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Brazil;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1";
			invoiceLine.JI_LinePrice = 600m;
			invoiceLine.JI_NetWeight = 100m;

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var cusEntryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault();

			var cusEntryLine = cusEntryHeader.MergedLines[0];

			AssertEquals("EIC Fee should be", 1000m, cusEntryLine.Fees.GetAmount(Constants.RateTypes.OtherExpensesICMS));
		}

		public void TestCalculateDutyAndTaxBySpecialRate()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "99999999", 10m, tariffType: Universal.Constants.TariffTypes.HarmonizedSystem, rateType: RateTypes.IPI, rateCode: RateCodes.IPI);

			var declaration = CreateTestDeclarationAndInvoiceLines(RatePreferenceType.Normal, false);
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.Invoices.AddNew().InvoiceLines.AddNew().JI_CEI = entryInstruction.PK;
			declaration.Invoices.AddNew().InvoiceLines.AddNew().JI_CEI = entryInstruction.PK;

			foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
			{
				var specialCase = invoiceLine.SpecialCaseTaxes.AddNew();
				specialCase.TaxGroup = Constants.RateCodes.IPI;
				specialCase.TaxType = SpecialCaseTaxTypeList.Codes.QuantityPerUnit;
				specialCase.RateOrUnitValue = 7m;
				specialCase.CurrencyCode = "BRL";
				specialCase.Quantity = 1000;
				specialCase.UnitOfMeasure = "KG";
			}

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.ResumeApportionment();
			DoMerge(declaration);

			foreach (var entryLine in declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().SelectMany(x => x.MergedLines))
			{
				var feeIPI = entryLine.Fees.GetElementWithThisCode(Constants.RateTypes.IPI);
				CombineAssertions(() =>
				{
					AssertEquals("IPI CF_ChargeType should be", Constants.RateTypes.IPI, feeIPI.CF_ChargeType);
					AssertEquals("IPI CF_BaseValue should be", 1000m, feeIPI.CF_BaseValue);
					AssertEquals("IPI CF_Rate should be", 7m, feeIPI.CF_Rate);
					AssertEquals("IPI CF_ChargeAmount should be", 7000m, feeIPI.CF_ChargeAmount);
					AssertEquals("IPI CF_MethodOfCalculation should be", SpecialCaseTaxTypeList.Codes.QuantityPerUnit, feeIPI.CF_MethodOfCalculation);
				});
			}
		}

		void DoMerge(JobDeclaration declaration) => new LineMerger(declaration).DoMerge();

		protected override void SetUp()
		{
			base.SetUp();

			var usdCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);
			var rate = usdCurrency.ExchangeRates.AddNew();
			rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			rate.RE_StartDate = ZDateTime.Now.AddDays(-2);
			rate.RE_ExpiryDate = ZDateTime.Now.AddDays(2);
			rate.RE_SellRate = 5;
			rate.RE_GC = GlbCompany.CurrentCompany.PK;
		}
	}
}
