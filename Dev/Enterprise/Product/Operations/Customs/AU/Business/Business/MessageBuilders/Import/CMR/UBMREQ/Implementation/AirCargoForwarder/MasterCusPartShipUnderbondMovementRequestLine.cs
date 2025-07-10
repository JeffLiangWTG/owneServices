using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class MasterCusPartShipUnderbondMovementRequestLine : CusMAWBUnderbondMovementRequestLine
	{
		public MasterCusPartShipUnderbondMovementRequestLine(CusPartShip partShip, CusMAWB mAWB)
			: base(mAWB)
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
