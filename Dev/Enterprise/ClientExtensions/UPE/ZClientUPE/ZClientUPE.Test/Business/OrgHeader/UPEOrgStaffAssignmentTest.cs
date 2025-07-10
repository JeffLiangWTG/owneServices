using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPEOrgStaffAssignment))]
	class UPEOrgStaffAssignmentTest : OrgStaffAssignmentsTest
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
			Factory.New<UPEOrgStaffAssignment>();
			AssertEquals("", ErrorReporter.LastMessageReported);
		}

		#region Regular Volume ADP Scoring
		public void TestRegularVolumeADPScoring()
		{
			var modifiedAssignment = Organisation.StaffAssignments.AddNew();
			modifiedAssignment.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;
			Factory.Save();
			modifiedAssignment.O8_Role = UPEStaffRoles.Codes.RV;
			Factory.Save();
			AssertEquals("RV count should be incremented for modified assignment", 1, ADPScoring.T4_RegularVolumeCount);
			var addedAssignment = Organisation.StaffAssignments.AddNew();
			addedAssignment.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;
			addedAssignment.O8_Department = "FRT";
			addedAssignment.O8_Role = UPEStaffRoles.Codes.RV;
			Factory.Save();
			AssertEquals("RV count should be incremented for added assignment", 2, ADPScoring.T4_RegularVolumeCount);
		}

		public void TestRegularVolumeADPScoring_WhenRoleChangedFromRV()
		{
			OrgStaffAssignments assignment = Organisation.StaffAssignments.AddNew();
			assignment.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;
			assignment.O8_Role = UPEStaffRoles.Codes.RV;
			Factory.Save();
			AssertEquals("RV count should be incremented when assigned", 1, ADPScoring.T4_RegularVolumeCount);
			assignment.O8_Role = UPEStaffRoles.Codes.Classifier;
			Factory.Save();
			AssertEquals("RV count should be decremented when unassignment", 0, ADPScoring.T4_RegularVolumeCount);
		}

		public void TestRegularVolumeADPScoring_WhenRVRoleDeleted()
		{
			OrgStaffAssignments decoyAssignment = Organisation.StaffAssignments.AddNew();
			decoyAssignment.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;
			decoyAssignment.O8_Role = UPEStaffRoles.Codes.AccountManager;
			OrgStaffAssignments assignment = Organisation.StaffAssignments.AddNew();
			assignment.O8_GS_NKPersonResponsible = GlbStaff.CurrentUser.GS_Code;
			assignment.O8_Role = UPEStaffRoles.Codes.RV;
			Factory.Save();
			AssertEquals("RV count should be incremented when assigned", 1, ADPScoring.T4_RegularVolumeCount);
			decoyAssignment.Delete();
			assignment.Delete();
			AssertEquals("RV count should be decremented when RV role is deleted", 0, ADPScoring.T4_RegularVolumeCount);
		}

		OrgHeader Organisation
		{
			get
			{
				if (fOrganisation == null)
				{
					fOrganisation = Factory.NewWithValidTestData<OrgHeader>();
				}

				return fOrganisation;
			}
		}

		OrgHeader fOrganisation;
		UPEADPScoring ADPScoring
		{
			get
			{
				return new UPEADPScoring.Loader(Factory).LoadOrCreate();
			}
		}
		#endregion
	}
}
