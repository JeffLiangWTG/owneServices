using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using Enterprise.Build.Database.Script.Public.Glow.TestHelpers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow
{
	[TestedType(typeof(TG_UPD_RatingCalcMinAndUnit))]
	class TG_UPD_RatingCalcMinAndUnitTest : DbCreateScriptTest
	{
		class LineWithMinAndUnit : TransactionedTestCase
		{
			RatingGeneratorForTests generator;
			GuidHelper pks;

			protected override void SetUp()
			{
				base.SetUp();

				generator = new RatingGeneratorForTests(TestConnection);
				pks = new GuidHelper();
				pks["rateLine"] = generator.NewRateLine();
				pks["minItem"] = generator.NewRateLineItem(pks["rateLine"], "MIN");
				pks["unitItem"] = generator.NewRateLineItem(pks["rateLine"], "UNT");
			}

			public void TestSetsMin()
			{
				Update();
				var item = generator.GetRateLineItem(pks["minItem"], "RateLineItems");
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
				pks["minItem2"] = generator.NewRateLineItem(pks["rateLine2"], "MIN");

				using (var command = TestConnection.Command("UPDATE dbo.RatingCalcMinAndUnit SET TM_Min = 20"))
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
				var commandText = "UPDATE dbo.RatingCalcMinAndUnit SET " + columnName + " = NEWID() WHERE TM_PK = @pk";

				using (var command = TestConnection.Command(commandText))
				{
					command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pks["minItem"]);

					AssertExceptionThrown(
						typeof(SqlException),
						"Updating of TM_PK, TM_TM_Unit or TM_TL is not allowed.",
						() => command.ExecuteNonQuery());
				}
			}

			void Update()
			{
				using (var command = TestConnection.Command("UPDATE dbo.RatingCalcMinAndUnit SET TM_Min = 20, TM_Unit = 60 WHERE TM_PK = @pk"))
				{
					command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pks["minItem"]);
					command.ExecuteNonQuery();
				}
			}
		}

		class LineWithMinOnly : TransactionedTestCase
		{
			RatingGeneratorForTests generator;
			GuidHelper pks;

			protected override void SetUp()
			{
				base.SetUp();

				generator = new RatingGeneratorForTests(TestConnection);
				pks = new GuidHelper();
				pks["rateLine"] = generator.NewRateLine();
				pks["minItem"] = generator.NewRateLineItem(pks["rateLine"], "MIN");

				using (var command = TestConnection.Command("UPDATE dbo.RatingCalcMinAndUnit SET TM_Min = 20, TM_Unit = 60 WHERE TM_PK = @pk"))
				{
					command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pks["minItem"]);
					command.ExecuteNonQuery();
				}
			}

			public void TestSetsMin()
			{
				var item = generator.GetRateLineItem(pks["minItem"], "RatingCalcMinAndUnit");
				AssertEquals(20m, item["TM_Min"]);
			}

			public void TestSetsUnit()
			{
				var item = generator.GetRateLineItem(pks["minItem"], "RatingCalcMinAndUnit");
				AssertEquals(60m, item["TM_Unit"]);
			}

			public void TestSetsTM_Unit_IsNewGuid()
			{
				var item = generator.GetRateLineItem(pks["minItem"], "RatingCalcMinAndUnit");
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

				using (var command = TestConnection.Command("UPDATE dbo.RatingCalcMinAndUnit SET TM_Min = 20, TM_Unit = 60 WHERE TM_PK = @pk"))
				{
					command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pks["unitItem"]);
					command.ExecuteNonQuery();
				}
			}

			public void TestSetsMin()
			{
				var item = generator.GetRateLineItem(pks["unitItem"], "RatingCalcMinAndUnit");
				AssertEquals(20m, item["TM_Min"]);
			}

			public void TestSetsUnit()
			{
				var item = generator.GetRateLineItem(pks["unitItem"], "RatingCalcMinAndUnit");
				AssertEquals(60m, item["TM_Unit"]);
			}

			public void TestSetsTM_Unit_IsNewGuid()
			{
				var item = generator.GetRateLineItem(pks["unitItem"], "RatingCalcMinAndUnit");
				pks.AssertIsNewGuid(item["TM_TM_Unit"]);
			}
		}
	}
}

