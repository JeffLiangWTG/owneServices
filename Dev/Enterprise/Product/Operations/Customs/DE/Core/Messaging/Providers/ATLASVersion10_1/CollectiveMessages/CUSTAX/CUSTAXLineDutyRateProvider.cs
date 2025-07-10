using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class CUSTAXLineDutyRateProvider : ICUSTAXLineDutyRate
	{
		public CUSTAXLineDutyRateProvider(GCTAXMBodyGoodsItemCustomsDutiesCustomsDutyCustomsDutyRate dutyRate)
		{
			this.dutyRate = CargoWise.Common.Argument.NotNull(dutyRate, nameof(dutyRate));
		}

		readonly GCTAXMBodyGoodsItemCustomsDutiesCustomsDutyCustomsDutyRate dutyRate;

		public string CriteriaType => dutyRate.CriteriaType;

		public string AssessmentScale => dutyRate.AssessmentScale;

		public decimal Rate => dutyRate.RateSpecified ? dutyRate.Rate : 0m;
	}
}
