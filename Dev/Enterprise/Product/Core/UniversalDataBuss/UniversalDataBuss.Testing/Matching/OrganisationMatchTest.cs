using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Management.Matching;

namespace Enterprise.UniversalDataBuss.Matching.Testing
{
	class OrganisationMatchTest : TestCaseWithFactory
	{
		public void TestNewToJobDocAddress()
		{
			AssertNotNull("OrganisationMatch.NewToJobDocAddress()"
				, OrganizationAddressMatchToJobDocAddress.New(MatchableOrganizationType.ConsigneeDocumentaryAddress, DocAddressType.ConsigneeDocumentaryAddress, organisationMatcher, OrganisationTypes.None, null));
		}

		OrganizationAddressMatchPool organisationMatcher
		{
			get { return new OrganizationAddressMatchPool(); }
		}
	}
}
