using System;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS309;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.UCC5.Testing;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(TS309Processor))]
	sealed class TS309ProcessorTest : TemporaryStorageHeaderMessageProcessorTest<TS309Processor, AISUCC5InboundEDIMessage, AISUCC5OutboundEDIMessage, Messaging.UCC5.TS309Provider>
	{
		public void TestInvalidationDecisionIsFalse()
		{
			var (declaration, header, outgoingMessage, incomingMessage) = CreateSetupData();
			incomingMessage.EM_MessageText = UpdateNodeValue(incomingMessage.EM_MessageText, "InvalidationDecision", currentBoolValue: true, newBoolValue: false);
			using (incomingMessage.Factory.AddDisposableService())
			{
				var processor = Processor;
				processor.PreProcessMessage(incomingMessage);
				AssertNoExceptionThrown(() => processor.ProcessMessage(incomingMessage));
				CombineAssertions("Process", () =>
				{
					AssertEquals("Message should have been set PRS.", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
					AssertEquals("AMA_MessageStatus", LogicalStatusList.Codes.Invalid, header.AMA_MessageStatus);
				});
			}
		}

		ZString UpdateNodeValue(ZString xmlInput, string targetNode, bool currentBoolValue, bool newBoolValue)
		{
			var doc = XDocument.Parse(xmlInput);
			XNamespace ns = "http://www.ros.ie/schemas/customs/TS309";

			var node = doc.Descendants(ns + targetNode).FirstOrDefault();
			if (node != null && string.Equals(node.Value?.Trim(), BoolToXmlValue(currentBoolValue), StringComparison.OrdinalIgnoreCase))
			{
				node.Value = BoolToXmlValue(newBoolValue);
			}

			return doc.ToString();

			static string BoolToXmlValue(bool input) => input ? "true" : "false";
		}

		protected override void AssertProcessResultCore(TemporaryStorageHeader header, AISUCC5InboundEDIMessage incomingMessage)
		{
			AssertEquals("AMA_MessageStatus", LogicalStatusList.Codes.Accepted, header.AMA_MessageStatus);
			AssertEquals("Entry Status", AISEntryStatusList.Codes.Invalid, header.CustomsStatus);

			AssertMessageInterpretation(incomingMessage, @"A Temporary Storage Declaration Invalidation Decision (TS309) message has been received for TSD MAN0001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Invalidation Decision</td><td>Y</td></tr><tr><td>Invalidation Initiated by Customs</td><td>N</td></tr><tr><td>Invalidation Justification</td><td>justification</td></tr><tr><td>Date of Invalidation Decision</td><td>05-Sep-23</td></tr><tr><td>Date of Invalidation Request</td><td>05-Sep-23</td></tr><tr><td>Date of Invalidation</td><td>05-Sep-23</td></tr></table><br />
<br />Functional Error: 1<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Error Reason</td><td>ER1</td></tr><tr><td>Error Type</td><td>13</td></tr><tr><td>Error Type Description</td><td>&nbsp;</td></tr><tr><td>Error Message</td><td>Functional Error Message 1</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 1</td></tr><tr><td>Error Pointer</td><td>ErrorPointer001</td></tr></table><br />
<br />Functional Error: 2<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Error Reason</td><td>ER2</td></tr><tr><td>Error Type</td><td>40</td></tr><tr><td>Error Type Description</td><td>&nbsp;</td></tr><tr><td>Error Message</td><td>Functional Error Message 2</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 2</td></tr><tr><td>Error Pointer</td><td>ErrorPointer002</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for MAN0001000",
				new[] { "A Temporary Storage Declaration Invalidation Decision (TS309) message has been received for TSD MAN0001000." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageFriendlyName => "TS309: Temporary Storage Declaration Invalidation Decision";

		protected override ZString MessageType => AISInterchangeTypeList.Codes.TS309;

		protected override TS309Processor Processor => new TS309Processor(logger, typeof(Ts309));

		protected override ZString MessageText => AISUCC5InterchangeProcessorTestHelper.GetTS309Text("21IEDUB11A782454R2", new DateTime(2023, 9, 5, 12, 30, 30));
	}
}
