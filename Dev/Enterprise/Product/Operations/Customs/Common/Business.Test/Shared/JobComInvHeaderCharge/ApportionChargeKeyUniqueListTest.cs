using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Common.Testing
{
	class TestApportionChargeKey : ApportionChargeKey
	{
		public TestApportionChargeKey(string chargeName, bool isDutiable, bool isGSTible, string apportionType, string isIncludedInITOT, string isIncludedInInvoice, string distributeBy, decimal percentage)
			: base(chargeName, isDutiable, isGSTible, apportionType, isIncludedInITOT, isIncludedInInvoice, distributeBy, percentage, false, "", false, false)
		{
		}
	}

	class ApportionChargeKeyUniqueListTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestAddUniquely()
		{
			var list = new ApportionChargeKeyUniqueList();
			ApportionChargeKey oFTFullKey = new TestApportionChargeKey(CustomsChargeTypeList.Codes.OverseasFreight, false, true, ApportionmentTypeList.Codes.FullApportionment, GroupIsIncludedInLinesOptionList.Codes.No, GroupIsIncludedInLinesOptionList.Codes.No, ChargeDistributeByList.Codes.Value, 0m);
			ApportionChargeKey oFTFullKey2 = new TestApportionChargeKey(CustomsChargeTypeList.Codes.OverseasFreight, false, true, ApportionmentTypeList.Codes.FullApportionment, GroupIsIncludedInLinesOptionList.Codes.No, GroupIsIncludedInLinesOptionList.Codes.No, ChargeDistributeByList.Codes.Value, 0m);
			ApportionChargeKey oFTPartialKey = new TestApportionChargeKey(CustomsChargeTypeList.Codes.OverseasFreight, false, true, ApportionmentTypeList.Codes.PartialApportionment, GroupIsIncludedInLinesOptionList.Codes.No, GroupIsIncludedInLinesOptionList.Codes.No, ChargeDistributeByList.Codes.Value, 0m);

			list.AddUniquely(oFTFullKey);
			NUnit.Framework.Assert.That(list.InternalUniqueList.Keys.Count, Is.EqualTo(1), "There should be one item in the hashtable");

			list.AddUniquely(oFTFullKey2);
			NUnit.Framework.Assert.That(list.InternalUniqueList.Keys.Count, Is.EqualTo(1), "There should be one item in the hashtable");

			list.AddUniquely(oFTPartialKey);
			NUnit.Framework.Assert.That(list.InternalUniqueList.Count, Is.EqualTo(2), "There should be two items in the internal list");

			list.AddUniquely(new ApportionChargeKey[] { oFTFullKey, oFTFullKey2, oFTPartialKey });
			NUnit.Framework.Assert.That(list.InternalUniqueList.Count, Is.EqualTo(2), "There should be two items in the internal list");
		}

		[ExpectNoExceptions]
		public void TestGetUniqueItems()
		{
			var list = new ApportionChargeKeyUniqueList();
			ApportionChargeKey oFTFullKey = new TestApportionChargeKey(CustomsChargeTypeList.Codes.OverseasFreight, false, true, ApportionmentTypeList.Codes.FullApportionment, GroupIsIncludedInLinesOptionList.Codes.No, GroupIsIncludedInLinesOptionList.Codes.No, ChargeDistributeByList.Codes.Value, 0m);
			ApportionChargeKey oFTFullKey2 = new TestApportionChargeKey(CustomsChargeTypeList.Codes.OverseasFreight, false, true, ApportionmentTypeList.Codes.FullApportionment, GroupIsIncludedInLinesOptionList.Codes.No, GroupIsIncludedInLinesOptionList.Codes.No, ChargeDistributeByList.Codes.Value, 0m);
			ApportionChargeKey oFTPartialKey = new TestApportionChargeKey(CustomsChargeTypeList.Codes.OverseasFreight, false, true, ApportionmentTypeList.Codes.PartialApportionment, GroupIsIncludedInLinesOptionList.Codes.No, GroupIsIncludedInLinesOptionList.Codes.No, ChargeDistributeByList.Codes.Value, 0m);
			list.AddUniquely(new ApportionChargeKey[] { oFTFullKey, oFTFullKey2, oFTPartialKey });

			var result = list.GetUniqueItems();
			NUnit.Framework.Assert.That(result.Length, Is.EqualTo(2), "Two items expected");
			NUnit.Framework.Assert.That(result[1], Is.EqualTo(oFTPartialKey), "OFT Partial Key");
			NUnit.Framework.Assert.That(result[0], Is.EqualTo(oFTFullKey), "OFT Full Key");
		}
	}
}
