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
	class JobRelatedChildWorkflowsControlTest : BMSTestCaseWithFactory
	{
		public void TestDoubleClickingShouldOpenTheWorkflow()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "ORG");

			var targetJob = Factory.NewWithValidTestData<OrgHeader>();
			var targetJobHeader = BMSTestHelper.CreateJobHeader(targetJob, description: "Target Job Header");
			var targetWorkflow1 = BMSTestHelper.CreateWorkflow(targetJobHeader, "Target Workflow 1");

			var childjob = Factory.NewWithValidTestData<OrgHeader>();
			var childJobHeader = BMSTestHelper.CreateJobHeader(childjob, description: "Child Job Header");
			var childWorkflow1 = BMSTestHelper.CreateWorkflow(childJobHeader, "Child Workflow 1");

			BMSTestHelper.CreateParentChildLink(targetWorkflow1, childWorkflow1);

			Factory.Save();

			using (var form = new ZForm(targetJob))
			using (var control = new TestJobRelatedChildWorkflowsControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(targetJob, string.Empty);
				form.Show();
				Application.DoEvents();

				var grid = control.Controls.Find("ChildWorkflowGrid", true).SingleOrDefault() as ZGrid;
				AssertNotNull("The control should have a ZDisplayGrid on it", grid);

				grid.Select(0);
				grid.PerformDoubleClickForTest();

				using (var orgForm = Application.OpenForms.OfType<ZOrganisationsForm>().SingleOrDefault())
				{
					AssertNotNull("Should open organisation form", orgForm);
					AssertEquals(childjob.PK, ((OrgHeader)orgForm.BusinessEntity).PK);
				}
			}
		}

		public void TestRemoveChildWorkflowButton()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "ORG");

			var targetJob = Factory.NewWithValidTestData<OrgHeader>();
			var targetJobHeader = BMSTestHelper.CreateJobHeader(targetJob, description: "Target Job Header");
			var targetWorkflow1 = BMSTestHelper.CreateWorkflow(targetJobHeader, "Target Workflow 1");

			var childjob = Factory.NewWithValidTestData<OrgHeader>();
			var childJobHeader = BMSTestHelper.CreateJobHeader(childjob, description: "Child Job Header");
			var childWorkflow1 = BMSTestHelper.CreateWorkflow(childJobHeader, "Child Workflow 1");

			var link = BMSTestHelper.CreateParentChildLink(targetWorkflow1, childWorkflow1);

			Factory.Save();

			using (var form = new ZForm(targetJob))
			using (var control = new TestJobRelatedChildWorkflowsControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(targetJob, string.Empty);
				form.Show();
				Application.DoEvents();

				var grid = control.Controls.Find("ChildWorkflowGrid", true).SingleOrDefault() as ZGrid;
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

		public void TestAddChildWorkflowButton_ShouldCreateLinksForJobHeader()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "ORG");

			var targetJob = Factory.NewWithValidTestData<OrgHeader>();
			var targetJobHeader = BMSTestHelper.CreateJobHeader(targetJob, description: "Target Job Header");
			BMSTestHelper.CreateWorkflow(targetJobHeader, "Target Workflow 1");

			var childjob = Factory.NewWithValidTestData<OrgHeader>();
			var childJobHeader = BMSTestHelper.CreateJobHeader(childjob, description: "Child Job Header");
			var childWorkflow1 = BMSTestHelper.CreateWorkflow(childJobHeader, "Child Workflow 1");
			var childWorkflow2 = BMSTestHelper.CreateWorkflow(childJobHeader, "Child Workflow 2");

			Factory.Save();

			using (var form = new ZForm(targetJob))
			using (var control = new TestJobRelatedChildWorkflowsControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(targetJob, string.Empty);
				form.Show();
				Application.DoEvents();

				var grid = control.Controls.Find("ChildWorkflowGrid", true).SingleOrDefault() as ZGrid;
				AssertNotNull("The control should have a ZDisplayGrid on it", grid);

				var button = control.Controls.Find("AddButton", true).SingleOrDefault() as ZButton;
				AssertNotNull(button);

				control.SelectedProcessHeadersToAddLink.Add(childWorkflow1);
				control.SelectedProcessHeadersToAddLink.Add(childWorkflow2);

				button.PerformClick();

				var query = new ZQuery(ProcessHeaderLinkSchema.FP_FH_HeaderTo, targetJobHeader.PK);
				var links = Factory.Load<ProcessHeaderLink>(query);
				AssertContainsExactElementsInAnyOrder("Two links should be created", new[] { childWorkflow1.PK, childWorkflow2.PK }, links.Select(l => l.FP_FH_HeaderFrom));
				Assert("Should allow saving the form", form.BusinessEntityForHasChanges.HasChanges);
			}
		}
	}
}
