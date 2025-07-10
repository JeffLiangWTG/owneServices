using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	public class NctsPackageLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUnloadedStatesList_IsNew()
		{
			var list = package.Lookups.UnloadedStates;
			Factory.TryGetValueFromCacheOnly("UnloadedStatesCore_True_Y", out CodeDescriptionPairList cachedList);

			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "DEC, DIF, MIS, NEW", list.CodesAsString);
				AssertSame("Cached", list, cachedList);
			});
		}

		public void TestUnloadedStatesList()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
			var list = package.Lookups.UnloadedStates;
			Factory.TryGetValueFromCacheOnly("UnloadedStatesCore_False_Y", out CodeDescriptionPairList cachedList);

			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "DEC, DIF, MIS", list.CodesAsString);
				AssertSame("Cached", list, cachedList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			package = goodsItem.Packages.AddNew();
		}

		NctsHeader nctsHeader;
		NctsPackage package;
	}
}
