using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Integration.Tests
{
	class ConsolToContainerLinkageServiceTest_ForwardingTest : ConsolToContainerLinkageServiceTest<ForwardingConsol, ForwardingContainer>
	{
	}

	[GuiTest]
	abstract class ConsolToContainerLinkageServiceTest<TConsol, TContainer> : BMSTestCaseWithFactory
		where TConsol : CommonConsol, IWorkflowProvider
		where TContainer : CommonContainer, IWorkflowProvider
	{
		public void TestLinkConsolToContainer_WhenNoTemplateLinkDefined_ShouldNotLink()
		{
			var consol = FreightTestHelper.CreateConsol<TConsol>(Factory);
			var container = FreightTestHelper.CreateContainer<TContainer>(Factory, consol);

			Factory.Save();

			AssertEquals(container, consol.Containers.SingleOrDefault());

			var consolJobHeader = ProcessJobHeader.GetForParentWithoutCreation(consol, Factory);
			var containerJobHeader = ProcessJobHeader.GetForParentWithoutCreation(container, Factory);

			AssertIsNotPrerequisite(containerJobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "containerWorkflow2"), consolJobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "consolWorkflow1"));
			AssertIsNotPrerequisite(consolJobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "consolWorkflow2"), containerJobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "containerWorkflow1"));
		}

		public void TestLinkConsolToContainer_WhenTemplateLinkDefined_ShouldLink()
		{
			CreateDependencyLink(containerTemplateWorkflow2, consolTemplateWorkflow1);
			CreateDependencyLink(consolTemplateWorkflow2, containerTemplateWorkflow1);

			var consol = FreightTestHelper.CreateConsol<TConsol>(Factory);
			var container = FreightTestHelper.CreateContainer<TContainer>(Factory, consol);

			Factory.Save();

			AssertEquals(container, consol.Containers.SingleOrDefault());

			var consolJobHeader = ProcessJobHeader.GetForParentWithoutCreation(consol, Factory);
			var containerJobHeader = ProcessJobHeader.GetForParentWithoutCreation(container, Factory);

			var consolWorkflow1 = consolJobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "consolWorkflow1");
			var consolWorkflow2 = consolJobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "consolWorkflow2");

			var containerWorkflow1 = containerJobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "containerWorkflow1");
			var containerWorkflow2 = containerJobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "containerWorkflow2");

			AssertIsPrerequisite(containerWorkflow2, consolWorkflow1);
			AssertIsPrerequisite(consolWorkflow2, containerWorkflow1);

			var link_containerToConsol = containerWorkflow2.PostrequisiteLinks.Single();
			var link_consolToContainer = consolWorkflow2.PostrequisiteLinks.Single();

			container.Delete();
			Factory.Save();

			AssertEquals(true, link_consolToContainer.IsDeleted);
			AssertEquals(true, link_consolToContainer.IsDeleted);
		}

		public void TestLinkConsolToContainer_ShouldLinkOnConsolSave_WayAfterCreatingJobLink_WhenTemplateBecomesApplicable_AsTheResultOfModifyingConsol()
		{
			consolTemplate.P0_LoadPortCountry = "ADALV"; // this makes the template specific (corresponds to JK_RL_NKLoadPort)
			CreateDependencyLink(containerTemplate.GetJobHeader(), consolTemplate.GetJobHeader());

			var consol = FreightTestHelper.CreateConsol<TConsol>(Factory);
			var container = FreightTestHelper.CreateContainer<TContainer>(Factory, consol);

			Factory.Save();

			AssertEquals(container, consol.Containers.SingleOrDefault());

			var consolJobHeader = ProcessJobHeader.GetForParentWithoutCreation(consol, Factory);
			var containerJobHeader = ProcessJobHeader.GetForParentWithoutCreation(container, Factory);

			AssertIsNotPrerequisite(containerJobHeader, consolJobHeader);

			consol.JK_RL_NKLoadPort = "ADALV"; // modify so that the specific template is applicable now (corresponds to P0_LoadPortCountry)

			Factory.Save();

			AssertIsPrerequisite(containerJobHeader, consolJobHeader);
		}

		public void TestLinkConsolToContainer_ShouldLinkOnConsolSave_WayAfterCreatingJobLink_WhenTemplateBecomesApplicable_AsTheResultOfModifyingContainer()
		{
			containerTemplate.P0_SubType2 = "FCL"; // this makes the template specific (corresponds to JC_ContainerMode)
			CreateDependencyLink(containerTemplate.GetJobHeader(), consolTemplate.GetJobHeader());

			var consol = FreightTestHelper.CreateConsol<TConsol>(Factory);
			var container = FreightTestHelper.CreateContainer<TContainer>(Factory, consol);

			Factory.Save();

			AssertEquals(container, consol.Containers.SingleOrDefault());

			var consolJobHeader = ProcessJobHeader.GetForParentWithoutCreation(consol, Factory);
			var containerJobHeader = ProcessJobHeader.GetForParentWithoutCreation(container, Factory);

			AssertIsNotPrerequisite(containerJobHeader, consolJobHeader);

			container.JC_ContainerMode = "FCL"; // modify so that the specific template is applicable now (corresponds to P0_LoadPortCountry)

			Factory.Save();

			AssertIsPrerequisite(containerJobHeader, consolJobHeader);
		}

		#region Implementation

		ProcessTaskTemplate containerTemplate;
		ProcessHeader containerTemplateWorkflow1, containerTemplateWorkflow2;

		ProcessTaskTemplate consolTemplate;
		ProcessHeader consolTemplateWorkflow1, consolTemplateWorkflow2;

		BusinessObjectFactory templateFactory;

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, WorkflowDescriptors.ContainerWorkflowDescriptorCode);
			BMSTestHelper.AddWorkflowType(config.System, WorkflowDescriptors.JobConsolWorkflowDescriptorCode);

			Factory.Save();

			templateFactory = Factory.CreateNewFactory();

			containerTemplate = BMSTestHelper.CreateWorkflowTemplate(templateFactory, WorkflowDescriptors.ContainerWorkflowDescriptorCode, Constants.TransportModes.Air);
			containerTemplateWorkflow1 = BMSTestHelper.CreateWorkflow(containerTemplate, "containerWorkflow1");
			containerTemplateWorkflow2 = BMSTestHelper.CreateWorkflow(containerTemplate, "containerWorkflow2");

			BMSTestHelper.CreateTask(containerTemplate, containerTemplateWorkflow1, GlbStaff.CurrentUser.GS_Code);
			BMSTestHelper.CreateTask(containerTemplate, containerTemplateWorkflow2, GlbStaff.CurrentUser.GS_Code);

			consolTemplate = BMSTestHelper.CreateWorkflowTemplate(templateFactory, WorkflowDescriptors.JobConsolWorkflowDescriptorCode, Constants.TransportModes.Air);
			consolTemplateWorkflow1 = BMSTestHelper.CreateWorkflow(consolTemplate, "consolWorkflow1");
			consolTemplateWorkflow2 = BMSTestHelper.CreateWorkflow(consolTemplate, "consolWorkflow2");

			BMSTestHelper.CreateTask(consolTemplate, consolTemplateWorkflow1, GlbStaff.CurrentUser.GS_Code);
			BMSTestHelper.CreateTask(consolTemplate, consolTemplateWorkflow2, GlbStaff.CurrentUser.GS_Code);

			templateFactory.Save();
		}

		void CreateDependencyLink(ProcessHeader fromWorkflow, ProcessHeader toWorkflow)
		{
			var loadedFromWorkflow = templateFactory.Load<ProcessHeader>(fromWorkflow.PK);
			var loadedToWorkflow = templateFactory.Load<ProcessHeader>(toWorkflow.PK);
			var loadedTemplate = templateFactory.Load<ProcessTaskTemplate>(containerTemplate.PK);

			BMSTestHelper.CreateDependencyLink(loadedTemplate, loadedFromWorkflow, loadedToWorkflow);

			templateFactory.Save();
		}

		#endregion
	}
}
