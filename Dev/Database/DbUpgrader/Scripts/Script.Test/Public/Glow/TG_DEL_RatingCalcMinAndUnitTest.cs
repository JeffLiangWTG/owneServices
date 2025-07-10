using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using Enterprise.Build.Database.Script.Public.Glow.TestHelpers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow
{
	[TestedType(typeof(TG_DEL_RatingCalcMinAndUnit))]
	class TG_DEL_RatingCalcMinAndUnitTest : DbCreateScriptTest
	{
		public void TestDeletesMinItemForLine()
		{
			var items = Delete();
			AssertCollectionNotContains("minItem1", items.Item2);
		}

		public void TestDeletesUnitItemForLine()
		{
			var items = Delete();
			AssertCollectionNotContains("unitItem1", items.Item2);
		}

		public void TestDoesNotDeleteOtherItemsForLine()
		{
			var items = Delete();
			AssertCollectionContains("flatItem1", items.Item2);
		}

		public void TestDoesNotDeleteItemsForOtherLines()
		{
			var items = Delete();
			AssertCollectionContains("minItem2", items.Item2);
		}

		Tuple<GuidHelper, string[]> Delete()
		{
			var generator = new RatingGeneratorForTests(TestConnection);
			var pks = new GuidHelper();

			pks["rateLine1"] = generator.NewRateLine();
			pks["flatItem1"] = generator.NewRateLineItem(pks["rateLine1"], "FLT");
			pks["unitItem1"] = generator.NewRateLineItem(pks["rateLine1"], "UNT");
			pks["minItem1"] = generator.NewRateLineItem(pks["rateLine1"], "MIN");

			pks["rateLine2"] = generator.NewRateLine();
			pks["minItem2"] = generator.NewRateLineItem(pks["rateLine2"], "MIN");

			using (var command = TestConnection.Command("DELETE dbo.RatingCalcMinAndUnit WHERE TM_PK = @pk"))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pks["minItem1"]);
				command.ExecuteNonQuery();
			}

			var commandText = pks.Fill("SELECT TM_PK FROM dbo.RateLineItems WHERE TM_TL IN ('@rateLine1', '@rateLine2')");
			var table = DataUtils.GetDataTableFromQuery(TestConnection, commandText);

			var remainingPKs = table.Rows
				.Cast<DataRow>()
				.Select(r => pks.Key(r["TM_PK"]))
				.ToArray();

			return Tuple.Create(pks, remainingPKs);
		}
	}
}

