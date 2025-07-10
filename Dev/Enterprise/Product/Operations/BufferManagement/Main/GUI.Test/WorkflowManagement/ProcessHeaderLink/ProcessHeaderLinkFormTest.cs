using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.GUI.Internal.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(ProcessHeaderLinkForm))]
	class ProcessHeaderLinkFormTest : ZFormBasherTest
	{
		#region Workflow Lookup Buttons

		public void TestWorkflowLookupButtons_ShouldSetPropertyValues()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

			var link = GetDependencyLink();

			BMSTestCaseWithFactory.AssertIsNotPrerequisite(workflow1, workflow2);

			using (var form = new ProcessHeaderLinkForm(link))
			{
				form.Show();

				AssertNull(link.HeaderFrom);
				AssertNull(link.HeaderTo);

				using (BusinessObjectModulePickerTest.SelectRecordsOnDialogShown(workflow1))
				{
					form.FindAndClickButton("FromWorkflowSelectorButton");
				}

				AssertEquals(workflow1, link.HeaderFrom);
				AssertNull(link.HeaderTo);

				using (BusinessObjectModulePickerTest.SelectRecordsOnDialogShown(workflow2))
				{
					form.FindAndClickButton("ToWorkflowSelectorButton");
				}

				AssertEquals(workflow1, link.HeaderFrom);
				AssertEquals(workflow2, link.HeaderTo);

				BMSTestCaseWithFactory.AssertIsPrerequisite(workflow1, workflow2);
			}
		}

		public void TestOpenWorkflowButtons()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var link = jobHeader1.GetOrCreateDependencyLink(jobHeader2);

			Factory.Save();

			using (var form = new ProcessHeaderLinkForm(link))
			{
				form.Show();

				var openFormsGetter = new Func<ZOrganisationsForm[]>(() => Application.OpenForms.OfType<ZOrganisationsForm>().ToArray());

				AssertEquals(0, openFormsGetter().Length);

				form.FindAndClickButton("OpenPrereqButton");

				using (var openForm = openFormsGetter().SingleOrDefault())
				{
					AssertNotNull(openForm);
					VisualBoardsTestCase.AssertSamePK((BusinessObject)jobHeader1.Parent, openForm.BusinessEntity);

					form.FindAndClickButton("OpenPrereqButton");
					AssertEquals(1, openFormsGetter().Length);

					form.FindAndClickButton("OpenDependentButton");

					using (var otherOpenForm = openFormsGetter().Except(new[] { openForm }).SingleOrDefault())
					{
						AssertNotNull(otherOpenForm);
						VisualBoardsTestCase.AssertSamePK((BusinessObject)jobHeader2.Parent, otherOpenForm.BusinessEntity);
					}
				}
			}
		}

		#endregion

		#region Caption

		public void TestFormCaption()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Malcolm");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Bill");

			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			using (var form = new ProcessHeaderLinkForm(link))
			{
				AssertEquals("Dependency Link from [Malcolm] to [Bill]", form.FormCaption);
			}

			using (var form = new ProcessHeaderLinkForm())
			{
				AssertEquals(string.Empty, form.FormCaption);
			}

			using (var form = new ProcessHeaderLinkForm(GetDependencyLink()))
			{
				AssertEquals("Dependency Link from [] to []", form.FormCaption);
			}
		}

		#endregion

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new ProcessHeaderLinkForm(GetDependencyLink());
		}

		ProcessHeaderLink GetDependencyLink()
		{
			var link = Factory.New<ProcessHeaderLink>();

			using (link.SuspendSettingHasChanges())
			{
				link.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;
			}

			return link;
		}

		#endregion
	}
}
