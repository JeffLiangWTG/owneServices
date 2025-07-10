using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using Enterprise.Build.Database.Script.Public.Glow.TestHelpers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow.Testing
{
	[TestedType(typeof(RatingCalcFlatAndUnit))]
	class RatingCalcFlatAndUnitTest : DbCreateScriptTest
	{
		public void TestLineWithFlatAndUnit_SelectsPKFromFlatItem()
		{
			var item = GetItem("rateLine1");
			var key = item.Item1.Key(item.Item2["TM_PK"]);
			AssertEquals("flatItem1", key);
		}

		public void TestLineWithFlatAndUnit_SelectsTM_UnitFromUnitItem()
		{
			var item = GetItem("rateLine1");
			var key = item.Item1.Key(item.Item2["TM_TM_Unit"]);
			AssertEquals("unitItem1", key);
		}

		public void TestLineWithFlatAndUnit_SelectsFlatFromFlatItem()
		{
			var item = GetItem("rateLine1");
			AssertEquals(11.22m, item.Item2["TM_Flat"]);
		}

		public void TestLineWithFlatAndUnit_SelectsUnitFromUnitItem()
		{
			var item = GetItem("rateLine1");
			AssertEquals(22.33m, item.Item2["TM_Unit"]);
		}

		public void TestLineWithFlatOnly_SelectsPKFromFlatItem()
		{
			var item = GetItem("rateLine2");
			var key = item.Item1.Key(item.Item2["TM_PK"]);
			AssertEquals("flatItem2", key);
		}

		public void TestLineWithFlatOnly_SelectsTM_UnitAsNull()
		{
			var item = GetItem("rateLine2");
			AssertEquals(DBNull.Value, item.Item2["TM_TM_Unit"]);
		}

		public void TestLineWithFlatOnly_SelectsFlatFromFlatItem()
		{
			var item = GetItem("rateLine2");
			AssertEquals(33.44m, item.Item2["TM_Flat"]);
		}

		public void TestLineWithFlatOnly_SelectsUnitAsZero()
		{
			var item = GetItem("rateLine2");
			AssertEquals(0m, item.Item2["TM_Unit"]);
		}

		public void TestLineWithUnitOnly_SelectsPKFromUnitItem()
		{
			var item = GetItem("rateLine3");
			var key = item.Item1.Key(item.Item2["TM_PK"]);
			AssertEquals("unitItem3", key);
		}

		public void TestLineWithUnitOnly_SelectsTM_UnitFromUnitItem()
		{
			var item = GetItem("rateLine3");
			var key = item.Item1.Key(item.Item2["TM_TM_Unit"]);
			AssertEquals("unitItem3", key);
		}

		public void TestLineWithUnitOnly_SelectsFlatAsZero()
		{
			var item = GetItem("rateLine3");
			AssertEquals(0m, item.Item2["TM_Flat"]);
		}

		public void TestLineWithUnitOnly_SelectsUnitFromUnitItem()
		{
			var item = GetItem("rateLine3");
			AssertEquals(44.55m, item.Item2["TM_Unit"]);
		}

		Tuple<GuidHelper, DataRow> GetItem(string lineKey)
		{
			var generator = new RatingGeneratorForTests(TestConnection);
			var pks = new GuidHelper();

			pks["rateLine1"] = generator.NewRateLine();
			pks["flatItem1"] = generator.NewRateLineItem(pks["rateLine1"], "FLT", value: 11.22m);
			pks["unitItem1"] = generator.NewRateLineItem(pks["rateLine1"], "UNT", value: 22.33m);

			pks["rateLine2"] = generator.NewRateLine();
			pks["flatItem2"] = generator.NewRateLineItem(pks["rateLine2"], "FLT", value: 33.44m);

			pks["rateLine3"] = generator.NewRateLine();
			pks["unitItem3"] = generator.NewRateLineItem(pks["rateLine3"], "UNT", value: 44.55m);

			var commandText = string.Format("SELECT * FROM dbo.RatingCalcFlatAndUnit WHERE TM_TL = ('{0}')", pks[lineKey]);
			var table = DataUtils.GetDataTableFromQuery(TestConnection, commandText);
			var item = table.Rows.Cast<DataRow>().Single();
			return Tuple.Create(pks, item);
		}
	}
}

