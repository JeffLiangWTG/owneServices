using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class HouseCusPartShipUnderbondMovementRequestLine : CusHAWBUnderbondMovementRequestLine
	{
		public HouseCusPartShipUnderbondMovementRequestLine(CusPartShip partShip, CusHAWB hAWB)
			: base(hAWB)
		{
			this.PartShip = partShip;
		}

		public override ZInt NumberOfPackages
		{
			get { return PartShip.CG_PiecesManifested; }
		}

		protected readonly CusPartShip PartShip;
	}
}
