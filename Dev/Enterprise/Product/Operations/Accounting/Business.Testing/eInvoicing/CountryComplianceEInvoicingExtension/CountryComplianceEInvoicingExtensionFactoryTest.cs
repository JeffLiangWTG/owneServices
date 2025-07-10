using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.EInvoicing.Testing
{
	public class CountryComplianceEInvoicingExtensionFactoryTest : TestCaseWithFactory
	{
		public void TestGetICountryComplianceEInvoicingExtensionFactory()
		{
			AssertType<CountryComplianceEInvoicingExtensionFactory>(ObjectFactory.Get<ICountryComplianceEInvoicingExtensionFactory>());
		}

		readonly Dictionary<string, Type> ExpectedComplianceInfoEInvoicingExtensionTypes = new Dictionary<string, Type>
		{
			{ CountryCodes.China, typeof(ChinaComplianceInfoEInvoicingExtension) },
			{ CountryCodes.KoreaSouth, typeof(KoreaSouthComplianceInfoEInvoicingExtension) },
			{ CountryCodes.Malaysia, typeof(MalaysiaComplianceInfoEInvoicingExtension) },
			{ CountryCodes.Mexico, typeof(MexicoComplianceInfoEInvoicingExtension) },
			{ CountryCodes.Turkey, typeof(TurkeyComplianceInfoEInvoicingExtension) },
			{ CountryCodes.VietNam, typeof(VietnamComplianceInfoEInvoicingExtension) },
		};

		public void TestGetCountryComplianceInfoExtension()
		{
			var countriesQuery = new ZQuery();
			countriesQuery.OrderBy = RefCountrySchema.Constants.RN_Code;
			var countries = Factory.Load<RefCountry>(countriesQuery);

			foreach (var country in countries)
			{
				var countryComplianceInfo = CountryComplianceEInvoicingExtensionFactory.GetCountryComplianceInfoExtension(country.Code);
				if (ExpectedComplianceInfoEInvoicingExtensionTypes.TryGetValue(country.Code, out var expectedType))
				{
					AssertType($"Country compliance info should be of type {nameof(expectedType)}", expectedType, countryComplianceInfo);
				}
				else
				{
					AssertNull($"The country {country.Code} does not have a compliance info EInvoicing extension", countryComplianceInfo);
				}
			}
		}

		public void TestGetICountryComplianceEInvoicingExtensionFactoryIsNotSingleton()
		{
			var countryComplianceEInvoicingExtensionFactory = ObjectFactory.Get<ICountryComplianceEInvoicingExtensionFactory>();
			var countryComplianceEInvoicingExtensionFactory2 = ObjectFactory.Get<ICountryComplianceEInvoicingExtensionFactory>();
			AssertNotEquals(countryComplianceEInvoicingExtensionFactory, countryComplianceEInvoicingExtensionFactory2);
		}

		public void TestGetCountryComplianceInfoExtension_CountryComplianceInfo_NotSingleton()
		{
			var countryComplianceInfo = CountryComplianceEInvoicingExtensionFactory.GetCountryComplianceInfoExtension(CountryCodes.KoreaSouth);
			var countryComplianceInfo2 = CountryComplianceEInvoicingExtensionFactory.GetCountryComplianceInfoExtension(CountryCodes.KoreaSouth);
			AssertNotEquals(countryComplianceInfo, countryComplianceInfo2);
		}

		public void TestGetIEInvoicingTransactionValidation()
		{
			var factory = ObjectFactory.Get<ICountryComplianceEInvoicingExtensionFactory>();
			var eInvoicingTransactionValidation = factory.GetIEInvoicingTransactionValidation(CountryCodes.VietNam);

			AssertNotNull("GetIEInvoicingTransactionValidation", eInvoicingTransactionValidation);
		}
	}
}
