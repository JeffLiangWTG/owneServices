using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business.ClusterKey
{
	/// <summary>
	/// Top level Cluster Key entity.
	/// Feeds Cluster Key value from a Universal ClusterKey Number Fountain
	/// and dictates it to entity Cluster Key tree.
	/// </summary>
	public interface IClusterKeyMaster : IClusterKeyMasterEntity
	{
	}

	public static class ClusterKeyEntityExtension
	{
		public static void CheckCanSetMasterClusterKey(this IClusterKeyMaster clusterKeyMaster)
		{
			var bizObj = Argument.NotNull(clusterKeyMaster as EnterpriseBusinessObject, "clusterKeyMaster as EnterpriseBusinessObject");

			if (!bizObj.IsInDatabase)
			{
				return;
			}

			if (clusterKeyMaster is IClusterKeyWorker clusterKeyWorkerOrMaster)
			{
				if (clusterKeyWorkerOrMaster.ClusterKeyPty.Value > 0
					&& ClusterKeyWorkerPropagationStrategy.IsParentEmpty(clusterKeyWorkerOrMaster)
					&& !ClusterKeyWorkerPropagationStrategy.IsParentDirty(clusterKeyWorkerOrMaster))
				{
					var msg = $" PK: {bizObj.PK}";
					msg += $" ParentPK: {clusterKeyWorkerOrMaster.FkToParentPty?.Value}";
					msg += $" ClusterKey: {clusterKeyWorkerOrMaster.ClusterKeyPty?.Value}";

					throw new InvalidOperationException(InvalidAttemptToModifyMidLevelMasterWhenParentFkHasNotChanged + msg);
				}
			}
			else
			{
				throw new InvalidOperationException(InvalidAttemptToModifyExistingTopLevelMaster);
			}
		}

		internal static string InvalidAttemptToModifyExistingTopLevelMaster => (NoResString)"Invalid attempt to modify top-level master cluster key on an existing business object.";
		internal static string InvalidAttemptToModifyMidLevelMasterWhenParentFkHasNotChanged => (NoResString)"Invalid attempt to modify mid-level master cluster key when the parent FK has not been changed.";
	}
}
