using CargoWise.Types;

namespace Enterprise.Customs.GB.Chief.EdiFact.UKCINV
{
	public class UkcinvUnderstanderEMR : UkcinvUnderstanderERS
	{
		public UkcinvUnderstanderEMR(UkCinvMessage messageToUnderstand)
			: base(messageToUnderstand)
		{
			emrResult = new EmrReport();
		}

		protected override ErsReport GetNewReport()
		{
			return emrResult;
		}

		protected override void ParseHeaderGEIs()
		{
			ZString soe = ZString.Empty;
			ZString roe = ZString.Empty;
			ZString crc = ZString.Empty;
			UkcinvUnderstander.GetGEIs(ref throwAway, ref soe, ref roe, ref throwAway, ref crc, ukCinv.GEI);
			emrResult.CustomsReturnCode = crc;
			emrResult.MasterRouteOfEntry = roe;
			emrResult.MasterStyleOfEntry = soe;
		}

		readonly EmrReport emrResult;
	}
}
