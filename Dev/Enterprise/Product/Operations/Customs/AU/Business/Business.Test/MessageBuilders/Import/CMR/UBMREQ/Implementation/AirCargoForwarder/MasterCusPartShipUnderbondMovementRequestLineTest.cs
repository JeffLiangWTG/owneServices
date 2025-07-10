namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class MasterCusPartShipUnderbondMovementRequestLineTest : CusMAWBUnderbondMovementRequestLineTest
	{
		public override void TestNumberOfPackages()
		{
			PartShip.CG_PiecesManifested = 5;
			AssertEquals("NumberOfPackages", 5, Line.NumberOfPackages);
		}

		MasterCusPartShipUnderbondMovementRequestLine line;
		protected override CusMAWBUnderbondMovementRequestLine Line => line ?? (line = new MasterCusPartShipUnderbondMovementRequestLine(PartShip, (CusMAWB)MAWB));

		CusPartShip partShip;
		CusPartShip PartShip => partShip ?? (partShip = MAWB.PartShips.AddNew());
	}
}
