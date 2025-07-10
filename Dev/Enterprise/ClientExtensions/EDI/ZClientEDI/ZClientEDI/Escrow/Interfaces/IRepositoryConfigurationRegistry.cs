using System.Collections.Generic;

namespace Enterprise.Client.EDI.Escrow
{
	interface IRepositoryConfigurationRegistry
	{
		IReadOnlyCollection<IRepository> MainRepositories { get; }
	}
}
