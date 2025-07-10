using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core.Testing;

namespace Enterprise.ComplianceRisk.Business.Test
{
	public class CentralizedValidatorTest : TestCaseWithFactory
	{
		public void TestShouldThrowExceptionWhenImplementIComplianceItemRiskStatusProviderDirectly()
		{
			new ComplianceRiskBusinessObject(GetInvalidJobBizOThatOnlyImplementItemProvider());
			AssertContains("The interface IComplianceItemRiskStatusProvider should not be implemented directly.", ExceptionReporterTestListener.Instance[0].Message);

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestShouldDisplayAllErrorMessageWhenValidationExceptionHappened()
		{
			CentralizedValidator.ValidateAll(GetInvalidJobBizOThatOnlyImplementCommodityProvider());
			AssertLessThanOrEqualTo(5, ExceptionReporterTestListener.Instance[0].Message.Split(new[] { System.Environment.NewLine }, System.StringSplitOptions.RemoveEmptyEntries).Length);

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestShouldMapToPartyValidatorWhenBizoWithInvalidPartyData()
		{
			CentralizedValidator.ValidateAll(GetInvalidJobBizOThatOnlyImplementPartyProvider());
			AssertContains("When implementing ICompliancePartyRiskStatusProvider, Parties should not be null", ExceptionReporterTestListener.Instance[0].Message);

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestShouldMapToCommodityValidatorWhenBizoWithInvalidCommodityData()
		{
			CentralizedValidator.ValidateAll(GetInvalidJobBizOThatOnlyImplementCommodityProvider());
			AssertContains("When implementing IComplianceCommodityRiskStatusProvider, Commodities should not be null", ExceptionReporterTestListener.Instance[0].Message);

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestShouldMapToLocationValidatorWhenBizOWithInvalidLocationData()
		{
			CentralizedValidator.ValidateAll(GetInvalidJobBizOThatOnlyImplementLocationProvider());
			AssertContains("When implementing IComplianceLocationRiskStatusProvider, Locations should not be null", ExceptionReporterTestListener.Instance[0].Message);

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestShouldNotThrowExceptionWhenAllFieldsCorrect()
		{
			var jobBizO = GetValidJobBizOThatImplementItemAllProviders();
			AssertNoExceptionThrown(() => new ComplianceRiskBusinessObject(jobBizO));
		}

		DummyBizObjThatImplementPartyAndLocationAndCommodityProvider GetValidJobBizOThatImplementItemAllProviders() => new(Factory);

		DummyBizObjThatImplementIComplianceItemRiskStatusProvider GetInvalidJobBizOThatOnlyImplementItemProvider() => new(Factory);

		DummyBizObjThatImplementICompliancePartyRiskStatusProviderWithInvalidValue GetInvalidJobBizOThatOnlyImplementPartyProvider() => new(Factory);

		DummyBizObjThatImplementIComplianceLocationRiskStatusProviderWithInvalidValue GetInvalidJobBizOThatOnlyImplementLocationProvider() => new(Factory);

		DummyBizObjThatImplementIComplianceCommodityRiskStatusProviderWithInvalidValue GetInvalidJobBizOThatOnlyImplementCommodityProvider() => new(Factory);
	}
}
