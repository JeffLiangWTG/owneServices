#if NETFRAMEWORK
using Enterprise.Environment;
using Enterprise.ZArchitecture.Web.GlobalBase;
using Enterprise.ZArchitecture.Web.Utilities.Environment;

namespace Enterprise.Accounting.Web
{
	public class Global : ZEnterpriseGlobal
	{
		protected override EnvProvider WebEnvProvider => new WebEnvironmentProvider();
	}
}
#endif
