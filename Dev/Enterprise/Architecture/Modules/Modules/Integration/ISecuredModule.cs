using Enterprise.Integration.Licensing;

namespace Enterprise.ZArchitecture.Modules
{
	public interface ISecuredModule : ISecured
	{
		ILicenceCheckpoint LicenceCheckpoint { get; }
	}
}
