using System.IO;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(DCTMessageInterpreter))]
	sealed class DCTMessageInterpreterTest : CustomsAndExciseReportInboundMessageInterpreterTest<DCTMessageInterpreter, DCTProvider>
	{
		protected override ZString MessageType => CustomsAndExciseReportTypeList.Codes.DCT;

		protected override DCTProvider GetProvider(TextReader reader) => new DCTProvider(CustomsAndExciseReportInterchangeProcessorTestHelper.CreateDCTMessage());

		protected override ZString ExpectedInterpretation => GetExpectedInterpretation();

		internal static ZString GetExpectedInterpretation() => @"Daily details – combined taxes report for Payers<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>EORI</td><td>IE1234567A</td></tr>
				<tr><td>Day</td><td>01-Aug-22</td></tr>
			</table><br />
			<br />
			Paid Orders<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>MRN</td><td>22IEDUB4BBFC22PER2</td></tr>
				<tr><td>Version</td><td>1</td></tr>
				<tr><td>Amendment</td><td>N</td></tr>
				<tr><td>Declaration Message Type</td><td>H1</td></tr>
				<tr><td>Payer</td><td>IE1234567A</td></tr>
				<tr><td>Importer</td><td>IE1234567A</td></tr>
				<tr><td>Importer Name</td><td>MR Test ONeill</td></tr>
				<tr><td>Declarant</td><td>IE7654321A</td></tr>
				<tr><td>Declarant Name</td><td>MR Test Murphy</td></tr>
				<tr><td>Received</td><td>2022-08-11T10:56:11.903+0100</td></tr>
				<tr><td>Tax Total</td><td>200.00</td></tr>
				<tr><td>Total Duty</td><td>150.00</td></tr>
				<tr><td>Vat On Duty</td><td>50.00</td></tr>
				<tr><td>Total Excise</td><td>0.00</td></tr>
				<tr><td>Vat On Excise</td><td>0.00</td></tr>
				<tr><td>Postponed Vat</td><td>100.00</td></tr>
				<tr><td>LRN</td><td>EXA214094_06AaxY</td></tr>
				<tr><td>UCR</td><td>123422342</td></tr>
				<tr><td>Commercial Transport Doc</td><td>N703124242</td></tr>
				<tr><td>Period</td><td>01-Jan-22</td></tr>
			</table><br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>MRN</td><td>22IEDUB4BBFC22PER2</td></tr>
				<tr><td>Version</td><td>2</td></tr>
				<tr><td>Amendment</td><td>Y</td></tr>
				<tr><td>Declaration Message Type</td><td>H1</td></tr>
				<tr><td>Payer</td><td>IE1234567A</td></tr>
				<tr><td>Importer</td><td>IE1234567A</td></tr>
				<tr><td>Importer Name</td><td>MR Test ONeill</td></tr>
				<tr><td>Declarant</td><td>IE7654321A</td></tr>
				<tr><td>Declarant Name</td><td>MR Test Murphy</td></tr>
				<tr><td>Received</td><td>2022-08-13T17:46:11.903+0100</td></tr>
				<tr><td>Tax Total</td><td>500.00</td></tr>
				<tr><td>Total Duty</td><td>50.00</td></tr>
				<tr><td>Vat On Duty</td><td>50.00</td></tr>
				<tr><td>Total Excise</td><td>350.00</td></tr>
				<tr><td>Vat On Excise</td><td>50.00</td></tr>
				<tr><td>Postponed Vat</td><td>100.00</td></tr>
				<tr><td>LRN</td><td>EXA214094_06AaxY</td></tr>
				<tr><td>UCR</td><td>1234787878342</td></tr>
				<tr><td>Commercial Transport Doc</td><td>N703124242</td></tr>
				<tr><td>Period</td><td>01-Jan-22</td></tr>
			</table>";
	}
}
