using CargoWise.Types;

namespace Enterprise.Integration.DocumentEngine
{
	public interface IDocDataMetricRanking
	{
		ZString GetMetricRanking(ZString metricCode, ZDecimal metricValue, bool isPercentage = false);
		ZString GetMetricTargetRanking(ZString metricCode);
		ZString GetMetricTargetRange(ZString metricCode);
		ZDecimal GetMetricTargetRangeMin(ZString metricCode);
		ZDecimal GetMetricTargetRangeMax(ZString metricCode);
	}
}
