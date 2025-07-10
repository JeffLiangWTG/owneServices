using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using Enterprise.Build.Database.Script.Public.Glow.TestHelpers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow.Testing
{
	[TestedType(typeof(RatingCalcSlidingBreak))]
	class RatingCalcSlidingBreakTest : DbCreateScriptTest
	{
		RatingGeneratorForTests generator;

		protected override void SetUp()
		{
			base.SetUp();
			generator = new RatingGeneratorForTests(TestConnection);
		}

		public void TestContainsItem_Base()
		{
			TestContains("BAS", true);
		}

		public void TestContainsItem_GreaterThan()
		{
			TestContains("+", true);
		}

		public void TestContainsItem_LessThan()
		{
			TestContains("-", true);
		}

		public void TestContainsItem_Max()
		{
			TestContains("MAX", true);
		}

		public void TestContainsItem_Min()
		{
			TestContains("MIN", true);
		}

		public void TestContainsItem_Unit()
		{
			TestContains("UNT", true);
		}

		public void TestDoesNotContainItem_Flat()
		{
			TestContains("FLT", false);
		}

		public void TestSelectsColumn_Break()
		{
			var item = InsertItem(rateLine => generator.NewRateLineItem(rateLine, "+", breakAmount: 123.456m));
			AssertEquals(123.456m, item["TM_Break"]);
		}

		public void TestSelectsColumn_FlatAmount()
		{
			var item = InsertItem(rateLine => generator.NewRateLineItem(rateLine, "+", flatAmount: 99.12m));
			AssertEquals(99.12m, item["TM_FlatAmount"]);
		}

		public void TestSelectsColumn_TL()
		{
			var linePK = Guid.Empty;

			var item = InsertItem(rateLine =>
			{
				linePK = rateLine;
				return generator.NewRateLineItem(rateLine, "+");
			});

			AssertEquals(linePK, item["TM_TL"]);
		}

		public void TestSelectsColumn_Type()
		{
			var item = InsertItem(rateLine => generator.NewRateLineItem(rateLine, "MAX"));
			AssertEquals("MAX", item["TM_Type"]);
		}

		public void TestSelectsColumn_Value()
		{
			var item = InsertItem(rateLine => generator.NewRateLineItem(rateLine, "+", value: 11.22m));
			AssertEquals(11.22m, item["TM_Value"]);
		}

		DataRow InsertItem(Func<Guid, Guid> itemFactory)
		{
			var rateLine = generator.NewRateLine();
			var item = itemFactory(rateLine);
			return generator.GetRateLineItem(item, "RatingCalcSlidingBreak");
		}

		DataTable InsertItems()
		{
			var pks = new GuidHelper();
			pks["rateLine1"] = generator.NewRateLine();
			pks["flatItem1"] = generator.NewRateLineItem(pks["rateLine1"], "FLT");
			pks["unitItem1"] = generator.NewRateLineItem(pks["rateLine1"], "UNT");
			pks["greaterThanItem1"] = generator.NewRateLineItem(pks["rateLine1"], "+");
			pks["maxItem1"] = generator.NewRateLineItem(pks["rateLine1"], "MAX");

			pks["rateLine2"] = generator.NewRateLine();
			pks["minItem2"] = generator.NewRateLineItem(pks["rateLine2"], "MIN");
			pks["flatItem2"] = generator.NewRateLineItem(pks["rateLine2"], "FLT");
			pks["lessThanItem2"] = generator.NewRateLineItem(pks["rateLine2"], "-");
			pks["baseItem2"] = generator.NewRateLineItem(pks["rateLine2"], "BAS");

			var commandText =
@"SELECT TM_Type
FROM dbo.RatingCalcSlidingBreak
WHERE TM_TL IN ('@rateLine1', '@rateLine2')";

			return DataUtils.GetDataTableFromQuery(TestConnection, pks.Fill(commandText));
		}

		void TestContains(string type, bool contains)
		{
			var table = InsertItems();
			var hasMinItem = table.Rows.Cast<DataRow>().Any(r => r["TM_Type"].ToString().TrimEnd().Equals(type));
			AssertEquals(contains, hasMinItem);
		}
	}
}

