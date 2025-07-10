using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsAdditionalInfoCollection<NctsAdditionalInfo>))]
	class NctsAdditionalInfoCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
			return new NctsAdditionalInfoCollection<NctsAdditionalInfo>(goodsItem);
		}

		public void TestNotFilteredOnSubType()
		{
			var collection = (NctsAdditionalInfoCollection<NctsAdditionalInfo>)GetCollectionToTest();
			var addInfo1 = collection.AddNew();
			addInfo1.CSI_SubType = "TRA";

			var addInfo2 = collection.AddNew();
			addInfo2.CSI_SubType = "INF";

			var addInfo3 = collection.AddNew();
			addInfo3.CSI_SubType = "REF";

			var addInfo4 = collection.AddNew();
			addInfo4.CSI_SubType = "XYZ";

			Factory.Save();
			collection.Reload(true);
			CombineAssertions(() =>
			{
				AssertEquals("Query should not contain CSI_SubType", false, collection.CompleteFilter.ToString().Contains("CSI_SubType"));
				AssertEquals("Collection should contain all 4 records", 4, collection.Count);
			});
		}
	}
}
