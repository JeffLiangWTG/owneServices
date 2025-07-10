using System;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public abstract class SpawningIndependentNetworkAction : JobNetworkAction
	{
		protected SpawningIndependentNetworkAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, executionStrategy: null, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		protected BusinessObjectFactory CreateNewFactoryForSpawnedNetwork()
		{
			var factory = new BusinessObjectFactory();
#if DEBUG
			factoryForSpawnedNetworkReference = new WeakReference<BusinessObjectFactory>(factory);
#endif
			return factory;
		}

		protected JobNetwork CreateSpawnedTemporaryNetwork(BMNCNShape spawnedDiagram)
		{
			return JobNetwork.CreateTemporaryNetwork(spawnedDiagram, Controller);
		}

#if DEBUG
		WeakReference<BusinessObjectFactory> factoryForSpawnedNetworkReference; // we don't want to keep a strong reference for the factory to prevent memory leaks

		BusinessObjectFactory GetFactoryForSpawnedNetwork()
		{
			BusinessObjectFactory factory = null;
			factoryForSpawnedNetworkReference?.TryGetTarget(out factory);
			return factory;
		}

		public BusinessObjectFactory FactoryForSpawnedNetwork_ExposedForTest => GetFactoryForSpawnedNetwork();
#endif
	}
}
