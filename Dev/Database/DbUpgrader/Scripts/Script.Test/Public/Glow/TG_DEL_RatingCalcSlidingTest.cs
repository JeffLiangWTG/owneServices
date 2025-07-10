using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using Enterprise.Build.Database.Script.Public.Glow.TestHelpers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow
{
	[TestedType(typeof(TG_DEL_RatingCalcSliding))]
	class TG_DEL_RatingCalcSlidingTest : DbCreateScriptTest
	{
		public void TestDeletesAccItemForLine()
		{
			var items = Delete();
			AssertCollectionNotContains("accItem1", items.Item2);
		}

		public void TestDeletesHclItemForLine()
		{
			var items = Delete();
			AssertCollectionNotContains("hclItem1", items.Item2);
		}

		public void TestDeletesInbItemForLine()
		{
			var items = Delete();
			AssertCollectionNotContains("inbItem1", items.Item2);
		}

		public void TestDoesNotDeleteOtherItemsForLine()
		{
			var items = Delete();
			AssertCollectionContains("flatItem1", items.Item2);
		}

		public void TestDoesNotDeleteItemsForOtherLines()
		{
			var items = Delete();
			AssertCollectionContains("accItem2", items.Item2);
		}

		Tuple<GuidHelper, string[]> Delete()
		{
			var generator = new RatingGeneratorForTests(TestConnection);
			var pks = new GuidHelper();

			pks["rateLine1"] = generator.NewRateLine();
			pks["flatItem1"] = generator.NewRateLineItem(pks["rateLine1"], "FLT");
			pks["accItem1"] = generator.NewRateLineItem(pks["rateLine1"], "ACC");
			pks["hclItem1"] = generator.NewRateLineItem(pks["rateLine1"], "HCL");
			pks["inbItem1"] = generator.NewRateLineItem(pks["rateLine1"], "INB");

			pks["rateLine2"] = generator.NewRateLine();
			pks["accItem2"] = generator.NewRateLineItem(pks["rateLine2"], "ACC");

			using (var command = TestConnection.Command("DELETE dbo.RatingCalcSliding WHERE TM_PK = @pk"))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pks["accItem1"]);
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

