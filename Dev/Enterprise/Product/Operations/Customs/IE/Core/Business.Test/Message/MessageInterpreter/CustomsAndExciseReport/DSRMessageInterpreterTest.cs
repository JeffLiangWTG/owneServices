using System.IO;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(DSRMessageInterpreter))]
	sealed class DSRMessageInterpreterTest : CustomsAndExciseReportInboundMessageInterpreterTest<DSRMessageInterpreter, DSRProvider>
	{
		protected override ZString MessageType => CustomsAndExciseReportTypeList.Codes.DSR;

		protected override DSRProvider GetProvider(TextReader reader) => new DSRProvider(CustomsAndExciseReportInterchangeProcessorTestHelper.CreateDSRMessage());

		protected override ZString ExpectedInterpretation => GetExpectedInterpretation();

		internal static ZString GetExpectedInterpretation() => @"Daily details – summary report for Payers<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>EORI</td><td>IE1234567A</td></tr>
				<tr><td>Date</td><td>01-Aug-22</td></tr>
				<tr><td>Tax Total</td><td>400.00</td></tr>
			</table><br />
			<br />
			Tax Breakdowns<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>Tax Type</td><td>A00</td></tr>
				<tr><td>Payable Amount</td><td>150.00</td></tr>
			</table>
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>Tax Type</td><td>B00</td></tr>
				<tr><td>Payable Amount</td><td>250.00</td></tr>
			</table>";
	}
}
