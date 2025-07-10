using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class UnloadingRemarkAddInfoLookupsTestUnloadingRemarkAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestYesNoCodeList()
		{
			var list = lookups.YesNoCodeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "N, Y", list.CodesAsString);
				AssertSame("Cached", list, lookups.YesNoCodeList);
			});
		}

		public void TestYesNoEmptyCodeList()
		{
			var list = lookups.YesNoEmptyCodeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "N, Y, ", list.CodesAsString);
				AssertSame("Cached", list, lookups.YesNoEmptyCodeList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var unloadingRemarkAddInfo = header.UnloadingRemark;
			lookups = new UnloadingRemarkAddInfoLookups(unloadingRemarkAddInfo);
		}

		UnloadingRemarkAddInfoLookups lookups;
	}
}
