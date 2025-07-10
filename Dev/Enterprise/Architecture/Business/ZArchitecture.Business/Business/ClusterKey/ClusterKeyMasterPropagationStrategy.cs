using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Business.ClusterKey
{
	class ClusterKeyMasterPropagationStrategy : ClusterKeyPropagationStrategy<IClusterKeyMaster>
	{
		public static ClusterKeyMasterPropagationStrategy New(IClusterKeyMaster clusterKeyMaster) => new ClusterKeyMasterPropagationStrategy(clusterKeyMaster);
		ClusterKeyMasterPropagationStrategy(IClusterKeyMaster clusterKeyMaster) : base(clusterKeyMaster)
		{
			universalClusterKeyNumberFountain = ClusterKeyNumberFountain.FountainProxy;
		}
		readonly INumberFountainProxy universalClusterKeyNumberFountain;

		protected override void SetClusterKeyIfRequiredCore()
		{
			SetMasterClusterKeyFromNumberFountainIfRequired();
		}

		void SetMasterClusterKeyFromNumberFountainIfRequired()
		{
			lock (clusterKeyEntity.ClusterKeyPty)
			{
				if (!HasKeyBeenSetInThisSaveOperationOrIsDeleted() && ShouldSetMasterClusterKey())
				{
					var newValue = (ZInt)universalClusterKeyNumberFountain.GetNext(clusterKeyEntityAsBizObj.Factory);
					clusterKeyEntity.ClusterKeyPty.Value = newValue;
				}
			}
		}

		bool ShouldSetMasterClusterKey()
		{
			return
				!clusterKeyEntityAsBizObj.IsInDatabase
				|| (clusterKeyEntity is IClusterKeyWorker clusterKeyWorkerOrMaster
					&& IsParentEmpty(clusterKeyWorkerOrMaster)
					&& IsParentDirty(clusterKeyWorkerOrMaster));
		}
	}
}
