using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.ElectronicMessaging.CountryFactory;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing
{
	public class GlobalEInvoicingObjectFactoryTest : TestCaseWithFactory
	{
		public void TestGetIGlobalEInvoicingObjectFactory()
		{
			AssertType<GlobalEInvoicingObjectFactory.CrossAssemblyAccess>(ObjectFactory.Get<IGlobalEInvoicingObjectFactory>());
		}

		public void TestGetCountryEInvoicingObjectFactoryInvalidCountries()
		{
			var countriesQuery = new ZQuery();
			countriesQuery.OrderBy = RefCountrySchema.Constants.RN_Code;
			var countries = Factory.Load<RefCountry>(countriesQuery);

			foreach (var country in countries)
			{
				var countryEInvoicingObjectFactory = GlobalEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(country.Code);
				if (GetExpectedObjectFactoryForCountry.TryGetValue(country.Code, out var expectedFactoryForCountry))
				{
					AssertType($"Country '{country.Code}' object factory", expectedFactoryForCountry, countryEInvoicingObjectFactory);
				}
				else
				{
					AssertNull($"The country '{country.Code}' isn't in the list of countries expected.", countryEInvoicingObjectFactory);
				}
			}
		}

		public void TestGlobalEInvoicingObjectFactorySupportedCountryCodes()
		{
			var factory = ObjectFactory.Get<IGlobalEInvoicingObjectFactory>();

			AssertContainsExactElementsInAnyOrder(GetCountriesSupportingElectronicInvoicing, factory.SupportedCountryCodes);

			AssertExpectedResultFromArrayList(GetCountriesSupportingElectronicInvoicing, true);
			AssertExpectedResultFromArrayList(GetCountriesNotSupportingElectronicInvoicing, false);

			void AssertExpectedResultFromArrayList(string[] countries, bool expectedResult)
			{
				foreach (var countryCode in countries)
				{
					AssertEquals(expectedResult, factory.DoesCountrySupportElectronicInvoicing(countryCode));
				}
			}
		}

		public void TestGetIGlobalEInvoicingObjectFactoryIsNotSingleton()
		{
			var globalEInvoicingObjectFactory = ObjectFactory.Get<IGlobalEInvoicingObjectFactory>();
			var globalEInvoicingObjectFactory2 = ObjectFactory.Get<IGlobalEInvoicingObjectFactory>();
			AssertNotEquals(globalEInvoicingObjectFactory, globalEInvoicingObjectFactory2);
		}

		public void TestGetCountryEInvoicingObjectFactory_ValidCountries_FactoriesAreNotSingleton()
		{
			var countriesQuery = new ZQuery();
			countriesQuery.OrderBy = RefCountrySchema.Constants.RN_Code;
			var countries = Factory.Load<RefCountry>(countriesQuery);

			foreach (var country in countries)
			{
				var countryEInvoicingObjectFactory = GlobalEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(country.Code);
				if (GetExpectedObjectFactoryForCountry.TryGetValue(country.Code, out var expectedFactoryForCountry))
				{
					var countryEInvoicingObjectFactory2 = GlobalEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(country.Code);
					AssertNotEquals(countryEInvoicingObjectFactory, countryEInvoicingObjectFactory2);
				}
			}
		}

		#region Implementations

		// Please keep the list of countries in alphabetical order.

		Dictionary<string, Type> GetExpectedObjectFactoryForCountry => new Dictionary<string, Type>
		{
		   { CountryCodes.Argentina,			typeof(Argentina.ArgentinaEInvoicingObjectFactory) },
		   { CountryCodes.Brazil,				typeof(Brazil.BrazilEInvoicingObjectFactory) },
		   { CountryCodes.Chile,				typeof(Chile.ChileEInvoicingObjectFactory) },
		   { CountryCodes.China,				typeof(China.ChinaEInvoicingObjectFactory) },
		   { CountryCodes.Colombia,				typeof(Colombia.ColombiaEInvoicingObjectFactory) },
		   { CountryCodes.CostaRica,			typeof(CostaRica.CostaRicaEInvoicingObjectFactory) },
		   { CountryCodes.DominicanRepublic,	typeof(DominicanRepublicEInvoicingObjectFactory) },
		   { CountryCodes.Egypt,				typeof(Egypt.EgyptEInvoicingObjectFactory) },
		   { CountryCodes.Hungary,				typeof(Hungary.HungaryEInvoicingObjectFactory) },
		   { CountryCodes.India,				typeof(India.IndiaEInvoicingObjectFactory) },
		   { CountryCodes.Israel,				typeof(Israel.IsraelEInvoicingObjectFactory) },
		   { CountryCodes.Jordan,				typeof(Jordan.JordanEInvoicingObjectFactory) },
		   { CountryCodes.KoreaSouth,			typeof(KoreaSouth.KoreaSouthEInvoicingObjectFactory) },
		   { CountryCodes.Latvia,				typeof(Latvia.LatviaEInvoicingObjectFactory) },
		   { CountryCodes.Malaysia,				typeof(Malaysia.MalaysiaEInvoicingObjectFactory) },
		   { CountryCodes.Mauritius,			typeof(Mauritius.MauritiusEInvoicingObjectFactory) },
		   { CountryCodes.Mexico,				typeof(Mexico.MexicoEInvoicingObjectFactory) },
		   { CountryCodes.Panama,				typeof(PanamaEInvoicingObjectFactory) },
		   { CountryCodes.Philippines,			typeof(Philippines.PhilippinesEInvoicingObjectFactory) },
		   { CountryCodes.Poland,				typeof(Poland.PolandEInvoicingObjectFactory) },
		   { CountryCodes.Romania,				typeof(Romania.RomaniaEInvoicingObjectFactory) },
		   { CountryCodes.SaudiArabia,			typeof(SaudiArabia.SaudiArabiaEInvoicingObjectFactory) },
		   { CountryCodes.Serbia,				typeof(Serbia.SerbiaEInvoicingObjectFactory) },
		   { CountryCodes.Spain,				typeof(Spain.SpainEInvoicingObjectFactory) },
		   { CountryCodes.Turkey,				typeof(Turkey.TurkeyEInvoicingObjectFactory) },
		   { CountryCodes.UnitedKingdom,		typeof(D365.D365ObjectFactory) },
		   { CountryCodes.Uruguay,				typeof(Uruguay.UruguayEInvoicingObjectFactory) },
		};

		// Please keep the list of countries in alphabetical order.

		string[] GetCountriesSupportingElectronicInvoicing => new string[]
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
		};

		string[] GetCountriesNotSupportingElectronicInvoicing => Country.LicenceKeyBuilderSupportedCountryCodes.Except(GetCountriesSupportingElectronicInvoicing).ToArray();

		#endregion Implementations
	}
}
