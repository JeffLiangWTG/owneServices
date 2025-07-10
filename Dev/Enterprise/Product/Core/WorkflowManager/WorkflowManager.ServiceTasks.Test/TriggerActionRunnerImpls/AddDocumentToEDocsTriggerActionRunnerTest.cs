using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.WorkflowManager.ServiceTasks.Testing
{
	sealed class AddDocumentToEDocsTriggerActionRunnerTest : TestCaseWithFactory
	{
		public void TestRun()
		{
			IWorkflowProvider workflowProvider = (IWorkflowProvider)Factory.New<Integration.Forwarding.IForwardingShipment>();
			AssertNotNull("Precondition: workflow provider is IDocumentSupportable", workflowProvider as IDocumentSupportable);

			ProcessTask trigger = workflowProvider.WorkflowItems.Triggers.AddNew();

			ProcessTaskNotification action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SU_Document = DocumentCommand.PK;

			Factory.Save();

			IStmPrintJob[] printJobs = Factory.Load<IStmPrintJob>(new ZQuery());
			AssertEquals("Precondition: no documents delivered", 0, printJobs.Length);

			new AddDocumentToEDocsTriggerActionRunner(action, (BusinessObject)workflowProvider).Process(null);

			printJobs = Factory.Load<IStmPrintJob>(new ZQuery());
			AssertEquals("1 document delivered", 1, printJobs.Length);

			BusinessObject printJob = printJobs[0] as BusinessObject;
			AssertEquals("Parent", workflowProvider.PK, printJob[StmPrintJobSchema.SP_ParentGuid]);
			AssertEquals("Document", DocumentCommand.SU_MenuName + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling, printJob[StmPrintJobSchema.SP_DocumentName]);
			AssertEquals("JobType should be DDS - 'Doc Manager'", "DDS", printJob[StmPrintJobSchema.SP_JobType]);

			var newFactory = Factory.CreateNewFactory();
			var deliveryGroups = newFactory.Load<StmDeliveryGroup>(new ZQuery());
			AssertEquals("No deliveryGroups in database because it is not save.", 0, deliveryGroups.Length);
		}

		public void TestRun_SetTemporaryUserContext()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			IWorkflowProvider workflowProvider = (IWorkflowProvider)Factory.New<Integration.Forwarding.IForwardingShipment>();
			AssertNotNull("Precondition: workflow provider is IDocumentSupportable", workflowProvider as IDocumentSupportable);

			ProcessTask trigger = workflowProvider.WorkflowItems.Triggers.AddNew();

			ProcessTaskNotification action = trigger.ProcessTaskNotifications.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs;
			action.PQ_SU_Document = DocumentCommand.PK;

			workflowProvider.Logs.AddNew(Events.CustomisableEvent00);
			var log = trigger.Logs.Find(l => l.SL_SE_NKEvent == Events.WorkflowTriggerEventCode).Single();

			Factory.Save();

			IStmPrintJob[] printJobs = Factory.Load<IStmPrintJob>(new ZQuery());
			AssertEquals("Precondition: no documents delivered", 0, printJobs.Length);

			var queuedLog = new QueuedLogForTesting(log, trigger);

			using (Env.SetTemporaryUserContext(staff1.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				WorkflowServiceTaskTestHelper.RunLogWalker();
			}

			printJobs = Factory.Load<IStmPrintJob>(new ZQuery());
			AssertEquals("1 document delivered", 1, printJobs.Length);

			BusinessObject printJob = printJobs[0] as BusinessObject;
			AssertEquals("Parent", workflowProvider.PK, printJob[StmPrintJobSchema.SP_ParentGuid]);
			AssertEquals("Document", DocumentCommand.SU_MenuName + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling, printJob[StmPrintJobSchema.SP_DocumentName]);
			AssertEquals("JobType should be DDS - 'Doc Manager'", "DDS", printJob[StmPrintJobSchema.SP_JobType]);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, printJob[StmPrintJobSchema.SP_GS_NKJobSubmittedBy]);
		}

		#region Implementation

		IDocumentCommand DocumentCommand
		{
			get { return documentCommand ?? (documentCommand = Factory.LoadTop1<IDocumentCommand>(new DocumentZQuery(BusinessContext.Shipment, "Delay Alert"))); }
		}
		IDocumentCommand documentCommand;

		#endregion
	}
}
