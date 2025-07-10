using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocOrgStaffAssignmentsCollection))]
	public class DocOrgStaffAssignmentsCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocOrgStaffAssignmentsCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			OrgStaffAssignments staffAssignments = Factory.NewWithValidTestData<OrgStaffAssignments>();
			return DocOrgStaffAssignments.New(staffAssignments, Factory);
		}

		protected override DocOrgStaffAssignmentsCollection GetCollectionToTest()
		{
			return new DocOrgStaffAssignmentsCollection(Factory);
		}
	}
}
