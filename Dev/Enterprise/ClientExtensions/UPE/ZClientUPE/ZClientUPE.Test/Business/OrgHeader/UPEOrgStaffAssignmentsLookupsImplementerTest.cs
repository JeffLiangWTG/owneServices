using CargoWise.Common;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business
{
	[TestedType(typeof(UPEOrgStaffAssignmentsLookupsImplementer))]
	public class UPEOrgStaffAssignmentsLookupsImplementerTest : OrgStaffAssignmentsLookupsImplementerTest
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		public void TestCustomisationsLoadedForRegistry()
		{
			ErrorReporter.Clear();
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			new UPEOrgStaffAssignmentsLookupsImplementer(Factory);
			AssertEquals("", ErrorReporter.LastMessageReported);
		}

		public void TestUPEStaffRoles()
		{
			var implementer = new UPEOrgStaffAssignmentsLookupsImplementer(Factory);
			Assert(implementer.StaffRoles.ContainsCode("CLS"));
			Assert(implementer.StaffRoles.ContainsCode("RV"));
		}
	}
}
