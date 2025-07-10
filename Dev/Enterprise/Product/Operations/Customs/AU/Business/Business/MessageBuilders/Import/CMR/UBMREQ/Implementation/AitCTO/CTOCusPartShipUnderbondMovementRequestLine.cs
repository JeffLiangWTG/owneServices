using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CTOCusPartShipUnderbondMovementRequestLine : CTOCusHAWBUnderbondMovementRequestLine
	{
		public CTOCusPartShipUnderbondMovementRequestLine(CusPartShip partShip, CTOCusHAWB hAWB)
			: base(hAWB)
		{
			this.partShip = partShip;
		}

		public override ZInt NumberOfPackages
		{
			get { return partShip.CG_PiecesManifested; }
		}

		readonly CusPartShip partShip;
	}
}
