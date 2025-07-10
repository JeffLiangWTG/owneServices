using CargoWise.EntityFramework;
using Enterprise.Integration.Freight;

namespace Enterprise.Integration.Schedule
{
	public interface IRoutingLegsOrderHelper
	{
		void InitializeFrom(BusinessObject parentWithTransports);
		ITransport FirstLeg { get; }
		ITransport LastLeg { get; }
	}
}
