using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Client.SWT.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;

namespace Enterprise.Client.SWT.GUI.Testing
{
	class BatchReportPrintingRunnerTest : BatchReportRunnerTest
	{
		protected override BatchReportRunner GetReportRunner()
		{
			return new BatchReportPrintingRunnerForTesting();
		}

		protected override void AssertPrintJob(StmPrintJob printJob)
		{
			AssertEquals("Job type should have been 'PRN'", nameof(PrintType.PRN), printJob.SP_JobType);
		}

		protected override void AssertPrintJobCreated(int printJobCount)
		{
			AssertEquals("One print job should have been created", 1, printJobCount);
		}

		class BatchReportPrintingRunnerForTesting : BatchReportPrintingRunner
		{
			protected override ReportCommand GetReportCommand()
			{
				return FactoryProvider.Current.Load<ReportCommand>(new ZGuid("B244B421-D343-407A-BD09-3EF80EDB4C6B"));
			}

			protected override DialogResult ShowPrinterSelectionForm()
			{
				StmPrintQueue printQueue = FactoryProvider.Current.New<StmPrintQueue>();
				printQueue.SQ_QueueName = "TestPrinter";
				printQueue.SQ_DisplayName = "TestPrinter";
				FactoryProvider.Current.Save();
				PrintingInstructions.PrinterDelivery.PrintQueuePK = printQueue.PK;
				return DialogResult.OK;
			}
		}
	}
}
