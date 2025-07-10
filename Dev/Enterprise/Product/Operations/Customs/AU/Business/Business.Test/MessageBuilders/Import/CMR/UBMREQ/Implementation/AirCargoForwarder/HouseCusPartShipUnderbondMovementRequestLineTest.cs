namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class HouseCusPartShipUnderbondMovementRequestLineTest : CusHAWBUnderbondMovementRequestLineTest
	{
		public override void TestNumberOfPackages()
		{
			PartShip.CG_PiecesManifested = 5;
			AssertEquals("NumberOfPackages", 5, Line.NumberOfPackages);
		}

		HouseCusPartShipUnderbondMovementRequestLine line;
		protected override CusMAWBUnderbondMovementRequestLine Line => line ??= new HouseCusPartShipUnderbondMovementRequestLine(PartShip, HAWB);

		CusPartShip partShip;
		CusPartShip PartShip => partShip ??= HAWB.PartShips.AddNew();
	}
}
