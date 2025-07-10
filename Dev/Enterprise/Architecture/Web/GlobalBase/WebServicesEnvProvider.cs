using Enterprise.Environment;

namespace Enterprise.ZArchitecture.Web.GlobalBase
{
	public class WebServicesEnvProvider : WebEnvProvider
	{
		protected override WebEnvironment NewWebEnvironment(IUserContextManager contextManager) => new WebServicesEnvironment(contextManager);
	}
}
