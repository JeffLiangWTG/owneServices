using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS304;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(TS304Processor))]
	sealed class TS304ProcessorTest : TemporaryStorageHeaderMessageProcessorTest<TS304Processor, AISInboundEDIMessage, AISOutboundEDIMessage, TS304Provider>
	{
		protected override void AssertProcessResultCore(TemporaryStorageHeader temporaryStorageHeader, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("Logical Status", LogicalStatusList.Codes.Accepted, temporaryStorageHeader.AMA_MessageStatus);
			AssertEquals("Entry Status", AISEntryStatusList.Codes.Registered, temporaryStorageHeader.CustomsStatus);

			AssertMessageInterpretation(incomingMessage, @"A Temporary Storage Declaration Amendment Request Registration (TS304) message has been received for Job MAN0001000.<br />
				<br />
				<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
					<tr>
						<td>MRN</td><td>21IEDUB11A782454R2</td>
					</tr>
					<tr>
						<td>Acceptance Date</td><td>05-Sep-23 12:15</td>
					</tr>
					<tr>
						<td>Remarks</td><td>Test Remarks</td>
					</tr>
				</table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for MAN0001000",
				new[] { "A Temporary Storage Declaration Amendment Request Registration (TS304) message has been received for Job MAN0001000." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageType => AISInterchangeTypeList.Codes.TS304;

		protected override ZString MessageText => AISInterchangeProcessorTestHelper.GetAISVersion2_0TS304Text("21IEDUB11A782454R2", new DateTime(2023, 9, 5, 12, 15, 30), "Test Remarks");

		protected override ZString MessageFriendlyName => "TS304: Temporary Storage Declaration Amendment Request Registration";

		protected override TS304Processor Processor => new TS304Processor(logger, typeof(Ts304));
	}
}
