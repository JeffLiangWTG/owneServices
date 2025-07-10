using System.IO;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(PSRMessageInterpreter))]
	sealed class PSRMessageInterpreterTest : CustomsAndExciseReportInboundMessageInterpreterTest<PSRMessageInterpreter, PSRProvider>
	{
		protected override ZString MessageType => CustomsAndExciseReportTypeList.Codes.PSR;

		protected override PSRProvider GetProvider(TextReader reader) => new PSRProvider(CustomsAndExciseReportInterchangeProcessorTestHelper.CreatePSRMessage());

		protected override ZString ExpectedInterpretation => GetExpectedInterpretation();

		internal static ZString GetExpectedInterpretation() => @"Period (monthly) details – summary report for Payers<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>EORI</td><td>IE1234567A</td></tr>
				<tr><td>Period</td><td>01-Aug-22</td></tr>
				<tr><td>Tax Total</td><td>400.00</td></tr>
			</table><br />
			<br />
			Tax Breakdowns<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>Tax Type</td><td>A00</td></tr>
				<tr><td>Payable Amount</td><td>150.00</td></tr>
			</table><br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>Tax Type</td><td>B00</td></tr>
				<tr><td>Payable Amount</td><td>250.00</td></tr>
			</table><br />
			<br />
			Daily Breakdowns<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>Date</td><td>10-Aug-22</td></tr>
				<tr><td>Tax Total</td><td>3,200.00</td></tr>
			</table><br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>Date</td><td>11-Aug-22</td></tr>
				<tr><td>Tax Total</td><td>200.00</td></tr>
			</table>";
	}
}
