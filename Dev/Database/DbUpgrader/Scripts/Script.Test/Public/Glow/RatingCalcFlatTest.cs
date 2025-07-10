using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using Enterprise.Build.Database.Script.Public.Glow.TestHelpers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow.Testing
{
	[TestedType(typeof(RatingCalcFlat))]
	class RatingCalcFlatTest : DbCreateScriptTest
	{
		RatingGeneratorForTests generator;

		protected override void SetUp()
		{
			base.SetUp();
			generator = new RatingGeneratorForTests(TestConnection);
		}

		public void TestContainsOnlyFlatItems()
		{
			var pks = new GuidHelper();

			pks["rateLine1"] = generator.NewRateLine();
			pks["flatItem1"] = generator.NewRateLineItem(pks["rateLine1"], "FLT");
			pks["unitItem1"] = generator.NewRateLineItem(pks["rateLine1"], "UNT");

			pks["rateLine2"] = generator.NewRateLine();
			pks["minItem2"] = generator.NewRateLineItem(pks["rateLine2"], "MIN");
			pks["flatItem2"] = generator.NewRateLineItem(pks["rateLine2"], "FLT");

			var commandText =
@"SELECT TM_PK
FROM dbo.RatingCalcFlat
WHERE TM_TL IN ('@rateLine1', '@rateLine2')";

			var table = DataUtils.GetDataTableFromQuery(TestConnection, pks.Fill(commandText));
			var expectedItems = new[] { "flatItem1", "flatItem2" };
			var actualItems = table.Rows.Cast<DataRow>().Select(r => pks.Key(r["TM_PK"]));
			AssertContainsExactElementsInAnyOrder(expectedItems, actualItems);
		}

		public void TestSelectsColumn_TL()
		{
			var linePK = Guid.Empty;

			var item = InsertItem(rateLine =>
			{
				linePK = rateLine;
				return generator.NewRateLineItem(rateLine, "FLT");
			});

			AssertEquals(linePK, item["TM_TL"]);
		}

		public void TestSelectsColumn_Value()
		{
			var item = InsertItem(rateLine => generator.NewRateLineItem(rateLine, "FLT", value: 11.22m));
			AssertEquals(11.22m, item["TM_Value"]);
		}

		DataRow InsertItem(Func<Guid, Guid> itemFactory)
		{
			var rateLine = generator.NewRateLine();
			var item = itemFactory(rateLine);
			return generator.GetRateLineItem(item, "RatingCalcFlat");
		}
	}
}

