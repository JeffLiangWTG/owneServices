using CargoWise.Types;
using Enterprise.eHubMessaging.Business.Interfaces;

namespace Enterprise.eHubMessaging.Business
{
	public interface IEHubServiceTaskHealthCheckJob
	{
		bool Execute();
		IHealthCheckResult Check();
		void Notify(IHealthCheckResult result);

		ZDateTime ServiceTaskNextRunTimeUtc { get; }
		int ServiceTaskPeriodInMinute { get; }
	}
}
