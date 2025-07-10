using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.PAVE.Common.DTO;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Service.Test
{
	public class ComponentSectionBuilderForCustomisedLayoutsTest : TestCaseWithFactory
	{
		#region Choosing Layouts Enabled on the Web

		[TestDate(2019, 08, 05)]
		public void TestGetData_ShouldReturnSummaryCardCustomisationData_ForEntitiesRelatedToJobsThatHaveCustomLayoutEnabledOnTheWeb_AndStandardPropertiesOtherwise()
		{
			BMSRegistry.Instance.PAVEOnTheWeb.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var system = BMSTestHelper.CreateSystem(Factory, "ORG", "INQ", "OPP");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader = ProcessJobHeaderProvider.GetForParent(org, Factory) as ProcessJobHeader;
			var orgWorkflow = orgHeader.ProcessHeaders[0];
			var orgTask = BMSTestHelper.CreateTask(orgWorkflow);

			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var enquiryHeader = ProcessJobHeaderProvider.GetForParent(enquiry, Factory) as ProcessJobHeader;
			var enquiryWorkflow = enquiryHeader.ProcessHeaders[0];
			var enquiryTask = BMSTestHelper.CreateTask(enquiryWorkflow);

			var opp = Factory.NewWithValidTestData<OrgOpportunity>();
			var oppHeader = ProcessJobHeaderProvider.GetForParent(opp, Factory) as ProcessJobHeader;
			var oppWorkflow = oppHeader.ProcessHeaders[0];
			var oppTask = BMSTestHelper.CreateTask(oppWorkflow);

			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			orgWorkflow.MoveToComponent(buffer);
			enquiryWorkflow.MoveToComponent(buffer);
			oppWorkflow.MoveToComponent(buffer);

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			section.SectionConfiguration.CardType = CardTypeList.Codes.Task;

			var orgCustomisationForSection = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard, name: "orgCustomisationForSection");
			var orgCustomisationForSectionLink = section.SectionConfiguration.CustomisedLayoutLinks.AddNew();
			orgCustomisationForSectionLink.FML_FM_ControlCustomisation = orgCustomisationForSection.PK;
			orgCustomisationForSectionLink.FML_JobType = "ORG";

			orgCustomisationForSection.FM_JobType = "ORG";
			orgCustomisationForSection.RenderOnTheWeb = true;
			BMSTestHelper.CreateLine(orgCustomisationForSection, PropertySourceList.Codes.ProcessTask, ProcessTasksSchema.Constants.P9_ActualDuration, PropertyTypeList.Codes.Duration);
			BMSTestHelper.CreateLine(orgCustomisationForSection, PropertySourceList.Codes.Workflow, ProcessHeaderSchema.Constants.FH_CompletionStatement, PropertyTypeList.Codes.Text);
			BMSTestHelper.CreateLine(orgCustomisationForSection, PropertySourceList.Codes.Job, OrgHeaderSchema.Constants.OH_IsActive, PropertyTypeList.Codes.Boolean);

			org.OH_FullName = "Jumping on the bed";
			AssertEquals("Precondition", "Jumping on the bed", (org as ICodeDescription).Description);
			AssertEquals("Precondition", "XVBQP68SIYXQ", (org as ICodeDescription).Code);
			AssertEquals(true, org.OH_IsActive);

			orgWorkflow.FH_CompletionStatement = "Workflaw";
			AssertEquals("Precondition", "Organization (XVBQP68SIYXQ) - Workflaw", orgWorkflow.Description);
			AssertEquals("Precondition", "OPN", orgWorkflow.FH_Status);

			orgTask.P9_Status = "WRK";
			orgTask.P9_ActualDuration = new ZDateTime(2019, 1, 1).AddMinutes(10);

			var enqCustomisationForSection = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard, name: "enqCustomisationForSection");
			var enqCustomisationForSectionLink = section.SectionConfiguration.CustomisedLayoutLinks.AddNew();
			enqCustomisationForSectionLink.FML_FM_ControlCustomisation = enqCustomisationForSection.PK;
			enqCustomisationForSectionLink.FML_JobType = "INQ";

			enqCustomisationForSection.FM_JobType = "INQ";
			enqCustomisationForSection.RenderOnTheWeb = false;
			BMSTestHelper.CreateLine(enqCustomisationForSection, PropertySourceList.Codes.ProcessTask, ProcessTasksSchema.Constants.P9_ActualDuration, PropertyTypeList.Codes.Duration);

			var enqCustomisationForSystem_WithLowPriority = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard, name: "enqCustomisationForSystem_WithLowPriority");
			var enqCustomisationForSystemLink = system.CustomisedLayoutLinks.AddNew();
			enqCustomisationForSystemLink.FML_FM_ControlCustomisation = enqCustomisationForSystem_WithLowPriority.PK;
			enqCustomisationForSystemLink.FML_JobType = "INQ";

			enqCustomisationForSystem_WithLowPriority.FM_JobType = "INQ";
			enqCustomisationForSystem_WithLowPriority.RenderOnTheWeb = true;
			BMSTestHelper.CreateLine(enqCustomisationForSystem_WithLowPriority, PropertySourceList.Codes.ProcessTask, ProcessTasksSchema.Constants.P9_ActualDuration, PropertyTypeList.Codes.Duration);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Precondition: all custom layouts",
				new BMControlCustomisation[] { orgCustomisationForSection, enqCustomisationForSection, enqCustomisationForSystem_WithLowPriority },
				BMControlCustomisation.GetPotentialControlCustomisationLinks(section.SectionConfiguration, CustomisedControlTypeList.Codes.TaskCard).Select(link => link.CustomisedLayout));

			var componentSectionDTO = BuildSectionDTO(section) as ComponentSectionDTO;
			var orgTaskDTO = componentSectionDTO.Tasks.SingleOrDefault(t => t.PK == orgTask.PK);
			var enquiryTaskDTO = componentSectionDTO.Tasks.SingleOrDefault(t => t.PK == enquiryTask.PK);
			var oppTaskDTO = componentSectionDTO.Tasks.SingleOrDefault(t => t.PK == oppTask.PK);
			AssertNotNull(orgTaskDTO);
			AssertNotNull(enquiryTaskDTO);
			AssertNotNull(oppTaskDTO);

			var orgWorkflowDTO = componentSectionDTO.Workflows.SingleOrDefault(w => w.PK == orgWorkflow.PK);
			var enquiryWorkflowDTO = componentSectionDTO.Workflows.SingleOrDefault(w => w.PK == enquiryWorkflow.PK);
			var oppWorkflowDTO = componentSectionDTO.Workflows.SingleOrDefault(w => w.PK == oppWorkflow.PK);
			AssertNotNull(orgWorkflowDTO);
			AssertNotNull(enquiryWorkflowDTO);
			AssertNotNull(oppWorkflowDTO);

			var orgJobDTO = componentSectionDTO.Jobs.SingleOrDefault(j => j.PK == org.PK);
			var enquiryJobDTO = componentSectionDTO.Jobs.SingleOrDefault(j => j.PK == enquiry.PK);
			var oppJobDTO = componentSectionDTO.Jobs.SingleOrDefault(j => j.PK == opp.PK);
			AssertNotNull(orgJobDTO);
			AssertNotNull(enquiryJobDTO);
			AssertNotNull(oppJobDTO);

			var expectedOrgTaskProperties = new Dictionary<string, object>()
			{
				["status"] = (ZString)"WRK", // compulsory task property
				["actualDuration"] = ZDateTime.DefaultDurationEpoch.AddMinutes(10),
			};
			AssertionHelper.AssertDTOProperties(expectedOrgTaskProperties, orgTaskDTO.Properties);
			var expectedOrgWorkflowProperties = new Dictionary<string, object>()
			{
				["completionStatement"] = (ZString)"Workflaw",
				["description"] = (ZString)"Organization (XVBQP68SIYXQ) - Workflaw", // compulsory workflow property
				["status"] = (ZString)"OPN", // compulsory workflow property
			};
			AssertionHelper.AssertDTOProperties(expectedOrgWorkflowProperties, orgWorkflowDTO.Properties);
			var expectedOrgJobProperties = new Dictionary<string, object>()
			{
				["isActive"] = (ZBool)true,
				["description"] = "Jumping on the bed", // compulsory job property
				["code"] = "XVBQP68SIYXQ", // compulsory job property
			};
			AssertionHelper.AssertDTOProperties(expectedOrgJobProperties, orgJobDTO.Properties);

			var standardTaskPropertiesNames = new string[]
			{
				"description",
				"note",
				"status",
				"type",
				"resourceCode",
				"lowEstimatedMinutes",
				"standardEstimatedMinutes",
				"highEstimatedMinutes",
				"estimateVariationFactor",
				"estimatedTimeToCompleteMinutes",
				"isStartable",
			};
			AssertContainsExactElementsInAnyOrder(standardTaskPropertiesNames, enquiryTaskDTO.Properties.Keys);
			AssertContainsExactElementsInAnyOrder(standardTaskPropertiesNames, oppTaskDTO.Properties.Keys);

			var expectedWorkflowProperties = new Dictionary<string, object>()
			{
				["description"] = (ZString)"Job Workflow",
				["status"] = (ZString)"OPN",
			};
			AssertionHelper.AssertDTOProperties(expectedWorkflowProperties, enquiryWorkflowDTO.Properties);
			AssertionHelper.AssertDTOProperties(expectedWorkflowProperties, oppWorkflowDTO.Properties);

			var standardJobPropertiesNames = new string[]
			{
				"code",
				"description",
			};
			AssertContainsExactElementsInAnyOrder(standardJobPropertiesNames, enquiryJobDTO.Properties.Keys);
			AssertContainsExactElementsInAnyOrder(standardJobPropertiesNames, oppJobDTO.Properties.Keys);
		}

		#endregion

		#region Choosing Layouts by Job Type

		[TestDate(2019, 08, 05)]
		public void TestGetData_ShouldReturnSummaryCardCustomisationData_ForLayoutWithNoSpecifiedJobTypeInTheLink_ButWithSpecifiedJobTypeInTheLayout_WhenThereIsNoLinkForJobType()
		{
			AssertReturnsSummaryCardCustomisationData_ForLayoutWithNoSpecifiedJobTypeInTheLink_WhenThereIsNoLinkForJobType(layoutToApplyShouldHaveJobTypeSpecified: true);
		}

		[TestDate(2019, 08, 05)]
		public void TestGetData_ShouldReturnSummaryCardCustomisationData_ForLayoutWithNoSpecifiedJobTypeInTheLink_AndNoSpecifiedJobTypeInTheLayout_WhenThereIsNoLinkForJobType()
		{
			AssertReturnsSummaryCardCustomisationData_ForLayoutWithNoSpecifiedJobTypeInTheLink_WhenThereIsNoLinkForJobType(layoutToApplyShouldHaveJobTypeSpecified: false);
		}

		void AssertReturnsSummaryCardCustomisationData_ForLayoutWithNoSpecifiedJobTypeInTheLink_WhenThereIsNoLinkForJobType(bool layoutToApplyShouldHaveJobTypeSpecified)
		{
			BMSRegistry.Instance.PAVEOnTheWeb.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader = ProcessJobHeaderProvider.GetForParent(org, Factory) as ProcessJobHeader;
			var orgWorkflow = orgHeader.ProcessHeaders[0];
			var orgTask = BMSTestHelper.CreateTask(orgWorkflow);

			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			orgWorkflow.MoveToComponent(buffer);

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			section.SectionConfiguration.CardType = CardTypeList.Codes.Task;

			var orgCustomisationLinkedWithNoJobType = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard);
			var orgCustomisationLinkWithNoJobType = section.SectionConfiguration.CustomisedLayoutLinks.AddNew();
			orgCustomisationLinkWithNoJobType.FML_FM_ControlCustomisation = orgCustomisationLinkedWithNoJobType.PK;
			AssertEquals(string.Empty, orgCustomisationLinkWithNoJobType.FML_JobType);

			orgCustomisationLinkedWithNoJobType.FM_JobType = layoutToApplyShouldHaveJobTypeSpecified ? "ORG" : string.Empty;
			orgCustomisationLinkedWithNoJobType.RenderOnTheWeb = true;
			BMSTestHelper.CreateLine(orgCustomisationLinkedWithNoJobType, PropertySourceList.Codes.ProcessTask, ProcessTasksSchema.Constants.P9_ActualDuration, PropertyTypeList.Codes.Duration);

			orgTask.P9_Status = "WRK";
			orgTask.P9_ActualDuration = new ZDateTime(2019, 1, 1).AddMinutes(10);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Precondition: all custom layouts",
				new BMControlCustomisation[] { orgCustomisationLinkedWithNoJobType },
				BMControlCustomisation.GetPotentialControlCustomisationLinks(section.SectionConfiguration, CustomisedControlTypeList.Codes.TaskCard).Select(link => link.CustomisedLayout));

			var componentSectionDTO = BuildSectionDTO(section) as ComponentSectionDTO;
			var orgTaskDTO = componentSectionDTO.Tasks.SingleOrDefault(t => t.PK == orgTask.PK);
			AssertNotNull(orgTaskDTO);

			var expectedOrgTaskProperties = new Dictionary<string, object>()
			{
				["status"] = (ZString)"WRK", // compulsory task property
				["actualDuration"] = ZDateTime.DefaultDurationEpoch.AddMinutes(10),
			};
			AssertionHelper.AssertDTOProperties(expectedOrgTaskProperties, orgTaskDTO.Properties);
		}

		[TestDate(2019, 08, 05)]
		public void TestGetData_ShouldReturnStandardProperties_WhenLayoutSpecifiedForGivenJobTypeByTheLink_IsSetForWrongJobType_AndThereIsNoLinkWithNoSpecifiedJobType()
		{
			BMSRegistry.Instance.PAVEOnTheWeb.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader = ProcessJobHeaderProvider.GetForParent(org, Factory) as ProcessJobHeader;
			var orgWorkflow = orgHeader.ProcessHeaders[0];
			var orgTask = BMSTestHelper.CreateTask(orgWorkflow);

			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			orgWorkflow.MoveToComponent(buffer);

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			section.SectionConfiguration.CardType = CardTypeList.Codes.Task;

			var orgCustomisationLinkedWithJobType = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard);
			var orgCustomisationLinkWithJobType = section.SectionConfiguration.CustomisedLayoutLinks.AddNew();
			orgCustomisationLinkWithJobType.FML_FM_ControlCustomisation = orgCustomisationLinkedWithJobType.PK;
			orgCustomisationLinkWithJobType.FML_JobType = "ORG";

			orgCustomisationLinkedWithJobType.FM_JobType = "INQ"; // wrong job type!
			orgCustomisationLinkedWithJobType.RenderOnTheWeb = true;
			BMSTestHelper.CreateLine(orgCustomisationLinkedWithJobType, PropertySourceList.Codes.ProcessTask, ProcessTasksSchema.Constants.P9_Status, PropertyTypeList.Codes.Text);

			orgTask.P9_Status = "WRK";
			orgTask.P9_ActualDuration = new ZDateTime(2019, 1, 1).AddMinutes(10);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Precondition: all custom layouts",
				new BMControlCustomisation[] { orgCustomisationLinkedWithJobType },
				BMControlCustomisation.GetPotentialControlCustomisationLinks(section.SectionConfiguration, CustomisedControlTypeList.Codes.TaskCard).Select(link => link.CustomisedLayout));

			var componentSectionDTO = BuildSectionDTO(section) as ComponentSectionDTO;
			var orgTaskDTO = componentSectionDTO.Tasks.SingleOrDefault(t => t.PK == orgTask.PK);
			AssertNotNull(orgTaskDTO);

			var standardTaskPropertiesNames = new string[]
			{
				"description",
				"note",
				"status",
				"type",
				"resourceCode",
				"lowEstimatedMinutes",
				"standardEstimatedMinutes",
				"highEstimatedMinutes",
				"estimateVariationFactor",
				"estimatedTimeToCompleteMinutes",
				"isStartable",
			};
			AssertContainsExactElementsInAnyOrder(standardTaskPropertiesNames, orgTaskDTO.Properties.Keys);
		}

		[TestDate(2019, 08, 05)]
		public void TestGetData_ShouldReturnSummaryCardCustomisationData_ForLinkWithNoSpecifiedJobType_ButWithSpecifiedJobTypeInTheLayout_WhenLayoutSpecifiedForGivenJobTypeByTheLink_IsSetForWrongJobType()
		{
			AssertReturnsSummaryCardCustomisationData_ForLinkWithNoSpecifiedJobType_WhenLayoutSpecifiedForGivenJobTypeByTheLink_IsSetForWrongJobType(layoutToApplyShouldHaveJobTypeSpecified: true);
		}

		[TestDate(2019, 08, 05)]
		public void TestGetData_ShouldReturnSummaryCardCustomisationData_ForLinkWithNoSpecifiedJobType_AndNoSpecifiedJobTypeInTheLayout_WhenLayoutSpecifiedForGivenJobTypeByTheLink_IsSetForWrongJobType()
		{
			AssertReturnsSummaryCardCustomisationData_ForLinkWithNoSpecifiedJobType_WhenLayoutSpecifiedForGivenJobTypeByTheLink_IsSetForWrongJobType(layoutToApplyShouldHaveJobTypeSpecified: false);
		}

		void AssertReturnsSummaryCardCustomisationData_ForLinkWithNoSpecifiedJobType_WhenLayoutSpecifiedForGivenJobTypeByTheLink_IsSetForWrongJobType(bool layoutToApplyShouldHaveJobTypeSpecified)
		{
			BMSRegistry.Instance.PAVEOnTheWeb.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader = ProcessJobHeaderProvider.GetForParent(org, Factory) as ProcessJobHeader;
			var orgWorkflow = orgHeader.ProcessHeaders[0];
			var orgTask = BMSTestHelper.CreateTask(orgWorkflow);

			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			orgWorkflow.MoveToComponent(buffer);

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			section.SectionConfiguration.CardType = CardTypeList.Codes.Task;

			var orgCustomisationLinkedWithJobType = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard, name: "orgCustomisationLinkedWithJobType");
			var orgCustomisationLinkWithJobType = section.SectionConfiguration.CustomisedLayoutLinks.AddNew();
			orgCustomisationLinkWithJobType.FML_FM_ControlCustomisation = orgCustomisationLinkedWithJobType.PK;
			orgCustomisationLinkWithJobType.FML_JobType = "ORG";

			orgCustomisationLinkedWithJobType.FM_JobType = "INQ"; // wrong job type!
			orgCustomisationLinkedWithJobType.RenderOnTheWeb = true;
			BMSTestHelper.CreateLine(orgCustomisationLinkedWithJobType, PropertySourceList.Codes.ProcessTask, ProcessTasksSchema.Constants.P9_Status, PropertyTypeList.Codes.Text);

			var orgCustomisationLinkedWithNoJobType = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard, name: "orgCustomisationLinkedWithNoJobType");
			var orgCustomisationLinkWithNoJobType = section.SectionConfiguration.CustomisedLayoutLinks.AddNew();
			orgCustomisationLinkWithNoJobType.FML_FM_ControlCustomisation = orgCustomisationLinkedWithNoJobType.PK;
			AssertEquals(string.Empty, orgCustomisationLinkWithNoJobType.FML_JobType);

			orgCustomisationLinkedWithNoJobType.FM_JobType = layoutToApplyShouldHaveJobTypeSpecified ? "ORG" : string.Empty;
			orgCustomisationLinkedWithNoJobType.RenderOnTheWeb = true;
			BMSTestHelper.CreateLine(orgCustomisationLinkedWithNoJobType, PropertySourceList.Codes.ProcessTask, ProcessTasksSchema.Constants.P9_ActualDuration, PropertyTypeList.Codes.Duration);

			orgTask.P9_Status = "WRK";
			orgTask.P9_ActualDuration = new ZDateTime(2019, 1, 1).AddMinutes(10);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Precondition: all custom layouts",
				new BMControlCustomisation[] { orgCustomisationLinkedWithJobType, orgCustomisationLinkedWithNoJobType },
				BMControlCustomisation.GetPotentialControlCustomisationLinks(section.SectionConfiguration, CustomisedControlTypeList.Codes.TaskCard).Select(link => link.CustomisedLayout));

			var componentSectionDTO = BuildSectionDTO(section) as ComponentSectionDTO;
			var orgTaskDTO = componentSectionDTO.Tasks.SingleOrDefault(t => t.PK == orgTask.PK);
			AssertNotNull(orgTaskDTO);

			var expectedOrgTaskProperties = new Dictionary<string, object>()
			{
				["status"] = (ZString)"WRK", // compulsory task property
				["actualDuration"] = ZDateTime.DefaultDurationEpoch.AddMinutes(10),
			};
			AssertionHelper.AssertDTOProperties(expectedOrgTaskProperties, orgTaskDTO.Properties);
		}

		public void TestGetData_ShouldReturnStandardProperties_WhenLayoutsForBothLinksWithAndWithoutJobType_AreSetForWrongJobTypes()
		{
			BMSRegistry.Instance.PAVEOnTheWeb.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader = ProcessJobHeaderProvider.GetForParent(org, Factory) as ProcessJobHeader;
			var orgWorkflow = orgHeader.ProcessHeaders[0];
			var orgTask = BMSTestHelper.CreateTask(orgWorkflow);

			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			orgWorkflow.MoveToComponent(buffer);

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			section.SectionConfiguration.CardType = CardTypeList.Codes.Task;

			var orgCustomisationLinkedWithJobType = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard, name: "orgCustomisationLinkedWithJobType");
			var orgCustomisationLinkWithJobType = section.SectionConfiguration.CustomisedLayoutLinks.AddNew();
			orgCustomisationLinkWithJobType.FML_FM_ControlCustomisation = orgCustomisationLinkedWithJobType.PK;
			orgCustomisationLinkWithJobType.FML_JobType = "ORG";

			orgCustomisationLinkedWithJobType.FM_JobType = "INQ"; // wrong job type!
			orgCustomisationLinkedWithJobType.RenderOnTheWeb = true;
			BMSTestHelper.CreateLine(orgCustomisationLinkedWithJobType, PropertySourceList.Codes.ProcessTask, ProcessTasksSchema.Constants.P9_Status, PropertyTypeList.Codes.Text);

			var orgCustomisationLinkedWithNoJobType = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard, name: "orgCustomisationLinkedWithNoJobType");
			var orgCustomisationLinkWithNoJobType = section.SectionConfiguration.CustomisedLayoutLinks.AddNew();
			orgCustomisationLinkWithNoJobType.FML_FM_ControlCustomisation = orgCustomisationLinkedWithNoJobType.PK;
			AssertEquals(string.Empty, orgCustomisationLinkWithNoJobType.FML_JobType);

			orgCustomisationLinkedWithNoJobType.FM_JobType = "OPP"; // wrong job type again!
			orgCustomisationLinkedWithNoJobType.RenderOnTheWeb = true;
			BMSTestHelper.CreateLine(orgCustomisationLinkedWithNoJobType, PropertySourceList.Codes.ProcessTask, ProcessTasksSchema.Constants.P9_ActualDuration, PropertyTypeList.Codes.Duration);

			orgTask.P9_Status = "WRK";
			orgTask.P9_ActualDuration = new ZDateTime(2019, 1, 1).AddMinutes(10);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Precondition: all custom layouts",
				new BMControlCustomisation[] { orgCustomisationLinkedWithJobType, orgCustomisationLinkedWithNoJobType },
				BMControlCustomisation.GetPotentialControlCustomisationLinks(section.SectionConfiguration, CustomisedControlTypeList.Codes.TaskCard).Select(link => link.CustomisedLayout));

			var componentSectionDTO = BuildSectionDTO(section) as ComponentSectionDTO;
			var orgTaskDTO = componentSectionDTO.Tasks.SingleOrDefault(t => t.PK == orgTask.PK);
			AssertNotNull(orgTaskDTO);

			var standardTaskPropertiesNames = new string[]
			{
				"description",
				"note",
				"status",
				"type",
				"resourceCode",
				"lowEstimatedMinutes",
				"standardEstimatedMinutes",
				"highEstimatedMinutes",
				"estimateVariationFactor",
				"estimatedTimeToCompleteMinutes",
				"isStartable",
			};
			AssertContainsExactElementsInAnyOrder(standardTaskPropertiesNames, orgTaskDTO.Properties.Keys);
		}

		#endregion

		#region Nested Job Properties

		public void TestGetData_ShouldReturnNestedJobPropertiesInCustomisedLayoutData_WithNamesCobminedAndConvertedToCamelCase()
		{
			BMSRegistry.Instance.PAVEOnTheWeb.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader = ProcessJobHeaderProvider.GetForParent(org, Factory) as ProcessJobHeader;
			var orgWorkflow = orgHeader.ProcessHeaders[0];
			var orgTask = BMSTestHelper.CreateTask(orgWorkflow);

			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			orgWorkflow.MoveToComponent(buffer);

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			section.SectionConfiguration.CardType = CardTypeList.Codes.Task;

			var orgCustomisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard);
			var orgCustomisationForSectionLink = section.SectionConfiguration.CustomisedLayoutLinks.AddNew();
			orgCustomisationForSectionLink.FML_FM_ControlCustomisation = orgCustomisation.PK;
			orgCustomisationForSectionLink.FML_JobType = "ORG";

			orgCustomisation.FM_JobType = "ORG";
			orgCustomisation.RenderOnTheWeb = true;
			BMSTestHelper.CreateLine(orgCustomisation, PropertySourceList.Codes.Job, "<CompanyData.Company.Address1>", PropertyTypeList.Codes.Text);

			org.CompanyData.Company.Address1 = "Neverland";

			Factory.Save();

			var componentSectionDTO = BuildSectionDTO(section) as ComponentSectionDTO;
			var orgJobDTO = componentSectionDTO.Jobs.SingleOrDefault(j => j.PK == org.PK);
			AssertNotNull(orgJobDTO);

			var expectedOrgJobProperties = new Dictionary<string, object>()
			{
				["companyDataCompanyAddress1"] = (ZString)"Neverland",
				["code"] = "XVBQP68SIYXQ",
				["description"] = string.Empty,
			};
			AssertionHelper.AssertDTOProperties(expectedOrgJobProperties, orgJobDTO.Properties);
		}

		public void TestGetData_ShoudReturnNullForNestedProperties_WhenIntermediateObjectIsNull()
		{
			BMSRegistry.Instance.PAVEOnTheWeb.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader = ProcessJobHeaderProvider.GetForParent(org, Factory) as ProcessJobHeader;
			var orgWorkflow = orgHeader.ProcessHeaders[0];
			var orgTask = BMSTestHelper.CreateTask(orgWorkflow);

			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			orgWorkflow.MoveToComponent(buffer);

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			section.SectionConfiguration.CardType = CardTypeList.Codes.Task;

			var orgCustomisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard);
			var orgCustomisationForSectionLink = section.SectionConfiguration.CustomisedLayoutLinks.AddNew();
			orgCustomisationForSectionLink.FML_FM_ControlCustomisation = orgCustomisation.PK;
			orgCustomisationForSectionLink.FML_JobType = "ORG";

			orgCustomisation.FM_JobType = "ORG";
			orgCustomisation.RenderOnTheWeb = true;
			BMSTestHelper.CreateLine(orgCustomisation, PropertySourceList.Codes.Job, "<Branch.HomePort.Code>", PropertyTypeList.Codes.Text);

			AssertNull("Precondition", org.Branch);

			Factory.Save();

			var componentSectionDTO = BuildSectionDTO(section) as ComponentSectionDTO;
			var orgJobDTO = componentSectionDTO.Jobs.SingleOrDefault(j => j.PK == org.PK);
			AssertNotNull(orgJobDTO);

			var expectedOrgJobProperties = new Dictionary<string, object>() // compulsory properties
			{
				["branchHomePortCode"] = null,
				["code"] = "XVBQP68SIYXQ",
				["description"] = "",
			};
			AssertionHelper.AssertDTOProperties(expectedOrgJobProperties, orgJobDTO.Properties);
		}

		#endregion

		#region Convert Hours

		[TestDate(2019, 08, 05)]
		public void TestGetData_ShouldConvertHoursIntoMinutesInCustomisedLayoutData()
		{
			BMSRegistry.Instance.PAVEOnTheWeb.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader = ProcessJobHeaderProvider.GetForParent(org, Factory) as ProcessJobHeader;
			var orgWorkflow = orgHeader.ProcessHeaders[0];
			var orgTask = BMSTestHelper.CreateTask(orgWorkflow);

			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			orgWorkflow.MoveToComponent(buffer);

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			section.SectionConfiguration.CardType = CardTypeList.Codes.Task;

			var orgCustomisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard);
			var orgCustomisationForSectionLink = section.SectionConfiguration.CustomisedLayoutLinks.AddNew();
			orgCustomisationForSectionLink.FML_FM_ControlCustomisation = orgCustomisation.PK;
			orgCustomisationForSectionLink.FML_JobType = "ORG";

			orgCustomisation.FM_JobType = "ORG";
			orgCustomisation.RenderOnTheWeb = true;
			BMSTestHelper.CreateLine(orgCustomisation, PropertySourceList.Codes.ProcessTask, "LowEstimatedDurationHours", PropertyTypeList.Codes.Number);
			BMSTestHelper.CreateLine(orgCustomisation, PropertySourceList.Codes.ProcessTask, "StandardEstimateHours", PropertyTypeList.Codes.Number);
			BMSTestHelper.CreateLine(orgCustomisation, PropertySourceList.Codes.ProcessTask, "HighEstimatedDurationHours", PropertyTypeList.Codes.Number);
			BMSTestHelper.CreateLine(orgCustomisation, PropertySourceList.Codes.ProcessTask, "P9_EstimateVariationFactor", PropertyTypeList.Codes.Number);

			orgTask.P9_EstimateVariationFactor = 2;
			orgTask.P9_EstDuration = new ZDateTime(2019, 1, 1).AddHours(1);
			AssertEquals("Precondition: LowEstimatedDurationHours", orgTask.LowEstimatedDurationHours, 1m);
			AssertEquals("Precondition: HighEstimatedDurationHours", orgTask.HighEstimatedDurationHours, 2m);
			AssertEquals("Precondition: StandardEstimateHours", orgTask.StandardEstimateHours, 1.5m);

			Factory.Save();

			var componentSectionDTO = BuildSectionDTO(section) as ComponentSectionDTO;
			var orgTaskDTO = componentSectionDTO.Tasks.SingleOrDefault(t => t.PK == orgTask.PK);
			AssertNotNull(orgTaskDTO);

			var expectedOrgTaskProperties = new Dictionary<string, object>()
			{
				["status"] = (ZString)"ASN",
				["lowEstimatedDurationMinutes"] = (ZDecimal)60,
				["highEstimatedDurationMinutes"] = (ZDecimal)120,
				["standardEstimateMinutes"] = (ZDecimal)90,
				["estimateVariationFactor"] = (ZDecimal)2,
			};
			AssertionHelper.AssertDTOProperties(expectedOrgTaskProperties, orgTaskDTO.Properties);
		}

		#endregion

		#region Incorrect Properties

		public void TestGetData_ShouldReturnNullAndErrorReport_WhenCannotFindProperty()
		{
			BMSRegistry.Instance.PAVEOnTheWeb.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader = ProcessJobHeaderProvider.GetForParent(org, Factory) as ProcessJobHeader;
			var orgWorkflow = orgHeader.ProcessHeaders[0];
			var orgTask = BMSTestHelper.CreateTask(orgWorkflow);

			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			orgWorkflow.MoveToComponent(buffer);

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			section.SectionConfiguration.CardType = CardTypeList.Codes.Task;

			var orgCustomisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard);
			var orgCustomisationForSectionLink = section.SectionConfiguration.CustomisedLayoutLinks.AddNew();
			orgCustomisationForSectionLink.FML_FM_ControlCustomisation = orgCustomisation.PK;
			orgCustomisationForSectionLink.FML_JobType = "ORG";

			orgCustomisation.FM_JobType = "ORG";
			orgCustomisation.RenderOnTheWeb = true;
			BMSTestHelper.CreateLine(orgCustomisation, PropertySourceList.Codes.ProcessTask, "SomeNonExistentProperty", PropertyTypeList.Codes.Duration);

			orgTask.P9_Status = "WRK";

			Factory.Save();

			var componentSectionDTO = BuildSectionDTO(section) as ComponentSectionDTO;
			var orgTaskDTO = componentSectionDTO.Tasks.SingleOrDefault(t => t.WorkflowPK == orgWorkflow.PK);
			AssertNotNull(orgTaskDTO);

			var expectedOrgTaskProperties = new Dictionary<string, object>()
			{
				["status"] = (ZString)"WRK",
				["someNonExistentProperty"] = null,
			};
			AssertionHelper.AssertDTOProperties(expectedOrgTaskProperties, orgTaskDTO.Properties);

			AssertEquals("Cannot find property SomeNonExistentProperty from path SomeNonExistentProperty on type Enterprise.MasterFiles.Business.OrgHeaderProcessTask", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestGetData_ShouldReturnNullAndErrorReport_WhenCannotFindNestedProperty()
		{
			BMSRegistry.Instance.PAVEOnTheWeb.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader = ProcessJobHeaderProvider.GetForParent(org, Factory) as ProcessJobHeader;
			var orgWorkflow = orgHeader.ProcessHeaders[0];
			var orgTask = BMSTestHelper.CreateTask(orgWorkflow);

			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			orgWorkflow.MoveToComponent(buffer);

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			section.SectionConfiguration.CardType = CardTypeList.Codes.Task;

			var orgCustomisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard);
			var orgCustomisationForSectionLink = section.SectionConfiguration.CustomisedLayoutLinks.AddNew();
			orgCustomisationForSectionLink.FML_FM_ControlCustomisation = orgCustomisation.PK;
			orgCustomisationForSectionLink.FML_JobType = "ORG";

			orgCustomisation.FM_JobType = "ORG";
			orgCustomisation.RenderOnTheWeb = true;
			BMSTestHelper.CreateLine(orgCustomisation, PropertySourceList.Codes.ProcessTask, "JobHeader.SomeNonExistentProperty", PropertyTypeList.Codes.Duration);

			orgTask.P9_Status = "WRK";

			Factory.Save();

			var componentSectionDTO = BuildSectionDTO(section) as ComponentSectionDTO;
			var orgTaskDTO = componentSectionDTO.Tasks.SingleOrDefault(t => t.WorkflowPK == orgWorkflow.PK);
			AssertNotNull(orgTaskDTO);

			var expectedOrgTaskProperties = new Dictionary<string, object>()
			{
				["status"] = (ZString)"WRK",
				["jobHeaderSomeNonExistentProperty"] = null,
			};
			AssertionHelper.AssertDTOProperties(expectedOrgTaskProperties, orgTaskDTO.Properties);

			AssertEquals("Cannot find property SomeNonExistentProperty from path JobHeader.SomeNonExistentProperty on type Enterprise.BufferManagement.Business.ProcessJobHeader", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#endregion

		ISection BuildSectionDTO(BMBoardSection section) => new ComponentSectionBuilder(section).Build();

		protected override void SetUp()
		{
			base.SetUp();

			Globals.IsWebService = true;
			Globals.IsUserInteractive = false;
			BMSTestHelper.EnableBMSInRegistry();
		}
	}
}
