using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM415V;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;
using IM415VProvider = Enterprise.Customs.IE.Messaging.UCC5.IM415VProvider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(IM415VMessageInterpreter))]
	class IM415VMessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISUCC5InboundEDIMessage, IM415VMessageInterpreter, IM415VProvider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM415V;

		protected override AISUCC5InboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISUCC5MailboxMessage(Factory, "B00000012", IEXmlObjectSerializer.Serialize(new Im415V
			{
				Declaration = new DeclarationType
				{
					AdditionalDeclarationType12 = "A",
					Lrn25 = "LRN123456789",
					Mrn = "21IEDUB11A782454R2",
					DeclarationAcknowledgementDate = "20230811",
				}
			}));
		}

		protected override ZString GetExpectedInterpretation(AISUCC5InboundEDIMessage message) => @"A Customs Declaration Acknowledgement (IM415V) message has been received from customs for Job B00000012.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Additional Declaration Type</td><td>A</td></tr><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Declaration Acknowledgement Date</td><td>2023-08-11</td></tr></table>";

		protected override IM415VProvider GetProvider(TextReader reader) => new IM415VProvider(new MailBoxItemProvider<Im415V>(reader).Message);
	}
}
