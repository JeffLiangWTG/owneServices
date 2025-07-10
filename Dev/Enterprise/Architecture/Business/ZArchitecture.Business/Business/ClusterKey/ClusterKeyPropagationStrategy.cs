using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.ClusterKey
{
	abstract class ClusterKeyPropagationStrategy<T> where T : IClusterKeyEntity
	{
		protected ClusterKeyPropagationStrategy(T clusterKeyEntity)
		{
			this.clusterKeyEntity = Argument.NotNull(clusterKeyEntity, nameof(clusterKeyEntity));
			clusterKeyEntityAsBizObj = Argument.NotNull(clusterKeyEntity as EnterpriseBusinessObject, "clusterKeyEntity as EnterpriseBusinessObject");
		}

		protected readonly T clusterKeyEntity;
		protected readonly EnterpriseBusinessObject clusterKeyEntityAsBizObj;
		protected const int ClusterKeyFlaggedForChangeValue = -1;

		/// <summary>
		/// Sets the cluster key field if not yet done in this Factory.Save()
		/// The cluster key value comes from:
		///   - Parent if it's a WORKER entity
		///   - Designated Number Fountain if it's a MASTER entity
		/// </summary>
		/// <returns>Cluster Key value</returns>
		public ZInt SetClusterKeyIfRequired()
		{
			if (!HasKeyBeenSetInThisSaveOperationOrIsDeleted())
			{
				SetClusterKeyIfRequiredCore();
				clusterKeyEntityAsBizObj.LastClusterKeySetTransactionId = clusterKeyEntityAsBizObj.Factory.TransactionId;
			}

			return clusterKeyEntity.ClusterKeyPty.Value;
		}

		protected abstract void SetClusterKeyIfRequiredCore();

		protected bool HasKeyBeenSetInThisSaveOperationOrIsDeleted()
		{
			return clusterKeyEntityAsBizObj.LastClusterKeySetTransactionId == clusterKeyEntityAsBizObj.Factory.TransactionId || clusterKeyEntityAsBizObj.IsDeleted;
		}

		public static bool IsParentEmpty(IClusterKeyWorker clusterKeyWorker)
		{
			return clusterKeyWorker.FkToParentPty == null || clusterKeyWorker.FkToParentPty.Value.IsEmpty;
		}

		public static bool IsParentDirty(IClusterKeyWorker clusterKeyWorker)
		{
			if (clusterKeyWorker.ClusterKeyPty.Value == ClusterKeyFlaggedForChangeValue)
			{
				return true;
			}

			if (clusterKeyWorker.FkToParentPty == null)
			{
				return false;
			}

			return clusterKeyWorker.FkToParentPty.HasChanges;
		}
	}
}
