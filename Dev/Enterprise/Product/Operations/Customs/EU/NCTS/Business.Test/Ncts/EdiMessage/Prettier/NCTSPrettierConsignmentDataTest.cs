using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NCTSPrettierConsignmentDataTest : TestCase
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("null goodsItems", () => new NCTSPrettierConsignmentData(ucrReference: "testReference", goodsItems: null));
		}

		public void TestUCRReference() => AssertEquals("testReference", prettierConsignmentData.UCRReference);

		public void TestGoodsItems() => AssertContainsExactElementsInExactOrder(goodsItems, prettierConsignmentData.GoodsItems);

		protected override void SetUp()
		{
			base.SetUp();

			goodsItems = new List<INCTSPrettierGoodsItemData> {
				new NCTSPrettierGoodsItemData(itemNumber: "TestNumber", ucrReference: "TestReference", description: "TestDescription", harmonizedSubHeadingCode: "TestSubHeadingCode")
			};
			prettierConsignmentData = new NCTSPrettierConsignmentData("testReference", goodsItems);
		}

		NCTSPrettierConsignmentData prettierConsignmentData;
		IReadOnlyCollection<INCTSPrettierGoodsItemData> goodsItems;
	}
}
