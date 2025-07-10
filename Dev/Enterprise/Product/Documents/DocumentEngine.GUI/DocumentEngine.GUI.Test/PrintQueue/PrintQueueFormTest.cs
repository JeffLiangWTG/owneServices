using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Scheduler.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	[TestedType(typeof(PrintQueueForm))]
	sealed class PrintQueueFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new PrintQueueForm(Factory.New<StmPrintQueue>());
		}

		public void TestSavingNonCancellableWhichCanNotBeDeletedBecauseOfFKViolationWithMessageGreaterThan200()
		{
			StmPrintQueue queue = Factory.NewWithValidTestData<StmPrintQueue>();
			queue.SQ_ServerName = "TEST";
			queue.SQ_QueueName = "TestPrintQueue";

			var accCheque = Factory.NewWithValidTestData<AccChequeBook>();
			accCheque.AK_SQ = queue.PK;
			accCheque.AK_Desc = "TestAccChequeBook";

			var accCompliance = Factory.NewWithValidTestData<AccComplianceSequence>();
			accCompliance.XD_SQ_DocumentPrintQueue = queue.PK;
			accCompliance.XD_Description = "TestAccComplianceSequence";

			accCompliance = Factory.NewWithValidTestData<AccComplianceSequence>();
			accCompliance.XD_SQ_DocumentPrintQueue = queue.PK;
			accCompliance.XD_Description = "TestAccComplianceSequence";

			var dummyTask = Factory.NewWithValidTestData<DummyProcessTask>();
			var dummyBizo = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var tasknotify = Factory.NewWithValidTestData<ProcessTaskNotification>();
			dummyTask.OverriddenParentTypeForTest = typeof(DummyWithWorkflow);
			dummyTask.P9_ParentID = dummyBizo.PK;
			dummyTask.P9_ParentTableCode = dummyBizo.TablePrefix;
			tasknotify.PQ_P9 = dummyTask.PK;
			tasknotify.PQ_SQ = queue.PK;

			var defaultPrinter1 = Factory.New<StmDefaultPrinter>();
			defaultPrinter1.SDP_SubjectTableCode = WhsWarehouseSchema.Constants.Prefix;
			var warehouse = Factory.New<IWhsWarehouse>();
			((BusinessObject)warehouse).FillWithValidTestData();
			warehouse.WW_WarehouseName = "TestWarehouse";
			defaultPrinter1.SDP_SubjectID = warehouse.PK;
			defaultPrinter1.SDP_SQ_Printer = queue.PK;

			var defaultPrinter2 = Factory.New<StmDefaultPrinter>();
			defaultPrinter2.SDP_SubjectTableCode = WhsAreaSchema.Constants.Prefix;
			var area = Factory.New<IWhsArea>();
			((BusinessObject)area).FillWithValidTestData();
			area.WA_Name = "TestArea";
			defaultPrinter2.SDP_SubjectID = area.PK;
			defaultPrinter2.SDP_SQ_Printer = queue.PK;

			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			printJob.SP_DocumentName = "TestPrintJob";
			printJob.SP_SQ = queue.PK;

			var scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			scheduleTask.S5_ScheduleDescription = "TestTask";
			var taskRecipient = Factory.NewWithValidTestData<StmScheduleTaskRecipient>();
			taskRecipient.S6_S5 = scheduleTask.PK;
			taskRecipient.S6_SQ = queue.PK;

			Factory.Save();
			string taskId = tasknotify.Parent.GetParentBusinessObject().HumanReadableName;

			using (var testForm = new PrintQueueForm(queue))
			using (var saveAndCloseButton = new ZToolStripButton())
			{
				ZFormPostingButtonsStrategy.SetupPosting(testForm, saveAndCloseButton, null, null);

				testForm.DisplayMode = ODisplayMode.Delete;

				testForm.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //"You are about to delete this record permanently from the system. Do you want to proceed?"
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				testForm.AcceptButton.PerformClick();
				Application.DoEvents();

				AssertEquals(typeof(HyperlinkAlertForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				var hyperlinkAlertBusinessObject = (HyperlinkAlertBusinessObject)ZFormModaliser.LastIBusinessShownOnDialogForTest;
				AssertNotNull(hyperlinkAlertBusinessObject);

				var topMessageLabel = hyperlinkAlertBusinessObject.MessageLabel;
				var textBox = hyperlinkAlertBusinessObject.LongMessageText;

				ZFormModaliser.LastFormShownDialogForTest.Dispose();

				AssertEquals("This record is in use by one or more record(s) with the following descriptions, and thus cannot be deleted. Please click View Details below to see full list.", topMessageLabel);
				AssertEquals(@"This record is in use by one or more record(s) of the module Cheque Books with the following descriptions, and thus cannot be deleted.
TestAccChequeBook

This record is in use by one or more record(s) of the module Compliance Sequence with the following descriptions, and thus cannot be deleted.
TestAccComplianceSequence
TestAccComplianceSequence

This record is in use by the task(s) in the record(s) with the following ID, and thus cannot be deleted.
Dummy Business Object Default

This record is in use by one or more record(s) of the module Warehouse with the following names, and thus cannot be deleted.
TestWarehouse

This record is in use by one or more record(s) of the module Areas with the following names, and thus cannot be deleted.
TestArea

This record is in use by one or more record(s) of the module Print Job with the following document names, and thus cannot be deleted.
TestPrintJob

This record is in use by one or more record(s) of the module Schedule Report with the following descriptions, and thus cannot be deleted.
TestTask
", textBox);
			}
		}

		public void TestSavingNonCancellableWhichCanNotBeDeletedBecauseOfOneFKViolationWithMessageLessThan200()
		{
			StmPrintQueue queue = Factory.NewWithValidTestData<StmPrintQueue>();
			queue.SQ_ServerName = "TEST";
			queue.SQ_QueueName = "TestPrintQueue";

			for (var i = 0; i < 3; i++)
			{
				AccChequeBook accCheque = Factory.NewWithValidTestData<AccChequeBook>();
				accCheque.AK_SQ = queue.PK;
				accCheque.AK_Desc = "TestAccChequeBook";
			}

			Factory.Save();

			using (var testForm = new PrintQueueForm(queue))
			using (var saveAndCloseButton = new ZToolStripButton())
			{
				ZFormPostingButtonsStrategy.SetupPosting(testForm, saveAndCloseButton, null, null);

				testForm.DisplayMode = ODisplayMode.Delete;

				testForm.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //"You are about to delete this record permanently from the system. Do you want to proceed?"
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				testForm.AcceptButton.PerformClick();
				Application.DoEvents();

				AssertEquals(@"This record is in use by one or more record(s) of the module Cheque Books with the following descriptions, and thus cannot be deleted.
TestAccChequeBook
TestAccChequeBook
TestAccChequeBook
", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowReplaceFormInDeleteModeOnly()
		{
			using (var printQueueForm = new PrintQueueForm(Factory.New<StmPrintQueue>()))
			{
				var printQueue = Factory.NewWithValidTestData<StmPrintQueue>();
				var bizo = new PrintQueueReplaceBizo(printQueue);

				using (var printQueueReplaceForm = new PrintQueueReplaceForm(printQueue.Factory, bizo, true))
				{
					int showCount = 0;

					printQueueReplaceForm.Shown += (sender, e) =>
					{
						if (printQueueForm.DisplayMode == ODisplayMode.Delete)
						{
							showCount++;
						}
					};

					ZFormModaliser.ShowDialogsInTest = true;
					AssertEquals(0, showCount);

					printQueueForm.DisplayMode = ODisplayMode.Edit;
					ZFormModaliser.ShowDialogWithoutDispose(printQueueReplaceForm);
					AssertEquals(0, showCount);

					printQueueForm.DisplayMode = ODisplayMode.Delete;
					ZFormModaliser.ShowDialogWithoutDispose(printQueueReplaceForm);
					AssertEquals(1, showCount);
				}
			}
		}

		public void TestClosePrintQueueFormIfCancelReplaceForm()
		{
			using (var printQueueForm = new PrintQueueForm(Factory.New<StmPrintQueue>()))
			{
				var printQueue = Factory.NewWithValidTestData<StmPrintQueue>();
				var bizo = new PrintQueueReplaceBizo(printQueue);

				using (var deleteButton = new ZToolStripButton())
				using (var printQueueReplaceForm = new PrintQueueReplaceForm(printQueue.Factory, bizo, true))
				{
					ZFormPostingButtonsStrategy.SetupPosting(printQueueForm, deleteButton, null, null);
					printQueueForm.DisplayMode = ODisplayMode.Delete;

					printQueueForm.Show();
					AssertEquals(true, printQueueForm.Visible);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					printQueueForm.AcceptButton.PerformClick();

					printQueueReplaceForm.DialogResult = DialogResult.Cancel;
					AssertEquals(false, printQueueForm.Visible);
				}
			}
		}
	}
}
