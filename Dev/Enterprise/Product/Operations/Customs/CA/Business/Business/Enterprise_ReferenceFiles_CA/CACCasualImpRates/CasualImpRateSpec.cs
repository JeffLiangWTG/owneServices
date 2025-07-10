using CargoWise.Types;

namespace Enterprise.Customs.CA.Business;

public class CasualImpRateSpec
{
	public ZString RateType { get; set; }
	public ZString Units { get; set; }
	public ZDecimal RegularRate { get; set; }
	public ZDecimal MinimumRate { get; set; }
	public ZDecimal MaximumRate { get; set; }

	public CasualImpRateSpec(ZString rateType, ZString units, ZDecimal regularRate, ZDecimal minimumRate, ZDecimal maximumRate)
	{
		this.RateType = rateType;
		this.Units = units;
		this.RegularRate = regularRate;
		this.MinimumRate = minimumRate;
		this.MaximumRate = maximumRate;
	}
}
