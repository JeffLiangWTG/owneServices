using CargoWise.Types;

namespace Enterprise.Accounting.Integration
{
	public interface IJobWithRevenueTotal
	{
		ZDecimal JH_TotalRevenue { get; }
	}
}
