using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test;

[TestedType(typeof(EDIOrgStaffAssignments))]
public class EDIOrgStaffAssignmentsTest : EnterpriseBusinessObjectTestCase
{
	#region Implementation

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		return Factory.NewWithValidTestData<EDIOrgStaffAssignments>();
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return Factory.NewWithValidTestData<EDIOrgStaffAssignments>();
	}

	#endregion
}
