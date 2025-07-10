using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test;

[TestedType(typeof(EDIOrgStaffAssignmentsCollection))]
public class EDIOrgStaffAssignmentsCollectionTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		return Factory.New<EDIOrgStaffAssignments>();
	}

	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var orgHeader = Factory.NewWithValidTestData<EDIOrgHeader>();
		return new EDIOrgStaffAssignmentsCollection(orgHeader);
	}
}
