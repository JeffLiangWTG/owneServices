using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class CusSealLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUnloadedStatesList_IsNew()
		{
			var list = lookups.UnloadedStates;
			CombineAssertions(() =>
			{
				AssertEquals("Codes from list", "DAM, DEC, MIS, NEW", list.CodesAsString);
				AssertSame("Cached", list, lookups.UnloadedStates);
			});
		}

		public void TestUnloadedStatesList()
		{
			CombineAssertions(() =>
			{
				var nctsHeader = Factory.New<NctsHeader>();
				container.ParentType = typeof(NctsHeader);
				container.BK_ParentID = nctsHeader.PK;
				container.BK_ParentTableCode = nctsHeader.TablePrefix;
				container.BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;
				container.BK_SealNumber = "ABC";
				Factory.Save();

				var list = container.Lookups.UnloadedStates;
				AssertEquals("Codes from list", "DAM, DEC, MIS", list.CodesAsString);
				AssertSame("Cached", list, container.Lookups.UnloadedStates);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			container = Factory.New<CusSeal>();
			lookups = container.Lookups;
		}

		CusSeal container;
		CusSealLookups lookups;
	}
}
