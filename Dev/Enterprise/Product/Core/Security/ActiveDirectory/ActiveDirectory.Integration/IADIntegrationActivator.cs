using System.Collections.Generic;
using Enterprise.Integration;

namespace Enterprise.Security.ActiveDirectory
{
	public interface IADIntegrationActivator
	{
		IEnumerable<EntitySynchronisedEventArgs> EnableIntegration(EntitiesToSync entitiesToSync);
		bool DisableIntegration(bool disableGroupOnly = false);
		void SaveChanges();
	}
}
