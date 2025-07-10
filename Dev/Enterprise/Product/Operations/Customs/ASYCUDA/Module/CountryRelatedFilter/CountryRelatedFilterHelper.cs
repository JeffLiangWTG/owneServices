using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ASYCUDA.Module
{
	class CountryRelatedFilterHelper
	{
		public static class ListGetters
		{
			public static Func<string, BusinessObjectFactory, CodeDescriptionPairList> MessageStatusGetter => (countryCode, factory) =>
			{
				var list = new CodeDescriptionPairList();
				var providers = ApplicationBusinessProvider.GetApplicationBusinessProviders(factory, countryCode);
				foreach (var provider in providers)
				{
					var msgStatusProvider = provider?.MessagingProvider?.MessageStatusProvider;
					if (msgStatusProvider != null)
					{
						list.AddPairsIfNotExist(msgStatusProvider.GetMessageStatusList(factory, countryCode).ToArray());
					}
				}
				return list;
			};

			public static Func<string, BusinessObjectFactory, CodeDescriptionPairList> ArrivalStatusGetter => (countryCode, factory) =>
			{
				var list = new CodeDescriptionPairList();
				var providers = ApplicationBusinessProvider.GetApplicationBusinessProviders(factory, countryCode);
				foreach (var provider in providers)
				{
					var msgStatusProvider = provider?.MessagingProvider?.MessageStatusProvider;
					if (msgStatusProvider != null)
					{
						list.AddPairsIfNotExist(msgStatusProvider.GetArrivalStatusList(factory, countryCode).ToArray());
					}
				}
				return list;
			};

			public static Func<string, BusinessObjectFactory, CodeDescriptionPairList> CustomsStatusGetter => (countryCode, factory) =>
			{
				var list = new CodeDescriptionPairList();
				var providers = ApplicationBusinessProvider.GetApplicationBusinessProviders(factory, countryCode);
				foreach (var provider in providers)
				{
					var msgStatusProvider = provider?.MessagingProvider?.MessageStatusProvider;
					if (msgStatusProvider != null)
					{
						list.AddPairsIfNotExist(msgStatusProvider.GetRegistrationStatusList(factory, countryCode, string.Join("|", provider.ManifestTypes)).ToArray());
					}
				}
				return list;
			};

			public static Func<string, BusinessObjectFactory, CodeDescriptionPairList> CustomsOfficeGetter =>
				(countryCode, factory) => AsycudaManifestHeaderLookups.GetCustomsOfficesListForCountry(factory, countryCode);

			public static Func<string, BusinessObjectFactory, CodeDescriptionPairList> CustomsNumberTypeGetter => (countryCode, factory) =>
			{
				var list = new CodeDescriptionPairList();
				var providers = ApplicationBusinessProvider.GetApplicationBusinessProviders(factory, countryCode);
				foreach (var provider in providers)
				{
					var messagingProvider = provider?.MessagingProvider;
					if (messagingProvider != null)
					{
						list.AddPairsIfNotExist(messagingProvider.GetCustomsEntryNumberTypeList(factory, countryCode).ToArray());
					}
				}
				return list;
			};

			public static Func<string, BusinessObjectFactory, CodeDescriptionPairList> CargoStatusGetter => (countryCode, factory) =>
			{
				var list = new CodeDescriptionPairList();
				var providers = ApplicationBusinessProvider.GetApplicationBusinessProviders(factory, countryCode);
				foreach (var provider in providers)
				{
					var msgStatusProvider = provider?.MessagingProvider?.MessageStatusProvider;
					if (msgStatusProvider != null)
					{
						list.AddPairsIfNotExist(msgStatusProvider.GetCargoStatusList(factory, countryCode).ToArray());
					}
				}
				return list;
			};
		}
	}
}
