using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsPreviousDocumentLookupsTest : TestCaseWithFactory
	{
		public void TestSubTypeList()
		{
			CombineAssertions(() =>
			{
				var subTypeList = lookups.SubTypeList;
				AssertEquals("Codes", "Y, P, Z, X", subTypeList.CodesAsString);
				AssertSame("Cached", subTypeList, lookups.SubTypeList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			var previousDocument = goodsItem.PreviousDocuments.AddNew();
			lookups = new NctsPreviousDocumentLookupsForTest(previousDocument);
		}
		NctsPreviousDocumentLookupsForTest lookups;

		class NctsPreviousDocumentLookupsForTest : NctsPreviousDocumentLookups
		{
			public NctsPreviousDocumentLookupsForTest(NctsPreviousDocument parent)
				: base(parent)
			{
			}
		}
	}
}
