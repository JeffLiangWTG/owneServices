using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	[TestedType(typeof(PrinterSelectionForm))]
	sealed class PrinterSelectionFormTest : ZFormBasherTest
	{
		public void TestChooseDefaultPrinter()
		{
			StmPrintQueueCollection printers = new StmPrintQueueCollection(Factory);
			printers.Load();
			printers.RemoveAndDeleteAll();

			StmPrintQueue printer1 = CreateNewPrintQueue("Printer1");

			StmPrintQueue printer2 = CreateNewPrintQueue("Printer2");

			StmPrintQueue printer3 = CreateNewPrintQueue("Printer3");

			Factory.Save();

			PrintTask testTask = new PrintTask();
			testTask.DeliveryInstructionsDefaultPK = ZGuid.NewZGuid();

			DeliveryInstructions instructions = new DeliveryInstructions();
			instructions.PrinterDelivery.PrintQueuePK = printer2.PK;
			testTask.SavePrinterDeliveryDefaults(instructions);

			using (MockPrinterSelectionForm form = new MockPrinterSelectionForm(instructions))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("3 printers in the list", 3, form.PrintersGrid.List.Count);
				AssertEquals("Printer 2 selected in grid", printer2.PK, form.PrintersGrid.SelectedElements[0].PK);
			}
		}

		StmPrintQueue CreateNewPrintQueue(string name)
		{
			StmPrintQueue queue = Factory.New<StmPrintQueue>();
			queue.SQ_ServerName = System.Environment.MachineName;
			queue.SQ_DisplayName = name;
			queue.SQ_QueueName = name;
			return queue;
		}

		public void TestConstructor()
		{
			MockDeliveryInstructions instructions = new MockDeliveryInstructions();

			using (MockPrinterSelectionForm form = new MockPrinterSelectionForm(instructions))
			{
				form.Show();
				AssertEquals(instructions, form.BusinessEntity);
			}
		}

		public void TestNoPrinterSelected()
		{
			MockDeliveryInstructions instructions = new MockDeliveryInstructions();
			instructions.AllowPrint = true;

			using (MockPrinterSelectionForm form = new MockPrinterSelectionForm(instructions))
			{
				form.Show();
				form.OKButton.PerformClick();
				AssertEquals("Error message", "Please select a printer to use.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPrinterNotInstalled()
		{
			MockDeliveryInstructions instructions = new MockDeliveryInstructions();
			instructions.AllowPrint = true;

			using (MockPrinterSelectionForm form = new MockPrinterSelectionForm(instructions))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.PrintersGrid.Select(1);
				form.OKButton.PerformClick();
				string expectedMessage = "The printer you have selected is not currently installed. Please see your system administrator.";
				AssertEquals("Error message", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.PrintersGrid.Select(0);
				form.OKButton.PerformClick();
				expectedMessage = "";
				AssertNull("Error message", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPrinterPermissionDenied()
		{
			MockDeliveryInstructions instructions = new MockDeliveryInstructions();
			instructions.AllowPrint = true;

			using (MockPrinterSelectionForm form = new MockPrinterSelectionForm(instructions))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.PrintersGrid.Select(2);
				form.OKButton.PerformClick();

				string expectedMessage = @"You do not have the security rights to print to the selected printer.
Please see your system administrator if you want to obtain the security rights for this printer.";

				AssertEquals("Error message", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.PrintersGrid.Select(0);
				form.OKButton.PerformClick();
				expectedMessage = "";
				AssertNull("Error message", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region Implementation
		protected override Form GetFormToBashCore()
		{
			return new PrinterSelectionForm(new DeliveryInstructions());
		}

		#endregion
	}
}
