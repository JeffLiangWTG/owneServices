using System;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.GUI;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.GUI.Testing
{
	public class OpportunityRelatedProjectsUserControlTest : TestCaseWithFactory
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

			public ControlForTest UserControl
			{
				get
				{
					return userControl ?? (userControl = new ControlForTest());
				}
			}

			ControlForTest userControl;
		}

		class ControlForTest : OpportunityRelatedProjectsUserControl
		{
			public new ZGrid ProjectGrid
			{
				get
				{
					return base.ProjectGrid;
				}
			}
		}

		#endregion

		public void TestOpenProject()
		{
			EDIOrgOpportunity opportunity = Factory.NewWithValidTestData<EDIOrgOpportunity>();
			EDIProject project = Factory.NewWithValidTestData<EDIProject>();
			project.WKP_P8_Opportunity = opportunity.PK;
			Factory.Save();
			using (FormForTest form = new FormForTest(opportunity))
			{
				form.Show();
				AssertEquals("Should have one related project", 1, form.UserControl.ProjectGrid.ListManager.Count);
				form.UserControl.ProjectGrid.Select(0);
				MethodInfo methodInfo = typeof(ZGrid).GetMethod("OnDoubleClick", BindingFlags.NonPublic | BindingFlags.Instance);
				methodInfo.Invoke(form.UserControl.ProjectGrid, new object[] { EventArgs.Empty });
				AssertEquals(typeof(EDIProjectForm), form.UserControl.LastController.LastShownForm.GetType());
				form.UserControl.LastController.LastShownForm.Dispose();
			}

			opportunity.P8_OH = Factory.NewWithValidTestData<EDIOrgHeader>().PK;
			using (FormForTest form = new FormForTest(opportunity))
			{
				form.Show();
				Assert("Opportunity has changes", opportunity.HasChanges);
				AssertEquals("Should have one related project", 1, form.UserControl.ProjectGrid.ListManager.Count);
				form.UserControl.ProjectGrid.Select(0);
				MethodInfo methodInfo = typeof(ZGrid).GetMethod("OnDoubleClick", BindingFlags.NonPublic | BindingFlags.Instance);
				methodInfo.Invoke(form.UserControl.ProjectGrid, new object[] { EventArgs.Empty });
				AssertEquals(typeof(EDIProjectForm), form.UserControl.LastController.LastShownForm.GetType());
				form.UserControl.LastController.LastShownForm.Dispose();
				AssertNullOrEmpty("Should allow opening related items even when there are unsaved changes", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[ExpectNoExceptions]
		public void TestOpenProject_NoExceptionIfOpenProjectTwice()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew() as EDIOrgOpportunity;
			var project = Factory.NewWithValidTestData<EDIProject>();
			project.ChangeClientOrganisation(org);
			project.WKP_P8_Opportunity = opportunity.PK;
			project.WorkflowItems.AddNew();
			Factory.Save();
			using (FormForTest form = new FormForTest(opportunity))
			{
				form.Show();
				form.UserControl.ProjectGrid.Select(0);
				MethodInfo methodInfo = typeof(ZGrid).GetMethod("OnDoubleClick", BindingFlags.NonPublic | BindingFlags.Instance);
				methodInfo.Invoke(form.UserControl.ProjectGrid, new object[] { EventArgs.Empty });
				var projectForm = (ZForm)form.UserControl.LastController.LastShownForm;
				AssertEquals(typeof(EDIProjectForm), projectForm.GetType());
				projectForm.Close();
				projectForm.Dispose();
				methodInfo.Invoke(form.UserControl.ProjectGrid, new object[] { EventArgs.Empty });
				projectForm = (ZForm)form.UserControl.LastController.LastShownForm;
				((EDIProject)projectForm.BusinessEntity).WorkflowItems.AddNew();
				((EDIProject)projectForm.BusinessEntity).WorkflowItems[0].P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				((EDIProject)projectForm.BusinessEntity).Factory.Save();
				projectForm.Close();
				projectForm.Dispose();
			}
		}
	}
}
