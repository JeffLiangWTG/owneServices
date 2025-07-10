using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.DocumentDelivery.Testing
{
	sealed class PrintTaskDeliveryButtonsControlTest : TestCaseWithFactory
	{
		#region Tests

		public void TestBind()
		{
			PrintTask task = new PrintTask();
			task.TaskSettings.DeliveryOptions = AllowedDeliveryOptions.All;
			using (TestForm form = new TestForm(task.TaskSettings))
			{
				form.Show();
				AssertEquals(true, form.Control.PreviewZButton.Visible);
			}

			task.TaskSettings.DeliveryOptions = AllowedDeliveryOptions.HardCopyOnly;
			using (TestForm form = new TestForm(task.TaskSettings))
			{
				form.Show();
				AssertEquals(true, form.Control.PreviewZButton.Visible);
			}

			task.TaskSettings.DeliveryOptions = AllowedDeliveryOptions.AllExceptPreview;
			using (TestForm form = new TestForm(task.TaskSettings))
			{
				form.Show();
				AssertEquals(false, form.Control.PreviewZButton.Visible);
			}
		}

		public void TestTaskSettings()
		{
			PrintTask task = new PrintTask();
			using (TestForm form = new TestForm(task.TaskSettings))
			{
				AssertNotNull(form.Control.TaskSettings);
				AssertEquals(task.TaskSettings, form.Control.TaskSettings);
			}
		}

		public void TestPreviewButtonClick()
		{
			PrintTask task = new PrintTask();
			using (TestForm form = new TestForm(task.TaskSettings))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				task.TaskSettings.AllowPreview = ZBool.False;
				form.Control.PreviewButton_Click(this, EventArgs.Empty);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("Cannot preview this many documents. Please print to view them."));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				task.TaskSettings.AllowPreview = ZBool.True;
				task.Add(new DocumentPack());
				AssertEquals("Precondition: Should be single document pack task", ZBool.False, task.TaskSettings.MultipleDocumentPacks);
				form.Control.PreviewButton_Click(this, EventArgs.Empty);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Close", form.Control.CloseZButton.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				task.TaskSettings.AllowPreview = ZBool.True;
				task.Add(new DocumentPack());
				task.Add(new DocumentPack());
				AssertEquals("Precondition: Should be multiple document pack task", ZBool.True, task.TaskSettings.MultipleDocumentPacks);
				form.Control.PreviewButton_Click(this, EventArgs.Empty);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("There is more than one document pack to preview. Would you like to continue?"));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			}
		}

		public void TestVisualizeButtonClick()
		{
			PrintTask task = new PrintTask();
			using (TestForm form = new TestForm(task.TaskSettings))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Precondition: SingleDocPackInstructions should be null", task.TaskSettings.SingleDocPackInstructions);
				form.Control.VisualiseButton_Click(this, EventArgs.Empty);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("Only available for single document pack."));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				task.Add(new DocumentPack());
				task.Add(new DocumentPack());
				AssertEquals("Precondition: TaskSettings shoud be for MutlipleDocumentPacks", ZBool.True, task.TaskSettings.MultipleDocumentPacks);
				form.Control.VisualiseButton_Click(this, EventArgs.Empty);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("Only available for single document pack."));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
			}
		}

		public void TestDeliverNotShowPrinterHelperDialog()
		{
			var printer = Factory.NewWithValidTestData<StmPrintQueue>();
			Factory.Save();

			var pack = new DocumentPack();
			var task = new PrintTask();
			task.Add(pack);

			var testReport = new Report(pack, null)
			{
				PrintCopyType = PrintCopyType.ALL,
				IncludedInPrint = true
			};

			var contact = new DocDeliveryContact(Factory)
			{
				DeliveryMethod = Core.Constants.ContactNotifyModes.Print,
				DeliveryAddress = "123@123.com",
			};

			var instructions = task.TaskSettings.DocPacksDeliveryInstructions[0];
			instructions.Recipients.RemoveAndDeleteAll();
			instructions.Recipients.Add(contact);
			instructions.DeliverablesToBePrinted.Add(testReport);
			task.TaskSettings.PrinterDelivery.PrintQueuePK = printer.PK;

			using (var form = new TestForm(task.TaskSettings))
			{
				form.Control.ShowErrorsEvent += Control_ShowErrorsEvent;
				form.Control.Deliver();
				AssertNull("Should not show PrinterHelpDialog", ZFormModaliser.LastFormShownDialogForTest?.Text);
			}
		}

		void Control_ShowErrorsEvent(object sender)
		{
		}

		#endregion

		#region Implementation

		internal class TestControl : PrintTaskDeliveryButtonsControl
		{
			public TestControl()
				: base()
			{
			}

			public ZButton PreviewZButton
			{
				get { return base.PreviewButton; }
			}

			public ZButton CloseZButton
			{
				get { return base.CloseButton; }
			}
		}

		internal class TestForm : ZForm
		{
			public TestForm(PrintTaskSettings entity)
				: base(entity)
			{
				Control = new TestControl();
				Control.SetDataBinding(CurrentDataItem, "");
				this.Controls.Add(Control);
			}

			public TestControl Control;
		}

		#endregion
	}
}
