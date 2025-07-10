using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using Enterprise.Build.Database.Script.Public.Glow.TestHelpers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow.Testing
{
	[TestedType(typeof(RatingCalcSliding))]
	class RatingCalcSlidingTest : DbCreateScriptTest
	{
		abstract class TestCase : TransactionedTestCase
		{
			protected Tuple<GuidHelper, DataRow> GetItemCore(
				string lineKey,
				string accItemAllText = "",
				string hclItemAllText = "",
				string inbItemAllText = "",
				string accItemAccText = "",
				string accItemAccHclText = "",
				string hclItemAccHclText = "",
				string accItemAccInbText = "",
				string inbItemAccInbText = "",
				string hclItemHclText = "",
				string hclItemHclInbText = "",
				string inbItemHclInbText = "",
				string inbItemInbText = "")
			{
				var generator = new RatingGeneratorForTests(TestConnection);
				var pks = new GuidHelper();

				pks["rateLineAll"] = generator.NewRateLine();
				pks["accItemAll"] = generator.NewRateLineItem(pks["rateLineAll"], "ACC", text: accItemAllText);
				pks["hclItemAll"] = generator.NewRateLineItem(pks["rateLineAll"], "HCL", text: hclItemAllText);
				pks["inbItemAll"] = generator.NewRateLineItem(pks["rateLineAll"], "INB", text: inbItemAllText);

				pks["rateLineAcc"] = generator.NewRateLine();
				pks["accItemAcc"] = generator.NewRateLineItem(pks["rateLineAcc"], "ACC", text: accItemAccText);

				pks["rateLineAccHcl"] = generator.NewRateLine();
				pks["accItemAccHcl"] = generator.NewRateLineItem(pks["rateLineAccHcl"], "ACC", text: accItemAccHclText);
				pks["hclItemAccHcl"] = generator.NewRateLineItem(pks["rateLineAccHcl"], "HCL", text: hclItemAccHclText);

				pks["rateLineAccInb"] = generator.NewRateLine();
				pks["accItemAccInb"] = generator.NewRateLineItem(pks["rateLineAccInb"], "ACC", text: accItemAccInbText);
				pks["inbItemAccInb"] = generator.NewRateLineItem(pks["rateLineAccInb"], "INB", text: inbItemAccInbText);

				pks["rateLineHcl"] = generator.NewRateLine();
				pks["hclItemHcl"] = generator.NewRateLineItem(pks["rateLineHcl"], "HCL", text: hclItemHclText);

				pks["rateLineHclInb"] = generator.NewRateLine();
				pks["hclItemHclInb"] = generator.NewRateLineItem(pks["rateLineHclInb"], "HCL", text: hclItemHclInbText);
				pks["inbItemHclInb"] = generator.NewRateLineItem(pks["rateLineHclInb"], "INB", text: inbItemHclInbText);

				pks["rateLineInb"] = generator.NewRateLine();
				pks["inbItemInb"] = generator.NewRateLineItem(pks["rateLineInb"], "INB", text: inbItemInbText);

				var commandText = string.Format("SELECT * FROM dbo.RatingCalcSliding WHERE TM_TL = ('{0}')", pks[lineKey]);
				var table = DataUtils.GetDataTableFromQuery(TestConnection, commandText);
				var item = table.Rows.Cast<DataRow>().Single();
				return Tuple.Create(pks, item);
			}
		}

		class LineWithAllItems : TestCase
		{
			public void TestSelectsPKFromAccItem()
			{
				var item = GetItem();
				var key = item.Item1.Key(item.Item2["TM_PK"]);
				AssertEquals("accItemAll", key);
			}

			public void TestSelectsTM_HigherChargeableLowerRateFromHclItem()
			{
				var item = GetItem();
				var key = item.Item1.Key(item.Item2["TM_TM_HigherChargeableLowerRate"]);
				AssertEquals("hclItemAll", key);
			}

			public void TestSelectsTM_UseInclusiveBreaksFromInbItem()
			{
				var item = GetItem();
				var key = item.Item1.Key(item.Item2["TM_TM_UseInclusiveBreaks"]);
				AssertEquals("inbItemAll", key);
			}

			public void TestSelectsUseAccumulationFromAccItem_True()
			{
				var item = GetItem(accItemText: "Y");
				AssertEquals(true, item.Item2["TM_UseAccumulation"]);
			}

			public void TestSelectsUseAccumulationFromAccItem_False()
			{
				var item = GetItem(accItemText: "N");
				AssertEquals(false, item.Item2["TM_UseAccumulation"]);
			}

			public void TestSelectsHigherChargeableLowerRateFromHclItem_True()
			{
				var item = GetItem(hclItemText: "Y");
				AssertEquals(true, item.Item2["TM_HigherChargeableLowerRate"]);
			}

			public void TestSelectsHigherChargeableLowerRateFromHclItem_False()
			{
				var item = GetItem(hclItemText: "N");
				AssertEquals(false, item.Item2["TM_HigherChargeableLowerRate"]);
			}

			public void TestSelectsUseInclusiveBreaksFromInbItem_True()
			{
				var item = GetItem(inbItemText: "Y");
				AssertEquals(true, item.Item2["TM_UseInclusiveBreaks"]);
			}

			public void TestSelectsUseInclusiveBreaksFromInbItem_False()
			{
				var item = GetItem(inbItemText: "N");
				AssertEquals(false, item.Item2["TM_UseInclusiveBreaks"]);
			}

			Tuple<GuidHelper, DataRow> GetItem(string accItemText = "", string hclItemText = "", string inbItemText = "")
			{
				return GetItemCore("rateLineAll", accItemAllText: accItemText, hclItemAllText: hclItemText, inbItemAllText: inbItemText);
			}
		}

		class LineWithAccOnly : TestCase
		{
			public void TestSelectsPKFromAccItem()
			{
				var item = GetItem();
				var key = item.Item1.Key(item.Item2["TM_PK"]);
				AssertEquals("accItemAcc", key);
			}

			public void TestSelectsTM_HigherChargeableLowerRateAsNull()
			{
				var item = GetItem();
				AssertEquals(DBNull.Value, item.Item2["TM_TM_HigherChargeableLowerRate"]);
			}

			public void TestSelectsTM_UseInclusiveBreaksAsNull()
			{
				var item = GetItem();
				AssertEquals(DBNull.Value, item.Item2["TM_TM_UseInclusiveBreaks"]);
			}

			public void TestSelectsUseAccumulationFromAccItem_True()
			{
				var item = GetItem("Y");
				AssertEquals(true, item.Item2["TM_UseAccumulation"]);
			}

			public void TestSelectsUseAccumulationFromAccItem_False()
			{
				var item = GetItem("N");
				AssertEquals(false, item.Item2["TM_UseAccumulation"]);
			}

			public void TestSelectsHigherChargeableLowerRateAsFalse()
			{
				var item = GetItem();
				AssertEquals(false, item.Item2["TM_HigherChargeableLowerRate"]);
			}

			public void TestSelectsUseInclusiveBreaksAsFalse()
			{
				var item = GetItem();
				AssertEquals(false, item.Item2["TM_UseInclusiveBreaks"]);
			}

			Tuple<GuidHelper, DataRow> GetItem(string accItemText = "")
			{
				return GetItemCore("rateLineAcc", accItemAccText: accItemText);
			}
		}

		class LineWithAccAndHcl : TestCase
		{
			public void TestSelectsPKFromAccItem()
			{
				var item = GetItem();
				var key = item.Item1.Key(item.Item2["TM_PK"]);
				AssertEquals("accItemAccHcl", key);
			}

			public void TestSelectsTM_HigherChargeableLowerRateFromHclItem()
			{
				var item = GetItem();
				var key = item.Item1.Key(item.Item2["TM_TM_HigherChargeableLowerRate"]);
				AssertEquals("hclItemAccHcl", key);
			}

			public void TestSelectsTM_UseInclusiveBreaksAsNull()
			{
				var item = GetItem();
				AssertEquals(DBNull.Value, item.Item2["TM_TM_UseInclusiveBreaks"]);
			}

			public void TestSelectsUseAccumulationFromAccItem_True()
			{
				var item = GetItem(accItemText: "Y");
				AssertEquals(true, item.Item2["TM_UseAccumulation"]);
			}

			public void TestSelectsUseAccumulationFromAccItem_False()
			{
				var item = GetItem(accItemText: "N");
				AssertEquals(false, item.Item2["TM_UseAccumulation"]);
			}

			public void TestSelectsHigherChargeableLowerRateFromHclItem_True()
			{
				var item = GetItem(hclItemText: "Y");
				AssertEquals(true, item.Item2["TM_HigherChargeableLowerRate"]);
			}

			public void TestSelectsHigherChargeableLowerRateFromHclItem_False()
			{
				var item = GetItem(hclItemText: "N");
				AssertEquals(false, item.Item2["TM_HigherChargeableLowerRate"]);
			}

			public void TestSelectsUseInclusiveBreaksAsFalse()
			{
				var item = GetItem();
				AssertEquals(false, item.Item2["TM_UseInclusiveBreaks"]);
			}

			Tuple<GuidHelper, DataRow> GetItem(string accItemText = "", string hclItemText = "")
			{
				return GetItemCore("rateLineAccHcl", accItemAccHclText: accItemText, hclItemAccHclText: hclItemText);
			}
		}

		class LineWithAccAndInb : TestCase
		{
			public void TestSelectsPKFromAccItem()
			{
				var item = GetItem();
				var key = item.Item1.Key(item.Item2["TM_PK"]);
				AssertEquals("accItemAccInb", key);
			}

			public void TestSelectsTM_HigherChargeableLowerRateAsNull()
			{
				var item = GetItem();
				AssertEquals(DBNull.Value, item.Item2["TM_TM_HigherChargeableLowerRate"]);
			}

			public void TestSelectsTM_UseInclusiveBreaksFromInbItem()
			{
				var item = GetItem();
				var key = item.Item1.Key(item.Item2["TM_TM_UseInclusiveBreaks"]);
				AssertEquals("inbItemAccInb", key);
			}

			public void TestSelectsUseAccumulationFromAccItem_True()
			{
				var item = GetItem(accItemText: "Y");
				AssertEquals(true, item.Item2["TM_UseAccumulation"]);
			}

			public void TestSelectsUseAccumulationFromAccItem_False()
			{
				var item = GetItem(accItemText: "N");
				AssertEquals(false, item.Item2["TM_UseAccumulation"]);
			}

			public void TestSelectsHigherChargeableLowerRateAsFalse()
			{
				var item = GetItem();
				AssertEquals(false, item.Item2["TM_HigherChargeableLowerRate"]);
			}

			public void TestSelectsUseInclusiveBreaksFromInbItem_True()
			{
				var item = GetItem(inbItemText: "Y");
				AssertEquals(true, item.Item2["TM_UseInclusiveBreaks"]);
			}

			public void TestSelectsUseInclusiveBreaksFromInbItem_False()
			{
				var item = GetItem(inbItemText: "N");
				AssertEquals(false, item.Item2["TM_UseInclusiveBreaks"]);
			}

			Tuple<GuidHelper, DataRow> GetItem(string accItemText = "", string inbItemText = "")
			{
				return GetItemCore("rateLineAccInb", accItemAccInbText: accItemText, inbItemAccInbText: inbItemText);
			}
		}

		class LineWithHclOnly : TestCase
		{
			public void TestSelectsPKFromHclItem()
			{
				var item = GetItem();
				var key = item.Item1.Key(item.Item2["TM_PK"]);
				AssertEquals("hclItemHcl", key);
			}

			public void TestSelectsTM_HigherChargeableLowerRateFromHclItem()
			{
				var item = GetItem();
				var key = item.Item1.Key(item.Item2["TM_TM_HigherChargeableLowerRate"]);
				AssertEquals("hclItemHcl", key);
			}

			public void TestSelectsTM_UseInclusiveBreaksAsNull()
			{
				var item = GetItem();
				AssertEquals(DBNull.Value, item.Item2["TM_TM_UseInclusiveBreaks"]);
			}

			public void TestSelectsUseAccumulationAsFalse()
			{
				var item = GetItem();
				AssertEquals(false, item.Item2["TM_UseAccumulation"]);
			}

			public void TestSelectsHigherChargeableLowerRateFromHclItem_True()
			{
				var item = GetItem("Y");
				AssertEquals(true, item.Item2["TM_HigherChargeableLowerRate"]);
			}

			public void TestSelectsHigherChargeableLowerRateFromHclItem_False()
			{
				var item = GetItem("N");
				AssertEquals(false, item.Item2["TM_HigherChargeableLowerRate"]);
			}

			public void TestSelectsUseInclusiveBreaksAsFalse()
			{
				var item = GetItem();
				AssertEquals(false, item.Item2["TM_UseInclusiveBreaks"]);
			}

			Tuple<GuidHelper, DataRow> GetItem(string hclItemText = "")
			{
				return GetItemCore("rateLineHcl", hclItemHclText: hclItemText);
			}
		}

		class LineWithHclAndInb : TestCase
		{
			public void TestSelectsPKFromHclItem()
			{
				var item = GetItem();
				var key = item.Item1.Key(item.Item2["TM_PK"]);
				AssertEquals("hclItemHclInb", key);
			}

			public void TestSelectsTM_HigherChargeableLowerRateFromHclItem()
			{
				var item = GetItem();
				var key = item.Item1.Key(item.Item2["TM_TM_HigherChargeableLowerRate"]);
				AssertEquals("hclItemHclInb", key);
			}

			public void TestSelectsTM_UseInclusiveBreaksFromInbItem()
			{
				var item = GetItem();
				var key = item.Item1.Key(item.Item2["TM_TM_UseInclusiveBreaks"]);
				AssertEquals("inbItemHclInb", key);
			}

			public void TestSelectsUseAccumulationAsFalse()
			{
				var item = GetItem();
				AssertEquals(false, item.Item2["TM_UseAccumulation"]);
			}

			public void TestSelectsHigherChargeableLowerRateFromHclItem_True()
			{
				var item = GetItem(hclItemText: "Y");
				AssertEquals(true, item.Item2["TM_HigherChargeableLowerRate"]);
			}

			public void TestSelectsHigherChargeableLowerRateFromHclItem_False()
			{
				var item = GetItem(hclItemText: "N");
				AssertEquals(false, item.Item2["TM_HigherChargeableLowerRate"]);
			}

			public void TestSelectsUseInclusiveBreaksFromInbItem_True()
			{
				var item = GetItem(inbItemText: "Y");
				AssertEquals(true, item.Item2["TM_UseInclusiveBreaks"]);
			}

			public void TestSelectsUseInclusiveBreaksFromInbItem_False()
			{
				var item = GetItem(inbItemText: "N");
				AssertEquals(false, item.Item2["TM_UseInclusiveBreaks"]);
			}

			Tuple<GuidHelper, DataRow> GetItem(string hclItemText = "", string inbItemText = "")
			{
				return GetItemCore("rateLineHclInb", hclItemHclInbText: hclItemText, inbItemHclInbText: inbItemText);
			}
		}

		class LineWithInbOnly : TestCase
		{
			public void TestSelectsPKFromInbItem()
			{
				var item = GetItem();
				var key = item.Item1.Key(item.Item2["TM_PK"]);
				AssertEquals("inbItemInb", key);
			}

			public void TestSelectsTM_HigherChargeableLowerRateAsNull()
			{
				var item = GetItem();
				AssertEquals(DBNull.Value, item.Item2["TM_TM_HigherChargeableLowerRate"]);
			}

			public void TestSelectsTM_UseInclusiveBreaksFromInbItem()
			{
				var item = GetItem();
				var key = item.Item1.Key(item.Item2["TM_TM_UseInclusiveBreaks"]);
				AssertEquals("inbItemInb", key);
			}

			public void TestSelectsUseAccumulationAsFalse()
			{
				var item = GetItem();
				AssertEquals(false, item.Item2["TM_UseAccumulation"]);
			}

			public void TestSelectsHigherChargeableLowerRateAsFalse()
			{
				var item = GetItem();
				AssertEquals(false, item.Item2["TM_HigherChargeableLowerRate"]);
			}

			public void TestSelectsUseInclusiveBreaksFromInbItem_True()
			{
				var item = GetItem("Y");
				AssertEquals(true, item.Item2["TM_UseInclusiveBreaks"]);
			}

			public void TestSelectsUseInclusiveBreaksFromInbItem_False()
			{
				var item = GetItem("N");
				AssertEquals(false, item.Item2["TM_UseInclusiveBreaks"]);
			}

			Tuple<GuidHelper, DataRow> GetItem(string inbItemText = "")
			{
				return GetItemCore("rateLineInb", inbItemInbText: inbItemText);
			}
		}
	}
}

