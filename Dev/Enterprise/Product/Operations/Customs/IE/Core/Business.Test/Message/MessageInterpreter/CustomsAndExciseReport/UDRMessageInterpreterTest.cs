using System.IO;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(UDRMessageInterpreter))]
	sealed class UDRMessageInterpreterTest : CustomsAndExciseReportInboundMessageInterpreterTest<UDRMessageInterpreter, UDRProvider>
	{
		protected override ZString MessageType => CustomsAndExciseReportTypeList.Codes.UDR;

		protected override UDRProvider GetProvider(TextReader reader) => new UDRProvider(CustomsAndExciseReportInterchangeProcessorTestHelper.CreateUDRMessage());

		protected override ZString ExpectedInterpretation => GetExpectedInterpretation();

		internal static ZString GetExpectedInterpretation() => @"Unpaid declarations report for Payers<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>EORI</td><td>IE1234567A</td></tr>
			</table><br />
			<br />
			Unpaid Orders<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>MRN</td><td>22IEDUB4BBFC22BPP3</td></tr>
				<tr><td>Version</td><td>1</td></tr>
				<tr><td>Tax Total</td><td>1,000.00</td></tr>
			</table><br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>MRN</td><td>22IEDUB4BBFC22BPP1</td></tr>
				<tr><td>Version</td><td>1</td></tr>
				<tr><td>Tax Total</td><td>2,000.00</td></tr>
			</table><br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>MRN</td><td>22IEDUB4BBFC22BPP2</td></tr>
				<tr><td>Version</td><td>1</td></tr>
				<tr><td>Tax Total</td><td>2,000.00</td></tr>
			</table>";
	}
}
