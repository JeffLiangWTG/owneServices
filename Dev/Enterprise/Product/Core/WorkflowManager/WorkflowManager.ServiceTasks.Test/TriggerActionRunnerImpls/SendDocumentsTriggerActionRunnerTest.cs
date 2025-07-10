using System;
using System.Data;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.WorkflowManager.ServiceTasks.Testing
{
	sealed class SendDocumentsTriggerActionRunnerTest : TestCaseWithFactory
	{
		public void TestRun_PersonalFallbackPrimaryWorkEmail1()
		{
			AssertEmail("primarywork@email.com", "justin@email.com", nameof(BusinessContext.GlbPerson), MessageRecipientPartyTypeList.Codes.PersonalFallbackPrimaryWorkEmail, "primarywork@email.com");
		}

		public void TestRun_PersonalFallbackPrimaryWorkEmail2()
		{
			AssertEmail("", "justin@email.com", nameof(BusinessContext.GlbPerson), MessageRecipientPartyTypeList.Codes.PersonalFallbackPrimaryWorkEmail, "justin@email.com");
		}

		public void TestRun_PersonPrimaryWorkEmail()
		{
			AssertEmail("primarywork@email.com", "justin@email.com", nameof(BusinessContext.GlbPerson), MessageRecipientPartyTypeList.Codes.PersonPrimaryWorkEmail, "justin@email.com");
		}

		public void TestRun_PersonalEmail()
		{
			AssertEmail("Just1n@email.com", "", nameof(BusinessContext.GlbPerson), MessageRecipientPartyTypeList.Codes.PersonalEmail, "Just1n@email.com");
		}

		void AssertEmail(string personalEmail, string primaryWorkEmail, string businessContext, string triggerParty, string expectedEmail)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = primaryWorkEmail;
			var person = GlbPerson.CreateFromStaff(Factory, staff);
			person.PER_EmailAddress = personalEmail;

			var wfProvider = (IWorkflowProvider)person;
			var trigger = wfProvider.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			var personDocument = GetRelateDocumentCommand("Document", "", businessContext);
			action.PQ_SU_Document = personDocument.PK;
			action.PQ_Calc_TriggerParty = triggerParty;

			Factory.Save();

			var printJobs = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("Precondition: no documents delivered", 0, printJobs.Length);

			GetRunner(action, person).Process(null);

			printJobs = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("1 document delivered", 1, printJobs.Length);

			var printJob = printJobs[0];
			AssertEquals("Parent", wfProvider.PK, printJob.SP_ParentGuid);
			AssertEquals("Email", expectedEmail, printJob.EmailToRecipients[0].SPR_EmailAddress.ToString());
			AssertEquals("Document", personDocument.SU_MenuName + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling, printJob.SP_DocumentName);
		}

		DocumentCommand GetRelateDocumentCommand(string documentName, string defaultAttachmentType, string businessContext)
		{
			var command = Factory.New<DocumentCommand>();
			command.SU_BusinessContext = businessContext;
			command.SU_MenuName = documentName;
			command.SU_PreventAutoDelivery = false;
			command.SU_DefaultAttachmentType = defaultAttachmentType;
			command.SU_IsPublished = true;
			command.SU_ContactType = ContactType.Receivables.Code;

			var docTemplate = Factory.New<StmTemplate>();
			docTemplate.SO_Name = "WhatEver";
			docTemplate.SO_Template = DocumentEngineTestHelperBase.CreateTemplateFromString(
				$@"{{A}}-[#Config]
{{A}}-[Name={documentName}]
{{A}}-[#SectionBody]
{{B}}-[<Now>]
{{A}}-[#EndOfReport]
");
			docTemplate.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.GenericFreightJob);

			var docPivot = Factory.New<StmMenuTemplatePivot>();
			docPivot.SI_SU = command.PK;
			docPivot.SI_SO = docTemplate.PK;

			return command;
		}

		public void TestRun_Email()
		{
			var trigger = WorkflowProvider.WorkflowItems.Triggers.AddNew();

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SU_Document = DocumentCommand.PK;
			action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			action.PQ_EmailAddr = "Just1n@outlook.com";
			var job = (BusinessObject)WorkflowProvider;

			Factory.Save();

			var printJobs = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("Precondition: no documents delivered", 0, printJobs.Length);

			GetRunner(action, job).Process(null);

			printJobs = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("1 document delivered", 1, printJobs.Length);

			var printJob = printJobs[0];
			AssertEquals("Parent", WorkflowProvider.PK, printJob.SP_ParentGuid);
			AssertEquals("Email", "Just1n@outlook.com", printJob.EmailToRecipients[0].SPR_EmailAddress.ToString());
			AssertEquals("Document", DocumentCommand.SU_MenuName + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling, printJob.SP_DocumentName);
			AssertEquals("Attachment Type", "PDF", printJob.SP_EmailAttachmentFormat);

			DocumentCommand.SU_DefaultAttachmentType = OrgConstants.AttachmentType.XLS;
			GetRunner(action, job).Process(null);
			printJobs = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("1 new document delivered", 2, printJobs.Length);

			printJob = printJobs.First(j => j.SP_EmailAttachmentFormat == "XLS");
			AssertNotNull(printJob);

			action.PQ_EmailAddr = "";
			AssertExceptionThrown<WorkflowValidationException>("Invalid email error should be thrown", action.EmailAddressInvalidOrEmptyErrorMessageForSendingDoc(job), () => GetRunner(action, job).Process(null));

			action.PQ_EmailAddr = "What";
			AssertExceptionThrown<WorkflowValidationException>("Invalid email error should be thrown", action.EmailAddressInvalidOrEmptyErrorMessageForSendingDoc(job), () => GetRunner(action, job).Process(null));
		}

		public void TestDoNotSendDocumentsDisabledBySU_PreventAutoDelivery()
		{
			var trigger = WorkflowProvider.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SU_Document = DocumentCommand.PK;
			DocumentCommand.SU_PreventAutoDelivery = true;
			consignee.MainAddress.OA_Email = "big@kahoona.com";
			document.OD_DefaultContact = true;
			document.OD_DocumentGroup = ContactType.Consignee.Code;

			Factory.Save();

			AssertExceptionThrown<WorkflowValidationException>(() => GetRunner(action, (BusinessObject)WorkflowProvider).Process(null));
			var printJobs = Factory.Load<IStmPrintJob>(new ZQuery());
			AssertEquals("Should not deliver", 0, printJobs.Length);
		}

		public void TestDoPrintDocumentsDisabledBySU_PreventAutoDelivery()
		{
			var trigger = WorkflowProvider.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SU_Document = DocumentCommand.PK;
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Print;
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			DocumentCommand.SU_PreventAutoDelivery = true;
			consignee.MainAddress.OA_Email = "big@kahoona.com";
			document.OD_DefaultContact = true;
			document.OD_DocumentGroup = ContactType.Consignee.Code;

			Factory.Save();

			GetRunner(action, (BusinessObject)WorkflowProvider).Process(null);
			var printJobs = Factory.Load<IStmPrintJob>(new ZQuery());
			AssertEquals("Should deliver", 1, printJobs.Length);
		}

		public void TestRun()
		{
			ProcessTask trigger = WorkflowProvider.WorkflowItems.Triggers.AddNew();

			ProcessTaskNotification action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SU_Document = DocumentCommand.PK;

			Factory.Save();

			StmPrintJob[] printJobs = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("Precondition: no documents delivered", 0, printJobs.Length);

			GetRunner(action, (BusinessObject)WorkflowProvider).Process(null);

			printJobs = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("1 document delivered", 1, printJobs.Length);

			var printJob = printJobs[0] as BusinessObject;
			AssertEquals("Parent", WorkflowProvider.PK, printJob[StmPrintJobSchema.SP_ParentGuid]);
			AssertEquals("Email", "mickey.mouse@cargowise.com", printJobs[0].EmailToRecipients[0].SPR_EmailAddress.ToString());
			AssertEquals("Document", DocumentCommand.SU_MenuName + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling, printJob[StmPrintJobSchema.SP_DocumentName]);

			IDocumentCommand anotherDocumentCommand = Factory.LoadTop1<IDocumentCommand>(new DocumentZQuery(BusinessContext.Shipment, "Arrival Notice"));
			AssertNotNull("Precondition", anotherDocumentCommand);

			action.PQ_SU_Document = anotherDocumentCommand.PK;

			GetRunner(action, (BusinessObject)WorkflowProvider).Process(null);

			printJobs = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("Nothing happened for not matched document", 1, printJobs.Length);
		}

		public void TestRun_Print()
		{
			IStmPrintQueue printQueue = Factory.New<IStmPrintQueue>();
			printQueue.QueueName = "HELLO";

			ProcessTask trigger = WorkflowProvider.WorkflowItems.Triggers.AddNew();

			ProcessTaskNotification action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SU_Document = DocumentCommand.PK;
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Print;
			action.PQ_SQ = printQueue.PK;

			Factory.Save();

			GetRunner(action, (BusinessObject)WorkflowProvider).Process(null);

			IStmPrintJob[] printJobs = Factory.Load<IStmPrintJob>(new ZQuery(StmPrintJobSchema.SP_SQ, printQueue.PK));
			AssertEquals("Document printed", 1, printJobs.Length);
		}

		public void TestRun_AutoDocumentDelivery()
		{
			ProcessTask trigger = WorkflowProvider.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.EditedARecordCode;

			ProcessTaskNotification action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SU_Document = DocumentCommand.PK;
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			Factory.Save();

			GetRunner(action, (BusinessObject)WorkflowProvider).Process(null);

			var query = new ZQuery();
			IStmPrintJob[] printJobsAfter = Factory.Load<IStmPrintJob>(query);
			AssertEquals("Document Auto Deliveried After", 1, printJobsAfter.Length);
			StmPrintJob printJobAfter = (StmPrintJob)printJobsAfter[0];
			AssertEquals("Printer Guid", ZGuid.Empty, printJobAfter.SP_SQ);
			AssertEquals("JobType", nameof(PrintType.EML), printJobAfter.SP_JobType);
			AssertEquals("DocumentName", "Delay Alert" + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling, printJobAfter.SP_DocumentName);
			AssertDeliveryGroup(printJobAfter, false);

			businessObject["JS_HouseBill"] = "House Bill 123";
			Factory.Save();

			MasterFiles.Business.Testing.MasterFilesTestHelper.RunLogWalker(); // Run real LWK
			AssertDeliveryGroup(printJobAfter, true);
		}

		void AssertDeliveryGroup(StmPrintJob printJob, bool isDeliveryGroupInDatabase)
		{
			var deliveryGroupId = printJob.SP_SB_DeliveryGroup;
			var deliveryGroup = Factory.Load<StmDeliveryGroup>(deliveryGroupId);
			AssertNotNull(deliveryGroup);
			AssertEquals("deliveryGroup.IsInDatabase", isDeliveryGroupInDatabase, deliveryGroup.IsInDatabase);
		}

		public void TestRun_SetTemporaryUserContext()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			IStmPrintQueue printQueue = Factory.New<IStmPrintQueue>();
			printQueue.QueueName = "HELLO";

			ProcessTask trigger = WorkflowProvider.WorkflowItems.Triggers.AddNew();

			ProcessTaskNotification action = trigger.ProcessTaskNotifications.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;
			action.PQ_SU_Document = DocumentCommand.PK;
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Print;
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			action.PQ_SQ = printQueue.PK;

			WorkflowProvider.Logs.AddNew(Events.CustomisableEvent00);
			trigger.Logs.Find(l => l.SL_SE_NKEvent == Events.WorkflowTriggerEventCode).Single();

			Factory.Save();
			using (Env.SetTemporaryUserContext(staff1.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				WorkflowServiceTaskTestHelper.RunLogWalker();
			}

			IStmPrintJob[] printJobs = Factory.Load<IStmPrintJob>(new ZQuery(StmPrintJobSchema.SP_SQ, printQueue.PK));
			AssertEquals("Document printed", 1, printJobs.Length);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, ((BusinessObject)printJobs[0])[StmPrintJobSchema.SP_GS_NKJobSubmittedBy]);
		}

		public void TestRun_SetupFetchStrategy()
		{
			var workflowProvider = Factory.New<DummyDocumentSupportableWorkflowProvider>();
			var trigger = workflowProvider.WorkflowItems.Triggers.AddNew();
			((MasterFiles.Business.Testing.DummyProcessTask)trigger).OverriddenParentTypeForTest = typeof(DummyDocumentSupportableWorkflowProvider);

			var command = Factory.New<DocumentCommand>();
			command.SU_IsPublished = true;
			command.SU_MenuName = "Test Menu Name";
			command.SU_MenuType = "DOC";
			command.SU_PreventAutoDelivery = false;
			command.SU_ContactType = ContactType.Miscellaneous.Code;
			command.SU_BusinessContext = nameof(BusinessContext.INVALID);
			command.Parent = workflowProvider;

			ProcessTaskNotification action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_SU_Document = command.PK;
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.AutoDocumentDelivery;

			var fetchStrategy = (DummyFetchStrategy)workflowProvider.FetchStrategy;
			Assert("Expecting Document Fetch Strategy not yet set up", !fetchStrategy.FetchStrategyInitialised);
			GetRunner(action, workflowProvider).Process(null);
			Assert("Expecting Document Fetch Strategy to be set up", fetchStrategy.FetchStrategyInitialised);
		}

		#region Implementation

		class DummyDocumentSupportableWorkflowProvider : MasterFiles.Business.Testing.DummyWithWorkflow, IDocumentSupportable
		{
			public DummyDocumentSupportableWorkflowProvider(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override DocumentSupporter DocumentSupporter => documentSupporter ?? (documentSupporter = new DummyDocumentSupporter(this));
			DocumentSupporter documentSupporter;

			protected override IBusinessObjectFetchStrategy GetFetchStrategy() => fetchStrategy ?? (fetchStrategy = new DummyFetchStrategy(this));
			IBusinessObjectFetchStrategy fetchStrategy;
		}

		class DummyDocumentSupporter : DocumentSupporter
		{
			public DummyDocumentSupporter(BusinessObject parentBusinessObject) : base(parentBusinessObject)
			{
			}

			public override BusinessContext BusinessContext => new BusinessContext();
			public override ISecurityCheckpoint CustomisationSecurityCheckpoint => null;
			protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun) => null;
			protected override Core.Constants.DataContext[] GetSupportedDataContexts() => null;
		}

		class DummyFetchStrategy : BusinessObjectFetchStrategy, IDocumentSupporterFetchStrategy
		{
			public DummyFetchStrategy(BusinessObject parent) : base(parent)
			{
			}

			public void AddDocumentSupporterFetchHints()
			{
				FetchStrategyInitialised = true;
			}

			public bool FetchStrategyInitialised;
		}

		DocumentCommand DocumentCommand
		{
			get { return documentCommand ?? (documentCommand = Factory.LoadTop1<DocumentCommand>(new DocumentZQuery(BusinessContext.Shipment, "Delay Alert"))); }
		}
		DocumentCommand documentCommand;

		IWorkflowProvider WorkflowProvider
		{
			get { return (IWorkflowProvider)BusinessObject; }
		}

		BusinessObject BusinessObject
		{
			get
			{
				if (businessObject == null)
				{
					businessObject = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();

					consignee = Factory.NewWithValidTestData<OrgHeader>();
					businessObject["ConsigneePK"] = consignee.PK;

					OrgContact contact = consignee.Contacts.AddNew();
					contact.OC_ContactName = "Test Contact";
					contact.OC_Email = "mickey.mouse@cargowise.com";

					document = contact.Documents.AddNew();
					document.OD_SU_MenuItem = DocumentCommand.PK;
				}

				return businessObject;
			}
		}
		BusinessObject businessObject;
		OrgDocument document;
		OrgHeader consignee;

		SendDocumentsTriggerActionRunner GetRunner(ProcessTaskNotification action, BusinessObject job, Lazy<IStmALog> logProvider = null)
		{
			return new SendDocumentsTriggerActionRunner(action, job, logProvider);
		}

		#endregion
	}
}
