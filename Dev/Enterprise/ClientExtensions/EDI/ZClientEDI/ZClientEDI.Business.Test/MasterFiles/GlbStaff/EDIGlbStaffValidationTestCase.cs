using System;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	public class EDIGlbStaffValidationTestCase : GlbStaffValidationRealTest
	{
		public void TestValidateCalendarEmailAddress()
		{
			var staff = Factory.New<EDIGlbStaff>();

			staff.Notes.AddNew(false, EDIPredefinedNoteTypes.Instance.StaffCalendarEmailAddress.Description, "samuel.wang");
			staff.Validation.ValidateAll();
			AssertEquals(true, staff.HasRowErrors);

			staff.CalendarEmailAddressNote.ST_NoteText = "resource.samuel.wang@cargowise.com";
			staff.Validation.ValidateAll();
			AssertEquals(false, staff.HasRowErrors);
		}

		public void TestNoMandatoryValidation()
		{
			var staff = Factory.New<EDIGlbStaff>();
			AssertEquals(Guid.Empty, staff.GS_GB_HomeBranch);
			AssertEquals(Guid.Empty, staff.GS_GE_HomeDepartment);

			staff.Validation.ValidateAll();
			AssertEquals("Precondition", false, staff.GS_GB_HomeBranchInfo.HasErrors());
			AssertEquals("Precondition", false, staff.GS_GE_HomeDepartmentInfo.HasErrors());

			staff.ShowBranchDepartmentAndPositionErrorsAsWarnings = true;
			staff.Validation.ValidateAll();
			AssertEquals("Errors should now be warnings", false, staff.GS_GB_HomeBranchInfo.HasWarnings());
			AssertEquals("Errors should now be warnings", false, staff.GS_GE_HomeDepartmentInfo.HasWarnings());
		}

		public override void TestValidateGS_GE_HomeDepartment()
		{
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				AssertHomeDepartmentError(false);
			}
		}

		public override void TestValidateGS_GB_HomeBranch()
		{
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				AssertHomeBranchError(false);
			}
		}
	}
}
