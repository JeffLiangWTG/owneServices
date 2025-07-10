using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class HouseCusPartShipUnderbondMovementRequestHeader : CusHAWBUnderbondMovementRequestHeader
	{
		public HouseCusPartShipUnderbondMovementRequestHeader(CusUnderbond underbond, CusHAWB hAWB, CusPartShip partShip)
			: base(underbond, hAWB)
		{
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
			get { return new HouseCusPartShipUnderbondMovementRequestLine(partShip, HAWB); }
		}

		#region Implementation

		readonly CusPartShip partShip;

		#endregion
	}
}
