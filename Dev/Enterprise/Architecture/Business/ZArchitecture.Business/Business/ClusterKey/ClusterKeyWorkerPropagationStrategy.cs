using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.ClusterKey
{
	class ClusterKeyWorkerPropagationStrategy : ClusterKeyPropagationStrategy<IClusterKeyWorker>
	{
		public static ClusterKeyWorkerPropagationStrategy New(IClusterKeyWorker clusterKeyWorker) => new ClusterKeyWorkerPropagationStrategy(clusterKeyWorker);
		ClusterKeyWorkerPropagationStrategy(IClusterKeyWorker clusterKeyWorker) : base(clusterKeyWorker) { }

		IClusterKeyEntity ClusterKeyParent
		{
			get
			{
				if (clusterKeyParent == null
					&& !IsParentEmpty(clusterKeyEntity)
					&& clusterKeyEntity.FkToParentPty.Value.IsValid)
				{
					clusterKeyParent = (IClusterKeyEntity)clusterKeyEntityAsBizObj.Factory.Load(clusterKeyEntity.ParentBizObjType, clusterKeyEntity.FkToParentPty.Value);
				}

				return clusterKeyParent;
			}
		}
		IClusterKeyEntity clusterKeyParent;

		/// <summary>
		/// Loads descendents recursively and flags them for saving if:
		///   - Cluster Key entity IS IN DATABASE (hence may have existing children with no changes or not even participating in the factory)
		///   - AND its FK to the Cluster Key Parent HAS CHANGES (hence its Cluster Key value will also change)
		/// This ensures the entire Cluster Key subtree:
		///   - is loaded into the factory AND has pending changes
		///   - so it participates on the Cluster Key setting mechanism
		/// </summary>
		public void LoadAndFlagDescendantsForSavingIfRequired()
		{
			if (IsInDatabaseAndNotDeleted(clusterKeyEntityAsBizObj) && IsParentDirty(clusterKeyEntity))
			{
				ResetKeyAndCascadeToDescendants();
			}
		}

		/// <summary>
		/// Uses recursion to get Cluster Key value from parent, setting it in its turn if needed.
		/// </summary>
		protected override void SetClusterKeyIfRequiredCore()
		{
			var newValue = (ClusterKeyParent == null) ? ZInt.Zero : ClusterKeyManager.SetClusterKeyIfNeeded(ClusterKeyParent);
			clusterKeyEntity.ClusterKeyPty.Value = newValue;
		}

		/// <summary>
		/// Uses recursion to load sub-tree of descendants.
		/// </summary>
		void ResetKeyAndCascadeToDescendants()
		{
			clusterKeyEntity.ClusterKeyPty.Value = ClusterKeyFlaggedForChangeValue;

			var clusterKeyChildList = clusterKeyEntity.ClusterKeyChildList ?? Enumerable.Empty<ClusterKeyChildInfo>();

			foreach (var clusterKeyChildInfo in clusterKeyChildList)
			{
				var allChildren = clusterKeyChildInfo.LoadChildObjects(clusterKeyEntityAsBizObj);
				var elegibleChildren = allChildren.Where(cke => IsInDatabaseAndNotDeleted((EnterpriseBusinessObject)cke));

				foreach (var childEntity in elegibleChildren)
				{
					New(childEntity).ResetKeyAndCascadeToDescendants();
				}
			}
		}

		protected static bool IsInDatabaseAndNotDeleted(EnterpriseBusinessObject bizObj)
		{
			return bizObj.IsInDatabase && !bizObj.IsDeleted;
		}
	}
}
