using System.Collections;
using CargoWise.Application;
using Enterprise.Customs.EU.TemporaryStorage.Business;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public static class TempStoragePremisesLayoutProviderHelper
	{
		public static ITempStoragePremisesLayoutProvider GetLayoutProvider(CusTempStorageRegPremises tempStorageRegPremises)
		{
			ITempStoragePremisesLayoutProvider provider = null;

			if (tempStorageRegPremises is CusTempStorageRegPremises premises)
			{
				var providers = ObjectFactory.Get<Hashtable>(ProvidersConfigurationName);
				if ((provider = GetLayoutProvider(providers, premises.LayoutProviderKey)) is null)
				{
					provider = GetLayoutProvider(providers, DefaultProviderKey);
				}
			}

			return provider;
		}

		static ITempStoragePremisesLayoutProvider GetLayoutProvider(Hashtable providers, string key)
		{
			ITempStoragePremisesLayoutProvider result = null;

			if (providers != null && providers.ContainsKey(key))
			{
				var objectHandle = (ObjectHandle)providers[key];
				result = (ITempStoragePremisesLayoutProvider)objectHandle.GetObject();
			}

			return result;
		}

		const string ProvidersConfigurationName = "TempStoragePremisesLayoutProviders";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		const string DefaultProviderKey = "Default";
	}
}
