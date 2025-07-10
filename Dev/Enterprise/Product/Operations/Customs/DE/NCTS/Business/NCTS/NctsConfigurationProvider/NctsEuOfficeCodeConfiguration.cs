using CargoWise.Types;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsEuOfficeCodeConfiguration : EU.NCTS.Business.NctsEuOfficeCodeConfiguration
	{
		public override ZBool AutomaticSequenceNumberEnabled => false;

		protected override EU.NCTS.Business.INctsEuOfficeCodePhase5ValidationDecider GetNctsEuOfficeCodeDeparturePhase5ValidationDecider() => new NctsEuOfficeCodeDeparturePhase5ValidationDecider();

		protected override EU.NCTS.Business.INctsEuOfficeCodePhase5ValidationDecider GetNctsEuOfficeCodeArrivalPhase5ValidationDecider() => new NctsEuOfficeCodeArrivalPhase5ValidationDecider();
	}
}
