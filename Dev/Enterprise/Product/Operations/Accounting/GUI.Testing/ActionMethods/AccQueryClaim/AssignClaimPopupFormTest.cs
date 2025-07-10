using System;
using System.Windows.Forms;
using CargoWise.Common.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(AssignClaimPopupForm))]
	class AssignClaimPopupFormTest : ZFormBasherTest
	{
		public void TestFormVerb()
		{
			ARAccQueryClaim claim = Factory.New<ARAccQueryClaim>();
			AccQueryClaimReassignAction action = new AccQueryClaimReassignAction(claim);
			using (AssignClaimPopupForm form = new AssignClaimPopupForm(action))
			{
				AssertEquals(String.Empty, form.FormVerb);
			}
		}

		public void TestCloseButton()
		{
			ARAccQueryClaim claim = Factory.New<ARAccQueryClaim>();
			AccQueryClaimReassignAction action = new AccQueryClaimReassignAction(claim);
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			action.StaffCode = ZString.Empty;

			using (AssignClaimPopupForm form = new AssignClaimPopupForm(action))
			{
				form.Show();
				form.OKButton_ForTestOnly.PerformClick();
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(DialogResult.None, form.DialogResult);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				action.StaffCode = staff.GS_Code;
				action.BranchPK = GlbBranch.CurrentBranch.PK;
				form.OKButton_ForTestOnly.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(DialogResult.OK, form.DialogResult);
			}
		}

		public void TestCancelButton()
		{
			DisposableLeakListener.Instance.StackTraceEnabled = true;
			ARAccQueryClaim claim = Factory.New<ARAccQueryClaim>();
			AccQueryClaimReassignAction action = new AccQueryClaimReassignAction(claim);
			action.StaffCode = ZString.Empty;

			using (AssignClaimPopupForm form = new AssignClaimPopupForm(action))
			{
				form.Show();
				form.CancelButtonX_ForTestOnly.PerformClick();
				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			ARAccQueryClaim claim = Factory.New<ARAccQueryClaim>();
			AccQueryClaimReassignAction action = new AccQueryClaimReassignAction(claim);
			return new AssignClaimPopupForm(action);
		}

		#endregion
	}
}
