using System;
using System.Diagnostics;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Shared;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Scheduler.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	[TestedType(typeof(PrintQueueReplaceForm))]
	sealed class PrintQueueReplaceFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new PrintQueueReplaceForm(Factory, new PrintQueueReplaceBizo(Factory.New<StmPrintQueue>()), false);
		}

		public void TestFormValidation()
		{
			var printQueue = Factory.NewWithValidTestData<StmPrintQueue>();
			printQueue.SQ_DisplayName = "Print Queue 1";
			var printQueue2 = Factory.NewWithValidTestData<StmPrintQueue>();
			printQueue2.SQ_DisplayName = "Replace Print Queue";

			var printQueueReplaceBizo = new PrintQueueReplaceBizo(printQueue);
			using (var form = new PrintQueueReplaceForm(Factory, printQueueReplaceBizo, false))
			{
				form.Show();

				form.OkButton.PerformClick();
				AssertEquals("Please select a printer.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();

				printQueueReplaceBizo.ReplacePrintQueuePK = printQueue2.PK;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

				form.OkButton.PerformClick();
				AssertEquals("This action will replace all usages of 'Print Queue 1' with 'REPLACE PRINT QUEUE'.\r\nDo you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				form.OkButton.PerformClick();
				AssertEquals("Print Queue 1 has been successfully replaced.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestHasDependantsProperty()
		{
			var printQueue = Factory.NewWithValidTestData<StmPrintQueue>();
			var printQueueReplaceBizo = new PrintQueueReplaceBizo(printQueue);

			using (var form = new PrintQueueReplaceForm(Factory, printQueueReplaceBizo, false))
			{
				AssertEquals("Now has no dependants", false, form.HasDependants);

				var accCheque = Factory.NewWithValidTestData<AccChequeBook>();
				accCheque.AK_SQ = printQueue.PK;
				var printJob = Factory.NewWithValidTestData<StmPrintJob>();
				printJob.SP_SQ = printQueue.PK;

				AssertEquals("Now has 2 dependants", true, form.HasDependants);

				accCheque.AK_SQ = new ZGuid();
				AssertEquals("Now has 1 dependant", true, form.HasDependants);

				printJob.SP_SQ = new ZGuid();
				AssertEquals("Now has no dependants", false, form.HasDependants);
			}
		}

		public void TestAllForeignKeysOnPrintQueuePK()
		{
			var sqlText = FormattableString.Invariant($@"SELECT
	OBJECT_NAME(f.parent_object_id) AS Table_Name
FROM 
	sys.foreign_keys AS f
	INNER JOIN sys.foreign_key_columns AS fc ON f.object_id = fc.constraint_object_id
	INNER JOIN sys.tables AS t ON f.referenced_object_id = t.object_id
WHERE 
	OBJECT_NAME(f.referenced_object_id) = '{StmPrintQueueSchema.Constants.TableName}'
GROUP BY 
	OBJECT_NAME(f.parent_object_id)
HAVING 
	OBJECT_NAME(f.parent_object_id) != '{StmPrintQueueSchema.Constants.TableName}'");

			var printQueue = Factory.NewWithValidTestData<StmPrintQueue>();
			var printQueueReplaceBizo = new PrintQueueReplaceBizo(printQueue);

			using (var form = new PrintQueueReplaceForm(Factory, printQueueReplaceBizo, false))
			using (var command = CargoWise.Data.Db.Connection.Command(sqlText))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var tableName = reader.GetString(0);
					AssertEquals($@"You should do these things:
											1. Add {tableName} into PrintQueueReplaceForm.printQueueDependants
											2. Add replacer into PrintQueueReplacer.SaveInTransaction
											3. Add checker into PrintQueueForm.GetMessageForCannotDeleteRecordInUseException", true, form.printQueueDependants.ContainsKey(tableName));
				}
			}
		}

		[StressTest]
		public void TestHasDependantsDoesntTakeTooLongToLoad()
		{
			var printQueue = Factory.NewWithValidTestData<StmPrintQueue>();
			var printQueueReplaceBizo = new PrintQueueReplaceBizo(printQueue);

			var shipment = Factory.NewWithValidTestData<DummyShipmentBusinessObject>();
			var processTask = Factory.NewWithValidTestData<ProcessTask>();
			processTask.P9_ParentID = shipment.PK;
			processTask.P9_ParentTableCode = shipment.TablePrefix;

			Factory.Save();

			var sqlText = FormattableString.Invariant($@"DECLARE @count INT = 0;

WHILE @count < 60000
BEGIN
	DECLARE @docPK UNIQUEIDENTIFIER = NEWID()
	INSERT INTO {StmMenuItemSchema.Constants.SqlSchemaName}.{StmMenuItemSchema.Constants.TableName}
	({StmMenuItemSchema.Constants.PK},
		{StmMenuItemSchema.Constants.SU_MenuName})
	VALUES (@docPK,
		CAST(@count AS varchar(5)))

	INSERT INTO dbo.ProcessTaskNotification
	({ProcessTaskNotificationSchema.Constants.PK},
		{ProcessTaskNotificationSchema.Constants.PQ_TriggerType},
		{ProcessTaskNotificationSchema.Constants.PQ_TriggerParty},
		{ProcessTaskNotificationSchema.Constants.PQ_EmailText},
		{ProcessTaskNotificationSchema.Constants.PQ_MessagePurpose},
		{ProcessTaskNotificationSchema.Constants.PQ_SQ},
		{ProcessTaskNotificationSchema.Constants.PQ_SU_Document},
		{ProcessTaskNotificationSchema.Constants.PQ_P9},
		{ProcessTaskNotificationSchema.Constants.PQ_EmailAddr},
		{ProcessTaskNotificationSchema.Constants.PQ_OH_Recipient},
		{ProcessTaskNotificationSchema.Constants.PQ_P0_WorkflowTemplate},
		{ProcessTaskNotificationSchema.Constants.PQ_P9T_Trigger},
		{ProcessTaskNotificationSchema.Constants.PQ_SourceTemplateNotification},
		{ProcessTaskNotificationSchema.Constants.PQ_TriggerPartyService})
	VALUES
	(NEWID(),
		@triggerType,
		@triggerParty,
		'',
		'',
		@printQueuePK,
		@docPK,
		@triggerPK,
		'',
		NULL,
		NULL,
		NULL,
		NULL,
		'');
	SET @count = @count + 1;
END");
			using (var printReplace = new PrintQueueReplaceForm(printQueue.Factory, printQueueReplaceBizo, false))
			{
				using (var command = CargoWise.Data.Db.Connection.Command(sqlText))
				{
					command.AddParameterBasedOnDbColumn("@triggerType", WorkflowTriggerActionTypeConstants.Codes.SendDocument, ProcessTaskNotificationSchema.PQ_TriggerType);
					command.AddParameterBasedOnDbColumn("@triggerParty", MessageRecipientPartyTypeList.Codes.Print, ProcessTaskNotificationSchema.PQ_TriggerParty);
					command.AddParameterBasedOnDbColumn("@printQueuePK", printQueue.PK.ToGuid(), ProcessTaskNotificationSchema.PQ_SQ);
					command.AddParameterBasedOnDbColumn("@triggerPK", processTask.PK.ToGuid(), ProcessTaskNotificationSchema.PQ_P9);
					command.ExecuteNonQuery();
				}

				var stopwatch = new Stopwatch();

				stopwatch.Start();
				bool temp = printReplace.HasDependants;
				stopwatch.Stop();

				double milliseconds = stopwatch.Elapsed.TotalMilliseconds;

				Assert("PrintQueueReplaceForm.HasDependants was false and took " + milliseconds + " milliseconds", temp);
				Assert("PrintQueueReplaceForm.HasDependants took " + milliseconds + " milliseconds", milliseconds < 10000.0);
			}
		}

		public void TestReplacePrinter()
		{
			TestReplacePrinter(false);
		}

		public void TestDeletePrinter()
		{
			TestReplacePrinter(true);
		}

		void TestReplacePrinter(bool deletePrinter)
		{
			var originalPrintQueue = Factory.NewWithValidTestData<StmPrintQueue>();
			originalPrintQueue.SQ_DisplayName = "Original Print Queue";
			var replacePrintQueue = Factory.NewWithValidTestData<StmPrintQueue>();
			replacePrintQueue.SQ_DisplayName = "New Print Queue";

			// AccChequeBook
			var accCheque = Factory.NewWithValidTestData<AccChequeBook>();
			accCheque.AK_SQ = originalPrintQueue.PK;
			accCheque.AK_Desc = "TestAccChequeBook";

			// AccComplianceSequence
			var accCompliance = Factory.NewWithValidTestData<AccComplianceSequence>();
			accCompliance.XD_SQ_DocumentPrintQueue = originalPrintQueue.PK;
			accCompliance.XD_Description = "TestAccComplianceSequence";

			// ProcessTaskNotification
			var dummyBO = Factory.New<DummyWithWorkflow>();
			var trigger = dummyBO.WorkflowItems.AddNew();
			trigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.FreightLoadedCode;
			var processTaskNotification = trigger.ProcessTaskNotifications.AddNew();
			processTaskNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			processTaskNotification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Print;
			processTaskNotification.PQ_SQ = originalPrintQueue.PK;

			// StmDefaultPrinter
			var defaultPrinter = Factory.NewWithValidTestData<StmDefaultPrinter>();
			defaultPrinter.SDP_SubjectTableCode = WhsWarehouseSchema.Constants.Prefix;
			var warehouse = Factory.New<IWhsWarehouse>();
			((BusinessObject)warehouse).FillWithValidTestData();
			warehouse.WW_WarehouseName = "TestWarehouse";
			defaultPrinter.SDP_SubjectID = warehouse.PK;
			defaultPrinter.SDP_SQ_Printer = originalPrintQueue.PK;

			// StmPrintJob
			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_DocumentName = "TestPrintJob";
			printJob.SP_SQ = originalPrintQueue.PK;

			// StmScheduleTask
			var scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			scheduleTask.S5_ScheduleDescription = "TestTask";
			var taskRecipient = Factory.NewWithValidTestData<StmScheduleTaskRecipient>();
			taskRecipient.S6_S5 = scheduleTask.PK;
			taskRecipient.S6_SQ = originalPrintQueue.PK;

			Factory.Save();

			if (!deletePrinter)
			{
				var printQueueReplaceBizo = new PrintQueueReplaceBizo(originalPrintQueue);
				using (var form = new PrintQueueReplaceForm(Factory, printQueueReplaceBizo, deletePrinter))
				{
					form.Show();

					printQueueReplaceBizo.ReplacePrintQueuePK = replacePrintQueue.PK;
					form.OkButton.PerformClick();
				}
			}
			else
			{
				using (var printQueueForm = new PrintQueueForm(originalPrintQueue))
				{
					var bizo = new PrintQueueReplaceBizo(originalPrintQueue);

					using (var deleteButton = new ZToolStripButton())
					using (var printQueueReplaceForm = new PrintQueueReplaceForm(originalPrintQueue.Factory, bizo, true))
					{
						ZFormPostingButtonsStrategy.SetupPosting(printQueueForm, deleteButton, null, null);
						printQueueForm.DisplayMode = ODisplayMode.Delete;
						printQueueForm.Show();

						printQueueReplaceForm.Show();
						bizo.ReplacePrintQueuePK = replacePrintQueue.PK;
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
						printQueueReplaceForm.OkButton.PerformClick();

						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
						printQueueForm.AcceptButton.PerformClick();
					}
				}
			}

			AssertEquals("The ProcessTaskNotification printer should be match the original printer", processTaskNotification.PQ_SQ, originalPrintQueue.PK);
			AssertEquals("The AccCheque printer should be match the original printer", accCheque.AK_SQ, originalPrintQueue.PK);
			AssertEquals("The AccCompliance printer should be match the original printer", accCompliance.XD_SQ_DocumentPrintQueue, originalPrintQueue.PK);
			AssertEquals("The DefaultPrinter1 printer should be match the original printer", defaultPrinter.SDP_SQ_Printer, originalPrintQueue.PK);
			AssertEquals("The PrintJob printer should be match the original printer", printJob.SP_SQ, originalPrintQueue.PK);
			AssertEquals("The TaskRecipient printer should be match the original printer", taskRecipient.S6_SQ, originalPrintQueue.PK);
			AssertEquals(0, Factory.SaveInTransactionActions.Count);

			processTaskNotification.Reload();
			accCheque.Reload();
			accCompliance.Reload();
			defaultPrinter.Reload();
			printJob.Reload();
			taskRecipient.Reload();

			AssertEquals("The ProcessTaskNotification print queue should be replaced", processTaskNotification.PQ_SQ, replacePrintQueue.PK);
			AssertEquals("The AccCheque print queue should be replaced", accCheque.AK_SQ, replacePrintQueue.PK);
			AssertEquals("The AccCompliance print queue should be replaced", accCompliance.XD_SQ_DocumentPrintQueue, replacePrintQueue.PK);
			AssertEquals("The DefaultPrinter1 print queue should be replaced", defaultPrinter.SDP_SQ_Printer, replacePrintQueue.PK);
			AssertEquals("The PrintJob print queue should be replaced", printJob.SP_SQ, replacePrintQueue.PK);
			AssertEquals("The TaskRecipient print queue should be replaced", taskRecipient.S6_SQ, replacePrintQueue.PK);

			if (deletePrinter)
			{
				Assert(originalPrintQueue.IsDeleted);

				var originalPrintQueue1 = Factory.NewWithValidTestData<StmPrintQueue>();
				originalPrintQueue1.SQ_DisplayName = "Original Printer 1";

				using (var printQueueForm = new PrintQueueForm(originalPrintQueue1))
				{
					using (var deleteButton = new ZToolStripButton())
					{
						ZFormPostingButtonsStrategy.SetupPosting(printQueueForm, deleteButton, null, null);
						printQueueForm.DisplayMode = ODisplayMode.Delete;
						printQueueForm.Show();

						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
						printQueueForm.AcceptButton.PerformClick();

						Assert(originalPrintQueue1.IsDeleted);
					}
				}
			}
		}

		public void TestNotificationsCanBeCleared()
		{
			var printQueue = Factory.NewWithValidTestData<StmPrintQueue>();
			printQueue.SQ_DisplayName = "Original Print Queue";
			var printQueue2 = Factory.NewWithValidTestData<StmPrintQueue>();
			printQueue2.SQ_DisplayName = "Replaced Print Queue";

			var printQueueReplaceBizo = new PrintQueueReplaceBizo(printQueue);
			using (var form = new PrintQueueReplaceForm(Factory, printQueueReplaceBizo, false))
			{
				form.Show();

				printQueueReplaceBizo.ReplacePrintQueuePK = new CargoWise.Types.ZGuid("5A8F1DB5-BEB3-4BF8-8AD6-2E6E582B4BCE");
				AssertEquals("Test an invalid business entity causes notifications", true, printQueueReplaceBizo.HasNotifications());

				printQueueReplaceBizo.ReplacePrintQueuePK = printQueue2.PK;
				AssertEquals("Test a valid business entity clears previous notifications", false, printQueueReplaceBizo.HasNotifications());

				printQueueReplaceBizo.ReplacePrintQueuePK = new CargoWise.Types.ZGuid("0002367C-B0D7-4E56-91B7-B69246A63C85");
				AssertEquals("Test an invalid business entity causes notifications", true, printQueueReplaceBizo.HasNotifications());

				printQueueReplaceBizo.ReplacePrintQueuePK = CargoWise.Types.ZGuid.Empty;
				AssertEquals("Test an empty business entity clears previous notifications", false, printQueueReplaceBizo.HasNotifications());
			}
		}
	}
}
