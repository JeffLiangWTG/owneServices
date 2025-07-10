using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPEStaffAssignmentUpdater))]
	public class UPEStaffAssignmentUpdaterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestUpdateClassiferRole()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			UPEUpdater.ProcessingProgressed += new EventHandler(UPEUpdater_ProcessingProgressed);
			FirstStaffToProcessAssignment = true;
			GlbStaff staffMember = Factory.NewWithValidTestData<GlbStaff>(TestBusinessObjectKind.MinimumRequiredToSave);
			UPEOrgHeader org1 = Factory.NewWithValidTestData<UPEOrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			UPEOrgStaffAssignment staffAssignment1 = (UPEOrgStaffAssignment)org1.StaffAssignments.AddNew();
			staffAssignment1.O8_Role = UPEStaffRoles.Codes.Classifier;
			staffAssignment1.O8_GS_NKPersonResponsible = staffMember.GS_Code;
			UPEOrgHeader org2 = Factory.NewWithValidTestData<UPEOrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			UPEOrgStaffAssignment staffAssignment2 = (UPEOrgStaffAssignment)org2.StaffAssignments.AddNew();
			staffAssignment2.O8_Role = UPEStaffRoles.Codes.Classifier;
			staffAssignment2.O8_GS_NKPersonResponsible = staffMember.GS_Code;
			UPEOrgHeader org3 = Factory.NewWithValidTestData<UPEOrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			UPEOrgStaffAssignment staffAssignment3 = (UPEOrgStaffAssignment)org3.StaffAssignments.AddNew();
			staffAssignment3.O8_Role = UPEStaffRoles.Codes.SalesRep;
			staffAssignment3.O8_GS_NKPersonResponsible = staffMember.GS_Code;
			Factory.Save();
			UPEUpdater.StaffPKToReplace = ZGuid.Empty;
			UPEUpdater.UpdateClassiferRole();
			AssertEquals("should still be the same staff member", staffMember.GS_Code, staffAssignment1.O8_GS_NKPersonResponsible);
			AssertEquals("should still be the same staff member", staffMember.GS_Code, staffAssignment2.O8_GS_NKPersonResponsible);
			AssertEquals("should still be the same staff member", staffMember.GS_Code, staffAssignment3.O8_GS_NKPersonResponsible);
			AssertEquals("should be 0 records updated", 0, UPEUpdater.NumberOfRecordsUpdated);
			UPEUpdater.StaffPKToReplace = staffMember.PK;
			UPEUpdater.NewStaffPK = ZGuid.Empty;
			UPEUpdater.UpdateClassiferRole();
			AssertEquals("should still be the same staff member", staffMember.GS_Code, staffAssignment1.O8_GS_NKPersonResponsible);
			AssertEquals("should still be the same staff member", staffMember.GS_Code, staffAssignment2.O8_GS_NKPersonResponsible);
			AssertEquals("should still be the same staff member", staffMember.GS_Code, staffAssignment3.O8_GS_NKPersonResponsible);
			AssertEquals("should be 0 records updated", 0, UPEUpdater.NumberOfRecordsUpdated);
			GlbStaff newStaffMember = Factory.NewWithValidTestData<GlbStaff>(TestBusinessObjectKind.MinimumRequiredToSave);
			Factory.Save();
			UPEUpdater.NewStaffPK = newStaffMember.PK;
			UPEUpdater.UpdateClassiferRole();
			AssertEquals("should be new staff member", newStaffMember.GS_Code, staffAssignment1.O8_GS_NKPersonResponsible);
			AssertEquals("should be new staff member", newStaffMember.GS_Code, staffAssignment2.O8_GS_NKPersonResponsible);
			AssertEquals("should not change, staff was not assigned the classifier role", staffMember.GS_Code, staffAssignment3.O8_GS_NKPersonResponsible);
			AssertEquals("should be 2 records updated", 2, UPEUpdater.NumberOfRecordsUpdated);
		}

		void UPEUpdater_ProcessingProgressed(object sender, EventArgs e)
		{
			if (FirstStaffToProcessAssignment)
			{
				AssertEquals("Percentage Complete should be 50%", 50, UPEUpdater.PercentageComplete);
				FirstStaffToProcessAssignment = false;
			}
			else
			{
				AssertEquals("Percentage Complete should be 100%", 100, UPEUpdater.PercentageComplete);
			}
		}

		bool FirstStaffToProcessAssignment;
		public void TestStaff()
		{
			UPEStaffAssignmentUpdater updater = new UPEStaffAssignmentUpdater(Factory);
			AssertEquals("Staff should be typeof GlbStaffCollection", typeof(GlbStaffCollection), updater.Staff.GetType());
		}

		public void TestValidateStaffPKToReplace()
		{
			UPEUpdater.StaffPKToReplace = ZGuid.Empty;
			UPEUpdater.ValidateStaffPKToReplace();
			AssertHasErrors("Should have errors when StaffPKToReplace is empty", UPEUpdater.StaffPKToReplaceInfo);
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>(TestBusinessObjectKind.MinimumRequiredToSave);
			UPEUpdater.StaffPKToReplace = staff.PK;
			UPEUpdater.ValidateStaffPKToReplace();
			AssertNoErrors("Should have no errors StaffPKToReplace should be valid", UPEUpdater.StaffPKToReplaceInfo);
		}

		public void TestValidateNewStaffPK()
		{
			UPEUpdater.NewStaffPK = ZGuid.Empty;
			UPEUpdater.ValidateNewStaffPK();
			AssertHasErrors("Should have errors when StaffPKToReplace is empty", UPEUpdater.NewStaffPKInfo);
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>(TestBusinessObjectKind.MinimumRequiredToSave);
			UPEUpdater.NewStaffPK = staff.PK;
			UPEUpdater.ValidateNewStaffPK();
			AssertNoErrors("Should have no errors StaffPKToReplace should be valid", UPEUpdater.NewStaffPKInfo);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new UPEStaffAssignmentUpdater(Factory);
		}

		UPEStaffAssignmentUpdater UPEUpdater
		{
			get
			{
				if (fUPEUpdater == null)
				{
					fUPEUpdater = new UPEStaffAssignmentUpdater(Factory);
				}

				return fUPEUpdater;
			}
		}

		UPEStaffAssignmentUpdater fUPEUpdater;
	}
}
