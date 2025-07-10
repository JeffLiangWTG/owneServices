using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Dash.Business.Testing
{
	[TestedType(typeof(DashOrgCandidateAddress))]
	sealed class DashOrgCandidateAddressTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var helpers = new DashBusinessObjectTestHelpers(Factory);
			var eDoc1 = helpers.CreateStorageFile();
			var dashDocument = helpers.CreateDashDocument(eDoc1.PK, "CIV");
			var dashOrgCandidate = helpers.CreateDashOrgCandidate(dashDocument.PK, "SHP");
			var dashOrgCandidateAddress = helpers.CreateDashOrgCandidateAddress(dashOrgCandidate.PK, "123 Foo Street, Bartown, BAZ 456");

			return dashOrgCandidateAddress;
		}
	}
}
