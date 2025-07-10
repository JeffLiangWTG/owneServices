using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(CreditControlledDocumentsApprovalForm))]
	internal class CreditControlledDocumentsApprovalForBasherTest : ZFormBasherTest
	{
		public virtual void TestFormParameters()
		{
			using (CreditControlledDocumentsApprovalForm testForm = (CreditControlledDocumentsApprovalForm)GetFormToBash())
			{
				testForm.Show();
				Application.DoEvents();
				AssertEquals(true, testForm.PostingButtonsUserControl_ForTestOnly.SaveAndCloseButton.Visible);
				AssertEquals(ODisplayMode.Edit, testForm.DisplayMode);
				AssertEquals("S&ave && Close", testForm.PostingButtonsUserControl_ForTestOnly.SaveAndCloseButton.Text);
				AssertEquals("TopGridPanel_ForTestOnly.Visible", false, testForm.TopGridPanel_ForTestOnly.Visible);
				AssertEquals("TopSingleRequestPanel_ForTestOnly.Visible", true, testForm.TopSingleRequestPanel_ForTestOnly.Visible);
				AssertEquals("ReasonDescriptionTextBox_ForTestOnly.ReadOnly", false, testForm.ReasonDescriptionTextBox_ForTestOnly.ReadOnly);
				AssertEquals("FormVerb", "Request", testForm.FormVerb);

				CreditControlledDocumentsApprovalBulk formBizo = (CreditControlledDocumentsApprovalBulk)testForm.BusinessEntity;
				formBizo.RunPreSaveValidation();
				AssertNoErrors("Precondition: ", formBizo);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.PostingButtonsUserControl_ForTestOnly.SaveAndCloseButton.PerformClick();
				AssertNull("User shouldn't be asked about changes before closing a form.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Factory must be saved.", true, formBizo.CreditControlledDocumentsApprovalsAllowedToProcess[0].IsInDatabase);
			}
		}

		public virtual void TestDontAskToSaveDataOnCanceling()
		{
			using (CreditControlledDocumentsApprovalForm testForm = (CreditControlledDocumentsApprovalForm)GetFormToBash())
			{
				testForm.Show();
				CreditControlledDocumentsApprovalBulk formBizo = (CreditControlledDocumentsApprovalBulk)testForm.BusinessEntity;
				formBizo.CreditControlledDocumentsApprovalsAllowedToProcess[0].XP_ReasonDescription = "other desc";
				formBizo.RunPreSaveValidation();
				AssertNoErrors("Precondition: ", formBizo);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.CancelButton.PerformClick();
				AssertNull("User shouldn't be asked about changes before closing a form.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestFormVerb()
		{
			ApprovalFromMode = CreditControlledDocumentsApprovalFormModes.Approve;
			using (CreditControlledDocumentsApprovalForm testForm = (CreditControlledDocumentsApprovalForm)GetFormToBash())
			{
				AssertEquals("FormVerb", "Approve", testForm.FormVerb);
			}

			ApprovalFromMode = CreditControlledDocumentsApprovalFormModes.Reject;
			using (CreditControlledDocumentsApprovalForm testForm = (CreditControlledDocumentsApprovalForm)GetFormToBash())
			{
				AssertEquals("FormVerb", "Reject", testForm.FormVerb);
			}

			ApprovalFromMode = CreditControlledDocumentsApprovalFormModes.Cancel;
			using (CreditControlledDocumentsApprovalForm testForm = (CreditControlledDocumentsApprovalForm)GetFormToBash())
			{
				AssertEquals("FormVerb", "Cancel", testForm.FormVerb);
			}

			ApprovalFromMode = CreditControlledDocumentsApprovalFormModes.SetDescription;
			using (CreditControlledDocumentsApprovalForm testForm = (CreditControlledDocumentsApprovalForm)GetFormToBash())
			{
				AssertEquals("FormVerb", "Request", testForm.FormVerb);
			}
		}

		public void TestReasonDescriptionTextBoxReadOnly()
		{
			foreach (var mode in Enum.GetValues(typeof(CreditControlledDocumentsApprovalFormModes)))
			{
				ApprovalFromMode = (CreditControlledDocumentsApprovalFormModes)mode;
				using (CreditControlledDocumentsApprovalForm testForm = (CreditControlledDocumentsApprovalForm)GetFormToBash())
				{
					testForm.Show();
					Application.DoEvents();
					var reasonDescriptionTextBox = (ZTextBox)testForm.DetailTabPage_ForTestOnly.Controls.Find("ReasonDescriptionTextBox", true).First();
					AssertNotNull(reasonDescriptionTextBox);
					if (ApprovalFromMode == CreditControlledDocumentsApprovalFormModes.SetDescription)
					{
						AssertEquals("ReasonDescriptionTextBox shouldn't be read only when we set description.", false, reasonDescriptionTextBox.ReadOnly);
					}
					else
					{
						AssertEquals("ReasonDescriptionTextBox should be read only.", true, reasonDescriptionTextBox.ReadOnly);
					}
				}
			}
		}

		public void TestApproveForAllDocumentsButton_Click_WhenApprovalsBecomeNull()
		{
			using (CreditControlledDocumentsApprovalForm testForm = (CreditControlledDocumentsApprovalForm)GetFormToBash())
			{
				testForm.BindingSource_ForTestOnly.DataSource = null;
				AssertNoExceptionThrown(() => testForm.OnApplyButtonClick_ForTestOnly(null, null));
			}
		}

		public void TestApproveForAllDocumentsButtonVisible()
		{
			Env.Security.OnCreditHoldControllerApproveAllDocuments.IsAllowed = true;
			foreach (var mode in Enum.GetValues(typeof(CreditControlledDocumentsApprovalFormModes)))
			{
				ApprovalFromMode = (CreditControlledDocumentsApprovalFormModes)mode;
				using (CreditControlledDocumentsApprovalForm testForm = (CreditControlledDocumentsApprovalForm)GetFormToBash())
				{
					testForm.Show();
					if (ApprovalFromMode == CreditControlledDocumentsApprovalFormModes.SetDescription)
					{
						AssertEquals("Approve All Documents button shouldn't be shown when we set description.", false, testForm.PostingButtonsUserControl_ForTestOnly.SaveButton.Visible);
					}
					else if (ApprovalFromMode == CreditControlledDocumentsApprovalFormModes.Approve)
					{
						AssertEquals("Approve All Documents button should be shown when we do approve action.", true, testForm.PostingButtonsUserControl_ForTestOnly.SaveButton.Visible);
						Assert(testForm.PostingButtonsUserControl_ForTestOnly.SaveButton.Text.Equals("Approve All Documents"));
					}
				}
			}
			Env.Security.OnCreditHoldControllerApproveAllDocuments.IsAllowed = false;
			foreach (var mode in Enum.GetValues(typeof(CreditControlledDocumentsApprovalFormModes)))
			{
				ApprovalFromMode = (CreditControlledDocumentsApprovalFormModes)mode;
				using (CreditControlledDocumentsApprovalForm testForm = (CreditControlledDocumentsApprovalForm)GetFormToBash())
				{
					testForm.Show();
					AssertEquals("Approve All Documents button shouldn't be shown when the security right is not granted", false, testForm.PostingButtonsUserControl_ForTestOnly.SaveButton.Visible);
				}
			}
		}

		public void TestEDocPlugin()
		{
			using (var testForm = (CreditControlledDocumentsApprovalForm)GetFormToBash())
			{
				testForm.Show();
				testForm.EDocsTabPage_ForTestOnly.Show();
				Application.DoEvents();
				AssertNotNull("eDocsPlugin should be initialized", testForm.EDocPlugIn_ForTestOnly);
				AssertNotNull("eDocsUserControl should be initialized", testForm.EdocUserControl_ForTestOnly);

				var approval = testForm.Approvals_ForTestOnly.CreditControlledDocumentsApprovalsAllowedToProcess[0];
				approval.DocManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 1, 2, 3 }), "TesteDocFile1", "MSC");
				AssertEquals("one eDoc attached to approval", 1, approval.DocManagerInfo.AllEDocs.Count);
				testForm.PostingButtonsUserControl_ForTestOnly.SaveAndCloseButton.PerformClick();

				var reloadedApproval = new BusinessObjectFactory().Load<CreditControlledDocumentsApproval>(approval.PK);
				AssertEquals("eDoc is saved with the approval factory save", 1, reloadedApproval.DocManagerInfo.AllEDocs.Count);
				var eDoc = reloadedApproval.DocManagerInfo.AllEDocs[0];
				AssertEquals("FileName", "TesteDocFile1", eDoc.FileName);
				AssertEquals("ImageData", new byte[] { 1, 2, 3 }, eDoc.ImageData);
				AssertEquals("DocType", "MSC", eDoc.DocType);
			}
		}

		public virtual void TestControlVisibility()
		{
			ApprovalFromMode = CreditControlledDocumentsApprovalFormModes.Approve;
			AssertControlVisibility(true, true, true, true, true, true);

			ApprovalFromMode = CreditControlledDocumentsApprovalFormModes.Reject;
			AssertControlVisibility(true, true, true, true, true, true);

			ApprovalFromMode = CreditControlledDocumentsApprovalFormModes.Cancel;
			AssertControlVisibility(true, true, true, true, true, true);

			ApprovalFromMode = CreditControlledDocumentsApprovalFormModes.View;
			AssertControlVisibility(true, true, true, true, true, true);

			ApprovalFromMode = CreditControlledDocumentsApprovalFormModes.SetDescription;
			AssertControlVisibility(false, true, true, true, true, false);
		}

		public virtual void TestOpenJob()
		{
			ApprovalFromMode = CreditControlledDocumentsApprovalFormModes.Approve;
			var testObjectCreator = new TestObjectCreator(Factory);
			var shipment = testObjectCreator.CreateShipment("S0001");
			var job = testObjectCreator.CreateJob(shipment);
			var approval = Factory.NewWithValidTestData<CreditControlledDocumentsApproval>();
			approval.Initialize(shipment);
			Factory.Save();

			ZForm lastForm = null;

			try
			{
				using (var form = new CreditControlledDocumentsApprovalForm(new CreditControlledDocumentsApprovalBulk(Factory, new[] { approval }), CreditControlledDocumentsApprovalFormModes.View))
				{
					form.Show();
					Application.DoEvents();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					form.OpenJobButton_ForTestOnly.PerformClick();

					lastForm = form.LastShownJobForm_ForTestOnly;
					AssertNotNull("Operational Job should be opened", lastForm);
					Assert("Should be the correct type", ObjectFactory.GetType<Freight.Integration.Forwarding.IForwardingShipmentForm>().IsAssignableFrom(lastForm.GetType()));
					AssertEquals("Display Mode", ODisplayMode.Browse, lastForm.DisplayMode);
				}
			}
			finally
			{
				if (lastForm != null)
				{
					lastForm.Dispose();
				}
			}
		}

		public void TestOrganisationsInBreachGridReadability()
		{
			foreach (var mode in Enum.GetValues(typeof(CreditControlledDocumentsApprovalFormModes)))
			{
				ApprovalFromMode = (CreditControlledDocumentsApprovalFormModes)mode;
				using (CreditControlledDocumentsApprovalForm testForm = (CreditControlledDocumentsApprovalForm)GetFormToBash())
				{
					testForm.Show();
					Application.DoEvents();
					AssertNotNull(testForm.OrgInBreachGrid_ForTestOnly);
					AssertEquals("Organisations In Breach Grid should be read only.", true, testForm.OrgInBreachGrid_ForTestOnly.ReadOnly);
					Assert(!testForm.OrgInBreachGrid_ForTestOnly.GetColumnStyle("OrgName").IsUnavailable);
					Assert(!testForm.OrgInBreachGrid_ForTestOnly.GetColumnStyle("OrgCode").IsUnavailable);
					var shouldBeUnavailable = ApprovalFromMode == CreditControlledDocumentsApprovalFormModes.SetDescription;
					AssertEquals(shouldBeUnavailable, testForm.OrgInBreachGrid_ForTestOnly.GetColumnStyle("OrgCreditLimit").IsUnavailable);
					AssertEquals(shouldBeUnavailable, testForm.OrgInBreachGrid_ForTestOnly.GetColumnStyle("IsUsingSettlementGroupCreditLimit").IsUnavailable);
					AssertEquals(shouldBeUnavailable, testForm.OrgInBreachGrid_ForTestOnly.GetColumnStyle("IsCreditOnHold").IsUnavailable);
					AssertEquals(shouldBeUnavailable, testForm.OrgInBreachGrid_ForTestOnly.GetColumnStyle("OverCreditLimit").IsUnavailable);
					AssertEquals(shouldBeUnavailable, testForm.OrgInBreachGrid_ForTestOnly.GetColumnStyle("StandardOverdueAmount").IsUnavailable);
					AssertEquals(shouldBeUnavailable, testForm.OrgInBreachGrid_ForTestOnly.GetColumnStyle("DisbursementOverdueAmount").IsUnavailable);
					AssertEquals(shouldBeUnavailable, testForm.OrgInBreachGrid_ForTestOnly.GetColumnStyle("StandardWIPsBilledToThisJob").IsUnavailable);
					AssertEquals(shouldBeUnavailable, testForm.OrgInBreachGrid_ForTestOnly.GetColumnStyle("DisbursementWIPsBilledToThisJob").IsUnavailable);
					Assert(!testForm.OrgInBreachGrid_ForTestOnly.GetColumnStyle("BreachReasons").IsUnavailable);
				}
			}
		}

		public void TestOrganisationInBreachGridAndOrganisationsDropDownList()
		{
			#region Data Setup

			var objectCreator = new TestObjectCreator(Factory);
			var localClient = objectCreator.CreateOrgHeader(TestObjectCreator.GetRandomString(3), false, true);
			localClient.CompanyData.OB_ARCreditLimit = 1m;
			localClient.CompanyData.OB_AROnCreditHold = true;

			var consignee = objectCreator.CreateOrgHeader(TestObjectCreator.GetRandomString(3), false, true);
			consignee.CompanyData.OB_ARCreditLimit = 1m;
			consignee.CompanyData.OB_AROnCreditHold = true;

			var consignor = objectCreator.CreateOrgHeader(TestObjectCreator.GetRandomString(3), false, true);

			var shipment = objectCreator.CreateShipment("S00001");
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			var shipmentJob = objectCreator.CreateJob(shipment, localClient, 0m, objectCreator.Agent, 0m);
			Factory.Save();

			localClient.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			consignee.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			consignor.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();

			#endregion

			var approval = Factory.NewWithValidTestData<CreditControlledDocumentsApproval>();
			approval.Initialize(shipment);
			Factory.Save();

			using (var form = new CreditControlledDocumentsApprovalForm(new CreditControlledDocumentsApprovalBulk(Factory, new[] { approval }), CreditControlledDocumentsApprovalFormModes.Approve))
			{
				form.Show();
				Application.DoEvents();
				form.OrgInBreachGrid_ForTestOnly.SelectAllElements();

				AssertEquals(2, form.OrgInBreachGrid_ForTestOnly.SelectedRowCount);
				var selectedOrgPKs = form.OrgInBreachGrid_ForTestOnly.SelectedElements.Cast<OrganisationInBreach>().Select(x => x.OrganisationPK);
				AssertContainsExactElementsInAnyOrder("Should contain local client & consignee as they are on credit hold", new[] { localClient.PK, consignee.PK }, selectedOrgPKs);
				Assert("Should not contain consignor as it is not on credit hold", !selectedOrgPKs.Contains(consignor.PK));

				form.MainTabControl_ForTestOnly.SelectTab("creditTabPage");
				Application.DoEvents();
				var dropDownList = form.OrgZGuidDropEdit_ForTestOnly.List;
				AssertEquals(2, dropDownList.Count);
				AssertContainsExactElementsInAnyOrder("Should contain local client & consignee as they are on credit hold", new[] { localClient, consignee }, dropDownList);
				Assert("Should not contain consignor as it is not on credit hold", !dropDownList.Contains(consignor));
			}
		}

		public virtual void TestCreditStatusTabPageIsNotReadOnlyInViewMode()
		{
			ApprovalFromMode = CreditControlledDocumentsApprovalFormModes.View;
			using (var testForm = (CreditControlledDocumentsApprovalForm)GetFormToBash())
			{
				testForm.Show();
				Application.DoEvents();
				Assert("Credit Status tab page should not be read only", !testForm.CreditTabPage_ForTestOnly.ShouldBeReadOnlyInViewMode);
			}
		}

		protected void AssertControlVisibility(bool isCreditTabPageVisible, bool isTopSingleRequestPanelVisisble, bool isDepartmentVisible, bool isIncoTermVisible, bool isApprovalLevelVisible, bool isOpenJobButtonVisible)
		{
			using (var testForm = (CreditControlledDocumentsApprovalForm)GetFormToBash())
			{
				testForm.Show();
				Application.DoEvents();
				Assert("Details tab visibility", testForm.DetailTabPage_ForTestOnly.TabVisible);
				AssertEquals("Credit Status tab visiblity", isCreditTabPageVisible, testForm.CreditTabPage_ForTestOnly.TabVisible);
				AssertEquals("TopSingleRequestPanel_ForTestOnly Visibility", isTopSingleRequestPanelVisisble, testForm.TopSingleRequestPanel_ForTestOnly.Visible);
				AssertEquals("Department text box visiblity", isDepartmentVisible, testForm.DepartmentTextBox_ForTestOnly.Visible);
				AssertEquals("Inco term panel visiblity", isIncoTermVisible, testForm.IncoTermPanel_ForTestOnly.Visible);
				AssertEquals("Approval level text box visiblity", isApprovalLevelVisible, testForm.ApprovalLevelTextBox_ForTestOnly.Visible);
				AssertEquals("Open Job button visiblity", isOpenJobButtonVisible, testForm.OpenJobButton_ForTestOnly.Visible);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var menutItem = Factory.New<StmMenuItem>();
			menutItem.SU_MenuPath = "menu/path";
			menutItem.SU_MenuName = "name";

			var formBizo = Factory.New<CreditControlledDocumentsApproval>();
			using (formBizo.SuspendSettingHasChanges())
			{
				var parentBusinessObject = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				formBizo.Initialize(parentBusinessObject, menutItem.PK);
				formBizo.XP_ReasonDescription = "Desc";
			}
			return new CreditControlledDocumentsApprovalForm(new CreditControlledDocumentsApprovalBulk(Factory, formBizo), ApprovalFromMode);
		}

		protected override void SetUp()
		{
			base.SetUp();

			ApprovalFromMode = CreditControlledDocumentsApprovalFormModes.SetDescription;
		}

		protected CreditControlledDocumentsApprovalFormModes ApprovalFromMode;

		#endregion
	}
}
