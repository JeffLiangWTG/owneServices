using System.Windows.Forms;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(CreditControlledDocumentsApprovalForm))]
	internal class CreditControlledDocumentsApprovalFormWithMultipleRequestsBasherTest : CreditControlledDocumentsApprovalForBasherTest
	{
		public override void TestFormParameters()
		{
			using (CreditControlledDocumentsApprovalForm testForm = (CreditControlledDocumentsApprovalForm)GetFormToBash())
			{
				testForm.Show();
				Application.DoEvents();
				AssertEquals(true, testForm.PostingButtonsUserControl_ForTestOnly.SaveAndCloseButton.Visible);
				AssertEquals(ODisplayMode.Edit, testForm.DisplayMode);
				AssertEquals("Approve", testForm.PostingButtonsUserControl_ForTestOnly.SaveAndCloseButton.Text);
				AssertEquals("TopGridPanel_ForTestOnly.Visible", true, testForm.TopGridPanel_ForTestOnly.Visible);
				AssertEquals("TopSingleRequestPanel_ForTestOnly.Visible", false, testForm.TopSingleRequestPanel_ForTestOnly.Visible);
				AssertEquals("ReasonDescriptionTextBox_ForTestOnly.ReadOnly", true, testForm.ReasonDescriptionTextBox_ForTestOnly.ReadOnly);
				AssertEquals("FormVerb", "Approve", testForm.FormVerb);

				CreditControlledDocumentsApprovalBulk formBizo = (CreditControlledDocumentsApprovalBulk)testForm.BusinessEntity;
				formBizo.RunPreSaveValidation();
				AssertNoErrors("Precondition: ", formBizo);
				testForm.PostingButtonsUserControl_ForTestOnly.SaveAndCloseButton.PerformClick();
				AssertEquals("Factory must not be saved.", true, formBizo.CreditControlledDocumentsApprovalsAllowedToProcess[0].IsInDatabase);
				AssertEquals("Factory must not be saved.", true, formBizo.CreditControlledDocumentsApprovalsAllowedToProcess[1].IsInDatabase);
			}
		}

		public override void TestControlVisibility()
		{
			ApprovalFromMode = CreditControlledDocumentsApprovalFormModes.Approve;
			AssertControlVisibility(true, false, false, false, false, false);

			ApprovalFromMode = CreditControlledDocumentsApprovalFormModes.Reject;
			AssertControlVisibility(true, false, false, false, false, false);

			ApprovalFromMode = CreditControlledDocumentsApprovalFormModes.Cancel;
			AssertControlVisibility(true, false, false, false, false, false);

			ApprovalFromMode = CreditControlledDocumentsApprovalFormModes.View;
			AssertControlVisibility(true, false, false, false, false, false);

			ApprovalFromMode = CreditControlledDocumentsApprovalFormModes.SetDescription;
			AssertControlVisibility(false, false, false, false, false, false);
		}

		public override void TestOpenJob()
		{
			using (CreditControlledDocumentsApprovalForm testForm = (CreditControlledDocumentsApprovalForm)GetFormToBash())
			{
				testForm.Show();
				Application.DoEvents();
				if (testForm.TopSingleRequestPanel_ForTestOnly.Visible)
				{
					Fail("Open job button is located in TopSingleRequestPanel, which is now visible for multiple requests. Please update this unit test accordingly.");
				}
				else
				{
					Assert("Open job button is located in TopSingleRequestPanel, which is not visible for multiple requests", true);
				}
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var menutItem = Factory.New<StmMenuItem>();
			menutItem.SU_MenuPath = "menu/path";
			menutItem.SU_MenuName = "name";

			var formBizo1 = Factory.New<CreditControlledDocumentsApproval>();
			using (formBizo1.SuspendSettingHasChanges())
			{
				var parentBusinessObject1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				formBizo1.Initialize(parentBusinessObject1, menutItem.PK);
				formBizo1.XP_ReasonDescription = "Desc";
			}

			var formBizo2 = Factory.New<CreditControlledDocumentsApproval>();
			using (formBizo2.SuspendSettingHasChanges())
			{
				var parentBusinessObject2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				formBizo2.Initialize(parentBusinessObject2, menutItem.PK);
				formBizo2.XP_ReasonDescription = "Desc";
			}
			return new CreditControlledDocumentsApprovalForm(new CreditControlledDocumentsApprovalBulk(Factory, formBizo1, formBizo2), ApprovalFromMode);
		}

		protected override void SetUp()
		{
			base.SetUp();

			ApprovalFromMode = CreditControlledDocumentsApprovalFormModes.Approve;
		}

		#endregion
	}
}
