using Xware.Xt.Grpc.Config;

namespace Enterprise.Messaging.Integration
{
	public interface IXtConfigurationProvider
	{
		Configuration GetConfiguration();
	}
}
