using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusHAWBUnderbondMovementRequestHeader : CusMAWBUnderbondMovementRequestHeader
	{
		public CusHAWBUnderbondMovementRequestHeader(CusUnderbond underbond, CusHAWB hAWB)
			: base(underbond, hAWB.MAWB)
		{
			this.HAWB = hAWB;
		}

		public override ZString TranshipmentOverseasDestinationPort
		{
			get
			{
				return underbond.C4_MovementReason == CMRUnderbondRequestCodes.Codes.Transshipment ?
					underbond.C4_RL_NKTranshipDestPort : ZString.Empty;
			}
		}

		public override IUnderbondMovementRequestLine Line
		{
			get { return new CusHAWBUnderbondMovementRequestLine(HAWB); }
		}

		#region Implementation

		protected readonly CusHAWB HAWB;

		#endregion
	}
}
