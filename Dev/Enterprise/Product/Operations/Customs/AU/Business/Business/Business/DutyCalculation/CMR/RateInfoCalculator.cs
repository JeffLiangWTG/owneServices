using System.Collections;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class RateInfoCalculator
	{
		public RateInfo[] GetRateApplicable(IFourRates referenceFileRate)
		{
			ArrayList result = new ArrayList();

			if (!referenceFileRate.CustomsRate.IsEmpty)
			{
				RateInfo rateInfo = new RateInfo();
				rateInfo.DutyRateField = DutyRateField.CustomsValue;
				rateInfo.Rate = referenceFileRate.CustomsRate;
				result.Add(rateInfo);
			}
			if (!referenceFileRate.FirstQtyRate.IsEmpty)
			{
				RateInfo rateInfo = new RateInfo();
				rateInfo.DutyRateField = DutyRateField.FirstQty;
				rateInfo.Rate = referenceFileRate.FirstQtyRate;
				rateInfo.Unit = referenceFileRate.FirstUQ;
				result.Add(rateInfo);
			}
			if (!referenceFileRate.SecondQtyRate.IsEmpty)
			{
				RateInfo rateInfo = new RateInfo();
				rateInfo.DutyRateField = DutyRateField.SecondQty;
				rateInfo.Rate = referenceFileRate.SecondQtyRate;
				rateInfo.Unit = referenceFileRate.SecondUQ;
				result.Add(rateInfo);
			}
			if (!referenceFileRate.OtherDutyFactorRate.IsEmpty)
			{
				RateInfo rateInfo = new RateInfo();
				rateInfo.DutyRateField = DutyRateField.OtherDutyFactor;
				rateInfo.Rate = referenceFileRate.OtherDutyFactorRate;
				result.Add(rateInfo);
			}

			return (RateInfo[])result.ToArray(typeof(RateInfo));
		}
	}
}
