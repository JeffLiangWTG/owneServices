using Enterprise.Environment;

namespace Enterprise.ZArchitecture.Web.GlobalBase
{
	public class WebServicesEnvironment : WebEnvironment
	{
		public WebServicesEnvironment(IUserContextManager userContextManager) : base(userContextManager) { }

		public override bool IsWeb => false;
		public override bool IsWebService => true;
	}
}
