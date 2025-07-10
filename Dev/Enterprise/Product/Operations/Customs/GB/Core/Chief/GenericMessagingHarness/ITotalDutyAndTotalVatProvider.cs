using CargoWise.Types;

namespace Enterprise.Customs.GB.Chief
{
	public interface ITotalDutyAndTotalVatProvider
	{
		ZDecimal TotalDuty { get; }
		ZDecimal TotalVAT { get; }
	}
}
