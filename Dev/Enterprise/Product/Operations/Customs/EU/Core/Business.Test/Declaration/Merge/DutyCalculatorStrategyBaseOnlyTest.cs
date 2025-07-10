using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.Testing;
using Enterprise.Customs.Universal.Testing;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using static Enterprise.Customs.EU.Business.Testing.LineMergerTestHelper;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(DutyCalculatorStrategy))]
	sealed class DutyCalculatorStrategyBaseOnlyTest : DutyCalculatorStrategyAbstractTest<DutyCalculatorStrategy>
	{
		protected override DutyCalculatorStrategy GetDutyCalculatorStrategy() => new DutyCalculatorStrategy((JobDeclaration)Declaration);

		public override void TestCalculateDuties()
		{
			using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, true))
			{
				var dtyTariffCode = "222";
				var stdPreferenceCode = "STD";

				lineMergerTestHelper.PopulateTestReferenceDataAndSave(dtyTariffCode, stdPreferenceCode);

				var declaration = CreateTestDeclarationAndInvoiceLines(dtyTariffCode, stdPreferenceCode);
				declaration.DoMerge();

				var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>();
				AssertEquals("Entry Header count", 1, entryHeaders.Count());

				var entryHeader = entryHeaders.Single();
				AssertEquals("Entry Line count", 2, entryHeader.MergedLines.Count);

				var entryLineA = entryHeader.MergedLines.Single(x => x.ProcedureCode == "A");
				var entryLineAExpectedFees = new[]
				{
					new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 14m, BaseValue = 70m, Rate = 20m, MethodOfCalculation = "%", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 15m, BaseValue = 3m, Rate = 5m, MethodOfCalculation = "HLT", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 60m, BaseValue = 500m, Rate = 0.12m, MethodOfCalculation = "KGM", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A20RateCode, ChargeAmount = 150m, BaseValue = 500m, Rate = 0.3m, MethodOfCalculation = "KGM", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A30RateCode, ChargeAmount = 3.5m, BaseValue = 7m, Rate = 0.5m, MethodOfCalculation = "DTN", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A40RateCode, ChargeAmount = 21m, BaseValue = 70m, Rate = 30m, MethodOfCalculation = "%", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A40RateCode, ChargeAmount = 30m, BaseValue = 500m, Rate = 0.06m, MethodOfCalculation = "KGM", OverrideReason = "" },
				};
				AssertEntryLineFees(entryLineA, entryLineAExpectedFees);

				var entryLineB = entryHeader.MergedLines.Single(x => x.ProcedureCode == "B");
				var entryLineBExpectedFees = new[]
				{
					new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 3.6m, BaseValue = 18m, Rate = 20m, MethodOfCalculation = "%", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 15.6m, BaseValue = 130m, Rate = 0.12m, MethodOfCalculation = "KGM", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A20RateCode, ChargeAmount = 39m, BaseValue = 130m, Rate = 0.3m, MethodOfCalculation = "KGM", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A20RateCode, ChargeAmount = 0.8m, BaseValue = 4m, Rate = 0.2m, MethodOfCalculation = "LTR", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A30RateCode, ChargeAmount = 0.65m, BaseValue = 1.3m, Rate = 0.5m, MethodOfCalculation = "DTN", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A40RateCode, ChargeAmount = 5.4m, BaseValue = 18m, Rate = 30m, MethodOfCalculation = "%", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A40RateCode, ChargeAmount = 7.8m, BaseValue = 130m, Rate = 0.06m, MethodOfCalculation = "KGM", OverrideReason = "" },
				};
				AssertEntryLineFees(entryLineB, entryLineBExpectedFees);

				// Merge again and assert fees are still the same
				declaration.DoMerge();
				AssertEntryLineFees(entryLineA, entryLineAExpectedFees);
				AssertEntryLineFees(entryLineB, entryLineBExpectedFees);
			}
		}
		public void TestCalculateDuties_WithCalculateDutyInProcedure()
		{
			using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, true))
			{
				var dtyTariffCode = "222";
				var stdPreferenceCode = "STD";

				lineMergerTestHelper.PopulateTestReferenceDataAndSave(dtyTariffCode, stdPreferenceCode, addProcedure: true);

				var declaration = CreateTestDeclarationAndInvoiceLines(dtyTariffCode, stdPreferenceCode);
				declaration.DoMerge();

				var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>();
				AssertEquals("Entry Header count", 1, entryHeaders.Count());

				var entryHeader = entryHeaders.Single();
				AssertEquals("Entry Line count", 2, entryHeader.MergedLines.Count);

				var entryLineA = entryHeader.MergedLines.Single(x => x.ProcedureCode == "A");
				var entryLineAExpectedFees = new[]
				{
					new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 14m, BaseValue = 70m, Rate = 20m, MethodOfCalculation = "%", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 15m, BaseValue = 3m, Rate = 5m, MethodOfCalculation = "HLT", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 60m, BaseValue = 500m, Rate = 0.12m, MethodOfCalculation = "KGM", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A20RateCode, ChargeAmount = 150m, BaseValue = 500m, Rate = 0.3m, MethodOfCalculation = "KGM", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A30RateCode, ChargeAmount = 3.5m, BaseValue = 7m, Rate = 0.5m, MethodOfCalculation = "DTN", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A40RateCode, ChargeAmount = 21m, BaseValue = 70m, Rate = 30m, MethodOfCalculation = "%", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A40RateCode, ChargeAmount = 30m, BaseValue = 500m, Rate = 0.06m, MethodOfCalculation = "KGM", OverrideReason = "" },
				};
				AssertEntryLineFees(entryLineA, entryLineAExpectedFees);

				var entryLineB = entryHeader.MergedLines.Single(x => x.ProcedureCode == "B");
				var entryLineBExpectedFees = System.Array.Empty<FeeAssertionObject>();
				AssertEntryLineFees(entryLineB, entryLineBExpectedFees);

				// Merge again and assert fees are still the same
				declaration.DoMerge();
				AssertEntryLineFees(entryLineA, entryLineAExpectedFees);
				AssertEntryLineFees(entryLineB, entryLineBExpectedFees);
			}
		}
		public void TestCalculateDutiesWithAdditionalAndOverrideFees()
		{
			using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, true))
			{
				var dtyTariffCode = "222";
				var stdPreferenceCode = "STD";

				lineMergerTestHelper.PopulateTestReferenceDataAndSave(dtyTariffCode, stdPreferenceCode);

				var declaration = CreateTestDeclarationAndInvoiceLines(dtyTariffCode, stdPreferenceCode);
				declaration.DoMerge();

				var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>();
				AssertEquals("Entry Header count", 1, entryHeaders.Count());

				var entryHeader = entryHeaders.Single();
				AssertEquals("Entry Line count", 2, entryHeader.MergedLines.Count);

				var entryLineA = entryHeader.MergedLines.Single(x => x.ProcedureCode == "A");
				var entryLineA_ExpectedFee_Sys_A00_Pct = new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 14m, BaseValue = 70m, Rate = 20m, MethodOfCalculation = "%", OverrideReason = "" };
				var entryLineA_ExpectedFee_Sys_A00_HTL = new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 15m, BaseValue = 3m, Rate = 5m, MethodOfCalculation = "HLT", OverrideReason = "" };
				var entryLineA_ExpectedFee_Sys_A00_KGM = new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 60m, BaseValue = 500m, Rate = 0.12m, MethodOfCalculation = "KGM", OverrideReason = "" };
				var entryLineA_ExpectedFee_Sys_A20_KGM = new FeeAssertionObject { ChargeType = RateCode.A20RateCode, ChargeAmount = 150m, BaseValue = 500m, Rate = 0.3m, MethodOfCalculation = "KGM", OverrideReason = "" };
				var entryLineA_ExpectedFee_Sys_A30_DTN = new FeeAssertionObject { ChargeType = RateCode.A30RateCode, ChargeAmount = 3.5m, BaseValue = 7m, Rate = 0.5m, MethodOfCalculation = "DTN", OverrideReason = "" };
				var entryLineA_ExpectedFee_Sys_A40_Pct = new FeeAssertionObject { ChargeType = RateCode.A40RateCode, ChargeAmount = 21m, BaseValue = 70m, Rate = 30m, MethodOfCalculation = "%", OverrideReason = "" };
				var entryLineA_ExpectedFee_Sys_A40_KGM = new FeeAssertionObject { ChargeType = RateCode.A40RateCode, ChargeAmount = 30m, BaseValue = 500m, Rate = 0.06m, MethodOfCalculation = "KGM", OverrideReason = "" };

				AssertEntryLineFees(
					entryLineA,
					[
						entryLineA_ExpectedFee_Sys_A00_Pct,
						entryLineA_ExpectedFee_Sys_A00_HTL,
						entryLineA_ExpectedFee_Sys_A00_KGM,
						entryLineA_ExpectedFee_Sys_A20_KGM,
						entryLineA_ExpectedFee_Sys_A30_DTN,
						entryLineA_ExpectedFee_Sys_A40_Pct,
						entryLineA_ExpectedFee_Sys_A40_KGM
					]
				);

				var entryLineB = entryHeader.MergedLines.Single(x => x.ProcedureCode == "B");
				var entryLineB_ExpectedFee_Sys_A00_Pct = new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 3.6m, BaseValue = 18m, Rate = 20m, MethodOfCalculation = "%", OverrideReason = "" };
				var entryLineB_ExpectedFee_Sys_A00_KGM = new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 15.6m, BaseValue = 130m, Rate = 0.12m, MethodOfCalculation = "KGM", OverrideReason = "" };
				var entryLineB_ExpectedFee_Sys_A20_KGM = new FeeAssertionObject { ChargeType = RateCode.A20RateCode, ChargeAmount = 39m, BaseValue = 130m, Rate = 0.3m, MethodOfCalculation = "KGM", OverrideReason = "" };
				var entryLineB_ExpectedFee_Sys_A20_LTR = new FeeAssertionObject { ChargeType = RateCode.A20RateCode, ChargeAmount = 0.8m, BaseValue = 4m, Rate = 0.2m, MethodOfCalculation = "LTR", OverrideReason = "" };
				var entryLineB_ExpectedFee_Sys_A30_DTN = new FeeAssertionObject { ChargeType = RateCode.A30RateCode, ChargeAmount = 0.65m, BaseValue = 1.3m, Rate = 0.5m, MethodOfCalculation = "DTN", OverrideReason = "" };
				var entryLineB_ExpectedFee_Sys_A40_Pct = new FeeAssertionObject { ChargeType = RateCode.A40RateCode, ChargeAmount = 5.4m, BaseValue = 18m, Rate = 30m, MethodOfCalculation = "%", OverrideReason = "" };
				var entryLineB_ExpectedFee_Sys_A40_KGM = new FeeAssertionObject { ChargeType = RateCode.A40RateCode, ChargeAmount = 7.8m, BaseValue = 130m, Rate = 0.06m, MethodOfCalculation = "KGM", OverrideReason = "" };

				AssertEntryLineFees(
					entryLineB,
					[
						entryLineB_ExpectedFee_Sys_A00_Pct,
						entryLineB_ExpectedFee_Sys_A00_KGM,
						entryLineB_ExpectedFee_Sys_A20_KGM,
						entryLineB_ExpectedFee_Sys_A20_LTR,
						entryLineB_ExpectedFee_Sys_A30_DTN,
						entryLineB_ExpectedFee_Sys_A40_Pct,
						entryLineB_ExpectedFee_Sys_A40_KGM
					]
				);

				var entryLineA_ExpectedFee_ADD_XXX = AddUserEnteredLineFee(entryLineA, chargeType: "XXX", amount: 10m, overrideReason: RateOverrideReasonList.Codes.Additional);
				var entryLineA_ExpectedFee_OVR_A00 = AddUserEnteredLineFee(entryLineA, chargeType: RateCode.A00RateCode, amount: 15m, overrideReason: RateOverrideReasonList.Codes.Override);

				AssertEntryLineFees(
					entryLineA,
					[
						entryLineA_ExpectedFee_Sys_A00_Pct,
						entryLineA_ExpectedFee_Sys_A00_HTL,
						entryLineA_ExpectedFee_Sys_A00_KGM,
						entryLineA_ExpectedFee_Sys_A20_KGM,
						entryLineA_ExpectedFee_Sys_A30_DTN,
						entryLineA_ExpectedFee_Sys_A40_Pct,
						entryLineA_ExpectedFee_Sys_A40_KGM,
						entryLineA_ExpectedFee_ADD_XXX,
						entryLineA_ExpectedFee_OVR_A00
					]
				);

				var entryLineB_ExpectedFee_ADD_ZZZ = AddUserEnteredLineFee(entryLineB, chargeType: "ZZZ", amount: 20m, overrideReason: RateOverrideReasonList.Codes.Additional);

				AssertEntryLineFees(
					entryLineB,
					[
						entryLineB_ExpectedFee_Sys_A00_Pct,
						entryLineB_ExpectedFee_Sys_A00_KGM,
						entryLineB_ExpectedFee_Sys_A20_KGM,
						entryLineB_ExpectedFee_Sys_A20_LTR,
						entryLineB_ExpectedFee_Sys_A30_DTN,
						entryLineB_ExpectedFee_Sys_A40_Pct,
						entryLineB_ExpectedFee_Sys_A40_KGM,
						entryLineB_ExpectedFee_ADD_ZZZ
					]
				);

				// Merge again and assert that:
				// * non-system calculated fees are still the same
				// * previous system calculated fees with an equivalent non-system overide fee are not recalculated
				declaration.DoMerge();

				AssertEntryLineFees(
					entryLineA,
					[
						entryLineA_ExpectedFee_Sys_A20_KGM,
						entryLineA_ExpectedFee_Sys_A30_DTN,
						entryLineA_ExpectedFee_Sys_A40_Pct,
						entryLineA_ExpectedFee_Sys_A40_KGM,
						entryLineA_ExpectedFee_ADD_XXX,
						entryLineA_ExpectedFee_OVR_A00
					]
				);

				AssertEntryLineFees(
					entryLineB,
					[
						entryLineB_ExpectedFee_Sys_A00_Pct,
						entryLineB_ExpectedFee_Sys_A00_KGM,
						entryLineB_ExpectedFee_Sys_A20_KGM,
						entryLineB_ExpectedFee_Sys_A20_LTR,
						entryLineB_ExpectedFee_Sys_A30_DTN,
						entryLineB_ExpectedFee_Sys_A40_Pct,
						entryLineB_ExpectedFee_Sys_A40_KGM,
						entryLineB_ExpectedFee_ADD_ZZZ
					]
				);
			}
		}

		public void TestCalculateDutiesWithAdditionalAndOverrideFees_WithCalculateDutyInProcedure()
		{
			using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, true))
			{
				var dtyTariffCode = "222";
				var stdPreferenceCode = "STD";

				lineMergerTestHelper.PopulateTestReferenceDataAndSave(dtyTariffCode, stdPreferenceCode, addProcedure: true);

				var declaration = CreateTestDeclarationAndInvoiceLines(dtyTariffCode, stdPreferenceCode);
				declaration.DoMerge();

				var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>();
				AssertEquals("Entry Header count", 1, entryHeaders.Count());

				var entryHeader = entryHeaders.Single();
				AssertEquals("Entry Line count", 2, entryHeader.MergedLines.Count);

				var entryLineA = entryHeader.MergedLines.Single(x => x.ProcedureCode == "A");
				var entryLineA_ExpectedFee_Sys_A00_Pct = new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 14m, BaseValue = 70m, Rate = 20m, MethodOfCalculation = "%", OverrideReason = "" };
				var entryLineA_ExpectedFee_Sys_A00_HTL = new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 15m, BaseValue = 3m, Rate = 5m, MethodOfCalculation = "HLT", OverrideReason = "" };
				var entryLineA_ExpectedFee_Sys_A00_KGM = new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 60m, BaseValue = 500m, Rate = 0.12m, MethodOfCalculation = "KGM", OverrideReason = "" };
				var entryLineA_ExpectedFee_Sys_A20_KGM = new FeeAssertionObject { ChargeType = RateCode.A20RateCode, ChargeAmount = 150m, BaseValue = 500m, Rate = 0.3m, MethodOfCalculation = "KGM", OverrideReason = "" };
				var entryLineA_ExpectedFee_Sys_A30_DTN = new FeeAssertionObject { ChargeType = RateCode.A30RateCode, ChargeAmount = 3.5m, BaseValue = 7m, Rate = 0.5m, MethodOfCalculation = "DTN", OverrideReason = "" };
				var entryLineA_ExpectedFee_Sys_A40_Pct = new FeeAssertionObject { ChargeType = RateCode.A40RateCode, ChargeAmount = 21m, BaseValue = 70m, Rate = 30m, MethodOfCalculation = "%", OverrideReason = "" };
				var entryLineA_ExpectedFee_Sys_A40_KGM = new FeeAssertionObject { ChargeType = RateCode.A40RateCode, ChargeAmount = 30m, BaseValue = 500m, Rate = 0.06m, MethodOfCalculation = "KGM", OverrideReason = "" };

				AssertEntryLineFees(
					entryLineA,
					[
						entryLineA_ExpectedFee_Sys_A00_Pct,
						entryLineA_ExpectedFee_Sys_A00_HTL,
						entryLineA_ExpectedFee_Sys_A00_KGM,
						entryLineA_ExpectedFee_Sys_A20_KGM,
						entryLineA_ExpectedFee_Sys_A30_DTN,
						entryLineA_ExpectedFee_Sys_A40_Pct,
						entryLineA_ExpectedFee_Sys_A40_KGM
					]
				);

				var entryLineB = entryHeader.MergedLines.Single(x => x.ProcedureCode == "B");
				AssertEntryLineFees(entryLineB, System.Array.Empty<FeeAssertionObject>());

				var entryLineA_ExpectedFee_ADD_XXX = AddUserEnteredLineFee(entryLineA, chargeType: "XXX", amount: 10m, overrideReason: RateOverrideReasonList.Codes.Additional);
				var entryLineA_ExpectedFee_OVR_A00 = AddUserEnteredLineFee(entryLineA, chargeType: RateCode.A00RateCode, amount: 15m, overrideReason: RateOverrideReasonList.Codes.Override);

				AssertEntryLineFees(
					entryLineA,
					[
						entryLineA_ExpectedFee_Sys_A00_Pct,
						entryLineA_ExpectedFee_Sys_A00_HTL,
						entryLineA_ExpectedFee_Sys_A00_KGM,
						entryLineA_ExpectedFee_Sys_A20_KGM,
						entryLineA_ExpectedFee_Sys_A30_DTN,
						entryLineA_ExpectedFee_Sys_A40_Pct,
						entryLineA_ExpectedFee_Sys_A40_KGM,
						entryLineA_ExpectedFee_ADD_XXX,
						entryLineA_ExpectedFee_OVR_A00
					]
				);

				var entryLineB_ExpectedFee_ADD_ZZZ = AddUserEnteredLineFee(entryLineB, chargeType: "ZZZ", amount: 20m, overrideReason: RateOverrideReasonList.Codes.Additional);

				AssertEntryLineFees(
					entryLineB,
					[
						entryLineB_ExpectedFee_ADD_ZZZ
					]
				);

				// Merge again and assert that:
				// * non-system calculated fees are still the same
				// * previous system calculated fees with an equivalent non-system overide fee are not recalculated
				declaration.DoMerge();

				AssertEntryLineFees(
					entryLineA,
					[
						entryLineA_ExpectedFee_Sys_A20_KGM,
						entryLineA_ExpectedFee_Sys_A30_DTN,
						entryLineA_ExpectedFee_Sys_A40_Pct,
						entryLineA_ExpectedFee_Sys_A40_KGM,
						entryLineA_ExpectedFee_ADD_XXX,
						entryLineA_ExpectedFee_OVR_A00
					]
				);

				AssertEntryLineFees(
					entryLineB,
					[
						entryLineB_ExpectedFee_ADD_ZZZ
					]
				);
			}
		}

		static FeeAssertionObject AddUserEnteredLineFee(CusEntryLine entryLine, ZString chargeType, ZDecimal amount, ZString overrideReason)
		{
			var newEnteredFee = entryLine.Fees.AddNew();
			newEnteredFee.CF_ChargeType = chargeType;
			newEnteredFee.CF_ChargeAmount = amount;
			newEnteredFee.CF_RateOverrideReasonCode = overrideReason;
			return new FeeAssertionObject { ChargeType = chargeType, ChargeAmount = amount, OverrideReason = overrideReason };
		}

		public void TestCalculateDutiesWithNoApplicableDutyRate()
		{
			using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, true))
			{
				var declaration = CreateTestDeclarationAndInvoiceLines("ANY", "ANY");
				declaration.DoMerge();

				var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>();
				AssertEquals("Entry Header count", 1, entryHeaders.Count());

				var entryHeader = entryHeaders.Single();
				AssertEquals("Entry Line count", 2, entryHeader.MergedLines.Count);

				foreach (CusEntryLine entryLine in entryHeader.MergedLines)
				{
					AssertEntryLineFees(entryLine, System.Array.Empty<FeeAssertionObject>());
				}

				// Add an entry fee
				var entryLine1 = entryHeader.MergedLines[0];
				entryLine1.Fees.AddNew().CF_RateOverrideReasonCode = "";
				entryLine1.Fees.AddNew().CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Additional;
				AssertEquals("Entry Line Fee count", 2, entryLine1.Fees.Count);

				// Merge should clear system calculated entry fees before re-calculating them using Universal Fee Calculation
				declaration.DoMerge();
				AssertEquals("Entry Line Fee count (after merge)", 1, entryLine1.Fees.Count);
				AssertEquals("Entry Line Fee CF_RateOverrideReasonCode", RateOverrideReasonList.Codes.Additional, entryLine1.Fees[0].CF_RateOverrideReasonCode);
			}
		}

		public void TestCalculateDutiesAndVatFeesWithoutUniversalFeeCalculation()
		{
			using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, false))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				declaration.Invoices.AddNew().InvoiceLines.AddNew();

				declaration.DoMerge();

				var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>();
				AssertEquals("Entry Header count", 1, entryHeaders.Count());
				var entryHeader = entryHeaders.Single();
				AssertEquals("Entry Line count", 1, entryHeader.MergedLines.Count);
				var entryLine = entryHeader.MergedLines.Single();
				AssertEquals("Entry Line Fee count (after 1st merge)", 0, entryLine.Fees.Count);

				// Add an entry fee
				entryLine.Fees.AddNew();
				AssertEquals("Entry Line Fee count (after manually adding a fee)", 1, entryLine.Fees.Count);
				entryLine.Fees[0].CF_ChargeAmount = 78m;
				entryLine.Fees[0].CF_RateOverrideReasonCode = ZString.Empty;

				CombineAssertions("[PRE-CONDITION] Entry Line Fee", () =>
				{
					AssertEquals("CF_ChargeAmount", 78m, entryLine.Fees[0].CF_ChargeAmount);
					AssertEquals("CF_RateOverrideReasonCode", ZString.Empty, entryLine.Fees[0].CF_RateOverrideReasonCode);
				});

				// Merge should not clear entry fees as it's not using Universal Fee Calculation
				declaration.DoMerge();
				AssertEquals("[After 2nd merge] Entry Line Fee count", 1, entryLine.Fees.Count);

				CombineAssertions("[After 2nd merge] Entry Line Fee", () =>
				{
					AssertEquals("CF_ChargeAmount", 78m, entryLine.Fees[0].CF_ChargeAmount);
					AssertEquals("CF_RateOverrideReasonCode", ZString.Empty, entryLine.Fees[0].CF_RateOverrideReasonCode);
				});
			}
		}

		public void TestCalculateVat()
		{
			using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, true))
			{
				var dtyTariffCode = "222";
				var stdPreferenceCode = "STD";

				lineMergerTestHelper.PopulateTestReferenceDataAndSave(dtyTariffCode, stdPreferenceCode);

				var declaration = CreateTestDeclarationAndInvoiceLines(dtyTariffCode, stdPreferenceCode, taxType: "ORD");
				declaration.DoMerge();

				var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>();
				AssertEquals("Entry Header count", 1, entryHeaders.Count());

				var entryHeader = entryHeaders.Single();
				AssertEquals("Entry Line count", 2, entryHeader.MergedLines.Count);

				var entryLineA = entryHeader.MergedLines.Single(x => x.ProcedureCode == "A");
				AssertSystemVatEntryLineFee(entryLineA, 79.97m, 363.50m, "%", 22m);

				var entryLineAExpectedFees = new[]
				{
					new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 14m, BaseValue = 70m, Rate = 20m, MethodOfCalculation = "%", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 15m, BaseValue = 3m, Rate = 5m, MethodOfCalculation = "HLT", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 60m, BaseValue = 500m, Rate = 0.12m, MethodOfCalculation = "KGM", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A20RateCode, ChargeAmount = 150m, BaseValue = 500m, Rate = 0.3m, MethodOfCalculation = "KGM", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A30RateCode, ChargeAmount = 3.5m, BaseValue = 7m, Rate = 0.5m, MethodOfCalculation = "DTN", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A40RateCode, ChargeAmount = 21m, BaseValue = 70m, Rate = 30m, MethodOfCalculation = "%", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A40RateCode, ChargeAmount = 30m, BaseValue = 500m, Rate = 0.06m, MethodOfCalculation = "KGM", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.VatRateCode, ChargeAmount = 79.97m, BaseValue = 363.50m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
				};
				AssertEntryLineFees(entryLineA, entryLineAExpectedFees);

				var entryLineB = entryHeader.MergedLines.Single(x => x.ProcedureCode == "B");
				AssertSystemVatEntryLineFee(entryLineB, 19.987m, 90.85m, "%", 22m);

				var entryLineBExpectedFees = new[]
				{
					new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 3.6m, BaseValue = 18m, Rate = 20m, MethodOfCalculation = "%", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 15.6m, BaseValue = 130m, Rate = 0.12m, MethodOfCalculation = "KGM", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A20RateCode, ChargeAmount = 39m, BaseValue = 130m, Rate = 0.3m, MethodOfCalculation = "KGM", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A20RateCode, ChargeAmount = 0.8m, BaseValue = 4m, Rate = 0.2m, MethodOfCalculation = "LTR", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A30RateCode, ChargeAmount = 0.65m, BaseValue = 1.3m, Rate = 0.5m, MethodOfCalculation = "DTN", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A40RateCode, ChargeAmount = 5.4m, BaseValue = 18m, Rate = 30m, MethodOfCalculation = "%", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A40RateCode, ChargeAmount = 7.8m, BaseValue = 130m, Rate = 0.06m, MethodOfCalculation = "KGM", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.VatRateCode, ChargeAmount = 19.987m, BaseValue = 90.85m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
				};
				AssertEntryLineFees(entryLineB, entryLineBExpectedFees);
			}
		}

		public void TestCalculateVat_WithCalculateDutyInProcedure()
		{
			using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, true))
			{
				var dtyTariffCode = "222";
				var stdPreferenceCode = "STD";

				lineMergerTestHelper.PopulateTestReferenceDataAndSave(dtyTariffCode, stdPreferenceCode, addProcedure: true);

				var declaration = CreateTestDeclarationAndInvoiceLines(dtyTariffCode, stdPreferenceCode, taxType: "ORD");
				declaration.DoMerge();

				var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>();
				AssertEquals("Entry Header count", 1, entryHeaders.Count());

				var entryHeader = entryHeaders.Single();
				AssertEquals("Entry Line count", 2, entryHeader.MergedLines.Count);

				var entryLineA = entryHeader.MergedLines.Single(x => x.ProcedureCode == "A");
				AssertSystemVatEntryLineFee(entryLineA, 79.97m, 363.50m, "%", 22m);

				var entryLineAExpectedFees = new[]
				{
					new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 14m, BaseValue = 70m, Rate = 20m, MethodOfCalculation = "%", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 15m, BaseValue = 3m, Rate = 5m, MethodOfCalculation = "HLT", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 60m, BaseValue = 500m, Rate = 0.12m, MethodOfCalculation = "KGM", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A20RateCode, ChargeAmount = 150m, BaseValue = 500m, Rate = 0.3m, MethodOfCalculation = "KGM", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A30RateCode, ChargeAmount = 3.5m, BaseValue = 7m, Rate = 0.5m, MethodOfCalculation = "DTN", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A40RateCode, ChargeAmount = 21m, BaseValue = 70m, Rate = 30m, MethodOfCalculation = "%", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A40RateCode, ChargeAmount = 30m, BaseValue = 500m, Rate = 0.06m, MethodOfCalculation = "KGM", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.VatRateCode, ChargeAmount = 79.97m, BaseValue = 363.50m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
				};
				AssertEntryLineFees(entryLineA, entryLineAExpectedFees);

				var entryLineB = entryHeader.MergedLines.Single(x => x.ProcedureCode == "B");
				AssertSystemVatEntryLineFee(entryLineB, 3.96m, 18m, "%", 22m);

				var entryLineBExpectedFees = new[]
				{
					new FeeAssertionObject { ChargeType = RateCode.VatRateCode, ChargeAmount = 3.96m, BaseValue = 18m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
				};
				AssertEntryLineFees(entryLineB, entryLineBExpectedFees);
			}
		}

		public void TestCalculateVatWithNoApplicableVatFee()
		{
			using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, true))
			{
				var dtyTariffCode = "222";
				var stdPreferenceCode = "STD";

				lineMergerTestHelper.PopulateTestReferenceDataAndSave(dtyTariffCode, stdPreferenceCode, addProcedure: true);

				var declaration = CreateTestDeclarationAndInvoiceLines(dtyTariffCode, stdPreferenceCode, taxType: "");
				declaration.DoMerge();

				var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>();
				AssertEquals("Entry Header count", 1, entryHeaders.Count());

				var entryHeader = entryHeaders.Single();
				AssertEquals("Entry Line count", 2, entryHeader.MergedLines.Count);

				var orderedEntryLines = entryHeader.MergedLines.OrderBy(x => x.InvoiceLines.Count);

				var entryLine1 = orderedEntryLines.First();
				AssertNull("VAT should be missing", GetSystemVatFee(entryLine1));

				var entryLine2 = orderedEntryLines.Skip(1).Single();
				AssertNull("VAT should be missing", GetSystemVatFee(entryLine2));
			}
		}

		public void TestCalculateVatWithAdditionalAndOverrideVatFees()
		{
			using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, true))
			{
				var dtyTariffCode = "222";
				var stdPreferenceCode = "STD";

				lineMergerTestHelper.PopulateTestReferenceDataAndSave(dtyTariffCode, stdPreferenceCode);

				var declaration = CreateTestDeclarationAndInvoiceLines(dtyTariffCode, stdPreferenceCode, taxType: "ORD");
				declaration.DoMerge();

				var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>();
				AssertEquals("Entry Header count", 1, entryHeaders.Count());

				var entryHeader = entryHeaders.Single();
				AssertEquals("Entry Line count", 2, entryHeader.MergedLines.Count);

				var entryLineA = entryHeader.MergedLines.Single(x => x.ProcedureCode == "A");
				var entryLineA_ExpectedVatFee_Sys = new FeeAssertionObject { ChargeType = RateCode.VatRateCode, ChargeAmount = 79.97m, BaseValue = 363.50m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" };
				AssertVatEntryLineFees(entryLineA, entryLineA_ExpectedVatFee_Sys);

				var entryLineB = entryHeader.MergedLines.Single(x => x.ProcedureCode == "B");
				var entryLineB_ExpectedVatFee_Sys = new FeeAssertionObject { ChargeType = RateCode.VatRateCode, ChargeAmount = 19.987m, BaseValue = 90.85m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" };
				AssertVatEntryLineFees(entryLineB, entryLineB_ExpectedVatFee_Sys);

				var entryLineA_UserEnteredFee_OVR_B00 = AddUserEnteredLineFee(entryLineA, chargeType: RateCode.VatRateCode, amount: 15m, overrideReason: RateOverrideReasonList.Codes.Override);
				AssertVatEntryLineFees(entryLineA, entryLineA_ExpectedVatFee_Sys, entryLineA_UserEnteredFee_OVR_B00);

				var entryLineB_UserEnteredFee_ADD_B00 = AddUserEnteredLineFee(entryLineB, chargeType: RateCode.VatRateCode, amount: 20m, overrideReason: RateOverrideReasonList.Codes.Additional);
				AssertVatEntryLineFees(entryLineB, entryLineB_ExpectedVatFee_Sys, entryLineB_UserEnteredFee_ADD_B00);

				// Merge again and assert that:
				// * non-system calculated VAT fees are still the same
				// * previous system calculated VAT fees with an equivalent non-system overide fee are not recalculated
				declaration.DoMerge();

				AssertVatEntryLineFees(entryLineA, entryLineA_UserEnteredFee_OVR_B00);
				AssertVatEntryLineFees(entryLineB, entryLineB_ExpectedVatFee_Sys, entryLineB_UserEnteredFee_ADD_B00);
			}
		}

		public void TestCalculateVatWithAdditionalAndOverrideVatFees_WithCalculateDutyInProcedure()
		{
			using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, true))
			{
				var dtyTariffCode = "222";
				var stdPreferenceCode = "STD";

				lineMergerTestHelper.PopulateTestReferenceDataAndSave(dtyTariffCode, stdPreferenceCode, addProcedure: true);

				var declaration = CreateTestDeclarationAndInvoiceLines(dtyTariffCode, stdPreferenceCode, taxType: "ORD");
				declaration.DoMerge();

				var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>();
				AssertEquals("Entry Header count", 1, entryHeaders.Count());

				var entryHeader = entryHeaders.Single();
				AssertEquals("Entry Line count", 2, entryHeader.MergedLines.Count);

				var entryLineA = entryHeader.MergedLines.Single(x => x.ProcedureCode == "A");
				var entryLineA_ExpectedVatFee_Sys = new FeeAssertionObject { ChargeType = RateCode.VatRateCode, ChargeAmount = 79.97m, BaseValue = 363.50m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" };
				AssertVatEntryLineFees(entryLineA, entryLineA_ExpectedVatFee_Sys);

				var entryLineB = entryHeader.MergedLines.Single(x => x.ProcedureCode == "B");
				var entryLineB_ExpectedVatFee_Sys = new FeeAssertionObject { ChargeType = RateCode.VatRateCode, ChargeAmount = 3.96m, BaseValue = 18m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" };
				AssertVatEntryLineFees(entryLineB, entryLineB_ExpectedVatFee_Sys);

				var entryLineA_UserEnteredFee_OVR_B00 = AddUserEnteredLineFee(entryLineA, chargeType: RateCode.VatRateCode, amount: 15m, overrideReason: RateOverrideReasonList.Codes.Override);
				AssertVatEntryLineFees(entryLineA, entryLineA_ExpectedVatFee_Sys, entryLineA_UserEnteredFee_OVR_B00);

				var entryLineB_UserEnteredFee_ADD_B00 = AddUserEnteredLineFee(entryLineB, chargeType: RateCode.VatRateCode, amount: 20m, overrideReason: RateOverrideReasonList.Codes.Additional);
				AssertVatEntryLineFees(entryLineB, entryLineB_ExpectedVatFee_Sys, entryLineB_UserEnteredFee_ADD_B00);

				// Merge again and assert that:
				// * non-system calculated VAT fees are still the same
				// * previous system calculated VAT fees with an equivalent non-system overide fee are not recalculated
				declaration.DoMerge();

				AssertVatEntryLineFees(entryLineA, entryLineA_UserEnteredFee_OVR_B00);
				AssertVatEntryLineFees(entryLineB, entryLineB_ExpectedVatFee_Sys, entryLineB_UserEnteredFee_ADD_B00);
			}
		}

		public void TestCalculateVatWithVatSuspendedProcedure()
		{
			using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, true))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var procedureA = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Latvia, "IM", "A", "", "   ", "Procedure A", "IMP", "10P");
				procedureA.ZZ6_CalculateVAT = true;
				var procedureB = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Latvia, "IM", "B", "", "   ", "Procedure B", "IMP", "10P");
				procedureB.ZZ6_CalculateVAT = false;
				Factory.Save();

				var dtyTariffCode = "222";
				var stdPreferenceCode = "STD";

				lineMergerTestHelper.PopulateTestReferenceDataAndSave(dtyTariffCode, stdPreferenceCode);

				var declaration = CreateTestDeclarationAndInvoiceLines(dtyTariffCode, stdPreferenceCode, taxType: "ORD");
				declaration.DoMerge();

				var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.Single();
				AssertEquals("Entry Line count", 2, entryHeader.MergedLines.Count);

				var entryLineA = entryHeader.MergedLines.Single(x => x.ProcedureCode == "A");
				AssertEquals("Entry line A has procedure A with ZZ6_CalculateVAT as true.", 79.97m, GetSystemVatFee(entryLineA).CF_ChargeAmount);

				var entryLineB = entryHeader.MergedLines.Single(x => x.ProcedureCode == "B");
				AssertNull("Entry line B has procedure B with ZZ6_CalculateVAT as false.", GetSystemVatFee(entryLineB));
			}
		}

		public void TestCalculateVatWithVatSuspendedProcedure_WithCalculateDutyInProcedure()
		{
			using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, true))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var procedureA = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Latvia, "IM", "A", "", "   ", "Procedure A", "IMP", "10P");
				procedureA.ZZ6_CalculateVAT = true;
				procedureA.ZZ6_CalculateDuty = false;
				var procedureB = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Latvia, "IM", "B", "", "   ", "Procedure B", "IMP", "10P");
				procedureB.ZZ6_CalculateVAT = false;
				procedureB.ZZ6_CalculateDuty = false;
				Factory.Save();

				var dtyTariffCode = "222";
				var stdPreferenceCode = "STD";

				lineMergerTestHelper.PopulateTestReferenceDataAndSave(dtyTariffCode, stdPreferenceCode);

				var declaration = CreateTestDeclarationAndInvoiceLines(dtyTariffCode, stdPreferenceCode, taxType: "ORD");
				declaration.DoMerge();

				var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.Single();
				AssertEquals("Entry Line count", 2, entryHeader.MergedLines.Count);

				var entryLineA = entryHeader.MergedLines.Single(x => x.ProcedureCode == "A");
				AssertEquals("Entry line A has procedure A with ZZ6_CalculateVAT as true.", 15.40m, GetSystemVatFee(entryLineA).CF_ChargeAmount);

				var entryLineB = entryHeader.MergedLines.Single(x => x.ProcedureCode == "B");
				AssertNull("Entry line B has procedure B with ZZ6_CalculateVAT as false.", GetSystemVatFee(entryLineB));
			}
		}

		public void TestPreviouslyEnteredMethodOfPaymentIsPreserved()
		{
			using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, true))
			{
				var dtyTariffCode = "222";
				var stdPreferenceCode = "STD";
				lineMergerTestHelper.PopulateTestReferenceDataAndSave(dtyTariffCode, stdPreferenceCode, addProcedure: true);
				var declaration = CreateTestDeclarationAndInvoiceLines(dtyTariffCode, stdPreferenceCode);
				declaration.DoMerge();

				AssertEquals("[PRE-CONDITION] Entry Headers", 1, declaration.CustomsEntryHeaders.Count);
				var entryHeader = declaration.CustomsEntryHeaders[0];
				AssertEquals("[PRE-CONDITION] Entry Lines", 2, entryHeader.MergedLines.Count);
				var entryLineA = entryHeader.MergedLines.Single(x => x.ProcedureCode == "A");
				var fees = entryLineA.Fees.OfType<CusEntryLineFee>();
				var a00Fees = fees.Where(x => x.CF_ChargeType == RateCode.A00RateCode);
				AssertGreaterThan("[PRE-CONDITION] A00 Line Fees Count in Entry Line A", a00Fees.Count(), 0);
				var a30Fees = fees.Where(x => x.CF_ChargeType == RateCode.A30RateCode);
				AssertGreaterThan("[PRE-CONDITION] A30 Line Fees Count in Entry Line A", a30Fees.Count(), 0);

				a00Fees.ToList().ForEach(x =>
				{
					x.CF_MethodOfPayment = "MP1";
					x.NationalFeeTypeCode = "NT1";
				});
				a30Fees.ToList().ForEach(x =>
				{
					x.CF_MethodOfPayment = "MP2";
					x.NationalFeeTypeCode = "NT2";
				});

				declaration.DoMerge();
				CombineAssertions("Checking Merge preserves previous entered MoP", () =>
				{
					AssertEquals("All A00 Line fees keep the previous entered MoP: MP1", true, a00Fees.All(x => x.CF_MethodOfPayment == "MP1" && x.NationalFeeTypeCode == "NT1"));
					AssertEquals("All A30 Line fees keep the previous entered MoP: MP2", true, a30Fees.All(x => x.CF_MethodOfPayment == "MP2" && x.NationalFeeTypeCode == "NT2"));
				});
			}
		}

		public void TestFeesCalculationSupportsExtraFeeCalculation()
		{
			using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, true))
			{
				var dtyTariffCode = "222";
				var stdPreferenceCode = "STD";
				lineMergerTestHelper.PopulateTestReferenceDataAndSave(dtyTariffCode, stdPreferenceCode, addProcedure: true);
				var declaration = CreateTestDeclarationAndInvoiceLines(dtyTariffCode, stdPreferenceCode);
				new LineMergerWithDummyExtraFeeCalculator(declaration).DoMerge();

				AssertEquals("[PRE-CONDITION] Entry Headers", 1, declaration.CustomsEntryHeaders.Count);
				var entryHeader = declaration.CustomsEntryHeaders[0];
				AssertEquals("[PRE-CONDITION] Entry Lines", 2, entryHeader.MergedLines.Count);
				var entryLineA = entryHeader.MergedLines.Single(x => x.ProcedureCode == "A");
				var fees = entryLineA.Fees.OfType<CusEntryLineFee>();

				AssertEquals("Fees should contains the Extra Fee [RateCode: XXX]", true, fees.Any(x => x.CF_ChargeType == "XXX"));
				AssertEquals("Fees should contains the Extra Fee [RateCode: YYY]", true, fees.Any(x => x.CF_ChargeType == "YYY"));
			}
		}

		public void TestCalculateVat_IncludesVatableExtraFeesButNotNonVatableExtraFees()
		{
			using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, true))
			{
				var dtyTariffCode = "222";
				var stdPreferenceCode = "STD";

				lineMergerTestHelper.PopulateTestReferenceDataAndSave(dtyTariffCode, stdPreferenceCode, addProcedure: true);

				var declaration = CreateTestDeclarationAndInvoiceLines(dtyTariffCode, stdPreferenceCode, taxType: "ORD");
				new LineMergerWithDummyExtraFeeCalculator(declaration).DoMerge();

				var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>();
				AssertEquals("Entry Header count", 1, entryHeaders.Count());

				var entryHeader = entryHeaders.Single();
				AssertEquals("Entry Line count", 2, entryHeader.MergedLines.Count);

				var entryLineA = entryHeader.MergedLines.Single(x => x.ProcedureCode == "A");
				var entryLineAExpectedFees = new[]
				{
					new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 14m, BaseValue = 70m, Rate = 20m, MethodOfCalculation = "%", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 15m, BaseValue = 3m, Rate = 5m, MethodOfCalculation = "HLT", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 60m, BaseValue = 500m, Rate = 0.12m, MethodOfCalculation = "KGM", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A20RateCode, ChargeAmount = 150m, BaseValue = 500m, Rate = 0.3m, MethodOfCalculation = "KGM", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A30RateCode, ChargeAmount = 3.5m, BaseValue = 7m, Rate = 0.5m, MethodOfCalculation = "DTN", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A40RateCode, ChargeAmount = 21m, BaseValue = 70m, Rate = 30m, MethodOfCalculation = "%", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A40RateCode, ChargeAmount = 30m, BaseValue = 500m, Rate = 0.06m, MethodOfCalculation = "KGM", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = "XXX", ChargeAmount = 100m, BaseValue = 10m, Rate = 10, MethodOfCalculation = "X", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.VatRateCode, ChargeAmount = 101.97m, BaseValue = 463.50m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = "YYY", ChargeAmount = 200m, BaseValue = 20m, Rate = 20, MethodOfCalculation = "X", OverrideReason = "" },
				};
				AssertEntryLineFees(entryLineA, entryLineAExpectedFees);
			}
		}

		public void TestMethodOfPaymentIsSetFromIntermediateResultButNotOverwrittenIfManuallyChanged()
		{
			using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, true))
			{
				var dtyTariffCode = "222";
				var stdPreferenceCode = "STD";
				lineMergerTestHelper.PopulateTestReferenceDataAndSave(dtyTariffCode, stdPreferenceCode);
				var declaration = CreateTestDeclarationAndInvoiceLines(dtyTariffCode, stdPreferenceCode);
				var lineMerger = new LineMergerWithDummyExtraFeeCalculator(declaration);
				lineMerger.DoMerge();

				AssertEquals("[PRE-CONDITION] Entry Headers", 1, declaration.CustomsEntryHeaders.Count);
				var entryHeader = declaration.CustomsEntryHeaders[0];
				AssertEquals("[PRE-CONDITION] Entry Lines", 2, entryHeader.MergedLines.Count);
				var entryLineA = entryHeader.MergedLines.Single(x => x.ProcedureCode == "A");

				var feeXXX = GetFeeXXX(entryLineA);
				AssertNotNull("Fees should contains the Extra Fee [RateCode: XXX]", feeXXX);
				AssertEquals("CF_MethodOfPayment has been defaulted from intermediate result", "Z", feeXXX.CF_MethodOfPayment);

				feeXXX.CF_MethodOfPayment = "T";
				lineMerger.DoMerge();
				feeXXX = GetFeeXXX(entryLineA);
				AssertEquals("CF_MethodOfPayment has not been overwritten as manually changed", "T", feeXXX.CF_MethodOfPayment);
			}

			CusEntryLineFee GetFeeXXX(CusEntryLine entryLine) => entryLine.Fees.GetElementWithThisCode("XXX");
		}

		public void TestFeesCalculationWithNonParticipatingMinMaxResults()
		{
			using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, true))
			{
				var stdPreferenceCode = "STD";
				var dtyTariffCode = "222";

				var configurationBuilder = DutyReferenceDataConfigurationBuilder.New(Factory)
					.AddPreferences(stdPreferenceCode)
					.AddTariffType(Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff)
					.AddTariff(dtyTariffCode, taxOrFeeCode: "DTY")
					.AddRateCode(RateTypeEnum.Duty, rateCode: RateCode.A00RateCode, rateFormula: "MIN(VFD * 0.2, IF([KGM] > 100, 1.0 * [KGM], 1.5 * [KGM]))", preference: "100");
				configurationBuilder.Configure();
				Factory.Save();

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_RX_NKInvoice_Currency = "EUR";
				var invLine = invoice.InvoiceLines.AddNew();
				invLine.JI_CEI = entryInstruction.PK;
				invLine.JI_Procedure = "A";
				invLine.JI_PrimaryPreference = stdPreferenceCode;
				invLine.JI_Tariff = dtyTariffCode;
				invLine.JI_CustomsQuantity = 10;
				invLine.JI_CustomsUnitQty = "KGM";
				invLine.JI_LinePrice = 50;

				new LineMergerIncludingNonParticipatingMinMaxResultsInDutyCalculation(declaration).DoMerge();

				var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>();
				AssertEquals("Entry Header count", 1, entryHeaders.Count());
				var entryHeader = entryHeaders.Single();
				AssertEquals("Entry Line count", 1, entryHeader.MergedLines.Count);
				var entryLine = entryHeader.MergedLines.Single();

				var expectedFees = new[]
				{
					new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 10m, BaseValue = 50m, Rate = 20m, MethodOfCalculation = "%", OverrideReason = "" },
					new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 0m, BaseValue = 10m, Rate = 1.5m, MethodOfCalculation = "KGM", OverrideReason = "" },
				};
				AssertEntryLineFees(entryLine, expectedFees);
			}
		}

		public void TestCalculateDuties_ShouldCalculateDutiesForEntryLine_False() => AssertCalculateDuties_ShouldCalculateDutiesForEntryLine(false);

		public void TestCalculateDuties_ShouldCalculateDutiesForEntryLine_True() => AssertCalculateDuties_ShouldCalculateDutiesForEntryLine(true);

		public void TestCalculateDuties_ShouldCalculateTaxesForEntryLine_False() => AssertCalculateDuties_ShouldCalculateTaxesForEntryLine(false);

		public void TestCalculateDuties_ShouldCalculateTaxesForEntryLine_True() => AssertCalculateDuties_ShouldCalculateTaxesForEntryLine(true);

		public void TestCalculateAdValoremDutyRateForInvoiceLine()
		{
			using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, configurationValue: true))
			{
				const string dtyTariffCode = "222";
				const string stdPreferenceCode = "STD";

				lineMergerTestHelper.PopulateTestReferenceDataAndSave(dtyTariffCode, stdPreferenceCode);

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_PrimaryPreference = stdPreferenceCode;
				invoiceLine.JI_Tariff = dtyTariffCode;
				invoiceLine.JI_SupplementaryCode1 = "7000";
				invoiceLine.JI_CustomsQuantity = 5;
				invoiceLine.JI_CustomsUnitQty = "DTN";
				invoiceLine.JI_LinePrice = 50;
				AssertEquals("Pre-Req: Invoice Line count", 1, declaration.InvoiceLines.Count);

				var dutyCalculatorStrategy = new Mock<DutyCalculatorStrategy>(declaration);
				dutyCalculatorStrategy.CallBase = true;
				var dutyCalculator = dutyCalculatorStrategy.Object;
				var result = dutyCalculator.CalculateAdValoremDutyRateForInvoiceLine(invoiceLine);
				AssertNull(result);

				invoiceLine.JI_SupplementaryCode1 = "8000";
				result = dutyCalculator.CalculateAdValoremDutyRateForInvoiceLine(invoiceLine);
				AssertNotNull(result.Value);
				AssertEquals(50m, result.Value);
			}
		}

		void AssertCalculateDuties_ShouldCalculateDutiesForEntryLine(bool shouldCalculateDutiesForEntryLine)
		{
			using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, true))
			{
				const string dtyTariffCode = "222";
				const string stdPreferenceCode = "STD";

				lineMergerTestHelper.PopulateTestReferenceDataAndSave(dtyTariffCode, stdPreferenceCode);

				var declaration = CreateTestDeclarationAndInvoiceLines(dtyTariffCode, stdPreferenceCode, taxType: "ORD");
				declaration.DoMerge();

				var entryHeader = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Single();
				AssertEquals("Entry Line count", 2, entryHeader.MergedLines.Count);

				var dutyCalculatorStrategy = new Mock<DutyCalculatorStrategy>(declaration);
				dutyCalculatorStrategy.CallBase = true;
				dutyCalculatorStrategy.Protected()
					.Setup<bool>("ShouldCalculateDutiesForEntryLine", ItExpr.IsAny<CusEntryLine>())
					.Returns(shouldCalculateDutiesForEntryLine);
				var dutyCalculator = dutyCalculatorStrategy.Object;
				dutyCalculator.CalculateDuties();

				var entryLineA = entryHeader.MergedLines.Single(x => x.ProcedureCode == "A");
				var entryLineB = entryHeader.MergedLines.Single(x => x.ProcedureCode == "B");
				if (!shouldCalculateDutiesForEntryLine)
				{
					var entryLineAExpectedFees = new[]
					{
						new FeeAssertionObject { ChargeType = RateCode.VatRateCode, ChargeAmount = 15.40m, BaseValue = 70m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
					};
					AssertEntryLineFees(entryLineA, entryLineAExpectedFees);
					var entryLineBExpectedFees = new[]
					{
						new FeeAssertionObject { ChargeType = RateCode.VatRateCode, ChargeAmount = 3.96m, BaseValue = 18m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
					};
					AssertEntryLineFees(entryLineB, entryLineBExpectedFees);
				}
				else
				{
					var entryLineAExpectedFees = new[]
					{
						new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 14m, BaseValue = 70m, Rate = 20m, MethodOfCalculation = "%", OverrideReason = "" },
						new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 15m, BaseValue = 3m, Rate = 5m, MethodOfCalculation = "HLT", OverrideReason = "" },
						new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 60m, BaseValue = 500m, Rate = 0.12m, MethodOfCalculation = "KGM", OverrideReason = "" },
						new FeeAssertionObject { ChargeType = RateCode.A20RateCode, ChargeAmount = 150m, BaseValue = 500m, Rate = 0.3m, MethodOfCalculation = "KGM", OverrideReason = "" },
						new FeeAssertionObject { ChargeType = RateCode.A30RateCode, ChargeAmount = 3.5m, BaseValue = 7m, Rate = 0.5m, MethodOfCalculation = "DTN", OverrideReason = "" },
						new FeeAssertionObject { ChargeType = RateCode.A40RateCode, ChargeAmount = 21m, BaseValue = 70m, Rate = 30m, MethodOfCalculation = "%", OverrideReason = "" },
						new FeeAssertionObject { ChargeType = RateCode.A40RateCode, ChargeAmount = 30m, BaseValue = 500m, Rate = 0.06m, MethodOfCalculation = "KGM", OverrideReason = "" },
						new FeeAssertionObject { ChargeType = RateCode.VatRateCode, ChargeAmount = 79.97m, BaseValue = 363.50m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
					};
					AssertEntryLineFees(entryLineA, entryLineAExpectedFees);
					var entryLineBExpectedFees = new[]
					{
						new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 3.6m, BaseValue = 18m, Rate = 20m, MethodOfCalculation = "%", OverrideReason = "" },
						new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 15.6m, BaseValue = 130m, Rate = 0.12m, MethodOfCalculation = "KGM", OverrideReason = "" },
						new FeeAssertionObject { ChargeType = RateCode.A20RateCode, ChargeAmount = 39m, BaseValue = 130m, Rate = 0.3m, MethodOfCalculation = "KGM", OverrideReason = "" },
						new FeeAssertionObject { ChargeType = RateCode.A20RateCode, ChargeAmount = 0.8m, BaseValue = 4m, Rate = 0.2m, MethodOfCalculation = "LTR", OverrideReason = "" },
						new FeeAssertionObject { ChargeType = RateCode.A30RateCode, ChargeAmount = 0.65m, BaseValue = 1.3m, Rate = 0.5m, MethodOfCalculation = "DTN", OverrideReason = "" },
						new FeeAssertionObject { ChargeType = RateCode.A40RateCode, ChargeAmount = 5.4m, BaseValue = 18m, Rate = 30m, MethodOfCalculation = "%", OverrideReason = "" },
						new FeeAssertionObject { ChargeType = RateCode.A40RateCode, ChargeAmount = 7.8m, BaseValue = 130m, Rate = 0.06m, MethodOfCalculation = "KGM", OverrideReason = "" },
						new FeeAssertionObject { ChargeType = RateCode.VatRateCode, ChargeAmount = 19.9870m, BaseValue = 90.85m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
					};
					AssertEntryLineFees(entryLineB, entryLineBExpectedFees);
				}
			}
		}

		void AssertCalculateDuties_ShouldCalculateTaxesForEntryLine(bool shouldCalculateTaxesForEntryLine)
		{
			using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, true))
			{
				const string dtyTariffCode = "222";
				const string stdPreferenceCode = "STD";

				lineMergerTestHelper.PopulateTestReferenceDataAndSave(dtyTariffCode, stdPreferenceCode, addProcedure: true);

				var declaration = CreateTestDeclarationAndInvoiceLines(dtyTariffCode, stdPreferenceCode, taxType: "ORD");
				declaration.DoMerge();

				var entryHeader = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Single();
				AssertEquals("Entry Line count", 2, entryHeader.MergedLines.Count);

				var dutyCalculatorStrategy = new Mock<DutyCalculatorStrategy>(declaration);
				dutyCalculatorStrategy.CallBase = true;
				dutyCalculatorStrategy.Protected()
					.Setup<bool>("ShouldCalculateTaxesForEntryLine", ItExpr.IsAny<CusEntryLine>())
					.Returns(shouldCalculateTaxesForEntryLine);
				var dutyCalculator = dutyCalculatorStrategy.Object;
				dutyCalculator.CalculateDuties();

				var entryLineA = entryHeader.MergedLines.Single(x => x.ProcedureCode == "A");
				var entryLineB = entryHeader.MergedLines.Single(x => x.ProcedureCode == "B");
				if (!shouldCalculateTaxesForEntryLine)
				{
					var entryLineAExpectedFees = new[]
					{
						new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 14m, BaseValue = 70m, Rate = 20m, MethodOfCalculation = "%", OverrideReason = "" },
						new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 15m, BaseValue = 3m, Rate = 5m, MethodOfCalculation = "HLT", OverrideReason = "" },
						new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 60m, BaseValue = 500m, Rate = 0.12m, MethodOfCalculation = "KGM", OverrideReason = "" },
						new FeeAssertionObject { ChargeType = RateCode.A20RateCode, ChargeAmount = 150m, BaseValue = 500m, Rate = 0.3m, MethodOfCalculation = "KGM", OverrideReason = "" },
						new FeeAssertionObject { ChargeType = RateCode.A30RateCode, ChargeAmount = 3.5m, BaseValue = 7m, Rate = 0.5m, MethodOfCalculation = "DTN", OverrideReason = "" },
						new FeeAssertionObject { ChargeType = RateCode.A40RateCode, ChargeAmount = 21m, BaseValue = 70m, Rate = 30m, MethodOfCalculation = "%", OverrideReason = "" },
						new FeeAssertionObject { ChargeType = RateCode.A40RateCode, ChargeAmount = 30m, BaseValue = 500m, Rate = 0.06m, MethodOfCalculation = "KGM", OverrideReason = "" },
					};
					AssertEntryLineFees(entryLineA, entryLineAExpectedFees);
					AssertEntryLineFees(entryLineB, Array.Empty<FeeAssertionObject>());
				}
				else
				{
					var entryLineAExpectedFees = new[]
					{
						new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 14m, BaseValue = 70m, Rate = 20m, MethodOfCalculation = "%", OverrideReason = "" },
						new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 15m, BaseValue = 3m, Rate = 5m, MethodOfCalculation = "HLT", OverrideReason = "" },
						new FeeAssertionObject { ChargeType = RateCode.A00RateCode, ChargeAmount = 60m, BaseValue = 500m, Rate = 0.12m, MethodOfCalculation = "KGM", OverrideReason = "" },
						new FeeAssertionObject { ChargeType = RateCode.A20RateCode, ChargeAmount = 150m, BaseValue = 500m, Rate = 0.3m, MethodOfCalculation = "KGM", OverrideReason = "" },
						new FeeAssertionObject { ChargeType = RateCode.A30RateCode, ChargeAmount = 3.5m, BaseValue = 7m, Rate = 0.5m, MethodOfCalculation = "DTN", OverrideReason = "" },
						new FeeAssertionObject { ChargeType = RateCode.A40RateCode, ChargeAmount = 21m, BaseValue = 70m, Rate = 30m, MethodOfCalculation = "%", OverrideReason = "" },
						new FeeAssertionObject { ChargeType = RateCode.A40RateCode, ChargeAmount = 30m, BaseValue = 500m, Rate = 0.06m, MethodOfCalculation = "KGM", OverrideReason = "" },
						new FeeAssertionObject { ChargeType = RateCode.VatRateCode, ChargeAmount = 79.97m, BaseValue = 363.50m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
					};
					AssertEntryLineFees(entryLineA, entryLineAExpectedFees);
					var entryLineBExpectedFees = new[]
					{
						new FeeAssertionObject { ChargeType = RateCode.VatRateCode, ChargeAmount = 3.96m, BaseValue = 18m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
					};
					AssertEntryLineFees(entryLineB, entryLineBExpectedFees);
				}
			}
		}

		JobDeclaration CreateTestDeclarationAndInvoiceLines(ZString dtyTariffCode, ZString stdPreferenceCode, string taxType = "")
			=> lineMergerTestHelper.CreateTestDeclarationAndInvoiceLines(dtyTariffCode, stdPreferenceCode, Common.CustomsChargeTypeList.Codes.AdditionCharge, taxType);

		void AssertVatEntryLineFees(CusEntryLine entryLine, params FeeAssertionObject[] expectedVatFees) => LineMergerTestHelper.AssertVatEntryLineFees(entryLine, ExpectedChargeAmountDecimalPlaces, expectedVatFees);

		#region LineMerger for Tests

		sealed class LineMergerWithDummyExtraFeeCalculator : LineMerger
		{
			public LineMergerWithDummyExtraFeeCalculator(JobDeclaration declaration) : base(declaration)
			{
			}

			JobDeclaration EuJobDeclaration => (JobDeclaration)Declaration;

			protected override Customs.Business.IDutyCalculatorStrategy GetNewDutyCalculatorStrategy() => new DutyCalculatorStrategyWithDummyExtraFeeCalculator(EuJobDeclaration);
		}

		sealed class DutyCalculatorStrategyWithDummyExtraFeeCalculator : DutyCalculatorStrategy
		{
			public DutyCalculatorStrategyWithDummyExtraFeeCalculator(JobDeclaration declaration) : base(declaration)
			{
			}

			protected override IEnumerable<IExtraFeeCalculator> GetExtraFeeCalculatorCollectionCore(CusEntryLine entryLine)
			{
				yield return new DummyExtraFeeCalculatorForTest();
			}

			protected override IEnumerable<IExtraFeeCalculator> GetNonVatableExtraFeeCalculatorCollectionCore(CusEntryLine entryLine)
			{
				yield return new DummyNonVatableExtraFeeCalculatorForTest();
			}
		}

		sealed class DummyExtraFeeCalculatorForTest : IExtraFeeCalculator
		{
			public ZString RateCode => "XXX";

			public IEnumerable<Customs.Business.IDutyCalculationIntermediateResult> CalculateExtraFees()
			{
				yield return new DutyCalculationIntermediateResult(100, 10, 10, "X") { MethodOfPayment = "Z" };
			}
		}

		sealed class DummyNonVatableExtraFeeCalculatorForTest : IExtraFeeCalculator
		{
			public ZString RateCode => "YYY";

			public IEnumerable<Customs.Business.IDutyCalculationIntermediateResult> CalculateExtraFees()
			{
				yield return new DutyCalculationIntermediateResult(200, 20, 20, "X") { MethodOfPayment = "Z" };
			}
		}

		sealed class LineMergerIncludingNonParticipatingMinMaxResultsInDutyCalculation : LineMerger
		{
			public LineMergerIncludingNonParticipatingMinMaxResultsInDutyCalculation(JobDeclaration declaration) : base(declaration)
			{
			}

			JobDeclaration EuJobDeclaration => (JobDeclaration)Declaration;

			protected override Customs.Business.IDutyCalculatorStrategy GetNewDutyCalculatorStrategy() => new DutyCalculatorStrategyIncludingNonParticipatingMinMaxResultsInDutyCalculation(EuJobDeclaration);
		}

		sealed class DutyCalculatorStrategyIncludingNonParticipatingMinMaxResultsInDutyCalculation : DutyCalculatorStrategy
		{
			public DutyCalculatorStrategyIncludingNonParticipatingMinMaxResultsInDutyCalculation(JobDeclaration declaration) : base(declaration)
			{
			}

			protected override bool ShouldDutyCalculationIncludeNonParticipatingMinMaxResults => true;
		}

		#endregion
	}
}
