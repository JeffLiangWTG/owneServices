using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.CountryCompliance.GlobalCountryFactory;
using Enterprise.Accounting.CountryCompliance.Implementation.Testing.CountrySpecificRegistryDefaultValues;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.CountryCompliance.Implementation.CountrySpecificRegistryDefaultValues.Testing
{
	public class IDefaultValuesForCountrySpecificRegistryItemsTest : TestCaseWithFactory
	{
		public void TestIDefaultValuesForCountrySpecificRegistryItemsImplementingCountry_ProvidesTestData_AND_HasCorrectDefaultValues()
		{
			var countriesQuery = new ZQuery();
			countriesQuery.OrderBy = RefCountrySchema.Constants.RN_Code;
			var countryCodes = Factory.Load<RefCountry>(countriesQuery);

			foreach (var country in countryCodes)
			{
				var defaultValuesForCountrySpecificRegistryItems = GetIDefaultValuesForCountrySpecificRegistryItems(country.Code);

				if (defaultValuesForCountrySpecificRegistryItems != null)
				{
					var testData = GetCountrySpecificTestData(country.Code);
					AssertNotNull($"Country ({country.Code}) extends DefaultValuesForCountrySpecificRegistryItems and should provide test data by extending DefaultTestDataForCountrySpecificRegistryItems.", testData);

					CombineAssertions(() =>
					{
						AssertEquals($"Country ({country.Code}) - ShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency", testData.ExpectedDefaultShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency, defaultValuesForCountrySpecificRegistryItems.GetShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency());
						AssertEquals($"Country ({country.Code}) - EInvoicingRequeueDelayTime", testData.EInvoicingRequeueDelayTime, defaultValuesForCountrySpecificRegistryItems.EInvoicingRequeueDelayTime);
						AssertEquals($"Country ({country.Code}) - ThirdPartyEInvoiceDocType", testData.ThirdPartyEInvoiceDocType, defaultValuesForCountrySpecificRegistryItems.ThirdPartyEInvoiceDocType);
						AssertEquals($"Country ({country.Code}) - PrintWatermarkForTransactionAwaitingApproval", testData.PrintWatermarkForTransactionAwaitingApproval, defaultValuesForCountrySpecificRegistryItems.PrintWatermarkForTransactionAwaitingApproval);
						AssertEquals($"Country ({country.Code}) - EnableGovernmentAllocatedNumberBehavior", testData.EnableGovernmentAllocatedNumberBehavior, defaultValuesForCountrySpecificRegistryItems.EnableGovernmentAllocatedNumberBehavior);
						AssertEquals($"Country ({country.Code}) - EInvoicingReversalCodes", testData.EInvoicingReversalCodesDefault, defaultValuesForCountrySpecificRegistryItems.EInvoicingReversalCodes);
						AssertEquals($"Country ({country.Code}) - EInvoicingAmendmentCodes", testData.EInvoicingAmendmentCodesDefault, defaultValuesForCountrySpecificRegistryItems.EInvoicingAmendmentCodes);
						AssertEquals($"Country ({country.Code}) - TaxMessageIsMandatoryPayables", testData.TaxMessageIsMandatoryPayables, defaultValuesForCountrySpecificRegistryItems.TaxMessageIsMandatoryPayables);
						AssertEquals($"Country ({country.Code}) - TaxMessageIsMandatoryReceivables", testData.TaxMessageIsMandatoryReceivables, defaultValuesForCountrySpecificRegistryItems.TaxMessageIsMandatoryReceivables);
						AssertEquals($"Country ({country.Code}) - SAFTGroupingCategory", testData.SAFTGroupingCategory, defaultValuesForCountrySpecificRegistryItems.SAFTGroupingCategory);
					});
				} else
				{
					var testData = GetCountrySpecificTestData(country.Code);
					AssertNull($"Country ({country.Code}) does not implement the interface but has expected values for it.", testData);
				}
			}
		}

		IDefaultValuesForCountrySpecificRegistryItems GetIDefaultValuesForCountrySpecificRegistryItems(ZString countryCode)
		{
			return ((IAccountingCountryComplianceGlobalFactory)new AccountingCountryComplianceGlobalFactory()).GetFeatureInterface<IDefaultValuesForCountrySpecificRegistryItems>(countryCode);
		}

		DefaultTestDataForCountrySpecificRegistryItems GetCountrySpecificTestData(string countryCode)
		{
			return countryCode switch
			{
				CountryCodes.Chile =>				new ChileRegistryItemsDefaultValuesTestData(),
				CountryCodes.China =>				new ChinaRegistryItemsDefaultValuesTestData(),
				CountryCodes.CzechRepublic =>		new CzechRepublicRegistryItemsDefaultValuesTestData(),
				CountryCodes.DominicanRepublic =>	new DominicanRepublicRegistryItemsDefaultValuesTestData(),
				CountryCodes.Israel =>				new IsraelRegistryItemsDefaultValuesTestData(),
				CountryCodes.Norway =>				new NorwayRegistryItemsDefaultValuesTestData(),
				CountryCodes.Kosovo =>				new KosovoRegistryItemsDefaultValuesTestData(),
				CountryCodes.SaintMartin =>			new SaintMartinRegistryItemsDefaultValuesTestData(),
				CountryCodes.Singapore =>			new SingaporeRegistryItemsDefaultValuesTestData(),
				CountryCodes.Spain =>				new SpainRegistryItemsDefaultValuesTestData(),
				CountryCodes.Turkey =>				new TurkeyRegistryItemsDefaultValuesTestData(),
				_ => null,
			};
		}
	}
}
