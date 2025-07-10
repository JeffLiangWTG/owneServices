using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class BMControlCustomisationLinkValidationTest : BusinessObjectValidationTestCase
	{
		public void TestJobType()
		{
			var link = Factory.New<BMControlCustomisationLink>();
			link.FML_JobType = "ZZZ";

			AssertHasError(link.FML_JobTypeInfo, "Enter a valid Job Type.");

			link.FML_JobType = WorkflowDescriptors.WorkItemWorkflowDescriptorCode;
			AssertNoErrors(link.FML_JobTypeInfo);

			link.FML_JobType = ZString.Empty;
			AssertNoErrors(link.FML_JobTypeInfo);
		}

		public void TestControlTypeUniqueByJobType()
		{
			var customisation1 = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard);
			var customisation2 = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard);
			var customisation3 = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard);

			var parent = BMSTestHelper.CreateSystem(Factory);

			var link1 = parent.CustomisedLayoutLinks.AddNew();
			var link2 = parent.CustomisedLayoutLinks.AddNew();

			link1.FML_FM_ControlCustomisation = customisation1.PK;
			link2.FML_FM_ControlCustomisation = customisation3.PK;

			link1.Validation.ValidateAll();
			link2.Validation.ValidateAll();

			AssertNoErrors("Customisations are for different control types - no errors", link1.FML_FM_ControlCustomisationInfo);
			AssertNoErrors("Customisations are for different control types - no errors", link2.FML_FM_ControlCustomisationInfo);

			link2.FML_FM_ControlCustomisation = customisation2.PK;

			link1.Validation.ValidateAll();
			link2.Validation.ValidateAll();

			AssertHasError("Customisations are for different control types - no errors", link1.FML_FM_ControlCustomisationInfo, "There is already a Task Detailed Card customization defined for this Job Type.");
			AssertHasError("Customisations are for different control types - no errors", link2.FML_FM_ControlCustomisationInfo, "There is already a Task Detailed Card customization defined for this Job Type.");

			link2.FML_JobType = "SIM";

			link1.Validation.ValidateAll();
			link2.Validation.ValidateAll();

			AssertNoErrors("Customisations are for different job types - no errors", link1.FML_FM_ControlCustomisationInfo);
			AssertNoErrors("Customisations are for different job types - no errors", link2.FML_FM_ControlCustomisationInfo);
		}

		public void TestControlType_ForSectionShowingWorkflowTickets()
		{
			var customisation1 = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard);
			var customisation2 = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.WorkflowSummaryCard);

			var parent = Factory.New<BMBoardSection>();
			parent.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;
			Assert(parent.SectionConfiguration.ShowWorkflowOrJobWorkflowCards);

			var link = parent.SectionConfiguration.CustomisedLayoutLinks.AddNew();

			link.FML_FM_ControlCustomisation = customisation1.PK;
			AssertHasError(link.FML_FM_ControlCustomisationInfo, "This section shows tickets for workflows. This layout is for tasks only and will not be used.");

			link.FML_FM_ControlCustomisation = customisation2.PK;
			AssertNoErrors(link.FML_FM_ControlCustomisationInfo);

			parent.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;
			Assert(parent.SectionConfiguration.ShowWorkflowOrJobWorkflowCards);

			link = parent.SectionConfiguration.CustomisedLayoutLinks.AddNew();

			link.FML_FM_ControlCustomisation = customisation1.PK;
			AssertHasError(link.FML_FM_ControlCustomisationInfo, "This section shows tickets for workflows. This layout is for tasks only and will not be used.");

			link.FML_FM_ControlCustomisation = customisation2.PK;
			parent.SectionConfiguration.CardType = CardTypeList.Codes.Task;
			Assert(!parent.SectionConfiguration.ShowWorkflowOrJobWorkflowCards);
			AssertHasError(link.FML_FM_ControlCustomisationInfo, "This section shows tickets for tasks. This layout is for workflows only and will not be used.");

			link.FML_FM_ControlCustomisation = customisation1.PK;
			AssertNoErrors(link.FML_FM_ControlCustomisationInfo);
		}
	}
}
