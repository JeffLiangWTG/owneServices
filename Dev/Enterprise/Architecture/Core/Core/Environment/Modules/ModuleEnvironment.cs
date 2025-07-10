using Enterprise.Integration.Licensing;
using Enterprise.Integration.Modules;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.Core;

class ModuleEnvironment : IModuleEnvironment
{
	public ISecurityCheckpoint SecurityNone => EnvProxy.Instance.Security.None;
	public ILicenceCheckpoint LicenceCore => EnvProxy.Instance.Licence.Core;
	public string CurrentCompanyCountryCode => EnvProxy.Instance.CurrentCompany.Country?.Code;
}
