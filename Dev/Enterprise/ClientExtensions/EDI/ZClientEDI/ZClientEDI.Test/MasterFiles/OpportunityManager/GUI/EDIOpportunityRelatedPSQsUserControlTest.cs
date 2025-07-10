using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	public class EDIOpportunityRelatedPSQsUserControlTest : TestCaseWithFactory
	{
		#region FormForTest
		class FormForTest : ZForm
		{
			public FormForTest(EDIOrgOpportunity opportunity) : base(opportunity)
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

			EDIOpportunityRelatedPSQsUserControlForTest userControl;
			public EDIOpportunityRelatedPSQsUserControlForTest UserControl
			{
				get
				{
					return userControl ?? (userControl = new EDIOpportunityRelatedPSQsUserControlForTest());
				}
			}
		}

		class EDIOpportunityRelatedPSQsUserControlForTest : EDIOpportunityRelatedPSQsUserControl
		{
			public new ZGrid PSQsGrid
			{
				get
				{
					return base.PSQsGrid;
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
		public void TestRelatedPSQsLoaded()
		{
			ProfessionalServicesQuote psq1 = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			ProfessionalServicesQuote psq2 = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			EDIOrgOpportunity opportunity = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			opportunity.CreateNewPSQOpportunityPivot(psq1);
			opportunity.CreateNewPSQOpportunityPivot(psq2);
			Factory.Save();
			using (FormForTest form = new FormForTest(opportunity))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals(2, form.UserControl.PSQsGrid.List.Count);
			}
		}

		public void TestNewPSQ()
		{
			ProfessionalServicesQuote psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			EDIOrgOpportunity opportunity = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			opportunity.CreateNewPSQOpportunityPivot(psq);
			Factory.Save();
			using (FormForTest form = new FormForTest(opportunity))
			{
				form.Show();
				Application.DoEvents();
				form.UserControl.NewButton.PerformClick();
				AssertEquals(typeof(ProfessionalServicesQuoteForm), form.UserControl.LastController.LastShownForm.GetType());
				form.UserControl.LastController.LastShownForm.Dispose();
			}
		}

		[ExpectNoExceptions]
		public void TestCloseOppFormBeforeSavingNewPSQ()
		{
			EDIOrgOpportunity opportunity = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			Factory.Save();
			using (FormForTest form = new FormForTest(opportunity))
			{
				form.Show();
				Application.DoEvents();
				form.UserControl.NewButton.PerformClick();
				ZForm newPSQForm = (ZForm)form.UserControl.LastController.LastShownForm;
				form.Dispose();
				newPSQForm.BusinessEntity.Factory.Save();
				newPSQForm.Dispose();
			}
		}

		public void TestDeletePSQ()
		{
			EDIOrgOpportunity opportunity = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			using (FormForTest form = new FormForTest(opportunity))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("Should not be able to remove a related PSQ from the grid, using the keyboard", RemoveAction.NoRemovePossible, form.UserControl.PSQsGrid.RemoveAction);
			}
		}

		public void TestEditPSQFromGrid()
		{
			EDIOrgOpportunity opportunity = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			using (FormForTest form = new FormForTest(opportunity))
			{
				form.Show();
				Application.DoEvents();
				Assert("Related PSQ Grid should be read-only", form.UserControl.PSQsGrid.ReadOnly);
			}
		}

		public void TestAttachPSQ()
		{
			EDIOrgOpportunity opportunity = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			using (FormForTest form = new FormForTest(opportunity))
			{
				form.Show();
				Application.DoEvents();
				form.UserControl.AttachButton.PerformClick();
				AssertEquals("Professional Services Quotes", form.UserControl.LastAttacher.LastShownAttachPopupForTesting.Text);
				form.UserControl.LastAttacher.LastShownAttachPopupForTesting.Dispose();
			}
		}

		public void TestEditPSQ()
		{
			EDIOrgOpportunity opportunity = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			ProfessionalServicesQuote psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			psq.CreateNewPSQOpportunityPivot(opportunity);
			Factory.Save();
			using (FormForTest form = new FormForTest(opportunity))
			{
				form.Show();
				Application.DoEvents();
				int opportunityRow = form.UserControl.PSQsGrid.ListManager.List[0] is ProfessionalServicesQuote ? 0 : 1;
				form.UserControl.PSQsGrid.ListManager.Position = opportunityRow;
				form.UserControl.PSQsGrid.Select(opportunityRow);
				form.UserControl.EditButton.PerformClick();
				AssertEquals(typeof(ProfessionalServicesQuoteForm), form.UserControl.LastController.LastShownForm.GetType());
				form.UserControl.LastController.LastShownForm.Dispose();
				MethodInfo methodInfo = typeof(ZGrid).GetMethod("OnDoubleClick", BindingFlags.NonPublic | BindingFlags.Instance);
				methodInfo.Invoke(form.UserControl.PSQsGrid, new object[] { EventArgs.Empty });
				AssertEquals(typeof(ProfessionalServicesQuoteForm), form.UserControl.LastController.LastShownForm.GetType());
				form.UserControl.LastController.LastShownForm.Dispose();
			}
		}

		public void TestDetatchPSQ()
		{
			EDIOrgOpportunity opportunity = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			ProfessionalServicesQuote psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			psq.CreateNewPSQOpportunityPivot(opportunity);
			Factory.Save();
			using (FormForTest form = new FormForTest(opportunity))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals(1, form.UserControl.PSQsGrid.List.Count);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.UserControl.PSQsGrid.Select(0);
				form.UserControl.DetatchButton.PerformClick();
				AssertEquals(0, form.UserControl.PSQsGrid.List.Count);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.UserControl.DetatchButton.PerformClick();
				AssertEquals(0, form.UserControl.PSQsGrid.List.Count);
			}
		}
	}
}
