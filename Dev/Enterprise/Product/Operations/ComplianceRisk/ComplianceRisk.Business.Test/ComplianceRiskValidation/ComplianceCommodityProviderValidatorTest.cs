using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Business.Test
{
	public class ComplianceCommodityProviderValidatorTest : TestCase
	{
		public void TestValidateCommodity()
		{
			var validationErrorList = new List<string>();
			var mockCommodityRiskStatusProvider = new Mock<IComplianceCommodityRiskStatusProvider>();
			mockCommodityRiskStatusProvider.Setup(provider => provider.Commodities).Returns((IEnumerable<ComplianceCommodity>)null);
			mockCommodityRiskStatusProvider.Setup(provider => provider.AssessmentPointPairInfo).Returns(new ComplianceAssessmentPointPairInfo());
			mockCommodityRiskStatusProvider.Setup(provider => provider.EffectiveDate).Returns(ZDateTime.BrettsBirthday);
			var commodityProviderValidator = new ComplianceCommodityProviderValidator(mockCommodityRiskStatusProvider.Object, validationErrorList);

			commodityProviderValidator.ValidateAll();

			AssertEquals(1, validationErrorList.Count);
			AssertEquals("When implementing IComplianceCommodityRiskStatusProvider, Commodities should not be null", validationErrorList[0]);
		}

		public void TestValidateEffectiveDate()
		{
			var validationErrorList = new List<string>();
			var mockCommodityRiskStatusProvider = new Mock<IComplianceCommodityRiskStatusProvider>();
			mockCommodityRiskStatusProvider.Setup(provider => provider.Commodities).Returns(new List<ComplianceCommodity>());
			mockCommodityRiskStatusProvider.Setup(provider => provider.AssessmentPointPairInfo).Returns(new ComplianceAssessmentPointPairInfo());
			mockCommodityRiskStatusProvider.Setup(provider => provider.EffectiveDate).Returns(ZDateTime.Invalid);
			var commodityProviderValidator = new ComplianceCommodityProviderValidator(mockCommodityRiskStatusProvider.Object, validationErrorList);

			commodityProviderValidator.ValidateAll();

			AssertEquals(1, validationErrorList.Count);
			AssertEquals("When implementing IComplianceCommodityRiskStatusProvider, EffectiveDate should be valid", validationErrorList[0]);

			validationErrorList.Clear();
			mockCommodityRiskStatusProvider.Setup(provider => provider.EffectiveDate).Returns(ZDateTime.Empty);
			commodityProviderValidator.ValidateAll();

			AssertEquals(1, validationErrorList.Count);
			AssertEquals("When implementing IComplianceCommodityRiskStatusProvider, EffectiveDate should be valid", validationErrorList[0]);
		}

		public void TestValidatePointPairInfo()
		{
			var validationErrorList = new List<string>();
			var mockCommodityRiskStatusProvider = new Mock<IComplianceCommodityRiskStatusProvider>();
			mockCommodityRiskStatusProvider.Setup(provider => provider.Commodities).Returns(new List<ComplianceCommodity>());
			mockCommodityRiskStatusProvider.Setup(provider => provider.AssessmentPointPairInfo).Returns((ComplianceAssessmentPointPairInfo)null);
			mockCommodityRiskStatusProvider.Setup(provider => provider.EffectiveDate).Returns((ZDateTime.BrettsBirthday));
			var commodityProviderValidator = new ComplianceCommodityProviderValidator(mockCommodityRiskStatusProvider.Object, validationErrorList);

			commodityProviderValidator.ValidateAll();

			AssertEquals(1, validationErrorList.Count);
			AssertEquals("When implementing IComplianceCommodityRiskStatusProvider, AssessmentPointPairInfo should not be null", validationErrorList[0]);
		}
	}
}
