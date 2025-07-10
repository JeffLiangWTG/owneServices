using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	class EDIProcessTasksTypeDeciderTest : ProcessTasksTypeDeciderTest
	{
		public void TestGetTypeForLoad_ProfessionalServicesQuote_DontHitTheDatabaseOverAndOver()
		{
			var jobs = new List<ProfessionalServicesQuote>();
			for (int i = 0; i < 5; i++)
			{
				var job = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
				var task = job.WorkflowItems.Tasks.AddNew();
				jobs.Add(job);
			}

			Factory.Save();
			var newFactory = new BusinessObjectFactory();

			var incidentMainsLoaded = 0;
			newFactory.Loaded += (s, e) => incidentMainsLoaded += e.NewObjects.OfType<IncidentMainBase>().Count();
			var loadedTasks = newFactory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.PK, jobs.SelectMany(w => w.WorkflowItems.Tasks).Select(t => t.PK)));

			AssertEquals("ProcessTask loaded with the correct type", typeof(PSQuoteProcessTask), loadedTasks[0].GetType());

			var expectedHits = new Dictionary<string, int>
			{
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ IncidentMainSchema.Constants.TableName, 1 },
			};

			AssertDbHits(expectedHits, newFactory);
			AssertEquals("Dont load anything except tasks", 0, incidentMainsLoaded);
		}

		public void TestGetTypeForLoad_ForNewWorkItemTask()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			ProcessTask task = workItem.WorkflowItems.Tasks.AddNew();
			AssertEquals("ParentTableCode", WorkItemSchema.Constants.Prefix, task.P9_ParentTableCode);
			Factory.Save();
			ProcessTask loadedTask = new BusinessObjectFactory().Load<ProcessTask>(task.PK);
			AssertEquals("ProcessTask loaded with the correct type", typeof(WorkItemProcessTask), loadedTask.GetType());
		}

		public void TestGetTypeForLoad_ForNewWorkItemMilestone()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			ProcessTask milestone = workItem.WorkflowItems.Milestones.AddNew();
			AssertEquals("P9_ParentTableCode", WorkItemSchema.Constants.Prefix, milestone.P9_ParentTableCode);
			Factory.Save();
			ProcessTask loadedMilestone = new BusinessObjectFactory().Load<ProcessTask>(milestone.PK);
			AssertEquals("ProcessTask loaded with the correct type", typeof(WorkItemProcessTask), loadedMilestone.GetType());
		}

		public void TestGetTypeForLoad_ForPSQTask()
		{
			ProfessionalServicesQuote psq = Factory.New<ProfessionalServicesQuote>();
			ProcessTask task = psq.WorkflowItems.Tasks.AddNew();
			AssertEquals("Parent of the task is an IncidentMain", IncidentMainSchema.Constants.Prefix, task.P9_ParentTableCode);
			Factory.Save();
			ProcessTask loadedTask = new BusinessObjectFactory().Load<ProcessTask>(task.PK);
			AssertEquals("ProcessTask loaded with the correct type", typeof(PSQuoteProcessTask), loadedTask.GetType());
		}

		public void TestGetTypeForLoad_ForPSQMilestone()
		{
			ProfessionalServicesQuote psq = Factory.New<ProfessionalServicesQuote>();
			ProcessTask milestone = psq.WorkflowItems.Milestones.AddNew();
			AssertEquals("Parent of the milestone is the professional service quote", IncidentMainSchema.Constants.Prefix, milestone.P9_ParentTableCode);
			Factory.Save();
			ProcessTask loadedMilestone = new BusinessObjectFactory().Load<ProcessTask>(milestone.PK);
			AssertEquals("ProcessTask loaded with the correct type", typeof(PSQuoteProcessTask), loadedMilestone.GetType());
		}

		public void TestGetTypeForLoad_ForSupportIncidentTask()
		{
			SupportIncident supportIncident = Factory.New<SupportIncident>();
			ProcessTask task = supportIncident.WorkflowItems.Tasks.AddNew();
			AssertEquals("Parent of the task is an IncidentMain", IncidentMainSchema.Constants.Prefix, task.P9_ParentTableCode);
			Factory.Save();
			ProcessTask loadedTask = new BusinessObjectFactory().Load<ProcessTask>(task.PK);
			AssertEquals("ProcessTask loaded with the correct type", typeof(SupportIncidentProcessTask), loadedTask.GetType());
		}

		public void TestGetTypeForLoad_ForSupportIncidentMilestone()
		{
			SupportIncident supportIncident = Factory.New<SupportIncident>();
			ProcessTask milestone = supportIncident.WorkflowItems.Milestones.AddNew();
			AssertEquals("Parent of the task is an IncidentMain", IncidentMainSchema.Constants.Prefix, milestone.P9_ParentTableCode);
			Factory.Save();
			ProcessTask loadedMilestone = new BusinessObjectFactory().Load<ProcessTask>(milestone.PK);
			AssertEquals("ProcessTask loaded with the correct type", typeof(SupportIncidentProcessTask), loadedMilestone.GetType());
		}

		public void TestGetQueryForLoad_IncidentMain()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			ProcessTask incidentTask = incident.WorkflowItems.AddNew();
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			ProcessTask workItemTask = workItem.WorkflowItems.AddNew();
			ProfessionalServicesQuote quote = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			ProcessTask quoteTask = quote.WorkflowItems.AddNew();
			Factory.Save();

			AssertProcessTasksLoaded(EDIJobInvoicingConsumerTypes.Incident.Code, incidentTask);
			AssertProcessTasksLoaded(EDIJobInvoicingConsumerTypes.PSQuote.Code, quoteTask);
		}

		public void TestGetTypeForLoad_ShouldNotLoadParentUnneccesarily()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var task = incident.WorkflowItems.Tasks.AddNew();

			Factory.Save();

			var supportIncidentInstantiationCount = 0;
			var newFactory = Factory.CreateNewFactory();
			newFactory.Loaded += (s, e) => supportIncidentInstantiationCount += e.NewObjects.OfType<SupportIncident>().Count();
			var loadedTask = newFactory.Load<ProcessTask>(task.PK);

			AssertType<SupportIncidentProcessTask>(loadedTask);
			AssertEquals("Should be no instantiated incidents in local cache", 0, supportIncidentInstantiationCount);
		}

		void AssertProcessTasksLoaded(string workflowTypeCode, params ProcessTask[] expectedTasks)
		{
			ProcessTask[] processTasks = GetProcessTasksForTestGetQueryForLoad(workflowTypeCode);
			AssertEquals(expectedTasks.Length, processTasks.Length);
			foreach (ProcessTask expectedTask in expectedTasks)
			{
				AssertCollectionContains(expectedTask, processTasks);
			}
		}

		ProcessTask[] GetProcessTasksForTestGetQueryForLoad(string code)
		{
			WorkflowDescriptor descriptor = WorkflowDescriptors.Instance.TryGetValueSafe(code);
			EDIProcessTaskTypeDecider typeDecider = new EDIProcessTaskTypeDecider();
			return Factory.Load<ProcessTask>(typeDecider.GetQueryForLoad(descriptor));
		}
	}
}
