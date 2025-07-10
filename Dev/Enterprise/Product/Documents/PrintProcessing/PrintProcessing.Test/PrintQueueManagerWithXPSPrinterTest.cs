using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.RemotePrinting.Engine;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.PrintProcessing.Testing
{
	sealed class PrintQueueManagerWithXPSPrinterTest : TestCaseWithFactory
	{
		[RequiresSoftware(RequiredSoftware.XpsPrinter)]
		public void TestPaperSizeGetsReadIn()
		{
			const string xpsPrinterName = RemotePrinting.Engine.Testing.FlexCelPrinterTest.XPSPrinterName;
			Assert(xpsPrinterName + " printer should exist on all machines running this test.", PrinterExists(xpsPrinterName));

			using (SafeInstalledPrinters.OverridePrintersForTesting(xpsPrinterName))
			{
				PrintQueueManager manager = new PrintQueueManager();
				manager.MaintainStmPrintQueue();
				var serverSubQuery = new ZDBOnlySubQuery(typeof(StmPrintServer), StmPrintQueueSchema.SQ_SPS_Server);
				serverSubQuery.AddToFilter(StmPrintServerSchema.SPS_ServerName, System.Environment.MachineName);
				var query = new ZDBOnlyQuery(typeof(StmPrintQueue));
				query.AddSubQuery(serverSubQuery, JoinCondition.And);
				query.AddToFilter(StmPrintQueueSchema.SQ_QueueName, xpsPrinterName);
				StmPrintQueue queue = Factory.LoadTop1<StmPrintQueue>(query);
				if (queue.SQ_PaperName == "A4")
				{
					AssertEquals("queue.SQ_PaperHeight for A4", 1169, queue.SQ_PaperHeight);
					AssertEquals("queue.SQ_PaperWidth for A4", 827, queue.SQ_PaperWidth);
				}
				else if (queue.SQ_PaperName == "Letter")
				{
					AssertEquals("queue.SQ_PaperHeight for Letter", 1100, queue.SQ_PaperHeight);
					AssertEquals("queue.SQ_PaperWidth for Letter", 850, queue.SQ_PaperWidth);
				}
				else
				{
					Fail("queue.SQ_PaperName should be either 'A4' or 'Letter' for '" + xpsPrinterName + "' on all Developer Machines. Was: [" + queue.SQ_PaperName + "]");
				}
			}
		}

		bool PrinterExists(string name)
		{
			foreach (var printer in SafeInstalledPrinters.AllPrinters)
			{
				if (name.Equals(printer.Name, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}
	}
}
