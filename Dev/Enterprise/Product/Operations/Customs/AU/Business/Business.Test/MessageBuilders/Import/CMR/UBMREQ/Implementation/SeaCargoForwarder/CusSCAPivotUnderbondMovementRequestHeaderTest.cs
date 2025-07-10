namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAPivotUnderbondMovementRequestHeaderTest : CusSCAOceanBillUnderbondMovementRequestHeaderAbstractTest
	{
		public void TestLine()
		{
			AssertEquals("RequestHeader.Type", typeof(CusSCAPivotUnderbondMovementRequestLine), RequestHeader.Line.GetType());
		}

		public void TestTranshipmentOverseasDestinationPort()
		{
			Underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.Transshipment;
			Underbond.C4_RL_NKTranshipDestPort = "CNSHA";
			AssertEquals("TranshipmentOverseasDestinationPort", "CNSHA", RequestHeader.TranshipmentOverseasDestinationPort);
		}

		protected override IUnderbondMovementRequestHeader RequestHeader
			=> new CusSCAPivotUnderbondMovementRequestHeader(Underbond, SCAOcean.HouseBills.AddNew().Pivot.AddNew());
	}
}
