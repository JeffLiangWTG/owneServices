using CargoWise.Types;

namespace Enterprise.Integration.Rating
{
	public interface IRatingHeader
	{
		ZGuid PK { get; }
		ZGuid TH_OH { get; set; }
		ZGuid TH_GC { get; set; }
		ZString TH_RateType { get; set; }
		ZString TH_QuoteNumber { get; set; }
		ZByte TH_GlobalRateLevel { get; set; }
	}
}
