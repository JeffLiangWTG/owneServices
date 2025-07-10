using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class CusHAWBUnderbondMovementRequestLineTest : CusMAWBUnderbondMovementRequestLineTest
	{
		public override void TestHouseBillOfLading()
		{
			AssertEquals("HouseBillOfLading", ZString.Empty, Line.HouseBillOfLading);
		}

		public override void TestHouseBillOfLadingComesFromCM_MasterHouseBill()
		{
			MAWB.CM_MasterHouseBill = "321";
			AssertEquals("HouseBillOfLading", ZString.Empty, Line.HouseAirWaybillNumber);
		}

		public override void TestNumberOfPackages()
		{
			HAWB.CS_PiecesManifested = 10;
			AssertEquals("NumberOfPackages", 10, Line.NumberOfPackages);
		}

		CusHAWBUnderbondMovementRequestLine line;
		protected override CusMAWBUnderbondMovementRequestLine Line => line ?? (line = new CusHAWBUnderbondMovementRequestLine(HAWB));

		CusHAWB hawb;
		protected CusHAWB HAWB => hawb ?? (hawb = ((CusMAWB)MAWB).ChildBills.AddNew());
	}
}
