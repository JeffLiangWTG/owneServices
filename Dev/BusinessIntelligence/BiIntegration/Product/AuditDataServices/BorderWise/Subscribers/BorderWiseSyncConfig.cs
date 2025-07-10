using BorderWise.Sync;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Definitions;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.AuditDataServices.BorderWise.Subscribers
{
	public class BorderWiseSyncConfig : IBorderWiseSyncConfig
	{
		BorderWiseSyncConfig() { }

		public static bool Enabled => ObjectFactory.Get<IBorderWiseSyncConfig>().Enabled;

		bool IBorderWiseSyncConfig.Enabled
		{
			get
			{
				var isEdi = ClientHookLoader.Instance.Client == Clients.EDI;
				return isEdi && BWSyncIsEnabled.LoadValue(Db.Connection);
			}
		}

		public static string SyncBootstrapServers => ObjectFactory.Get<IBorderWiseSyncConfig>().SyncBootstrapServers;
		string IBorderWiseSyncConfig.SyncBootstrapServers => BWSyncBootstrapServers.LoadValue(Db.Connection);

		public static string SyncTopicOutbound => ObjectFactory.Get<IBorderWiseSyncConfig>().SyncTopicOutbound;
		string IBorderWiseSyncConfig.SyncTopicOutbound => BWSyncTopic.LoadValue(Db.Connection);

		public static string SyncTopicInbound => ObjectFactory.Get<IBorderWiseSyncConfig>().SyncTopicInbound;
		string IBorderWiseSyncConfig.SyncTopicInbound => BWSyncTopicInbound.LoadValue(Db.Connection);

		public static string SyncUserName => ObjectFactory.Get<IBorderWiseSyncConfig>().SyncUserName;
		string IBorderWiseSyncConfig.SyncUserName => BWSyncUserName.LoadValue(Db.Connection);

		public static string SyncPassword => ObjectFactory.Get<IBorderWiseSyncConfig>().SyncPassword;
		string IBorderWiseSyncConfig.SyncPassword => BWSyncPassword.LoadValue(Db.Connection);

		#region Registry Items

		BoolDbRegistryItem BWSyncIsEnabled => bwSyncIsEnabled ?? (bwSyncIsEnabled = new BoolDbRegistryItem("BorderWise_SyncEnabled", defaultValue: false));
		BoolDbRegistryItem bwSyncIsEnabled;

		StringDbRegistryItem BWSyncBootstrapServers => bwSyncBootstrapServers ?? (bwSyncBootstrapServers = new StringDbRegistryItem("BorderWise_SyncBootstrapServers", defaultValue: string.Empty));
		StringDbRegistryItem bwSyncBootstrapServers;

		StringDbRegistryItem BWSyncTopic => bwSyncTopic ?? (bwSyncTopic = new StringDbRegistryItem("BorderWise_SyncTopic", defaultValue: string.Empty));
		StringDbRegistryItem bwSyncTopic;

		StringDbRegistryItem BWSyncTopicInbound => bwSyncTopicInbound ?? (bwSyncTopicInbound = new StringDbRegistryItem("BorderWise_SyncTopic_Inbound", defaultValue: string.Empty));
		StringDbRegistryItem bwSyncTopicInbound;

		StringDbRegistryItem BWSyncUserName => bwSyncUserName ?? (bwSyncUserName = new StringDbRegistryItem("BorderWise_SyncUserName", defaultValue: string.Empty));
		StringDbRegistryItem bwSyncUserName;

		StringDbRegistryItem BWSyncPassword => bwSyncPassword ?? (bwSyncPassword = new StringDbRegistryItem("BorderWise_SyncPassword", defaultValue: string.Empty));
		StringDbRegistryItem bwSyncPassword;

		#endregion
	}
}
