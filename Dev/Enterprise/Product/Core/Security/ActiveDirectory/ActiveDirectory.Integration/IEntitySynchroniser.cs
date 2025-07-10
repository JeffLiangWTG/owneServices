using System;
using System.Collections.Generic;
using Enterprise.Integration;

namespace Enterprise.Security.ActiveDirectory
{
	public interface ISynchronisationDirector
	{
		void Synchronise(EntitiesToSync? entitiesToSync = null, SyncMode? preferredSyncMode = null);
		IEnumerable<IADEntity> EntitiesWithErrors { get; }
		void Save();
		event EntitySynchronisedEventHandler EntitySynchronised;
		void SyncUsersToRoboticGroupIfRequired(IEnumerable<IADEntity> entities);
	}

	public interface IEntitySynchroniser : ISynchronisationDirector
	{
		event EventHandler<SyncProgressEventArgs> ProgressUpdated;
	}

	public delegate void EntitySynchronisedEventHandler(object sender, EntitySynchronisedEventArgs e);
}
