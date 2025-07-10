using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Dash.Business.Testing
{
	[TestedType(typeof(DashOrgCandidate))]
	sealed class DashOrgCandidateTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var helpers = new DashBusinessObjectTestHelpers(Factory);
			var eDoc1 = helpers.CreateStorageFile();
			var dashDocument = helpers.CreateDashDocument(eDoc1.PK, "CIV");
			var dashOrgCandidate = helpers.CreateDashOrgCandidate(dashDocument.PK, "SHP");

			return dashOrgCandidate;
		}
	}
}
