using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Integration.Modules;

public interface IModuleEnvironment
{
	ISecurityCheckpoint SecurityNone { get; }
	ILicenceCheckpoint LicenceCore { get; }
	string CurrentCompanyCountryCode { get; }
}
