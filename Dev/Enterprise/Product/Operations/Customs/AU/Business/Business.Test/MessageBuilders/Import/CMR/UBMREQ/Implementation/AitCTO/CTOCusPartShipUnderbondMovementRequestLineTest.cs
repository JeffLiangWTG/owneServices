namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CTOCusPartShipUnderbondMovementRequestLineTest : CTOCusHAWBUnderbondMovementRequestLineTest
	{
		public override void TestNumberOfPackages()
		{
			PartShip.CG_PiecesManifested = 5;
			AssertEquals("NumberOfPackages", 5, Line.NumberOfPackages);
		}

		CTOCusPartShipUnderbondMovementRequestLine line;
		protected override CusMAWBUnderbondMovementRequestLine Line => line ?? (line = new CTOCusPartShipUnderbondMovementRequestLine(PartShip, HAWB));

		CusPartShip partShip;
		CusPartShip PartShip => partShip ?? (partShip = HAWB.PartShips.AddNew());
	}
}
