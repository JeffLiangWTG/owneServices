using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NctsPackageLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUnloadedStates()
		{
			var package = goodsItem.Packages.AddNew();
			package.UnloadedStatus = NctsUnloadedStateList.Codes.MIS;
			CombineAssertions(() =>
			{
				var statusList = package.Lookups.UnloadedStates;
				Factory.TryGetValueFromCacheOnly("UnloadedStatesCore_False_Y", out CodeDescriptionPairList cachedList);

				AssertEquals("StatusList CodesAsString", "DEC, DIF, MIS", statusList.CodesAsString);
				AssertEquals("DEC Description", "As Declared", statusList.GetDescriptionFromCode(NctsUnloadedStateList.Codes.DEC));
				AssertEquals("DIF Description", "Differences to Declared", statusList.GetDescriptionFromCode(NctsUnloadedStateList.Codes.DIF));
				AssertEquals("MIS Description", "Missing", statusList.GetDescriptionFromCode(NctsUnloadedStateList.Codes.MIS));
				AssertSame("Cached", statusList, cachedList);

				package.UnloadedStatus = NctsUnloadedStateList.Codes.NEW;
				statusList = package.Lookups.UnloadedStates;
				Factory.TryGetValueFromCacheOnly("UnloadedStatesCore_True_Y", out cachedList);

				AssertEquals("StatusList CodesAsString", "DEC, DIF, MIS, NEW", statusList.CodesAsString);
				AssertEquals("NEW Description", "New", statusList.GetDescriptionFromCode(NctsUnloadedStateList.Codes.NEW));
				AssertSame("Cached", statusList, cachedList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var bill = nctsHeader.Bills.AddNew();
			goodsItem = bill.ArrivalGoodsItems.AddNew();
		}

		NctsArrivalCargoDesc goodsItem;
	}
}
