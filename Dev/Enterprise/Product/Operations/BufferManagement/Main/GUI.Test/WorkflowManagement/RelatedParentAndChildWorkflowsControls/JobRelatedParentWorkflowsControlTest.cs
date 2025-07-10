using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.GUI.Test
{
	class JobRelatedParentWorkflowsControlTest : BMSTestCaseWithFactory
	{
		public void TestDoubleClickingShouldOpenTheWorkflow()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "ORG");

			var targetJob = Factory.NewWithValidTestData<OrgHeader>();
			var targetJobHeader = BMSTestHelper.CreateJobHeader(targetJob, description: "Target Job Header");
			var targetWorkflow1 = BMSTestHelper.CreateWorkflow(targetJobHeader, "Target Workflow 1");

			var parentjob = Factory.NewWithValidTestData<OrgHeader>();
			var parentJobHeader = BMSTestHelper.CreateJobHeader(parentjob, description: "Parent Job Header");
			var parentWorkflow1 = BMSTestHelper.CreateWorkflow(parentJobHeader, "Parent Workflow 1");

			BMSTestHelper.CreateParentChildLink(parentWorkflow1, targetWorkflow1);

			Factory.Save();

			using (var form = new ZForm(targetJob))
			using (var control = new TestJobRelatedParentWorkflowsControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(targetJob, string.Empty);
				form.Show();
				Application.DoEvents();

				var grid = control.Controls.Find("ParentWorkflowGrid", true).SingleOrDefault() as ZGrid;
				AssertNotNull("The control should have a ZDisplayGrid on it", grid);

				grid.Select(0);
				grid.PerformDoubleClickForTest();

				using (var orgForm = Application.OpenForms.OfType<ZOrganisationsForm>().SingleOrDefault())
				{
					AssertNotNull("Should open organisation form", orgForm);
					AssertEquals(parentjob.PK, ((OrgHeader)orgForm.BusinessEntity).PK);
				}
			}
		}

		public void TestRemoveParentWorkflowButton()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "ORG");

			var targetJob = Factory.NewWithValidTestData<OrgHeader>();
			var targetJobHeader = BMSTestHelper.CreateJobHeader(targetJob, description: "Target Job Header");
			var targetWorkflow1 = BMSTestHelper.CreateWorkflow(targetJobHeader, "Target Workflow 1");

			var parentjob = Factory.NewWithValidTestData<OrgHeader>();
			var parentJobHeader = BMSTestHelper.CreateJobHeader(parentjob, description: "Parent Job Header");
			var parentWorkflow1 = BMSTestHelper.CreateWorkflow(parentJobHeader, "Parent Workflow 1");

			var link = BMSTestHelper.CreateParentChildLink(parentWorkflow1, targetWorkflow1);

			Factory.Save();

			using (var form = new ZForm(targetJob))
			using (var control = new TestJobRelatedParentWorkflowsControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(targetJob, string.Empty);
				form.Show();
				Application.DoEvents();

				var grid = control.Controls.Find("ParentWorkflowGrid", true).SingleOrDefault() as ZGrid;
				AssertNotNull("The control should have a ZDisplayGrid on it", grid);

				grid.Select(0);

				var button = control.Controls.Find("RemoveButton", true).SingleOrDefault() as ZButton;
				AssertNotNull(button);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				button.PerformClick();

				Assert("The link should be deleted", link.IsDeleted);
				Assert("Should allow saving the form", form.BusinessEntityForHasChanges.HasChanges);
			}
		}

		public void TestAddParentWorkflowButton_ShouldCreateLinksForJobHeader()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "ORG");

			var targetJob = Factory.NewWithValidTestData<OrgHeader>();
			var targetJobHeader = BMSTestHelper.CreateJobHeader(targetJob, description: "Target Job Header");
			BMSTestHelper.CreateWorkflow(targetJobHeader, "Target Workflow 1");

			var parentjob = Factory.NewWithValidTestData<OrgHeader>();
			var parentJobHeader = BMSTestHelper.CreateJobHeader(parentjob, description: "Parent Job Header");
			var parentWorkflow1 = BMSTestHelper.CreateWorkflow(parentJobHeader, "Parent Workflow 1");
			var parentWorkflow2 = BMSTestHelper.CreateWorkflow(parentJobHeader, "Parent Workflow 2");

			Factory.Save();

			using (var form = new ZForm(targetJob))
			using (var control = new TestJobRelatedParentWorkflowsControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(targetJob, string.Empty);
				form.Show();
				Application.DoEvents();

				var grid = control.Controls.Find("ParentWorkflowGrid", true).SingleOrDefault() as ZGrid;
				AssertNotNull("The control should have a ZDisplayGrid on it", grid);

				var button = control.Controls.Find("AddButton", true).SingleOrDefault() as ZButton;
				AssertNotNull(button);

				control.SelectedProcessHeadersToAddLink.Add(parentWorkflow1);
				control.SelectedProcessHeadersToAddLink.Add(parentWorkflow2);

				button.PerformClick();

				var query = new ZQuery(ProcessHeaderLinkSchema.FP_FH_HeaderFrom, targetJobHeader.PK);
				var links = Factory.Load<ProcessHeaderLink>(query);
				AssertContainsExactElementsInAnyOrder("Two links should be created", new[] { parentWorkflow1.PK, parentWorkflow2.PK }, links.Select(l => l.FP_FH_HeaderTo));
				Assert("Should allow saving the form", form.BusinessEntityForHasChanges.HasChanges);
			}
		}
	}
}
