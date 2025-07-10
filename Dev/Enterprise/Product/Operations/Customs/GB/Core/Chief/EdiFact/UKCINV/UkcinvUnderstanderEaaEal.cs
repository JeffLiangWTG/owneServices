using CargoWise.Types;

namespace Enterprise.Customs.GB.Chief.EdiFact.UKCINV
{
	public class UkcinvUnderstanderEaaEal : UkcinvUnderstanderEMR
	{
		public UkcinvUnderstanderEaaEal(UkCinvMessage messageToUnderstand)
			: base(messageToUnderstand)
		{
			eaaEalResult = new EaaEalReport();
		}
		protected override void ParseHeaderGEIs()
		{
			ZString crc = ZString.Empty;
			UkcinvUnderstander.GetGEIs(ref throwAway, ref throwAway, ref throwAway, ref throwAway, ref crc, ukCinv.GEI);
			eaaEalResult.CustomsReturnCode = crc;
		}

		protected override ErsReport GetNewReport()
		{
			return eaaEalResult;
		}

		readonly EaaEalReport eaaEalResult;
	}
}
