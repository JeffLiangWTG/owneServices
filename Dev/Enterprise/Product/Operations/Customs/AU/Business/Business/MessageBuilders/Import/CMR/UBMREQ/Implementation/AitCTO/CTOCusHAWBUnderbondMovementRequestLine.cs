using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CTOCusHAWBUnderbondMovementRequestLine : CusMAWBUnderbondMovementRequestLine
	{
		public CTOCusHAWBUnderbondMovementRequestLine(CTOCusHAWB hAWB)
			: base(hAWB.MAWB)
		{
			this.hAWB = hAWB;
		}

		public override ZString MasterAirWaybillNumber
		{
			get { return hAWB.CS_HAWB; }
		}

		public override ZInt NumberOfPackages
		{
			get { return hAWB.CS_PiecesManifested; }
		}

		readonly CTOCusHAWB hAWB;
	}
}
