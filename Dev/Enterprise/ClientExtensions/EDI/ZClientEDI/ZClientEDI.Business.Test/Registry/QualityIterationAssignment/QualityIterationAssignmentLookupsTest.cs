using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	internal sealed class QualityIterationAssignmentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestProductAreaList_ShouldNotBeEmpty()
		{
			var assignment = new QualityIterationAssignment();
			AssertNotEquals("The release group list should not be empty.", 0, assignment.Lookups.ReleaseGroupList.Count);
		}
	}
}