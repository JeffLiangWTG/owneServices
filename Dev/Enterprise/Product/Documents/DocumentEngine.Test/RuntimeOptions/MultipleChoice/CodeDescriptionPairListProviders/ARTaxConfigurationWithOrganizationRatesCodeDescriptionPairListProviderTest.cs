using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Moq;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class ARTaxConfigurationWithOrganizationRatesCodeDescriptionPairListProviderTest : CodeDescriptionPairListProviderTest
	{
		public override void TestIsReturningCorrectCollection()
		{
			var factory = new BusinessObjectFactory();

			var mockHelper = TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper();

			var taxConfigurationCollection = new AccTaxConfigurationCollection(factory);

			var taxConfig1 = taxConfigurationCollection.AddNew();
			taxConfig1.ETC_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			taxConfig1.ETC_Code = taxConfig_AP_Code;
			taxConfig1.ETC_Description = taxConfig_AP_Desc;

			var expectedResult = new CodeDescriptionPairList();
			AssertGetARTaxConfigurationThatSupportsOrganisationRates();

			var taxConfig2 = taxConfigurationCollection.AddNew();
			taxConfig2.ETC_Ledger = TaxConfigurationLedgers.AccountsReceivable.Code;
			taxConfig2.ETC_Code = taxConfig_AR_Code;
			taxConfig2.ETC_Description = taxConfig_AR_Desc;

			expectedResult = ExpectedTaxConfigurationsForCompany();
			AssertGetARTaxConfigurationThatSupportsOrganisationRates();

			void AssertGetARTaxConfigurationThatSupportsOrganisationRates()
			{
				mockHelper.Reset();
				mockHelper.Setup(x => x.GetTaxConfigurationThatSupportsOrganisationRates(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>())).Returns(taxConfigurationCollection);

				var actualResult = CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList();
				AssertContainsExactElementsInAnyOrder(expectedResult, actualResult);

				mockHelper.Verify(x => x.GetTaxConfigurationThatSupportsOrganisationRates(It.IsAny<ReadOnlyBusinessObjectFactory>(), GlbCompany.CurrentCompany), Times.Once);
			}
		}

		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new ARTaxConfigurationWithOrganizationRatesCodeDescriptionPairListProvider();
		}

		CodeDescriptionPairList ExpectedTaxConfigurationsForCompany()
		{
			var expectedResult = new CodeDescriptionPairList();
			expectedResult.AddPair(taxConfig_AR_Code, taxConfig_AR_Desc);

			return expectedResult;
		}

		const string taxConfig_AR_Code = "AR";
		const string taxConfig_AR_Desc = "AR config";

		const string taxConfig_AP_Code = "AP";
		const string taxConfig_AP_Desc = "AP config";
	}
}
