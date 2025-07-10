using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Testing
{
	[TestedType(typeof(AlterPaymentApprovalForm))]
	public class AlterPaymentApprovalFormTestCase : ZFormBasherTest
	{
		public void TestSaveAsDraftButton()
		{
			using (AlterPaymentApprovalForm form = (AlterPaymentApprovalForm)GetFormToBash())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Show();
				AssertEquals(true, form.SaveAsDraftButton_ForTestOnly.IsDisposed);
			}
		}

		public void TestOWinForm_ClosingDoesNothng()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (AlterPaymentApprovalForm form = (AlterPaymentApprovalForm)GetFormToBash())
			{
				PaymentApprovalWithAuthorisation approval = form.BusinessEntity as PaymentApprovalWithAuthorisation;
				AssertControlsAreReadonly(approval, true);
				approval.AV_Amount = 100M;
				form.ZForm_Closing_ForTestOnly(null, new CancelEventArgs());
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertControlsAreReadonly(approval, false);
			}
		}

		void AssertControlsAreReadonly(PaymentApprovalBase approval, bool shouldBeReadOnly)
		{
			AssertEquals(shouldBeReadOnly, approval.AV_OHInfo.ReadOnly);
		}

		public void TestShowPreSaveDialogs()
		{
			using (AlterPaymentApprovalForm form = (AlterPaymentApprovalForm)GetFormToBash())
			{
				AssertEquals("ShowPreSaveDialogs_ForTestOnly", ContinueWithSave.No, form.ShowPreSaveDialogs_ForTestOnly());
			}
		}

		public void TestValidateAndSave()
		{
			using (AlterPaymentApprovalForm form = (AlterPaymentApprovalForm)GetFormToBash())
			{
				AssertEquals("ValidateAndSave_ForTestOnly", ContinueWithSave.Yes, form.ValidateAndSave_ForTestOnly());
			}
		}

		public void TestValidateAndSaveDoesNotSave()
		{
			using (AlterPaymentApprovalForm form = (AlterPaymentApprovalForm)GetFormToBash())
			{
				PaymentApprovalWithAuthorisation approval = form.BusinessEntity as PaymentApprovalWithAuthorisation;
				AssertEquals("Precondition: Payment is not already in Database", false, approval.IsInDatabase);
				form.ValidateAndSave_ForTestOnly();
				AssertEquals("Payment should not be in database", false, approval.IsInDatabase);
			}
		}

		protected override Form GetFormToBashCore()
		{
			APPaymentApprovalWithAuthorisation approval = Factory.New<APPaymentApprovalWithAuthorisation>();
			approval.AV_OH = TestOrgHeader.PK;
			approval.HasChanges = false;
			var result = new AlterPaymentApprovalForm(approval);
			result.ControllerID = ControllerIDs.APPaymentProcessing;
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestOrgHeader = Factory.New<OrgHeader>();
			TestOrgHeader.OH_Code = Accounting.Business.TestObjectCreator.GetRandomString(8);
			TestOrgHeader.CompanyData.OB_IsCreditor = true;
			Factory.Save();
		}

		OrgHeader TestOrgHeader;
	}
}
