using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ForeignOperatorCollection))]
	sealed class ForeignOperatorCollectionTest : ActiveBusinessObjectCollectionTestCase<ForeignOperatorCollection>
	{
		protected override ForeignOperatorCollection GetCollectionToTest()
		{
			var catalog = Factory.New<CusGoodsCatalog>();
			return catalog.ForeignOperators;
		}

		public override void TestDelete()
		{
			var goodsCatalog = Factory.New<CusGoodsCatalog>();

			var catalogProductionInfoACC = goodsCatalog.ForeignOperators.AddNew();
			catalogProductionInfoACC.CGI_CustomsStatus = CustomsPostedStatusList.Codes.Accepted;

			AssertEquals("ForeignOperators count should be 1", 1, goodsCatalog.ForeignOperators.Count);
			goodsCatalog.ForeignOperators.Delete(goodsCatalog.ForeignOperators[0]);
			Assert(!catalogProductionInfoACC.IsDeleted);
			AssertEquals("ForeignOperators count should be 1", 1, goodsCatalog.ForeignOperators.Count);
			AssertEquals("CGI_CustomsStatus should be ", CustomsPostedStatusList.Codes.DeletePending, catalogProductionInfoACC.CGI_CustomsStatus);

			var catalogProductionInfoUPD = goodsCatalog.ForeignOperators.AddNew();
			catalogProductionInfoUPD.CGI_CustomsStatus = CustomsPostedStatusList.Codes.UpdatePending;

			AssertEquals("ForeignOperators count should be 2", 2, goodsCatalog.ForeignOperators.Count);
			goodsCatalog.ForeignOperators.Delete(goodsCatalog.ForeignOperators[1]);
			Assert(catalogProductionInfoUPD.IsDeleted);
			AssertEquals("ForeignOperators count should be 2", 1, goodsCatalog.ForeignOperators.Count);

			var catalogProductionInfoACT = goodsCatalog.ForeignOperators.AddNew();
			catalogProductionInfoACT.CGI_CustomsStatus = CustomsPostedStatusList.Codes.Active;

			AssertEquals("ForeignOperators count should be 2", 2, goodsCatalog.ForeignOperators.Count);
			goodsCatalog.ForeignOperators.Delete(goodsCatalog.ForeignOperators[1]);
			Assert(catalogProductionInfoACT.IsDeleted);
			AssertEquals("ForeignOperators count should be 2", 1, goodsCatalog.ForeignOperators.Count);
		}
	}
}
