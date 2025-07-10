using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusMAWBUnderbondMovementRequestHeader : CusUnderbondUnderbondMovementRequestHeader
	{
		public CusMAWBUnderbondMovementRequestHeader(CusUnderbond underbond, CusMAWBBase mAWB)
			: base(underbond)
		{
			this.mAWB = mAWB;
		}

		protected override ZString FlightNumberDefault
		{
			get { return mAWB.CM_FlightNo; }
		}

		protected override ZDateTime EstimatedDateOfArrivalDefault
		{
			get { return mAWB.CM_ArrivalDate; }
		}

		public override bool IsBureau
		{
			get { return mAWB.CM_IsBureau; }
		}

		public override sealed ZString VoyageNumber
		{
			get { return ZString.Empty; }
		}

		public override sealed ZString VesselID
		{
			get { return ZString.Empty; }
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
			get { return new CusMAWBUnderbondMovementRequestLine(mAWB); }
		}

		readonly CusMAWBBase mAWB;
	}
}
