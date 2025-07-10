using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CTOCusPartShipUnderbondMovementRequestHeader : CTOCusHAWBUnderbondMovementRequestHeader
	{
		public CTOCusPartShipUnderbondMovementRequestHeader(CusUnderbond underbond, CTOCusHAWB hAWB, CusPartShip partShip)
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
			get { return new CTOCusPartShipUnderbondMovementRequestLine(partShip, HAWB); }
		}

		readonly CusPartShip partShip;
	}
}
