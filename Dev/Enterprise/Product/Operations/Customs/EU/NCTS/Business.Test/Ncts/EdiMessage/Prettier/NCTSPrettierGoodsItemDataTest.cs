using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NCTSPrettierGoodsItemDataTest : TestCase
	{
		public void TestItemNumber() => AssertEquals("TestNumber", prettierGoodsItemData.ItemNumber);
		public void TestUCRReference() => AssertEquals("TestReference", prettierGoodsItemData.UCRReference);
		public void TestDescription() => AssertEquals("TestDescription", prettierGoodsItemData.Description);
		public void TestHarmonizedSubHeadingCode() => AssertEquals("TestSubHeadingCode", prettierGoodsItemData.HarmonizedSubHeadingCode);

		protected override void SetUp()
		{
			base.SetUp();

			prettierGoodsItemData = new NCTSPrettierGoodsItemData(
				itemNumber: "TestNumber",
				ucrReference: "TestReference",
				description: "TestDescription",
				harmonizedSubHeadingCode: "TestSubHeadingCode");
		}

		NCTSPrettierGoodsItemData prettierGoodsItemData;
	}
}
