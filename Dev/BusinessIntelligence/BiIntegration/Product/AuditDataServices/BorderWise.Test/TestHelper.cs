using System;
using BorderWise.Sync;
using CargoWise.Application;
using CargoWise.Common;

namespace Enterprise.AuditDataServices.BorderWise.Test
{
	static class TestHelper
	{
		public static IDisposable EnableSyncForTest(IBorderWiseChangesPublisher publisher = null)
		{
			return EnableSyncForTest(out _, publisher);
		}

		public static IDisposable EnableSyncForTest(out BorderWiseSyncConfigOverride config, IBorderWiseChangesPublisher publisher = null)
		{
			var result = new DisposableList(2);

			config = new BorderWiseSyncConfigOverride { Enabled = true };
			result.Add(ObjectFactory.Substitute(nameof(IBorderWiseSyncConfig), config));
			if (publisher != null)
			{
				result.Add(ObjectFactory.Substitute(nameof(IBorderWiseChangesPublisher), publisher));
			}

			return result;
		}

		public static string GetDateTimeUtcNowString()
		{
			return DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
		}
	}

	#region BorderWiseSyncConfigOverride

	class BorderWiseSyncConfigOverride : IBorderWiseSyncConfig
	{
		public bool Enabled { get; set; }
		public string SyncBootstrapServers { get; set; }
		public string SyncTopicOutbound { get; set; }
		public string SyncTopicInbound { get; set; }
		public string SyncUserName { get; set; }
		public string SyncPassword { get; set; }
	}

	#endregion
}
