using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class MasterCusPartShipUnderbondMovementRequestHeader : CusMAWBUnderbondMovementRequestHeader
	{
		public MasterCusPartShipUnderbondMovementRequestHeader(CusUnderbond underbond, CusMAWB mAWB, CusPartShip partShip)
			: base(underbond, mAWB)
		{
			this.mAWB = mAWB;
			this.partShip = partShip;
		}

		protected override ZString FlightNumberDefault
		{
			get { return partShip.CG_FlightNo; }
		}

		protected override ZDateTime EstimatedDateOfArrivalDefault
		{
			get { return partShip.CG_ArrivalDate; }
		}

		public override IUnderbondMovementRequestLine Line
		{
			get { return new MasterCusPartShipUnderbondMovementRequestLine(partShip, mAWB); }
		}

		#region Implementation

		readonly CusMAWB mAWB;
		readonly CusPartShip partShip;

		#endregion
	}
}
