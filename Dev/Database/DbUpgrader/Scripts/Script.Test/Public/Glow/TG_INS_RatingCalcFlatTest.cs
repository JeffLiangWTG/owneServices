using System;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using Enterprise.Build.Database.Script.Public.Glow.TestHelpers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow
{
	[TestedType(typeof(TG_INS_RatingCalcFlat))]
	class TG_INS_RatingCalcFlatTest : DbCreateScriptTest
	{
		public void TestSetsTL()
		{
			var item = InsertItem();
			AssertEquals(item.Item1, item.Item2["TM_TL"]);
		}

		public void TestSetsTypeToFlat()
		{
			var item = InsertItem();
			AssertEquals("FLT", item.Item2["TM_Type"]);
		}

		public void TestSetsValue()
		{
			var item = InsertItem();
			AssertEquals(990.123m, item.Item2["TM_Value"]);
		}

		Tuple<Guid, DataRow> InsertItem()
		{
			var generator = new RatingGeneratorForTests(TestConnection);
			var pks = new GuidHelper();
			pks["rateLine"] = generator.NewRateLine();
			pks["item"] = Guid.NewGuid();

			var commandText = pks.Fill("INSERT dbo.RatingCalcFlat (TM_PK, TM_TL, TM_Value, TM_SystemLastEditTimeUtc, TM_SystemLastEditUser, TM_SystemCreateTimeUtc, TM_SystemCreateUser) VALUES ('@item', '@rateLine', 990.123, GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			using (var command = TestConnection.Command(commandText))
			{
				command.ExecuteNonQuery();
			}

			return Tuple.Create(pks["rateLine"], generator.GetRateLineItem(pks["item"], "RateLineItems"));
		}
	}
}

