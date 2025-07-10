using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Messaging
{
	public interface IUnloadingRemark : IUnloadingRemarkInterface
	{
		ZString UnloadingRemark { get; }
		ZString UnloadingRemarkLanguage { get; }
	}
}
