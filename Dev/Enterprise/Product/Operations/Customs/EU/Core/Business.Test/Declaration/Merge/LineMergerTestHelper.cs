using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static NUnit.Framework.Assertion;
using CusEntryLine = Enterprise.Customs.EU.Business.Declaration.CusEntryLine;
using CusEntryLineFee = Enterprise.Customs.EU.Business.Declaration.CusEntryLineFee;

namespace Enterprise.Customs.EU.Business.Testing
{
	public class LineMergerTestHelper
	{
		public LineMergerTestHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		UniversalReferenceTestDataHelper RefDataHelper => refDataHelper ?? (refDataHelper = new UniversalReferenceTestDataHelper(factory));
		UniversalReferenceTestDataHelper refDataHelper;

		public void CreateDataGrouping()
		{
			var eunDataGrouping = RefDataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			RefDataHelper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, parent: eunDataGrouping);
			factory.Save();
		}

		public void PopulateTestReferenceDataAndSave(ZString dtyTariffCode, ZString stdPreferenceCode, bool addProcedure = false)
		{
			var configurationBuilder = DutyReferenceDataConfigurationBuilder.New(factory)
				.AddPreferences(stdPreferenceCode)
				.AddTaxOrFee("ORD", taxRate: 0.22m);
			factory.Save();

			configurationBuilder.AddTariffType(Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff)
				.AddTariff(dtyTariffCode, taxOrFeeCode: "DTY")
				.AddRateCode(RateTypeEnum.Duty, rateCode: RateCode.A00RateCode, rateFormula: "#ADSZ(1)# + 0.12 * [KGM] + 0.2 * VFD", preference: "100", "7000", "7200")
				.AddRateCode(RateTypeEnum.Duty, rateCode: RateCode.A00RateCode, rateFormula: "0.5 * VFD", preference: "8000")
				.AddRateCode(RateTypeEnum.Duty, rateCode: RateCode.A20RateCode, rateFormula: "0.3 * [KGM] + #EA(2)#", preference: "", "7000", "7200")
				.AddRateCode(RateTypeEnum.Antidumping, rateCode: RateCode.A30RateCode, rateFormula: "0.5 * [DTN]", preference: "", "7000", "7200")
				.AddRateCode(RateTypeEnum.CounterVailing, rateCode: RateCode.A40RateCode, rateFormula: "0.3 * VFD + 0.06 * [KGM]", preference: "", "7000", "7200")
				.AddRateCode(RateTypeEnum.Duty, rateCode: "XXX", rateFormula: "1", preference: "", "7000", "7200")
				.AddRateCode(RateTypeEnum.Duty, rateCode: "YYY", rateFormula: "1", preference: "", "7000", "7200");

			var meuTariffTypeBuilder = configurationBuilder.AddTariffType(Customs.Business.UniversalReferenceConstants.CusTariffTypes.MeursingTariff);
			meuTariffTypeBuilder.AddTariff("7000")
				.AddRateCode(RateTypeEnum.Duty, rateCode: UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyOnSugarContents, "5 * [HLT]", preference: "", "1");
			meuTariffTypeBuilder.AddTariff("7200")
				.AddRateCode(RateTypeEnum.Duty, rateCode: UniversalReferenceConstants.RefCusRateCodes.AgriculturalComponent, "0.2 * [LTR]", preference: "", "2");

			if (addProcedure)
			{
				var helper = new UniversalReferenceTestDataHelper(factory);
				var procedure1 = helper.CreateOrFindExistingRefCusProcedure(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "IM", "A", "", "   ", "Procedure A", "IMP", "10P");
				procedure1.ZZ6_CalculateDuty = true;
				var procedure2 = helper.CreateOrFindExistingRefCusProcedure(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "IM", "B", "", "   ", "Procedure B", "IMP", "10P");
				procedure2.ZZ6_CalculateDuty = false;
			}

			configurationBuilder.Configure();

			factory.Save();
		}

		public JobDeclaration CreateTestDeclarationAndInvoiceLines(ZString dtyTariffCode, ZString stdPreferenceCode, string vatableAdditionChargeCode, string taxType = "")
		{
			var declaration = GetJobDeclaration();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "EUR";
			var invLine1 = invoice1.InvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction.PK;
			invLine1.JI_Procedure = "A";
			invLine1.JI_PrimaryPreference = stdPreferenceCode;
			invLine1.JI_Tariff = dtyTariffCode;
			invLine1.JI_SupplementaryCode1 = "7000";
			invLine1.JI_CustomsQuantity = 5;
			invLine1.JI_CustomsUnitQty = "DTN";
			invLine1.JI_CustomsSecondQuantity = 300;
			invLine1.JI_CustomsSecondUnitQty = "KGM";
			invLine1.JI_CustomsThirdQuantity = 0.1;
			invLine1.JI_CustomsThirdUnitQty = "KLT";
			invLine1.JI_ZZF_NKTaxType = taxType;
			invLine1.JI_LinePrice = 50;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = "EUR";
			var invLine2 = invoice2.InvoiceLines.AddNew();
			invLine2.JI_CEI = entryInstruction.PK;
			invLine2.JI_Procedure = "A";
			invLine2.JI_PrimaryPreference = stdPreferenceCode;
			invLine2.JI_Tariff = dtyTariffCode;
			invLine2.JI_SupplementaryCode1 = "7000";
			invLine2.JI_CustomsQuantity = 2;
			invLine2.JI_CustomsUnitQty = "DTN";
			invLine2.JI_CustomsSecondQuantity = 200;
			invLine2.JI_CustomsSecondUnitQty = "KGM";
			invLine2.JI_CustomsThirdQuantity = 0.2;
			invLine2.JI_CustomsThirdUnitQty = "KLT";
			invLine2.JI_ZZF_NKTaxType = taxType;
			invLine2.JI_LinePrice = 20;

			var invoice3 = declaration.Invoices.AddNew();
			invoice3.JZ_RX_NKInvoice_Currency = "EUR";
			var invLine3 = invoice3.InvoiceLines.AddNew();
			invLine3.JI_CEI = entryInstruction.PK;
			invLine3.JI_Procedure = "B";
			invLine3.JI_PrimaryPreference = stdPreferenceCode;
			invLine3.JI_Tariff = dtyTariffCode;
			invLine3.JI_SupplementaryCode1 = "7200";
			invLine3.JI_CustomsQuantity = 100;
			invLine3.JI_CustomsUnitQty = "KG";
			invLine3.JI_CustomsSecondQuantity = 3;
			invLine3.JI_CustomsSecondUnitQty = "LTR";
			invLine3.JI_ZZF_NKTaxType = taxType;
			invLine3.JI_LinePrice = 5;

			var invoice4 = declaration.Invoices.AddNew();
			invoice4.JZ_RX_NKInvoice_Currency = "EUR";
			var invLine4 = invoice4.InvoiceLines.AddNew();
			invLine4.JI_CEI = entryInstruction.PK;
			invLine4.JI_Procedure = "B";
			invLine4.JI_PrimaryPreference = stdPreferenceCode;
			invLine4.JI_Tariff = dtyTariffCode;
			invLine4.JI_SupplementaryCode1 = "7200";
			invLine4.JI_CustomsQuantity = 30;
			invLine4.JI_CustomsUnitQty = "KG";
			invLine4.JI_CustomsSecondQuantity = 1;
			invLine4.JI_CustomsSecondUnitQty = "LTR";
			invLine4.JI_ZZF_NKTaxType = taxType;
			invLine4.JI_LinePrice = 3;
			var packingCost = invLine4.Charges.AddNew(vatableAdditionChargeCode, 10);
			packingCost.J7_IsGSTApplicable = true;

			return declaration;
		}

		public static CusEntryLineFee GetSystemVatFee(CusEntryLine entryLine) => entryLine.Fees.Cast<CusEntryLineFee>().SingleOrDefault(x => x.CF_ChargeType == RateCode.VatRateCode && x.CF_RateOverrideReasonCode == ZString.Empty);
		static IEnumerable<CusEntryLineFee> GetVatFees(CusEntryLine entryLine) => entryLine.Fees.Cast<CusEntryLineFee>().Where(x => x.CF_ChargeType == RateCode.VatRateCode);

		public static void AssertEntryLineFees(CusEntryLine entryLine, IReadOnlyList<FeeAssertionObject> expectedFees, int expectedChargeAmountDecimalPlaces = 4)
		{
			var entryLineFees = entryLine.Fees;
			AssertEntryLineFees(entryLine.ProcedureCode, "All", entryLineFees.Cast<CusEntryLineFee>(), expectedFees, expectedChargeAmountDecimalPlaces);
		}

		static void AssertEntryLineFees(ZString entryLineProcedure, ZString feeType, IEnumerable<CusEntryLineFee> actualFees, IReadOnlyList<FeeAssertionObject> expectedFees, int expectedChargeAmountDecimalPlaces = 4)
		{
			var assertionMsgPrefix = $"Entry Line - Procedure {entryLineProcedure} - {feeType} Fees";
			var orderedActualFees = actualFees.OrderBy(x => x.CF_RateOverrideReasonCode + "." + x.CF_ChargeType + "." + x.CF_MethodOfCalculation).ToArray();
			var orderedExpectedFees = expectedFees.OrderBy(x => x.OverrideReason + "." + x.ChargeType + "." + x.MethodOfCalculation).ToArray();
			var shortestArrayLength = Math.Min(orderedExpectedFees.Length, orderedActualFees.Length);
			var longestArrayLength = Math.Max(orderedExpectedFees.Length, orderedActualFees.Length);

			Assertion.CombineAssertions(assertionMsgPrefix, () =>
			{
				AssertEquals("Count", expectedFees.Count, actualFees.Count());

				for (int i = 0; i < shortestArrayLength; i++)
				{
					var orderedExpectedFee = orderedExpectedFees[i];
					var orderedActualFee = orderedActualFees[i];

					var feeId = $"Fee ({orderedActualFee.CF_RateOverrideReasonCode}.{orderedActualFee.CF_ChargeType}.{orderedActualFee.CF_MethodOfCalculation})";

					AssertEquals($"{feeId} CF_ChargeType", orderedExpectedFee.ChargeType, orderedActualFee.CF_ChargeType);
					AssertEquals($"{feeId} CF_ChargeAmount", GetExpectedChargeAmount(orderedExpectedFee, orderedActualFee.ChargeAmountRounder), orderedActualFee.CF_ChargeAmount);
					AssertEquals($"{feeId} CF_BaseValue", orderedExpectedFee.BaseValue, orderedActualFees[i].CF_BaseValue);
					AssertEquals($"{feeId} CF_Rate", orderedExpectedFee.Rate, orderedActualFees[i].CF_Rate);
					AssertEquals($"{feeId} CF_MethodOfCalculation", orderedExpectedFee.MethodOfCalculation, orderedActualFees[i].CF_MethodOfCalculation);
					AssertEquals($"{feeId} CF_RateOverrideReasonCode", orderedExpectedFee.OverrideReason, orderedActualFee.CF_RateOverrideReasonCode);

					AssertEquals($"{feeId} G4_Type", orderedExpectedFee.ChargeType, orderedActualFee.G4_Type);
					AssertEquals($"{feeId} G4_BaseAmount", orderedExpectedFee.BaseValue, orderedActualFee.G4_BaseAmount);
					AssertEquals($"{feeId} G4_RateDuty", orderedExpectedFee.MethodOfCalculation, orderedActualFee.G4_RateDuty);
					AssertEquals($"{feeId} G4_RateOverride", orderedExpectedFee.OverrideReason, orderedActualFee.G4_RateOverride);
				}

				const string unmatchingFeeMsg = "{0} (Type [{1}], Amount [{2}], BaseValue [{3}], Rate [{4}], MethodOfCalculation [{5}], OverrideReason [{6}])";

				for (int i = shortestArrayLength; i < longestArrayLength; i++)
				{
					var unmatchingFeeInfo = (orderedExpectedFees.Length > shortestArrayLength)
						? string.Format(unmatchingFeeMsg, "Expected fee not found", orderedExpectedFees[i].ChargeType, orderedExpectedFees[i].ChargeAmount, orderedExpectedFees[i].BaseValue, orderedExpectedFees[i].Rate, orderedExpectedFees[i].MethodOfCalculation, orderedExpectedFees[i].OverrideReason)
						: string.Format(unmatchingFeeMsg, "Unexpected fee found", orderedActualFees[i].CF_ChargeType, orderedActualFees[i].CF_ChargeAmount, orderedActualFees[i].CF_BaseValue, orderedActualFees[i].CF_Rate, orderedActualFees[i].CF_MethodOfCalculation, orderedActualFees[i].CF_RateOverrideReasonCode);
					Fail(unmatchingFeeInfo);
				}
			});

			ZDecimal GetExpectedChargeAmount(FeeAssertionObject expectedFee, IFeeRounder rounder) => rounder.Round(expectedFee.ChargeAmount);
		}

		public static void AssertSystemVatEntryLineFee(CusEntryLine entryLine, ZDecimal chargeAmount, ZDecimal baseValue, ZString methodOfCalculation, ZDecimal rate, int expectedChargeAmountDecimalPlaces = 4)
		{
			var systemVatFee = GetSystemVatFee(entryLine);

			AssertEntryLineFees(
				entryLine.ProcedureCode,
				RateCode.VatRateCode,
				new CusEntryLineFee[] { systemVatFee },
				[new FeeAssertionObject() { ChargeType = RateCode.VatRateCode, ChargeAmount = chargeAmount, BaseValue = baseValue, Rate = rate, MethodOfCalculation = methodOfCalculation, OverrideReason = "" }],
				expectedChargeAmountDecimalPlaces
			);
		}

		public static void AssertVatEntryLineFees(CusEntryLine entryLine, int expectedChargeAmountDecimalPlaces = 4, params FeeAssertionObject[] expectedVatFees)
		{
			var vatFees = GetVatFees(entryLine);
			AssertEntryLineFees(entryLine.ProcedureCode, RateCode.VatRateCode, vatFees, expectedVatFees, expectedChargeAmountDecimalPlaces);
		}

		protected virtual JobDeclaration GetJobDeclaration() => factory.New<JobDeclaration>();

		public static class RateCode
		{
			public static string A00RateCode => UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			public static string A20RateCode => UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyCountervailingSafeguardChargeVariableCharge;
			public static string A30RateCode => UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty;
			public static string A40RateCode => UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty;
			public static string VatRateCode => UniversalReferenceConstants.RefCusRateCodes.Vat;
		}
	}
}
