using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Security.ActiveDirectory.GUI;

namespace Enterprise.Security.ActiveDirectory.Test
{
	public class SynchronisationDirectorProviderForTest : SynchronisationDirectorProvider
	{
		protected override ISynchronisationDirector GetSyncDirectorCore(BusinessObjectFactory factory, IEnumerable<BusinessObject> items)
		{
			SyncDirector = SyncDirectorOverride ?? base.GetSyncDirectorCore(factory, items);

			SyncHistories = new List<EntitySynchronisedEventArgs>();
			SyncDirector.EntitySynchronised += (s, e) => SyncHistories.Add(e);

			if (extraEntitySynchronisedCallback != null)
			{
				SyncDirector.EntitySynchronised += extraEntitySynchronisedCallback;
			}
			return SyncDirector;
		}

		public List<EntitySynchronisedEventArgs> SyncHistories { get; private set; }

		public ISynchronisationDirector SyncDirector { get; private set; }

		public void SetExtraEntitySynchronisedCallback(EntitySynchronisedEventHandler e)
		{
			extraEntitySynchronisedCallback = e;
		}

		EntitySynchronisedEventHandler extraEntitySynchronisedCallback;

		public ISynchronisationDirector SyncDirectorOverride;
	}
}
