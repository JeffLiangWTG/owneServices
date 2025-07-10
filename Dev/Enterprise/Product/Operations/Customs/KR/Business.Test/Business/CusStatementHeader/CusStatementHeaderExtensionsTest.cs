using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	class CusStatementHeaderExtensionsTest : TestCaseWithFactory
	{
		const string FLA = "FLA";

		[TestDate(2024, 06, 01)]
		public void TestIsWithin6MonthsSinceDueDate()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_DueDate = new ZDateTime("2024-01-01");
			AssertEquals(true, statement.IsWithin6MonthsSinceDueDate());

			statement.B2_DueDate = new ZDateTime("2023-11-30");
			AssertEquals(false, statement.IsWithin6MonthsSinceDueDate());
		}

		[TestDate(2023, 03, 21)]
		public void TestCalculateLatePaymentInterest()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType(FLA, "Flat");
			helper.CreateTaxOrFee(Constants.ZZ.RefCusTaxOrFeeCodes.LatePaymentInterests.AnnualRateWithinSixMonths, 0.029m, Core.Constants.CountryCodes.KoreaSouth, 0m, 0m, FLA, new ZDateTime("2023-03-20"), new ZDateTime("2024-03-21"));
			helper.CreateTaxOrFee(Constants.ZZ.RefCusTaxOrFeeCodes.LatePaymentInterests.AnnualRateWithinSixMonths, 0.012m, Core.Constants.CountryCodes.KoreaSouth, 0m, 0m, FLA, new ZDateTime("2021-03-16"), new ZDateTime("2023-03-19"));
			helper.CreateTaxOrFee(Constants.ZZ.RefCusTaxOrFeeCodes.LatePaymentInterests.AnnualRateWithinSixMonths, 0.018m, Core.Constants.CountryCodes.KoreaSouth, 0m, 0m, FLA, new ZDateTime("2020-03-13"), new ZDateTime("2021-03-15"));
			Factory.Save();

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_DueDate = new ZDateTime("2023-01-30");
			AssertEquals("1578.082 + 158.904 => 1570 + 150", 1720m, statement.CalculateInterestOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.LatePaymentInterests.AnnualRateWithinSixMonths, 1000000m, ZDateTime.Today));
		}

		[TestDate(2022, 2, 19)]
		public void TestCalculateAdditionalDutyLatePaymentInterest()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType(FLA, "Flat");
			helper.CreateTaxOrFee(Constants.ZZ.RefCusTaxOrFeeCodes.LatePaymentInterests.DailyRateAfterSixMonths, 0.0003m, Core.Constants.CountryCodes.KoreaSouth, 0m, 0m, FLA, ZDateTime.MinSmallDateTimeValue, new ZDateTime("2019-02-11"));
			helper.CreateTaxOrFee(Constants.ZZ.RefCusTaxOrFeeCodes.LatePaymentInterests.DailyRateAfterSixMonths, 0.00025m, Core.Constants.CountryCodes.KoreaSouth, 0m, 0m, FLA, new ZDateTime("2019-02-12"), new ZDateTime("2022-02-14"));
			helper.CreateTaxOrFee(Constants.ZZ.RefCusTaxOrFeeCodes.LatePaymentInterests.DailyRateAfterSixMonths, 0.00022m, Core.Constants.CountryCodes.KoreaSouth, 0m, 0m, FLA, new ZDateTime("2022-02-15"), ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_DueDate = new ZDateTime(2022, 02, 14);
			AssertEquals("TotalAmountPayableOneDayAfterDueDate: 11111111 * 0.00022", 2440m, statement.CalculateInterestOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.LatePaymentInterests.DailyRateAfterSixMonths, 11111111m, new ZDateTime(2022, 02, 15)));

			statement.B2_DueDate = new ZDateTime(2022, 02, 10);
			AssertEquals("Late Payment Interest: (11111111 * 0.00025 * 4) + (11111111 * 0.00022 * 5)", 23330m, statement.CalculateInterestOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.LatePaymentInterests.DailyRateAfterSixMonths, 11111111m, ZDateTime.Today));
		}

		[TestDate(2024, 9, 30)]
		public void TestCalculatePenaltyOnAdditionalDuty()
		{
			var dataGrouping = Core.Constants.CountryCodes.KoreaSouth;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType(FLA, "Flat");
			helper.CreateTaxOrFee(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.IllegitimateMissedDeclaration, 0.4m, dataGrouping, 0m, 0m, FLA, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "Duty Penalty of Illegitimate Missed Declaration");
			helper.CreateTaxOrFee(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.GeneralMissedDeclaration, 0.2m, dataGrouping, 0m, 0m, FLA, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "Duty Penalty of General Missed Declaration");
			helper.CreateTaxOrFee(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.FraudulentIllegitimateMissedDeclaration, 0.6m, dataGrouping, 0m, 0m, FLA, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "Duty Penalty of International Illegitimate Missed Declaration");
			helper.CreateTaxOrFee(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.IllegitimateLateDeclaration, 0.4m, dataGrouping, 0m, 0m, FLA, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "Duty Penalty of Illegitimate Late Declaration");
			helper.CreateTaxOrFee(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.GeneralLateDeclaration, 0.1m, dataGrouping, 0m, 0m, FLA, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "Duty Penalty of General Late Declaration");
			helper.CreateTaxOrFee(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.FraudulentIllegitimateLateDeclaration, 0.6m, dataGrouping, 0m, 0m, FLA, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "Duty Penalty of International Illegitimate Late Declaration");

			helper.CreateTaxOrFee(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyReductionRates.WithinTwelveMonths, 0.3m, dataGrouping, 0m, 0m, FLA, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "Duty Penalty Rate Within Twelve Months");
			helper.CreateTaxOrFee(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyReductionRates.WithinEighteenMonths, 0.2m, dataGrouping, 0m, 0m, FLA, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "Duty Penalty Rate Within Eighteen Months");
			helper.CreateTaxOrFee(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyReductionRates.WithinTwentyFourMonths, 0.1m, dataGrouping, 0m, 0m, FLA, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "Duty Penalty Rate Within TwentyFour Months");
			Factory.Save();

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_DueDate = ZDateTime.Invalid;
			var codes = Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyReductionRates.MonthsAndRateCodes;

			CombineAssertions("When statementHeader.B2_DueDate is not valid, it is not calculated.", () =>
			{
				AssertEquals("DPE1", 0m, statement.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.IllegitimateMissedDeclaration, 1000000m, ZDateTime.Today));
				AssertEquals("DPE2", 0m, statement.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.GeneralMissedDeclaration, 2000000m, ZDateTime.Today));
				AssertEquals("DPEA", 0m, statement.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.FraudulentIllegitimateMissedDeclaration, 5000000m, ZDateTime.Today));
				AssertEquals("DPE3", 0m, statement.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.IllegitimateLateDeclaration, 3000000m, ZDateTime.Today));
				AssertEquals("DPE4", 0m, statement.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.GeneralLateDeclaration, 4000000m, ZDateTime.Today));
				AssertEquals("DPEB", 0m, statement.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.FraudulentIllegitimateLateDeclaration, 6000000m, ZDateTime.Today));
			});

			statement.B2_DueDate = new ZDateTime("2024-03-30");
			CombineAssertions("No Reduction", () =>
			{
				AssertEquals("DPE1: 1000000 * 0.4", 400000m, statement.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.IllegitimateMissedDeclaration, 1000000m, ZDateTime.Today));
				AssertEquals("DPE2: 2000000 * 0.2", 400000m, statement.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.GeneralMissedDeclaration, 2000000m, ZDateTime.Today));
				AssertEquals("DPEA: 5000000 * 0.6", 3000000m, statement.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.FraudulentIllegitimateMissedDeclaration, 5000000m, ZDateTime.Today));
				AssertEquals("DPE3: 3000000 * 0.4", 1200000m, statement.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.IllegitimateLateDeclaration, 3000000m, ZDateTime.Today));
				AssertEquals("DPE4: 4000000 * 0.1", 400000m, statement.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.GeneralLateDeclaration, 4000000m, ZDateTime.Today));
				AssertEquals("DPEB: 6000000 * 0.6", 3600000m, statement.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.FraudulentIllegitimateLateDeclaration, 6000000m, ZDateTime.Today));
			});

			statement.B2_DueDate = new ZDateTime("2024-03-30");
			CombineAssertions("Reduction: Not applicable for DPE1/2/A (WithinTwelveMonths)", () =>
			{
				AssertEquals("DPE3: 3000000 * 0.4 * (1 - 0.3)", 840000m, statement.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.IllegitimateLateDeclaration, 3000000m, ZDateTime.Today, codes));
				AssertEquals("DPE4: 4000000 * 0.1 * (1 - 0.3)", 280000m, statement.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.GeneralLateDeclaration, 4000000m, ZDateTime.Today, codes));
				AssertEquals("DPEB: 6000000 * 0.6 * (1 - 0.3)", 2520000m, statement.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.FraudulentIllegitimateLateDeclaration, 6000000m, ZDateTime.Today, codes));
			});

			statement.B2_DueDate = new ZDateTime("2023-09-29");
			CombineAssertions("Reduction: Not applicable for DPE1/2/A (WithinEighteenMonths)", () =>
			{
				AssertEquals("DPE3: 3000000 * 0.4 * (1 - 0.2)", 960000m, statement.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.IllegitimateLateDeclaration, 3000000m, ZDateTime.Today, codes));
				AssertEquals("DPE4: 4000000 * 0.1 * (1 - 0.2)", 320000m, statement.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.GeneralLateDeclaration, 4000000m, ZDateTime.Today, codes));
				AssertEquals("DPEB: 6000000 * 0.6 * (1 - 0.2)", 2880000m, statement.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.FraudulentIllegitimateLateDeclaration, 6000000m, ZDateTime.Today, codes));
			});

			statement.B2_DueDate = new ZDateTime("2023-03-29");
			CombineAssertions("Reduction: Not applicable for DPE1/2/A (WithinTwentyFourMonths)", () =>
			{
				AssertEquals("DPE3: 3000000 * 0.4 * (1 - 0.1)", 1080000m, statement.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.IllegitimateLateDeclaration, 3000000m, ZDateTime.Today, codes));
				AssertEquals("DPE4: 4000000 * 0.1 * (1 - 0.1)", 360000m, statement.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.GeneralLateDeclaration, 4000000m, ZDateTime.Today, codes));
				AssertEquals("DPEB: 6000000 * 0.6 * (1 - 0.1)", 3240000m, statement.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.DutyPenaltyCauseCodes.FraudulentIllegitimateLateDeclaration, 6000000m, ZDateTime.Today, codes));
			});
		}

		[TestDate(2024, 10, 31)]
		public void TestCalculatePenaltyOnAdditionalTax()
		{
			var dataGrouping = Core.Constants.CountryCodes.KoreaSouth;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.IllegitimateMissedDeclaration, 0.4m, dataGrouping, 0m, 0m, FLA, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "Penalty of Illegitimate Missed Declaration");
			helper.CreateTaxOrFee(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.GeneralMissedDeclaration, 0.2m, dataGrouping, 0m, 0m, FLA, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "Penalty of General Missed Declaration");
			helper.CreateTaxOrFee(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.InternationalIllegitimateMissedDeclaration, 0.6m, dataGrouping, 0m, 0m, FLA, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "Penalty of International Illegitimate Missed Declaration");
			helper.CreateTaxOrFee(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.IllegitimateLateDeclaration, 0.4m, dataGrouping, 0m, 0m, FLA, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "Penalty of Illegitimate Late Declaration");
			helper.CreateTaxOrFee(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.GeneralLateDeclaration, 0.1m, dataGrouping, 0m, 0m, FLA, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "Penalty of General Late Declaration");
			helper.CreateTaxOrFee(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.InternationalIllegitimateLateDeclaration, 0.6m, dataGrouping, 0m, 0m, FLA, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "Penalty of International Illegitimate Late Declaration");

			helper.CreateTaxOrFee(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyReductionRates.WithinTwelveMonths, 0.3m, dataGrouping, 0m, 0m, FLA, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "Tax Penalty Rate Within Twelve Months");
			helper.CreateTaxOrFee(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyReductionRates.WithinEighteenMonths, 0.2m, dataGrouping, 0m, 0m, FLA, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "Tax Penalty Rate Within Eighteen Months");
			helper.CreateTaxOrFee(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyReductionRates.WithinTwentyFourMonths, 0.1m, dataGrouping, 0m, 0m, FLA, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "Tax Penalty Rate Within TwentyFour Months");
			Factory.Save();

			var statementHeader = Factory.New<CusStatementHeader>();
			statementHeader.B2_DueDate = ZDateTime.Invalid;
			var codes = Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyReductionRates.MonthsAndRateCodes;

			CombineAssertions("When statementHeader.B2_DueDate is not valid, it is not calculated.", () =>
			{
				AssertEquals("TPE1", 0m, statementHeader.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.IllegitimateMissedDeclaration, 11111111m, ZDateTime.Today));
				AssertEquals("TPE2", 0m, statementHeader.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.GeneralMissedDeclaration, 11111111m, ZDateTime.Today));
				AssertEquals("TPEA", 0m, statementHeader.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.InternationalIllegitimateMissedDeclaration, 11111111m, ZDateTime.Today));
				AssertEquals("TPE3", 0m, statementHeader.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.IllegitimateLateDeclaration, 11111111m, ZDateTime.Today));
				AssertEquals("TPE4", 0m, statementHeader.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.GeneralLateDeclaration, 11111111m, ZDateTime.Today));
				AssertEquals("TPEB", 0m, statementHeader.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.InternationalIllegitimateLateDeclaration, 11111111m, ZDateTime.Today));
			});

			statementHeader.B2_DueDate = new ZDateTime("2024-04-30");
			CombineAssertions(() =>
			{
				AssertEquals("TPE1: 11111111 * 0.4 ", 4444440m, statementHeader.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.IllegitimateMissedDeclaration, 11111111m, ZDateTime.Today));
				AssertEquals("TPE2: 11111111 * 0.2 ", 2222220m, statementHeader.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.GeneralMissedDeclaration, 11111111m, ZDateTime.Today));
				AssertEquals("TPEA: 11111111 * 0.6 ", 6666660m, statementHeader.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.InternationalIllegitimateMissedDeclaration, 11111111m, ZDateTime.Today));
				AssertEquals("TPE3: 11111111 * 0.4 ", 4444440m, statementHeader.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.IllegitimateLateDeclaration, 11111111m, ZDateTime.Today));
				AssertEquals("TPE4: 11111111 * 0.1 ", 1111110m, statementHeader.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.GeneralLateDeclaration, 11111111m, ZDateTime.Today));
				AssertEquals("TPEB: 11111111 * 0.6 ", 6666660m, statementHeader.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.InternationalIllegitimateLateDeclaration, 11111111m, ZDateTime.Today));
			});

			statementHeader.B2_DueDate = new ZDateTime("2024-04-30");
			CombineAssertions("Reduction: Not applicable for TPE1/2/A (WithinTwelveMonths)", () =>
			{
				AssertEquals("TPE3: 11111111 * 0.4 * (1 - 0.3)", 3111110m, statementHeader.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.IllegitimateLateDeclaration, 11111111m, ZDateTime.Today, codes));
				AssertEquals("TPE4: 11111111 * 0.1 * (1 - 0.3)", 777770m, statementHeader.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.GeneralLateDeclaration, 11111111m, ZDateTime.Today, codes));
				AssertEquals("TPEB: 11111111 * 0.6 * (1 - 0.3)", 4666660m, statementHeader.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.InternationalIllegitimateLateDeclaration, 11111111m, ZDateTime.Today, codes));
			});

			statementHeader.B2_DueDate = new ZDateTime("2023-10-30");
			CombineAssertions("Reduction: Not applicable for TPE1/2/A (WithinEighteenMonths)", () =>
			{
				AssertEquals("TPE3: 11111111 * 0.4 * (1 - 0.2)", 3555550m, statementHeader.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.IllegitimateLateDeclaration, 11111111m, ZDateTime.Today, codes));
				AssertEquals("TPE4: 11111111 * 0.1 * (1 - 0.2)", 888880m, statementHeader.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.GeneralLateDeclaration, 11111111m, ZDateTime.Today, codes));
				AssertEquals("TPEB: 11111111 * 0.6 * (1 - 0.2)", 5333330m, statementHeader.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.InternationalIllegitimateLateDeclaration, 11111111m, ZDateTime.Today, codes));
			});

			statementHeader.B2_DueDate = new ZDateTime("2023-04-29");
			CombineAssertions("Reduction: Not applicable for TPE1/2/A (WithinTwentyFourMonths)", () =>
			{
				AssertEquals("TPE3: 11111111 * 0.4 * (1 - 0.1)", 3999990m, statementHeader.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.IllegitimateLateDeclaration, 11111111m, ZDateTime.Today, codes));
				AssertEquals("TPE4: 11111111 * 0.1 * (1 - 0.1)", 999990m, statementHeader.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.GeneralLateDeclaration, 11111111m, ZDateTime.Today, codes));
				AssertEquals("TPEB: 11111111 * 0.6 * (1 - 0.1)", 5999990m, statementHeader.CalculatePenaltyOnAdditionalDutyOrTax(Constants.ZZ.RefCusTaxOrFeeCodes.TaxPenaltyCauseCodes.InternationalIllegitimateLateDeclaration, 11111111m, ZDateTime.Today, codes));
			});
		}

		[TestDate(2024, 06, 01)]
		public void TestExtensionMethod_Is6MonthsAfterDueDate()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_DueDate = new ZDateTime("2024-01-01");
			Assert(!statement.Is6MonthsAfterDueDate());

			statement.B2_DueDate = new ZDateTime("2023-11-30");
			Assert(statement.Is6MonthsAfterDueDate());
		}
	}
}
