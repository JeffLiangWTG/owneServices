using System.IO;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(BALMessageInterpreter))]
	sealed class BALMessageInterpreterTest : CustomsAndExciseReportInboundMessageInterpreterTest<BALMessageInterpreter, BALProvider>
	{
		protected override ZString MessageType => CustomsAndExciseReportTypeList.Codes.BAL;

		protected override BALProvider GetProvider(TextReader reader) => new BALProvider(CustomsAndExciseReportInterchangeProcessorTestHelper.CreateBALMessage());

		protected override ZString ExpectedInterpretation => GetExpectedInterpretation();

		internal static ZString GetExpectedInterpretation() => @"Balance<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>Total</td><td>11061096.00</td></tr>
				<tr><td>Cash</td><td>9361096.00</td></tr>
				<tr><td>Deferred</td><td>1700000.00</td></tr>
			</table>";
	}
}
