using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using Enterprise.Build.Database.Script.Public.Glow.TestHelpers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow
{
	[TestedType(typeof(TG_UPD_RatingCalcSliding))]
	class TG_UPD_RatingCalcSlidingTest : DbCreateScriptTest
	{
		class LineWithAllItems : TransactionedTestCase
		{
			RatingGeneratorForTests generator;
			GuidHelper pks;

			protected override void SetUp()
			{
				base.SetUp();

				generator = new RatingGeneratorForTests(TestConnection);
				pks = new GuidHelper();
				pks["rateLine"] = generator.NewRateLine();
				pks["accItem"] = generator.NewRateLineItem(pks["rateLine"], "ACC");
				pks["hclItem"] = generator.NewRateLineItem(pks["rateLine"], "HCL");
				pks["inbItem"] = generator.NewRateLineItem(pks["rateLine"], "INB");
			}

			public void TestSetsUseAccumulation()
			{
				UpdateToTrue("TM_UseAccumulation");
				var item = generator.GetRateLineItem(pks["accItem"], "RateLineItems");
				AssertEquals("Y", item["TM_Text"]);
			}

			public void TestSetsHigherChargeableLowerRate()
			{
				UpdateToTrue("TM_HigherChargeableLowerRate");
				var item = generator.GetRateLineItem(pks["hclItem"], "RateLineItems");
				AssertEquals("Y", item["TM_Text"]);
			}

			public void TestSetsUseInclusiveBreaks()
			{
				UpdateToTrue("TM_UseInclusiveBreaks");
				var item = generator.GetRateLineItem(pks["inbItem"], "RateLineItems");
				AssertEquals("Y", item["TM_Text"]);
			}

			public void TestThrowsErrorIfUpdatingMultipleRows()
			{
				pks["rateLine2"] = generator.NewRateLine();
				pks["accItem"] = generator.NewRateLineItem(pks["rateLine2"], "ACC");

				using (var command = TestConnection.Command("UPDATE dbo.RatingCalcSliding SET TM_UseAccumulation = 1"))
				{
					AssertExceptionThrown(typeof(SqlException), "Cannot update more than 1 row at a time.", () => command.ExecuteNonQuery());
				}
			}

			public void TestThrowsErrorIfUpdatingPK()
			{
				AssertCannotUpdateKey("TM_PK");
			}

			public void TestThrowsErrorIfUpdatingTM_HigherChargeableLowerRate()
			{
				AssertCannotUpdateKey("TM_TM_HigherChargeableLowerRate");
			}

			public void TestThrowsErrorIfUpdatingTM_UseInclusiveBreaks()
			{
				AssertCannotUpdateKey("TM_TM_UseInclusiveBreaks");
			}

			public void TestThrowsErrorIfUpdatingTL()
			{
				AssertCannotUpdateKey("TM_TL");
			}

			void AssertCannotUpdateKey(string columnName)
			{
				var commandText = "UPDATE dbo.RatingCalcSliding SET " + columnName + " = NEWID() WHERE TM_PK = @pk";

				using (var command = TestConnection.Command(commandText))
				{
					command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pks["accItem"]);

					AssertExceptionThrown(
						typeof(SqlException),
						"Updating of TM_PK, TM_TM_HigherChargeableLowerRate, TM_TM_UseInclusiveBreaks or TM_TL is not allowed.",
						() => command.ExecuteNonQuery());
				}
			}

			void UpdateToTrue(string columnName)
			{
				var commandText = "UPDATE dbo.RatingCalcSliding SET " + columnName + " = 1 WHERE TM_PK = @pk";

				using (var command = TestConnection.Command(commandText))
				{
					command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pks["accItem"]);
					command.ExecuteNonQuery();
				}
			}
		}

		class LineWithAccOnly : TransactionedTestCase
		{
			RatingGeneratorForTests generator;
			GuidHelper pks;

			protected override void SetUp()
			{
				base.SetUp();

				generator = new RatingGeneratorForTests(TestConnection);
				pks = new GuidHelper();
				pks["rateLine"] = generator.NewRateLine();
				pks["accItem"] = generator.NewRateLineItem(pks["rateLine"], "ACC");
			}

			public void TestSetsUseAccumulation()
			{
				UpdateToTrue("TM_UseAccumulation");
				var item = generator.GetRateLineItem(pks["accItem"], "RatingCalcSliding");
				AssertEquals(true, item["TM_UseAccumulation"]);
			}

			public void TestSetsHigherChargeableLowerRate()
			{
				UpdateToTrue("TM_HigherChargeableLowerRate");
				var item = generator.GetRateLineItem(pks["accItem"], "RatingCalcSliding");
				AssertEquals(true, item["TM_HigherChargeableLowerRate"]);
			}

			public void TestSetsUseInclusiveBreaks()
			{
				UpdateToTrue("TM_UseInclusiveBreaks");
				var item = generator.GetRateLineItem(pks["accItem"], "RatingCalcSliding");
				AssertEquals(true, item["TM_UseInclusiveBreaks"]);
			}

			public void TestSetsTM_HigherChargeableLowerRate_IsNewGuid()
			{
				UpdateToTrue("TM_UseAccumulation");
				var item = generator.GetRateLineItem(pks["accItem"], "RatingCalcSliding");
				pks.AssertIsNewGuid(item["TM_TM_HigherChargeableLowerRate"]);
			}

			public void TestSetsTM_UseInclusiveBreaks_NotEmpty()
			{
				UpdateToTrue("TM_UseAccumulation");
				var item = generator.GetRateLineItem(pks["accItem"], "RatingCalcSliding");
				pks.AssertIsNewGuid(item["TM_TM_UseInclusiveBreaks"]);
			}

			void UpdateToTrue(string columnName)
			{
				var commandText = "UPDATE dbo.RatingCalcSliding SET " + columnName + " = 1 WHERE TM_PK = @pk";

				using (var command = TestConnection.Command(commandText))
				{
					command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pks["accItem"]);
					command.ExecuteNonQuery();
				}
			}
		}

		class LineWithHclOnly : TransactionedTestCase
		{
			RatingGeneratorForTests generator;
			GuidHelper pks;

			protected override void SetUp()
			{
				base.SetUp();

				generator = new RatingGeneratorForTests(TestConnection);
				pks = new GuidHelper();
				pks["rateLine"] = generator.NewRateLine();
				pks["hclItem"] = generator.NewRateLineItem(pks["rateLine"], "HCL");
			}

			public void TestSetsUseAccumulation()
			{
				UpdateToTrue("TM_UseAccumulation");
				var item = generator.GetRateLineItem(pks["hclItem"], "RatingCalcSliding");
				AssertEquals(true, item["TM_UseAccumulation"]);
			}

			public void TestSetsHigherChargeableLowerRate()
			{
				UpdateToTrue("TM_HigherChargeableLowerRate");
				var item = generator.GetRateLineItem(pks["hclItem"], "RatingCalcSliding");
				AssertEquals(true, item["TM_HigherChargeableLowerRate"]);
			}

			public void TestSetsUseInclusiveBreaks()
			{
				UpdateToTrue("TM_UseInclusiveBreaks");
				var item = generator.GetRateLineItem(pks["hclItem"], "RatingCalcSliding");
				AssertEquals(true, item["TM_UseInclusiveBreaks"]);
			}

			public void TestSetsTM_HigherChargeableLowerRate_IsNewGuid()
			{
				UpdateToTrue("TM_UseAccumulation");
				var item = generator.GetRateLineItem(pks["hclItem"], "RatingCalcSliding");
				pks.AssertIsNewGuid(item["TM_TM_HigherChargeableLowerRate"]);
			}

			public void TestSetsTM_UseInclusiveBreaks_NotEmpty()
			{
				UpdateToTrue("TM_UseAccumulation");
				var item = generator.GetRateLineItem(pks["hclItem"], "RatingCalcSliding");
				pks.AssertIsNewGuid(item["TM_TM_UseInclusiveBreaks"]);
			}

			void UpdateToTrue(string columnName)
			{
				var commandText = "UPDATE dbo.RatingCalcSliding SET " + columnName + " = 1 WHERE TM_PK = @pk";

				using (var command = TestConnection.Command(commandText))
				{
					command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pks["hclItem"]);
					command.ExecuteNonQuery();
				}
			}
		}

		class LineWithInbOnly : TransactionedTestCase
		{
			RatingGeneratorForTests generator;
			GuidHelper pks;

			protected override void SetUp()
			{
				base.SetUp();

				generator = new RatingGeneratorForTests(TestConnection);
				pks = new GuidHelper();
				pks["rateLine"] = generator.NewRateLine();
				pks["inbItem"] = generator.NewRateLineItem(pks["rateLine"], "INB");
			}

			public void TestSetsUseAccumulation()
			{
				UpdateToTrue("TM_UseAccumulation");
				var item = generator.GetRateLineItem(pks["inbItem"], "RatingCalcSliding");
				AssertEquals(true, item["TM_UseAccumulation"]);
			}

			public void TestSetsHigherChargeableLowerRate()
			{
				UpdateToTrue("TM_HigherChargeableLowerRate");
				var item = generator.GetRateLineItem(pks["inbItem"], "RatingCalcSliding");
				AssertEquals(true, item["TM_HigherChargeableLowerRate"]);
			}

			public void TestSetsUseInclusiveBreaks()
			{
				UpdateToTrue("TM_UseInclusiveBreaks");
				var item = generator.GetRateLineItem(pks["inbItem"], "RatingCalcSliding");
				AssertEquals(true, item["TM_UseInclusiveBreaks"]);
			}

			public void TestSetsTM_HigherChargeableLowerRate_IsNewGuid()
			{
				UpdateToTrue("TM_UseAccumulation");
				var item = generator.GetRateLineItem(pks["inbItem"], "RatingCalcSliding");
				pks.AssertIsNewGuid(item["TM_TM_HigherChargeableLowerRate"]);
			}

			public void TestSetsTM_UseInclusiveBreaks_NotEmpty()
			{
				UpdateToTrue("TM_UseAccumulation");
				var item = generator.GetRateLineItem(pks["inbItem"], "RatingCalcSliding");
				pks.AssertIsNewGuid(item["TM_TM_UseInclusiveBreaks"]);
			}

			void UpdateToTrue(string columnName)
			{
				var commandText = "UPDATE dbo.RatingCalcSliding SET " + columnName + " = 1 WHERE TM_PK = @pk";

				using (var command = TestConnection.Command(commandText))
				{
					command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pks["inbItem"]);
					command.ExecuteNonQuery();
				}
			}
		}
	}
}

