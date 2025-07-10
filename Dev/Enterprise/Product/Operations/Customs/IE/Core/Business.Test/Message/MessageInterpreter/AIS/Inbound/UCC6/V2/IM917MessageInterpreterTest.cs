using System.Collections.ObjectModel;
using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM917;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM917MessageInterpreter))]
	sealed class IM917MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM917MessageInterpreter, IM917Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM917;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			var data = new Im917
			{
				XmlNegativeAcknowledgement = new Collection<XmlNegativeAcknowledgement>(new[]
				{
					new XmlNegativeAcknowledgement
					{
						ErrorLineNumber = "1",
						ErrorReason = "SUSPECT",
						ErrorColumnNumber = "4"
					},
					new XmlNegativeAcknowledgement
					{
						ErrorLineNumber = "2",
						ErrorReason = "DODGY",
						ErrorColumnNumber = "6"
					}
				})
			};

			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00000012", IEXmlObjectSerializer.Serialize(data));
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"A Syntax Error Notification (IM917) message has been received for Job B00000012.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Error Line Number</td><td>1</td></tr><tr><td>Error Reason</td><td>SUSPECT</td></tr><tr><td>Error Column Number</td><td>4</td></tr></table><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Error Line Number</td><td>2</td></tr><tr><td>Error Reason</td><td>DODGY</td></tr><tr><td>Error Column Number</td><td>6</td></tr></table>";

		protected override IM917Provider GetProvider(TextReader reader) => new IM917Provider(new MailBoxItemProvider<Im917>(reader).Message);
	}
}
