using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.ComplianceRisk.Business.Test;

public class CompliancePartyProviderValidatorTest : TestCaseWithFactory
{
	public void TestValidateParties()
	{
		var errors = new List<string>();
		var provider = new Mock<ICompliancePartyRiskStatusProvider>();
		provider.Setup(p => p.Parties).Returns((IEnumerable<ScreeningParty>)null);
		var validator = new CompliancePartyProviderValidator(provider.Object, errors);

		validator.ValidateAll();

		AssertCollectionContains("When implementing ICompliancePartyRiskStatusProvider, Parties should not be null", errors);
	}

	public void TestValidatePartiesWithUnsupportedTypes()
	{
		var errors = new List<string>();
		var provider = new Mock<ICompliancePartyRiskStatusProvider>();
		provider.Setup(p => p.Parties).Returns(new[] { new ScreeningParty(Factory.NewWithValidTestData<OrgHeader>(), "Test", Factory.NewWithValidTestData<RefCountry>()) });
		var validator = new CompliancePartyProviderValidator(provider.Object, errors);

		validator.ValidateAll();

		AssertCollectionContains(@"When implementing ICompliancePartyRiskStatusProvider, every element of Parties should be instance of following supported types:
Enterprise.MasterFiles.Business.OrgHeader
Enterprise.MasterFiles.Business.IScreeningPartyForVessel
Enterprise.MasterFiles.Business.JobDocAddress
Enterprise.MasterFiles.Business.RefVessel
", errors);
	}
}
