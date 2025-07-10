using System.Collections;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

public class TSCustomsNumberViewStmNumsBusinessProviderFactory
{
	public TSCustomsNumberViewStmNumsBusinessProviderFactory(BusinessObjectFactory factory)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
	}
	readonly BusinessObjectFactory factory;

	public TSCustomsNumberViewStmNumsBusinessProvider GetProvider(string providerKey, ZGuid parentPk)
	{
		if (!parentPk.IsValid)
		{
			return null;
		}

		var cacheKey = FormattableString.Invariant($"TSCustomsNumberViewStmNumsBusinessProvider_{providerKey}_{parentPk}");
		return factory.GetCachedValue(cacheKey, () =>
		{
			TSCustomsNumberViewStmNumsBusinessProvider provider = null;
			var providers = ObjectFactory.Get<Hashtable>("TSCustomsNumberViewStmNumsBusinessProviders");

			if (providers is not null && !TryGetProvider(providers, providerKey, providerKey, parentPk, out provider))
			{
				_ = TryGetProvider(providers, DefaultProviderValueResolverKey, providerKey, parentPk, out provider);
			}

			return provider;
		});
	}

	bool TryGetProvider(Hashtable providers, string key, ZString providerKey, ZGuid parentPk, out TSCustomsNumberViewStmNumsBusinessProvider provider)
	{
		provider = null;
		if (!providers.ContainsKey(key))
		{
			return false;
		}

		var objectHandle = (ObjectHandle)providers[key];
		provider = (TSCustomsNumberViewStmNumsBusinessProvider)objectHandle.GetObject(factory, providerKey, parentPk);
		return true;
	}

	protected virtual string DefaultProviderValueResolverKey => (NoResString)"Default";
}
