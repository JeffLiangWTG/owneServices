using System.IO;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(PTTMessageInterpreter))]
	sealed class PTTMessageInterpreterTest : CustomsAndExciseReportInboundMessageInterpreterTest<PTTMessageInterpreter, PTTProvider>
	{
		protected override ZString MessageType => CustomsAndExciseReportTypeList.Codes.PTT;

		protected override PTTProvider GetProvider(TextReader reader) => new PTTProvider(CustomsAndExciseReportInterchangeProcessorTestHelper.CreatePTTMessage());

		protected override ZString ExpectedInterpretation => GetExpectedInterpretation();

		internal static ZString GetExpectedInterpretation() => @"Period (monthly) details – tax type report for Payers<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>EORI</td><td>IE1234567A</td></tr>
				<tr><td>Period</td><td>01-Aug-22</td></tr>
			</table><br />
			<br />
			Tax Details<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>MRN</td><td>22IEDUB4BBFC22PER2</td></tr>
				<tr><td>Version</td><td>1</td></tr>
				<tr><td>1DC</td><td>0</td></tr>
				<tr><td>1A1</td><td>0</td></tr>
				<tr><td>1B2</td><td>0</td></tr>
				<tr><td>A00</td><td>5</td></tr>
				<tr><td>1B3</td><td>0</td></tr>
				<tr><td>1D5</td><td>0</td></tr>
				<tr><td>A45</td><td>0</td></tr>
				<tr><td>B00</td><td>5</td></tr>
				<tr><td>1D6</td><td>0</td></tr>
				<tr><td>A35</td><td>0</td></tr>
				<tr><td>B00EX</td><td>0</td></tr>
				<tr><td>1S1</td><td>0</td></tr>
				<tr><td>1E1</td><td>0</td></tr>
				<tr><td>A40</td><td>0</td></tr>
				<tr><td>A30</td><td>0</td></tr>
				<tr><td>1C1</td><td>0</td></tr>
				<tr><td>2E2</td><td>0</td></tr>
				<tr><td>A20</td><td>0</td></tr>
			</table><br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>MRN</td><td>22IEDUB4BBFC22AZR2</td></tr>
				<tr><td>Version</td><td>3</td></tr>
				<tr><td>1DC</td><td>0</td></tr>
				<tr><td>1A1</td><td>0</td></tr>
				<tr><td>1B2</td><td>0</td></tr>
				<tr><td>A00</td><td>5000</td></tr>
				<tr><td>1B3</td><td>200</td></tr>
				<tr><td>1D5</td><td>0</td></tr>
				<tr><td>A45</td><td>0</td></tr>
				<tr><td>B00</td><td>50</td></tr>
				<tr><td>1D6</td><td>0</td></tr>
				<tr><td>A35</td><td>0</td></tr>
				<tr><td>B00EX</td><td>0</td></tr>
				<tr><td>1S1</td><td>0</td></tr>
				<tr><td>1E1</td><td>0</td></tr>
				<tr><td>A40</td><td>0</td></tr>
				<tr><td>A30</td><td>0</td></tr>
				<tr><td>1C1</td><td>0</td></tr>
				<tr><td>2E2</td><td>0</td></tr>
				<tr><td>A20</td><td>0</td></tr>
			</table>";
	}
}
