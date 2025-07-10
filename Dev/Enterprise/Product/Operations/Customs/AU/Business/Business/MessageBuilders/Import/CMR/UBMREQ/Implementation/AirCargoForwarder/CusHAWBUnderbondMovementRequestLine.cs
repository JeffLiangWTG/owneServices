using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusHAWBUnderbondMovementRequestLine : CusMAWBUnderbondMovementRequestLine
	{
		public CusHAWBUnderbondMovementRequestLine(CusHAWB hAWB)
			: base(hAWB.MAWB)
		{
			this.hAWB = hAWB;
		}

		public override ZString HouseAirWaybillNumber
		{
			get { return hAWB.CS_HAWB; }
		}

		public override ZInt NumberOfPackages
		{
			get { return hAWB.CS_PiecesManifested; }
		}

		readonly CusHAWB hAWB;
	}
}
