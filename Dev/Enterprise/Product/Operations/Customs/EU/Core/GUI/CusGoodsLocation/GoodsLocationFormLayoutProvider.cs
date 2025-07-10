using System.Collections;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class GoodsLocationFormLayoutProvider : IGoodsLocationFormLayoutProvider
	{
		public static IGoodsLocationFormLayoutProvider GetLayoutProvider(ICusGoodsLocationProvider goodsLocationProvider)
		{
			IGoodsLocationFormLayoutProvider result = null;

			if (goodsLocationProvider != null)
			{
				var providers = ObjectFactory.Get<Hashtable>(GoodsLocationFormLayoutProvidersConfigurationName);
				var providerKey = goodsLocationProvider.ProviderKey;

				if (!providerKey.IsEmpty)
				{
					result = GetProvider(providerKey);

					if (result == null)
					{
						var euProviderKey = GetEuProviderKey(providerKey);

						if (!euProviderKey.IsEmpty)
						{
							result = GetProvider(euProviderKey);
						}
					}
				}

				if (result == null)
				{
					result = GetProvider(DefaultProviderKey);
				}

				IGoodsLocationFormLayoutProvider GetProvider(string key)
				{
					var objectHandle = (ObjectHandle)providers[key];
					return (IGoodsLocationFormLayoutProvider)objectHandle?.GetObject();
				}
			}

			return result;
		}

		static ZString GetEuProviderKey(ZString providerKey)
		{
			if (providerKey.Length > 2)
			{
				var countryCode = providerKey.Left(2).ToString();

				if (ObjectFactory.Get<Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(countryCode))
				{
					var applicationCode = providerKey.Right(providerKey.Length - 2).ToString();
					var applicationList = new GoodsLocationProviderApplications();

					if (applicationList.ContainsCode(applicationCode))
					{
						return Constants.CountryCodes.EuropeanUnion + applicationCode;
					}
				}
			}

			return string.Empty;
		}

		IPanelLayoutProvider IGoodsLocationFormLayoutProvider.GetGoodsLocationLayout() => new CusGoodsLocationLayout();

		const string GoodsLocationFormLayoutProvidersConfigurationName = "GoodsLocationFormLayoutProviders";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		const string DefaultProviderKey = "Default";
	}
}
