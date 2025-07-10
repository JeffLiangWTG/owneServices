#define CODE_ANALYSIS

using System;
using CargoWise.Common.Collections;

namespace Enterprise.Security
{
	public interface ISecurityOverrideProviderSource
	{
		ISecurityOverrideProvider Provider { get; set; }
	}

	public static class SecurityOverrideProviderSource
	{
		public static ISecurityOverrideProviderSource Get(object obj)
		{
			return obj as ISecurityOverrideProviderSource ?? (obj == null ? null : new DefaultAccessSecurityProviderSource(obj));
		}

		public static IDisposable TemporarySetProvider(this ISecurityOverrideProviderSource providerSource, ISecurityOverrideProvider provider)
		{
			var result = new TemporarySetProviderAction(providerSource);
			try
			{
				providerSource.Provider = provider;
			}
#pragma warning disable ENT0001
			catch
#pragma warning restore ENT0001
			{
				result.Dispose();

				throw;
			}

			return result;
		}

		class DefaultAccessSecurityProviderSource : ISecurityOverrideProviderSource
		{
			public DefaultAccessSecurityProviderSource(object owner)
			{
				this.owner = owner;
			}

			readonly object owner;

			public ISecurityOverrideProvider Provider
			{
				get { return StoredProviders[owner] ?? new DefaultAccessSecurityProvider(); }
				set { StoredProviders[owner] = value; }
			}

			static WeakReferencedKeyDictionary<object, ISecurityOverrideProvider> StoredProviders
			{
				get { return storedProviders ?? (storedProviders = new WeakReferencedKeyDictionary<object, ISecurityOverrideProvider>()); }
			}
			[ThreadStatic]
			static WeakReferencedKeyDictionary<object, ISecurityOverrideProvider> storedProviders;
		}

		class TemporarySetProviderAction : IDisposable
		{
			public TemporarySetProviderAction(ISecurityOverrideProviderSource providerSource)
			{
				source = providerSource;
				oldProvider = source.Provider;
			}

			public void Dispose()
			{
				source.Provider = oldProvider;
				source = null;
				oldProvider = null;
			}

			ISecurityOverrideProvider oldProvider;
			ISecurityOverrideProviderSource source;
		}
	}
}
