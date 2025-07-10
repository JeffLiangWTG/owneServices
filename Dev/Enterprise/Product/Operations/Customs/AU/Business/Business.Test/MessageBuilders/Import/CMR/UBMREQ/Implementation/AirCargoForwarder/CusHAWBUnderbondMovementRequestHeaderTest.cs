namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class CusHAWBUnderbondMovementRequestHeaderTest : CusMAWBUnderbondMovementRequestHeaderTest
	{
		public override void TestTranshipmentOverseasDestinationPort()
		{
			Underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.Transshipment;
			Underbond.C4_RL_NKTranshipDestPort = "CNSHA";
			AssertEquals("TranshipmentOverseasDestinationPort", "CNSHA", RequestHeader.TranshipmentOverseasDestinationPort);
		}

		public override void TestLine()
		{
			AssertEquals("LineType", typeof(CusHAWBUnderbondMovementRequestLine), RequestHeader.Line.GetType());
		}

		protected override IUnderbondMovementRequestHeader RequestHeader => new CusHAWBUnderbondMovementRequestHeader(Underbond, HAWB);

		CusHAWB hawb;
		protected CusHAWB HAWB => hawb ?? (hawb = ((CusMAWB)MAWB).ChildBills.AddNew());
	}
}
