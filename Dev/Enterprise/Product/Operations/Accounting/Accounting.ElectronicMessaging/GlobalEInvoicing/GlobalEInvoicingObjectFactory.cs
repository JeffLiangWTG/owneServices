#define CODE_ANALYSIS

using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Argentina;
using Enterprise.Accounting.ElectronicMessaging.Brazil;
using Enterprise.Accounting.ElectronicMessaging.Chile;
using Enterprise.Accounting.ElectronicMessaging.China;
using Enterprise.Accounting.ElectronicMessaging.Colombia;
using Enterprise.Accounting.ElectronicMessaging.CostaRica;
using Enterprise.Accounting.ElectronicMessaging.CountryFactory;
using Enterprise.Accounting.ElectronicMessaging.D365;
using Enterprise.Accounting.ElectronicMessaging.Egypt;
using Enterprise.Accounting.ElectronicMessaging.Hungary;
using Enterprise.Accounting.ElectronicMessaging.India;
using Enterprise.Accounting.ElectronicMessaging.Israel;
using Enterprise.Accounting.ElectronicMessaging.Jordan;
using Enterprise.Accounting.ElectronicMessaging.KoreaSouth;
using Enterprise.Accounting.ElectronicMessaging.Latvia;
using Enterprise.Accounting.ElectronicMessaging.Malaysia;
using Enterprise.Accounting.ElectronicMessaging.Mauritius;
using Enterprise.Accounting.ElectronicMessaging.Mexico;
using Enterprise.Accounting.ElectronicMessaging.Philippines;
using Enterprise.Accounting.ElectronicMessaging.Poland;
using Enterprise.Accounting.ElectronicMessaging.Romania;
using Enterprise.Accounting.ElectronicMessaging.SaudiArabia;
using Enterprise.Accounting.ElectronicMessaging.Serbia;
using Enterprise.Accounting.ElectronicMessaging.Spain;
using Enterprise.Accounting.ElectronicMessaging.Turkey;
using Enterprise.Accounting.ElectronicMessaging.Uruguay;
using Enterprise.Integration.Accounting;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing
{
	public static class GlobalEInvoicingObjectFactory
	{
		public static ICountryEInvoicingObjectFactory GetICountryEInvoicingObjectFactory(ZString countryCode)
		{
#if DEBUG
			if (countryCode == TestCountryCode || TestCountryCode == TestCountryCodeForAllCountry)
			{
				return TestCountryFactory;
			}
#endif
			return GetGlobalEInvoicingObjectFactory(countryCode);
		}

		static CountryEInvoicingObjectFactory GetGlobalEInvoicingObjectFactory(ZString countryCode)
		{
			// Please keep the list of countries in alphabetical order.
			return (string)countryCode switch
			{
				CountryCodes.Argentina => new ArgentinaEInvoicingObjectFactory(),
				CountryCodes.Brazil => new BrazilEInvoicingObjectFactory(),
				CountryCodes.Chile => new ChileEInvoicingObjectFactory(),
				CountryCodes.China => new ChinaEInvoicingObjectFactory(),
				CountryCodes.Colombia => new ColombiaEInvoicingObjectFactory(),
				CountryCodes.CostaRica => new CostaRicaEInvoicingObjectFactory(),
				CountryCodes.DominicanRepublic => new DominicanRepublicEInvoicingObjectFactory(),
				CountryCodes.Egypt => new EgyptEInvoicingObjectFactory(),
				CountryCodes.Hungary => new HungaryEInvoicingObjectFactory(),
				CountryCodes.India => new IndiaEInvoicingObjectFactory(),
				CountryCodes.Israel => new IsraelEInvoicingObjectFactory(),
				CountryCodes.Jordan => new JordanEInvoicingObjectFactory(),
				CountryCodes.KoreaSouth => new KoreaSouthEInvoicingObjectFactory(),
				CountryCodes.Latvia => new LatviaEInvoicingObjectFactory(),
				CountryCodes.Malaysia => new MalaysiaEInvoicingObjectFactory(),
				CountryCodes.Mauritius => new MauritiusEInvoicingObjectFactory(),
				CountryCodes.Mexico => new MexicoEInvoicingObjectFactory(),
				CountryCodes.Panama => new PanamaEInvoicingObjectFactory(),
				CountryCodes.Philippines => new PhilippinesEInvoicingObjectFactory(),
				CountryCodes.Poland => new PolandEInvoicingObjectFactory(),
				CountryCodes.Romania => new RomaniaEInvoicingObjectFactory(),
				CountryCodes.SaudiArabia => new SaudiArabiaEInvoicingObjectFactory(),
				CountryCodes.Serbia => new SerbiaEInvoicingObjectFactory(),
				CountryCodes.Spain => new SpainEInvoicingObjectFactory(),
				CountryCodes.Turkey => new TurkeyEInvoicingObjectFactory(),
				CountryCodes.UnitedKingdom => new D365ObjectFactory(),
				CountryCodes.Uruguay => new UruguayEInvoicingObjectFactory(),
				_ => null,
			};
		}

		// Please keep the list of countries in alphabetical order.

		readonly static ImmutableHashSet<ZString> CountriesSupportingElectronicInvoicing = new ZString[]
		{
			CountryCodes.Argentina,
			CountryCodes.Brazil,
			CountryCodes.Chile,
			CountryCodes.China,
			CountryCodes.Colombia,
			CountryCodes.CostaRica,
			CountryCodes.DominicanRepublic,
			CountryCodes.Egypt,
			CountryCodes.Fiji,
			CountryCodes.Germany,
			CountryCodes.Hungary,
			CountryCodes.India,
			CountryCodes.Israel,
			CountryCodes.Italy,
			CountryCodes.Jordan,
			CountryCodes.KoreaSouth,
			CountryCodes.Latvia,
			CountryCodes.Malaysia,
			CountryCodes.Mauritius,
			CountryCodes.Mexico,
			CountryCodes.Panama,
			CountryCodes.Philippines,
			CountryCodes.Poland,
			CountryCodes.Romania,
			CountryCodes.SaudiArabia,
			CountryCodes.Serbia,
			CountryCodes.Spain,
			CountryCodes.Turkey,
			CountryCodes.UnitedKingdom,
			CountryCodes.Uruguay,
			CountryCodes.VietNam,
			CountryCodes.WesternSamoa,
		}.ToImmutableHashSet();

		[CodeAlive("Used through interface declared in Spring.Net xml")]
		public class CrossAssemblyAccess : IGlobalEInvoicingObjectFactory
		{
			IReadOnlyCollection<ZString> IGlobalEInvoicingObjectFactory.SupportedCountryCodes => CountriesSupportingElectronicInvoicing;

			ZBool IGlobalEInvoicingObjectFactory.DoesCountrySupportElectronicInvoicing(ZString countryCode) => CountriesSupportingElectronicInvoicing.Contains(countryCode);

			ICountryEInvoicingObjectFactorySettings IGlobalEInvoicingObjectFactory.GetCountryEInvoicingObjectFactorySettings(ZString countryCode) => GetGlobalEInvoicingObjectFactory(countryCode);

			ICountryEInvoicingRegistryInformationProvider IGlobalEInvoicingObjectFactory.GetCountryEInvoicingRegistryInformationProvider(ZString countryCode) => GetGlobalEInvoicingObjectFactory(countryCode);

			IBatchCreatorStrategy IGlobalEInvoicingObjectFactory.GetCountryEInvoicingBatchCreatorStrategy(ZString countryCode) => GetGlobalEInvoicingObjectFactory(countryCode) as IBatchCreatorStrategy;
		}

#if DEBUG
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021", Justification = "Static fields in this class do not need to be thread safe")]
		internal static ZString TestCountryCode { get; set; } = "Netlandia";

		internal const string TestCountryCodeForAllCountry = "{23AF10C0-272E-468E-B22E-AF85AE703C5A}";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021", Justification = "Static fields in this class do not need to be thread safe")]
		internal static ICountryEInvoicingObjectFactory TestCountryFactory { get; set; }
#endif
	}
}
