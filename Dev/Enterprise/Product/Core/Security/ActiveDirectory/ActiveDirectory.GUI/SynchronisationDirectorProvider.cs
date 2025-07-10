using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Security.ActiveDirectory.GUI
{
	public class SynchronisationDirectorProvider : ISynchronisationDirectorProvider
	{
		public ISynchronisationDirector GetSyncDirector(BusinessObjectFactory factory, IEnumerable<BusinessObject> items)
		{
			return GetSyncDirectorCore(factory, items);
		}

		protected virtual ISynchronisationDirector GetSyncDirectorCore(BusinessObjectFactory factory, IEnumerable<BusinessObject> items)
		{
			return new SynchronisationDirector(factory, items);
		}
	}
}
