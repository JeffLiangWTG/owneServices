namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class CTOCusHAWBUnderbondMovementRequestHeaderTest : CusMAWBUnderbondMovementRequestHeaderTest
	{
		public override void TestLine()
		{
			AssertEquals("LineType", typeof(CTOCusHAWBUnderbondMovementRequestLine), RequestHeader.Line.GetType());
		}

		protected override IUnderbondMovementRequestHeader RequestHeader => new CTOCusHAWBUnderbondMovementRequestHeader(Underbond, HAWB);

		CusMAWBBase mawb;
		protected override CusMAWBBase MAWB => mawb ?? (mawb = Factory.New<CTOCusMAWB>());

		CTOCusHAWB hawb;
		protected CTOCusHAWB HAWB => hawb ?? (hawb = ((CTOCusMAWB)MAWB).ChildBills.AddNew());
	}
}
