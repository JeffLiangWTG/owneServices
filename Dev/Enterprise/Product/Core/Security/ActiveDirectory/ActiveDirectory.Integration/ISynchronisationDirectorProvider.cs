using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Security.ActiveDirectory
{
	public interface ISynchronisationDirectorProvider
	{
		ISynchronisationDirector GetSyncDirector(BusinessObjectFactory factory, IEnumerable<BusinessObject> items);
	}
}
