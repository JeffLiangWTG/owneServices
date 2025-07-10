using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Core;
using Enterprise.CustomerService.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(SupportIncidentEscalateAction))]
	public class SupportIncidentEscalationActionTest : SupportIncidentActionTestCase
	{
		public void TestRecalculateProductAreaOnModuleChange()
		{
			var productAreas = new CodeDescriptionPairList();
			productAreas.AddPair("PA1", "Product Area 1");
			productAreas.AddPair("PA2", "Product Area 2");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productAreas);

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "desc", true);
			var sysMapping = product.ModuleMappings.AddNew("XXX", "XXX Description", "PA1", true);
			sysMapping.SourceModuleMappings.AddNew("SourceModule1", "PA2");
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			//var sourceModules = new SourceModuleCollection();
			//sourceModules.AddNew("SourceModule1", "Menu Item A", "", ModuleListType.MenuSection, "PA1", true, true, "ENT");
			//EDIDataRegistry.Instance.SourceModules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, sourceModules);

			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Priority = "CR9";
			incident.IM_Module = "XXX";

			AssertEquals("PA1", incident.ProductArea);

			Factory.Save();

			var escalateAction = new SupportIncidentEscalateAction(incident, "CR9");
			escalateAction.EscalationStage = "DEF";
			escalateAction.EscalationCriticality = "CR4";
			escalateAction.SectionRequirementService = "XXX";

			escalateAction.SynchroniseToIncident();

			AssertEquals("CR4", incident.IM_Priority);
			AssertEquals("XXX", incident.IM_Module);
			AssertEquals("PA1", incident.ProductArea);
		}

		public void TestPerformAction_CorrectCriticalityForWorkflowTemplateMatching()
		{
			var templateOrg = Factory.NewWithValidTestData<OrgHeader>();
			EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templateOrg.PK.ToGuid());

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "INC";
			template.P0_OH_Client = templateOrg.PK;
			var header = template.ProcessHeaders.AddNew();
			header.FH_CompletionStatement = IncidentEventFactory.Codes.Escalate + " AAA";

			var task1 = template.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = header.PK;
			task1.P9_Sequence = 10;
			task1.P9_Type = "AAA";
			task1.P9_Description = "CR5 Task";
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task1.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			task1.TemplateConditions.TemplateCondition2Value = @"""<IM_Priority>"" == ""CR5""";

			var task2 = template.WorkflowItems.AddNew();
			task2.P9_FH_ProcessHeader = header.PK;
			task2.P9_Sequence = 10;
			task2.P9_Type = "AAA";
			task2.P9_Description = "Non CR5 Task";
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task2.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			task2.TemplateConditions.TemplateCondition2Value = @"""<IM_Priority>"" != ""CR5""";

			var incident = Factory.New<SupportIncident>();
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");

			Factory.Save();

			var action = new SupportIncidentEscalateAction(incident, incident.IM_Priority);
			action.EscalationStage = SupportIncidentCategoriesList.Codes.Support;
			action.EscalationCriticality = Constants.CustomerService.CriticalityCodes.CR5_Training;
			action.SynchroniseToIncident();
			Factory.Save();

			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
			AssertEquals(Constants.CustomerService.CriticalityCodes.CR5_Training, incident.IM_Priority);
			AssertEquals(1, incident.WorkflowItems.Count);
			AssertEquals("CR5 Task", incident.WorkflowItems[0].P9_Description);

			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
			action = new SupportIncidentEscalateAction(incident, Constants.CustomerService.CriticalityCodes.CR5_Training);
			action.EscalationStage = SupportIncidentCategoriesList.Codes.FeatureRequest;
			action.EscalationCriticality = Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
			action.SynchroniseToIncident();
			Factory.Save();
			AssertEquals("Should have criticality change message", true, incident.EConversation.GetTimeOrderedMessages().Any(msg => msg.Body == "Criticality changed from CR5 to CR6"));
		}

		public void TestPerformAction_CorrectProductAreaForWorkflowTemplateMatching()
		{
			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew(Cr8ModuleList.Codes.CarbonEnvironmentalCompliance, Cr8ModuleList.Descriptions.CarbonEnvironmentalCompliance, ProductAreaList.Codes.ARC, true);
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var templateOrg = Factory.NewWithValidTestData<OrgHeader>();
			EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templateOrg.PK.ToGuid());

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "INC";
			template1.P0_OH_Client = templateOrg.PK;
			var header1 = template1.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = IncidentEventFactory.Codes.Escalate + " GEN";

			var task1 = template1.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = header1.PK;
			task1.P9_Sequence = 10;
			task1.P9_Type = "AAA";
			task1.P9_Description = "GEN Task";
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = "INC";
			template2.P0_OH_Client = templateOrg.PK;
			template2.P0_SubType4 = ProductAreaList.Codes.ARC;
			var header2 = template2.ProcessHeaders.AddNew();
			header2.FH_CompletionStatement = IncidentEventFactory.Codes.Escalate + " ARC";

			var task2 = template2.WorkflowItems.AddNew();
			task2.P9_FH_ProcessHeader = header2.PK;
			task2.P9_Sequence = 10;
			task2.P9_Type = "AAA";
			task2.P9_Description = "ARC Task";
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;

			Factory.Save();

			var action = new SupportIncidentEscalateAction(incident, incident.IM_Priority);
			action.EscalationStage = SupportIncidentCategoriesList.Codes.ComplianceRequirement;
			action.EscalationCriticality = Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
			action.SectionRequirementService = Cr8ModuleList.Codes.CarbonEnvironmentalCompliance;
			action.SynchroniseToIncident();

			AssertEquals(SupportIncidentCategoriesList.Codes.ComplianceRequirement, incident.IM_Category);
			AssertEquals(Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement, incident.IM_Priority);
			AssertEquals(Cr8ModuleList.Codes.CarbonEnvironmentalCompliance, incident.IM_Module);
			AssertEquals(ProductAreaList.Codes.ARC, incident.ProductArea);
			AssertEquals(1, incident.WorkflowItems.Count);
			AssertEquals("ARC Task", incident.WorkflowItems[0].P9_Description);
		}

		public void TestPerformAction_CloseCR6FeatureRequest()
		{
			var templateOrg = Factory.NewWithValidTestData<OrgHeader>();
			EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templateOrg.PK.ToGuid());

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "INC";
			template1.P0_OH_Client = templateOrg.PK;
			template1.P0_SubType2 = SupportIncidentCategoriesList.Codes.FeatureRequest;
			var header1 = template1.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "ESC Escalate Feature";

			var task11 = template1.WorkflowItems.AddNew();
			task11.P9_FH_ProcessHeader = header1.PK;
			task11.P9_Sequence = 10;
			task11.P9_Type = "INV";
			task11.P9_Description = "Feature Accepted";
			task11.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR5_Training;
			var existingTask1 = incident.WorkflowItems.AddNew();
			existingTask1.P9_Sequence = 10;
			var existingTask2 = incident.WorkflowItems.AddNew();
			existingTask2.P9_Sequence = 20;

			Factory.Save();

			var action = new SupportIncidentEscalateAction(incident, incident.IM_Priority);
			action.EscalationStage = SupportIncidentCategoriesList.Codes.FeatureRequest;
			action.EscalationCriticality = Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
			action.SynchroniseToIncident();

			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.FeatureAccepted, incident.IM_ResolutionCode);
			AssertEquals(3, incident.WorkflowItems.Count);
			AssertEquals("Feature Accepted", incident.WorkflowItems[2].P9_Description);
			AssertEquals("Escalate Feature", incident.WorkflowItems[2].ProcessHeader.FH_CompletionStatement);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, incident.WorkflowItems[2].P9_Status);
		}

		public void TestShouldNotReOpen_WhenStageBeingToContentDevelopment()
		{
			var templateOrg = Factory.NewWithValidTestData<OrgHeader>();
			EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templateOrg.PK.ToGuid());

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "INC";
			template1.P0_OH_Client = templateOrg.PK;
			template1.P0_SubType2 = SupportIncidentCategoriesList.Codes.ContentDevelopment;
			var header1 = template1.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "ESC Escalate Feature";

			var task11 = template1.WorkflowItems.AddNew();
			task11.P9_FH_ProcessHeader = header1.PK;
			task11.P9_Sequence = 10;
			task11.P9_Type = "INV";
			task11.P9_Description = "Feature Accepted";
			task11.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR5_Training;
			var existingTask1 = incident.WorkflowItems.AddNew();
			existingTask1.P9_Sequence = 10;
			var existingTask2 = incident.WorkflowItems.AddNew();
			existingTask2.P9_Sequence = 20;
			incident.IM_Status = SupportIncidentLookups.Status.Closed;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed;
			incident.IM_ClosureResolution = SupportIncidentLookups.DispositionList.Constants.Closed.TrainingFlaggedForContentDevelopment;
			Factory.Save();

			var action = new SupportIncidentEscalateAction(incident, incident.IM_Priority);
			action.EscalationStage = SupportIncidentCategoriesList.Codes.ContentDevelopment;
			action.EscalationCriticality = Constants.CustomerService.CriticalityCodes.CR5_Training;
			action.SynchroniseToIncident();

			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.TrainingFlaggedForContentDevelopment, incident.IM_ClosureResolution);
			AssertEquals(3, incident.WorkflowItems.Count);
			AssertEquals("Feature Accepted", incident.WorkflowItems[2].P9_Description);
			AssertEquals("Escalate Feature", incident.WorkflowItems[2].ProcessHeader.FH_CompletionStatement);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, incident.WorkflowItems[2].P9_Status);
		}

		public void TestSectionRequirementServiceHumanReadableName()
		{
			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentEscalateAction(incident, incident.IM_Priority);
			AssertEquals("Menu Section", action.SectionRequirementServiceInfo.HumanReadableName);

			action.EscalationCriticality = Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
			AssertEquals("Requirement", action.SectionRequirementServiceInfo.HumanReadableName);

			action.EscalationCriticality = Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
			AssertEquals("Service", action.SectionRequirementServiceInfo.HumanReadableName);

			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
			action = new SupportIncidentEscalateAction(incident, incident.IM_Priority);
			action.EscalationCriticality = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			action.SectionRequirementService = "";
			AssertHasError(action.SectionRequirementServiceInfo, "Please enter a Menu Section.");

			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			action = new SupportIncidentEscalateAction(incident, incident.IM_Priority);
			action.EscalationCriticality = Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
			action.SectionRequirementService = "";
			AssertHasError(action.SectionRequirementServiceInfo, "Please enter a Requirement.");

			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			action = new SupportIncidentEscalateAction(incident, incident.IM_Priority);
			action.EscalationCriticality = Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
			action.SectionRequirementService = "";
			AssertHasError(action.SectionRequirementServiceInfo, "Please enter a Service.");
		}

		public void TestEscalationStageList()
		{
			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentEscalateAction(incident, incident.IM_Priority);
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
			Assert(!action.EscalationStageList.ContainsCode(SupportIncidentCategoriesList.Codes.Support));

			incident = Factory.New<SupportIncident>();
			incident.SetupForNewCreatedDefect();
			action = new SupportIncidentEscalateAction(incident, incident.IM_Priority);
			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);
			Assert(!action.EscalationStageList.ContainsCode(SupportIncidentCategoriesList.Codes.Defect));

			incident = Factory.New<SupportIncident>();
			incident.SetupForNewCreatedFeatureRequest();
			action = new SupportIncidentEscalateAction(incident, incident.IM_Priority);
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
			Assert(!action.EscalationStageList.ContainsCode(SupportIncidentCategoriesList.Codes.FeatureRequest));

			incident = Factory.New<SupportIncident>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.ComplianceRequirement;
			action = new SupportIncidentEscalateAction(incident, incident.IM_Priority);
			Assert(!action.EscalationStageList.ContainsCode(SupportIncidentCategoriesList.Codes.ComplianceRequirement));

			incident = Factory.New<SupportIncident>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.CustomerServiceRequest;
			action = new SupportIncidentEscalateAction(incident, incident.IM_Priority);
			Assert(!action.EscalationStageList.ContainsCode(SupportIncidentCategoriesList.Codes.CustomerServiceRequest));
		}

		public void TestEscalationStageListNotContainsSUP_WhenHasWorkitemAttached()
		{
			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentEscalateAction(incident, incident.IM_Priority);
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
			Assert(!action.EscalationStageList.ContainsCode(SupportIncidentCategoriesList.Codes.Support));

			incident.SetupForNewCreatedDefect();
			incident.RelatedWorkItems.AddNew();
			action = new SupportIncidentEscalateAction(incident, incident.IM_Priority);
			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);
			Assert(!action.EscalationStageList.ContainsCode(SupportIncidentCategoriesList.Codes.Support));

			var incidentCR6 = Factory.New<SupportIncident>();
			incidentCR6.SetupForNewCreatedFeatureRequest();
			incidentCR6.RelatedWorkItems.AddNew();
			var actionCR6 = new SupportIncidentEscalateAction(incidentCR6, incidentCR6.IM_Priority);
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incidentCR6.IM_Category);
			Assert(!actionCR6.EscalationStageList.ContainsCode(SupportIncidentCategoriesList.Codes.Support));
		}

		public void TestProductAreaList()
		{
			#region Test Data
			{
				var menuSectionCollection = new SystemProductCollection();
				var entProduct = menuSectionCollection.AddNew("ENT", "ediEnterprise / CargoWise One", true);
				entProduct.ModuleMappings.AddNew("AR1", "ediArchiveManager", ProductAreaList.Codes.ARC, false);
				entProduct.ModuleMappings.AddNew("CCC", "Test Module C", ProductAreaList.Codes.CUS, false);
				entProduct.ModuleMappings.AddNew("DDD", "Test Module D", ProductAreaList.Codes.CUS, false);

				var glwProduct = menuSectionCollection.AddNew("GLW", "Glow", false);
				glwProduct.ModuleMappings.AddNew("WEB", "Web Module", ProductAreaList.Codes.PAV, false);

				EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, menuSectionCollection);
			}
			{
				var cr8Collection = new SystemProductCollection();
				var entProduct = cr8Collection.AddNew("ENT", "ediEnterprise / CargoWise One", true);
				entProduct.ModuleMappings.AddNew("8EA", "8EA", ProductAreaList.Codes.GEO, false);
				entProduct.ModuleMappings.AddNew("8EB", "8EB", ProductAreaList.Codes.CUS, false);

				var glwProduct = cr8Collection.AddNew("GLW", "Glow", true);
				glwProduct.ModuleMappings.AddNew("8GA", "8GA", ProductAreaList.Codes.CIL, false);
				glwProduct.ModuleMappings.AddNew("8GB", "8GB", ProductAreaList.Codes.CIL, false);

				EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cr8Collection);
			}
			{
				var cr9Collection = new SystemProductCollection();
				var entProduct = cr9Collection.AddNew("ENT", "ediEnterprise / CargoWise One", true);
				entProduct.ModuleMappings.AddNew("9EA", "9EA", ProductAreaList.Codes.RAT, false);

				var glwProduct = cr9Collection.AddNew("GLW", "Glow", true);

				EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cr9Collection);
			}
			#endregion

			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = "ENT";
			var action = new SupportIncidentEscalateAction(incident, Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround);
			action.EscalationCriticality = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					ProductAreaList.Codes.ARC,
					ProductAreaList.Codes.CUS
				},
				action.ProductAreaList.Cast<ICodeDescription>().Select(x => x.Code));

			action.EscalationCriticality = Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					ProductAreaList.Codes.GEO,
					ProductAreaList.Codes.CUS
				},
				action.ProductAreaList.Cast<ICodeDescription>().Select(x => x.Code));

			action.EscalationCriticality = Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					ProductAreaList.Codes.RAT
				},
				action.ProductAreaList.Cast<ICodeDescription>().Select(x => x.Code));
		}

		public void TestWillRecalculateERequestStatusWhenEscalateIncidentToCNT()
		{
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var task1 = incident1.WorkflowItems.AddNew();
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			incident1.SetupForNewCreatedFeatureRequest();
			var workItem1 = Factory.NewWithValidTestData<NewWorkItem>();
			workItem1.WKI_Status = "WRK";
			incident1.RelatedItems.Add(workItem1);
			Factory.Save();

			var templateOrg = Factory.NewWithValidTestData<OrgHeader>();
			EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templateOrg.PK.ToGuid());

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "INC";
			template1.P0_OH_Client = templateOrg.PK;
			template1.P0_SubType2 = SupportIncidentCategoriesList.Codes.ContentDevelopment;
			var header1 = template1.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "ESC Escalate Feature";

			var task11 = template1.WorkflowItems.AddNew();
			task11.P9_FH_ProcessHeader = header1.PK;
			task11.P9_Sequence = 10;
			task11.P9_Type = "INV";
			task11.P9_Description = "Feature Accepted";
			task11.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task11.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			Factory.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var incident1Reload = factory2.Load<SupportIncident>(incident1.PK);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, incident1Reload.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.Status.Working, incident1Reload.IM_Status);

			var escalateAction = new SupportIncidentEscalateAction(incident1, "CR5");
			escalateAction.EscalationStage = SupportIncidentCategoriesList.Codes.ContentDevelopment;
			escalateAction.EscalationCriticality = "CR5";

			escalateAction.SynchroniseToIncident();
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction, incident1.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.Status.Working, incident1.IM_Status);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var incident = Factory.New<SupportIncident>();
			return new SupportIncidentEscalateAction(incident, incident.IM_Priority);
		}
	}
}
