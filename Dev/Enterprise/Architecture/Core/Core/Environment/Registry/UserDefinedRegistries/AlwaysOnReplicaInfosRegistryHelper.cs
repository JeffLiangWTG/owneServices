using System;
using System.Linq;
using System.Text.Json;
using CargoWise.Data.SqlServer;

namespace Enterprise.ZArchitecture.Environment
{
	public static class AlwaysOnReplicaInfosRegistryHelper
	{
		public static AlwaysOnReplicaInfo[] Deserialise(string[] value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			return value.Select(v => JsonSerializer.Deserialize<AlwaysOnReplicaInfo>(v)).ToArray();
		}

		public static string[] Serialise(AlwaysOnReplicaInfo[] value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			return value.Select(v => JsonSerializer.Serialize(v)).ToArray();
		}
	}
}
