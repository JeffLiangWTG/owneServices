using System.Text;
using CargoWise.EntityFramework.Testing;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.ZArchitecture.Core.Testing;

namespace Enterprise.ComplianceRisk.Business.Test
{
	public abstract class ComplianceRiskBusinessObjectTestCase : TestCaseWithFactory
	{
		public void TestItemRiskStatusProviderIsNotNull()
		{
			AssertNotNull(GetComplianceItemRiskStatusProvider());
		}

		public void TestShouldProvideNecessaryDataWhenImplementComplianceRiskProviders()
		{
			var complianceItemRiskStatusProvider = GetComplianceItemRiskStatusProvider();

			CentralizedValidator.ValidateAll(complianceItemRiskStatusProvider);
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestComplianceRiskSupportAndBillable()
		{
			var complianceItemRiskStatusProvider = GetComplianceItemRiskStatusProvider();
			var messageBuilder = new StringBuilder("Incorrect ComplianceRiskSupport!");
			if ((complianceItemRiskStatusProvider.ComplianceRiskSupport & ComplianceRiskSupport.SupportInitialization) is ComplianceRiskSupport.SupportInitialization)
			{
				messageBuilder.AppendLine("Please make sure the scripts of STL collectors has also been updated synchronously by MDM team.");
			}

			AssertEquals(messageBuilder.ToString(), SupporterInfo_DoNotModifyThisBeforeCheckingBaseImplementation, complianceItemRiskStatusProvider.ComplianceRiskSupport);
		}

		public virtual void TestRiskCalculateFactor()
		{
			var complianceItemRiskStatusProvider = GetComplianceItemRiskStatusProvider();
			if (complianceItemRiskStatusProvider is IComplianceCommodityRiskStatusProvider provider)
			{
				AssertEquals(CommodityRiskCalculateFactor.All, provider.RiskCalculateFactor);
			}
		}

		protected virtual ComplianceRiskSupport SupporterInfo_DoNotModifyThisBeforeCheckingBaseImplementation => ComplianceRiskSupport.None;

		protected abstract IComplianceItemRiskStatusProvider GetComplianceItemRiskStatusProvider();
	}
}
