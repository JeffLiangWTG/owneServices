using System;
using Enterprise.Registry.Business;

namespace Enterprise.LogWalker.Testing
{
	public static class LogUtility
	{
		public static void EnableLog(CodeDescriptionBoolRegistryItem registryItem, string key)
		{
			var pairs = registryItem.Value;
			pairs.Set(key, true);
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pairs);
		}

		public static void DisableLog(CodeDescriptionBoolRegistryItem registryItem, string key)
		{
			var pairs = registryItem.Value;
			pairs.Set(key, false);
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pairs);
		}
	}
}
