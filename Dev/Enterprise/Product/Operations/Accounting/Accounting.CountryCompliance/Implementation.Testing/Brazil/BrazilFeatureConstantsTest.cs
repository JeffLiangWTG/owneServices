using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.CountryCompliance.GlobalCountryFactory;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Accounting.CountryCompliance.Interfaces.DataObjects;
using Enterprise.Core;

namespace Enterprise.Accounting.CountryCompliance.Implementation.Brazil.Testing
{
	public class BrazilFeatureConstantsTest : TestCaseWithFactory
	{
		public void TestBrazilCountryFactoryReturnsFeatureConstants()
		{
			var featureConstants = GetFeatureInterface();
			AssertNotNull(featureConstants);
		}

		public void TestFeatureConstantsReturnsSupportedFeatures()
		{
			var featureConstants = GetFeatureInterface();
			SupportedFeatures supportedFeatures = featureConstants.GetFeatureContants<SupportedFeatures>();
			AssertContainsExactElementsInAnyOrder(new[] { Features.IncludeGovernmentBatchReferenceInXUT }, supportedFeatures.Features);
		}

		public void TestFeatureConstantsReturnsNullForOthers()
		{
			var featureConstants = GetFeatureInterface();
			var features = featureConstants.GetFeatureContants<object>();
			AssertNull(features);
		}

		IFeatureConstants GetFeatureInterface() => ((IAccountingCountryComplianceGlobalFactory)new AccountingCountryComplianceGlobalFactory()).GetFeatureInterface<IFeatureConstants>(Constants.CountryCodes.Brazil);
	}
}
