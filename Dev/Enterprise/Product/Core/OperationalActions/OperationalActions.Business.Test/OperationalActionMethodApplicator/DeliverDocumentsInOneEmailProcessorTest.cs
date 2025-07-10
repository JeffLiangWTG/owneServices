using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Services.OperationalActions.Business.OperationalActionRunnerLookups;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	sealed class DeliverDocumentsInOneEmailProcessorTest : TestCaseWithFactory
	{
		#region OnlyElectronicBulkDeliveryMethod DeliverDocumentsInOneEmail
		public void TestDeliverDocumentsInOneEmail_NoRecipient()
		{
			var action = GetAction(ContactType.Consignor.Code);
			var consigneeEmail = TestHelper.NewConsigneeWithContact(Core.Constants.ContactNotifyModes.Email);
			var shipment = TestHelper.NewShipmentWithConsignee("JERRYTEST001", consigneeEmail);
			Factory.Save();
			var selectedRecords = new SelectedRecords()
			{ PrimaryKeys = new[] { shipment.PK } };
			var runner = new OperationalActionRunner(action, shipment.GetType(), selectedRecords)
			{ DeliverDocumentsInOneEmail = true, AttachmentOptions = AttachmentOptionsCodes.SingleAttachment, BulkDeliveryMethod = OnlyElectronicBulkDeliveryMethod.CodeText };
			var dummyLog = new DummyOperationalActionLog();
			var factoryForChanges = new BusinessObjectFactory();
			runner.Run(dummyLog, factoryForChanges);
			AssertContains($"ERROR: There is no valid recipient for [{shipment.HumanReadableName}]", dummyLog.MessagesString());
			AssertEquals("No printed document", 0, TestHelper.CountPrintJobs(shipment.PK));
		}

		public void TestDeliverDocumentsInOneEmail_DifferentRecipient()
		{
			var action = GetAction(ContactType.Consignee.Code);
			var consigneeEmail = TestHelper.NewConsigneeWithContact(Core.Constants.ContactNotifyModes.Email);
			var shipment1 = TestHelper.NewShipmentWithConsignee("JERRYTEST001", consigneeEmail);
			var consigneeEmail2 = TestHelper.NewConsigneeWithContact(Core.Constants.ContactNotifyModes.Email);
			consigneeEmail2.OH_FullName = "JerryTestConsignee2";
			var shipment2 = TestHelper.NewShipmentWithConsignee("JERRYTEST002", consigneeEmail2);
			Factory.Save();
			var selectedRecords = new SelectedRecords()
			{ PrimaryKeys = new[] { shipment1.PK, shipment2.PK } };
			var runner = new OperationalActionRunner(action, shipment1.GetType(), selectedRecords)
			{ DeliverDocumentsInOneEmail = true, AttachmentOptions = AttachmentOptionsCodes.SingleAttachment, BulkDeliveryMethod = OnlyElectronicBulkDeliveryMethod.CodeText };
			var dummyLog = new DummyOperationalActionLog();
			var factoryForChanges = new BusinessObjectFactory();
			runner.Run(dummyLog, factoryForChanges);
			AssertContains("ERROR: The Deliver Documents In One Email option only supports sending documents to contacts in a single organization, delivering documents to multiple organizations is not supported.", dummyLog.MessagesString());
			AssertEquals("No printed document", 0, TestHelper.CountPrintJobs(shipment1.PK));
			AssertEquals("No printed document", 0, TestHelper.CountPrintJobs(shipment2.PK));
		}

		public void TestDeliverDocumentsInOneEmail_SingleAttachment() => AssertDeliverDocumentsInOneEmail_SingleAttachment(string.Empty, false, false);
		public void TestDeliverDocumentsInOneEmail_SingleAttachment_SubjectLineMacros() => AssertDeliverDocumentsInOneEmail_SingleAttachment("Jerry Test - JERRYTEST001", true, false);
		public void TestDeliverDocumentsInOneEmail_SingleAttachment_MenuDataContext() => AssertDeliverDocumentsInOneEmail_SingleAttachment($"{GlbCompany.CurrentCompany.GC_Name} ({GlbBranch.CurrentBranch.GB_BranchName}) - Jerry Test Command - JERRYTEST001", false, true);
		public void TestDeliverDocumentsInOneEmail_SingleAttachment_SubjectLineMacrosAndMenuDataContext() => AssertDeliverDocumentsInOneEmail_SingleAttachment("Jerry Test - JERRYTEST001", true, true);
		public void TestDeliverDocumentsInOneEmail_SingleAttachment_MultipleDocumentCommands() => AssertDeliverDocumentsInOneEmail_SingleAttachment("Jerry Test Primary - JERRYTEST001", true, true, true);
		void AssertDeliverDocumentsInOneEmail_SingleAttachment(string expectedEmailSubjectLine, bool setSubjectLineMacros = false, bool setMenuDataContext = false, bool setPrimaryCommand = false)
		{
			var action = GetAction(ContactType.Consignee.Code, setSubjectLineMacros, setMenuDataContext, setPrimaryCommand: setPrimaryCommand);
			var consigneeEmail = TestHelper.NewConsigneeWithContact(Core.Constants.ContactNotifyModes.Email);
			var shipment1 = TestHelper.NewShipmentWithConsignee("JERRYTEST001", consigneeEmail);
			var shipment2 = TestHelper.NewShipmentWithConsignee("JERRYTEST002", consigneeEmail);
			var shipment3 = TestHelper.NewShipmentWithConsignee("JERRYTEST003", consigneeEmail);
			Factory.Save();
			var selectedRecords = new SelectedRecords()
			{ PrimaryKeys = new[] { shipment1.PK, shipment2.PK, shipment3.PK } };
			var runner = new OperationalActionRunner(action, shipment1.GetType(), selectedRecords)
			{ DeliverDocumentsInOneEmail = true, AttachmentOptions = AttachmentOptionsCodes.SingleAttachment, BulkDeliveryMethod = OnlyElectronicBulkDeliveryMethod.CodeText };
			var dummyLog = new DummyOperationalActionLog();
			var factoryForChanges = new BusinessObjectFactory();
			runner.Run(dummyLog, factoryForChanges);
			var printJob1 = TestHelper.PrintJobs(shipment1.PK);
			var groups = TestHelper.DeliveryGroups(printJob1.Select(p => p.SP_SB_DeliveryGroup).ToArray());
			AssertEquals("Should have 1 StmDeliveryGroup", 1, groups.Length);
			AssertEquals(expectedEmailSubjectLine, groups[0].SB_EmailSubjectLine);
			AssertEquals("Should have printed 1 document for shipment1", 1, printJob1.Length);
			AssertContains("Email subject should contains the first job number", "JERRYTEST001", printJob1[0].SP_EmailSubjectLine);
			AssertEquals("Should not attach the merged delivery document to job", true, printJob1[0].SP_EDocsProcessed);
			AssertEquals("No printed document for shipment2", 0, TestHelper.CountPrintJobs(shipment2.PK));
			AssertEquals("No printed document for shipment3", 0, TestHelper.CountPrintJobs(shipment3.PK));
		}

		public void TestDeliverDocumentsInOneEmail_SingleAttachment_BatchRun()
		{
			var action = GetAction(ContactType.Consignee.Code, false, false, true, true);
			var consigneeEmail = TestHelper.NewConsigneeWithContact(Core.Constants.ContactNotifyModes.Email);
			var shipment1 = TestHelper.NewShipmentWithConsignee("JERRYTEST001", consigneeEmail);
			var shipment2 = TestHelper.NewShipmentWithConsignee("JERRYTEST002", consigneeEmail);
			var shipment3 = TestHelper.NewShipmentWithConsignee("JERRYTEST003", consigneeEmail);
			Factory.Save();
			using (var note = DocumentNote.LoadNoteWithExclusiveMutex((IStmNoteParent)shipment3))
			{
				AssertEquals("Note is new", true, !note.IsInDatabase);
				var field = (TextField)note.UserDefinedFieldList["TextFieldName"];
				field.Value = "JerryTestField";
				note.Factory.Save();
			}

			using (var dummyModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.JobShipment))
			{
				var filters = dummyModule.FilterBusinessObject;
				var filter = (ModuleTextFilter)filters.ModuleFilters["Shipment #"];
				filter.Property = "JERRYTEST";
				filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				filter.IsActive = true;
				dummyModule.PerformSearch_ForTest();
				var moduleSelection = new ModuleSelection(dummyModule);
				var runner = new OperationalActionRunner(action, shipment1.GetType(), moduleSelection)
				{ DeliverDocumentsInOneEmail = true, AttachmentOptions = AttachmentOptionsCodes.SingleAttachment, BulkDeliveryMethod = OnlyElectronicBulkDeliveryMethod.CodeText, RunOnAllMatchingRecords = true, };
				var dummyLog = new DummyOperationalActionLog();
				var factoryForChanges = new BusinessObjectFactory();
				runner.BatchRun(dummyLog, (log, factory) =>
				{
				});
				var printJob3 = TestHelper.PrintJobs(shipment3.PK);
				var groups = TestHelper.DeliveryGroups(printJob3.Select(p => p.SP_SB_DeliveryGroup).ToArray());
				AssertEquals("Should have 1 StmDeliveryGroup", 1, groups.Length);
				AssertEquals("Jerry Test - JerryTestField", groups[0].SB_EmailSubjectLine);
				AssertEquals("Should have printed 1 document for shipment3", 1, printJob3.Length);
				AssertEquals("Should not attach the merged delivery document to job", true, printJob3[0].SP_EDocsProcessed);
				AssertEquals("No printed document for shipment1", 0, TestHelper.CountPrintJobs(shipment1.PK));
				AssertEquals("No printed document for shipment2", 0, TestHelper.CountPrintJobs(shipment2.PK));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
		public void TestDeliverDocumentsInOneEmail_SingleAttachment_DocumentsFileSizeGreaterThanSizeLimitInBytes()
		{
			var docType = EDocsTestHelper.CreateDocType(Factory, "AAA", "UNL", "A Test Document");
			var bigFileBytes = File.ReadAllBytes(Path.Combine(BaseSourcePath, BigFilePath));
			var documentFactory = EDocsTestHelper.GetDocumentFactory(Factory);
			var action = GetAction(ContactType.Consignee.Code, true, false, docType: docType);
			var consigneeEmail = TestHelper.NewConsigneeWithContact(Core.Constants.ContactNotifyModes.Email);
			var shipment = TestHelper.NewShipmentWithConsignee($"JERRYTEST0", consigneeEmail);
			var storageMain = EDocsTestHelper.GetOrCreateStorageMain(documentFactory, shipment);
			storageMain.AddFileOrDocument(bigFileBytes, "Test File", "AAA", false, description: string.Empty);
			documentFactory.Save();
			Factory.Save();
			var pks = new[] { shipment.PK };
			var selectedRecords = new SelectedRecords()
			{ PrimaryKeys = pks };
			var runner = new OperationalActionRunner(action, shipment.GetType(), selectedRecords)
			{ DeliverDocumentsInOneEmail = true, AttachmentOptions = AttachmentOptionsCodes.SingleAttachment, BulkDeliveryMethod = OnlyElectronicBulkDeliveryMethod.CodeText };
			using (SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			{
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				var dummyLog = new DummyOperationalActionLog();
				var factoryForChanges = new BusinessObjectFactory();
				runner.Run(dummyLog, factoryForChanges);
				AssertEquals("Deliver Documents in One Email", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Email attachment file size has exceeded the attachment limit size, do you still want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				var query = new ZQuery(StmPrintJobSchema.SP_ParentGuid, SQLComparisonOperator.Equal, pks);
				var results = Factory.Load<StmPrintJob>(query);
				AssertEquals("Should no printed jobs", 0, results.Length);
				UnitTestUserNotification.Instance.ClearMessages();
				AssertNullOrEmpty("UnitTestUserNotification.Instance.LastMessage.Caption", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertNullOrEmpty("UnitTestUserNotification.Instance.LastMessage.Text", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				dummyLog = new DummyOperationalActionLog();
				factoryForChanges = new BusinessObjectFactory();
				runner.Run(dummyLog, factoryForChanges);
				results = Factory.Load<StmPrintJob>(query);
				AssertEquals("Deliver Documents in One Email", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Email attachment file size has exceeded the attachment limit size, do you still want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should have printed 1 document and 1 file for shipment1", 2, results.Length);
				AssertEquals("Should not attach the merged delivery document to job", true, results[0].SP_EDocsProcessed);
				var groups = TestHelper.DeliveryGroups(results.Select(p => p.SP_SB_DeliveryGroup).ToArray());
				var expectedEmailSubjectLine = $"Jerry Test - JERRYTEST0";
				AssertEquals("Should have 1 StmDeliveryGroup", 1, groups.Length);
				AssertEquals(expectedEmailSubjectLine, groups[0].SB_EmailSubjectLine);
			}
		}

		public void TestDeliverDocumentsInOneEmail_MultipleAttachments() => AssertDeliverDocumentsInOneEmail_MultipleAttachments($"{GlbCompany.CurrentCompany.GC_Name} ({GlbBranch.CurrentBranch.GB_BranchName}) - Jerry Test Command", false, false);
		public void TestDeliverDocumentsInOneEmail_MultipleAttachments_SubjectLineMacros() => AssertDeliverDocumentsInOneEmail_MultipleAttachments("Jerry Test -", true, false);
		public void TestDeliverDocumentsInOneEmail_MultipleAttachments_MenuDataContext() => AssertDeliverDocumentsInOneEmail_MultipleAttachments($"{GlbCompany.CurrentCompany.GC_Name} ({GlbBranch.CurrentBranch.GB_BranchName}) - Jerry Test Command", false, true);
		public void TestDeliverDocumentsInOneEmail_MultipleAttachments_SubjectLineMacrosAndMenuDataContext() => AssertDeliverDocumentsInOneEmail_MultipleAttachments("Jerry Test -", true, true);
		public void TestDeliverDocumentsInOneEmail_MultipleAttachments_MultipleDocumentCommands() => AssertDeliverDocumentsInOneEmail_MultipleAttachments("Jerry Test Primary -", true, true, true);
		void AssertDeliverDocumentsInOneEmail_MultipleAttachments(string expectedEmailSubjectLine, bool setSubjectLineMacros = false, bool setMenuDataContext = false, bool setPrimaryCommand = false)
		{
			var action = GetAction(ContactType.Consignee.Code, setSubjectLineMacros, setMenuDataContext, setPrimaryCommand: setPrimaryCommand);
			var consigneeEmail = TestHelper.NewConsigneeWithContact(Core.Constants.ContactNotifyModes.Email);
			var shipment1 = TestHelper.NewShipmentWithConsignee("JERRYTEST001", consigneeEmail);
			var shipment2 = TestHelper.NewShipmentWithConsignee("JERRYTEST002", consigneeEmail);
			var shipment3 = TestHelper.NewShipmentWithConsignee("JERRYTEST003", consigneeEmail);
			Factory.Save();
			var selectedRecords = new SelectedRecords()
			{ PrimaryKeys = new[] { shipment1.PK, shipment2.PK, shipment3.PK } };
			var runner = new OperationalActionRunner(action, shipment1.GetType(), selectedRecords)
			{ DeliverDocumentsInOneEmail = true, AttachmentOptions = AttachmentOptionsCodes.MultipleAttachments, BulkDeliveryMethod = OnlyElectronicBulkDeliveryMethod.CodeText };
			var dummyLog = new DummyOperationalActionLog();
			var factoryForChanges = new BusinessObjectFactory();
			runner.Run(dummyLog, factoryForChanges);
			var printJob1 = TestHelper.PrintJobs(shipment1.PK);
			var printJob2 = TestHelper.PrintJobs(shipment2.PK);
			var printJob3 = TestHelper.PrintJobs(shipment3.PK);
			var expectedCount = setPrimaryCommand ? 2 : 1;
			AssertEquals($"Should have printed {expectedCount} document for shipment1", expectedCount, printJob1.Length);
			AssertEquals($"Should have printed {expectedCount} document for shipment2", expectedCount, printJob2.Length);
			AssertEquals($"Should have printed {expectedCount} document for shipment3", expectedCount, printJob3.Length);
			var groupPKs = printJob1.Select(p => p.SP_SB_DeliveryGroup).Concat(printJob2.Select(p => p.SP_SB_DeliveryGroup)).Concat(printJob3.Select(p => p.SP_SB_DeliveryGroup)).ToArray();
			var groups = TestHelper.DeliveryGroups(groupPKs);
			AssertEquals("Should have 1 StmDeliveryGroup", 1, groups.Length);
			AssertEquals(expectedEmailSubjectLine, groups[0].SB_EmailSubjectLine);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
		public void TestDeliverDocumentsInOneEmail_MultipleAttachments_DocumentsFileSizeGreaterThanSizeLimitInBytes()
		{
			var docType = EDocsTestHelper.CreateDocType(Factory, "AAA", "UNL", "A Test Document");
			var bigFileBytes = File.ReadAllBytes(Path.Combine(BaseSourcePath, BigFilePath));
			var documentFactory = EDocsTestHelper.GetDocumentFactory(Factory);
			var action = GetAction(ContactType.Consignee.Code, true, false, docType: docType);
			var consigneeEmail = TestHelper.NewConsigneeWithContact(Core.Constants.ContactNotifyModes.Email);
			var shipment = TestHelper.NewShipmentWithConsignee($"JERRYTEST0", consigneeEmail);
			var storageMain = EDocsTestHelper.GetOrCreateStorageMain(documentFactory, shipment);
			storageMain.AddFileOrDocument(bigFileBytes, "Test File", "AAA", false, description: string.Empty);
			documentFactory.Save();
			Factory.Save();
			var pks = new[] { shipment.PK };
			var selectedRecords = new SelectedRecords()
			{ PrimaryKeys = pks };
			var runner = new OperationalActionRunner(action, shipment.GetType(), selectedRecords)
			{ DeliverDocumentsInOneEmail = true, AttachmentOptions = AttachmentOptionsCodes.MultipleAttachments, BulkDeliveryMethod = OnlyElectronicBulkDeliveryMethod.CodeText };
			using (SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			{
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				var dummyLog = new DummyOperationalActionLog();
				var factoryForChanges = new BusinessObjectFactory();
				runner.Run(dummyLog, factoryForChanges);
				AssertEquals("Deliver Documents in One Email", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Email attachment file size has exceeded the attachment limit size, do you still want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				var query = new ZQuery(StmPrintJobSchema.SP_ParentGuid, SQLComparisonOperator.Equal, pks);
				var results = Factory.Load<StmPrintJob>(query);
				AssertEquals("Should no printed jobs", 0, results.Length);
				UnitTestUserNotification.Instance.ClearMessages();
				AssertNullOrEmpty("UnitTestUserNotification.Instance.LastMessage.Caption", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertNullOrEmpty("UnitTestUserNotification.Instance.LastMessage.Text", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				dummyLog = new DummyOperationalActionLog();
				factoryForChanges = new BusinessObjectFactory();
				runner.Run(dummyLog, factoryForChanges);
				results = Factory.Load<StmPrintJob>(query);
				AssertEquals("Deliver Documents in One Email", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Email attachment file size has exceeded the attachment limit size, do you still want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should have printed 1 document and 1 file for shipment1", 2, results.Length);
				var groups = TestHelper.DeliveryGroups(results.Select(p => p.SP_SB_DeliveryGroup).ToArray());
				var expectedEmailSubjectLine = $"Jerry Test -";
				AssertEquals("Should have 1 StmDeliveryGroup", 1, groups.Length);
				AssertEquals(expectedEmailSubjectLine, groups[0].SB_EmailSubjectLine);
			}
		}

		public void TestDeliverDocumentsInOneEmail_OverrideEmail()
		{
			var action = GetAction(ContactType.Consignee.Code, true);
			var consigneeEmail = TestHelper.NewConsigneeWithContact(Core.Constants.ContactNotifyModes.Email);
			var shipment1 = TestHelper.NewShipmentWithConsignee("JERRYTEST001", consigneeEmail);
			Factory.Save();
			var selectedRecords = new SelectedRecords()
			{ PrimaryKeys = new[] { shipment1.PK } };
			var runner = new OperationalActionRunner(action, shipment1.GetType(), selectedRecords)
			{ DeliverDocumentsInOneEmail = true, BulkDeliveryMethod = OnlyElectronicBulkDeliveryMethod.CodeText, OverrideRecipientEmail = true, RecipientEmail = "override@email.com" };
			var dummyLog = new DummyOperationalActionLog();
			var factoryForChanges = new BusinessObjectFactory();

			runner.Run(dummyLog, factoryForChanges);

			var printJob = TestHelper.PrintJobs(shipment1.PK);
			AssertEquals(1, printJob.Length);
			AssertEquals(1, printJob[0].EmailToRecipients.Count);
			AssertEquals("override@email.com", printJob[0].EmailToRecipients[0].SPR_EmailAddress);
		}

		OperationalAction GetAction(string contactType, bool setSubjectLineMacros = false, bool setMenuDataContext = false, bool setUserDefinedFieldList = false, bool setFields = false, RefDocType docType = null, bool setPrimaryCommand = false)
		{
			var action = Factory.New<OperationalAction>();
			action.Context = new OperationalActionContext(new MockOperationalActionSupportable().OperationalActionSupporter, "Module Name");
			var command = TestHelper.CreateNewDocumentCommand("Jerry Test Template", "Jerry Test Command", contactType, setSubjectLineMacros, setMenuDataContext, setUserDefinedFieldList, setFields, docType, setPrimaryCommand);
			action.DocumentPivots.AddNew().SF_SU_Outward = command.PK;
			if (setPrimaryCommand)
			{
				var primaryCommand = TestHelper.CreateNewDocumentCommand("Jerry Test Primary Template", "Jerry Test Primary Command", contactType, setSubjectLineMacros, setMenuDataContext, setUserDefinedFieldList, setFields, docType, setPrimaryCommand);
				action.DocumentPivots.AddNew().SF_SU_Outward = primaryCommand.PK;
			}

			Factory.Save();
			return action;
		}

		#endregion
		DeliverDocumentsTestHelper testHelper;
		DeliverDocumentsTestHelper TestHelper => testHelper ?? (testHelper = new DeliverDocumentsTestHelper(Factory));
		const string BigFilePath = @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\2MB.dat";
	}
}
