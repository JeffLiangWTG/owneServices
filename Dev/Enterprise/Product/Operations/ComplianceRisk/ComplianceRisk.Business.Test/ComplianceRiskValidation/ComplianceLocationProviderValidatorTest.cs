using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.ComplianceRisk.Business.Test
{
	public class ComplianceLocationProviderValidatorTest : TestCaseWithFactory
	{
		public void TestValidateLocation()
		{
			var validationErrorList = new List<string>();
			var mockLocationRiskStatusProvider = new Mock<IComplianceLocationRiskStatusProvider>();
			mockLocationRiskStatusProvider.Setup(provider => provider.Locations).Returns((IEnumerable<ScreeningParty>)null);
			var locationProviderValidator = new ComplianceLocationProviderValidator(mockLocationRiskStatusProvider.Object, validationErrorList);

			locationProviderValidator.ValidateAll();

			AssertEquals(1, validationErrorList.Count);
			AssertEquals("When implementing IComplianceLocationRiskStatusProvider, Locations should not be null", validationErrorList[0]);
		}

		public void TestValidateLocationIsCountry()
		{
			var validationErrorList = new List<string>();
			var mockLocationRiskStatusProvider = new Mock<IComplianceLocationRiskStatusProvider>();
			var country = Factory.NewWithValidTestData<RefCountry>();

			mockLocationRiskStatusProvider.Setup(provider => provider.Locations).Returns(new[] { new ScreeningParty(country, "party", (RefCountry)null) });
			var locationProviderValidator = new ComplianceLocationProviderValidator(mockLocationRiskStatusProvider.Object, validationErrorList);

			locationProviderValidator.ValidateAll();

			AssertEquals(1, validationErrorList.Count);
			AssertEquals("When implementing IComplianceLocationRiskStatusProvider, location.Country should not be null", validationErrorList[0]);
		}

		public void TestValidateRN_CodeOrRN_Desc()
		{
			var validationErrorList = new List<string>();
			var mockLocationRiskStatusProvider = new Mock<IComplianceLocationRiskStatusProvider>();
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = ZString.Empty;
			country.RN_Desc = ZString.Empty;

			mockLocationRiskStatusProvider.Setup(provider => provider.Locations).Returns(new[] { new ScreeningParty(country, "party", country) });
			var locationProviderValidator = new ComplianceLocationProviderValidator(mockLocationRiskStatusProvider.Object, validationErrorList);

			locationProviderValidator.ValidateAll();

			AssertEquals(2, validationErrorList.Count);
			AssertEquals("When implementing IComplianceLocationRiskStatusProvider, location.Code should not be empty", validationErrorList[0]);
			AssertEquals("When implementing IComplianceLocationRiskStatusProvider, location.LocationDescription should not be empty", validationErrorList[1]);
		}
	}
}
