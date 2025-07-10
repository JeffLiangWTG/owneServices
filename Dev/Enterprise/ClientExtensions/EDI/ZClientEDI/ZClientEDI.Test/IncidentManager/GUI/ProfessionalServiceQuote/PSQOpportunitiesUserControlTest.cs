using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.MasterFiles.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	public class PSQOpportunitiesUserControlTest : TestCaseWithFactory
	{
		#region FormForTest
		class FormForTest : ZForm
		{
			public FormForTest(ProfessionalServicesQuote psq) : base(psq)
			{
			}

			protected override void InitialiseForm()
			{
				base.InitialiseForm();
				Controls.Add(UserControl);
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					if (userControl != null)
					{
						userControl.Dispose();
					}
				}

				base.Dispose(disposing);
			}

			PSQOpportunitiesUserControlForTest userControl;
			public PSQOpportunitiesUserControlForTest UserControl
			{
				get
				{
					return userControl ?? (userControl = new PSQOpportunitiesUserControlForTest());
				}
			}
		}

		class PSQOpportunitiesUserControlForTest : PSQRelatedOpportunitiesUserControl
		{
			public new ZGrid OpportunitiesGrid
			{
				get
				{
					return base.OpportunitiesGrid;
				}
			}

			public new ZButton DetatchButton
			{
				get
				{
					return base.DetatchButton;
				}
			}

			public new ZButton EditButton
			{
				get
				{
					return base.EditButton;
				}
			}

			public new ZButton NewButton
			{
				get
				{
					return base.NewButton;
				}
			}

			public new ZButton AttachButton
			{
				get
				{
					return base.AttachButton;
				}
			}
		}

		#endregion
		public void TestPSQRelatedOpportunitiesLoaded()
		{
			EDIOrgOpportunity opportunity = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			EDIOrgOpportunity opportunity2 = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			ProfessionalServicesQuote psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			psq.CreateNewPSQOpportunityPivot(opportunity);
			psq.CreateNewPSQOpportunityPivot(opportunity2);
			Factory.Save();
			using (FormForTest form = new FormForTest(psq))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals(2, form.UserControl.OpportunitiesGrid.List.Count);
			}
		}

		public void TestNewOpportunity()
		{
			EDIOrgOpportunity opportunity = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			ProfessionalServicesQuote psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			psq.CreateNewPSQOpportunityPivot(opportunity);
			Factory.Save();
			using (FormForTest form = new FormForTest(psq))
			{
				form.Show();
				Application.DoEvents();
				form.UserControl.NewButton.PerformClick();
				AssertEquals(typeof(EDIOpportunityManagementForm), form.UserControl.LastController.LastShownForm.GetType());
				form.UserControl.LastController.LastShownForm.Dispose();
			}
		}

		[ExpectNoExceptions]
		public void TestClosePSQFormBeforeSavingNewOpp()
		{
			ProfessionalServicesQuote psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			OrgStaffAssignments assignment = client.StaffAssignments.AddNew();
			GlbStaff salesRep = Factory.NewWithValidTestData<GlbStaff>();
			salesRep.GS_Code = "GAN";
			salesRep.GS_FullName = "Gandalf the Grey";
			assignment.O8_GS_NKPersonResponsible = salesRep.GS_Code;
			assignment.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			psq.IM_OH_Client = client.PK;
			psq.IM_OC_Contact = contact.PK;
			Factory.Save();
			using (FormForTest form = new FormForTest(psq))
			{
				form.Show();
				Application.DoEvents();
				form.UserControl.NewButton.PerformClick();
				ZForm newOppForm = (ZForm)form.UserControl.LastController.LastShownForm;
				form.Dispose();
				newOppForm.BusinessEntity.Factory.Save();
				newOppForm.Dispose();
			}
		}

		public void TestDeleteOpportunity()
		{
			ProfessionalServicesQuote psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			using (FormForTest form = new FormForTest(psq))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("Should not be able to remove a related opportunity from the grid, using the keyboard", RemoveAction.NoRemovePossible, form.UserControl.OpportunitiesGrid.RemoveAction);
			}
		}

		public void TestEditOpportunityOnGrid()
		{
			ProfessionalServicesQuote psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			using (FormForTest form = new FormForTest(psq))
			{
				form.Show();
				Application.DoEvents();
				Assert("Related Opportunities Grid should be read-only", form.UserControl.OpportunitiesGrid.ReadOnly);
			}
		}

		public void TestAttachOpportunity()
		{
			ProfessionalServicesQuote psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			using (FormForTest form = new FormForTest(psq))
			{
				form.Show();
				Application.DoEvents();
				form.UserControl.AttachButton.PerformClick();
				AssertEquals("Opportunity Management", form.UserControl.LastAttacher.LastShownAttachPopupForTesting.Text);
				form.UserControl.LastAttacher.LastShownAttachPopupForTesting.Dispose();
			}
		}

		public void TestEditOpportunity()
		{
			EDIOrgOpportunity opportunity = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			ProfessionalServicesQuote psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			psq.CreateNewPSQOpportunityPivot(opportunity);
			Factory.Save();
			using (FormForTest form = new FormForTest(psq))
			{
				form.Show();
				Application.DoEvents();
				int opportunityRow = form.UserControl.OpportunitiesGrid.ListManager.List[0] is EDIOrgOpportunity ? 0 : 1;
				form.UserControl.OpportunitiesGrid.ListManager.Position = opportunityRow;
				form.UserControl.OpportunitiesGrid.Select(opportunityRow);
				form.UserControl.EditButton.PerformClick();
				AssertEquals(typeof(EDIOpportunityManagementForm), form.UserControl.LastController.LastShownForm.GetType());
				form.UserControl.LastController.LastShownForm.Dispose();
				MethodInfo methodInfo = typeof(ZGrid).GetMethod("OnDoubleClick", BindingFlags.NonPublic | BindingFlags.Instance);
				methodInfo.Invoke(form.UserControl.OpportunitiesGrid, new object[] { EventArgs.Empty });
				AssertEquals(typeof(EDIOpportunityManagementForm), form.UserControl.LastController.LastShownForm.GetType());
				form.UserControl.LastController.LastShownForm.Dispose();
			}
		}

		public void TestDetatchOpportunity()
		{
			EDIOrgOpportunity opportunity = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			ProfessionalServicesQuote psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			psq.CreateNewPSQOpportunityPivot(opportunity);
			Factory.Save();
			using (FormForTest form = new FormForTest(psq))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals(1, form.UserControl.OpportunitiesGrid.List.Count);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.UserControl.OpportunitiesGrid.Select(0);
				form.UserControl.DetatchButton.PerformClick();
				AssertEquals(0, form.UserControl.OpportunitiesGrid.List.Count);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.UserControl.DetatchButton.PerformClick();
				AssertEquals(0, form.UserControl.OpportunitiesGrid.List.Count);
			}
		}
	}
}
