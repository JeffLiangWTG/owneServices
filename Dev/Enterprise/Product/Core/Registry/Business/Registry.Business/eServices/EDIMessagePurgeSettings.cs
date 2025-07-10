using System.Collections;
using CargoWise.Application;

namespace Enterprise.Registry.Business
{
	public static class EDIMessagePurgeSettings
	{
		public static ApplicationCodeObjCollection GetAllPurgeSettings()
		{
			var result = new ApplicationCodeObjCollection();
			var configs = ObjectFactory.Get<IEnumerable>("MessagePurgeSettingsConfigList");
			foreach (PurgeSettingsConfig config in configs)
			{
				result.AddRange(config.GetPurgeSettings());
			}
			return new ApplicationCodeObjCollectionMergeManager().MergeApplicationCodeObjCollections(result, GetClientSpecificPurgeSetting());
		}

		static ApplicationCodeObjCollection GetClientSpecificPurgeSetting()
		{
			var zClientResult = new ApplicationCodeObjCollection();
			var clientSpecificConfig = ZClientSpecificPurgeSettingConfig.New();
			zClientResult.AddRange(clientSpecificConfig.GetPurgeSettings());
			return zClientResult;
		}
	}
}
