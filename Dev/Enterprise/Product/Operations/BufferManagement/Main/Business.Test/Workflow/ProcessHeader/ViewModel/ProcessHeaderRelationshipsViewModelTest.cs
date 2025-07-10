using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ProcessHeaderRelationshipsViewModel))]
	class ProcessHeaderRelationshipsViewModelTest : NonPersistentBusinessObjectTestCase
	{
		public void TestNavigateTo_ShouldMaintainCollectionWithOneItemOnly()
		{
			var job1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var workflow1_1 = jobHeader1.ProcessHeaders.AddNew();
			workflow1_1.FH_CompletionStatement = "workflow1_1";
			var workflow1_2 = jobHeader1.ProcessHeaders.AddNew();
			workflow1_1.FH_CompletionStatement = "workflow1_2";
			var job2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);
			var workflow2_1 = jobHeader2.ProcessHeaders.AddNew();
			workflow1_1.FH_CompletionStatement = "workflow2_1";
			var workflow2_2 = jobHeader2.ProcessHeaders.AddNew();
			workflow1_1.FH_CompletionStatement = "workflow2_2";

			Factory.Save();

			var viewModel = new ProcessHeaderRelationshipsViewModel(Factory);
			AssertEquals(false, viewModel.HasChanges);
			AssertEquals(0, viewModel.ActiveProcessHeaders.Count);
			AssertEquals(0, viewModel.TouchedProcessHeaders.Count);

			viewModel.NavigateTo(jobHeader1);
			AssertEquals(false, viewModel.HasChanges);
			AssertEquals(1, viewModel.ActiveProcessHeaders.Count);
			AssertEquals(1, viewModel.TouchedProcessHeaders.Count);
			AssertEquals(jobHeader1, viewModel.ActiveProcessHeaders[0]);
			AssertEquals(jobHeader1, viewModel.TouchedProcessHeaders[0]);

			viewModel.NavigateTo(jobHeader2);
			AssertEquals(false, viewModel.HasChanges);
			AssertEquals(1, viewModel.ActiveProcessHeaders.Count);
			AssertEquals(2, viewModel.TouchedProcessHeaders.Count);
			AssertEquals(jobHeader2, viewModel.ActiveProcessHeaders[0]);
			AssertEquals(jobHeader2, viewModel.TouchedProcessHeaders[1]);

			viewModel.NavigateTo(workflow1_1);
			AssertEquals(false, viewModel.HasChanges);
			AssertEquals(1, viewModel.ActiveProcessHeaders.Count);
			AssertEquals(3, viewModel.TouchedProcessHeaders.Count);
			AssertEquals(workflow1_1, viewModel.ActiveProcessHeaders[0]);
			AssertEquals(workflow1_1, viewModel.TouchedProcessHeaders[2]);

			viewModel.NavigateTo(workflow2_1);
			AssertEquals(false, viewModel.HasChanges);
			AssertEquals(1, viewModel.ActiveProcessHeaders.Count);
			AssertEquals(4, viewModel.TouchedProcessHeaders.Count);
			AssertEquals(workflow2_1, viewModel.ActiveProcessHeaders[0]);
			AssertEquals(workflow2_1, viewModel.TouchedProcessHeaders[3]);

			viewModel.NavigateTo(jobHeader2);
			AssertEquals(false, viewModel.HasChanges);
			AssertEquals(1, viewModel.ActiveProcessHeaders.Count);
			AssertEquals(4, viewModel.TouchedProcessHeaders.Count);
			AssertEquals(jobHeader2, viewModel.ActiveProcessHeaders[0]);

			jobHeader2.GetOrCreateDependencyLink(jobHeader1);

			AssertEquals(true, viewModel.HasChanges);
		}

		public void TestNagivateTo_ShouldKeepTrackOfJobHeaders()
		{
			var job1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var workflow1_1 = jobHeader1.ProcessHeaders.AddNew();
			workflow1_1.FH_CompletionStatement = "workflow1_1";
			var job2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);
			var workflow2_1 = jobHeader2.ProcessHeaders.AddNew();
			workflow2_1.FH_CompletionStatement = "workflow2_1";

			Factory.Save();

			var viewModel = new ProcessHeaderRelationshipsViewModel(Factory);

			viewModel.NavigateTo(workflow1_1);
			AssertEquals(2, viewModel.TouchedProcessHeaders.Count);
			AssertEquals(workflow1_1, viewModel.TouchedProcessHeaders[0]);
			AssertEquals(jobHeader1, viewModel.TouchedProcessHeaders[1]);

			viewModel.NavigateTo(jobHeader1);
			AssertEquals(2, viewModel.TouchedProcessHeaders.Count);
			AssertEquals(workflow1_1, viewModel.TouchedProcessHeaders[0]);
			AssertEquals(jobHeader1, viewModel.TouchedProcessHeaders[1]);

			viewModel.NavigateTo(jobHeader2);
			AssertEquals(3, viewModel.TouchedProcessHeaders.Count);
			AssertEquals(workflow1_1, viewModel.TouchedProcessHeaders[0]);
			AssertEquals(jobHeader1, viewModel.TouchedProcessHeaders[1]);
			AssertEquals(jobHeader2, viewModel.TouchedProcessHeaders[2]);

			viewModel.NavigateTo(workflow2_1);
			AssertEquals(4, viewModel.TouchedProcessHeaders.Count);
			AssertEquals(workflow1_1, viewModel.TouchedProcessHeaders[0]);
			AssertEquals(jobHeader1, viewModel.TouchedProcessHeaders[1]);
			AssertEquals(jobHeader2, viewModel.TouchedProcessHeaders[2]);
			AssertEquals(workflow2_1, viewModel.TouchedProcessHeaders[3]);
		}

		public void TestAddLinks_ShouldValidateInvalidLinks_Dependency()
		{
			BMSRegistry.Instance.AutomaticallyValidateWorkflowLoopsOnSave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var job1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			jobHeader1.FH_CompletionStatement = "jobHeader1";
			var workflow1 = jobHeader1.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "workflow1";

			var job2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);
			jobHeader2.FH_CompletionStatement = "jobHeader2";
			var workflow2 = jobHeader2.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "workflow2";

			AddAndGetParentChildLink(jobHeader1, workflow2);

			Factory.Save();

			Assert("Workflow2 is a parent of JobHeader1", workflow2.ChildLinks.ToArray().Any(link => link.FP_FH_HeaderFrom == jobHeader1.PK));
			CombineAssertions("JobHeader1 has no dependent links, or links with errors", () =>
			{
				AssertEquals(0, jobHeader1.PostrequisiteLinks_ForBinding.Count);
				AssertEquals(0, jobHeader1.PrerequisiteLinks_ForBinding.Count);

				foreach (var link in jobHeader1.Links.Union(jobHeader2.Links))
				{
					AssertNoErrors(link.FP_FH_HeaderFromInfo);
				}
			});

			var depLink = AddAndGetPrerequisiteLink(jobHeader1, jobHeader2);
			depLink.RunPreSaveValidation();
			AssertHasErrors("Our dependent link should have errors as it is evil", depLink.FP_FH_HeaderToInfo);
		}

		public void TestAddLinks_ShouldValidateInvalidLinks_ParentChild()
		{
			BMSRegistry.Instance.AutomaticallyValidateWorkflowLoopsOnSave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var job1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			jobHeader1.FH_CompletionStatement = "jobHeader1";
			var workflow1 = jobHeader1.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "workflow1";

			var job2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);
			jobHeader2.FH_CompletionStatement = "jobHeader2";
			var workflow2 = jobHeader2.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "workflow2";

			AddAndGetPrerequisiteLink(jobHeader1, workflow2);

			Factory.Save();

			Assert("JobHeader1 is a prereq of Workflow1", workflow2.PrerequisiteLinks_ForBinding.ToArray().Any(link => link.FP_FH_HeaderFrom == jobHeader1.PK));
			CombineAssertions("Workflow2 has no parent child links, or links with validation errors", () =>
			{
				AssertEquals(0, jobHeader1.ParentLinks_ForBinding.Count);
				AssertEquals(0, jobHeader1.ChildLinks_ForBinding.Count);

				foreach (var link in jobHeader1.Links.Union(jobHeader2.Links))
				{
					AssertNoErrors(link.FP_FH_HeaderFromInfo);
				}
			});

			var pcLink = AddAndGetParentChildLink(jobHeader2, jobHeader1);
			pcLink.RunPreSaveValidation();
			AssertHasErrors("Our familial relationships should have errors as it is evil", pcLink.FP_FH_HeaderToInfo);
		}

		#region Implementation

		/// <summary>
		/// Adds a Prerequisite link:
		/// fromHeader -> toHeader
		/// fromHeader becomes a prerequisite, toHeader becomes a postrequisite
		/// </summary>
		ProcessHeaderLink AddAndGetPrerequisiteLink(ProcessHeader fromHeader, ProcessHeader toHeader)
		{
			if (!toHeader.PrerequisiteLinks_ForBinding.Any(link => link.FP_FH_HeaderFrom == fromHeader.PK))
			{
				// to clarify, this reads as "add fromHeader to the pool of prerequisites of toHeader"
				new ProcessHeaderRelationshipsViewModel(Factory).AddLinks(new[] { fromHeader }, toHeader.PrerequisiteLinks_ForBinding, RelationshipDirection.From);
			}

			return toHeader.PrerequisiteLinks_ForBinding.First(link => link.FP_FH_HeaderFrom == fromHeader.PK);
		}

		/// <summary>
		/// Adds a Parent Child link:
		/// child -> parent
		/// child becomes a child of parent
		/// </summary>
		ProcessHeaderLink AddAndGetParentChildLink(ProcessHeader child, ProcessHeader parent)
		{
			if (!parent.ChildLinks_ForBinding.Any(link => link.FP_FH_HeaderFrom == child.PK))
			{
				// to clarify, this reads as "add child to the pool of child workflows for parent"
				new ProcessHeaderRelationshipsViewModel(Factory).AddLinks(new[] { child }, parent.ChildLinks_ForBinding, RelationshipDirection.From);
			}

			return parent.ChildLinks_ForBinding.First(link => link.FP_FH_HeaderFrom == child.PK);
		}

		#endregion
	}
}
