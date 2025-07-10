using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.DataTransfer.Native.Adapter;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting
{
	class WorkflowTemplateTest : TestCaseWithFactory
	{
		public void TestImportWorkflowTemplate()
		{
			var capability = Factory.New<GlbCapability>();
			capability.G4_Code = "DEV";

			Factory.Save();

			using (var stream = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.ConcreteUnitTesting.Testing.Workflow.WorkflowTemplateWithLinks.xml")))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream.BaseStream);
				var insertLog = manager.GetLogs();
				AssertMultilineASCIIEquals("Insert Log Text", @"
--- Start Import Process --------------------------------------------------------------
Processed: WorkflowTemplate
--- Import Process Finished -----------------------------------------------------------
ProcessTaskTemplate - 1 inserts, 0 updates, 0 deletes
ProcessTemplateTrigger - 1 inserts, 0 updates, 0 deletes
ProcessTaskNotification_UniversalTriggers - 1 inserts, 0 updates, 0 deletes
ProcessJobHeader - 1 inserts, 0 updates, 0 deletes
Workflow - 2 inserts, 0 updates, 0 deletes
ProcessTasks - 4 inserts, 0 updates, 0 deletes
ProcessTaskNotification_RegularTriggers - 2 inserts, 0 updates, 0 deletes
ProcessHeaderLink - 1 inserts, 0 updates, 0 deletes

				".Trim(), insertLog);
			}
			var query = new ZQuery();
			query.AddToFilter(ProcessTaskTemplateSchema.P0_Name, "test workitem");

			var loadedProcessTaskTemplate = Factory.Load<ProcessTaskTemplate>(query).Single();

			AssertEquals("2 tasks, 1 milestone, 1 trigger", 4, loadedProcessTaskTemplate.WorkflowItems.Count);
			AssertEquals(1, loadedProcessTaskTemplate.TemplateTriggers.Count);
			AssertEquals(1, loadedProcessTaskTemplate.TemplateTriggers[0].TriggerActions.Count);

			var universalTrigger = loadedProcessTaskTemplate.TemplateTriggers[0];
			var triggerAction = (ProcessTaskNotification)universalTrigger.TriggerActions[0];

			AssertEquals(Events.CustomisableEvent00Code, universalTrigger.TriggerEventCode);
			AssertEquals("Universal trigger", universalTrigger.Description);
			AssertEquals(ProcessTasksLookups.UserDefinedCondition, universalTrigger.TemplateCondition2);
			AssertEquals("\"<WKI_WorkItemType>\"==\"ENT\"", universalTrigger.TemplateCondition2Value);

			AssertEquals(WorkflowTriggerActionTypeConstants.Codes.SetField, triggerAction.PQ_TriggerType);
			AssertEquals("<WKI_Summary>", triggerAction.PQ_FieldName);
			AssertEquals("<WKI_Summary> Boop", triggerAction.PQ_FieldValue);

			var jobWorkflow = loadedProcessTaskTemplate.ProcessHeaders.OfType<IProcessJobHeader>().Single(ph => ph.FH_CompletionStatement == "Job is complete.");
			var workflow1 = loadedProcessTaskTemplate.ProcessHeaders.ToArray().Cast<IProcessHeader>().Single(ph => ph.FH_CompletionStatement == "p1");
			var workflow2 = loadedProcessTaskTemplate.ProcessHeaders.ToArray().Cast<IProcessHeader>().Single(ph => ph.FH_CompletionStatement == "p2");

			AssertEquals(jobWorkflow.PK, workflow1.FH_FH_ParentHeader);
			AssertEquals(jobWorkflow.PK, workflow2.FH_FH_ParentHeader);

			var link1 = workflow1.LinksFromMeToOthers[0];

			AssertEquals(link1.FP_FH_HeaderTo, workflow2.PK);

			var tasks = loadedProcessTaskTemplate.WorkflowItems.OfType<IProcessTask>();

			AssertEquals(1, tasks.Count(t => t.P9_FH_ProcessHeader == workflow1.PK));
			AssertEquals(1, tasks.Count(t => t.P9_FH_ProcessHeader == workflow2.PK));
		}

		[TestDate(2016, 06, 03, 0, 0, 0)]
		public void TestExportWorkflowTemplate()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "gorp";

			var capability = Factory.New<GlbCapability>();
			capability.G4_Code = "DEV";

			var def = Factory.New<ITagDefinition>();
			def.TGD_Code = "AAA";

			var mag = Factory.New<ITagMagnitude>();
			mag.TGM_Code = "ZZZ";
			mag.TGM_TGD_Tag = def.PK;

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_Name = "le boop teh snoot";
			template.P0_ProcessType = "ORG";

			var normalTrigger = template.WorkflowItems.Triggers.AddNew();
			normalTrigger.P9_Description = "Groop";
			normalTrigger.TriggerConditions.TriggerEventCode = Events.TagWasAddedOrRemovedCode;

			normalTrigger.TemplateConditions.TemplateCondition2 = TemplateProcessTaskLookups.UserDefinedCondition;
			normalTrigger.TemplateConditions.TemplateCondition2Value = "NoogieNoogieNoogie";

			var triggerCompletionAction = normalTrigger.ProcessTaskNotifications.AddNew();
			triggerCompletionAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			triggerCompletionAction.PQ_FieldName = "Football";

			var universalTrigger = (ITemplateTrigger)template.TemplateTriggers.AddNew();
			universalTrigger.TriggerEventCode = Events.TagWasAddedOrRemovedCode;
			universalTrigger.Description = "Triggah";

			var universalTriggerCompletionAction = (ProcessTaskNotification)universalTrigger.TriggerActions.AddNew();
			universalTriggerCompletionAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			universalTriggerCompletionAction.PQ_FieldName = "Soccer";

			var workflow1 = template.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "p1";
			var workflow2 = template.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "p2";

			workflow2.AddTag(mag, false);

			var task = template.TasksExcludingCompletionStatements.AddNew();
			task.P9_FH_ProcessHeader = workflow1.PK;

			var link = (IProcessHeaderLink)template.ProcessHeaderLinks.AddNew();
			link.FP_FH_HeaderFrom = workflow1.PK;
			link.FP_FH_HeaderTo = workflow2.PK;
			link.FP_LinkType = "DEP";

			var rule = (IProcessTemplateReleaseGroupRule)template.ReleaseGroupRules.AddNew();
			rule.PTR_Name = "Rool";
			rule.PTR_AreAllWorkflowCategoriesApplicable = false;
			rule.PTR_Sequence = 1;
			rule.PTR_ValueSelectionMacro = "<macro>";

			var category = (IProcessTemplateReleaseGroupRuleCategory)rule.Categories.AddNew();
			category.PTC_Category = "MEW";

			var mapping = (IProcessTemplateReleaseGroupRuleMapping)rule.GroupMappings.AddNew();
			mapping.PTM_Value = "Toast";
			mapping.PTM_GG_Group = group.PK;

			Factory.Save();

			using (var baseStream = NativeDataTransferTestHelper.ExportToStream(template))
			{
				link.Delete();
				task.Delete();
				workflow1.Delete();
				workflow2.Delete();
				mapping.Delete();
				category.Delete();
				rule.Delete();
				template.Delete();

				Factory.Save();

				var insertLog = NativeDataTransferTestHelper.ImportAndGetInsertLog(baseStream);
				AssertMultilineASCIIEquals("Insert Log Text", @"
--- Start Import Process --------------------------------------------------------------
Processed: WorkflowTemplate
--- Import Process Finished -----------------------------------------------------------
ProcessTaskTemplate - 1 inserts, 0 updates, 0 deletes
ProcessTemplateTrigger - 1 inserts, 0 updates, 0 deletes
ProcessTaskNotification_UniversalTriggers - 1 inserts, 0 updates, 0 deletes
ProcessJobHeader - 1 inserts, 0 updates, 0 deletes
Workflow - 2 inserts, 0 updates, 0 deletes
ProcessTasks - 2 inserts, 0 updates, 0 deletes
TagLink - 1 inserts, 0 updates, 0 deletes
ProcessTaskNotification_RegularTriggers - 1 inserts, 0 updates, 0 deletes
TemplateConditionNote - 1 inserts, 0 updates, 0 deletes
ProcessHeaderLink - 1 inserts, 0 updates, 0 deletes
ReleaseGroupRule - 1 inserts, 0 updates, 0 deletes
RuleCategory - 1 inserts, 0 updates, 0 deletes
GroupRuleMapping - 1 inserts, 0 updates, 0 deletes
				".Trim(), insertLog);

				var query = new ZQuery();
				query.AddToFilter(ProcessTaskTemplateSchema.P0_Name, "le boop teh snoot");

				var loadedProcessTaskTemplate = Factory.Load<ProcessTaskTemplate>(query).Single();

				AssertEquals(2, loadedProcessTaskTemplate.WorkflowItems.Count);

				var newJobHeader = loadedProcessTaskTemplate.ProcessHeaders.OfType<IProcessJobHeader>().Single(ph => ph.FH_CompletionStatement == "Job is complete.");
				workflow1 = loadedProcessTaskTemplate.ProcessHeaders.ToArray().Cast<IProcessHeader>().Single(ph => ph.FH_CompletionStatement == "p1");
				workflow2 = loadedProcessTaskTemplate.ProcessHeaders.ToArray().Cast<IProcessHeader>().Single(ph => ph.FH_CompletionStatement == "p2");

				AssertEquals(1, workflow2.TagLinks_ForBinding.Count);
				AssertEquals(mag.PK, workflow2.TagLinks_ForBinding[0].TGL_TGM_Magnitude);

				AssertEquals(newJobHeader.PK, workflow1.FH_FH_ParentHeader);
				AssertEquals(newJobHeader.PK, workflow2.FH_FH_ParentHeader);

				var link1 = workflow1.LinksFromMeToOthers[0];

				AssertEquals(link1.FP_FH_HeaderTo, workflow2.PK);

				var loadedTask = (ProcessTask)loadedProcessTaskTemplate.WorkflowItems.Tasks.Single();
				AssertEquals(workflow1.PK, loadedTask.P9_FH_ProcessHeader);

				var loadedTrigger = (ProcessTask)loadedProcessTaskTemplate.WorkflowItems.Triggers.Single();
				AssertEquals("Groop", loadedTrigger.P9_Description);
				AssertEquals(1, loadedTrigger.ProcessTaskNotifications.Count);
				AssertEquals("Football", loadedTrigger.ProcessTaskNotifications[0].PQ_FieldName);
				AssertEquals(TemplateProcessTaskLookups.UserDefinedCondition, loadedTrigger.TemplateConditions.TemplateCondition2);
				AssertEquals("NoogieNoogieNoogie", loadedTrigger.TemplateConditions.TemplateCondition2Value);

				var loadedUniversalTrigger = loadedProcessTaskTemplate.TemplateTriggers.Cast<ITemplateTrigger>().Single();
				AssertEquals("Triggah", loadedUniversalTrigger.Description);

				AssertEquals(1, loadedUniversalTrigger.TriggerActions.Count);
				AssertEquals("Soccer", loadedUniversalTrigger.TriggerActions.Cast<ProcessTaskNotification>().Single().PQ_FieldName);

				var loadedRule = loadedProcessTaskTemplate.ReleaseGroupRules.Cast<IProcessTemplateReleaseGroupRule>().Single();
				AssertEquals("Rool", loadedRule.PTR_Name);
				AssertEquals(false, loadedRule.PTR_AreAllWorkflowCategoriesApplicable);
				AssertEquals((byte)1, loadedRule.PTR_Sequence);
				AssertEquals("<macro>", loadedRule.PTR_ValueSelectionMacro);

				var loadedCategory = loadedRule.Categories.Cast<IProcessTemplateReleaseGroupRuleCategory>().Single();
				AssertEquals("MEW", loadedCategory.PTC_Category);

				var loadedMapping = loadedRule.GroupMappings.Cast<IProcessTemplateReleaseGroupRuleMapping>().Single();
				AssertEquals("Toast", loadedMapping.PTM_Value);
				AssertEquals(group.PK, loadedMapping.PTM_GG_Group);
			}
		}

		public void TestExportAndImportTemplate_ShouldMatchByName()
		{
			var template1 = Factory.New<ProcessTaskTemplate>();
			var template2 = Factory.New<ProcessTaskTemplate>();

			template1.P0_Name = "Sean Spicer";
			template2.P0_Name = "Sean Spider";

			var task1 = template1.WorkflowItems.Tasks.AddNew();
			var task2 = template2.WorkflowItems.Tasks.AddNew();

			task1.P9_Description = "Malcolm";
			task2.P9_Description = "Trumble";

			Factory.Save();

			using (var template1Stream = NativeDataTransferTestHelper.ExportToStream(template1))
			using (var template2Stream = NativeDataTransferTestHelper.ExportToStream(template2))
			{
				template1.Delete();
				task2.P9_Description = "Turnbull";

				Factory.Save();

				var log1 = NativeDataTransferTestHelper.ImportAndGetInsertLog(template1Stream);
				var log2 = NativeDataTransferTestHelper.ImportAndGetInsertLog(template2Stream);

				AssertMultilineASCIIEquals("",
@"--- Start Import Process --------------------------------------------------------------
Processed: WorkflowTemplate
--- Import Process Finished -----------------------------------------------------------
ProcessTaskTemplate - 1 inserts, 0 updates, 0 deletes
ProcessTasks - 1 inserts, 0 updates, 0 deletes", log1);

				AssertMultilineASCIIEquals("",
@"--- Start Import Process --------------------------------------------------------------
Processed: WorkflowTemplate
--- Import Process Finished -----------------------------------------------------------
ProcessTaskTemplate - 0 inserts, 0 updates, 0 deletes
ProcessTasks - 0 inserts, 1 updates, 0 deletes", log2);

				var newFactory = Factory.CreateNewFactory();

				var loadedTemplate1 = newFactory.Load<ProcessTaskTemplate>(new ZQuery(ProcessTaskTemplateSchema.P0_Name, "Sean Spicer")).SingleOrDefault();
				var loadedTemplate2 = newFactory.Load<ProcessTaskTemplate>(new ZQuery(ProcessTaskTemplateSchema.P0_Name, "Sean Spider")).SingleOrDefault();

				AssertNotNull(loadedTemplate1);
				AssertNotNull(loadedTemplate2);

				AssertNotEquals("Should have created a new template to replace the one that was deleted", template1.PK, loadedTemplate1.PK);
				AssertEquals(template2.PK, loadedTemplate2.PK);

				AssertEquals(1, loadedTemplate1.WorkflowItems.Tasks.Count);
				AssertEquals(1, loadedTemplate2.WorkflowItems.Tasks.Count);

				AssertEquals("Malcolm", loadedTemplate1.WorkflowItems.Tasks[0].P9_Description);
				AssertEquals("Trumble", loadedTemplate2.WorkflowItems.Tasks[0].P9_Description);
			}
		}

		public void TestExportAndImportTemplate_WithUdfConditions()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_Name = "TemplateWithUDF";

			var task = template.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Task";
			task.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			task.TemplateConditions.TemplateCondition2Value = "\"1\"==\"1\"";

			Factory.Save();

			using (var templateStream = NativeDataTransferTestHelper.ExportToStream(template))
			{
				template.Delete();
				Factory.Save();

				var log = NativeDataTransferTestHelper.ImportAndGetInsertLog(templateStream);

				AssertMultilineASCIIEquals("",
@"--- Start Import Process --------------------------------------------------------------
Processed: WorkflowTemplate
--- Import Process Finished -----------------------------------------------------------
ProcessTaskTemplate - 1 inserts, 0 updates, 0 deletes
ProcessTasks - 1 inserts, 0 updates, 0 deletes
TemplateConditionNote - 1 inserts, 0 updates, 0 deletes", log);

				var newFactory = Factory.CreateNewFactory();
				var importedTemplate = newFactory.Load<ProcessTaskTemplate>(new ZQuery(ProcessTaskTemplateSchema.P0_Name, "TemplateWithUDF")).SingleOrDefault();

				CombineAssertions("After exporting and re importing template the template conditions should be the same", () =>
				{
					AssertNotNull(importedTemplate);
					var importedTask = (ProcessTask)importedTemplate.WorkflowItems.Tasks.First();
					AssertEquals(ProcessTasksLookups.UserDefinedCondition, importedTask.TemplateConditions.TemplateCondition2);
					AssertEquals("\"1\"==\"1\"", importedTask.TemplateConditions.TemplateCondition2Value);
				});
			}
		}

		public void TestOldXmlWithUdfInStmNotesStillWorks()
		{
			#region Xml blob
			var nativeXML = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>9999723</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <WorkflowTemplate version=""2.0"">
      <ProcessTaskTemplate Action=""MERGE"">
        <PK>9bf33f72-4ac3-4d2d-a834-ecaf52f84b1c</PK>
        <ProcessType>SHP</ProcessType>
        <SubType1></SubType1>
        <SubType2></SubType2>
        <SubType3></SubType3>
        <LoadPortCountry></LoadPortCountry>
        <DischargePortCountry></DischargePortCountry>
        <SubType4></SubType4>
        <OrgAssessmentOrder></OrgAssessmentOrder>
        <FormState>PD94bWwgdmVyc2lvbj0iMS4wIj8+DQo8Rm9ybUN1c3RvbWlzYXRpb25TZXR0aW5nc1N0b3JhZ2UgeG1sbnM6eHNkPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYSIgeG1sbnM6eHNpPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYS1pbnN0YW5jZSIgeG1sbnM9Imh0dHA6Ly93d3cuZWRpLmNvbS5hdS9FbnRlcnByaXNlU2VydmljZS8iPg0KICA8VGFiPg0KICAgIDxOYW1lPlNoaXBtZW50RGV0YWlsc1RhYlBhZ2U8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPkJhc2ljIFJlZ2lzdHJhdGlvbjwvRGVzY3JpcHRpb24+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgPC9UYWI+DQogIDxUYWI+DQogICAgPE5hbWU+QWRkaXRpb25hbFRhYlBhZ2U8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPkFkZGl0aW9uYWwgRGV0YWlsPC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICA8L1RhYj4NCiAgPFRhYj4NCiAgICA8TmFtZT5SZWxhdGVkU2hpcG1lbnRzVGFiUGFnZTwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+UmVsYXRlZCBTaGlwbWVudHM8L0Rlc2NyaXB0aW9uPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogIDwvVGFiPg0KICA8VGFiPg0KICAgIDxOYW1lPlJvdXRpbmdUYWJQYWdlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Sb3V0aW5nPC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICA8L1RhYj4NCiAgPFRhYj4NCiAgICA8TmFtZT5Db250YWluZXJEZXRhaWxzVGFiUGFnZTwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+UGFja2luZzwvRGVzY3JpcHRpb24+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgPC9UYWI+DQogIDxUYWI+DQogICAgPE5hbWU+UGlja3VwVGFiUGFnZTwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+UGlja3VwPC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICA8L1RhYj4NCiAgPFRhYj4NCiAgICA8TmFtZT5EZWxpdmVyeVRhYlBhZ2U8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPkRlbGl2ZXJ5PC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICA8L1RhYj4NCiAgPFRhYj4NCiAgICA8TmFtZT5Xb3JrZmxvd1RhYlBhZ2U8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPldvcmtmbG93ICZhbXA7IFRyYWNraW5nPC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICA8L1RhYj4NCiAgPFRhYj4NCiAgICA8TmFtZT5CaWxsaW5nVGFiUGFnZTwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+QmlsbGluZzwvRGVzY3JpcHRpb24+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgPC9UYWI+DQogIDxUYWI+DQogICAgPE5hbWU+QWRkcmVzc2VzVGFiUGFnZTwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+QWRkcmVzc2VzPC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICA8L1RhYj4NCiAgPFRhYj4NCiAgICA8TmFtZT5Ccm9rZXJhZ2VUYWJQYWdlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Ccm9rZXJhZ2U8L0Rlc2NyaXB0aW9uPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogIDwvVGFiPg0KICA8VGFiPg0KICAgIDxOYW1lPkRvY0RhdGFUYWJQYWdlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Eb2MgRGF0YTwvRGVzY3JpcHRpb24+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgPC9UYWI+DQogIDxUYWI+DQogICAgPE5hbWU+ZURvY3NUYWJQYWdlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5lRG9jczwvRGVzY3JpcHRpb24+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgPC9UYWI+DQogIDxUYWI+DQogICAgPE5hbWU+Tm90ZXNUYWJQYWdlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Ob3RlczwvRGVzY3JpcHRpb24+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgPC9UYWI+DQogIDxUYWI+DQogICAgPE5hbWU+RXZlbnRUYWJQYWdlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Mb2dzPC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICA8L1RhYj4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPkFkZGl0aW9uYWxUZXJtczwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+QWRkaXRpb25hbCBUZXJtczwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4xMjwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5BaXJ3YXlCaWxsRGltczwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+QWlyIFdheWJpbGwgRGltczwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4xODwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5BdmlhdGlvblNlY3VyaXR5PC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5BdmlhdGlvbiBTZWN1cml0eTwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4xNjwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5CaWxsRGV0YWlsczwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+QmlsbCBQcmludHMvSXNzdWUgRGF0ZTwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4yMzwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5DaGFyZ2VhYmxlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5DaGFyZ2VhYmxlPC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5CYXNpYyBSZWdpc3RyYXRpb248L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjU8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+Q2hhcmdlc0FwcGx5PC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5DaGFyZ2VzIEFwcGx5PC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5CYXNpYyBSZWdpc3RyYXRpb248L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjI0PC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPkNvbW11bml0eVRyYW5zaXRTdGF0dXM8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPkNvbW11bml0eSBUcmFuc2l0IFN0YXR1czwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4zMDwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5Db25zb2xzPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Db25zb2xpZGF0aW9uIERldGFpbHM8L0Rlc2NyaXB0aW9uPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QWRkaXRpb25hbCBEZXRhaWw8L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5MZWZ0IFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4wPC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPkNvbnRhaW5lck1vZGVPdmVycmlkZTwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+Q29udGFpbmVyIE1vZGUgT3ZlcnJpZGU8L0Rlc2NyaXB0aW9uPg0KICAgIDxHcm91cD5EZXRhaWxzPC9Hcm91cD4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50Pk1pZGRsZSBUb3A8L1BsYWNlbWVudD4NCiAgICA8UG9zaXRpb24+MjI8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+Q29udHJvbGxpbmdBZ2VudEFkZHJlc3M8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPkNvbnRyb2xsaW5nIEFnZW50PC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+ZmFsc2U8L1Zpc2libGU+DQogICAgPFBvc2l0aW9uPjA8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+Q29udHJvbGxpbmdDdXN0b21lckFkZHJlc3M8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPkNvbnRyb2xsaW5nIEN1c3RvbWVyPC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+ZmFsc2U8L1Zpc2libGU+DQogICAgPFBvc2l0aW9uPjA8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+Q3VzdG9tRmllbGRzPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5DdXN0b20gRmllbGRzPC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50PlJpZ2h0IFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4wPC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPkN1c3RvbXNFbnRyeU51bWJlcjwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+Q3VzdG9tcyBDbGVhcmFuY2UvUGVybWl0IE5vLjwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4xNDwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5FRnJlaWdodFN0YXR1czwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+ZS1mcmVpZ2h0IFN0YXR1czwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4zMjwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5Fc3RFeHBvcnRDdXN0b21zQ2xlYXJMYWJlbDwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+RXN0aW1hdGVkIEV4cG9ydCBDbGVhcmFuY2UgRGF0ZTwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPmZhbHNlPC9WaXNpYmxlPg0KICAgIDxQb3NpdGlvbj4wPC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPkV4cG9ydFN0YXRlbWVudDwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+RXhwb3J0ZXIgU3RhdGVtZW50PC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5CYXNpYyBSZWdpc3RyYXRpb248L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjI1PC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPkRlc2NyaXB0aW9uPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Hb29kcyBEZXNjcmlwdGlvbjwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj45PC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPkhvdXNlQmlsbDwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+SG91c2UgYmlsbCBOdW1iZXI8L0Rlc2NyaXB0aW9uPg0KICAgIDxHcm91cD5EZXRhaWxzPC9Hcm91cD4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50Pk1pZGRsZSBUb3A8L1BsYWNlbWVudD4NCiAgICA8UG9zaXRpb24+MDwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5Ib3VzZWJpbGxUeXBlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Ib3VzZSBiaWxsIFR5cGU8L0Rlc2NyaXB0aW9uPg0KICAgIDxHcm91cD5EZXRhaWxzPC9Hcm91cD4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50Pk1pZGRsZSBUb3A8L1BsYWNlbWVudD4NCiAgICA8UG9zaXRpb24+MTk8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+UGF5bWVudFRlcm08L05hbWU+DQogICAgPERlc2NyaXB0aW9uPklOQ08gLyBQYXltZW50IFRlcm08L0Rlc2NyaXB0aW9uPg0KICAgIDxHcm91cD5EZXRhaWxzPC9Hcm91cD4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50Pk1pZGRsZSBUb3A8L1BsYWNlbWVudD4NCiAgICA8UG9zaXRpb24+MTE8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+SVNGQmlsbFN0YXR1czwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+SVNGIEJpbGwgU3RhdHVzPC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5CYXNpYyBSZWdpc3RyYXRpb248L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjI5PC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPkxvYWRpbmdNZXRlcnM8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPkxvYWRpbmcgTWV0ZXJzPC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5CYXNpYyBSZWdpc3RyYXRpb248L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjQ8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+TWFya3NOdW1iZXJzPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5NYXJrcyAmYW1wOyBOdW1iZXJzPC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5CYXNpYyBSZWdpc3RyYXRpb248L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjEwPC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPk5vdGlmeVBhcnR5PC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Ob3RpZnkgUGFydHk8L0Rlc2NyaXB0aW9uPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+UmlnaHQgQm90dG9tPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjA8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+T25Cb2FyZDwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+T24gQm9hcmQgRGV0YWlsczwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4yMTwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5PcmRlckxpbmtzPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5PcmRlciBNYW5hZ2VtZW50PC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkFkZGl0aW9uYWwgRGV0YWlsPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+UmlnaHQgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjA8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+T3JkZXJVcGRhdGVDdXRPZmY8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPk9yZGVyIFVwZGF0ZSBDdXRvZmYgRGF0ZTwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPmZhbHNlPC9WaXNpYmxlPg0KICAgIDxQb3NpdGlvbj4wPC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPk9yaWdpbkRlc3RpbmF0aW9uRGF0ZXM8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPk9yaWdpbiwgRGVzdGluYXRpb24gYW5kIERhdGVzPC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5CYXNpYyBSZWdpc3RyYXRpb248L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjE8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+UGFja3NWYWx1ZXM8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPlBhY2thZ2VzIGFuZCBHb29kcyBWYWx1ZTwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj42PC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPlBoYXNlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5QaGFzZTwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4yODwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5SZWZlcmVuY2VOdW1iZXJzPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5SZWZlcmVuY2UgTnVtYmVyczwvRGVzY3JpcHRpb24+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5BZGRpdGlvbmFsIERldGFpbDwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50PlJpZ2h0IEJvdHRvbTwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4wPC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPlJlbGVhc2VUeXBlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5SZWxlYXNlIFR5cGU8L0Rlc2NyaXB0aW9uPg0KICAgIDxHcm91cD5EZXRhaWxzPC9Hcm91cD4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50Pk1pZGRsZSBUb3A8L1BsYWNlbWVudD4NCiAgICA8UG9zaXRpb24+MTc8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+U2NyZWVuaW5nU3RhdHVzPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5TY3JlZW5pbmcgU3RhdHVzPC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5CYXNpYyBSZWdpc3RyYXRpb248L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjI3PC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPlNlcnZpY2VMZXZlbDwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+U2VydmljZSBMZXZlbDwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4xMzwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5TZXJ2aWNlczwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+U2VydmljZXM8L0Rlc2NyaXB0aW9uPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QWRkaXRpb25hbCBEZXRhaWw8L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgQm90dG9tPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjA8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+U2hpcHBlckNPRDwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+U2hpcHBlciBDT0Q8L0Rlc2NyaXB0aW9uPg0KICAgIDxHcm91cD5EZXRhaWxzPC9Hcm91cD4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50Pk1pZGRsZSBUb3A8L1BsYWNlbWVudD4NCiAgICA8UG9zaXRpb24+MjY8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+RnJlaWdodFNwb3RSYXRlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5TcG90IFJhdGU8L0Rlc2NyaXB0aW9uPg0KICAgIDxHcm91cD5EZXRhaWxzPC9Hcm91cD4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50Pk1pZGRsZSBUb3A8L1BsYWNlbWVudD4NCiAgICA8UG9zaXRpb24+MzE8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+V2VpZ2h0Vm9sdW1lPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5XZWlnaHQvVm9sIENsaWVudC9DYXJyaWVyPC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkFkZGl0aW9uYWwgRGV0YWlsPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4wPC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPldlaWdodFZvbHVtZUNoYXJnZWFibGU8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPldlaWdodC9Wb2x1bWU8L0Rlc2NyaXB0aW9uPg0KICAgIDxHcm91cD5EZXRhaWxzPC9Hcm91cD4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50Pk1pZGRsZSBUb3A8L1BsYWNlbWVudD4NCiAgICA8UG9zaXRpb24+MzwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5GcmVpZ2h0UmF0ZXNBbmRHYXRld2F5czwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+RnJlaWdodCBSYXRlcyBhbmQgR2F0ZXdheXM8L0Rlc2NyaXB0aW9uPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QWRkaXRpb25hbCBEZXRhaWw8L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5MZWZ0IEJvdHRvbTwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4wPC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCjwvRm9ybUN1c3RvbWlzYXRpb25TZXR0aW5nc1N0b3JhZ2U+</FormState>
        <RespondToCascadedEvents>false</RespondToCascadedEvents>
        <RecalculateScheduledDate>true</RecalculateScheduledDate>
        <EffectiveStartDateUtc></EffectiveStartDateUtc>
        <TaskFallbackMethod>EFB</TaskFallbackMethod>
        <MilestoneFallbackMethod>EFB</MilestoneFallbackMethod>
        <TriggerFallbackMethod>EFB</TriggerFallbackMethod>
        <IsSystem>false</IsSystem>
        <IsActive>false</IsActive>
        <SubType5></SubType5>
        <SystemCreateTimeUtc>2016-01-15T15:09:00</SystemCreateTimeUtc>
        <SystemLastEditTimeUtc>2020-08-04T13:33:00</SystemLastEditTimeUtc>
        <IsPartialTemplate>false</IsPartialTemplate>
        <IsUniversal>false</IsUniversal>
        <EffectiveEndDateUtc></EffectiveEndDateUtc>
        <CustomFieldFallback>NFB</CustomFieldFallback>
        <Name>MY TEMPLATE</Name>
        <Description>Global shipment to various (ODS, uBase, eDC, D-Track)</Description>
        <ReleaseGroupFallbackMethod>EFB</ReleaseGroupFallbackMethod>
        <ProcessTasksCollection>
          <ProcessTasks Action=""MERGE"">
            <PK>a1657888-8844-4f2e-b92e-211ebe87de57</PK>
            <TaskID>T518559990</TaskID>
            <Sequence>2027</Sequence>
            <Type>TRG</Type>
            <Status>OPN</Status>
            <ScheduledDate></ScheduledDate>
            <EstDuration></EstDuration>
            <ActualDate></ActualDate>
            <ActualDuration></ActualDuration>
            <Notes></Notes>
            <MilestoneExceptionAdded></MilestoneExceptionAdded>
            <IsCalendarItem>false</IsCalendarItem>
            <SuspendedAt></SuspendedAt>
            <TotalSuspendedDuration></TotalSuspendedDuration>
            <NonWorkHours></NonWorkHours>
            <TaskCannotBeDeleted>false</TaskCannotBeDeleted>
            <Condition1></Condition1>
            <AndOr></AndOr>
            <Condition2>UDF</Condition2>
            <ReferencedID></ReferencedID>
            <IsPublished>true</IsPublished>
            <EstimatedDefaultedFrom></EstimatedDefaultedFrom>
            <EstimatedDefaultTimeDelta></EstimatedDefaultTimeDelta>
            <EstimatedDefaultFromPredecessor>0</EstimatedDefaultFromPredecessor>
            <TriggerField></TriggerField>
            <ParentTemplateID></ParentTemplateID>
            <ExceptionAddedUTC></ExceptionAddedUTC>
            <ScheduledDateUTC></ScheduledDateUTC>
            <ActualDateUTC></ActualDateUTC>
            <SuspendedAtUTC></SuspendedAtUTC>
            <Condition2Value>""&lt;GetDepartureCFSDocAddress.Organisation.OH_Code&gt;""==""6402432266""||""&lt;GetDepartureCFSDocAddress.Organisation.OH_Code&gt;""==""65521006""||""&lt;JS_JK_ReceivingAgent&gt;""==""6402432266""||""&lt;JS_JK_ReceivingAgent&gt;""==""6402432374""||""&lt;JS_JK_ReceivingAgent&gt;""==""6406259703""||""&lt;JS_JK_ReceivingAgent&gt;""==""6402432470""||""&lt;JS_JK_SendingAgent&gt;""==""6402432266""||""&lt;JS_JK_SendingAgent&gt;""==""6402432374""||""&lt;JS_JK_SendingAgent&gt;""==""6406259703""||""&lt;JS_JK_SendingAgent&gt;""==""6402432470""||""&lt;JS_JK_SendingAgent&gt;""==""6402432375""||""&lt;JS_RL_NKOrigin&gt;""==""USORD""||""&lt;JS_RL_NKOrigin&gt;""==""USCHI""||""&lt;Origin.CountryCode.Code&gt;""==""US""||""&lt;Destination.CountryCode.Code&gt;""==""US""||""&lt;Consols.JK_RL_NKDischargePort&gt;""==""USORD""</Condition2Value>
            <RespondToCascadedEvents>true</RespondToCascadedEvents>
            <CascadedEventsContext></CascadedEventsContext>
            <EstimateVariationFactor>2.00</EstimateVariationFactor>
            <EstimatedTimeToComplete></EstimatedTimeToComplete>
            <CompletedTimeUtc></CompletedTimeUtc>
            <OriginalScheduledDateUtc></OriginalScheduledDateUtc>
            <RecalculateScheduledDate>true</RecalculateScheduledDate>
            <IsInterruptable>false</IsInterruptable>
            <Description>C2C - Manual Resend</Description>
            <ReferencedTableCode></ReferencedTableCode>
            <CardNote>-06951432810000000656</CardNote>
            <LineTriggerType></LineTriggerType>
            <TriggerCondition></TriggerCondition>
            <EstimatedHandoverTimeUtc></EstimatedHandoverTimeUtc>
            <TriggerFiredCountdown>100</TriggerFiredCountdown>
            <PenetrationResetDateUtc></PenetrationResetDateUtc>
            <TriggerContext>DEF</TriggerContext>
            <ProcessTaskNotification_RegularTriggersCollection TableName=""ProcessTaskNotification"">
              <ProcessTaskNotification_RegularTriggers Action=""MERGE"">
                <PK>78e7c86a-c870-4d3b-a42a-334c7aca94d4</PK>
                <TriggerType>XUS</TriggerType>
                <TriggerParty>ORP</TriggerParty>
                <EmailText></EmailText>
                <MessagePurpose>C2C</MessagePurpose>
                <EmailAddr></EmailAddr>
                <SourceTemplateNotification></SourceTemplateNotification>
                <TriggerPartyService></TriggerPartyService>
                <Document TableName=""StmMenuItem"" />
                <StmPrintQueue />
                <Recipient TableName=""OrgHeader"">
                  <Code>ACCGRELAX</Code>
                  <PK>6016fbe9-a8a1-4561-b0a7-fbfcd8b317b4</PK>
                </Recipient>
                <WorkflowTemplate TableName=""ProcessTaskTemplate"" />
                <Trigger TableName=""ProcessTemplateTrigger"" />
              </ProcessTaskNotification_RegularTriggers>
            </ProcessTaskNotification_RegularTriggersCollection>
            <GlbCapability />
            <GlbStaff />
            <AssignedGroup TableName=""GlbGroup"" />
            <OrgAddress />
            <OrgContact />
            <MilestoneEvent TableName=""StmEvent"">
              <Code>Z47</Code>
              <PK>05487041-bf4d-4d23-aa4b-56c7f8ef5cf3</PK>
            </MilestoneEvent>
            <TaskCompletionEvent TableName=""StmEvent"" />
            <ExceptionEvent TableName=""StmEvent"" />
            <OriginCountry TableName=""RefCountry"" />
            <DestinationCountry TableName=""RefCountry"" />
            <GlbCompany />
            <ProcessHeader />
            <TriggerBranch TableName=""GlbBranch"" />
            <TriggerDepartment TableName=""GlbDepartment"" />
          </ProcessTasks>
        </ProcessTasksCollection>
        <OrgAddress />
        <OrgHeader />
        <WhsWarehouse />
        <GlbCompany />
        <GlbBranch />
        <GlbDepartment />
        <BufferManagementSystem TableName=""BMSystem"" />
      </ProcessTaskTemplate>
    </WorkflowTemplate>
  </Body>
</Native>";
			#endregion

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ACCGRELAX";
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "DAU";
			Factory.Save();

			var message1 = Factory.New<Messaging.Integration.IEDIMessage>();
			message1.EM_MessageText = nativeXML;

			var logger = new TestErrorLogger();
			var processor = new NativeDataMessageProcessor(logger);
			processor.Process(message1);

			AssertMultilineASCIIEquals("Log Messages after processor.Process(message)", @"
Information - ProcessTaskTemplate - 1 inserts, 0 updates, 0 deletes
Information - ProcessTasks - 1 inserts, 0 updates, 0 deletes
Information - ProcessTaskNotification_RegularTriggers - 1 inserts, 0 updates, 0 deletes
Information - TemplateConditionNote - 1 inserts, 0 updates, 0 deletes
Information - --------------------------------------------------------------------------------
Information - Imported: WorkflowTemplate".Trim(), logger.Logs.Trim());

			var template = new BusinessObjectFactory().Load<ProcessTaskTemplate>(new ZQuery(ProcessTaskTemplateSchema.P0_Name, "MY TEMPLATE")).First();
			AssertEquals(1, template.WorkflowItems.Count);
			AssertEquals("\"<GetDepartureCFSDocAddress.Organisation.OH_Code>\"==\"6402432266\"||\"<GetDepartureCFSDocAddress.Organisation.OH_Code>\"==\"65521006\"||\"<JS_JK_ReceivingAgent>\"==\"6402432266\"||\"<JS_JK_ReceivingAgent>\"==\"6402432374\"||\"<JS_JK_ReceivingAgent>\"==\"6406259703\"||\"<JS_JK_ReceivingAgent>\"==\"6402432470\"||\"<JS_JK_SendingAgent>\"==\"6402432266\"||\"<JS_JK_SendingAgent>\"==\"6402432374\"||\"<JS_JK_SendingAgent>\"==\"6406259703\"||\"<JS_JK_SendingAgent>\"==\"6402432470\"||\"<JS_JK_SendingAgent>\"==\"6402432375\"||\"<JS_RL_NKOrigin>\"==\"USORD\"||\"<JS_RL_NKOrigin>\"==\"USCHI\"||\"<Origin.CountryCode.Code>\"==\"US\"||\"<Destination.CountryCode.Code>\"==\"US\"||\"<Consols.JK_RL_NKDischargePort>\"==\"USORD\"", template.WorkflowItems[0].P9_Condition2Value);
		}

		public void TestExportAndImportTemplate_WithTMPCompletionTriggerAction_ShouldMatchByName()
		{
			var partialTemplate = Factory.New<ProcessTaskTemplate>();
			partialTemplate.P0_ProcessType = "WKI";
			partialTemplate.P0_Name = "Partial";

			var normalTemplate = Factory.New<ProcessTaskTemplate>();
			normalTemplate.P0_ProcessType = "WKI";
			normalTemplate.P0_Name = "Normal";

			var trigger = normalTemplate.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = "TAG";

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			triggerAction.PQ_P0_WorkflowTemplate = partialTemplate.PK;

			Factory.Save();

			using (var template1Stream = NativeDataTransferTestHelper.ExportToStream(partialTemplate))
			using (var template2Stream = NativeDataTransferTestHelper.ExportToStream(normalTemplate))
			{
				partialTemplate.Delete();
				normalTemplate.Delete();

				Factory.Save();

				var log1 = NativeDataTransferTestHelper.ImportAndGetInsertLog(template1Stream);
				var log2 = NativeDataTransferTestHelper.ImportAndGetInsertLog(template2Stream);

				AssertMultilineASCIIEquals("",
@"--- Start Import Process --------------------------------------------------------------
Processed: WorkflowTemplate
--- Import Process Finished -----------------------------------------------------------
ProcessTaskTemplate - 1 inserts, 0 updates, 0 deletes", log1);

				AssertMultilineASCIIEquals("",
@"--- Start Import Process --------------------------------------------------------------
Processed: WorkflowTemplate
--- Import Process Finished -----------------------------------------------------------
ProcessTaskTemplate - 1 inserts, 0 updates, 0 deletes
ProcessTasks - 1 inserts, 0 updates, 0 deletes
ProcessTaskNotification_RegularTriggers - 1 inserts, 0 updates, 0 deletes", log2);

				var newFactory = Factory.CreateNewFactory();

				var loadedTemplate1 = newFactory.Load<ProcessTaskTemplate>(new ZQuery(ProcessTaskTemplateSchema.P0_Name, "Partial")).SingleOrDefault();
				var loadedTemplate2 = newFactory.Load<ProcessTaskTemplate>(new ZQuery(ProcessTaskTemplateSchema.P0_Name, "Normal")).SingleOrDefault();

				AssertNotNull(loadedTemplate1);
				AssertNotNull(loadedTemplate2);

				AssertEquals(1, loadedTemplate2.WorkflowItems.Triggers.Count);
				AssertEquals(1, loadedTemplate2.WorkflowItems.Triggers[0].ProcessTaskNotifications.Count);

				var loadedTriggerAction = loadedTemplate2.WorkflowItems.Triggers[0].ProcessTaskNotifications[0];

				AssertEquals(WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce, loadedTriggerAction.PQ_TriggerType);
				AssertEquals(loadedTemplate1.PK, loadedTriggerAction.PQ_P0_WorkflowTemplate);
			}
		}

		public void TestExportWorkflowTemplateIncludesCustomFields()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_Name = "ONE SMALL TEMPLATE FOR MAN";
			template.P0_ProcessType = "ORG";

			var customColumn1 = template.GenCustomColumnDefinitions.AddNew();
			customColumn1.XC_DisplaySequence = 1;
			customColumn1.XC_Name = "COLOUR";
			customColumn1.XC_Type = "STR";

			var customColumn2 = template.GenCustomColumnDefinitions.AddNew();
			customColumn2.XC_DisplaySequence = 2;
			customColumn2.XC_Name = "STYLE";
			customColumn2.XC_Type = "STR";

			Factory.Save();

			using (var baseStream = NativeDataTransferTestHelper.ExportToStream(template))
			{
				template.Delete();
				customColumn1.Delete();
				customColumn2.Delete();
				Factory.Save();

				var insertLog = NativeDataTransferTestHelper.ImportAndGetInsertLog(baseStream);
				AssertMultilineASCIIEquals("We Exported, so now we should be able to Import and see GenCustomColumnDefinitions coming in.", @"
--- Start Import Process --------------------------------------------------------------
Processed: WorkflowTemplate
--- Import Process Finished -----------------------------------------------------------
ProcessTaskTemplate - 1 inserts, 0 updates, 0 deletes
ProcessJobHeader - 1 inserts, 0 updates, 0 deletes
GenCustomColumnDefinition - 2 inserts, 0 updates, 0 deletes
				".Trim(), insertLog);
			}

			var query = new ZQuery(ProcessTaskTemplateSchema.P0_Name, "ONE SMALL TEMPLATE FOR MAN");
			var loadedProcessTaskTemplate = Factory.Load<ProcessTaskTemplate>(query).Single();
			AssertMultilineASCIIEquals("loadedProcessTaskTemplate.GenCustomColumnDefinitions", @"
1-|-COLOUR-|-STR-|-
2-|-STYLE-|-STR-|-
".Trim(), string.Join("\r\n", loadedProcessTaskTemplate.GenCustomColumnDefinitions.OrderBy(o => o.XC_DisplaySequence).Select(o => $"{o.XC_DisplaySequence}-|-{o.XC_Name}-|-{o.XC_Type}-|-{o.CustomAddOnRule?.XR_Code}")));
		}

		public void TestImportWorkflowTemplateImportsCustomFields()
		{
			var rule = Factory.New<GenCustomAddOnRule>();
			rule.XR_Code = "RULE1";
			rule.XR_Description = "Rule No 1";

			rule = Factory.New<GenCustomAddOnRule>();
			rule.XR_Code = "RULE2";
			rule.XR_Description = "Rule No 2";

			Factory.Save();

			using (var stream = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.ConcreteUnitTesting.Testing.Workflow.WorkflowTemplate_CustomFields.xml")))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream.BaseStream);
				var insertLog = manager.GetLogs();
				AssertMultilineASCIIEquals("Insert Log Text", @"
--- Start Import Process --------------------------------------------------------------
Processed: WorkflowTemplate
--- Import Process Finished -----------------------------------------------------------
ProcessTaskTemplate - 1 inserts, 0 updates, 0 deletes
GenCustomColumnDefinition - 6 inserts, 0 updates, 0 deletes
				".Trim(), insertLog);
			}

			var query = new ZQuery(ProcessTaskTemplateSchema.P0_Name, "Consol Air Fair");
			var loadedProcessTaskTemplate = Factory.Load<ProcessTaskTemplate>(query).Single();
			AssertMultilineASCIIEquals("loadedProcessTaskTemplate.GenCustomColumnDefinitions", @"
1-|-Unsigned-|-INT-|-
2-|-Binary-|-BOO-|-
3-|-Chicken-|-CBO-|-RULE1
4-|-First-|-DAT-|-
5-|-Dewey-|-DEC-|-
6-|-Silly-|-STR-|-RULE2
".Trim(), string.Join("\r\n", loadedProcessTaskTemplate.GenCustomColumnDefinitions.OrderBy(o => o.XC_DisplaySequence).Select(o => $"{o.XC_DisplaySequence}-|-{o.XC_Name}-|-{o.XC_Type}-|-{o.CustomAddOnRule?.XR_Code}")));

			using (var stream = new StreamReader(GetType().Assembly.GetManifestResourceStream("Enterprise.DataTransfer.Native.ConcreteUnitTesting.Testing.Workflow.WorkflowTemplate_CustomFields.xml")))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream.BaseStream);
				var insertLog = manager.GetLogs();
				AssertMultilineASCIIEquals("Should find taskTemplate", @"
--- Start Import Process --------------------------------------------------------------
Processed: WorkflowTemplate
--- Import Process Finished -----------------------------------------------------------
ProcessTaskTemplate - 0 inserts, 0 updates, 0 deletes
GenCustomColumnDefinition - 0 inserts, 0 updates, 0 deletes
				".Trim(), insertLog);
			}

			var reloadedProcessTaskTemplate = new BusinessObjectFactory().Load<ProcessTaskTemplate>(query).Single();
			AssertMultilineASCIIEquals("reloadedProcessTaskTemplate.GenCustomColumnDefinitions", @"
1-|-Unsigned-|-INT-|-
2-|-Binary-|-BOO-|-
3-|-Chicken-|-CBO-|-RULE1
4-|-First-|-DAT-|-
5-|-Dewey-|-DEC-|-
6-|-Silly-|-STR-|-RULE2
".Trim(), string.Join("\r\n", reloadedProcessTaskTemplate.GenCustomColumnDefinitions.OrderBy(o => o.XC_DisplaySequence).Select(o => $"{o.XC_DisplaySequence}-|-{o.XC_Name}-|-{o.XC_Type}-|-{o.CustomAddOnRule?.XR_Code}")));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
			_ = TestConfigsHelper.CreateSchematicTestConfig(Factory);
		}

		#endregion
	}
}
