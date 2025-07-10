using Enterprise.DocumentEngine;
using Enterprise.PrintProcessing.PrintJobAutomation;
using NUnit.Framework;

namespace Enterprise.PrintProcessing.Test.PrintJobAutomation
{
	public class PrintJobProcessTest : TestCase
	{
		public PrintJobProcessTest() : base() { }

		public void TestPrintDocProcess()
		{
			AssertNotNull("Diagnostic process", PrintJobProcess.GetPrintDocProcess(PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestPDFFileName));
		}
	}
}
