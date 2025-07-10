using CargoWise.ActiveDirectory;
using CargoWise.Integration;

namespace Enterprise.Security.ActiveDirectory
{
	public class DirectorySearcherProvider : IDirectorySearcherProvider
	{
		public IDirectorySearcher GetDirectorySearcher(IDomainCredentials domainCredentials, bool requireDomainWritePrivilege)
		{
			return DirectorySearcherFactory.GetDirectorySearcher(domainCredentials, requireDomainWritePrivilege);
		}
	}
}
