using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Portugal.Testing
{
	class PortugalEnableDocumentSigningServiceProviderTest : TestCaseWithFactory
	{
		[TestDate(2024, 3, 8)]
		public void TestPortugalEnableDocumentSigningServiceProvider_BeforeStartDate()
		{
			AssertPortugalEnableDocumentSigningServiceProvider(isProduction: true, expectedValue: false);
			AssertPortugalEnableDocumentSigningServiceProvider(isProduction: false, expectedValue: false);
		}

		[TestDate(2026, 3, 8)]
		public void TestPortugalEnableDocumentSigningServiceProvider_AfterStartDate()
		{
			AssertPortugalEnableDocumentSigningServiceProvider(isProduction: true, expectedValue: true);
			AssertPortugalEnableDocumentSigningServiceProvider(isProduction: false, expectedValue: false);
		}

		void AssertPortugalEnableDocumentSigningServiceProvider(bool isProduction, bool expectedValue)
		{
			var licenceType = isProduction ? DatabaseTypes.Codes.Production : DatabaseTypes.Codes.Test;
			LicenceTypeChanger.SetSystemLicence(licenceType);
			AssertEquals(isProduction, Env.Instance.IsProductionSystem);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			{
				var ptSigningOptionCountryFactory = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCodes.Portugal) as IEnableDocumentSigningServiceProvider;
				AssertEquals("Service enabled", expectedValue, ptSigningOptionCountryFactory.IsEnableDocumentSigningService());
			}
		}
	}
}
