using System;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using Enterprise.Build.Database.Script.Public.Glow.TestHelpers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow
{
	[TestedType(typeof(TG_INS_RatingCalcMinAndUnit))]
	class TG_INS_RatingCalcMinAndUnitTest : DbCreateScriptTest
	{
		public void TestSetsTL()
		{
			var item = InsertItem();
			AssertEquals(item.Item1["rateLine"], item.Item2["TM_TL"]);
		}

		public void TestSetsTM_Unit_IsNewGuid()
		{
			var item = InsertItem();
			item.Item1.AssertIsNewGuid(item.Item2["TM_TM_Unit"]);
		}

		public void TestSetsMin()
		{
			var item = InsertItem();
			AssertEquals(11.22m, item.Item2["TM_Min"]);
		}

		public void TestSetsUnit()
		{
			var item = InsertItem();
			AssertEquals(33.44m, item.Item2["TM_Unit"]);
		}

		Tuple<GuidHelper, DataRow> InsertItem()
		{
			var generator = new RatingGeneratorForTests(TestConnection);
			var pks = new GuidHelper();
			pks["rateLine"] = generator.NewRateLine();
			pks["item"] = Guid.NewGuid();

			var commandText = pks.Fill("INSERT dbo.RatingCalcMinAndUnit (TM_PK, TM_TL, TM_Min, TM_Unit, TM_SystemLastEditTimeUtc, TM_SystemLastEditUser, TM_SystemCreateTimeUtc, TM_SystemCreateUser) VALUES ('@item', '@rateLine', 11.22, 33.44, GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			using (var command = TestConnection.Command(commandText))
			{
				command.ExecuteNonQuery();
			}

			return Tuple.Create(pks, generator.GetRateLineItem(pks["item"], "RatingCalcMinAndUnit"));
		}
	}
}

