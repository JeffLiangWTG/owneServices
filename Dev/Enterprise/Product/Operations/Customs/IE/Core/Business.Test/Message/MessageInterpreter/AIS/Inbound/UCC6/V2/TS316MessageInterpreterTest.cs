using System;
using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS316;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(TS316MessageInterpreter))]
	sealed class TS316MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, TS316MessageInterpreter, TS316Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.TS316;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			var messageText = IEXmlObjectSerializer.Serialize(new Ts316()
			{
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.DeclarationType11()
				{
					Lrn = "LRN123456789",
					RejectionDate = new DateTime(2023, 09, 07),
					RejectionMotivationText = "Sample Text 123",
				},
				FunctionalError = new System.Collections.ObjectModel.Collection<CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MFunctionalErrorType01>
			{
				new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MFunctionalErrorType01
				{
					SequenceNumber = "1",
					ErrorPointer = "ErrorPointer001",
					ErrorCode = "13",
					ErrorReason = "ER1",
					Remarks = "Functional Error Remarks 1",
					OriginalAttributeValue = "Original Attribute Value 1",
				},
				new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MFunctionalErrorType01
				{
					SequenceNumber = "2",
					ErrorPointer = "ErrorPointer002",
					ErrorCode = "52",
					ErrorReason = "ER2",
					Remarks = "Functional Error Remarks 2",
					OriginalAttributeValue = "Original Attribute Value 2",
				},
			}
			});
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "MAN0001000", messageText);
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"A TSD Rejection (TS316) message has been received for Job MAN0001000.<br/>
<br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td colspan=""2"">TS316 – Temporary Storage Declaration Rejection message</td></tr><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>Declaration Rejection Date</td><td>07-Sep-23</td></tr><tr><td>Declaration Rejection Reason</td><td>Sample Text 123</td></tr></table><br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Functional Error</td><td>1</td></tr><tr><td>Error Pointer</td><td>ErrorPointer001</td></tr><tr><td>Error Code</td><td>13</td></tr><tr><td>Error Code Description</td><td>13</td></tr><tr><td>Error Reason</td><td>ER1</td></tr><tr><td>Remarks</td><td>Functional Error Remarks 1</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 1</td></tr></table><br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Functional Error</td><td>2</td></tr><tr><td>Error Pointer</td><td>ErrorPointer002</td></tr><tr><td>Error Code</td><td>52</td></tr><tr><td>Error Code Description</td><td>52</td></tr><tr><td>Error Reason</td><td>ER2</td></tr><tr><td>Remarks</td><td>Functional Error Remarks 2</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 2</td></tr></table>";

		protected override TS316Provider GetProvider(TextReader reader) => new TS316Provider(new MailBoxItemProvider<Ts316>(reader).Message);
	}
}
