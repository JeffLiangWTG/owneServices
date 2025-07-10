using System;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using Enterprise.Build.Database.Script.Public.Glow.TestHelpers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow
{
	[TestedType(typeof(TG_INS_RatingCalcSliding))]
	class TG_INS_RatingCalcSlidingTest : DbCreateScriptTest
	{
		public void TestSetsTL()
		{
			var item = InsertItem();
			AssertEquals(item.Item1["rateLine"], item.Item2["TM_TL"]);
		}

		public void TestSetsTM_HigherChargeableLowerRate_IsNewGuid()
		{
			var item = InsertItem();
			item.Item1.AssertIsNewGuid(item.Item2["TM_TM_HigherChargeableLowerRate"]);
		}

		public void TestSetsTM_UseInclusiveBreaks_IsNewGuid()
		{
			var item = InsertItem();
			item.Item1.AssertIsNewGuid(item.Item2["TM_TM_UseInclusiveBreaks"]);
		}

		public void TestSetsUseAccumulation_True()
		{
			var item = InsertItem(useAccumulation: true);
			AssertEquals(true, item.Item2["TM_UseAccumulation"]);
		}

		public void TestSetsUseAccumulation_False()
		{
			var item = InsertItem(useAccumulation: false);
			AssertEquals(false, item.Item2["TM_UseAccumulation"]);
		}

		public void TestSetsHigherChargeableLowerRate_True()
		{
			var item = InsertItem(higherChargeableLowerRate: true);
			AssertEquals(true, item.Item2["TM_HigherChargeableLowerRate"]);
		}

		public void TestSetsHigherChargeableLowerRate_False()
		{
			var item = InsertItem(higherChargeableLowerRate: false);
			AssertEquals(false, item.Item2["TM_HigherChargeableLowerRate"]);
		}

		public void TestSetsUseInclusiveBreaks_True()
		{
			var item = InsertItem(useInclusiveBreaks: true);
			AssertEquals(true, item.Item2["TM_UseInclusiveBreaks"]);
		}

		public void TestSetsUseInclusiveBreaks_False()
		{
			var item = InsertItem(useInclusiveBreaks: false);
			AssertEquals(false, item.Item2["TM_UseInclusiveBreaks"]);
		}

		Tuple<GuidHelper, DataRow> InsertItem(bool useAccumulation = false, bool higherChargeableLowerRate = false, bool useInclusiveBreaks = false)
		{
			var generator = new RatingGeneratorForTests(TestConnection);
			var pks = new GuidHelper();
			pks["rateLine"] = generator.NewRateLine();
			pks["item"] = Guid.NewGuid();

			var commandText =
@"INSERT dbo.RatingCalcSliding (TM_PK, TM_TL, TM_UseAccumulation, TM_HigherChargeableLowerRate, TM_UseInclusiveBreaks, TM_SystemLastEditTimeUtc, TM_SystemLastEditUser, TM_SystemCreateTimeUtc, TM_SystemCreateUser)
VALUES (@itemPK, @rateLinePK, @useAccumulation, @higherChargeableLowerRate, @useInclusiveBreaks, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = TestConnection.Command(commandText))
			{
				command.AddParameter("@itemPK", SqlDbType.UniqueIdentifier, pks["item"]);
				command.AddParameter("@rateLinePK", SqlDbType.UniqueIdentifier, pks["rateLine"]);
				command.AddParameter("@useAccumulation", SqlDbType.Bit, useAccumulation);
				command.AddParameter("@higherChargeableLowerRate", SqlDbType.Bit, higherChargeableLowerRate);
				command.AddParameter("@useInclusiveBreaks", SqlDbType.Bit, useInclusiveBreaks);
				command.ExecuteNonQuery();
			}

			return Tuple.Create(pks, generator.GetRateLineItem(pks["item"], "RatingCalcSliding"));
		}
	}
}

