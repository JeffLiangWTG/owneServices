using System;
using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS304;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(TS304MessageInterpreter))]
	sealed class TS304MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, TS304MessageInterpreter, TS304Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.TS304;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "MAN0001000", AISInterchangeProcessorTestHelper.GetAISVersion2_0TS304Text("21IEDUB11A782454R2", new DateTime(2023, 08, 11, 15, 45, 0), "Test Remarks"));
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"A Temporary Storage Declaration Amendment Request Registration (TS304) message has been received for Job MAN0001000.<br />
				<br />
				<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
					<tr>
						<td>MRN</td><td>21IEDUB11A782454R2</td>
					</tr>
					<tr>
						<td>Acceptance Date</td><td>11-Aug-23 15:45</td>
					</tr>
					<tr>
						<td>Remarks</td><td>Test Remarks</td>
					</tr>
				</table>";

		protected override TS304Provider GetProvider(TextReader reader) => new TS304Provider(new MailBoxItemProvider<Ts304>(reader).Message);
	}
}
