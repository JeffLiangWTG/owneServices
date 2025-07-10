using CargoWise.ActiveDirectory;
using CargoWise.Integration;

namespace Enterprise.Security.ActiveDirectory
{
	public interface IDirectorySearcherProvider
	{
		IDirectorySearcher GetDirectorySearcher(IDomainCredentials domainCredentials, bool requireDomainWritePrivilege);
	}
}
