using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Common;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Common.Testing
{
	[TestedType(typeof(Report_ClientRates))]
	class Report_ClientRatesTest : DbCreateScriptTest
	{
		[TestDate(2015, 01, 01)]
		public void TestPercentageBreakCalculator()
		{
			var helper = new TestDbHelper(TestConnection);

			var localChargeCode = helper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "NEWCHG");
			var globalChargeCode = helper.InsertChargeCode(companyPK: null, code: "NEWCHG");

			var supplier = helper.InsertOrgHeader("SUPPLIER", "Supplier");

			var client = helper.InsertOrgHeader("CLIENT", "Client");
			var localClientRate = CreateClientRate(helper, client, TestDbHelper.DefaultCompanyPK);
			CreateRateEntryWithRateLine(helper, localClientRate, "AIR", "LSE", "", "USD", supplier, "PEB", localChargeCode, "-", tm_value: "", tm_break: "1", tm_breakMinimum: "10.123", tm_flatAmount: "11.123");
			CreateRateEntryWithRateLine(helper, localClientRate, "AIR", "ULD", "", "USD", supplier, "PEB", localChargeCode, "+", tm_value: "", tm_break: "1", tm_breakMinimum: "20.123", tm_flatAmount: "21.123");

			var globalClientRate = CreateClientRate(helper, client, company: null);
			CreateRateEntryWithRateLine(helper, globalClientRate, "AIR", "LSE", "", "USD", supplier, "PEB", globalChargeCode, "-", tm_value: "", tm_break: "1", tm_breakMinimum: "100.123", tm_flatAmount: "110.123");
			CreateRateEntryWithRateLine(helper, globalClientRate, "AIR", "ULD", "", "USD", supplier, "PEB", globalChargeCode, "+", tm_value: "", tm_break: "1", tm_breakMinimum: "200.123", tm_flatAmount: "210.123");

			var result = DataUtils.GetDataTableFromQuery(TestConnection, QueryString("", Guid.Empty, includeGlobal: 'N'));
			AssertContainsExactElementsInAnyOrder
			(
				message: "PEB Calculator -/+ should show correct decimal places (not include global)",
				expected: new[]
				{
					$"LineCalculator: PEB, LineItemTypeDescription: -  , ValueCurrency: , LineItemValueOrBreakMinimum: 10.1230, LineItemFlatAmountIsZeroReplacedWithNull: 11.1230, Published: Local, LocalChargePK: {localChargeCode}",
					$"LineCalculator: PEB, LineItemTypeDescription: +  , ValueCurrency: , LineItemValueOrBreakMinimum: 20.1230, LineItemFlatAmountIsZeroReplacedWithNull: 21.1230, Published: Local, LocalChargePK: {localChargeCode}"
				},
				actual: result.Rows.Cast<DataRow>().Select(row => GetRowAsString(row))
			);

			result = DataUtils.GetDataTableFromQuery(TestConnection, QueryString("", Guid.Empty, includeGlobal: 'Y'));
			AssertContainsExactElementsInAnyOrder
			(
				message: "PEB Calculator -/+ should show correct decimal places (include global)",
				expected: new[]
				{
					$"LineCalculator: PEB, LineItemTypeDescription: -  , ValueCurrency: , LineItemValueOrBreakMinimum: 10.1230, LineItemFlatAmountIsZeroReplacedWithNull: 11.1230, Published: Local, LocalChargePK: {localChargeCode}",
					$"LineCalculator: PEB, LineItemTypeDescription: +  , ValueCurrency: , LineItemValueOrBreakMinimum: 20.1230, LineItemFlatAmountIsZeroReplacedWithNull: 21.1230, Published: Local, LocalChargePK: {localChargeCode}",
					$"LineCalculator: PEB, LineItemTypeDescription: -  , ValueCurrency: , LineItemValueOrBreakMinimum: 100.1230, LineItemFlatAmountIsZeroReplacedWithNull: 110.1230, Published: Global, LocalChargePK: {localChargeCode}",
					$"LineCalculator: PEB, LineItemTypeDescription: +  , ValueCurrency: , LineItemValueOrBreakMinimum: 200.1230, LineItemFlatAmountIsZeroReplacedWithNull: 210.1230, Published: Global, LocalChargePK: {localChargeCode}"
				},
				actual: result.Rows.Cast<DataRow>().Select(row => GetRowAsString(row))
			);
		}

		static string GetRowAsString(DataRow row) => $"LineCalculator: {row["LineCalculator"]}, LineItemTypeDescription: {row["LineItemTypeDescription"]}, ValueCurrency: {row["ValueCurrency"]}, LineItemValueOrBreakMinimum: {row["LineItemValueOrBreakMinimum"]}, LineItemFlatAmountIsZeroReplacedWithNull: {row["LineItemFlatAmountIsZeroReplacedWithNull"]}, Published: {row["Published"]}, LocalChargePK: {row["LocalChargePK"]}";

		[TestDate(2015, 01, 01)]
		public void TestCostAndCompanyTariffBasedCalculator()
		{
			var helper = new TestDbHelper(TestConnection);

			var localChargeCode = helper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "NEWCHG");
			var globalChargeCode = helper.InsertChargeCode(companyPK: null, code: "NEWCHG");

			var supplier = helper.InsertOrgHeader("SUPPLIER", "Supplier");

			var client = helper.InsertOrgHeader("CLIENT", "Client");
			var localClientRate = CreateClientRate(helper, client, TestDbHelper.DefaultCompanyPK);
			CreateRateEntryWithRateLine(helper, localClientRate, "AIR", "LSE", "", "USD", supplier, "CST", localChargeCode, "PRU", "10.1234");
			CreateRateEntryWithRateLine(helper, localClientRate, "AIR", "ULD", "", "USD", supplier, "CTB", localChargeCode, "PRU", "10.1234");

			var globalClientRate = CreateClientRate(helper, client, company: null);
			CreateRateEntryWithRateLine(helper, globalClientRate, "AIR", "LSE", "", "USD", supplier, "CST", globalChargeCode, "PRU", "100.1234");
			CreateRateEntryWithRateLine(helper, globalClientRate, "AIR", "ULD", "", "USD", supplier, "CTB", globalChargeCode, "PRU", "100.1234");

			var result = DataUtils.GetDataTableFromQuery(TestConnection, QueryString("", Guid.Empty, includeGlobal: 'N'));
			AssertContainsExactElementsInAnyOrder
			(
				message: "CST and CTB UnitPercentageChange should show correct LineItemTypeDescription and ValueCurrency",
				expected: new[]
				{
					$"LineCalculator: CST, LineItemTypeDescription: Unit Percentage Change, ValueCurrency: , LineItemValueOrBreakMinimum: 10.1234, LineItemFlatAmountIsZeroReplacedWithNull: , Published: Local, LocalChargePK: {localChargeCode}",
					$"LineCalculator: CTB, LineItemTypeDescription: Unit Percentage Change, ValueCurrency: , LineItemValueOrBreakMinimum: 10.1234, LineItemFlatAmountIsZeroReplacedWithNull: , Published: Local, LocalChargePK: {localChargeCode}",
				},
				actual: result.Rows.Cast<DataRow>().Select(row => GetRowAsString(row))
			);

			result = DataUtils.GetDataTableFromQuery(TestConnection, QueryString("", Guid.Empty, includeGlobal: 'Y'));
			AssertContainsExactElementsInAnyOrder
			(
				message: "CST and CTB UnitPercentageChange should show correct LineItemTypeDescription and ValueCurrency",
				expected: new[]
				{
					$"LineCalculator: CST, LineItemTypeDescription: Unit Percentage Change, ValueCurrency: , LineItemValueOrBreakMinimum: 10.1234, LineItemFlatAmountIsZeroReplacedWithNull: , Published: Local, LocalChargePK: {localChargeCode}",
					$"LineCalculator: CTB, LineItemTypeDescription: Unit Percentage Change, ValueCurrency: , LineItemValueOrBreakMinimum: 10.1234, LineItemFlatAmountIsZeroReplacedWithNull: , Published: Local, LocalChargePK: {localChargeCode}",
					$"LineCalculator: CST, LineItemTypeDescription: Unit Percentage Change, ValueCurrency: , LineItemValueOrBreakMinimum: 100.1234, LineItemFlatAmountIsZeroReplacedWithNull: , Published: Global, LocalChargePK: {localChargeCode}",
					$"LineCalculator: CTB, LineItemTypeDescription: Unit Percentage Change, ValueCurrency: , LineItemValueOrBreakMinimum: 100.1234, LineItemFlatAmountIsZeroReplacedWithNull: , Published: Global, LocalChargePK: {localChargeCode}",
				},
				actual: result.Rows.Cast<DataRow>().Select(row => GetRowAsString(row))
			);
		}

		#region Implementation

		static string QueryString(string mode, Guid branch, char includeGlobal)
			=> $"SELECT * FROM [Report_ClientRates]('{mode}', {((branch == Guid.Empty) ? "null" : $"'{branch}'")}, '{TestDbHelper.DefaultCompanyPK}', '{includeGlobal}')";

		static Guid CreateClientRate(TestDbHelper helper, Guid client, Guid? company)
		{
			var pk = Guid.NewGuid();
			helper.Insert("RatingHeader", new
			{
				TH_PK = pk,
				TH_RateType = "SAL",
				TH_OH = client,
				TH_GC = company,
			});

			return pk;
		}

		static (Guid, Guid) CreateRateEntryAndRateLine(TestDbHelper helper, Guid header, string category, string mode, string rateEntryCurrency, string rateLineCurrency, Guid supplier, string calculator, Guid chargeCode)
		{
			var entry = Guid.NewGuid();
			helper.Insert("RateEntry", new
			{
				TI_PK = entry,
				TI_TH = header,
				TI_GC_Publisher = TestDbHelper.DefaultCompanyPK,
				TI_RateCategory = category,
				TI_Mode = mode,
				TI_RateStartDate = helper.ToDate("2014-05-25"),
				TI_OH_Supplier = supplier,
				TI_OriginLRC = "USLAX",
				TI_DestinationLRC = "AUSYD",
				TI_RX_NKCurrency = rateEntryCurrency,
			});

			var rateLine = Guid.NewGuid();
			helper.Insert("RateLines", new
			{
				TL_PK = rateLine,
				TL_TI = entry,
				TL_AC = chargeCode,
				TL_RateCalculator = calculator,
				TL_RX_NKCurrency = rateLineCurrency,
			});

			return (entry, rateLine);
		}

		static void CreateRateLineItem(TestDbHelper helper, Guid rateLine, string tm_type, string tm_value = default, string tm_break = default, string tm_breakMinimum = default, string tm_flatAmount = default)
		{
			helper.Insert("RateLineItems", new
			{
				TM_PK = Guid.NewGuid(),
				TM_TL = rateLine,
				TM_Type = tm_type,
				TM_Value = tm_value ?? string.Empty,
				TM_Break = tm_break ?? "0",
				TM_BreakMinimum = tm_breakMinimum ?? "0",
				TM_FlatAmount = tm_flatAmount ?? "0",
			});
		}

		static void CreateRateEntryWithRateLine(TestDbHelper helper, Guid header, string category, string mode, string rateEntryCurrency, string rateLineCurrency, Guid supplier, string calculator, Guid chargeCode, string tm_type, string tm_value = default, string tm_break = default, string tm_breakMinimum = default, string tm_flatAmount = default)
		{
			var (_, rateLine) = CreateRateEntryAndRateLine(helper, header, category, mode, rateEntryCurrency, rateLineCurrency, supplier, calculator, chargeCode);
			CreateRateLineItem(helper, rateLine, tm_type, tm_value, tm_break, tm_breakMinimum, tm_flatAmount);
		}

		#endregion
	}
}

