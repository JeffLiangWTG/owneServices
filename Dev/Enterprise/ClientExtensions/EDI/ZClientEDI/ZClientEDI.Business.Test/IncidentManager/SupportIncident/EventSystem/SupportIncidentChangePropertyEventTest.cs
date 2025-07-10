using System;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class SupportIncidentChangePropertyEventTest : IncidentEventTestCase
	{
		public override void TestTrigger()
		{
			CreateWorkflowTemplate();

			var areas = new CodeDescriptionPairList();
			areas.AddPair("ARC", "ARC");
			areas.AddPair("INT", "INT");
			areas.AddPair("XRM", "XRM");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, areas);

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("SAA", "Module ARC", "ARC", false);
			product.ModuleMappings.AddNew("SBB", "Module INT", "INT", false);
			product.ModuleMappings.AddNew("SCC", "Module XRM", "XRM", false);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = "ENT";
			incident.ProductArea = "ARC";
			incident.IM_Module = "SAA";
			incident.IM_SourceModuleId = "ABC";
			incident.IM_Priority = "CR6";
			incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			incident.IM_RN_NKCountry = "AU";
			incident.IM_Language = "EN";

			var incidentEvent = GetNewEventForTest(incident);
			Factory.Save();

			AssertEquals(2, incident.WorkflowItems.Count);
			AssertEquals("ARC Task 1", incident.WorkflowItems[0].P9_Description);
			AssertEquals("ARC Task 2", incident.WorkflowItems[1].P9_Description);
			AssertEquals(10, incident.WorkflowItems[0].P9_Sequence);
			AssertEquals(20, incident.WorkflowItems[1].P9_Sequence);
			AssertEquals("Level 1 Support", incident.WorkflowItems[0].ProcessHeader.FH_CompletionStatement);
			AssertEquals("Level 1 Support", incident.WorkflowItems[1].ProcessHeader.FH_CompletionStatement);

			incident.IM_Module = "SBB";
			incident.ProductArea = "INT";
			incidentEvent.Trigger();
			Factory.Save();
			AssertEquals(4, incident.WorkflowItems.Count);
			AssertEquals("INT Task 1", incident.WorkflowItems[2].P9_Description);
			AssertEquals("INT Task 2", incident.WorkflowItems[3].P9_Description);
			AssertEquals(30, incident.WorkflowItems[2].P9_Sequence);
			AssertEquals(40, incident.WorkflowItems[3].P9_Sequence);
			AssertEquals("Level 1 Support", incident.WorkflowItems[2].ProcessHeader.FH_CompletionStatement);
			AssertEquals("Level 1 Support INT", incident.WorkflowItems[3].ProcessHeader.FH_CompletionStatement);

			incident.IM_Module = "SAA";
			incident.ProductArea = "ARC";
			incidentEvent.Trigger();
			Factory.Save();
			AssertEquals(6, incident.WorkflowItems.Count);
			AssertEquals("ARC Task 1", incident.WorkflowItems[4].P9_Description);
			AssertEquals("ARC Task 2", incident.WorkflowItems[5].P9_Description);
			AssertEquals(50, incident.WorkflowItems[4].P9_Sequence);
			AssertEquals(60, incident.WorkflowItems[5].P9_Sequence);
			AssertEquals("Level 1 Support", incident.WorkflowItems[4].ProcessHeader.FH_CompletionStatement);
			AssertEquals("Level 1 Support", incident.WorkflowItems[5].ProcessHeader.FH_CompletionStatement);

			incident.IM_Module = "SCC";
			incident.ProductArea = "XRM";
			incidentEvent.Trigger();
			Factory.Save();
			AssertEquals(6, incident.WorkflowItems.Count);

			incident.WorkflowItems.RemoveAndDeleteAll();
			incident.IM_Module = "SAA";
			incident.ProductArea = "ARC";
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(incident);
			incidentEvent.Trigger();
			Factory.Save();
			AssertEquals(2, incident.WorkflowItems.Count);
			AssertEquals("ARC Task 1", incident.WorkflowItems[0].P9_Description);
			AssertEquals("ARC Task 2", incident.WorkflowItems[1].P9_Description);
		}

		public override void TestTrigger_DeleteUnsavedEventTasks()
		{
			CreateWorkflowTemplate();

			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.ProductArea = "ARC";
			var incidentEvent = GetNewEventForTest(incident);
			Factory.Save();

			AssertEquals(2, incident.WorkflowItems.Count);
			AssertEquals("ARC Task 1", incident.WorkflowItems[0].P9_Description);
			AssertEquals("ARC Task 2", incident.WorkflowItems[1].P9_Description);
			AssertEquals(10, incident.WorkflowItems[0].P9_Sequence);
			AssertEquals(20, incident.WorkflowItems[1].P9_Sequence);
			AssertEquals("Level 1 Support", incident.WorkflowItems[0].ProcessHeader.FH_CompletionStatement);
			AssertEquals("Level 1 Support", incident.WorkflowItems[1].ProcessHeader.FH_CompletionStatement);

			var jobHeader = ProcessJobHeaderProvider.GetForParent(incident, incident.Factory);
			AssertEquals(1, jobHeader.ProcessHeaders.Count);

			incident.ProductArea = "INT";
			incidentEvent.Trigger();
			AssertEquals(4, incident.WorkflowItems.Count);
			AssertEquals("INT Task 1", incident.WorkflowItems[2].P9_Description);
			AssertEquals("INT Task 2", incident.WorkflowItems[3].P9_Description);
			AssertEquals(30, incident.WorkflowItems[2].P9_Sequence);
			AssertEquals(40, incident.WorkflowItems[3].P9_Sequence);
			AssertEquals("Level 1 Support", incident.WorkflowItems[2].ProcessHeader.FH_CompletionStatement);
			AssertEquals("Level 1 Support INT", incident.WorkflowItems[3].ProcessHeader.FH_CompletionStatement);
			var intProcessHeaderPk = incident.WorkflowItems[3].ProcessHeader.PK;
			AssertEquals(2, jobHeader.ProcessHeaders.Count);
			AssertEquals(true, jobHeader.ProcessHeaders.Contains(intProcessHeaderPk));

			incident.ProductArea = "ARC";
			incidentEvent.Trigger();
			AssertEquals(4, incident.WorkflowItems.Count);
			AssertEquals("ARC Task 1", incident.WorkflowItems[2].P9_Description);
			AssertEquals("ARC Task 2", incident.WorkflowItems[3].P9_Description);
			AssertEquals(30, incident.WorkflowItems[2].P9_Sequence);
			AssertEquals(40, incident.WorkflowItems[3].P9_Sequence);
			AssertEquals("Level 1 Support", incident.WorkflowItems[2].ProcessHeader.FH_CompletionStatement);
			AssertEquals("Level 1 Support", incident.WorkflowItems[3].ProcessHeader.FH_CompletionStatement);

			AssertEquals(1, jobHeader.ProcessHeaders.Count);
			AssertEquals("Process header should be deleted", false, jobHeader.ProcessHeaders.Contains(intProcessHeaderPk));
		}

		public void TestTrigger_SuspendStatusAndDispositionCalculationWhenApplyingTemplate()
		{
			var incident = GetNewIncidentForTest() as SupportIncident;
			var existingTask1 = incident.WorkflowItems.AddNew();
			existingTask1.P9_Sequence = 10;
			existingTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			var existingTask2 = incident.WorkflowItems.AddNew();
			existingTask2.P9_Sequence = 20;
			existingTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var incidentEvent = GetNewEventForTest(incident);

			Factory.Save();

			incident.OnCloseIncident += (s, e) => { Assert("The on closing incident event handler should not be called when triggering event", false); };

			incidentEvent.Trigger();
			AssertEquals("No new tasks because template is not set up", 2, incident.WorkflowItems.Count);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, incident.WorkflowItems[0].P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, incident.WorkflowItems[1].P9_Status);
			AssertEquals("Incident status is calculated after event is completed", SupportIncidentLookups.Status.Closed, incident.IM_Status);
		}

		public void TestTrigger_NotAppliedIfSameTasksExist()
		{
			var areas = new CodeDescriptionPairList();
			areas.AddPair("ARC", "Architecture");
			areas.AddPair("INT", "International Logistics");
			areas.AddPair("XRM", "XRMs");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, areas);

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("SAA", "Super Module A", "ARC", false);
			product.ModuleMappings.AddNew("SBB", "Super Module B", "INT", false);
			product.ModuleMappings.AddNew("SCC", "Super Module C", "XRM", false);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			#region Template 1

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "INC";
			template1.P0_SubType1 = ProductTypes.Codes.Enterprise;

			var header1 = template1.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "Level 1 Support";

			var task11 = template1.WorkflowItems.AddNew();
			task11.P9_FH_ProcessHeader = header1.PK;
			task11.P9_Sequence = 10;
			task11.P9_Description = "MAA Task 1";
			task11.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task11.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			task11.TemplateConditions.TemplateCondition2Value = @"""<IM_SourceModuleId>"" == ""MAA""";

			var task12 = template1.WorkflowItems.AddNew();
			task12.P9_FH_ProcessHeader = header1.PK;
			task12.P9_Sequence = 10;
			task12.P9_Description = "MBB Task 1";
			task12.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task12.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			task12.TemplateConditions.TemplateCondition2Value = @"""<IM_SourceModuleId>"" == ""MBB""";

			#endregion

			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "sam@test.inc";
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_OH_Client = org.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.ProductArea = "ARC";
			incident.IM_Module = "SAA";
			incident.IM_SourceModuleId = "MAA";
			Factory.Save();

			AssertEquals(1, incident.WorkflowItems.Count);
			AssertEquals("MAA Task 1", incident.WorkflowItems[0].P9_Description);
			AssertEquals(10, incident.WorkflowItems[0].P9_Sequence);

			var incidentEvent = GetNewEventForTest(incident);
			incident.IM_SourceModuleId = "MBB";
			incidentEvent.Trigger();

			AssertEquals("Task from best matched template is added", 2, incident.WorkflowItems.Count);
			AssertEquals("MBB Task 1", incident.WorkflowItems[1].P9_Description);
			AssertEquals(20, incident.WorkflowItems[1].P9_Sequence);

			incidentEvent.Trigger();
			AssertEquals("No new task as best matched template has applied", 2, incident.WorkflowItems.Count);
		}

		void CreateWorkflowTemplate()
		{
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = IncidentTemplateType;
			template1.P0_SubType4 = "ARC";

			var header1 = template1.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "Level 1 Support";

			var task11 = template1.WorkflowItems.AddNew();
			task11.P9_FH_ProcessHeader = header1.PK;
			task11.P9_Sequence = 10;
			task11.P9_Description = "ARC Task 1";
			task11.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var task12 = template1.WorkflowItems.AddNew();
			task12.P9_FH_ProcessHeader = header1.PK;
			task12.P9_Sequence = 20;
			task12.P9_Description = "ARC Task 2";
			task12.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = IncidentTemplateType;
			template2.P0_SubType4 = "INT";

			var header21 = template2.ProcessHeaders.AddNew();
			header21.FH_CompletionStatement = "Level 1 Support";
			var header22 = template2.ProcessHeaders.AddNew();
			header22.FH_CompletionStatement = "Level 1 Support INT";

			var task21 = template2.WorkflowItems.AddNew();
			task21.P9_FH_ProcessHeader = header21.PK;
			task21.P9_Sequence = 10;
			task21.P9_Description = "INT Task 1";
			task21.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var task22 = template2.WorkflowItems.AddNew();
			task22.P9_FH_ProcessHeader = header22.PK;
			task22.P9_Sequence = 20;
			task22.P9_Description = "INT Task 2";
			task22.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			Factory.Save();
		}

		protected override IIncidentEvent GetNewEventForTest(IIncidentEventConsumer incident)
		{
			return new SupportIncidentChangePropertyEvent((SupportIncident)incident);
		}

		protected override IIncidentEventConsumer GetNewIncidentForTest()
		{
			return Factory.NewWithValidTestData<SupportIncident>();
		}
	}
}
