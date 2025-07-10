using System;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	static class ExtensionMethodsForTesting
	{
		public static void SetValue(this CodePairRegistryItem registryItem, string value)
		{
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}
	}
}
