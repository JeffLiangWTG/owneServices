using System;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.Core
{
	public static class SecondaryServerConnectionProviderProvider
	{
		public static ISecondaryServerConnectionProvider GetProvider(Action<Exception, string> showException = null, Action<string> addLogs = null)
		{
			if (provider_Override.IsOverriden)
			{
				return provider_Override.Value;
			}
			return new SecondaryServerConnectionProvider(showException, addLogs);
		}

		public static ISecondaryServerConnectionProvider GetProvider(SecondaryServerConnectionDetailsProvider detailsProvider)
		{
			if (provider_Override.IsOverriden)
			{
				return provider_Override.Value;
			}

			return new SecondaryServerConnectionProvider(detailsProvider);
		}

		public static IDisposable OverrideProvider(ISecondaryServerConnectionProvider connectionProvider)
		{
			provider_Override.Value = connectionProvider;

			return new DisposableAction(() => provider_Override.ResetValue());
		}

		static readonly LazyOverridable<ISecondaryServerConnectionProvider> provider_Override = new LazyOverridable<ISecondaryServerConnectionProvider>(() => null);
	}
}
