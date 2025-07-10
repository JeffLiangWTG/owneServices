using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class CusInBondMoveDetailLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUnloadedStateLookup_IsNew()
		{
			CombineAssertions(() =>
			{
				var unloadedStateList = moveDetail.Lookups.UnloadedStatesList;
				AssertEquals("Unloaded States CodesAsString", "DEC, DIF, MIS, NEW", unloadedStateList.CodesAsString);
				AssertSame("Cached", unloadedStateList, moveDetail.Lookups.UnloadedStatesList);
			});
		}

		public void TestUnloadedStateLookup()
		{
			CombineAssertions(() =>
			{
				var nctsHeader = Factory.New<NctsHeader>();
				var movement = Factory.New<NctsArrivalMovementHeader>();
				movement.BM_BH = nctsHeader.PK;
				moveDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				moveDetail.B9_BM = movement.PK;
				Factory.Save();

				var unloadedStateList = lookups.UnloadedStatesList;
				AssertEquals("Values", "DEC, DIF, MIS", unloadedStateList.CodesAsString);
				AssertSame("Cached", unloadedStateList, moveDetail.Lookups.UnloadedStatesList);
			});
		}

		public void TestVessels()
		{
			AssertType<RefVesselCollection>(lookups.Vessels);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			moveDetail = header.Bills.AddNew().MovementDetail;
			lookups = moveDetail.Lookups;
		}
		CusInBondMoveDetail moveDetail;
		CusInBondMoveDetailLookups lookups;
	}
}
