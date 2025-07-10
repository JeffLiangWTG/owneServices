using System.Collections;
using CargoWise.Application;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public static class TemporaryStorageLayoutProviderHelper
	{
		public static ITemporaryStorageLayoutProvider GetLayoutProvider(TemporaryStorageHeader temporaryStorageHeader)
		{
			ITemporaryStorageLayoutProvider provider = null;

			if (temporaryStorageHeader is TemporaryStorageHeader header)
			{
				var providers = ObjectFactory.Get<Hashtable>(ProvidersConfigurationName);
				if ((provider = GetLayoutProvider(providers, header.LayoutProviderKey)) is null)
				{
					provider = GetLayoutProvider(providers, DefaultProviderKey);
				}
			}

			return provider;
		}

		static ITemporaryStorageLayoutProvider GetLayoutProvider(Hashtable providers, string key)
		{
			ITemporaryStorageLayoutProvider result = null;

			if (providers != null && providers.ContainsKey(key))
			{
				var objectHandle = (ObjectHandle)providers[key];
				result = (ITemporaryStorageLayoutProvider)objectHandle.GetObject();
			}

			return result;
		}

		const string ProvidersConfigurationName = "UCC6TemporaryStorageLayoutProviders";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		const string DefaultProviderKey = "Default";
	}
}
