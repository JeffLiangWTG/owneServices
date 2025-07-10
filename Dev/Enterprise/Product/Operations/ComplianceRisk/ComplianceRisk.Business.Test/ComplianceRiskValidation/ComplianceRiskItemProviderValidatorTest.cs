using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Business.Test
{
	public class ComplianceRiskItemProviderValidatorTest : TestCase
	{
		public void TestValidateParentId()
		{
			var validationErrorList = new List<string>();
			var mockCommodityRiskStatusProvider = new Mock<IComplianceItemRiskStatusProvider>();
			mockCommodityRiskStatusProvider.Setup(provider => provider.ParentID).Returns(ZGuid.Empty);
			mockCommodityRiskStatusProvider.Setup(provider => provider.ParentTableCode).Returns("JS");
			mockCommodityRiskStatusProvider.Setup(provider => provider.Factory).Returns(new BusinessObjectFactory());
			var commodityProviderValidator = new ComplianceRiskItemProviderValidator(mockCommodityRiskStatusProvider.Object, validationErrorList);

			commodityProviderValidator.ValidateAll();

			AssertEquals(1, validationErrorList.Count);
			AssertEquals("When implementing IComplianceItemRiskStatusProvider, ParentID should not be empty or a invalid value", validationErrorList[0]);

			validationErrorList.Clear();
			mockCommodityRiskStatusProvider.Setup(provider => provider.ParentID).Returns(ZGuid.Invalid);
			commodityProviderValidator = new ComplianceRiskItemProviderValidator(mockCommodityRiskStatusProvider.Object, validationErrorList);

			commodityProviderValidator.ValidateAll();

			AssertEquals(1, validationErrorList.Count);
			AssertEquals("When implementing IComplianceItemRiskStatusProvider, ParentID should not be empty or a invalid value", validationErrorList[0]);
		}

		public void TestValidateParentTableCode()
		{
			var validationErrorList = new List<string>();
			var mockCommodityRiskStatusProvider = new Mock<IComplianceItemRiskStatusProvider>();
			mockCommodityRiskStatusProvider.Setup(provider => provider.ParentID).Returns(ZGuid.BrettsGuid);
			mockCommodityRiskStatusProvider.Setup(provider => provider.ParentTableCode).Returns(ZString.Empty);
			mockCommodityRiskStatusProvider.Setup(provider => provider.Factory).Returns(new BusinessObjectFactory());
			var commodityProviderValidator = new ComplianceRiskItemProviderValidator(mockCommodityRiskStatusProvider.Object, validationErrorList);

			commodityProviderValidator.ValidateAll();

			AssertEquals(1, validationErrorList.Count);
			AssertEquals("When implementing IComplianceItemRiskStatusProvider, ParentTableCode should not be empty", validationErrorList[0]);
		}

		public void TestValidateFactory()
		{
			var validationErrorList = new List<string>();
			var mockCommodityRiskStatusProvider = new Mock<IComplianceItemRiskStatusProvider>();
			mockCommodityRiskStatusProvider.Setup(provider => provider.ParentID).Returns(ZGuid.BrettsGuid);
			mockCommodityRiskStatusProvider.Setup(provider => provider.ParentTableCode).Returns("JS");
			mockCommodityRiskStatusProvider.Setup(provider => provider.Factory).Returns((BusinessObjectFactory)null);
			var commodityProviderValidator = new ComplianceRiskItemProviderValidator(mockCommodityRiskStatusProvider.Object, validationErrorList);

			commodityProviderValidator.ValidateAll();

			AssertEquals(1, validationErrorList.Count);
			AssertEquals("When implementing IComplianceItemRiskStatusProvider, Factory should not be null", validationErrorList[0]);
		}
	}
}
