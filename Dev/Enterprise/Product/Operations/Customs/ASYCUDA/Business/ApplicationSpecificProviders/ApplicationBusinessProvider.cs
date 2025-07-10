using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public abstract class ApplicationBusinessProvider
	{
		public void Initialise(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, "factory");
		}

		protected BusinessObjectFactory factory;

		public abstract Type AsycudaManifestHeaderType { get; }

		public IReadOnlyList<IManifestType> ManifestTypes => manifestTypes ??= AllManifestTypes.Where(x => x.Enabled).ToList();
		IReadOnlyList<IManifestType> manifestTypes;

		public IReadOnlyList<IManifestType> AllManifestTypes => allManifestTypes ??= CreateManifestTypes();
		IReadOnlyList<IManifestType> allManifestTypes;

		internal void ResetManifestTypes()
		{
			manifestTypes = null;
			allManifestTypes = null;
		}

		protected abstract IReadOnlyList<IManifestType> CreateManifestTypes();

		public IReadOnlyList<ZString> ApplicableCountryCodes(Directions direction, ZString transportMode, string manifestStyle = null) => ApplicableCountryCodesCore(direction, transportMode, manifestStyle);

		protected virtual IReadOnlyList<ZString> ApplicableCountryCodesCore(Directions direction, ZString transportMode, string manifestStyle = null)
			=> manifestStyle == null || ManifestTypes.Any(manifestType => manifestType.ApplicableManifestStyles.Contains(manifestStyle)) ? CountryCodes : new ReadOnlyCollection<ZString>(new List<ZString>());

		public IReadOnlyList<ZString> CountryCodes => countryCodes ?? (countryCodes = CreateCountryCodes());
		IReadOnlyList<ZString> countryCodes;

		protected abstract IReadOnlyList<ZString> CreateCountryCodes();

		public virtual string ApplicationCode => ZString.Empty;

		public virtual IEnumerable<string> GetAcceptableTransportModesFromAsycudaManifestHeader(BusinessObjectFactory factory, string countryCode, string applicationCode, Directions direction)
		{
			return ManifestTypes.Where(x => x.ApplicableManifestStyles.Contains(applicationCode))
				.SelectMany(x => x.ApplicableTransportModes).ToArray();
		}

		public abstract MessagingProvider MessagingProvider { get; }

		public abstract FeatureProvider FeatureProvider { get; }

		public static ApplicationBusinessProvider GetApplicationBusinessProvider(AsycudaManifestHeader header)
			=> GetApplicationBusinessProvider(header.Factory, header.GetApplicationProviderKey());

		public static ApplicationBusinessProvider GetApplicationBusinessProvider(BusinessObjectFactory factory, (ZString CountryOrGrouping, ZString ManfestTypeCode, ZString ApplicationCode) applicationProviderKey)
		{
			var countryOrGrouping = applicationProviderKey.CountryOrGrouping;
			var manifestTypeCode = applicationProviderKey.ManfestTypeCode;
			var applicationCode = applicationProviderKey.ApplicationCode;
			var key = (countryOrGrouping, manifestTypeCode, applicationCode);

			var dictionary = GetApplicationBusinessProvidersDictionary(factory);
			if (dictionary.ContainsKey(key))
			{
				return dictionary[key];
			}

			var defaultProvider = ObjectFactory.Get<ApplicationBusinessProvider>("ASYCUDAManifest.ApplicationBusinessProvider");
			defaultProvider.Initialise(factory);
			return defaultProvider;
		}

		public static IEnumerable<ApplicationBusinessProvider> GetAllApplicationBusinessProviders(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("GlobalManifestApplicationBusinessProviders", () =>
			{
				var providers = ObjectFactory.Get<IEnumerable>("GlobalManifestApplicationBusinessProvider").Cast<ApplicationBusinessProvider>().ToArray();
				foreach (var provider in providers)
				{
					provider.Initialise(factory);
				}
				return providers;
			});
		}

		public static IDictionary<(ZString CountryOrGrouping, ZString ManifestTypeCode, ZString ApplicationCode), ApplicationBusinessProvider> GetApplicationBusinessProvidersDictionary(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("GetApplicationBusinessProvidersDictionary", () =>
			{
				var dictionary = new Dictionary<(ZString CountryOrGrouping, ZString ManifestTypeCode, ZString ApplicationCode), ApplicationBusinessProvider>();
				var errorMessage = new ZStringBuilder();
				foreach (var provider in GetAllApplicationBusinessProviders(factory))
				{
					if (provider.AllManifestTypes.Count > 0)
					{
						foreach (var countryCode in provider.CountryCodes)
						{
							foreach (var manifestType in provider.AllManifestTypes)
							{
								foreach (var manifestStyle in manifestType.ApplicableManifestStyles)
								{
									TryAdd((countryCode, manifestType.Code, manifestStyle), provider);
								}
							}
						}
					}
					else if (!provider.ApplicationCode.IsNullOrEmpty())
					{
						foreach (var countryCode in provider.CountryCodes)
						{
							TryAdd((countryCode, ZString.Empty, provider.ApplicationCode), provider);
						}
					}
				}
				if (!errorMessage.IsEmpty)
				{
					const string errorKey = "Enterprise.Customs.ASYCUDA.Business.ApplicationBusinessProvider.DuplicateProvider";
					ErrorReporter.ReportOnce(errorKey, "Ambigous Application Provider ignored. See debug information below:\r\n" + errorMessage.ToStringWithNewLineBetweenAppends());
				}
				return dictionary;

				void TryAdd((ZString CountryOrGrouping, ZString ManifestTypeCode, ZString ApplicationCode) key, ApplicationBusinessProvider newProvider)
				{
					if (dictionary.TryGetValue(key, out var provider))
					{
						errorMessage
							.AppendFormat("CountryOrGrouping='{0}' ManifestTypeCode='{1}' ApplicationCode='{2}'", key.CountryOrGrouping, key.ManifestTypeCode, key.ApplicationCode)
							.AppendFormat("  Provider1={0} (AllManifestTypes.Count={1})", provider.GetType().FullName, provider.AllManifestTypes.Count.ToString())
							.AppendFormat("  Provider2={0} (AllManifestTypes.Count={1})", newProvider.GetType().FullName, newProvider.AllManifestTypes.Count.ToString());
					}
					else
					{
						dictionary.Add(key, newProvider);
					}
				}
			});
		}

		public static IEnumerable<ApplicationBusinessProvider> GetApplicationBusinessProviders(BusinessObjectFactory factory, string countryCode, string manifestType = null)
		{
			var cacheKey = string.Format(CultureInfo.InvariantCulture, "ApplicationBusinessProviders_{0}_{1}", countryCode, manifestType); // CachedValueKey
			var result = factory.GetCachedValue(cacheKey, () =>
			{
				var providers = GetAllApplicationBusinessProviders(factory).Where(x => x.CountryCodes.Contains(countryCode));
				if (!string.IsNullOrEmpty(manifestType))
				{
					providers = providers.Where(x => x.ManifestTypes.Any(y => y.Code == manifestType));
				}
				if (providers.IsNullOrEmpty())
				{
					var defaultProvider = ObjectFactory.Get<ApplicationBusinessProvider>("ASYCUDAManifest.ApplicationBusinessProvider");
					defaultProvider.Initialise(factory);
					providers = new[] { defaultProvider };
				}
				return providers;
			});
			return result;
		}

		public static IEnumerable<IManifestType> GetAllApplicationManifestTypes(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(nameof(GetAllApplicationManifestTypes), () => GetAllApplicationBusinessProviders(factory).SelectMany(c => c.ManifestTypes));
		}

		public static IList<string> GetManifestCountriesWithActiveManifestTypes(BusinessObjectFactory factory, string manifestStyle = null)
		{
			var cacheKey = string.Format(CultureInfo.InvariantCulture, "ManifestCountries_{0}", manifestStyle); // CachedValueKey
			return factory.GetCachedValue<IList<string>>(cacheKey, () =>
			{
				return GetManifestApplicationBusinessProvidersForActiveManifestTypes(factory, manifestStyle)
					.SelectMany(x => x.CountryCodes).Select(x => x.ToString()).Distinct().ToList();
			});
		}

		public static IEnumerable<ApplicationBusinessProvider> GetManifestApplicationBusinessProvidersForActiveManifestTypes(BusinessObjectFactory factory, string manifestStyle = null)
		{
			var cacheKey = string.Format(CultureInfo.InvariantCulture, "ManifestApplicationBusinessProvidersForActiveManifestTypes_{0}", manifestStyle); // CachedValueKey
			return factory.GetCachedValue(cacheKey, () =>
			{
				Func<IManifestType, bool> filter = x => true;
				if (!string.IsNullOrEmpty(manifestStyle))
				{
					filter = x => x.ApplicableManifestStyles.Contains(manifestStyle);
				}

				return GetAllApplicationBusinessProviders(factory).Where(provider => provider.ManifestTypes.Any(filter));
			});
		}

		public static ICodeDescriptionPairList GetManifestTypeListByCountry(BusinessObjectFactory factory, ZString country)
		{
			return factory.GetCachedValue($"GetManifestTypeListByCountry_{country}", () =>
			{
				var result = new CodeDescriptionPairList();
				if (country.IsEmpty)
				{
					result.AddRange(GetAllApplicationManifestTypes(factory).ToArray());
				}
				else
				{
					var manifestTypes = GetApplicationBusinessProviders(factory, country).SelectMany(provider => provider.ManifestTypes).ToArray();
					result.AddRange(manifestTypes);
				}

				result.Sort();
				return result;
			});
		}

		public IEnumerable<IManifestType> GetApplicableManifestTypes(Directions direction, string transportMode)
		{
			return GetApplicableManifestTypesCore(direction, transportMode);
		}

		protected virtual IEnumerable<IManifestType> GetApplicableManifestTypesCore(Directions direction, string transportMode)
		{
			return ManifestTypes.Where(manifestType =>
						manifestType.ApplicableManifestStyles.Contains(ApplicationCodeTypeList.Codes.Consolidator));
		}

		public virtual IEnumerable<(ZString CountryCode, ZString Description)> GetManifestDescriptions(BusinessObjectFactory factory, IEnumerable<ZString> countryCodes, Func<IManifestType, bool> filter) => countryCodes.Select(countryCode => (countryCode, RefCountry.LoadFromCountryCode(factory, countryCode).Description));

		#region GetCustomsDeclarationDataObjectWriter

		public IAsycudaForCustomsDeclarationDataObjectWriter GetCustomsDeclarationDataObjectWriter(IDataWritingManager manager, AsycudaManifestHeader header)
		{
			return GetCustomsDeclarationDataObjectWriterCore(manager, GetAsycudaManifestHeaderDataObjectWriterHelper(header));
		}

		protected abstract IAsycudaForCustomsDeclarationDataObjectWriter GetCustomsDeclarationDataObjectWriterCore(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper);

		#endregion

		#region GetAsycudaManifestHeaderDataObjectWriter

		public IAsycudaManifestHeaderDataObjectWriter GetAsycudaManifestHeaderDataObjectWriter(IDataWritingManager manager) => GetAsycudaManifestHeaderDataObjectWriterCore(manager);
		protected abstract IAsycudaManifestHeaderDataObjectWriter GetAsycudaManifestHeaderDataObjectWriterCore(IDataWritingManager manager);

		#endregion

		public AsycudaManifestUniversalMessagingHelper GetUniversalMessagingHelper(INotifications notifications) => GetUniversalMessagingHelperCore(notifications);
		protected virtual AsycudaManifestUniversalMessagingHelper GetUniversalMessagingHelperCore(INotifications notifications) => new AsycudaManifestUniversalMessagingHelper(notifications);

		public AsycudaManifestDataObjectReaderHelper GetAsycudaManifestDataObjectReaderHelper(string countryCode) => GetAsycudaManifestDataObjectReaderHelperCore(countryCode);
		protected virtual AsycudaManifestDataObjectReaderHelper GetAsycudaManifestDataObjectReaderHelperCore(string countryCode) => new AsycudaManifestDataObjectReaderHelper(countryCode, factory);

		public AsycudaUniversalEventMessageProcessor GetNewAsycudaUniversalEventMessageProcessor(IXmlSessionTracker logger, Event universalEvent, AsycudaEDIMessage ediMessage, AsycudaManifestHeader manifestHeader) => universalEvent?.DataContext == null ? null : GetNewAsycudaUniversalEventMessageProcessorCore(logger, universalEvent, ediMessage, manifestHeader);
		protected virtual AsycudaUniversalEventMessageProcessor GetNewAsycudaUniversalEventMessageProcessorCore(IXmlSessionTracker logger, Event universalEvent, AsycudaEDIMessage ediMessage, AsycudaManifestHeader manifestHeader)
			=> universalEvent.DataContext.ActionPurposeCode == AsycudaEventMessageConstants.ActionPurpose.ERR ? new AsycudaUniversalEventMessageFailureProcessor(logger, universalEvent, ediMessage, manifestHeader) : null;

		public AsycudaManifestHeaderDataObjectWriterHelper GetAsycudaManifestHeaderDataObjectWriterHelper(AsycudaManifestHeader header) => GetAsycudaManifestHeaderDataObjectWriterHelperCore(header);
		protected virtual AsycudaManifestHeaderDataObjectWriterHelper GetAsycudaManifestHeaderDataObjectWriterHelperCore(AsycudaManifestHeader header) => new AsycudaManifestHeaderDataObjectWriterHelper(header);

		public AsycudaBillEventContextReader GetAsycudaBillEventContextReader(AsycudaBill bill) => GetAsycudaBillEventContextReaderCore(bill);
		protected virtual AsycudaBillEventContextReader GetAsycudaBillEventContextReaderCore(AsycudaBill bill) => new AsycudaBillEventContextReader(bill);

		public virtual ZString PackedItemTariffDataGrouping => ZString.Empty;
		public virtual ZString PackedItemTariffType => Universal.Constants.TariffTypes.HarmonizedSystem;
		public virtual List<SelectionStyle> SelectNomenclatureModes => new List<SelectionStyle> { SelectionStyle.Tariff };

		public ZDateTime GetEffectiveDateForDutyRate(AsycudaManifestHeader header) => GetEffectiveDateForDutyRateCore(header);
		protected virtual ZDateTime GetEffectiveDateForDutyRateCore(AsycudaManifestHeader header) => ZDateTime.Today;

		public virtual bool SupportsAutoSendGlobalManifest(string manifestType) => false;

		public virtual IProcessor GetSendGlobalManifestProcessor(AsycudaManifestHeader header) => null;
	}
}
