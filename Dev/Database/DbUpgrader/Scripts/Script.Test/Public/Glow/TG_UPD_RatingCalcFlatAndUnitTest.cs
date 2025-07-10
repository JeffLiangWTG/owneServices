using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using Enterprise.Build.Database.Script.Public.Glow.TestHelpers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow
{
	[TestedType(typeof(TG_UPD_RatingCalcFlatAndUnit))]
	class TG_UPD_RatingCalcFlatAndUnitTest : DbCreateScriptTest
	{
		class LineWithFlatAndUnit : TransactionedTestCase
		{
			RatingGeneratorForTests generator;
			GuidHelper pks;

			protected override void SetUp()
			{
				base.SetUp();

				generator = new RatingGeneratorForTests(TestConnection);
				pks = new GuidHelper();
				pks["rateLine"] = generator.NewRateLine();
				pks["flatItem"] = generator.NewRateLineItem(pks["rateLine"], "FLT");
				pks["unitItem"] = generator.NewRateLineItem(pks["rateLine"], "UNT");
			}

			public void TestSetsFlat()
			{
				Update();
				var item = generator.GetRateLineItem(pks["flatItem"], "RateLineItems");
				AssertEquals(20m, item["TM_Value"]);
			}

			public void TestSetsUnit()
			{
				Update();
				var item = generator.GetRateLineItem(pks["unitItem"], "RateLineItems");
				AssertEquals(60m, item["TM_Value"]);
			}

			public void TestThrowsErrorIfUpdatingMultipleRows()
			{
				pks["rateLine2"] = generator.NewRateLine();
				pks["flatItem2"] = generator.NewRateLineItem(pks["rateLine2"], "FLT");

				using (var command = TestConnection.Command("UPDATE dbo.RatingCalcFlatAndUnit SET TM_Flat = 20"))
				{
					AssertExceptionThrown(typeof(SqlException), "Cannot update more than 1 row at a time.", () => command.ExecuteNonQuery());
				}
			}

			public void TestThrowsErrorIfUpdatingPK()
			{
				AssertCannotUpdateKey("TM_PK");
			}

			public void TestThrowsErrorIfUpdatingTM_Unit()
			{
				AssertCannotUpdateKey("TM_TM_Unit");
			}

			public void TestThrowsErrorIfUpdatingTL()
			{
				AssertCannotUpdateKey("TM_TL");
			}

			void AssertCannotUpdateKey(string columnName)
			{
				var commandText = "UPDATE dbo.RatingCalcFlatAndUnit SET " + columnName + " = NEWID() WHERE TM_PK = @pk";

				using (var command = TestConnection.Command(commandText))
				{
					command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pks["flatItem"]);

					AssertExceptionThrown(
						typeof(SqlException),
						"Updating of TM_PK, TM_TM_Unit or TM_TL is not allowed.",
						() => command.ExecuteNonQuery());
				}
			}

			void Update()
			{
				using (var command = TestConnection.Command("UPDATE dbo.RatingCalcFlatAndUnit SET TM_Flat = 20, TM_Unit = 60 WHERE TM_PK = @pk"))
				{
					command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pks["flatItem"]);
					command.ExecuteNonQuery();
				}
			}
		}

		class LineWithFlatOnly : TransactionedTestCase
		{
			RatingGeneratorForTests generator;
			GuidHelper pks;

			protected override void SetUp()
			{
				base.SetUp();

				generator = new RatingGeneratorForTests(TestConnection);
				pks = new GuidHelper();
				pks["rateLine"] = generator.NewRateLine();
				pks["flatItem"] = generator.NewRateLineItem(pks["rateLine"], "FLT");

				using (var command = TestConnection.Command("UPDATE dbo.RatingCalcFlatAndUnit SET TM_Flat = 20, TM_Unit = 60 WHERE TM_PK = @pk"))
				{
					command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pks["flatItem"]);
					command.ExecuteNonQuery();
				}
			}

			public void TestSetsFlat()
			{
				var item = generator.GetRateLineItem(pks["flatItem"], "RatingCalcFlatAndUnit");
				AssertEquals(20m, item["TM_Flat"]);
			}

			public void TestSetsUnit()
			{
				var item = generator.GetRateLineItem(pks["flatItem"], "RatingCalcFlatAndUnit");
				AssertEquals(60m, item["TM_Unit"]);
			}

			public void TestSetsTM_Unit_IsNewGuid()
			{
				var item = generator.GetRateLineItem(pks["flatItem"], "RatingCalcFlatAndUnit");
				pks.AssertIsNewGuid(item["TM_TM_Unit"]);
			}
		}

		class LineWithUnitOnly : TransactionedTestCase
		{
			RatingGeneratorForTests generator;
			GuidHelper pks;

			protected override void SetUp()
			{
				base.SetUp();

				generator = new RatingGeneratorForTests(TestConnection);
				pks = new GuidHelper();
				pks["rateLine"] = generator.NewRateLine();
				pks["unitItem"] = generator.NewRateLineItem(pks["rateLine"], "UNT");

				using (var command = TestConnection.Command("UPDATE dbo.RatingCalcFlatAndUnit SET TM_Flat = 20, TM_Unit = 60 WHERE TM_PK = @pk"))
				{
					command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pks["unitItem"]);
					command.ExecuteNonQuery();
				}
			}

			public void TestSetsFlat()
			{
				var item = generator.GetRateLineItem(pks["unitItem"], "RatingCalcFlatAndUnit");
				AssertEquals(20m, item["TM_Flat"]);
			}

			public void TestSetsUnit()
			{
				var item = generator.GetRateLineItem(pks["unitItem"], "RatingCalcFlatAndUnit");
				AssertEquals(60m, item["TM_Unit"]);
			}

			public void TestSetsTM_Unit_IsNewGuid()
			{
				var item = generator.GetRateLineItem(pks["unitItem"], "RatingCalcFlatAndUnit");
				pks.AssertIsNewGuid(item["TM_TM_Unit"]);
			}
		}
	}
}

