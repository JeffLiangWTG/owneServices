using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.ClusterKey
{
	static class ClusterKeyManager
	{
		/// <summary>
		/// Sets the cluster key field if not yet done in this Factory.Save().
		/// </summary>
		/// <param name="clusterKeyEntity">Cluster Key entity</param>
		/// <returns>Cluster Key value</returns>
		public static ZInt SetClusterKeyIfNeeded(IClusterKeyEntity clusterKeyEntity)
		{
			return SetClusterKeyIfNeededWithTypeDecider(clusterKeyEntity);
		}

		/// <summary>
		/// Loads sub-tree of Cluster Key descendants and flags them for saving if the entity's Cluster Key value is changing and hence needs to be cascaded.
		/// </summary>
		/// <param name="clusterKeyWorker">Cluster Key worker entity</param>
		public static void LoadAndFlagDescendantsForSavingIfKeyCascadingNeeded(IClusterKeyWorker clusterKeyWorker)
		{
			var workerPropagationStrategy = ClusterKeyWorkerPropagationStrategy.New(clusterKeyWorker);
			workerPropagationStrategy.LoadAndFlagDescendantsForSavingIfRequired();
		}

		static ZInt SetClusterKeyIfNeededWithTypeDecider(IClusterKeyEntity clusterKeyEntity)
		{
			if (clusterKeyEntity is IClusterKeyWorker clusterKeyWorker && !IsDetachedWorkerOrMasterEntity(clusterKeyWorker))
			{
				var workerPropagationStrategy = ClusterKeyWorkerPropagationStrategy.New(clusterKeyWorker);
				return workerPropagationStrategy.SetClusterKeyIfRequired();
			}
			else if (clusterKeyEntity is IClusterKeyMaster masterClusterKey)
			{
				var masterPropagationStrategy = ClusterKeyMasterPropagationStrategy.New(masterClusterKey);
				return masterPropagationStrategy.SetClusterKeyIfRequired();
			}
			else
			{
				throw new ArgumentException("Invalid ClusterKeyEntity type", nameof(clusterKeyEntity));
			}
		}

		static bool IsDetachedWorkerOrMasterEntity(IClusterKeyWorker clusterKeyEntity)
		{
			return clusterKeyEntity is IClusterKeyMaster && ClusterKeyMasterPropagationStrategy.IsParentEmpty(clusterKeyEntity);
		}
	}
}
