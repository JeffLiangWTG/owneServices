using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class IncidentListHelperTest : TestCaseWithFactory
	{
		public void TestOrganisationContactList()
		{
			OrgHeader client1 = Factory.New<OrgHeader>();
			OrgHeader client2 = Factory.New<OrgHeader>();

			OrgContact contact1 = Factory.New<OrgContact>();
			OrgContact contact2 = Factory.New<OrgContact>();

			contact1.OC_OH = client1.PK;
			contact2.OC_OH = client2.PK;

			Incident.IM_OH_Client = client1.PK;
			ListHelper.OrganisationContactList.Load();
			AssertEquals("OrganisationContactList should contain Contact1.", true, ListHelper.OrganisationContactList.Contains(contact1));
			AssertEquals("OrganisationContactList should NOT contain Contact2.", false, ListHelper.OrganisationContactList.Contains(contact2));

			Incident.IM_OH_Client = client2.PK;
			ListHelper.OrganisationContactList.Load();
			AssertEquals("OrganisationContactList should NOT contain Contact1.", false, ListHelper.OrganisationContactList.Contains(contact1));
			AssertEquals("OrganisationContactList should contain Contact2.", true, ListHelper.OrganisationContactList.Contains(contact2));
		}

		public void TestStaffList()
		{
			GlbStaff staff = Factory.New<GlbStaff>();

			AssertEquals("StaffList should contain Staff.", true, ListHelper.StaffList.Contains(staff));
		}

		public void TestErrorLogList()
		{
			BusinessObject log1 = Factory.New<EdiHelpErrorLog>();
			BusinessObject log2 = Factory.New<EdiHelpErrorLog>();

			ListHelper.ErrorLogList.Load();

			Assert(ListHelper.ErrorLogList.Contains(log1.PK));
			Assert(ListHelper.ErrorLogList.Contains(log2.PK));
		}

		public void TestReleaseBuildList()
		{
			ReleaseBuild releaseBuild = Factory.New<ReleaseBuild>();

			ListHelper.ReleaseBuildList.Load();
			AssertEquals("ReleaseBuildList should contain ReleaseBuild.", true, ListHelper.ReleaseBuildList.Contains(releaseBuild));
		}

		#region Group Dependent Staff List

		public void TestGroupDependentStaffListCollectionMembers()
		{
			GlbStaff staff1 = Factory.New<GlbStaff>();
			GlbStaff staff2 = Factory.New<GlbStaff>();

			staff1.GS_IsActive = true;
			staff2.GS_IsActive = false;

			AssertEquals("ActiveGroupDependentStaffList should contain Staff1.", true, ListHelper.ActiveGroupDependentStaffList.Contains(staff1));
			AssertEquals("ActiveGroupDependentStaffList should NOT contain Staff2.", false, ListHelper.ActiveGroupDependentStaffList.Contains(staff2));

			AssertEquals("GroupDependentStaffList should contain Staff1.", true, ListHelper.GroupDependentStaffList.Contains(staff1));
			AssertEquals("GroupDependentStaffList should contain Staff2.", true, ListHelper.GroupDependentStaffList.Contains(staff2));
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			Incident = Factory.New<ProfessionalServicesQuote>();
			ListHelper = GetNewListHelper();
		}

		protected virtual IncidentListHelper GetNewListHelper()
		{
			return new IncidentListHelper(Incident);
		}

		protected ProfessionalServicesQuote Incident;
		protected IncidentListHelper ListHelper;

		#endregion
	}
}
