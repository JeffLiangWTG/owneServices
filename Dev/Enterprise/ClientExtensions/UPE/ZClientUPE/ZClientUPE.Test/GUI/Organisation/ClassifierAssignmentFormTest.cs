using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Testing
{
	[TestedType(typeof(ClassifierAssignmentForm))]
	public class ClassifierAssignmentFormTest : ZFormBasherTest
	{
		public void TestForCaption()
		{
			AssertEquals("Form caption should be 'Update Clasifier Role'", "Update Clasifier Roles", UPEForm.FormCaption);
		}

		public void TestOKButton_Click()
		{
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
			using (ClassifierAssignmentFormTestClass uPEForm = new ClassifierAssignmentFormTestClass(UPEUpdater))
			{
				UPEUpdater.StaffPKToReplace = ZGuid.Empty;
				uPEForm.TestOKButtonClick();
				AssertEquals("should still be the same staff member", staffMember.GS_Code, staffAssignment1.O8_GS_NKPersonResponsible);
				AssertEquals("should still be the same staff member", staffMember.GS_Code, staffAssignment2.O8_GS_NKPersonResponsible);
				AssertEquals("should still be the same staff member", staffMember.GS_Code, staffAssignment3.O8_GS_NKPersonResponsible);
				AssertEquals("should be 0 records updated", "0 staff assignment(s) have been updated.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (ClassifierAssignmentFormTestClass uPEForm = new ClassifierAssignmentFormTestClass(UPEUpdater))
			{
				UPEUpdater.StaffPKToReplace = staffMember.PK;
				UPEUpdater.NewStaffPK = ZGuid.Empty;
				uPEForm.TestOKButtonClick();
				AssertEquals("should still be the same staff member", staffMember.GS_Code, staffAssignment1.O8_GS_NKPersonResponsible);
				AssertEquals("should still be the same staff member", staffMember.GS_Code, staffAssignment2.O8_GS_NKPersonResponsible);
				AssertEquals("should still be the same staff member", staffMember.GS_Code, staffAssignment3.O8_GS_NKPersonResponsible);
				AssertEquals("should be 0 records updated", "0 staff assignment(s) have been updated.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			GlbStaff newStaffMember = Factory.NewWithValidTestData<GlbStaff>(TestBusinessObjectKind.MinimumRequiredToSave);
			Factory.Save();
			using (ClassifierAssignmentFormTestClass uPEForm = new ClassifierAssignmentFormTestClass(UPEUpdater))
			{
				UPEUpdater.NewStaffPK = newStaffMember.PK;
				uPEForm.TestOKButtonClick();
				AssertEquals("should be new staff member", newStaffMember.GS_Code, staffAssignment1.O8_GS_NKPersonResponsible);
				AssertEquals("should be new staff member", newStaffMember.GS_Code, staffAssignment2.O8_GS_NKPersonResponsible);
				AssertEquals("should not change, staff was not assigned the classifier role", staffMember.GS_Code, staffAssignment3.O8_GS_NKPersonResponsible);
				AssertEquals("should be 2 records updated", "2 staff assignment(s) have been updated.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new ClassifierAssignmentForm(new UPEStaffAssignmentUpdater(Factory));
		}

		#region Implementation
		ClassifierAssignmentFormTestClass UPEForm;
		UPEStaffAssignmentUpdater UPEUpdater;
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			UPEUpdater = new UPEStaffAssignmentUpdater(Factory);
			UPEForm = new ClassifierAssignmentFormTestClass(UPEUpdater);
		}

		protected override void TearDown()
		{
			if (!UPEForm.IsDisposed)
			{
				UPEForm.Dispose();
			}

			base.TearDown();
		}

		class ClassifierAssignmentFormTestClass : ClassifierAssignmentForm
		{
			public ClassifierAssignmentFormTestClass(UPEStaffAssignmentUpdater updater) : base(updater)
			{
			}

			public void TestOKButtonClick()
			{
				base.OKButton_Click(null, EventArgs.Empty);
			}

			public ProgressForm ProgressBar
			{
				get
				{
					return base.Progress;
				}
			}
		}
		#endregion
	}
}
