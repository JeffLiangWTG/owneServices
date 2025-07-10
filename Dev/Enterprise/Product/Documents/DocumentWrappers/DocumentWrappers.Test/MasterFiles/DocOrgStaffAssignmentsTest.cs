using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocOrgStaffAssignments))]
	public class DocOrgStaffAssignmentsTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocOrgStaffAssignments.New(orgStaffAssignmentsData, Factory)
			};
		}

		OrgStaffAssignments orgStaffAssignmentsData;
		protected override void SetUp()
		{
			orgStaffAssignmentsData = Factory.New<OrgStaffAssignments>();
			base.SetUp();
		}

		public void TestEmail()
		{
			AssertEquals("Email", orgStaffAssignmentsData.Email, ((DocOrgStaffAssignments)Wrappers[0]).Email);
		}

		public void TestName()
		{
			AssertEquals("Name", orgStaffAssignmentsData.Name, ((DocOrgStaffAssignments)Wrappers[0]).Name);
		}

		public void TestO8_Department()
		{
			orgStaffAssignmentsData.O8_Department = "SEA";
			AssertEquals("O8_Department", "SEA", ((DocOrgStaffAssignments)Wrappers[0]).O8_Department);
		}

		public void TestO8_Role()
		{
			orgStaffAssignmentsData.O8_Role = "CLS";
			AssertEquals("O8_Role", "CLS", ((DocOrgStaffAssignments)Wrappers[0]).O8_Role);
		}

		public void TestO8_GC()
		{
			AssertEquals("O8_GC", orgStaffAssignmentsData.O8_GC, ((DocOrgStaffAssignments)Wrappers[0]).O8_GC);
		}

		public void TestResponsiblePersonLoginName()
		{
			AssertEquals("ResponsiblePersonLoginName", orgStaffAssignmentsData.ResponsiblePersonLoginName, ((DocOrgStaffAssignments)Wrappers[0]).ResponsiblePersonLoginName);
		}

		public void TestResponsiblePersonName()
		{
			AssertEquals("ResponsiblePersonName", orgStaffAssignmentsData.ResponsiblePersonName, ((DocOrgStaffAssignments)Wrappers[0]).ResponsiblePersonName);
		}

		public void TestRoleDescription()
		{
			AssertEquals("RoleDescription", orgStaffAssignmentsData.RoleDescription, ((DocOrgStaffAssignments)Wrappers[0]).RoleDescription);
		}
	}
}
