namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class CTOCusHAWBUnderbondMovementRequestLineTest : CusMAWBUnderbondMovementRequestLineTest
	{
		public override void TestMasterAirWaybillNumber()
		{
			HAWB.CS_HAWB = "123";
			AssertEquals("MasterAirWaybillNumber", "123", Line.MasterAirWaybillNumber);
		}

		public override void TestNumberOfPackages()
		{
			HAWB.CS_PiecesManifested = 10;
			AssertEquals("NumberOfPackages", 10, Line.NumberOfPackages);
		}

		CTOCusHAWBUnderbondMovementRequestLine line;
		protected override CusMAWBUnderbondMovementRequestLine Line => line ?? (line = new CTOCusHAWBUnderbondMovementRequestLine(HAWB));

		CTOCusMAWB mawb;
		protected override CusMAWBBase MAWB => mawb ?? (mawb = Factory.New<CTOCusMAWB>());

		CTOCusHAWB hawb;
		protected CTOCusHAWB HAWB => hawb ?? (hawb = ((CTOCusMAWB)MAWB).ChildBills.AddNew());
	}
}
