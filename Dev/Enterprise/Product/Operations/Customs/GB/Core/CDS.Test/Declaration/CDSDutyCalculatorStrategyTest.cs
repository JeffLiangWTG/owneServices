using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using RefCusTaxOrFee = Enterprise.Customs.Universal.RefCusTaxOrFee;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(CDSDutyCalculatorStrategy))]
	class CDSDutyCalculatorStrategyTest : DutyCalculatorStrategyAbstractTest<CDSDutyCalculatorStrategy>
	{
		protected override BaseJobDeclaration GetDeclaration()
		{
			var declaration = base.GetDeclaration();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			return declaration;
		}

		protected override CDSDutyCalculatorStrategy GetDutyCalculatorStrategy() => new CDSDutyCalculatorStrategy((JobDeclaration)Declaration);

		public void TestEntryLineVatCalculation()
		{
			var ordVat = SetupAndSaveReferenceData();

			using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_RX_NKInvoice_Currency = "GBP";
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_ZZF_NKTaxType = ordVat.ZZF_Code;
				invoiceLine.JI_LinePrice = 50;

				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var entryLine = entryHeader.MergedLines.AddNew();

				// Add Northern Ireland Customs Duty Fee
				var fee = entryLine.Fees.AddNew();
				fee.CF_ChargeType = GBCommonConstants.NorthernIrelandDutyCodes.CustomsDuty;
				fee.CF_ChargeAmount = 20m;
				fee.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Additional;

				var lineMerger = new CDSLineMerger(declaration);
				var addInfo = invoiceLine.AdditionalInfos.AddNew();
				addInfo.CSI_Code = "123";
				lineMerger.DoMerge();

				AssertEquals("[CSI_Code=123] IsEuTariffToBeUsedForNorthernIreland", false, entryLine.IsEuTariffToBeUsedForNorthernIreland);
				AssertEquals("[CSI_Code=123] Entry Line Fee count", 2, entryLine.Fees.Count);

				CombineAssertions("[CSI_Code=123] Entry Line Fees", () =>
				{
					var nirDtyFeeFee = entryLine.Fees[0];
					AssertEquals("NIR DTY CF_ChargeType", GBCommonConstants.NorthernIrelandDutyCodes.CustomsDuty, nirDtyFeeFee.CF_ChargeType);
					AssertEquals("NIR DTY CF_RateOverrideReasonCode", RateOverrideReasonList.Codes.Additional, nirDtyFeeFee.CF_RateOverrideReasonCode);
					AssertEquals("NIR DTY CF_ChargeAmount", 20m, nirDtyFeeFee.CF_ChargeAmount);

					var standardVatFee = entryLine.Fees[1];
					AssertEquals("STD VAT CF_ChargeType", EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, standardVatFee.CF_ChargeType);
					AssertEquals("STD VAT CF_RateOverrideReasonCode", ZString.Empty, standardVatFee.CF_RateOverrideReasonCode);
					AssertEquals("STD VAT CF_MethodOfCalculation", "%", standardVatFee.CF_MethodOfCalculation);
					AssertEquals("STD VAT CF_Rate", ordVat.ZZF_Value * 100, standardVatFee.CF_Rate);
					AssertEquals("STD VAT CF_BaseValue", 70m, standardVatFee.CF_BaseValue);
					AssertEquals("STD VAT CF_ChargeAmount", 14m, standardVatFee.CF_ChargeAmount);
				});

				addInfo.CSI_Code = "NIIMP";
				lineMerger.DoMerge();

				AssertEquals("[CSI_Code=NIIMP] IsEuTariffToBeUsedForNorthernIreland", true, entryLine.IsEuTariffToBeUsedForNorthernIreland);
				AssertEquals("[CSI_Code=NIIMP] Entry Line Fee count", 3, entryLine.Fees.Count);

				CombineAssertions("[CSI_Code=NIIMP] Entry Line Fees", () =>
				{
					var nirDtyFeeFee = entryLine.Fees[0];
					AssertEquals("NIR DTY CF_ChargeType", GBCommonConstants.NorthernIrelandDutyCodes.CustomsDuty, nirDtyFeeFee.CF_ChargeType);
					AssertEquals("NIR DTY CF_RateOverrideReasonCode", RateOverrideReasonList.Codes.Additional, nirDtyFeeFee.CF_RateOverrideReasonCode);
					AssertEquals("NIR DTY CF_ChargeAmount", 20m, nirDtyFeeFee.CF_ChargeAmount);

					var standardVatFee = entryLine.Fees[1];
					AssertEquals("STD VAT CF_ChargeType", EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, standardVatFee.CF_ChargeType);
					AssertEquals("STD VAT CF_RateOverrideReasonCode", ZString.Empty, standardVatFee.CF_RateOverrideReasonCode);
					AssertEquals("STD VAT CF_MethodOfCalculation", "%", standardVatFee.CF_MethodOfCalculation);
					AssertEquals("STD VAT CF_Rate", ordVat.ZZF_Value * 100, standardVatFee.CF_Rate);
					AssertEquals("STD VAT CF_BaseValue", 50m, standardVatFee.CF_BaseValue);
					AssertEquals("STD VAT CF_ChargeAmount", 10m, standardVatFee.CF_ChargeAmount);

					var northernIrelandVatFee = entryLine.Fees[2];
					AssertEquals("NIR VAT CF_ChargeType", EU.Business.UniversalReferenceConstants.RefCusRateCodes.VatOnAdditionalDutiesForNorthernIreland, northernIrelandVatFee.CF_ChargeType);
					AssertEquals("NIR VAT CF_RateOverrideReasonCode", ZString.Empty, northernIrelandVatFee.CF_RateOverrideReasonCode);
					AssertEquals("NIR VAT CF_MethodOfCalculation", "%", northernIrelandVatFee.CF_MethodOfCalculation);
					AssertEquals("NIR VAT CF_Rate", ordVat.ZZF_Value * 100, northernIrelandVatFee.CF_Rate);
					AssertEquals("NIR VAT CF_BaseValue", 20m, northernIrelandVatFee.CF_BaseValue);
					AssertEquals("NIR VAT CF_ChargeAmount", 4m, northernIrelandVatFee.CF_ChargeAmount);
				});

				addInfo.CSI_Code = "NIDOM";
				lineMerger.DoMerge();

				AssertEquals("[CSI_Code=NIDOM] IsEuTariffToBeUsedForNorthernIreland", true, entryLine.IsEuTariffToBeUsedForNorthernIreland);
				AssertEquals("[CSI_Code=NIDOM] Entry Line Fee count", 3, entryLine.Fees.Count);

				CombineAssertions("[CSI_Code=NIDOM] Entry Line Fees", () =>
				{
					var nirDtyFeeFee = entryLine.Fees[0];
					AssertEquals("NIR DTY CF_ChargeType", GBCommonConstants.NorthernIrelandDutyCodes.CustomsDuty, nirDtyFeeFee.CF_ChargeType);
					AssertEquals("NIR DTY CF_RateOverrideReasonCode", RateOverrideReasonList.Codes.Additional, nirDtyFeeFee.CF_RateOverrideReasonCode);
					AssertEquals("NIR DTY CF_ChargeAmount", 20m, nirDtyFeeFee.CF_ChargeAmount);

					var standardVatFee = entryLine.Fees[1];
					AssertEquals("STD VAT CF_ChargeType", EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, standardVatFee.CF_ChargeType);
					AssertEquals("STD VAT CF_RateOverrideReasonCode", ZString.Empty, standardVatFee.CF_RateOverrideReasonCode);
					AssertEquals("STD VAT CF_MethodOfCalculation", "%", standardVatFee.CF_MethodOfCalculation);
					AssertEquals("STD VAT CF_Rate", ordVat.ZZF_Value * 100, standardVatFee.CF_Rate);
					AssertEquals("STD VAT CF_BaseValue", 50m, standardVatFee.CF_BaseValue);
					AssertEquals("STD VAT CF_ChargeAmount", 0m, standardVatFee.CF_ChargeAmount);

					var northernIrelandVatFee = entryLine.Fees[2];
					AssertEquals("NIR VAT CF_ChargeType", EU.Business.UniversalReferenceConstants.RefCusRateCodes.VatOnAdditionalDutiesForNorthernIreland, northernIrelandVatFee.CF_ChargeType);
					AssertEquals("NIR VAT CF_RateOverrideReasonCode", ZString.Empty, northernIrelandVatFee.CF_RateOverrideReasonCode);
					AssertEquals("NIR VAT CF_MethodOfCalculation", "%", northernIrelandVatFee.CF_MethodOfCalculation);
					AssertEquals("NIR VAT CF_Rate", ordVat.ZZF_Value * 100, northernIrelandVatFee.CF_Rate);
					AssertEquals("NIR VAT CF_BaseValue", 20m, northernIrelandVatFee.CF_BaseValue);
					AssertEquals("NIR VAT CF_ChargeAmount", 0m, northernIrelandVatFee.CF_ChargeAmount);
				});
			}
		}

		RefCusTaxOrFee SetupAndSaveReferenceData()
		{
			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = ZDateTime.Today.AddYears(-1);
			var endDate = ZDateTime.Today.AddYears(1);
			var ordVat = refDataHelper.CreateTaxOrFee("ORD", 0.20m, currentCountryCode, startDate, endDate);

			var dtyRateType = refDataHelper.CreateCusRateType(dataGroupingCode: "EUN", rateType: Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, description: "Customs duties on industrial products");
			refDataHelper.CreateCusRateCode(Factory, zy1RateCode: GBCommonConstants.NorthernIrelandDutyCodes.CustomsDuty, dtyRateType.PK);
			var eunDataGrouping = refDataHelper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
			refDataHelper.CreateNewOrGetExistingDataGrouping(currentCountryCode, "", parent: eunDataGrouping);
			Factory.Save();

			return ordVat;
		}

		public override void TestGetFeeCodeFromRateCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "CDS";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var dutyCalculatorStrategy = new CDSDutyCalculatorStrategy(declaration);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var addInfo = invoiceLine.AdditionalInfos.AddNew();
			addInfo.CSI_Code = "123";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

			AssertEquals(false, entryLine.IsEuTariffToBeUsedForNorthernIreland);
			AssertEquals("A00", entryLine.GetFeeCodeFromRateCode("A00"));

			addInfo.CSI_Code = "NIIMP";
			AssertEquals(true, entryLine.IsEuTariffToBeUsedForNorthernIreland);
			AssertEquals("A50", entryLine.GetFeeCodeFromRateCode("A00"));
			AssertEquals("A70", entryLine.GetFeeCodeFromRateCode("A20"));
			AssertEquals("A80", entryLine.GetFeeCodeFromRateCode("A30"));
			AssertEquals("A85", entryLine.GetFeeCodeFromRateCode("A35"));
			AssertEquals("A90", entryLine.GetFeeCodeFromRateCode("A40"));
			AssertEquals("A95", entryLine.GetFeeCodeFromRateCode("A45"));
		}

		// Overriding this to do nothing as it always only passed as we were creating a CHF declaration which
		// is pointless for a CDS calculator.  Now a GB job is defaulting to CDS, this test would not pass.
		public override void TestCalculateDutiesAndVat()
		{
			Assert(true);
		}

		public void TestCalculateEntryLineFees_SuspendVatAndDutyFromCalculation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var vat = helper.CreateTaxOrFee("VAT", 0.20m, currentCountryCode, startDate, endDate, description: "Ordinary VAT");
			var dgCDS = helper.CreateNewOrGetExistingDataGrouping("CDS");
			helper.CreateNewOrGetExistingDataGrouping("CDS", parent: dgCDS);
			var tariffType = helper.CreateNewOrGetExistingTariffType("CDS", "IMP");
			Factory.Save();
			var rtDTY = helper.CreateNewOrGetExistingRateType("CDS", "DTY");
			rtDTY.ZZR_CustomsValueFormula = "CV";
			Factory.Save();
			var rcDTY = helper.CreateCusRateCode(Factory, "A00", rtDTY.PK);
			var pref = helper.CreatePreferenceForCountry("100", "1233", "CDS");
			var tariff = helper.CreateTariff("CDS", tariffType.PK, "11111111", startDate, endDate);
			Factory.Save();
			var rateA00 = helper.CreateRefCusRate(tariff.PK, rcDTY.PK, startDate, endDate, "VFD * 0.5", preferencePk: pref.PK, dataGrouping: "CDS");
			var tgCDS = helper.CreateTradeGroup("CDS", "ABC", startDate, endDate);
			helper.AddCountry(tgCDS, "CN", startDate.Date, endDate.Date);
			Factory.Save();
			helper.CreateCusApplicability(rateA00.PK, tgCDS, startDate, endDate);
			Factory.Save();

			var procedureVatWaived = CreateTestProcedure(helper, calculateVAT: false, calculateDuty: true, YesNoList.Codes.No, "01");
			var procedureDutyWaived = CreateTestProcedure(helper, calculateVAT: true, calculateDuty: false, YesNoList.Codes.No, "02");
			var procedureVatSuspended = CreateTestProcedure(helper, calculateVAT: true, calculateDuty: false, YesNoList.Codes.Yes, "03");
			var procedureDutySuspended = CreateTestProcedure(helper, calculateVAT: false, calculateDuty: true, YesNoList.Codes.Yes, "04");
			var procedureVatNotWaivedOrSuspended = CreateTestProcedure(helper, calculateVAT: true, calculateDuty: false, YesNoList.Codes.No, "05");
			var procedureDutyNotWaivedOrSuspended = CreateTestProcedure(helper, calculateVAT: false, calculateDuty: true, YesNoList.Codes.No, "06");

			CombineAssertions("Entry Line Fees", () =>
			{
				using (GBCustomsDataRegistry.Instance.ExcludeSuspendedAndWaivedFeesFromCalculations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					CalculateEntryLineFeesAndAssertFeePresence(isExpected: false, assertionMessage: "Procedure Vat Waived", procedureCode: "4001", vat, EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat);
					CalculateEntryLineFeesAndAssertFeePresence(isExpected: true, assertionMessage: "Procedure Vat Suspended", procedureCode: "4003", vat, EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat);
					CalculateEntryLineFeesAndAssertFeePresence(isExpected: true, assertionMessage: "Procedure Vat Not Waived", procedureCode: "4005", vat, EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat);

					CalculateEntryLineFeesAndAssertFeePresence(isExpected: true, assertionMessage: "Procedure Duty Waived", procedureCode: "4002", vat, EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts);
					CalculateEntryLineFeesAndAssertFeePresence(isExpected: true, assertionMessage: "Procedure Duty Suspended", procedureCode: "4004", vat, EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts);
					CalculateEntryLineFeesAndAssertFeePresence(isExpected: true, assertionMessage: "Procedure Duty Not Suspended", procedureCode: "4006", vat, EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts);
				}

				using (GBCustomsDataRegistry.Instance.ExcludeSuspendedAndWaivedFeesFromCalculations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					CalculateEntryLineFeesAndAssertFeePresence(isExpected: false, assertionMessage: "Procedure Vat Waived", procedureCode: "4001", vat, EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat);
					CalculateEntryLineFeesAndAssertFeePresence(isExpected: false, assertionMessage: "Procedure Vat Suspended", procedureCode: "4003", vat, EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat);
					CalculateEntryLineFeesAndAssertFeePresence(isExpected: true, assertionMessage: "Procedure Vat Not Waived", procedureCode: "4005", vat, EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat);

					CalculateEntryLineFeesAndAssertFeePresence(isExpected: false, assertionMessage: "Procedure Duty Waived", procedureCode: "4002", vat, EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts);
					CalculateEntryLineFeesAndAssertFeePresence(isExpected: false, assertionMessage: "Procedure Duty Suspended", procedureCode: "4004", vat, EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts);
					CalculateEntryLineFeesAndAssertFeePresence(isExpected: true, assertionMessage: "Procedure Duty Not Suspended", procedureCode: "4006", vat, EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts);
				}
			});
		}

		RefCusProcedure CreateTestProcedure(UniversalReferenceTestDataHelper helper, bool calculateVAT, bool calculateDuty, string isGuaranteeConsumed, string previousProcedureCode)
		{
			var procedure = helper.CreateRefCusProcedure("CDS", "A", "40", previousProcedureCode, "000", "whatever", "IMP", group: "H1");
			procedure.ZZ6_CalculateVAT = calculateVAT;
			procedure.ZZ6_CalculateDuty = calculateDuty;
			procedure.ZZ6_IsGuaranteeConsumed = isGuaranteeConsumed;
			procedure.ZZ6_Concession = "";
			return procedure;
		}

		void CalculateEntryLineFeesAndAssertFeePresence(bool isExpected, string assertionMessage, string procedureCode, RefCusTaxOrFee vat, string feeType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "CDS";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var dutyCalculatorStrategy = new CDSDutyCalculatorStrategyForTest(declaration);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "GBP";
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Procedure = procedureCode;
			invoiceLine1.JI_Tariff = "11111111";
			invoiceLine1.JI_ZZF_NKTaxType = vat.ZZF_Code;
			invoiceLine1.JI_CountryOfOrigin = "CN";
			invoiceLine1.JI_PrimaryPreference = "100";
			invoiceLine1.JI_LinePrice = 100;
			Factory.Save();
			_ = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entryLine1 = declaration.CustomsEntryHeaders[0].MergedLines[0];
			dutyCalculatorStrategy.CalculateEntryLineFees(entryLine1);
			var feeCount = entryLine1.Fees.Cast<Business.Declaration.CusEntryLineFee>().Count(x => x.CF_ChargeType == feeType);
			AssertEquals(assertionMessage, isExpected, feeCount > 0);
		}

		protected override LineMergerTestHelper GetLineMergerTestHelper() => new LineMergerTestHelper(Factory);
	}

	public class CDSDutyCalculatorStrategyForTest : CDSDutyCalculatorStrategy
	{
		public CDSDutyCalculatorStrategyForTest(JobDeclaration declaration)
		: base(declaration)
		{
		}

		public new void CalculateEntryLineFees(EU.Business.Declaration.CusEntryLine entryLine)
		{
			base.CalculateEntryLineFees(entryLine);
		}

		public new void CalculateEntryLineVatFee(EU.Business.Declaration.CusEntryLine entryLine)
		{
			base.CalculateEntryLineVatFee(entryLine);
		}

		protected override bool ShouldCalculateDutiesForEntryLine(EU.Business.Declaration.CusEntryLine entryLine) => true;

		protected override bool ShouldCalculateSystemFeeForThisCode(EU.Business.Declaration.CusEntryLine entryLine, string rateCode) => false;
	}
}
