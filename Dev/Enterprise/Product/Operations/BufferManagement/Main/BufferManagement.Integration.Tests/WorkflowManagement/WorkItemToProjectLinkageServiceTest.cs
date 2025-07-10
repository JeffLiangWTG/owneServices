using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Integration;

namespace Enterprise.BufferManagement.Integration.Tests
{
	class WorkItemToProjectLinkageServiceTest : BMSTestCaseWithFactory
	{
		public void TestLinkWorkItemToProject_WhenBufferManagementEnabledForWorkItemOnly_ShouldNotProposeLinks()
		{
			SetupSystemAndTemplates(WorkflowDescriptors.WorkItemWorkflowDescriptorCode);
			CreateAndAttachWorkItemAndProject(out var workItemJobHeader, out var projectJobHeader);

			AssertNotNull(workItemJobHeader);
			AssertNull(projectJobHeader);
		}

		public void TestLinkWorkItemToProject_WhenBufferManagementEnabledForProjectOnly_ShouldNotProposeLinks()
		{
			SetupSystemAndTemplates(WorkflowDescriptors.ProjectWorkflowDescriptorCode);
			CreateAndAttachWorkItemAndProject(out var workItemJobHeader, out var projectJobHeader);

			AssertNull(workItemJobHeader);
			AssertNotNull(projectJobHeader);
		}

		public void TestLinkWorkItemToProject_WhenBufferManagementEnabledForBothModules_ShouldProposeLinks()
		{
			SetupSystemAndTemplates(WorkflowDescriptors.WorkItemWorkflowDescriptorCode, WorkflowDescriptors.ProjectWorkflowDescriptorCode);
			CreateAndAttachWorkItemAndProject(out var workItemJobHeader, out var projectJobHeader);

			AssertIsPrerequisite(workItemJobHeader, projectJobHeader);
		}

		#region Implementation

		void SetupSystemAndTemplates(params string[] workflowTypes)
		{
			var system = BMSTestHelper.CreateSystem(Factory, workflowTypes);
			var jobHeaders = new List<ProcessJobHeader>();

			foreach (var type in workflowTypes)
			{
				var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, type);
				var templateJobHeader = template.GetJobHeader();
				BMSTestHelper.CreateTask(template, templateJobHeader);

				jobHeaders.Add(templateJobHeader);
			}

			if (jobHeaders.Count >= 2)
			{
				// Create an inter-job template link
				BMSTestHelper.CreateDependencyLink(jobHeaders[0].Template, jobHeaders[0], jobHeaders[1]);
			}

			Factory.Save();
		}

		void CreateAndAttachWorkItemAndProject(out ProcessJobHeader workItemJobHeader, out ProcessJobHeader projectJobHeader)
		{
			var workItem = Factory.New<IWorkItem>();
			var project = Factory.NewWithValidTestData<Project>();

			((BusinessObject)workItem).FillWithValidTestData();

			Factory.Save();

			((IWorkTaskRelatedItemSource)workItem).RelatedItems.Add(project);

			workItemJobHeader = ProcessJobHeader.GetForParentWithoutCreation((IWorkflowProvider)workItem, Factory);
			projectJobHeader = ProcessJobHeader.GetForParentWithoutCreation(project, Factory);
		}

		#endregion
	}
}
